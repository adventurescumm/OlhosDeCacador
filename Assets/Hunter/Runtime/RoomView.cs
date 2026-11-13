using System;
using System.Collections.Generic;
using UnityEngine;

namespace OlhosDeCacador
{
    public sealed class RoomView : IDisposable
    {
        public GameObject Root { get; private set; }
        public readonly Dictionary<string, HuntTarget> Targets = new Dictionary<string, HuntTarget>();
        public readonly List<WorldHotspot> Hotspots = new List<WorldHotspot>();
        readonly List<Material> roomMaterials = new List<Material>();
        public static Color Hex(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }

        public RoomView(MapData map, RoomData room, Progress progress, bool reduceMotion)
        {
            Root = new GameObject(room.name);
            try
            {
                var floor = RoomMaterial(map.floor); var wall = RoomMaterial(map.wall);
                var edge = RoomMaterial(map.color); var yellow = RoomMaterial("#FFD35B");
                Box("Base da caixa", new Vector3(0, -.22f, 0), new Vector3(12, .4f, 7), floor);
                Box("Fundo da caixa", new Vector3(0, 1.8f, 3.6f), new Vector3(12, 4, .18f), wall);
                Box("Lateral esquerda", new Vector3(-6, .4f, 0), new Vector3(.18f, 1.2f, 7), edge);
                Box("Lateral direita", new Vector3(6, .4f, 0), new Vector3(.18f, 1.2f, 7), edge);
                Box("Borda da frente", new Vector3(0, -.10f, -3.52f), new Vector3(12.2f, .24f, .15f), edge);
                Box("Faixa do fundo", new Vector3(0, 3.7f, 3.45f), new Vector3(12.1f, .23f, .25f), yellow);
                // Shelves, raised floors and stage frames are part of the primitive room form.
                if (room.layout == "shelves" || room.layout == "hub")
                {
                    Box("Prateleira do fundo", new Vector3(0, 1.65f, 2.65f), new Vector3(10.8f, .16f, 1.2f), edge);
                    Box("Travessa da estante", new Vector3(0, 2.5f, 3.3f), new Vector3(.16f, 2.3f, .3f), yellow);
                }
                if (room.layout == "space")
                {
                    for (int i = 0; i < 5; i++)
                        Box("Faixa do piso lunar", new Vector3(-4 + i * 2, .002f, 0), new Vector3(.08f, .01f, 5.8f), yellow);
                    // Thin primitive supports for the existing toy rocks suspended at the back.
                    foreach (float x in new[] { -2.4f, 1.2f, 3.5f })
                        Box("Fio do móbile", new Vector3(x, 3.35f, 3.05f), new Vector3(.018f, .7f, .018f), yellow);
                }
                if (room.layout == "city")
                {
                    Box("Rua de blocos", new Vector3(0, .005f, -.2f), new Vector3(10.6f, .02f, 1.4f), wall);
                    for (int i = 0; i < 7; i++)
                        Box("Faixa da rua", new Vector3(-4.5f + i * 1.5f, .02f, -.2f), new Vector3(.55f, .02f, .09f), yellow);
                }
                // Toy blocks and room trim use only the explicitly permitted primitives.
                for (int i = 0; i < 6; i++)
                {
                    var tile = i % 2 == 0 ? yellow : edge;
                    Box("Bloco da moldura", new Vector3(-5.2f + i * 2.08f, .09f, -3.15f), new Vector3(.7f, .18f, .32f), tile);
                }
                foreach (var p in room.objects)
                {
                if (!string.IsNullOrEmpty(p.id) && progress.Has(p.id)) continue;
                    Add(p, reduceMotion);
                }
                Physics.SyncTransforms();
            }
            catch { Dispose(); throw; }
        }
        Material RoomMaterial(string color)
        {
            // Solid materials are used only on permitted primitive room geometry.
            // The persistent materials in Resources also keep this shader in player builds.
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("Shader URP/Lit ausente. Execute Preparar projeto no editor.");
            var mat = new Material(shader);
            mat.SetColor("_BaseColor", Hex(color)); mat.SetFloat("_Smoothness", .08f);
            roomMaterials.Add(mat); return mat;
        }
        void Box(string name, Vector3 position, Vector3 scale, Material material)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube); box.name = name;
            box.transform.SetParent(Root.transform, false); box.transform.localPosition = position;
            box.transform.localScale = scale; box.GetComponent<Renderer>().sharedMaterial = material;
            box.GetComponent<BoxCollider>().enabled = true;
        }
        void Add(PropData p, bool reduceMotion)
        {
            var source = Resources.Load<GameObject>("Hunter/Models/" + p.asset);
            if (source == null) throw new InvalidOperationException("Modelo ausente: " + p.asset);
            var wrapper = new GameObject(string.IsNullOrEmpty(p.id) ? p.asset : p.id);
            wrapper.transform.SetParent(Root.transform, false);
            var model = UnityEngine.Object.Instantiate(source, wrapper.transform);
            model.name = p.asset;
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            var renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) throw new InvalidOperationException("Modelo sem geometria: " + p.asset);
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            float longest = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            if (longest < .0001f) throw new InvalidOperationException("Escala inválida: " + p.asset);
            // Normalize only the transform. Original geometry, UVs and textures are retained.
            float scale = p.size / longest;
            model.transform.localScale *= scale;
            model.transform.localPosition = new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z) * scale;
            foreach (var mesh in model.GetComponentsInChildren<MeshFilter>())
            {
                if (mesh.sharedMesh == null) continue;
                var collider = mesh.gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = mesh.sharedMesh;
            }
            wrapper.transform.localPosition = new Vector3(p.x, p.y, p.z);
            wrapper.transform.localRotation = Quaternion.Euler(0, p.rotation, 0);
            if (!string.IsNullOrEmpty(p.id))
            {
                var target = wrapper.AddComponent<HuntTarget>(); target.Configure(p, !reduceMotion);
                Targets.Add(p.id, target);
            }
            else if (!string.IsNullOrEmpty(p.action))
            {
                var hotspot = wrapper.AddComponent<WorldHotspot>(); hotspot.Configure(p, !reduceMotion);
                Hotspots.Add(hotspot);
            }
        }
        public void Dispose()
        {
            if (Root != null) { Root.SetActive(false); UnityEngine.Object.Destroy(Root); Root = null; }
            foreach (var material in roomMaterials) UnityEngine.Object.Destroy(material);
            roomMaterials.Clear(); Targets.Clear(); Hotspots.Clear();
        }
    }
}
