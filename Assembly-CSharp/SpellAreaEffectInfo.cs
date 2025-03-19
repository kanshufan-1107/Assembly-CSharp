using System;
using Blizzard.T5.Game.Spells;
using Blizzard.T5.Game.Spells.SuperSpells;
using UnityEngine;

[Serializable]
public class SpellAreaEffectInfo : ISpellAreaEffectInfo
{
	[SerializeField]
	private bool m_Enabled = true;

	[SerializeField]
	private Spell m_Prefab;

	[SerializeField]
	private bool m_UseSuperSpellLocation;

	[SerializeField]
	private SpellLocation m_Location;

	[SerializeField]
	private bool m_SetParentToLocation;

	[SerializeField]
	private SpellFacing m_Facing;

	[SerializeField]
	private SpellFacingOptions m_FacingOptions;

	[SerializeField]
	private float m_SpawnDelaySecMin;

	[SerializeField]
	private float m_SpawnDelaySecMax;

	public bool Enabled => m_Enabled;

	public ISpell Prefab => m_Prefab;

	public bool UseSuperSpellLocation => m_UseSuperSpellLocation;

	public SpellLocation Location => m_Location;

	public bool SetParentToLocation => m_SetParentToLocation;

	public SpellFacing Facing => m_Facing;

	public SpellFacingOptions FacingOptions => m_FacingOptions;

	public float SpawnDelaySecMin => m_SpawnDelaySecMin;

	public float SpawnDelaySecMax => m_SpawnDelaySecMax;
}
