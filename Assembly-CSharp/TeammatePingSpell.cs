public class TeammatePingSpell : Spell
{
	protected override void OnDestroy()
	{
		NotifyPortalOfPingRemoval();
		base.OnDestroy();
	}

	public override void Hide()
	{
		NotifyPortalOfPingRemoval();
		base.Hide();
	}

	private void NotifyPortalOfPingRemoval()
	{
		Actor actor = GetComponentInParent<Actor>();
		if (TeammateBoardViewer.Get() != null)
		{
			TeammateBoardViewer.Get().RemovePortalPingIfInteriorIsActor(actor);
		}
	}
}
