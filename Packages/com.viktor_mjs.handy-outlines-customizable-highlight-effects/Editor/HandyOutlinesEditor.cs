using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HandyOutlines
{
	public class HandyOutlinesEditor : EditorWindow
	{
		#region Editor Elements
		[SerializeField] private VisualTreeAsset _uxml;
		private VisualElement _root, _setupScreen;
		private Button _setupButton;
		private ScrollView _settingsScreen;
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
		private VisualElement _textureSizeElement;
		private Slider _textureSizeXField, _textureSizeYField;
		private LayerMaskField _layerMaskField;
		#endregion

		private HandyOutlinesSettings _settings;
		private SerializedObject _serializedSettings;
		private const string SETTINGS_PATH = "Packages/com.viktor_mjs.handy-outlines-customizable-highlight-effects/Runtime/HandyOutlinesSettings.asset";
		private const string MATERIALS_PATH = "Packages/com.viktor_mjs.handy-outlines-customizable-highlight-effects/Runtime/OutlineMaterials";

		[MenuItem("Tools/Handy Outlines")]
		public static void ShowWindow()
		{
			HandyOutlinesEditor window = GetWindow<HandyOutlinesEditor>("Handy Outlines");
			window.titleContent = new GUIContent("Handy Outlines");
		}

        private void CreateGUI()
        {
			//Read or create settings asset
			_settings = LoadSettings();
			_serializedSettings = new SerializedObject(_settings);

			//Load UXML
            _root = rootVisualElement;
			_uxml.CloneTree(_root);

			//Bind serialized settings to UI
			_root.Bind(_serializedSettings);

			AssignUIElements();
			RegisterCallbacks();
			CheckSetup();
        }

		/// <summary>
		/// Loads the settings scriptable object or creates one if it doesn't exist.
		/// </summary>
		/// <returns></returns> The loaded or newly created settings scriptable object.
		private HandyOutlinesSettings LoadSettings()
		{
			_settings = AssetDatabase.LoadAssetAtPath<HandyOutlinesSettings>(SETTINGS_PATH);
			if (_settings != null) return _settings;
			_settings = ScriptableObject.CreateInstance<HandyOutlinesSettings>();
			AssetDatabase.CreateAsset(_settings, SETTINGS_PATH);
			AssetDatabase.SaveAssets();
			return _settings;
		}

		/// <summary>
		/// Assigns all UI elements from the UXML to their respective variables.
		/// </summary>
		private void AssignUIElements()
		{
			_setupScreen = _root.Q<VisualElement>("Setup_Screen");
			_setupButton = _root.Q<Button>("Setup_Button");
			_settingsScreen = _root.Q<ScrollView>("Settings_Screen");
			_outlineFilterField = _root.Q<EnumField>("Outline_Filter_Field");
			_outlineModeField = _root.Q<EnumField>("Outline_Mode_Field");
			_outlineColorField = _root.Q<ColorField>("Outline_Color_Field");
			_depthSettingsElement = _root.Q<VisualElement>("Depth_Settings_Element");
			_depthThicknessField = _root.Q<Slider>("Depth_Thickness_Field");
			_depthOpacityField = _root.Q<Slider>("Depth_Opacity_Field");
			_depthReductionField = _root.Q<Slider>("Depth_Reduction_Field");
			_normalsSettingsElement = _root.Q<VisualElement>("Normal_Settings_Element");
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
			_wavesSettingsElement = _root.Q<VisualElement>("Wave_Settings_Element");
			_wavesFrequencyField = _root.Q<EnumField>("Wave_Frequency_Field");
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
			_animateLinesField = _root.Q<EnumField>("Animate_Distortion_Field");
			_animationSpeedField = _root.Q<Slider>("Animation_Speed_Field");
			_bloomEffectField = _root.Q<EnumField>("Bloom_Effect_Field");
			_bloomColorField = _root.Q<ColorField>("Bloom_Color_Field");
			_bloomIntensityField = _root.Q<Slider>("Bloom_Intensity_Field");
			_bloomIntermitentSpeedField = _root.Q<Slider>("Bloom_Intermitent_Speed_Field");
			_customTextureEffectField = _root.Q<EnumField>("Custom_Texture_Effect_Field");
			_customTextureField = _root.Q<ObjectField>("Custom_Texture_Field");
			_textureSizeElement = _root.Q<VisualElement>("Texture_Size_Element");
			_textureSizeXField = _root.Q<Slider>("Texture_Size_X_Field");
			_textureSizeYField = _root.Q<Slider>("Texture_Size_Y_Field");
			_layerMaskField = _root.Q<LayerMaskField>("Layer_Mask_Field");
		}

		private void RegisterCallbacks()
		{
			_setupButton.clicked += OnSetupButtonClicked;
			SerializedProperty filterProp = _serializedSettings.FindProperty("_outlineFilter");
			_root.TrackPropertyValue(filterProp, newFilter => OnFilterChanged(newFilter));
			SerializedProperty modeProp = _serializedSettings.FindProperty("_outlineMode");
			_root.TrackPropertyValue(modeProp, newMode => OnModeChanged(newMode));
			SerializedProperty styleProp = _serializedSettings.FindProperty("_outlineStyle");
			_root.TrackPropertyValue(styleProp, newStyle => OnStyleChanged(newStyle));
			SerializedProperty blendingProp = _serializedSettings.FindProperty("_blendMode");
			_root.TrackPropertyValue(blendingProp, newBlending => OnBlendingChanged(newBlending));
			SerializedProperty distortionProp = _serializedSettings.FindProperty("_noiseEffect");
			_root.TrackPropertyValue(distortionProp, newDistortion => OnDistortionChanged(newDistortion));
			SerializedProperty frequencyProp = _serializedSettings.FindProperty("_noiseFrequency");
			_root.TrackPropertyValue(frequencyProp, newFrequency => OnFrequencyChanged(newFrequency));
			

			_root.TrackSerializedObjectValue(_serializedSettings, so => { _settings.UpdateOutlineMaterial(); });
		}

		/// <summary>
		/// Checks if the outline system is set up in the current project and toggles between setup and settings screens accordingly.
		/// </summary>
		private void CheckSetup() //TODO: Implement actual setup check
		{
			if(true) // Replace this with actual setup check
			{
				_setupScreen.style.display = DisplayStyle.None;
				_settingsScreen.style.display = DisplayStyle.Flex;
			}
			else
			{
				_setupScreen.style.display = DisplayStyle.Flex;
				_settingsScreen.style.display = DisplayStyle.None;
			}
		}

		#region Callbacks
		private void OnSetupButtonClicked()
		{
			_setupScreen.style.display = DisplayStyle.None;
			_settingsScreen.style.display = DisplayStyle.Flex;
			//TODO: Implement actual setup process
		}

		private void OnFilterChanged(SerializedProperty newFilter)
		{
			Debug.Log("Outline filter changed");
			int newValue = newFilter.enumValueIndex;
			Material newMat = GetOutlineMaterial((HandyOutlinesSettings.OutlineFilter)newValue);
			if (newMat != null)
			{
				OutlineFeature.SharedOutlineMaterial = newMat;
				OutlineFeature outlineFeature = GetOutlineFeature(); 
				if (outlineFeature != null)
				{
					outlineFeature.material = newMat;
					EditorUtility.SetDirty(outlineFeature);
					AssetDatabase.SaveAssets();
				}

				_settings.UpdateOutlineMaterial(newMat);
			}
			else
			{
				Debug.LogWarning($"Material for filter {(HandyOutlinesSettings.OutlineFilter)newValue} not found in {MATERIALS_PATH}");
			}
		}

		private void OnModeChanged(SerializedProperty newMode)
		{
			int newValue = newMode.enumValueIndex;
			switch ((HandyOutlinesSettings.OutlineMode)newValue)
			{
				case HandyOutlinesSettings.OutlineMode.DepthOnly:
					_depthSettingsElement.style.display = DisplayStyle.Flex;
					_normalsSettingsElement.style.display = DisplayStyle.None;
					break;
				case HandyOutlinesSettings.OutlineMode.DepthAndNormals:
					_depthSettingsElement.style.display = DisplayStyle.Flex;
					_normalsSettingsElement.style.display = DisplayStyle.Flex;
					break;
			}
		}

		private void OnStyleChanged(SerializedProperty newStyle)
		{
			int newValue = newStyle.enumValueIndex;
			switch ((HandyOutlinesSettings.OutlineStyle)newValue)
			{
				case HandyOutlinesSettings.OutlineStyle.Simple:
					_outerOutlineSettingsElement.style.display = DisplayStyle.None;
					break;
				case HandyOutlinesSettings.OutlineStyle.Double:
					_outerOutlineSettingsElement.style.display = DisplayStyle.Flex;
					break;
			}
		}

		private void OnBlendingChanged(SerializedProperty newBlending)
		{
			int newValue = newBlending.enumValueIndex;
			switch ((HandyOutlinesSettings.LightBlend)newValue)
			{
				case HandyOutlinesSettings.LightBlend.Off:
					_lightBlendingFactorField.style.display = DisplayStyle.None;
					break;
				case HandyOutlinesSettings.LightBlend.On:
					_lightBlendingFactorField.style.display = DisplayStyle.Flex;
					break;
			}
		}

		private void OnDistortionChanged(SerializedProperty newDistortion)
		{
			int newValue = newDistortion.enumValueIndex;
			switch ((HandyOutlinesSettings.NoiseEffect)newValue)
			{
				case HandyOutlinesSettings.NoiseEffect.Off:
					_wavesSettingsElement.style.display = DisplayStyle.None;
					_pencilSettingsElement.style.display = DisplayStyle.None;
					_customSettingsElement.style.display = DisplayStyle.None;
					_animateLinesField.style.display = DisplayStyle.None;
					break;
				case HandyOutlinesSettings.NoiseEffect.Waves:
					_wavesSettingsElement.style.display = DisplayStyle.Flex;
					_pencilSettingsElement.style.display = DisplayStyle.None;
					_customSettingsElement.style.display = DisplayStyle.None;
					_animateLinesField.style.display = DisplayStyle.Flex;
					break;
				case HandyOutlinesSettings.NoiseEffect.Pencil:
					_wavesSettingsElement.style.display = DisplayStyle.None;
					_pencilSettingsElement.style.display = DisplayStyle.Flex;
					_customSettingsElement.style.display = DisplayStyle.None;
					_animateLinesField.style.display = DisplayStyle.Flex;
					break;
				case HandyOutlinesSettings.NoiseEffect.Custom:
					_wavesSettingsElement.style.display = DisplayStyle.None;
					_pencilSettingsElement.style.display = DisplayStyle.None;
					_customSettingsElement.style.display = DisplayStyle.Flex;
					_animateLinesField.style.display = DisplayStyle.Flex;
					break;
			}
		}

		private void OnFrequencyChanged(SerializedProperty newFrequency)
		{
			int newValue = newFrequency.enumValueIndex;
			switch ((HandyOutlinesSettings.NoiseFrequency)newValue)
			{
				case HandyOutlinesSettings.NoiseFrequency.Low:
					_waveCustomFrequencyField.value = 200f;
					_waveCustomFrequencyField.style.display = DisplayStyle.None;
					break;
				case HandyOutlinesSettings.NoiseFrequency.Mid:
					_waveCustomFrequencyField.value = 500f;
					_waveCustomFrequencyField.style.display = DisplayStyle.None;
					break;
				case HandyOutlinesSettings.NoiseFrequency.High:
					_waveCustomFrequencyField.value = 1000f;
					_waveCustomFrequencyField.style.display = DisplayStyle.None;
					break;
				case HandyOutlinesSettings.NoiseFrequency.Custom:
					_waveCustomFrequencyField.style.display = DisplayStyle.Flex;
					break;
			}
		}

		#endregion

		#region Helpers
		private Material GetOutlineMaterial(HandyOutlinesSettings.OutlineFilter newFilter)
		{
			string materialName = $"Outline_{newFilter}";
            string[] guids = AssetDatabase.FindAssets($"{materialName} t:Material", new[] {MATERIALS_PATH});
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                Material newMat = AssetDatabase.LoadAssetAtPath<Material>(path);
                return newMat;
            }
            else
            {
                Debug.LogWarning($" {materialName} not found in {MATERIALS_PATH}");
            }
            return null;
		}

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
		#endregion
	}
}
