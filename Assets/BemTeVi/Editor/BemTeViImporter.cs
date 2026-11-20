using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;

namespace BemTeVi.Editor
{
    // Materials are persistent translations of the source MTL files, scoped per model.
    // This avoids name collisions between the different packs' "colormap" materials.
    public sealed class BemTeViImporter : AssetPostprocessor
    {
        internal const string Models = "Assets/Resources/BemTeVi/Models/";
        internal const string Materials = "Assets/Resources/BemTeVi/Materials/";
        internal const string UI = "Assets/Resources/BemTeVi/UI/";
        [Serializable] sealed class MaterialBindings { public ModelBinding[] materials; }
        [Serializable] sealed class ModelBinding { public string model, name; }
        bool IsModel => assetPath.StartsWith(Models, StringComparison.Ordinal);
        public override uint GetVersion() => 12;
        internal static Material OriginalMaterial(string modelPath, string materialName)
        {
            string model = modelPath.Substring(Models.Length);
            model = model.Substring(0, model.LastIndexOf('.'));
            string name = materialName.Replace(" (Instance)", "").Trim();
            return AssetDatabase.LoadAssetAtPath<Material>(Materials + model + "/" + name + ".mat");
        }
        internal static bool Remap(ModelImporter model, string path)
        {
            bool changed = false;
            var current = model.GetExternalObjectMap();
            var bindings = JsonUtility.FromJson<MaterialBindings>(File.ReadAllText("Assets/Resources/BemTeVi/material-bindings.json"));
            string asset = path.Substring(Models.Length);
            asset = asset.Substring(0, asset.LastIndexOf('.'));
            foreach (string name in bindings.materials.Where(b => b.model == asset).Select(b => b.name).Distinct())
            {
                var material = OriginalMaterial(path, name);
                if (material == null) continue; // Leave unrelated/older models without a shipped material alone.
                var key = new AssetImporter.SourceAssetIdentifier(typeof(Material), name);
                if (current.TryGetValue(key, out var previous) && previous != null &&
                    AssetDatabase.GetAssetPath(previous) == AssetDatabase.GetAssetPath(material)) continue;
                model.AddRemap(key, material); changed = true;
            }
            return changed;
        }
        void OnPreprocessModel()
        {
            if (!IsModel) return;
            var model = (ModelImporter)assetImporter;
            model.importAnimation = false; model.importCameras = false; model.importLights = false;
            model.isReadable = false; model.addCollider = false;
            // Smooth lighting across rounded toy surfaces, retaining sharp right-angle edges.
            // This changes import shading only: source vertices, UVs and artwork remain untouched.
            model.importNormals = assetPath.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase) ? ModelImporterNormals.Import : ModelImporterNormals.Calculate;
            model.normalSmoothingSource = ModelImporterNormalSmoothingSource.FromAngle;
            model.normalSmoothingAngle = 75;
            // Only importer settings here. Mutating external remaps during import can restart it.
            model.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            model.materialLocation = ModelImporterMaterialLocation.InPrefab;
            model.materialName = ModelImporterMaterialName.BasedOnMaterialName;
        }
        Material OnAssignMaterialModel(Material material, Renderer renderer)
        {
            return IsModel ? OriginalMaterial(assetPath, material.name) : null;
        }
        void OnPreprocessTexture()
        {
            ConfigureTexture((TextureImporter)assetImporter, assetPath);
        }
        internal static bool ConfigureTexture(TextureImporter texture, string path)
        {
            bool ui = path.StartsWith(UI, StringComparison.Ordinal);
            if (!ui && !path.StartsWith(Models, StringComparison.Ordinal)) return false;
            var type = ui ? TextureImporterType.Sprite : TextureImporterType.Default;
            bool toyPack = path.Contains("/furniture-kit/") || path.Contains("/toy-car-kit/") || path.Contains("/food-kit/") || path.Contains("/polyhaven-duck/") || path.Contains("/mini-characters/") || path.Contains("/holiday-kit/") || path.Contains("/train-kit/") || path.Contains("/brick-kit/");
            var filter = ui || toyPack ? FilterMode.Bilinear : FilterMode.Point;
            var wrap = ui ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
            string name = Path.GetFileNameWithoutExtension(path);
            int borderSize = name == "panel_border_brown" ? 32 : name.StartsWith("panel_") ? 20 : 0;
            var border = new Vector4(borderSize, borderSize, borderSize, borderSize);
            bool changed = texture.textureType != type || texture.textureShape != TextureImporterShape.Texture2D ||
                !texture.sRGBTexture || texture.mipmapEnabled || texture.filterMode != filter ||
                texture.wrapModeU != wrap || texture.wrapModeV != wrap || texture.wrapModeW != wrap ||
                texture.maxTextureSize != 1024 || texture.textureCompression != TextureImporterCompression.Uncompressed;
            if (ui) changed |= texture.spriteImportMode != SpriteImportMode.Single ||
                texture.spritePixelsPerUnit != 100 || texture.spriteBorder != border || !texture.alphaIsTransparency;
            if (!changed) return false;
            texture.textureType = type;
            // Default texture type alone does not reset a previously imported Cubemap.
            texture.textureShape = TextureImporterShape.Texture2D;
            texture.sRGBTexture = true; texture.mipmapEnabled = false;
            texture.filterMode = filter; texture.maxTextureSize = 1024;
            texture.wrapMode = wrap; // Repeat preserves the original negative UVs on the robots.
            texture.textureCompression = TextureImporterCompression.Uncompressed;
            if (ui)
            {
                texture.spriteImportMode = SpriteImportMode.Single;
                texture.spritePixelsPerUnit = 100; texture.spriteBorder = border;
                texture.alphaIsTransparency = true;
            }
            return true;
        }
        void OnPreprocessAudio()
        {
            bool voice = assetPath.StartsWith("Assets/Resources/BemTeVi/Narration/", StringComparison.Ordinal);
            if (!voice && !assetPath.StartsWith("Assets/Resources/BemTeVi/Audio/", StringComparison.Ordinal)) return;
            var audio = (AudioImporter)assetImporter; audio.forceToMono = true;
            var settings = audio.defaultSampleSettings;
            settings.loadType = voice ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.Vorbis; settings.quality = voice ? .8f : .6f;
            audio.defaultSampleSettings = settings;
        }
    }
    public static class BemTeViMaterialRepair
    {
        static bool repairing;
        const string BindingsPath = "Assets/Resources/BemTeVi/material-bindings.json";
        // The source manifest describes diffuse-only materials (Kd/map_Kd).
        // Read serialized slots before HasProperty/GetTexture: Unity validates all
        // saved textures on first material access, even when querying _MainTex.
        internal static bool RestoreDiffuseTextureSlots(Material material)
        {
            bool changed = false;
            using (var serialized = new SerializedObject(material))
            {
                var slots = serialized.FindProperty("m_SavedProperties.m_TexEnvs");
                for (int i = 0; slots != null && i < slots.arraySize; i++)
                {
                    var slot = slots.GetArrayElementAtIndex(i);
                    string name = slot.FindPropertyRelative("first").stringValue;
                    if (name == "_BaseMap" || name == "_MainTex") continue;
                    var texture = slot.FindPropertyRelative("second.m_Texture");
                    if (texture.objectReferenceValue == null) continue;
                    texture.objectReferenceValue = null;
                    changed = true;
                }
                if (changed) serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            // These features must not remain enabled after restoring diffuse-only maps.
            foreach (string keyword in new[] { "_NORMALMAP", "_PARALLAXMAP", "_OCCLUSIONMAP",
                "_METALLICSPECGLOSSMAP", "_SPECGLOSSMAP", "_DETAIL_MULX2", "_DETAIL_SCALED" })
            {
                if (!material.IsKeywordEnabled(keyword)) continue;
                material.DisableKeyword(keyword); changed = true;
            }
            return changed;
        }
        internal static void ValidateDiffuseTextureSlots(Material material, string path)
        {
            using (var serialized = new SerializedObject(material))
            {
                var slots = serialized.FindProperty("m_SavedProperties.m_TexEnvs");
                for (int i = 0; slots != null && i < slots.arraySize; i++)
                {
                    var slot = slots.GetArrayElementAtIndex(i);
                    string name = slot.FindPropertyRelative("first").stringValue;
                    if (name == "_BaseMap" || name == "_MainTex") continue;
                    if (slot.FindPropertyRelative("second.m_Texture").objectReferenceValue != null)
                        throw new BuildFailedException("Textura fora do mapa de cor original em " + name + ": " + path + ". Execute 5. Restaurar materiais originais.");
                }
            }
        }
        [MenuItem("Bem-Te-Vi/5. Restaurar materiais originais")]
        public static void Repair()
        {
            if (repairing || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new BuildFailedException("Aguarde o fim da importação/compilação e saia do Play antes de restaurar os materiais.");
            if (!Directory.Exists(BemTeViImporter.Models)) return;
            repairing = true;
            try
            {
                int textures = 0, materials = 0, models = 0;
                // This method is only called by menu/build commands, never by import callbacks.
                foreach (string folder in new[] { BemTeViImporter.Models, BemTeViImporter.UI })
                {
                    if (!Directory.Exists(folder)) continue;
                    foreach (string file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories).Where(p => p.EndsWith(".png") || p.EndsWith(".jpg") || p.EndsWith(".exr")))
                    {
                        string path = file.Replace('\\', '/');
                        var texture = AssetImporter.GetAtPath(path) as TextureImporter;
                        if (texture != null && BemTeViImporter.ConfigureTexture(texture, path))
                        { texture.SaveAndReimport(); textures++; }
                    }
                }
                if (!File.Exists(BindingsPath)) throw new BuildFailedException("Arquivo de vínculos de materiais ausente.");
                var bindings = JsonUtility.FromJson<MaterialBindings>(File.ReadAllText(BindingsPath));
                if (bindings?.materials == null) throw new BuildFailedException("Vínculos de materiais inválidos.");
                var shader = Shader.Find(BemTeViRenderPipeline.LitShader);
                if (shader == null) throw new BuildFailedException("Aguarde a instalação do URP antes de restaurar os materiais.");
                foreach (var source in bindings.materials)
                {
                    Texture2D texture = null;
                    if (!string.IsNullOrEmpty(source.texture))
                    {
                        texture = AssetDatabase.LoadAssetAtPath<Texture2D>(source.texture);
                        if (texture == null || texture.dimension != TextureDimension.Tex2D)
                            throw new BuildFailedException("Textura não importada como Texture2D: " + source.texture);
                    }
                    var mat = AssetDatabase.LoadAssetAtPath<Material>(source.material);
                    if (mat == null) throw new BuildFailedException("Material ausente: " + source.material);
                    bool changed = RestoreDiffuseTextureSlots(mat);
                    // Correct a legacy Standard texture before switching shaders, if necessary.
                    changed |= SetTexture(mat, "_MainTex", texture);
                    if (mat.shader != shader) { mat.shader = shader; mat.shaderKeywords = new string[0]; changed = true; }
                    changed |= SetTexture(mat, "_BaseMap", texture);
                    changed |= SetTexture(mat, "_MainTex", texture);
                    var color = new Color(source.color[0], source.color[1], source.color[2], source.color[3]);
                    if (mat.GetColor("_BaseColor") != color) { mat.SetColor("_BaseColor", color); changed = true; }
                    if (mat.HasProperty("_Color") && mat.GetColor("_Color") != color)
                    { mat.SetColor("_Color", color); changed = true; }
                    if (changed) { EditorUtility.SetDirty(mat); materials++; }
                }
                if (materials > 0) AssetDatabase.SaveAssets();
                // Persist remaps only after all textures and materials are available.
                foreach (string file in Directory.GetFiles(BemTeViImporter.Models, "*", SearchOption.AllDirectories).Where(p => p.EndsWith(".obj") || p.EndsWith(".fbx")))
                {
                    string path = file.Replace('\\', '/');
                    var model = AssetImporter.GetAtPath(path) as ModelImporter;
                    if (model != null && BemTeViImporter.Remap(model, path)) { model.SaveAndReimport(); models++; }
                }
                Debug.Log($"Reparo concluído: {textures} texturas 2D, {materials} materiais e {models} vínculos de modelos corrigidos. Arquivos já corretos não foram reimportados pelo comando.");
            }
            finally { repairing = false; }
        }
        static bool SetTexture(Material material, string property, Texture2D texture)
        {
            if (!material.HasProperty(property) || material.GetTexture(property) == texture) return false;
            material.SetTexture(property, texture); return true;
        }
        [Serializable] sealed class MaterialBindings { public MaterialBinding[] materials; }
        [Serializable] sealed class MaterialBinding { public string material, texture; public float[] color; }
    }
}
