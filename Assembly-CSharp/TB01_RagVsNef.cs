using System.Collections;
using UnityEngine;

public class TB01_RagVsNef : MissionEntity
{
	private Card m_ragnarosCard;

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		if (missionEvent != 1)
		{
			yield break;
		}
		foreach (Player value in GameState.Get().GetPlayerMap().Values)
		{
			Entity hero = value.GetHero();
			Card heroCard = hero.GetCard();
			if (hero.GetCardId() == "TBA01_1")
			{
				m_ragnarosCard = heroCard;
			}
		}
		GameState.Get().SetBusy(busy: true);
		CardSoundSpell spell = m_ragnarosCard.PlayEmote(EmoteType.THREATEN);
		if (spell.m_CardSoundData.m_AudioSource == null || spell.m_CardSoundData.m_AudioSource.clip == null)
		{
			GameState.Get().SetBusy(busy: false);
			yield break;
		}
		float clipLength = spell.m_CardSoundData.m_AudioSource.clip.length;
		yield return new WaitForSeconds((float)((double)clipLength * 0.8));
		GameState.Get().SetBusy(busy: false);
	}
}
