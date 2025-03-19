using System.Collections.Generic;
using UnityEngine;

namespace Hearthstone.Tendril;

public class TendrilUtilities
{
	public static List<Transform> CollectBones(Transform leafBone, int chainLength, out Transform root)
	{
		root = leafBone;
		List<Transform> bones = new List<Transform>();
		for (int i = 0; i < chainLength + 1; i++)
		{
			if (!(root.parent != null))
			{
				break;
			}
			bones.Add(root);
			root = root.parent;
		}
		bones.Reverse();
		return bones;
	}

	public static Vector3 Target(List<Transform> bones, int index)
	{
		if (index < bones.Count - 1)
		{
			return bones[index + 1].position;
		}
		float distance = Vector3.Distance(bones[index].position, bones[index - 1].position);
		return bones[index].position + Direction(bones, index) * distance;
	}

	public static Vector3 Direction(List<Transform> bones, int index)
	{
		if (index < bones.Count - 1)
		{
			return (bones[index + 1].position - bones[index].position).normalized;
		}
		return Quaternion.Inverse(bones[index - 1].rotation * Quaternion.Inverse(bones[index].rotation)) * (bones[index].position - bones[index - 1].position).normalized;
	}
}
