using UnityEngine;

public class TooltipBoneLayout : MonoBehaviour
{
	public GameObject m_topLeftTooltipBone;

	public GameObject m_bottomLeftTooltipBone;

	public GameObject m_topRightTooltipBone;

	public GameObject m_bottomRightTooltipBone;

	public GameObject m_bottomMiddleTooltipBone;

	[Range(-1f, 1f)]
	public float m_manualHorizontalAdjustment;

	[Range(-1f, 1f)]
	public float m_manualVerticalAdjustment;

	private void Start()
	{
		if (!HasAllBones())
		{
			AttemptRepairBoneReferences();
		}
	}

	private void AttemptRepairBoneReferences()
	{
		Transform[] childBones = GetComponentsInChildren<Transform>();
		if (childBones != null && childBones.Length != 0)
		{
			AttemptRepairSingleBoneReference(ref m_topLeftTooltipBone, "UpperLeft", childBones);
			AttemptRepairSingleBoneReference(ref m_bottomLeftTooltipBone, "LowerLeft", childBones);
			AttemptRepairSingleBoneReference(ref m_topRightTooltipBone, "UpperRight", childBones);
			AttemptRepairSingleBoneReference(ref m_bottomRightTooltipBone, "LowerRight", childBones);
			AttemptRepairSingleBoneReference(ref m_bottomMiddleTooltipBone, "BottomMiddle", childBones);
		}
	}

	private void AttemptRepairSingleBoneReference(ref GameObject missingBone, string expectedName, Transform[] childBones)
	{
		if (!(missingBone != null))
		{
			SearchForBone(ref missingBone, expectedName, childBones);
			string parentObjectName = FindParentObjectName();
			if (missingBone == null)
			{
				Log.MissingAssets.PrintError("Error in TooltipBoneLayout: missing bone with expected name " + expectedName + " and we could not recover. Parent object is \"" + parentObjectName + "\".");
			}
			else
			{
				Log.MissingAssets.PrintWarning("Error in TooltipBoneLayout: missing bone with expected name " + expectedName + " but we were able to recover. Parent object is \"" + parentObjectName + "\".");
			}
		}
	}

	private void SearchForBone(ref GameObject missingBone, string expectedName, Transform[] childBones)
	{
		if (missingBone != null || string.IsNullOrEmpty(expectedName) || childBones == null)
		{
			return;
		}
		foreach (Transform childBone in childBones)
		{
			if (childBone.gameObject.name == expectedName)
			{
				missingBone = childBone.gameObject;
				break;
			}
		}
	}

	private string FindParentObjectName()
	{
		Actor parentActor = GetComponentInParent<Actor>();
		if (parentActor == null)
		{
			return "Unknown prefab";
		}
		return parentActor.gameObject.name;
	}

	public bool HasAllBones()
	{
		if (m_topLeftTooltipBone != null && m_bottomLeftTooltipBone != null && m_topRightTooltipBone != null && m_bottomRightTooltipBone != null)
		{
			return m_bottomMiddleTooltipBone != null;
		}
		return false;
	}
}
