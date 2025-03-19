using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public abstract class CustomViewPass : ScriptableRenderPass
{
	private static readonly List<CustomViewPass>[] queues;

	private CustomViewEntryPoint whereScheduled = CustomViewEntryPoint.Count;

	public bool isScheduled => whereScheduled != CustomViewEntryPoint.Count;

	static CustomViewPass()
	{
		queues = new List<CustomViewPass>[6];
		for (int i = 0; i < 6; i++)
		{
			queues[i] = new List<CustomViewPass>(5);
		}
	}

	public static List<CustomViewPass> GetQueue(CustomViewEntryPoint whenToRender)
	{
		if (whenToRender == CustomViewEntryPoint.Count)
		{
			Debug.LogError("Invalid entrypoint");
			return null;
		}
		return queues[(int)whenToRender];
	}

	public void ChangeSchedule(CustomViewEntryPoint whenToRender)
	{
		Unschedule();
		Schedule(whenToRender);
	}

	public void Schedule(CustomViewEntryPoint whenToRender)
	{
		if (whenToRender != whereScheduled)
		{
			if (whenToRender == CustomViewEntryPoint.Count)
			{
				Debug.LogError("Invalid entrypoint");
				return;
			}
			if (isScheduled)
			{
				Debug.LogError("Pass Already in Queue:" + whereScheduled);
				return;
			}
			queues[(int)whenToRender].Add(this);
			whereScheduled = whenToRender;
		}
	}

	public void Unschedule()
	{
		if (isScheduled)
		{
			List<CustomViewPass> theQueue = queues[(int)whereScheduled];
			if (theQueue != null)
			{
				whereScheduled = CustomViewEntryPoint.Count;
				theQueue.Remove(this);
			}
		}
	}
}
