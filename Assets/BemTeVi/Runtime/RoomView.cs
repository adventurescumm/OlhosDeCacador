using System;
using System.Collections.Generic;
using UnityEngine;

namespace BemTeVi
{
    public sealed class RoomView : IDisposable
    {
        public GameObject Root { get; private set; }
        public readonly Dictionary<string, SearchTarget> Targets = new Dictionary<string, SearchTarget>();
        public readonly List<WorldHotspot> Hotspots = new List<WorldHotspot>();
        readonly MapData map;
        readonly Progress progress;
        readonly List<Material> roomMaterials = new List<Material>();
        public static Color Hex(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }

        public RoomView(MapData map, RoomData room, Progress progress, bool reduceMotion)
        {
            this.map = map; this.progress = progress;
            var stage = Application.isPlaying ? BemTeViStage.Active : null;
            bool authored = stage != null && stage.contentRoot != null &&
                (stage.stageId == room.id || (room.id == "quarto" && stage.stageId == "inicio"));
            Root = authored ? stage.contentRoot : new GameObject(room.name);
            if (authored) stage.contentRoot = null;
            try
            {
                if (!authored)
                {
                    if (room.geometry != null && room.geometry.Length > 0)
                        foreach (var shape in room.geometry) Architecture(shape);
                    else
                    {
                        var floor = RoomMaterial(string.IsNullOrEmpty(room.floor) ? map.floor : room.floor);
                        var wall = RoomMaterial(string.IsNullOrEmpty(room.wall) ? map.wall : room.wall);
                        var edge = RoomMaterial(string.IsNullOrEmpty(room.color) ? map.color : room.color);
                        var cream = RoomMaterial("#FFF0CB"); var coral = RoomMaterial("#E991A8");
                        float width = room.width > 0 ? room.width : 12, depth = room.depth > 0 ? room.depth : 8;
                        float back = depth * .5f;
                        Box("Piso do quarto", new Vector3(0, -.15f, 0), new Vector3(width, .3f, depth), floor);
                        Box("Parede do fundo", new Vector3(0, 2.7f, back), new Vector3(width, 5.4f, .16f), wall);
                        Box("Parede esquerda", new Vector3(-width * .5f, 2, 1), new Vector3(.16f, 4, depth - 2), wall);
                        Box("Parede direita", new Vector3(width * .5f, 2, 1), new Vector3(.16f, 4, depth - 2), edge);
                        Box("Rodapé", new Vector3(0, .16f, back - .14f), new Vector3(width, .32f, .22f), cream);
                        Box("Moldura alta", new Vector3(0, 5.1f, back - .1f), new Vector3(width, .15f, .2f), cream);
                        // Permitted primitive architectural details; all furniture and toys are downloaded models.
                        for (int i = 0; i < 12; i++)
                            Box("Friso da parede", new Vector3(-width * .5f + .5f + i * (width - 1) / 11, .9f, back - .1f), new Vector3(.055f, 1.5f, .08f), cream);
                        if (room.layout == "search")
                        {
                            Box("Prateleira de brinquedos", new Vector3(0, 1.69f, 2.65f), new Vector3(11.3f, .16f, 1.0f), edge);
                            Box("Filete da prateleira", new Vector3(0, 1.72f, 2.13f), new Vector3(11.4f, .12f, .1f), cream);
                        }
                        if (room.layout == "bedroom")
                        {
                            Box("Faixa de cor na parede", new Vector3(0, 1.8f, back - .11f), new Vector3(width, .3f, .08f), coral);
                            Box("Prateleira alta", new Vector3(-4, 3.3f, back - .6f), new Vector3(4.5f, .12f, 1.0f), cream);
                        }
                        var cord = RoomMaterial("#DAC2A2");
                        Box("Fio das luzinhas", new Vector3(0, 4.5f, back - .28f), new Vector3(width - .8f, .025f, .025f), cord);
                        string[] bulbColors = { "#FFD586", "#ECA6BC", "#9DE3CF", "#B8B0EF" };
                        for (int i = 0; i < 12; i++)
                        {
                            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere); ball.name = "Luzinha de festa";
                            ball.transform.SetParent(Root.transform, false);
                            ball.transform.localPosition = new Vector3(-width * .5f + .65f + i * (width - 1.3f) / 11, 4.43f, back - .28f);
                            ball.transform.localScale = Vector3.one * .18f;
                            var glow = RoomMaterial(bulbColors[i % bulbColors.Length]);
                            glow.EnableKeyword("_EMISSION"); glow.SetColor("_EmissionColor", Hex(bulbColors[i % bulbColors.Length]) * .35f);
                            ball.GetComponent<Renderer>().sharedMaterial = glow;
                            ball.GetComponent<Collider>().enabled = false;
                        }
                        var lamp = new GameObject("Luz quente do quarto", typeof(Light)); lamp.transform.SetParent(Root.transform, false);
                        lamp.transform.localPosition = new Vector3(-3.5f, 4, -.5f);
                        var bulb = lamp.GetComponent<Light>(); bulb.type = LightType.Point; bulb.range = 13; bulb.intensity = 1.7f;
                        bulb.color = Hex("#FFE4BB"); bulb.shadows = LightShadows.None;
                    }
                    foreach (var p in room.objects)
                    {

                        Add(p, reduceMotion);
                    }
                }
                else
                {
                    foreach (var item in Root.GetComponentsInChildren<BemTeViSceneProp>(true))
                    {
                        if (item.data == null) throw new InvalidOperationException("Objeto de cena sem dados: " + item.name);

                        Bind(item.gameObject, item.data, reduceMotion);
                    }
                }
                if (!authored) foreach (var passage in Hotspots)
                {
                    if (passage.Data.action != "exit" && passage.Data.action != "world") continue;
                    AddPassageSign(passage, room);
                }
                ApplyMechanisms(progress, false);
                Physics.SyncTransforms();
            }
            catch { Dispose(); throw; }
        }
        void Architecture(PrimitiveData p)
        {
            var kind = p.shape == "sphere" ? PrimitiveType.Sphere : p.shape == "cylinder" ? PrimitiveType.Cylinder : PrimitiveType.Cube;
            var go = GameObject.CreatePrimitive(kind); go.name = p.name;
            go.transform.SetParent(Root.transform, false);
            go.transform.localPosition = new Vector3(p.x, p.y, p.z);
            go.transform.localRotation = Quaternion.Euler(p.tiltX, p.rotation, p.tiltZ);
            go.transform.localScale = new Vector3(p.width, p.height / (kind == PrimitiveType.Cylinder ? 2 : 1), p.depth);
            go.GetComponent<Renderer>().sharedMaterial = RoomMaterial(p.color);
            if (kind == PrimitiveType.Cube) go.GetComponent<Collider>().enabled = !p.decorative;
            else
            {
                // Capsule/Sphere colliders inflate under nonuniform scale: a flat window
                // would become an invisible solid ball. Match the visible primitive mesh.
                var approximate = go.GetComponent<Collider>(); approximate.enabled = false;
                DestroySceneObject(approximate);
                var exact = go.AddComponent<MeshCollider>();
                exact.sharedMesh = go.GetComponent<MeshFilter>().sharedMesh; exact.enabled = !p.decorative;
            }
        }
        void AddPassageSign(WorldHotspot passage, RoomData room)
        {
            var data = passage.Data;
            if (string.IsNullOrEmpty(data.landmark)) return;
            var sign = new GameObject("Placa — " + data.landmark);
            sign.transform.SetParent(passage.transform, false);
            // A small physical plaque beside the entrance, kept clear of the search itself.
            sign.transform.position = passage.transform.position + new Vector3(0, .12f, -data.size * .35f - .28f);
            var eye = new Vector3(room.camera[0], room.camera[1], room.camera[2]);
            sign.transform.rotation = Quaternion.LookRotation(sign.transform.position - eye);
            var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "Madeira da placa"; plate.transform.SetParent(sign.transform, false);
            plate.transform.localPosition = new Vector3(0, 0, .035f);
            plate.transform.localScale = new Vector3(2.6f, .43f, .065f);
            plate.GetComponent<Renderer>().sharedMaterial = RoomMaterial("#F5DFAD");
            var label = new GameObject("Destino", typeof(TextMesh)).GetComponent<TextMesh>();
            label.transform.SetParent(sign.transform, false);
            label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center;
            label.font = Resources.Load<Font>("BemTeVi/UI/Itim-Regular");
            label.fontSize = 64; label.characterSize = .052f;
            label.color = Hex("#342437"); label.text = data.landmark;
            label.GetComponent<MeshRenderer>().sharedMaterial = label.font.material;
            float textWidth = label.GetComponent<Renderer>().bounds.size.magnitude;
            if (textWidth > .001f) label.transform.localScale = Vector3.one * (2.35f / textWidth);
        }
        public void ApplyMechanisms(Progress progress, bool animate)
        {
            foreach (var spot in Hotspots)
                if (!string.IsNullOrEmpty(spot.Data.gate)) spot.SetOpen(progress.Opened(spot.Data.gate), animate);
            foreach (var item in Root.GetComponentsInChildren<BemTeViSceneProp>(true))
                if (!string.IsNullOrEmpty(item.data.showAfter)) item.gameObject.SetActive(progress.Opened(item.data.showAfter));
        }
        Material RoomMaterial(string color)
        {
            // Solid materials are used only on permitted primitive room geometry.
            // The persistent materials in Resources also keep this shader in player builds.
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("Shader URP/Lit ausente. Execute Preparar projeto no editor.");
            var mat = new Material(shader);
            mat.SetColor("_BaseColor", Hex(color)); mat.SetFloat("_Smoothness", .28f);
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
            var source = Resources.Load<GameObject>("BemTeVi/Models/" + p.asset);
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
            wrapper.transform.localRotation = Quaternion.Euler(p.tiltX, p.rotation, p.tiltZ);
            wrapper.AddComponent<BemTeViSceneProp>().data = p;
            Bind(wrapper, p, reduceMotion);
        }
        void Bind(GameObject wrapper, PropData p, bool reduceMotion)
        {
            if (progress.Active(map, p))
            {
                var target = wrapper.AddComponent<SearchTarget>(); target.Configure(p, !reduceMotion);
                target.RestoreCollected(progress.Has(p.id));
                Targets.Add(p.id, target);
            }
            else if (!string.IsNullOrEmpty(p.action) || !string.IsNullOrEmpty(p.id))
            {
                var data = p;
                if (!string.IsNullOrEmpty(p.id))
                {
                    data = JsonUtility.FromJson<PropData>(JsonUtility.ToJson(p));
                    data.action = "inspect"; data.label = "";
                }
                var hotspot = wrapper.AddComponent<WorldHotspot>(); hotspot.Configure(data, !reduceMotion);
                Hotspots.Add(hotspot);
            }
        }
        public void SetReducedMotion(bool reduced)
        {
            foreach (var target in Targets.Values) if (target != null) target.Configure(target.Data, !reduced);
            foreach (var spot in Hotspots) if (spot != null) spot.Configure(spot.Data, !reduced);
        }
        static void DestroySceneObject(UnityEngine.Object value)
        {
            if (Application.isPlaying) UnityEngine.Object.Destroy(value);
            else UnityEngine.Object.DestroyImmediate(value);
        }
        public void Dispose()
        {
            if (Root != null) { Root.SetActive(false); DestroySceneObject(Root); Root = null; }
            foreach (var material in roomMaterials) DestroySceneObject(material);
            roomMaterials.Clear(); Targets.Clear(); Hotspots.Clear();
        }
    }
}
