using System;
using UnityEngine;

namespace Hearthstone;

public readonly struct BattlegroundsEmoteId : IBattlegroundsSkinId, IEquatable<BattlegroundsEmoteId>
{
	private readonly int m_value;

	private BattlegroundsEmoteId(int value)
	{
		m_value = value;
	}

	public static BattlegroundsEmoteId FromTrustedValue(int value)
	{
		return new BattlegroundsEmoteId(value);
	}

	public static BattlegroundsEmoteId? FromUntrustedValue(int value)
	{
		if (value >= 0)
		{
			return new BattlegroundsEmoteId(value);
		}
		Debug.LogWarning("BattlegroundsEmoteId.FromUntrustedValue ignoring unexpected negative value!");
		return null;
	}

	public bool IsValid()
	{
		return m_value >= 0;
	}

	public bool IsDefaultEmote()
	{
		return IsDefaultEmoteId(m_value);
	}

	public int ToValue()
	{
		return m_value;
	}

	private static bool IsDefaultEmoteId(int emoteId)
	{
		return GameDbf.BattlegroundsEmote.GetRecord(emoteId)?.IsDefault ?? false;
	}

	public bool Equals(BattlegroundsEmoteId other)
	{
		return m_value == other.m_value;
	}

	public override bool Equals(object obj)
	{
		if (obj is BattlegroundsEmoteId id)
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

	public static bool operator !=(BattlegroundsEmoteId left, BattlegroundsEmoteId right)
	{
		return !left.Equals(right);
	}
}
