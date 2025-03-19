using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Time;
using Blizzard.T5.Game.Spells;
using Blizzard.T5.Game.Spells.SuperSpells;
using Hearthstone.Progression;
using PegasusGame;
using UnityEngine;

public class TB_BaconShop : MissionEntity
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static Map<GameEntityOption, string> s_stringOptions = InitStringOptions();

	private readonly WaitForSeconds MAX_DESTROY_HERO_TIME = new WaitForSeconds(10f);

	private AssetReference BACON_PHASE_POPUP = new AssetReference("BaconTurnIndicator.prefab:6342ffe02abc782459036566466d277c");

	private static readonly AssetReference Bob_BrassRing_Quote = new AssetReference("Bob_BrassRing_Quote.prefab:89385ff7d67aa1e49bcf25bc15ca61f6");

	protected readonly string[] CHO_GALL_CONFIRMATION_STRINGS = new string[5] { "GAMEPLAY_MULLIGAN_CHO_GALL_1", "GAMEPLAY_MULLIGAN_CHO_GALL_2", "GAMEPLAY_MULLIGAN_CHO_GALL_3", "GAMEPLAY_MULLIGAN_CHO_GALL_4", "GAMEPLAY_MULLIGAN_CHO_GALL_5" };

	private string m_currentSessionChoGallString;

	protected int m_gamePhase = 1;

	private GameObject m_phasePopup;

	private bool m_gameplaySceneLoaded;

	private Coroutine m_destroyHeroTrackingCoroutine;

	protected Coroutine m_showOpposingHeroActorLegendaryVFXCoroutine;

	private DuosPortal m_duosPortal;

	private Notification m_techLevelCounter;

	private int m_displayedTechLevelNumber;

	private List<BaconHeroMulliganBestPlaceVisual> m_mulliganBestPlaceVisuals = new List<BaconHeroMulliganBestPlaceVisual>();

	private readonly EmoteType[] m_gameNotificationEmotes = new EmoteType[13]
	{
		EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_01,
		EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_02,
		EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_03,
		EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_04,
		EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_05,
		EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_06,
		EmoteType.BATTLEGROUNDS_VISUAL_TRIPLE,
		EmoteType.BATTLEGROUNDS_VISUAL_HOT_STREAK,
		EmoteType.BATTLEGROUNDS_VISUAL_KNOCK_OUT,
		EmoteType.BATTLEGROUNDS_VISUAL_BANANA,
		EmoteType.BATTLEGROUNDS_VISUAL_HERO_BUDDY,
		EmoteType.BATTLEGROUNDS_VISUAL_DOUBLE_HERO_BUDDY,
		EmoteType.BATTLEGROUNDS_VISUAL_QUEST_COMPLETE
	};

	private readonly EmoteType[] m_priorityEmotes = new EmoteType[1] { EmoteType.BATTLEGROUNDS_VISUAL_BANANA };

	private Map<int, bool> m_emotesAllowedForPlayer = new Map<int, bool>();

	private Map<int, QueueList<NotificationManager.SpeechBubbleOptions>> m_emotesQueuedForPlayer = new Map<int, QueueList<NotificationManager.SpeechBubbleOptions>>();

	private Map<int, LinkedList<NotificationManager.SpeechBubbleOptions>> m_gameNotificationsQueuedForPlayer = new Map<int, LinkedList<NotificationManager.SpeechBubbleOptions>>();

	private bool m_gameNotificationEmotesAllowed = true;

	private HashSet<string> m_heroesGreeted = new HashSet<string>();

	private HashSet<string> m_greetedByHeroes = new HashSet<string>();

	private GameObject m_duckObj;

	private SoundDucker m_fxDucker;

	private Coroutine m_optionsFailsafeCoroutine;

	private static readonly PlatformDependentValue<Vector3> BATTLEGROUNDS_MULLIGAN_ACTOR_SCALE = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(1.5f, 1.1f, 1.5f),
		Phone = new Vector3(0.9f, 1.1f, 0.9f)
	};

	private BaconGuideConfig m_GuideConfig;

	private string m_FavoriteGuideCardId;

	private long m_hasSeenInGameWinVO;

	private long m_hasSeenInGameLoseVO;

	private static readonly AssetReference GuideConfigManager = new AssetReference("GuideConfigManager.prefab:0ce1cf2cade0b7a4aab2f7eeda97b768");

	private static readonly AssetReference VO_DALA_BOSS_99h_Male_Human_FirstDefeat_01 = new AssetReference("VO_DALA_BOSS_99h_Male_Human_FirstDefeat_01.prefab:4ddd2298c91dc9649b98c65a0cef0760");

	private static readonly AssetReference VO_DALA_BOSS_99h_Male_Human_FirstVictory_01 = new AssetReference("VO_DALA_BOSS_99h_Male_Human_FirstVictory_01.prefab:e40b154f86185d3428ffa48867241f76");

	private static readonly AssetReference VO_DALA_BOSS_99h_Male_Human_Hire_01 = new AssetReference("VO_DALA_BOSS_99h_Male_Human_Hire_01.prefab:bfd9513b46b92e84da5f22e01a0387a4");

	private static readonly AssetReference VO_DALA_BOSS_99h_Male_Human_RecruitWork_01 = new AssetReference("VO_DALA_BOSS_99h_Male_Human_RecruitWork_01.prefab:a5e1a6db102be6d4495aa1cd7dc7ddfc");

	private static readonly AssetReference VO_DALA_BOSS_99h_Male_Human_ShopFirstTime_01 = new AssetReference("VO_DALA_BOSS_99h_Male_Human_ShopFirstTime_01.prefab:8070938a2c3ba2f4ea92b7f0b5fdf280");

	public BattlegroundsRatingChange RatingChangeData { get; set; }

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool>
		{
			{
				GameEntityOption.ALWAYS_SHOW_MULLIGAN_TIMER,
				true
			},
			{
				GameEntityOption.MULLIGAN_IS_CHOOSE_ONE,
				true
			},
			{
				GameEntityOption.MULLIGAN_TIMER_HAS_ALTERNATE_POSITION,
				true
			},
			{
				GameEntityOption.CARDS_IN_TOOLTIP_SHIFTED_DURING_MULLIGAN,
				true
			},
			{
				GameEntityOption.CARDS_IN_TOOLTIP_DONT_CYCLE_DURING_MULLIGAN,
				true
			},
			{
				GameEntityOption.MULLIGAN_HAS_HERO_LOBBY,
				true
			},
			{
				GameEntityOption.DIM_OPPOSING_HERO_DURING_MULLIGAN,
				true
			},
			{
				GameEntityOption.HANDLE_COIN,
				false
			},
			{
				GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS,
				true
			},
			{
				GameEntityOption.DO_OPENING_TAUNTS,
				false
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
				GameEntityOption.USE_COMPACT_ENCHANTMENT_BANNERS,
				true
			},
			{
				GameEntityOption.ALLOW_FATIGUE,
				false
			},
			{
				GameEntityOption.MOUSEOVER_DELAY_OVERRIDDEN,
				true
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
				GameEntityOption.HAS_ALTERNATE_ENEMY_EMOTE_ACTOR,
				true
			},
			{
				GameEntityOption.USES_PREMIUM_EMOTES,
				true
			},
			{
				GameEntityOption.CAN_SQUELCH_OPPONENT,
				true
			},
			{
				GameEntityOption.USES_BIG_CARDS,
				false
			},
			{
				GameEntityOption.DISPLAY_MULLIGAN_DETAIL_LABEL,
				true
			}
		};
	}

	private static Map<GameEntityOption, string> InitStringOptions()
	{
		return new Map<GameEntityOption, string>
		{
			{
				GameEntityOption.ALTERNATE_MULLIGAN_ACTOR_NAME,
				"Bacon_Leaderboard_Hero.prefab:776977f5238a24647adcd67933f7d4b0"
			},
			{
				GameEntityOption.ALTERNATE_MULLIGAN_LOBBY_ACTOR_NAME,
				"Bacon_Leaderboard_Hero.prefab:776977f5238a24647adcd67933f7d4b0"
			},
			{
				GameEntityOption.VICTORY_SCREEN_PREFAB_PATH,
				"BaconTwoScoop.prefab:1e3e06c045e65674f9a8afccb8bcdec4"
			},
			{
				GameEntityOption.DEFEAT_SCREEN_PREFAB_PATH,
				"BaconTwoScoop.prefab:1e3e06c045e65674f9a8afccb8bcdec4"
			},
			{
				GameEntityOption.RULEBOOK_POPUP_PREFAB_PATH,
				"BaconInfoPopup.prefab:d5b6f1d5443d48947891de53cdd6c323"
			},
			{
				GameEntityOption.DEFEAT_AUDIO_PATH,
				null
			}
		};
	}

	public TB_BaconShop()
	{
		m_gameOptions.AddOptions(s_booleanOptions, s_stringOptions);
		HistoryManager.Get().DisableHistory();
		PlayerLeaderboardManager.Get().SetEnabled(enabled: true);
		PlayerLeaderboardManager.Get().SetAllowFakePlayers(enabled: true);
		EndTurnButton.Get().SetDisabled(disabled: true);
		SceneMgr.Get().RegisterSceneLoadedEvent(OnGameplaySceneLoaded);
		InitializePhasePopup();
		InitailizeViewTeammate();
		InitializeTurnTimer();
		m_gamePhase = 1;
		GameEntity.Coroutines.StartCoroutine(OnShopPhase(expectStateChangeCallback: false));
		if (!GameMgr.Get().IsSpectator())
		{
			m_optionsFailsafeCoroutine = GameEntity.Coroutines.StartCoroutine(CollectOptionsFailsafe());
		}
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.BACON, GameSaveKeySubkeyId.BACON_HAS_SEEN_FIRST_VICTORY_TUTORIAL, out m_hasSeenInGameWinVO);
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.BACON, GameSaveKeySubkeyId.BACON_HAS_SEEN_FIRST_DEFEAT_TUTORIAL, out m_hasSeenInGameLoseVO);
		Network.Get().RequestGameRoundHistory();
		Network.Get().RequestRealtimeBattlefieldRaces();
		Network.Get().RegisterNetHandler(BattlegroundsRatingChange.PacketID.ID, OnBattlegroundsRatingChange);
		if (GameState.Get() != null)
		{
			GameState.Get().RegisterTurnChangedListener(OnTurnEnded);
		}
	}

	protected virtual string GetFavoriteBattlegroundsGuideSkinCardId()
	{
		return CollectionManager.Get().GetFavoriteBattlegroundsGuideSkinCardId();
	}

	private BaconGuideConfig GetGuideConfig()
	{
		if (m_GuideConfig != null)
		{
			return m_GuideConfig;
		}
		m_FavoriteGuideCardId = GetFavoriteBattlegroundsGuideSkinCardId();
		m_GuideConfig = LoadGuideConfig(m_FavoriteGuideCardId);
		return m_GuideConfig;
	}

	public static BaconGuideConfig LoadGuideConfig(string cardId)
	{
		BaconGuideConfigManager component = AssetLoader.Get().InstantiatePrefab(GuideConfigManager).GetComponent<BaconGuideConfigManager>();
		if (component == null)
		{
			Log.Gameplay.PrintError("TB_BaconShop: failed to load GuideConfigManager");
		}
		BaconGuideConfig guideConfig = component.GetGuideConfigForSkinCardId(cardId);
		UnityEngine.Object.Destroy(component);
		return guideConfig;
	}

	public override void OnDecommissionGame()
	{
		if (BaconBoard.Get() != null)
		{
			BaconBoard.Get().RemoveStateChangeCallback(OnStateChange);
		}
		if (SceneMgr.Get() != null)
		{
			SceneMgr.Get().UnregisterSceneLoadedEvent(OnGameplaySceneLoaded);
		}
		if (Network.Get() != null)
		{
			Network.Get().RemoveNetHandler(BattlegroundsRatingChange.PacketID.ID, OnBattlegroundsRatingChange);
		}
		if (GameState.Get() != null)
		{
			GameState.Get().UnregisterTurnChangedListener(OnTurnEnded);
		}
		StopAllDuosTutorialPopups();
		TimeScaleMgr.Get().SetGameTimeScale(1f);
		base.OnDecommissionGame();
	}

	private void OnGameplaySceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		if (mode == SceneMgr.Mode.GAMEPLAY)
		{
			m_gameplaySceneLoaded = true;
			SceneMgr.Get().UnregisterSceneLoadedEvent(OnGameplaySceneLoaded);
			ManaCrystalMgr.Get().SetEnemyManaCounterActive(active: false);
			OverrideZonePlayBaseTransitionTime();
			int nextOpponentId = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.NEXT_OPPONENT_PLAYER_ID);
			PlayerLeaderboardManager.Get().SetNextOpponent(nextOpponentId);
			GameState.Get().GetOpposingSidePlayer().SetCardBackId(GameState.Get().GetFriendlySidePlayer().GetOriginalCardBackId());
			if (BaconBoard.Get() != null)
			{
				BaconBoard.Get().AddStateChangeCallback(OnStateChange);
			}
		}
	}

	protected bool GetEnemyDeckTooltipContent(ref string headline, ref string description, int index)
	{
		switch (index)
		{
		case 0:
		{
			List<TAG_RACE> availableRaces = GameState.Get().GetAvailableRacesInBattlegroundsExcludingAmalgam();
			headline = GameStrings.Get("GAMEPLAY_TOOLTIP_BACON_AVAILABLE_RACES_HEADLINE");
			string separator = GameStrings.Get("GAMEPLAY_SEPARATOR") + " ";
			description = string.Join(separator, availableRaces.ConvertAll((TAG_RACE race) => GameStrings.GetRaceNameBattlegrounds(race)));
			return true;
		}
		case 1:
		{
			List<TAG_RACE> missingRaces = GameState.Get().GetMissingRacesInBattlegrounds();
			if (missingRaces.Count == 0)
			{
				return false;
			}
			headline = GameStrings.Get("GAMEPLAY_TOOLTIP_BACON_UNAVAILABLE_RACES_HEADLINE");
			string separator2 = GameStrings.Get("GAMEPLAY_SEPARATOR") + " ";
			description = string.Join(separator2, missingRaces.ConvertAll((TAG_RACE race) => GameStrings.GetRaceNameBattlegrounds(race)));
			return true;
		}
		case 2:
		{
			List<TAG_RACE> inactiveRaces = GameState.Get().GetInactiveRacesInBattlegrounds();
			if (inactiveRaces.Count == 0)
			{
				return false;
			}
			inactiveRaces.Sort((TAG_RACE a, TAG_RACE b) => string.Compare(GameStrings.GetRaceNameBattlegrounds(a), GameStrings.GetRaceNameBattlegrounds(b), StringComparison.Ordinal));
			headline = GameStrings.Get("GAMEPLAY_TOOLTIP_BACON_INACTIVE_RACES_HEADLINE");
			string separator3 = GameStrings.Get("GAMEPLAY_SEPARATOR") + " ";
			description = "";
			for (int i = 0; i < inactiveRaces.Count; i++)
			{
				description = string.Format("{0}{1}{2}", description, (i != 0) ? separator3 : "", GameStrings.GetRaceNameBattlegrounds(inactiveRaces[i]));
			}
			return true;
		}
		default:
			if (GameMgr.Get().IsBattlegroundDuoGame())
			{
				return GetFriendlyDeckTooltipContent(ref headline, ref description, index - 3);
			}
			return false;
		}
	}

	protected bool GetFriendlyDeckTooltipContent(ref string headline, ref string description, int index)
	{
		if (index == 0)
		{
			int turn = GameState.Get().GetTurn() / 2 + 1;
			if (m_gamePhase == 2)
			{
				turn--;
				headline = GameStrings.Format("GAMEPLAY_TOOLTIP_BACON_COMBAT_HEADLINE", turn);
			}
			else
			{
				headline = GameStrings.Format("GAMEPLAY_TOOLTIP_BACON_TURN_HEADLINE", turn);
			}
			description = "";
			return true;
		}
		bool darkmoonPrizesActive = GameState.Get().GetGameEntity().GetTag(GAME_TAG.DARKMOON_FAIRE_PRIZES_ACTIVE) == 1;
		if (index == 1 && darkmoonPrizesActive)
		{
			int deckCount = GameState.Get().GetFriendlySidePlayer().GetDeckZone()
				.GetCards()
				.Count;
			int numTurnsUntilPrize = 4 - deckCount;
			headline = GameStrings.Get("GAMEPLAY_TOOLTIP_BACON_DARKMOON_PRIZES_HEADLINE");
			description = GameStrings.Format("GAMEPLAY_TOOLTIP_BACON_DARKMOON_PRIZES_DESC", numTurnsUntilPrize);
			return true;
		}
		return false;
	}

	protected bool GetFriendlyManaTooltipContent(ref string headline, ref string description, int index)
	{
		if (index == 0)
		{
			headline = GameStrings.Get("GAMEPLAY_TOOLTIP_MANA_COIN_HEADLINE");
			description = GameStrings.Get("GAMEPLAY_TOOLTIP_BACON_GOLD");
			return true;
		}
		return false;
	}

	public override InputManager.ZoneTooltipSettings GetZoneTooltipSettings()
	{
		return new InputManager.ZoneTooltipSettings
		{
			EnemyDeck = new InputManager.TooltipSettings(allowed: true, GetEnemyDeckTooltipContent, 5),
			EnemyHand = new InputManager.TooltipSettings(allowed: false),
			EnemyMana = new InputManager.TooltipSettings(allowed: false),
			FriendlyDeck = new InputManager.TooltipSettings(allowed: true, GetFriendlyDeckTooltipContent, 2),
			FriendlyMana = new InputManager.TooltipSettings(allowed: true, GetFriendlyManaTooltipContent)
		};
	}

	public override string GetMulliganDetailText()
	{
		List<TAG_RACE> availableRaces = GameState.Get().GetAvailableRacesInBattlegroundsExcludingAmalgam();
		if (availableRaces.Count == 0)
		{
			return null;
		}
		availableRaces.Sort((TAG_RACE a, TAG_RACE b) => string.Compare(GameStrings.GetRaceNameBattlegrounds(a), GameStrings.GetRaceNameBattlegrounds(b), StringComparison.Ordinal));
		_ = GameStrings.Get("GAMEPLAY_SEPARATOR") + " ";
		string fullMulliganString = "";
		int length = fullMulliganString.Length;
		for (int x = 0; x < availableRaces.Count; x++)
		{
			TAG_RACE tag = availableRaces[x];
			if (length > 20)
			{
				fullMulliganString += "\n";
				length = 0;
			}
			string raceText = GameStrings.GetRaceNameBattlegrounds(tag);
			if (x < availableRaces.Count - 1)
			{
				raceText += ", ";
			}
			fullMulliganString += raceText;
			length += raceText.Length;
		}
		return fullMulliganString;
	}

	public override Vector3 NameBannerPosition(Player.Side side)
	{
		if (side == Player.Side.FRIENDLY)
		{
			return new Vector3(0f, 5f, 11f);
		}
		return base.NameBannerPosition(side);
	}

	public override Vector3 GetMulliganTimerAlternatePosition()
	{
		if (MulliganManager.Get() == null || MulliganManager.Get().GetMulliganBanner() == null)
		{
			return new Vector3(100f, 0f, 0f);
		}
		Vector3 offset = Vector3.zero;
		if (TeammateBoardViewer.Get() != null && TeammateBoardViewer.Get().IsViewingTeammate())
		{
			offset = TeammateBoardViewer.Get().GetTeammateBoardPosition();
		}
		if (GameState.Get().IsInChoiceMode() && MulliganManager.Get().GetMulliganButton() != null)
		{
			return MulliganManager.Get().GetMulliganButton().transform.position + offset;
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			return MulliganManager.Get().GetMulliganBanner().transform.position + new Vector3(-1.8f, 0f, -0.91f) + offset;
		}
		return MulliganManager.Get().GetMulliganBanner().transform.position + offset;
	}

	protected override Spell BlowUpHero(Card card, SpellType spellType)
	{
		if (card != null && card.GetActor() != null)
		{
			PlayMakerFSM heroPlaymaker = card.GetActor().GetComponent<PlayMakerFSM>();
			if (heroPlaymaker != null)
			{
				heroPlaymaker.enabled = false;
			}
		}
		if (m_optionsFailsafeCoroutine != null)
		{
			GameEntity.Coroutines.StopCoroutine(m_optionsFailsafeCoroutine);
		}
		if (GameState.Get().IsMulliganManagerActive())
		{
			Transform parent = card.GetActor().gameObject.transform.parent;
			parent.position = new Vector3(-7.7726f, 0.0055918f, -8.054f);
			parent.localScale = new Vector3(1.134f, 1.134f, 1.134f);
			MulliganManager.Get().StopAllCoroutines();
		}
		return base.BlowUpHero(card, spellType);
	}

	protected override Spell ActivateSpellForDestroyedHero(Card card, SpellType spellType)
	{
		if (spellType == SpellType.ENDGAME_LOSE_FRIENDLY && m_gamePhase == 2)
		{
			Entity opposingHero = GameState.Get().GetOpposingSidePlayer().GetHero();
			FinisherGameplaySettings finisherSettings = FinisherGameplaySettings.GetFinisherGameplaySettings(opposingHero);
			if (finisherSettings == null)
			{
				Debug.LogError($"Error [TB_BaconShop] ActivateSpellForDestroyedHero finisherSettings was null from {opposingHero}");
				return card.ActivateActorSpell(spellType);
			}
			if (finisherSettings.IgnoreDestroyPrefabs)
			{
				return null;
			}
			string spellPrefabName;
			if (!string.IsNullOrEmpty(finisherSettings.FirstPlaceVictoryDestroyPlayerPrefab) && GameState.Get().CountPlayersAlive() == 1)
			{
				spellPrefabName = finisherSettings.FirstPlaceVictoryDestroyPlayerPrefab;
			}
			else
			{
				if (string.IsNullOrEmpty(finisherSettings.DestroyPlayerPrefab))
				{
					return card.ActivateActorSpell(spellType);
				}
				spellPrefabName = finisherSettings.DestroyPlayerPrefab;
			}
			Spell spell = SpellManager.Get().GetSpell(spellPrefabName);
			if (spell == null)
			{
				Debug.LogError("Error [TB_BaconShop] ActivateSpellForDestroyedHero spell could not be found for " + spellPrefabName);
				return card.ActivateActorSpell(spellType);
			}
			spell.SetSpellType(spellType);
			Entity hero = GameState.Get().GetFriendlySidePlayer().GetHero();
			GameObject sourceObject = opposingHero.GetCard().gameObject;
			GameObject targetObject = hero.GetCard().gameObject;
			spell.SetSource(sourceObject);
			spell.Location = SpellLocation.SOURCE;
			if (spell is ISuperSpell superSpell)
			{
				superSpell.TargetInfo.Behavior = SpellTargetBehavior.DEFAULT;
			}
			spell.AddTarget(targetObject);
			spell.AddFinishedCallback(OnFriendlyHeroDestroyed);
			m_destroyHeroTrackingCoroutine = spell.StartCoroutine(EnsureHeroDestroyedCompletes(spell));
			spell.Activate();
			Card friendlyCard = GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard();
			Spell spell2 = base.ActivateSpellForDestroyedHero(friendlyCard, spellType);
			spell2.transform.parent = null;
			spell2.ActivateState(SpellStateType.ACTION);
			return spell;
		}
		return base.ActivateSpellForDestroyedHero(card, spellType);
	}

	private void OnFriendlyHeroDestroyed(Spell spell, object _)
	{
		spell.GetTarget().SetActive(value: false);
		if (m_destroyHeroTrackingCoroutine != null)
		{
			spell.StopCoroutine(m_destroyHeroTrackingCoroutine);
			m_destroyHeroTrackingCoroutine = null;
		}
		if (m_showOpposingHeroActorLegendaryVFXCoroutine != null)
		{
			StopCoroutine(m_showOpposingHeroActorLegendaryVFXCoroutine);
			m_showOpposingHeroActorLegendaryVFXCoroutine = null;
		}
	}

	private IEnumerator EnsureHeroDestroyedCompletes(Spell spell)
	{
		yield return MAX_DESTROY_HERO_TIME;
		m_destroyHeroTrackingCoroutine = null;
		Log.Spells.PrintError("Destroy hero spell " + spell.gameObject.name + " did not terminate and was killed to prevent game hang. Run the finisher in the authoring scene to diagnose potential problems.");
		spell.ReleaseSpell();
	}

	public override bool ShouldDelayShowingCardInTooltip()
	{
		if (GameState.Get().IsMulliganManagerActive())
		{
			return false;
		}
		return true;
	}

	public override ActorStateType GetMulliganChoiceHighlightState()
	{
		return ActorStateType.CARD_SELECTABLE;
	}

	public override void NotifyMulliganButtonReady()
	{
		LoadDuosPortal();
		PlayDuosTutorial(DuosPopupTutoiral.DUOS_TUTORIALS.VIEW_TEAMMATE);
	}

	public override bool IsHeroMulliganLobbyFinished()
	{
		if (!GameState.Get().IsMulliganPhase() || CountPlayersFinishedMulligan() == CountPlayersInGame())
		{
			return true;
		}
		return false;
	}

	public override bool IsTeammateHeroMulliganFinished()
	{
		if (!GameMgr.Get().IsBattlegroundDuoGame())
		{
			return true;
		}
		return GetFriendlyTeammateHeroEntity() != null;
	}

	public override Entity GetFriendlyTeammateHeroEntity()
	{
		if (!GameMgr.Get().IsBattlegroundDuoGame())
		{
			return null;
		}
		int teammateID = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.BACON_DUO_TEAMMATE_PLAYER_ID);
		foreach (SharedPlayerInfo sph in GameState.Get().GetPlayerInfoMap().Values)
		{
			if (sph.GetPlayerId() == teammateID)
			{
				return sph.GetPlayerHero();
			}
		}
		return null;
	}

	public override void NotifyHeroMulliganLobbyFinished()
	{
		if (TeammateBoardViewer.Get() != null && GameMgr.Get().IsBattlegroundDuoGame())
		{
			if (TeammateBoardViewer.Get().IsViewingTeammate())
			{
				m_duosPortal.PortalPushed();
			}
			m_duosPortal.SetPortalClickable(clickable: false);
			m_duosPortal.RemovePing();
			TeammateBoardViewer.Get().DeleteDiscoverEntities(includeChosen: true);
		}
	}

	public override void NotifyOfMulliganEnded()
	{
		if (TeammateBoardViewer.Get() != null && GameMgr.Get().IsBattlegroundDuoGame())
		{
			TeammateBoardViewer.Get().InitGameModeButtons();
			LoadDuosPortal();
			PlayDuosTutorial(DuosPopupTutoiral.DUOS_TUTORIALS.HEALTH, null, GetFriendlyHeroActor());
		}
	}

	public override void NotifyMulligainHeroSelected(Actor heroActor)
	{
		if ((bool)m_duosPortal)
		{
			m_duosPortal.SetHeroActor(heroActor);
		}
		heroActor.SetSkipArmorAnimationActive(active: false);
	}

	public override bool HeroRequiresDoubleConfirmation(Entity mulliganHeroEntity)
	{
		if (mulliganHeroEntity == null)
		{
			return false;
		}
		if (mulliganHeroEntity.GetEntityDef().GetCardId() == "BGDUO_HERO_222" || mulliganHeroEntity.GetEntityDef().GetCardId() == "BGDUO_HERO_223" || mulliganHeroEntity.GetTag(GAME_TAG.BACON_SKIN_PARENT_ID) == 104981 || mulliganHeroEntity.GetTag(GAME_TAG.BACON_SKIN_PARENT_ID) == 104982)
		{
			return true;
		}
		return false;
	}

	public override string GetMultiStepMulliganConfirmButtonText(Entity mulliganHeroEntity)
	{
		if (mulliganHeroEntity == null)
		{
			return null;
		}
		if (mulliganHeroEntity.GetEntityDef().GetCardId() == "BGDUO_HERO_222" || mulliganHeroEntity.GetEntityDef().GetCardId() == "BGDUO_HERO_223" || mulliganHeroEntity.GetTag(GAME_TAG.BACON_SKIN_PARENT_ID) == 104981 || mulliganHeroEntity.GetTag(GAME_TAG.BACON_SKIN_PARENT_ID) == 104982)
		{
			if (m_currentSessionChoGallString == null)
			{
				m_currentSessionChoGallString = CHO_GALL_CONFIRMATION_STRINGS[UnityEngine.Random.Range(0, CHO_GALL_CONFIRMATION_STRINGS.Length)];
			}
			return m_currentSessionChoGallString;
		}
		return null;
	}

	private int CountPlayersFinishedMulligan()
	{
		int numFinished = 0;
		foreach (SharedPlayerInfo value in GameState.Get().GetPlayerInfoMap().Values)
		{
			if (value.GetPlayerHero() != null)
			{
				numFinished++;
			}
		}
		return numFinished;
	}

	private int CountPlayersInGame()
	{
		return GameState.Get().GetPlayerInfoMap().Values.Count;
	}

	public override bool ShouldDoAlternateMulliganIntro()
	{
		return true;
	}

	public override bool DoAlternateMulliganIntro()
	{
		if (!ShouldDoAlternateMulliganIntro())
		{
			return false;
		}
		GameEntity.Coroutines.StartCoroutine(DoBaconAlternateMulliganIntroWithTiming());
		return true;
	}

	protected override void HandleMulliganTagChange()
	{
		MulliganManager.Get().BeginMulligan();
	}

	public override Vector3 GetAlternateMulliganActorScale()
	{
		return BATTLEGROUNDS_MULLIGAN_ACTOR_SCALE;
	}

	public override int GetNumberOfFakeMulliganCardsToShowOnLeft(int numOriginalCards)
	{
		if (numOriginalCards >= 3)
		{
			return 0;
		}
		return 1;
	}

	public override int GetNumberOfFakeMulliganCardsToShowOnRight(int numOriginalCards)
	{
		if (numOriginalCards >= 4)
		{
			return 0;
		}
		return 1;
	}

	public override void ConfigureFakeMulliganCardActor(Actor actor, bool shown)
	{
		PlayerLeaderboardMainCardActor playerLeaderboardActor = actor as PlayerLeaderboardMainCardActor;
		if (!(playerLeaderboardActor == null))
		{
			playerLeaderboardActor.ToggleLockedHeroView(shown);
		}
	}

	public override void ConfigureLockedMulliganCardActor(Actor actor, bool shown)
	{
		PlayerLeaderboardMainCardActor playerLeaderboardActor = actor as PlayerLeaderboardMainCardActor;
		if (!(playerLeaderboardActor == null))
		{
			playerLeaderboardActor.TogglePartiallyRevealedLockedHeroView(shown);
		}
	}

	public override bool IsGameSpeedupConditionInEffect()
	{
		if (Gameplay.Get() == null || GameState.Get() == null || GameState.Get().GetGameEntity() == null || !GameState.Get().GetGameEntity().HasTag(GAME_TAG.ALLOW_GAME_SPEEDUP))
		{
			return false;
		}
		return m_gamePhase == 2;
	}

	public override void ApplyMulliganActorStateChanges(Actor baseActor)
	{
		PlayerLeaderboardMainCardActor obj = (PlayerLeaderboardMainCardActor)baseActor;
		obj.SetAlternateNameTextActive(active: false);
		obj.m_playerNameBackground.SetActive(value: false);
		obj.m_nameTextMesh.gameObject.SetActive(value: true);
	}

	public override void ApplyMulliganActorLobbyStateChanges(Actor baseActor)
	{
		PlayerLeaderboardMainCardActor obj = (PlayerLeaderboardMainCardActor)baseActor;
		obj.SetAlternateNameTextActive(active: false);
		obj.m_nameTextMesh.gameObject.SetActive(value: false);
		obj.m_playerNameBackground.SetActive(value: true);
		obj.SetFullyHighlighted(highlighted: false);
		obj.SetConfirmHighlighted(highlighted: false);
	}

	public override void ClearMulliganActorStateChanges(Actor baseActor)
	{
		PlayerLeaderboardMainCardActor obj = (PlayerLeaderboardMainCardActor)baseActor;
		obj.SetAlternateNameTextActive(active: false);
		obj.m_nameTextMesh.gameObject.SetActive(value: false);
		obj.m_playerNameBackground.SetActive(value: false);
		obj.m_playerNameText.gameObject.SetActive(value: false);
		obj.SetFullyHighlighted(highlighted: false);
		obj.SetConfirmHighlighted(highlighted: false);
	}

	public override string GetMulliganBannerText()
	{
		return GameStrings.Get("GAMEPLAY_BACON_MULLIGAN_CHOOSE_HERO_BANNER");
	}

	public override string GetMulliganBannerSubtitleText()
	{
		return null;
	}

	public override string GetMulliganWaitingText()
	{
		return string.Format(GameStrings.Get("GAMEPLAY_BACON_MULLIGAN_WAITING_BANNER"), CountPlayersFinishedMulligan(), CountPlayersInGame());
	}

	public override string GetMulliganWaitingSubtitleText()
	{
		if (MulliganManager.Get() != null && MulliganManager.Get().IsMulliganTimerActive())
		{
			return GameStrings.Get("GAMEPLAY_BACON_MULLIGAN_WAITING_BANNER_SUBTITLE");
		}
		return null;
	}

	public override void QueueEntityForRemoval(Entity entity)
	{
		GameState.Get().QueueEntityForRemoval(entity);
	}

	protected IEnumerator DoBaconAlternateMulliganIntroWithTiming()
	{
		SceneMgr.Get().NotifySceneLoaded();
		MulliganManager.Get().LoadMulliganButton();
		LoadDuosPortal();
		while (LoadingScreen.Get().IsPreviousSceneActive() || LoadingScreen.Get().IsFadingOut())
		{
			yield return null;
		}
		GameMgr.Get().UpdatePresence();
		GameState.Get().GetGameEntity().NotifyOfHeroesFinishedAnimatingInMulligan();
		ScreenEffectsMgr.Get().SetActive(enabled: true);
	}

	public override void OnMulliganCardsDealt(List<Card> startingCards)
	{
		foreach (Card hero in startingCards)
		{
			AssetLoader.Get().InstantiatePrefab(new AssetReference("BaconHeroMulliganBestPlaceVisual.prefab:6e6437cf53cbc0e4fbf0b3d6ce5a6856"), OnBestPlaceVisualLoaded, hero);
		}
	}

	private void OnBestPlaceVisualLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		Card hero = (Card)callbackData;
		int heroDbId = GameUtils.TranslateCardIdToDbId(hero.GetEntity().GetCardId());
		int bestPlace = GetBestPlaceForHero(heroDbId);
		BaconHeroMulliganBestPlaceVisual visual = go.GetComponent<BaconHeroMulliganBestPlaceVisual>();
		m_mulliganBestPlaceVisuals.Add(visual);
		visual.SetVisualActive(bestPlace, heroDbId);
		GameUtils.SetParent(go, hero.gameObject);
	}

	private int GetBestPlaceForHero(int heroId)
	{
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.BACON, GameSaveKeySubkeyId.BACON_BEST_HERO_PLACE, out List<long> bestPlace);
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.BACON, GameSaveKeySubkeyId.BACON_BEST_HERO_PLACE_HERO, out List<long> bestPlaceHero);
		if (bestPlace == null || bestPlaceHero == null)
		{
			return int.MaxValue;
		}
		if (bestPlace.Count != bestPlaceHero.Count)
		{
			Debug.LogError("Error in GetBestPlaceForHero: List size mismatch!");
			return int.MaxValue;
		}
		for (int i = 0; i < bestPlaceHero.Count; i++)
		{
			if (bestPlaceHero[i] == heroId && i < bestPlace.Count)
			{
				return (int)bestPlace[i];
			}
		}
		return int.MaxValue;
	}

	public override void OnMulliganBeginDealNewCards()
	{
		foreach (BaconHeroMulliganBestPlaceVisual visual in m_mulliganBestPlaceVisuals)
		{
			if (visual != null)
			{
				visual.Hide();
			}
		}
	}

	public override bool ShouldPlayHeroBlowUpSpells(TAG_PLAYSTATE playState)
	{
		return playState != TAG_PLAYSTATE.WON;
	}

	private void OverrideZonePlayBaseTransitionTime()
	{
		if (GameState.Get() != null)
		{
			ZonePlay battlefieldZone = GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone();
			ZonePlay opposingPlayZone = GameState.Get().GetOpposingSidePlayer().GetBattlefieldZone();
			battlefieldZone.OverrideBaseTransitionTime(0.5f);
			battlefieldZone.ResetTransitionTime();
			opposingPlayZone.OverrideBaseTransitionTime(0.5f);
			opposingPlayZone.ResetTransitionTime();
		}
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		if (missionEvent == 1)
		{
			m_gamePhase = 1;
			yield return OnShopPhase(expectStateChangeCallback: true);
		}
		if (missionEvent == 5)
		{
			m_gamePhase = 1;
			yield return OnShopPhase(expectStateChangeCallback: false);
		}
		if (missionEvent == 2)
		{
			m_gamePhase = 2;
			yield return OnCombatPhase();
		}
		if (missionEvent == 3)
		{
			int tag = GetFreezeButtonCard().GetEntity().GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1);
			int maxUsage = GetFreezeButtonCard().GetEntity().GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_2);
			maxUsage--;
			if (tag >= maxUsage)
			{
				SetInputEnableForFrozenButton(isEnabled: false);
			}
			else
			{
				SetInputEnableForFrozenButton(isEnabled: false);
				yield return new WaitForSeconds(0.75f);
				SetInputEnableForFrozenButton(isEnabled: true);
			}
		}
		if (missionEvent == 4)
		{
			SetInputEnableForRefreshButton(isEnabled: false);
			yield return new WaitForSeconds(0.75f);
			SetInputEnableForRefreshButton(isEnabled: true);
		}
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor bobActor = GetBobActor();
		if (bobActor == null || bobActor.GetEntity() == null)
		{
			yield return null;
		}
		string voLine = null;
		string voLine2;
		Actor heroActor;
		switch (missionEvent)
		{
		case 101:
			if (ShouldPlayRateVO(0.25f) && !m_enemySpeaking)
			{
				voLine = GetGuideConfig().GetRandomShopUpgradeLine();
				yield return PlayBobLineWithOffsetBubble(voLine);
			}
			break;
		case 102:
			if (!m_enemySpeaking)
			{
				voLine = GetGuideConfig().GetHighestTierLine();
				yield return PlayBobLineWithOffsetBubble(voLine);
			}
			break;
		case 103:
			OnBoughtCard();
			if (ShouldPlayRateVO(0.15f) && !m_enemySpeaking)
			{
				voLine = GetGuideConfig().GetRandomRecruitSmallLine();
				yield return PlayBobLineWithOffsetBubble(voLine);
			}
			break;
		case 104:
			OnBoughtCard();
			if (ShouldPlayRateVO(0.2f) && !m_enemySpeaking)
			{
				voLine = GetGuideConfig().GetRandomRecruitMediumLine();
				yield return PlayBobLineWithOffsetBubble(voLine);
			}
			break;
		case 105:
			OnBoughtCard();
			if (ShouldPlayRateVO(0.25f) && !m_enemySpeaking)
			{
				voLine = GetGuideConfig().GetRandomRecruitLargeLine();
				yield return PlayBobLineWithOffsetBubble(voLine);
			}
			break;
		case 106:
			if (ShouldPlayRateVO(0.25f) && !m_enemySpeaking)
			{
				voLine = GetGuideConfig().GetRandomTripleLine();
				yield return PlayBobLineWithOffsetBubble(voLine);
			}
			break;
		case 107:
			if (ShouldPlayRateVO(0.15f) && !m_enemySpeaking)
			{
				voLine = GetGuideConfig().GetRandomSellingLine();
				yield return PlayBobLineWithOffsetBubble(voLine);
			}
			break;
		case 108:
			if (ShouldPlayRateVO(0.1f) && !m_enemySpeaking)
			{
				voLine = GetGuideConfig().GetRandomFreezingLine();
				yield return PlayBobLineWithOffsetBubble(voLine);
			}
			break;
		case 109:
			if (ShouldPlayRateVO(0.1f) && !m_enemySpeaking)
			{
				voLine = GetGuideConfig().GetRandomRefreshLine();
				yield return PlayBobLineWithOffsetBubble(voLine);
			}
			break;
		case 110:
			if (ShouldPlayRateVO(0.25f) && !m_enemySpeaking)
			{
				voLine = GetGuideConfig().GetRandomPossibleTripleLine();
				yield return PlayBobLineWithOffsetBubble(voLine);
			}
			break;
		case 111:
		{
			if (m_enemySpeaking)
			{
				break;
			}
			Actor friendlyActor = GetFriendlyHeroActor();
			if (friendlyActor != null && friendlyActor.LegendaryHeroSkinConfig != null && !friendlyActor.LegendaryHeroSkinConfig.CheckBartenderGreetLine(GetFavoriteBattlegroundsGuideSkinCardId(), out voLine2) && friendlyActor.LegendaryHeroSkinConfig.CheckStartGameLine(out voLine))
			{
				yield return PlayVOLineWithOffsetBubble(voLine, friendlyActor);
			}
			else
			{
				string heroCardID = GameState.Get().GetFriendlySidePlayer().GetHero()
					.GetCardId();
				string friendlyHeroCardBaseId = CollectionManager.Get().GetBattlegroundsBaseHeroCardId(heroCardID);
				if (heroCardID != friendlyHeroCardBaseId && GetGuideConfig().CheckHeroSpecificLine(heroCardID, out voLine))
				{
					yield return PlayBobLineWithOffsetBubble(voLine);
				}
				else if (GetGuideConfig().CheckHeroSpecificLine(friendlyHeroCardBaseId, out voLine))
				{
					yield return PlayBobLineWithOffsetBubble(voLine);
				}
				else
				{
					voLine = GetGuideConfig().GetRandomNewGameLine();
					yield return PlayBobLineWithOffsetBubble(voLine);
				}
			}
			if (friendlyActor != null && friendlyActor.LegendaryHeroSkinConfig != null && friendlyActor.LegendaryHeroSkinConfig.CheckBartenderGreetLine(GetFavoriteBattlegroundsGuideSkinCardId(), out voLine))
			{
				yield return PlayVOLineWithOffsetBubble(voLine, friendlyActor);
			}
			break;
		}
		case 112:
			if (!m_enemySpeaking)
			{
				voLine = GetGuideConfig().GetRandomPostCombatGeneralLine();
				yield return PlayBobLineWithOffsetBubble(voLine);
			}
			break;
		case 113:
		{
			Actor friendlyActor = GetFriendlyHeroActor();
			int friendlyPlayerId = GameState.Get().GetFriendlyPlayerId();
			int currentWinStreak = PlayerLeaderboardManager.Get().GetLatestWinStreakForPlayer(friendlyPlayerId);
			if (friendlyActor != null && friendlyActor.LegendaryHeroSkinConfig != null)
			{
				friendlyActor.LegendaryHeroSkinConfig.TryActivateVFX_WinStreak(currentWinStreak);
			}
			if (!m_enemySpeaking)
			{
				if (friendlyActor != null && friendlyActor.LegendaryHeroSkinConfig != null && friendlyActor.LegendaryHeroSkinConfig.CheckWinStreakLine(currentWinStreak, out voLine))
				{
					yield return PlayVOLineWithOffsetBubble(voLine, GetFriendlyHeroActor());
					break;
				}
				voLine = GetGuideConfig().GetRandomPostCombatWinLine();
				yield return PlayBobLineWithOffsetBubble(voLine);
			}
			break;
		}
		case 114:
			if (!m_enemySpeaking)
			{
				voLine = GetGuideConfig().GetRandomPostCombatLoseLine();
				yield return PlayBobLineWithOffsetBubble(voLine);
			}
			break;
		case 115:
			if (!m_enemySpeaking && !CheckHeroGreet(out heroActor, out voLine2, setGreeted: false))
			{
				voLine = GetGuideConfig().GetRandomPostShopGeneralLine();
				yield return PlayBobLineWithOffsetBubble(voLine);
			}
			break;
		case 116:
			if (!m_enemySpeaking && !CheckHeroGreet(out heroActor, out voLine2, setGreeted: false))
			{
				voLine = GetGuideConfig().GetRandomPostShopLoseLine();
				yield return PlayBobLineWithOffsetBubble(voLine);
			}
			break;
		case 117:
			if (!m_enemySpeaking && !CheckHeroGreet(out heroActor, out voLine2, setGreeted: false))
			{
				voLine = GetGuideConfig().GetRandomPostShopWinLine();
				yield return PlayBobLineWithOffsetBubble(voLine);
			}
			break;
		case 118:
			if (!m_enemySpeaking && !CheckHeroGreet(out heroActor, out voLine2, setGreeted: false))
			{
				voLine = GetGuideConfig().GetRandomPostShopIsFirstLine();
				yield return PlayBobLineWithOffsetBubble(voLine);
			}
			break;
		case 119:
			if (!m_enemySpeaking)
			{
				voLine = GetGuideConfig().GetAFKLine();
				yield return PlayBobLineWithOffsetBubble(voLine);
			}
			break;
		case 120:
			if (!m_enemySpeaking)
			{
				yield return HandleKnockoutVO();
			}
			break;
		case 121:
			if (!m_enemySpeaking)
			{
				yield return PlayWisdomballVOLine("VO_DALA_BOSS_60h_Male_Human_FloatingHead_Trigger_HealPlayer_01.prefab:d0a3f9b5c01e04d458178ca8c5069d66");
			}
			break;
		case 122:
			if (!m_enemySpeaking)
			{
				yield return PlayWisdomballVOLine("VO_DALA_BOSS_60h_Male_Human_FloatingHead_Trigger_SelfDamage_01.prefab:ce7a5a15de006d041ad515427fc6f72f");
			}
			break;
		case 123:
			if (!m_enemySpeaking)
			{
				yield return PlayWisdomballVOLine("VO_DALA_BOSS_60h_Male_Human_FloatingHead_Trigger_MirrorImage_01.prefab:8789714bb9a92d143bb2024188b8ddd0");
			}
			break;
		case 124:
			if (!m_enemySpeaking)
			{
				yield return PlayWisdomballVOLine("VO_DALA_BOSS_60h_Male_Human_FloatingHead_Trigger_CopyCards_01.prefab:ad01bc4d23eab3e4f86c994d722cf247");
			}
			break;
		case 125:
			if (!m_enemySpeaking)
			{
				yield return PlayWisdomballVOLine("VO_DALA_BOSS_60h_Male_Human_FloatingHead_Trigger_Treasure_01.prefab:b9fae030ab3026a4bb17f592028c276d");
			}
			break;
		case 126:
			if (!m_enemySpeaking)
			{
				yield return PlayWisdomballVOLine("VO_DALA_BOSS_60h_Male_Human_FloatingHead_Trigger_RandomLegendary_01.prefab:9273a8457f705514f9755153f0c7abf6");
			}
			break;
		case 127:
			PlayDuosTutorial(DuosPopupTutoiral.DUOS_TUTORIALS.BONUS_DAMAGE, null, null, null, Player.Side.FRIENDLY);
			break;
		case 128:
			PlayDuosTutorial(DuosPopupTutoiral.DUOS_TUTORIALS.BONUS_DAMAGE, null, null, null, Player.Side.OPPOSING);
			break;
		}
	}

	protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
	{
		AchievementManager.Get().UnpauseToastNotifications();
		PlayerLeaderboardManager.Get().UpdateLayout();
		if (m_duosPortal != null)
		{
			if (TeammateBoardViewer.Get().IsViewingTeammate())
			{
				m_duosPortal.PortalPushed();
			}
			m_duosPortal.SetPortalClickable(clickable: false);
		}
		if (gameResult == TAG_PLAYSTATE.WON && m_hasSeenInGameWinVO == 0L)
		{
			yield return new WaitForSeconds(5f);
			yield return Gameplay.Get().StartCoroutine(PlayBigCharacterQuoteAndWait(Bob_BrassRing_Quote, VO_DALA_BOSS_99h_Male_Human_FirstVictory_01));
		}
		int currentPlace = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetRealTimePlayerLeaderboardPlace();
		if (gameResult == TAG_PLAYSTATE.LOST && m_hasSeenInGameLoseVO == 0L && currentPlace > 4)
		{
			yield return new WaitForSeconds(5f);
			yield return Gameplay.Get().StartCoroutine(PlayBigCharacterQuoteAndWait(Bob_BrassRing_Quote, VO_DALA_BOSS_99h_Male_Human_FirstDefeat_01));
		}
	}

	public override void UpdateNameDisplay()
	{
		if (GameState.Get() != null && GameState.Get().GetOpposingSidePlayer() != null && GameState.Get().GetFriendlySidePlayer() != null)
		{
			GameState.Get().GetOpposingSidePlayer().UpdateDisplayInfo();
			GameState.Get().GetFriendlySidePlayer().UpdateDisplayInfo();
			UpdateNameBanner();
		}
	}

	public void SwitchedPlayersInCombat()
	{
		if (GameMgr.Get().IsBattlegroundDuoGame() && GameState.Get().GetGameEntity().IsInBattlegroundsCombatPhase())
		{
			int friendlyCombatPlayerId = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.BACON_CURRENT_COMBAT_PLAYER_ID);
			int opposingCombatPlayerId = GameState.Get().GetOpposingSidePlayer().GetTag(GAME_TAG.BACON_CURRENT_COMBAT_PLAYER_ID);
			int nextOpponentPlayerId = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.NEXT_OPPONENT_PLAYER_ID);
			int friendlyPlayerId = GameState.Get().GetFriendlySidePlayer().GetPlayerId();
			bool friendlyPlayerGoingFirst = GameState.Get().GetFriendlySidePlayer().HasTag(GAME_TAG.BACON_DUO_PLAYER_FIGHTS_FIRST_NEXT_COMBAT);
			bool swapedOpponentPlayer = opposingCombatPlayerId != nextOpponentPlayerId && opposingCombatPlayerId > 0;
			if ((friendlyCombatPlayerId != friendlyPlayerId && friendlyPlayerGoingFirst) || (friendlyCombatPlayerId == friendlyPlayerId && !friendlyPlayerGoingFirst))
			{
				PlayDuosTutorial(DuosPopupTutoiral.DUOS_TUTORIALS.HERO_SWAP, null, GetFriendlyHeroActor());
			}
			else if (swapedOpponentPlayer)
			{
				PlayDuosTutorial(DuosPopupTutoiral.DUOS_TUTORIALS.HERO_SWAP, null, GetOpposingHeroActor());
			}
		}
	}

	public void OnCardRecievedFromTeammate(Card passedCard)
	{
		PlayDuosTutorial(DuosPopupTutoiral.DUOS_TUTORIALS.RECEIVE, passedCard);
	}

	public void OnBoughtCard()
	{
		if (GameState.Get().GetFriendlySidePlayer().GetNumAvailableResources() != 0 && GameState.Get().GetTurn() > 1)
		{
			PlayDuosTutorial(DuosPopupTutoiral.DUOS_TUTORIALS.PASS);
		}
	}

	protected IEnumerator CollectOptionsFailsafe()
	{
		while (true)
		{
			Network.Options options = GameState.Get().GetOptionsPacket();
			bool discoverActive = ChoiceCardMgr.Get() != null && ChoiceCardMgr.Get().HasChoices();
			float timer = 0f;
			while (options == null && !GameState.Get().IsMulliganManagerActive() && IsShopPhase() && !discoverActive)
			{
				timer += Time.deltaTime;
				if (timer > 5f)
				{
					Network.Get().ResendOptions();
					timer = 0f;
				}
				options = GameState.Get().GetOptionsPacket();
				discoverActive = ChoiceCardMgr.Get() != null && ChoiceCardMgr.Get().HasChoices();
				yield return null;
			}
			yield return null;
		}
	}

	protected virtual IEnumerator OnShopPhase(bool expectStateChangeCallback)
	{
		PlayerLeaderboardManager.Get().SetOddManOutOpponentHero(null);
		if (m_showOpposingHeroActorLegendaryVFXCoroutine != null)
		{
			StopCoroutine(m_showOpposingHeroActorLegendaryVFXCoroutine);
			m_showOpposingHeroActorLegendaryVFXCoroutine = null;
		}
		TimeScaleMgr.Get().SetGameTimeScale(1f);
		AchievementManager.Get().UnpauseToastNotifications();
		yield return ShowPopup("Shop", expectStateChangeCallback);
		PlayerLeaderboardManager.Get().UpdateLayout();
		UpdateNameDisplay();
		ShowTechLevelDisplay(shown: true);
		yield return new WaitForSeconds(3f);
		int turn = GameState.Get().GetTurn();
		if (turn >= 5)
		{
			PlayDuosTutorial(DuosPopupTutoiral.DUOS_TUTORIALS.PING);
		}
		else if (turn >= 3)
		{
			PlayDuosTutorial(DuosPopupTutoiral.DUOS_TUTORIALS.COMBAT_ORDER);
		}
		else
		{
			PlayDuosTutorial(DuosPopupTutoiral.DUOS_TUTORIALS.COMBAT);
		}
		SetGameNotificationEmotesEnabled(enabled: true);
		GameState.Get().GetTimeTracker().ResetAccruedLostTime();
		if ((bool)m_duosPortal)
		{
			m_duosPortal.SetPortalClickable(clickable: true);
			m_duosPortal.Grow();
			TeammateBoardViewer.Get().SetBlockViewingTeammate(block: false);
		}
		if (TeammatePingWheelManager.Get() != null)
		{
			TeammatePingWheelManager.Get().SetPingWheelDisabled(disabled: false);
		}
	}

	protected virtual IEnumerator OnCombatPhase()
	{
		AchievementManager.Get().PauseToastNotifications();
		ZoneMgr.Get().AutoCorrectZones(ZoneMgr.Get().GetCancellationToken(), ignorePurePosChange: false);
		if ((bool)m_duosPortal)
		{
			m_duosPortal.SetPortalClickable(clickable: true);
			if (TeammateBoardViewer.Get().IsViewingOrTransitioningToTeammateView())
			{
				m_duosPortal.PortalPushed();
			}
			TeammateBoardViewer.Get().SetBlockViewingTeammate(block: true);
			m_duosPortal.SetPortalClickable(clickable: false);
			m_duosPortal.ApplyTeammateHeroTexture();
			yield return null;
			m_duosPortal.Shrink();
		}
		if (TeammatePingWheelManager.Get() != null)
		{
			TeammatePingWheelManager.Get().ClearAllPings();
			TeammatePingWheelManager.Get().SetPingWheelDisabled(disabled: true);
		}
		StopAllDuosTutorialPopups();
		yield return ShowPopup("Combat", expectStateChangeCallback: true);
		GameEntity.Coroutines.StartCoroutine(WaitAndHideActiveSpeechBubble());
		ShowTechLevelDisplay(shown: false);
		UpdateNameDisplay();
		ForceShowFriendlyHeroActor();
		InputManager.Get().HidePhoneHand();
		GameState.Get().GetTimeTracker().ResetAccruedLostTime();
		Actor friendlyActor = GetFriendlyHeroActor();
		TriggerCombatStartLegendaryVFX(friendlyActor);
		m_showOpposingHeroActorLegendaryVFXCoroutine = GameEntity.Coroutines.StartCoroutine(WaitThenShowOpposingHeroActorLegendaryVFX());
	}

	private IEnumerator WaitThenShowOpposingHeroActorLegendaryVFX()
	{
		while (GetOpposingHeroActor() == null)
		{
			yield return null;
		}
		Actor opposingActor = GetOpposingHeroActor();
		TriggerCombatStartLegendaryVFX(opposingActor);
	}

	public override void HandleRealTimeMissionEvent(int missionEvent)
	{
		if (missionEvent == 2)
		{
			SetGameNotificationEmotesEnabled(enabled: false);
		}
	}

	private bool TriggerCombatStartLegendaryVFX(Actor actor)
	{
		if (actor != null && actor.LegendaryHeroSkinConfig != null)
		{
			return actor.LegendaryHeroSkinConfig.TryActivateVFX_CombatStart();
		}
		return false;
	}

	private bool CheckHeroGreet(out Actor heroActor, out string voLine, bool setGreeted = true)
	{
		Actor friendlyActor = GetFriendlyHeroActor();
		Actor opposingActor = GetOpposingHeroActor();
		string friendlyCardId = ((friendlyActor != null && friendlyActor.GetEntity() != null) ? friendlyActor.GetEntity().GetCardId() : null);
		string opposingCardId = ((opposingActor != null && opposingActor.GetEntity() != null) ? opposingActor.GetEntity().GetCardId() : null);
		string friendlyLine = null;
		string opposingLine = null;
		bool friendlyCanGreet = friendlyActor != null && friendlyActor.LegendaryHeroSkinConfig != null && friendlyActor.LegendaryHeroSkinConfig.CheckGreetLine(opposingCardId, out friendlyLine) && !m_heroesGreeted.Contains(opposingCardId);
		bool opposingCanGreet = opposingActor != null && opposingActor.LegendaryHeroSkinConfig != null && opposingActor.LegendaryHeroSkinConfig.CheckGreetLine(friendlyCardId, out opposingLine) && !m_greetedByHeroes.Contains(opposingCardId);
		if (!friendlyCanGreet && !opposingCanGreet)
		{
			heroActor = null;
			voLine = null;
			return false;
		}
		if (friendlyCanGreet && opposingCanGreet)
		{
			int fDefense = friendlyActor.GetEntity().GetCurrentDefense();
			int oDefense = opposingActor.GetEntity().GetCurrentDefense();
			if (fDefense == oDefense)
			{
				if (friendlyCardId.CompareTo(opposingCardId) < 0)
				{
					opposingCanGreet = false;
				}
				else
				{
					friendlyCanGreet = false;
				}
			}
			else if (fDefense > oDefense)
			{
				opposingCanGreet = false;
			}
			else
			{
				friendlyCanGreet = false;
			}
		}
		if (friendlyCanGreet)
		{
			voLine = friendlyLine;
			heroActor = friendlyActor;
			if (setGreeted)
			{
				m_heroesGreeted.Add(opposingCardId);
			}
		}
		else
		{
			voLine = opposingLine;
			heroActor = opposingActor;
			if (setGreeted)
			{
				m_greetedByHeroes.Add(opposingCardId);
			}
		}
		return true;
	}

	private IEnumerator HandleKnockoutVO()
	{
		Actor friendly = GetFriendlyHeroActor();
		Actor opponent = GetOpposingHeroActor();
		if (!(friendly != null) || !(opponent != null) || (!(friendly.LegendaryHeroSkinConfig != null) && !(opponent.LegendaryHeroSkinConfig != null)))
		{
			yield break;
		}
		int fAtk = friendly.GetEntity().GetATK();
		int oppAtk = opponent.GetEntity().GetATK();
		int fDefense = friendly.GetEntity().GetCurrentDefense();
		int oppDefense = opponent.GetEntity().GetCurrentDefense();
		if ((fAtk > 0 || oppAtk > 0) && oppDefense != 0 && fDefense != 0)
		{
			if (fAtk > oppAtk)
			{
				yield return PlayVOIfLethal(friendly, GameState.Get().GetFriendlySidePlayer(), oppDefense);
			}
			else
			{
				yield return PlayVOIfLethal(opponent, GameState.Get().GetOpposingSidePlayer(), fDefense);
			}
		}
	}

	private IEnumerator PlayVOIfLethal(Actor actor, Player player, int defense)
	{
		string voLine = null;
		if (player != null && actor != null && actor.LegendaryHeroSkinConfig != null && actor.LegendaryHeroSkinConfig.CheckKnockoutLine(out voLine))
		{
			int attack = player.GetTag(GAME_TAG.PLAYER_TECH_LEVEL) + GetPlayerCombinedMinionsTechLevel(player);
			int damageCap = GameState.Get().GetGameEntity().GetTag(GAME_TAG.BACON_COMBAT_DAMAGE_CAP);
			if (damageCap != 0 && attack > damageCap && GameState.Get().GetGameEntity().GetRealtimeBaconDamageCapEnabled())
			{
				attack = damageCap;
			}
			if (attack >= defense)
			{
				StartDuckingFx();
				yield return PlayVOLineWithOffsetBubble(voLine, actor);
				StopDuckingFx();
			}
		}
	}

	private int GetPlayerCombinedMinionsTechLevel(Player player)
	{
		if (player == null)
		{
			return 0;
		}
		int sum = 0;
		foreach (Card card in player.GetBattlefieldZone().GetCards())
		{
			Entity cardEntity = card.GetEntity();
			if (cardEntity.GetController() == player && cardEntity.IsMinion())
			{
				sum += cardEntity.GetTechLevel();
			}
		}
		return sum;
	}

	private void StartDuckingFx()
	{
		if (m_duckObj == null)
		{
			m_duckObj = new GameObject();
		}
		if (m_fxDucker == null)
		{
			m_fxDucker = m_duckObj.AddComponent<SoundDucker>();
			List<SoundDuckedCategoryDef> duckDefs = new List<SoundDuckedCategoryDef>();
			SoundDuckedCategoryDef def = new SoundDuckedCategoryDef();
			def.m_Category = Global.SoundCategory.FX;
			duckDefs.Add(def);
			m_fxDucker.m_DuckAllCategories = false;
			m_fxDucker.SetDuckedCategoryDefs(duckDefs);
		}
		m_fxDucker.StartDucking();
	}

	private void StopDuckingFx()
	{
		if (m_fxDucker != null)
		{
			m_fxDucker.StopDucking();
		}
	}

	private void OnTurnEnded(int oldTurn, int newTurn, object userData)
	{
		if (!GameState.Get().IsFriendlySidePlayerTurn())
		{
			(HearthstonePerformance.Get()?.GetCurrentPerformanceFlow<FlowPerformanceBattlegrounds>())?.OnNewRoundStart();
			GameEntity.Coroutines.StartCoroutine(GameState.Get().RejectUnresolvedChangesAfterDelay());
		}
	}

	public override string GetAttackSpellControllerOverride(Entity attacker)
	{
		if (attacker == null)
		{
			return null;
		}
		if (attacker.IsHero())
		{
			return "AttackSpellController_Battlegrounds_Hero.prefab:28f08e692e201ad479a765c2ef8717b9";
		}
		return "AttackSpellController_Battlegrounds_Minion.prefab:922da2c91f4cca1458b5901204d1d26c";
	}

	public override string GetVictoryScreenBannerText()
	{
		int currentPlace = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetRealTimePlayerLeaderboardPlace();
		if (currentPlace == 0)
		{
			return string.Empty;
		}
		if (GameMgr.Get().IsBattlegroundDuoGame() && currentPlace <= 4)
		{
			return GameStrings.Get("GAMEPLAY_DUOS_END_OF_GAME_PLACE_" + currentPlace);
		}
		return GameStrings.Get("GAMEPLAY_END_OF_GAME_PLACE_" + currentPlace);
	}

	public override string GetBestNameForPlayer(int playerId)
	{
		string playerInfoName = ((GameState.Get().GetPlayerInfoMap().ContainsKey(playerId) && GameState.Get().GetPlayerInfoMap()[playerId] != null) ? GameState.Get().GetPlayerInfoMap()[playerId].GetName() : null);
		string playerInfoHeroName = ((GameState.Get().GetPlayerInfoMap().ContainsKey(playerId) && GameState.Get().GetPlayerInfoMap()[playerId] != null && GameState.Get().GetPlayerInfoMap()[playerId].GetHero() != null) ? GameState.Get().GetPlayerInfoMap()[playerId].GetHero().GetName() : null);
		bool isFriendlySide = GameState.Get().GetPlayerMap().ContainsKey(playerId) && GameState.Get().GetPlayerMap()[playerId].IsFriendlySide();
		int teamMateID = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.BACON_DUO_TEAMMATE_PLAYER_ID);
		bool streamerModeEnabled = Options.Get().GetBool(Option.STREAMER_MODE);
		if (playerInfoHeroName == null)
		{
			playerInfoHeroName = ((PlayerLeaderboardManager.Get() != null && PlayerLeaderboardManager.Get().GetTileForPlayerId(playerId) != null) ? PlayerLeaderboardManager.Get().GetTileForPlayerId(playerId).GetHeroName() : null);
		}
		if (isFriendlySide)
		{
			if (streamerModeEnabled || playerInfoName == null)
			{
				return GameStrings.Get("GAMEPLAY_HIDDEN_PLAYER_NAME");
			}
			return playerInfoName;
		}
		if (streamerModeEnabled)
		{
			if (playerInfoHeroName == null)
			{
				Player player = GameState.Get().GetFriendlySidePlayer();
				if (player != null)
				{
					int seed = GameMgr.Get().GetGameHandle() + playerId;
					return player.GetRandomName(seed);
				}
				return GameStrings.Get("GAMEPLAY_MISSING_OPPONENT_NAME");
			}
			return playerInfoHeroName;
		}
		if (playerInfoName == null)
		{
			Player player2 = GameState.Get().GetFriendlySidePlayer();
			if (player2 != null)
			{
				int seed2 = GameMgr.Get().GetGameHandle() + playerId;
				return player2.GetRandomName(seed2);
			}
			if (teamMateID == playerId)
			{
				return GameStrings.Get("GAMEPLAY_MISSING_TEAMMATE_NAME");
			}
			return GameStrings.Get("GAMEPLAY_MISSING_OPPONENT_NAME");
		}
		return playerInfoName;
	}

	public override string GetNameBannerOverride(Player.Side side)
	{
		if (GameState.Get() == null)
		{
			return null;
		}
		if (!IsCustomGameModeAIHero())
		{
			int playerId = 0;
			if (side == Player.Side.OPPOSING)
			{
				playerId = GameState.Get().GetOpposingSidePlayer().GetTag(GAME_TAG.BACON_CURRENT_COMBAT_PLAYER_ID);
				if (playerId == 0)
				{
					playerId = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.NEXT_OPPONENT_PLAYER_ID);
				}
			}
			else
			{
				playerId = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.BACON_CURRENT_COMBAT_PLAYER_ID);
				if (playerId == 0)
				{
					playerId = GameState.Get().GetFriendlyPlayerId();
				}
			}
			return GetBestNameForPlayer(playerId);
		}
		if (m_gamePhase == 2)
		{
			if (PlayerLeaderboardManager.Get() == null || PlayerLeaderboardManager.Get().GetOddManOutOpponentHero() == null)
			{
				if (GameState.Get().GetOpposingSidePlayer() == null || GameState.Get().GetOpposingSidePlayer().GetHero() == null)
				{
					return null;
				}
				return GameState.Get().GetOpposingSidePlayer().GetHero()
					.GetName();
			}
			return PlayerLeaderboardManager.Get().GetOddManOutOpponentHero().GetName();
		}
		if (side != Player.Side.OPPOSING)
		{
			if (TeammateBoardViewer.Get() != null && TeammateBoardViewer.Get().IsViewingTeammate())
			{
				return GetBestNameForPlayer(GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.BACON_DUO_TEAMMATE_PLAYER_ID));
			}
			return GetBestNameForPlayer(GameState.Get().GetFriendlyPlayerId());
		}
		if (GameState.Get().GetOpposingSidePlayer() == null || GameState.Get().GetOpposingSidePlayer().GetHero() == null)
		{
			return null;
		}
		return GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetName();
	}

	public override void PlayAlternateEnemyEmote(int playerId, EmoteType emoteType, int battlegroundsEmoteId = 0)
	{
		string localizedString = "";
		Actor tileActor = null;
		NotificationManager.VisualEmoteType visualEmoteType = NotificationManager.VisualEmoteType.NONE;
		PlayerLeaderboardCard playerCard = PlayerLeaderboardManager.Get().GetTileForPlayerId(playerId);
		if (!(playerCard == null))
		{
			tileActor = playerCard.m_tileActor;
			switch (emoteType)
			{
			case EmoteType.OOPS:
				localizedString = GameStrings.Get("GAMEPLAY_BACON_TEXT_EMOTE_OOPS");
				break;
			case EmoteType.GREETINGS:
				localizedString = GameStrings.Get("GAMEPLAY_BACON_TEXT_EMOTE_GREETINGS");
				break;
			case EmoteType.THREATEN:
				localizedString = GameStrings.Get("GAMEPLAY_BACON_TEXT_EMOTE_THREATEN");
				break;
			case EmoteType.WELL_PLAYED:
				localizedString = GameStrings.Get("GAMEPLAY_BACON_TEXT_EMOTE_WELL_PLAYED");
				break;
			case EmoteType.WOW:
				localizedString = GameStrings.Get("GAMEPLAY_BACON_TEXT_EMOTE_WOW");
				break;
			case EmoteType.THANKS:
				localizedString = GameStrings.Get("GAMEPLAY_BACON_TEXT_EMOTE_THANKS");
				break;
			case EmoteType.SORRY:
				localizedString = GameStrings.Get("GAMEPLAY_BACON_TEXT_EMOTE_SORRY");
				break;
			case EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_01:
				visualEmoteType = NotificationManager.VisualEmoteType.TECH_UP_01;
				break;
			case EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_02:
				visualEmoteType = NotificationManager.VisualEmoteType.TECH_UP_02;
				break;
			case EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_03:
				visualEmoteType = NotificationManager.VisualEmoteType.TECH_UP_03;
				break;
			case EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_04:
				visualEmoteType = NotificationManager.VisualEmoteType.TECH_UP_04;
				break;
			case EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_05:
				visualEmoteType = NotificationManager.VisualEmoteType.TECH_UP_05;
				break;
			case EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_06:
				visualEmoteType = NotificationManager.VisualEmoteType.TECH_UP_06;
				break;
			case EmoteType.BATTLEGROUNDS_VISUAL_TECH_UP_07:
				visualEmoteType = NotificationManager.VisualEmoteType.TECH_UP_07;
				break;
			case EmoteType.BATTLEGROUNDS_VISUAL_HOT_STREAK:
				visualEmoteType = NotificationManager.VisualEmoteType.HOT_STREAK;
				break;
			case EmoteType.BATTLEGROUNDS_VISUAL_BANANA:
				visualEmoteType = NotificationManager.VisualEmoteType.BANANA;
				break;
			case EmoteType.BATTLEGROUNDS_VISUAL_HERO_BUDDY:
				visualEmoteType = NotificationManager.VisualEmoteType.HERO_BUDDY;
				break;
			case EmoteType.BATTLEGROUNDS_VISUAL_DOUBLE_HERO_BUDDY:
				visualEmoteType = NotificationManager.VisualEmoteType.DOUBLE_HERO_BUDDY;
				break;
			case EmoteType.BATTLEGROUNDS_VISUAL_TRIPLE:
				visualEmoteType = NotificationManager.VisualEmoteType.TRIPLE;
				break;
			case EmoteType.BATTLEGROUNDS_VISUAL_ONE:
				visualEmoteType = NotificationManager.VisualEmoteType.BATTLEGROUNDS_01;
				break;
			case EmoteType.BATTLEGROUNDS_VISUAL_TWO:
				visualEmoteType = NotificationManager.VisualEmoteType.BATTLEGROUNDS_02;
				break;
			case EmoteType.BATTLEGROUNDS_VISUAL_THREE:
				visualEmoteType = NotificationManager.VisualEmoteType.BATTLEGROUNDS_03;
				break;
			case EmoteType.BATTLEGROUNDS_VISUAL_FOUR:
				visualEmoteType = NotificationManager.VisualEmoteType.BATTLEGROUNDS_04;
				break;
			case EmoteType.BATTLEGROUNDS_VISUAL_FIVE:
				visualEmoteType = NotificationManager.VisualEmoteType.BATTLEGROUNDS_05;
				break;
			case EmoteType.BATTLEGROUNDS_VISUAL_SIX:
				visualEmoteType = NotificationManager.VisualEmoteType.BATTLEGROUNDS_06;
				break;
			case EmoteType.COLLECTIBLE_BATTLEGROUNDS_EMOTE:
				visualEmoteType = NotificationManager.VisualEmoteType.COLLECTIBLE_BATTLEGROUNDS_EMOTE;
				break;
			case EmoteType.BATTLEGROUNDS_VISUAL_QUEST_COMPLETE:
				visualEmoteType = NotificationManager.VisualEmoteType.QUEST_COMPLETE;
				break;
			default:
				localizedString = GameStrings.Get("GAMEPLAY_BACON_TEXT_EMOTE_INVALID");
				break;
			}
			if (visualEmoteType == NotificationManager.VisualEmoteType.COLLECTIBLE_BATTLEGROUNDS_EMOTE)
			{
				NotificationManager.SpeechBubbleOptions options = CreateBattlegroundsEmoteOptions(tileActor, playerId, battlegroundsEmoteId);
				RequestNotification(options, emoteType);
			}
			else if (localizedString != null || visualEmoteType != 0)
			{
				NotificationManager.SpeechBubbleOptions options2 = CreateStandardEmoteOptions(tileActor, localizedString, playerId, visualEmoteType);
				RequestNotification(options2, emoteType);
			}
		}
	}

	private NotificationManager.SpeechBubbleOptions CreateBattlegroundsEmoteOptions(Actor actor, int playerId, int battlegroundsEmoteId)
	{
		return new NotificationManager.SpeechBubbleOptions().WithActor(actor).WithSpeechBubbleDirection(Notification.SpeechBubbleDirection.TopLeft).WithParentToActor(parentToActor: true)
			.WithSpeechBubbleGroup(playerId)
			.WithVisualEmoteType(NotificationManager.VisualEmoteType.COLLECTIBLE_BATTLEGROUNDS_EMOTE)
			.WithFinishCallback(OnNotificationEnded)
			.WithBattlegroundsEmoteId(battlegroundsEmoteId);
	}

	private NotificationManager.SpeechBubbleOptions CreateStandardEmoteOptions(Actor actor, string localizedString, int playerId, NotificationManager.VisualEmoteType visualEmoteType)
	{
		return new NotificationManager.SpeechBubbleOptions().WithActor(actor).WithBubbleScale(0.3f).WithSpeechText(localizedString)
			.WithSpeechBubbleDirection(Notification.SpeechBubbleDirection.MiddleLeft)
			.WithParentToActor(parentToActor: false)
			.WithDestroyWhenNewCreated(destroyWhenNewCreated: true)
			.WithSpeechBubbleGroup(playerId)
			.WithVisualEmoteType(visualEmoteType)
			.WithEmoteDuration(1.5f)
			.WithFinishCallback(OnNotificationEnded);
	}

	private void RequestNotification(NotificationManager.SpeechBubbleOptions options, EmoteType emoteType)
	{
		int playerId = options.speechBubbleGroup;
		if (!m_emotesAllowedForPlayer.ContainsKey(playerId))
		{
			m_emotesAllowedForPlayer.Add(playerId, value: true);
			m_emotesQueuedForPlayer.Add(playerId, new QueueList<NotificationManager.SpeechBubbleOptions>());
			m_gameNotificationsQueuedForPlayer.Add(playerId, new LinkedList<NotificationManager.SpeechBubbleOptions>());
		}
		if (m_gameNotificationEmotes.Contains(emoteType))
		{
			if (m_priorityEmotes.Contains(emoteType))
			{
				m_gameNotificationsQueuedForPlayer[playerId].AddFirst(options);
			}
			else
			{
				m_gameNotificationsQueuedForPlayer[playerId].AddLast(options);
			}
		}
		else
		{
			m_emotesQueuedForPlayer[playerId].Enqueue(options);
		}
		PlayEmotesIfPossibleForPlayer(playerId);
	}

	private void OnNotificationEnded(int playerId)
	{
		if (m_emotesAllowedForPlayer.ContainsKey(playerId))
		{
			m_emotesAllowedForPlayer[playerId] = true;
			PlayEmotesIfPossibleForPlayer(playerId);
		}
	}

	private void PlayEmotesIfPossibleForPlayer(int playerId)
	{
		if (m_emotesAllowedForPlayer.ContainsKey(playerId) && m_emotesAllowedForPlayer[playerId])
		{
			if (m_emotesQueuedForPlayer.ContainsKey(playerId) && m_emotesQueuedForPlayer[playerId].Count > 0)
			{
				NotificationManager.Get().CreateSpeechBubble(m_emotesQueuedForPlayer[playerId].Dequeue());
				m_emotesAllowedForPlayer[playerId] = false;
			}
			else if (m_gameNotificationEmotesAllowed && m_gameNotificationsQueuedForPlayer.ContainsKey(playerId) && m_gameNotificationsQueuedForPlayer[playerId].Count > 0)
			{
				NotificationManager.Get().CreateSpeechBubble(m_gameNotificationsQueuedForPlayer[playerId].First.Value);
				m_gameNotificationsQueuedForPlayer[playerId].RemoveFirst();
				m_emotesAllowedForPlayer[playerId] = false;
			}
		}
	}

	private void SetGameNotificationEmotesEnabled(bool enabled)
	{
		m_gameNotificationEmotesAllowed = enabled;
		if (!m_gameNotificationEmotesAllowed)
		{
			return;
		}
		foreach (int playerId in m_emotesAllowedForPlayer.Keys.ToList())
		{
			PlayEmotesIfPossibleForPlayer(playerId);
		}
	}

	public override bool ShouldUseAlternateNameForPlayer(Player.Side side)
	{
		return side == Player.Side.OPPOSING;
	}

	private bool IsCustomGameModeAIHero()
	{
		if (!IsShopPhase())
		{
			return GameState.Get().GetFriendlySidePlayer().HasTag(GAME_TAG.BACON_ODD_PLAYER_OUT);
		}
		return true;
	}

	public override string GetTurnTimerCountdownText(float timeRemainingInTurn)
	{
		if (m_gamePhase == 2)
		{
			return GameStrings.Get("GAMEPLAY_BACON_COMBAT_END_TURN_BUTTON_TEXT");
		}
		if (m_gamePhase == 1)
		{
			if (timeRemainingInTurn == 0f)
			{
				if (!TurnTimer.Get().IsRopeActive())
				{
					return GameStrings.Get("GAMEPLAY_BACON_SHOP_END_TURN_BUTTON_TEXT");
				}
				return "";
			}
			AchievementManager achieveMgr = AchievementManager.Get();
			if (timeRemainingInTurn < achieveMgr.GetNotificationPauseBufferSeconds() && !achieveMgr.ToastNotificationsPaused)
			{
				achieveMgr.PauseToastNotifications();
			}
			return GameStrings.Format("GAMEPLAY_END_TURN_BUTTON_COUNTDOWN", Mathf.CeilToInt(timeRemainingInTurn));
		}
		return "";
	}

	public override void NotifyOfGameOver(TAG_PLAYSTATE gameResult)
	{
		AchievementManager.Get().UnpauseToastNotifications();
		base.NotifyOfGameOver(gameResult);
	}

	protected void InitializePhasePopup()
	{
		AssetLoader.Get().InstantiatePrefab(BACON_PHASE_POPUP, delegate(AssetReference assetRef, GameObject go, object callbackData)
		{
			m_phasePopup = go;
			m_phasePopup.SetActive(value: false);
		});
	}

	private void InitailizeViewTeammate()
	{
		if (GameMgr.Get().IsBattlegroundDuoGame())
		{
			TeammateBoardViewer.Get().Initialize();
		}
	}

	private void LoadDuosPortal()
	{
		if (!GameMgr.Get().IsBattlegroundDuoGame())
		{
			return;
		}
		if (m_duosPortal != null)
		{
			m_duosPortal.ApplyTeammateHeroTexture("SwapTexture");
			return;
		}
		AssetLoader.Get().InstantiatePrefab("BaconFX_Duos_DeckPortal.prefab:9bef8e7ceedeb084496e70350a5db112", delegate(AssetReference assetRef, GameObject go, object callbackData)
		{
			if (go == null)
			{
				Log.Gameplay.PrintError("TB_BaconShop: failed to load DuosPortal from BaconFX_Duos_DeckPortal.prefab");
			}
			else
			{
				m_duosPortal = go.GetComponent<DuosPortal>();
				if (m_duosPortal == null)
				{
					Log.Gameplay.PrintError("TB_BaconShop: failed to load DuosPortal from BaconFX_Duos_DeckPortal.prefab");
				}
				else if (!(TeammateBoardViewer.Get() == null))
				{
					m_duosPortal.gameObject.transform.position = TeammateBoardViewer.Get().transform.position;
					TeammateBoardViewer.Get()?.SetDuosPortal(m_duosPortal);
					DuosPopupTutoiral.Get()?.SetDuosPortal(m_duosPortal);
					m_duosPortal.ApplyTeammateHeroTexture();
					m_duosPortal.SetPortalClickedCallback(PortalClickedCallback);
					m_duosPortal.SetPortaPingedCallback(PortalPingedCallback);
				}
			}
		});
	}

	private void PortalClickedCallback()
	{
		StopDuosTutorialPopup(DuosPopupTutoiral.DUOS_TUTORIALS.TEAMMATE_PINGED);
		if (!TeammateBoardViewer.Get().IsViewingTeammate())
		{
			PlayDuosTutorial(DuosPopupTutoiral.DUOS_TUTORIALS.VIEW_SELF);
		}
		else
		{
			StopDuosTutorialPopup(DuosPopupTutoiral.DUOS_TUTORIALS.VIEW_SELF);
		}
	}

	private void PortalPingedCallback()
	{
		PlayDuosTutorial(DuosPopupTutoiral.DUOS_TUTORIALS.TEAMMATE_PINGED);
	}

	public void NotifyPingWheelActive(GameObject pingObject)
	{
		StopDuosTutorialPopup(DuosPopupTutoiral.DUOS_TUTORIALS.PING);
		if (Options.Get().GetBool(Option.BG_DUOS_TUTORIAL_PLAYED_PING, defaultVal: false))
		{
			PlayDuosTutorial(DuosPopupTutoiral.DUOS_TUTORIALS.PING_WHEEL, null, null, pingObject);
		}
	}

	public void NotifyPingWheelClosed()
	{
		StopDuosTutorialPopup(DuosPopupTutoiral.DUOS_TUTORIALS.PING_WHEEL);
	}

	private void PlayDuosTutorial(DuosPopupTutoiral.DUOS_TUTORIALS duosTutorial, Card targetCard = null, Actor targetActor = null, GameObject go = null, Player.Side side = Player.Side.NEUTRAL)
	{
		if (DuosPopupTutoiral.Get() != null)
		{
			DuosPopupTutoiral.Get().PlayDuosTutorial(duosTutorial, targetCard, targetActor, go, side);
		}
	}

	private void StopDuosTutorialPopup(DuosPopupTutoiral.DUOS_TUTORIALS duosTutorial)
	{
		if (DuosPopupTutoiral.Get() != null)
		{
			DuosPopupTutoiral.Get().StopDuosTutorialPopup(duosTutorial);
		}
	}

	public void StopAllDuosTutorialPopups()
	{
		if (DuosPopupTutoiral.Get() != null)
		{
			DuosPopupTutoiral.Get().StopAllDuosTutorialPopups();
		}
	}

	protected IEnumerator ShowPopup(string playmakerState, bool expectStateChangeCallback)
	{
		if (!m_gameplaySceneLoaded)
		{
			yield break;
		}
		while (m_phasePopup == null)
		{
			yield return null;
		}
		m_phasePopup.SetActive(value: true);
		PlayMakerFSM phaseFsm = m_phasePopup.GetComponent<PlayMakerFSM>();
		phaseFsm.SetState(playmakerState);
		if (!expectStateChangeCallback || BaconBoard.Get() == null)
		{
			while (phaseFsm.ActiveStateName != "Idle")
			{
				yield return null;
			}
			yield return null;
			phaseFsm.SetState("Death");
		}
	}

	protected void OnStateChange(TAG_BOARD_VISUAL_STATE newState)
	{
		GameEntity.Coroutines.StartCoroutine(StateChangeCoroutine(newState));
	}

	protected IEnumerator StateChangeCoroutine(TAG_BOARD_VISUAL_STATE newState)
	{
		while (m_phasePopup == null)
		{
			yield return null;
		}
		while (!m_phasePopup.activeSelf)
		{
			yield return null;
		}
		PlayMakerFSM phaseFsm = m_phasePopup.GetComponent<PlayMakerFSM>();
		while (phaseFsm.ActiveStateName != "Idle")
		{
			yield return null;
		}
		yield return null;
		phaseFsm.SetState("Death");
		if (newState == TAG_BOARD_VISUAL_STATE.COMBAT)
		{
			yield return TryPlayHeroGreet();
		}
	}

	public override bool IsStateChangePopupVisible()
	{
		if (m_phasePopup == null)
		{
			return false;
		}
		if (!m_phasePopup.activeSelf)
		{
			return false;
		}
		PlayMakerFSM phaseFsm = m_phasePopup.GetComponent<PlayMakerFSM>();
		if (phaseFsm.ActiveStateName == "StateStart" || phaseFsm.ActiveStateName == "Done")
		{
			return false;
		}
		return true;
	}

	protected IEnumerator TryPlayHeroGreet()
	{
		Actor heroActor = null;
		if (CheckHeroGreet(out heroActor, out var voLine))
		{
			yield return PlayVOLineWithOffsetBubble(voLine, heroActor, 1.5f);
		}
	}

	protected void UpdateNameBanner()
	{
		if (!(Gameplay.Get() == null))
		{
			NameBanner nameBanner = Gameplay.Get().GetNameBannerForSide(Player.Side.OPPOSING);
			if (nameBanner != null)
			{
				nameBanner.UpdatePlayerNameBanner();
			}
			nameBanner = Gameplay.Get().GetNameBannerForSide(Player.Side.FRIENDLY);
			if (nameBanner != null)
			{
				nameBanner.UpdatePlayerNameBanner();
			}
		}
	}

	protected void InitializeTurnTimer()
	{
		TurnTimer.Get().SetGameModeSettings(new TurnTimerGameModeSettings
		{
			m_RopeFuseVolume = 0.05f,
			m_EndTurnButtonExplosionVolume = 0f,
			m_RopeRolloutVolume = 0.3f,
			m_PlayMusicStinger = false,
			m_PlayTimeoutFx = false,
			m_PlayTickSound = true
		});
	}

	public bool IsShopPhase()
	{
		return m_gamePhase == 1;
	}

	public bool IsBattlePhase()
	{
		return m_gamePhase == 2;
	}

	private void OnBattlegroundsRatingChange()
	{
		BattlegroundsRatingChange battlegroundsRatingChange = Network.Get().GetBattlegroundsRatingChange();
		RatingChangeData = battlegroundsRatingChange;
	}

	public override void NotifyOfMinionDied(Entity minion)
	{
		base.NotifyOfMinionDied(minion);
		BaconBoard.Get().NotifyOfMinionDied(minion);
	}

	private int GetTechLevelInt()
	{
		if (GameState.Get() == null || GameState.Get().GetFriendlySidePlayer() == null)
		{
			return 0;
		}
		return GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.PLAYER_TECH_LEVEL);
	}

	private void InitTurnCounter()
	{
		GameObject turnCounterGo = AssetLoader.Get().InstantiatePrefab("BaconTechLevelRibbon.prefab:ad60cd0fe1c8eea4bb2f12cc280acda8");
		if (turnCounterGo == null)
		{
			Log.Gameplay.PrintError("InitTurnCounter() - Unable to load tier icon prefab");
			return;
		}
		m_techLevelCounter = turnCounterGo.GetComponent<Notification>();
		PlayMakerFSM component = m_techLevelCounter.GetComponent<PlayMakerFSM>();
		component.FsmVariables.GetFsmInt("TechLevel").Value = GetTechLevelInt();
		component.SendEvent("Birth");
		Zone opposingHeroZone = ZoneMgr.Get().FindZoneOfType<ZoneHero>(Player.Side.OPPOSING);
		m_techLevelCounter.transform.localPosition = opposingHeroZone.transform.position + new Vector3(-1.294f, 0.21f, -0.152f);
		m_techLevelCounter.transform.localScale = Vector3.one * 0.58f;
		GameEntity.Coroutines.StartCoroutine(KeepTechLevelUpToDateCoroutine());
	}

	protected void ShowTechLevelDisplay(bool shown)
	{
		if (m_techLevelCounter == null)
		{
			InitTurnCounter();
		}
		if (m_techLevelCounter != null)
		{
			m_techLevelCounter.gameObject.SetActive(shown);
		}
	}

	private IEnumerator KeepTechLevelUpToDateCoroutine()
	{
		while (true)
		{
			if (!m_techLevelCounter.gameObject.activeInHierarchy)
			{
				yield return null;
			}
			int techLevel = GetTechLevelInt();
			if (techLevel != m_displayedTechLevelNumber)
			{
				PlayMakerFSM component = m_techLevelCounter.GetComponent<PlayMakerFSM>();
				component.FsmVariables.GetFsmInt("TechLevel").Value = techLevel;
				component.SendEvent("Action");
				UpdateTechLevelDisplayText(techLevel);
			}
			yield return null;
		}
	}

	public override void ToggleAlternateMulliganActorHighlight(Card card, bool highlighted)
	{
		PlayerLeaderboardMainCardActor actor = card.GetActor() as PlayerLeaderboardMainCardActor;
		if (actor != null)
		{
			actor.SetFullyHighlighted(highlighted);
		}
	}

	public override void ToggleAlternateMulliganActorConfirmHighlight(Card card, bool highlighted)
	{
		PlayerLeaderboardMainCardActor actor = card.GetActor() as PlayerLeaderboardMainCardActor;
		if (actor != null)
		{
			actor.SetConfirmHighlighted(highlighted);
		}
	}

	public override bool ToggleAlternateMulliganActorHighlight(Actor actor, bool? highlighted = null)
	{
		PlayerLeaderboardMainCardActor mainCardActor = actor as PlayerLeaderboardMainCardActor;
		if (mainCardActor != null)
		{
			bool isHighlighted = ((!highlighted.HasValue) ? (!mainCardActor.m_fullSelectionHighlight.activeSelf) : highlighted.Value);
			mainCardActor.SetFullyHighlighted(isHighlighted);
			return isHighlighted;
		}
		return false;
	}

	private void UpdateTechLevelDisplayText(int techLevel)
	{
		string counterName = GameStrings.Get("GAMEPLAY_BACON_TAVERN_TIER");
		m_techLevelCounter.ChangeDialogText(counterName, "", "", "");
		m_displayedTechLevelNumber = techLevel;
	}

	public override bool IsInBattlegroundsShopPhase()
	{
		return IsShopPhase();
	}

	public override bool IsInBattlegroundsCombatPhase()
	{
		return IsBattlePhase();
	}

	private static void StopCoroutine(Coroutine coroutine)
	{
		if (coroutine != null)
		{
			GameEntity.Coroutines.StopCoroutine(coroutine);
		}
	}

	private IEnumerator WaitAndHideActiveSpeechBubble()
	{
		yield return new WaitForSeconds(1f);
		NotificationManager.Get().DestroyNotification(m_ActiveSpeechBubble, 0f);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		HashSet<string> hashSet = new HashSet<string>();
		hashSet.UnionWith(SoundFilesForPreload());
		foreach (string soundFile in hashSet)
		{
			PreloadSound(soundFile);
		}
	}

	protected virtual List<string> SoundFilesForPreload()
	{
		List<string> list = new List<string>(new List<string> { VO_DALA_BOSS_99h_Male_Human_ShopFirstTime_01, VO_DALA_BOSS_99h_Male_Human_FirstDefeat_01, VO_DALA_BOSS_99h_Male_Human_FirstVictory_01, VO_DALA_BOSS_99h_Male_Human_Hire_01, VO_DALA_BOSS_99h_Male_Human_RecruitWork_01 });
		list.AddRange(GetGuideConfig().GetAllVOLines());
		return list;
	}

	public override void OnPlayThinkEmote()
	{
		if (m_enemySpeaking)
		{
			return;
		}
		Player currentPlayer = GameState.Get().GetCurrentPlayer();
		if (currentPlayer == null || !currentPlayer.IsFriendlySide())
		{
			return;
		}
		Card currentHeroCard = currentPlayer.GetHeroCard();
		if (currentHeroCard == null || currentHeroCard.HasActiveEmoteSound())
		{
			return;
		}
		Actor bobActor = GetBobActor();
		if (bobActor == null || bobActor.GetEntity() == null)
		{
			return;
		}
		if (currentPlayer.GetNumAvailableResources() <= 2)
		{
			if (ShouldPlayRateVO(0.1f))
			{
				string voLine = GetGuideConfig().PopRandomSpecialIdleLine();
				GameEntity.Coroutines.StartCoroutine(PlayBobLineWithOffsetBubble(voLine));
			}
		}
		else if (ShouldPlayRateVO(0.05f))
		{
			string voLine2 = GetGuideConfig().GetRandomIdleLine();
			GameEntity.Coroutines.StartCoroutine(PlayBobLineWithOffsetBubble(voLine2));
		}
	}

	protected Actor GetBobActor()
	{
		Entity opposingHero = GameState.Get().GetOpposingSidePlayer().GetHero();
		if (opposingHero != null && opposingHero.GetCardId() == m_FavoriteGuideCardId)
		{
			return opposingHero.GetHeroCard().GetActor();
		}
		return null;
	}

	protected Actor GetFriendlyHeroActor()
	{
		return GameState.Get().GetFriendlySidePlayer()?.GetHero().GetHeroCard().GetActor();
	}

	protected Actor GetOpposingHeroActor()
	{
		Entity opposingHero = GameState.Get().GetOpposingSidePlayer().GetHero();
		if (opposingHero != null && opposingHero.GetCardId() != m_FavoriteGuideCardId)
		{
			return opposingHero.GetHeroCard().GetActor();
		}
		int nextOpponentId = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.NEXT_OPPONENT_PLAYER_ID);
		Map<int, SharedPlayerInfo> playerInfoMap = GameState.Get().GetPlayerInfoMap();
		SharedPlayerInfo pInfo = null;
		if (playerInfoMap != null && playerInfoMap.ContainsKey(nextOpponentId))
		{
			pInfo = playerInfoMap[nextOpponentId];
		}
		if (pInfo != null && pInfo.GetPlayerHero() != null && pInfo.GetPlayerHero().GetCard() != null)
		{
			return pInfo.GetPlayerHero().GetCard().GetActor();
		}
		return null;
	}

	protected bool ShouldPlayRateVO(float chance)
	{
		float randomNum = UnityEngine.Random.Range(0f, 1f);
		return chance > randomNum;
	}

	protected IEnumerator PlayVOLineWithOffsetBubble(string voLine, Actor actor, float wait = 0f)
	{
		if (actor != null && actor.GetEntity() != null)
		{
			if (wait != 0f)
			{
				yield return new WaitForSeconds(wait);
			}
			yield return PlayVOLineWithoutText(voLine, actor);
		}
	}

	protected IEnumerator PlayVOLineWithoutText(string voLine, Actor actor)
	{
		if (actor != null && actor.GetEntity() != null)
		{
			Notification.SpeechBubbleDirection direction = (actor.GetEntity().IsControlledByFriendlySidePlayer() ? Notification.SpeechBubbleDirection.TopLeft : Notification.SpeechBubbleDirection.BottomLeft);
			m_enemySpeaking = true;
			yield return PlaySoundAndWait(voLine, "", direction, actor, Time.timeScale);
			m_enemySpeaking = false;
		}
	}

	protected IEnumerator PlayBobLineWithoutText(string voLine)
	{
		Actor bobActor = GetBobActor();
		if (bobActor != null && bobActor.GetEntity() != null)
		{
			m_enemySpeaking = true;
			yield return PlaySoundAndWait(voLine, "", Notification.SpeechBubbleDirection.TopLeft, bobActor, Time.timeScale);
			m_enemySpeaking = false;
		}
	}

	protected virtual IEnumerator PlayBobLineWithOffsetBubble(string voLine)
	{
		Actor bobActor = GetBobActor();
		if (bobActor != null && bobActor.GetEntity() != null)
		{
			yield return PlayBobLineWithoutText(voLine);
		}
	}

	private IEnumerator PlayWisdomballVOLine(string voLine)
	{
		Actor wisdomballActor = null;
		foreach (Card questRewardCard in GetQuestRewardCards(Player.Side.FRIENDLY))
		{
			Actor cardActor = questRewardCard.GetActor();
			if (cardActor.CardDefName == "BG24_Reward_313" || cardActor.CardDefName == "BG24_Reward_313(Clone)")
			{
				wisdomballActor = cardActor;
			}
		}
		m_enemySpeaking = true;
		RemovePreloadedSound(voLine);
		PreloadSound(voLine);
		while (IsPreloadingAssets())
		{
			yield return null;
		}
		yield return PlayVOLineWithOffsetBubble(voLine, wisdomballActor);
		m_enemySpeaking = false;
	}

	protected Card GetGameModeButtonBySlot(int buttonSlot)
	{
		List<Zone> list = ZoneMgr.Get().FindZonesForSide(Player.Side.FRIENDLY);
		Zone buttonZone = null;
		foreach (Zone zone in list)
		{
			if (zone is ZoneGameModeButton && ((ZoneGameModeButton)zone).m_ButtonSlot == buttonSlot)
			{
				buttonZone = zone;
			}
		}
		if (buttonZone == null)
		{
			return null;
		}
		return buttonZone.GetFirstCard();
	}

	public static Card GetHeroBuddyCard(Player.Side playerSide)
	{
		List<Zone> list = ZoneMgr.Get().FindZonesForSide(playerSide);
		Zone heroBuddyZone = null;
		foreach (Zone zone in list)
		{
			if (zone is ZoneBattlegroundHeroBuddy)
			{
				heroBuddyZone = zone;
			}
		}
		if (heroBuddyZone == null)
		{
			return null;
		}
		return heroBuddyZone.GetFirstCard();
	}

	public static List<Card> GetQuestRewardCards(Player.Side playerSide)
	{
		GameState gameState = GameState.Get();
		if (gameState != null)
		{
			Player player = ((playerSide == Player.Side.OPPOSING) ? gameState.GetOpposingPlayer() : gameState.GetFriendlySidePlayer());
			if (player != null)
			{
				return player.GetQuestRewardCards();
			}
		}
		return new List<Card>();
	}

	public Card GetFreezeButtonCard()
	{
		return GetGameModeButtonBySlot(1);
	}

	public Card GetRefreshButtonCard()
	{
		return GetGameModeButtonBySlot(2);
	}

	public Card GetTavernUpgradeButtonCard()
	{
		return GetGameModeButtonBySlot(3);
	}

	protected void SetInputEnableForBuy(bool isEnabled)
	{
		foreach (Card card in GameState.Get().GetOpposingSidePlayer().GetBattlefieldZone()
			.GetCards())
		{
			card.SetInputEnabled(isEnabled);
		}
	}

	protected void SetInputEnableForRefreshButton(bool isEnabled)
	{
		Card card = GetRefreshButtonCard();
		if (card != null)
		{
			card.SetInputEnabled(isEnabled);
		}
	}

	protected void SetInputEnableForTavernUpgradeButton(bool isEnabled)
	{
		Card card = GetTavernUpgradeButtonCard();
		if (card != null)
		{
			card.SetInputEnabled(isEnabled);
		}
	}

	protected void SetInputEnableForFrozenButton(bool isEnabled)
	{
		Card card = GetFreezeButtonCard();
		if (card != null)
		{
			card.SetInputEnabled(isEnabled);
		}
	}

	public override bool NotifyOfPlayError(PlayErrors.ErrorType error, int? errorParam, Entity errorSource)
	{
		if (error == PlayErrors.ErrorType.REQ_ATTACK_GREATER_THAN_0)
		{
			return true;
		}
		return false;
	}

	private void ForceShowFriendlyHeroActor()
	{
		Card friendlyHeroCard = GameState.Get().GetFriendlySidePlayer().GetHeroCard();
		if ((bool)friendlyHeroCard)
		{
			friendlyHeroCard.ShowCard();
			if (friendlyHeroCard.GetActor() != null)
			{
				friendlyHeroCard.GetActor().Show();
			}
		}
	}
}
