using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Store;
using PegasusLettuce;
using PegasusShared;
using UnityEngine;

public class NarrativeManager : MonoBehaviour
{
	public delegate void CharacterQuotePlayedCallback();

	private const float DELAY_TIME_FOR_QUEST_PROGRESS = 1.5f;

	private const float DELAY_TIME_FOR_QUEST_COMPLETE = 1.5f;

	private const float DELAY_TIME_FOR_AUTO_DESTROY_QUEST_RECEIVED = 3.8f;

	private const float DELAY_TIME_BEFORE_QUEST_DESTROY = 0.8f;

	private const float DELAY_TIME_FOR_AUTO_DESTROY_POST_DESTROY = 1.3f;

	private const float DELAY_TIME_BEFORE_SHOW_BANNER = 1f;

	private const float FALLBACK_DURATION_ON_AUDIO_LOADING_FAIL = 3.5f;

	private static NarrativeManager s_instance;

	private Map<string, AudioSource> m_preloadedSounds = new Map<string, AudioSource>();

	private int m_preloadsNeeded;

	private Queue<CharacterDialogSequence> m_characterDialogSequenceToShow = new Queue<CharacterDialogSequence>();

	private Notification m_activeCharacterDialogNotification;

	private bool m_isBannerShowing;

	private bool m_showingBlockingDialog;

	private bool m_isProcessingQueuedDialogSequence;

	private bool m_hasDoneAllPopupsShownEvent;

	private static Map<ScheduledCharacterDialogEvent, Option> m_lastSeenScheduledCharacterDialogOptions = new Map<ScheduledCharacterDialogEvent, Option>
	{
		{
			ScheduledCharacterDialogEvent.DOUBLE_GOLD_QUEST_GRANTED,
			Option.LATEST_SEEN_SCHEDULED_DOUBLE_GOLD_VO
		},
		{
			ScheduledCharacterDialogEvent.ALL_POPUPS_SHOWN,
			Option.LATEST_SEEN_SCHEDULED_ALL_POPUPS_SHOWN_VO
		},
		{
			ScheduledCharacterDialogEvent.ENTERED_ARENA_DRAFT,
			Option.LATEST_SEEN_SCHEDULED_ENTERED_ARENA_DRAFT
		},
		{
			ScheduledCharacterDialogEvent.LOGIN_FLOW_COMPLETE,
			Option.LATEST_SEEN_SCHEDULED_LOGIN_FLOW_COMPLETE
		},
		{
			ScheduledCharacterDialogEvent.WELCOME_QUESTS_SHOWN,
			Option.LATEST_SEEN_SCHEDULED_WELCOME_QUEST_SHOWN_VO
		},
		{
			ScheduledCharacterDialogEvent.GENERIC_REWARD_SHOWN,
			Option.LATEST_SEEN_SCHEDULED_GENERIC_REWARD_SHOWN_VO
		},
		{
			ScheduledCharacterDialogEvent.ARENA_REWARD_SHOWN,
			Option.LATEST_SEEN_SCHEDULED_ARENA_REWARD_SHOWN_VO
		}
	};

	private static Map<ScheduledCharacterDialogEvent, GameSaveDataManager.GameSaveKeyTuple> m_lastSeenScheduledCharacterDialogKeys = new Map<ScheduledCharacterDialogEvent, GameSaveDataManager.GameSaveKeyTuple>
	{
		{
			ScheduledCharacterDialogEvent.ENTERED_BATTLEGROUNDS,
			new GameSaveDataManager.GameSaveKeyTuple
			{
				Key = GameSaveKeyId.CHARACTER_DIALOG,
				Subkey = GameSaveKeySubkeyId.CHARACTER_DIALOG_LAST_SEEN_BACON
			}
		},
		{
			ScheduledCharacterDialogEvent.BATTLEGROUNDS_LUCKY_DRAW_BUTTON_SHOWN,
			new GameSaveDataManager.GameSaveKeyTuple
			{
				Key = GameSaveKeyId.CHARACTER_DIALOG,
				Subkey = GameSaveKeySubkeyId.CHARACTER_DIALOG_LAST_SEEN_BACON
			}
		},
		{
			ScheduledCharacterDialogEvent.ENTERED_TAVERN_BRAWL,
			new GameSaveDataManager.GameSaveKeyTuple
			{
				Key = GameSaveKeyId.CHARACTER_DIALOG,
				Subkey = GameSaveKeySubkeyId.CHARACTER_DIALOG_LAST_SEEN_TAVERN_BRAWL
			}
		},
		{
			ScheduledCharacterDialogEvent.PURCHASED_BUNDLE,
			new GameSaveDataManager.GameSaveKeyTuple
			{
				Key = GameSaveKeyId.CHARACTER_DIALOG,
				Subkey = GameSaveKeySubkeyId.CHARACTER_DIALOG_LAST_SEEN_PURCHASED_BUNDLE
			}
		},
		{
			ScheduledCharacterDialogEvent.ENTERED_LUCKY_DRAW,
			new GameSaveDataManager.GameSaveKeyTuple
			{
				Key = GameSaveKeyId.CHARACTER_DIALOG,
				Subkey = GameSaveKeySubkeyId.CHARACTER_DIALOG_LAST_SEEN_LUCKY_DRAW
			}
		}
	};

	private Map<ScheduledCharacterDialogEvent, List<ScheduledCharacterDialogDbfRecord>> m_scheduledCharacterDialogData = new Map<ScheduledCharacterDialogEvent, List<ScheduledCharacterDialogDbfRecord>>();

	public void Awake()
	{
		s_instance = this;
	}

	public void Update()
	{
		if (m_showingBlockingDialog)
		{
			OverlayUI.Get().RequestActivateClickBlocker();
		}
	}

	public void OnDestroy()
	{
		if (s_instance != null)
		{
			CleanUpEverything();
			s_instance = null;
		}
	}

	public static NarrativeManager Get()
	{
		return s_instance;
	}

	public void Initialize()
	{
		HearthstoneApplication.Get().WillReset += WillReset;
		SceneMgr.Get().RegisterScenePreLoadEvent(OnScenePreLoad);
		PopupDisplayManager.Get().QuestPopups.RegisterCompletedQuestShownListener(s_instance.OnQuestCompleteShown);
		PopupDisplayManager.Get().RewardPopups.RegisterGenericRewardShownListener(s_instance.OnGenericRewardShown);
		NetCache.Get().RegisterUpdatedListener(typeof(NetCache.NetCacheMercenariesVillageVisitorInfo), OnVillageVisitorStateUpdated);
		StoreManager.Get().RegisterSuccessfulPurchaseListener(OnBundlePurchased);
		StartCoroutine(WaitForAchievesThenInit());
	}

	private void OnScenePreLoad(SceneMgr.Mode prevMode, SceneMgr.Mode mode, object userData)
	{
		if (mode == SceneMgr.Mode.GAMEPLAY)
		{
			CleanUpExceptListeners();
		}
	}

	public void OnQuestCompleteShown(int achieveId)
	{
		Achievement quest = AchieveManager.Get().GetAchievement(achieveId);
		if (quest.QuestDialogId != 0 && quest.OnCompleteDialogSequence != null)
		{
			if (quest.OnCompleteDialogSequence.m_deferOnComplete)
			{
				EnqueueIfNotPresent(quest.OnCompleteDialogSequence);
			}
			else
			{
				PushDialogSequence(quest.OnCompleteDialogSequence);
			}
		}
	}

	public void OnGenericRewardShown(long originData)
	{
		TriggerScheduledCharacterDialogEvent(ScheduledCharacterDialogEvent.GENERIC_REWARD_SHOWN, (int)originData);
	}

	private void EnqueueIfNotPresent(CharacterDialogSequence sequence)
	{
		foreach (CharacterDialogSequence item in m_characterDialogSequenceToShow)
		{
			if (item.m_characterDialogRecord == sequence.m_characterDialogRecord)
			{
				return;
			}
		}
		m_characterDialogSequenceToShow.Enqueue(sequence);
	}

	public void ShowOutstandingQuestDialogs()
	{
		StartCoroutine(ShowOutstandingCharacterDialogSequence());
	}

	public void OnWelcomeQuestsShown(List<Achievement> questsShown, List<Achievement> newlyAvailableQuests)
	{
		TriggerScheduledCharacterDialogEvent(ScheduledCharacterDialogEvent.WELCOME_QUESTS_SHOWN, 0L);
		bool doubleGoldEventActive = EventTimingManager.Get().IsEventActive(EventTimingType.SPECIAL_EVENT_GOLD_DOUBLED);
		foreach (Achievement quest in questsShown)
		{
			if (quest.AutoDestroy)
			{
				StartCoroutine(DestroyAndReplaceQuest(quest));
				break;
			}
			if (quest.QuestDialogId != 0)
			{
				StartCoroutine(HandleQuestReceived(quest));
				break;
			}
			if (doubleGoldEventActive && quest.IsAffectedByDoubleGold && newlyAvailableQuests.Contains(quest) && !AchieveManager.Get().HasActiveAutoDestroyQuests() && !AchieveManager.Get().HasActiveUnseenWelcomeQuestDialog() && OnDoubleGoldQuestGranted())
			{
				break;
			}
		}
	}

	public bool HasCharacterDialogSequenceToShow()
	{
		return m_characterDialogSequenceToShow.Count > 0;
	}

	public bool IsShowingBlockingDialog()
	{
		return m_showingBlockingDialog;
	}

	public void PushDialogSequence(CharacterDialogSequence sequence)
	{
		EnqueueIfNotPresent(sequence);
		StartCoroutine(ShowOutstandingCharacterDialogSequence());
	}

	public IEnumerator<IAsyncJobResult> Job_WaitForOutstandingCharacterDialog()
	{
		StartCoroutine(ShowOutstandingCharacterDialogSequence());
		while (m_isProcessingQueuedDialogSequence)
		{
			yield return null;
		}
	}

	public IEnumerator ShowOutstandingCharacterDialogSequence(int villageRecordID = 0, bool skipPreDialogueWait = false, Action doneCallback = null)
	{
		if (m_characterDialogSequenceToShow.Count == 0 || m_isProcessingQueuedDialogSequence)
		{
			yield break;
		}
		m_isProcessingQueuedDialogSequence = true;
		if (!skipPreDialogueWait)
		{
			yield return new WaitForSeconds(1.5f);
		}
		int bannerIDToShow = 0;
		while (m_characterDialogSequenceToShow.Count > 0)
		{
			CharacterDialogSequence sequence = m_characterDialogSequenceToShow.Peek();
			SetDialogBlocker(sequence.m_blockInput);
			if (sequence != null && sequence.m_onCompleteBannerId != 0)
			{
				bannerIDToShow = sequence.m_onCompleteBannerId;
			}
			if (sequence.m_onPreShow != null)
			{
				sequence.m_onPreShow(sequence);
			}
			yield return StartCoroutine(PlayerCharacterDialogSequence(sequence));
			m_characterDialogSequenceToShow.Dequeue();
			if (villageRecordID != 0)
			{
				MarkVillageDialogueAsSeen(villageRecordID);
			}
			if (doneCallback != null)
			{
				doneCallback();
				break;
			}
		}
		if (bannerIDToShow != 0)
		{
			yield return new WaitForSeconds(1f);
			m_isBannerShowing = true;
			BannerManager.Get().ShowBanner(bannerIDToShow, OnQuestDialogCompleteBannerClosed);
		}
		SetDialogBlocker(value: false);
		while (m_isBannerShowing)
		{
			yield return null;
		}
		m_isProcessingQueuedDialogSequence = false;
	}

	public bool OnDoubleGoldQuestGranted()
	{
		return TriggerScheduledCharacterDialogEvent(ScheduledCharacterDialogEvent.DOUBLE_GOLD_QUEST_GRANTED, 0L);
	}

	public bool OnAllPopupsShown()
	{
		if (m_hasDoneAllPopupsShownEvent)
		{
			return false;
		}
		m_hasDoneAllPopupsShownEvent = true;
		return TriggerScheduledCharacterDialogEvent(ScheduledCharacterDialogEvent.ALL_POPUPS_SHOWN, 0L);
	}

	public bool OnArenaDraftStarted()
	{
		return TriggerScheduledCharacterDialogEvent(ScheduledCharacterDialogEvent.ENTERED_ARENA_DRAFT, 0L);
	}

	public bool OnArenaRewardsShown()
	{
		return TriggerScheduledCharacterDialogEvent(ScheduledCharacterDialogEvent.ARENA_REWARD_SHOWN, 0L);
	}

	public void OnLoginFlowComplete()
	{
		TriggerScheduledCharacterDialogEvent(ScheduledCharacterDialogEvent.LOGIN_FLOW_COMPLETE, 0L);
	}

	public bool OnBattlegroundsEntered()
	{
		return TriggerScheduledCharacterDialogEvent(ScheduledCharacterDialogEvent.ENTERED_BATTLEGROUNDS, 0L);
	}

	public bool OnBattlegroundsLuckyDrawButtonShown()
	{
		return TriggerScheduledCharacterDialogEvent(ScheduledCharacterDialogEvent.BATTLEGROUNDS_LUCKY_DRAW_BUTTON_SHOWN, 0L);
	}

	public bool OnLuckyDrawEntered()
	{
		return TriggerScheduledCharacterDialogEvent(ScheduledCharacterDialogEvent.ENTERED_LUCKY_DRAW, 0L);
	}

	public bool OnTavernBrawlEntered()
	{
		return TriggerScheduledCharacterDialogEvent(ScheduledCharacterDialogEvent.ENTERED_TAVERN_BRAWL, 0L);
	}

	public void OnBundlePurchased(ProductInfo bundle, PaymentMethod purchaseMethod)
	{
		if (bundle != null)
		{
			TriggerScheduledCharacterDialogEvent(ScheduledCharacterDialogEvent.PURCHASED_BUNDLE, bundle.Id.Value);
		}
	}

	private void SetDialogBlocker(bool value)
	{
		m_showingBlockingDialog = value;
		if (FriendChallengeMgr.Get() != null)
		{
			FriendChallengeMgr.Get().UpdateMyAvailability();
		}
	}

	private void OnQuestDialogCompleteBannerClosed()
	{
		m_isBannerShowing = false;
	}

	private IEnumerator WaitForAchievesThenInit()
	{
		while (AchieveManager.Get() == null)
		{
			yield return null;
		}
		while (!AchieveManager.Get().IsReady())
		{
			yield return null;
		}
		PreloadActiveQuestDialog();
		InitScheduledCharacterDialogData();
		AchieveManager.Get().RegisterAchievesUpdatedListener(s_instance.OnAchievesUpdated);
		GameToastMgr.Get().RegisterQuestProgressToastShownListener(s_instance.OnQuestProgressToastShown);
		TavernBrawlManager.Get().OnTavernBrawlUpdated += s_instance.OnTavernBrawlUpdated;
	}

	private IEnumerator DestroyAndReplaceQuest(Achievement quest)
	{
		yield return new WaitForSeconds(3.8f);
		SoundDucker ducker = base.gameObject.AddComponent<SoundDucker>();
		ducker.m_DuckedCategoryDefs = new List<SoundDuckedCategoryDef>();
		foreach (Global.SoundCategory soundCategory in Enum.GetValues(typeof(Global.SoundCategory)))
		{
			if (soundCategory == Global.SoundCategory.AMBIENCE || soundCategory == Global.SoundCategory.MUSIC)
			{
				SoundDuckedCategoryDef categoryDef = new SoundDuckedCategoryDef();
				categoryDef.m_Category = soundCategory;
				categoryDef.m_BeginSec = 0f;
				ducker.m_DuckedCategoryDefs.Add(categoryDef);
			}
		}
		ducker.StartDucking();
		if (quest.QuestDialogId != 0)
		{
			foreach (CharacterDialog dialog in quest.OnReceivedDialogSequence)
			{
				if (IsCharacterDialogDisplayable(dialog))
				{
					yield return new WaitForSeconds(dialog.waitBefore);
					yield return StartCoroutine(PlayCharacterQuoteAndWait(dialog));
					yield return new WaitForSeconds(dialog.waitAfter);
				}
			}
		}
		yield return new WaitForSeconds(0.8f);
		int nextQuestId = WelcomeQuests.Get().CompleteAndReplaceAutoDestroyQuestTile(quest.ID);
		yield return new WaitForSeconds(1.3f);
		Achievement nextQuest = AchieveManager.Get().GetAchievement(nextQuestId);
		if (nextQuest.QuestDialogId != 0)
		{
			int numLinesToPlay = nextQuest.OnReceivedDialogSequence.Count;
			foreach (CharacterDialog dialog in nextQuest.OnReceivedDialogSequence)
			{
				numLinesToPlay--;
				if (IsCharacterDialogDisplayable(dialog))
				{
					yield return new WaitForSeconds(dialog.waitBefore);
					if (numLinesToPlay == 0)
					{
						yield return StartCoroutine(PlayCharacterQuoteAndWait(dialog, OnWelcomeQuestNarrativeFinished));
					}
					else
					{
						yield return StartCoroutine(PlayCharacterQuoteAndWait(dialog));
					}
					yield return new WaitForSeconds(dialog.waitAfter);
				}
			}
		}
		if (ducker != null)
		{
			ducker.StopDucking();
			UnityEngine.Object.Destroy(ducker);
		}
	}

	private IEnumerator HandleQuestReceived(Achievement quest)
	{
		int numLinesToPlay = quest.OnReceivedDialogSequence.Count;
		if (Options.Get().GetInt(Option.LATEST_SEEN_WELCOME_QUEST_DIALOG) == quest.ID || numLinesToPlay <= 0)
		{
			OnWelcomeQuestNarrativeFinished();
			yield break;
		}
		SoundDucker ducker = base.gameObject.AddComponent<SoundDucker>();
		ducker.m_DuckedCategoryDefs = new List<SoundDuckedCategoryDef>();
		foreach (Global.SoundCategory soundCategory in Enum.GetValues(typeof(Global.SoundCategory)))
		{
			if (soundCategory == Global.SoundCategory.AMBIENCE || soundCategory == Global.SoundCategory.MUSIC)
			{
				SoundDuckedCategoryDef categoryDef = new SoundDuckedCategoryDef();
				categoryDef.m_Category = soundCategory;
				categoryDef.m_BeginSec = 0f;
				ducker.m_DuckedCategoryDefs.Add(categoryDef);
			}
		}
		ducker.StartDucking();
		foreach (CharacterDialog dialog in quest.OnReceivedDialogSequence)
		{
			numLinesToPlay--;
			if (IsCharacterDialogDisplayable(dialog))
			{
				yield return new WaitForSeconds(dialog.waitBefore);
				if (numLinesToPlay == 0)
				{
					yield return StartCoroutine(PlayCharacterQuoteAndWait(dialog, OnWelcomeQuestNarrativeFinished));
				}
				else
				{
					yield return StartCoroutine(PlayCharacterQuoteAndWait(dialog));
				}
				yield return new WaitForSeconds(dialog.waitAfter);
			}
		}
		Options.Get().SetInt(Option.LATEST_SEEN_WELCOME_QUEST_DIALOG, quest.ID);
		if (ducker != null)
		{
			ducker.StopDucking();
			UnityEngine.Object.Destroy(ducker);
		}
	}

	private IEnumerator PlayCharacterQuoteAndWait(CharacterDialog dialog, CharacterQuotePlayedCallback callback = null, float waitTimeScale = 1f)
	{
		float minimumDurationSeconds = dialog.minimumDurationSeconds;
		if (Localization.DoesLocaleNeedExtraReadingTime(Localization.GetLocale()))
		{
			minimumDurationSeconds += dialog.localeExtraSeconds;
		}
		AudioSource sound = null;
		bool noSoundSpecified = string.IsNullOrEmpty(dialog.audioName);
		if (!noSoundSpecified)
		{
			sound = GetPreloadedSound(dialog.audioName);
			if (sound == null || sound.clip == null)
			{
				RemovePreloadedSound(dialog.audioName);
				PreloadSound(dialog.audioName);
				while (IsPreloadingAssets())
				{
					yield return null;
				}
				sound = GetPreloadedSound(dialog.audioName);
				if (sound == null || sound.clip == null)
				{
					Debug.Log("NarrativeManager.PlaySoundAndWait() - sound error - " + dialog.audioName);
					yield break;
				}
			}
		}
		float durationSeconds = minimumDurationSeconds;
		if (sound != null)
		{
			durationSeconds = Mathf.Max(minimumDurationSeconds, sound.clip.length);
		}
		else if (sound == null && !noSoundSpecified)
		{
			durationSeconds = 3.5f;
		}
		float waitTime = durationSeconds * waitTimeScale;
		waitTime += 0.5f;
		Log.NarrativeManager.Print("PlayCharacterQuoteAndWait - durationSeconds: {0}  waitTimeScale: {1}", durationSeconds, waitTimeScale);
		string dialogueText = ((!string.IsNullOrEmpty(dialog.bubbleText)) ? dialog.bubbleText.GetString() : (string.IsNullOrEmpty(dialog.audioName) ? "***TEXT NOT FOUND***" : GameStrings.Get(new AssetReference(dialog.audioName).GetLegacyAssetName())));
		if (dialog.useInnkeeperQuote)
		{
			m_activeCharacterDialogNotification = NotificationManager.Get().CreateInnkeeperQuote(UserAttentionBlocker.NONE, dialogueText, dialog.audioName, null);
			m_activeCharacterDialogNotification.ShowWithExistingPopups = true;
		}
		else if (dialog.useBannerStyle && !string.IsNullOrEmpty(dialog.bannerPrefabName))
		{
			m_activeCharacterDialogNotification = NotificationManager.Get().CreateCharacterQuote(dialog.bannerPrefabName, NotificationManager.GetDefaultDialogueBannerPos(dialog.canvasAnchor), dialogueText, dialog.audioName, allowRepeatDuringSession: true, durationSeconds, delegate
			{
				waitTime = 0f;
			}, dialog.canvasAnchor);
		}
		else if (!dialog.useBannerStyle && !string.IsNullOrEmpty(dialog.prefabName))
		{
			m_activeCharacterDialogNotification = NotificationManager.Get().CreateBigCharacterQuoteWithText(dialog.prefabName, NotificationManager.DEFAULT_CHARACTER_POS, dialog.audioName, dialogueText, durationSeconds, delegate
			{
				waitTime = 0f;
			}, useOverlayUI: true, dialog.useAltSpeechBubble ? Notification.SpeechBubbleDirection.TopLeft : Notification.SpeechBubbleDirection.BottomLeft, dialog.persistPrefab, dialog.useAltPosition);
		}
		while (waitTime > 0f)
		{
			waitTime -= Time.deltaTime;
			yield return null;
		}
		callback?.Invoke();
	}

	private void OnAchievesUpdated(List<Achievement> updatedAchieves, List<Achievement> completedAchieves, object userData)
	{
		List<Achievement> activeQuests = AchieveManager.Get().GetActiveQuests();
		PreloadQuestDialog(activeQuests);
	}

	private void OnQuestProgressToastShown(int achieveId)
	{
		StartCoroutine(HandleOnQuestProgressToastShown(achieveId));
	}

	private void OnTavernBrawlUpdated()
	{
		if (!TavernBrawlManager.Get().IsTavernBrawlActive(BrawlType.BRAWL_TYPE_TAVERN_BRAWL))
		{
			return;
		}
		foreach (TavernBrawlMission mission in TavernBrawlManager.Get().Missions)
		{
			if (mission.FirstTimeSeenCharacterDialogID > 0)
			{
				PreloadDialogSequence(mission.FirstTimeSeenCharacterDialogSequence);
			}
		}
	}

	private IEnumerator HandleOnQuestProgressToastShown(int achieveId)
	{
		yield return new WaitForSeconds(1.5f);
		Achievement quest = AchieveManager.Get().GetAchievement(achieveId);
		if ((quest?.QuestDialogId ?? 0) != 0)
		{
			if (quest.Progress == 1)
			{
				yield return PlayerCharacterDialogSequence(quest.OnProgress1DialogSequence);
			}
			else if (quest.Progress == 2)
			{
				yield return PlayerCharacterDialogSequence(quest.OnProgress2DialogSequence);
			}
		}
	}

	public void OnAchieveDismissed(Achievement achieve)
	{
		if (achieve.OnDismissDialogSequence != null)
		{
			StartCoroutine(PlayerCharacterDialogSequence(achieve.OnDismissDialogSequence));
		}
	}

	private static bool IsCharacterDialogDisplayable(CharacterDialog dialog)
	{
		if (dialog.useInnkeeperQuote)
		{
			return true;
		}
		if (!string.IsNullOrEmpty(dialog.prefabName))
		{
			return true;
		}
		Log.All.Print("CharacterDialogItem id={0} is not displayable. To be displayable, either USE_INNKEEPER_QUOTE must be true or PREFAB_NAME is not null/empty.", dialog.dbfRecordId);
		return false;
	}

	private IEnumerator PlayerCharacterDialogSequence(CharacterDialogSequence dialogSequence)
	{
		if (dialogSequence == null)
		{
			yield break;
		}
		if (!dialogSequence.m_ignorePopups)
		{
			yield return StartCoroutine(PopupDisplayManager.Get().WaitForAllPopups());
		}
		foreach (CharacterDialog dialog in dialogSequence)
		{
			if (IsCharacterDialogDisplayable(dialog))
			{
				yield return new WaitForSeconds(dialog.waitBefore);
				yield return StartCoroutine(PlayCharacterQuoteAndWait(dialog));
				yield return new WaitForSeconds(dialog.waitAfter);
			}
		}
	}

	private void OnWelcomeQuestNarrativeFinished()
	{
		if (WelcomeQuests.Get() != null)
		{
			WelcomeQuests.Get().ActivateClickCatcher();
		}
	}

	private void PreloadActiveQuestDialog()
	{
		PreloadQuestDialog(AchieveManager.Get().GetActiveQuests());
	}

	private void PreloadQuestDialog(Achievement achievement)
	{
		if (achievement.QuestDialogId != 0)
		{
			PreloadDialogSequence(achievement.OnReceivedDialogSequence);
			PreloadDialogSequence(achievement.OnCompleteDialogSequence);
			PreloadDialogSequence(achievement.OnProgress1DialogSequence);
			PreloadDialogSequence(achievement.OnProgress2DialogSequence);
			PreloadDialogSequence(achievement.OnDismissDialogSequence);
		}
	}

	private void PreloadQuestDialog(List<Achievement> activeAchievements)
	{
		foreach (Achievement achievement in activeAchievements)
		{
			PreloadQuestDialog(achievement);
		}
	}

	private void PreloadDialogSequence(CharacterDialogSequence questDialogSequence)
	{
		foreach (CharacterDialog dialog in questDialogSequence)
		{
			if (!string.IsNullOrEmpty(dialog.audioName))
			{
				PreloadSound(dialog.audioName);
			}
		}
	}

	private void PreloadQuestDialog(List<string> audioNames)
	{
		foreach (string audioName in audioNames)
		{
			if (!string.IsNullOrEmpty(audioName))
			{
				PreloadSound(audioName);
			}
		}
	}

	private void PreloadSound(string soundPath)
	{
		if (!CheckPreloadedSound(soundPath))
		{
			m_preloadsNeeded++;
			SoundLoader.LoadSound(soundPath, OnSoundLoaded, null, SoundManager.Get().GetPlaceholderSound());
		}
	}

	private void OnSoundLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_preloadsNeeded--;
		if (assetRef == null)
		{
			Debug.LogWarning($"NarrativeManager.OnSoundLoaded() - Asset ref was null)");
			return;
		}
		if (go == null)
		{
			Debug.LogWarning($"NarrativeManager.OnSoundLoaded() - FAILED to load \"{assetRef}\"");
			return;
		}
		AudioSource source = go.GetComponent<AudioSource>();
		if (source == null)
		{
			Debug.LogWarning($"NarrativeManager.OnSoundLoaded() - ERROR \"{assetRef}\" has no Spell component");
		}
		else if (!CheckPreloadedSound(assetRef.ToString()))
		{
			m_preloadedSounds.Add(assetRef.ToString(), source);
		}
	}

	private void RemovePreloadedSound(string soundPath)
	{
		m_preloadedSounds.Remove(soundPath);
	}

	private bool CheckPreloadedSound(string soundPath)
	{
		AudioSource sound;
		return m_preloadedSounds.TryGetValue(soundPath, out sound);
	}

	private AudioSource GetPreloadedSound(string soundPath)
	{
		if (m_preloadedSounds.TryGetValue(soundPath, out var sound))
		{
			return sound;
		}
		Debug.LogError($"NarrativeManager.GetPreloadedSound() - \"{soundPath}\" was not preloaded");
		return null;
	}

	private bool IsPreloadingAssets()
	{
		return m_preloadsNeeded > 0;
	}

	private void SetLastSeenScheduledCharacterDialog(int scheduledDialogId, ScheduledCharacterDialogEvent eventType)
	{
		if (eventType == ScheduledCharacterDialogEvent.INVALID)
		{
			Log.NarrativeManager.PrintError("NarrativeManager.SetLastSeenScheduledCharacterDialog was passed an INVALID ScheduledCharacterDialogEvent");
			return;
		}
		if (m_lastSeenScheduledCharacterDialogKeys.ContainsKey(eventType))
		{
			SetLastSeenScheduledCharacterDialog_GameSaveData(scheduledDialogId, eventType);
			return;
		}
		if (m_lastSeenScheduledCharacterDialogOptions.ContainsKey(eventType))
		{
			SetLastSeenScheduledCharacterDialog_ServerOption(scheduledDialogId, eventType);
			return;
		}
		Log.NarrativeManager.PrintError("NarrativeManager has no storage mechanism for event {0}", eventType.ToString());
	}

	private int GetLastSeenScheduledCharacterDialog(ScheduledCharacterDialogEvent eventType)
	{
		if (eventType == ScheduledCharacterDialogEvent.INVALID)
		{
			Log.NarrativeManager.PrintError("NarrativeManager.GetLastSeenScheduledCharacterDialog was passed an INVALID ScheduledCharacterDialogEvent");
			return -1;
		}
		if (m_lastSeenScheduledCharacterDialogKeys.ContainsKey(eventType))
		{
			return GetLastSeenScheduledCharacterDialog_GameSaveData(eventType);
		}
		if (m_lastSeenScheduledCharacterDialogOptions.ContainsKey(eventType))
		{
			return GetLastSeenScheduledCharacterDialog_ServerOption(eventType);
		}
		Log.NarrativeManager.PrintError("NarrativeManager has no storage mechanism for event {0}", eventType.ToString());
		return -1;
	}

	private void SetLastSeenScheduledCharacterDialog_ServerOption(int scheduledDialogId, ScheduledCharacterDialogEvent eventType)
	{
		m_lastSeenScheduledCharacterDialogOptions.TryGetValue(eventType, out var option);
		if (option == Option.INVALID)
		{
			Log.NarrativeManager.PrintError("NarrativeManager.SetLastSeenScheduledCharacterDialog option mapping had no corresponding option for event: {0}", eventType);
		}
		else
		{
			Options.Get().SetInt(option, scheduledDialogId);
		}
	}

	private int GetLastSeenScheduledCharacterDialog_ServerOption(ScheduledCharacterDialogEvent eventType)
	{
		m_lastSeenScheduledCharacterDialogOptions.TryGetValue(eventType, out var option);
		if (option == Option.INVALID)
		{
			Log.NarrativeManager.PrintError("NarrativeManager.GetLastSeenScheduledCharacterDialog option mapping had no corresponding option for event: {0}", eventType);
			return -1;
		}
		return Options.Get().GetInt(option);
	}

	private void SetLastSeenScheduledCharacterDialog_GameSaveData(int scheduledDialogId, ScheduledCharacterDialogEvent eventType)
	{
		GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(m_lastSeenScheduledCharacterDialogKeys[eventType].Key, m_lastSeenScheduledCharacterDialogKeys[eventType].Subkey, scheduledDialogId));
	}

	private int GetLastSeenScheduledCharacterDialog_GameSaveData(ScheduledCharacterDialogEvent eventType)
	{
		GameSaveDataManager.Get().GetSubkeyValue(m_lastSeenScheduledCharacterDialogKeys[eventType].Key, m_lastSeenScheduledCharacterDialogKeys[eventType].Subkey, out long lastSeenValue);
		return (int)lastSeenValue;
	}

	private void InitScheduledCharacterDialogData()
	{
		foreach (ScheduledCharacterDialogDbfRecord record in GameDbf.ScheduledCharacterDialog.GetRecords())
		{
			if (!GeneralUtils.ForceBool(record.Enabled))
			{
				continue;
			}
			EventTimingType eventTimingTrigger = record.Event;
			if ((record.Event != EventTimingType.UNKNOWN && EventTimingManager.Get().HasEventEnded(eventTimingTrigger)) || (!record.ShowToNewPlayer && !GameUtils.IsAnyTutorialComplete()) || (!record.ShowToReturningPlayer && ReturningPlayerMgr.Get().SuppressOldPopups))
			{
				continue;
			}
			ScheduledCharacterDialogEvent dialogEvent = EnumUtils.GetEnum<ScheduledCharacterDialogEvent>(record.ClientEvent.ToString(), StringComparison.OrdinalIgnoreCase);
			if (GetLastSeenScheduledCharacterDialogDisplayOrder(dialogEvent) < record.DisplayOrder)
			{
				if (!m_scheduledCharacterDialogData.ContainsKey(dialogEvent))
				{
					m_scheduledCharacterDialogData[dialogEvent] = new List<ScheduledCharacterDialogDbfRecord>();
				}
				PreloadQuestDialog(CharacterDialogSequence.GetAudioOfCharacterDialogSequence(record.CharacterDialogId));
				m_scheduledCharacterDialogData[dialogEvent].Add(record);
			}
		}
	}

	private int GetLastSeenScheduledCharacterDialogDisplayOrder(ScheduledCharacterDialogEvent dialogEvent)
	{
		int lastSeenDialogID = GetLastSeenScheduledCharacterDialog(dialogEvent);
		int displayOrder = -1;
		ScheduledCharacterDialogDbfRecord lastSeenDialogRecord = GameDbf.ScheduledCharacterDialog.GetRecord(lastSeenDialogID);
		if (lastSeenDialogRecord != null)
		{
			displayOrder = lastSeenDialogRecord.DisplayOrder;
		}
		return displayOrder;
	}

	public void ResetScheduledCharacterDialogEvent_Debug()
	{
		if (HearthstoneApplication.IsPublic())
		{
			return;
		}
		foreach (ScheduledCharacterDialogEvent eventType in Enum.GetValues(typeof(ScheduledCharacterDialogEvent)))
		{
			if (eventType != 0)
			{
				SetLastSeenScheduledCharacterDialog(0, eventType);
			}
		}
		InitScheduledCharacterDialogData();
	}

	public bool TriggerScheduledCharacterDialogEvent_Debug(ScheduledCharacterDialogEvent eventType)
	{
		if (HearthstoneApplication.IsPublic())
		{
			return false;
		}
		return TriggerScheduledCharacterDialogEvent(eventType, 0L);
	}

	private bool TriggerScheduledCharacterDialogEvent(ScheduledCharacterDialogEvent eventType, long eventData = 0L)
	{
		if (!m_scheduledCharacterDialogData.ContainsKey(eventType))
		{
			return false;
		}
		if (UserAttentionManager.IsBlockedBy(UserAttentionBlocker.SET_ROTATION_INTRO))
		{
			return false;
		}
		ScheduledCharacterDialogDbfRecord recordToUse = null;
		int lastSeenDialogForEventTypeDisplayOrder = GetLastSeenScheduledCharacterDialogDisplayOrder(eventType);
		foreach (ScheduledCharacterDialogDbfRecord record in m_scheduledCharacterDialogData[eventType])
		{
			EventTimingType eventTimingTrigger = record.Event;
			if ((eventTimingTrigger == EventTimingType.UNKNOWN || EventTimingManager.Get().IsEventActive(eventTimingTrigger)) && (eventData == 0L || eventData == record.ClientEventData) && record.DisplayOrder > lastSeenDialogForEventTypeDisplayOrder && (recordToUse == null || recordToUse.DisplayOrder > record.DisplayOrder))
			{
				recordToUse = record;
			}
		}
		if (recordToUse == null)
		{
			return false;
		}
		CharacterDialogSequence dialogSequence = new CharacterDialogSequence(recordToUse.CharacterDialogId);
		if (dialogSequence == null)
		{
			return false;
		}
		dialogSequence.m_onPreShow = delegate
		{
			SetLastSeenScheduledCharacterDialog(recordToUse.ID, eventType);
		};
		PushDialogSequence(dialogSequence);
		return true;
	}

	private void OnVillageVisitorStateUpdated()
	{
		PreloadDialogForActiveVillageTasks();
	}

	public void PreloadMercenaryTutorialDialogue()
	{
		if (GameUtils.IsMercenariesVillageTutorialComplete())
		{
			return;
		}
		foreach (LettuceTutorialVoDbfRecord vo in GameDbf.LettuceTutorialVo.GetRecords())
		{
			if (vo.TutorialDialog != 0)
			{
				CharacterDialogSequence tutorialDialogSequence = new CharacterDialogSequence(vo.TutorialDialog);
				PreloadDialogSequence(tutorialDialogSequence);
			}
		}
	}

	private void PreloadDialogForActiveVillageTasks()
	{
		foreach (MercenariesVisitorState state in LettuceVillageDataUtil.VisitorStates)
		{
			if (state.ActiveTaskState == null)
			{
				continue;
			}
			VisitorTaskDbfRecord record = LettuceVillageDataUtil.GetTaskRecordByID(state.ActiveTaskState.TaskId);
			if (record != null)
			{
				if (record.OnAssignedDialog != 0)
				{
					CharacterDialogSequence assignDialogSequence = new CharacterDialogSequence(record.OnAssignedDialog);
					PreloadDialogSequence(assignDialogSequence);
				}
				if (record.OnCompleteDialog != 0)
				{
					CharacterDialogSequence completeDialogSequence = new CharacterDialogSequence(record.OnCompleteDialog);
					PreloadDialogSequence(completeDialogSequence);
				}
			}
		}
	}

	public void PreloadDialogForActiveVillageBuildings()
	{
		foreach (BuildingTierDbfRecord building in LettuceVillageDataUtil.GetTierRecordsThatCanBeBuilt())
		{
			if (building.OnUpgradedDialog != 0)
			{
				CharacterDialogSequence upgradeDialogSequence = new CharacterDialogSequence(building.OnUpgradedDialog);
				PreloadDialogSequence(upgradeDialogSequence);
			}
		}
	}

	public void OnVillageTaskClaimed(VisitorTaskDbfRecord record, Action callback = null)
	{
		if (record != null && record.OnCompleteDialog != 0)
		{
			PlayVillageDialogue(record.OnCompleteDialog, callback);
		}
	}

	public void OnVillageEntered()
	{
		foreach (MercenariesVisitorState state in LettuceVillageDataUtil.VisitorStates)
		{
			if (state.ActiveTaskState != null)
			{
				VisitorTaskDbfRecord record = LettuceVillageDataUtil.GetTaskRecordByID(state.ActiveTaskState.TaskId);
				if (record != null && record.OnAssignedDialog != 0 && CanPlayVillageDialogue(record.OnAssignedDialog))
				{
					PlayVillageDialogue(record.OnAssignedDialog);
				}
			}
		}
	}

	public void OnVillageBuildingUpgraded(BuildingTierDbfRecord record, Action callback = null)
	{
		if (record.OnUpgradedDialog > 0)
		{
			PlayVillageDialogue(record.OnUpgradedDialog, callback);
		}
	}

	private void PlayVillageDialogue(int recordID, Action doneCallback = null)
	{
		CharacterDialogSequence dialogSequence = new CharacterDialogSequence(recordID);
		EnqueueIfNotPresent(dialogSequence);
		StartCoroutine(ShowOutstandingCharacterDialogSequence(recordID, skipPreDialogueWait: true, doneCallback));
	}

	public void PlayMercenariesTutorialDialogue(int recordID, Action doneCallback = null)
	{
		CharacterDialogSequence dialogSequence = new CharacterDialogSequence(recordID);
		EnqueueIfNotPresent(dialogSequence);
		StartCoroutine(ShowOutstandingCharacterDialogSequence(0, skipPreDialogueWait: true, doneCallback));
	}

	private void MarkVillageDialogueAsSeen(int dialogID)
	{
		GameSaveDataManager.SubkeySaveRequest saveRequest = GameSaveDataManager.Get().GenerateSaveRequestToRemoveValueFromSubkeyIfItExists(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_VILLAGE_RECENTLY_PLAYED_TASK_DIALOGS, dialogID);
		if (saveRequest != null)
		{
			GameSaveDataManager.Get().SaveSubkey(saveRequest);
		}
	}

	private bool CanPlayVillageDialogue(int dialogID)
	{
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.MERCENARIES, GameSaveKeySubkeyId.MERCENARIES_VILLAGE_RECENTLY_PLAYED_TASK_DIALOGS, out List<long> values);
		values = values ?? new List<long>();
		return !values.Contains(dialogID);
	}

	private void WillReset()
	{
		CleanUpEverything();
	}

	private void CleanUpEverything()
	{
		CleanUpExceptListeners();
		if (ServiceManager.TryGet<AchieveManager>(out var achieveManager))
		{
			achieveManager.RemoveAchievesUpdatedListener(OnAchievesUpdated);
		}
		if (GameToastMgr.Get() != null)
		{
			GameToastMgr.Get().RemoveQuestProgressToastShownListener(OnQuestProgressToastShown);
		}
		if (ServiceManager.TryGet<PopupDisplayManager>(out var popupDisplayManager))
		{
			popupDisplayManager.QuestPopups.RemoveCompletedQuestShownListener(OnQuestCompleteShown);
		}
		if (ServiceManager.TryGet<TavernBrawlManager>(out var tavernBrawlManager))
		{
			tavernBrawlManager.OnTavernBrawlUpdated -= OnTavernBrawlUpdated;
		}
		if (HearthstoneApplication.Get() != null)
		{
			HearthstoneApplication.Get().WillReset -= WillReset;
		}
		if (ServiceManager.TryGet<SceneMgr>(out var sceneMgr))
		{
			sceneMgr.UnregisterScenePreLoadEvent(OnScenePreLoad);
		}
		if (ServiceManager.TryGet<LoginManager>(out var loginManager))
		{
			loginManager.OnFullLoginFlowComplete -= OnLoginFlowComplete;
		}
		if (StoreManager.Get() != null)
		{
			StoreManager.Get().RemoveSuccessfulPurchaseListener(OnBundlePurchased);
		}
		if (NetCache.Get() != null)
		{
			NetCache.Get().RemoveUpdatedListener(typeof(NetCache.NetCacheMercenariesVillageVisitorInfo), OnVillageVisitorStateUpdated);
		}
	}

	private void CleanUpExceptListeners()
	{
		StopAllCoroutines();
		m_characterDialogSequenceToShow.Clear();
		m_preloadedSounds.Clear();
		if (NotificationManager.Get() != null && m_activeCharacterDialogNotification != null)
		{
			NotificationManager.Get().DestroyNotification(m_activeCharacterDialogNotification, 0f);
		}
		m_preloadsNeeded = 0;
		m_isBannerShowing = false;
		m_showingBlockingDialog = false;
		m_isProcessingQueuedDialogSequence = false;
		m_hasDoneAllPopupsShownEvent = false;
	}

	public List<Option> Cheat_ClearAllSeen()
	{
		List<Option> options = new List<Option>();
		options.AddRange(m_lastSeenScheduledCharacterDialogOptions.Values);
		options.Add(Option.LATEST_SEEN_WELCOME_QUEST_DIALOG);
		options.Add(Option.LATEST_SEEN_TAVERNBRAWL_SEASON_CHALKBOARD);
		options.Add(Option.LATEST_SEEN_TAVERNBRAWL_SEASON);
		foreach (Option option in options)
		{
			Options.Get().DeleteOption(option);
		}
		return options;
	}
}
