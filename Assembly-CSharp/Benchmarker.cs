using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core;
using Hearthstone.Streaming;
using UnityEngine;

public class Benchmarker : IService
{
	private BenchmarkRunner m_cpuBenchmarkRunner;

	private BenchmarkRunner m_gpuBenchmarkRunner;

	private Coroutine m_activeBenchmarkCoro;

	private readonly bool m_benchmarkEnabled = true;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		if (m_benchmarkEnabled)
		{
			m_cpuBenchmarkRunner = new BenchmarkRunner(Option.CPU_BENCHMARK_RESULT, new CPUBenchmark(), 30);
			m_gpuBenchmarkRunner = new BenchmarkRunner(Option.GPU_BENCHMARK_RESULT, new GPUBenchmark(), 30);
			RunBenchmarks();
			while (m_activeBenchmarkCoro != null)
			{
				yield return null;
			}
		}
	}

	public Type[] GetDependencies()
	{
		return new Type[3]
		{
			typeof(GameDownloadManager),
			typeof(SceneMgr),
			typeof(IAssetLoader)
		};
	}

	public void Shutdown()
	{
		if (m_activeBenchmarkCoro != null)
		{
			Processor.CancelCoroutine(m_activeBenchmarkCoro);
			m_activeBenchmarkCoro = null;
		}
	}

	public void RunBenchmarks()
	{
		m_activeBenchmarkCoro = Processor.RunCoroutine(RunBenchmarksCoroutine());
	}

	private IEnumerator RunBenchmarksCoroutine()
	{
		yield return m_cpuBenchmarkRunner.Run();
		if (!m_cpuBenchmarkRunner.Valid)
		{
			Debug.LogError("Couldn't get valid CPU benchmark, aborting sending benchmark data.");
			m_activeBenchmarkCoro = null;
			yield break;
		}
		int cpuResult = m_cpuBenchmarkRunner.AverageFrameMSResult;
		Debug.Log("CPU Benchmark Result (Average Frame Time): " + cpuResult + "ms");
		yield return m_gpuBenchmarkRunner.Run();
		if (!m_gpuBenchmarkRunner.Valid)
		{
			Debug.LogError("Couldn't get valid GPU benchmark, aborting sending benchmark data.");
			m_activeBenchmarkCoro = null;
			yield break;
		}
		int gpuResult = m_gpuBenchmarkRunner.AverageFrameMSResult;
		Debug.Log("GPU Benchmark Result (Average Frame Time): " + gpuResult + "ms");
		if (m_cpuBenchmarkRunner.BenchmarkRun || m_gpuBenchmarkRunner.BenchmarkRun)
		{
			TelemetryManager.Client().SendBenchmarkResult(cpuResult, gpuResult, 1);
		}
		m_activeBenchmarkCoro = null;
	}
}
