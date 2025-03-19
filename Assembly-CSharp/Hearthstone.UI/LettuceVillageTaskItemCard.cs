using UnityEngine;

namespace Hearthstone.UI;

public class LettuceVillageTaskItemCard : Card
{
	private bool m_hasUpdatedActor;

	private EntityDef m_previousLoadedEntityDef;

	protected override GameObject LoadActorByActorAssetType(EntityDef entityDef, TAG_PREMIUM premium)
	{
		if (!Application.IsPlaying(this))
		{
			return base.LoadActorByActorAssetType(entityDef, premium);
		}
		if (m_previousLoadedEntityDef == null || entityDef.GetCardId() != m_previousLoadedEntityDef.GetCardId())
		{
			m_hasUpdatedActor = false;
			m_previousLoadedEntityDef = entityDef;
			return base.LoadActorByActorAssetType(entityDef, premium);
		}
		return m_cardActor.gameObject;
	}

	protected override void UpdateActor()
	{
		if (!m_hasUpdatedActor)
		{
			base.UpdateActor();
			m_hasUpdatedActor = true;
		}
	}
}
