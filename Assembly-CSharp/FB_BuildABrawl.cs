using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FB_BuildABrawl : MissionEntity
{
	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]>
	{
		{
			10,
			new string[1] { "FB_BUILDABRAWL_INTRO" }
		},
		{
			11,
			new string[1] { "FB_BUILDABRAWL_NOTSTARTED" }
		},
		{
			1,
			new string[17]
			{
				"", "FB_BUILDABRAWL_001", "FB_BUILDABRAWL_002", "FB_BUILDABRAWL_003", "FB_BUILDABRAWL_004", "FB_BUILDABRAWL_005", "FB_BUILDABRAWL_006", "FB_BUILDABRAWL_007", "FB_BUILDABRAWL_008", "FB_BUILDABRAWL_009",
				"FB_BUILDABRAWL_010", "FB_BUILDABRAWL_011", "FB_BUILDABRAWL_012", "FB_BUILDABRAWL_013", "FB_BUILDABRAWL_014", "FB_BUILDABRAWL_015", "FB_BUILDABRAWL_016"
			}
		}
	};

	private Player friendlySidePlayer;

	private float popUpScale = 1.25f;

	private Vector3 popUpPos;

	private int brawl;

	public override void PreloadAssets()
	{
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		friendlySidePlayer = GameState.Get().GetFriendlySidePlayer();
		brawl = friendlySidePlayer.GetTag(GAME_TAG.SCORE_VALUE_3);
		Debug.Log("Brawl # " + brawl);
		while (m_enemySpeaking)
		{
			yield return null;
		}
		popUpPos = new Vector3(0f, 0f, -40f);
		if (m_popUpInfo.ContainsKey(missionEvent))
		{
			if (missionEvent == 10)
			{
				Notification popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]) + "\n" + GameStrings.Get(m_popUpInfo[1][brawl]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return new WaitForSeconds(4f);
				NotificationManager.Get().DestroyNotification(popup, 0f);
			}
			if (missionEvent == 11)
			{
				Notification popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return new WaitForSeconds(4f);
				NotificationManager.Get().DestroyNotification(popup, 0f);
			}
		}
	}
}
