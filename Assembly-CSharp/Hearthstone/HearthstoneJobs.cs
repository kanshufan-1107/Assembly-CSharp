using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using UnityEngine;

namespace Hearthstone;

public static class HearthstoneJobs
{
	private static List<IJobDependency> s_dependencyBuilder = new List<IJobDependency>();

	public static JobDefinition CreateJobFromDependency(string jobID, IJobDependency dependency)
	{
		return CreateJobFromDependency(jobID, dependency, JobFlags.None);
	}

	public static JobDefinition CreateJobFromDependency(string jobID, IJobDependency dependency, JobFlags jobFlags)
	{
		IJobDependency[] dependencies = null;
		return new JobDefinition(jobID, Job_YieldDependency(dependency), jobFlags, dependencies);
	}

	public static JobDefinition CreateJobFromDependency(string jobID, IJobDependency dependency, params object[] dependenciesIdentifiers)
	{
		return CreateJobFromDependency(jobID, dependency, JobFlags.None, dependenciesIdentifiers);
	}

	public static JobDefinition CreateJobFromDependency(string jobID, IJobDependency dependency, JobFlags jobFlags, params object[] dependenciesIdentifiers)
	{
		return new JobDefinition(jobID, Job_YieldDependency(dependency), jobFlags, BuildDependencies(dependenciesIdentifiers));
	}

	public static JobDefinition CreateJobFromAction(string jobID, Action action, params IJobDependency[] dependencies)
	{
		return CreateJobFromAction(jobID, action, JobFlags.None, dependencies);
	}

	public static JobDefinition CreateJobFromAction(string jobID, Action action, JobFlags jobFlags, params IJobDependency[] dependencies)
	{
		return new JobDefinition(jobID, Job_InvokeAction(action), jobFlags, dependencies);
	}

	public static JobDefinition CreateJobFromAction(string jobID, Action action, params object[] dependenciesIdentifiers)
	{
		return CreateJobFromAction(jobID, action, JobFlags.None, dependenciesIdentifiers);
	}

	public static JobDefinition CreateJobFromAction(string jobID, Action action, JobFlags jobFlags, params object[] dependenciesIdentifiers)
	{
		return new JobDefinition(jobID, Job_InvokeAction(action), jobFlags, BuildDependencies(dependenciesIdentifiers));
	}

	public static IEnumerator<IAsyncJobResult> Job_YieldDependency(IJobDependency dependency)
	{
		yield return dependency;
	}

	public static IEnumerator<IAsyncJobResult> Job_InvokeAction(Action action)
	{
		if (action != null && (!(action.Target is UnityEngine.Object) || !action.Target.Equals(null)))
		{
			action();
		}
		yield break;
	}

	public static IJobDependency[] BuildDependencies(params object[] dependencyIdentifiers)
	{
		s_dependencyBuilder.Clear();
		foreach (object dependencyIdentifier in dependencyIdentifiers)
		{
			if (dependencyIdentifier is IJobDependency dependency)
			{
				s_dependencyBuilder.Add(dependency);
				continue;
			}
			Type dependencyType = dependencyIdentifier as Type;
			if (dependencyType != null && typeof(IService).IsAssignableFrom(dependencyType))
			{
				s_dependencyBuilder.Add(ServiceManager.CreateServiceDependency(dependencyType));
				continue;
			}
			Log.Jobs.PrintWarning("[HearthstoneJobs.BuildDependencies] Failed to create job dependency from identifier ({0}).", dependencyIdentifier);
		}
		if (s_dependencyBuilder.Count <= 0)
		{
			return null;
		}
		return s_dependencyBuilder.ToArray();
	}
}
