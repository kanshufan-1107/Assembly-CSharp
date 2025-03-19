using System;
using Blizzard.T5.Game.Spells;
using UnityEngine;

[Serializable]
public class SpellTargetInfo : ISpellTargetInfo
{
	[SerializeField]
	private SpellTargetBehavior m_Behavior;

	[SerializeField]
	private int m_RandomTargetCountMin = 8;

	[SerializeField]
	private int m_RandomTargetCountMax = 10;

	[SerializeField]
	private bool m_SuppressPlaySounds;

	public SpellTargetBehavior Behavior
	{
		get
		{
			return m_Behavior;
		}
		set
		{
			m_Behavior = value;
		}
	}

	public int RandomTargetCountMin
	{
		get
		{
			return m_RandomTargetCountMin;
		}
		set
		{
			m_RandomTargetCountMin = value;
		}
	}

	public int RandomTargetCountMax
	{
		get
		{
			return m_RandomTargetCountMax;
		}
		set
		{
			m_RandomTargetCountMax = value;
		}
	}

	public bool SuppressPlaySounds => m_SuppressPlaySounds;
}
