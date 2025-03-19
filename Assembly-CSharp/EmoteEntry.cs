using UnityEngine;

public class EmoteEntry
{
	private EmoteType m_emoteType;

	private Spell m_emoteSpell;

	private CardSoundSpell m_emoteSoundSpell;

	private string m_emoteGameStringKey;

	private string m_emoteSpellPath;

	private string m_emoteSoundSpellPath;

	private Component m_owner;

	public EmoteEntry(EmoteType type, string spellPath, string soundSpellPath, string stringKey, Card owner)
	{
		m_emoteType = type;
		m_emoteSpellPath = spellPath;
		m_emoteSoundSpellPath = soundSpellPath;
		m_emoteGameStringKey = stringKey;
		m_owner = owner;
	}

	public EmoteEntry(EmoteType type, string spellPath, string soundSpellPath, string stringKey, Actor owner)
	{
		m_emoteType = type;
		m_emoteSpellPath = spellPath;
		m_emoteSoundSpellPath = soundSpellPath;
		m_emoteGameStringKey = stringKey;
		m_owner = owner;
	}

	public EmoteType GetEmoteType()
	{
		return m_emoteType;
	}

	public string GetGameStringKey()
	{
		if (m_emoteSoundSpell != null)
		{
			string dynamicStringKey = m_emoteSoundSpell.DetermineGameStringKey();
			if (!string.IsNullOrEmpty(dynamicStringKey))
			{
				m_emoteGameStringKey = dynamicStringKey;
			}
		}
		return m_emoteGameStringKey;
	}

	private void LoadSoundSpell()
	{
		if (!string.IsNullOrEmpty(m_emoteSoundSpellPath))
		{
			m_emoteSoundSpell = SpellManager.Get().GetSpell(m_emoteSoundSpellPath) as CardSoundSpell;
			if (m_emoteSoundSpell == null)
			{
				Error.AddDevFatalUnlessWorkarounds("EmoteEntry.LoadSoundSpell() - \"{0}\" does not have a Spell component.", m_emoteSoundSpellPath);
			}
			else if (m_owner != null)
			{
				SpellUtils.SetupSoundSpell(m_emoteSoundSpell, m_owner);
			}
		}
	}

	public CardSoundSpell GetSoundSpell(bool loadIfNeeded = true)
	{
		if (m_emoteSoundSpell == null && loadIfNeeded)
		{
			LoadSoundSpell();
		}
		return m_emoteSoundSpell;
	}

	public Spell GetSpell(bool loadIfNeeded = true)
	{
		if (m_emoteSpell == null && loadIfNeeded && !string.IsNullOrEmpty(m_emoteSpellPath))
		{
			m_emoteSpell = SpellUtils.LoadAndSetupSpell(m_emoteSpellPath, m_owner);
		}
		return m_emoteSpell;
	}

	public void Clear()
	{
		SpellManager spellManager = SpellManager.Get();
		if (spellManager != null)
		{
			if (m_emoteSoundSpell != null)
			{
				spellManager.ReleaseSpell(m_emoteSoundSpell);
				m_emoteSoundSpell = null;
			}
			if (m_emoteSpell != null)
			{
				spellManager.ReleaseSpell(m_emoteSpell);
				m_emoteSpell = null;
			}
		}
	}
}
