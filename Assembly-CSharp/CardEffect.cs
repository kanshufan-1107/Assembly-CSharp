using System.Collections.Generic;

public class CardEffect
{
	private Spell m_spell;

	private List<CardSoundSpell> m_soundSpells;

	private string m_spellPath;

	private List<string> m_soundSpellPaths;

	private Card m_owner;

	public CardEffect(CardEffectDef def, Card owner)
	{
		m_spellPath = def.m_SpellPath;
		m_soundSpellPaths = def.m_SoundSpellPaths;
		m_owner = owner;
		if (m_soundSpellPaths != null)
		{
			m_soundSpells = new List<CardSoundSpell>(m_soundSpellPaths.Count);
			for (int i = 0; i < m_soundSpellPaths.Count; i++)
			{
				m_soundSpells.Add(null);
			}
		}
	}

	public CardEffect(string spellPath, Card owner)
	{
		m_spellPath = spellPath;
		m_owner = owner;
	}

	public Spell GetSpell(bool loadIfNeeded = true)
	{
		if (m_spell == null && !string.IsNullOrEmpty(m_spellPath) && loadIfNeeded)
		{
			m_spell = SpellUtils.LoadAndSetupSpell(m_spellPath, m_owner);
		}
		return m_spell;
	}

	public Spell LoadSpell()
	{
		if (!string.IsNullOrEmpty(m_spellPath))
		{
			m_spell = SpellUtils.LoadAndSetupSpell(m_spellPath, m_owner);
		}
		return m_spell;
	}

	public void LoadSoundSpell(int index)
	{
		if (index >= 0 && m_soundSpellPaths != null && index < m_soundSpellPaths.Count && !string.IsNullOrEmpty(m_soundSpellPaths[index]) && m_soundSpells[index] == null)
		{
			string path = m_soundSpellPaths[index];
			CardSoundSpell spell = SpellManager.Get().GetSpell(path) as CardSoundSpell;
			m_soundSpells[index] = spell;
			if (spell == null)
			{
				Error.AddDevFatal("CardEffect.LoadSoundSpell() - FAILED TO LOAD \"{0}\" (PATH: \"{1}\") (index {2})", m_spellPath, path, index);
			}
			else if (m_owner != null)
			{
				SpellUtils.SetupSoundSpell(spell, m_owner);
			}
		}
	}

	public List<CardSoundSpell> GetSoundSpells(bool loadIfNeeded = true)
	{
		if (m_soundSpells == null)
		{
			return null;
		}
		if (loadIfNeeded)
		{
			for (int i = 0; i < m_soundSpells.Count; i++)
			{
				LoadSoundSpell(i);
			}
		}
		return m_soundSpells;
	}

	public void Clear()
	{
		SpellManager spellManager = SpellManager.Get();
		if (spellManager == null)
		{
			return;
		}
		if (m_spell != null)
		{
			spellManager.ReleaseSpell(m_spell);
		}
		if (m_soundSpells == null)
		{
			return;
		}
		for (int i = 0; i < m_soundSpells.Count; i++)
		{
			Spell spell = m_soundSpells[i];
			if (spell != null)
			{
				spellManager.ReleaseSpell(spell);
			}
		}
	}

	public void LoadAll()
	{
		GetSpell();
		if (m_soundSpellPaths != null)
		{
			for (int i = 0; i < m_soundSpellPaths.Count; i++)
			{
				LoadSoundSpell(i);
			}
		}
	}

	public void PurgeSpells()
	{
		SpellUtils.PurgeSpell(m_spell);
		SpellUtils.PurgeSpells(m_soundSpells);
	}
}
