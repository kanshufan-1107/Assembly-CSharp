using System;
using Blizzard.T5.Game.Spells;
using Blizzard.T5.Game.Spells.SuperSpells;
using UnityEngine;

[Serializable]
public class SpellImpactInfo : ISpellImpactInfo
{
	[SerializeField]
	private bool m_Enabled = true;

	[Tooltip("This spell will be chosen by default if the card deals no damage.")]
	[SerializeField]
	private Spell m_Prefab;

	[SerializeField]
	[Tooltip("If the card deals damage, the spell in the appropriate damage range will be chosen. If the damage exceeds all ranges, we pick the one with the highest maximum range. If the damage number is not within any specified range, we will use the default spell (see above)")]
	private SpellValueRange[] m_damageAmountImpactSpells;

	[SerializeField]
	private bool m_UseSuperSpellLocation;

	[SerializeField]
	private SpellLocation m_Location = SpellLocation.TARGET;

	[SerializeField]
	private bool m_SetParentToLocation;

	[SerializeField]
	private float m_SpawnDelaySecMin;

	[SerializeField]
	private float m_SpawnDelaySecMax;

	[SerializeField]
	private float m_SpawnOffset;

	[SerializeField]
	private float m_GameDelaySecMin = 0.5f;

	[SerializeField]
	private float m_GameDelaySecMax = 0.5f;

	[SerializeField]
	private bool m_AdjustRotation;

	[SerializeField]
	private Vector3 m_ImpactRotationAdjustment = Vector3.zero;

	public bool Enabled => m_Enabled;

	public ISpell Prefab => m_Prefab;

	public ISpellValueRange[] DamageAmountImpactSpells => m_damageAmountImpactSpells;

	public bool UseSuperSpellLocation => m_UseSuperSpellLocation;

	public SpellLocation Location => m_Location;

	public bool SetParentToLocation => m_SetParentToLocation;

	public float SpawnDelaySecMin => m_SpawnDelaySecMin;

	public float SpawnDelaySecMax => m_SpawnDelaySecMax;

	public float SpawnOffset => m_SpawnOffset;

	public float GameDelaySecMin => m_GameDelaySecMin;

	public float GameDelaySecMax => m_GameDelaySecMax;

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

	public Vector3 ImpactRotationAdjustment => m_ImpactRotationAdjustment;
}
