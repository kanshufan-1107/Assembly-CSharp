public static class PlayMakerUtils
{
	public static bool CanPlaymakerGetTag(GAME_TAG tag)
	{
		if (tag == GAME_TAG.CARDRACE)
		{
			return false;
		}
		return true;
	}
}
