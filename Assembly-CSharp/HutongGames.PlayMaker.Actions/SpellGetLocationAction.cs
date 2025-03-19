using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Get position from the bones in the board")]
[ActionCategory("Pegasus")]
public class SpellGetLocationAction : SpellAction
{
	public FsmOwnerDefault m_SpellObject;

	[Tooltip("Choose a location from the spell to get the position from")]
	public SpellLocation m_Location;

	[Tooltip("Choose a bone, usually used with board location")]
	public FsmString m_Bone;

	[Tooltip("Store the position")]
	[UIHint(UIHint.Variable)]
	public FsmVector3 m_StorePosition;

	protected override GameObject GetSpellOwner()
	{
		return base.Fsm.GetOwnerDefaultTarget(m_SpellObject);
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (!(m_spell == null))
		{
			GameObject locationObj = SpellUtils.GetSpellLocationObject(m_spell, m_Location, m_Bone.Value);
			if (locationObj != null)
			{
				m_StorePosition.Value = locationObj.transform.position;
			}
			Finish();
		}
	}
}
