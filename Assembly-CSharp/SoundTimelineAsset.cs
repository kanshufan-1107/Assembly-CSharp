using UnityEngine;
using UnityEngine.Playables;

public class SoundTimelineAsset : PlayableAsset
{
	public ExposedReference<AudioSource> m_AudioSource;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		ScriptPlayable<SoundTimelineBehavior> playable = ScriptPlayable<SoundTimelineBehavior>.Create(graph);
		SoundTimelineBehavior soundCountrolBehaviour = playable.GetBehaviour();
		if (soundCountrolBehaviour.m_AudioSource == null)
		{
			soundCountrolBehaviour.m_AudioSource = m_AudioSource.Resolve(graph.GetResolver());
		}
		return playable;
	}
}
