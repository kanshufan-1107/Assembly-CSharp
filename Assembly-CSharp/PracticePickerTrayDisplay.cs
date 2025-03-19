using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using PegasusShared;
using UnityEngine;

[CustomEditClass]
public class PracticePickerTrayDisplay : MonoBehaviour
{
	public delegate void TrayLoaded();

	[CustomEditField(Sections = "UI")]
	public UberText m_trayLabel;

	[CustomEditField(Sections = "UI")]
	public StandardPegButtonNew m_backButton;

	[CustomEditField(Sections = "UI")]
	public PlayButton m_playButton;

	[CustomEditField(Sections = "AI Button Settings")]
	public PracticeAIButton m_AIButtonPrefab;

	[CustomEditField(Sections = "AI Button Settings")]
	public GameObject m_AIButtonsContainer;

	[SerializeField]
	private float m_AIButtonHeight = 5f;

	[CustomEditField(Sections = "Animation Settings")]
	public float m_trayAnimationTime = 0.5f;

	[CustomEditField(Sections = "Animation Settings")]
	public iTween.EaseType m_trayInEaseType = iTween.EaseType.easeOutBounce;

	[CustomEditField(Sections = "Animation Settings")]
	public iTween.EaseType m_trayOutEaseType = iTween.EaseType.easeOutCubic;

	private static PracticePickerTrayDisplay s_instance;

	private List<ScenarioDbfRecord> m_sortedMissionRecords = new List<ScenarioDbfRecord>();

	private List<PracticeAIButton> m_practiceAIButtons = new List<PracticeAIButton>();

	private List<Achievement> m_lockedHeroes = new List<Achievement>();

	private PracticeAIButton m_selectedPracticeAIButton;

	private Map<string, DefLoader.DisposableFullDef> m_heroDefs = new Map<string, DefLoader.DisposableFullDef>();

	private int m_heroDefsToLoad;

	private List<TrayLoaded> m_TrayLoadedListeners = new List<TrayLoaded>();

	private bool m_buttonsCreated;

	private bool m_buttonsReady;

	private bool m_heroesLoaded;

	private bool m_shown;

	private const float PRACTICE_TRAY_MATERIAL_Y_OFFSET = -0.045f;

	[CustomEditField(Sections = "AI Button Settings")]
	public float AIButtonHeight
	{
		get
		{
			return m_AIButtonHeight;
		}
		set
		{
			m_AIButtonHeight = value;
			UpdateAIButtonPositions();
		}
	}

	private void Awake()
	{
		s_instance = this;
		InitMissionRecords();
		Transform[] components = base.gameObject.GetComponents<Transform>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].gameObject.SetActive(value: false);
		}
		base.gameObject.SetActive(value: true);
		if (m_backButton != null)
		{
			m_backButton.SetText(GameStrings.Get("GLOBAL_BACK"));
			m_backButton.AddEventListener(UIEventType.RELEASE, BackButtonReleased);
		}
		m_trayLabel.Text = GameStrings.Get("GLUE_CHOOSE_OPPONENT");
		m_playButton.AddEventListener(UIEventType.RELEASE, PlayGameButtonRelease);
		m_heroDefsToLoad = m_sortedMissionRecords.Count;
		foreach (ScenarioDbfRecord sortedMissionRecord in m_sortedMissionRecords)
		{
			string cardId = GameUtils.GetMissionHeroCardId(sortedMissionRecord.ID);
			DefLoader.Get().LoadFullDef(cardId, OnFullDefLoaded);
		}
		SoundManager.Get().Load("choose_opponent_panel_slide_on.prefab:66491d3d01ed663429ab80daf6a5e880");
		SoundManager.Get().Load("choose_opponent_panel_slide_off.prefab:3139d09eb94899d41b9bf612649f47bf");
		InitButtons();
		StartCoroutine(NotifyWhenTrayLoaded());
		GameMgr.Get().RegisterFindGameEvent(OnFindGameEvent);
	}

	private void OnDestroy()
	{
		if (GameMgr.Get() != null)
		{
			GameMgr.Get().UnregisterFindGameEvent(OnFindGameEvent);
		}
		m_heroDefs.DisposeValuesAndClear();
		s_instance = null;
	}

	private void Start()
	{
		m_playButton.SetText(GameStrings.Get("GLOBAL_PLAY"));
		m_playButton.SetOriginalLocalPosition();
		m_playButton.Disable();
	}

	public static PracticePickerTrayDisplay Get()
	{
		return s_instance;
	}

	public void Init()
	{
		int numButtons = m_sortedMissionRecords.Count;
		for (int i = 0; i < numButtons; i++)
		{
			PracticeAIButton button = (PracticeAIButton)GameUtils.Instantiate(m_AIButtonPrefab, m_AIButtonsContainer);
			LayerUtils.SetLayer(button, m_AIButtonsContainer.gameObject.layer);
			m_practiceAIButtons.Add(button);
		}
		UpdateAIButtonPositions();
		foreach (PracticeAIButton practiceAIButton in m_practiceAIButtons)
		{
			practiceAIButton.SetOriginalLocalPosition();
			practiceAIButton.AddEventListener(UIEventType.RELEASE, AIButtonPressed);
		}
		m_buttonsCreated = true;
		LoanerDeckDisplay loanerDeckDisplay = LoanerDeckDisplay.Get();
		if (loanerDeckDisplay != null)
		{
			loanerDeckDisplay.LoanerDeckInfoDataModel.CurrentSceneMode = "PRACTICE";
		}
	}

	public void Show()
	{
		m_shown = true;
		iTween.Stop(base.gameObject);
		Transform[] components = base.gameObject.GetComponents<Transform>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].gameObject.SetActive(value: true);
		}
		base.gameObject.SetActive(value: true);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("position", PracticeDisplay.Get().GetPracticePickerShowPosition());
		args.Add("islocal", true);
		args.Add("time", m_trayAnimationTime);
		args.Add("easetype", m_trayInEaseType);
		args.Add("delay", 0.001f);
		iTween.MoveTo(base.gameObject, args);
		SoundManager.Get().LoadAndPlay("choose_opponent_panel_slide_on.prefab:66491d3d01ed663429ab80daf6a5e880");
		if (m_selectedPracticeAIButton != null)
		{
			m_playButton.Enable();
		}
		Navigation.Push(OnNavigateBack);
	}

	public void Hide()
	{
		m_shown = false;
		iTween.Stop(base.gameObject);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("position", PracticeDisplay.Get().GetPracticePickerHidePosition());
		args.Add("islocal", true);
		args.Add("time", m_trayAnimationTime);
		args.Add("easetype", m_trayOutEaseType);
		args.Add("oncomplete", (Action<object>)delegate
		{
			base.gameObject.SetActive(value: false);
		});
		args.Add("delay", 0.001f);
		iTween.MoveTo(base.gameObject, args);
		SoundManager.Get().LoadAndPlay("choose_opponent_panel_slide_off.prefab:3139d09eb94899d41b9bf612649f47bf");
	}

	public void OnGameDenied()
	{
		UpdateAIButtons();
	}

	public bool IsShown()
	{
		return m_shown;
	}

	public void AddTrayLoadedListener(TrayLoaded dlg)
	{
		m_TrayLoadedListeners.Add(dlg);
	}

	public void RemoveTrayLoadedListener(TrayLoaded dlg)
	{
		m_TrayLoadedListeners.Remove(dlg);
	}

	public bool IsLoaded()
	{
		return m_buttonsReady;
	}

	private void InitMissionRecords()
	{
		int practiceDbId = 2;
		AdventureModeDbId modeId = AdventureConfig.Get().GetSelectedMode();
		int modeDbId = (int)modeId;
		m_sortedMissionRecords = GameDbf.Scenario.GetRecords((ScenarioDbfRecord r) => r.AdventureId == practiceDbId && r.ModeId == modeDbId);
		m_sortedMissionRecords.Sort(GameUtils.MissionSortComparison);
	}

	private void InitButtons()
	{
		StartCoroutine(InitButtonsWhenReady());
	}

	private IEnumerator InitButtonsWhenReady()
	{
		while (!m_buttonsCreated)
		{
			yield return null;
		}
		while (!m_heroesLoaded)
		{
			yield return null;
		}
		UpdateAIButtons();
		m_buttonsReady = true;
	}

	private void OnFullDefLoaded(string cardId, DefLoader.DisposableFullDef def, object userData)
	{
		m_heroDefs.SetOrReplaceDisposable(cardId, def);
		m_heroDefsToLoad--;
		if (m_heroDefsToLoad <= 0)
		{
			m_heroesLoaded = true;
		}
	}

	private void SetSelectedButton(PracticeAIButton button)
	{
		if (m_selectedPracticeAIButton != null && m_selectedPracticeAIButton != button)
		{
			m_selectedPracticeAIButton.Deselect();
		}
		m_selectedPracticeAIButton = button;
	}

	private void DisableAIButtons()
	{
		for (int i = 0; i < m_practiceAIButtons.Count; i++)
		{
			m_practiceAIButtons[i].SetEnabled(enabled: false);
		}
	}

	private void EnableAIButtons()
	{
		for (int i = 0; i < m_practiceAIButtons.Count; i++)
		{
			m_practiceAIButtons[i].SetEnabled(enabled: true);
		}
	}

	private bool OnNavigateBack()
	{
		Hide();
		if (DeckPickerTray.GetTray() != null)
		{
			DeckPickerTray.GetTray().ResetCurrentMode();
		}
		return true;
	}

	private void BackButtonReleased(UIEvent e)
	{
		Navigation.GoBack();
	}

	private void PlayGameButtonRelease(UIEvent e)
	{
		CollectionDeck deck = DeckPickerTrayDisplay.Get().GetSelectedCollectionDeck();
		if (deck == null)
		{
			Debug.LogError("Trying to play practice game with deck null deck!");
			return;
		}
		e.GetElement().SetEnabled(enabled: false);
		DisableAIButtons();
		if (AdventureConfig.Get().GetSelectedMode() == AdventureModeDbId.EXPERT && !Options.Get().GetBool(Option.HAS_PLAYED_EXPERT_AI, defaultVal: false))
		{
			Options.Get().SetBool(Option.HAS_PLAYED_EXPERT_AI, val: true);
		}
		if (deck.IsLoanerDeck && FreeDeckMgr.Get().Status == FreeDeckMgr.FreeDeckStatus.TRIAL_PERIOD)
		{
			int templateId = deck.DeckTemplateId;
			if (templateId <= 0)
			{
				Debug.LogError("Trying to play practice game with deck template ID 0!");
				return;
			}
			GameMgr gameMgr = GameMgr.Get();
			int missionID = m_selectedPracticeAIButton.GetMissionID();
			long deckId = 0L;
			int deckTemplateId = templateId;
			gameMgr.FindGame(GameType.GT_VS_AI, FormatType.FT_WILD, missionID, 0, deckId, null, null, restoreSavedGameState: false, null, null, 0L, GameType.GT_UNKNOWN, deckTemplateId);
		}
		else
		{
			long myDeckID = DeckPickerTrayDisplay.Get().GetSelectedDeckID();
			if (myDeckID <= 0)
			{
				Debug.LogError("Trying to play practice game with deck ID 0!");
				return;
			}
			GameMgr.Get().FindGame(GameType.GT_VS_AI, FormatType.FT_WILD, m_selectedPracticeAIButton.GetMissionID(), 0, myDeckID, null, null, restoreSavedGameState: false, null, null, 0L);
		}
		Navigation.RemoveHandler(OnNavigateBack);
		PracticeDisplay.Get().DismissTrays();
	}

	private void AIButtonPressed(UIEvent e)
	{
		PracticeAIButton button = (PracticeAIButton)e.GetElement();
		if (!(m_selectedPracticeAIButton == button))
		{
			m_playButton.Enable();
			SetSelectedButton(button);
			button.Select();
		}
	}

	private void UpdateAIButtons()
	{
		UpdateAIDeckButtons();
		if (m_selectedPracticeAIButton == null)
		{
			m_playButton.Disable();
		}
		else
		{
			m_playButton.Enable();
		}
	}

	private void UpdateAIButtonPositions()
	{
		int i = 0;
		foreach (PracticeAIButton practiceAIButton in m_practiceAIButtons)
		{
			TransformUtil.SetLocalPosZ(practiceAIButton, (0f - m_AIButtonHeight) * (float)i++);
		}
	}

	private void UpdateAIDeckButtons()
	{
		for (int i = 0; i < m_sortedMissionRecords.Count; i++)
		{
			ScenarioDbfRecord scenarioDbfRecord = m_sortedMissionRecords[i];
			int missionDbId = scenarioDbfRecord.ID;
			string cardId = GameUtils.GetMissionHeroCardId(missionDbId);
			DefLoader.DisposableFullDef def = m_heroDefs[cardId];
			TAG_CLASS cardClass = def.EntityDef.GetClass();
			string missionName = scenarioDbfRecord.ShortName;
			PracticeAIButton button = m_practiceAIButtons[i];
			button.SetInfo(missionName, cardClass, def.DisposableCardDef, missionDbId, flip: false);
			bool shouldShowQuestBang = false;
			foreach (Achievement lockedHero in m_lockedHeroes)
			{
				if (lockedHero.ClassReward.Value == cardClass)
				{
					shouldShowQuestBang = true;
					break;
				}
			}
			button.ShowQuestBang(shouldShowQuestBang);
			if (button == m_selectedPracticeAIButton)
			{
				button.Select();
			}
			else
			{
				button.Deselect();
			}
		}
		bool num = AdventureConfig.Get().GetSelectedMode() == AdventureModeDbId.EXPERT;
		bool hasSeenExpertAI = Options.Get().GetBool(Option.HAS_SEEN_EXPERT_AI, defaultVal: false);
		if (num && !hasSeenExpertAI)
		{
			Options.Get().SetBool(Option.HAS_SEEN_EXPERT_AI, val: true);
			hasSeenExpertAI = true;
		}
	}

	private IEnumerator NotifyWhenTrayLoaded()
	{
		while (!m_buttonsReady)
		{
			yield return null;
		}
		FireTrayLoadedEvent();
	}

	private void FireTrayLoadedEvent()
	{
		TrayLoaded[] array = m_TrayLoadedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i]();
		}
	}

	private bool OnFindGameEvent(FindGameEventData eventData, object userData)
	{
		if (eventData.m_state == FindGameState.INVALID)
		{
			EnableAIButtons();
		}
		return false;
	}
}
