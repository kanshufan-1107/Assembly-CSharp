using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.DataModels;
using Hearthstone.DungeonCrawl;
using Hearthstone.Progression;
using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
public class AdventureDungeonCrawlPlayMat : MonoBehaviour
{
	public delegate void RewardOptionSelectedCallback(AdventureDungeonCrawlRewardOption.OptionData rewardData);

	public delegate void AssetLoadCompletedCallback();

	public enum PlayMatState
	{
		READY_FOR_DATA,
		SHOWING_OPTIONS,
		TRANSITIONING_FROM_PREV_STATE,
		SHOWING_NEXT_BOSS,
		SHOWING_BOSS_GRAVEYARD,
		DEPRECATED_PVPDR_ACTIVE,
		DEPRECATED_PVPDR_REWARD
	}

	public enum OptionType
	{
		INVALID,
		LOOT,
		TREASURE,
		SHRINE_TREASURE,
		HERO_POWER,
		DECK,
		TREASURE_SATCHEL
	}

	[Serializable]
	public class PlaymatStyleOverride
	{
		public DungeonRunVisualStyle VisualStyle;

		public Color PhoneHeaderTextColor;

		public Color PhoneHeaderOutlineColor;

		public ParticleSystem NextBossDustEffectSmall;

		public ParticleSystem NextBossDustEffectLarge;

		public ParticleSystem NextBossDustEffectLargeMotes;

		public ParticleSystem FacedownBossesDustEffect;

		[CustomEditField(Sections = "SFX Overrides", T = EditType.SOUND_PREFAB)]
		public string NextBossFlipSmallSFX;

		[CustomEditField(Sections = "SFX Overrides", T = EditType.SOUND_PREFAB)]
		public string NextBossFlipLargeSFX;

		[CustomEditField(Sections = "SFX Overrides", T = EditType.SOUND_PREFAB)]
		public string NextBossFlipCrowdReactionSmallSFX;

		[CustomEditField(Sections = "SFX Overrides", T = EditType.SOUND_PREFAB)]
		public string NextBossFlipCrowdReactionMediumSFX;

		[CustomEditField(Sections = "SFX Overrides", T = EditType.SOUND_PREFAB)]
		public string NextBossFlipCrowdReactionLargeSFX;

		[CustomEditField(Sections = "SFX Overrides", T = EditType.SOUND_PREFAB)]
		public string BossDeckDropSFX;

		[CustomEditField(Sections = "SFX Overrides", T = EditType.SOUND_PREFAB)]
		public string BossDeckMagicallyAppearSFX;

		[CustomEditField(Sections = "String Overrides")]
		public List<HeaderStringOverride> ChooseTreasureHeaderString;

		[CustomEditField(Sections = "String Overrides")]
		public List<HeaderStringOverride> ChooseLootHeaderString;
	}

	[Serializable]
	public class HeaderStringOverride
	{
		public int MinimumDefeatedBosses;

		public string HeaderString;
	}

	[CustomEditField(Sections = "UI")]
	public UberText m_headerText;

	[CustomEditField(Sections = "UI")]
	public AsyncReference m_PlayButtonReference;

	[CustomEditField(Sections = "UI")]
	public GameObject m_PlayButtonRoot;

	[CustomEditField(Sections = "UI")]
	public GameObject m_PlayButtonPlate;

	[CustomEditField(Sections = "UI")]
	public List<NestedPrefabPlatformOverride> m_rewardOptionNestedPrefabs = new List<NestedPrefabPlatformOverride>();

	[CustomEditField(Sections = "UI")]
	public List<AdventureDungeonCrawlHeroPowerOption> m_heroPowerOptions;

	[CustomEditField(Sections = "UI")]
	public List<AdventureDungeonCrawlDeckOption> m_deckOptions;

	[CustomEditField(Sections = "UI")]
	public Widget m_treasureSatchelWidget;

	[CustomEditField(Sections = "UI")]
	public GameObject m_optionsPane;

	[CustomEditField(Sections = "UI")]
	public GameObject m_nextBossPane;

	[CustomEditField(Sections = "UI")]
	public NestedPrefabBase m_bossGraveyardPane;

	[CustomEditField(Sections = "UI")]
	public GameObject m_allCards;

	[CustomEditField(Sections = "UI")]
	public GameObject m_facedownCards;

	[CustomEditField(Sections = "UI")]
	public GameObject m_bossHeroPowerTooltipBone;

	[CustomEditField(Sections = "UI")]
	public float m_bossHeroPowerTooltipPulseRate;

	[CustomEditField(Sections = "UI")]
	public float m_bossHeroPowerTooltipDelayAfterVo;

	[CustomEditField(Sections = "UI")]
	public PlayNewParticles m_nextBossPlayNewParticlesScript;

	[CustomEditField(Sections = "UI")]
	public PlayNewParticles m_facedownBossesPlayNewParticlesScript;

	[CustomEditField(Sections = "UI")]
	public AsyncReference m_treasureSatchelReference;

	[CustomEditField(Sections = "UI")]
	public AsyncReference m_treasureInspectReference;

	[CustomEditField(Sections = "UI")]
	public AsyncReference m_platformControllerReference;

	[CustomEditField(Sections = "UI")]
	public AsyncReference m_paperControllerReference;

	[CustomEditField(Sections = "UI")]
	public AsyncReference m_paperControllerReference_phone;

	[CustomEditField(Sections = "UI")]
	public GameObject m_selectedOptionClickBlocker;

	[CustomEditField(Sections = "Animations")]
	public Animation m_nextBossFlipAnimation;

	[CustomEditField(Sections = "Animations")]
	public string m_nextBossFlipSmallName;

	[CustomEditField(Sections = "Animations")]
	public string m_nextBossFlipLargeName;

	[CustomEditField(Sections = "Animations")]
	public Animation m_bossDeckDropAnimation;

	[CustomEditField(Sections = "Animations")]
	public float m_delayAfterDeckDrop = 1f;

	[CustomEditField(Sections = "Animations")]
	public float m_lootDropDelay = 0.05f;

	[CustomEditField(Sections = "SFX", T = EditType.SOUND_PREFAB)]
	public string m_nextBossFlipSmallSFXDefault;

	[CustomEditField(Sections = "SFX", T = EditType.SOUND_PREFAB)]
	public string m_nextBossFlipLargeSFXDefault;

	[CustomEditField(Sections = "SFX", T = EditType.SOUND_PREFAB)]
	public string m_nextBossFlipCrowdReactionSmallSFXDefault;

	[CustomEditField(Sections = "SFX", T = EditType.SOUND_PREFAB)]
	public string m_nextBossFlipCrowdReactionMediumSFXDefault;

	[CustomEditField(Sections = "SFX", T = EditType.SOUND_PREFAB)]
	public string m_nextBossFlipCrowdReactionLargeSFXDefault;

	[CustomEditField(Sections = "SFX")]
	public float m_nextBossFlipCrowdReactionDelay = 0.5f;

	[CustomEditField(Sections = "SFX", T = EditType.SOUND_PREFAB)]
	public string m_bossDeckDropSFXDefault;

	[CustomEditField(Sections = "SFX", T = EditType.SOUND_PREFAB)]
	public string m_bossDeckMagicallyAppearSFXDefault;

	[CustomEditField(Sections = "SFX", T = EditType.SOUND_PREFAB)]
	public string m_bossMouseOverSFXDefault;

	[CustomEditField(Sections = "Styles")]
	public List<PlaymatStyleOverride> m_playmatStyleOverride;

	[CustomEditField(Sections = "Bones")]
	public Transform m_nextBossBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_nextBossFaceBone;

	[CustomEditField(Sections = "Bones")]
	public Transform m_nextBossBackBone;

	[CustomEditField(Sections = "Bones")]
	public List<Transform> m_bossCardBones = new List<Transform>();

	[CustomEditField(Sections = "Bones")]
	public GameObject m_BossPowerBone;

	[CustomEditField(Sections = "Bones")]
	public List<Transform> m_cardBackBones = new List<Transform>();

	[CustomEditField(Sections = "Bones")]
	public SlidingTray m_MobilePlayButtonSlidingTrayBone;

	private PlayMatState m_playMatState;

	private PlayMatState m_lastVisualPlayMatState;

	private bool m_startCallFinished;

	private Actor m_bossActor;

	private DefLoader.DisposableCardDef m_bossCardDef;

	private EntityDef m_bossEntityDef;

	private List<Actor> m_defeatedBossActors = new List<Actor>();

	private GameObject m_nextBossCardBack;

	private Actor m_topDefeatedBoss;

	private List<GameObject> m_cardBacks = new List<GameObject>();

	private PlayButton m_playButton;

	private List<AdventureDungeonCrawlRewardOption> m_rewardOptions;

	private AdventureDungeonCrawlBossGraveyard m_bossGraveyard;

	private bool m_subsceneTransitionComplete;

	private CardBack m_cardBack;

	private OptionType m_currentOptionType;

	private int m_numBossesDefeated;

	private int m_bossesPerRun;

	private bool m_allowPlayButtonAnimation;

	private bool m_setUpDefeatedBossesCompleted;

	private int m_playerHeroDbId;

	private PlaymatStyleOverride m_matchingPlaymatStyle;

	private string m_nextBossFlipSmallSFXOverride;

	private string m_nextBossFlipLargeSFXOverride;

	private string m_nextBossFlipCrowdReactionSmallSFXOverride;

	private string m_nextBossFlipCrowdReactionMediumSFXOverride;

	private string m_nextBossFlipCrowdReactionLargeSFXOverride;

	private string m_bossDeckDropSFXOverride;

	private string m_bossDeckMagicallyAppearSFXOverride;

	private string m_chooseTreasureHeaderStringOverride;

	private string m_chooseLootHeaderStringOverride;

	private List<AdventureDungeonCrawlTreasureOption> m_treasureSatchelOptions;

	public Widget m_treasureInspectWidget;

	private bool m_loadingCardback;

	private Notification m_bossHeroPowerTooltip;

	private bool m_shouldShowBossHeroPowerTooltip;

	private VisualController m_paperController;

	private IDungeonCrawlData m_dungeonCrawlData;

	private static readonly PlatformDependentValue<string> HERO_POWER_TOOLTIP_STRING = new PlatformDependentValue<string>(PlatformCategory.Screen)
	{
		PC = "GLUE_ADVENTURE_DUNGEON_CRAWL_BOSS_HERO_POWER_TOOLTIP",
		Phone = "GLUE_ADVENTURE_DUNGEON_CRAWL_BOSS_HERO_POWER_TOOLTIP_PHONE"
	};

	private const string TREASURE_SATCHEL_OPTION_SELECTED_EVENT = "CODE_TREASURE_OPTION_SELECTED";

	private const string TREASURE_SATCHEL_SHOW_EVENT = "CODE_TREASURE_SATCHEL_SHOW";

	private const string TREASURE_SATCHEL_OUTRO_COMPLETE_EVENT = "CODE_TREASURE_SATCHEL_OUTRO_COMPLETE";

	private bool m_playMatStateInitialized;

	public bool IsNextMissionASpecialEncounter { get; set; }

	public PlayButton PlayButton => m_playButton;

	private void Awake()
	{
		m_treasureSatchelReference.RegisterReadyListener<Widget>(OnTreasureSatchelReady);
		m_treasureInspectReference.RegisterReadyListener<Widget>(OnTreasureInspectReady);
		if (m_treasureSatchelWidget != null)
		{
			m_treasureSatchelWidget.gameObject.SetActive(value: false);
		}
	}

	private void Start()
	{
		m_rewardOptions = new List<AdventureDungeonCrawlRewardOption>(m_rewardOptionNestedPrefabs.Count);
		for (int i = 0; i < m_rewardOptionNestedPrefabs.Count; i++)
		{
			NestedPrefabBase nestedPrefab = m_rewardOptionNestedPrefabs[i];
			if (nestedPrefab == null)
			{
				Debug.LogWarningFormat("AdventureDungeonCrawlPlayMat.Start - m_rewardOptionNestedPrefabs have null values. Skipping index {0}...", i);
				continue;
			}
			AdventureDungeonCrawlRewardOption rewardOption = nestedPrefab.PrefabGameObject(instantiateIfNeeded: true).GetComponent<AdventureDungeonCrawlRewardOption>();
			switch (i)
			{
			case 0:
				TransformUtil.SetLocalPosX(rewardOption.m_deckTray.m_deckBigCard, 0.27f);
				rewardOption.m_deckTray.m_deckBigCard.m_flipHeroPowerHorizontalPosition = true;
				break;
			case 1:
				rewardOption.m_deckTray.m_deckBigCard.m_flipHeroPowerHorizontalPosition = true;
				break;
			}
			rewardOption.m_deckTray.m_deckBigCard.m_disableCollidersOnHeroPower = true;
			rewardOption.m_deckTray.m_deckBigCard.m_showTooltipsForAdventure = true;
			m_rewardOptions.Add(rewardOption);
		}
		m_PlayButtonReference.RegisterReadyListener<PlayButton>(OnPlayButtonReady);
		SetUpPlayButton();
		m_startCallFinished = true;
	}

	private void OnDestroy()
	{
		m_bossCardDef?.Dispose();
		m_bossCardDef = null;
	}

	public void Initialize(IDungeonCrawlData data)
	{
		m_dungeonCrawlData = data;
		AdventureConfig.Get().GetAdventureDataModel().SelectedHeroId = GameUtils.TranslateDbIdToCardId((int)data.SelectedHeroCardDbId);
		SetPlaymatVisualStyle();
		foreach (AdventureDungeonCrawlRewardOption rewardOption in m_rewardOptions)
		{
			rewardOption.Initalize(m_dungeonCrawlData);
		}
		m_paperControllerReference.RegisterReadyListener<VisualController>(OnPaperControllerReady);
		m_paperControllerReference_phone.RegisterReadyListener<VisualController>(OnPaperControllerReady);
	}

	private void Update()
	{
		if (m_playMatState != PlayMatState.TRANSITIONING_FROM_PREV_STATE)
		{
			return;
		}
		bool isReady = true;
		if (m_currentOptionType == OptionType.INVALID)
		{
			return;
		}
		IEnumerable<AdventureOptionWidget> outWidgetOptions = null;
		if (m_currentOptionType == OptionType.HERO_POWER)
		{
			outWidgetOptions = m_heroPowerOptions.Cast<AdventureOptionWidget>();
		}
		else if (m_currentOptionType == OptionType.DECK)
		{
			outWidgetOptions = m_deckOptions.Cast<AdventureOptionWidget>();
		}
		else if (m_currentOptionType == OptionType.TREASURE_SATCHEL)
		{
			outWidgetOptions = m_treasureSatchelOptions.Cast<AdventureOptionWidget>();
			if (m_treasureSatchelWidget == null || !m_treasureSatchelWidget.IsReady || m_treasureSatchelWidget.HasPendingActions)
			{
				isReady = false;
			}
		}
		if (outWidgetOptions != null)
		{
			foreach (AdventureOptionWidget option in outWidgetOptions)
			{
				if (option.IsOutroPlaying || !option.IsReady)
				{
					isReady = false;
					break;
				}
			}
		}
		else
		{
			for (int index = 0; index < m_rewardOptions.Count; index++)
			{
				if (m_rewardOptions[index].OutroIsPlaying())
				{
					isReady = false;
					break;
				}
			}
		}
		if (isReady)
		{
			SetPlayMatState(PlayMatState.READY_FOR_DATA, setAsInitialized: true);
		}
	}

	public bool IsReady()
	{
		if (!m_startCallFinished)
		{
			return false;
		}
		if (m_bossActor == null)
		{
			return false;
		}
		if (m_playButton == null)
		{
			Log.Adventures.PrintWarning("PlayButton not ready yet!");
			return false;
		}
		return true;
	}

	public void SetTreasureSatchelOptionSelectedCallback(AdventureDungeonCrawlTreasureOption.TreasureSelectedOptionCallback callback)
	{
		if (m_treasureSatchelReference == null)
		{
			Debug.LogError("AdventureDungeonCrawlPlayMat.SetTreasureSatchelOptionSelectedCallback - m_treasureSatchelReference was null!");
			return;
		}
		m_treasureSatchelReference.RegisterReadyListener<Widget>(delegate
		{
			if (m_treasureSatchelOptions != null)
			{
				foreach (AdventureDungeonCrawlTreasureOption current in m_treasureSatchelOptions)
				{
					if (current != null)
					{
						current.SetOptionCallbacks(callback);
					}
				}
			}
		});
	}

	public void SetDeckOptionSelectedCallback(AdventureDungeonCrawlDeckOption.DeckOptionSelectedCallback callback)
	{
		foreach (AdventureDungeonCrawlDeckOption deckOption in m_deckOptions)
		{
			deckOption.SetOptionCallbacks(callback);
		}
	}

	public void SetHeroPowerOptionCallback(AdventureDungeonCrawlHeroPowerOption.HeroPowerSelectedOptionCallback selectedCallback, AdventureDungeonCrawlHeroPowerOption.HeroPowerHoverOptionCallback rolloverCallback, AdventureDungeonCrawlHeroPowerOption.HeroPowerHoverOptionCallback rolloutCallback)
	{
		foreach (AdventureDungeonCrawlHeroPowerOption heroPowerOption in m_heroPowerOptions)
		{
			heroPowerOption.SetOptionCallbacks(selectedCallback, rolloverCallback, rolloutCallback);
		}
	}

	public void SetRewardOptionSelectedCallback(RewardOptionSelectedCallback callback)
	{
		foreach (AdventureDungeonCrawlRewardOption rewardOption in m_rewardOptions)
		{
			rewardOption.SetOptionChosenCallback(delegate
			{
				callback(rewardOption.GetOptionData());
			});
		}
	}

	public void DeselectAllDeckOptionsWithoutId(int deckId)
	{
		foreach (AdventureDungeonCrawlDeckOption option in m_deckOptions)
		{
			if (option.DeckId != deckId)
			{
				option.Deselect();
			}
		}
	}

	public void SetBossActor(Actor bossActor)
	{
		m_bossActor = bossActor;
		if (m_bossActor != null && m_bossCardDef != null && m_bossEntityDef != null)
		{
			SetUpBossCard();
		}
	}

	public void SetBossFullDef(DefLoader.DisposableCardDef cardDef, EntityDef entityDef)
	{
		m_bossCardDef?.Dispose();
		m_bossCardDef = cardDef;
		m_bossEntityDef = entityDef;
		if (m_bossActor != null && m_bossCardDef != null && m_bossEntityDef != null)
		{
			SetUpBossCard();
		}
	}

	private void SetUpBossCard()
	{
		if (m_bossActor == null)
		{
			Log.Adventures.PrintError("AdventureDungeonCrawlDisplay.SetUpBossCard - m_BossActor is null!");
			return;
		}
		if (m_bossCardDef == null || m_bossEntityDef == null)
		{
			Log.Adventures.PrintError("AdventureDungeonCrawlDisplay.SetUpBossCard - m_bossFullDef is null!");
			return;
		}
		m_bossActor.SetCardDef(m_bossCardDef);
		m_bossActor.SetEntityDef(m_bossEntityDef);
		m_bossActor.SetPremium(TAG_PREMIUM.NORMAL);
		PegUIElement bossPegUI = m_bossActor.GetCollider().gameObject.GetComponent<PegUIElement>();
		if ((bool)bossPegUI)
		{
			bossPegUI.AddEventListener(UIEventType.ROLLOVER, delegate
			{
				SoundManager.Get().LoadAndPlay(m_bossMouseOverSFXDefault);
			});
		}
		else
		{
			Debug.LogError("Could not find PegUIElement for Boss");
		}
		m_bossActor.SetCardbackUpdateIgnore(ignoreUpdate: true);
		if (m_cardBack != null)
		{
			m_bossActor.m_cardMesh.GetComponent<Renderer>().GetMaterial(m_bossActor.m_cardBackMatIdx).mainTexture = m_cardBack.m_CardBackTexture;
		}
		m_bossActor.Show();
	}

	public void SetCardBack(int cardBackId)
	{
		m_loadingCardback = true;
		if (!CardBackManager.Get().LoadCardBackByIndex(cardBackId, OnCardBackLoaded))
		{
			Log.Adventures.PrintError("AdventureDungeonCrawlPlayMat.SetCardBack() - failed to load CardBack {0}", cardBackId);
			m_loadingCardback = false;
		}
	}

	public void SetPlayerHeroDbId(int heroDbId)
	{
		m_playerHeroDbId = heroDbId;
	}

	private void OnCardBackLoaded(CardBackManager.LoadCardBackData cardbackData)
	{
		m_loadingCardback = false;
		m_cardBack = cardbackData.m_CardBack;
		if (m_bossActor != null)
		{
			m_bossActor.m_cardMesh.GetComponent<Renderer>().GetMaterial(m_bossActor.m_cardBackMatIdx).mainTexture = m_cardBack.m_CardBackTexture;
		}
		if (m_cardBackBones.Count < 1)
		{
			Log.Adventures.PrintError("AdventureDungeonCrawlPlayMat.OnCardBackLoaded() - Can't attach the cardbacks to a bone, as m_cardBackBones are not defined!");
			return;
		}
		m_nextBossCardBack = cardbackData.m_GameObject;
		Actor actor = m_nextBossCardBack.GetComponent<Actor>();
		if (actor != null)
		{
			actor.SetCardbackUpdateIgnore(ignoreUpdate: true);
		}
		GameUtils.SetParent(m_nextBossCardBack, m_nextBossBackBone, withRotation: true);
		m_cardBacks.Clear();
	}

	public void SetUpDefeatedBosses(List<long> defeatedBossIds, int bossesPerRun)
	{
		if (m_setUpDefeatedBossesCompleted)
		{
			Debug.LogError("Calling SetUpDefeatedBosses, when this has already been called! Please investigate - you should not be doing this!");
			return;
		}
		m_numBossesDefeated = defeatedBossIds?.Count ?? 0;
		m_bossesPerRun = bossesPerRun;
		int numBonesToUse = Mathf.Min(m_bossCardBones.Count - 1, m_numBossesDefeated);
		if (m_numBossesDefeated >= bossesPerRun)
		{
			Log.Adventures.PrintWarning("AdventureDungeonCrawlPlayMat.SetUpDefeatedBosses() - Your run is done!  Why are you trying to set up defeated bosses?");
			return;
		}
		if (m_defeatedBossActors.Count < m_numBossesDefeated)
		{
			if (m_bossActor == null)
			{
				Log.Adventures.PrintError("AdventureDungeonCrawlDisplay attempting to clone from m_BossActor, but it is null!");
			}
			else
			{
				while (m_defeatedBossActors.Count < numBonesToUse)
				{
					Actor defeatedBossActor = UnityEngine.Object.Instantiate(m_bossActor.gameObject).GetComponent<Actor>();
					GameUtils.SetParent(defeatedBossActor, m_bossCardBones[m_defeatedBossActors.Count], withRotation: true);
					defeatedBossActor.GetHealthObject().Hide();
					m_defeatedBossActors.Add(defeatedBossActor);
				}
			}
		}
		if (numBonesToUse > 0 && m_defeatedBossActors.Count >= numBonesToUse)
		{
			int lastDefeatedBossId = (int)defeatedBossIds[defeatedBossIds.Count - 1];
			string lastDefeatedBossCardId = GameUtils.TranslateDbIdToCardId(lastDefeatedBossId);
			if (lastDefeatedBossCardId == null)
			{
				Log.Adventures.PrintWarning("AdventureDungeonCrawlPlayMat.SetUpDefeatedBosses() - No cardId for last defeated boss dbId {0}!", lastDefeatedBossId);
			}
			else
			{
				m_topDefeatedBoss = m_defeatedBossActors[m_defeatedBossActors.Count - 1];
				using DefLoader.DisposableFullDef def = DefLoader.Get().GetFullDef(lastDefeatedBossCardId);
				m_topDefeatedBoss.SetEntityDef(def.EntityDef);
				m_topDefeatedBoss.SetCardDef(def.DisposableCardDef);
				m_topDefeatedBoss.SetPremium(TAG_PREMIUM.NORMAL);
				m_topDefeatedBoss.UpdateAllComponents();
			}
		}
		TransformUtil.AttachAndPreserveLocalTransform(m_nextBossBone, m_bossCardBones[Mathf.Min(numBonesToUse, m_bossCardBones.Count - 1)]);
		if (numBonesToUse == 0)
		{
			m_allCards.SetActive(value: false);
		}
		m_setUpDefeatedBossesCompleted = true;
	}

	public void SetUpCardBacks(int numUndefeatedBosses, AssetLoadCompletedCallback callback)
	{
		StartCoroutine(SetUpCardBacksWhenReady(numUndefeatedBosses, callback));
	}

	private IEnumerator SetUpCardBacksWhenReady(int numUndefeatedBosses, AssetLoadCompletedCallback callback)
	{
		while (m_loadingCardback)
		{
			yield return null;
		}
		if (m_nextBossCardBack == null)
		{
			Log.Adventures.PrintError("AdventureDungeonCrawlPlayMat.SetUpCardBacksWhenReady() - done loading cardback, but it must have failed!  Can't make more cardbacks!");
			callback?.Invoke();
			yield break;
		}
		int numCardBacksToShow = Mathf.Min(numUndefeatedBosses, m_cardBackBones.Count);
		if (numCardBacksToShow == 0)
		{
			m_facedownCards.SetActive(value: false);
		}
		else
		{
			while (m_cardBacks.Count < numCardBacksToShow)
			{
				GameObject cardBack = UnityEngine.Object.Instantiate(m_nextBossCardBack);
				Actor actor = cardBack.GetComponent<Actor>();
				if (actor != null)
				{
					actor.SetCardbackUpdateIgnore(ignoreUpdate: true);
				}
				GameUtils.SetParent(cardBack, m_cardBackBones[m_cardBacks.Count], withRotation: true);
				m_cardBacks.Add(cardBack);
			}
		}
		callback?.Invoke();
	}

	private void SetUpPlayButton()
	{
		if ((bool)UniversalInputManager.UsePhoneUI && m_MobilePlayButtonSlidingTrayBone != null)
		{
			GameUtils.SetParent(m_PlayButtonRoot, m_MobilePlayButtonSlidingTrayBone);
			m_PlayButtonPlate.SetActive(value: true);
		}
	}

	private void EnablePlayButton(bool enabled)
	{
		if (enabled)
		{
			m_playButton.Enable();
		}
		else
		{
			m_playButton.Disable();
		}
		if ((bool)UniversalInputManager.UsePhoneUI && m_MobilePlayButtonSlidingTrayBone != null)
		{
			m_MobilePlayButtonSlidingTrayBone.ToggleTraySlider(enabled, null, m_allowPlayButtonAnimation);
		}
	}

	public void ShowTreasureOptions(List<long> treasureOptions)
	{
		if (treasureOptions == null || treasureOptions.Count == 0)
		{
			Log.Adventures.PrintWarning("AdventureDungeonCrawlPlayMat - Attempting to show Treasure, but no treasure was passed in!");
			return;
		}
		m_currentOptionType = OptionType.TREASURE;
		SetPlayMatState(PlayMatState.SHOWING_OPTIONS, setAsInitialized: false);
		for (int i = 0; i < treasureOptions.Count; i++)
		{
			if (i < m_rewardOptions.Count && m_rewardOptions[i] != null)
			{
				m_rewardOptions[i].SetRewardData(new AdventureDungeonCrawlRewardOption.OptionData(OptionType.TREASURE, new List<long> { treasureOptions[i] }, i));
			}
		}
		SetPlayMatStateAsInitializedAndPlayTransition();
	}

	public void ShowLootOptions(List<long> classLootOptionsA, List<long> classLootOptionsB, List<long> classLootOptionsC)
	{
		if ((classLootOptionsA == null || classLootOptionsA.Count == 0) && (classLootOptionsB == null || classLootOptionsB.Count == 0) && (classLootOptionsC == null || classLootOptionsC.Count == 0))
		{
			Log.Adventures.PrintWarning("AdventureDungeonCrawlPlayMat - Attempting to show Loot, but no loot was passed in!");
			return;
		}
		List<List<long>> classLootOptions = new List<List<long>> { classLootOptionsA, classLootOptionsB, classLootOptionsC };
		m_currentOptionType = OptionType.LOOT;
		SetPlayMatState(PlayMatState.SHOWING_OPTIONS, setAsInitialized: false);
		for (int index = 0; index < m_rewardOptions.Count; index++)
		{
			AdventureDungeonCrawlRewardOption rewardOption = m_rewardOptions[index];
			if (index >= classLootOptions.Count)
			{
				break;
			}
			rewardOption.SetRewardData(new AdventureDungeonCrawlRewardOption.OptionData(OptionType.LOOT, classLootOptions[index], index));
		}
		SetPlayMatStateAsInitializedAndPlayTransition();
	}

	public void ShowShrineOptions(List<long> shrineOptions)
	{
		m_currentOptionType = OptionType.SHRINE_TREASURE;
		SetPlayMatState(PlayMatState.SHOWING_OPTIONS, setAsInitialized: false);
		if (shrineOptions == null || shrineOptions.Count == 0)
		{
			Debug.LogError("ShowShrineOptions - No shrines provided.");
			return;
		}
		for (int index = 0; index < m_rewardOptions.Count && shrineOptions.Count > index; index++)
		{
			m_rewardOptions[index].SetRewardData(new AdventureDungeonCrawlRewardOption.OptionData(OptionType.SHRINE_TREASURE, new List<long> { shrineOptions[index] }, index));
		}
		SetPlayMatStateAsInitializedAndPlayTransition();
	}

	public void ShowTreasureSatchel(List<AdventureLoadoutTreasuresDbfRecord> adventureLoadoutTreasures, GameSaveKeyId adventureGameSaveServerKey, GameSaveKeyId adventureGameSaveClientKey)
	{
		if (m_treasureSatchelWidget == null)
		{
			Debug.LogError("AdventureDungeonCrawlPlayMat.ShowTreasureSatchel - m_treasureSatchel is null!");
			return;
		}
		m_currentOptionType = OptionType.TREASURE_SATCHEL;
		SetPlayMatState(PlayMatState.SHOWING_OPTIONS, setAsInitialized: false);
		m_treasureSatchelWidget.gameObject.SetActive(value: true);
		m_treasureSatchelWidget.Hide();
		StartCoroutine(ShowTreasureSatchelWhenReady(adventureLoadoutTreasures, adventureGameSaveServerKey, adventureGameSaveClientKey));
	}

	private IEnumerator ShowTreasureSatchelWhenReady(List<AdventureLoadoutTreasuresDbfRecord> adventureLoadoutTreasures, GameSaveKeyId adventureGameSaveServerKey, GameSaveKeyId adventureGameSaveClientKey)
	{
		while (!m_subsceneTransitionComplete || !m_treasureSatchelWidget.IsReady || m_treasureSatchelWidget.IsChangingStates)
		{
			yield return null;
		}
		m_treasureSatchelWidget.TriggerEvent("CODE_TREASURE_SATCHEL_SHOW");
		while (m_treasureSatchelWidget.IsChangingStates)
		{
			yield return null;
		}
		m_treasureSatchelWidget.Show();
		if (adventureLoadoutTreasures.Count > m_treasureSatchelOptions.Count)
		{
			Log.Adventures.PrintWarning("AdventureDungeonCrawlPlayMat.ShowTreasureSatchelWhenReady - there are more Adventure Loadout Treasures than option visuals to show them! Number of Loadout Treasures: {0} Number of PlayMat options: {1}", adventureLoadoutTreasures.Count, m_treasureSatchelOptions.Count);
		}
		m_treasureSatchelWidget.GetDataModel(32, out var dataModel);
		if (!(dataModel is AdventureTreasureSatchelDataModel satchelDataModel))
		{
			Log.Adventures.PrintError("AdventureDungeonCrawlPlayMat.ShowTreasureSatchelWhenReady - satchel has no data model!");
			yield break;
		}
		satchelDataModel.Cards.Clear();
		List<long> treasureRunWins = TreasureWinsForScenario(adventureGameSaveServerKey, (int)m_dungeonCrawlData.GetMission());
		GameSaveDataManager.Get().GetSubkeyValue(adventureGameSaveClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_NEWLY_UNLOCKED_LOADOUT_TREASURES, out List<long> newlyUnlockedTreasures);
		int i = 0;
		while (i < adventureLoadoutTreasures.Count)
		{
			if (i > m_treasureSatchelOptions.Count - 1)
			{
				Log.Adventures.PrintWarning("AdventureDungeonCrawlPlayMat.ShowTreasureSatchelWhenReady - there are not enough Adventure Loadout Treasures to fill the PlayMat options!  Number of CardDataModels: {0} Number of PlayMat options: {1}", adventureLoadoutTreasures.Count, m_treasureSatchelOptions.Count);
				break;
			}
			bool locked = false;
			bool upgraded = false;
			string lockedText = string.Empty;
			string unlockCriteriaText = adventureLoadoutTreasures[i].UnlockCriteriaText;
			int cardDbId = adventureLoadoutTreasures[i].CardId;
			bool hasUnlockAchieve = adventureLoadoutTreasures[i].UnlockAchievement > 0;
			if (adventureLoadoutTreasures[i].UnlockValue > 0 || hasUnlockAchieve)
			{
				locked = !m_dungeonCrawlData.AdventureTreasureIsUnlocked(adventureGameSaveServerKey, adventureLoadoutTreasures[i], out var unlockProgress, out var _);
				if (locked && !string.IsNullOrEmpty(unlockCriteriaText))
				{
					int achQuota = 0;
					if (adventureLoadoutTreasures[i].UnlockAchievement > 0)
					{
						achQuota = AchievementManager.Get().GetAchievementDataModel(adventureLoadoutTreasures[i].UnlockAchievement).Quota;
					}
					int unlockValue = adventureLoadoutTreasures[i].UnlockValue + achQuota;
					lockedText = string.Format(unlockCriteriaText, unlockProgress, unlockValue);
				}
			}
			if (adventureLoadoutTreasures[i].UpgradeValue > 0)
			{
				upgraded = m_dungeonCrawlData.AdventureTreasureIsUpgraded(adventureGameSaveServerKey, adventureLoadoutTreasures[i], out var _);
				if (upgraded)
				{
					cardDbId = adventureLoadoutTreasures[i].UpgradedCardId;
				}
			}
			bool completed = treasureRunWins?.Contains(cardDbId) ?? false;
			bool newlyUnlocked = newlyUnlockedTreasures?.Contains(cardDbId) ?? false;
			AdventureDungeonCrawlTreasureOption treasureSatchelOption = m_treasureSatchelOptions[i];
			CardDataModel cardDataModel = new CardDataModel();
			satchelDataModel.Cards.Add(cardDataModel);
			if (cardDbId != 0 && cardDataModel != null)
			{
				string cardId = GameUtils.TranslateDbIdToCardId(cardDbId);
				CardDbfRecord record = GameDbf.GetIndex().GetCardRecord(cardId);
				cardDataModel.CardId = cardId;
				cardDataModel.FlavorText = record?.FlavorText;
			}
			while (!treasureSatchelOption.IsReady)
			{
				yield return null;
			}
			treasureSatchelOption.Init(cardDbId, locked, lockedText, upgraded, completed, newlyUnlocked, delegate
			{
				if (treasureSatchelOption.IsNewlyUnlocked)
				{
					GameSaveDataManager.SubkeySaveRequest subkeySaveRequest = GameSaveDataManager.Get().GenerateSaveRequestToRemoveValueFromSubkeyIfItExists(adventureGameSaveClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_NEWLY_UNLOCKED_LOADOUT_TREASURES, cardDbId);
					if (subkeySaveRequest != null)
					{
						Log.Adventures.Print("Treasure Card {0} was Newly Unlocked but the player just acknowledged it, so saving that it is no longer Newly Unlocked.", cardDbId);
						GameSaveDataManager.Get().SaveSubkey(subkeySaveRequest);
						treasureSatchelOption.IsNewlyUnlocked = false;
					}
				}
			});
			int num = i + 1;
			i = num;
		}
		foreach (AdventureDungeonCrawlTreasureOption treasureSatchelOption2 in m_treasureSatchelOptions)
		{
			while (!treasureSatchelOption2.IsReady)
			{
				yield return null;
			}
		}
		SetPlayMatStateAsInitializedAndPlayTransition();
	}

	public void ShowHeroPowers(List<AdventureHeroPowerDbfRecord> adventureHeroPowers, GameSaveKeyId adventureGameSaveServerKey, GameSaveKeyId adventureGameSaveClientKey)
	{
		m_currentOptionType = OptionType.HERO_POWER;
		SetPlayMatState(PlayMatState.SHOWING_OPTIONS, setAsInitialized: false);
		if (adventureHeroPowers.Count > m_heroPowerOptions.Count)
		{
			Log.Adventures.PrintWarning("There are more Adventure Hero Powers than option visuals to shown them! Number of Hero Powers: {0} Number of PlayMat options: {1}", adventureHeroPowers.Count, m_heroPowerOptions.Count);
		}
		List<long> heroPowerRunWins = HeroPowerWinsForScenario(adventureGameSaveServerKey, (int)m_dungeonCrawlData.GetMission());
		GameSaveDataManager.Get().GetSubkeyValue(adventureGameSaveClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_NEWLY_UNLOCKED_HERO_POWERS, out List<long> newlyUnlockedHeroPowers);
		for (int i = 0; i < m_heroPowerOptions.Count; i++)
		{
			if (i > adventureHeroPowers.Count - 1)
			{
				Log.Adventures.PrintWarning("There are not enough Adventure Hero Powers to fill the PlayMat options!  Number of Hero Powers: {0} Number of PlayMat options: {1}", adventureHeroPowers.Count, m_heroPowerOptions.Count);
				break;
			}
			bool locked = false;
			string lockedText = string.Empty;
			string unlockCriteriaText = adventureHeroPowers[i].UnlockCriteriaText;
			int heroPowerDbId = adventureHeroPowers[i].CardId;
			bool hasUnlockAchieve = adventureHeroPowers[i].UnlockAchievement > 0;
			if (adventureHeroPowers[i].UnlockValue > 0 || hasUnlockAchieve)
			{
				locked = !m_dungeonCrawlData.AdventureHeroPowerIsUnlocked(adventureGameSaveServerKey, adventureHeroPowers[i], out var unlockProgress, out var _);
				if (locked && !string.IsNullOrEmpty(unlockCriteriaText))
				{
					int achQuota = 0;
					if (adventureHeroPowers[i].UnlockAchievement > 0)
					{
						achQuota = AchievementManager.Get().GetAchievementDataModel(adventureHeroPowers[i].UnlockAchievement).Quota;
					}
					int unlockValue = adventureHeroPowers[i].UnlockValue + achQuota;
					lockedText = string.Format(unlockCriteriaText, unlockProgress, unlockValue);
				}
			}
			bool completed = heroPowerRunWins?.Contains(heroPowerDbId) ?? false;
			bool newlyUnlocked = newlyUnlockedHeroPowers?.Contains(heroPowerDbId) ?? false;
			AdventureDungeonCrawlHeroPowerOption heroPowerOption = m_heroPowerOptions[i];
			heroPowerOption.Init(heroPowerDbId, locked, lockedText, completed, newlyUnlocked, delegate
			{
				if (heroPowerOption.IsNewlyUnlocked)
				{
					GameSaveDataManager.SubkeySaveRequest subkeySaveRequest = GameSaveDataManager.Get().GenerateSaveRequestToRemoveValueFromSubkeyIfItExists(adventureGameSaveClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_NEWLY_UNLOCKED_HERO_POWERS, heroPowerDbId);
					if (subkeySaveRequest != null)
					{
						Log.Adventures.Print("Hero Power {0} was Newly Unlocked but the player just acknowledged it, so saving that it is no longer Newly Unlocked.", heroPowerDbId);
						GameSaveDataManager.Get().SaveSubkey(subkeySaveRequest);
						heroPowerOption.IsNewlyUnlocked = false;
					}
				}
			});
		}
		SetPlayMatStateAsInitializedAndPlayTransition();
	}

	public void ShowDecks(List<AdventureDeckDbfRecord> adventureDecks, GameSaveKeyId adventureGameSaveServerKey, GameSaveKeyId adventureGameSaveClientKey)
	{
		m_currentOptionType = OptionType.DECK;
		SetPlayMatState(PlayMatState.SHOWING_OPTIONS, setAsInitialized: false);
		if (adventureDecks.Count > m_deckOptions.Count)
		{
			Log.Adventures.PrintWarning("There are more Adventure Decks than option visuals to shown them! Number of Decks: {0} Number of PlayMat options: {1}", adventureDecks.Count, m_deckOptions.Count);
		}
		List<long> deckRunWins = DeckWinsForScenario(adventureGameSaveServerKey, (int)m_dungeonCrawlData.GetMission());
		GameSaveDataManager.Get().GetSubkeyValue(adventureGameSaveClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_NEWLY_UNLOCKED_DECKS, out List<long> newlyUnlockedDecks);
		for (int i = 0; i < m_deckOptions.Count; i++)
		{
			if (i > adventureDecks.Count - 1)
			{
				Log.Adventures.PrintWarning("There are not enough Adventure Decks to fill the PlayMat options!  Number of Decks: {0} Number of PlayMat options: {1}", adventureDecks.Count, m_deckOptions.Count);
				break;
			}
			bool locked = false;
			string lockedText = string.Empty;
			string unlockCriteriaText = adventureDecks[i].UnlockCriteriaText;
			if (adventureDecks[i].UnlockValue > 0)
			{
				locked = !m_dungeonCrawlData.AdventureDeckIsUnlocked(adventureGameSaveServerKey, adventureDecks[i], out var unlockProgress, out var _);
				if (locked && !string.IsNullOrEmpty(unlockCriteriaText))
				{
					lockedText = string.Format(unlockCriteriaText, unlockProgress, adventureDecks[i].UnlockValue);
				}
			}
			AdventureDeckDbfRecord deckRecord = adventureDecks[i];
			bool completed = deckRunWins?.Contains(deckRecord.DeckId) ?? false;
			bool newlyUnlocked = newlyUnlockedDecks?.Contains(deckRecord.DeckId) ?? false;
			AdventureDungeonCrawlDeckOption deckOption = m_deckOptions[i];
			deckOption.Init(adventureDecks[i], locked, lockedText, completed, newlyUnlocked, delegate
			{
				if (deckOption.IsNewlyUnlocked)
				{
					GameSaveDataManager.SubkeySaveRequest subkeySaveRequest = GameSaveDataManager.Get().GenerateSaveRequestToRemoveValueFromSubkeyIfItExists(adventureGameSaveClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_NEWLY_UNLOCKED_DECKS, deckRecord.DeckId);
					if (subkeySaveRequest != null)
					{
						Log.Adventures.Print("Deck {0} was Newly Unlocked but the player just acknowledged it, so saving that it is no longer Newly Unlocked.", deckRecord.DeckId);
						GameSaveDataManager.Get().SaveSubkey(subkeySaveRequest);
						deckOption.IsNewlyUnlocked = false;
					}
				}
			});
		}
		m_playButton.SetText("GLUE_CHOOSE");
		SetPlayMatStateAsInitializedAndPlayTransition();
	}

	private GameSaveDataManager.AdventureDungeonCrawlWingProgressSubkeys WingProgressSubkeysForScenario(int scenarioId)
	{
		GameSaveDataManager.GetProgressSubkeysForDungeonCrawlWing(GameUtils.GetWingRecordFromMissionId(scenarioId), out var progressSubkeys);
		return progressSubkeys;
	}

	private List<long> HeroPowerWinsForScenario(GameSaveKeyId adventureGameSaveServerKey, int scenarioId)
	{
		GameSaveDataManager.AdventureDungeonCrawlWingProgressSubkeys wingProgressSubkeys = WingProgressSubkeysForScenario(scenarioId);
		if (wingProgressSubkeys.heroPowerWins == (GameSaveKeySubkeyId)0)
		{
			return new List<long>();
		}
		GameSaveDataManager.Get().GetSubkeyValue(adventureGameSaveServerKey, wingProgressSubkeys.heroPowerWins, out List<long> heroPowerWins);
		return heroPowerWins;
	}

	private List<long> DeckWinsForScenario(GameSaveKeyId adventureGameSaveServerKey, int scenarioId)
	{
		GameSaveDataManager.AdventureDungeonCrawlWingProgressSubkeys wingProgressSubkeys = WingProgressSubkeysForScenario(scenarioId);
		if (wingProgressSubkeys.deckWins == (GameSaveKeySubkeyId)0)
		{
			return new List<long>();
		}
		GameSaveDataManager.Get().GetSubkeyValue(adventureGameSaveServerKey, wingProgressSubkeys.deckWins, out List<long> deckWins);
		return deckWins;
	}

	private List<long> TreasureWinsForScenario(GameSaveKeyId adventureGameSaveServerKey, int scenarioId)
	{
		GameSaveDataManager.AdventureDungeonCrawlWingProgressSubkeys wingProgressSubkeys = WingProgressSubkeysForScenario(scenarioId);
		if (wingProgressSubkeys.treasureWins == (GameSaveKeySubkeyId)0)
		{
			return new List<long>();
		}
		GameSaveDataManager.Get().GetSubkeyValue(adventureGameSaveServerKey, wingProgressSubkeys.treasureWins, out List<long> treasureWins);
		return treasureWins;
	}

	public void ShowNextBoss(string playButtonText)
	{
		SetPlayMatState(PlayMatState.SHOWING_NEXT_BOSS, setAsInitialized: true);
		m_playButton.SetText(playButtonText);
	}

	public void ShowEmptyState()
	{
		SetPlayMatState(PlayMatState.READY_FOR_DATA, setAsInitialized: true);
	}

	public void SetShouldShowBossHeroPowerTooltip(bool show)
	{
		m_shouldShowBossHeroPowerTooltip = show;
	}

	public void ShowBossHeroPowerTooltip()
	{
		StartCoroutine(ShowBossHeroPowerTooltipWhenReady());
	}

	private IEnumerator ShowBossHeroPowerTooltipWhenReady()
	{
		yield return new WaitForSeconds(0.5f);
		bool wasWaitingOnVO = false;
		while (NotificationManager.Get().IsQuotePlaying)
		{
			wasWaitingOnVO = true;
			yield return new WaitForEndOfFrame();
		}
		if (wasWaitingOnVO)
		{
			yield return new WaitForSeconds(m_bossHeroPowerTooltipDelayAfterVo);
		}
		if ((!(m_bossHeroPowerTooltip != null) || m_bossHeroPowerTooltip.IsDying()) && m_shouldShowBossHeroPowerTooltip)
		{
			m_bossHeroPowerTooltip = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, m_bossHeroPowerTooltipBone.transform.localPosition, m_bossHeroPowerTooltipBone.transform.localScale, GameStrings.Get(HERO_POWER_TOOLTIP_STRING));
			m_bossHeroPowerTooltip.ShowPopUpArrow(Notification.PopUpArrowDirection.Left);
			m_bossHeroPowerTooltip.PulseReminderEveryXSeconds(m_bossHeroPowerTooltipPulseRate);
		}
	}

	public void HideBossHeroPowerTooltip(bool immediate = false)
	{
		m_shouldShowBossHeroPowerTooltip = false;
		if (!(m_bossHeroPowerTooltip != null))
		{
			return;
		}
		if (immediate)
		{
			UnityEngine.Object.Destroy(m_bossHeroPowerTooltip.gameObject);
			m_bossHeroPowerTooltip = null;
			return;
		}
		Notification bossHeroPowerTooltip = m_bossHeroPowerTooltip;
		bossHeroPowerTooltip.OnFinishDeathState = (Action<int>)Delegate.Combine(bossHeroPowerTooltip.OnFinishDeathState, (Action<int>)delegate
		{
			m_bossHeroPowerTooltip = null;
		});
		m_bossHeroPowerTooltip.PlayDeath();
	}

	public PlayMatState GetPlayMatState()
	{
		return m_playMatState;
	}

	public OptionType GetPlayMatOptionType()
	{
		return m_currentOptionType;
	}

	private void SetPlayMatState(PlayMatState state, bool setAsInitialized)
	{
		if (PlayMatState.TRANSITIONING_FROM_PREV_STATE == m_playMatState && state != 0)
		{
			Log.Adventures.PrintError("Attempting to set Adventure Dungeon Crawl Play Mat to state {0}, but still in state TRANSITIONING_FROM_PREV_STATE! This is not allowed!", state);
			return;
		}
		Log.Adventures.Print("Setting Adventure Dungeon Crawl Play Mat to state {0}", state);
		m_playMatStateInitialized = false;
		if (PlayMatState.TRANSITIONING_FROM_PREV_STATE != state)
		{
			m_nextBossPane.SetActive(PlayMatState.SHOWING_NEXT_BOSS == state);
			m_optionsPane.SetActive(PlayMatState.SHOWING_OPTIONS == state);
			m_bossGraveyardPane.gameObject.SetActive(PlayMatState.SHOWING_BOSS_GRAVEYARD == state);
			SetHeaderTextForState(state);
		}
		if (PlayMatState.SHOWING_OPTIONS == state && m_selectedOptionClickBlocker != null)
		{
			m_selectedOptionClickBlocker.SetActive(value: true);
		}
		EnablePlayButton(enabled: false);
		if (m_playMatState != 0 && m_playMatState != PlayMatState.TRANSITIONING_FROM_PREV_STATE)
		{
			m_lastVisualPlayMatState = m_playMatState;
		}
		m_playMatState = state;
		if (setAsInitialized)
		{
			SetPlayMatStateAsInitializedAndPlayTransition();
		}
	}

	private void SetPlayMatStateAsInitializedAndPlayTransition()
	{
		m_playMatStateInitialized = true;
		if (m_subsceneTransitionComplete)
		{
			PlayStateTransition(m_playMatState);
		}
	}

	private void SetHeaderTextForState(PlayMatState state)
	{
		SetHeaderOverrideStrings();
		m_headerText.Show();
		switch (state)
		{
		case PlayMatState.SHOWING_OPTIONS:
			switch (m_currentOptionType)
			{
			case OptionType.TREASURE:
			{
				string treasureHeaderString = (string.IsNullOrEmpty(m_chooseTreasureHeaderStringOverride) ? "GLUE_ADVENTURE_DUNGEON_CRAWL_CHOOSE_TREASURE" : m_chooseTreasureHeaderStringOverride);
				m_headerText.Text = GameStrings.Get(treasureHeaderString);
				break;
			}
			case OptionType.LOOT:
			{
				string lootHeaderString = (string.IsNullOrEmpty(m_chooseLootHeaderStringOverride) ? "GLUE_ADVENTURE_DUNGEON_CRAWL_CHOOSE_LOOT" : m_chooseLootHeaderStringOverride);
				m_headerText.Text = GameStrings.Get(lootHeaderString);
				break;
			}
			case OptionType.SHRINE_TREASURE:
				m_headerText.Text = GameStrings.Get("GLUE_ADVENTURE_DUNGEON_CRAWL_CHOOSE_SHRINE");
				break;
			case OptionType.HERO_POWER:
				m_headerText.Text = GameStrings.Get("GLUE_ADVENTURE_DUNGEON_CRAWL_CHOOSE_HERO_POWER");
				break;
			case OptionType.DECK:
				m_headerText.Text = GameStrings.Get("GLUE_ADVENTURE_DUNGEON_CRAWL_CHOOSE_DECK");
				break;
			case OptionType.TREASURE_SATCHEL:
				m_headerText.Hide();
				break;
			}
			break;
		case PlayMatState.SHOWING_NEXT_BOSS:
			if (IsNextMissionASpecialEncounter)
			{
				m_headerText.Text = GameStrings.Get("GLUE_ADVENTURE_DUNGEON_CRAWL_SPECIAL_ENCOUNTER");
				break;
			}
			m_headerText.Text = GameStrings.Format("GLUE_ADVENTURE_DUNGEON_CRAWL_CHALLENGE_COUNT", m_numBossesDefeated + 1, m_bossesPerRun);
			break;
		}
	}

	public void ShowRunEnd(List<long> defeatedBossIds, long bossWhoDefeatedMeId, int numTotalBosses, bool hasCompletedAdventureWithAllClasses, bool firstTimeCompletedAsClass, int numClassesCompleted, GameSaveKeyId adventureGameSaveDataServerKey, GameSaveKeyId adventureGameSaveDataClientKey, AssetLoadCompletedCallback loadCompletedCallback, AdventureDungeonCrawlBossGraveyard.RunEndSequenceCompletedCallback sequenceCompletedCallback)
	{
		StartCoroutine(ShowRunEndAfterGraveyardIsInitialized(defeatedBossIds, bossWhoDefeatedMeId, numTotalBosses, hasCompletedAdventureWithAllClasses, firstTimeCompletedAsClass, numClassesCompleted, adventureGameSaveDataServerKey, adventureGameSaveDataClientKey, loadCompletedCallback, sequenceCompletedCallback));
	}

	public void OnSubSceneLoaded()
	{
		HideContentBeforeIntroAnims();
	}

	public void OnSubSceneTransitionComplete()
	{
		StartCoroutine(ProcessSubsceneTransitionCompleteWhenReady());
	}

	private IEnumerator ProcessSubsceneTransitionCompleteWhenReady()
	{
		while (GameUtils.IsAnyTransitionActive() || PopupDisplayManager.Get().IsShowing)
		{
			yield return null;
		}
		m_subsceneTransitionComplete = true;
		m_allowPlayButtonAnimation = true;
		if (m_bossGraveyard != null)
		{
			m_bossGraveyard.OnSubSceneTransitionComplete();
		}
		PlayStateTransition(m_playMatState);
	}

	public void PlayRewardOptionSelected(AdventureDungeonCrawlRewardOption.OptionData optionData)
	{
		SetPlayMatState(PlayMatState.TRANSITIONING_FROM_PREV_STATE, setAsInitialized: true);
		for (int i = 0; i < m_rewardOptions.Count; i++)
		{
			m_rewardOptions[i].DisableInteraction();
			m_rewardOptions[i].PlayOutro(optionData.index == i);
		}
	}

	public void PlayDeckOptionSelected()
	{
		PlayWidgetOptionSelected(m_deckOptions.Cast<AdventureOptionWidget>());
	}

	public void PlayHeroPowerOptionSelected()
	{
		PlayWidgetOptionSelected(m_heroPowerOptions.Cast<AdventureOptionWidget>());
	}

	public void PlayTreasureSatchelOptionSelected()
	{
		PlayWidgetOptionSelected(m_treasureSatchelOptions.Cast<AdventureOptionWidget>());
	}

	public void PlayTreasureSatchelOptionHidden()
	{
		PlayWidgetOptionSelected(m_treasureSatchelOptions.Cast<AdventureOptionWidget>());
		if (m_treasureSatchelWidget != null)
		{
			m_treasureSatchelWidget.TriggerEvent("PLAY_SATCHEL_MOTE_OUT");
		}
	}

	private void PlayWidgetOptionSelected(IEnumerable<AdventureOptionWidget> options)
	{
		SetPlayMatState(PlayMatState.TRANSITIONING_FROM_PREV_STATE, setAsInitialized: true);
		foreach (AdventureOptionWidget option in options)
		{
			option.PlayOutro();
		}
	}

	public Actor GetActorToAnimateFrom(string cardId, int index)
	{
		if (m_currentOptionType != OptionType.TREASURE_SATCHEL)
		{
			if (index < 0 || index > m_rewardOptions.Count)
			{
				return null;
			}
			return m_rewardOptions[index].GetActorFromCardId(cardId);
		}
		if (index < 0 || index > m_treasureSatchelOptions.Count)
		{
			return null;
		}
		return m_treasureSatchelOptions[index].CardActor;
	}

	private void PlayStateTransition(PlayMatState state)
	{
		if (m_playMatStateInitialized)
		{
			switch (state)
			{
			case PlayMatState.SHOWING_NEXT_BOSS:
				StartCoroutine(PlayNextBossAnimations(m_lastVisualPlayMatState == PlayMatState.SHOWING_OPTIONS));
				break;
			case PlayMatState.SHOWING_OPTIONS:
				StartCoroutine(HandleOptionIntroAnimations());
				break;
			}
		}
	}

	private IEnumerator PlayNextBossAnimations(bool transitionFromPrevState)
	{
		bool num = m_defeatedBossActors.Count == 0;
		bool finalBoss = m_defeatedBossActors.Count == m_bossesPerRun - 1;
		if (num)
		{
			m_allCards.SetActive(value: true);
			m_bossDeckDropAnimation.Play();
			SoundManager.Get().LoadAndPlay(m_bossDeckDropSFXOverride);
			while (m_bossDeckDropAnimation.isPlaying)
			{
				yield return null;
			}
			yield return new WaitForSeconds(m_delayAfterDeckDrop);
		}
		else if (transitionFromPrevState)
		{
			if (m_nextBossCardBack != null)
			{
				Actor cardBackActor = m_nextBossCardBack.GetComponent<Actor>();
				if (cardBackActor != null)
				{
					cardBackActor.ActivateSpellBirthState(SpellType.SUMMON_IN_DUNGEON_CRAWL);
					cardBackActor.ActivateSpellBirthState(DraftDisplay.GetSpellTypeForRarity(TAG_RARITY.RARE));
				}
			}
			if (m_topDefeatedBoss != null)
			{
				m_topDefeatedBoss.ActivateSpellBirthState(SpellType.SUMMON_IN_DUNGEON_CRAWL);
				m_topDefeatedBoss.ActivateSpellBirthState(DraftDisplay.GetSpellTypeForRarity(TAG_RARITY.RARE));
			}
			SoundManager.Get().LoadAndPlay(m_bossDeckMagicallyAppearSFXOverride);
			yield return new WaitForSeconds(0.7f);
		}
		m_nextBossFlipAnimation.Play(finalBoss ? m_nextBossFlipLargeName : m_nextBossFlipSmallName);
		SoundManager.Get().LoadAndPlay(finalBoss ? m_nextBossFlipLargeSFXOverride : m_nextBossFlipSmallSFXOverride);
		yield return new WaitForSeconds(m_nextBossFlipCrowdReactionDelay);
		string crowdReactionSFX = m_nextBossFlipCrowdReactionMediumSFXOverride;
		if (m_numBossesDefeated == m_bossesPerRun - 1)
		{
			crowdReactionSFX = m_nextBossFlipCrowdReactionLargeSFXOverride;
		}
		else if (m_numBossesDefeated <= 3)
		{
			crowdReactionSFX = m_nextBossFlipCrowdReactionSmallSFXOverride;
		}
		SoundManager.Get().LoadAndPlay(crowdReactionSFX);
		while (m_nextBossFlipAnimation.isPlaying)
		{
			yield return null;
		}
		EnablePlayButton(enabled: true);
		ShowBossHeroPowerTooltip();
		PlayNextBossVO();
	}

	private void PlayNextBossVO()
	{
		if (m_bossActor.GetEntityDef() != null)
		{
			WingDbId wingId = GameUtils.GetWingIdFromMissionId(m_dungeonCrawlData.GetMission());
			int bossID = GameUtils.TranslateCardIdToDbId(m_bossActor.GetEntityDef().GetCardId());
			if (m_numBossesDefeated + 1 < m_bossesPerRun || !DungeonCrawlSubDef_VOLines.PlayVOLine(m_dungeonCrawlData.GetSelectedAdventure(), wingId, m_playerHeroDbId, DungeonCrawlSubDef_VOLines.VOEventType.FINAL_BOSS_REVEAL, bossID, allowRepeatDuringSession: false))
			{
				DungeonCrawlSubDef_VOLines.PlayVOLine(m_dungeonCrawlData.GetSelectedAdventure(), wingId, m_playerHeroDbId, DungeonCrawlSubDef_VOLines.BOSS_REVEAL_EVENTS, bossID, allowRepeatDuringSession: false);
			}
		}
	}

	private IEnumerator HandleOptionIntroAnimations()
	{
		if (m_currentOptionType == OptionType.TREASURE || m_currentOptionType == OptionType.SHRINE_TREASURE)
		{
			yield return StartCoroutine(PlayRewardOptionAnimations(m_rewardOptions, 0f));
		}
		else if (m_currentOptionType == OptionType.LOOT)
		{
			List<AdventureDungeonCrawlRewardOption> sortedOptions = new List<AdventureDungeonCrawlRewardOption>(m_rewardOptions);
			if (m_rewardOptions.Count >= 2)
			{
				sortedOptions[0] = m_rewardOptions[1];
				sortedOptions[1] = m_rewardOptions[0];
			}
			yield return StartCoroutine(PlayRewardOptionAnimations(sortedOptions, m_lootDropDelay));
		}
		else if (m_currentOptionType == OptionType.HERO_POWER)
		{
			yield return StartCoroutine(PlayWidgetOptionAnimations(m_heroPowerOptions.Cast<AdventureOptionWidget>(), 0f));
		}
		else if (m_currentOptionType == OptionType.DECK)
		{
			yield return StartCoroutine(PlayWidgetOptionAnimations(m_deckOptions.Cast<AdventureOptionWidget>(), m_lootDropDelay));
		}
		else if (m_currentOptionType == OptionType.TREASURE_SATCHEL)
		{
			yield return StartCoroutine(PlayWidgetOptionAnimations(m_treasureSatchelOptions.Cast<AdventureOptionWidget>(), 0f));
		}
		if (m_selectedOptionClickBlocker != null)
		{
			m_selectedOptionClickBlocker.SetActive(value: false);
		}
		PlaySelectedOptionVO();
	}

	private IEnumerator PlayRewardOptionAnimations(IEnumerable<AdventureDungeonCrawlRewardOption> options, float dropDelay)
	{
		HideContentBeforeIntroAnims();
		yield return new WaitForSeconds(0.5f);
		foreach (AdventureDungeonCrawlRewardOption option in options)
		{
			while (!option.IsInitialized())
			{
				yield return null;
			}
		}
		foreach (AdventureDungeonCrawlRewardOption option3 in options)
		{
			option3.gameObject.SetActive(value: true);
			option3.PlayIntro();
			yield return new WaitForSeconds(dropDelay);
		}
		foreach (AdventureDungeonCrawlRewardOption option in options)
		{
			while (option.IntroIsPlaying())
			{
				yield return null;
			}
		}
		int i = 0;
		foreach (AdventureDungeonCrawlRewardOption option2 in options)
		{
			if (!option2.gameObject.activeInHierarchy)
			{
				Debug.LogWarning("AdventureDungeonCrawlPlayMat: The reward option at " + i + " was inactive when it was supposed to show");
				option2.gameObject.SetActive(value: true);
			}
			i++;
		}
	}

	private IEnumerator PlayWidgetOptionAnimations(IEnumerable<AdventureOptionWidget> options, float dropDelay)
	{
		foreach (AdventureOptionWidget option in options)
		{
			while (!option.IsReady)
			{
				yield return null;
			}
		}
		foreach (AdventureOptionWidget option2 in options)
		{
			option2.PlayIntro();
			if (dropDelay > 0f)
			{
				yield return new WaitForSeconds(dropDelay);
			}
		}
		foreach (AdventureOptionWidget option in options)
		{
			while (option.IsIntroPlaying)
			{
				yield return null;
			}
		}
	}

	private void PlaySelectedOptionVO()
	{
		if (m_currentOptionType == OptionType.TREASURE || m_currentOptionType == OptionType.SHRINE_TREASURE || m_currentOptionType == OptionType.TREASURE_SATCHEL)
		{
			PlayTreasureOfferVO();
		}
		else if (m_currentOptionType == OptionType.LOOT)
		{
			PlayLootPackOfferVO();
		}
		else if (m_currentOptionType == OptionType.HERO_POWER)
		{
			PlayHeroPowerOfferVO();
		}
		else if (m_currentOptionType == OptionType.DECK)
		{
			PlayDeckOfferVO();
		}
	}

	private void PlayTreasureOfferVO()
	{
		Options.Get().SetBool(Option.HAS_JUST_SEEN_LOOT_NO_TAKE_CANDLE_VO, val: false);
		WingDbId wingId = GameUtils.GetWingIdFromMissionId(m_dungeonCrawlData.GetMission());
		if (DungeonCrawlSubDef_VOLines.PlayVOLine(m_dungeonCrawlData.GetSelectedAdventure(), wingId, m_playerHeroDbId, DungeonCrawlSubDef_VOLines.OFFER_TREASURE_EVENTS))
		{
			return;
		}
		foreach (AdventureDungeonCrawlRewardOption rewardOption in m_rewardOptions)
		{
			int treasureID = rewardOption.GetTreasureDatabaseID();
			if (DungeonCrawlSubDef_VOLines.PlayVOLine(m_dungeonCrawlData.GetSelectedAdventure(), wingId, m_playerHeroDbId, DungeonCrawlSubDef_VOLines.OFFER_TREASURE_EVENTS, treasureID))
			{
				if (treasureID == 47251)
				{
					Options.Get().SetBool(Option.HAS_JUST_SEEN_LOOT_NO_TAKE_CANDLE_VO, val: true);
				}
				break;
			}
		}
	}

	private void PlayLootPackOfferVO()
	{
		WingDbId wingId = GameUtils.GetWingIdFromMissionId(m_dungeonCrawlData.GetMission());
		DungeonCrawlSubDef_VOLines.PlayVOLine(m_dungeonCrawlData.GetSelectedAdventure(), wingId, m_playerHeroDbId, DungeonCrawlSubDef_VOLines.OFFER_LOOT_PACKS_EVENTS);
	}

	private void PlayHeroPowerOfferVO()
	{
		WingDbId wingId = GameUtils.GetWingIdFromMissionId(m_dungeonCrawlData.GetMission());
		DungeonCrawlSubDef_VOLines.PlayVOLine(m_dungeonCrawlData.GetSelectedAdventure(), wingId, m_playerHeroDbId, DungeonCrawlSubDef_VOLines.OFFER_HERO_POWER_EVENTS, (int)m_dungeonCrawlData.SelectedHeroPowerDbId);
	}

	private void PlayDeckOfferVO()
	{
		WingDbId wingId = GameUtils.GetWingIdFromMissionId(m_dungeonCrawlData.GetMission());
		DungeonCrawlSubDef_VOLines.PlayVOLine(m_dungeonCrawlData.GetSelectedAdventure(), wingId, m_playerHeroDbId, DungeonCrawlSubDef_VOLines.OFFER_DECK_EVENTS, (int)m_dungeonCrawlData.SelectedDeckId);
	}

	private void HideContentBeforeIntroAnims()
	{
		if (m_playMatState != PlayMatState.SHOWING_OPTIONS)
		{
			return;
		}
		foreach (AdventureDungeonCrawlRewardOption rewardOption in m_rewardOptions)
		{
			if (rewardOption != null)
			{
				rewardOption.gameObject.SetActive(value: false);
			}
		}
	}

	private IEnumerator ShowRunEndAfterGraveyardIsInitialized(List<long> defeatedBossIds, long bossWhoDefeatedMeId, int numTotalBosses, bool hasCompletedAdventureWithAllClasses, bool firstTimeCompletedAsClass, int numClassesCompleted, GameSaveKeyId adventureGameSaveDataServerKey, GameSaveKeyId adventureGameSaveDataClientKey, AssetLoadCompletedCallback loadCompletedCallback, AdventureDungeonCrawlBossGraveyard.RunEndSequenceCompletedCallback sequenceCompletedCallback)
	{
		SetPlayMatState(PlayMatState.SHOWING_BOSS_GRAVEYARD, setAsInitialized: false);
		while (!m_bossGraveyardPane.PrefabIsLoaded() || m_paperController == null)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		if (m_bossGraveyard == null)
		{
			m_bossGraveyard = m_bossGraveyardPane.PrefabGameObject().GetComponent<AdventureDungeonCrawlBossGraveyard>();
			if (m_subsceneTransitionComplete)
			{
				m_bossGraveyard.OnSubSceneTransitionComplete();
			}
		}
		if (m_paperController != null && (bool)UniversalInputManager.UsePhoneUI)
		{
			m_paperController.gameObject.SetActive(value: false);
		}
		SetPlayMatStateAsInitializedAndPlayTransition();
		m_bossGraveyard.ShowRunEnd(m_dungeonCrawlData, defeatedBossIds, bossWhoDefeatedMeId, numTotalBosses, hasCompletedAdventureWithAllClasses, firstTimeCompletedAsClass, numClassesCompleted, m_playerHeroDbId, adventureGameSaveDataServerKey, adventureGameSaveDataClientKey, loadCompletedCallback, sequenceCompletedCallback);
	}

	private void SetPlaymatVisualStyle()
	{
		DungeonRunVisualStyle visualStyle = m_dungeonCrawlData.VisualStyle;
		m_nextBossFlipSmallSFXOverride = m_nextBossFlipSmallSFXDefault;
		m_nextBossFlipLargeSFXOverride = m_nextBossFlipLargeSFXDefault;
		m_nextBossFlipCrowdReactionSmallSFXOverride = m_nextBossFlipCrowdReactionSmallSFXDefault;
		m_nextBossFlipCrowdReactionMediumSFXOverride = m_nextBossFlipCrowdReactionMediumSFXDefault;
		m_nextBossFlipCrowdReactionLargeSFXOverride = m_nextBossFlipCrowdReactionLargeSFXDefault;
		m_bossDeckDropSFXOverride = m_bossDeckDropSFXDefault;
		m_bossDeckMagicallyAppearSFXOverride = m_bossDeckMagicallyAppearSFXDefault;
		foreach (PlaymatStyleOverride playmatStyle in m_playmatStyleOverride)
		{
			if (playmatStyle.VisualStyle == visualStyle)
			{
				m_matchingPlaymatStyle = playmatStyle;
				if ((bool)UniversalInputManager.UsePhoneUI)
				{
					m_headerText.TextColor = playmatStyle.PhoneHeaderTextColor;
					m_headerText.OutlineColor = playmatStyle.PhoneHeaderOutlineColor;
				}
				if (m_nextBossPlayNewParticlesScript != null)
				{
					m_nextBossPlayNewParticlesScript.m_Target = playmatStyle.NextBossDustEffectSmall.gameObject;
					m_nextBossPlayNewParticlesScript.m_Target2 = playmatStyle.NextBossDustEffectLargeMotes.gameObject;
					m_nextBossPlayNewParticlesScript.m_Target3 = playmatStyle.NextBossDustEffectLarge.gameObject;
				}
				if (m_facedownBossesPlayNewParticlesScript != null)
				{
					m_facedownBossesPlayNewParticlesScript.m_Target = playmatStyle.FacedownBossesDustEffect.gameObject;
				}
				if (!string.IsNullOrEmpty(playmatStyle.NextBossFlipSmallSFX))
				{
					m_nextBossFlipSmallSFXOverride = playmatStyle.NextBossFlipSmallSFX;
				}
				if (!string.IsNullOrEmpty(playmatStyle.NextBossFlipLargeSFX))
				{
					m_nextBossFlipLargeSFXOverride = playmatStyle.NextBossFlipLargeSFX;
				}
				if (!string.IsNullOrEmpty(playmatStyle.NextBossFlipCrowdReactionSmallSFX))
				{
					m_nextBossFlipCrowdReactionSmallSFXOverride = playmatStyle.NextBossFlipCrowdReactionSmallSFX;
				}
				if (!string.IsNullOrEmpty(playmatStyle.NextBossFlipCrowdReactionMediumSFX))
				{
					m_nextBossFlipCrowdReactionMediumSFXOverride = playmatStyle.NextBossFlipCrowdReactionMediumSFX;
				}
				if (!string.IsNullOrEmpty(playmatStyle.NextBossFlipCrowdReactionLargeSFX))
				{
					m_nextBossFlipCrowdReactionLargeSFXOverride = playmatStyle.NextBossFlipCrowdReactionLargeSFX;
				}
				if (!string.IsNullOrEmpty(playmatStyle.BossDeckDropSFX))
				{
					m_bossDeckDropSFXOverride = playmatStyle.BossDeckDropSFX;
				}
				if (!string.IsNullOrEmpty(playmatStyle.BossDeckMagicallyAppearSFX))
				{
					m_bossDeckMagicallyAppearSFXOverride = playmatStyle.BossDeckMagicallyAppearSFX;
				}
				break;
			}
		}
	}

	private void SetHeaderOverrideStrings()
	{
		if (m_matchingPlaymatStyle == null)
		{
			return;
		}
		HeaderStringOverride chooseTreasureOverride = null;
		HeaderStringOverride chooseLootOverride = null;
		if (m_matchingPlaymatStyle.ChooseTreasureHeaderString.Any())
		{
			chooseTreasureOverride = m_matchingPlaymatStyle.ChooseTreasureHeaderString.OrderByDescending((HeaderStringOverride s) => s.MinimumDefeatedBosses).First((HeaderStringOverride s) => s.MinimumDefeatedBosses <= m_numBossesDefeated);
		}
		if (m_matchingPlaymatStyle.ChooseLootHeaderString.Any())
		{
			chooseLootOverride = m_matchingPlaymatStyle.ChooseLootHeaderString.OrderByDescending((HeaderStringOverride s) => s.MinimumDefeatedBosses).First((HeaderStringOverride s) => s.MinimumDefeatedBosses <= m_numBossesDefeated);
		}
		if (chooseTreasureOverride != null)
		{
			m_chooseTreasureHeaderStringOverride = chooseTreasureOverride.HeaderString;
		}
		if (chooseLootOverride != null)
		{
			m_chooseLootHeaderStringOverride = chooseLootOverride.HeaderString;
		}
	}

	private void OnPlayButtonReady(PlayButton playButton)
	{
		if (playButton == null)
		{
			Error.AddDevWarning("UI Error!", "PlayButtonReference is null, or does not have a PlayButton component on it!");
		}
		else
		{
			m_playButton = playButton;
		}
	}

	private void OnPaperControllerReady(VisualController paperController)
	{
		if (paperController == null)
		{
			Error.AddDevWarning("UI Issue!", "PlayMat's m_paperControllerReference is null! Can't set the correct PlayMat texture!.");
			return;
		}
		if (m_dungeonCrawlData == null)
		{
			Error.AddDevWarning("UI Issue!", "PlayMat's m_dungeonCrawlData is null! Can't set the correct PlayMat texture!.");
			return;
		}
		m_paperController = paperController;
		int scenarioId = (int)m_dungeonCrawlData.GetMission();
		WingDbfRecord wingRecord = GameUtils.GetWingRecordFromMissionId(scenarioId);
		if (wingRecord == null)
		{
			Log.Adventures.PrintError("No WingDbfRecord found for ScenarioDbId {0}!", scenarioId);
		}
		else
		{
			paperController.SetState(wingRecord.VisualStateName);
		}
	}

	private void OnTreasureSatchelReady(Widget widget)
	{
		if (widget == null)
		{
			Debug.LogError("AdventureDungeonCrawlPlayMat.OnTreasureSatchelReady - widget was null!");
			return;
		}
		AdventureTreasureSatchelDataModel satchelDataModel = new AdventureTreasureSatchelDataModel();
		m_treasureSatchelOptions = new List<AdventureDungeonCrawlTreasureOption>(widget.GetComponentsInChildren<AdventureDungeonCrawlTreasureOption>());
		foreach (AdventureDungeonCrawlTreasureOption option in m_treasureSatchelOptions)
		{
			satchelDataModel.LoadoutOptions.Add(option.GetDataModel());
		}
		widget.BindDataModel(satchelDataModel);
		widget.RegisterEventListener(delegate(string eventName)
		{
			if (eventName == "CODE_TREASURE_SATCHEL_OUTRO_COMPLETE")
			{
				widget.Hide();
			}
			else if (eventName == "CODE_TREASURE_OPTION_SELECTED")
			{
				if (m_treasureInspectWidget == null || !m_treasureInspectWidget.GetDataModel(27, out var model))
				{
					Debug.LogError("AdventureDungeonCrawlPlayMat.OnTreasureSatchelReady - selected event called with no CardDataModel found or treasure inspect widget didn't load!");
				}
				else if (!(model is CardDataModel))
				{
					Debug.LogError("AdventureDungeonCrawlPlayMat.OnTreasureSatchelReady - selected event called but CardDataModel was null!");
				}
				else
				{
					EventDataModel dataModel = widget.GetDataModel<EventDataModel>();
					int num = 0;
					if (dataModel.Payload is IConvertible)
					{
						num = Convert.ToInt32(dataModel.Payload);
					}
					for (int i = 0; i < m_treasureSatchelOptions.Count; i++)
					{
						AdventureDungeonCrawlTreasureOption adventureDungeonCrawlTreasureOption = m_treasureSatchelOptions[i];
						if (i == num)
						{
							adventureDungeonCrawlTreasureOption.Select();
						}
						else
						{
							adventureDungeonCrawlTreasureOption.Deselect();
						}
					}
				}
			}
		});
	}

	private void OnTreasureInspectReady(Widget widget)
	{
		if (widget == null)
		{
			Debug.LogError("AdventureDungeonCrawlPlayMat.OnTreasureSatchelReady - widget was null!");
		}
		else
		{
			m_treasureInspectWidget = widget;
		}
	}

	public bool IsPaperControllerReady()
	{
		if (m_paperController != null)
		{
			return !m_paperController.IsChangingStates;
		}
		return false;
	}
}
