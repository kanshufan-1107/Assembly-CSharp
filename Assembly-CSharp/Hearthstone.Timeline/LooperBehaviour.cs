using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Hearthstone.Timeline;

[Serializable]
public class LooperBehaviour : PlayableBehaviour
{
	public enum BreakoutMode
	{
		Random,
		Pattern
	}

	public delegate void LoopHandler(PlayableDirector director);

	public static LoopHandler OnLoop;

	[SerializeField]
	private BreakoutMode m_breakoutMode;

	[Range(0f, 1f)]
	[Tooltip("How likely is it that the track will loop?\n0 = Never loop.\n1 = Always loop.")]
	[SerializeField]
	private float m_chanceToLoop = 1f;

	[SerializeField]
	[Tooltip("The loop/breakout pattern.")]
	private bool[] m_pattern = new bool[1];

	public LooperAsset ClipAsset { get; set; }

	public override void OnBehaviourPause(Playable playable, FrameData info)
	{
		if (info.evaluationType == FrameData.EvaluationType.Playback && playable.GetTime() + (double)info.deltaTime >= playable.GetDuration())
		{
			switch (m_breakoutMode)
			{
			case BreakoutMode.Random:
				if (Mathf.Approximately(m_chanceToLoop, 1f) || (m_chanceToLoop > 0f && m_chanceToLoop > UnityEngine.Random.value))
				{
					GoToTime(playable, ClipAsset.StartTime);
				}
				break;
			case BreakoutMode.Pattern:
				if (m_pattern.Length != 0)
				{
					ClipAsset.PatternIndex %= m_pattern.Length;
					if (m_pattern[ClipAsset.PatternIndex])
					{
						GoToTime(playable, ClipAsset.StartTime);
					}
					ClipAsset.PatternIndex++;
				}
				break;
			}
		}
		base.OnBehaviourPause(playable, info);
	}

	private void GoToTime(Playable playable, double time)
	{
		PlayableDirector director = playable.GetGraph().GetResolver() as PlayableDirector;
		DirectorUpdateMode originalUpdateMode = director.timeUpdateMode;
		director.timeUpdateMode = DirectorUpdateMode.Manual;
		director.Stop();
		director.time = time;
		playable.SetTime(time);
		director.timeUpdateMode = originalUpdateMode;
		director.Play();
		OnLoop?.Invoke(director);
	}
}
