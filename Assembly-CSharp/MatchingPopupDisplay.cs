using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using HutongGames.PlayMaker;
using PegasusShared;
using UnityEngine;

public class MatchingPopupDisplay : TransitionPopup
{
	public UberText m_tipOfTheDay;

	public GameObject m_nameContainer;

	public GameObject m_wildVines;

	public GameObject m_classicPewter;

	public GameObject m_battlegroundsCrown;

	public GameObject m_battlegroundsDuosCrowns;

	private List<GameObject> m_spinnerTexts = new List<GameObject>();

	private SceneMgr.Mode m_gameMode;

	private const int NUM_SPINNER_ENTRIES = 10;

	protected override void Awake()
	{
		base.Awake();
		m_nameContainer.SetActive(value: false);
		m_title.gameObject.SetActive(value: false);
		m_tipOfTheDay.gameObject.SetActive(value: false);
		m_wildVines.SetActive(value: false);
		m_classicPewter.SetActive(value: false);
		m_battlegroundsCrown.SetActive(value: false);
		m_battlegroundsDuosCrowns.SetActive(value: false);
		SoundManager.Get().Load("FindOpponent_mechanism_start.prefab:effa04f444ca08840b677d98fc8abf39");
	}

	public override void Hide()
	{
		if (m_shown)
		{
			Navigation.RemoveHandler(OnNavigateBack);
			base.Hide();
		}
	}

	public override void Show()
	{
		SetupSpinnerText();
		UpdateTipOfTheDay();
		GenerateRandomSpinnerTexts(IsMultiOpponentGame());
		m_title.Text = GetTitleTextBasedOnScenario();
		base.Show();
	}

	protected override void OnGameConnecting(FindGameEventData eventData)
	{
		base.OnGameConnecting(eventData);
		IncreaseTooltipProgress();
	}

	protected override void OnGameEntered(FindGameEventData eventData)
	{
		EnableCancelButtonIfPossible();
	}

	protected override void OnGameDelayed(FindGameEventData eventData)
	{
		EnableCancelButtonIfPossible();
	}

	protected override void OnAnimateShowFinished()
	{
		base.OnAnimateShowFinished();
		EnableCancelButtonIfPossible();
	}

	private void SetupSpinnerText()
	{
		for (int index = 1; index <= 10; index++)
		{
			GameObject go = GameObjectUtils.FindChild(base.gameObject, "NAME_" + index).gameObject;
			m_spinnerTexts.Add(go);
		}
	}

	private void GenerateRandomSpinnerTexts(bool isPlural)
	{
		string prefix = (isPlural ? "GLUE_SPINNER_PLURAL_" : "GLUE_SPINNER_");
		int index = 1;
		List<string> names = new List<string>();
		while (true)
		{
			string randomName = GameStrings.Get(prefix + index);
			if (randomName == prefix + index)
			{
				break;
			}
			names.Add(randomName);
			index++;
		}
		GameObjectUtils.FindChild(base.gameObject, "NAME_PerfectOpponent").gameObject.GetComponent<UberText>().Text = GetWorthyOpponentTextBasedOnScenario();
		for (index = 0; index < 10; index++)
		{
			int randomIndex = Mathf.FloorToInt(Random.value * (float)names.Count);
			m_spinnerTexts[index].GetComponent<UberText>().Text = names[randomIndex];
			names.RemoveAt(randomIndex);
		}
	}

	private IEnumerator StopSpinnerDelay()
	{
		yield return new WaitForSeconds(3.5f);
		Hide();
	}

	private bool OnNavigateBack()
	{
		if (!m_cancelButton.gameObject.activeSelf)
		{
			return false;
		}
		GetComponent<PlayMakerFSM>().SendEvent("Cancel");
		FireMatchCanceledEvent();
		if (FriendChallengeMgr.Get() != null)
		{
			FriendChallengeMgr.Get().CancelChallenge();
		}
		if (PartyManager.Get().IsInParty() && PartyManager.Get().IsPartyLeader())
		{
			PartyManager.Get().CancelQueue();
		}
		return true;
	}

	protected override void OnCancelButtonReleased(UIEvent e)
	{
		base.OnCancelButtonReleased(e);
		if (PartyManager.Get().IsInParty() && !PartyManager.Get().IsPartyLeader())
		{
			PartyManager.Get().CancelQueue();
		}
		else
		{
			Navigation.GoBack();
		}
	}

	private void UpdateTipOfTheDay()
	{
		m_gameMode = SceneMgr.Get().GetMode();
		if (m_gameMode == SceneMgr.Mode.TOURNAMENT)
		{
			m_tipOfTheDay.Text = GameStrings.GetTip(TipCategory.PLAY, Options.Get().GetInt(Option.TIP_PLAY_PROGRESS, 0));
		}
		else if (m_gameMode == SceneMgr.Mode.DRAFT)
		{
			m_tipOfTheDay.Text = GameStrings.GetTip(TipCategory.FORGE, Options.Get().GetInt(Option.TIP_FORGE_PROGRESS, 0));
		}
		else if (m_gameMode == SceneMgr.Mode.BACON)
		{
			switch (m_gameType)
			{
			case GameType.GT_BATTLEGROUNDS:
				m_tipOfTheDay.Text = GameStrings.GetRandomTip(new List<TipCategory>
				{
					TipCategory.BACON,
					TipCategory.BACON_SOLO
				});
				break;
			case GameType.GT_BATTLEGROUNDS_DUO:
				m_tipOfTheDay.Text = GameStrings.GetRandomTip(new List<TipCategory>
				{
					TipCategory.BACON,
					TipCategory.BACON_DUO
				});
				break;
			}
		}
		else if (SceneMgr.Get().IsInLettuceMode())
		{
			m_tipOfTheDay.Text = GameStrings.GetRandomTip(TipCategory.LETTUCE);
		}
		else if (m_gameMode == SceneMgr.Mode.TAVERN_BRAWL)
		{
			if (TavernBrawlManager.Get().IsCurrentSeasonSessionBased)
			{
				m_tipOfTheDay.Text = GameStrings.GetRandomTip(TipCategory.HEROICBRAWL);
			}
			else
			{
				m_tipOfTheDay.Text = GameStrings.GetRandomTip(TipCategory.TAVERNBRAWL);
			}
		}
		else
		{
			m_tipOfTheDay.Text = GameStrings.GetRandomTip(TipCategory.DEFAULT);
		}
	}

	private void IncreaseTooltipProgress()
	{
		if (m_gameMode == SceneMgr.Mode.TOURNAMENT)
		{
			Options.Get().SetInt(Option.TIP_PLAY_PROGRESS, Options.Get().GetInt(Option.TIP_PLAY_PROGRESS, 0) + 1);
		}
		else if (m_gameMode == SceneMgr.Mode.DRAFT)
		{
			Options.Get().SetInt(Option.TIP_FORGE_PROGRESS, Options.Get().GetInt(Option.TIP_FORGE_PROGRESS, 0) + 1);
		}
	}

	protected override void ShowPopup()
	{
		SoundManager.Get().LoadAndPlay("FindOpponent_mechanism_start.prefab:effa04f444ca08840b677d98fc8abf39");
		base.ShowPopup();
		PlayMakerFSM component = GetComponent<PlayMakerFSM>();
		FsmBool playSpinMusic = component.FsmVariables.FindFsmBool("PlaySpinningMusic");
		if (playSpinMusic != null)
		{
			playSpinMusic.Value = m_gameMode != SceneMgr.Mode.TAVERN_BRAWL;
		}
		component.SendEvent("Birth");
		RenderUtils.EnableRenderers(m_nameContainer, enable: false);
		m_title.gameObject.SetActive(value: true);
		m_tipOfTheDay.gameObject.SetActive(value: true);
		bool showVines = false;
		bool showPewter = false;
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.TOURNAMENT)
		{
			switch (m_gameType)
			{
			case GameType.GT_RANKED:
				showVines = m_formatType == FormatType.FT_WILD;
				showPewter = m_formatType == FormatType.FT_TWIST;
				break;
			case GameType.GT_CASUAL:
			{
				if (!m_deckId.HasValue)
				{
					break;
				}
				CollectionManager collectionManager = CollectionManager.Get();
				if (collectionManager != null)
				{
					CollectionDeck deck = collectionManager.GetDeck(m_deckId.Value);
					if (deck != null)
					{
						showVines = deck.FormatType == FormatType.FT_WILD;
						showPewter = deck.FormatType == FormatType.FT_TWIST;
					}
				}
				break;
			}
			}
		}
		else
		{
			switch (m_gameType)
			{
			case GameType.GT_BATTLEGROUNDS:
				m_battlegroundsCrown.SetActive(value: true);
				m_battlegroundsDuosCrowns.SetActive(value: false);
				break;
			case GameType.GT_BATTLEGROUNDS_DUO:
				m_battlegroundsCrown.SetActive(value: false);
				m_battlegroundsDuosCrowns.SetActive(value: true);
				break;
			}
		}
		m_wildVines.SetActive(showVines);
		m_classicPewter.SetActive(showPewter);
		Navigation.Push(OnNavigateBack);
	}

	protected override void OnGameplaySceneLoaded()
	{
		m_nameContainer.SetActive(value: true);
		GetComponent<PlayMakerFSM>().SendEvent("Death");
		StartCoroutine(StopSpinnerDelay());
		Navigation.Clear();
	}

	private string GetTitleTextBasedOnScenario()
	{
		if (!IsMultiOpponentGame())
		{
			return GameStrings.Get("GLUE_MATCHMAKER_FINDING_OPPONENT");
		}
		return GameStrings.Get("GLUE_MATCHMAKER_FINDING_OPPONENTS");
	}

	private string GetWorthyOpponentTextBasedOnScenario()
	{
		if (!IsMultiOpponentGame())
		{
			return GameStrings.Get("GLUE_MATCHMAKER_PERFECT_OPPONENT");
		}
		return GameStrings.Get("GLUE_MATCHMAKER_PERFECT_OPPONENTS");
	}

	private bool IsMultiOpponentGame()
	{
		ScenarioDbfRecord record = GameDbf.Scenario.GetRecord(m_scenarioId);
		if (record == null)
		{
			return false;
		}
		return record.Players > 2;
	}
}
