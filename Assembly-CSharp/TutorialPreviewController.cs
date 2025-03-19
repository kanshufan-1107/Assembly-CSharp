using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;
using UnityEngine.Video;

[CustomEditClass]
public class TutorialPreviewController : MonoBehaviour
{
	private enum TutorialPreviewWidgetState
	{
		NOTHING_SELECTED,
		TRADITIONAL_SELECTED,
		BATTLEGROUNDS_SELECTED,
		MERCENARIES_SELECTED
	}

	[CustomEditField(T = EditType.VIDEO)]
	public string m_traditionalVideo;

	[CustomEditField(T = EditType.VIDEO)]
	public string m_battlegroundsVideo;

	[CustomEditField(T = EditType.VIDEO)]
	public string m_mercenariesVideo;

	private AudioSource m_lastPickedVO;

	private Coroutine m_lastCoroutine;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_tutorialTraditionalDescriptionVO = "VO_FTUE_01_Jaina_ModeDescription.prefab:715d99597ad67a14d9dbb371def5526c";

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_tutorialBattlegroundsDescriptionVO = "VO_FTUE_02_Bob_ModeDescription.prefab:96c4ac8907771ff4aa0db0e4136a65c9";

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_tutorialMercenariesDescriptionVO = "VO_FTUE_03_Valeera_ModeDescription.prefab:db54cf1e5b5f0034ba57c75a9c589338";

	private Dictionary<string, AudioSource> m_tutorialDescriptionVOMap;

	public float m_bannerAudioDelay = 0.5f;

	public Spell m_portalSpell;

	public VisualController m_brassRingVisualController;

	public VideoPlayer m_videoPlayer;

	public Spell m_confirmButtonSpell;

	public GameObject m_root;

	private AssetHandle<VideoClip> m_loadedVideo;

	private Widget m_widget;

	private VisualController m_visualController;

	private Action m_onSelectionConfirmedCallback;

	private TutorialPreviewWidgetState m_currentState;

	private TutorialPreviewDataModel m_tutorialPreviewDataModel;

	private const string CancelClicked = "CANCEL_CLICKED";

	private const string ConfirmClicked = "CONFIRM_CLICKED";

	private const string BannerIntroFinished = "BANNER_INTRO_FINISHED";

	private const string BannerOutroFinished = "BANNER_OUTRO_FINISHED";

	private const string ShowPopupEvent = "SHOW_POPUP";

	private const string DismissPopupEvent = "DISMISS_POPUP";

	private const string StateDeckBattle = "DECKBATTLE";

	private const string StateBattlegrounds = "BATTLEGROUNDS";

	private const string StateMercenaries = "MERCENARIES";

	private const string StateShow = "SHOW";

	private const string StateShowInForeground = "SHOW_IN_FOREGROUND";

	private const string StateHide = "HIDE";

	private const string StateOutro = "OUTRO";

	public const string GameModeTraditional = "traditional";

	public const string GameModeBattlegrounds = "battlegrounds";

	public const string GameModeMercenaries = "mercenaries";

	private bool m_isReopeningPortal;

	private bool m_isPortalAnimating;

	private bool m_isBannerAnimating;

	public bool IsPlayingPreview
	{
		get
		{
			if (m_videoPlayer != null)
			{
				return m_videoPlayer.isPlaying;
			}
			return false;
		}
	}

	public bool IsAnimating
	{
		get
		{
			if (!m_isPortalAnimating && !m_isBannerAnimating)
			{
				return m_isReopeningPortal;
			}
			return true;
		}
	}

	public static event Action PreviewOpened;

	public static event Action PreviewClosed;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_visualController = GetComponent<VisualController>();
		m_tutorialPreviewDataModel = new TutorialPreviewDataModel();
		m_widget.BindDataModel(m_tutorialPreviewDataModel);
		m_widget.RegisterEventListener(OnVideoPreviewEvent);
		m_tutorialDescriptionVOMap = new Dictionary<string, AudioSource>();
	}

	private void OnDestroy()
	{
		AssetHandle.SafeDispose(ref m_loadedVideo);
	}

	public void StartTraditionalTutorialPreviewVideo(Action OnPlayerConfirmedSelection)
	{
		if (m_currentState != TutorialPreviewWidgetState.TRADITIONAL_SELECTED)
		{
			m_onSelectionConfirmedCallback = OnPlayerConfirmedSelection;
			ShowTutorialVideo(TutorialPreviewWidgetState.TRADITIONAL_SELECTED);
		}
	}

	public void StartBattleGroundsTutorialPreviewVideo(Action OnPlayerConfirmedSelection)
	{
		if (m_currentState != TutorialPreviewWidgetState.BATTLEGROUNDS_SELECTED)
		{
			m_onSelectionConfirmedCallback = OnPlayerConfirmedSelection;
			ShowTutorialVideo(TutorialPreviewWidgetState.BATTLEGROUNDS_SELECTED);
		}
	}

	public void StartMercenariesTutorialPreviewVideo(Action OnPlayerConfirmedSelection)
	{
		if (m_currentState != TutorialPreviewWidgetState.MERCENARIES_SELECTED)
		{
			m_onSelectionConfirmedCallback = OnPlayerConfirmedSelection;
			ShowTutorialVideo(TutorialPreviewWidgetState.MERCENARIES_SELECTED);
		}
	}

	private void ResetTutorialPreview(bool isReopening = false)
	{
		StopVideo();
		m_visualController.SetState("HIDE");
		SoundManager.Get().Stop(m_lastPickedVO);
		m_isPortalAnimating = false;
		m_isBannerAnimating = false;
		if (!isReopening)
		{
			m_isReopeningPortal = false;
			m_currentState = TutorialPreviewWidgetState.NOTHING_SELECTED;
		}
	}

	private void ShowBrassRingBanner()
	{
		if (m_lastCoroutine != null)
		{
			StopCoroutine(m_lastCoroutine);
		}
		switch (m_currentState)
		{
		case TutorialPreviewWidgetState.TRADITIONAL_SELECTED:
			m_brassRingVisualController.SetState("DECKBATTLE");
			m_lastCoroutine = StartCoroutine(PlayTutorialPreviewVO(m_tutorialTraditionalDescriptionVO));
			m_isBannerAnimating = true;
			break;
		case TutorialPreviewWidgetState.BATTLEGROUNDS_SELECTED:
			m_brassRingVisualController.SetState("BATTLEGROUNDS");
			m_lastCoroutine = StartCoroutine(PlayTutorialPreviewVO(m_tutorialBattlegroundsDescriptionVO));
			m_isBannerAnimating = true;
			break;
		case TutorialPreviewWidgetState.MERCENARIES_SELECTED:
			m_brassRingVisualController.SetState("MERCENARIES");
			m_lastCoroutine = StartCoroutine(PlayTutorialPreviewVO(m_tutorialMercenariesDescriptionVO));
			m_isBannerAnimating = true;
			break;
		default:
			Debug.LogError("TutorialPreviewController:PlayTutorialPreviewVO: Unknown state " + m_currentState);
			break;
		}
	}

	private IEnumerator OpenPortal(TutorialPreviewWidgetState nextState)
	{
		string videoReference;
		switch (nextState)
		{
		default:
			yield break;
		case TutorialPreviewWidgetState.TRADITIONAL_SELECTED:
			m_tutorialPreviewDataModel.SelectedMode = "traditional";
			videoReference = m_traditionalVideo;
			break;
		case TutorialPreviewWidgetState.BATTLEGROUNDS_SELECTED:
			m_tutorialPreviewDataModel.SelectedMode = "battlegrounds";
			videoReference = m_battlegroundsVideo;
			break;
		case TutorialPreviewWidgetState.MERCENARIES_SELECTED:
			m_tutorialPreviewDataModel.SelectedMode = "mercenaries";
			videoReference = m_mercenariesVideo;
			break;
		}
		m_currentState = nextState;
		m_isPortalAnimating = true;
		TutorialPreviewController.PreviewOpened?.Invoke();
		PrepareVideo(videoReference);
		m_tutorialPreviewDataModel.IsNewPlayer = !GameUtils.IsAnyTutorialComplete();
		float timer = 0f;
		float videoTimeout = GameUtils.TutorialPreviewVideosTimeout();
		while (!m_videoPlayer.isPrepared && timer < videoTimeout)
		{
			timer += Time.unscaledDeltaTime;
			yield return null;
		}
		if (!m_videoPlayer.isPrepared)
		{
			StartCoroutine(StartSelectedTutorialOnPrepareFailure());
			yield break;
		}
		ShowBrassRingBanner();
		m_visualController.SetState(GameUtils.IsAnyTutorialComplete() ? "SHOW_IN_FOREGROUND" : "SHOW");
		m_portalSpell.AddStateFinishedCallback(OnSpellStateFinished);
		m_portalSpell.Activate();
		m_confirmButtonSpell.Activate();
		PlayVideo();
	}

	private IEnumerator ReOpenPortal(TutorialPreviewWidgetState nextState)
	{
		m_isReopeningPortal = true;
		ClosePortal();
		while (m_isPortalAnimating || m_isBannerAnimating)
		{
			yield return null;
		}
		yield return OpenPortal(nextState);
	}

	public void ClosePortal()
	{
		m_isPortalAnimating = true;
		m_portalSpell.ActivateState(SpellStateType.DEATH);
		m_isBannerAnimating = true;
		m_brassRingVisualController.SetState("OUTRO");
		m_confirmButtonSpell.ActivateState(SpellStateType.DEATH);
		SoundManager.Get().Stop(m_lastPickedVO);
	}

	private void PlayVideo()
	{
		if ((bool)m_videoPlayer.clip)
		{
			m_videoPlayer.Play();
		}
	}

	private void StopVideo()
	{
		if ((bool)m_videoPlayer.clip)
		{
			m_videoPlayer.Stop();
		}
	}

	private IEnumerator StartSelectedTutorial()
	{
		if (m_onSelectionConfirmedCallback == null)
		{
			Debug.LogError("Confirmed tutorial Selection with null Callback");
			yield break;
		}
		SendConfirmationToTelemetry();
		ClosePortal();
		while (IsAnimating)
		{
			yield return null;
		}
		ResetTutorialPreview();
		m_onSelectionConfirmedCallback();
	}

	private IEnumerator StartSelectedTutorialOnPrepareFailure()
	{
		Debug.LogWarning("VideoPlayer.Prepare() failed to prepare video skipping tutorial preview movie.");
		if (m_onSelectionConfirmedCallback == null)
		{
			Debug.LogError("Failed VideoPlayer.Prepare() with null Callback");
			yield break;
		}
		SendTimeoutToTelemetry();
		ResetTutorialPreview();
		m_onSelectionConfirmedCallback();
	}

	private IEnumerator PlayTutorialPreviewVO(string clipReference)
	{
		SoundManager.Get().Stop(m_lastPickedVO);
		yield return new WaitForSeconds(m_bannerAudioDelay);
		if (!m_tutorialDescriptionVOMap.ContainsKey(clipReference))
		{
			GameObject soundObject = SoundLoader.LoadSound(clipReference);
			if ((bool)soundObject)
			{
				AudioSource audioSource = soundObject.GetComponent<AudioSource>();
				if ((bool)audioSource)
				{
					m_tutorialDescriptionVOMap.Add(clipReference, audioSource);
					SoundManager.Get().Play(audioSource);
					m_lastPickedVO = audioSource;
				}
			}
		}
		else
		{
			AudioSource clip = m_tutorialDescriptionVOMap[clipReference];
			SoundManager.Get().Play(clip);
			m_lastPickedVO = clip;
		}
	}

	private void ShowPopup()
	{
		OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.CENTER, destroyOnSceneLoad: false, (!UniversalInputManager.UsePhoneUI) ? CanvasScaleMode.HEIGHT : CanvasScaleMode.WIDTH);
		UIContext.GetRoot().ShowPopup(base.gameObject);
	}

	private void DismissPopup()
	{
		UIContext.GetRoot().DismissPopup(base.gameObject);
	}

	private void OnVideoPreviewEvent(string eventName)
	{
		switch (eventName)
		{
		case "CONFIRM_CLICKED":
			if (!IsAnimating)
			{
				StartCoroutine(StartSelectedTutorial());
			}
			break;
		case "CANCEL_CLICKED":
			if (!IsAnimating)
			{
				ClosePortal();
			}
			break;
		case "BANNER_INTRO_FINISHED":
			m_isBannerAnimating = false;
			break;
		case "BANNER_OUTRO_FINISHED":
			m_isBannerAnimating = false;
			break;
		case "SHOW_POPUP":
			ShowPopup();
			break;
		case "DISMISS_POPUP":
			DismissPopup();
			break;
		}
	}

	private void OnSpellStateFinished(Spell spell, SpellStateType type, object data)
	{
		switch (type)
		{
		case SpellStateType.BIRTH:
			m_isPortalAnimating = false;
			m_isReopeningPortal = false;
			break;
		case SpellStateType.DEATH:
			m_portalSpell.Deactivate();
			ResetTutorialPreview(m_isReopeningPortal);
			TutorialPreviewController.PreviewClosed?.Invoke();
			break;
		}
	}

	private void SendConfirmationToTelemetry()
	{
		string modeSelected;
		switch (m_currentState)
		{
		default:
			return;
		case TutorialPreviewWidgetState.TRADITIONAL_SELECTED:
			modeSelected = "traditional";
			break;
		case TutorialPreviewWidgetState.BATTLEGROUNDS_SELECTED:
			modeSelected = "battlegrounds";
			break;
		case TutorialPreviewWidgetState.MERCENARIES_SELECTED:
			modeSelected = "mercenaries";
			break;
		}
		TelemetryManager.Client().SendFTUELetsGoButtonClicked(modeSelected);
	}

	private void SendTimeoutToTelemetry()
	{
		string modeSelected;
		switch (m_currentState)
		{
		default:
			return;
		case TutorialPreviewWidgetState.TRADITIONAL_SELECTED:
			modeSelected = "traditional";
			break;
		case TutorialPreviewWidgetState.BATTLEGROUNDS_SELECTED:
			modeSelected = "battlegrounds";
			break;
		case TutorialPreviewWidgetState.MERCENARIES_SELECTED:
			modeSelected = "mercenaries";
			break;
		}
		TelemetryManager.Client().SendFTUEVideoTimeout(modeSelected);
	}

	private void ShowTutorialVideo(TutorialPreviewWidgetState nextState)
	{
		if (m_visualController.State == "SHOW" || m_visualController.State == "SHOW_IN_FOREGROUND")
		{
			StartCoroutine(ReOpenPortal(nextState));
		}
		else
		{
			StartCoroutine(OpenPortal(nextState));
		}
	}

	private void PrepareVideo(string nextVideoRef)
	{
		m_root.SetActive(value: true);
		StopVideo();
		AssetLoader.Get().LoadAsset(ref m_loadedVideo, nextVideoRef);
		if (!m_loadedVideo)
		{
			Debug.LogError("Tutorial video failed to load.");
			return;
		}
		m_videoPlayer.clip = m_loadedVideo;
		m_videoPlayer.Prepare();
	}
}
