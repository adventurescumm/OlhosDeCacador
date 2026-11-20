using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BemTeVi.Editor
{
    public static class BemTeViSceneBuilder
    {
        const string Folder = "Assets/BemTeVi/Scenes/";
        sealed class Entry { public string id; public MapData map; public RoomData room; }
        static IEnumerable<Entry> Entries(Catalog catalog)
        {
            var bedroom = new MapData { color = catalog.hub.color, floor = catalog.hub.floor, wall = catalog.hub.wall };
            yield return new Entry { id = "inicio", map = bedroom, room = catalog.hub };
            yield return new Entry { id = "quarto", map = bedroom, room = catalog.hub };
            foreach (var map in catalog.maps)
                foreach (var room in new[] { map.junction }.Concat(map.rooms).Concat(new[] { map.challenge }))
                    yield return new Entry { id = room.id, map = map, room = room };
        }
        static string PathFor(string id) => Folder + (id == "inicio" || id == "quarto" ? "Menus" : id.Split('_')[0]) + "/" + id + ".unity";
        static bool CurrentContent(string path) => File.ReadAllText(path).Replace("\r", "").Contains("  contentVersion: " + BemTeViStage.ContentVersion + "\n");
        public static void EnsureScenes(bool rebuild = false)
        {
            var catalog = JsonUtility.FromJson<Catalog>(Resources.Load<TextAsset>("BemTeVi/catalog").text);
            var entries = Entries(catalog).ToArray();
            var previous = SceneManager.GetActiveScene();
            SceneSetup[] reopen = null;
            if (entries.Any(e => SceneManager.GetSceneByPath(PathFor(e.id)).isLoaded &&
                (rebuild || !File.Exists(PathFor(e.id)) || !CurrentContent(PathFor(e.id)))))
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) throw new InvalidOperationException("Preparação cancelada; as cenas abertas foram preservadas.");
                reopen = EditorSceneManager.GetSceneManagerSetup();
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
            try
            {
                foreach (var entry in entries)
                {
                    string path = PathFor(entry.id);
                    if (rebuild || !File.Exists(path) || !CurrentContent(path)) Bake(entry, path, catalog);
                }
            }
            finally
            {
                if (reopen != null) EditorSceneManager.RestoreSceneManagerSetup(reopen);
                else if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Folder + "Main.unity", true) }
                .Concat(entries.Select(e => new EditorBuildSettingsScene(PathFor(e.id), true))).ToArray();
            AssetDatabase.SaveAssets();
        }
        [MenuItem("Bem-Te-Vi/7. Recriar cenas a partir do catalogo")]
        public static void Rebuild()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
                throw new InvalidOperationException("Saia do Play e aguarde a importação antes de editar cenas.");
            if (!EditorUtility.DisplayDialog("Recriar cenas", "Isso substitui as alterações manuais nas 17 cenas de conteúdo pela versão do catalog.json. Main e o progresso salvo não são alterados.", "Recriar", "Cancelar")) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            // Close an existing instance of the scene before replacing it on disk.
            var setup = EditorSceneManager.GetSceneManagerSetup();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            try { EnsureScenes(true); }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
        }
        static void Bake(Entry entry, string path, Catalog catalog)
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                var stage = new GameObject("00 — Configuração: " + entry.room.name).AddComponent<BemTeViStage>();
                stage.stageId = entry.id; stage.cameraPosition = Vec(entry.room.camera); stage.lookAt = Vec(entry.room.lookAt);
                stage.fieldOfView = entry.room.fieldOfView;
                var view = new RoomView(entry.map, entry.room, new Progress(catalog), true);
                stage.contentRoot = view.Root; stage.baked = true; stage.contentVersion = BemTeViStage.ContentVersion;
                view.Root.name = "01 — Cenário editável";
                var architecture = Group(view.Root.transform, "Arquitetura e iluminação");
                var decoration = Group(view.Root.transform, "Brinquedos e decoração");
                var targets = Group(view.Root.transform, "Objetos da charada");
                var links = Group(view.Root.transform, "Passagens e mecanismos");
                foreach (var child in view.Root.transform.Cast<Transform>().ToArray())
                {
                    if (child == architecture || child == decoration || child == targets || child == links) continue;
                    var prop = child.GetComponent<BemTeViSceneProp>();
                    child.SetParent(prop == null ? architecture : !string.IsNullOrEmpty(prop.data.id) ? targets :
                        !string.IsNullOrEmpty(prop.data.action) && prop.data.action != "inspect" ? links : decoration, true);
                }
                foreach (var target in view.Root.GetComponentsInChildren<SearchTarget>(true)) UnityEngine.Object.DestroyImmediate(target);
                foreach (var spot in view.Root.GetComponentsInChildren<WorldHotspot>(true)) UnityEngine.Object.DestroyImmediate(spot);
                PersistPrimitiveMaterials(view.Root);
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(.42f, .44f, .52f); RenderSettings.skybox = null;
                if (!EditorSceneManager.SaveScene(scene, path)) throw new InvalidOperationException("Não foi possível salvar " + path);
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }
        static Vector3 Vec(float[] v) => new Vector3(v[0], v[1], v[2]);
        static Transform Group(Transform root, string name)
        {
            var go = new GameObject(name); go.transform.SetParent(root, false); return go.transform;
        }
        static void PersistPrimitiveMaterials(GameObject root)
        {
            const string materials = "Assets/BemTeVi/Settings/RoomMaterials";
            if (!AssetDatabase.IsValidFolder(materials)) AssetDatabase.CreateFolder("Assets/BemTeVi/Settings", "RoomMaterials");
            foreach (var renderer in root.GetComponentsInChildren<Renderer>())
            {
                var list = renderer.sharedMaterials;
                for (int i = 0; i < list.Length; i++)
                {
                    var material = list[i];
                    if (material == null || AssetDatabase.Contains(material)) continue;
                    string id = ColorUtility.ToHtmlStringRGBA(material.GetColor("_BaseColor")) + (material.IsKeywordEnabled("_EMISSION") ? "_luz" : "_solido");
                    string path = materials + "/" + id + ".mat";
                    var saved = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (saved == null) { material.name = id; AssetDatabase.CreateAsset(material, path); saved = material; }
                    list[i] = saved;
                }
                renderer.sharedMaterials = list;
            }
            AssetDatabase.SaveAssets();
        }
    }
}
