using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BenchmarkRunner
{
	private Option m_option;

	private IBenchmark m_benchmark;

	private int m_numRuns;

	private bool m_forceRun;

	private bool m_benchmarkRun;

	[CompilerGenerated]
	private BenchmarkSpec _003CBenchmarkSpec_003Ek__BackingField = BenchmarkSpec.Low;

	public int AverageFrameMSResult { get; set; }

	public bool Valid => AverageFrameMSResult != 0;

	public bool BenchmarkRun => m_benchmarkRun;

	public BenchmarkRunner(Option option, IBenchmark benchmark, int numRuns, bool forceRun = false)
	{
		m_option = option;
		AverageFrameMSResult = Options.Get().GetInt(option);
		m_benchmark = benchmark;
		m_numRuns = numRuns;
		m_forceRun = forceRun;
	}

	public IEnumerator Run()
	{
		if (Valid && !m_forceRun)
		{
			UpdateBenchmarkSpecFromAverageFT();
			yield break;
		}
		_ = Time.realtimeSinceStartup;
		List<float> frameTimes = new List<float>();
		if (m_benchmark.Setup())
		{
			m_benchmark.Run();
			yield return null;
			for (int runs = 0; runs < m_numRuns; runs++)
			{
				frameTimes.Add(Time.unscaledDeltaTime);
				m_benchmark.Run();
				yield return null;
			}
			AverageFrameMSResult = Mathf.RoundToInt(frameTimes.Average((float x) => x * 1000f));
			Options.Get().SetInt(m_option, AverageFrameMSResult);
			UpdateBenchmarkSpecFromAverageFT();
			m_benchmarkRun = Valid;
		}
	}

	private void UpdateBenchmarkSpecFromAverageFT()
	{
	}
}
