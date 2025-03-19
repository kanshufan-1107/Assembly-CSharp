using System.Collections.Generic;

public struct CutsceneLoadedActors
{
	public Actor FriendlyHeroActor { get; set; }

	public Actor FriendlyHeroPowerActor { get; set; }

	public Actor OpponentHeroActor { get; set; }

	public Actor OpponentHeroPowerActor { get; set; }

	public Actor FriendlyAlternateFormHeroActor { get; set; }

	public List<Actor> FriendlyMinions { get; set; }

	public List<Actor> OpponentMinions { get; set; }
}
