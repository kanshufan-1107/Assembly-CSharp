namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Stores the value of an Actor's Entity's tag in passed int. If the Actor doesn't have an Entity set, this won't work. (ex. dummy actors)")]
public class GetActorTagValueAction : FsmStateAction
{
	public FsmGameObject m_actorObject;

	public GAME_TAG m_tagToCheck;

	[Tooltip("Output variable.")]
	[UIHint(UIHint.Variable)]
	[RequiredField]
	public FsmInt m_TagValue;

	public override void Reset()
	{
		m_TagValue = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_TagValue == null)
		{
			global::Log.Gameplay.PrintError("{0}.OnEnter() - No variable hooked up to store tag value!", this);
			Finish();
			return;
		}
		Actor actorObject = m_actorObject.Value.GetComponent<Actor>();
		if (actorObject == null)
		{
			global::Log.All.PrintError("{0}.OnEnter() - FAILED to find actor component", this);
			Finish();
			return;
		}
		Entity ent = actorObject.GetEntity();
		if (ent == null)
		{
			global::Log.All.PrintError("{0}.OnEnter() - FAILED to find attached entity", this);
			Finish();
		}
		else if (!PlayMakerUtils.CanPlaymakerGetTag(m_tagToCheck))
		{
			global::Log.Spells.PrintError("{0}.OnEnter() - Playmaker should not check value of tag {1}", this, m_tagToCheck);
			Finish();
		}
		else
		{
			m_TagValue.Value = ent.GetTag(m_tagToCheck);
			Finish();
		}
	}
}
