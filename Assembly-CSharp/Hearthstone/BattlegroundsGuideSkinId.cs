using System;
using UnityEngine;

namespace Hearthstone;

public readonly struct BattlegroundsGuideSkinId : IBattlegroundsSkinId, IEquatable<BattlegroundsGuideSkinId>
{
	private readonly int m_value;

	private BattlegroundsGuideSkinId(int value)
	{
		m_value = value;
	}

	public static BattlegroundsGuideSkinId FromTrustedValue(int value)
	{
		return new BattlegroundsGuideSkinId(value);
	}

	public static BattlegroundsGuideSkinId? FromUntrustedValue(int value)
	{
		if (value > 0)
		{
			return new BattlegroundsGuideSkinId(value);
		}
		Debug.LogWarning("BattlegroundsGuideSkinId.FromUntrustedValue ignoring unexpectedly zero or negative value!");
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

	public bool Equals(BattlegroundsGuideSkinId other)
	{
		return m_value == other.m_value;
	}

	public override bool Equals(object obj)
	{
		if (obj is BattlegroundsGuideSkinId id)
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

	public static bool operator ==(BattlegroundsGuideSkinId left, BattlegroundsGuideSkinId right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(BattlegroundsGuideSkinId left, BattlegroundsGuideSkinId right)
	{
		return !left.Equals(right);
	}
}
