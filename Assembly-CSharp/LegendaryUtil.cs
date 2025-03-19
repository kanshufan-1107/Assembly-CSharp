using UnityEngine;

public class LegendaryUtil
{
	public static Actor FindLegendaryHeroActor(GameObject go)
	{
		Transform parent = go.transform.parent;
		while (parent != null)
		{
			if (parent.gameObject.TryGetComponent<Actor>(out var actor))
			{
				return actor;
			}
			parent = parent.parent;
		}
		return null;
	}

	public static ILegendaryHeroPortrait FindLegendaryPortrait(GameObject go)
	{
		Actor actor = FindLegendaryHeroActor(go);
		if (actor != null)
		{
			return actor.LegendaryHeroPortrait;
		}
		return null;
	}
}
