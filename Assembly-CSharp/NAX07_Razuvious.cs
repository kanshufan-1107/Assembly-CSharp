using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NAX07_Razuvious : NAX_MissionEntity
{
	private bool m_heroPowerLinePlayed;

	public override void PreloadAssets()
	{
		PreloadSound("VO_NAX7_01_HP_02.prefab:cb3aadc3fbe355e40bbd5463f09ffdf8");
		PreloadSound("VO_NAX7_01_START_01.prefab:3fc94f039bccb2d4ca0e0a242b2f955e");
		PreloadSound("VO_NAX7_01_EMOTE_05.prefab:a116dabbfbb825e4cb519d18a2c21779");
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
						m_soundName = "VO_NAX7_01_EMOTE_05.prefab:a116dabbfbb825e4cb519d18a2c21779",
						m_stringTag = "VO_NAX7_01_EMOTE_05"
					}
				}
			}
		};
	}

	protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
	{
		if (gameResult == TAG_PLAYSTATE.WON && !GameMgr.Get().IsClassChallengeMission())
		{
			yield return new WaitForSeconds(5f);
			NotificationManager.Get().CreateKTQuote("VO_KT_RAZUVIOUS2_59", "VO_KT_RAZUVIOUS2_59.prefab:58901b0d8c4e834489caca72c1fb5ecc");
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
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		if (entity.GetCardId() == "NAX7_03" && !m_heroPowerLinePlayed)
		{
			m_heroPowerLinePlayed = true;
			Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech("VO_NAX7_01_HP_02.prefab:cb3aadc3fbe355e40bbd5463f09ffdf8", Notification.SpeechBubbleDirection.TopRight, enemyActor));
		}
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		yield return Gameplay.Get().StartCoroutine(base.HandleMissionEventWithTiming(missionEvent));
		if (missionEvent != 1)
		{
			yield break;
		}
		bool understudiesAreInPlay = false;
		PowerTaskList taskList = GameState.Get().GetPowerProcessor().GetCurrentTaskList();
		Entity sourceEntity = taskList?.GetSourceEntity();
		if (sourceEntity != null && sourceEntity.GetCardId() == "NAX7_05")
		{
			foreach (PowerTask task in taskList.GetTaskList())
			{
				Network.PowerHistory power = task.GetPower();
				if (power.Type != Network.PowerType.META_DATA)
				{
					continue;
				}
				Network.HistMetaData metaData = power as Network.HistMetaData;
				if (metaData.MetaType != 0 || metaData.Info == null || metaData.Info.Count == 0)
				{
					continue;
				}
				for (int j = 0; j < metaData.Info.Count; j++)
				{
					Entity targetEntity = GameState.Get().GetEntity(metaData.Info[j]);
					if (targetEntity != null && targetEntity.GetCardId() == "NAX7_02")
					{
						understudiesAreInPlay = true;
						break;
					}
				}
				if (understudiesAreInPlay)
				{
					break;
				}
			}
		}
		if (understudiesAreInPlay)
		{
			Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCard()
				.GetActor();
			Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech("VO_NAX7_01_START_01.prefab:3fc94f039bccb2d4ca0e0a242b2f955e", Notification.SpeechBubbleDirection.TopRight, enemyActor));
		}
	}
}
