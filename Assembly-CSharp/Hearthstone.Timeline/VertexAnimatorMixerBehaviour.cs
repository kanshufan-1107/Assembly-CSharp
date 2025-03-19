using UnityEngine;
using UnityEngine.Playables;

namespace Hearthstone.Timeline;

public class VertexAnimatorMixerBehaviour : PlayableBehaviour
{
	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		if (Application.isPlaying)
		{
			return;
		}
		int inputCount = playable.GetInputCount();
		if (inputCount <= 0)
		{
			return;
		}
		float highestWeight = float.NegativeInfinity;
		ScriptPlayable<VertexAnimatorBehaviour>? inputPlayable = null;
		for (int input = 0; input < inputCount; input++)
		{
			float inputWeight = playable.GetInputWeight(input);
			if (inputWeight > highestWeight)
			{
				highestWeight = inputWeight;
				inputPlayable = (ScriptPlayable<VertexAnimatorBehaviour>)playable.GetInput(input);
			}
		}
		if (inputPlayable.HasValue)
		{
			inputPlayable.Value.GetBehaviour().ScrubFrame(inputPlayable.Value);
		}
	}
}
