using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ICC_05_Lanathel : ICC_MissionEntity
{
	private Notification endTurnNotifier;

	private bool m_hasBiteNoTargetsVOPlayed;

	private HashSet<string> m_playedLines = new HashSet<string>();

	private List<string> m_BloodEssenceLines = new List<string> { "VO_ICC05_Lanathel_Female_Sanlayn_BloodEssence_01.prefab:da59f033f11d565499ec9da44ce6f46b", "VO_ICC05_Lanathel_Female_Sanlayn_BloodEssence_02.prefab:e2c7a28dfc86e7a4c94d00b380224e54", "VO_ICC05_Lanathel_Female_Sanlayn_BloodEssence_03.prefab:5771ffb67a64672498e2996de625f0bc" };

	public override void PreloadAssets()
	{
		foreach (string soundFile in new List<string>
		{
			"VO_ICC_841_Female_Sanlayn_Death_02.prefab:bcbec586c14d0ea47b294118f9fed020", "VO_ICC05_Lanathel_Female_Sanlayn_Intro_01.prefab:a5927ce80194b9d4abee763cc3451c13", "VO_ICC05_Lanathel_Female_Sanlayn_BossBite_01.prefab:e847622ab6b89e14b908a5092c681bd6", "VO_ICC05_Lanathel_Female_Sanlayn_BiteReminder_01.prefab:e7562028fc82af24e8b5408d70ae79d4", "VO_ICC05_Lanathel_Female_Sanlayn_PlayerBiteAcolyte_01.prefab:88ff928135afd344ebabeefc3747231e", "VO_ICC05_Lanathel_Female_Sanlayn_BloodEssence_01.prefab:da59f033f11d565499ec9da44ce6f46b", "VO_ICC05_Lanathel_Female_Sanlayn_BloodEssence_02.prefab:e2c7a28dfc86e7a4c94d00b380224e54", "VO_ICC05_Lanathel_Female_Sanlayn_BloodEssence_03.prefab:5771ffb67a64672498e2996de625f0bc", "VO_ICC05_Lanathel_Female_Sanlayn_Turn3_01.prefab:57e21d323fff0954dbcc930fbebb3f03", "VO_ICC05_Lanathel_Female_Sanlayn_Turn3_02.prefab:c6d817960f09ec94d842f61fbdd3c26a",
			"VO_ICC05_Lanathel_Female_Sanlayn_Wounded_01.prefab:b0dcbf6d181d5944e9e55b2f5a4b4bb1", "VO_ICC05_LichKing_Male_Human_Win_01.prefab:453fe532c40bbfa44a1aac8d99041f7e", "VO_ICC05_LichKing_Male_Human_Lose_01.prefab:9978afae17851584795146a392d5cb67", "VO_ICC05_Lanathel_Female_Sanlayn_EmoteResponse_01.prefab:8617d4664523cb94aa4847447cbfbb8f", "VO_ICC05_Lanathel_Female_Sanlayn_LichKing_01.prefab:2067c9859da805b4faea5fc664f0ada9", "VO_ICC05_LichKing_Male_Human_LichKing_02.prefab:a56f85a63706c304786eed0e3c861377", "VO_ICC05_Lanathel_Female_Sanlayn_TransformDK_01.prefab:534ffc634d9b4a74fa1a8067ebb0aa85", "VO_ICC05_Lanathel_Female_Sanlayn_BiteOoze_01.prefab:b3ff24b5195c4764fb18ef87f48f1fbd", "VO_ICC05_Lanathel_Female_Sanlayn_BiteSpikes_01.prefab:4b0a39c8dab009e4cb479e4e8bcd14ea", "VO_ICC05_Lanathel_Female_Sanlayn_BiteShell_01.prefab:229be1532f6fe31418f81b56784b4cd4",
			"VO_ICC05_Lanathel_Female_Sanlayn_BitePoisonous_01.prefab:e72be5987d5bb3144b46077aad7e23e7", "VO_ICC05_Lanathel_Female_Sanlayn_BloodPrince_01.prefab:c3b8a4ae2da8a4448a74c93f258f85c4", "VO_ICC05_Lanathel_Female_Sanlayn_PlayerBloodBite_01.prefab:c66830daa3ed9f045b6c3cbc347514c5", "VO_PrinceKeleseth_Male_Vampire_ResponseLanaThel_01.prefab:fddf65c2cd5115745845aa45b32017dc", "VO_PrinceTaldaram_Male_Vampire_ResponseLanaThel_01.prefab:ab050ef2ad0f9a4498de5779b3e3d677", "VO_PrinceValanar_Male_Vampire_ResponseLanaThel_01.prefab:3c482cfa6dfd9aa4182cfe35b66b4bcc"
		})
		{
			PreloadSound(soundFile);
		}
	}

	public override bool NotifyOfEndTurnButtonPushed()
	{
		bool bHaveUsedBite = true;
		Network.Options options = GameState.Get().GetOptionsPacket();
		if (options != null && options.List != null)
		{
			if (options.List.Count == 1)
			{
				NotificationManager.Get().DestroyAllArrows();
				return true;
			}
			for (int i = 0; i < options.List.Count; i++)
			{
				Network.Options.Option option = options.List[i];
				if (option.Type == Network.Options.Option.OptionType.POWER && GameState.Get().GetEntity(option.Main.ID).GetCardId() == "ICCA05_002p" && option.Main.PlayErrorInfo.IsValid())
				{
					bHaveUsedBite = false;
				}
			}
		}
		if (bHaveUsedBite)
		{
			return true;
		}
		bool bEveryoneHasBeenBitten = true;
		List<Card> list = new List<Card>();
		list.AddRange(GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone()
			.GetCards());
		list.AddRange(GameState.Get().GetOpposingSidePlayer().GetBattlefieldZone()
			.GetCards());
		foreach (Card card in list)
		{
			if (card == null || card.GetEntity() == null)
			{
				continue;
			}
			Entity entity = card.GetEntity();
			bool bThisMinionWasBitten = false;
			foreach (Entity enchantment in entity.GetEnchantments())
			{
				if (enchantment.GetCardId() == "ICCA05_002e")
				{
					bThisMinionWasBitten = true;
				}
			}
			if (!bThisMinionWasBitten)
			{
				bEveryoneHasBeenBitten = false;
			}
		}
		if (bEveryoneHasBeenBitten)
		{
			return true;
		}
		if (endTurnNotifier != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(endTurnNotifier);
		}
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		if (!m_hasBiteNoTargetsVOPlayed)
		{
			if (m_enemySpeaking)
			{
				return false;
			}
			m_hasBiteNoTargetsVOPlayed = true;
			Gameplay.Get().StartCoroutine(PlayBossLine(enemyActor, "VO_ICC05_Lanathel_Female_Sanlayn_BiteReminder_01.prefab:e7562028fc82af24e8b5408d70ae79d4"));
		}
		else
		{
			Vector3 endTurnPos = EndTurnButton.Get().transform.position;
			Vector3 popUpPos = new Vector3(endTurnPos.x - 3f, endTurnPos.y, endTurnPos.z);
			string textID = "ICC_05_LANATHEL_01";
			endTurnNotifier = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale(), GameStrings.Get(textID));
			NotificationManager.Get().DestroyNotification(endTurnNotifier, 2.5f);
		}
		return false;
	}

	protected override void InitEmoteResponses()
	{
		m_emoteResponseGroups = new List<EmoteResponseGroup>
		{
			new EmoteResponseGroup
			{
				m_triggers = new List<EmoteType>(MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS),
				m_responses = new List<EmoteResponse>
				{
					new EmoteResponse
					{
						m_soundName = "VO_ICC05_Lanathel_Female_Sanlayn_EmoteResponse_01.prefab:8617d4664523cb94aa4847447cbfbb8f",
						m_stringTag = "VO_ICC05_Lanathel_Female_Sanlayn_EmoteResponse_01"
					}
				}
			},
			new EmoteResponseGroup
			{
				m_triggers = new List<EmoteType> { EmoteType.START },
				m_responses = new List<EmoteResponse>
				{
					new EmoteResponse
					{
						m_soundName = "VO_ICC05_Lanathel_Female_Sanlayn_Intro_01.prefab:a5927ce80194b9d4abee763cc3451c13",
						m_stringTag = "VO_ICC05_Lanathel_Female_Sanlayn_Intro_01"
					}
				}
			}
		};
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		string missionEventName = "PLAYED_MISSION_EVENT_" + missionEvent;
		switch (missionEvent)
		{
		case 101:
			m_playedLines.Add(missionEventName);
			yield return PlayBossLine(enemyActor, "VO_ICC05_Lanathel_Female_Sanlayn_BossBite_01.prefab:e847622ab6b89e14b908a5092c681bd6");
			break;
		case 104:
			yield return PlayEasterEggLine(enemyActor, "VO_ICC05_Lanathel_Female_Sanlayn_PlayerBiteAcolyte_01.prefab:88ff928135afd344ebabeefc3747231e");
			break;
		case 105:
			yield return PlayLineOnlyOnce(enemyActor, "VO_ICC05_Lanathel_Female_Sanlayn_Wounded_01.prefab:b0dcbf6d181d5944e9e55b2f5a4b4bb1");
			break;
		case 106:
			if (m_BloodEssenceLines.Count != 0)
			{
				GameState.Get().SetBusy(busy: true);
				string randomBloodEssenceLine = m_BloodEssenceLines[Random.Range(0, m_BloodEssenceLines.Count)];
				m_BloodEssenceLines.Remove(randomBloodEssenceLine);
				yield return PlayLineOnlyOnce(enemyActor, randomBloodEssenceLine);
				GameState.Get().SetBusy(busy: false);
			}
			break;
		case 107:
			yield return PlayBossLine(enemyActor, "VO_ICC_841_Female_Sanlayn_Death_02.prefab:bcbec586c14d0ea47b294118f9fed020");
			break;
		case 108:
			m_playedLines.Add(missionEventName);
			yield return PlayLineOnlyOnce(enemyActor, "VO_ICC05_Lanathel_Female_Sanlayn_BitePoisonous_01.prefab:e72be5987d5bb3144b46077aad7e23e7");
			break;
		case 109:
			m_playedLines.Add(missionEventName);
			yield return PlayLineOnlyOnce(enemyActor, "VO_ICC05_Lanathel_Female_Sanlayn_BiteOoze_01.prefab:b3ff24b5195c4764fb18ef87f48f1fbd");
			break;
		case 110:
			m_playedLines.Add(missionEventName);
			yield return PlayLineOnlyOnce(enemyActor, "VO_ICC05_Lanathel_Female_Sanlayn_BiteSpikes_01.prefab:4b0a39c8dab009e4cb479e4e8bcd14ea");
			break;
		case 111:
			m_playedLines.Add(missionEventName);
			yield return PlayLineOnlyOnce(enemyActor, "VO_ICC05_Lanathel_Female_Sanlayn_BiteShell_01.prefab:229be1532f6fe31418f81b56784b4cd4");
			break;
		case 114:
			m_playedLines.Add(missionEventName);
			yield return PlayEasterEggLine(enemyActor, "VO_ICC05_Lanathel_Female_Sanlayn_PlayerBloodBite_01.prefab:c66830daa3ed9f045b6c3cbc347514c5");
			break;
		}
	}

	protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		if (turn == 3)
		{
			GameState.Get().SetBusy(busy: true);
			yield return PlayLineOnlyOnce(enemyActor, "VO_ICC05_Lanathel_Female_Sanlayn_Turn3_01.prefab:57e21d323fff0954dbcc930fbebb3f03");
			yield return PlayLineOnlyOnce(enemyActor, "VO_ICC05_Lanathel_Female_Sanlayn_Turn3_02.prefab:c6d817960f09ec94d842f61fbdd3c26a");
			GameState.Get().SetBusy(busy: false);
		}
	}

	protected override IEnumerator RespondToFriendlyPlayedCardWithTiming(Entity entity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		while (entity.GetCardType() == TAG_CARDTYPE.INVALID)
		{
			yield return null;
		}
		if (!m_playedLines.Contains(entity.GetCardId()))
		{
			string cardID = entity.GetCardId();
			m_playedLines.Add(cardID);
			Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCard()
				.GetActor();
			yield return WaitForEntitySoundToFinish(entity);
			switch (cardID)
			{
			case "ICC_314":
				yield return PlayEasterEggLine(enemyActor, "VO_ICC05_Lanathel_Female_Sanlayn_LichKing_01.prefab:2067c9859da805b4faea5fc664f0ada9");
				yield return PlayEasterEggLine(GetLichKingFriendlyMinion(), "VO_ICC05_LichKing_Male_Human_LichKing_02.prefab:a56f85a63706c304786eed0e3c861377");
				break;
			case "ICC_851":
				yield return PlayEasterEggLine(enemyActor, "VO_ICC05_Lanathel_Female_Sanlayn_BloodPrince_01.prefab:c3b8a4ae2da8a4448a74c93f258f85c4");
				yield return PlayEasterEggLine(GetActorByCardId("ICC_851"), "VO_PrinceKeleseth_Male_Vampire_ResponseLanaThel_01.prefab:fddf65c2cd5115745845aa45b32017dc");
				break;
			case "ICC_852":
				yield return PlayEasterEggLine(enemyActor, "VO_ICC05_Lanathel_Female_Sanlayn_BloodPrince_01.prefab:c3b8a4ae2da8a4448a74c93f258f85c4");
				yield return PlayEasterEggLine(GetActorByCardId("ICC_852"), "VO_PrinceTaldaram_Male_Vampire_ResponseLanaThel_01.prefab:ab050ef2ad0f9a4498de5779b3e3d677");
				break;
			case "ICC_853":
				yield return PlayEasterEggLine(enemyActor, "VO_ICC05_Lanathel_Female_Sanlayn_BloodPrince_01.prefab:c3b8a4ae2da8a4448a74c93f258f85c4");
				yield return PlayEasterEggLine(GetActorByCardId("ICC_853"), "VO_PrinceValanar_Male_Vampire_ResponseLanaThel_01.prefab:3c482cfa6dfd9aa4182cfe35b66b4bcc");
				break;
			}
			yield return IfPlayerPlaysDKHeroVO(entity, enemyActor, "VO_ICC05_Lanathel_Female_Sanlayn_TransformDK_01.prefab:534ffc634d9b4a74fa1a8067ebb0aa85");
		}
	}

	protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
	{
		if (gameResult == TAG_PLAYSTATE.WON)
		{
			yield return new WaitForSeconds(5f);
			yield return PlayClosingLine("LichKing_Banner_Quote.prefab:d42a8f4f69919f449b3dd8ebceaf2a3c", "VO_ICC05_LichKing_Male_Human_Win_01.prefab:453fe532c40bbfa44a1aac8d99041f7e");
		}
		if (gameResult == TAG_PLAYSTATE.LOST)
		{
			yield return new WaitForSeconds(5f);
			string voLine = "VO_ICC05_LichKing_Male_Human_Lose_01.prefab:9978afae17851584795146a392d5cb67";
			if (!NotificationManager.Get().HasSoundPlayedThisSession(voLine))
			{
				yield return Gameplay.Get().StartCoroutine(PlayCharacterQuoteAndWait("LichKing_Banner_Quote.prefab:d42a8f4f69919f449b3dd8ebceaf2a3c", voLine));
			}
		}
	}
}
