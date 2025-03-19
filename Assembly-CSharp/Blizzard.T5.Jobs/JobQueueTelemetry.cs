using System.Collections.Generic;
using UnityEngine;

namespace Blizzard.T5.Jobs;

public class JobQueueTelemetry
{
	private JobQueue m_jobQueue;

	private string m_testType;

	private Dictionary<JobDefinition, float> m_jobDurations = new Dictionary<JobDefinition, float>();

	public JobQueueTelemetry(JobQueue jobQueue, JobQueueAlerts jobQueueAlerts, string testType)
	{
		if (jobQueue != null)
		{
			m_jobQueue = jobQueue;
			m_testType = testType;
			m_jobQueue.AddJobErrorListener(OnJobError);
		}
	}

	private bool OnJobError(JobDefinition job, string reason)
	{
		float jobDuration = CalculateJobDuration(job, Time.realtimeSinceStartup);
		TelemetryManager.Client().SendJobFinishFailure(job.Name, reason, m_testType, 4008765.ToString(), jobDuration);
		m_jobDurations.Remove(job);
		return false;
	}

	private float CalculateJobDuration(JobDefinition job, float jobEndTime)
	{
		return 0f;
	}
}
