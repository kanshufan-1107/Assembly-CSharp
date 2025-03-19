using System;
using Shared.Scripts.Util;

namespace Shared.Scripts.Game.Shop.Product;

public readonly struct Attribute : IEquatable<Attribute>
{
	public string Name { get; }

	public string Value { get; }

	public static bool IsValid(string name, string value)
	{
		if (!string.IsNullOrEmpty(name))
		{
			return !string.IsNullOrEmpty(value);
		}
		return false;
	}

	public static Maybe<Attribute> CreateFrom(string name, string value)
	{
		if (!IsValid(name, value))
		{
			return Maybe.None;
		}
		return new Attribute(name, value);
	}

	private Attribute(string name, string value)
	{
		Name = name.ToLower();
		Value = value.ToLower();
	}

	public bool Equals(Attribute other)
	{
		string name = Name;
		string value = Value;
		string name2 = other.Name;
		string value2 = other.Value;
		if (name == name2)
		{
			return value == value2;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is Attribute other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (Name, Value).GetHashCode();
	}
}
