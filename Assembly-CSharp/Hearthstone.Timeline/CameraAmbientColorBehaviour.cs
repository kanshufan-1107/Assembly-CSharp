using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Hearthstone.Timeline;

[Serializable]
[Obsolete("Use CameraOverlay instead.", false)]
public class CameraAmbientColorBehaviour : PlayableBehaviour
{
	public Color m_ambientColor = new Color(0.2f, 0.2f, 0.2f);

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		Color finalcolor = Color.black;
		int inputcount = playable.GetInputCount();
		if (inputcount != 0)
		{
			for (int i = 0; i < inputcount; i++)
			{
				float inputweight = playable.GetInputWeight(i);
				CameraAmbientColorBehaviour input = ((ScriptPlayable<CameraAmbientColorBehaviour>)playable.GetInput(i)).GetBehaviour();
				finalcolor += input.m_ambientColor * inputweight;
			}
			RenderSettings.ambientLight = finalcolor;
		}
	}
}
