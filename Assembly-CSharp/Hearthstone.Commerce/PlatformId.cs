using System;
using Shared.Scripts.Util;

namespace Hearthstone.Commerce;

public readonly struct PlatformId : IEquatable<PlatformId>
{
	public enum Platform
	{
		Apple = 1,
		Google,
		Amazon
	}

	public Platform Value { get; }

	public static bool IsValid(int value)
	{
		if ((uint)(value - 1) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static Maybe<PlatformId> CreateFrom(int value)
	{
		if (!IsValid(value))
		{
			return Maybe.None;
		}
		return CreateFromValidated((uint)value);
	}

	public static PlatformId CreateFromValidated(uint value)
	{
		return new PlatformId((Platform)value);
	}

	private PlatformId(Platform value)
	{
		Value = value;
	}

	public bool Equals(PlatformId other)
	{
		return Value == other.Value;
	}

	public override bool Equals(object obj)
	{
		if (obj is PlatformId other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public override string ToString()
	{
		return Value.ToString();
	}
}
