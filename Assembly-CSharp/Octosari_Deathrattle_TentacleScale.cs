using UnityEngine;

public class Octosari_Deathrattle_TentacleScale : MonoBehaviour
{
	public GameObject[] bones;

	public AnimationCurve boneAimingWeights;

	public Vector3 boneStretchingMul = Vector3.up;

	public AnimationCurve boneStretchingWeights;

	public AnimationCurve stretchingByTargetDistance;

	public AnimationCurve deformAnimation;

	public Animation animComponent;

	public Transform tentacleTarget;

	private AnimationState animState;

	private int bonesCount;

	private float boneWeightSampler;

	private float stretchingByDistanceMul;

	private void Start()
	{
		string animName = animComponent.clip.name;
		animState = animComponent[animName];
		bonesCount = bones.Length;
		boneWeightSampler = ((bonesCount < 2) ? 1 : (bonesCount - 1));
	}

	private void LateUpdate()
	{
		if (!(animState == null) && animComponent.isPlaying && animState.time != 0f && stretchingByDistanceMul != 0f)
		{
			float currentNormalizedTime = animState.normalizedTime;
			float deformFactor = deformAnimation.Evaluate(currentNormalizedTime);
			int boneNum = 0;
			GameObject[] array = bones;
			foreach (GameObject bone in array)
			{
				float boneStratchingWeight = boneStretchingWeights.Evaluate((float)boneNum / boneWeightSampler);
				float boneAimingWeight = boneAimingWeights.Evaluate((float)boneNum / boneWeightSampler);
				Transform boneTransform = bone.transform;
				Vector3 boneLocalPos = boneTransform.localPosition;
				boneLocalPos += Vector3.Scale(boneLocalPos, boneStratchingWeight * deformFactor * boneStretchingMul * stretchingByDistanceMul);
				bone.transform.localPosition = boneLocalPos;
				Vector3 boneAimVec = boneTransform.up;
				Vector3 targetAimVec = tentacleTarget.position - boneTransform.position;
				Quaternion rotDiff = boneTransform.rotation;
				rotDiff.SetFromToRotation(boneAimVec, targetAimVec);
				bone.transform.rotation = Quaternion.Lerp(boneTransform.rotation, rotDiff * bone.transform.rotation, deformFactor * boneAimingWeight);
				boneNum++;
			}
		}
	}

	public void Setup()
	{
		float targetDistance = Vector3.Distance(tentacleTarget.position, base.transform.position);
		stretchingByDistanceMul = stretchingByTargetDistance.Evaluate(targetDistance);
	}
}
