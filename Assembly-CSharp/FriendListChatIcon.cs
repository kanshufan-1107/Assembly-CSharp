using System.Collections.Generic;
using System.Linq;
using Blizzard.GameService.SDK.Client.Integration;
using UnityEngine;

public class FriendListChatIcon : MonoBehaviour
{
	private BnetPlayer m_player;

	public BnetPlayer GetPlayer()
	{
		return m_player;
	}

	public bool SetPlayer(BnetPlayer player)
	{
		if (m_player == player)
		{
			return false;
		}
		m_player = player;
		UpdateIcon();
		return true;
	}

	public void UpdateIcon()
	{
		if (m_player == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		List<BnetWhisper> whispers = BnetWhisperMgr.Get().GetWhispersWithPlayer(m_player);
		if (whispers == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		if (WhisperUtil.IsSpeaker(BnetPresenceMgr.Get().GetMyPlayer(), whispers[whispers.Count - 1]))
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		PlayerChatInfo chatInfo = ChatMgr.Get().GetPlayerChatInfo(m_player);
		if (chatInfo != null && whispers.LastOrDefault((BnetWhisper whisper) => WhisperUtil.IsSpeaker(m_player, whisper)) == chatInfo.GetLastSeenWhisper())
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			base.gameObject.SetActive(value: true);
		}
	}
}
