using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.Core.Streaming;
using Hearthstone.Progression;
using Hearthstone.Streaming;
using UnityEngine;

public class GameEntity : Entity
{
	protected class EndGameScreenContext
	{
		public EndGameScreen m_screen;

		public Spell m_enemyBlowUpSpell;

		public Spell m_friendlyBlowUpSpell;

		public Spell m_endOfGameSpell;
	}

	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static Map<GameEntityOption, string> s_stringOptions = InitStringOptions();

	private Map<string, AudioSource> m_preloadedSounds = new Map<string, AudioSource>();

	private int m_preloadsNeeded;

	private int m_realTimeTurn;

	private int m_realTimeStep;

	private Spell m_endOfGameSpell;

	private const string DefaultEmoteHandlerReference = "EmoteHandler.prefab:5d44be0e8bb7fd14d9fbdbda6a74ab91";

	private const string BattlegroundsEmoteHandlerReference = "BattlegroundsEmoteHandler.prefab:212598c2e67d4b74c85d4913af706d9b";

	private const string EnemyEmoteHandlerReference = "EnemyEmoteHandler.prefab:6ace3edd8826cad4aaa0d0e0eb085012";

	private Coroutine m_destroyHeroTrackingCoroutine;

	private readonly WaitForSeconds MAX_DESTROY_HERO_TIME = new WaitForSeconds(10f);

	private static MonoBehaviour s_coroutines;

	protected GameEntityOptions m_gameOptions = new GameEntityOptions(s_booleanOptions, s_stringOptions);

	private int m_inputBlockerCount;

	public string Uuid { get; set; }

	public List<Network.HistCreateGame.ActionInfo> OnLoadActions { get; } = new List<Network.HistCreateGame.ActionInfo>();

	protected static MonoBehaviour Coroutines
	{
		get
		{
			if (s_coroutines == null)
			{
				s_coroutines = new GameObject().AddComponent<EmptyScript>();
			}
			return s_coroutines;
		}
	}

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool>
		{
			{
				GameEntityOption.ALWAYS_SHOW_MULLIGAN_TIMER,
				false
			},
			{
				GameEntityOption.MULLIGAN_IS_CHOOSE_ONE,
				false
			},
			{
				GameEntityOption.MULLIGAN_TIMER_HAS_ALTERNATE_POSITION,
				false
			},
			{
				GameEntityOption.CARDS_IN_TOOLTIP_SHIFTED_DURING_MULLIGAN,
				false
			},
			{
				GameEntityOption.MULLIGAN_REQUIRES_CONFIRMATION,
				true
			},
			{
				GameEntityOption.MULLIGAN_HAS_HERO_LOBBY,
				false
			},
			{
				GameEntityOption.DIM_OPPOSING_HERO_DURING_MULLIGAN,
				false
			},
			{
				GameEntityOption.HANDLE_COIN,
				true
			},
			{
				GameEntityOption.MULLIGAN_USES_ALTERNATE_ACTORS,
				false
			},
			{
				GameEntityOption.DO_OPENING_TAUNTS,
				true
			},
			{
				GameEntityOption.SUPPRESS_CLASS_NAMES,
				false
			},
			{
				GameEntityOption.USE_SECRET_CLASS_NAMES,
				true
			},
			{
				GameEntityOption.ALLOW_NAME_BANNER_MODE_ICONS,
				true
			},
			{
				GameEntityOption.USE_COMPACT_ENCHANTMENT_BANNERS,
				true
			},
			{
				GameEntityOption.ALLOW_FATIGUE,
				true
			},
			{
				GameEntityOption.MOUSEOVER_DELAY_OVERRIDDEN,
				false
			},
			{
				GameEntityOption.ALLOW_ENCHANTMENT_SPARKLES,
				true
			},
			{
				GameEntityOption.ALLOW_SLEEP_FX,
				true
			},
			{
				GameEntityOption.HAS_ALTERNATE_ENEMY_EMOTE_ACTOR,
				false
			},
			{
				GameEntityOption.USES_PREMIUM_EMOTES,
				false
			},
			{
				GameEntityOption.CAN_SQUELCH_OPPONENT,
				true
			},
			{
				GameEntityOption.KEYWORD_HELP_DELAY_OVERRIDDEN,
				false
			},
			{
				GameEntityOption.SHOW_CRAZY_KEYWORD_TOOLTIP,
				false
			},
			{
				GameEntityOption.SHOW_HERO_TOOLTIPS,
				false
			},
			{
				GameEntityOption.USES_BIG_CARDS,
				true
			},
			{
				GameEntityOption.DISABLE_TOOLTIPS,
				false
			},
			{
				GameEntityOption.DELAY_CARD_SOUND_SPELLS,
				false
			},
			{
				GameEntityOption.DISPLAY_MULLIGAN_DETAIL_LABEL,
				false
			},
			{
				GameEntityOption.WAIT_FOR_RATING_INFO,
				true
			},
			{
				GameEntityOption.DISABLE_POWER_LOGGING,
				false
			}
		};
	}

	private static Map<GameEntityOption, string> InitStringOptions()
	{
		return new Map<GameEntityOption, string>
		{
			{
				GameEntityOption.ALTERNATE_MULLIGAN_ACTOR_NAME,
				null
			},
			{
				GameEntityOption.ALTERNATE_MULLIGAN_LOBBY_ACTOR_NAME,
				null
			},
			{
				GameEntityOption.VICTORY_SCREEN_PREFAB_PATH,
				"VictoryTwoScoop.prefab:b31e3c6c1e80ced4183c3e231c567669"
			},
			{
				GameEntityOption.DEFEAT_SCREEN_PREFAB_PATH,
				"DefeatTwoScoop.prefab:6535dd92d63fce1478220e9bc50e926b"
			},
			{
				GameEntityOption.RULEBOOK_POPUP_PREFAB_PATH,
				null
			},
			{
				GameEntityOption.VICTORY_AUDIO_PATH,
				"victory_jingle.prefab:23f19dd07c7a5114abe5f525099cbac4"
			},
			{
				GameEntityOption.DEFEAT_AUDIO_PATH,
				"defeat_jingle.prefab:0744a10f38e92f1438a02349c29a7b76"
			}
		};
	}

	public void AddInputBlocker()
	{
		m_inputBlockerCount++;
	}

	public void RemoveInputBlocker()
	{
		m_inputBlockerCount--;
	}

	public bool IsInputEnabled()
	{
		return m_inputBlockerCount <= 0;
	}

	private void OnGameplaySceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		if (mode != SceneMgr.Mode.GAMEPLAY)
		{
			return;
		}
		SceneMgr.Get().UnregisterSceneLoadedEvent(OnGameplaySceneLoaded);
		RemoteActionHandler remoteActionHandler = RemoteActionHandler.Get();
		foreach (Network.HistCreateGame.ActionInfo action in OnLoadActions)
		{
			Network.UserUI payload = new Network.UserUI();
			payload.playerId = action.PlayerID;
			payload.selectionInfo = new Network.UserUI.SelectionInfo();
			payload.selectionInfo.SelectedEntityID = action.SelectedEntityID;
			remoteActionHandler.HandleAction(payload);
		}
		OnLoadActions.Clear();
	}

	public GameEntity()
	{
		PreloadAssets();
		SceneMgr.Get().RegisterSceneLoadedEvent(OnGameplaySceneLoaded);
	}

	public virtual void OnCreate()
	{
	}

	public virtual void OnCreateGame()
	{
	}

	public virtual void OnDecommissionGame()
	{
		if (SceneMgr.Get() != null)
		{
			SceneMgr.Get().UnregisterSceneLoadedEvent(OnGameplaySceneLoaded);
		}
	}

	public void FadeOutHeroActor(Actor actorToFade)
	{
		ToggleSpotLight(actorToFade.GetHeroSpotlight(), bOn: false);
		Renderer portraitMeshRenderer = actorToFade.m_portraitMesh.GetComponent<Renderer>();
		Material heroMat = portraitMeshRenderer.GetMaterial(actorToFade.m_portraitMatIdx);
		Material heroFrameMat = portraitMeshRenderer.GetMaterial(actorToFade.m_portraitFrameMatIdx);
		float lightBlend = heroMat.GetFloat("_LightingBlend");
		Action<object> heroLightBlendUpdate = delegate(object amount)
		{
			if (!heroMat || !heroFrameMat)
			{
				Log.Graphics.PrintWarning("Actor's portrait HeroMat or HeroFrameMat materials are null");
			}
			else
			{
				heroMat.SetFloat("_LightingBlend", (float)amount);
				heroFrameMat.SetFloat("_LightingBlend", (float)amount);
				actorToFade.UpdateCustomFrameLightingBlend((float)amount);
				actorToFade.UpdateCustomFrameDiamondMaterial();
			}
		};
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("time", 0.25f);
		args.Add("from", lightBlend);
		args.Add("to", 1f);
		args.Add("onupdate", heroLightBlendUpdate);
		args.Add("onupdatetarget", actorToFade.gameObject);
		iTween.ValueTo(actorToFade.gameObject, args);
	}

	public void FadeOutActor(Actor actorToFade)
	{
		Renderer portraitMeshRenderer = actorToFade.m_portraitMesh.GetComponent<Renderer>();
		Material mat = portraitMeshRenderer.GetMaterial(actorToFade.m_portraitMatIdx);
		Material frameMat = portraitMeshRenderer.GetMaterial(actorToFade.m_portraitFrameMatIdx);
		float lightBlend = mat.GetFloat("_LightingBlend");
		Action<object> lightBlendUpdate = delegate(object amount)
		{
			mat.SetFloat("_LightingBlend", (float)amount);
			frameMat.SetFloat("_LightingBlend", (float)amount);
			actorToFade.UpdateCustomFrameLightingBlend((float)amount);
			actorToFade.UpdateCustomFrameDiamondMaterial();
		};
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("time", 0.25f);
		args.Add("from", lightBlend);
		args.Add("to", 1f);
		args.Add("onupdate", lightBlendUpdate);
		args.Add("onupdatetarget", actorToFade.gameObject);
		iTween.ValueTo(actorToFade.gameObject, args);
	}

	private void ToggleSpotLight(Light light, bool bOn)
	{
		float fadeDuration = 0.1f;
		float defaultIntensity = 1.3f;
		Action<object> changeIntensity = delegate(object amount)
		{
			light.intensity = (float)amount;
		};
		Action<object> disableLight = delegate
		{
			light.enabled = false;
		};
		if (bOn)
		{
			light.enabled = true;
			light.intensity = 0f;
			Hashtable args2 = iTweenManager.Get().GetTweenHashTable();
			args2.Add("time", fadeDuration);
			args2.Add("from", 0f);
			args2.Add("to", defaultIntensity);
			args2.Add("onupdate", changeIntensity);
			args2.Add("onupdatetarget", light.gameObject);
			iTween.ValueTo(light.gameObject, args2);
		}
		else
		{
			Hashtable args3 = iTweenManager.Get().GetTweenHashTable();
			args3.Add("time", fadeDuration);
			args3.Add("from", light.intensity);
			args3.Add("to", 0f);
			args3.Add("onupdate", changeIntensity);
			args3.Add("onupdatetarget", light.gameObject);
			args3.Add("oncomplete", disableLight);
			iTween.ValueTo(light.gameObject, args3);
		}
	}

	public void FadeInHeroActor(Actor actorToFade)
	{
		FadeInHeroActor(actorToFade, 0f);
	}

	public void FadeInHeroActor(Actor actorToFade, float lightBlendAmount)
	{
		if (!actorToFade)
		{
			Log.Graphics.PrintWarning("Actor to fade is null!");
			return;
		}
		ToggleSpotLight(actorToFade.GetHeroSpotlight(), bOn: true);
		if (!actorToFade.m_portraitMesh)
		{
			Log.Graphics.PrintWarning("Actor's portrait mesh is null!");
			return;
		}
		Renderer portraitMeshComponentRender = actorToFade.m_portraitMesh.GetComponent<Renderer>();
		if (!portraitMeshComponentRender)
		{
			Log.Graphics.PrintWarning("Actor's portrait mesh component render is null!");
			return;
		}
		Material heroMat = portraitMeshComponentRender.GetMaterial(actorToFade.m_portraitMatIdx);
		Material heroFrameMat = portraitMeshComponentRender.GetMaterial(actorToFade.m_portraitFrameMatIdx);
		if (!heroMat || !heroFrameMat)
		{
			Log.Graphics.PrintWarning("Actor's portrait HeroMat or HeroFrameMat materials are null");
			return;
		}
		float lightBlend = heroMat.GetFloat("_LightingBlend");
		Action<object> heroLightBlendUpdate = delegate(object amount)
		{
			if (!heroMat || !heroFrameMat)
			{
				Log.Graphics.PrintWarning("Actor's portrait HeroMat or HeroFrameMat materials are null");
			}
			else
			{
				heroMat.SetFloat("_LightingBlend", (float)amount);
				heroFrameMat.SetFloat("_LightingBlend", (float)amount);
				actorToFade.UpdateCustomFrameLightingBlend((float)amount);
				actorToFade.UpdateCustomFrameDiamondMaterial();
			}
		};
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("time", 0.25f);
		args.Add("from", lightBlend);
		args.Add("to", lightBlendAmount);
		args.Add("onupdate", heroLightBlendUpdate);
		args.Add("onupdatetarget", actorToFade.gameObject);
		iTween.ValueTo(actorToFade.gameObject, args);
	}

	public void FadeInActor(Actor actorToFade)
	{
		FadeInActor(actorToFade, 0f);
	}

	public void FadeInActor(Actor actorToFade, float lightBlendAmount)
	{
		Renderer portraitMeshRenderer = actorToFade.m_portraitMesh.GetComponent<Renderer>();
		Material mat = portraitMeshRenderer.GetMaterial(actorToFade.m_portraitMatIdx);
		Material frameMat = portraitMeshRenderer.GetMaterial(actorToFade.m_portraitFrameMatIdx);
		float lightBlend = mat.GetFloat("_LightingBlend");
		Action<object> actorLightBlendUpdate = delegate(object amount)
		{
			mat.SetFloat("_LightingBlend", (float)amount);
			frameMat.SetFloat("_LightingBlend", (float)amount);
			actorToFade.UpdateCustomFrameLightingBlend((float)amount);
			actorToFade.UpdateCustomFrameDiamondMaterial();
		};
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("time", 0.25f);
		args.Add("from", lightBlend);
		args.Add("to", lightBlendAmount);
		args.Add("onupdate", actorLightBlendUpdate);
		args.Add("onupdatetarget", actorToFade.gameObject);
		iTween.ValueTo(actorToFade.gameObject, args);
	}

	public void PreloadSound(string soundPath)
	{
		m_preloadsNeeded++;
		SoundLoader.LoadSound(soundPath, OnSoundLoaded, null, SoundManager.Get().GetPlaceholderSound());
	}

	protected void PreloadPrefab(AssetReference assetRef, PrefabCallback<GameObject> callback, object callbackData = null, AssetLoadingOptions options = AssetLoadingOptions.None)
	{
		m_preloadsNeeded++;
		PrefabCallback<GameObject> onLoadAttempted = delegate(AssetReference loadedAssetRef, GameObject loadedGameObject, object loadedCallbackData)
		{
			m_preloadsNeeded--;
			callback(loadedAssetRef, loadedGameObject, loadedCallbackData);
		};
		if (!AssetLoader.Get().InstantiatePrefab(assetRef, onLoadAttempted, callbackData, options))
		{
			onLoadAttempted(assetRef, null, callbackData);
		}
	}

	private void OnSoundLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_preloadsNeeded--;
		if (assetRef == null)
		{
			Debug.LogWarning(string.Format("GameEntity.OnSoundLoaded() - ERROR missing Asset Ref for sound!", assetRef));
			return;
		}
		if (go == null)
		{
			Debug.LogWarning($"GameEntity.OnSoundLoaded() - FAILED to load \"{assetRef}\"");
			return;
		}
		AudioSource source = go.GetComponent<AudioSource>();
		if (source == null)
		{
			Debug.LogWarning($"GameEntity.OnSoundLoaded() - ERROR \"{assetRef}\" has no Spell component");
			return;
		}
		string assetStrng = assetRef.ToString();
		if (!CheckPreloadedSound(assetStrng))
		{
			m_preloadedSounds.Add(assetStrng, source);
		}
	}

	public void RemovePreloadedSound(string soundPath)
	{
		m_preloadedSounds.Remove(soundPath);
	}

	public bool CheckPreloadedSound(string soundPath)
	{
		AudioSource sound;
		return m_preloadedSounds.TryGetValue(soundPath, out sound);
	}

	public AudioSource GetPreloadedSound(string soundPath)
	{
		if (m_preloadedSounds.TryGetValue(soundPath, out var sound))
		{
			return sound;
		}
		Debug.LogError($"GameEntity.GetPreloadedSound() - \"{soundPath}\" was not preloaded");
		return null;
	}

	public bool IsPreloadingAssets()
	{
		return m_preloadsNeeded > 0;
	}

	public GameEntityOptions GetGameOptions()
	{
		return m_gameOptions;
	}

	public int GetRealTimeStep()
	{
		return m_realTimeStep;
	}

	public override bool HasValidDisplayName()
	{
		return false;
	}

	public override string GetName()
	{
		return "GameEntity";
	}

	public override string GetDebugName()
	{
		return "GameEntity";
	}

	public override void OnTagsChanged(TagDeltaList changeList, bool fromShowEntity)
	{
		for (int i = 0; i < changeList.Count; i++)
		{
			TagDelta change = changeList[i];
			OnTagChanged(change);
		}
	}

	public override void InitRealTimeValues(List<Network.Entity.Tag> tags)
	{
		base.InitRealTimeValues(tags);
		foreach (Network.Entity.Tag tag in tags)
		{
			switch ((GAME_TAG)tag.Name)
			{
			case GAME_TAG.TURN:
				SetRealTimeTurn(tag.Value);
				GameState.Get().TriggerTurnTimerUpdateForTurn(tag.Value);
				break;
			case GAME_TAG.STEP:
				SetRealTimeStep(tag.Value);
				break;
			case GAME_TAG.COIN_MANA_GEM:
				if (tag.Value != 0)
				{
					ManaCrystalMgr.Get().SetManaCrystalType(ManaCrystalType.COIN);
				}
				break;
			case GAME_TAG.BOARD_VISUAL_STATE:
				if (tag.Value > 0)
				{
					Board.Get().ChangeBoardVisualState((TAG_BOARD_VISUAL_STATE)tag.Value);
				}
				break;
			}
		}
	}

	public override void OnRealTimeTagChanged(Network.HistTagChange change)
	{
		switch ((GAME_TAG)change.Tag)
		{
		case GAME_TAG.MISSION_EVENT:
			HandleRealTimeMissionEvent(change.Value);
			break;
		case GAME_TAG.TURN:
			SetRealTimeTurn(change.Value);
			EndTurnButton.Get().OnTurnChanged();
			GameState.Get().UpdateOptionHighlights();
			break;
		case GAME_TAG.STEP:
			SetRealTimeStep(change.Value);
			break;
		case GAME_TAG.COIN_MANA_GEM:
			if (change.Value != 0)
			{
				ManaCrystalMgr.Get().SetManaCrystalType(ManaCrystalType.COIN);
			}
			break;
		case GAME_TAG.BACON_COMBAT_DAMAGE_CAP_ENABLED:
			SetRealtimeBaconDamageCapEnabled(change.Value);
			break;
		}
	}

	public override void OnTagChanged(TagDelta change)
	{
		base.OnTagChanged(change);
		switch ((GAME_TAG)change.tag)
		{
		case GAME_TAG.TURN:
			EndTurnButton.Get().OnTurnChanged();
			GameState.Get().UpdateOptionHighlights();
			InputManager.Get()?.HidePlayerStarshipUI();
			break;
		case GAME_TAG.END_TURN_BUTTON_ALTERNATIVE_APPEARANCE:
			EndTurnButton.Get().ApplyAlternativeAppearance();
			break;
		case GAME_TAG.TURN_INDICATOR_ALTERNATIVE_APPEARANCE:
			TurnStartManager.Get().ApplyAlternativeAppearance();
			break;
		case GAME_TAG.BOARD_VISUAL_STATE:
			Board.Get().ChangeBoardVisualState((TAG_BOARD_VISUAL_STATE)change.newValue);
			break;
		case GAME_TAG.BACON_CHOSEN_BOARD_SKIN_ID:
			BaconBoard.Get()?.OnBoardSkinChosen(change.newValue);
			break;
		case GAME_TAG.DECK_SWAP_ACTIVE:
			if (change.oldValue == 0 || change.newValue == 0)
			{
				Board.Get().PlayDeckSwapSpell();
			}
			break;
		case GAME_TAG.BACON_COMBAT_DAMAGE_CAP_ENABLED:
			PlayerLeaderboardManager.Get().UpdateDamageCap();
			break;
		}
	}

	private void SetRealTimeTurn(int turn)
	{
		m_realTimeTurn = turn;
	}

	private void SetRealTimeStep(int step)
	{
		m_realTimeStep = step;
	}

	public bool IsCurrentTurnRealTime()
	{
		return m_realTimeTurn == GetTag(GAME_TAG.TURN);
	}

	public bool IsMulliganActiveRealTime()
	{
		return m_realTimeStep <= 4;
	}

	public virtual void PreloadAssets()
	{
	}

	public virtual void NotifyOfStartOfTurnEventsFinished()
	{
	}

	public virtual bool NotifyOfEndTurnButtonPushed()
	{
		return true;
	}

	public virtual bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
	{
		return true;
	}

	public virtual void NotifyOfCardMousedOver(Entity mousedOverEntity)
	{
	}

	public virtual void NotifyOfCardMousedOff(Entity mousedOffEntity)
	{
	}

	public virtual bool NotifyOfCardTooltipDisplayShow(Card card)
	{
		return true;
	}

	public virtual void NotifyOfBigCardForCardInPlayShown(Actor mainCardActor, Actor extraCardActor)
	{
	}

	public virtual void NotifyOfCardTooltipDisplayHide(Card card)
	{
	}

	public virtual void NotifyOfCardTooltipBigCardActorShow()
	{
	}

	public virtual void NotifyOfCoinFlipResult()
	{
	}

	public virtual bool NotifyOfPlayError(PlayErrors.ErrorType error, int? errorParam, Entity errorSource)
	{
		return false;
	}

	public virtual string[] NotifyOfKeywordHelpPanelDisplay(Entity entity)
	{
		return null;
	}

	public virtual List<TooltipPanelManager.TooltipPanelData> GetOverwriteKeywordHelpPanelDisplay(Entity entity)
	{
		return null;
	}

	public virtual bool GetEntityBaseForKeywordTooltips(Entity source, bool isHistoryTile, out EntityBase entityBaseForTooltips, out List<EntityBase> additionalEntityBaseForTooltips)
	{
		entityBaseForTooltips = null;
		additionalEntityBaseForTooltips = null;
		return false;
	}

	public virtual bool SuppressMousedOverCardTooltip(out bool resetTimer)
	{
		resetTimer = false;
		return false;
	}

	public virtual void NotifyOfCardGrabbed(Entity entity)
	{
	}

	public virtual void NotifyOfCardDropped(Entity entity)
	{
	}

	public virtual void NotifyOfTargetModeStarted(Entity entity)
	{
	}

	public virtual void NotifyOfTargetModeCancelled()
	{
	}

	public virtual void NotifyOfHelpPanelDisplay(int numPanels)
	{
	}

	public virtual void NotifyOfDebugCommand(int command)
	{
	}

	public virtual void NotifyOfManaCrystalSpawned()
	{
	}

	public virtual void NotifyOfEnemyManaCrystalSpawned()
	{
	}

	public virtual void NotifyOfTooltipZoneMouseOver(TooltipZone tooltip)
	{
	}

	public virtual void NotifyOfHistoryTokenMousedOver(GameObject mousedOverTile)
	{
	}

	public virtual void NotifyOfHistoryTokenMousedOut()
	{
	}

	public virtual void NotifyOfCustomIntroFinished()
	{
	}

	public virtual void NotifyOfGameOver(TAG_PLAYSTATE playState)
	{
		PegCursor.Get().SetMode(PegCursor.Mode.STOPWAITING);
		MusicManager.Get().StartPlaylist(MusicPlaylistType.UI_EndGameScreen);
		Card enemyHeroCard = GameState.Get().GetOpposingSidePlayer().GetHeroCard();
		Card friendlyHeroCard = GameState.Get().GetFriendlySidePlayer().GetHeroCard();
		Gameplay.Get().SaveOriginalTimeScale();
		AchievementManager.Get()?.PauseToastNotifications();
		Spell enemyBlowUpSpell = null;
		Spell friendlyBlowUpSpell = null;
		if (ShouldPlayHeroBlowUpSpells(playState))
		{
			switch (playState)
			{
			case TAG_PLAYSTATE.WON:
			{
				string audioAsset = GetGameOptions().GetStringOption(GameEntityOption.VICTORY_AUDIO_PATH);
				if (!string.IsNullOrEmpty(audioAsset))
				{
					SoundManager.Get().LoadAndPlay(audioAsset);
				}
				enemyBlowUpSpell = BlowUpHero(enemyHeroCard, SpellType.ENDGAME_LOSE_ENEMY);
				break;
			}
			case TAG_PLAYSTATE.LOST:
			{
				string audioAsset = GetGameOptions().GetStringOption(GameEntityOption.DEFEAT_AUDIO_PATH);
				if (!string.IsNullOrEmpty(audioAsset))
				{
					SoundManager.Get().LoadAndPlay(audioAsset);
				}
				friendlyBlowUpSpell = BlowUpHero(friendlyHeroCard, SpellType.ENDGAME_LOSE_FRIENDLY);
				break;
			}
			case TAG_PLAYSTATE.TIED:
			{
				string audioAsset = GetGameOptions().GetStringOption(GameEntityOption.DEFEAT_AUDIO_PATH);
				if (!string.IsNullOrEmpty(audioAsset))
				{
					SoundManager.Get().LoadAndPlay(audioAsset);
				}
				enemyBlowUpSpell = BlowUpHero(enemyHeroCard, SpellType.ENDGAME_DRAW);
				friendlyBlowUpSpell = BlowUpHero(friendlyHeroCard, SpellType.ENDGAME_DRAW);
				break;
			}
			}
		}
		ShowEndGameScreen(playState, enemyBlowUpSpell, friendlyBlowUpSpell);
	}

	public virtual void NotifyOfRealTimeTagChange(Entity entity, Network.HistTagChange tagChange)
	{
	}

	public virtual void ToggleAlternateMulliganActorHighlight(Card card, bool highlighted)
	{
	}

	public virtual void ToggleAlternateMulliganActorConfirmHighlight(Card card, bool highlighted)
	{
	}

	public virtual bool ToggleAlternateMulliganActorHighlight(Actor actor, bool? highlighted = null)
	{
		return false;
	}

	public virtual bool ShouldPlayHeroBlowUpSpells(TAG_PLAYSTATE playState)
	{
		return true;
	}

	public virtual string GetVictoryScreenBannerText()
	{
		return GameStrings.Get("GAMEPLAY_END_OF_GAME_VICTORY");
	}

	public virtual string GetDefeatScreenBannerText()
	{
		return GameStrings.Get("GAMEPLAY_END_OF_GAME_DEFEAT");
	}

	public virtual string GetTieScreenBannerText()
	{
		return GameStrings.Get("GAMEPLAY_END_OF_GAME_TIE");
	}

	public virtual void NotifyOfHeroesFinishedAnimatingInMulligan()
	{
	}

	public virtual bool NotifyOfTooltipDisplay(TooltipZone tooltip)
	{
		return false;
	}

	public virtual void NotifyOfTooltipHide(TooltipZone tooltip)
	{
	}

	public virtual void NotifyOfMulliganInitialized()
	{
		if (!GameMgr.Get().IsTraditionalTutorial())
		{
			if (GameMgr.Get().IsBattlegroundsMatchOrTutorial())
			{
				AssetLoader.Get().InstantiatePrefab("BattlegroundsEmoteHandler.prefab:212598c2e67d4b74c85d4913af706d9b", EmoteHandlerDoneLoadingCallback, null, AssetLoadingOptions.IgnorePrefabPosition);
			}
			else
			{
				AssetLoader.Get().InstantiatePrefab("EmoteHandler.prefab:5d44be0e8bb7fd14d9fbdbda6a74ab91", EmoteHandlerDoneLoadingCallback, null, AssetLoadingOptions.IgnorePrefabPosition);
			}
			if ((!GameMgr.Get().IsAI() || GameUtils.IsMatchmadeGameType(GameMgr.Get().GetGameType(), null)) && GetGameOptions().GetBooleanOption(GameEntityOption.CAN_SQUELCH_OPPONENT))
			{
				AssetLoader.Get().InstantiatePrefab("EnemyEmoteHandler.prefab:6ace3edd8826cad4aaa0d0e0eb085012", EnemyEmoteHandlerDoneLoadingCallback, null, AssetLoadingOptions.IgnorePrefabPosition);
			}
		}
	}

	public virtual AudioSource GetAnnouncerLine(Card heroCard, Card.AnnouncerLineType type)
	{
		return heroCard.GetAnnouncerLine(type);
	}

	public virtual void NotifyOfMulliganEnded()
	{
	}

	private void EmoteHandlerDoneLoadingCallback(AssetReference assetRef, GameObject go, object callbackData)
	{
		go.transform.position = ZoneMgr.Get().FindZoneOfType<ZoneHero>(Player.Side.FRIENDLY).transform.position;
	}

	protected virtual void EnemyEmoteHandlerDoneLoadingCallback(AssetReference assetRef, GameObject go, object callbackData)
	{
		go.transform.position = ZoneMgr.Get().FindZoneOfType<ZoneHero>(Player.Side.OPPOSING).transform.position;
	}

	public virtual void NotifyOfGamePackOpened()
	{
	}

	public virtual void NotifyOfDefeatCoinAnimation()
	{
	}

	public virtual void SendCustomEvent(int eventID)
	{
	}

	public virtual string GetTurnStartReminderText()
	{
		return "";
	}

	public virtual void NotifyMulliganButtonReady()
	{
	}

	public virtual bool IsHeroMulliganLobbyFinished()
	{
		return true;
	}

	public virtual bool IsTeammateHeroMulliganFinished()
	{
		return true;
	}

	public virtual Entity GetFriendlyTeammateHeroEntity()
	{
		return null;
	}

	public virtual void NotifyHeroMulliganLobbyFinished()
	{
	}

	public virtual void NotifyMulligainHeroSelected(Actor heroActor)
	{
	}

	public virtual bool HeroRequiresDoubleConfirmation(Entity mulliganHeroEntity)
	{
		return false;
	}

	public virtual string GetMultiStepMulliganConfirmButtonText(Entity mulliganHeroEntity)
	{
		return null;
	}

	public virtual ActorStateType GetMulliganChoiceHighlightState()
	{
		return ActorStateType.CARD_IDLE;
	}

	public virtual bool ShouldDelayShowingCardInTooltip()
	{
		return true;
	}

	public virtual Vector3 NameBannerPosition(Player.Side side)
	{
		if (side == Player.Side.FRIENDLY)
		{
			return new Vector3(0f, 5f, 22f);
		}
		return new Vector3(0f, 5f, -10f);
	}

	public virtual void PlayAlternateEnemyEmote(int playerId, EmoteType emoteType, int battlegroundsEmoteId = 0)
	{
	}

	public virtual Vector3 GetMulliganTimerAlternatePosition()
	{
		return Vector3.zero;
	}

	private bool ShouldSkipMulligan()
	{
		return HasTag(GAME_TAG.SKIP_MULLIGAN);
	}

	public virtual bool ShouldDoAlternateMulliganIntro()
	{
		return ShouldSkipMulligan();
	}

	public virtual bool DoAlternateMulliganIntro()
	{
		if (ShouldSkipMulligan())
		{
			Coroutines.StartCoroutine(SkipStandardMulliganWithTiming());
			return true;
		}
		return false;
	}

	protected IEnumerator SkipStandardMulliganWithTiming()
	{
		GameState.Get().SetMulliganBusy(busy: true);
		SceneMgr.Get().NotifySceneLoaded();
		while (LoadingScreen.Get().IsPreviousSceneActive() || LoadingScreen.Get().IsFadingOut())
		{
			yield return null;
		}
		GameMgr.Get().UpdatePresence();
		MulliganManager.Get().SkipMulligan();
	}

	public virtual string GetMulliganDetailText()
	{
		return null;
	}

	public virtual void OnMulliganCardsDealt(List<Card> startingCards)
	{
	}

	public virtual void OnMulliganBeginDealNewCards()
	{
	}

	public virtual float GetAdditionalTimeToWaitForSpells()
	{
		return 0f;
	}

	public virtual bool ShouldShowBigCard()
	{
		return true;
	}

	public virtual string GetBestNameForPlayer(int playerId)
	{
		string playerName = ((GameState.Get().GetPlayerMap().ContainsKey(playerId) && GameState.Get().GetPlayerMap()[playerId] != null) ? GameState.Get().GetPlayerMap()[playerId].GetName() : null);
		bool num = GameState.Get().GetPlayerMap().ContainsKey(playerId) && GameState.Get().GetPlayerMap()[playerId].IsFriendlySide();
		bool streamerModeEnabled = Options.Get().GetBool(Option.STREAMER_MODE);
		if (num)
		{
			if (streamerModeEnabled || playerName == null)
			{
				return GameStrings.Get("GAMEPLAY_HIDDEN_PLAYER_NAME");
			}
			return playerName;
		}
		if (streamerModeEnabled)
		{
			return GameStrings.Get("GAMEPLAY_MISSING_OPPONENT_NAME");
		}
		if (playerName == null)
		{
			return GameStrings.Get("GAMEPLAY_MISSING_OPPONENT_NAME");
		}
		return playerName;
	}

	public virtual List<RewardData> GetCustomRewards()
	{
		return null;
	}

	public virtual void HandleRealTimeMissionEvent(int missionEvent)
	{
	}

	public virtual void OnPlayThinkEmote()
	{
		if (!GameMgr.Get().IsAI())
		{
			EmoteType thinkEmote = EmoteType.THINK1;
			switch (UnityEngine.Random.Range(1, 4))
			{
			case 1:
				thinkEmote = EmoteType.THINK1;
				break;
			case 2:
				thinkEmote = EmoteType.THINK2;
				break;
			case 3:
				thinkEmote = EmoteType.THINK3;
				break;
			}
			GameState.Get().GetCurrentPlayer().GetHeroCard()?.PlayEmote(thinkEmote);
		}
	}

	public virtual IEnumerator OnPlayThinkEmoteWithTiming()
	{
		if (!GameMgr.Get().IsAI())
		{
			EmoteType thinkEmote = EmoteType.THINK1;
			switch (UnityEngine.Random.Range(1, 4))
			{
			case 1:
				thinkEmote = EmoteType.THINK1;
				break;
			case 2:
				thinkEmote = EmoteType.THINK2;
				break;
			case 3:
				thinkEmote = EmoteType.THINK3;
				break;
			}
			AudioSource activeAudioSource = GameState.Get().GetCurrentPlayer().GetHeroCard()
				.PlayEmote(thinkEmote)
				.GetActiveAudioSource();
			yield return new WaitForSeconds(activeAudioSource.clip.length);
		}
	}

	public virtual void OnEmotePlayed(Card card, EmoteType emoteType, CardSoundSpell emoteSpell)
	{
	}

	public virtual void NotifyOfOpponentWillPlayCard(string cardId, Entity playedEntity)
	{
	}

	public virtual void NotifyOfOpponentPlayedCard(Entity entity)
	{
	}

	public virtual void NotifyOfFriendlyPlayedCard(Entity entity)
	{
	}

	public virtual void NotifyOfResetGameStarted()
	{
	}

	public virtual void NotifyOfResetGameFinished(Entity source, Entity oldGameEntity)
	{
	}

	public virtual void NotifyOfEntityAttacked(Entity attacker, Entity defender)
	{
	}

	public virtual void NotifyOfMinionPlayed(Entity minion)
	{
	}

	public virtual void NotifyOfHeroChanged(Entity newHero)
	{
	}

	public virtual void NotifyOfWeaponEquipped(Entity weapon)
	{
	}

	public virtual void NotifyOfSpellPlayed(Entity spell, Entity target)
	{
	}

	public virtual void NotifyOfHeroPowerUsed(Entity heroPower, Entity target)
	{
	}

	public virtual void NotifyOfMinionDied(Entity minion)
	{
	}

	public virtual void NotifyOfHeroDied(Entity hero)
	{
	}

	public virtual void NotifyOfWeaponDestroyed(Entity weapon)
	{
	}

	public virtual string UpdateCardText(Card card, Actor bigCardActor, string text)
	{
		return text;
	}

	public virtual void ApplyMulliganActorStateChanges(Actor baseActor)
	{
	}

	public virtual void ApplyMulliganActorLobbyStateChanges(Actor baseActor)
	{
	}

	public virtual void ClearMulliganActorStateChanges(Actor baseActor)
	{
	}

	public virtual string GetMulliganBannerText()
	{
		return GameStrings.Get("GAMEPLAY_MULLIGAN_STARTING_HAND");
	}

	public virtual string GetMulliganBannerSubtitleText()
	{
		return GameStrings.Get("GAMEPLAY_MULLIGAN_SUBTITLE");
	}

	public virtual string GetMulliganWaitingText()
	{
		return GameStrings.Get("GAMEPLAY_MULLIGAN_STARTING_HAND");
	}

	public virtual string GetMulliganWaitingSubtitleText()
	{
		return null;
	}

	public virtual Vector3 GetAlternateMulliganActorScale()
	{
		return new Vector3(1f, 1f, 1f);
	}

	public virtual int GetNumberOfFakeMulliganCardsToShowOnLeft(int numOriginalCards)
	{
		return 0;
	}

	public virtual int GetNumberOfFakeMulliganCardsToShowOnRight(int numOriginalCards)
	{
		return 0;
	}

	public virtual void ConfigureFakeMulliganCardActor(Actor actor, bool shown)
	{
	}

	public virtual void ConfigureLockedMulliganCardActor(Actor actor, bool shown)
	{
	}

	public virtual Entity GetExtraMouseOverBigCardEntity(Entity source)
	{
		return null;
	}

	public virtual bool ShowMouseOverBigCardImmediately(Entity mouseOverEntity)
	{
		return false;
	}

	public virtual bool ShouldSuppressCardMouseOver(Entity mouseOverEntity)
	{
		return false;
	}

	public virtual bool ShouldSuppressHistoryMouseOver()
	{
		return false;
	}

	public virtual bool ShouldSuppressOptionHighlight(Entity entity)
	{
		return false;
	}

	public virtual bool IsGameSpeedupConditionInEffect()
	{
		return false;
	}

	public virtual void StartMulliganSoundtracks(bool soft)
	{
		if (soft)
		{
			MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_MulliganSoft);
		}
		else
		{
			MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_Mulligan);
		}
	}

	public virtual void StartGameplaySoundtracks()
	{
		Board board = Board.Get();
		MusicPlaylistType boardMusic = MusicPlaylistType.InGame_Default;
		bool shouldFallbackToDefaultMusic = board == null;
		bool isExpansionMusicAvailable = GameDownloadManagerProvider.Get().IsReadyAssetsInTags(new string[2]
		{
			DownloadTags.GetTagString(DownloadTags.Quality.MusicExpansion),
			DownloadTags.GetTagString(DownloadTags.Content.Base)
		});
		if (!(shouldFallbackToDefaultMusic || !isExpansionMusicAvailable))
		{
			boardMusic = board.m_BoardMusic;
		}
		MusicManager.Get().StartPlaylist(boardMusic);
	}

	public virtual string GetAlternatePlayerName()
	{
		return "";
	}

	public virtual void QueueEntityForRemoval(Entity entity)
	{
	}

	public virtual IEnumerator PlayMissionIntroLineAndWait()
	{
		yield break;
	}

	public virtual IEnumerator DoActionsAfterIntroBeforeMulligan()
	{
		yield break;
	}

	public virtual IEnumerator DoActionsBeforeDealingBaseMulliganCards()
	{
		yield break;
	}

	public virtual IEnumerator DoActionsAfterDealingBaseMulliganCards()
	{
		yield break;
	}

	public virtual IEnumerator DoActionsBeforeCoinFlip()
	{
		yield break;
	}

	public virtual IEnumerator DoActionsAfterCoinFlip()
	{
		yield break;
	}

	public virtual IEnumerator DoActionsAfterDealingBonusCard()
	{
		yield break;
	}

	public virtual IEnumerator DoActionsBeforeSpreadingMulliganCards()
	{
		yield break;
	}

	public virtual IEnumerator DoActionsAfterSpreadingMulliganCards()
	{
		yield break;
	}

	public virtual IEnumerator DoGameSpecificPostIntroActions()
	{
		yield break;
	}

	public virtual IEnumerator DoCustomIntro(Card friendlyHero, Card enemyHero, HeroLabel friendlyHeroLabel, HeroLabel enemyHeroLabel, GameStartVsLetters versusText)
	{
		yield break;
	}

	public virtual void OnCustomIntroCancelled(Card friendlyHero, Card enemyHero, HeroLabel friendlyHeroLabel, HeroLabel enemyHeroLabel, GameStartVsLetters versusText)
	{
	}

	public virtual bool ShouldAllowCardGrab(Entity entity)
	{
		return true;
	}

	public virtual string CustomChoiceBannerText()
	{
		return null;
	}

	public virtual InputManager.ZoneTooltipSettings GetZoneTooltipSettings()
	{
		return new InputManager.ZoneTooltipSettings();
	}

	public virtual bool IsInBattlegroundsShopPhase()
	{
		return false;
	}

	public virtual bool IsInBattlegroundsCombatPhase()
	{
		return false;
	}

	public virtual bool IsStateChangePopupVisible()
	{
		return false;
	}

	protected virtual Spell BlowUpHero(Card card, SpellType spellType)
	{
		if (card == null)
		{
			return null;
		}
		Actor cardActor = card.GetActor();
		if (cardActor != null)
		{
			cardActor.ActivateAllSpellsDeathStates();
		}
		Spell result = ActivateSpellForDestroyedHero(card, spellType);
		Gameplay.Get().StartCoroutine(HideOtherElements(card));
		return result;
	}

	protected virtual Spell ActivateSpellForDestroyedHero(Card card, SpellType spellType)
	{
		if (card == null)
		{
			return null;
		}
		Card enemyHeroCard = GameState.Get().GetOpposingSidePlayer().GetHeroCard();
		Card friendlyHeroCard = GameState.Get().GetFriendlySidePlayer().GetHeroCard();
		if (spellType == SpellType.ENDGAME_DRAW)
		{
			if (card == friendlyHeroCard)
			{
				return card.ActivateActorSpell(SpellType.ENDGAME_LOSE_FRIENDLY);
			}
			return card.ActivateActorSpell(spellType);
		}
		Player dyingPlayer = card.GetController();
		DeathSpellType deathSpellType = (DeathSpellType)dyingPlayer.GetTag(GAME_TAG.DEATH_SPELL_OVERRIDE);
		Spell spell = DeathSpellConfig.Get().GetSpell(deathSpellType);
		if (spell != null)
		{
			spell.SetSource(card.gameObject);
			spell.AddFinishedCallback(OnFriendlyHeroDestroyed);
			m_destroyHeroTrackingCoroutine = spell.StartCoroutine(EnsureHeroDestroyedCompletes(spell));
			spell.Activate();
			return spell;
		}
		Card obj = ((card == enemyHeroCard) ? friendlyHeroCard : enemyHeroCard);
		SpellType opposingSpellType = ((card == enemyHeroCard) ? SpellType.ENDGAME_VICTORY_STRIKE_FRIENDLY : SpellType.ENDGAME_VICTORY_STRIKE_ENEMY);
		Spell victoryStrike = obj.GetActorSpell(opposingSpellType);
		if (victoryStrike != null)
		{
			List<GameObject> enemyMinions = new List<GameObject>();
			foreach (Card battlefieldCard in dyingPlayer.GetBattlefieldZone().GetCards())
			{
				Entity cardEntity = battlefieldCard.GetEntity();
				if (cardEntity.GetController() == dyingPlayer && (cardEntity.IsMinion() || cardEntity.IsLocation()))
				{
					enemyMinions.Add(battlefieldCard.gameObject);
				}
			}
			victoryStrike.AddTargets(enemyMinions);
			victoryStrike.ActivateState(SpellStateType.ACTION);
			return victoryStrike;
		}
		return card.ActivateActorSpell(spellType);
	}

	private void OnFriendlyHeroDestroyed(Spell spell, object _)
	{
		if (m_destroyHeroTrackingCoroutine != null)
		{
			spell.StopCoroutine(m_destroyHeroTrackingCoroutine);
			m_destroyHeroTrackingCoroutine = null;
		}
	}

	private IEnumerator EnsureHeroDestroyedCompletes(Spell spell)
	{
		yield return MAX_DESTROY_HERO_TIME;
		m_destroyHeroTrackingCoroutine = null;
		Log.Spells.PrintError("Destroy hero spell " + spell.gameObject.name + " did not terminate and was killed to prevent game hang. Run the finisher in the authoring scene to diagnose potential problems.");
		spell.ReleaseSpell();
	}

	protected IEnumerator HideOtherElements(Card card)
	{
		yield return new WaitForSeconds(0.5f);
		Player controller = card.GetEntity().GetController();
		if (controller.GetHeroPowerCard() != null)
		{
			controller.GetHeroPowerCard().HideCard();
			controller.GetHeroPowerCard().GetActor().ToggleForceIdle(bOn: true);
			controller.GetHeroPowerCard().GetActor().SetActorState(ActorStateType.CARD_IDLE);
			controller.GetHeroPowerCard().GetActor().DoCardDeathVisuals();
			controller.GetHeroPowerCard().DeactivateCustomKeywordEffect();
		}
		if (controller.GetWeaponCard() != null)
		{
			controller.GetWeaponCard().HideCard();
			controller.GetWeaponCard().GetActor().ToggleForceIdle(bOn: true);
			controller.GetWeaponCard().GetActor().SetActorState(ActorStateType.CARD_IDLE);
			controller.GetWeaponCard().GetActor().DoCardDeathVisuals();
		}
		card.GetActor().HideArmorSpell();
		GemObject healthObject = card.GetActor().GetHealthObject();
		if (healthObject != null)
		{
			healthObject.Hide();
		}
		GemObject attackObject = card.GetActor().GetAttackObject();
		if (attackObject != null)
		{
			attackObject.Hide();
		}
		card.GetActor().ToggleForceIdle(bOn: true);
		card.GetActor().SetActorState(ActorStateType.CARD_IDLE);
	}

	protected void ShowEndGameScreen(TAG_PLAYSTATE playState, Spell enemyBlowUpSpell, Spell friendlyBlowUpSpell)
	{
		string endGameScreenName = null;
		switch (playState)
		{
		case TAG_PLAYSTATE.WON:
			endGameScreenName = GetGameOptions().GetStringOption(GameEntityOption.VICTORY_SCREEN_PREFAB_PATH);
			break;
		case TAG_PLAYSTATE.LOST:
		case TAG_PLAYSTATE.TIED:
			endGameScreenName = GetGameOptions().GetStringOption(GameEntityOption.DEFEAT_SCREEN_PREFAB_PATH);
			break;
		}
		if (endGameScreenName == null)
		{
			return;
		}
		GameObject endGameScreenObject = AssetLoader.Get().InstantiatePrefab(endGameScreenName, AssetLoadingOptions.IgnorePrefabPosition);
		if (!endGameScreenObject)
		{
			Debug.LogErrorFormat("GameEntity.ShowEndGameScreen() - FAILED to load \"{0}\"", endGameScreenName);
			return;
		}
		EndGameScreen screen = endGameScreenObject.GetComponent<EndGameScreen>();
		if (!screen)
		{
			Debug.LogErrorFormat("GameEntity.ShowEndGameScreen() - \"{0}\" does not have an EndGameScreen component", endGameScreenName);
			return;
		}
		EndGameScreenContext context = new EndGameScreenContext();
		context.m_screen = screen;
		context.m_enemyBlowUpSpell = enemyBlowUpSpell;
		context.m_friendlyBlowUpSpell = friendlyBlowUpSpell;
		context.m_endOfGameSpell = ActivateEndOfGameSpell();
		if ((bool)enemyBlowUpSpell && !enemyBlowUpSpell.IsFinished())
		{
			enemyBlowUpSpell.AddFinishedCallback(OnBlowUpSpellFinished, context);
		}
		if ((bool)friendlyBlowUpSpell && !friendlyBlowUpSpell.IsFinished())
		{
			friendlyBlowUpSpell.AddFinishedCallback(OnBlowUpSpellFinished, context);
		}
		if (context.m_endOfGameSpell != null && !context.m_endOfGameSpell.IsFinished())
		{
			context.m_endOfGameSpell.AddFinishedCallback(OnBlowUpSpellFinished, context);
		}
		ShowEndGameScreenAfterEffects(context);
	}

	public virtual bool ShouldShowHeroClassDuringMulligan(Player.Side playerSide)
	{
		return true;
	}

	public virtual bool ShouldUseAlternateNameForPlayer(Player.Side side)
	{
		return false;
	}

	public virtual string GetNameBannerOverride(Player.Side side)
	{
		return null;
	}

	public virtual string GetNameBannerSubtextOverride(Player.Side playerSide)
	{
		return null;
	}

	public virtual string GetTurnTimerCountdownText(float timeRemainingInTurn)
	{
		return null;
	}

	public virtual string GetAttackSpellControllerOverride(Entity attacker)
	{
		return null;
	}

	public virtual ZonePlay.PlayZoneSizeOverride GetPlayZoneSizeOverride()
	{
		return null;
	}

	private void OnBlowUpSpellFinished(Spell spell, object userData)
	{
		EndGameScreenContext context = (EndGameScreenContext)userData;
		ShowEndGameScreenAfterEffects(context);
	}

	private void ShowEndGameScreenAfterEffects(EndGameScreenContext context)
	{
		if (AreBlowUpSpellsFinished(context))
		{
			Gameplay.Get().RestoreOriginalTimeScale();
			AchievementManager.Get()?.UnpauseToastNotifications();
			context.m_screen.Show();
		}
	}

	private bool AreBlowUpSpellsFinished(EndGameScreenContext context)
	{
		if (context.m_enemyBlowUpSpell != null && !context.m_enemyBlowUpSpell.IsFinished())
		{
			return false;
		}
		if (context.m_friendlyBlowUpSpell != null && !context.m_friendlyBlowUpSpell.IsFinished())
		{
			return false;
		}
		if (context.m_endOfGameSpell != null && !context.m_endOfGameSpell.IsFinished())
		{
			return false;
		}
		return true;
	}

	public virtual float? GetThinkEmoteDelayOverride()
	{
		return null;
	}

	public virtual Notification.SpeechBubbleDirection GetEmoteDirectionOverride(EmoteType emoteType)
	{
		return Notification.SpeechBubbleDirection.None;
	}

	public virtual string[] GetOverrideBoardClickSounds()
	{
		return null;
	}

	public virtual void OnTurnStartManagerFinished()
	{
	}

	public virtual void OnTurnTimerEnded(bool isFriendlyPlayerTurnTimer)
	{
	}

	public virtual bool GetAlternativeEndTurnButtonText(out string myTurnText, out string waitingText)
	{
		myTurnText = string.Empty;
		waitingText = string.Empty;
		return false;
	}

	public virtual bool ShouldOverwriteEndTurnButtonNoMorePlaysState(out bool hasNoMorePlay)
	{
		hasNoMorePlay = false;
		return false;
	}

	public virtual bool ShouldAutoCorrectZone(Zone zone)
	{
		return true;
	}

	public virtual bool OverwriteZoneDeckToAcceptEntity(ZoneDeck deckZone, int controllerId, TAG_ZONE zoneTag, TAG_CARDTYPE cardType, Entity entity)
	{
		return false;
	}

	public virtual bool OverwriteEndTurnReminder(Entity entity, out bool showReminder)
	{
		showReminder = false;
		return false;
	}

	private Spell ActivateEndOfGameSpell()
	{
		string endOfGameSpellPath = GetGameOptions().GetStringOption(GameEntityOption.END_OF_GAME_SPELL_PREFAB_PATH);
		if (string.IsNullOrEmpty(endOfGameSpellPath))
		{
			return null;
		}
		GameObject go = AssetLoader.Get().InstantiatePrefab(endOfGameSpellPath, AssetLoadingOptions.IgnorePrefabPosition);
		if (go == null)
		{
			return null;
		}
		m_endOfGameSpell = go.GetComponent<Spell>();
		if (m_endOfGameSpell == null)
		{
			return null;
		}
		m_endOfGameSpell.Activate();
		return m_endOfGameSpell;
	}

	public void ActivateEndOfGameSpellState(SpellStateType stateType)
	{
		if (m_endOfGameSpell != null)
		{
			m_endOfGameSpell.ActivateState(stateType);
		}
	}

	public virtual bool OverwriteCurrentPlayer(Player player, out bool isCurrentPlayer)
	{
		isCurrentPlayer = false;
		return false;
	}

	public virtual bool Overwrite_IsInZone_ForInputManager(Entity entity, TAG_ZONE zoneTag, TAG_ZONE finalZoneTag, out bool isInZone)
	{
		isInZone = false;
		return false;
	}

	public virtual void UpdateNameDisplay()
	{
	}
}
