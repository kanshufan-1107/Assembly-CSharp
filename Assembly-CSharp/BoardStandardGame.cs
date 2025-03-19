using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class BoardStandardGame : BoardLayout
{
	public Actor[] m_DeckActors;

	private static BoardStandardGame s_instance;

	public static BoardStandardGame Get()
	{
		return s_instance;
	}

	public override void Awake()
	{
		base.Awake();
		if (s_instance == null)
		{
			s_instance = this;
		}
	}

	private void Start()
	{
		DeckColors();
	}

	public void DeckColors()
	{
		if (Board.Get() == null || m_DeckActors == null)
		{
			return;
		}
		Actor[] deckActors = m_DeckActors;
		foreach (Actor deckActor in deckActors)
		{
			if (deckActor == null)
			{
				continue;
			}
			MeshRenderer renderer = deckActor.GetMeshRenderer();
			if (!(renderer == null))
			{
				Material material = renderer.GetMaterial();
				if (material != null)
				{
					material.color = Board.Get().m_DeckColor;
				}
			}
		}
	}
}
