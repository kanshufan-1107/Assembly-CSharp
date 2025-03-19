using System;
using UnityEngine;

namespace Hearthstone;

public readonly struct BattlegroundsHeroSkinId : IBattlegroundsSkinId, IEquatable<BattlegroundsHeroSkinId>
{
	private readonly int m_value;

	private BattlegroundsHeroSkinId(int value)
	{
		m_value = value;
	}

	public static BattlegroundsHeroSkinId FromTrustedValue(int value)
	{
		return new BattlegroundsHeroSkinId(value);
	}

	public static BattlegroundsHeroSkinId? FromUntrustedValue(int value)
	{
		if (value > 0)
		{
			return new BattlegroundsHeroSkinId(value);
		}
		Debug.LogWarning("BattlegroundsHeroSkinId.FromUntrustedValue ignoring unexpectedly zero or negative value!");
		return null;
	}

	public bool IsValid()
	{
		return m_value > 0;
	}

	public int ToValue()
	{
		return m_value;
	}

	public bool Equals(BattlegroundsHeroSkinId other)
	{
		return m_value == other.m_value;
	}

	public override bool Equals(object obj)
	{
		if (obj is BattlegroundsHeroSkinId id)
		{
			return Equals(id);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_value;
	}

	public override string ToString()
	{
		int value = m_value;
		return value.ToString();
	}
}
