using System.Collections;
using Blizzard.T5.AssetManager;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.DataModels;
using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

public class NameBanner : MonoBehaviour
{
	private const float SKINNED_BANNER_STANDARD_OFFSET = 1f;

	private const float SKINNED_BANNER_MEDAL_OFFSET = 10f;

	private const float SKINNED_BANNER_MIN_SIZE = 12f;

	private const float SKINNED_MEDAL_BANNER_MIN_SIZE = 17f;

	private const float SKINNED_GAME_ICON_BANNER_MIN_SIZE = 17f;

	public GameObject m_alphaBannerSkinned;

	public GameObject m_alphaBannerBone;

	public GameObject m_medalBannerSkinned;

	public GameObject m_medalBannerBone;

	public GameObject m_modeIconBannerSkinned;

	public GameObject m_modeIconBannerBone;

	public GameObject m_alphaBanner;

	public GameObject m_alphaBannerLeft;

	public GameObject m_alphaBannerMiddle;

	public GameObject m_alphaBannerRight;

	public GameObject m_medalAlphaBanner;

	public GameObject m_medalAlphaBannerLeft;

	public GameObject m_medalAlphaBannerMiddle;

	public GameObject m_medalAlphaBannerRight;

	public bool m_canShowModeIcons;

	public bool m_isGameplayNameBanner;

	public GameModeIcon m_casualStandardGameModeIcon;

	public GameModeIcon m_casualWildGameModeIcon;

	public GameModeIcon m_arenaGameModeIcon;

	public GameModeIcon m_adventureGameModeIcon;

	public GameObject m_adventureIcon;

	public GameObject m_adventureShadow;

	public GameModeIcon m_friendlyGameModeIcon;

	public TavernBrawlGameModeIcon m_tavernBrawlGameModeIcon;

	public GameModeIcon m_heroicSessionBasedTavernBrawlIcon;

	public TavernBrawlGameModeIcon m_normalSessionBasedTavernBrawlIcon;

	public GameModeIcon m_pvpdrGameModeIcon;

	public GameObject m_nameText;

	public GameObject m_longNameText;

	public Transform m_nameBone;

	public Transform m_classBone;

	public Transform m_longNameBone;

	public Transform m_longClassBone;

	public Transform m_medalNameBone;

	public Transform m_medalClassBone;

	public Transform m_longMedalNameBone;

	public Transform m_longMedalClassBone;

	public AsyncReference m_rankedMedalWidgetReference;

	public UberText m_playerName;

	public UberText m_subtext;

	public UberText m_longPlayerName;

	public UberText m_longSubtext;

	public float FUDGE_FACTOR = 0.1915f;

	private const float MARGIN_FACTOR = 0.1562f;

	private int m_playerId;

	private Player.Side m_playerSide;

	private const float UNKNOWN_NAME_WAIT = 5f;

	private const float RANK_WAIT = 5f;

	private Transform m_nameBoneToUse;

	private Transform m_classBoneToUse;

	private UberText m_currentPlayerName;

	private UberText m_currentSubtext;

	private int m_missionId;

	private bool m_useLongName;

	private bool m_shouldCenterName = true;

	private FormatType m_formatType;

	private bool m_shouldShowRankedMedal;

	private bool m_initialized;

	private MedalInfoTranslator m_medalInfo;

	private RankedMedal m_rankedMedal;

	private RankedPlayDataModel m_rankedDataModel;

	private Widget m_medalWidget;

	private AssetHandle<Texture> m_gameModeIconTexture;

	public bool IsWaitingForMedal
	{
		get
		{
			if (m_shouldShowRankedMedal)
			{
				if (m_medalInfo != null && !(m_medalWidget == null))
				{
					return m_medalWidget.IsChangingStates;
				}
				return true;
			}
			return false;
		}
	}

	private void Update()
	{
		UpdateAnchor();
	}

	private void OnDestroy()
	{
		AssetHandle.SafeDispose(ref m_gameModeIconTexture);
	}

	public void SetName(string name)
	{
		m_currentPlayerName.Text = name;
		if (m_alphaBannerSkinned != null)
		{
			AdjustSkinnedBanner();
		}
		else
		{
			AdjustBanner();
		}
	}

	private void SetMobilePositionOffset()
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			float localPositionXOffset = BaseUI.Get().m_BnetBar.HorizontalMargin * 0.1562f;
			TransformUtil.SetLocalPosX(base.gameObject, base.gameObject.transform.localPosition.x + localPositionXOffset);
		}
	}

	private void AdjustBanner()
	{
		Vector3 worldScale = TransformUtil.ComputeWorldScale(m_currentPlayerName.gameObject);
		float fudgeFactor = FUDGE_FACTOR * worldScale.x * m_currentPlayerName.GetTextWorldSpaceBounds().size.x;
		float nameWidth = m_currentPlayerName.GetTextWorldSpaceBounds().size.x * worldScale.x + fudgeFactor;
		float middleWidth = m_alphaBannerMiddle.GetComponent<Renderer>().bounds.size.x;
		float medalNameWidth = m_currentPlayerName.GetTextBounds().size.x;
		if (m_medalAlphaBannerMiddle != null)
		{
			MeshRenderer medalMiddleRenderer = m_medalAlphaBannerMiddle.GetComponentInChildren<MeshRenderer>(includeInactive: true);
			float medalMiddleWidth = medalMiddleRenderer.bounds.size.x;
			if (nameWidth > middleWidth)
			{
				if (m_shouldShowRankedMedal)
				{
					TransformUtil.SetLocalScaleX(m_medalAlphaBannerMiddle, medalNameWidth / medalMiddleWidth);
					TransformUtil.SetPoint(m_medalAlphaBannerRight, Anchor.LEFT, medalMiddleRenderer.gameObject, Anchor.RIGHT, new Vector3(0f, 0f, 0f));
				}
				else
				{
					TransformUtil.SetLocalScaleX(m_alphaBannerMiddle, nameWidth / middleWidth);
					TransformUtil.SetPoint(m_alphaBannerRight, Anchor.LEFT, m_alphaBannerMiddle, Anchor.RIGHT, new Vector3(0f - fudgeFactor, 0f, 0f));
				}
			}
		}
		else if (nameWidth > middleWidth)
		{
			TransformUtil.SetLocalScaleX(m_alphaBanner, nameWidth / middleWidth);
		}
	}

	private void AdjustSkinnedBanner()
	{
		bool shouldShowGameIconBanner = false;
		if (!m_shouldShowRankedMedal && ShouldShowGameIconBanner())
		{
			shouldShowGameIconBanner = true;
		}
		if (m_currentPlayerName == null)
		{
			return;
		}
		UberText largestCurrentText = m_currentPlayerName;
		if (m_shouldShowRankedMedal)
		{
			float nameEdgeX = 0f - largestCurrentText.GetTextBounds().size.x - 10f;
			if (nameEdgeX > -17f)
			{
				nameEdgeX = -17f;
			}
			if (m_medalBannerBone != null)
			{
				Vector3 bonePos = m_medalBannerBone.transform.localPosition;
				m_medalBannerBone.transform.localPosition = new Vector3(nameEdgeX, bonePos.y, bonePos.z);
			}
		}
		else if (shouldShowGameIconBanner)
		{
			float nameEdgeX = 0f - largestCurrentText.GetTextBounds().size.x - 10f;
			if (nameEdgeX > -17f)
			{
				nameEdgeX = -17f;
			}
			if (m_modeIconBannerBone != null)
			{
				Vector3 bonePos2 = m_modeIconBannerBone.transform.localPosition;
				m_modeIconBannerBone.transform.localPosition = new Vector3(nameEdgeX, bonePos2.y, bonePos2.z);
			}
		}
		else
		{
			float nameEdgeX = 0f - largestCurrentText.GetTextBounds().size.x - 1f;
			if (nameEdgeX > -12f)
			{
				nameEdgeX = -12f;
			}
			if (m_alphaBannerBone != null)
			{
				Vector3 bonePos3 = m_alphaBannerBone.transform.localPosition;
				m_alphaBannerBone.transform.localPosition = new Vector3(nameEdgeX, bonePos3.y, bonePos3.z);
			}
		}
	}

	public void SetSubtext(string subtext)
	{
		if (m_currentSubtext != null)
		{
			m_currentSubtext.gameObject.SetActive(value: true);
			m_currentSubtext.Text = subtext;
		}
		if (m_currentPlayerName != null)
		{
			Vector3 position = ((m_classBoneToUse == null) ? m_nameBoneToUse.localPosition : m_classBoneToUse.localPosition);
			m_currentPlayerName.transform.localPosition = position;
		}
	}

	public void PositionNameText(bool shouldTween)
	{
		if (m_shouldCenterName && !(m_currentPlayerName == null))
		{
			if ((bool)UniversalInputManager.UsePhoneUI || !shouldTween)
			{
				m_currentPlayerName.transform.position = m_nameBoneToUse.position;
				return;
			}
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("position", m_nameBoneToUse.localPosition);
			args.Add("islocal", true);
			args.Add("time", 1f);
			iTween.MoveTo(m_currentPlayerName.gameObject, args);
		}
	}

	public void PositionNameText_Reconnect()
	{
		if (!(m_currentPlayerName == null))
		{
			m_currentPlayerName.transform.position = m_nameBoneToUse.position;
			OnSubtextFadeComplete();
		}
	}

	public void FadeOutSubtext()
	{
		if (m_currentSubtext == null)
		{
			return;
		}
		bool isAdventureBoss = GameUtils.IsExpansionAdventure(GameUtils.GetAdventureId(m_missionId)) && !GameUtils.IsClassChallengeMission(m_missionId);
		if (m_playerSide == Player.Side.OPPOSING && isAdventureBoss)
		{
			m_shouldCenterName = false;
		}
		else if (m_playerSide == Player.Side.FRIENDLY && !string.IsNullOrEmpty(GameState.Get().GetGameEntity().GetAlternatePlayerName()))
		{
			if (m_adventureGameModeIcon != null)
			{
				m_adventureGameModeIcon.Show(show: false);
			}
			iTween.FadeTo(base.gameObject, 0f, 1f);
		}
		else
		{
			Hashtable fadeArgs = iTweenManager.Get().GetTweenHashTable();
			fadeArgs.Add("alpha", 0f);
			fadeArgs.Add("time", 1f);
			fadeArgs.Add("oncomplete", "OnSubtextFadeComplete");
			fadeArgs.Add("oncompletetarget", base.gameObject);
			iTween.FadeTo(m_currentSubtext.gameObject, fadeArgs);
		}
	}

	public void OnSubtextFadeComplete()
	{
		m_currentSubtext.gameObject.SetActive(value: false);
	}

	public void FadeIn()
	{
		if (m_alphaBannerSkinned != null)
		{
			iTween.FadeTo(m_alphaBannerSkinned.gameObject, 1f, 1f);
		}
		else
		{
			iTween.FadeTo(m_alphaBanner.gameObject, 1f, 1f);
		}
		iTween.FadeTo(m_currentPlayerName.gameObject, 1f, 1f);
	}

	public void Initialize(Player.Side side)
	{
		m_playerSide = side;
		m_currentPlayerName = m_playerName;
		m_currentSubtext = m_subtext;
		m_nameText.SetActive(value: true);
		m_useLongName = false;
		m_playerName.Text = string.Empty;
		m_nameBoneToUse = m_nameBone;
		if ((bool)m_longNameText)
		{
			m_longNameText.SetActive(value: false);
		}
		m_missionId = GameMgr.Get().GetMissionId();
		m_formatType = GameMgr.Get().GetFormatType();
		m_shouldShowRankedMedal = GameUtils.IsGameTypeRanked();
		m_initialized = true;
		UpdateAnchor();
		if (!GameState.Get().GetBooleanGameOption(GameEntityOption.ALLOW_NAME_BANNER_MODE_ICONS))
		{
			m_canShowModeIcons = false;
		}
		if (!m_canShowModeIcons)
		{
			m_shouldShowRankedMedal = false;
		}
		if (m_shouldShowRankedMedal)
		{
			StartCoroutine(UpdateMedalWhenReady());
			m_rankedMedalWidgetReference.RegisterReadyListener<Widget>(OnRankedMedalWidgetReady);
		}
	}

	public void Show()
	{
		if (m_playerSide == Player.Side.OPPOSING && GameState.Get().GetBooleanGameOption(GameEntityOption.DISABLE_OPPONENT_NAME_BANNER))
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			StartCoroutine(UpdateName());
		}
	}

	public Player.Side GetPlayerSide()
	{
		return m_playerSide;
	}

	public void Unload()
	{
		Object.DestroyImmediate(base.gameObject);
	}

	public void UseLongName()
	{
		m_currentPlayerName = m_longPlayerName;
		m_currentSubtext = m_longSubtext;
		m_longNameText.SetActive(value: true);
		m_nameText.SetActive(value: false);
		m_useLongName = true;
	}

	public void UpdateHeroNameBanner()
	{
		Player wantedPlayer = GameState.Get().GetPlayer(m_playerId);
		SetName(wantedPlayer.GetHero().GetName());
	}

	public void UpdatePlayerNameBanner()
	{
		Player wantedPlayer = GameState.Get().GetPlayer(m_playerId);
		SetName(wantedPlayer.GetName());
	}

	public void UpdateSubtext()
	{
		StartCoroutine(UpdateSubtextImpl());
	}

	private void UpdateAnchor()
	{
		if (!m_initialized || OverlayUI.Get() == null)
		{
			return;
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			if (m_playerSide == Player.Side.FRIENDLY)
			{
				OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.BOTTOM_RIGHT);
			}
			else
			{
				OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.BOTTOM_LEFT);
			}
			return;
		}
		if (m_playerSide == Player.Side.FRIENDLY)
		{
			OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.BOTTOM_LEFT);
		}
		else
		{
			OverlayUI.Get().AddGameObject(base.gameObject, CanvasAnchor.TOP_LEFT);
		}
		if (GameState.Get() != null && GameState.Get().GetGameEntity() != null)
		{
			base.transform.localPosition = GameState.Get().GetGameEntity().NameBannerPosition(m_playerSide);
		}
	}

	private static Player GetPlayerForSide(Player.Side side)
	{
		return side switch
		{
			Player.Side.FRIENDLY => GameState.Get().GetLocalSidePlayer(), 
			Player.Side.OPPOSING => GameState.Get().GetOpposingPlayer(), 
			_ => null, 
		};
	}

	private IEnumerator UpdateName()
	{
		while (GameState.Get().GetPlayerMap().Count == 0)
		{
			yield return null;
		}
		Player p = null;
		while (p == null)
		{
			p = GetPlayerForSide(m_playerSide);
			yield return null;
		}
		m_playerId = p.GetPlayerId();
		string nameToDisplay = p.GetName();
		if (p.IsHuman() && Options.Get().GetBool(Option.STREAMER_MODE) && !SpectatorManager.Get().IsInSpectatorMode())
		{
			nameToDisplay = ((!p.IsLocalUser()) ? GameStrings.Get("GAMEPLAY_MISSING_OPPONENT_NAME") : GameStrings.Get("GAMEPLAY_HIDDEN_PLAYER_NAME"));
		}
		if (p.IsLocalUser())
		{
			string altName = GameState.Get().GetGameEntity().GetAlternatePlayerName();
			if (!string.IsNullOrEmpty(altName))
			{
				nameToDisplay = altName;
			}
		}
		string overrideName = GameState.Get().GetGameEntity().GetNameBannerOverride(m_playerSide);
		if (!string.IsNullOrEmpty(overrideName))
		{
			nameToDisplay = overrideName;
		}
		float timeStart = Time.time;
		while (string.IsNullOrEmpty(nameToDisplay))
		{
			if (Time.time - timeStart >= 5f)
			{
				if (GameMgr.Get().GetReconnectType() == GameReconnectType.GAMEPLAY)
				{
					string lastDisplayedName = GameMgr.Get().GetLastDisplayedPlayerName(m_playerId);
					if (!string.IsNullOrEmpty(lastDisplayedName))
					{
						nameToDisplay = lastDisplayedName;
					}
				}
				if (string.IsNullOrEmpty(nameToDisplay))
				{
					nameToDisplay = ((!p.IsLocalUser()) ? GameStrings.Get("GAMEPLAY_MISSING_OPPONENT_NAME") : GameStrings.Get("GAMEPLAY_HIDDEN_PLAYER_NAME"));
				}
				break;
			}
			yield return null;
			nameToDisplay = p.GetName();
		}
		bool shouldShowGameIconBanner = false;
		if (ShouldShowGameIconBanner())
		{
			shouldShowGameIconBanner = true;
		}
		if (!m_canShowModeIcons)
		{
			shouldShowGameIconBanner = false;
		}
		if (m_shouldShowRankedMedal)
		{
			m_nameBoneToUse = (m_useLongName ? m_longMedalNameBone : m_medalNameBone);
			m_classBoneToUse = (m_useLongName ? m_longMedalClassBone : m_medalClassBone);
			if (m_medalBannerSkinned == null)
			{
				if (m_medalAlphaBanner != null)
				{
					m_medalAlphaBanner.SetActive(value: true);
				}
			}
			else
			{
				m_medalBannerSkinned.SetActive(value: true);
				m_alphaBannerSkinned.SetActive(value: false);
				if (m_medalAlphaBanner != null)
				{
					m_medalAlphaBanner.SetActive(value: false);
				}
			}
			if (m_alphaBanner != null)
			{
				m_alphaBanner.SetActive(value: false);
			}
		}
		else if (shouldShowGameIconBanner)
		{
			m_nameBoneToUse = (m_useLongName ? m_longMedalNameBone : m_medalNameBone);
			m_classBoneToUse = (m_useLongName ? m_longMedalClassBone : m_medalClassBone);
			if (m_modeIconBannerSkinned == null)
			{
				if (m_medalAlphaBanner != null)
				{
					m_medalAlphaBanner.SetActive(value: true);
				}
			}
			else
			{
				m_modeIconBannerSkinned.SetActive(value: true);
				m_alphaBannerSkinned.SetActive(value: false);
				if (m_medalAlphaBanner != null)
				{
					m_medalAlphaBanner.SetActive(value: false);
				}
			}
			if (m_alphaBanner != null)
			{
				m_alphaBanner.SetActive(value: false);
			}
		}
		else
		{
			m_nameBoneToUse = (m_useLongName ? m_longNameBone : m_nameBone);
			m_classBoneToUse = (m_useLongName ? m_longClassBone : m_classBone);
			if (m_alphaBannerSkinned == null)
			{
				if (m_alphaBanner != null)
				{
					m_alphaBanner.SetActive(value: true);
				}
			}
			else
			{
				m_alphaBannerSkinned.SetActive(value: true);
				if (m_alphaBanner != null)
				{
					m_alphaBanner.SetActive(value: false);
				}
				m_medalBannerSkinned.SetActive(value: false);
			}
			if (m_medalAlphaBanner != null)
			{
				m_medalAlphaBanner.SetActive(value: false);
			}
			if (m_medalBannerSkinned != null)
			{
				m_medalBannerSkinned.SetActive(value: false);
			}
		}
		SetName(nameToDisplay);
		if (GameMgr.Get().IsTraditionalTutorial() || m_isGameplayNameBanner)
		{
			SetMobilePositionOffset();
			m_shouldCenterName = true;
			PositionNameText(shouldTween: false);
			yield break;
		}
		AdventureDbId adventureId = GameUtils.GetAdventureId(m_missionId);
		if (m_shouldShowRankedMedal)
		{
			if (m_medalWidget != null)
			{
				m_medalWidget.Show();
			}
		}
		else if (m_playerSide == Player.Side.FRIENDLY && !UniversalInputManager.UsePhoneUI)
		{
			if (GameUtils.ShouldShowAdventureModeIcon())
			{
				AdventureDbfRecord adventureRecord = GameDbf.Adventure.GetRecord((int)adventureId);
				if (m_adventureGameModeIcon != null)
				{
					AssetLoader.Get().LoadAsset(ref m_gameModeIconTexture, adventureRecord.GameModeIcon);
					m_adventureIcon.GetComponent<MeshRenderer>().GetMaterial().SetTexture("_MainTex", m_gameModeIconTexture);
					m_adventureGameModeIcon.Show(show: true);
				}
			}
			else if (GameUtils.ShouldShowCasualModeIcon())
			{
				if (m_formatType == FormatType.FT_STANDARD)
				{
					m_casualStandardGameModeIcon.Show(show: true);
				}
				else
				{
					m_casualWildGameModeIcon.Show(show: true);
				}
			}
			else if (GameUtils.ShouldShowArenaModeIcon())
			{
				m_arenaGameModeIcon.Show(show: true);
				uint wins = p.GetArenaWins();
				uint losses = p.GetArenaLosses();
				if (p.GetGameAccountId() == BnetPresenceMgr.Get().GetMyGameAccountId())
				{
					timeStart = Time.time;
					while (!DraftManager.Get().CanShowWinsLosses)
					{
						yield return null;
						if (Time.time - timeStart >= 5f)
						{
							break;
						}
					}
					wins = (uint)DraftManager.Get().GetWins();
					losses = (uint)DraftManager.Get().GetLosses();
				}
				m_arenaGameModeIcon.SetText(wins.ToString());
				m_arenaGameModeIcon.ShowXMarks(losses);
			}
			else if (GameUtils.ShouldShowFriendlyChallengeIcon())
			{
				TavernBrawlMission mission = TavernBrawlManager.Get().CurrentMission();
				if (mission != null && mission.missionId == GameMgr.Get().GetMissionId())
				{
					m_tavernBrawlGameModeIcon.Show(show: true);
					m_tavernBrawlGameModeIcon.ShowFriendlyChallengeBanner(showBanner: true);
				}
				else
				{
					m_friendlyGameModeIcon.Show(show: true);
					m_friendlyGameModeIcon.ShowWildVines(m_formatType == FormatType.FT_WILD);
				}
			}
			else if (GameUtils.ShouldShowTavernBrawlModeIcon())
			{
				if (TavernBrawlManager.Get().IsCurrentSeasonSessionBased)
				{
					uint wins2 = p.GetTavernBrawlWins();
					uint losses2 = p.GetTavernBrawlLosses();
					if (p.GetGameAccountId() == BnetPresenceMgr.Get().GetMyGameAccountId())
					{
						wins2 = (uint)TavernBrawlManager.Get().GamesWon;
						losses2 = (uint)TavernBrawlManager.Get().GamesLost;
					}
					GameModeIcon obj = ((TavernBrawlManager.Get().CurrentSeasonBrawlMode == TavernBrawlMode.TB_MODE_HEROIC) ? m_heroicSessionBasedTavernBrawlIcon : m_normalSessionBasedTavernBrawlIcon);
					obj.Show(show: true);
					obj.SetText(wins2.ToString());
					obj.ShowXMarks(losses2);
				}
				else
				{
					m_tavernBrawlGameModeIcon.Show(show: true);
					m_tavernBrawlGameModeIcon.ShowFriendlyChallengeBanner(showBanner: false);
				}
			}
			else if (GameUtils.ShouldShowPvpDrModeIcon())
			{
				m_pvpdrGameModeIcon.Show(show: true);
				uint wins3 = p.GetDuelsWins();
				uint losses3 = p.GetDuelsLosses();
				m_pvpdrGameModeIcon.SetText(wins3.ToString());
				m_pvpdrGameModeIcon.ShowXMarks(losses3);
			}
		}
		yield return UpdateSubtextImpl();
		if (GameState.Get().GetGameEntity().ShouldDoAlternateMulliganIntro())
		{
			FadeOutSubtext();
			PositionNameText(shouldTween: false);
		}
	}

	private IEnumerator UpdateSubtextImpl()
	{
		AdventureModeDbId adventureMode = GameUtils.GetAdventureModeId(m_missionId);
		AdventureDbId adventureId = GameUtils.GetAdventureId(m_missionId);
		bool isAdventureBoss = GameUtils.IsExpansionAdventure(adventureId) && adventureMode != AdventureModeDbId.CLASS_CHALLENGE;
		GameEntity gameEntity = GameState.Get().GetGameEntity();
		Player player = GameState.Get().GetPlayer(m_playerId);
		if (gameEntity != null && gameEntity.GetNameBannerSubtextOverride(m_playerSide) != null)
		{
			SetSubtext(gameEntity.GetNameBannerSubtextOverride(m_playerSide));
		}
		else if (m_playerSide == Player.Side.OPPOSING && isAdventureBoss)
		{
			AdventureDataDbfRecord adventureData = GameUtils.GetAdventureDataRecord((int)adventureId, (int)adventureMode);
			if (adventureData != null)
			{
				SetSubtext(((string)adventureData.ShortName).ToUpper());
			}
		}
		else
		{
			Entity hero = player.GetHero();
			bool isSetSubtextCalled = false;
			if (hero != null)
			{
				while (hero.GetClass() == TAG_CLASS.INVALID)
				{
					yield return null;
				}
				if (hero.GetClass() != TAG_CLASS.NEUTRAL)
				{
					isSetSubtextCalled = true;
					SetSubtext(GameStrings.GetClassName(player.GetHero().GetClass()).ToUpper());
				}
			}
			if (!isSetSubtextCalled)
			{
				m_currentPlayerName.transform.position = m_nameBoneToUse.position;
			}
		}
		if (GameState.Get().GetGameEntity().ShouldDoAlternateMulliganIntro())
		{
			FadeOutSubtext();
			PositionNameText(shouldTween: false);
		}
		if (GameMgr.Get().IsReconnect() && GameState.Get().IsMainPhase())
		{
			PositionNameText_Reconnect();
		}
	}

	public void UpdateMedalChange(MedalInfoTranslator medalInfo)
	{
		medalInfo.CreateOrUpdateDataModel(m_formatType, ref m_rankedDataModel, RankedMedal.DisplayMode.Default);
		if (!(m_rankedMedal == null))
		{
			if (!m_shouldShowRankedMedal || medalInfo == null || !medalInfo.IsDisplayable())
			{
				m_rankedMedal.gameObject.SetActive(value: false);
				return;
			}
			m_rankedMedal.gameObject.SetActive(value: true);
			m_rankedMedal.BindRankedPlayDataModel(m_rankedDataModel);
			m_medalWidget.Show();
		}
	}

	public void UpdatePvpDRInfo(PVPDRLobbyDataModel dataModel)
	{
		m_pvpdrGameModeIcon.SetText(dataModel.Wins.ToString());
		m_pvpdrGameModeIcon.ShowXMarks((uint)dataModel.Losses);
	}

	private bool ShouldShowGameIconBanner()
	{
		if (m_playerSide == Player.Side.FRIENDLY && !UniversalInputManager.UsePhoneUI && !GameUtils.IsPracticeMission(m_missionId))
		{
			return !GameUtils.IsTutorialMission(m_missionId);
		}
		return false;
	}

	private void OnRankedMedalWidgetReady(Widget widget)
	{
		if (!(widget == null))
		{
			widget.Hide();
			m_medalWidget = widget;
			m_rankedMedal = widget.GetComponentInChildren<RankedMedal>();
		}
	}

	private IEnumerator UpdateMedalWhenReady()
	{
		if (m_medalInfo != null)
		{
			yield break;
		}
		Player player = GetPlayerForSide(m_playerSide);
		float timeStart = Time.time;
		while (player.GetRank() == null || m_rankedMedal == null)
		{
			yield return null;
			if (Time.time - timeStart >= 5f)
			{
				break;
			}
		}
		m_medalInfo = player.GetRank();
		if (m_medalInfo == null || !m_medalInfo.IsDisplayable())
		{
			m_shouldShowRankedMedal = false;
		}
		if (m_shouldShowRankedMedal && m_playerSide == Player.Side.OPPOSING)
		{
			Player friendlyPlayer = GetPlayerForSide(Player.Side.FRIENDLY);
			MedalInfoTranslator friendlyPlayerMedalInfo = null;
			if (friendlyPlayer != null)
			{
				friendlyPlayerMedalInfo = friendlyPlayer.GetRank();
			}
			if (friendlyPlayer == null || friendlyPlayerMedalInfo == null || !friendlyPlayerMedalInfo.GetCurrentMedal(m_formatType).RankConfig.ShowOpponentRankInGame)
			{
				m_shouldShowRankedMedal = false;
			}
		}
		if (m_shouldShowRankedMedal)
		{
			UpdateMedalChange(m_medalInfo);
		}
	}
}
