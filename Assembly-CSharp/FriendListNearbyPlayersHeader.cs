public class FriendListNearbyPlayersHeader : FriendListItemHeader
{
	private int m_StoredNearbyPlayerCount;

	private bool NearbyPlayersEnabled => Options.Get().GetBool(Option.NEARBY_PLAYERS);

	public void SetText(int nearbyPlayerCount)
	{
		SetText(NearbyPlayersEnabled ? GameStrings.Format("GLOBAL_FRIENDLIST_NEARBY_PLAYERS_HEADER", nearbyPlayerCount) : GameStrings.Format("GLOBAL_FRIENDLIST_NEARBY_PLAYERS_DISABLED_HEADER"));
		m_StoredNearbyPlayerCount = nearbyPlayerCount;
	}
}
