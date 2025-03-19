using UnityEngine;

public class BigCardTooltipDisplayBones : MonoBehaviour
{
	public GameObject_MobileOverride m_BoneRigs;

	public TooltipBoneLayout GetRigForCurrentPlatform()
	{
		if (m_BoneRigs == null)
		{
			return null;
		}
		GameObject correctRig = m_BoneRigs.GetValueForScreen(PlatformSettings.Screen, null);
		if (correctRig == null)
		{
			return null;
		}
		TooltipBoneLayout layout = correctRig.GetComponent<TooltipBoneLayout>();
		if (layout == null)
		{
			return null;
		}
		return layout;
	}

	public bool HasBonesForCurrentPlatform()
	{
		TooltipBoneLayout correctRig = GetRigForCurrentPlatform();
		if (correctRig == null)
		{
			return false;
		}
		return correctRig.HasAllBones();
	}
}
