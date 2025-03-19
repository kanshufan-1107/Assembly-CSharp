using System;
using System.Collections.Generic;
using System.Linq;

namespace Shared.Scripts.Util.ValueTypes;

public abstract class Record : IEquatable<Record>
{
	public virtual bool Equals(Record other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (other.GetType() != GetType())
		{
			return false;
		}
		return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((Record)obj);
	}

	public static bool operator ==(Record left, Record right)
	{
		if (((object)left == null) ^ ((object)right == null))
		{
			return false;
		}
		return left?.Equals(right) ?? true;
	}

	public static bool operator !=(Record left, Record right)
	{
		return !(left == right);
	}

	public override int GetHashCode()
	{
		return (from x in GetEqualityComponents()
			select x?.GetHashCode() ?? 0).Aggregate((int x, int y) => x ^ y);
	}

	protected abstract IEnumerable<object> GetEqualityComponents();
}
