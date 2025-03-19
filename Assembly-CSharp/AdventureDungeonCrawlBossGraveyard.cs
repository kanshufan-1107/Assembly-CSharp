using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hearthstone.DungeonCrawl;
using UnityEngine;

[CustomEditClass]
public class AdventureDungeonCrawlBossGraveyard : MonoBehaviour
{
	public delegate void RunEndSequenceCompletedCallback();

	private const int MAX_GRAVEYARD_BOSSES_TO_SHOW = 8;

	private const float CHANCE_TO_PLAY_RARE_DEFEAT_LINE = 0.2f;

	[CustomEditField(Sections = "UI")]
	public NestedPrefab m_bossArchNestedPrefab;

	[CustomEditField(Sections = "UI")]
	public float m_bossArchSpacingHorizontal;

	[CustomEditField(Sections = "UI")]
	public float m_bossArchSpacingVertical;

	[CustomEditField(Sections = "UI")]
	public int m_bossesPerRow = 4;

	[CustomEditField(Sections = "UI")]
	public UberText m_defeatedCount;

	[CustomEditField(Sections = "Animations")]
	public string m_bossFlipSmallAnimName;

	[CustomEditField(Sections = "Animations")]
	public string m_bossFlipLargeAnimName;

	[CustomEditField(Sections = "Animations")]
	public string m_bossFlipNoDesaturateAnimName;

	[CustomEditField(Sections = "Animations")]
	public float m_delayPerBossFlip = 0.63f;

	[CustomEditField(Sections = "Animations")]
	public float m_delayAfterBossFlips = 1.5f;

	[CustomEditField(Sections = "Animations")]
	public float m_delayBeforeRunWinVO;

	[CustomEditField(Sections = "SFX", T = EditType.SOUND_PREFAB)]
	public string m_bossFlipSmallSFX;

	[CustomEditField(Sections = "SFX", T = EditType.SOUND_PREFAB)]
	public string m_bossFlipLargeSFX;

	[CustomEditField(Sections = "SFX", T = EditType.SOUND_PREFAB)]
	public string m_victorySequenceStartSFX;

	[CustomEditField(Sections = "SFX", T = EditType.SOUND_PREFAB)]
	public string m_defeatSequenceStartSFX;

	[CustomEditField(Sections = "Rewards")]
	public GameObject m_rewardPopupContainer;

	private List<AdventureDungeonCrawlBossGraveyardActor> m_bossArches = new List<AdventureDungeonCrawlBossGraveyardActor>();

	private Actor m_bossLostToActor;

	private bool m_subsceneTransitionComplete;

	private bool m_emoteLoadingComplete;

	private EmoteEntryDef m_bossLostToEmoteDef;

	private CardSoundSpell m_bossLostToEmoteSoundSpell;

	private bool m_runCompleteSequenceSeen;

	private List<AdventureHeroPowerDbfRecord> m_justUnlockedHeroPowers;

	private List<AdventureDeckDbfRecord> m_justUnlockedDecks;

	private List<AdventureLoadoutTreasuresDbfRecord> m_justUnlockedLoadoutTreasures;

	private List<AdventureLoadoutTreasuresDbfRecord> m_justUpgradedLoadoutTreasures;

	private IDungeonCrawlData m_dungeonCrawlData;

	private ScreenEffectsHandle m_screenEffectsHandle;

	private void Start()
	{
		m_bossArches.Add(m_bossArchNestedPrefab.PrefabGameObject().GetComponent<AdventureDungeonCrawlBossGraveyardActor>());
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	private void Update()
	{
		if (Application.isEditor)
		{
			UpdateLayout();
		}
	}

	private void OnDestroy()
	{
		if (FullScreenFXMgr.Get() != null)
		{
			m_screenEffectsHandle.StopEffect();
		}
	}

	private void OnBonusChallengeUnlockObjectLoaded(Reward reward, object callbackData)
	{
		GameUtils.SetParent(reward, m_rewardPopupContainer);
		reward.Show(updateCacheValues: false);
	}

	private IEnumerator PlayBossFlippingSequence(int numBossesToShow, bool defeatedLastBoss)
	{
		float flipDelayTime = m_delayPerBossFlip;
		for (int i = 0; i < numBossesToShow; i++)
		{
			bool lastBoss = i == numBossesToShow - 1;
			Animator flipAnim = m_bossArches[i].GetComponent<Animator>();
			if (flipAnim != null)
			{
				string stateName = ((!lastBoss) ? m_bossFlipSmallAnimName : (defeatedLastBoss ? m_bossFlipLargeAnimName : m_bossFlipNoDesaturateAnimName));
				flipAnim.Play(stateName);
				SoundManager.Get().LoadAndPlay(lastBoss ? m_bossFlipLargeSFX : m_bossFlipSmallSFX);
				yield return new WaitForSeconds(flipDelayTime);
				flipDelayTime *= 0.9f;
				if (flipDelayTime < 0.1f)
				{
					flipDelayTime = 0.1f;
				}
			}
		}
		yield return new WaitForSeconds(m_delayAfterBossFlips);
	}

	private IEnumerator PlayDefeatSequence(int numBossesToShow, int numDefeatedBosses, int numTotalBosses, int bossWhoDefeatedMeId, int heroDbId, GameSaveKeyId adventureServerKeyId, RunEndSequenceCompletedCallback completedCallback)
	{
		if (m_bossLostToActor == null)
		{
			Log.Adventures.PrintError("AdventureDungeonCrawlBossGraveyard.PlayDefeatSequence() - Can't PlayDefeatSequence() without a m_bossLostToActor!");
			yield break;
		}
		bool bossHasEmote = LoadBossLostToEmote();
		if (!bossHasEmote)
		{
			Log.Adventures.Print("No EmoteDef set for DUNGEON_CRAWL_DEFEAT_TAUNT for boss {0}.", m_bossLostToActor.CardDefName);
		}
		while (!m_subsceneTransitionComplete || GameUtils.IsAnyTransitionActive())
		{
			yield return null;
		}
		VignetteParameters? vignette = VignetteParameters.Default;
		ScreenEffectParameters screenEffectParameters = new ScreenEffectParameters(ScreenEffectType.VIGNETTE, ScreenEffectPassLocation.PERSPECTIVE, 0.5f, iTween.EaseType.easeOutCirc, null, vignette, null, null);
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
		if (!string.IsNullOrEmpty(m_defeatSequenceStartSFX))
		{
			SoundManager.Get().LoadAndPlay(m_defeatSequenceStartSFX);
		}
		yield return StartCoroutine(PlayBossFlippingSequence(numBossesToShow, defeatedLastBoss: false));
		if (bossHasEmote)
		{
			while (!m_emoteLoadingComplete)
			{
				yield return null;
			}
			Notification notification = NotificationManager.Get().CreateSpeechBubble(GameStrings.Get(m_bossLostToEmoteDef.m_emoteGameStringKey), m_bossLostToActor);
			if (m_bossLostToEmoteSoundSpell == null)
			{
				NotificationManager.Get().DestroyNotification(notification, 5f);
			}
			else
			{
				m_bossLostToEmoteSoundSpell.AddFinishedCallback(delegate
				{
					NotificationManager.Get().DestroyNotification(notification, 0f);
					Object.Destroy(m_bossLostToEmoteSoundSpell.gameObject);
				});
				m_bossLostToEmoteSoundSpell.Reactivate();
			}
			while (notification != null)
			{
				yield return null;
			}
		}
		bool hasPlayedVO = false;
		WingDbId wingId = GameUtils.GetWingIdFromMissionId(m_dungeonCrawlData.GetMission());
		if (numDefeatedBosses >= numTotalBosses - 1)
		{
			hasPlayedVO = DungeonCrawlSubDef_VOLines.PlayVOLine(m_dungeonCrawlData.GetSelectedAdventure(), wingId, heroDbId, DungeonCrawlSubDef_VOLines.FINAL_BOSS_LOSS_EVENTS, bossWhoDefeatedMeId);
		}
		if (!hasPlayedVO)
		{
			DungeonCrawlSubDef_VOLines.PlayVOLine(m_dungeonCrawlData.GetSelectedAdventure(), wingId, heroDbId, DungeonCrawlSubDef_VOLines.VOEventType.BOSS_LOSS_1, bossWhoDefeatedMeId);
		}
		m_screenEffectsHandle.StopEffect();
		if (!HasNewlyUnlockedGSDRewardsToShow())
		{
			yield break;
		}
		PopupDisplayManager.Get().RewardPopups.ShowRewardsForAdventureUnlocks(m_justUnlockedHeroPowers, m_justUnlockedDecks, m_justUnlockedLoadoutTreasures, m_justUpgradedLoadoutTreasures, delegate
		{
			if (!m_runCompleteSequenceSeen)
			{
				MarkRunCompleteSequenceAsSeen(adventureServerKeyId, completedCallback);
			}
		});
	}

	private static void PlayVictoryVO(IDungeonCrawlData dungeonCrawlData, bool hasCompletedAdventureWithAllClasses, bool hasSeenCompleteWithAllClassesFirstTime, bool firstTimeCompletedAsClass, int numClassesCompleted, int heroDbId)
	{
		AdventureDbId adventureDbId = dungeonCrawlData.GetSelectedAdventure();
		GameSaveKeyId adventureGameSaveClientKey = dungeonCrawlData.GetGameSaveClientKey();
		WingDbId wingId = GameUtils.GetWingIdFromMissionId(dungeonCrawlData.GetMission());
		bool hasPlayedVO = false;
		if (hasCompletedAdventureWithAllClasses)
		{
			if (!hasSeenCompleteWithAllClassesFirstTime)
			{
				hasPlayedVO = hasPlayedVO || DungeonCrawlSubDef_VOLines.PlayVOLine(adventureDbId, wingId, heroDbId, DungeonCrawlSubDef_VOLines.VOEventType.COMPLETE_ALL_CLASSES_FIRST_TIME);
				GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(adventureGameSaveClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_COMPLETE_ALL_CLASSES_VO, 1L));
			}
			else
			{
				hasPlayedVO = hasPlayedVO || DungeonCrawlSubDef_VOLines.PlayVOLine(adventureDbId, wingId, heroDbId, DungeonCrawlSubDef_VOLines.VOEventType.COMPLETE_ALL_CLASSES, 0, allowRepeatDuringSession: false);
			}
		}
		if (!hasPlayedVO && firstTimeCompletedAsClass && numClassesCompleted > 0)
		{
			int voIndex = numClassesCompleted - 1;
			if (voIndex < DungeonCrawlSubDef_VOLines.CLASS_COMPLETE_EVENTS.Length)
			{
				hasPlayedVO = hasPlayedVO || DungeonCrawlSubDef_VOLines.PlayVOLine(adventureDbId, wingId, heroDbId, DungeonCrawlSubDef_VOLines.CLASS_COMPLETE_EVENTS[voIndex], 0, allowRepeatDuringSession: false);
			}
		}
		if (!hasPlayedVO)
		{
			if (!GameSaveDataManager.Get().GetSubkeyValue(adventureGameSaveClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_WING_COMPLETE_VO, out List<long> wingsSeen))
			{
				wingsSeen = new List<long>();
			}
			bool num = wingsSeen.Contains((long)wingId);
			int voIndex2 = wingsSeen.Count;
			if (!num && voIndex2 < DungeonCrawlSubDef_VOLines.WING_COMPLETE_EVENTS.Length)
			{
				hasPlayedVO = hasPlayedVO || DungeonCrawlSubDef_VOLines.PlayVOLine(adventureDbId, wingId, heroDbId, DungeonCrawlSubDef_VOLines.WING_COMPLETE_EVENTS[voIndex2], 0, allowRepeatDuringSession: false);
				if (hasPlayedVO)
				{
					wingsSeen.Add((long)wingId);
					GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(adventureGameSaveClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_WING_COMPLETE_VO, wingsSeen.ToArray()));
				}
			}
		}
		if (!hasPlayedVO)
		{
			hasPlayedVO = hasPlayedVO || DungeonCrawlSubDef_VOLines.PlayVOLine(adventureDbId, wingId, heroDbId, DungeonCrawlSubDef_VOLines.VOEventType.WING_COMPLETE_GENERAL, 0, allowRepeatDuringSession: false);
		}
		if (!hasPlayedVO)
		{
			AdventureWingDef wingDef = dungeonCrawlData.GetWingDef(wingId);
			if (AdventureUtils.CanPlayWingCompleteQuote(wingDef))
			{
				string gameString = new AssetReference(wingDef.m_CompleteQuoteVOLine).GetLegacyAssetName();
				NotificationManager.Get().CreateCharacterQuote(wingDef.m_CompleteQuotePrefab, GameStrings.Get(gameString), wingDef.m_CompleteQuoteVOLine, allowRepeatDuringSession: false);
			}
		}
	}

	private IEnumerator PlayVictorySequence(int numBossesToShow, bool hasCompletedAdventureWithAllClasses, bool firstTimeCompletedAsClass, int numClassesCompleted, int heroDbId, RunEndSequenceCompletedCallback completedCallback)
	{
		while (!m_subsceneTransitionComplete || GameUtils.IsAnyTransitionActive())
		{
			yield return null;
		}
		VignetteParameters? vignette = VignetteParameters.Default;
		ScreenEffectParameters screenEffectParameters = new ScreenEffectParameters(ScreenEffectType.VIGNETTE, ScreenEffectPassLocation.PERSPECTIVE, 0.5f, iTween.EaseType.easeOutCirc, null, vignette, null, null);
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
		if (!string.IsNullOrEmpty(m_victorySequenceStartSFX))
		{
			SoundManager.Get().LoadAndPlay(m_victorySequenceStartSFX);
		}
		yield return StartCoroutine(PlayBossFlippingSequence(numBossesToShow, defeatedLastBoss: true));
		AdventureDbId adventureDbId = m_dungeonCrawlData.GetSelectedAdventure();
		AdventureModeDbId adventureModeDbId = m_dungeonCrawlData.GetSelectedMode();
		AdventureDef advDef = m_dungeonCrawlData.GetAdventureDef();
		AdventureSubDef advSubDef = ((advDef == null) ? null : advDef.GetSubDef(adventureModeDbId));
		if (advDef != null && !string.IsNullOrEmpty(advDef.m_BannerRewardPrefab))
		{
			TAG_CLASS heroClass = GameUtils.GetTagClassFromCardDbId(heroDbId);
			string bannerWithClassName = GameStrings.FormatLocalizedString(advSubDef.GetCompleteBannerText(), GetClassNameFromTagClass(heroClass));
			BannerManager.Get().ShowBanner(advDef.m_BannerRewardPrefab, null, bannerWithClassName);
		}
		yield return new WaitForSeconds(m_delayBeforeRunWinVO);
		GameSaveKeyId adventureGameSaveClientKey = m_dungeonCrawlData.GetGameSaveClientKey();
		GameSaveDataManager.Get().GetSubkeyValue(adventureGameSaveClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_COMPLETE_ALL_CLASSES_VO, out long hasSeenCompleteWithAllClassesFirstTimeLong);
		bool hasSeenCompleteWithAllClassesFirstTime = hasSeenCompleteWithAllClassesFirstTimeLong == 1;
		PlayVictoryVO(m_dungeonCrawlData, hasCompletedAdventureWithAllClasses, hasSeenCompleteWithAllClassesFirstTime, firstTimeCompletedAsClass, numClassesCompleted, heroDbId);
		m_screenEffectsHandle.StopEffect();
		PopupDisplayManager.Get().RewardPopups.ShowRewardsForAdventureUnlocks(m_justUnlockedHeroPowers, m_justUnlockedDecks, m_justUnlockedLoadoutTreasures, m_justUpgradedLoadoutTreasures, delegate
		{
			AdventureDataDbfRecord adventureDataRecord = GameUtils.GetAdventureDataRecord((int)adventureDbId, (int)adventureModeDbId);
			ShowAdditionalPopupsAfterOutstandingPopups(hasCompletedAdventureWithAllClasses, hasSeenCompleteWithAllClassesFirstTime, adventureDataRecord, completedCallback);
		});
	}

	private void MarkRunCompleteSequenceAsSeen(GameSaveKeyId adventureServerKeyId, RunEndSequenceCompletedCallback completedCallback)
	{
		m_runCompleteSequenceSeen = true;
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(adventureServerKeyId, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_LATEST_DUNGEON_RUN_COMPLETE, 1L));
		completedCallback();
	}

	private void ShowAdditionalPopupsAfterOutstandingPopups(bool hasCompletedAdventureWithAllClasses, bool hasSeenCompleteWithAllClassesFirstTime, AdventureDataDbfRecord adventureDataRecord, RunEndSequenceCompletedCallback completedCallback)
	{
		if (hasCompletedAdventureWithAllClasses && !hasSeenCompleteWithAllClassesFirstTime)
		{
			string shownPrefab = adventureDataRecord.PrefabShownOnComplete;
			if (!string.IsNullOrEmpty(shownPrefab))
			{
				new BonusChallengeUnlockData(shownPrefab, adventureDataRecord.DungeonCrawlBossCardPrefab).LoadRewardObject(delegate(Reward reward, object data)
				{
					reward.RegisterHideListener(delegate
					{
						MarkRunCompleteSequenceAsSeen((GameSaveKeyId)adventureDataRecord.GameSaveDataServerKey, completedCallback);
						Navigation.GoBack();
						m_dungeonCrawlData.SetSelectedAdventureMode((AdventureDbId)adventureDataRecord.AdventureId, AdventureModeDbId.BONUS_CHALLENGE);
					});
					OnBonusChallengeUnlockObjectLoaded(reward, data);
				});
				return;
			}
		}
		MarkRunCompleteSequenceAsSeen((GameSaveKeyId)adventureDataRecord.GameSaveDataServerKey, completedCallback);
	}

	private string GetClassNameFromTagClass(TAG_CLASS deckClass)
	{
		List<GuestHero> adventurers = m_dungeonCrawlData.GetGuestHeroesForCurrentAdventure();
		if (adventurers.Count > 0)
		{
			foreach (GuestHero hero in adventurers)
			{
				if (GameUtils.GetTagClassFromCardDbId(hero.cardDbId) == deckClass)
				{
					GuestHeroDbfRecord cardAdventurerRecord = GameDbf.GuestHero.GetRecord((GuestHeroDbfRecord r) => r.CardId == hero.cardDbId);
					if (cardAdventurerRecord != null)
					{
						return cardAdventurerRecord.Name;
					}
				}
			}
		}
		return GameStrings.GetClassName(deckClass);
	}

	private void DisableBoss(Actor boss)
	{
		boss.transform.Rotate(new Vector3(0f, 0f, 180f));
	}

	private bool LoadBossLostToEmote()
	{
		if (!m_bossLostToActor.HasCardDef || m_bossLostToActor.EmoteDefs == null)
		{
			Log.Adventures.PrintWarning("AdventureDungeonCrawlBossGraveyard.PlayDefeatSequence() - No cardDef found for the boss you lost to!");
			return false;
		}
		m_bossLostToEmoteDef = m_bossLostToActor.EmoteDefs.Find((EmoteEntryDef e) => e.m_emoteType == EmoteType.DUNGEON_CRAWL_DEFEAT_TAUNT);
		if (m_bossLostToEmoteDef == null)
		{
			return false;
		}
		EmoteEntryDef superRareDef = m_bossLostToActor.EmoteDefs.Find((EmoteEntryDef e) => e.m_emoteType == EmoteType.DUNGEON_CRAWL_DEFEAT_TAUNT_SUPER_RARE);
		if (superRareDef != null && Random.Range(0f, 1f) < 0.2f)
		{
			m_bossLostToEmoteDef = superRareDef;
		}
		AssetLoader.Get().InstantiatePrefab(m_bossLostToEmoteDef.m_emoteSoundSpellPath, delegate(AssetReference assetRef, GameObject go, object callbackData)
		{
			m_emoteLoadingComplete = true;
			if (go == null)
			{
				Log.Adventures.PrintError("AdventureDungeonCrawlBossGraveyard.PlayDefeatSequence() - Failed to load CardSoundSpell at path {0}!", m_bossLostToEmoteDef.m_emoteSoundSpellPath);
			}
			else
			{
				m_bossLostToEmoteSoundSpell = go.GetComponent<CardSoundSpell>();
			}
		});
		return true;
	}

	private void UpdateLayout()
	{
		Vector3 startingPos = m_bossArches[0].transform.localPosition;
		for (int i = 1; i < m_bossArches.Count; i++)
		{
			m_bossArches[i].transform.localPosition = new Vector3(startingPos.x + m_bossArchSpacingHorizontal * (float)(i % m_bossesPerRow), startingPos.y, startingPos.z + m_bossArchSpacingVertical * (float)(i / m_bossesPerRow));
		}
	}

	private void CheckForNewlyUnlockedAdventureRewards(GameSaveKeyId gameSaveServerKey, GameSaveKeyId gameSaveClientKey, int heroCardDbId)
	{
		List<GameSaveDataManager.SubkeySaveRequest> subkeyUpdates = new List<GameSaveDataManager.SubkeySaveRequest>();
		List<AdventureHeroPowerDbfRecord> adventureHeroPowers = AdventureUtils.GetHeroPowersForDungeonCrawlHero(m_dungeonCrawlData, heroCardDbId);
		List<AdventureLoadoutTreasuresDbfRecord> adventureLoadoutTreasures = AdventureUtils.GetTreasuresForDungeonCrawlHero(m_dungeonCrawlData, heroCardDbId);
		m_justUnlockedHeroPowers = DungeonCrawlUtil.CheckForNewlyUnlockedGSDRewardsOfSpecificType(adventureHeroPowers.Cast<DbfRecord>(), m_dungeonCrawlData, gameSaveServerKey, gameSaveClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_AWARDED_HERO_POWERS, GameSaveKeySubkeyId.DUNGEON_CRAWL_NEWLY_UNLOCKED_HERO_POWERS, subkeyUpdates).Cast<AdventureHeroPowerDbfRecord>().ToList();
		TAG_CLASS heroClass = AdventureUtils.GetHeroClassFromHeroId(heroCardDbId);
		List<AdventureDeckDbfRecord> adventureDecks = m_dungeonCrawlData.GetDecksForClass(heroClass);
		m_justUnlockedDecks = DungeonCrawlUtil.CheckForNewlyUnlockedGSDRewardsOfSpecificType(adventureDecks.Cast<DbfRecord>(), m_dungeonCrawlData, gameSaveServerKey, gameSaveClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_AWARDED_DECKS, GameSaveKeySubkeyId.DUNGEON_CRAWL_NEWLY_UNLOCKED_DECKS, subkeyUpdates).Cast<AdventureDeckDbfRecord>().ToList();
		m_justUnlockedLoadoutTreasures = DungeonCrawlUtil.CheckForNewlyUnlockedGSDRewardsOfSpecificType(adventureLoadoutTreasures.Cast<DbfRecord>(), m_dungeonCrawlData, gameSaveServerKey, gameSaveClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_AWARDED_LOADOUT_TREASURES, GameSaveKeySubkeyId.DUNGEON_CRAWL_NEWLY_UNLOCKED_LOADOUT_TREASURES, subkeyUpdates).Cast<AdventureLoadoutTreasuresDbfRecord>().ToList();
		m_justUpgradedLoadoutTreasures = DungeonCrawlUtil.CheckForNewlyUnlockedGSDRewardsOfSpecificType(adventureLoadoutTreasures.Cast<DbfRecord>(), m_dungeonCrawlData, gameSaveServerKey, gameSaveClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_AWARDED_LOADOUT_TREASURES, GameSaveKeySubkeyId.DUNGEON_CRAWL_NEWLY_UNLOCKED_LOADOUT_TREASURES, subkeyUpdates, checkForUpgrades: true).Cast<AdventureLoadoutTreasuresDbfRecord>().ToList();
		if (subkeyUpdates.Count > 0)
		{
			GameSaveDataManager.Get().SaveSubkeys(subkeyUpdates);
		}
	}

	private bool HasNewlyUnlockedGSDRewardsToShow()
	{
		if ((m_justUnlockedHeroPowers == null || m_justUnlockedHeroPowers.Count <= 0) && (m_justUnlockedDecks == null || m_justUnlockedDecks.Count <= 0) && (m_justUnlockedLoadoutTreasures == null || m_justUnlockedLoadoutTreasures.Count <= 0))
		{
			if (m_justUpgradedLoadoutTreasures != null)
			{
				return m_justUpgradedLoadoutTreasures.Count > 0;
			}
			return false;
		}
		return true;
	}

	public void OnSubSceneTransitionComplete()
	{
		m_subsceneTransitionComplete = true;
	}

	public void ShowRunEnd(IDungeonCrawlData dungeonCrawlData, List<long> defeatedBossIds, long bossWhoDefeatedMeId, int numTotalBosses, bool hasCompletedAdventureWithAllClasses, bool firstTimeCompletedAsClass, int numClassesCompleted, int heroDbId, GameSaveKeyId adventureGameSaveServerKey, GameSaveKeyId adventureGameSaveClientKey, AdventureDungeonCrawlPlayMat.AssetLoadCompletedCallback loadCompletedCallback, RunEndSequenceCompletedCallback completedCallback)
	{
		if (dungeonCrawlData == null)
		{
			Log.Adventures.PrintError("Error!  AdventureDungeonCrawlBossGraveyard.ShowRunEnd() called with null dungeonCrawlData!)");
			return;
		}
		m_dungeonCrawlData = dungeonCrawlData;
		if (m_bossArches.Count < 1)
		{
			Log.Adventures.PrintError("Error!  AdventureDungeonCrawlBossGraveyard.ShowRunEnd() called when m_bossArches is empty! (Probably because Start() has not yet executed.)");
			return;
		}
		CheckForNewlyUnlockedAdventureRewards(adventureGameSaveServerKey, adventureGameSaveClientKey, heroDbId);
		int numDefeated = defeatedBossIds?.Count ?? 0;
		bool isDefeat = numDefeated < numTotalBosses;
		int numBossesToShow = Mathf.Min(8, numDefeated + (isDefeat ? 1 : 0));
		int numBossesOffset = Mathf.Max(0, numDefeated - numBossesToShow + (isDefeat ? 1 : 0));
		m_defeatedCount.Text = GameStrings.Format("GLUE_ADVENTURE_DUNGEON_CRAWL_DEFEATED_COUNT", numDefeated, numTotalBosses);
		Actor baseActor = m_bossArches[0];
		while (m_bossArches.Count < 8)
		{
			GameObject obj = Object.Instantiate(baseActor.gameObject);
			obj.transform.parent = baseActor.transform.parent;
			obj.transform.localScale = baseActor.transform.localScale;
			AdventureDungeonCrawlBossGraveyardActor newActor = obj.GetComponent<AdventureDungeonCrawlBossGraveyardActor>();
			m_bossArches.Add(newActor);
		}
		for (int i = 0; i < m_bossArches.Count; i++)
		{
			AdventureDungeonCrawlBossGraveyardActor actor = m_bossArches[i];
			actor.SetStyle(m_dungeonCrawlData);
			DisableBoss(actor);
		}
		for (int j = 0; j < numBossesToShow; j++)
		{
			int bossIndex = j + numBossesOffset;
			bool bossWhoDefeatedMe = bossIndex == numDefeated;
			long dbId = (bossWhoDefeatedMe ? bossWhoDefeatedMeId : defeatedBossIds[bossIndex]);
			string cardId = GameUtils.TranslateDbIdToCardId((int)dbId);
			if (cardId == null)
			{
				Log.Adventures.PrintWarning("AdventureDungeonCrawlBossGraveyard.SetBossDbIds() - No cardId for boss dbId {0}, in arch number {1}!", dbId, j);
				continue;
			}
			using (DefLoader.DisposableFullDef def = DefLoader.Get().GetFullDef(cardId))
			{
				m_bossArches[j].SetFullDef(def);
				def.CardDef.m_AlwaysRenderPremiumPortrait = false;
				m_bossArches[j].SetPremium(TAG_PREMIUM.NORMAL);
				m_bossArches[j].UpdateAllComponents();
				m_bossArches[j].Show();
			}
			if (bossWhoDefeatedMe)
			{
				m_bossLostToActor = m_bossArches[j];
				continue;
			}
			Flipbook flipbook = m_bossArches[j].GetComponentInChildren<Flipbook>(includeInactive: true);
			if (flipbook != null)
			{
				flipbook.gameObject.SetActive(value: true);
			}
		}
		UpdateLayout();
		loadCompletedCallback();
		StopAllCoroutines();
		if (isDefeat)
		{
			if (!HasNewlyUnlockedGSDRewardsToShow())
			{
				MarkRunCompleteSequenceAsSeen(adventureGameSaveServerKey, completedCallback);
			}
			StartCoroutine(PlayDefeatSequence(numBossesToShow, numDefeated, numTotalBosses, (int)bossWhoDefeatedMeId, heroDbId, adventureGameSaveServerKey, completedCallback));
		}
		else
		{
			StartCoroutine(PlayVictorySequence(numBossesToShow, hasCompletedAdventureWithAllClasses, firstTimeCompletedAsClass, numClassesCompleted, heroDbId, completedCallback));
		}
	}
}
