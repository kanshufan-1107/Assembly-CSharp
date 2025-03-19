using UnityEngine;

public class SoundOptionsMenu : MonoBehaviour
{
	[Header("Sound")]
	[SerializeField]
	private ScrollbarControl m_masterVolume;

	[SerializeField]
	private ScrollbarControl m_musicVolume;

	[SerializeField]
	private ScrollbarControl m_dialogVolume;

	[SerializeField]
	private ScrollbarControl m_ambienceVolume;

	[SerializeField]
	private ScrollbarControl m_soundEffectVolume;

	[SerializeField]
	private UIBButton m_resetDefaultsButton;

	[SerializeField]
	private CheckBox m_backgroundSound;

	[SerializeField]
	private CheckBox m_monoSound;

	[SerializeField]
	private AudioSliderAssetReferences m_audioSliderAssetReferences;

	[SerializeField]
	[Header("Other")]
	private UIBButton m_backButton;

	[SerializeField]
	private UIBButton m_doneButton;

	private static SoundOptionsMenu s_instance;

	private bool m_isShown;

	private PegUIElement m_inputBlocker;

	private Vector3 m_normalScale;

	private Vector3 m_hiddenScale;

	public static SoundOptionsMenu Get()
	{
		return s_instance;
	}

	public void Awake()
	{
		s_instance = this;
		m_normalScale = base.transform.localScale;
		m_hiddenScale = 0.01f * m_normalScale;
		OverlayUI.Get().AddGameObject(base.gameObject);
		if (m_backButton != null)
		{
			m_backButton.AddEventListener(UIEventType.RELEASE, OnBackButtonReleased);
		}
		if (m_doneButton != null)
		{
			m_doneButton.AddEventListener(UIEventType.RELEASE, OnDoneButtonReleased);
		}
		VolumeSlidersUtils.InitializeVolumeSlider(m_masterVolume, Option.SOUND_VOLUME, m_audioSliderAssetReferences.m_onMasterVolumeReleasedAudio, base.gameObject);
		VolumeSlidersUtils.InitializeVolumeSlider(m_musicVolume, Option.MUSIC_VOLUME, m_audioSliderAssetReferences.m_onMusicVolumeReleasedAudio, base.gameObject);
		VolumeSlidersUtils.InitializeVolumeSlider(m_dialogVolume, Option.DIALOG_VOLUME, m_audioSliderAssetReferences.m_onDialogVolumeReleasedAudio, base.gameObject);
		VolumeSlidersUtils.InitializeVolumeSlider(m_ambienceVolume, Option.AMBIENCE_VOLUME, m_audioSliderAssetReferences.m_onAmbienceVolumeReleasedAudio, base.gameObject);
		VolumeSlidersUtils.InitializeVolumeSlider(m_soundEffectVolume, Option.SOUND_EFFECT_VOLUME, m_audioSliderAssetReferences.m_onSoundEffectVolumeReleasedAudio, base.gameObject);
		if (m_resetDefaultsButton != null)
		{
			m_resetDefaultsButton.AddEventListener(UIEventType.RELEASE, OnResetToDefaultsButtonReleased);
		}
		if (m_backgroundSound != null)
		{
			if (PlatformSettings.IsMobile())
			{
				m_backgroundSound.gameObject.SetActive(value: false);
			}
			else
			{
				m_backgroundSound.AddEventListener(UIEventType.RELEASE, delegate
				{
					ToggleSoundOptionHandler(Option.BACKGROUND_SOUND, m_backgroundSound);
				});
				m_backgroundSound.SetChecked(Options.Get().GetBool(Option.BACKGROUND_SOUND));
				Options.Get().RegisterChangedListener(Option.BACKGROUND_SOUND, delegate(Option option, object prevValue, bool existed, object userData)
				{
					OnTogglableSoundOptionChanged(option, m_backgroundSound);
				});
			}
		}
		if (m_monoSound != null)
		{
			m_monoSound.AddEventListener(UIEventType.RELEASE, delegate
			{
				ToggleSoundOptionHandler(Option.SOUND_MONO_ENABLED, m_monoSound);
			});
			m_monoSound.SetChecked(Options.Get().GetBool(Option.SOUND_MONO_ENABLED));
			Options.Get().RegisterChangedListener(Option.SOUND_MONO_ENABLED, delegate(Option option, object prevValue, bool existed, object userData)
			{
				OnTogglableSoundOptionChanged(option, m_monoSound);
			});
		}
		CreateInputBlocker();
	}

	public void Show()
	{
		ShowOrHide(showOrHide: true);
		AnimationUtil.ShowWithPunch(base.gameObject, m_hiddenScale, 1.1f * m_normalScale, m_normalScale, null, noFade: true);
	}

	public void Hide()
	{
		ShowOrHide(showOrHide: false);
	}

	private void ShowOrHide(bool showOrHide)
	{
		m_isShown = showOrHide;
		base.gameObject.SetActive(showOrHide);
	}

	public bool IsShown()
	{
		return m_isShown;
	}

	private void OnBackButtonReleased(UIEvent e)
	{
		Hide();
		if (OptionsMenu.Get() != null && !OptionsMenu.Get().IsShown())
		{
			OptionsMenu.Get().Show();
		}
	}

	private void OnDoneButtonReleased(UIEvent e)
	{
		Hide();
	}

	private void OnResetToDefaultsButtonReleased(UIEvent e)
	{
		Options options = Options.Get();
		options.RevertFloatToDefault(Option.SOUND_VOLUME);
		options.RevertFloatToDefault(Option.MUSIC_VOLUME);
		options.RevertFloatToDefault(Option.DIALOG_VOLUME);
		options.RevertFloatToDefault(Option.AMBIENCE_VOLUME);
		options.RevertFloatToDefault(Option.SOUND_EFFECT_VOLUME);
		options.RevertBoolToDefault(Option.BACKGROUND_SOUND);
		options.RevertBoolToDefault(Option.SOUND_MONO_ENABLED);
	}

	private void CreateInputBlocker()
	{
		GameObject inputBlocker = CameraUtils.CreateInputBlocker(CameraUtils.FindFirstByLayer(base.gameObject.layer), "SoundOptionMenuInputBlocker", this, base.transform, 10f);
		inputBlocker.layer = base.gameObject.layer;
		m_inputBlocker = inputBlocker.AddComponent<PegUIElement>();
		m_inputBlocker.AddEventListener(UIEventType.RELEASE, delegate
		{
			Hide();
		});
	}

	private void ToggleSoundOptionHandler(Option optionToToggle, CheckBox optionCheckbox)
	{
		if (optionCheckbox != null)
		{
			Options.Get().SetBool(optionToToggle, optionCheckbox.IsChecked());
		}
	}

	private void OnTogglableSoundOptionChanged(Option option, CheckBox optionCheckbox)
	{
		if (optionCheckbox != null)
		{
			optionCheckbox.SetChecked(Options.Get().GetBool(option));
		}
	}
}
