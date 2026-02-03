using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;
using UnityEditor.SceneManagement;

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

		#region Class Variables
		private HandyOutlinesSettings _settings;
		private SerializedObject _serializedSettings;
		private const string SETTINGS_PATH = "Packages/com.viktor_mjs.handy-outlines-customizable-highlight-effects/Runtime/HandyOutlinesSettings.asset";
		private const string MATERIALS_PATH = "Packages/com.viktor_mjs.handy-outlines-customizable-highlight-effects/Runtime/OutlineMaterials";
		#endregion

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
			InitialUISync();
			RegisterCallbacks();
			CheckSetup();
        }

        private void OnEnable()
        {
            EditorSceneManager.sceneOpened += (scene, mode) => { OnBloomCheckNeeded(); };
			EditorApplication.hierarchyChanged += () => { OnBloomCheckNeeded(); };
        }

        private void OnDisable()
        {
			EditorSceneManager.sceneOpened -= (scene, mode) => { OnBloomCheckNeeded(); };
			EditorApplication.hierarchyChanged -= () => { OnBloomCheckNeeded(); };
        } 

        /// <summary>
        /// Loads the settings scriptable object or creates one if it doesn't exist.
        /// </summary>
        /// <returns></returns> The loaded or newly created settings scriptable object.
        private HandyOutlinesSettings LoadSettings()
		{
			_settings = AssetDatabase.LoadAssetAtPath<HandyOutlinesSettings>(SETTINGS_PATH);
			if (_settings != null) return _settings;
			_settings = CreateInstance<HandyOutlinesSettings>();
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
			_animationSpeedField = _root.Q<Slider>("Animate_Speed_Field");
			_bloomEffectField = _root.Q<EnumField>("Bloom_Mode_Field");
			_bloomColorField = _root.Q<ColorField>("Bloom_Color_Field");
			_bloomIntensityField = _root.Q<Slider>("Bloom_Intensity_Field");
			_bloomIntermitentSpeedField = _root.Q<Slider>("Intermitent_Speed_Field");
			_customTextureEffectField = _root.Q<EnumField>("Custom_Texture_Effect_Field");
			_customTextureField = _root.Q<ObjectField>("Custom_Texture_Field");
			_textureSizeElement = _root.Q<VisualElement>("Texture_Size_Element");
			_textureSizeXField = _root.Q<Slider>("Texture_Size_X_Field");
			_textureSizeYField = _root.Q<Slider>("Texture_Size_Y_Field");
			_layerMaskField = _root.Q<LayerMaskField>("Layer_Mask_Field");
		}

		/// <summary>
		/// Initial synchronization of the UI based on the current settings values.
		/// </summary>
		private void InitialUISync()
		{
			OnFilterChanged(_serializedSettings.FindProperty("_outlineFilter"));
			OnModeChanged(_serializedSettings.FindProperty("_outlineMode"));
			OnStyleChanged(_serializedSettings.FindProperty("_outlineStyle"));
			OnBlendingChanged(_serializedSettings.FindProperty("_blendMode"));
			OnDistortionChanged(_serializedSettings.FindProperty("_noiseEffect"));
			OnFrequencyChanged(_serializedSettings.FindProperty("_noiseFrequency"));
			OnDirectionChanged(_serializedSettings.FindProperty("_distortionAxis"));
			OnAnimateLinesChanged(_serializedSettings.FindProperty("_animateLines"));
			OnBloomModeChanged(_serializedSettings.FindProperty("_bloomEffect"));
			OnCustomTextureEffectChanged(_serializedSettings.FindProperty("_textureEffect"));
			OnCustomTextureChanged(_serializedSettings.FindProperty("_customTex"));
		}

		/// <summary>
		/// Registers all necessary callbacks for UI elements to handle user interactions.
		/// </summary>
		private void RegisterCallbacks()
		{
			_setupButton.clicked += OnSetupButtonClicked;
			_root.TrackPropertyValue(_serializedSettings.FindProperty("_outlineFilter"), newFilter => OnFilterChanged(newFilter));
			_root.TrackPropertyValue(_serializedSettings.FindProperty("_outlineMode"), newMode => OnModeChanged(newMode));
			_root.TrackPropertyValue(_serializedSettings.FindProperty("_outlineStyle"), newStyle => OnStyleChanged(newStyle));
			_root.TrackPropertyValue(_serializedSettings.FindProperty("_blendMode"), newBlending => OnBlendingChanged(newBlending));
			_root.TrackPropertyValue(_serializedSettings.FindProperty("_noiseEffect"), newDistortion => OnDistortionChanged(newDistortion));
			_root.TrackPropertyValue(_serializedSettings.FindProperty("_noiseFrequency"), newFrequency => OnFrequencyChanged(newFrequency));
			_root.TrackPropertyValue(_serializedSettings.FindProperty("_distortionAxis"), newDirection => OnDirectionChanged(newDirection));
			_root.TrackPropertyValue(_serializedSettings.FindProperty("_animateLines"), newAnimate => OnAnimateLinesChanged(newAnimate));
			_root.TrackPropertyValue(_serializedSettings.FindProperty("_bloomEffect"), newBloomMode => OnBloomModeChanged(newBloomMode));
			_root.TrackPropertyValue(_serializedSettings.FindProperty("_textureEffect"), newCustomTextureMode => OnCustomTextureEffectChanged(newCustomTextureMode));
			_root.TrackPropertyValue(_serializedSettings.FindProperty("_customTex"), newTexture => OnCustomTextureChanged(newTexture));
			_root.TrackPropertyValue(_serializedSettings.FindProperty("_excludeLayerMask"), newLayerMask => OnExcludeLayerMaskChanged(newLayerMask));
			_root.TrackSerializedObjectValue(_serializedSettings, so => { _settings.UpdateOutlineMaterial(); });
		}

		/// <summary>
		/// Checks if the outline system is set up in the current project and toggles between setup and settings screens accordingly.
		/// </summary>
		private void CheckSetup()
		{
			if(GetOutlineFeature() == null)
			{
				_setupScreen.style.display = DisplayStyle.Flex;
				_settingsScreen.style.display = DisplayStyle.None;
			}
			else
			{
				_setupScreen.style.display = DisplayStyle.None;
				_settingsScreen.style.display = DisplayStyle.Flex;
			}
		}

		#region Callbacks
		private void OnSetupButtonClicked()
		{
			_setupScreen.style.display = DisplayStyle.None;
			_settingsScreen.style.display = DisplayStyle.Flex;
			AddOutlineFeature();
			CheckSetup();
		}

		/// <summary>
		/// Handles changes to the outline filter selection.
		/// </summary>
		/// <param name="newFilter"></param> The new selected filter property.
		private void OnFilterChanged(SerializedProperty newFilter)
		{
			Material newMat = GetOutlineMaterial((HandyOutlinesSettings.OutlineFilter)newFilter.enumValueIndex);
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

				_depthThicknessField.highValue = GetMaxThickness((HandyOutlinesSettings.OutlineFilter)newFilter.enumValueIndex);
				_normalThicknessField.highValue = GetMaxThickness((HandyOutlinesSettings.OutlineFilter)newFilter.enumValueIndex);
				_outerThicknessField.highValue = GetMaxThickness((HandyOutlinesSettings.OutlineFilter)newFilter.enumValueIndex) + 4f;
				_outerNormalThicknessField.highValue = GetMaxThickness((HandyOutlinesSettings.OutlineFilter)newFilter.enumValueIndex) + 4f;
			}
			else
			{
				Debug.LogWarning($"Material for filter {(HandyOutlinesSettings.OutlineFilter)newFilter.enumValueIndex} not found in {MATERIALS_PATH}");
			}
		}

		/// <summary>
		/// Handles changes to the outline mode selection.
		/// </summary>
		/// <param name="newMode"></param> The new selected mode property.
		private void OnModeChanged(SerializedProperty newMode)
		{
			switch (newMode.enumValueIndex)
			{
				case (int)HandyOutlinesSettings.OutlineMode.DepthOnly:
					_depthSettingsElement.style.display = DisplayStyle.Flex;
					_normalsSettingsElement.style.display = DisplayStyle.None;
					break;
				case (int)HandyOutlinesSettings.OutlineMode.DepthAndNormals:
					_depthSettingsElement.style.display = DisplayStyle.Flex;
					_normalsSettingsElement.style.display = DisplayStyle.Flex;
					break;
			}
		}

		/// <summary>
		/// Handles changes to the outline style selection.
		/// </summary>
		/// <param name="newStyle"></param> The new selected style property.
		private void OnStyleChanged(SerializedProperty newStyle)
		{
			switch (newStyle.enumValueIndex)
			{
				case (int)HandyOutlinesSettings.OutlineStyle.Simple:
					_outerOutlineSettingsElement.style.display = DisplayStyle.None;
					break;
				case (int)HandyOutlinesSettings.OutlineStyle.Double:
					_outerOutlineSettingsElement.style.display = DisplayStyle.Flex;
					break;
			}
		}

		/// <summary>
		/// Handles changes to the light blending mode selection.
		/// </summary>
		/// <param name="newBlending"></param> The new selected blending property.
		private void OnBlendingChanged(SerializedProperty newBlending)
		{
			switch (newBlending.enumValueIndex)
			{
				case (int)HandyOutlinesSettings.LightBlend.Off:
					_lightBlendingFactorField.style.display = DisplayStyle.None;
					break;
				case (int)HandyOutlinesSettings.LightBlend.On:
					_lightBlendingFactorField.style.display = DisplayStyle.Flex;
					break;
			}
		}

		/// <summary>
		/// Handles changes to the distortion effect selection.
		/// </summary>
		/// <param name="newDistortion"></param> The new selected distortion property.
		private void OnDistortionChanged(SerializedProperty newDistortion)
		{
			switch (newDistortion.enumValueIndex)
			{
				case (int)HandyOutlinesSettings.NoiseEffect.Off:
					_wavesSettingsElement.style.display = DisplayStyle.None;
					_pencilSettingsElement.style.display = DisplayStyle.None;
					_customSettingsElement.style.display = DisplayStyle.None;
					_animateLinesField.style.display = DisplayStyle.None;
					_animationSpeedField.style.display = DisplayStyle.None;
					break;
				case (int)HandyOutlinesSettings.NoiseEffect.Waves:
					_wavesSettingsElement.style.display = DisplayStyle.Flex;
					_pencilSettingsElement.style.display = DisplayStyle.None;
					_customSettingsElement.style.display = DisplayStyle.None;
					_animateLinesField.style.display = DisplayStyle.Flex;
					if ((HandyOutlinesSettings.AnimateLines)_animateLinesField.value == HandyOutlinesSettings.AnimateLines.On) 
					{
						_animationSpeedField.style.display = DisplayStyle.Flex;
					}
					break;
				case (int)HandyOutlinesSettings.NoiseEffect.Pencil:
					_wavesSettingsElement.style.display = DisplayStyle.None;
					_pencilSettingsElement.style.display = DisplayStyle.Flex;
					_customSettingsElement.style.display = DisplayStyle.None;
					_animateLinesField.style.display = DisplayStyle.Flex;
					if ((HandyOutlinesSettings.AnimateLines)_animateLinesField.value == HandyOutlinesSettings.AnimateLines.On) 
					{
						_animationSpeedField.style.display = DisplayStyle.Flex;
					}
					break;
				case (int)HandyOutlinesSettings.NoiseEffect.Custom:
					_wavesSettingsElement.style.display = DisplayStyle.None;
					_pencilSettingsElement.style.display = DisplayStyle.None;
					_customSettingsElement.style.display = DisplayStyle.Flex;
					_animateLinesField.style.display = DisplayStyle.Flex;
					if ((HandyOutlinesSettings.AnimateLines)_animateLinesField.value == HandyOutlinesSettings.AnimateLines.On) 
					{
						_animationSpeedField.style.display = DisplayStyle.Flex;
					}
					break;
			}
		}

		/// <summary>
		/// Handles changes to the noise frequency selection.
		/// </summary>
		/// <param name="newFrequency"></param> The new selected frequency property.
		private void OnFrequencyChanged(SerializedProperty newFrequency)
		{
			switch (newFrequency.enumValueIndex)
			{
				case (int)HandyOutlinesSettings.NoiseFrequency.Low:
					_waveCustomFrequencyField.value = 200f;
					_waveCustomFrequencyField.style.display = DisplayStyle.None;
					break;
				case (int)HandyOutlinesSettings.NoiseFrequency.Mid:
					_waveCustomFrequencyField.value = 500f;
					_waveCustomFrequencyField.style.display = DisplayStyle.None;
					break;
				case (int)HandyOutlinesSettings.NoiseFrequency.High:
					_waveCustomFrequencyField.value = 1000f;
					_waveCustomFrequencyField.style.display = DisplayStyle.None;
					break;
				case (int)HandyOutlinesSettings.NoiseFrequency.Custom:
					_waveCustomFrequencyField.style.display = DisplayStyle.Flex;
					break;
			}
		}

		/// <summary>
		/// Handles changes to the distortion direction selection.
		/// </summary>
		/// <param name="newDirection"></param> The new selected direction property.
		private void OnDirectionChanged(SerializedProperty newDirection)
		{
			switch (newDirection.enumValueIndex)
			{
				case (int)HandyOutlinesSettings.DistortionAxis.X:
				_waveIntensityXField.style.display = DisplayStyle.Flex;
					_pencilIntensityXField.style.display = DisplayStyle.Flex;
					_customIntensityXField.style.display = DisplayStyle.Flex;
					_waveIntensityYField.style.display = DisplayStyle.None;
					_pencilIntensityYField.style.display = DisplayStyle.None;
					_customIntensityYField.style.display = DisplayStyle.None;
					_waveIntensityYField.value = 0f; //Just need one of the three to reset as they are bound to the same property
					break;
				case (int)HandyOutlinesSettings.DistortionAxis.Y:
					_waveIntensityXField.style.display = DisplayStyle.None;
					_pencilIntensityXField.style.display = DisplayStyle.None;
					_customIntensityXField.style.display = DisplayStyle.None;
					_waveIntensityYField.style.display = DisplayStyle.Flex;
					_pencilIntensityYField.style.display = DisplayStyle.Flex;
					_customIntensityYField.style.display = DisplayStyle.Flex;
					_waveIntensityXField.value = 0f; //Just need one of the three to reset as they are bound to the same property
					break;
				case (int)HandyOutlinesSettings.DistortionAxis.BothDirections:
					_waveIntensityXField.style.display = DisplayStyle.Flex;
					_pencilIntensityXField.style.display = DisplayStyle.Flex;
					_customIntensityXField.style.display = DisplayStyle.Flex;
					_waveIntensityYField.style.display = DisplayStyle.Flex;
					_pencilIntensityYField.style.display = DisplayStyle.Flex;
					_customIntensityYField.style.display = DisplayStyle.Flex;
					break;
			}
		}

		/// <summary>
		/// Handles changes to the animate lines selection.
		/// </summary>
		/// <param name="newAnimate"></param> The new selected animate lines property.
		private void OnAnimateLinesChanged(SerializedProperty newAnimate)
		{
			switch (newAnimate.enumValueIndex)
			{
				case (int)HandyOutlinesSettings.AnimateLines.Off:
					_animationSpeedField.style.display = DisplayStyle.None;
					break;
				case (int)HandyOutlinesSettings.AnimateLines.On:
					_animationSpeedField.style.display = DisplayStyle.Flex;
					break;
			}
		}

		/// <summary>
		/// Handles changes to the bloom effect selection.
		/// </summary>
		/// <param name="newBloomMode"></param> The new selected bloom effect property.
		private void OnBloomModeChanged(SerializedProperty newBloomMode)
		{
			switch (newBloomMode.enumValueIndex)
			{
				case (int)HandyOutlinesSettings.BloomEffect.Off:
					_bloomColorField.style.display = DisplayStyle.None;
					_bloomIntensityField.style.display = DisplayStyle.None;
					_bloomIntermitentSpeedField.style.display = DisplayStyle.None;
					break;
				case (int)HandyOutlinesSettings.BloomEffect.Simple:
					_bloomColorField.style.display = DisplayStyle.Flex;
					_bloomIntensityField.style.display = DisplayStyle.Flex;
					_bloomIntermitentSpeedField.style.display = DisplayStyle.None;
					_outlineColorField.value = Color.white; //Force outline color to white for best bloom results
					if (_outlineStyleField.value.Equals(HandyOutlinesSettings.OutlineStyle.Double)) _outerColorField.value = Color.white;
					CheckVolumeSetup();
					break;
				case (int)HandyOutlinesSettings.BloomEffect.Intermitent:
					_bloomColorField.style.display = DisplayStyle.Flex;
					_bloomIntensityField.style.display = DisplayStyle.Flex;
					_bloomIntermitentSpeedField.style.display = DisplayStyle.Flex;
					_outlineColorField.value = Color.white; //Force outline color to white for best bloom results
					if (_outlineStyleField.value.Equals(HandyOutlinesSettings.OutlineStyle.Double)) _outerColorField.value = Color.white;
					CheckVolumeSetup();
					break;
			}
		}

		/// <summary>
		/// Handles changes to the custom texture effect selection.
		/// </summary>
		/// <param name="newCustomTextureMode"></param> The new selected custom texture effect property.
		private void OnCustomTextureEffectChanged(SerializedProperty newCustomTextureMode)
		{
			switch (newCustomTextureMode.enumValueIndex)
			{
				case (int)HandyOutlinesSettings.TextureEffect.Off:
					_customTextureField.style.display = DisplayStyle.None;
					_textureSizeElement.style.display = DisplayStyle.None;
					break;
				case (int)HandyOutlinesSettings.TextureEffect.On:
					_customTextureField.style.display = DisplayStyle.Flex;
					if(_customTextureField.value != null) _textureSizeElement.style.display = DisplayStyle.Flex;
					break;
			}
		}

		/// <summary>
		/// Handles changes to the custom texture selection.
		/// </summary>
		/// <param name="newTexture"></param> The new selected custom texture property.
		private void OnCustomTextureChanged(SerializedProperty newTexture)
		{
			if (newTexture.objectReferenceValue != null) _textureSizeElement.style.display = DisplayStyle.Flex;
			else _textureSizeElement.style.display = DisplayStyle.None;
		}

		private void OnExcludeLayerMaskChanged(SerializedProperty newLayerMask)
		{
			OutlineFeature.SharedSettings.excludedLayerMask = (LayerMask)newLayerMask.intValue;
		}
		#endregion

		#region Helpers
		/// <summary>
		/// Retrieves the outline material corresponding to the selected outline filter.
		/// </summary>
		/// <param name="newFilter"></param> The selected outline filter.
		/// <returns></returns> The corresponding outline material.
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

		/// <summary>
		/// Retrieves the OutlineFeature from the current Universal Render Pipeline Asset.
		/// </summary>
		/// <returns></returns> The OutlineFeature if found, otherwise null.
		public static OutlineFeature GetOutlineFeature()
        {
            UniversalRenderPipelineAsset urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

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
            ScriptableRendererData rendererData = rendererDataProp.objectReferenceValue as ScriptableRendererData;
            
			if (rendererData == null)
            {
                Debug.LogWarning("RendererData is null in URP asset.");
                return null;
            }

            foreach (var feature in rendererData.rendererFeatures)
            {
                if (feature is OutlineFeature outlineFeature) return outlineFeature;
            }
			Debug.LogWarning("No OutlineFeature found in the current URP RendererData.");
            return null;
        }

		/// <summary>
		/// Adds the OutlineFeature and the SSAO to the current Universal Render Pipeline Asset.
		/// </summary>
		private void AddOutlineFeature()
		{
			UniversalRenderPipelineAsset urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
			if (urpAsset == null) return;

			SerializedObject so = new SerializedObject(urpAsset);
			int defaultRendererIndex = so.FindProperty("m_DefaultRendererIndex").intValue;
			SerializedProperty rendererDataList = so.FindProperty("m_RendererDataList");
			ScriptableRendererData rendererData = rendererDataList.GetArrayElementAtIndex(defaultRendererIndex).objectReferenceValue as ScriptableRendererData;

			if (rendererData == null) return;

			bool hasSSAO = false;
			foreach (var rendererFeature in rendererData.rendererFeatures)
			{
				if (rendererFeature != null && rendererFeature.GetType().Name.Contains("ScreenSpaceAmbientOcclusion"))
				{
					hasSSAO = true;
					break;
				}
			}

			if (!hasSSAO)
			{
				var ssaoFeature = CreateInstance<ScreenSpaceAmbientOcclusion>();
				ssaoFeature.name = "Screen Space Ambient Occlusion";
				AssetDatabase.AddObjectToAsset(ssaoFeature, rendererData);
				rendererData.rendererFeatures.Add(ssaoFeature);
			}

			OutlineFeature feature = CreateInstance<OutlineFeature>();
			feature.name = "Handy Outline Feature";
			
			feature.material = GetOutlineMaterial(_settings.GetOutlineFilter());

			AssetDatabase.AddObjectToAsset(feature, rendererData);
			
			rendererData.rendererFeatures.Add(feature);
			
			EditorUtility.SetDirty(urpAsset);
			EditorUtility.SetDirty(rendererData);

			GraphicsSettings.defaultRenderPipeline = null;
			GraphicsSettings.defaultRenderPipeline = urpAsset;

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
		}

		/// <summary>
		/// Checks and sets up a global volume with Bloom effect if it doesn't already exist.
		/// </summary>
		private void CheckVolumeSetup()
		{
			Volume volume = FindAnyObjectByType<Volume>();
			
			if (volume == null)
			{
				GameObject volumeInstance = new GameObject("HandyOutlines_GlobalVolume");
				volume = volumeInstance.AddComponent<Volume>();
				volume.isGlobal = true;
			}

			if (volume.profile == null) volume.profile = CreateInstance<VolumeProfile>();

			if (!volume.profile.TryGet<Bloom>(out var bloom)) volume.profile.Add<Bloom>(true);

			EditorUtility.SetDirty(volume.profile);
		}

		private void OnBloomCheckNeeded()
		{
			if(_settings != null && (HandyOutlinesSettings.BloomEffect)_bloomEffectField.value != HandyOutlinesSettings.BloomEffect.Off)
			{
				CheckVolumeSetup();
			}
		}

		public int GetMaxThickness(HandyOutlinesSettings.OutlineFilter filter)
        {
            switch (filter)
            {
                case HandyOutlinesSettings.OutlineFilter.Laplacian:
                    return 6;
                case HandyOutlinesSettings.OutlineFilter.DoG:
                    return 7;
                default:
                    return 8;
            }
        }
		#endregion
	}
}