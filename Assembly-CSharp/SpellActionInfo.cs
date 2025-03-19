using System;
using Blizzard.T5.Game.Spells;
using Blizzard.T5.Game.Spells.SuperSpells;
using UnityEngine;

[Serializable]
public class SpellActionInfo : ISpellActionInfo
{
	[SerializeField]
	private SpellVisualShowTime m_ShowSpellVisuals;

	[SerializeField]
	private float m_StartDelayMin;

	[SerializeField]
	private float m_StartDelayMax;

	public SpellVisualShowTime ShowSpellVisuals => m_ShowSpellVisuals;

	public float StartDelayMin => m_StartDelayMin;

	public float StartDelayMax => m_StartDelayMax;
}
