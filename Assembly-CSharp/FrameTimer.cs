using UnityEngine;

public class FrameTimer
{
	public bool IsRunning => !TimeStopped.HasValue;

	public int FrameStarted { get; private set; }

	public int? FrameStopped { get; private set; }

	public int FramesTaken
	{
		get
		{
			if (FrameStopped.HasValue)
			{
				return FrameStopped.Value - FrameStarted;
			}
			return Time.frameCount - FrameStarted;
		}
	}

	public float TimeStarted { get; private set; }

	public float? TimeStopped { get; private set; }

	public float TimeTaken
	{
		get
		{
			if (TimeStopped.HasValue)
			{
				return TimeStopped.Value - TimeStarted;
			}
			return Time.realtimeSinceStartup - TimeStarted;
		}
	}

	public string TimeTakenInfo => $"{TimeTaken} Seconds, {FramesTaken} Frames";

	public void StartRecording()
	{
		if (IsRunning)
		{
			StopRecording();
		}
		FrameStarted = Time.frameCount;
		FrameStopped = null;
		TimeStarted = Time.realtimeSinceStartup;
		TimeStopped = null;
	}

	public void StopRecording()
	{
		if (!TimeStopped.HasValue)
		{
			FrameStopped = Time.frameCount;
			TimeStopped = Time.realtimeSinceStartup;
		}
	}
}
