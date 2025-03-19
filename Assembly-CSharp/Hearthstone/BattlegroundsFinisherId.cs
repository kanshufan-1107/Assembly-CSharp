using System;
using UnityEngine;

namespace Hearthstone;

public readonly struct BattlegroundsFinisherId : IBattlegroundsSkinId, IEquatable<BattlegroundsFinisherId>
{
	private readonly int m_value;

	private BattlegroundsFinisherId(int value)
	{
		m_value = value;
	}

	public static BattlegroundsFinisherId FromTrustedValue(int value)
	{
		return new BattlegroundsFinisherId(value);
	}

	public static BattlegroundsFinisherId? FromUntrustedValue(int value)
	{
		if (value > 0)
		{
			return new BattlegroundsFinisherId(value);
		}
		Debug.LogWarning("BattlegroundsFinisherId.FromUntrustedValue ignoring unexpectedly zero or negative value!");
		return null;
	}

	public bool IsValid()
	{
		return m_value > 0;
	}

	public bool IsDefaultFinisher()
	{
		return 1 == m_value;
	}

	public int ToValue()
	{
		return m_value;
	}

	public bool Equals(BattlegroundsFinisherId other)
	{
		return m_value == other.m_value;
	}

	public override bool Equals(object obj)
	{
		if (obj is BattlegroundsFinisherId id)
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
