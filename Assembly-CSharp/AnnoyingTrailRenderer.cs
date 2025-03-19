using System;
using UnityEngine;

public class AnnoyingTrailRenderer : MonoBehaviour
{
	public GameObject GameObjectWithLineRenderer;

	public LineRenderer TargetLineRenderer;

	public int NumDivisions = 50;

	public float TrailMaxTimeLag = 0.5f;

	public GameObject GameObjectWithAnimator;

	public Animator _runningAnimator;

	public bool MoveToAnimParent;

	public AnimationClip SourceAnimationClip;

	public GameObject NodeAvatarParent;

	public GameObject NodeAvatar;

	private float _animTime;

	private Vector3[] _nodeArray;

	public bool ReverseNodeArray;

	public bool OutputWorldSpace = true;

	private float _cutoffTime;

	private void OnEnable()
	{
		if ((bool)GameObjectWithAnimator)
		{
			_runningAnimator = GameObjectWithAnimator.GetComponent<Animator>();
		}
		if (!TargetLineRenderer)
		{
			TargetLineRenderer = GameObjectWithLineRenderer.GetComponent<LineRenderer>();
		}
		TargetLineRenderer.sortingOrder = 1000;
		if (MoveToAnimParent)
		{
			base.gameObject.transform.parent = GameObjectWithAnimator.transform.parent.transform;
			base.gameObject.transform.localPosition = GameObjectWithAnimator.transform.localPosition;
			base.gameObject.transform.localScale = GameObjectWithAnimator.transform.localScale;
			base.gameObject.transform.rotation = GameObjectWithAnimator.transform.rotation;
		}
	}

	private void Update()
	{
		if ((bool)_runningAnimator)
		{
			_animTime = GetAnimationPosition(_runningAnimator);
		}
		else if (_cutoffTime == 0f)
		{
			_cutoffTime = Time.time;
		}
		_nodeArray = RefreshNodeArray(NumDivisions, SourceAnimationClip, NodeAvatarParent, NodeAvatar, _animTime, TrailMaxTimeLag, _cutoffTime);
		RestructureLine(TargetLineRenderer, _nodeArray);
	}

	private Vector3[] RefreshNodeArray(int numDivisions, AnimationClip sourceAnimClip, GameObject nodeAvatarParent, GameObject nodeAvatar, float animTime, float trailMaxTimeLag, float cutoffTime)
	{
		float sourceAnimClipLength = sourceAnimClip.length;
		float trailHeadTime = ((!_runningAnimator) ? (animTime * sourceAnimClipLength + (Time.time - cutoffTime)) : (animTime * sourceAnimClipLength));
		float trailHeadTimeClamped = Mathf.Clamp(trailHeadTime, 0f, sourceAnimClipLength);
		float trailTailTime = Mathf.Clamp(trailHeadTime - trailMaxTimeLag, 0f, sourceAnimClipLength);
		float num = trailHeadTimeClamped - trailTailTime;
		float trailLengthNormalized = num / trailMaxTimeLag;
		int numDivisionsAdjusted = Mathf.RoundToInt((float)numDivisions * trailLengthNormalized);
		float divLength = num / (float)numDivisionsAdjusted;
		Vector3[] nodeArray = new Vector3[numDivisionsAdjusted];
		for (int i = 0; i < numDivisionsAdjusted; i++)
		{
			float samplePosition = trailHeadTimeClamped - divLength * (float)i;
			sourceAnimClip.SampleAnimation(nodeAvatarParent, samplePosition);
			if (OutputWorldSpace)
			{
				nodeArray[i] = nodeAvatar.transform.position;
			}
			else
			{
				nodeArray[i] = nodeAvatar.transform.localPosition;
			}
		}
		if (ReverseNodeArray)
		{
			Array.Reverse(nodeArray);
		}
		return nodeArray;
	}

	private void RestructureLine(LineRenderer lineRenderer, Vector3[] nodeArray)
	{
		lineRenderer.positionCount = nodeArray.Length;
		lineRenderer.SetPositions(nodeArray);
	}

	private float GetAnimationPosition(Animator runningAnimator)
	{
		float animTime = 0f;
		if (runningAnimator.GetCurrentAnimatorClipInfoCount(0) > 0)
		{
			animTime = runningAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		}
		return animTime;
	}
}
