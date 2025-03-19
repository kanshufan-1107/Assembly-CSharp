using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class IK_Chain
{
	public Transform leafBone;

	public Transform chainTarget;

	public Transform chainPole;

	public float globalWeight;

	public float chainWeight;

	public float leafWeight;

	public int chainLength;

	public List<Transform> m_bones;

	protected List<Quaternion> m_startRotationBone = new List<Quaternion>();

	protected Transform m_root;

	protected bool m_initialized;

	public virtual void Initialize()
	{
		if (!(leafBone == null) && !(chainTarget == null))
		{
			m_bones = CollectBones(out var root);
			m_root = root;
			InitializeIK();
		}
	}

	protected virtual void InitializeIK()
	{
	}

	public virtual void Resolve()
	{
		if (m_initialized)
		{
			ResolveIK();
		}
	}

	protected virtual void ResolveIK()
	{
	}

	protected List<Transform> CollectBones(out Transform root)
	{
		root = leafBone;
		List<Transform> bones = new List<Transform>();
		for (int i = 0; i < chainLength + 1; i++)
		{
			if (root.parent != null)
			{
				bones.Add(root);
				root = root.parent;
			}
			else
			{
				chainLength = bones.Count;
			}
		}
		return bones;
	}
}
