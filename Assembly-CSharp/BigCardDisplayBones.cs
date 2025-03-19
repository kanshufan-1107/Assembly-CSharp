using UnityEngine;

public class BigCardDisplayBones : MonoBehaviour
{
	public GameObject_MobileOverride m_BoneRigs;

	public void GetRigForCurrentPlatform(out GameObject rig, out BigCardBoneLayout.ScaleSettings scale)
	{
		if (m_BoneRigs == null)
		{
			rig = null;
			scale = null;
			return;
		}
		GameObject correctRig = m_BoneRigs.GetValueForScreen(PlatformSettings.Screen, null);
		if (correctRig == null)
		{
			rig = null;
			scale = null;
			return;
		}
		BigCardBoneLayout layout = correctRig.GetComponent<BigCardBoneLayout>();
		if (layout == null || !layout.HasAllBones())
		{
			rig = null;
			scale = null;
		}
		else
		{
			rig = correctRig;
			scale = layout.m_scaleSettings;
		}
	}

	public bool HasBonesForCurrentPlatform()
	{
		GetRigForCurrentPlatform(out var correctRig, out var _);
		if (correctRig == null)
		{
			return false;
		}
		BigCardBoneLayout boneLayout = correctRig.GetComponent<BigCardBoneLayout>();
		if (boneLayout == null)
		{
			return false;
		}
		return boneLayout.HasAllBones();
	}
}
