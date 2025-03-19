using System;

namespace Shared.Scripts.Util;

public readonly struct Maybe<T>
{
	private readonly T m_value;

	private readonly bool m_hasValue;

	private Maybe(T value)
	{
		m_value = value;
		m_hasValue = true;
	}

	public void Match(Action<T> some, Action none = null)
	{
		if (m_hasValue)
		{
			some(m_value);
		}
		else
		{
			none?.Invoke();
		}
	}

	public static implicit operator Maybe<T>(T value)
	{
		if (value == null)
		{
			return default(Maybe<T>);
		}
		return new Maybe<T>(value);
	}

	public static implicit operator Maybe<T>(Maybe.MaybeNone value)
	{
		return default(Maybe<T>);
	}

	public bool TryGetValue(out T value)
	{
		if (m_hasValue)
		{
			value = m_value;
			return true;
		}
		value = default(T);
		return false;
	}
}
public static class Maybe
{
	public class MaybeNone
	{
	}

	public static MaybeNone None { get; } = new MaybeNone();
}
