using System;

namespace Hearthstone.Timeline;

internal struct DiscreteTime : IComparable
{
	public static readonly DiscreteTime kMaxTime = new DiscreteTime(long.MaxValue);

	private readonly long m_DiscreteTime2;

	private DiscreteTime(long time)
	{
		m_DiscreteTime2 = time;
	}

	public DiscreteTime(double time)
	{
		m_DiscreteTime2 = DoubleToDiscreteTime2(time);
	}

	public DiscreteTime OneTickAfter()
	{
		return new DiscreteTime(m_DiscreteTime2 + 1);
	}

	public int CompareTo(object obj)
	{
		if (obj is DiscreteTime)
		{
			long discreteTime = m_DiscreteTime2;
			return discreteTime.CompareTo(((DiscreteTime)obj).m_DiscreteTime2);
		}
		return 1;
	}

	public bool Equals(DiscreteTime other)
	{
		return m_DiscreteTime2 == other.m_DiscreteTime2;
	}

	public override bool Equals(object obj)
	{
		if (obj is DiscreteTime)
		{
			return Equals((DiscreteTime)obj);
		}
		return false;
	}

	private static long DoubleToDiscreteTime2(double time)
	{
		double number = time / 1E-12 + 0.5;
		if (number < 9.223372036854776E+18 && number > -9.223372036854776E+18)
		{
			return (long)number;
		}
		throw new ArgumentOutOfRangeException("Time is over the discrete range.");
	}

	private static double ToDouble(long time)
	{
		return (double)time * 1E-12;
	}

	public static explicit operator double(DiscreteTime b)
	{
		return ToDouble(b.m_DiscreteTime2);
	}

	public static explicit operator DiscreteTime(double time)
	{
		return new DiscreteTime(time);
	}

	public override string ToString()
	{
		long discreteTime = m_DiscreteTime2;
		return discreteTime.ToString();
	}

	public override int GetHashCode()
	{
		long discreteTime = m_DiscreteTime2;
		return discreteTime.GetHashCode();
	}
}
