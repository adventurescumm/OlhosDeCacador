using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace BemTeVi.Editor
{
    public static class BemTeViBuild
    {
        const string Scene = "Assets/BemTeVi/Scenes/Main.unity";
        [MenuItem("Bem-Te-Vi/1. Preparar projeto")]
        public static void Prepare()
        {
            BemTeViRenderPipeline.EnsureConfigured();
            BemTeViMaterialRepair.Repair();
            PlayerSettings.companyName = "BemTeVi";
            PlayerSettings.productName = "Bem-Te-Vi";
            PlayerSettings.defaultScreenWidth = 1440; PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.allowFullscreenSwitch = true;
            PlayerSettings.resizableWindow = true; PlayerSettings.runInBackground = false;
            PlayerSettings.bundleVersion = "0.12.0";
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            // Use only Input System Package (New); the editor may require a restart.
            var player = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var handling = player.FindProperty("activeInputHandler");
            if (handling != null && handling.intValue != 1)
            {
                handling.intValue = 1; player.ApplyModifiedPropertiesWithoutUndo();
                Debug.LogWarning("Input System Package (New) ativado. Reinicie o editor antes de jogar ou compilar.");
            }
            if (!File.Exists(Scene))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Scene));
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                new GameObject("Bem-Te-Vi").AddComponent<BemTeViGame>();
                EditorSceneManager.SaveScene(scene, Scene);
            }
            BemTeViSceneBuilder.EnsureScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("Projeto preparado. Abra Main.unity e pressione Play.");
        }
        [MenuItem("Bem-Te-Vi/2. Verificar conteudo")]
        public static void Validate()
        {
            BemTeViRenderPipeline.Validate();
            // Validate the serialized slots before loading models or querying shaders.
            var bindings = JsonUtility.FromJson<MaterialBindings>(Resources.Load<TextAsset>("BemTeVi/material-bindings").text);
            foreach (var source in bindings.materials)
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(source.material);
                if (material == null) throw new BuildFailedException("Material ausente: " + source.material);
                BemTeViMaterialRepair.ValidateDiffuseTextureSlots(material, source.material);
            }
            var inputPackage = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(Mouse).Assembly);
            if (inputPackage == null || inputPackage.version != "1.20.1")
                throw new BuildFailedException("Resolva o pacote Input System 1.20.1 antes de continuar.");
            var player = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var handling = player.FindProperty("activeInputHandler");
            if (handling == null || handling.intValue != 1)
                throw new BuildFailedException("Selecione Active Input Handling: Input System Package (New) e reinicie o editor.");
#if !ENABLE_INPUT_SYSTEM
            throw new BuildFailedException("O Input System ainda não está ativo nesta sessão. Reinicie o editor.");
#endif
            var text = Resources.Load<TextAsset>("BemTeVi/catalog");
            if (text == null) throw new BuildFailedException("Catálogo ausente.");
            var catalog = JsonUtility.FromJson<Catalog>(text.text);
            if (catalog.maps.Length != 3) throw new BuildFailedException("São necessários três mapas.");
            ValidateStages(catalog);
            var expectedScenes = new[] { "Main", "inicio", "quarto" }.Concat(catalog.maps.SelectMany(m => new[] { m.junction.id, m.challenge.id }.Concat(m.rooms.Select(r => r.id)))).OrderBy(x => x);
            if (!EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => Path.GetFileNameWithoutExtension(s.path)).OrderBy(x => x).SequenceEqual(expectedScenes))
                throw new BuildFailedException("As 18 cenas precisam estar no build. Execute Preparar projeto.");
            var ids = catalog.maps.SelectMany(m => m.AllTargets).Select(p => p.id).ToArray();
            if (ids.Distinct().Count() != ids.Length || ids.Length != 162) throw new BuildFailedException("IDs duplicados ou quantidade de alvos inválida.");
            foreach (var map in catalog.maps)
            {
                if (map.rooms.Length != 3 || map.AllTargets.Count() != 54) throw new BuildFailedException("Mapa incompleto: " + map.id);
                foreach (var room in map.rooms)
                    if (string.IsNullOrEmpty(room.id) || string.IsNullOrEmpty(room.riddle))
                        throw new BuildFailedException("Charada ou ID da tela ausente: " + room.name);
                foreach (var p in map.rooms.SelectMany(r => r.objects))
                {
                    var model = Resources.Load<GameObject>("BemTeVi/Models/" + p.asset);
                    if (model == null || model.GetComponentsInChildren<MeshFilter>().Length == 0)
                        throw new BuildFailedException("Modelo não importado: " + p.asset);
                    foreach (var renderer in model.GetComponentsInChildren<Renderer>())
                    foreach (var mat in renderer.sharedMaterials)
                    {
                        if (mat == null || mat.shader == null || mat.shader.name != BemTeViRenderPipeline.LitShader)
                            throw new BuildFailedException("Material inválido: " + p.asset);
                        string expected = BemTeViImporter.Materials + p.asset + "/" + mat.name + ".mat";
                        if (AssetDatabase.GetAssetPath(mat) != expected)
                            throw new BuildFailedException("Material original não vinculado: " + p.asset + ". Execute 5. Restaurar materiais originais.");
                    }
                }
            }
            foreach (var source in bindings.materials)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(source.material);
                var expected = new Color(source.color[0], source.color[1], source.color[2], source.color[3]);
                if (mat == null || mat.shader == null || mat.shader.name != BemTeViRenderPipeline.LitShader)
                    throw new BuildFailedException("Material não convertido para URP/Lit: " + source.material);
                var actual = mat.GetColor("_BaseColor");
                if ((actual - expected).maxColorComponent > .002f || (expected - actual).maxColorComponent > .002f)
                    throw new BuildFailedException("Cor diferente do MTL original: " + source.material);
                var expectedTexture = string.IsNullOrEmpty(source.texture) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(source.texture);
                if (!string.IsNullOrEmpty(source.texture) && (expectedTexture == null || expectedTexture.dimension != TextureDimension.Tex2D))
                    throw new BuildFailedException("Textura precisa ser Texture2D: " + source.texture + ". Execute 5. Restaurar materiais originais.");
                if (mat.GetTexture("_BaseMap") != expectedTexture)
                    throw new BuildFailedException("Textura original não vinculada: " + source.material);
                var legacyTexture = mat.GetTexture("_MainTex");
                if (legacyTexture != null && legacyTexture.dimension != TextureDimension.Tex2D)
                    throw new BuildFailedException("Textura incompatível em _MainTex: " + source.material + ". Execute 5. Restaurar materiais originais.");
                if (!string.IsNullOrEmpty(source.texture))
                {
                    var importer = AssetImporter.GetAtPath(source.texture) as TextureImporter;
                    if (importer == null || importer.textureType != TextureImporterType.Default || importer.textureShape != TextureImporterShape.Texture2D ||
                        importer.wrapMode != TextureWrapMode.Repeat || !importer.sRGBTexture || importer.maxTextureSize < 1024)
                        throw new BuildFailedException("Importação de textura incorreta: " + source.texture + ". Execute 5. Restaurar materiais originais.");
                }
            }
            var bank = Resources.Load<NarrationBank>("BemTeVi/Narration/NarrationBank");
            if (bank == null || bank.readings == null || bank.readings.Length != 27)
                throw new BuildFailedException("Os 27 espaços de narração precisam estar configurados.");
            var rooms = catalog.maps.SelectMany(m => m.rooms).ToArray();
            if (bank.discoveries == null || bank.discoveries.Length != ids.Length ||
                ids.Any(id => bank.discoveries.Count(d => d != null && d.itemId == id) != 1))
                throw new BuildFailedException("Cada objeto precisa de um campo de confirmação no NarrationBank.");
            foreach (var room in rooms)
            for (int round = 0; round < Progress.RoundCount; round++)
                if (bank.readings.Count(r => r != null && r.roomId == room.id && r.round == round && r.recordingScript == room.riddles[round]) != 1)
                    throw new BuildFailedException("Roteiro de gravação não corresponde à charada: " + room.id + "/" + round);
            foreach (string art in new[] { "panel_brown", "panel_border_brown", "banner_classic_curtain", "minimap_icon_star_yellow" })
                if (Resources.Load<Sprite>("BemTeVi/UI/" + art) == null) throw new BuildFailedException("Sprite não importado: " + art);
            foreach (string font in new[] { "Itim-Regular", "Boogaloo-Regular" })
                if (Resources.Load<Font>("BemTeVi/UI/" + font) == null) throw new BuildFailedException("Fonte não importada: " + font);
            int recordings = rooms.Sum(r => Enumerable.Range(0, 3).Count(round => bank.ForRoom(r.id, round) != null));
            Debug.Log("Conteúdo íntegro: URP e Input System configurados; 3 mundos, 27 charadas, 162 alvos, materiais URP/Lit com cores e texturas originais; " + recordings + "/27 gravações humanas. Ainda é necessário testar a jogabilidade.");
        }
        [Serializable] sealed class MaterialBindings { public MaterialBinding[] materials; }
        [Serializable] sealed class MaterialBinding { public string model, name, material, texture; public float[] color; }
        static void ValidateStages(Catalog catalog)
        {
            if (catalog.hub == null || catalog.hub.objects == null)
                throw new BuildFailedException("O quarto de seleção está ausente do catálogo.");
            var entrances = catalog.hub.objects.Where(p => p.action == "world").Select(p => p.destination).ToArray();
            if (entrances.Length != 3 || !entrances.OrderBy(x => x).SequenceEqual(catalog.maps.Select(m => m.id).OrderBy(x => x)))
                throw new BuildFailedException("O quarto precisa de uma entrada para cada mundo.");
            foreach (var map in catalog.maps)
            {
                if (map.puzzleRounds == null || map.puzzleRounds.Length != 3 || map.puzzleRounds.Any(r => r.solution == null || r.solution.Length < 3 || r.clues == null || r.clues.Length != 3))
                    throw new BuildFailedException("As três rodadas de mecanismos estão incompletas: " + map.id);
                if (map.challenge == null) throw new BuildFailedException("Cenário do desafio ausente: " + map.id);
                if (map.junction == null || map.gates == null || map.gates.Length != 2)
                    throw new BuildFailedException("Caminho ou duas passagens ausentes: " + map.id);
                foreach (var room in map.rooms)
                {
                    if (room.riddles == null || room.riddles.Length != 3 ||
                        Enumerable.Range(0, 3).Any(round => room.objects.Count(p => !string.IsNullOrEmpty(p.id) && p.round == round) != 6 || room.riddles[round].Split('\n').Length != 6))
                        throw new BuildFailedException("São necessárias três rodadas de seis alvos e seis versos: " + room.id);
                    if (room.objects.Count(p => p.action == "exit") < 2)
                        throw new BuildFailedException("Cada maquete precisa de pelo menos duas passagens físicas: " + room.id);
                }
                var stages = map.rooms.Concat(new[] { map.junction, map.challenge }).ToArray();
                foreach (var stage in stages)
                foreach (var passage in stage.objects.Where(p => p.action == "exit"))
                    if (passage.destination != "quarto" && !stages.Any(r => r.id == passage.destination))
                        throw new BuildFailedException("Passagem sem destino: " + stage.id + " / " + passage.destination);
                if (!map.junction.objects.Any(p => p.action == "exit" && p.destination == "quarto"))
                    throw new BuildFailedException("A entrada do mundo precisa retornar ao quarto.");
                foreach (var gate in map.gates)
                {
                    if (gate.sourceRoom < 0 || gate.sourceRoom > 1 || gate.roomId != map.rooms[gate.sourceRoom + 1].id ||
                        !map.rooms[gate.sourceRoom].objects.Any(p => p.id == gate.tokenId) ||
                        stages.Sum(r => r.objects.Count(p => p.action == "gate" && p.destination == gate.id)) != 1)
                        throw new BuildFailedException("Peça ou mecanismo inválido: " + gate.id);
                }
                foreach (string action in map.puzzle == "lock" ? new[] { "digit" } : map.puzzle == "memory" ? new[] { "signal" } : new[] { "supply", "destination" })
                {
                    int count = action == "signal" ? 4 : 3;
                    if (!map.challenge.objects.Where(p => p.action == action).Select(p => p.value).OrderBy(v => v).SequenceEqual(Enumerable.Range(0, count)))
                        throw new BuildFailedException("Peças incompletas/duplicadas no desafio: " + map.id + "/" + action);
                }
            }
            foreach (var stage in catalog.maps.SelectMany(m => m.rooms.Concat(new[] { m.junction, m.challenge })).Concat(new[] { catalog.hub }))
            {
                if (stage.camera == null || stage.camera.Length != 3 || stage.lookAt == null || stage.lookAt.Length != 3 || stage.fieldOfView < 20 || stage.fieldOfView > 60)
                    throw new BuildFailedException("Enquadramento inválido: " + stage.id);
                foreach (var prop in stage.objects)
                {
                    // Match RoomView's extension-free Resources lookup: models may be OBJ or FBX.
                    var model = Resources.Load<GameObject>("BemTeVi/Models/" + prop.asset);
                    if (model == null)
                        throw new BuildFailedException("Modelo ausente ou não importado: " + BemTeViImporter.Models + prop.asset + " (OBJ/FBX). Aguarde a importação terminar.");
                    string path = AssetDatabase.GetAssetPath(model);
                    ValidateImporterVersion(path, "ModelImporter", 25);
                }
            }
            foreach (string path in Directory.GetFiles("Assets/Resources/BemTeVi/Audio", "*.ogg", SearchOption.AllDirectories))
                ValidateImporterVersion(path, "AudioImporter", 6);
        }
        static void ValidateImporterVersion(string path, string importer, int minimum)
        {
            string meta = File.ReadAllText(path + ".meta");
            var block = Regex.Match(meta, "(?m)^" + importer + @":\r?\n((?:[ \t].*\r?\n|\r?\n)*)");
            var version = Regex.Match(block.Value, @"(?m)^  serializedVersion: (\d+)");
            if (!version.Success || int.Parse(version.Groups[1].Value) < minimum)
                throw new BuildFailedException("Importador sem versão suportada: " + path + ". Feche Unity e siga Docs/CORRECAO_IMPORTADORES.md.");
        }
        [MenuItem("Bem-Te-Vi/3. Compilar Windows")]
        public static void Windows()
        {
            const BuildTarget target = BuildTarget.StandaloneWindows64;
            const string path = "Builds/Windows/BemTeVi.exe";
            if (!BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target), target))
                throw new BuildFailedException("Instale o módulo " + target + " pelo Unity Hub.");
            Prepare(); Validate(); Directory.CreateDirectory(Path.GetDirectoryName(path));
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = scenes, locationPathName = path, target = target, options = BuildOptions.None });
            if (report.summary.result != BuildResult.Succeeded) throw new BuildFailedException("A compilação falhou. Veja o Console.");
            Debug.Log("Build concluído: " + Path.GetFullPath(path));
        }
    }
}
