using System.Linq;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Given a fake minion in play, destroy it and clean up the zone.")]
[ActionCategory("Pegasus")]
public class DestroyLettuceAbilityMinionPreviewInPlayAction : FsmStateAction
{
	public FsmGameObject m_FakeMinionGameObject;

	public override void Reset()
	{
		m_FakeMinionGameObject = new FsmGameObject
		{
			UseVariable = true
		};
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_FakeMinionGameObject == null || m_FakeMinionGameObject.Value == null)
		{
			Finish();
		}
		else if (m_FakeMinionGameObject.Value.GetComponent<Actor>() == null)
		{
			Debug.LogError("DestroyLettuceAbilityMinionPreviewInPlayAction - No actor attached to minion.");
			Finish();
		}
		else
		{
			ZoneMgr.Get().FindZonesOfType<ZonePlay>(Player.Side.FRIENDLY).FirstOrDefault()
				.SortWithSpotForLettuceAbilityCard(-1);
			Object.Destroy(m_FakeMinionGameObject.Value);
			Finish();
		}
	}
}
