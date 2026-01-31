using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace HandyOutlines
{
	public class HandyOutlinesEditor : EditorWindow
	{
		#region Editor Enums
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

		#region Editor Elements
		[SerializeField] private VisualTreeAsset _uxml;
		private VisualElement _root, _setupScreen;
		private Button _setupButton;
		private ScrollView _mainScrollView;
		private EnumField _outlineFilterField, _outlineModeField;
		private ColorField _outlineColorField;
		private VisualElement _depthSettingsElement;
		private Slider _depthThicknessField, _depthOpacityField, _depthReductionField;
		private VisualElement _normalsSettingsElement;
		private Slider _normalThicknessField, _normalOpacityField, _normalReductionField;
		private EnumField _outlineStyleField;
		private VisualElement _outerOutlineSettingsElement;
		private ColorField _outerColorField;
		private Slider _outerThicknessField, _outerNormalThicknessField;
		private EnumField _lightBlendingField;
		private Slider _lightBlendingFactorField;
		private EnumField _distortionModeField;
		private VisualElement _wavesSettingsElement;
		private EnumField _wavesFrequencyField;
		private Slider _waveCustomFrequencyField;
		private EnumField _waveDirectionField;
		private Slider _waveIntensityXField, _waveIntensityYField;
		private VisualElement _pencilSettingsElement;
		private EnumField _pencilDirectionField;
		private Slider _pencilIntensityXField, _pencilIntensityYField;
		private VisualElement _customSettingsElement;
		private EnumField _customDirectionField;
		private Slider _customIntensityXField, _customIntensityYField, _customScaleField;
		private EnumField _animateLinesField;
		private Slider _animationSpeedField;
		private EnumField _bloomEffectField;
		private ColorField _bloomColorField;
		private Slider _bloomIntensityField, _bloomIntermitentSpeedField;
		private EnumField _customTextureEffectField;
		private ObjectField _customTextureField;
		private LayerMaskField _layerMaskField;
		#endregion

		#region Editor variables
		private Material _outlineMaterial;
		private OutlineFilter _outlineFilter = OutlineFilter.Sobel;
		private float _filterSelector = 1f;
		private OutlineMode _outlineMode = OutlineMode.DepthOnly;
		private float _outlineThickness = 1f, _outlineStrength = 1f, _outlineThreshold = 0.5f;
		private Color _outlineColor = Color.white;
		private float _edgeMin = 0.01f;
		private OutlineStyle _outlineStyle = OutlineStyle.Simple;
		private bool doubleMode = true;
		private float _doubleThickness = 1f, _doubleNormalThickness = 2f;
		private Color _doubleColor = Color.white;
		private LightBlend _blendMode = LightBlend.Off;
		private bool applyLightColor = false;
		private float _lightFactor = 1f;
		private bool _applyNoise = false, _animateNoise = false;
		private NoiseEffect _noiseEffect = NoiseEffect.Off;
		private NoiseFrequency _noiseFrequency = NoiseFrequency.Low;
		private AnimateLines _animateLines = AnimateLines.Off;
		private float _stepTime = 0.2f, _noiseScale = 0.3f;
		private DistortionAxis _distortionAxis = DistortionAxis.BothDirections;
		private Vector2 _noiseStrength = new Vector2(0.1f, 0.1f);
		private bool _useNormal = true;
		private float _normalThreshold = 1f, _normalStrength = 0.7f, _normalThickness = 2f;
		private bool _useBloom = false;
		private float _bloomIntensity = 1f;
		private Color _bloomColor = Color.white;
		private BloomEffect _bloomEffect = BloomEffect.Off;
		private bool _useIntermitent = false;
		private float _bloomIntermitentSpeed = 3f;
		private bool _CameraOrtographic = false;
		private TextureEffect _textureEffect = TextureEffect.Off;
		private bool _useCustomTexture = false;
		private Texture2D _customTex;
		private Vector2 _texSize = new Vector2(20f, 10f);
		private Volume _sceneVolume;
		private LayerMask _excludeLayerMask = 0;
		#endregion

		[MenuItem("Tools/Handy Outlines")]
		public static void ShowWindow()
		{
			HandyOutlinesEditor window = GetWindow<HandyOutlinesEditor>("Handy Outlines");
			window.titleContent = new GUIContent("Handy Outlines");
		}

        private void CreateGUI()
        {
            _root = rootVisualElement;
			_uxml.CloneTree(_root);
			AssignUIElements();
			CheckSetup();
        }

		/// <summary>
		/// Assigns all UI elements from the UXML to their respective variables.
		/// </summary>
		private void AssignUIElements()
		{
			_setupScreen = _root.Q<VisualElement>("Setup_Screen");
			_setupButton = _root.Q<Button>("Setup_Button");
			_mainScrollView = _root.Q<ScrollView>("Main_ScrollView");
			_outlineFilterField = _root.Q<EnumField>("Outline_Filter_Field");
			_outlineModeField = _root.Q<EnumField>("Outline_Mode_Field");
			_outlineColorField = _root.Q<ColorField>("Outline_Color_Field");
			_depthSettingsElement = _root.Q<VisualElement>("Depth_Settings_Element");
			_depthThicknessField = _root.Q<Slider>("Depth_Thickness_Field");
			_depthOpacityField = _root.Q<Slider>("Depth_Opacity_Field");
			_depthReductionField = _root.Q<Slider>("Depth_Reduction_Field");
			_normalsSettingsElement = _root.Q<VisualElement>("Normals_Settings_Element");
			_normalThicknessField = _root.Q<Slider>("Normal_Thickness_Field");
			_normalOpacityField = _root.Q<Slider>("Normal_Opacity_Field");
			_normalReductionField = _root.Q<Slider>("Normal_Reduction_Field");
			_outlineStyleField = _root.Q<EnumField>("Outline_Style_Field");
			_outerOutlineSettingsElement = _root.Q<VisualElement>("Outer_Outline_Settings_Element");
			_outerColorField = _root.Q<ColorField>("Outer_Color_Field");
			_outerThicknessField = _root.Q<Slider>("Outer_Thickness_Field");
			_outerNormalThicknessField = _root.Q<Slider>("Outer_Normal_Thickness_Field");
			_lightBlendingField = _root.Q<EnumField>("Light_Blending_Field");
			_lightBlendingFactorField = _root.Q<Slider>("Light_Blending_Factor_Field");
			_distortionModeField = _root.Q<EnumField>("Distortion_Mode_Field");
			_wavesSettingsElement = _root.Q<VisualElement>("Waves_Settings_Element");
			_wavesFrequencyField = _root.Q<EnumField>("Waves_Frequency_Field");
			_waveCustomFrequencyField = _root.Q<Slider>("Wave_Custom_Frequency_Field");
			_waveDirectionField = _root.Q<EnumField>("Wave_Direction_Field");
			_waveIntensityXField = _root.Q<Slider>("Wave_Intensity_X_Field");
			_waveIntensityYField = _root.Q<Slider>("Wave_Intensity_Y_Field");
			_pencilSettingsElement = _root.Q<VisualElement>("Pencil_Settings_Element");
			_pencilDirectionField = _root.Q<EnumField>("Pencil_Direction_Field");
			_pencilIntensityXField = _root.Q<Slider>("Pencil_Intensity_X_Field");
			_pencilIntensityYField = _root.Q<Slider>("Pencil_Intensity_Y_Field");
			_customSettingsElement = _root.Q<VisualElement>("Custom_Settings_Element");
			_customDirectionField = _root.Q<EnumField>("Custom_Direction_Field");
			_customIntensityXField = _root.Q<Slider>("Custom_Intensity_X_Field");
			_customIntensityYField = _root.Q<Slider>("Custom_Intensity_Y_Field");
			_customScaleField = _root.Q<Slider>("Custom_Scale_Field");
			_animateLinesField = _root.Q<EnumField>("Animate_Lines_Field");
			_animationSpeedField = _root.Q<Slider>("Animation_Speed_Field");
			_bloomEffectField = _root.Q<EnumField>("Bloom_Effect_Field");
			_bloomColorField = _root.Q<ColorField>("Bloom_Color_Field");
			_bloomIntensityField = _root.Q<Slider>("Bloom_Intensity_Field");
			_bloomIntermitentSpeedField = _root.Q<Slider>("Bloom_Intermitent_Speed_Field");
			_customTextureEffectField = _root.Q<EnumField>("Custom_Texture_Effect_Field");
			_customTextureField = _root.Q<ObjectField>("Custom_Texture_Field");
			_layerMaskField = _root.Q<LayerMaskField>("Layer_Mask_Field");
		}

		private void CheckSetup()
		{
			//TODO
		}
	}
}
