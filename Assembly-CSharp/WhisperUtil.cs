using Blizzard.GameService.SDK.Client.Integration;

public static class WhisperUtil
{
	public static BnetPlayer GetSpeaker(BnetWhisper whisper)
	{
		return BnetUtils.GetPlayer(whisper.GetSpeakerId());
	}

	public static BnetPlayer GetReceiver(BnetWhisper whisper)
	{
		return BnetUtils.GetPlayer(whisper.GetReceiverId());
	}

	public static BnetPlayer GetTheirPlayer(BnetWhisper whisper)
	{
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		if (myself == null)
		{
			return null;
		}
		BnetPlayer speaker = GetSpeaker(whisper);
		BnetPlayer receiver = GetReceiver(whisper);
		if (myself == speaker)
		{
			return receiver;
		}
		if (myself == receiver)
		{
			return speaker;
		}
		return null;
	}

	public static BnetAccountId GetTheirAccountId(BnetWhisper whisper)
	{
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		if (myself == null)
		{
			return null;
		}
		if (myself.HasAccount(whisper.GetSpeakerId()))
		{
			return whisper.GetReceiverId();
		}
		if (myself.HasAccount(whisper.GetReceiverId()))
		{
			return whisper.GetSpeakerId();
		}
		return null;
	}

	public static bool IsDisplayable(BnetWhisper whisper)
	{
		BnetPlayer speaker = GetSpeaker(whisper);
		BnetPlayer receiver = GetReceiver(whisper);
		if (speaker == null)
		{
			return false;
		}
		if (!speaker.IsDisplayable())
		{
			return false;
		}
		if (receiver == null)
		{
			return false;
		}
		if (!receiver.IsDisplayable())
		{
			return false;
		}
		return true;
	}

	public static bool IsSpeaker(BnetPlayer player, BnetWhisper whisper)
	{
		return player?.HasAccount(whisper.GetSpeakerId()) ?? false;
	}

	public static bool IsSpeakerOrReceiver(BnetPlayer player, BnetWhisper whisper)
	{
		if (player == null)
		{
			return false;
		}
		if (!player.HasAccount(whisper.GetSpeakerId()))
		{
			return player.HasAccount(whisper.GetReceiverId());
		}
		return true;
	}
}
