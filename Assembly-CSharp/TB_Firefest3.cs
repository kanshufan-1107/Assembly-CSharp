using System.Collections;
using UnityEngine;

public class TB_Firefest3 : MissionEntity
{
	private Actor headActor;

	private Card headCard;

	private static readonly AssetReference VO_Rakanishu_Male_Elemental_FF_Start_02 = new AssetReference("VO_Rakanishu_Male_Elemental_FF_Start_02.prefab:8985db50d3217a349812bd24624db30d");

	public override void PreloadAssets()
	{
		PreloadSound(VO_Rakanishu_Male_Elemental_FF_Start_02);
	}

	private void GetHorsemanHead()
	{
		int headID = GameState.Get().GetGameEntity().GetTag(GAME_TAG.TAG_SCRIPT_DATA_ENT_1);
		if (headID != 0)
		{
			Entity headEnt = GameState.Get().GetEntity(headID);
			if (headEnt != null)
			{
				headCard = headEnt.GetCard();
			}
			if (headCard != null)
			{
				headActor = headCard.GetActor();
			}
		}
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		if (missionEvent == 15)
		{
			GetHorsemanHead();
		}
		while (m_enemySpeaking)
		{
			yield return null;
		}
		if (missionEvent != 15)
		{
			GetHorsemanHead();
		}
		if (missionEvent == 10)
		{
			Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(VO_Rakanishu_Male_Elemental_FF_Start_02, Notification.SpeechBubbleDirection.TopRight, headActor));
			GameState.Get().SetBusy(busy: true);
			yield return new WaitForSeconds(5f);
			GameState.Get().SetBusy(busy: false);
		}
	}
}
