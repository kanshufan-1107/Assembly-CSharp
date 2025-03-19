using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Triggers a generic event in the 3D Portrait Scene. This can be handled with generic event handlers")]
[ActionCategory("Pegasus")]
public class TriggerLegendaryHeroGenericEvent : FsmStateAction
{
	[RequiredField]
	[CheckForComponent(typeof(Actor))]
	[Tooltip("Actors game object")]
	public FsmOwnerDefault m_HeroObject;

	[RequiredField]
	[Tooltip("The name of the event to trigger.")]
	public string m_EventName;

	[Tooltip("Any data to send with the event.")]
	public FsmGameObject m_EventData;

	public override void OnEnter()
	{
		base.OnEnter();
		GameObject go = base.Fsm.GetOwnerDefaultTarget(m_HeroObject);
		if (!go)
		{
			Finish();
			return;
		}
		Actor actor = go.GetComponent<Actor>();
		if (actor == null)
		{
			global::Log.Spells.PrintError("{0}.OnEnter() - FAILED to find actor on game object {1}", this, m_HeroObject);
			Finish();
			return;
		}
		ILegendaryHeroPortrait portrait = actor.LegendaryHeroPortrait;
		if (portrait == null)
		{
			global::Log.Spells.PrintError("{0}.OnEnter() - FAILED to find legendary portrait on actor {1}", this, m_HeroObject);
			Finish();
		}
		else
		{
			Object eventData = m_EventData.Value;
			portrait.RaiseGenericEvent(m_EventName, eventData);
			Finish();
		}
	}
}
