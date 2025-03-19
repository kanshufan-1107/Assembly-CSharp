public class ZoneTeammatePlay : Zone
{
	public override string ToString()
	{
		return $"{base.ToString()} (Teammate Play)";
	}

	public override bool CanAcceptTags(int controllerId, TAG_ZONE zoneTag, TAG_CARDTYPE cardType, Entity entity)
	{
		return false;
	}

	public override void UpdateLayout()
	{
		m_updatingLayout++;
		UpdateLayoutFinished();
	}
}
