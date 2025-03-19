using System;

public class Filter<T>
{
	public Func<T, bool> PassesFilter { get; }

	public Filter(Func<T, bool> passesFilter)
	{
		PassesFilter = passesFilter;
	}
}
