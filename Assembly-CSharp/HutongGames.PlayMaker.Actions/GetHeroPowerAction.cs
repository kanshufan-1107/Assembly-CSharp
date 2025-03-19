using Blizzard.T5.Core.Utils;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Use the spell to find the hero power")]
public class GetHeroPowerAction : FsmStateAction
{
	[Tooltip("FSM Object to store the Hero Power in when retrieved")]
	public FsmGameObject m_HeroPowerGameObject;

	[Tooltip("Provide an actor to specifically get the hero power from, uses Owner otherwise")]
	public FsmGameObject m_OverrideOwnerToGetHeroPowerFrom;

	public override void Reset()
	{
		m_HeroPowerGameObject = null;
		m_OverrideOwnerToGetHeroPowerFrom = null;
	}

	public override void OnEnter()
	{
		GameObject HeroGameObject = base.Owner;
		if (m_OverrideOwnerToGetHeroPowerFrom != null)
		{
			HeroGameObject = m_OverrideOwnerToGetHeroPowerFrom.Value;
		}
		if (HeroGameObject == null)
		{
			Debug.LogError("GetHeroPowerAction: HeroGameObject is null!");
			return;
		}
		Spell spell = HeroGameObject.GetComponentInChildren<Spell>();
		if (spell == null)
		{
			spell = GameObjectUtils.FindComponentInThisOrParents<Spell>(HeroGameObject);
			if (spell == null)
			{
				Finish();
				return;
			}
		}
		if (spell == null)
		{
			Debug.LogWarning("GetHeroPowerAction: spell is null!");
			return;
		}
		Card card = spell.GetSourceCard();
		if (card == null)
		{
			Actor actor = GameObjectUtils.FindComponentInThisOrParents<Actor>(HeroGameObject);
			if (actor == null)
			{
				Debug.LogWarning("GetHeroPowerAction: actor is null!");
				return;
			}
			card = actor.GetCard();
			if (card == null)
			{
				Debug.LogWarning("GetHeroPowerAction: card is null!");
				return;
			}
		}
		Card heroPowerCard = card.GetHeroPowerCard();
		if (heroPowerCard == null)
		{
			Debug.LogWarning("GetHeroPowerAction: heroPowerCard is null!");
			return;
		}
		Actor heroPowerCardActor = heroPowerCard.GetActor();
		if (heroPowerCardActor == null)
		{
			Debug.LogWarning("GetHeroPowerAction: heroPowerCardActor is null!");
			return;
		}
		GameObject heroPowerGameObject = heroPowerCardActor.gameObject;
		if (!m_HeroPowerGameObject.IsNone)
		{
			m_HeroPowerGameObject.Value = heroPowerGameObject;
		}
		Finish();
	}
}
