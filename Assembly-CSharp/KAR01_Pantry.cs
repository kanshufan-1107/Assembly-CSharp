using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KAR01_Pantry : KAR_MissionEntity
{
	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		PreloadSound("VO_SilverwareGolem_Male_SilverwareGolem_SilverwareEmoteResponse_01.prefab:79a423cdcbf24144e886d12dba0a5422");
		PreloadSound("VO_SilverwareGolem_Male_SilverwareGolem_SilverwareKnifeJuggler_01.prefab:0d1779295cc055244982e057e6656dd0");
		PreloadSound("VO_Moroes_Male_Human_SilverwareResponse_01.prefab:f0b8b095a9b178d4d9acaf294c95a172");
		PreloadSound("VO_Moroes_Male_Human_SilverwareTurn3_02.prefab:be3de0e6492393345a175bb487db70a1");
		PreloadSound("VO_SilverwareGolem_Male_SilverwareGolem_SilverwareForkedLightning_01.prefab:dff5ceda9dcf03741a2444f78f0f0e23");
		PreloadSound("VO_Moroes_Male_Human_MedivhSkinResponse_01.prefab:74cd0ae7e1f7b9c4ca755b74c156406d");
		PreloadSound("VO_Moroes_Male_Human_SilverwareWin_01.prefab:123fc6f36a45c6e468dcd3faeb80a109");
		PreloadSound("VO_SilverwareGolem_Male_SilverwareGolem_SilverwarePlateTossing_01.prefab:f2ff07a0e24c0ea4cbda1e154de5462b");
		PreloadSound("VO_SilverwareGolem_Male_SilverwareGolem_SilverwareHeroPower_01.prefab:1ae8be96b941b384aabb4487fc24fecb");
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
						m_soundName = "VO_SilverwareGolem_Male_SilverwareGolem_SilverwareEmoteResponse_01.prefab:79a423cdcbf24144e886d12dba0a5422",
						m_stringTag = "VO_SilverwareGolem_Male_SilverwareGolem_SilverwareEmoteResponse_01"
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
		string missionEventName = "PLAYED_MISSION_EVENT_" + missionEvent;
		if (!m_playedLines.Contains(missionEventName))
		{
			m_playedLines.Add(missionEventName);
			Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
				.GetActor();
			switch (missionEvent)
			{
			case 1:
				yield return PlayEasterEggLine(enemyActor, "VO_SilverwareGolem_Male_SilverwareGolem_SilverwareKnifeJuggler_01.prefab:0d1779295cc055244982e057e6656dd0");
				break;
			case 2:
				yield return PlayMissionFlavorLine("Moroes_BigQuote.prefab:321274c1b67d79a4ba421a965bbc9e6d", "VO_Moroes_Male_Human_SilverwareResponse_01.prefab:f0b8b095a9b178d4d9acaf294c95a172");
				break;
			}
		}
	}

	protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		if (turn == 1)
		{
			yield return PlayOpeningLine("Moroes_BigQuote.prefab:321274c1b67d79a4ba421a965bbc9e6d", "VO_Moroes_Male_Human_SilverwareTurn3_02.prefab:be3de0e6492393345a175bb487db70a1");
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
		if (m_playedLines.Contains(entity.GetCardId()))
		{
			yield break;
		}
		string cardID = entity.GetCardId();
		m_playedLines.Add(cardID);
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		if (!(cardID == "EX1_251"))
		{
			if (cardID == "CS2_034_H1")
			{
				yield return PlayEasterEggLine("Moroes_BigQuote.prefab:321274c1b67d79a4ba421a965bbc9e6d", "VO_Moroes_Male_Human_MedivhSkinResponse_01.prefab:74cd0ae7e1f7b9c4ca755b74c156406d");
			}
		}
		else
		{
			yield return PlayEasterEggLine(enemyActor, "VO_SilverwareGolem_Male_SilverwareGolem_SilverwareForkedLightning_01.prefab:dff5ceda9dcf03741a2444f78f0f0e23");
		}
	}

	protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
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
			switch (cardID)
			{
			case "KAR_A02_13":
			case "KAR_A02_13H":
				yield return PlayBossLine(enemyActor, "VO_SilverwareGolem_Male_SilverwareGolem_SilverwareHeroPower_01.prefab:1ae8be96b941b384aabb4487fc24fecb");
				break;
			case "KAR_A02_11":
				yield return PlayBossLine(enemyActor, "VO_SilverwareGolem_Male_SilverwareGolem_SilverwarePlateTossing_01.prefab:f2ff07a0e24c0ea4cbda1e154de5462b");
				break;
			}
		}
	}

	protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
	{
		if (gameResult == TAG_PLAYSTATE.WON)
		{
			yield return new WaitForSeconds(5f);
			yield return PlayClosingLine("Moroes_Quote.prefab:ea3a21837aab2b0448ce4090103724cf", "VO_Moroes_Male_Human_SilverwareWin_01.prefab:123fc6f36a45c6e468dcd3faeb80a109");
		}
	}
}
