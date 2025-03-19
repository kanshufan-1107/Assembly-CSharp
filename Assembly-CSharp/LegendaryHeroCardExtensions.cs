public static class LegendaryHeroCardExtensions
{
	public static void ActivateLegendaryHeroAnimEvent(this Card heroCard, LegendaryHeroAnimations animation)
	{
		if (heroCard != null)
		{
			Actor heroActor = heroCard.GetActor();
			if (heroActor != null)
			{
				heroActor.LegendaryHeroPortrait?.RaiseAnimationEvent(animation);
			}
		}
	}
}
