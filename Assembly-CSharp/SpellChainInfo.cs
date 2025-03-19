using System;
using Blizzard.T5.Game.Spells;
using UnityEngine;

[Serializable]
public class SpellChainInfo
{
	[SerializeField]
	private bool m_Enabled = true;

	[SerializeField]
	private Spell m_Prefab;

	[SerializeField]
	private float m_SpawnDelayMin;

	[SerializeField]
	private float m_SpawnDelayMax;

	public bool Enabled => m_Enabled;

	public ISpell Prefab => m_Prefab;

	public float SpawnDelayMin => m_SpawnDelayMin;

	public float SpawnDelayMax => m_SpawnDelayMax;
}
