using UnityEngine;

namespace Hearthstone.Tendril;

public class TendrilLookAt : TendrilBase
{
	public enum LookDirection
	{
		Forward,
		Backward,
		Right,
		Left,
		Up,
		Down
	}

	public LookDirection lookDirection;

	public Vector2 decay = new Vector2(0.1f, 0.25f);

	private Vector3 m_targetLerp;

	protected override bool InitializeConditions()
	{
		return target != null;
	}

	protected override void InitializeTendril()
	{
		m_targetLerp = target.position;
		base.InitializeTendril();
	}

	protected override void ResolveTendril()
	{
		float lerp = speed * Time.deltaTime;
		m_targetLerp = Vector3.Lerp(m_targetLerp, target.position, lerp);
		for (int i = 0; i < bones.Count; i++)
		{
			Vector3 targetDirection = bones[i].InverseTransformDirection((m_targetLerp - bones[i].position).normalized);
			Quaternion lookRotation = Quaternion.FromToRotation(bones[i].InverseTransformDirection(Direction(bones[i])), targetDirection);
			if (i < bones.Count - 1)
			{
				float falloff = Mathf.Lerp(decay.x, decay.y, Mathf.InverseLerp(chainLength, i, 0f));
				falloff = Mathf.Clamp(falloff * weight * controlWeight, 0f, 1f);
				bones[i].localRotation = Quaternion.Lerp(bones[i].localRotation, bones[i].localRotation * lookRotation, falloff);
			}
			else
			{
				bones[i].localRotation = Quaternion.Lerp(bones[i].localRotation, bones[i].localRotation * lookRotation, leafWeight * weight * controlWeight);
			}
		}
		base.ResolveTendril();
	}

	private Vector3 Direction(Transform bone)
	{
		return lookDirection switch
		{
			LookDirection.Forward => bone.forward, 
			LookDirection.Backward => -bone.forward, 
			LookDirection.Right => bone.right, 
			LookDirection.Left => -bone.right, 
			LookDirection.Up => bone.up, 
			_ => -bone.up, 
		};
	}

	protected override void TendrilGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(target.position, 0.25f);
		base.TendrilGizmos();
	}
}
