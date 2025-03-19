using UnityEngine;

namespace Hearthstone.TrailMaker;

internal static class TrailUtility
{
	public static float GetTime()
	{
		return Time.time;
	}

	public static void UpdateDistances(this MutableArray<TrailPosition> trail)
	{
		if (trail != null && trail.Count > 1)
		{
			float minTime = trail.GetFirst().time;
			float duration = trail.GetLast().time - minTime;
			for (int i = 0; i < trail.Count; i++)
			{
				TrailPosition trailPosition = trail.Get(i);
				trailPosition.distance = (trailPosition.time - minTime) / duration;
				trail.Set(i, trailPosition);
			}
		}
	}

	public static void RemoveOld(this MutableArray<TrailPosition> trail, float lifespan)
	{
		while (trail.Count > 0 && GetTime() - trail.GetFirst().time > lifespan)
		{
			trail.RemoveFromStart();
		}
	}

	public static TrailPosition Lerp(TrailPosition a, TrailPosition b, float t)
	{
		TrailPosition result = default(TrailPosition);
		result.origin = Vector3.Lerp(a.origin, b.origin, t);
		result.min = Vector3.Lerp(a.min, b.min, t);
		result.max = Vector3.Lerp(a.max, b.max, t);
		result.time = Mathf.Lerp(a.time, b.time, t);
		result.distance = Mathf.Lerp(a.distance, b.distance, t);
		return result;
	}

	public static TrailPosition GenerateDataForArbitraryTime(this MutableArray<TrailPosition> trail, float time)
	{
		if (trail == null || trail.Count < 1)
		{
			return default(TrailPosition);
		}
		time = Mathf.Clamp01(time);
		float t = time * (float)trail.Count;
		int index = Mathf.FloorToInt(t);
		if (index >= trail.Count - 1)
		{
			return trail.GetLast();
		}
		return Lerp(trail.Get(index), trail.Get(index + 1), t - (float)index);
	}

	public static void Dissolve(this MutableArray<TrailPosition> trail, int maxCount)
	{
		int removeCount = trail.Count - maxCount;
		if (removeCount <= 0)
		{
			return;
		}
		if ((float)removeCount <= (float)trail.Count * 0.5f)
		{
			int count = trail.Count / removeCount;
			for (int i = trail.Count - count; i >= 0; i -= count)
			{
				trail.RemoveAt(i);
			}
			return;
		}
		int count2 = trail.Count / (trail.Count - removeCount);
		int startIndex = trail.Count - 2;
		for (int i2 = trail.Count - count2; i2 > -removeCount; i2 -= count2)
		{
			int j = startIndex;
			while (j > i2 && j > 0)
			{
				trail.RemoveAt(j);
				j--;
			}
			startIndex = i2 - 1;
		}
	}
}
