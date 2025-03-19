using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TB_HeadlessRedux : MissionEntity
{
	private static readonly AssetReference VO_CS2_222_Attack_02 = new AssetReference("VO_CS2_222_Attack_02.prefab:c3191e3764f78654899b70a311936b93");

	private static readonly AssetReference VO_HeadlessHorseman_Male_Human_HallowsEve_13 = new AssetReference("VO_HeadlessHorseman_Male_Human_HallowsEve_13.prefab:a015bfc61fca6a0489f276e3e2fbb0a3");

	private float popUpScale = 1f;

	private Vector3 popUpPos;

	private int _announcerLinesPlayed;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]>
	{
		{
			10,
			new string[2] { "TB_HEADLESSREDUX_01", "TB_HEADLESSREDUX_03" }
		},
		{
			11,
			new string[2] { "TB_HEADLESSREDUX_02", "TB_HEADLESSREDUX_04" }
		}
	};

	private Player friendlySidePlayer;

	private int isPlayerHorseman;

	public override void PreloadAssets()
	{
		PreloadSound(VO_HeadlessHorseman_Male_Human_HallowsEve_13.ToString());
		PreloadSound(VO_CS2_222_Attack_02.ToString());
	}

	public override AudioSource GetAnnouncerLine(Card heroCard, Card.AnnouncerLineType type)
	{
		_announcerLinesPlayed++;
		return _announcerLinesPlayed switch
		{
			1 => GetPreloadedSound(VO_CS2_222_Attack_02.ToString()), 
			2 => GetPreloadedSound(VO_HeadlessHorseman_Male_Human_HallowsEve_13.ToString()), 
			_ => base.GetAnnouncerLine(heroCard, type), 
		};
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		friendlySidePlayer = GameState.Get().GetFriendlySidePlayer();
		isPlayerHorseman = friendlySidePlayer.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1);
		while (m_enemySpeaking)
		{
			yield return null;
		}
		popUpPos = new Vector3(0f, 0f, 0f);
		if (m_popUpInfo.ContainsKey(missionEvent))
		{
			if (isPlayerHorseman == 1)
			{
				Notification popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][1]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				GameState.Get().SetBusy(busy: true);
				yield return new WaitForSeconds(4f);
				NotificationManager.Get().DestroyNotification(popup, 0f);
				yield return new WaitForSeconds(1f);
				popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[11][1]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return new WaitForSeconds(4f);
				NotificationManager.Get().DestroyNotification(popup, 0f);
				GameState.Get().SetBusy(busy: false);
			}
			else
			{
				Notification popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				GameState.Get().SetBusy(busy: true);
				yield return new WaitForSeconds(4f);
				NotificationManager.Get().DestroyNotification(popup, 0f);
				yield return new WaitForSeconds(1f);
				popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[11][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return new WaitForSeconds(4f);
				NotificationManager.Get().DestroyNotification(popup, 0f);
				GameState.Get().SetBusy(busy: false);
			}
		}
	}
}
