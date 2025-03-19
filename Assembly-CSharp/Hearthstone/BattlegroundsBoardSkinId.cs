using System;
using UnityEngine;

namespace Hearthstone;

public readonly struct BattlegroundsBoardSkinId : IBattlegroundsSkinId, IEquatable<BattlegroundsBoardSkinId>
{
	private readonly int m_value;

	private BattlegroundsBoardSkinId(int value)
	{
		m_value = value;
	}

	public static BattlegroundsBoardSkinId FromTrustedValue(int value)
	{
		return new BattlegroundsBoardSkinId(value);
	}

	public static BattlegroundsBoardSkinId? FromUntrustedValue(int value)
	{
		if (value > 0)
		{
			return new BattlegroundsBoardSkinId(value);
		}
		Debug.LogWarning("BattlegroundsBoardSkinId.FromUntrustedValue ignoring unexpectedly zero or negative value!");
		return null;
	}

	public bool IsValid()
	{
		return m_value > 0;
	}

	public bool IsDefaultBoard()
	{
		return 1 == m_value;
	}

	public int ToValue()
	{
		return m_value;
	}

	public bool Equals(BattlegroundsBoardSkinId other)
	{
		return m_value == other.m_value;
	}

	public override bool Equals(object obj)
	{
		if (obj is BattlegroundsBoardSkinId id)
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
