using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OlhosDeCacador.Editor
{
    /// <summary>Configures the game's URP assets without changing models, textures or narration.</summary>
    public static class HunterRenderPipeline
    {
        public const string PipelinePath = "Assets/Hunter/Settings/HunterURP.asset";
        public const string RendererPath = "Assets/Hunter/Settings/HunterRenderer.asset";
        public const string LitShader = "Universal Render Pipeline/Lit";

        [MenuItem("Olhos de Cacador/6. Configurar URP")]
        public static void EnsureConfigured()
        {
            // Shipped Graphics/Quality assets already select URP. Run repairs explicitly,
            // outside the import cycle, instead of scheduling asset writes after every reload.
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new BuildFailedException("Aguarde o fim da importação/compilação e saia do Play antes de configurar URP.");
            // The serialized assets ship with the project. Recreate only if missing.
            if (!AssetDatabase.IsValidFolder("Assets/Hunter/Settings"))
                AssetDatabase.CreateFolder("Assets/Hunter", "Settings");
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (renderer == null)
            {
                if (File.Exists(RendererPath)) throw new BuildFailedException("Renderer URP não importado. Confira os erros do Package Manager.");
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                renderer.renderingMode = RenderingMode.Forward;
                renderer.depthPrimingMode = DepthPrimingMode.Disabled;
                AssetDatabase.CreateAsset(renderer, RendererPath);
            }
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                if (File.Exists(PipelinePath)) throw new BuildFailedException("Pipeline URP não importado. Confira os erros do Package Manager.");
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                pipeline.supportsHDR = false;
                pipeline.msaaSampleCount = 2; pipeline.renderScale = 1;
                pipeline.supportsCameraDepthTexture = false;
                pipeline.supportsCameraOpaqueTexture = false;
                pipeline.mainLightShadowmapResolution = 1024;
                pipeline.shadowDistance = 35; pipeline.shadowCascadeCount = 1;
                pipeline.useSRPBatcher = true;
                var defaults = new SerializedObject(pipeline);
                // URP 17.6 exposes these shadow properties as read-only to project scripts.
                // Configure the serialized fields, as the URP Inspector does, and keep
                // the editor's shadow-variant flag consistent with the two shadow settings.
                defaults.FindProperty("m_MainLightShadowsSupported").boolValue = true;
                defaults.FindProperty("m_AdditionalLightShadowsSupported").boolValue = false;
                defaults.FindProperty("m_AnyShadowsSupported").boolValue = true;
                defaults.FindProperty("m_SoftShadowsSupported").boolValue = true;
                defaults.FindProperty("m_AdditionalLightsRenderingMode").intValue = 1;
                defaults.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
            }

            // Keep the project's renderer reference valid while preserving its quality tuning.
            var settings = new SerializedObject(pipeline);
            var renderers = settings.FindProperty("m_RendererDataList");
            renderers.arraySize = 1;
            renderers.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
            settings.FindProperty("m_DefaultRendererIndex").intValue = 0;
            settings.FindProperty("m_AdditionalLightsRenderingMode").intValue = 1;
            settings.FindProperty("m_AdditionalLightsPerObjectLimit").intValue = 2;
            settings.ApplyModifiedPropertiesWithoutUndo();
            if (GraphicsSettings.defaultRenderPipeline != pipeline)
                GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.ForEach(() =>
            {
                if (QualitySettings.renderPipeline != pipeline) QualitySettings.renderPipeline = pipeline;
            });
            // URP initializes/registers its own Global Settings and package resources on activation.
            AssetDatabase.SaveAssets();
        }

        public static void Validate()
        {
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(UniversalRenderPipelineAsset).Assembly);
            if (package == null || package.version != "17.6.0")
                throw new BuildFailedException("Este projeto requer o pacote URP 17.6.0 da Unity 6000.6.4f1.");
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (pipeline == null || renderer == null || GraphicsSettings.defaultRenderPipeline != pipeline)
                throw new BuildFailedException("URP não configurado. Execute 1. Preparar projeto.");
            for (int i = 0; i < QualitySettings.names.Length; i++)
                if (QualitySettings.GetRenderPipelineAssetAt(i) != pipeline)
                    throw new BuildFailedException("Qualidade sem o URP do jogo: " + QualitySettings.names[i]);
            var settings = new SerializedObject(pipeline);
            var renderers = settings.FindProperty("m_RendererDataList");
            if (renderers.arraySize != 1 || renderers.GetArrayElementAtIndex(0).objectReferenceValue != renderer ||
                settings.FindProperty("m_DefaultRendererIndex").intValue != 0)
                throw new BuildFailedException("Referência do renderer inválida. Execute 6. Configurar URP.");
            if (renderer.renderingMode != RenderingMode.Forward)
                throw new BuildFailedException("HunterRenderer deve usar Forward para esta configuração Windows.");
            if (Shader.Find(LitShader) == null)
                throw new BuildFailedException("Shader URP/Lit ausente. Aguarde a resolução do pacote URP 17.6.0.");
        }
    }
}
