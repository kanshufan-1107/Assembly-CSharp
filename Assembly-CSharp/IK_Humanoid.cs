using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IK_Humanoid : MonoBehaviour
{
	[Serializable]
	public class ChainData
	{
		public enum ChainTag
		{
			Untagged,
			LookAt,
			Spine,
			Limb
		}

		public ChainTag chainTag;

		public IK_Limb chain01;

		public IK_LookAt chain02;

		public Vector3 offset;

		public bool visible;

		public bool advanced;

		public IK_Chain Chain
		{
			get
			{
				if (chainTag == ChainTag.LookAt)
				{
					return chain02;
				}
				return chain01;
			}
		}
	}

	public List<ChainData> chainData = new List<ChainData>();

	private void Awake()
	{
		foreach (ChainData chainDatum in chainData)
		{
			chainDatum.Chain.Initialize();
		}
	}

	public IK_LookAt AddLookAt()
	{
		return new IK_LookAt
		{
			globalWeight = 1f,
			chainWeight = 1f,
			leafWeight = 1f,
			chainLength = 4,
			decay = new Vector2(0.1f, 0.25f)
		};
	}

	public IK_Limb AddLimb()
	{
		return new IK_Limb
		{
			globalWeight = 1f,
			chainWeight = 1f,
			leafWeight = 1f,
			chainLength = 2,
			snapBack = 0.5f,
			delta = 0.1f,
			iterations = 10
		};
	}

	public bool GetChainByTag(ChainData.ChainTag tag, out ChainData chain)
	{
		chain = chainData.FirstOrDefault((ChainData x) => x.chainTag == tag);
		return chain != null;
	}

	public List<ChainData> GetLimbs()
	{
		return new List<ChainData>(chainData.Where((ChainData x) => x.chainTag == ChainData.ChainTag.Limb));
	}

	public List<ChainData> GetIKChains()
	{
		return new List<ChainData>(chainData.Where((ChainData x) => x.chainTag != ChainData.ChainTag.LookAt));
	}

	private void LateUpdate()
	{
		if (GetChainByTag(ChainData.ChainTag.Spine, out var spine))
		{
			spine.Chain.Resolve();
		}
		if (GetChainByTag(ChainData.ChainTag.LookAt, out var lookAt))
		{
			lookAt.Chain.Resolve();
		}
		foreach (ChainData limb in GetLimbs())
		{
			limb.Chain.Resolve();
		}
	}
}
