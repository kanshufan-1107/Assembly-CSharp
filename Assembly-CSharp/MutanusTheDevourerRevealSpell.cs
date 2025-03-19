using UnityEngine;

public class MutanusTheDevourerRevealSpell : Spell
{
	protected override void OnBirth(SpellStateType prevStateType)
	{
		Card sourceCard = GetSourceCard();
		Entity sourceEntity = sourceCard.GetEntity();
		if (sourceEntity != null && sourceEntity.IsControlledByOpposingSidePlayer() && sourceEntity.GetZone() == TAG_ZONE.HAND)
		{
			string assetPath = ActorNames.GetHandActor(sourceEntity);
			sourceCard.UpdateActor(forceIfNullZone: false, assetPath);
		}
		foreach (GameObject target in GetTargets())
		{
			if (target == null)
			{
				continue;
			}
			Card targetCard = target.GetComponent<Card>();
			if (!(targetCard == null))
			{
				Entity targetEntity = targetCard.GetEntity();
				if (targetEntity != null && targetEntity.IsControlledByOpposingSidePlayer() && targetEntity.GetZone() == TAG_ZONE.HAND)
				{
					string assetPath2 = ActorNames.GetHandActor(targetEntity);
					targetCard.UpdateActor(forceIfNullZone: false, assetPath2);
				}
			}
		}
		base.OnBirth(prevStateType);
	}

	public override void OnSpellFinished()
	{
		base.OnSpellFinished();
	}
}
