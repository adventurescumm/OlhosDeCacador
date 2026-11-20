using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BemTeVi.Editor
{
    public static class BemTeViSearchReview
    {
        [Serializable] public sealed class Sample
        {
            public string room, id, mainOccluder;
            public int round, silhouetteSamples, visibleSamples;
            public float exposure, pixelSpan;
        }
        [Serializable] public sealed class Report { public Sample[] targets; }

        // Compare the target's own mesh silhouette to the first surface on each ray.
        // Empty space in a thin or ring-shaped mesh is not counted as occlusion.
        public static Sample Measure(Camera camera, BemTeViSceneProp target, int count = 13)
        {
            var renderers = target.GetComponentsInChildren<Renderer>();
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            Vector2 min = Vector2.one * float.MaxValue, max = Vector2.one * float.MinValue;
            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                var corner = bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z));
                var screen = camera.WorldToViewportPoint(corner);
                min = Vector2.Min(min, screen); max = Vector2.Max(max, screen);
            }
            int silhouette = 0, visible = 0;
            var occluders = new Dictionary<string, int>();
            for (int y = 0; y < count; y++)
            for (int x = 0; x < count; x++)
            {
                var point = new Vector3(Mathf.Lerp(min.x, max.x, (x + .5f) / count),
                    Mathf.Lerp(min.y, max.y, (y + .5f) / count), 0);
                var ray = camera.ViewportPointToRay(point);
                float nearest = float.MaxValue, own = float.MaxValue; string nearestName = "";
                foreach (var hit in Physics.RaycastAll(ray, 70, ~0, QueryTriggerInteraction.Ignore))
                {
                    var prop = hit.collider.GetComponentInParent<BemTeViSceneProp>();
                    if (hit.distance < nearest)
                    {
                        nearest = hit.distance;
                        nearestName = prop == null ? hit.collider.gameObject.name :
                            !string.IsNullOrEmpty(prop.data.conceals) ? "cover:" + prop.data.conceals :
                            !string.IsNullOrEmpty(prop.data.id) ? "target:" + prop.data.id : prop.data.asset;
                    }
                    if (prop == target) own = Mathf.Min(own, hit.distance);
                }
                if (own == float.MaxValue) continue;
                silhouette++;
                if (nearest >= own - .002f && point.x > .01f && point.x < .99f && point.y > .01f && point.y < .99f) visible++;
                else if (!string.IsNullOrEmpty(nearestName)) occluders[nearestName] = occluders.TryGetValue(nearestName, out var n) ? n + 1 : 1;
            }
            return new Sample { id = target.data.id, round = target.data.round, silhouetteSamples = silhouette,
                mainOccluder = occluders.Count == 0 ? "" : occluders.OrderByDescending(pair => pair.Value).First().Key,
                visibleSamples = visible, exposure = silhouette == 0 ? 0 : (float)visible / silhouette,
                pixelSpan = Mathf.Max((max.x - min.x) * 1440, (max.y - min.y) * 760) };
        }

        public static void FitCovers()
        {
            const string catalogPath = "Assets/Resources/BemTeVi/catalog.json";
            string source = File.ReadAllText(catalogPath);
            var catalog = JsonUtility.FromJson<Catalog>(source);
            var previous = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
            try
            {
                var camera = new GameObject("Câmera de composição", typeof(Camera)).GetComponent<Camera>();
                camera.aspect = 1440f / 760;
                int fitted = 0;
                foreach (var map in catalog.maps)
                foreach (var room in map.rooms)
                using (var view = new RoomView(map, room, new Progress(catalog), true))
                {
                    camera.transform.position = Vec(room.camera); camera.transform.LookAt(Vec(room.lookAt)); camera.fieldOfView = room.fieldOfView;
                    var props = view.Root.GetComponentsInChildren<BemTeViSceneProp>();
                    foreach (var cover in props.Where(p => !string.IsNullOrEmpty(p.data.conceals)))
                    {
                        var target = props.First(p => p.data.id == cover.data.conceals);
                        var direction = Vector3.ProjectOnPlane(camera.transform.position - target.transform.position, Vector3.up).normalized;
                        var side = Vector3.Cross(Vector3.up, direction).normalized;
                        var origin = cover.transform.position;
                        Vector3 best = origin; float bestScale = 1, bestScore = float.MaxValue;
                        foreach (float scale in new[] { .68f, .84f, 1f, 1.18f, 1.35f })
                        foreach (float gap in new[] { .05f, .20f, .42f, .68f })
                        foreach (float lateral in new[] { -.16f, .04f, .22f })
                        {
                            cover.transform.localScale = Vector3.one * scale;
                            var tb = BoundsOf(target); var cb = BoundsOf(cover);
                            float distance = (tb.size.z + cb.size.z) * .5f + gap;
                            var point = target.transform.position + direction * distance + side * lateral;
                            point.y = origin.y;
                            cover.transform.position = point;
                            Physics.SyncTransforms();
                            var sample = Measure(camera, target, 13);
                            float score = Mathf.Abs(sample.exposure - target.data.searchExposure) +
                                (sample.visibleSamples < 3 || sample.exposure < .30f ? 10 : 0) + Mathf.Abs(scale - 1) * .015f;
                            if (score >= bestScore) continue;
                            bestScore = score; best = point; bestScale = scale;
                        }
                        cover.transform.position = best; cover.transform.localScale = Vector3.one * bestScale;
                        cover.data.x = best.x; cover.data.y = best.y; cover.data.z = best.z; cover.data.size *= bestScale;
                        Physics.SyncTransforms();
                        string expression = "\\{[^{}]*\"conceals\"\\s*:\\s*\"" + Regex.Escape(cover.data.conceals) + "\"[^{}]*\\}";
                        int matches = 0;
                        source = Regex.Replace(source, expression, match =>
                        {
                            matches++; string body = match.Value;
                            body = Number(body, "x", cover.data.x); body = Number(body, "y", cover.data.y);
                            body = Number(body, "z", cover.data.z); return Number(body, "size", cover.data.size);
                        });
                        if (matches != 1) throw new InvalidOperationException("Cobertura duplicada/ausente: " + cover.data.conceals);
                        fitted++;
                    }
                }
                File.WriteAllText(catalogPath, source, new UTF8Encoding(false));
                AssetDatabase.ImportAsset(catalogPath, ImportAssetOptions.ForceUpdate);
                Debug.Log("SEARCH_COMPOSITION: " + fitted + " coberturas ajustadas sem mover os suportes dos alvos.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded) UnityEngine.SceneManagement.SceneManager.SetActiveScene(previous);
            }
        }
        static Bounds BoundsOf(Component item)
        {
            var renderers = item.GetComponentsInChildren<Renderer>(); var bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds); return bounds;
        }
        static string Number(string body, string field, float value) => Regex.Replace(body,
            "(\"" + field + "\"\\s*:\\s*)[-+0-9.eE]+", match => match.Groups[1].Value + value.ToString("0.#####", CultureInfo.InvariantCulture));
        static Vector3 Vec(float[] point) => new Vector3(point[0], point[1], point[2]);
    }
}
