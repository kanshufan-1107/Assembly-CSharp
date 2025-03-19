using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Fires an event based on the tag value of a specified entity")]
public class SendEventBasedOnEntityTagAction : SpellAction
{
	public class ValueRangeEvent
	{
		public ValueRange m_range;

		public FsmEvent m_event;
	}

	public FsmOwnerDefault m_spellObject;

	public ValueRangeEvent[] m_rangeEventArray;

	public Which m_whichEntity;

	public GAME_TAG m_tagToCheck;

	protected override GameObject GetSpellOwner()
	{
		return base.Fsm.GetOwnerDefaultTarget(m_spellObject);
	}

	public override void OnEnter()
	{
		base.OnEnter();
		Entity ent = GetEntity(m_whichEntity);
		if (ent == null)
		{
			global::Log.All.PrintWarning("{0}.OnEnter() - FAILED to find relevant entity: \"{1}\"", this, m_whichEntity);
			Finish();
			return;
		}
		if (!PlayMakerUtils.CanPlaymakerGetTag(m_tagToCheck))
		{
			global::Log.Spells.PrintError("{0}.OnEnter() - Playmaker should not check value of tag {1}", this, m_tagToCheck);
			Finish();
			return;
		}
		FsmEvent eventToSend = null;
		if (m_tagToCheck == GAME_TAG.DATABASE_ID)
		{
			int dbId = GameUtils.TranslateCardIdToDbId(ent.GetCardId());
			eventToSend = DetermineEventToSend(dbId);
		}
		else
		{
			eventToSend = DetermineEventToSend(ent.GetTag(m_tagToCheck));
		}
		if (eventToSend != null)
		{
			base.Fsm.Event(eventToSend);
		}
		else
		{
			global::Log.All.PrintError("{0}.OnEnter() - FAILED to find any event that could be sent, did you set up ranges?", this);
		}
		Finish();
	}

	private FsmEvent DetermineEventToSend(int tagValue)
	{
		return SpellUtils.GetAppropriateElementAccordingToRanges(m_rangeEventArray, (ValueRangeEvent x) => x.m_range, tagValue)?.m_event;
	}
}
