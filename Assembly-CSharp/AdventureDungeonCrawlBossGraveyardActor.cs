using System;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.DungeonCrawl;
using UnityEngine;

public class AdventureDungeonCrawlBossGraveyardActor : Actor
{
	[Serializable]
	public class BossGraveyardActorVisualStyle
	{
		public DungeonRunVisualStyle VisualStyle;

		public Material BossBackerMaterial;
	}

	public MeshRenderer m_BossBackerRenderer;

	public List<BossGraveyardActorVisualStyle> m_BossGraveyardActorStyle;

	public void SetStyle(IDungeonCrawlData data)
	{
		DungeonRunVisualStyle visualStyle = data.VisualStyle;
		foreach (BossGraveyardActorVisualStyle actorStyle in m_BossGraveyardActorStyle)
		{
			if (visualStyle == actorStyle.VisualStyle)
			{
				m_BossBackerRenderer.SetMaterial(actorStyle.BossBackerMaterial);
				break;
			}
		}
	}
}
