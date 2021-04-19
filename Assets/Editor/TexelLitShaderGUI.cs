using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class TexelLitShaderGUI : BaseShaderGUI
{
        // Properties
        private TexelLitGUI.TexelLitProperties _texelLitProperties;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int Emission = Shader.PropertyToID("_Emission");
        private static readonly int AlphaClip = Shader.PropertyToID("_AlphaClip");
        private static readonly int Surface = Shader.PropertyToID("_Surface");
        private static readonly int Blend = Shader.PropertyToID("_Blend");
        private static readonly int WorkflowMode = Shader.PropertyToID("_WorkflowMode");
        private static readonly int SpecGlossMap = Shader.PropertyToID("_SpecGlossMap");
        private static readonly int MetallicSpecGlossMap = Shader.PropertyToID("_MetallicSpecGlossMap");
        private static readonly int MetallicGlossMap = Shader.PropertyToID("_MetallicGlossMap");

        // collect properties from the material properties
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            _texelLitProperties = new TexelLitGUI.TexelLitProperties(properties);
        }

        // material changed check
        public override void MaterialChanged(Material material)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            SetMaterialKeywords(material, TexelLitGUI.SetMaterialKeywords);
        }

        // material main surface options
        public override void DrawSurfaceOptions(Material material)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            // Detect any changes to the material
            EditorGUI.BeginChangeCheck();
            if (_texelLitProperties.workflowMode != null)
            {
                DoPopup(TexelLitGUI.Styles.WorkflowModeText, _texelLitProperties.workflowMode, Enum.GetNames(typeof(TexelLitGUI.WorkflowMode)));
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in blendModeProp.targets)
                    MaterialChanged((Material)obj);
            }
            base.DrawSurfaceOptions(material);
        }

        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);
            TexelLitGUI.Inputs(_texelLitProperties, materialEditor, material);
            DrawEmissionProperties(material, true);
            DrawTileOffset(materialEditor, baseMapProp);
        }

        // material main advanced options
        public override void DrawAdvancedOptions(Material material)
        {
            if (_texelLitProperties.reflections != null && _texelLitProperties.highlights != null)
            {
                EditorGUI.BeginChangeCheck();
                materialEditor.ShaderProperty(_texelLitProperties.highlights, TexelLitGUI.Styles.HighlightsText);
                materialEditor.ShaderProperty(_texelLitProperties.reflections, TexelLitGUI.Styles.ReflectionsText);
                if(EditorGUI.EndChangeCheck())
                {
                    MaterialChanged(material);
                }
            }

            base.DrawAdvancedOptions(material);
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor(EmissionColor, material.GetColor(Emission));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialBlendMode(material);
                return;
            }

            var surfaceType = SurfaceType.Opaque;
            var blendMode = BlendMode.Alpha;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                surfaceType = SurfaceType.Opaque;
                material.SetFloat(AlphaClip, 1);
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                surfaceType = SurfaceType.Transparent;
                blendMode = BlendMode.Alpha;
            }
            material.SetFloat(Surface, (float)surfaceType);
            material.SetFloat(Blend, (float)blendMode);

            if (oldShader.name.Equals("Standard (Specular setup)"))
            {
                material.SetFloat(WorkflowMode, (float)TexelLitGUI.WorkflowMode.Specular);
                var texture = material.GetTexture(SpecGlossMap);
                if (texture != null)
                    material.SetTexture(MetallicSpecGlossMap, texture);
            }
            else
            {
                material.SetFloat(WorkflowMode, (float)TexelLitGUI.WorkflowMode.Metallic);
                var texture = material.GetTexture(MetallicGlossMap);
                if (texture != null)
                    material.SetTexture(MetallicSpecGlossMap, texture);
            }

            MaterialChanged(material);
        }
    }
}
