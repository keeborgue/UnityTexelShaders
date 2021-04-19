using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Editor
{
    public static class TexelLitGUI
    {
        private static readonly int Surface = Shader.PropertyToID("_Surface");
        private static readonly int Mode = Shader.PropertyToID("_WorkflowMode");
        private static readonly int SpecGlossMap = Shader.PropertyToID("_SpecGlossMap");
        private static readonly int MetallicGlossMap = Shader.PropertyToID("_MetallicGlossMap");
        private static readonly int SpecularHighlights = Shader.PropertyToID("_SpecularHighlights");
        private static readonly int EnvironmentReflections = Shader.PropertyToID("_EnvironmentReflections");
        private static readonly int OcclusionMap = Shader.PropertyToID("_OcclusionMap");
        private static readonly int SmoothnessTextureChannel = Shader.PropertyToID("_SmoothnessTextureChannel");

        public enum WorkflowMode
        {
            Specular = 0,
            Metallic
        }

        private enum SmoothnessMapChannel
        {
            SpecularMetallicAlpha,
            AlbedoAlpha
        }

        public static class Styles
        {
            public static readonly GUIContent WorkflowModeText = new GUIContent("Workflow Mode",
                "Select a workflow that fits your textures. Choose between Metallic or Specular.");

            public static readonly GUIContent SpecularMapText =
                new GUIContent("Specular Map", "Sets and configures the map and color for the Specular workflow.");

            public static readonly GUIContent MetallicMapText =
                new GUIContent("Metallic Map", "Sets and configures the map for the Metallic workflow.");

            public static readonly GUIContent SmoothnessText = new GUIContent("Smoothness",
                "Controls the spread of highlights and reflections on the surface.");

            public static readonly GUIContent SmoothnessMapChannelText =
                new GUIContent("Source",
                    "Specifies where to sample a smoothness map from. By default, uses the alpha channel for your map.");

            public static readonly GUIContent HighlightsText = new GUIContent("Specular Highlights",
                "When enabled, the Material reflects the shine from direct lighting.");

            public static readonly GUIContent ReflectionsText =
                new GUIContent("Environment Reflections",
                    "When enabled, the Material samples reflections from the nearest Reflection Probes or Lighting Probe.");

            public static readonly GUIContent OcclusionText = new GUIContent("Occlusion Map",
                "Sets an occlusion map to simulate shadowing from ambient lighting.");

            public const string PosterizationColorText = "Color Posterization Steps";
            public const string PosterizationLightText = "Light Posterization Steps";

            public static readonly string[] MetallicSmoothnessChannelNames = {"Metallic Alpha", "Albedo Alpha"};
            public static readonly string[] SpecularSmoothnessChannelNames = {"Specular Alpha", "Albedo Alpha"};
        }

        public readonly struct TexelLitProperties
        {
            // Surface Option Props
            public readonly MaterialProperty workflowMode;

            // Surface Input Props
            public readonly MaterialProperty metallic;
            public readonly MaterialProperty specColor;
            public readonly MaterialProperty metallicGlossMap;
            public readonly MaterialProperty specGlossMap;
            public readonly MaterialProperty smoothness;
            public readonly MaterialProperty smoothnessMapChannel;
            public readonly MaterialProperty bumpMapProp;
            public readonly MaterialProperty bumpScaleProp;
            public readonly MaterialProperty occlusionStrength;
            public readonly MaterialProperty occlusionMap;

            // Advanced Props
            public readonly MaterialProperty highlights;
            public readonly MaterialProperty reflections;

            // Posterization Props
            public readonly MaterialProperty posterizationStepCount;
            public readonly MaterialProperty lightPosterizationStepCount;

            public TexelLitProperties(MaterialProperty[] properties)
            {
                // Surface Option Props
                workflowMode = BaseShaderGUI.FindProperty("_WorkflowMode", properties, false);
                // Surface Input Props
                metallic = BaseShaderGUI.FindProperty("_Metallic", properties);
                specColor = BaseShaderGUI.FindProperty("_SpecColor", properties, false);
                metallicGlossMap = BaseShaderGUI.FindProperty("_MetallicGlossMap", properties);
                specGlossMap = BaseShaderGUI.FindProperty("_SpecGlossMap", properties, false);
                smoothness = BaseShaderGUI.FindProperty("_Smoothness", properties, false);
                smoothnessMapChannel = BaseShaderGUI.FindProperty("_SmoothnessTextureChannel", properties, false);
                bumpMapProp = BaseShaderGUI.FindProperty("_BumpMap", properties, false);
                bumpScaleProp = BaseShaderGUI.FindProperty("_BumpScale", properties, false);
                occlusionStrength = BaseShaderGUI.FindProperty("_OcclusionStrength", properties, false);
                occlusionMap = BaseShaderGUI.FindProperty("_OcclusionMap", properties, false);
                // Advanced Props
                highlights = BaseShaderGUI.FindProperty("_SpecularHighlights", properties, false);
                reflections = BaseShaderGUI.FindProperty("_EnvironmentReflections", properties, false);
                //Posterization Props
                posterizationStepCount = BaseShaderGUI.FindProperty("_PosterizationStepCount", properties, false);
                lightPosterizationStepCount = BaseShaderGUI.FindProperty("_LightPosterizationStepCount", properties, false);
            }
        }

        public static void Inputs(TexelLitProperties properties, MaterialEditor materialEditor, Material material)
        {
            DoMetallicSpecularArea(properties, materialEditor, material);
            BaseShaderGUI.DrawNormalArea(materialEditor, properties.bumpMapProp, properties.bumpScaleProp);

            if (properties.occlusionMap != null)
            {
                materialEditor.TexturePropertySingleLine(Styles.OcclusionText, properties.occlusionMap,
                    properties.occlusionMap.textureValue != null ? properties.occlusionStrength : null);
            }
            
            if (properties.posterizationStepCount != null)
            {
                materialEditor.VectorProperty(
                    properties.posterizationStepCount,
                    Styles.PosterizationColorText);
            }

            if (properties.lightPosterizationStepCount != null)
            {
                materialEditor.VectorProperty(
                    properties.lightPosterizationStepCount,
                    Styles.PosterizationLightText);
            }
        }

        private static void DoMetallicSpecularArea(TexelLitProperties properties, MaterialEditor materialEditor, Material material)
        {
            string[] smoothnessChannelNames;
            bool hasGlossMap;
            if (properties.workflowMode == null ||
                (WorkflowMode) properties.workflowMode.floatValue == WorkflowMode.Metallic)
            {
                hasGlossMap = properties.metallicGlossMap.textureValue != null;
                smoothnessChannelNames = Styles.MetallicSmoothnessChannelNames;
                materialEditor.TexturePropertySingleLine(Styles.MetallicMapText, properties.metallicGlossMap,
                    hasGlossMap ? null : properties.metallic);
            }
            else
            {
                hasGlossMap = properties.specGlossMap.textureValue != null;
                smoothnessChannelNames = Styles.SpecularSmoothnessChannelNames;
                BaseShaderGUI.TextureColorProps(materialEditor, Styles.SpecularMapText, properties.specGlossMap,
                    hasGlossMap ? null : properties.specColor);
            }
            EditorGUI.indentLevel++;
            DoSmoothness(properties, material, smoothnessChannelNames);
            EditorGUI.indentLevel--;
        }

        private static void DoSmoothness(TexelLitProperties properties, Material material, string[] smoothnessChannelNames)
        {
            var opaque = ((BaseShaderGUI.SurfaceType) material.GetFloat(Surface) ==
                          BaseShaderGUI.SurfaceType.Opaque);
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = properties.smoothness.hasMixedValue;
            var smoothness = EditorGUILayout.Slider(Styles.SmoothnessText, properties.smoothness.floatValue, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
                properties.smoothness.floatValue = smoothness;
            EditorGUI.showMixedValue = false;

            if (properties.smoothnessMapChannel != null) // smoothness channel
            {
                EditorGUI.indentLevel++;
                EditorGUI.BeginDisabledGroup(!opaque);
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = properties.smoothnessMapChannel.hasMixedValue;
                var smoothnessSource = (int) properties.smoothnessMapChannel.floatValue;
                if (opaque)
                    smoothnessSource = EditorGUILayout.Popup(Styles.SmoothnessMapChannelText, smoothnessSource,
                        smoothnessChannelNames);
                else
                    EditorGUILayout.Popup(Styles.SmoothnessMapChannelText, 0, smoothnessChannelNames);
                if (EditorGUI.EndChangeCheck())
                    properties.smoothnessMapChannel.floatValue = smoothnessSource;
                EditorGUI.showMixedValue = false;
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }

        private static SmoothnessMapChannel GetSmoothnessMapChannel(Material material)
        {
            var ch = (int) material.GetFloat(SmoothnessTextureChannel);
            return ch == (int) SmoothnessMapChannel.AlbedoAlpha
                ? SmoothnessMapChannel.AlbedoAlpha
                : SmoothnessMapChannel.SpecularMetallicAlpha;
        }

        public static void SetMaterialKeywords(Material material)
        {
            // Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
            // (MaterialProperty value might come from renderer material property block)
            bool hasGlossMap;
            var isSpecularWorkFlow = false;
            var opaque = ((BaseShaderGUI.SurfaceType) material.GetFloat(Surface) ==
                          BaseShaderGUI.SurfaceType.Opaque);
            if (material.HasProperty("_WorkflowMode"))
            {
                isSpecularWorkFlow = (WorkflowMode) material.GetFloat(Mode) == WorkflowMode.Specular;
                if (isSpecularWorkFlow)
                    hasGlossMap = material.GetTexture(SpecGlossMap) != null;
                else
                    hasGlossMap = material.GetTexture(MetallicGlossMap) != null;
            }
            else
            {
                hasGlossMap = material.GetTexture(MetallicGlossMap) != null;
            }

            CoreUtils.SetKeyword(material, "_SPECULAR_SETUP", isSpecularWorkFlow);

            CoreUtils.SetKeyword(material, "_METALLICSPECGLOSSMAP", hasGlossMap);

            if (material.HasProperty("_SpecularHighlights"))
                CoreUtils.SetKeyword(material, "_SPECULARHIGHLIGHTS_OFF",
                    material.GetFloat(SpecularHighlights) == 0.0f);
            if (material.HasProperty("_EnvironmentReflections"))
                CoreUtils.SetKeyword(material, "_ENVIRONMENTREFLECTIONS_OFF",
                    material.GetFloat(EnvironmentReflections) == 0.0f);
            if (material.HasProperty("_OcclusionMap"))
                CoreUtils.SetKeyword(material, "_OCCLUSIONMAP", material.GetTexture(OcclusionMap));

            if (material.HasProperty("_SmoothnessTextureChannel"))
            {
                CoreUtils.SetKeyword(material, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A",
                    GetSmoothnessMapChannel(material) == SmoothnessMapChannel.AlbedoAlpha && opaque);
            }
        }
    }
}