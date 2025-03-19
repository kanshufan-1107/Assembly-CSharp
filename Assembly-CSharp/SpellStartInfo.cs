using System;
using Blizzard.T5.Game.Spells;
using Blizzard.T5.Game.Spells.SuperSpells;
using UnityEngine;

[Serializable]
public class SpellStartInfo : ISpellStartInfo
{
	[SerializeField]
	private bool m_Enabled = true;

	[SerializeField]
	private Spell m_Prefab;

	[SerializeField]
	private bool m_UseSuperSpellLocation = true;

	[SerializeField]
	private bool m_DeathAfterAllMissilesFire = true;

	[SerializeField]
	private bool m_AdjustRotation;

	[SerializeField]
	private Vector3 m_StartRotationAdjustment = Vector3.zero;

	public bool Enabled => m_Enabled;

	public ISpell Prefab => m_Prefab;

	public bool UseSuperSpellLocation => m_UseSuperSpellLocation;

	public bool DeathAfterAllMissilesFire => m_DeathAfterAllMissilesFire;

	public bool AdjustRotation
	{
		get
		{
			return m_AdjustRotation;
		}
		set
		{
			m_AdjustRotation = value;
		}
	}

	public Vector3 StartRotationAdjustment => m_StartRotationAdjustment;
}
