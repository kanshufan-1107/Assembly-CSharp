using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FB_ELObrawl : MissionEntity
{
	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]> { 
	{
		10,
		new string[3] { "FB_ELO_FAVORED", "FB_ELO_UNDERDOG", "FB_ELO_EVEN" }
	} };

	private Player friendlySidePlayer;

	private float popUpScale = 1f;

	private Vector3 popUpPos;

	public override void PreloadAssets()
	{
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		friendlySidePlayer = GameState.Get().GetFriendlySidePlayer();
		int isPlayerUnderdog = friendlySidePlayer.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1);
		while (m_enemySpeaking)
		{
			yield return null;
		}
		popUpPos = new Vector3(0f, 0f, -40f);
		if (m_popUpInfo.ContainsKey(missionEvent))
		{
			switch (isPlayerUnderdog)
			{
			case 3:
			{
				Notification popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][2]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return new WaitForSeconds(4f);
				NotificationManager.Get().DestroyNotification(popup, 0f);
				break;
			}
			case 1:
			{
				Notification popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][1]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return new WaitForSeconds(4f);
				NotificationManager.Get().DestroyNotification(popup, 0f);
				break;
			}
			default:
			{
				Notification popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return new WaitForSeconds(4f);
				NotificationManager.Get().DestroyNotification(popup, 0f);
				break;
			}
			}
		}
	}
}
