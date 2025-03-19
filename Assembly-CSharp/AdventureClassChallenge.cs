using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Core;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using PegasusShared;
using UnityEngine;

[CustomEditClass]
public class AdventureClassChallenge : MonoBehaviour
{
	private class ClassChallengeData
	{
		public ScenarioDbfRecord scenarioRecord;

		public bool unlocked;

		public bool defeated;

		public string heroID0;

		public string heroID1;

		public string name;

		public string title;

		public string description;

		public string completedDescription;

		public string opponentName;
	}

	private class HeroLoadData
	{
		public int heroNum;

		public string heroID;

		public DefLoader.DisposableFullDef fulldef;
	}

	private readonly float[] EMPTY_SLOT_UV_OFFSET = new float[6] { 0f, 0.223f, 0.377f, 0.535f, 0.69f, 0.85f };

	private const float CHALLENGE_BUTTON_OFFSET = 4.3f;

	private const int VISIBLE_SLOT_COUNT = 10;

	[CustomEditField(Sections = "DBF Stuff")]
	public UberText m_ModeName;

	[CustomEditField(Sections = "Class Challenge Buttons")]
	public GameObject m_ClassChallengeButtonPrefab;

	[CustomEditField(Sections = "Class Challenge Buttons")]
	public Vector3 m_ClassChallengeButtonSpacing;

	[CustomEditField(Sections = "Class Challenge Buttons")]
	public GameObject m_ChallengeButtonContainer;

	[CustomEditField(Sections = "Class Challenge Buttons")]
	public GameObject m_EmptyChallengeButtonSlot;

	[CustomEditField(Sections = "Class Challenge Buttons")]
	public float m_ChallengeButtonHeight;

	[CustomEditField(Sections = "Class Challenge Buttons")]
	public UIBScrollable m_ChallengeButtonScroller;

	[CustomEditField(Sections = "Hero Portraits")]
	public GameObject m_LeftHeroContainer;

	[CustomEditField(Sections = "Hero Portraits")]
	public GameObject m_RightHeroContainer;

	[CustomEditField(Sections = "Hero Portraits")]
	public UberText m_LeftHeroName;

	[CustomEditField(Sections = "Hero Portraits")]
	public UberText m_RightHeroName;

	[CustomEditField(Sections = "Versus Text", T = EditType.GAME_OBJECT)]
	public string m_VersusTextPrefab;

	[CustomEditField(Sections = "Versus Text")]
	public GameObject m_VersusTextContainer;

	[CustomEditField(Sections = "Versus Text")]
	public Color m_VersusTextColor;

	[CustomEditField(Sections = "Text")]
	public UberText m_ChallengeTitle;

	[CustomEditField(Sections = "Text")]
	public UberText m_ChallengeDescription;

	[CustomEditField(Sections = "Basic UI")]
	public PlayButton m_PlayButton;

	[CustomEditField(Sections = "Basic UI")]
	public UIBButton m_BackButton;

	[CustomEditField(Sections = "Reward UI")]
	public AdventureClassChallengeChestButton m_ChestButton;

	[CustomEditField(Sections = "Reward UI")]
	public GameObject m_ChestButtonCover;

	[CustomEditField(Sections = "Reward UI")]
	public Transform m_RewardBone;

	private List<ClassChallengeData> m_ClassChallenges = new List<ClassChallengeData>();

	private Map<int, int> m_ScenarioChallengeLookup = new Map<int, int>();

	private int m_UVoffset;

	private AdventureClassChallengeButton m_SelectedButton;

	private GameObject m_LeftHero;

	private GameObject m_RightHero;

	private int m_SelectedScenario;

	private bool m_gameDenied;

	private void Awake()
	{
		base.transform.position = new Vector3(-500f, 0f, 0f);
		m_BackButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			BackButton();
		});
		m_PlayButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			Play();
		});
		m_EmptyChallengeButtonSlot.SetActive(value: false);
		AssetLoader.Get().InstantiatePrefab(m_VersusTextPrefab, OnVersusLettersLoaded);
	}

	private void Start()
	{
		GameMgr.Get().RegisterFindGameEvent(OnFindGameEvent);
		InitModeName();
		InitAdventureChallenges();
		Box.Get().AddTransitionFinishedListener(OnBoxTransitionFinished);
		Navigation.PushUnique(OnNavigateBack);
		StartCoroutine(CreateChallengeButtons());
	}

	private void OnDestroy()
	{
		GameMgr.Get().UnregisterFindGameEvent(OnFindGameEvent);
		if (Box.Get() != null)
		{
			Box.Get().RemoveTransitionFinishedListener(OnBoxTransitionFinished);
		}
	}

	private void InitModeName()
	{
		AdventureDbId selectedAdventure = AdventureConfig.Get().GetSelectedAdventure();
		int modeDbId = (int)AdventureConfig.Get().GetSelectedMode();
		AdventureDataDbfRecord dataRecord = GameUtils.GetAdventureDataRecord((int)selectedAdventure, modeDbId);
		string name = (UniversalInputManager.UsePhoneUI ? dataRecord.ShortName : dataRecord.Name);
		m_ModeName.Text = name;
	}

	private void InitAdventureChallenges()
	{
		List<ScenarioDbfRecord> records = GameDbf.Scenario.GetRecords();
		records.Sort(delegate(ScenarioDbfRecord a, ScenarioDbfRecord b)
		{
			int sortOrder = a.SortOrder;
			int sortOrder2 = b.SortOrder;
			return sortOrder - sortOrder2;
		});
		foreach (ScenarioDbfRecord scenario in records)
		{
			if (scenario.AdventureId == (int)AdventureConfig.Get().GetSelectedAdventure() && scenario.ModeId == 4)
			{
				int id1 = scenario.Player1HeroCardId;
				int id2 = scenario.ClientPlayer2HeroCardId;
				if (id2 == 0)
				{
					id2 = scenario.Player2HeroCardId;
				}
				ClassChallengeData data = new ClassChallengeData();
				data.scenarioRecord = scenario;
				data.heroID0 = GameUtils.TranslateDbIdToCardId(id1);
				data.heroID1 = GameUtils.TranslateDbIdToCardId(id2);
				data.unlocked = AdventureProgressMgr.Get().CanPlayScenario(scenario.ID);
				if (AdventureProgressMgr.Get().HasDefeatedScenario(scenario.ID))
				{
					data.defeated = true;
				}
				else
				{
					data.defeated = false;
				}
				data.name = scenario.ShortName;
				data.title = scenario.Name;
				data.description = (((bool)UniversalInputManager.UsePhoneUI && !string.IsNullOrEmpty(scenario.ShortDescription)) ? scenario.ShortDescription : scenario.Description);
				data.completedDescription = scenario.CompletedDescription;
				data.opponentName = scenario.OpponentName;
				m_ScenarioChallengeLookup.Add(scenario.ID, m_ClassChallenges.Count);
				m_ClassChallenges.Add(data);
			}
		}
	}

	private int BossCreateParamsSortComparison(ClassChallengeData data1, ClassChallengeData data2)
	{
		return GameUtils.MissionSortComparison(data1.scenarioRecord, data2.scenarioRecord);
	}

	private IEnumerator CreateChallengeButtons()
	{
		int buttonCount = 0;
		int lastSelectedScenario = (int)AdventureConfig.Get().GetLastSelectedMission();
		for (int i = 0; i < m_ClassChallenges.Count; i++)
		{
			ClassChallengeData cdata = m_ClassChallenges[i];
			if (cdata.unlocked)
			{
				GameObject obj = (GameObject)GameUtils.Instantiate(m_ClassChallengeButtonPrefab, m_ChallengeButtonContainer);
				obj.transform.localPosition = m_ClassChallengeButtonSpacing * buttonCount;
				AdventureClassChallengeButton challengeButton = obj.GetComponent<AdventureClassChallengeButton>();
				challengeButton.m_Text.Text = cdata.name;
				challengeButton.m_ScenarioID = cdata.scenarioRecord.ID;
				bool hasReward = AdventureProgressMgr.Get().ScenarioHasRewardData(challengeButton.m_ScenarioID);
				challengeButton.m_Chest.SetActive(!cdata.defeated && hasReward);
				challengeButton.m_Checkmark.SetActive(cdata.defeated);
				challengeButton.AddEventListener(UIEventType.RELEASE, ButtonPressed);
				LoadButtonPortrait(challengeButton, cdata.heroID1);
				if (lastSelectedScenario == challengeButton.m_ScenarioID || !m_SelectedButton)
				{
					m_SelectedButton = challengeButton;
				}
				buttonCount++;
			}
		}
		int emptySlotCount = 10 - buttonCount;
		if (emptySlotCount <= 0)
		{
			Debug.LogError($"Adventure Class Challenge tray UI doesn't support scrolling yet. More than {10} buttons where added.");
			yield break;
		}
		for (int s = 0; s < emptySlotCount; s++)
		{
			GameObject obj2 = (GameObject)GameUtils.Instantiate(m_EmptyChallengeButtonSlot, m_ChallengeButtonContainer);
			obj2.transform.localPosition = m_ClassChallengeButtonSpacing * (buttonCount + s);
			obj2.transform.localRotation = Quaternion.identity;
			obj2.SetActive(value: true);
			obj2.GetComponentInChildren<Renderer>().GetMaterial().mainTextureOffset = new UnityEngine.Vector2(0f, EMPTY_SLOT_UV_OFFSET[m_UVoffset]);
			m_UVoffset++;
			if (m_UVoffset > 5)
			{
				m_UVoffset = 0;
			}
		}
		yield return null;
		if (m_SelectedButton == null)
		{
			Debug.LogError("AdventureClassChallenge.m_SelectedButton is null!\nThis it's likely that this means there are no valid class challenges available but we still tried to load the screen.");
			Navigation.RemoveHandler(OnNavigateBack);
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
			yield break;
		}
		SetSelectedButton(m_SelectedButton);
		m_SelectedButton.Select(playSound: false);
		GetRewardCardForSelectedScenario();
		m_PlayButton.Enable();
		if (m_ChallengeButtonScroller != null)
		{
			m_ChallengeButtonScroller.SetScrollHeightCallback(() => m_ChallengeButtonHeight * (float)m_ClassChallenges.Count);
		}
		GetComponent<AdventureSubScene>().SetIsLoaded(loaded: true);
	}

	private void ButtonPressed(UIEvent e)
	{
		if (!(m_ChallengeButtonScroller != null) || !m_ChallengeButtonScroller.IsTouchDragging())
		{
			AdventureClassChallengeButton button = (AdventureClassChallengeButton)e.GetElement();
			m_SelectedButton.Deselect();
			SetSelectedButton(button);
			button.Select(playSound: true);
			m_SelectedScenario = button.m_ScenarioID;
			m_SelectedButton = button;
			GetRewardCardForSelectedScenario();
		}
	}

	private void SetSelectedButton(AdventureClassChallengeButton button)
	{
		int scenarioID = button.m_ScenarioID;
		AdventureConfig.Get().SetMission((ScenarioDbId)scenarioID);
		SetScenario(scenarioID);
	}

	private void LoadButtonPortrait(AdventureClassChallengeButton button, string heroID)
	{
		DefLoader.Get().LoadCardDef(heroID, OnButtonCardDefLoaded, button);
	}

	private void OnButtonCardDefLoaded(string cardId, DefLoader.DisposableCardDef disposableCardDef, object userData)
	{
		AdventureClassChallengeButton button = (AdventureClassChallengeButton)userData;
		ServiceManager.Get<DisposablesCleaner>()?.Attach(button.gameObject, disposableCardDef);
		Material portraitMat = disposableCardDef.CardDef.GetPracticeAIPortrait();
		if (portraitMat != null)
		{
			portraitMat.mainTexture = disposableCardDef.CardDef.GetPortraitTexture(TAG_PREMIUM.NORMAL);
			button.SetPortraitMaterial(portraitMat);
		}
	}

	private void SetScenario(int scenarioID)
	{
		m_SelectedScenario = scenarioID;
		ClassChallengeData challengeData = m_ClassChallenges[m_ScenarioChallengeLookup[scenarioID]];
		LoadHero(0, challengeData.heroID0);
		LoadHero(1, challengeData.heroID1);
		m_RightHeroName.Text = challengeData.opponentName;
		m_ChallengeTitle.Text = challengeData.title;
		if (challengeData.defeated)
		{
			m_ChallengeDescription.Text = challengeData.completedDescription;
		}
		else
		{
			m_ChallengeDescription.Text = challengeData.description;
		}
		if (!UniversalInputManager.UsePhoneUI)
		{
			bool hasReward = AdventureProgressMgr.Get().ScenarioHasRewardData(scenarioID);
			if (m_ClassChallenges[m_ScenarioChallengeLookup[scenarioID]].defeated || !hasReward)
			{
				m_ChestButton.gameObject.SetActive(value: false);
				m_ChestButtonCover.SetActive(value: true);
			}
			else
			{
				m_ChestButton.gameObject.SetActive(value: true);
				m_ChestButtonCover.SetActive(value: false);
			}
		}
	}

	private void LoadHero(int heroNum, string heroID)
	{
		HeroLoadData data = new HeroLoadData();
		data.heroNum = heroNum;
		data.heroID = heroID;
		DefLoader.Get().LoadFullDef(heroID, OnHeroFullDefLoaded, data);
	}

	private void OnHeroFullDefLoaded(string cardId, DefLoader.DisposableFullDef fullDef, object userData)
	{
		if (fullDef == null)
		{
			Debug.LogWarning($"AdventureClassChallenge.OnHeroFullDefLoaded() - FAILED to load \"{cardId}\"");
			return;
		}
		HeroLoadData data = (HeroLoadData)userData;
		data.fulldef = fullDef;
		AssetLoader.Get().InstantiatePrefab("Card_Play_Hero.prefab:42cbbd2c4969afb46b3887bb628de19d", OnActorLoaded, data, AssetLoadingOptions.IgnorePrefabPosition);
	}

	private void OnActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		HeroLoadData data = (HeroLoadData)callbackData;
		using (data.fulldef)
		{
			if (go == null)
			{
				Debug.LogWarning($"AdventureClassChallenge.OnActorLoaded() - FAILED to load actor \"{assetRef}\"");
				return;
			}
			Actor actor = go.GetComponent<Actor>();
			if (actor == null)
			{
				Debug.LogWarning($"AdventureClassChallenge.OnActorLoaded() - ERROR actor \"{base.name}\" has no Actor component");
				return;
			}
			actor.TurnOffCollider();
			actor.SetUnlit();
			Object.Destroy(actor.m_healthObject);
			Object.Destroy(actor.m_attackObject);
			actor.SetEntityDef(data.fulldef.EntityDef);
			actor.SetCardDef(data.fulldef.DisposableCardDef);
			actor.SetPremium(TAG_PREMIUM.NORMAL);
			actor.UpdateAllComponents();
			GameObject container = m_LeftHeroContainer;
			if (data.heroNum == 0)
			{
				Object.Destroy(m_LeftHero);
				m_LeftHero = go;
				m_LeftHeroName.Text = data.fulldef.EntityDef.GetName();
			}
			else
			{
				Object.Destroy(m_RightHero);
				m_RightHero = go;
				container = m_RightHeroContainer;
			}
			GameUtils.SetParent(actor, container);
			actor.transform.localRotation = Quaternion.identity;
			actor.transform.localScale = Vector3.one;
			actor.GetAttackObject().Hide();
			actor.Show();
		}
	}

	private void OnVersusLettersLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogError($"AdventureClassChallenge.OnVersusLettersLoaded() - FAILED to load \"{assetRef}\"");
			return;
		}
		GameUtils.SetParent(go, m_VersusTextContainer);
		go.GetComponentInChildren<VS>().ActivateShadow();
		go.transform.localRotation = Quaternion.identity;
		go.transform.Rotate(new Vector3(0f, 180f, 0f));
		go.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
		Component[] renders = go.GetComponentsInChildren(typeof(Renderer));
		for (int i = 0; i < renders.Length - 1; i++)
		{
			((Renderer)renders[i]).GetMaterial().SetColor("_Color", m_VersusTextColor);
		}
	}

	private static bool OnNavigateBack()
	{
		AdventureConfig.Get().SubSceneGoBack();
		return true;
	}

	private void BackButton()
	{
		Navigation.GoBack();
	}

	private void Play()
	{
		m_PlayButton.Disable();
		GameMgr.Get().FindGame(GameType.GT_VS_AI, FormatType.FT_WILD, m_SelectedScenario, 0, 0L, null, null, restoreSavedGameState: false, null, null, 0L);
	}

	private bool OnFindGameEvent(FindGameEventData eventData, object userData)
	{
		if (eventData.m_state == FindGameState.INVALID)
		{
			m_PlayButton.Enable();
		}
		return false;
	}

	private void GetRewardCardForSelectedScenario()
	{
		if (!(m_RewardBone == null))
		{
			m_ChestButton.m_IsRewardLoading = true;
			List<RewardData> rewardData = AdventureProgressMgr.Get().GetImmediateRewardsForDefeatingScenario(m_SelectedScenario);
			if (rewardData != null && rewardData.Count > 0)
			{
				rewardData[0].LoadRewardObject(RewardCardLoaded);
			}
		}
	}

	private void RewardCardLoaded(Reward reward, object callbackData)
	{
		if (reward == null)
		{
			Debug.LogWarning($"AdventureClassChallenge.RewardCardLoaded() - FAILED to load reward \"{base.name}\"");
			return;
		}
		if (reward.gameObject == null)
		{
			Debug.LogWarning($"AdventureClassChallenge.RewardCardLoaded() - Reward GameObject is null \"{base.name}\"");
			return;
		}
		reward.gameObject.transform.parent = m_ChestButton.transform;
		CardReward cardReward = reward.GetComponent<CardReward>();
		if (m_ChestButton.m_RewardCard != null)
		{
			Object.Destroy(m_ChestButton.m_RewardCard);
		}
		m_ChestButton.m_RewardCard = cardReward.m_nonHeroCardsRoot;
		GameUtils.SetParent(cardReward.m_nonHeroCardsRoot, m_RewardBone);
		cardReward.m_nonHeroCardsRoot.SetActive(value: false);
		Object.Destroy(cardReward.gameObject);
		m_ChestButton.m_IsRewardLoading = false;
	}

	private void OnBoxTransitionFinished(object userData)
	{
		Box.Get().RemoveTransitionFinishedListener(OnBoxTransitionFinished);
	}
}
