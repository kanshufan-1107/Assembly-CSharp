using System;
using Blizzard.T5.Game.Spells;
using UnityEngine;

[Serializable]
public class SpellValueRange : ISpellValueRange
{
	[SerializeField]
	private ValueRange m_range;

	[SerializeField]
	private SpellBase m_spellPrefab;

	public ValueRange Range => m_range;

	public ISpell SpellPrefab => m_spellPrefab;
}
