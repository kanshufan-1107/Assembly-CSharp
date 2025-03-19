using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Hearthstone.Timeline;

[Serializable]
public class MaterialModifierBehaviour : TimelineEffectBehaviour<MaterialModifierHelper>
{
	[SerializeField]
	[Min(0f)]
	private int m_materialIndex;

	[SerializeField]
	private MaterialModifierEntryCollection m_modifiers = new MaterialModifierEntryCollection();

	public PlayableGraph PlayableGraph { get; set; }

	public MaterialModifierTrack TrackAsset { get; set; }

	protected override object[] GetHelperInitializationData(PlayableInfo _)
	{
		Renderer renderer = TrackAsset.GetTrackBinding<Renderer>(PlayableGraph);
		if (renderer == null)
		{
			return null;
		}
		if (m_modifiers == null)
		{
			return null;
		}
		return new object[3] { renderer, m_materialIndex, m_modifiers };
	}

	protected override void InitializeFrame(Playable playable, FrameData info, object playerData)
	{
	}

	protected override void OnEnter(Playable playable, FrameData info)
	{
	}

	protected override void OnExit(Playable playable, FrameData info)
	{
	}

	protected override void UpdateFrame(Playable playable, FrameData info, object playerData, float normalizedTime)
	{
		base.Helper.UpdateEffect((float)playable.GetTime(), !playable.GetGraph().IsPlaying());
	}
}
