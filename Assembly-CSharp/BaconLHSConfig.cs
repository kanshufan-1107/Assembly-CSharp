using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BaconLHSConfig", menuName = "ScriptableObjects/BaconLHSConfig", order = 2)]
[CustomEditClass]
public class BaconLHSConfig : ScriptableObject
{
	[Serializable]
	public class ValueVFXDef
	{
		public int m_value;

		public bool m_onlyExactMatch = true;

		[CustomEditField(T = EditType.SPELL)]
		public string m_vfxAsset;
	}

	public class ValueVFX
	{
		public int m_value;

		public bool m_onlyExactMatch = true;

		public Spell m_vfxSpell;

		public ValueVFX(ValueVFXDef def)
		{
			m_value = def.m_value;
			m_onlyExactMatch = def.m_onlyExactMatch;
		}
	}

	[Serializable]
	public class ValueLine
	{
		public int m_value;

		public bool m_onlyExactMatch = true;

		[CustomEditField(T = EditType.SOUND_PREFAB)]
		public string m_VOLine;
	}

	[Serializable]
	public class CardSpecificLine
	{
		public string m_cardId;

		[CustomEditField(T = EditType.SOUND_PREFAB)]
		public string m_VOLine;
	}

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public string m_VOPicked;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public string m_VOStartOfGame;

	[CustomEditField(Sections = "VoiceOver")]
	public List<CardSpecificLine> m_VOBartenderGreet;

	[CustomEditField(Sections = "VoiceOver")]
	public List<ValueLine> m_VOWinStreak;

	[CustomEditField(Sections = "VoiceOver")]
	public List<CardSpecificLine> m_VOHeroGreet;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public List<string> m_VOGreet;

	[CustomEditField(Sections = "VoiceOver", T = EditType.SOUND_PREFAB)]
	public List<string> m_VOKnockout;

	[CustomEditField(Sections = "VFX", T = EditType.SPELL)]
	public string m_VFXSocketInDef;

	[CustomEditField(Sections = "VFX", T = EditType.SPELL)]
	public string m_VFXCombatStartDef;

	public List<ValueVFXDef> m_VFXWinStreakDef;

	private Card m_heroCard;

	private Spell m_VFXSocketIn;

	private Spell m_VFXCombatStart;

	private List<ValueVFX> m_VFXWinStreak;

	private Dictionary<string, List<string>> m_VOBartenderGreetDict;

	private Dictionary<string, List<string>> m_VOHeroGreetDict;

	public void InitAllAssets(Card heroCard)
	{
		m_heroCard = heroCard;
		PreLoadAllVFX();
		PreLoadAllVO();
		ConfigureAllVO();
	}

	public void InitCombatAssets(Card heroCard)
	{
		m_heroCard = heroCard;
		PreLoadCombatVFX();
		PreLoadCombatVO();
		ConfigureCombatVO();
	}

	private void ConfigureAllVO()
	{
		ConfigureCombatVO();
		if (m_VOBartenderGreet == null || m_VOBartenderGreet.Count == 0)
		{
			return;
		}
		m_VOBartenderGreetDict = new Dictionary<string, List<string>>();
		foreach (CardSpecificLine line in m_VOBartenderGreet)
		{
			if (!string.IsNullOrEmpty(line.m_cardId) && !string.IsNullOrEmpty(line.m_VOLine))
			{
				if (!m_VOBartenderGreetDict.ContainsKey(line.m_cardId))
				{
					m_VOBartenderGreetDict.Add(line.m_cardId, new List<string>());
				}
				m_VOBartenderGreetDict[line.m_cardId]?.Add(line.m_VOLine);
			}
		}
	}

	private void ConfigureCombatVO()
	{
		if (m_VOHeroGreet == null || m_VOHeroGreet.Count == 0)
		{
			return;
		}
		m_VOHeroGreetDict = new Dictionary<string, List<string>>();
		foreach (CardSpecificLine line in m_VOHeroGreet)
		{
			if (!string.IsNullOrEmpty(line.m_cardId) && !string.IsNullOrEmpty(line.m_VOLine))
			{
				if (!m_VOHeroGreetDict.ContainsKey(line.m_cardId))
				{
					m_VOHeroGreetDict.Add(line.m_cardId, new List<string>());
				}
				m_VOHeroGreetDict[line.m_cardId]?.Add(line.m_VOLine);
			}
		}
	}

	private void PreLoadAllVO()
	{
		if (GameState.Get() == null || GameState.Get().GetGameEntity() == null)
		{
			return;
		}
		foreach (string soundFile in GetAllVOLines())
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	private void PreLoadCombatVO()
	{
		if (GameState.Get() == null || GameState.Get().GetGameEntity() == null)
		{
			return;
		}
		foreach (string soundFile in GetCombatVOLines())
		{
			GameState.Get().GetGameEntity().PreloadSound(soundFile);
		}
	}

	public List<string> GetAllVOLines()
	{
		List<string> ret = GetCombatVOLines();
		Action<string> addIfNotNull = delegate(string x)
		{
			if (!string.IsNullOrEmpty(x))
			{
				ret.Add(x);
			}
		};
		foreach (ValueLine line in m_VOWinStreak)
		{
			addIfNotNull(line.m_VOLine);
		}
		foreach (CardSpecificLine line2 in m_VOBartenderGreet)
		{
			addIfNotNull(line2.m_VOLine);
		}
		addIfNotNull(m_VOStartOfGame);
		return ret;
	}

	private List<string> GetCombatVOLines()
	{
		List<string> ret = new List<string>();
		Action<string> addIfNotNull = delegate(string x)
		{
			if (!string.IsNullOrEmpty(x))
			{
				ret.Add(x);
			}
		};
		Action<List<string>> addRangeIfNotNull = delegate(List<string> x)
		{
			if (x != null)
			{
				ret.AddRange(x);
			}
		};
		foreach (CardSpecificLine line in m_VOHeroGreet)
		{
			addIfNotNull(line.m_VOLine);
		}
		addRangeIfNotNull(m_VOGreet);
		addRangeIfNotNull(m_VOKnockout);
		return ret;
	}

	private void PreLoadAllVFX()
	{
		PreLoadCombatVFX();
		if (!string.IsNullOrEmpty(m_VFXSocketInDef))
		{
			AssetLoader.Get().InstantiatePrefab(m_VFXSocketInDef, OnSpellLoaded_SocketIn);
		}
		m_VFXWinStreak = new List<ValueVFX>();
		foreach (ValueVFXDef def in m_VFXWinStreakDef)
		{
			if (!string.IsNullOrEmpty(def.m_vfxAsset))
			{
				AssetLoader.Get().InstantiatePrefab(def.m_vfxAsset, OnSpellLoaded_WinStreak, def);
			}
		}
	}

	private void PreLoadCombatVFX()
	{
		if (!string.IsNullOrEmpty(m_VFXCombatStartDef))
		{
			AssetLoader.Get().InstantiatePrefab(m_VFXCombatStartDef, OnSpellLoaded_CombatStart);
		}
	}

	private void OnSpellLoaded_SocketIn(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (!(go == null))
		{
			m_VFXSocketIn = go.GetComponent<Spell>();
			SpellUtils.SetupSpell(m_VFXSocketIn, m_heroCard);
			m_VFXSocketIn.transform.parent = m_heroCard.transform;
		}
	}

	private void OnSpellLoaded_CombatStart(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (!(go == null))
		{
			m_VFXCombatStart = go.GetComponent<Spell>();
			SpellUtils.SetupSpell(m_VFXCombatStart, m_heroCard);
		}
	}

	private void OnSpellLoaded_WinStreak(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (!(go == null) && callbackData is ValueVFXDef def)
		{
			ValueVFX vfx = new ValueVFX(def);
			vfx.m_vfxSpell = go.GetComponent<Spell>();
			SpellUtils.SetupSpell(vfx.m_vfxSpell, m_heroCard);
			m_VFXWinStreak.Add(vfx);
		}
	}

	public bool TryGetAllBartenderGreet(string bartenderId, out List<string> voLines)
	{
		if (m_VOBartenderGreetDict.ContainsKey(bartenderId))
		{
			voLines = m_VOBartenderGreetDict[bartenderId];
			return true;
		}
		voLines = null;
		return false;
	}

	public bool TryGetAllHeroGreet(string heroId, out List<string> voLines)
	{
		if (m_VOHeroGreetDict.ContainsKey(heroId))
		{
			voLines = m_VOHeroGreetDict[heroId];
			return true;
		}
		voLines = null;
		return false;
	}

	public bool CheckBartenderGreetLine(string bartenderId, out string voLine)
	{
		if (m_VOBartenderGreetDict != null && m_VOBartenderGreetDict.ContainsKey(bartenderId) && m_VOBartenderGreetDict[bartenderId] != null && m_VOBartenderGreetDict[bartenderId].Count > 0)
		{
			voLine = TryGetRandomLine(m_VOBartenderGreetDict[bartenderId]);
			return !string.IsNullOrEmpty(voLine);
		}
		voLine = null;
		return false;
	}

	public bool CheckStartGameLine(out string line)
	{
		line = m_VOStartOfGame;
		return !string.IsNullOrEmpty(line);
	}

	public bool CheckWinStreakLine(int streak, out string line)
	{
		line = null;
		if (m_VOWinStreak == null || m_VOWinStreak.Count == 0)
		{
			return false;
		}
		int val = -1;
		foreach (ValueLine vo in m_VOWinStreak)
		{
			if (vo.m_onlyExactMatch)
			{
				if (vo.m_value == streak)
				{
					val = streak;
					line = vo.m_VOLine;
					break;
				}
			}
			else if (vo.m_value > val && vo.m_value <= streak)
			{
				val = vo.m_value;
				line = vo.m_VOLine;
			}
		}
		return !string.IsNullOrEmpty(line);
	}

	public bool CheckGreetLine(string heroCardId, out string line)
	{
		if (heroCardId == null)
		{
			line = "";
			return false;
		}
		if (m_VOHeroGreetDict != null && m_VOHeroGreetDict.ContainsKey(heroCardId) && m_VOHeroGreetDict[heroCardId] != null && m_VOHeroGreetDict[heroCardId].Count > 0)
		{
			line = TryGetRandomLine(m_VOHeroGreetDict[heroCardId]);
			return !string.IsNullOrEmpty(line);
		}
		line = TryGetRandomLine(m_VOGreet);
		return !string.IsNullOrEmpty(line);
	}

	public string GetPickedLine()
	{
		return m_VOPicked;
	}

	public bool CheckKnockoutLine(out string line)
	{
		line = TryGetRandomLine(m_VOKnockout);
		return !string.IsNullOrEmpty(line);
	}

	public bool TryActivateVFX_SocketIn()
	{
		if (m_VFXSocketIn == null || m_heroCard == null)
		{
			return false;
		}
		m_VFXSocketIn.Activate();
		return true;
	}

	public bool TryActivateVFX_CombatStart()
	{
		if (m_VFXCombatStart == null || m_heroCard == null)
		{
			return false;
		}
		m_VFXCombatStart.Activate();
		return true;
	}

	public bool TryActivateVFX_WinStreak(int currentStreak)
	{
		Spell vfxSpell = null;
		if (m_VFXWinStreak == null || m_VFXWinStreak.Count == 0)
		{
			return false;
		}
		int val = -1;
		foreach (ValueVFX vfx in m_VFXWinStreak)
		{
			if (vfx.m_onlyExactMatch)
			{
				if (vfx.m_value == currentStreak)
				{
					val = vfx.m_value;
					vfxSpell = vfx.m_vfxSpell;
					break;
				}
			}
			else if (vfx.m_value > val && vfx.m_value <= currentStreak)
			{
				val = vfx.m_value;
				vfxSpell = vfx.m_vfxSpell;
			}
		}
		if (vfxSpell != null && m_heroCard != null)
		{
			vfxSpell.Activate();
			return true;
		}
		return false;
	}

	protected string TryGetRandomLine(List<string> lines)
	{
		if (lines == null)
		{
			return null;
		}
		if (lines.Count == 0)
		{
			return null;
		}
		return lines[UnityEngine.Random.Range(0, lines.Count)];
	}
}
