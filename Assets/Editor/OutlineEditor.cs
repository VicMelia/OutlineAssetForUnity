using UnityEngine;
using UnityEditor;
using static UltimateOutlineManager;
using static OutlineFeature;
using UnityEditorInternal;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

[CustomEditor(typeof(UltimateOutlineManager))]
public class OutlineEditor : Editor
{
    [SerializeField]
    private OutlineFeatureSettings _settings = SharedSettings;
    public override void OnInspectorGUI()
    {
        var manager = (UltimateOutlineManager)target;
        serializedObject.Update();

        bool hasChanged = false;

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(manager), typeof(UltimateOutlineManager), false);
        }
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        Color filterSettingsColor = new Color(0.3f, 0.5f, 0.8f, 0.2f);
        GUILayout.BeginVertical(GetBoxStyle(filterSettingsColor));
        SetFilterSettings(target, manager);
        GUILayout.EndVertical();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        Color outlineSettingsColor = new Color(0.2f, 0.4f, 0.5f, 0.2f);
        GUILayout.BeginVertical(GetBoxStyle(outlineSettingsColor));
        SetOulineSettings(target, manager);
        GUILayout.EndVertical();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        Color lightSettingsColor = new Color(0.7f, 0.6f, 0.3f, 0.2f);
        GUILayout.BeginVertical(GetBoxStyle(lightSettingsColor));
        SetLightSettings(target, manager);
        GUILayout.EndVertical();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        Color noiseSettingsColor = new Color(0.6f, 0.1f, 0.2f, 0.2f);
        GUILayout.BeginVertical(GetBoxStyle(noiseSettingsColor));
        SetNoiseSettings(target, manager);
        GUILayout.EndVertical();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        Color bloomSettingsColor = new Color(0.8f, 0.3f, 0.5f, 0.2f);
        GUILayout.BeginVertical(GetBoxStyle(bloomSettingsColor));
        SetBloomSettings(target, manager);
        GUILayout.EndVertical();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        Color textureSettingsColor = new Color(0.2f, 0.6f, 0.5f, 0.2f);
        GUILayout.BeginVertical(GetBoxStyle(textureSettingsColor));
        SetCustomTextureSettings(target, manager);
        GUILayout.EndVertical();

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

    private void SetFilterSettings(Object target, UltimateOutlineManager manager)
    {
        EditorGUILayout.LabelField("Filter Settings", EditorStyles.boldLabel);
        var newFilter = (OutlineFilter)EditorGUILayout.EnumPopup("Filter Mode", manager.Filter);
        EditorGUILayout.Space();

        if (newFilter != manager.Filter)
        {
            manager.Filter = newFilter;
            manager.FilterSelector = GetFilterLevel(newFilter);
            string materialFolder = "Assets/OutlineAsset/OutlineMaterials";
            
            Material newMat = GetFilterMaterial(materialFolder, newFilter);
            if (newMat != null)
            {
                SharedOutlineMaterial = newMat;
                var outlineFeature = GetOutlineFeature(); 
                if (outlineFeature != null)
                {
                    outlineFeature.material = newMat;
                    EditorUtility.SetDirty(outlineFeature);
                    AssetDatabase.SaveAssets();
                }

                manager.outlineMaterial = newMat; //updates material properties
                EditorUtility.SetDirty(manager);
            }
            else
            {
                Debug.LogWarning($"Material for filter {newFilter} not found in {materialFolder}");
            }
        }
        manager.FilterSelector = GetFilterLevel(manager.Filter);
    }

    private void SetOulineSettings(Object target, UltimateOutlineManager manager)
    {
        EditorGUILayout.LabelField("Outline Settings", EditorStyles.boldLabel);
        manager.Mode = (OutlineMode)EditorGUILayout.EnumPopup("Outline Mode", manager.Mode);
        EditorGUILayout.Space();
        manager.OutlineColor = EditorGUILayout.ColorField("Outline Color", manager.OutlineColor);
        EditorGUILayout.Space();

        EditorGUI.indentLevel++;
        GUILayout.BeginVertical(GetBoxStyle(new Color(0.1f, 0.1f, 0.1f, 0.3f)));
        EditorGUILayout.LabelField("Depth Settings", EditorStyles.miniBoldLabel);
        manager.OutlineThickness = EditorGUILayout.Slider("Outline Thickness", manager.OutlineThickness, 0.1f, GetMaxThickness(manager.Filter));
        manager.OutlineStrength = EditorGUILayout.Slider("Outline Opacity", manager.OutlineStrength, 0f, 1f);
        manager.Threshold = EditorGUILayout.Slider("Threshold", manager.Threshold, 0.05f, 1f);
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        switch (manager.Mode)
        {
            case OutlineMode.DepthAndNormals:
                EditorGUILayout.Space();
                GUILayout.BeginVertical(GetBoxStyle(new Color(0.1f, 0.1f, 0.1f, 0.3f)));
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Normal Settings", EditorStyles.miniBoldLabel);
                manager.UseNormal = true;
                manager.NormalThickness = EditorGUILayout.Slider("Normal Thickness", manager.NormalThickness, 0.1f, manager.OutlineThickness);
                manager.NormalStrength = EditorGUILayout.Slider("Normal Opacity", manager.NormalStrength, 0f, 1f);
                manager.NormalThreshold = EditorGUILayout.Slider("Normal Threshold", manager.NormalThreshold, 0.1f, 15f);

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                break;

            default:
                if (manager.UseNormal) manager.UseNormal = false;
                break;
        }

        EditorGUILayout.Space();

    }

    private void SetLightSettings(Object target, UltimateOutlineManager manager)
    {

        EditorGUILayout.LabelField("Light Color", EditorStyles.boldLabel);
        manager.BlendMode = (LightBlend)EditorGUILayout.EnumPopup("Light Color Blending", manager.BlendMode);
        if (manager.BlendMode == LightBlend.On)
        {
            EditorGUILayout.Space();
            manager.ApplyLightColor = true;
            manager.LightFactor = EditorGUILayout.Slider("Light Factor", manager.LightFactor, 0f, 1f);
        }
        else
        {
            if (manager.ApplyLightColor) manager.ApplyLightColor = false;
        }
        EditorGUILayout.Space();
    }

    private void SetNoiseSettings(Object target, UltimateOutlineManager manager)
    {

        EditorGUILayout.LabelField("Noise Settings", EditorStyles.boldLabel);
        manager.noiseEffect = (NoiseEffect)EditorGUILayout.EnumPopup("Distortion Mode", manager.noiseEffect);
        if (manager.noiseEffect == NoiseEffect.Waves)
        {
            manager.ApplyNoise = true;
            EditorGUI.indentLevel++;
            GUILayout.BeginVertical(GetBoxStyle(new Color(0.1f, 0.1f, 0.1f, 0.3f)));
            EditorGUILayout.LabelField("Wave Settings", EditorStyles.miniBoldLabel);
            manager.noiseFrequency = (NoiseFrequency)EditorGUILayout.EnumPopup("Noise Frequency", manager.noiseFrequency);

            if (manager.noiseFrequency == NoiseFrequency.Low)
            {
                manager.NoiseScale = 200f;
            }
            else if (manager.noiseFrequency == NoiseFrequency.Mid)
            {
                manager.NoiseScale = 500f;
            }

            else if (manager.noiseFrequency == NoiseFrequency.High)
            {
                manager.NoiseScale = 1000f;
            }

            else if (manager.noiseFrequency == NoiseFrequency.Custom)
            {
                manager.NoiseScale = EditorGUILayout.Slider("Wave Frequency", manager.NoiseScale, 0.01f, 2000f);
            }

            manager.distortionAxis = (DistortionAxis)EditorGUILayout.EnumPopup("Distortion Direction", manager.distortionAxis);
            EditorGUI.indentLevel++;
            if (manager.distortionAxis == DistortionAxis.X)
            {
                float x = EditorGUILayout.Slider("Distortion Intensity (X)", manager.NoiseStrength.x, 0.01f, 0.1f);
                manager.NoiseStrength = new Vector2(x, 0f);
            }
            else if (manager.distortionAxis == DistortionAxis.Y)
            {
                float y = EditorGUILayout.Slider("Distortion Intensity (Y)", manager.NoiseStrength.y, 0.01f, 0.1f);
                manager.NoiseStrength = new Vector2(0f, y);
            }
            else if (manager.distortionAxis == DistortionAxis.BothDirections)
            {
                float x = EditorGUILayout.Slider("Distortion Intensity (X)", manager.NoiseStrength.x, 0.01f, 0.1f);
                float y = EditorGUILayout.Slider("Distortion Intensity (Y)", manager.NoiseStrength.y, 0.01f, 0.1f);
                manager.NoiseStrength = new Vector2(x, y);
            }
            EditorGUI.indentLevel--;
            GUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUI.indentLevel--;
            manager.animateLines = (AnimateLines)EditorGUILayout.EnumPopup("Animate Distortion", manager.animateLines);

            if (manager.animateLines == AnimateLines.On)
            {
                manager.AnimateNoise = true;
                manager.StepTime = EditorGUILayout.Slider("Distortion Speed", manager.StepTime, 0.01f, 0.5f);
            }
            else
            {
                if (manager.AnimateNoise) manager.AnimateNoise = false;
            }

        }

        else if (manager.noiseEffect == NoiseEffect.Pencil)
        {
            manager.ApplyNoise = true;
            EditorGUI.indentLevel++;
            GUILayout.BeginVertical(GetBoxStyle(new Color(0.1f, 0.1f, 0.1f, 0.3f)));
            EditorGUILayout.LabelField("Pencil Settings", EditorStyles.miniBoldLabel);
            manager.NoiseScale = 5000f;

            manager.distortionAxis = (DistortionAxis)EditorGUILayout.EnumPopup("Distortion Direction", manager.distortionAxis);
            EditorGUI.indentLevel++;
            if (manager.distortionAxis == DistortionAxis.X)
            {
                float x = EditorGUILayout.Slider("Distortion Intensity (X)", manager.NoiseStrength.x, 0.01f, 0.1f);
                manager.NoiseStrength = new Vector2(x, 0f);
            }
            else if (manager.distortionAxis == DistortionAxis.Y)
            {
                float y = EditorGUILayout.Slider("Distortion Intensity (Y)", manager.NoiseStrength.y, 0.01f, 0.1f);
                manager.NoiseStrength = new Vector2(0f, y);
            }
            else if (manager.distortionAxis == DistortionAxis.BothDirections)
            {
                float x = EditorGUILayout.Slider("Distortion Intensity (X)", manager.NoiseStrength.x, 0.01f, 0.1f);
                float y = EditorGUILayout.Slider("Distortion Intensity (Y)", manager.NoiseStrength.y, 0.01f, 0.1f);
                manager.NoiseStrength = new Vector2(x, y);
            }
            EditorGUI.indentLevel--;
            GUILayout.EndVertical();



            EditorGUILayout.Space();

            EditorGUI.indentLevel--;
            manager.animateLines = (AnimateLines)EditorGUILayout.EnumPopup("Animate Distortion", manager.animateLines);

            if (manager.animateLines == AnimateLines.On)
            {
                manager.AnimateNoise = true;
                manager.StepTime = EditorGUILayout.Slider("Distortion Speed", manager.StepTime, 0.01f, 0.5f);
            }
            else
            {
                if (manager.AnimateNoise) manager.AnimateNoise = false;
            }

        }
        else if (manager.noiseEffect == NoiseEffect.Custom)
        {
            EditorGUILayout.Space();
            manager.ApplyNoise = true;
            EditorGUI.indentLevel++;
            GUILayout.BeginVertical(GetBoxStyle(new Color(0.1f, 0.1f, 0.1f, 0.3f)));
            manager.distortionAxis = (DistortionAxis)EditorGUILayout.EnumPopup("Distortion Direction", manager.distortionAxis);

            if (manager.distortionAxis == DistortionAxis.X)
            {
                float x = EditorGUILayout.Slider("Distortion Intensity (X)", manager.NoiseStrength.x, 0.01f, 0.1f);
                manager.NoiseStrength = new Vector2(x, 0f);
            }
            else if (manager.distortionAxis == DistortionAxis.Y)
            {
                float y = EditorGUILayout.Slider("Distortion Intensity (Y)", manager.NoiseStrength.y, 0.01f, 0.1f);
                manager.NoiseStrength = new Vector2(0f, y);
            }
            else if (manager.distortionAxis == DistortionAxis.BothDirections)
            {
                float x = EditorGUILayout.Slider("Distortion Intensity (X)", manager.NoiseStrength.x, 0.01f, 0.1f);
                float y = EditorGUILayout.Slider("Distortion Intensity (Y)", manager.NoiseStrength.y, 0.01f, 0.1f);
                manager.NoiseStrength = new Vector2(x, y);
            }
            EditorGUI.indentLevel--;
            GUILayout.EndVertical();


            manager.NoiseScale = EditorGUILayout.Slider("Distortion Scale", manager.NoiseScale, 0f, 5000f);
            manager.animateLines = (AnimateLines)EditorGUILayout.EnumPopup("Animate Distortion", manager.animateLines);

            if (manager.animateLines == AnimateLines.On)
            {
                manager.AnimateNoise = true;
                manager.StepTime = EditorGUILayout.Slider("Distortion Speed", manager.StepTime, 0.1f, 0.5f);
            }
            else
            {
                if (manager.AnimateNoise) manager.AnimateNoise = false;
            }

        }

        else
        {
            if (manager.ApplyNoise) manager.ApplyNoise = false;
            if (manager.AnimateNoise) manager.AnimateNoise = false;
        }
        EditorGUILayout.Space();
    }

    private void SetBloomSettings(Object target, UltimateOutlineManager manager)
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

    private void SetCustomTextureSettings(Object target, UltimateOutlineManager manager)
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

    private void SetLayerSettings(Object target, UltimateOutlineManager manager)
    {
        EditorGUILayout.LabelField("Layer Mask", EditorStyles.boldLabel);
        manager.excludedLayerMask = LayerMaskField("Exclude from", manager.excludedLayerMask);
        if (_settings != null)
        {
            _settings.excludedLayerMask = manager.excludedLayerMask;
        }
    }

    private GUIStyle GetBoxStyle(Color c)
    {
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, c); //Background color
        tex.Apply();
        boxStyle.normal.background = tex;
        return boxStyle;
    }

    private int GetFilterLevel(OutlineFilter filter)
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

            default: return 5;
        }
    }

    private int GetMaxThickness(OutlineFilter filter)
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

    private LayerMask LayerMaskField(string label, LayerMask selected)
    {
        var layers = InternalEditorUtility.layers;
        var layerNumbers = new int[layers.Length];

        for (int i = 0; i < layers.Length; i++)
            layerNumbers[i] = LayerMask.NameToLayer(layers[i]);

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

    private Material GetFilterMaterial(string materialFolder, OutlineFilter newFilter)
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

    private OutlineFeature GetOutlineFeature()
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
