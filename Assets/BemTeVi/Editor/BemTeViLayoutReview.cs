using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BemTeVi.Editor
{
    // Actual Unity renders and mesh raycasts for reviewing the fixed camera compositions.
    public static class BemTeViLayoutReview
    {
        [MenuItem("Bem-Te-Vi/8. Conferir enquadramentos e gerar previas")]
        public static void Capture()
        {
            var setup = EditorSceneManager.GetSceneManagerSetup();
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
            const string folder = "Docs/Previews/v15";
            Directory.CreateDirectory(folder);
            try
            {
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(.42f, .44f, .52f);
                var lamp = new GameObject("Luz de conferência", typeof(Light)).GetComponent<Light>();
                lamp.type = LightType.Directional; lamp.intensity = 1.2f; lamp.color = RoomView.Hex("#FFF0D9");
                lamp.transform.rotation = Quaternion.Euler(45, -35, 0); lamp.shadows = LightShadows.Soft;
                var camera = new GameObject("Câmera de conferência", typeof(Camera)).GetComponent<Camera>();
                camera.nearClipPlane = .1f; camera.farClipPlane = 70;
                camera.clearFlags = CameraClearFlags.SolidColor;
                var catalog = JsonUtility.FromJson<Catalog>(Resources.Load<TextAsset>("BemTeVi/catalog").text);
                int inspected = 0, passages = 0;
                var failures = new System.Collections.Generic.List<string>();
                var observations = new System.Collections.Generic.List<BemTeViSearchReview.Sample>();
                var bedroom = new MapData { rooms = Array.Empty<RoomData>(), junction = catalog.hub };
                foreach (var map in catalog.maps.Concat(new[] { bedroom }))
                foreach (var stage in new[] { map.junction }.Concat(map.rooms).Concat(new[] { map.challenge }).Where(s => s != null))
                {
                    using (var view = new RoomView(map, stage, new Progress(catalog), true))
                    {
                        camera.transform.position = Vec(stage.camera); camera.transform.LookAt(Vec(stage.lookAt));
                        camera.fieldOfView = stage.fieldOfView; camera.backgroundColor = RoomView.Hex(stage.wall);
                        camera.aspect = 1440f / (stage == map.junction ? 900 : 760);
                        Physics.SyncTransforms();
                        foreach (var route in view.Hotspots.Where(h => h.Data.action == "exit" || h.Data.action == "world"))
                        {
                            var sign = route.GetComponentInChildren<TextMesh>();
                            if (sign == null) { failures.Add(stage.id + "/placa ausente/" + route.Data.destination); continue; }
                            var center = sign.GetComponent<Renderer>().bounds.center;
                            var screen = camera.WorldToViewportPoint(center);
                            bool onScreen = screen.z > 0 && screen.x > .01f && screen.x < .99f && screen.y > .01f && screen.y < .99f;
                            bool clickable = Physics.Raycast(camera.transform.position, center-camera.transform.position, out var routeHit, 70) &&
                                routeHit.collider.GetComponentInParent<WorldHotspot>() == route;
                            if (!onScreen || !clickable) failures.Add(stage.id + "/passagem/" + route.Data.destination);
                            passages++;
                        }
                        foreach (var item in view.Root.GetComponentsInChildren<BemTeViSceneProp>())
                        {
                            if (string.IsNullOrEmpty(item.data.id)) continue;
                            var sample = BemTeViSearchReview.Measure(camera, item); sample.room = stage.id;
                            observations.Add(sample);
                            if (sample.exposure < .28f || sample.visibleSamples < 3 || sample.pixelSpan < 28)
                                failures.Add(stage.id + "/" + item.data.id + " (visível=" + sample.exposure.ToString("F2") + ", px=" + sample.pixelSpan.ToString("F0") + ")");
                            inspected++;
                        }
                        var target = new RenderTexture(1440, stage == map.junction ? 900 : 760, 24);
                        var previous = RenderTexture.active;
                        try
                        {
                            camera.targetTexture = target; camera.Render(); camera.Render(); RenderTexture.active = target;
                            var texture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
                            texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); texture.Apply();
                            File.WriteAllBytes(folder + "/" + stage.id + ".png", texture.EncodeToPNG());
                            UnityEngine.Object.DestroyImmediate(texture);
                        }
                        finally
                        {
                            camera.targetTexture = null; RenderTexture.active = previous;
                            target.Release(); UnityEngine.Object.DestroyImmediate(target);
                        }
                    }
                }
                Directory.CreateDirectory("Logs");
                File.WriteAllText("Logs/SearchVisibility-v15.json", JsonUtility.ToJson(new BemTeViSearchReview.Report { targets = observations.ToArray() }, true));
                if (failures.Count > 0) throw new InvalidOperationException("Alvos/passagens a corrigir: " + string.Join(", ", failures));
                Debug.Log("LAYOUT_REVIEW_PASS: " + inspected + " alvos e " + passages + " passagens visíveis e clicáveis. Prévias Unity em " + folder);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                EditorSceneManager.RestoreSceneManagerSetup(setup);
            }
        }
        public static void Batch()
        {
            EditorSceneManager.OpenScene("Assets/BemTeVi/Scenes/Main.unity", OpenSceneMode.Single);
            BemTeViSceneBuilder.EnsureScenes(true);
            BemTeViBuild.Prepare(); BemTeViBuild.Validate(); Capture();
        }
        public static void BatchFit()
        {
            EditorSceneManager.OpenScene("Assets/BemTeVi/Scenes/Main.unity", OpenSceneMode.Single);
            BemTeViSearchReview.FitCovers();
            Batch();
        }
        static Vector3 Vec(float[] v) => new Vector3(v[0], v[1], v[2]);
    }
}
