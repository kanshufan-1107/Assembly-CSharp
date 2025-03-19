using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[Tooltip("Shake Minions")]
[ActionCategory("Pegasus")]
public class ShakeMinionsAction : FsmStateAction
{
	public enum MinionsToShakeEnum
	{
		All,
		Target,
		SelectedGameObject
	}

	[Tooltip("Impact Object Location")]
	[RequiredField]
	public FsmOwnerDefault gameObject;

	[Tooltip("Shake Type")]
	[RequiredField]
	public ShakeMinionType shakeType = ShakeMinionType.RandomDirection;

	[Tooltip("Minions To Shake")]
	[RequiredField]
	public MinionsToShakeEnum MinionsToShake;

	[RequiredField]
	[Tooltip("Shake Intensity")]
	public ShakeMinionIntensity shakeSize = ShakeMinionIntensity.SmallShake;

	[RequiredField]
	[Tooltip("Custom Shake Intensity 0-1. Used when Shake Size is Custom")]
	public FsmFloat customShakeIntensity;

	[RequiredField]
	[Tooltip("Radius - 0 = for all objects")]
	public FsmFloat radius;

	public override void Reset()
	{
		gameObject = null;
		MinionsToShake = MinionsToShakeEnum.All;
		shakeType = ShakeMinionType.RandomDirection;
		shakeSize = ShakeMinionIntensity.SmallShake;
		customShakeIntensity = 0.1f;
		radius = 0f;
	}

	public override void OnEnter()
	{
		DoShakeMinions();
		Finish();
	}

	private void DoShakeMinions()
	{
		GameObject go = base.Fsm.GetOwnerDefaultTarget(gameObject);
		if (go == null)
		{
			Finish();
		}
		else if (MinionsToShake == MinionsToShakeEnum.All)
		{
			MinionShake.ShakeAllMinions(go, shakeType, go.transform.position, shakeSize, customShakeIntensity.Value, radius.Value, 0f);
		}
		else if (MinionsToShake == MinionsToShakeEnum.Target)
		{
			MinionShake.ShakeTargetMinion(go, shakeType, go.transform.position, shakeSize, customShakeIntensity.Value, 0f, 0f);
		}
		else if (MinionsToShake == MinionsToShakeEnum.SelectedGameObject)
		{
			MinionShake.ShakeObject(go, shakeType, go.transform.position, shakeSize, customShakeIntensity.Value, 0f, 0f);
		}
	}
}
