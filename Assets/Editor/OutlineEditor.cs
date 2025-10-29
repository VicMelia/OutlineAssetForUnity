using UnityEngine;
using UnityEditor;
using static UltimateOutlineManager;

[CustomEditor(typeof(UltimateOutlineManager))]
public class OutlineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var manager = (UltimateOutlineManager)target;
        serializedObject.Update();


        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(manager), typeof(UltimateOutlineManager), false);
        }
        EditorGUILayout.Space();

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

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(manager);
            SceneView.RepaintAll();
        }
    }

    void SetOulineSettings(Object target, UltimateOutlineManager manager)
    {
        EditorGUILayout.LabelField("Outline Settings", EditorStyles.boldLabel);
        manager.Mode = (OutlineMode)EditorGUILayout.EnumPopup("Outline Mode", manager.Mode);
        EditorGUILayout.Space();
        manager.OutlineColor = EditorGUILayout.ColorField("Outline Color", manager.OutlineColor);
        EditorGUILayout.Space();

        EditorGUI.indentLevel++;
        GUILayout.BeginVertical(GetBoxStyle(new Color(0.1f, 0.1f, 0.1f, 0.3f)));
        EditorGUILayout.LabelField("Depth Settings", EditorStyles.miniBoldLabel);
        manager.OutlineThickness = EditorGUILayout.Slider("Outline Thickness", manager.OutlineThickness, 0.1f, 10f);
        manager.OutlineStrength = EditorGUILayout.Slider("Outline Opacity", manager.OutlineStrength, 0f, 1f);
        manager.Threshold = EditorGUILayout.Slider("Threshold", manager.Threshold, 0.1f, 2f);
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        //manager.EdgeMin = EditorGUILayout.FloatField("Edge Min", manager.EdgeMin);

        if (manager.Mode == OutlineMode.DepthAndNormals)
        {
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
        }
        else
        {
            if (manager.UseNormal) manager.UseNormal = false;
        }

        EditorGUILayout.Space();

    }

    void SetLightSettings(Object target, UltimateOutlineManager manager)
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

    void SetNoiseSettings(Object target, UltimateOutlineManager manager)
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

    void SetBloomSettings(Object target, UltimateOutlineManager manager)
    {
        EditorGUILayout.LabelField("Bloom", EditorStyles.boldLabel);
        manager.bloomEffect = (BloomEffect)EditorGUILayout.EnumPopup("Bloom Mode", manager.bloomEffect);
        if (manager.bloomEffect == BloomEffect.Simple)
        {
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

    void SetCustomTextureSettings(Object target, UltimateOutlineManager manager)
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

    GUIStyle GetBoxStyle(Color c)
    {
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, c); // color del fondo
        tex.Apply();
        boxStyle.normal.background = tex;
        return boxStyle;
    }


}
