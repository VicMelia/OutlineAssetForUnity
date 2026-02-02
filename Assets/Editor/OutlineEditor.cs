using UnityEngine;
using UnityEditor;
using static OutlineManager;
using static HandyOutlines.OutlineFeature;
using HandyOutlines;
using UnityEditorInternal;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

/// <summary>
/// Custom editor for the OutlineManager component.
/// Provides a detailed inspector with grouped sections for filter, outline, lighting,
/// noise, bloom, custom texture, and layer mask settings.
/// Handles updating materials and URP OutlineFeature properties automatically.
/// </summary>
[CustomEditor(typeof(OutlineManager))]
public class OutlineEditor : Editor
{
    [SerializeField]
    private OutlineFeatureSettings _settings = SharedSettings;

    #region OnInspectorUGUI
    public override void OnInspectorGUI()
    {
        var manager = (OutlineManager)target;
        serializedObject.Update();

        bool hasChanged = false;

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(manager), typeof(OutlineManager), false);
        }
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();

        //Filter settings
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        Color filterSettingsColor = new Color(0.3f, 0.5f, 0.8f, 0.2f);
        GUILayout.BeginVertical(OutlineEditorHelpers.GetBoxStyle(filterSettingsColor));
        SetFilterSettings(target, manager); 
        GUILayout.EndVertical();

        //Outline settings
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        Color outlineSettingsColor = new Color(0.2f, 0.4f, 0.5f, 0.2f);
        GUILayout.BeginVertical(OutlineEditorHelpers.GetBoxStyle(outlineSettingsColor));
        SetOulineSettings(target, manager);
        GUILayout.EndVertical();

        //Light settings
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        Color lightSettingsColor = new Color(0.7f, 0.6f, 0.3f, 0.2f);
        GUILayout.BeginVertical(OutlineEditorHelpers.GetBoxStyle(lightSettingsColor));
        SetLightSettings(target, manager);
        GUILayout.EndVertical();

        //Noise settings
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        Color noiseSettingsColor = new Color(0.6f, 0.1f, 0.2f, 0.2f);
        GUILayout.BeginVertical(OutlineEditorHelpers.GetBoxStyle(noiseSettingsColor));
        SetNoiseSettings(target, manager);
        GUILayout.EndVertical();

        //Bloom settings
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        Color bloomSettingsColor = new Color(0.8f, 0.3f, 0.5f, 0.2f);
        GUILayout.BeginVertical(OutlineEditorHelpers.GetBoxStyle(bloomSettingsColor));
        SetBloomSettings(target, manager);
        GUILayout.EndVertical();

        //Custom texture settings
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        Color textureSettingsColor = new Color(0.2f, 0.6f, 0.5f, 0.2f);
        GUILayout.BeginVertical(OutlineEditorHelpers.GetBoxStyle(textureSettingsColor));
        SetCustomTextureSettings(target, manager);
        GUILayout.EndVertical();

        //Layer settings
        SetLayerSettings(target, manager);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            hasChanged = true;
        }
        if (hasChanged)
        {
            SceneView.RepaintAll();
        }
    }
    #endregion

    #region Material Editor

    /// <summary>
    /// Displays and handles the filter settings section in the inspector.
    /// Updates the OutlineManager filter mode, selects the appropriate material, 
    /// and assigns it to the OutlineFeature in URP if available.
    /// </summary>
    private void SetFilterSettings(Object target, OutlineManager manager)
    {
        EditorGUILayout.LabelField("Filter Settings", EditorStyles.boldLabel);
        var newFilter = (OutlineFilter)EditorGUILayout.EnumPopup("Filter Mode", manager.Filter);
        EditorGUILayout.Space();

        if (newFilter != manager.Filter)
        {
            manager.Filter = newFilter;
            manager.FilterSelector = OutlineEditorHelpers.GetFilterLevel(newFilter);
            string materialFolder = "Assets/OutlineAsset/OutlineMaterials";
            Material newMat = OutlineEditorHelpers.GetFilterMaterial(materialFolder, newFilter);
            if (newMat != null)
            {
                SharedOutlineMaterial = newMat;
                var outlineFeature = OutlineEditorHelpers.GetOutlineFeature(); 
                if (outlineFeature != null)
                {
                    outlineFeature.material = newMat;
                    EditorUtility.SetDirty(outlineFeature);
                    AssetDatabase.SaveAssets();
                }

                manager.outlineMaterial = newMat;
                EditorUtility.SetDirty(manager);
            }
            else
            {
                Debug.LogWarning($"Material for filter {newFilter} not found in {materialFolder}");
            }
        }
        manager.FilterSelector = OutlineEditorHelpers.GetFilterLevel(manager.Filter);
    }

    /// <summary>
    /// Displays and handles the outline settings section in the inspector.
    /// Allows configuration of outline mode, color, thickness, strength, threshold, normal-based effects, 
    /// and double outline style.
    /// </summary>
    private void SetOulineSettings(Object target, OutlineManager manager)
    {
        //Base outline settings
        EditorGUILayout.LabelField("Outline Settings", EditorStyles.boldLabel);
        manager.Mode = (OutlineMode)EditorGUILayout.EnumPopup("Outline Mode", manager.Mode);
        EditorGUILayout.Space();
        manager.OutlineColor = EditorGUILayout.ColorField("Outline Color", manager.OutlineColor);
        EditorGUILayout.Space();
        EditorGUI.indentLevel++;

        //Depth-normal settings
        GUILayout.BeginVertical(OutlineEditorHelpers.GetBoxStyle(new Color(0.1f, 0.1f, 0.1f, 0.3f)));
        EditorGUILayout.LabelField("Depth Settings", EditorStyles.miniBoldLabel);
        manager.OutlineThickness = EditorGUILayout.Slider("Outline Thickness", manager.OutlineThickness, 0.1f, OutlineEditorHelpers.GetMaxThickness(manager.Filter));
        manager.OutlineStrength = EditorGUILayout.Slider("Outline Opacity", manager.OutlineStrength, 0f, 1f);
        manager.Threshold = EditorGUILayout.Slider("Outline Reduction", manager.Threshold, 0f, 1f);
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        switch (manager.Mode)
        {
            case OutlineMode.DepthAndNormals:
                EditorGUILayout.Space();
                GUILayout.BeginVertical(OutlineEditorHelpers.GetBoxStyle(new Color(0.1f, 0.1f, 0.1f, 0.3f)));
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Normal Settings", EditorStyles.miniBoldLabel);
                manager.UseNormal = true;
                manager.NormalThickness = EditorGUILayout.Slider("Normal Thickness", manager.NormalThickness, 0.1f, manager.OutlineThickness);
                manager.NormalStrength = EditorGUILayout.Slider("Normal Opacity", manager.NormalStrength, 0f, 1f);
                manager.NormalThreshold = EditorGUILayout.Slider("Normal Reduction", manager.NormalThreshold, 0.1f, 15f);
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                break;

            default:
                if (manager.UseNormal) manager.UseNormal = false;
                break;
        }
        EditorGUILayout.Space();

        //Simple-double settings
        manager.Style = (OutlineStyle)EditorGUILayout.EnumPopup("Outline Style", manager.Style);
        switch (manager.Style)
        {
            case OutlineStyle.Double:
                EditorGUILayout.Space();
                GUILayout.BeginVertical(OutlineEditorHelpers.GetBoxStyle(new Color(0.1f, 0.1f, 0.1f, 0.3f)));
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Outer Outline Settings", EditorStyles.miniBoldLabel);
                manager.DoubleMode = true;
                manager.DoubleColor = EditorGUILayout.ColorField("Outer Color", manager.DoubleColor);
                manager.DoubleThickness = EditorGUILayout.Slider("Outer Thickness", manager.DoubleThickness, manager.OutlineThickness + 2f, OutlineEditorHelpers.GetMaxThickness(manager.Filter) + 4f);

                if(manager.Mode == OutlineMode.DepthAndNormals)
                {
                    manager.DoubleNormalThickness = EditorGUILayout.Slider("Outer Normal Thickness", manager.DoubleNormalThickness, manager.NormalThickness + 0.1f, manager.DoubleThickness);
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                break;

            default:
                if (manager.DoubleMode) manager.DoubleMode = false;
                break;
        }
    }

    /// <summary>
    /// Displays and handles the light settings section in the inspector.
    /// Configures light color blending and light factor for the outline effect.
    /// </summary>
    private void SetLightSettings(Object target, OutlineManager manager)
    {
        EditorGUILayout.LabelField("Light Color", EditorStyles.boldLabel);
        manager.BlendMode = (LightBlend)EditorGUILayout.EnumPopup("Light Color Blending", manager.BlendMode);
        switch (manager.BlendMode)
        {
            case LightBlend.On:
                EditorGUILayout.Space();
                manager.ApplyLightColor = true;
                manager.LightFactor = EditorGUILayout.Slider("Light Factor", manager.LightFactor, 0f, 1f);
                break;

            default:
                manager.ApplyLightColor = false;
                break;
        }
        EditorGUILayout.Space();
    }

    /// <summary>
    /// Displays and handles the noise settings section in the inspector.
    /// Configures distortion mode, frequency, direction, intensity, and animation speed.
    /// Supports Waves, Pencil, and Custom noise types.
    /// </summary>
    private void SetNoiseSettings(Object target, OutlineManager manager)
    {
        EditorGUILayout.LabelField("Noise Settings", EditorStyles.boldLabel);
        manager.noiseEffect = (NoiseEffect)EditorGUILayout.EnumPopup("Distortion Mode", manager.noiseEffect);

        switch (manager.noiseEffect)
        {
            case NoiseEffect.Waves: //Wave frequency (low, mid, high, custom)
                manager.ApplyNoise = true;
                EditorGUI.indentLevel++;
                GUILayout.BeginVertical(OutlineEditorHelpers.GetBoxStyle(new Color(0.1f, 0.1f, 0.1f, 0.3f)));
                EditorGUILayout.LabelField("Wave Settings", EditorStyles.miniBoldLabel);

                manager.noiseFrequency = (NoiseFrequency)EditorGUILayout.EnumPopup("Noise Frequency", manager.noiseFrequency);
                manager.NoiseScale = manager.noiseFrequency switch
                {
                    NoiseFrequency.Low => 200f,
                    NoiseFrequency.Mid => 500f,
                    NoiseFrequency.High => 1000f,
                    NoiseFrequency.Custom => EditorGUILayout.Slider("Wave Frequency", manager.NoiseScale, 0.01f, 2000f),
                    _ => manager.NoiseScale
                };

                manager.distortionAxis = (DistortionAxis)EditorGUILayout.EnumPopup("Distortion Direction", manager.distortionAxis);
                ApplyDistortionAxis(manager);
                GUILayout.EndVertical();
                EditorGUI.indentLevel--;

                // Animate Lines
                manager.animateLines = (AnimateLines)EditorGUILayout.EnumPopup("Animate Distortion", manager.animateLines);
                manager.AnimateNoise = manager.animateLines == AnimateLines.On;
                if (manager.AnimateNoise)
                {
                    manager.StepTime = EditorGUILayout.Slider("Distortion Speed", manager.StepTime, 0.01f, 0.5f);
                }
                break;

            case NoiseEffect.Pencil: //Pencil noise scale
                manager.ApplyNoise = true;
                EditorGUI.indentLevel++;
                GUILayout.BeginVertical(OutlineEditorHelpers.GetBoxStyle(new Color(0.1f, 0.1f, 0.1f, 0.3f)));
                EditorGUILayout.LabelField("Pencil Settings", EditorStyles.miniBoldLabel);
                manager.NoiseScale = 5000f;
                manager.distortionAxis = (DistortionAxis)EditorGUILayout.EnumPopup("Distortion Direction", manager.distortionAxis);
                ApplyDistortionAxis(manager);
                GUILayout.EndVertical();
                EditorGUI.indentLevel--;

                // Animate Lines
                manager.animateLines = (AnimateLines)EditorGUILayout.EnumPopup("Animate Distortion", manager.animateLines);
                manager.AnimateNoise = manager.animateLines == AnimateLines.On;
                if (manager.AnimateNoise)
                {
                    manager.StepTime = EditorGUILayout.Slider("Distortion Speed", manager.StepTime, 0.01f, 0.5f);
                }
                break;

            case NoiseEffect.Custom:
                manager.ApplyNoise = true;
                EditorGUI.indentLevel++;
                GUILayout.BeginVertical(OutlineEditorHelpers.GetBoxStyle(new Color(0.1f, 0.1f, 0.1f, 0.3f)));
                manager.distortionAxis = (DistortionAxis)EditorGUILayout.EnumPopup("Distortion Direction", manager.distortionAxis);
                ApplyDistortionAxis(manager);
                GUILayout.EndVertical();
                EditorGUI.indentLevel--;
                manager.NoiseScale = EditorGUILayout.Slider("Distortion Scale", manager.NoiseScale, 0f, 5000f);

                // Animate Lines
                manager.animateLines = (AnimateLines)EditorGUILayout.EnumPopup("Animate Distortion", manager.animateLines);
                manager.AnimateNoise = manager.animateLines == AnimateLines.On;
                if (manager.AnimateNoise)
                {
                    manager.StepTime = EditorGUILayout.Slider("Distortion Speed", manager.StepTime, 0.01f, 0.5f);
                }
                break;

            default:
                manager.ApplyNoise = false;
                break;
        }
        EditorGUILayout.Space();
    }
    /// <summary>
    /// Applies the distortion intensity to the outline material based on the selected distortion axis.
    /// </summary>
    private void ApplyDistortionAxis(OutlineManager manager)
    {
        switch (manager.distortionAxis)
        {
            case DistortionAxis.X:
                float x = EditorGUILayout.Slider("Distortion Intensity (X)", manager.NoiseStrength.x, 0.01f, 0.1f);
                manager.NoiseStrength = new Vector2(x, 0f);
                break;
            case DistortionAxis.Y:
                float y = EditorGUILayout.Slider("Distortion Intensity (Y)", manager.NoiseStrength.y, 0.01f, 0.1f);
                manager.NoiseStrength = new Vector2(0f, y);
                break;
            case DistortionAxis.BothDirections:
                float bx = EditorGUILayout.Slider("Distortion Intensity (X)", manager.NoiseStrength.x, 0.01f, 0.1f);
                float by = EditorGUILayout.Slider("Distortion Intensity (Y)", manager.NoiseStrength.y, 0.01f, 0.1f);
                manager.NoiseStrength = new Vector2(bx, by);
                break;
        }
    }

    ///<summary>
    /// Displays and handles the bloom settings section in the inspector.
    /// Supports simple bloom and intermittent bloom, configuring color, intensity, and speed.
    /// </summary>
    private void SetBloomSettings(Object target, OutlineManager manager)
    {
        EditorGUILayout.LabelField("Bloom", EditorStyles.boldLabel);
        manager.bloomEffect = (BloomEffect)EditorGUILayout.EnumPopup("Bloom Mode", manager.bloomEffect);
        if (manager.bloomEffect == BloomEffect.Simple)
        {
            manager.OutlineColor = Color.white; //White base color is necessary
            if (manager.UseIntermitent) manager.UseIntermitent = false;
            EditorGUILayout.Space();
            manager.UseBloom = true;
            manager.BloomColor = EditorGUILayout.ColorField("Bloom Color", manager.BloomColor);
            manager.BloomIntensity = EditorGUILayout.Slider("Bloom Intensity", manager.BloomIntensity, 1f, 5f);
        }

        else if (manager.bloomEffect == BloomEffect.Intermitent)
        {
            EditorGUILayout.Space();
            manager.UseBloom = true;
            manager.UseIntermitent = true;
            manager.BloomColor = EditorGUILayout.ColorField("Bloom Color", manager.BloomColor);
            manager.BloomIntensity = EditorGUILayout.Slider("Bloom Intensity", manager.BloomIntensity, 1f, 5f);
            manager.IntermitentSpeed = EditorGUILayout.Slider("Intermitent Speed", manager.IntermitentSpeed, 0.5f, 5f);
        }
        else
        {
            if (manager.UseBloom) manager.UseBloom = false;
            if (manager.UseIntermitent) manager.UseIntermitent = false;
        }
        EditorGUILayout.Space();
    }

    /// <summary>
    /// Displays and handles the custom texture settings section in the inspector.
    /// Allows the user to assign a texture and configure its size.
    /// </summary>
    private void SetCustomTextureSettings(Object target, OutlineManager manager)
    {
        EditorGUILayout.LabelField("Custom Texture (Advanced)", EditorStyles.boldLabel);
        manager.textureEffect = (TextureEffect)EditorGUILayout.EnumPopup("Custom Texture", manager.textureEffect);
        if (manager.textureEffect == TextureEffect.On)
        {
            manager.UseCustomTexture = true;
            manager.CustomTex = (Texture2D)EditorGUILayout.ObjectField("Texture", manager.CustomTex, typeof(Texture2D), false);

            if (manager.CustomTex != null)
            {
                EditorGUILayout.LabelField("Texture Size");
                manager.TexSize.x = EditorGUILayout.Slider("X", manager.TexSize.x, 0f, 30f);
                manager.TexSize.y = EditorGUILayout.Slider("Y", manager.TexSize.y, 0f, 30f);

            }
        }
        else
        {
            if (manager.UseCustomTexture) manager.UseCustomTexture = false;
            manager.CustomTex = null;
        }
    }

    /// <summary>
    /// Displays and handles the layer mask settings section in the inspector.
    /// Allows the user to exclude specific layers from being affected by the outline effect.
    /// </summary>
    private void SetLayerSettings(Object target, OutlineManager manager)
    {
        EditorGUILayout.LabelField("Layer Mask", EditorStyles.boldLabel);
        manager.excludedLayerMask = OutlineEditorHelpers.LayerMaskField("Exclude from", manager.excludedLayerMask);
        if (_settings != null)
        {
            _settings.excludedLayerMask = manager.excludedLayerMask;
        }
    }
    #endregion

    #region Helpers
    public static class OutlineEditorHelpers
    {
        /// <summary>
        /// Returns a GUIStyle for a colored box used in grouping inspector sections.
        /// </summary>
        public static GUIStyle GetBoxStyle(Color c)
        {
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, c); //Background color
            tex.Apply();
            boxStyle.normal.background = tex;
            return boxStyle;
        }

        /// <summary>
        /// Returns the integer filter level corresponding to the selected OutlineFilter.
        /// </summary>
        public static int GetFilterLevel(OutlineFilter filter)
        {
            switch (filter)
            {
                case OutlineFilter.RobertsCross:
                    return 0;

                case OutlineFilter.Sobel:
                    return 1;

                case OutlineFilter.Prewitt:
                    return 2;

                case OutlineFilter.Scharr:
                    return 3;

                case OutlineFilter.Laplacian:
                    return 4;

                default:
                    return 5;
            }
        }

        /// <summary>
        /// Returns the maximum allowed outline thickness based on the selected OutlineFilter.
        /// </summary>
        public static int GetMaxThickness(OutlineFilter filter)
        {
            switch (filter)
            {
                case OutlineFilter.Laplacian:
                    return 6;
                case OutlineFilter.DoG:
                    return 7;
                default:
                    return 8;
            }
        }

        /// <summary>
        /// Custom LayerMask field that allows selecting multiple layers in the inspector.
        /// </summary>
        public static LayerMask LayerMaskField(string label, LayerMask selected)
        {
            var layers = InternalEditorUtility.layers;
            var layerNumbers = new int[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                layerNumbers[i] = LayerMask.NameToLayer(layers[i]);
            }
            int maskWithoutEmpty = 0;
            for (int i = 0; i < layers.Length; i++)
            {
                if (((1 << layerNumbers[i]) & selected.value) > 0)
                {
                    maskWithoutEmpty |= (1 << i);
                }
            }
            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers);

            int mask = 0;
            for (int i = 0; i < layers.Length; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNumbers[i]);
            }

            selected.value = mask;
            return selected;
        }

        /// <summary>
        /// Searches for and returns the material corresponding to the current outline filter.
        /// </summary>
        public static Material GetFilterMaterial(string materialFolder, OutlineFilter newFilter)
        {
            string materialName = $"Outline_{newFilter}";
            string[] guids = AssetDatabase.FindAssets($"{materialName} t:Material", new[] { materialFolder });
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                Material newMat = AssetDatabase.LoadAssetAtPath<Material>(path);
                return newMat;
            }
            else
            {
                Debug.LogWarning($" {materialName} not found in {materialFolder}");
            }
            return null;
        }

        /// <summary>
        /// Returns the current URP RendererData for an OutlineFeature.
        /// </summary>
        public static OutlineFeature GetOutlineFeature()
        {
            var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset == null)
            {
                Debug.LogWarning("No Universal Render Pipeline Asset found");
                return null;
            }
            SerializedObject so = new SerializedObject(urpAsset);
            SerializedProperty rendererDataListProp = so.FindProperty("m_RendererDataList");
            if (rendererDataListProp == null || rendererDataListProp.arraySize == 0)
            {
                Debug.LogWarning("URP asset has no RendererData list");
                return null;
            }
            int defaultRendererIndex = 0;
            SerializedProperty defaultRendererProp = so.FindProperty("m_DefaultRendererIndex");
            if (defaultRendererProp != null)
            {
                defaultRendererIndex = defaultRendererProp.intValue;
            }
            SerializedProperty rendererDataProp = rendererDataListProp.GetArrayElementAtIndex(defaultRendererIndex);
            var rendererData = rendererDataProp.objectReferenceValue as ScriptableRendererData;
            if (rendererData == null)
            {
                Debug.LogWarning("RendererData is null in URP asset.");
                return null;
            }
            //Search for outline feature
            foreach (var feature in rendererData.rendererFeatures)
            {
                if (feature is OutlineFeature outlineFeature)
                    return outlineFeature;
            }
            Debug.LogWarning("No OutlineFeature found in renderer features.");
            return null;
        }
    }
    #endregion
}
