using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RLK_Prologue_Fight_002 : RLK_Prologue_Dungeon
{
	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Arthas_InGame_MinionDies_02_A = new AssetReference("VO_RLK_Prologue_Male_Human_Arthas_InGame_MinionDies_02_A.prefab:f459120a7b592ae44a3e5342c00596f0");

	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Arthas_InGame_PlayerPlaysCard_02_A = new AssetReference("VO_RLK_Prologue_Male_Human_Arthas_InGame_PlayerPlaysCard_02_A.prefab:5556f9ef8e44acc4aa354f6f05e4f330");

	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Arthas_InGame_PlayerPlaysCard_02_C = new AssetReference("VO_RLK_Prologue_Male_Human_Arthas_InGame_PlayerPlaysCard_02_C.prefab:30b9ca815425fd746a044aa9121396dc");

	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Arthas_InGame_Turn_03_02_B = new AssetReference("VO_RLK_Prologue_Male_Human_Arthas_InGame_Turn_03_02_B.prefab:d7026c60516dd014c97317e519ab9d53");

	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Arthas_InGame_Turn_09_02_B = new AssetReference("VO_RLK_Prologue_Male_Human_Arthas_InGame_Turn_09_02_B.prefab:d563cc61e01f1904c94a6b771e39802c");

	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Arthas_InGame_Turn_17_02_B = new AssetReference("VO_RLK_Prologue_Male_Human_Arthas_InGame_Turn_17_02_B.prefab:6b0d8cc0cfee3c747ad5e35b42a7f5d6");

	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Arthas_InGame_VictoryPreExplosion_02_B = new AssetReference("VO_RLK_Prologue_Male_Human_Arthas_InGame_VictoryPreExplosion_02_B.prefab:eec2da0f199898f44b4baf5328a47e97");

	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Uther_InGame_BossIdle_02_A = new AssetReference("VO_RLK_Prologue_Male_Human_Uther_InGame_BossIdle_02_A.prefab:88b7986d4ce97904792fb49a38a29038");

	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Uther_InGame_BossIdle_02_B = new AssetReference("VO_RLK_Prologue_Male_Human_Uther_InGame_BossIdle_02_B.prefab:2200d0bd065e939499112ef4905c1855");

	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Uther_InGame_BossIdle_02_C = new AssetReference("VO_RLK_Prologue_Male_Human_Uther_InGame_BossIdle_02_C.prefab:6f12e912a75174e4cbcdc698d0a6c0e7");

	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Uther_InGame_EmoteResponse_02_A = new AssetReference("VO_RLK_Prologue_Male_Human_Uther_InGame_EmoteResponse_02_A.prefab:f8bce042a8aa21d49b145e41abc4b225");

	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Uther_InGame_Introduction_02_A = new AssetReference("VO_RLK_Prologue_Male_Human_Uther_InGame_Introduction_02_A.prefab:9b690cc86354cb14799eb1ca9b6fc15c");

	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Uther_InGame_LossPostExplosion_02_A = new AssetReference("VO_RLK_Prologue_Male_Human_Uther_InGame_LossPostExplosion_02_A.prefab:03f6e340032402c4284f1b10452c9703");

	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Uther_InGame_PlayerPlaysCard_02_B = new AssetReference("VO_RLK_Prologue_Male_Human_Uther_InGame_PlayerPlaysCard_02_B.prefab:37e16093e92adbb42a2b669fd9a5ab7e");

	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Uther_InGame_PlayerPlaysCard_02_C = new AssetReference("VO_RLK_Prologue_Male_Human_Uther_InGame_PlayerPlaysCard_02_C.prefab:4acb69a96b646994f91bab872e108844");

	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Uther_InGame_PlayerPlaysCard_02_D = new AssetReference("VO_RLK_Prologue_Male_Human_Uther_InGame_PlayerPlaysCard_02_D.prefab:eb2fffa18c634e44ab9b3dbe53bd7118");

	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Uther_InGame_Turn_03_02_A = new AssetReference("VO_RLK_Prologue_Male_Human_Uther_InGame_Turn_03_02_A.prefab:1cb0764cda4524a4bb3e54fdeeeea420");

	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Uther_InGame_Turn_09_02_A = new AssetReference("VO_RLK_Prologue_Male_Human_Uther_InGame_Turn_09_02_A.prefab:c64156a39fd91cb4a9c25cee034080ab");

	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Uther_InGame_Turn_17_02_A = new AssetReference("VO_RLK_Prologue_Male_Human_Uther_InGame_Turn_17_02_A.prefab:da9db0582f511b94785d0f6a7a0a4e5e");

	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Uther_InGame_VictoryPostExplosion_02_C = new AssetReference("VO_RLK_Prologue_Male_Human_Uther_InGame_VictoryPostExplosion_02_C.prefab:6a4eb15653c8e26498da73a6c68d38a3");

	private static readonly AssetReference VO_RLK_Prologue_Male_Human_Uther_InGame_VictoryPreExplosion_02_A = new AssetReference("VO_RLK_Prologue_Male_Human_Uther_InGame_VictoryPreExplosion_02_A.prefab:7b2fc20b094baf64fbdc0c27d0dfc78f");

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_RLK_Prologue_Male_Human_Uther_InGame_BossIdle_02_A, VO_RLK_Prologue_Male_Human_Uther_InGame_BossIdle_02_B, VO_RLK_Prologue_Male_Human_Uther_InGame_BossIdle_02_C };

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]> { 
	{
		1,
		new string[1] { "VO_RLK_Prologue_Male_Human_Arthas_InGame_PlayerPlaysCard_02_C" }
	} };

	private float popUpScale = 1f;

	private Vector3 popUpPos;

	private Notification handBounceArrow;

	private bool bhasMinionDeathBeenTriggered;

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_RLK_Prologue_Male_Human_Arthas_InGame_MinionDies_02_A, VO_RLK_Prologue_Male_Human_Arthas_InGame_PlayerPlaysCard_02_A, VO_RLK_Prologue_Male_Human_Arthas_InGame_PlayerPlaysCard_02_C, VO_RLK_Prologue_Male_Human_Arthas_InGame_Turn_03_02_B, VO_RLK_Prologue_Male_Human_Arthas_InGame_Turn_09_02_B, VO_RLK_Prologue_Male_Human_Arthas_InGame_Turn_17_02_B, VO_RLK_Prologue_Male_Human_Arthas_InGame_VictoryPreExplosion_02_B, VO_RLK_Prologue_Male_Human_Uther_InGame_BossIdle_02_A, VO_RLK_Prologue_Male_Human_Uther_InGame_BossIdle_02_B, VO_RLK_Prologue_Male_Human_Uther_InGame_BossIdle_02_C,
			VO_RLK_Prologue_Male_Human_Uther_InGame_EmoteResponse_02_A, VO_RLK_Prologue_Male_Human_Uther_InGame_Introduction_02_A, VO_RLK_Prologue_Male_Human_Uther_InGame_LossPostExplosion_02_A, VO_RLK_Prologue_Male_Human_Uther_InGame_PlayerPlaysCard_02_B, VO_RLK_Prologue_Male_Human_Uther_InGame_PlayerPlaysCard_02_C, VO_RLK_Prologue_Male_Human_Uther_InGame_PlayerPlaysCard_02_D, VO_RLK_Prologue_Male_Human_Uther_InGame_Turn_03_02_A, VO_RLK_Prologue_Male_Human_Uther_InGame_Turn_09_02_A, VO_RLK_Prologue_Male_Human_Uther_InGame_Turn_17_02_A, VO_RLK_Prologue_Male_Human_Uther_InGame_VictoryPostExplosion_02_C,
			VO_RLK_Prologue_Male_Human_Uther_InGame_VictoryPreExplosion_02_A
		};
		SetBossVOLines(m_SoundFiles);
		foreach (string soundFile in m_SoundFiles)
		{
			PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
	}

	public override void NotifyOfMinionDied(Entity entity)
	{
		Gameplay.Get().StartCoroutine(NotifyOfMinionDiedWithTiming(entity));
	}

	protected Actor GetEnemyActorByCardId(string cardId)
	{
		Player player = GameState.Get().GetOpposingSidePlayer();
		foreach (Card card in player.GetBattlefieldZone().GetCards())
		{
			Entity cardEntity = card.GetEntity();
			if (cardEntity.GetControllerId() == player.GetPlayerId() && cardEntity.GetCardId() == cardId)
			{
				return cardEntity.GetCard().GetActor();
			}
		}
		return null;
	}

	private IEnumerator ShowArrowInSeconds(float seconds, bool bShowInHandZone)
	{
		yield return new WaitForSeconds(seconds);
		List<Card> handCards = GameState.Get().GetFriendlySidePlayer().GetHandZone()
			.GetCards();
		if (handCards.Count != 0)
		{
			Card cardInHand = handCards[0];
			while (iTween.Count(cardInHand.gameObject) > 0)
			{
				yield return null;
			}
			if (!cardInHand.IsMousedOver() && !(InputManager.Get().GetHeldCard() == cardInHand))
			{
				ShowHandBouncingArrow(bShowInHandZone);
			}
		}
	}

	private void ShowHandBouncingArrow(bool bShowInHandZone)
	{
		if (handBounceArrow != null)
		{
			return;
		}
		List<Card> cardsInHand = GameState.Get().GetFriendlySidePlayer().GetHandZone()
			.GetCards();
		if (cardsInHand.Count == 0)
		{
			return;
		}
		Card cardInHand = null;
		if (bShowInHandZone)
		{
			foreach (Card card in cardsInHand)
			{
				if (card == null)
				{
					continue;
				}
				Entity ent = card.GetEntity();
				if (ent != null)
				{
					int bloodCost = ent.GetTag(GAME_TAG.COST_BLOOD);
					int unholyCost = ent.GetTag(GAME_TAG.COST_UNHOLY);
					int frostCost = ent.GetTag(GAME_TAG.COST_FROST);
					if (bloodCost + unholyCost + frostCost > 0)
					{
						cardInHand = card;
						break;
					}
				}
			}
			if (cardInHand == null)
			{
				cardInHand = cardsInHand[0];
			}
		}
		else
		{
			cardInHand = cardsInHand[0];
		}
		Vector3 cardInHandPosition = cardInHand.transform.position;
		Vector3 bounceArrowPos = (bShowInHandZone ? ((!UniversalInputManager.UsePhoneUI) ? new Vector3(cardInHandPosition.x + -0.7f, cardInHandPosition.y + 0.2f, cardInHandPosition.z + 0.95f) : new Vector3(cardInHandPosition.x + -0.75f, cardInHandPosition.y + 0.2f, cardInHandPosition.z + 0.95f)) : ((!UniversalInputManager.UsePhoneUI) ? new Vector3(cardInHandPosition.x + 4.9f, cardInHandPosition.y + 1f, cardInHandPosition.z + 0.89f) : new Vector3(cardInHandPosition.x + 3.47f, cardInHandPosition.y + 0.2f, cardInHandPosition.z + 2.23f)));
		handBounceArrow = NotificationManager.Get().CreateBouncingArrow(UserAttentionBlocker.NONE, bounceArrowPos, new Vector3(0f, 0f, 0f));
		handBounceArrow.transform.parent = cardInHand.transform;
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (missionEvent)
		{
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_RLK_Prologue_Male_Human_Uther_InGame_Introduction_02_A);
			break;
		case 506:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_RLK_Prologue_Male_Human_Uther_InGame_LossPostExplosion_02_A);
			GameState.Get().SetBusy(busy: false);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_RLK_Prologue_Male_Human_Uther_InGame_EmoteResponse_02_A);
			break;
		default:
			yield return base.HandleMissionEventWithTiming(missionEvent);
			break;
		}
	}

	protected override IEnumerator RespondToFriendlyPlayedCardWithTiming(Entity entity)
	{
		yield return base.RespondToFriendlyPlayedCardWithTiming(entity);
		while (m_enemySpeaking)
		{
			yield return null;
		}
		while (entity.GetCardType() == TAG_CARDTYPE.INVALID)
		{
			yield return null;
		}
		if (!m_playedLines.Contains(entity.GetCardId()) || entity.GetCardType() == TAG_CARDTYPE.HERO_POWER)
		{
			yield return WaitForEntitySoundToFinish(entity);
			string cardID = entity.GetCardId();
			m_playedLines.Add(cardID);
			GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCard()
				.GetActor();
			GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
			switch (cardID)
			{
			case "RLK_Prologue_RLK_730":
				yield break;
			case "RLK_Prologue_RLK_012":
				yield break;
			}
			_ = cardID == "RLK_Prologue_RLK_071";
		}
	}

	protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
	{
		if (handBounceArrow != null)
		{
			NotificationManager.Get().DestroyNotification(handBounceArrow, 0f);
			handBounceArrow = null;
		}
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (turn)
		{
		case 1:
		{
			GameState gameState = GameState.Get();
			if (gameState == null)
			{
				Debug.LogError($"RLK_Prologue.HandleMissionEventWithTiming(): GameState is null");
			}
			else
			{
				if (m_popup != null)
				{
					break;
				}
				if (m_popUpInfo == null)
				{
					Debug.LogError($"RLK_Prologue.HandleMissionEventWithTiming(): m_popUpInfo is null");
					break;
				}
				string[] gameStringKeys = null;
				if (!m_popUpInfo.TryGetValue(turn, out gameStringKeys))
				{
					Debug.LogError($"RLK_Prologue.HandleMissionEventWithTiming(): gameStringKeys is null");
					break;
				}
				if (gameStringKeys.Length == 0)
				{
					Debug.LogError($"RLK_Prologue.HandleMissionEventWithTiming(): gameStringKeys is empty");
					break;
				}
				NotificationManager notificationManager = NotificationManager.Get();
				if (notificationManager == null)
				{
					Debug.LogError($"RLK_Prologue.HandleMissionEventWithTiming(): notificationManager is null");
					break;
				}
				gameState.SetBusy(busy: true);
				Gameplay.Get().StartCoroutine(ShowArrowInSeconds(0f, bShowInHandZone: true));
				string gameStringKey = gameStringKeys[0];
				m_popup = notificationManager.CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(gameStringKey), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_RLK_Prologue_Male_Human_Arthas_InGame_PlayerPlaysCard_02_C, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("RLK_Prologue_Arthas_002p"), 2.5f));
				notificationManager.DestroyNotification(m_popup, 0f);
				m_popup = null;
				NotificationManager.Get().DestroyNotification(handBounceArrow, 0f);
				handBounceArrow = null;
				gameState.SetBusy(busy: false);
			}
			break;
		}
		case 3:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_RLK_Prologue_Male_Human_Uther_InGame_Turn_03_02_A);
			yield return MissionPlayVO(friendlyActor, VO_RLK_Prologue_Male_Human_Arthas_InGame_Turn_03_02_B);
			GameState.Get().SetBusy(busy: false);
			break;
		case 9:
			yield return MissionPlayVO(enemyActor, VO_RLK_Prologue_Male_Human_Uther_InGame_Turn_09_02_A);
			yield return MissionPlayVO(friendlyActor, VO_RLK_Prologue_Male_Human_Arthas_InGame_Turn_09_02_B);
			break;
		case 15:
			yield return MissionPlayVO(enemyActor, VO_RLK_Prologue_Male_Human_Uther_InGame_Turn_17_02_A);
			yield return MissionPlayVO(friendlyActor, VO_RLK_Prologue_Male_Human_Arthas_InGame_Turn_17_02_B);
			break;
		}
	}

	public IEnumerator NotifyOfMinionDiedWithTiming(Entity entity)
	{
		string cardID = entity.GetCardId();
		GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (cardID)
		{
		case "HERO_11bpt":
		case "RLK_Prologue_503":
		case "RLK_Prologue_RLK_506":
		case "RLK_Prologue_RLK_708":
		case "RLK_Prologue_RLK_731":
		case "RLK_Prologue_RLK_082":
		case "RLK_Prologue_RLK_720":
		case "RLK_Prologue_066":
		case "RLK_Prologue_RLK_079":
		case "RLK_Prologue_RLK_071":
		case "RLK_Prologue_RLK_741":
		case "RLK_Prologue_RLK_711":
			if (!bhasMinionDeathBeenTriggered)
			{
				GameState.Get().SetBusy(busy: true);
				Gameplay.Get().StartCoroutine(ShowArrowInSeconds(0f, bShowInHandZone: false));
				bhasMinionDeathBeenTriggered = true;
				yield return MissionPlayVO(friendlyActor, VO_RLK_Prologue_Male_Human_Arthas_InGame_MinionDies_02_A);
				GameState.Get().SetBusy(busy: false);
			}
			break;
		}
	}
}
