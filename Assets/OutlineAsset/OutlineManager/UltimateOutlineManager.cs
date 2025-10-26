using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class UltimateOutlineManager : MonoBehaviour
{
    [Header("Shader & Material Settings")]
    private Material _material;

    [Header("Outline Mode")]
    public OutlineMode Mode = OutlineMode.DepthOnly;

    [Header("Outline Settings")]
    [Range(1, 10)] public float OutlineThickness = 1f;
    [Range(0, 1)] public float OutlineStrength = 1f;
    [Range(0, 2)] public float Threshold = 0.3f;
    [ColorUsage(true, true)] public Color OutlineColor = Color.white;
    public float EdgeMin = 0.1f;

    [Header("Light Blend")]
    public LightBlend BlendMode = LightBlend.Off;
    public bool ApplyLightColor = false;
    [Range(0, 1)] public float LightFactor = 1f;

    [Header("Noise Effect")]
    [Tooltip("Enable to apply noise to the outline for a stylized effect.")]
    public bool ApplyNoise = false;
    public bool AnimateNoise = false;
    public NoiseEffect noiseEffect = NoiseEffect.Off;
    public NoiseFrequency noiseFrequency = NoiseFrequency.Low;
    public AnimateLines animateLines = AnimateLines.Off;
    [Range(0.01f, 0.5f)] public float StepTime = 0.2f;
    [Range(20f, 1000f)] public float NoiseScale = 0.3f;
    public DistortionAxis distortionAxis = DistortionAxis.BothDirections;
    public Vector2 NoiseStrength = new Vector2(0.01f, 0.01f);

    [Header("Normal Settings")]
    public bool UseNormal = true;
    [Range(0f, 15f)] public float NormalThreshold = 1f;
    [Range(0, 1f)] public float NormalStrength = 0.7f;
    [Range(0.1f, 10f)] public float NormalThickness = 2f;

    [Header("Bloom Settings")]
    public bool UseBloom = false;
    [Range(1, 5)] public float BloomIntensity = 2;
    [ColorUsage(true, true)] public Color BloomColor = Color.white;
    public BloomEffect bloomEffect = BloomEffect.Off;

    [Header("Intermittent Effect")]
    public bool UseIntermitent = false;
    [Range(0.5f, 5f)] public float IntermitentSpeed = 3f;

    [Header("Camera Settings")]
    public bool CameraOrtographic = false;

    [Header("Custom Texture")]
    public TextureEffect textureEffect = TextureEffect.Off;
    public bool UseCustomTexture = false;
    public Texture CustomTex;
    public Vector2 TexSize = new Vector2(20, 10);

    [Header("Global Volume")]
    private Volume _sceneVolume;

    public enum OutlineMode
    {
        DepthOnly,
        DepthAndNormals
    }

    public enum LightBlend
    {
        Off,
        On
    }

    public enum NoiseEffect
    {
        Off,
        Waves,
        Pencil,
        Custom
    }

    public enum NoiseFrequency
    {
        Low,
        Mid,
        High,
        Custom
    }

    public enum DistortionAxis
    {
        X,
        Y,
        BothDirections
    }

    public enum AnimateLines
    {
        Off,
        On
    }

    public enum BloomEffect
    {
        Off,
        Simple,
        Intermitent
    }

    public enum TextureEffect
    {
        Off,
        On
    }

    private void OnEnable()
    {
        if (OutlineFeature.SharedOutlineMaterial != null)
        {
            _material = OutlineFeature.SharedOutlineMaterial;
        }
        else
        {
            Debug.LogWarning("UltimateOutlineManager: No se encontró el material de OutlineFeature. Asegúrate de que la feature esté activa en el Renderer.");
        }
        Camera camera = Camera.main;
        camera.nearClipPlane = 3f;
        camera.farClipPlane = 1000f;
        SetBloom();
    }

    private void Update()
    {
        if (_material == null)
        {
            return;
        }

        // Outline
        _material.SetFloat("_OutlineThickness", OutlineThickness);
        _material.SetFloat("_OutlineStrength", OutlineStrength);
        _material.SetFloat("_Threshold", Threshold);
        _material.SetColor("_OutlineColor", OutlineColor);
        _material.SetFloat("_EdgeMin", EdgeMin);

        // Noise
        _material.SetFloat("_ApplyNoise", ApplyNoise ? 1f : 0f);
        _material.SetFloat("_AnimateNoise", AnimateNoise ? 1f : 0f);
        _material.SetFloat("_NoiseScale", NoiseScale);
        _material.SetVector("_NoiseStrength", NoiseStrength);

        // Lighting
        _material.SetFloat("_ApplyLightColor", ApplyLightColor ? 1f : 0f);
        _material.SetFloat("_LightFactor", LightFactor);
        _material.SetFloat("_StepTime", StepTime);

        // Normals
        _material.SetFloat("_UseNormal", UseNormal ? 1f : 0f);
        _material.SetFloat("_NormalThreshold", NormalThreshold);
        _material.SetFloat("_NormalStrength", NormalStrength);

        // Bloom
        _material.SetFloat("_UseBloom", UseBloom ? 1f : 0f);
        _material.SetFloat("_BloomIntensity", BloomIntensity);
        _material.SetColor("_BloomColor", BloomColor);

        // Intermittent
        _material.SetFloat("_UseIntermitent", UseIntermitent ? 1f : 0f);
        _material.SetFloat("_IntermitentSpeed", IntermitentSpeed);

        // Camera
        _material.SetFloat("_CameraOrtographic", CameraOrtographic ? 1f : 0f);
        _material.SetFloat("_NormalThickness", NormalThickness);

        // Custom Texture
        _material.SetFloat("_CustomTexture", UseCustomTexture ? 1f : 0f);
        if (CustomTex != null)
            _material.SetTexture("_CustomTex", CustomTex);
        _material.SetVector("_TexSize", TexSize);

        //Global volume (Bloom)
        if (_sceneVolume != null && _sceneVolume.profile != null)
        {
            if (_sceneVolume.profile.TryGet<Bloom>(out var bloom))
            {
                bloom.tint.overrideState = true;
                bloom.tint.value = BloomColor;
            }
        }

    }

    private void SetBloom()
    {
        _sceneVolume = FindAnyObjectByType<Volume>();
        if (_sceneVolume == null)
        {
            var volumeGO = new GameObject("Global Volume");
            var volume = volumeGO.AddComponent<Volume>();
            volume.isGlobal = true;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>(); //not data asset
            if (!profile.TryGet<Bloom>(out var bloom))
            {
                bloom = profile.Add<Bloom>(true);
                bloom.active = true;
                bloom.threshold.overrideState = true;
                bloom.intensity.overrideState = true;
                bloom.scatter.overrideState = true;
                bloom.tint.overrideState = true;
                bloom.highQualityFiltering.overrideState = true;

                bloom.clamp.overrideState = false;
                bloom.downscale.overrideState = false;
                bloom.maxIterations.overrideState = false;
                bloom.dirtTexture.overrideState = false;
                bloom.dirtIntensity.overrideState = false;

                //initial values
                bloom.threshold.value = 2f;
                bloom.intensity.value = 2f;
                bloom.scatter.value = 0.5f;
                bloom.tint.value = BloomColor;
                bloom.highQualityFiltering.value = true;

            }
            volume.profile = profile;
            _sceneVolume = volume;
        }
    }
}
