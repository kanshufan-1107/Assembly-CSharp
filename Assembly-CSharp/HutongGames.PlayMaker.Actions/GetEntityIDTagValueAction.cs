namespace HutongGames.PlayMaker.Actions;

[Tooltip("Stores the specified tag value for Entity with passed Entity ID in passed int.")]
[ActionCategory("Pegasus")]
public class GetEntityIDTagValueAction : FsmStateAction
{
	public FsmInt m_entityID;

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
		if (m_entityID.IsNone)
		{
			global::Log.Spells.PrintError("{0}.OnEnter() - No entity ID was passed!", this);
			Finish();
			return;
		}
		if (m_TagValue.IsNone)
		{
			global::Log.Spells.PrintError("{0}.OnEnter() - No variable hooked up to store tag value!", this);
			Finish();
			return;
		}
		Entity ent = GameState.Get().GetEntity(m_entityID.Value);
		if (ent == null)
		{
			global::Log.Spells.PrintError("{0}.OnEnter() - FAILED to find entity with id {1}", this, m_entityID.Value);
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
