using System.Collections.Generic;
using UnityEngine;

public class CPUBenchmark : IBenchmark
{
	public override bool Setup()
	{
		return true;
	}

	public override void Run()
	{
		List<int> numbers = new List<int>();
		for (int i = 0; i < 10000; i++)
		{
			numbers.Add(Random.Range(0, 10000000));
		}
		numbers.Sort();
	}

	public override void Cleanup()
	{
	}
}
