using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Blizzard.T5.Core;
using Blizzard.T5.MaterialService.Extensions;
using PegasusGame;
using PegasusLettuce;
using PegasusShared;
using UnityEngine;

public class LettuceMissionEntity : MissionEntity
{
	public delegate void OnEmoteBanterPlayedDelegate(LettuceMissionEntity letlMissionEntity, EmoteType emoteType, AudioSource audioSource);

	protected class CardSpeedCamparer : IComparer<int>, IComparer<Card>
	{
		private bool m_lowToHigh;

		public CardSpeedCamparer(bool lowToHigh = true)
		{
			m_lowToHigh = lowToHigh;
		}

		public int Compare(Card c1, Card c2)
		{
			int speed1 = c1.GetPreparedLettuceAbilitySpeedValue();
			int speed2 = c2.GetPreparedLettuceAbilitySpeedValue();
			return Compare(speed1, speed2);
		}

		public int Compare(int speed1, int speed2)
		{
			if (m_lowToHigh)
			{
				return speed1.CompareTo(speed2);
			}
			return speed2.CompareTo(speed1);
		}
	}

	private static readonly AssetReference LETTUCE_PHASE_POPUP = new AssetReference("LettuceTurnIndicator.prefab:bb1b08b3add6d3047bf4b787e266c26e");

	private GameObject m_phasePopup;

	protected int m_gamePhase = 1;

	private Entity m_entityThatJustCancelledAttack;

	private int m_prevSelectedCharacterZonePosition;

	private int m_numPlayActorShifting;

	private bool m_isCameraShifting;

	protected bool m_abilityOrderSpeechBubblesEnabled = true;

	protected bool m_enemyAbilityOrderSpeechBubblesEnabled = true;

	private List<MercenariesExperienceUpdate> m_endGameExperienceUpdates = new List<MercenariesExperienceUpdate>();

	private InputManager.ZoneTooltipSettings m_zoneTooltipSettings;

	private MercenariesBenchVisualController m_benchVisualController;

	private int m_blockingPowerProcessingCount;

	private LettuceFakeHandController m_fakeHandController = new LettuceFakeHandController();

	private ScreenEffectsHandle m_screenEffectsHandle;

	private readonly List<OnEmoteBanterPlayedDelegate> m_onEmoteBanterPlayedCallbacks = new List<OnEmoteBanterPlayedDelegate>();

	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static Map<GameEntityOption, string> s_stringOptions = InitStringOptions();

	protected Notification m_popupTutorialNotification;

	public MercenariesPvPRatingUpdate RatingChangeData { get; set; }

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool>
		{
			{
				GameEntityOption.DIM_OPPOSING_HERO_DURING_MULLIGAN,
				true
			},
			{
				GameEntityOption.HANDLE_COIN,
				false
			},
			{
				GameEntityOption.DO_OPENING_TAUNTS,
				false
			},
			{
				GameEntityOption.SKIP_HERO_LOAD,
				true
			},
			{
				GameEntityOption.SUPPRESS_CLASS_NAMES,
				true
			},
			{
				GameEntityOption.ALLOW_NAME_BANNER_MODE_ICONS,
				false
			},
			{
				GameEntityOption.DISABLE_RESTART_BUTTON,
				true
			},
			{
				GameEntityOption.DISABLE_CARD_TYPE_BANNER,
				true
			},
			{
				GameEntityOption.USE_COMPACT_ENCHANTMENT_BANNERS,
				true
			},
			{
				GameEntityOption.ALLOW_FATIGUE,
				false
			},
			{
				GameEntityOption.ALLOW_ENCHANTMENT_SPARKLES,
				false
			},
			{
				GameEntityOption.ALLOW_SLEEP_FX,
				false
			},
			{
				GameEntityOption.DISABLE_NONMERC_MANA_GEM,
				true
			},
			{
				GameEntityOption.DISABLE_SPELL_MANA_GEM,
				true
			},
			{
				GameEntityOption.SHOW_SPEED_WING_ON_ACTOR,
				true
			},
			{
				GameEntityOption.FLIP_END_TURN_BUTTON_WHEN_ENTERING_NO_MORE_PLAY,
				true
			},
			{
				GameEntityOption.ALWAYS_USE_FAST_CARD_DRAW_SCALE,
				true
			},
			{
				GameEntityOption.DISABLE_DELAY_BETWEEN_BIG_CARD_DISPLAY_AND_POWER_PROCESSING,
				true
			},
			{
				GameEntityOption.USE_FASTER_ATTACK_SPELL_BIRTH_STATE,
				true
			},
			{
				GameEntityOption.EARLY_CONCEDE_PROCESS_SUB_SPELL_IN_FINAL_WRAPUP_STEP,
				true
			}
		};
	}

	private static Map<GameEntityOption, string> InitStringOptions()
	{
		return new Map<GameEntityOption, string>
		{
			{
				GameEntityOption.VICTORY_SCREEN_PREFAB_PATH,
				"VictoryTwoScoop_Lettuce.prefab:8cc3d04e21ce8334eb7c5a97d0d61086"
			},
			{
				GameEntityOption.DEFEAT_SCREEN_PREFAB_PATH,
				"DefeatTwoScoop_Lettuce.prefab:126f120867ecadd448b08d33a1f50ae9"
			},
			{
				GameEntityOption.END_OF_GAME_SPELL_PREFAB_PATH,
				"Lettuce_EndOfGameSpell.prefab:a739ec7b56e6bd14f825ba03fc0ebbfe"
			},
			{
				GameEntityOption.VICTORY_AUDIO_PATH,
				null
			},
			{
				GameEntityOption.DEFEAT_AUDIO_PATH,
				null
			}
		};
	}

	public LettuceMissionEntity(VoPlaybackHandler voHandler = null)
		: base(voHandler)
	{
		m_gameOptions.AddOptions(s_booleanOptions, s_stringOptions);
		if (GameMgr.Get().GetGameType() == GameType.GT_MERCENARIES_PVE)
		{
			m_gameOptions.SetBooleanOption(GameEntityOption.DISABLE_OPPONENT_NAME_BANNER, value: true);
		}
		m_zoneTooltipSettings = new InputManager.ZoneTooltipSettings
		{
			EnemyDeck = new InputManager.TooltipSettings(allowed: true, GetEnemyDeckTooltipContent),
			EnemyHand = new InputManager.TooltipSettings(allowed: false),
			EnemyMana = new InputManager.TooltipSettings(allowed: false),
			FriendlyDeck = InputManager.TooltipSettings.CreateCustomHandler(OnFriendlyDeckMouseOver, OnFriendlyDeckMouseOut),
			FriendlyHand = new InputManager.TooltipSettings(allowed: false),
			FriendlyMana = new InputManager.TooltipSettings(allowed: false)
		};
		InitializePhasePopup();
		InitializePlayZonePosition();
		SceneMgr.Get().RegisterSceneLoadedEvent(OnGameplaySceneLoaded);
		GameState.Get().RegisterOptionsReceivedListener(OnOptionsReceived);
		GameState.Get().RegisterOptionsSentListener(OnOptionsSent);
		GameState.Get().RegisterFriendlyTurnStartedListener(OnFriendlyTurnStarted);
		EndTurnButton.Get().RegisterButtonUnblockedListener(OnEndTurnButtonUnblocked);
		Network.Get().RegisterNetHandler(MercenariesRewardUpdate.PacketID.ID, OnMercenariesRewardUpdate);
		m_screenEffectsHandle = new ScreenEffectsHandle(this);
	}

	private void InitializePlayZonePosition()
	{
		BoardLayout boardLayout = Gameplay.Get().GetBoardLayout();
		Transform friendlyZoneBone = boardLayout.FindBone("FriendlyPlay_Combat");
		ZoneMgr.Get().FindZoneOfType<ZonePlay>(Player.Side.FRIENDLY).transform.position = friendlyZoneBone.transform.position;
		Transform opposingZoneBone = boardLayout.FindBone("OpposingPlay_Combat");
		ZoneMgr.Get().FindZoneOfType<ZonePlay>(Player.Side.OPPOSING).transform.position = opposingZoneBone.transform.position;
	}

	private void OnFriendlyDeckMouseOver(Action<string, string> showRegularTooltip)
	{
		m_benchVisualController?.OnFriendlyBenchMouseOver(showRegularTooltip);
	}

	private void OnFriendlyDeckMouseOut()
	{
		m_benchVisualController?.OnFriendlyBenchMouseOut();
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		PreloadPrefab("MercenariesBenchVisualController.prefab:320ca7517518ebe4f88977bd4291da36", delegate(AssetReference assetRef, GameObject gameObject, object callbackData)
		{
			m_benchVisualController = gameObject.GetComponent<MercenariesBenchVisualController>();
		});
	}

	public override void OnDecommissionGame()
	{
		if (SceneMgr.Get() != null)
		{
			SceneMgr.Get().UnregisterSceneLoadedEvent(OnGameplaySceneLoaded);
		}
		if (GameState.Get() != null)
		{
			GameState.Get().UnregisterOptionsReceivedListener(OnOptionsReceived);
			GameState.Get().UnregisterOptionsSentListener(OnOptionsSent);
			GameState.Get().UnregisterFriendlyTurnStartedListener(OnFriendlyTurnStarted);
		}
		if (EndTurnButton.Get() != null)
		{
			EndTurnButton.Get().UnregisterButtonUnblockedListener(OnEndTurnButtonUnblocked);
		}
		if (Network.Get() != null)
		{
			Network.Get().RemoveNetHandler(MercenariesRewardUpdate.PacketID.ID, OnMercenariesRewardUpdate);
		}
		base.OnDecommissionGame();
	}

	public override void OnTagChanged(TagDelta change)
	{
		base.OnTagChanged(change);
		if (change.tag == 2224)
		{
			if (change.newValue == 0)
			{
				HideOpposingFakeHand();
			}
			else
			{
				ShowOpposingFakeHand();
			}
		}
	}

	private void StartBlockingPowerProcessing()
	{
		m_blockingPowerProcessingCount++;
		GameState.Get().SetBusy(busy: true);
	}

	private void StopBlockingPowerProcessingIfPossible()
	{
		m_blockingPowerProcessingCount--;
		if (m_blockingPowerProcessingCount == 0)
		{
			GameState.Get().SetBusy(busy: false);
		}
	}

	private void InitializePhasePopup()
	{
		GameObjectCallback callback = delegate(AssetReference assetRef, GameObject go, object callbackData)
		{
			m_phasePopup = go;
			m_phasePopup.SetActive(value: false);
		};
		if (!AssetLoader.Get().LoadGameObject(LETTUCE_PHASE_POPUP, callback))
		{
			callback(LETTUCE_PHASE_POPUP, null, null);
		}
	}

	private void ShowPopup(string playmakerState)
	{
		GameEntity.Coroutines.StartCoroutine(ShowPopupCoroutine(playmakerState));
	}

	private IEnumerator ShowPopupCoroutine(string playmakerState)
	{
		StartBlockingPowerProcessing();
		AddInputBlocker();
		while (m_phasePopup == null)
		{
			yield return null;
		}
		m_phasePopup.SetActive(value: true);
		PlayMakerFSM playmaker = m_phasePopup.GetComponent<PlayMakerFSM>();
		playmaker.SetState(playmakerState);
		while (playmaker.ActiveStateName != "Hide")
		{
			yield return null;
		}
		RemoveInputBlocker();
		AttemptAutoInput();
		Entity sourceEntity = ZoneMgr.Get().GetLettuceAbilitiesSourceEntity();
		if (sourceEntity != null)
		{
			foreach (int entityID in sourceEntity.GetLettuceAbilityEntityIDs())
			{
				Card abilityCard = GameState.Get().GetEntity(entityID)?.GetCard();
				if (abilityCard != null)
				{
					abilityCard.UpdateActorState();
				}
			}
		}
		StopBlockingPowerProcessingIfPossible();
	}

	private void AttemptAutoInput()
	{
		if (InputManager.Get().PermitDecisionMakingInput() && !GameState.Get().IsResponsePacketBlocked() && !SceneMgr.Get().IsTransitioning() && m_gamePhase != 3 && !m_isCameraShifting)
		{
			switch (GameState.Get().GetGameEntity().GetTag<ACTION_STEP_TYPE>(GAME_TAG.ACTION_STEP_TYPE))
			{
			case ACTION_STEP_TYPE.DEFAULT:
				AutoSelectNextPendingMercenary();
				break;
			case ACTION_STEP_TYPE.LETTUCE_MERCENARY_SELECTION:
				AutoEndTurn();
				break;
			}
		}
	}

	private void AutoSelectNextPendingMercenary()
	{
		if (HasTag(GAME_TAG.LETTUCE_DISABLE_AUTO_SELECT_NEXT_MERC) || ZoneMgr.Get().GetLettuceAbilitiesSourceEntity() != null)
		{
			return;
		}
		if (m_entityThatJustCancelledAttack != null)
		{
			ZoneMgr.Get().DisplayLettuceAbilitiesForEntity(m_entityThatJustCancelledAttack);
			RemoteActionHandler.Get().NotifyOpponentOfSelection(m_entityThatJustCancelledAttack.GetEntityId());
			m_entityThatJustCancelledAttack = null;
			return;
		}
		Entity nextPendingMerc = GetNextPendingMercenary();
		if (nextPendingMerc != null)
		{
			ZoneMgr.Get().DisplayLettuceAbilitiesForEntity(nextPendingMerc);
			RemoteActionHandler.Get().NotifyOpponentOfSelection(nextPendingMerc.GetEntityId());
		}
	}

	private Entity GetNextPendingMercenary()
	{
		Network.Options options = GameState.Get()?.GetOptionsPacket();
		if (options == null)
		{
			return null;
		}
		ZonePlay friendlyPlayZone = ZoneMgr.Get()?.FindZoneOfType<ZonePlay>(Player.Side.FRIENDLY);
		if (friendlyPlayZone == null)
		{
			return null;
		}
		List<Card> eligibleCards = new List<Card>();
		foreach (Card card in friendlyPlayZone.GetCards())
		{
			Entity entity = card.GetEntity();
			if (entity != null && entity.IsMinion() && entity.GetController() != null && entity.GetController().IsTeamLeader() && (!entity.HasSelectedLettuceAbility() || !entity.HasTag(GAME_TAG.LETTUCE_HAS_MANUALLY_SELECTED_ABILITY)))
			{
				eligibleCards.Add(card);
			}
		}
		int totalCount = friendlyPlayZone.GetCards().Count;
		eligibleCards.Sort(delegate(Card lhs, Card rhs)
		{
			int num = lhs.GetEntity().GetZonePosition() - m_prevSelectedCharacterZonePosition;
			if (num <= 0)
			{
				num += totalCount;
			}
			int num2 = rhs.GetEntity().GetZonePosition() - m_prevSelectedCharacterZonePosition;
			if (num2 <= 0)
			{
				num2 += totalCount;
			}
			return num - num2;
		});
		foreach (Card item in eligibleCards)
		{
			Entity entity2 = item.GetEntity();
			bool hasValidAbilityOptions = false;
			foreach (int abilityEntityID in entity2.GetLettuceAbilityEntityIDs())
			{
				Network.Options.Option option = options.GetOptionFromEntityID(abilityEntityID);
				if (option != null && (option.Main.PlayErrorInfo.IsValid() || option.HasValidSubOption()))
				{
					hasValidAbilityOptions = true;
					break;
				}
			}
			if (hasValidAbilityOptions)
			{
				return entity2;
			}
		}
		return null;
	}

	private void AutoEndTurn()
	{
		Network.Options options = GameState.Get().GetOptionsPacket();
		if (options == null)
		{
			return;
		}
		bool hasPowerOptions = false;
		foreach (Network.Options.Option item in options.List)
		{
			if (item.Type != Network.Options.Option.OptionType.END_TURN)
			{
				hasPowerOptions = true;
				break;
			}
		}
		if (!hasPowerOptions)
		{
			InputManager.Get().DoEndTurnButton();
		}
	}

	private void OnGameplaySceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		if (mode != SceneMgr.Mode.GAMEPLAY)
		{
			return;
		}
		SceneMgr.Get().UnregisterSceneLoadedEvent(OnGameplaySceneLoaded);
		ACTION_STEP_TYPE actionStepType = GetTag<ACTION_STEP_TYPE>(GAME_TAG.ACTION_STEP_TYPE);
		bool isActionStep = GameState.Get().IsActionStep();
		if (isActionStep)
		{
			switch (actionStepType)
			{
			case ACTION_STEP_TYPE.DEFAULT:
				GameEntity.Coroutines.StartCoroutine(OnPreparationPhase());
				break;
			case ACTION_STEP_TYPE.LETTUCE_MERCENARY_SELECTION:
				GameEntity.Coroutines.StartCoroutine(OnNominationPhase());
				break;
			}
		}
		if (Board.Get() is MercenariesBoard board)
		{
			int seed = GetTag(GAME_TAG.GAME_SEED);
			bool isFinalBoss = GameUtils.IsFinalBossNodeType(GetTag(GAME_TAG.LETTUCE_NODE_TYPE));
			bool allowLightingEffects = true;
			int bountyId = GetTag(GAME_TAG.LETTUCE_CURRENT_BOUNTY_ID);
			LettuceBountyDbfRecord bountyRecord = GameDbf.LettuceBounty.GetRecord(bountyId);
			if (bountyRecord != null && bountyRecord.BountySetRecord != null && bountyRecord.BountySetRecord.IsTutorial)
			{
				allowLightingEffects = false;
			}
			board.RandomizeVisuals(isFinalBoss, allowLightingEffects, seed);
		}
		int currentTurn = GetTag(GAME_TAG.TURN);
		if (currentTurn > 0)
		{
			if (isActionStep && actionStepType == ACTION_STEP_TYPE.DEFAULT)
			{
				UpdateAllMercenaryAbilityOrderBubbleText();
			}
			if (HasTag(GAME_TAG.LETTUCE_SHOW_OPPOSING_FAKE_HAND))
			{
				ShowOpposingFakeHand();
			}
			OnLettuceMissionEntityReconnect(currentTurn);
		}
		OnLettuceMissionEntityGameSceneLoaded();
	}

	protected virtual void OnLettuceMissionEntityGameSceneLoaded()
	{
	}

	protected virtual void OnLettuceMissionEntityReconnect(int currentTurn)
	{
	}

	private void OnOptionsSent(Network.Options.Option option, object userData)
	{
		HideWeaknessSplats();
	}

	private void OnOptionsReceived(object userData)
	{
		AttemptAutoInput();
	}

	private void OnFriendlyTurnStarted(object userData)
	{
		AttemptAutoInput();
	}

	private void OnEndTurnButtonUnblocked(object userData)
	{
		AttemptAutoInput();
	}

	private void OnMercenariesRewardUpdate()
	{
		MercenariesRewardUpdate response = Network.Get().MercenariesRewardUpdate();
		if (response == null || response.RewardType == LettuceRewardContents.Type.TYPE_PVE_CHEST || response.RewardType == LettuceRewardContents.Type.TYPE_PVE_BOSS_CHEST || response.RewardType == LettuceRewardContents.Type.TYPE_PVP_CHEST || response.RewardType == LettuceRewardContents.Type.TYPE_PVE_CONSOLATION || response.RewardType == LettuceRewardContents.Type.TYPE_PVE_AUTO_RETIRED)
		{
			return;
		}
		foreach (MercenariesExperienceUpdate experienceUpdate in response.ExperienceUpdates)
		{
			if (experienceUpdate.HasMercenaryId && experienceUpdate.HasExpDelta)
			{
				m_endGameExperienceUpdates.Add(experienceUpdate);
			}
		}
	}

	public override InputManager.ZoneTooltipSettings GetZoneTooltipSettings()
	{
		return m_zoneTooltipSettings;
	}

	private bool GetEnemyDeckTooltipContent(ref string headline, ref string description, int index)
	{
		if (index == 0)
		{
			headline = GameStrings.Get("GAMEPLAY_TOOLTIP_LETTUCE_ENEMYBENCH_HEADLINE");
			ZoneDeck deck = ZoneMgr.Get().FindZoneOfType<ZoneDeck>(Player.Side.OPPOSING);
			ZoneHand hand = ZoneMgr.Get().FindZoneOfType<ZoneHand>(Player.Side.OPPOSING);
			description = GameStrings.Format("GAMEPLAY_TOOLTIP_LETTUCE_ENEMYBENCH_DESCRIPTION", deck.GetCardCount() + hand.GetCardCount());
			return true;
		}
		return false;
	}

	public List<MercenariesExperienceUpdate> GetMercenaryExperienceUpdates()
	{
		return m_endGameExperienceUpdates;
	}

	public override Entity GetExtraMouseOverBigCardEntity(Entity source)
	{
		if (source == null)
		{
			return null;
		}
		Entity abilityEntity = null;
		int lettuceSelectedAbilityEntityID = source.GetSelectedLettuceAbilityID();
		if (lettuceSelectedAbilityEntityID != 0)
		{
			abilityEntity = GameState.Get().GetEntity(lettuceSelectedAbilityEntityID);
		}
		else if (!source.ShouldShowEquipmentTextOnMerc())
		{
			abilityEntity = source.GetEquipmentEntity();
		}
		return abilityEntity;
	}

	public override bool ShowMouseOverBigCardImmediately(Entity mouseOverEntity)
	{
		if (mouseOverEntity == null)
		{
			return false;
		}
		if (UniversalInputManager.Get().IsTouchMode())
		{
			return false;
		}
		if (mouseOverEntity.IsMinion())
		{
			return true;
		}
		if (mouseOverEntity.IsLettuceAbility())
		{
			return true;
		}
		return false;
	}

	public override bool SuppressMousedOverCardTooltip(out bool resetTimer)
	{
		resetTimer = false;
		MercenariesAbilityTray tray = ZoneMgr.Get().GetLettuceZoneController().GetAbilityTray();
		if (tray == null)
		{
			return false;
		}
		if (tray.IsAnimating())
		{
			resetTimer = true;
			return true;
		}
		return false;
	}

	public override bool ShouldSuppressCardMouseOver(Entity mouseOverEntity)
	{
		if (m_gamePhase == 3 || m_isCameraShifting)
		{
			return true;
		}
		return false;
	}

	public override bool ShouldSuppressHistoryMouseOver()
	{
		if (m_gamePhase == 3 || m_isCameraShifting)
		{
			return true;
		}
		return false;
	}

	public override bool NotifyOfTooltipDisplay(TooltipZone tooltip)
	{
		if (m_gamePhase == 3 || m_isCameraShifting)
		{
			return true;
		}
		return false;
	}

	public override bool ShouldSuppressOptionHighlight(Entity entity)
	{
		if (GameState.Get().GetGameEntity().HasTag(GAME_TAG.ALLOW_MOVE_MINION))
		{
			return false;
		}
		if (entity != null && entity.IsMinion() && entity.IsControlledByFriendlySidePlayer())
		{
			Card card = entity.GetCard();
			if ((object)card != null && card.GetZone()?.m_ServerTag == TAG_ZONE.PLAY)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void OnAbilityTrayShown(Entity entity)
	{
	}

	public virtual void OnAbilityTrayDismissed()
	{
	}

	public void SetEntityThatJustCancelledAbilitySelection(Entity entity)
	{
		m_entityThatJustCancelledAttack = entity;
	}

	public void SetPrevSelectedCharacterZonePosition(int zonePos)
	{
		m_prevSelectedCharacterZonePosition = zonePos;
	}

	public override List<TooltipPanelManager.TooltipPanelData> GetOverwriteKeywordHelpPanelDisplay(Entity entity)
	{
		if (entity.IsControlledByFriendlySidePlayer() && GameState.Get().IsActionStep() && GetTag<ACTION_STEP_TYPE>(GAME_TAG.ACTION_STEP_TYPE) == ACTION_STEP_TYPE.LETTUCE_MERCENARY_SELECTION)
		{
			List<TooltipPanelManager.TooltipPanelData> results = new List<TooltipPanelManager.TooltipPanelData>();
			foreach (int abilityEntityID in entity.GetLettuceAbilityEntityIDs())
			{
				Entity ability = GameState.Get().GetEntity(abilityEntityID);
				if (ability != null && ability.HasTag(GAME_TAG.LETTUCE_IS_TREASURE_CARD) && !ability.HasTag(GAME_TAG.MERCENARIES_DISCOVER_SOURCE))
				{
					results.Add(new TooltipPanelManager.TooltipPanelData
					{
						m_title = ability.GetName(),
						m_description = UberText.RemoveMarkupAndCollapseWhitespaces(ability.GetCardTextInHand(), replaceCarriageReturnWithBreakHint: true, preserveBreakHint: true)
					});
				}
			}
			foreach (int abilityEntityID2 in entity.GetLettuceAbilityEntityIDs())
			{
				Entity ability2 = GameState.Get().GetEntity(abilityEntityID2);
				if (ability2 != null && !ability2.HasTag(GAME_TAG.MERCENARIES_DISCOVER_SOURCE) && !ability2.HasTag(GAME_TAG.LETTUCE_IS_TREASURE_CARD) && !ability2.IsLettuceEquipment())
				{
					results.Add(new TooltipPanelManager.TooltipPanelData
					{
						m_title = ability2.GetName(),
						m_description = UberText.RemoveMarkupAndCollapseWhitespaces(ability2.GetCardTextInHand(), replaceCarriageReturnWithBreakHint: true, preserveBreakHint: true)
					});
				}
			}
			int tagValue = entity.GetTag(GAME_TAG.LETTUCE_FACTION);
			if (tagValue != 0)
			{
				TAG_LETTUCE_FACTION tAG_LETTUCE_FACTION = (TAG_LETTUCE_FACTION)tagValue;
				string key = "GLUE_LETTUCE_FACTION_" + tAG_LETTUCE_FACTION;
				string title = GameStrings.Get("GLUE_LETTUCE_FACTION");
				string localText = GameStrings.Get(key);
				if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(localText))
				{
					results.Add(new TooltipPanelManager.TooltipPanelData
					{
						m_title = title,
						m_description = localText
					});
				}
			}
			return results;
		}
		return null;
	}

	public override bool GetEntityBaseForKeywordTooltips(Entity source, bool isHistoryTile, out EntityBase entityBaseForTooltips, out List<EntityBase> additionalEntityBaseForTooltips)
	{
		entityBaseForTooltips = null;
		additionalEntityBaseForTooltips = null;
		if (isHistoryTile)
		{
			return false;
		}
		if (!source.IsMinion())
		{
			return false;
		}
		Zone zone = source.GetCard().GetZone();
		if ((object)zone == null || zone.m_ServerTag != TAG_ZONE.PLAY)
		{
			return false;
		}
		int lettuceSelectedAbilityEntityID = source.GetSelectedLettuceAbilityID();
		Entity abilityEntity = GameState.Get().GetEntity(lettuceSelectedAbilityEntityID);
		Entity equipmentEntity = source.GetEquipmentEntity();
		if (abilityEntity != null)
		{
			entityBaseForTooltips = abilityEntity;
			additionalEntityBaseForTooltips = new List<EntityBase> { source };
			if (equipmentEntity != null)
			{
				additionalEntityBaseForTooltips.Add(equipmentEntity);
			}
			return true;
		}
		if (equipmentEntity != null)
		{
			entityBaseForTooltips = equipmentEntity;
			additionalEntityBaseForTooltips = new List<EntityBase> { source };
			return true;
		}
		return false;
	}

	private int GetNominatedMercenariesCount()
	{
		int nominatedCount = 0;
		ZonePlay friendlyPlayZone = ZoneMgr.Get().FindZoneOfType<ZonePlay>(Player.Side.FRIENDLY);
		if (friendlyPlayZone != null)
		{
			foreach (Card card in friendlyPlayZone.GetCards())
			{
				Entity entity = card.GetEntity();
				if (entity != null && entity.IsMercenary() && entity.IsControlledByFriendlySidePlayer())
				{
					nominatedCount++;
				}
			}
		}
		return nominatedCount;
	}

	private int GetBenchedMercenariesCount()
	{
		int benchedCount = 0;
		ZoneHand friendlyHandZone = ZoneMgr.Get().FindZoneOfType<ZoneHand>(Player.Side.FRIENDLY);
		if (friendlyHandZone != null)
		{
			foreach (Card card in friendlyHandZone.GetCards())
			{
				Entity entity = card.GetEntity();
				if (entity != null && entity.IsMercenary() && entity.IsControlledByFriendlySidePlayer())
				{
					benchedCount++;
				}
			}
		}
		return benchedCount;
	}

	public override string GetTurnTimerCountdownText(float timeRemainingInTurn)
	{
		if (m_gamePhase == 3)
		{
			return GameStrings.Get("GAMEPLAY_LETTUCE_COMBAT_BUTTON");
		}
		return null;
	}

	public override bool GetAlternativeEndTurnButtonText(out string myTurnText, out string waitingText)
	{
		myTurnText = GameStrings.Get("GAMEPLAY_LETTUCE_READY_BUTTON");
		waitingText = GameStrings.Get("GAMEPLAY_LETTUCE_WAIT_BUTTON");
		if (m_gamePhase == 1)
		{
			int nominatedCount = GetNominatedMercenariesCount();
			int benchedCount = GetBenchedMercenariesCount();
			int maxCount = GameState.Get().GetLocalSidePlayer().GetTag(GAME_TAG.LETTUCE_MAX_IN_PLAY_MERCENARIES);
			maxCount = Math.Min(nominatedCount + benchedCount, maxCount);
			if (nominatedCount < maxCount)
			{
				myTurnText = GameStrings.Format("GAMEPLAY_LETTUCE_MERCENARY_SELECT_BUTTON", nominatedCount, maxCount);
			}
		}
		else if (m_gamePhase == 2)
		{
			myTurnText = GameStrings.Get("GAMEPLAY_LETTUCE_FIGHT_BUTTON");
		}
		else if (m_gamePhase == 3)
		{
			waitingText = string.Empty;
		}
		return true;
	}

	public override bool ShouldOverwriteEndTurnButtonNoMorePlaysState(out bool hasNoMorePlay)
	{
		hasNoMorePlay = false;
		if (GameState.Get().IsActionStep())
		{
			switch (GetTag<ACTION_STEP_TYPE>(GAME_TAG.ACTION_STEP_TYPE))
			{
			case ACTION_STEP_TYPE.DEFAULT:
				if (GetNextPendingMercenary() == null)
				{
					hasNoMorePlay = true;
					return true;
				}
				break;
			case ACTION_STEP_TYPE.LETTUCE_MERCENARY_SELECTION:
			{
				int nominatedCount = GetNominatedMercenariesCount();
				int benchedMercenariesCount = GetBenchedMercenariesCount();
				int maxCount = GameState.Get().GetLocalSidePlayer().GetTag(GAME_TAG.LETTUCE_MAX_IN_PLAY_MERCENARIES);
				maxCount = Math.Min(benchedMercenariesCount + nominatedCount, maxCount);
				if (nominatedCount >= maxCount)
				{
					hasNoMorePlay = true;
					return true;
				}
				break;
			}
			}
		}
		return false;
	}

	public override bool ShouldAutoCorrectZone(Zone zone)
	{
		if (zone is ZoneLettuceAbility)
		{
			return false;
		}
		if (zone is ZoneDeck)
		{
			return false;
		}
		if (zone is ZoneHand)
		{
			return false;
		}
		return true;
	}

	public override bool OverwriteZoneDeckToAcceptEntity(ZoneDeck deckZone, int controllerId, TAG_ZONE zoneTag, TAG_CARDTYPE cardType, Entity entity)
	{
		if ((zoneTag == TAG_ZONE.DECK || zoneTag == TAG_ZONE.SETASIDE) && entity.IsMercenary())
		{
			Player controller = GameState.Get().GetPlayer(controllerId);
			if (controller == null)
			{
				return false;
			}
			if (controller.IsOpposingSide() && deckZone == ZoneMgr.Get().FindZoneOfType<ZoneDeck>(Player.Side.OPPOSING))
			{
				return true;
			}
		}
		return false;
	}

	public override bool OverwriteEndTurnReminder(Entity entity, out bool showReminder)
	{
		showReminder = false;
		return true;
	}

	public void AddInputBlockerFriendlyAbilityZone()
	{
		GameState.Get().GetFriendlySidePlayer().GetLettuceAbilityZone()
			.AddInputBlocker();
	}

	public void RemoveInputBlockerFriendlyAbilityZone()
	{
		GameState.Get().GetFriendlySidePlayer().GetLettuceAbilityZone()
			.RemoveInputBlocker();
	}

	protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
	{
		Board.Get().ChangeBoardVisualState(TAG_BOARD_VISUAL_STATE.SHOP);
		SetFullScreenFXForPreparation();
		yield return TweenCameraZoom(zoomedIn: false);
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		switch (missionEvent)
		{
		case 1:
			m_gamePhase = missionEvent;
			yield return OnNominationPhase();
			break;
		case 2:
			m_gamePhase = missionEvent;
			yield return OnPreparationPhase();
			break;
		case 3:
			m_gamePhase = missionEvent;
			yield return OnCombatPhase();
			break;
		}
	}

	private IEnumerator OnNominationPhase()
	{
		EndTurnButton.Get().UpdateButtonText();
		TurnTimer.Get().OnMercenariesPhaseChange();
		ZoneMgr.Get().ClearLocalChangeListHistory();
		ShiftPlayZoneForGamePhase(1);
		SetFullScreenFXForPreparation();
		Board.Get().ChangeBoardVisualState(TAG_BOARD_VISUAL_STATE.COMBAT);
		GameEntity.Coroutines.StartCoroutine(TweenCameraZoom(zoomedIn: false));
		yield break;
	}

	private IEnumerator OnPreparationPhase()
	{
		EndTurnButton.Get().UpdateButtonText();
		TurnTimer.Get().OnMercenariesPhaseChange();
		ZoneMgr.Get().ClearLocalChangeListHistory();
		ShiftPlayZoneForGamePhase(2);
		string phasePopupState = ((!HasTag(GAME_TAG.LETTUCE_OVERTIME)) ? "Prep" : "Fatigue");
		ShowPopup(phasePopupState);
		SetFullScreenFXForPreparation();
		ShowAllMercenaryAbilityOrderBubbles();
		Board.Get().ChangeBoardVisualState(TAG_BOARD_VISUAL_STATE.SHOP);
		GameEntity.Coroutines.StartCoroutine(TweenCameraZoom(zoomedIn: false));
		yield break;
	}

	private IEnumerator OnCombatPhase()
	{
		EndTurnButton.Get().UpdateButtonText();
		TurnTimer.Get().OnMercenariesPhaseChange();
		ZoneMgr.Get().ClearLocalChangeListHistory();
		ZoneMgr.Get().DismissMercenariesAbilityTray();
		m_prevSelectedCharacterZonePosition = 0;
		ShiftPlayZoneForGamePhase(3);
		ShowPopup("Combat");
		SetFullScreenFXForCombat();
		HideAllMercenaryAbilityOrderBubbles();
		Board.Get().ChangeBoardVisualState(TAG_BOARD_VISUAL_STATE.COMBAT);
		GameEntity.Coroutines.StartCoroutine(TweenCameraZoom(zoomedIn: true));
		yield break;
	}

	private void ShowOpposingFakeHand()
	{
		StartBlockingPowerProcessing();
		m_fakeHandController.ShowOpposingFakeHand(StopBlockingPowerProcessingIfPossible);
	}

	private void HideOpposingFakeHand()
	{
		StartBlockingPowerProcessing();
		m_fakeHandController.HideOpposingFakeHand(StopBlockingPowerProcessingIfPossible);
	}

	private IEnumerator TweenCameraZoom(bool zoomedIn)
	{
		if (!(BoardCameras.Get() == null))
		{
			float finalFieldOfView = (zoomedIn ? BoardCameras.Get().m_FieldOfViewZoomed : BoardCameras.Get().m_FieldOfViewDefault);
			m_isCameraShifting = true;
			yield return BoardCameras.Get().TweenCameraFieldOfView(finalFieldOfView, 0.5f);
			m_isCameraShifting = false;
		}
	}

	private void ShiftPlayZoneForGamePhase(int phase)
	{
		if (Gameplay.Get() == null || ZoneMgr.Get() == null)
		{
			return;
		}
		Transform friendlyZoneBone = null;
		Transform opposingZoneBone = null;
		if (phase == 2)
		{
			friendlyZoneBone = Gameplay.Get().GetBoardLayout().FindBone("FriendlyPlay_Prep");
			opposingZoneBone = Gameplay.Get().GetBoardLayout().FindBone("OpposingPlay_Prep");
		}
		else
		{
			friendlyZoneBone = Gameplay.Get().GetBoardLayout().FindBone("FriendlyPlay_Combat");
			opposingZoneBone = Gameplay.Get().GetBoardLayout().FindBone("OpposingPlay_Combat");
		}
		ZonePlay friendlyZonePlay = ZoneMgr.Get().FindZoneOfType<ZonePlay>(Player.Side.FRIENDLY);
		ZonePlay opposingZonePlay = ZoneMgr.Get().FindZoneOfType<ZonePlay>(Player.Side.OPPOSING);
		m_numPlayActorShifting = 0;
		List<Tuple<Card, Spell>> cardsToPlaySpell = new List<Tuple<Card, Spell>>();
		if (!Mathf.Approximately(friendlyZonePlay.transform.position.z, friendlyZoneBone.transform.position.z))
		{
			SpellType spellType = ((phase == 2) ? SpellType.MERCENARIES_PHASE_TRANSITION_MOVE_DOWN : SpellType.MERCENARIES_PHASE_TRANSITION_MOVE_UP);
			foreach (Card card in friendlyZonePlay.GetCards())
			{
				Spell spell = card.GetActorSpell(spellType);
				if (spell != null)
				{
					cardsToPlaySpell.Add(new Tuple<Card, Spell>(card, spell));
				}
			}
		}
		if (!Mathf.Approximately(opposingZonePlay.transform.position.z, opposingZoneBone.transform.position.z))
		{
			SpellType spellType2 = ((phase == 2) ? SpellType.MERCENARIES_PHASE_TRANSITION_MOVE_UP : SpellType.MERCENARIES_PHASE_TRANSITION_MOVE_DOWN);
			foreach (Card card2 in opposingZonePlay.GetCards())
			{
				Spell spell2 = card2.GetActorSpell(spellType2);
				if (spell2 != null)
				{
					cardsToPlaySpell.Add(new Tuple<Card, Spell>(card2, spell2));
				}
			}
		}
		friendlyZonePlay.transform.position = friendlyZoneBone.transform.position;
		opposingZonePlay.transform.position = opposingZoneBone.transform.position;
		m_numPlayActorShifting = cardsToPlaySpell.Count;
		if (m_numPlayActorShifting > 0)
		{
			StartBlockingPowerProcessing();
			GameEntity.Coroutines.StartCoroutine(WaitForZoneThenShiftActorsInPlay(cardsToPlaySpell));
		}
		else
		{
			friendlyZonePlay.UpdateLayout();
			opposingZonePlay.UpdateLayout();
		}
	}

	private IEnumerator WaitForZoneThenShiftActorsInPlay(List<Tuple<Card, Spell>> cardsToPlaySpell)
	{
		ZonePlay friendlyPlayZone = ZoneMgr.Get().FindZoneOfType<ZonePlay>(Player.Side.FRIENDLY);
		while (friendlyPlayZone.IsUpdatingLayout())
		{
			yield return null;
		}
		ZonePlay opposingPlayZone = ZoneMgr.Get().FindZoneOfType<ZonePlay>(Player.Side.OPPOSING);
		while (opposingPlayZone.IsUpdatingLayout())
		{
			yield return null;
		}
		foreach (Tuple<Card, Spell> item2 in cardsToPlaySpell)
		{
			Card card = item2.Item1;
			Spell item = item2.Item2;
			card.SetTransitionStyle(ZoneTransitionStyle.INSTANT);
			item.AddFinishedCallback(OnSpellFinished_ShiftActorInPlay);
			item.ActivateState(SpellStateType.BIRTH);
		}
	}

	private void OnSpellFinished_ShiftActorInPlay(Spell spell, object userData)
	{
		m_numPlayActorShifting--;
		if (m_numPlayActorShifting <= 0)
		{
			ZoneMgr.Get().FindZoneOfType<ZonePlay>(Player.Side.FRIENDLY).UpdateLayout();
			ZoneMgr.Get().FindZoneOfType<ZonePlay>(Player.Side.OPPOSING).UpdateLayout();
			StopBlockingPowerProcessingIfPossible();
		}
	}

	protected bool ShouldSortAbilitiesLowToHigh()
	{
		bool sortLowToHigh = true;
		GameEntity gameEntity = GameState.Get()?.GetGameEntity();
		if (gameEntity != null)
		{
			sortLowToHigh = !gameEntity.HasTag(GAME_TAG.LETTUCE_COMBAT_FROM_HIGH_TO_LOW);
		}
		return sortLowToHigh;
	}

	public virtual void UpdateAllMercenaryAbilityOrderBubbleText(bool hideUnselectedAbilityBubbles = false)
	{
		if (m_gamePhase == 3 || !m_abilityOrderSpeechBubblesEnabled)
		{
			return;
		}
		List<Card> minionsInPlay = GetAllMinionsInPlay();
		minionsInPlay.Sort(new CardSpeedCamparer(ShouldSortAbilitiesLowToHigh()));
		for (int i = 0; i < minionsInPlay.Count; i++)
		{
			Card minionInPlay = minionsInPlay[i];
			int order = minionsInPlay.IndexOf(minionInPlay) + 1;
			bool orderIsTied = false;
			if (i > 0)
			{
				Card previousMinionInPlay = minionsInPlay[i - 1];
				if (minionInPlay.GetPreparedLettuceAbilitySpeedValue() == previousMinionInPlay.GetPreparedLettuceAbilitySpeedValue())
				{
					orderIsTied = true;
					order = previousMinionInPlay.GetLettuceAbilityActionOrder();
					previousMinionInPlay.SetLettuceAbilityActionOrder(order, orderIsTied);
				}
			}
			minionInPlay.SetLettuceAbilityActionOrder(order, orderIsTied);
		}
		foreach (Card minionInPlay2 in minionsInPlay)
		{
			if (m_enemyAbilityOrderSpeechBubblesEnabled || !minionInPlay2.GetEntity().IsControlledByOpposingSidePlayer())
			{
				minionInPlay2.UpdateLettuceSpeechBubbleText(hideUnselectedAbilityBubbles);
			}
		}
	}

	private void HideAllMercenaryAbilityOrderBubbles()
	{
		foreach (Card item in GetAllMinionsInPlay())
		{
			Spell speechBubbleSpell = item.GetActorSpell(SpellType.MERCENARIES_SPEECH_BUBBLE, loadIfNeeded: false);
			if (speechBubbleSpell != null)
			{
				speechBubbleSpell.ActivateState(SpellStateType.DEATH);
			}
		}
	}

	public void ShowAllMercenaryAbilityOrderBubbles()
	{
		if (!m_abilityOrderSpeechBubblesEnabled)
		{
			return;
		}
		foreach (Card card in GetAllMinionsInPlay())
		{
			if (m_enemyAbilityOrderSpeechBubblesEnabled || !card.GetEntity().IsControlledByOpposingSidePlayer())
			{
				Spell speechBubbleSpell = card.GetActorSpell(SpellType.MERCENARIES_SPEECH_BUBBLE);
				if (speechBubbleSpell != null)
				{
					speechBubbleSpell.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmString("Text").Value = string.Empty;
					speechBubbleSpell.ActivateState(SpellStateType.BIRTH);
				}
			}
		}
	}

	protected List<Card> GetAllMinionsInPlay()
	{
		if (GameState.Get() == null || GameState.Get().GetFriendlySidePlayer() == null || GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone() == null || GameState.Get().GetOpposingSidePlayer() == null || GameState.Get().GetOpposingSidePlayer().GetBattlefieldZone() == null)
		{
			return new List<Card>();
		}
		List<Card> list = new List<Card>();
		list.AddRange(GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone()
			.GetCards());
		list.AddRange(GameState.Get().GetOpposingSidePlayer().GetBattlefieldZone()
			.GetCards());
		return list;
	}

	private void SetFullScreenFXForCombat()
	{
		VignetteParameters? vignette = new VignetteParameters(1.25f);
		ScreenEffectParameters screenEffectParameters = new ScreenEffectParameters(ScreenEffectType.VIGNETTE, ScreenEffectPassLocation.PERSPECTIVE, 0.5f, iTween.EaseType.easeOutCirc, null, vignette, null, null);
		m_screenEffectsHandle.StartEffect(screenEffectParameters);
	}

	private void SetFullScreenFXForPreparation()
	{
		m_screenEffectsHandle.StopEffect(0.5f, iTween.EaseType.easeOutCirc);
	}

	public override void NotifyOfResetGameStarted()
	{
		base.NotifyOfResetGameStarted();
		SetFullScreenFXForPreparation();
	}

	public override string GetAttackSpellControllerOverride(Entity attacker)
	{
		if (attacker == null)
		{
			return null;
		}
		if (!attacker.IsLettuceMercenary())
		{
			return null;
		}
		int level = GameUtils.GetMercenaryLevelFromExperience(attacker.GetTag(GAME_TAG.LETTUCE_MERCENARY_EXPERIENCE));
		if (level > 20)
		{
			return "AttackSpellController_Mercenaries_HighLevel.prefab:a1d93a294c041f740ba2ea9e2756a3ce";
		}
		if (level > 10)
		{
			return "AttackSpellController_Mercenaries_MidLevel.prefab:ee14d0ca7c274cd49a45beab7d4bc422";
		}
		if (level > 0)
		{
			return "AttackSpellController_Mercenaries_LowLevel.prefab:f63eae2726570f548984f477386eca7e";
		}
		return null;
	}

	public override ZonePlay.PlayZoneSizeOverride GetPlayZoneSizeOverride()
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			return null;
		}
		return new ZonePlay.PlayZoneSizeOverride
		{
			m_scale = 1.15f,
			m_slotWidthModifier = 1.15f
		};
	}

	public void ShowWeaknessSplatsForMercenary(Entity pointOfViewMercenary)
	{
		ZonePlay enemyPlayZone = ZoneMgr.Get().FindZoneOfType<ZonePlay>(Player.Side.OPPOSING);
		if (!(enemyPlayZone != null))
		{
			return;
		}
		foreach (Card card in enemyPlayZone.GetCards())
		{
			bool hasAdvantage = false;
			hasAdvantage = pointOfViewMercenary.IsMyLettuceRoleStrongAgainst(card.GetEntity());
			if (!hasAdvantage)
			{
				foreach (Entity ent in card.GetEntity().GetEnchantments())
				{
					if (ent.GetCardId().Equals("LETL_000_07e") && ent.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1) == pointOfViewMercenary.GetEntityId())
					{
						hasAdvantage = true;
						break;
					}
				}
			}
			if (hasAdvantage && !card.GetEntity().IsDormant() && !card.GetEntity().HasTag(GAME_TAG.UNTOUCHABLE))
			{
				card.ShowWeaknessSplat();
			}
		}
	}

	public void HideWeaknessSplats()
	{
		ZonePlay enemyPlayZone = ZoneMgr.Get().FindZoneOfType<ZonePlay>(Player.Side.OPPOSING);
		if (enemyPlayZone != null)
		{
			foreach (Card card in enemyPlayZone.GetCards())
			{
				card.HideWeaknessSplat();
			}
		}
		ZonePlay friendlyPlayZone = ZoneMgr.Get().FindZoneOfType<ZonePlay>(Player.Side.FRIENDLY);
		if (!(friendlyPlayZone != null))
		{
			return;
		}
		foreach (Card card2 in friendlyPlayZone.GetCards())
		{
			card2.HideWeaknessSplat();
		}
	}

	public override void NotifyOfCardMousedOver(Entity mousedOverEntity)
	{
		base.NotifyOfCardMousedOver(mousedOverEntity);
		if (mousedOverEntity.GetCard().GetZone() is ZoneHand && mousedOverEntity.IsMercenary())
		{
			ShowWeaknessSplatsForMercenary(mousedOverEntity);
		}
	}

	public override void NotifyOfCardMousedOff(Entity mousedOffEntity)
	{
		base.NotifyOfCardMousedOff(mousedOffEntity);
		if (mousedOffEntity.GetCard().GetZone() is ZoneHand && mousedOffEntity.IsMercenary())
		{
			HideWeaknessSplats();
		}
	}

	public override void NotifyOfCardGrabbed(Entity entity)
	{
		base.NotifyOfCardGrabbed(entity);
		if (entity.GetCard().GetZone() is ZoneHand && entity.IsMercenary())
		{
			ShowWeaknessSplatsForMercenary(entity);
		}
	}

	public override void NotifyOfCardDropped(Entity entity)
	{
		base.NotifyOfCardDropped(entity);
		if (entity.GetCard().GetZone() is ZoneHand && entity.IsMercenary())
		{
			HideWeaknessSplats();
		}
	}

	public override bool OverwriteCurrentPlayer(Player player, out bool isCurrentPlayer)
	{
		isCurrentPlayer = true;
		return true;
	}

	public override bool Overwrite_IsInZone_ForInputManager(Entity entity, TAG_ZONE zoneTag, TAG_ZONE finalZoneTag, out bool isInZone)
	{
		isInZone = false;
		if (zoneTag == TAG_ZONE.PLAY && finalZoneTag == TAG_ZONE.LETTUCE_ABILITY && entity.IsLettuceAbility())
		{
			Entity abilitySourceEntity = ZoneMgr.Get().GetLettuceAbilitiesSourceEntity();
			if (abilitySourceEntity != null && abilitySourceEntity == entity.GetLettuceAbilityOwner())
			{
				isInZone = true;
				return true;
			}
		}
		return false;
	}

	protected void CreateTutorialDialog(AssetReference assetPrefab, string headlineGameString, string bodyTextGameString, string buttonGameString, UIEvent.Handler buttonHandler, UnityEngine.Vector2 materialOffset)
	{
		GameObject actorObject = AssetLoader.Get().InstantiatePrefab(assetPrefab);
		if (actorObject == null)
		{
			Debug.LogError("Unable to load tutorial dialog TutorialIntroDialog prefab.");
			return;
		}
		TutorialNotification notification = actorObject.GetComponent<TutorialNotification>();
		if (notification == null)
		{
			Debug.LogError("TutorialNotification component does not exist on TutorialIntroDialog prefab.");
			return;
		}
		TransformUtil.AttachAndPreserveLocalTransform(actorObject.transform, OverlayUI.Get().m_heightScale.m_Center);
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			actorObject.transform.localScale = 1.5f * actorObject.transform.localScale;
		}
		m_popupTutorialNotification = notification;
		notification.headlineUberText.Text = GameStrings.Get(headlineGameString);
		notification.speechUberText.Text = GameStrings.Get(bodyTextGameString);
		notification.m_ButtonStart.SetText(GameStrings.Get(buttonGameString));
		notification.artOverlay.GetMaterial().mainTextureOffset = materialOffset;
		notification.m_ButtonStart.AddEventListener(UIEventType.RELEASE, delegate(UIEvent e)
		{
			buttonHandler?.Invoke(e);
			notification.m_ButtonStart.ClearEventListeners();
			NotificationManager.Get().DestroyNotification(notification, 0f);
			UpdateAllMercenaryAbilityOrderBubbleText();
		});
		m_popupTutorialNotification.PlayBirth();
		UniversalInputManager.Get().SetGameDialogActive(active: true);
	}

	protected void DestroyNotification(Notification notification, bool hideImmediately = false)
	{
		if (notification != null)
		{
			if (hideImmediately)
			{
				NotificationManager.Get().DestroyNotificationNowWithNoAnim(notification);
			}
			else
			{
				NotificationManager.Get().DestroyNotification(notification, 0f);
			}
		}
	}

	protected bool IsAnyFriendlyAbilitySelected()
	{
		foreach (Card card in GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone()
			.GetCards())
		{
			Entity entity = card.GetEntity();
			if (entity != null && entity.GetSelectedLettuceAbilityID() != 0)
			{
				return true;
			}
		}
		return false;
	}

	protected Card GetLeftMostMinionInFriendlyPlay()
	{
		foreach (Card card in GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone()
			.GetCards())
		{
			if (card.GetEntity().GetTag(GAME_TAG.ZONE_POSITION) == 1)
			{
				return card;
			}
		}
		return null;
	}

	protected Card GetRightMostMinionInFriendlyPlay()
	{
		List<Card> cardsInPlay = GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone()
			.GetCards();
		foreach (Card card in cardsInPlay)
		{
			if (card.GetEntity().GetTag(GAME_TAG.ZONE_POSITION) == cardsInPlay.Count)
			{
				return card;
			}
		}
		return null;
	}

	protected Card GetAbilityButtonBySlot(int abilitySlot)
	{
		List<Card> displayedAbilityCards = ZoneMgr.Get().GetDisplayedLettuceAbilityCards();
		if (abilitySlot >= displayedAbilityCards.Count)
		{
			return null;
		}
		return displayedAbilityCards[abilitySlot];
	}

	protected void GetSpeakersForTeams(List<int> teams, EmoteType emoteType, out Card enemySpeaker, out Card friendlySpeaker)
	{
		enemySpeaker = (friendlySpeaker = null);
		if (teams == null || teams.Count == 0 || emoteType == EmoteType.INVALID)
		{
			return;
		}
		List<Card> list = new List<Card>();
		list.AddRange(GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone()
			.GetCards());
		list.AddRange(GameState.Get().GetOpposingSidePlayer().GetBattlefieldZone()
			.GetCards());
		List<Card> friendlyMinions = new List<Card>();
		List<Card> enemyMinions = new List<Card>();
		foreach (Card card in list)
		{
			if (card == null)
			{
				continue;
			}
			Entity entity = card.GetEntity();
			if (!entity.IsMercenary() || entity.GetTag<TAG_PREMIUM>(GAME_TAG.PREMIUM) != TAG_PREMIUM.DIAMOND || card.GetEmoteEntry(emoteType) == null || (emoteType == EmoteType.START && card.GetEmoteEntry(EmoteType.MIRROR_START) == null))
			{
				continue;
			}
			int team = card.GetController().GetTeamId();
			if (teams.Contains(team))
			{
				if (card.GetEntity().IsControlledByFriendlySidePlayer())
				{
					friendlyMinions.Add(card);
				}
				else
				{
					enemyMinions.Add(card);
				}
			}
		}
		if (friendlyMinions.Count > 0)
		{
			friendlySpeaker = friendlyMinions[UnityEngine.Random.Range(0, friendlyMinions.Count)];
		}
		if (enemyMinions.Count > 0)
		{
			enemySpeaker = enemyMinions[UnityEngine.Random.Range(0, enemyMinions.Count)];
		}
	}

	protected EmoteType ConvertHistoryVoBanterEventToEmoteType(PowerHistoryVoBanter.ClientEmoteEvent emoteType)
	{
		switch (emoteType)
		{
		case PowerHistoryVoBanter.ClientEmoteEvent.START:
			return EmoteType.START;
		case PowerHistoryVoBanter.ClientEmoteEvent.THREATEN:
			return EmoteType.THREATEN;
		case PowerHistoryVoBanter.ClientEmoteEvent.WELL_PLAYED:
			return EmoteType.WELL_PLAYED;
		case PowerHistoryVoBanter.ClientEmoteEvent.INVALID:
			return EmoteType.INVALID;
		default:
		{
			string message = MethodBase.GetCurrentMethod().ReflectedType.Name + "." + MethodBase.GetCurrentMethod().Name + "(): " + $"Unknown Vo Banter Emote type: {emoteType}. Unable to convert to {typeof(EmoteType)}.";
			Log.Gameplay.PrintWarning(message);
			return EmoteType.INVALID;
		}
		}
	}

	public void RegisterOnEmoteBanterPlayedEvent(OnEmoteBanterPlayedDelegate callback)
	{
		if (!m_onEmoteBanterPlayedCallbacks.Contains(callback))
		{
			m_onEmoteBanterPlayedCallbacks.Add(callback);
		}
	}

	public void UnregisterOnEmoteBanterPlayedEvent(OnEmoteBanterPlayedDelegate callback)
	{
		m_onEmoteBanterPlayedCallbacks.Remove(callback);
	}

	private void OnEmoteBanterPlayed(EmoteType emoteType, AudioSource audioSource)
	{
		OnEmoteBanterPlayedDelegate[] array = m_onEmoteBanterPlayedCallbacks.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](this, emoteType, audioSource);
		}
	}

	protected IEnumerator PlayEmoteBanterWithTiming(EmoteType emoteType, params Card[] speakers)
	{
		if (speakers == null)
		{
			yield break;
		}
		while (GameState.Get().IsBusy())
		{
			yield return null;
		}
		m_enemySpeaking = true;
		GameState.Get().SetBusy(busy: true);
		int i = 0;
		while (i < speakers.Length)
		{
			Card minion = speakers[i];
			if (!(minion == null))
			{
				if (i >= 1 && emoteType == EmoteType.START && speakers[i - 1] != null)
				{
					EntityDef currentEntityDef = speakers[i].GetEntity().GetEntityDef();
					int currentMercId = int.MinValue;
					if (currentEntityDef != null)
					{
						currentMercId = GameUtils.GetMercenaryIdFromCardId(GameUtils.TranslateCardIdToDbId(currentEntityDef.GetCardId()));
					}
					EntityDef previousEntityDef = speakers[i - 1].GetEntity().GetEntityDef();
					int previousMercId = int.MinValue;
					if (currentEntityDef != null)
					{
						previousMercId = GameUtils.GetMercenaryIdFromCardId(GameUtils.TranslateCardIdToDbId(previousEntityDef.GetCardId()));
					}
					if (currentMercId != int.MinValue && previousMercId != int.MinValue && currentMercId == previousMercId)
					{
						emoteType = EmoteType.MIRROR_START;
					}
				}
				CardSoundSpell soundSpell = minion.PlayEmote(emoteType);
				if (soundSpell != null && soundSpell.GetActiveAudioSource() != null)
				{
					OnEmoteBanterPlayed(emoteType, soundSpell.GetActiveAudioSource());
					yield return new WaitForSeconds(soundSpell.GetActiveAudioSource().clip.length);
				}
				if (i < speakers.Length - 1)
				{
					yield return new WaitForSeconds(0.25f);
				}
			}
			int num = i + 1;
			i = num;
		}
		GameState.Get().SetBusy(busy: false);
		m_enemySpeaking = false;
	}

	public bool OnVoBanter_TeamDialogue(List<int> teams, PowerHistoryVoBanter.ClientEmoteEvent emoteEvent)
	{
		EmoteType emote = ConvertHistoryVoBanterEventToEmoteType(emoteEvent);
		if (emote == EmoteType.INVALID)
		{
			return false;
		}
		if (teams == null || teams.Count == 0)
		{
			return false;
		}
		GetSpeakersForTeams(teams, emote, out var enemySpeaker, out var friendlySpeaker);
		GameEntity.Coroutines.StartCoroutine(PlayEmoteBanterWithTiming(emote, enemySpeaker, friendlySpeaker));
		return true;
	}

	public bool OnVoBanter_OneSpeaker(int speakerId, PowerHistoryVoBanter.ClientEmoteEvent emoteEvent)
	{
		EmoteType emote = ConvertHistoryVoBanterEventToEmoteType(emoteEvent);
		if (emote == EmoteType.INVALID)
		{
			return false;
		}
		Entity speaker = GameState.Get().GetEntity(speakerId);
		if (speaker == null)
		{
			return false;
		}
		if (speaker.GetZone() != TAG_ZONE.PLAY)
		{
			return false;
		}
		GameEntity.Coroutines.StartCoroutine(PlayEmoteBanterWithTiming(emote, speaker.GetCard()));
		return true;
	}
}
