using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HandyOutlines
{
    public class HandyOutlinesSettings : ScriptableObject
    {
        #region Settings Enums
        public enum OutlineFilter
        {
            RobertsCross,
            Sobel,
            Prewitt,
            Scharr,
            Laplacian,
            [InspectorName("Difference of Gaussians (DoG)")]
            DoG
        }
        public enum OutlineMode
        {
            DepthOnly,
            DepthAndNormals
        }
        public enum OutlineStyle
        {
            Simple,
            Double
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
        #endregion

        #region Settings Variables
        [SerializeField] private Material _outlineMaterial;
        [SerializeField] private OutlineFilter _outlineFilter = OutlineFilter.Sobel;
        [SerializeField] private OutlineMode _outlineMode = OutlineMode.DepthOnly;
        [SerializeField] private float _outlineThickness = 1f, _outlineStrength = 1f, _outlineThreshold = 0.5f;
        [SerializeField] private Color _outlineColor = Color.white;
        [SerializeField] private OutlineStyle _outlineStyle = OutlineStyle.Simple;
        [SerializeField] private float _doubleThickness = 1f, _doubleNormalThickness = 2f;
        [SerializeField] private Color _doubleColor = Color.white;
        [SerializeField] private LightBlend _blendMode = LightBlend.Off;
        [SerializeField] private float _lightFactor = 1f;
        [SerializeField] private NoiseEffect _noiseEffect = NoiseEffect.Off;
        [SerializeField] private NoiseFrequency _noiseFrequency = NoiseFrequency.Low;
        [SerializeField] private AnimateLines _animateLines = AnimateLines.Off;
        [SerializeField] private float _stepTime = 0.2f, _noiseScale = 0.3f;
        [SerializeField] private DistortionAxis _distortionAxis = DistortionAxis.BothDirections;
        [SerializeField] private Vector2 _noiseStrength = new Vector2(0.1f, 0.1f);
        [SerializeField] private float _normalThreshold = 1f, _normalStrength = 0.7f, _normalThickness = 2f;
        [SerializeField] private float _bloomIntensity = 1f;
        [SerializeField] private Color _bloomColor = Color.white;
        [SerializeField] private BloomEffect _bloomEffect = BloomEffect.Off;
        [SerializeField] private float _bloomIntermitentSpeed = 3f;
        [SerializeField] private TextureEffect _textureEffect = TextureEffect.Off;
        [SerializeField] private Texture2D _customTex;
        [SerializeField] private Vector2 _texSize = new Vector2(20f, 10f);
        [SerializeField] private LayerMask _excludeLayerMask = 0;
        #endregion

        public void UpdateOutlineMaterial(Material outlineMaterial = null)
        {
            if (outlineMaterial != null) _outlineMaterial = outlineMaterial;
            if (_outlineMaterial == null) return;

            //Outline
            _outlineMaterial.SetFloat("_OutlineThickness", _outlineThickness);
            _outlineMaterial.SetFloat("_OutlineStrength", _outlineStrength);
            _outlineMaterial.SetFloat("_Threshold", _outlineThreshold);
            _outlineMaterial.SetColor("_OutlineColor", _outlineColor);

            //Double Outline
            _outlineMaterial.SetFloat("_DoubleMode", (float)_outlineStyle);
            _outlineMaterial.SetFloat("_DoubleThickness", _doubleThickness);
            _outlineMaterial.SetFloat("_DoubleNormalThickness", _doubleNormalThickness);
            _outlineMaterial.SetColor("_DoubleColor", _doubleColor);

            //Noise
            _outlineMaterial.SetFloat("_ApplyNoise", _noiseEffect == NoiseEffect.Off ? 0f : 1f);
            _outlineMaterial.SetFloat("_AnimateNoise", (float)_animateLines);
            _outlineMaterial.SetFloat("_NoiseScale", _noiseScale);
            _outlineMaterial.SetVector("_NoiseStrength", _noiseStrength);
            _outlineMaterial.SetFloat("_StepTime", _stepTime);

            //Lighting
            _outlineMaterial.SetFloat("_ApplyLightColor", (float)_blendMode);
            _outlineMaterial.SetFloat("_LightFactor", _lightFactor);

            //Normals
            _outlineMaterial.SetFloat("_UseNormal", (float)_outlineMode);
            _outlineMaterial.SetFloat("_NormalThreshold", _normalThreshold);
            _outlineMaterial.SetFloat("_NormalStrength", _normalStrength);
            _outlineMaterial.SetFloat("_NormalThickness", _normalThickness);

            //Bloom
            _outlineMaterial.SetFloat("_UseBloom", _bloomEffect == BloomEffect.Off ? 0f : 1f);
            _outlineMaterial.SetFloat("_BloomIntensity", _bloomIntensity);
            _outlineMaterial.SetColor("_BloomColor", _bloomColor);

            //Intermittent
            _outlineMaterial.SetFloat("_UseIntermitent", _bloomEffect == BloomEffect.Intermitent ? 1f : 0f);
            _outlineMaterial.SetFloat("_IntermitentSpeed", _bloomIntermitentSpeed);

            //Custom Texture
            _outlineMaterial.SetFloat("_CustomTexture", (float)_textureEffect);
            _outlineMaterial.SetTexture("_CustomTex", _customTex);
            _outlineMaterial.SetVector("_TexSize", _texSize);

            //Global volume (Bloom)
            /*if (_sceneVolume != null && _sceneVolume.profile != null)
            {
                if (_sceneVolume.profile.TryGet<Bloom>(out var bloom))
                {
                    bloom.tint.overrideState = true;
                    bloom.tint.value = _bloomColor;
                }
            }*/
        }

        public OutlineFilter GetOutlineFilter()
        {
            return _outlineFilter;
        }
    }
}
