using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class LegendaryHeroAnimTimelineManager : MonoBehaviour
{
	public SerializableDictionary<LegendaryHeroAnimController.InternalState, TimelineAsset> m_timelines = new SerializableDictionary<LegendaryHeroAnimController.InternalState, TimelineAsset>();

	[NonSerialized]
	public LegendaryHeroAnimController m_animController;

	[NonSerialized]
	public PlayableDirector m_playableDirector;

	[NonSerialized]
	public LegendaryHeroAnimController.InternalState m_activeTimeline;

	public bool UpdatePlayableAsset(LegendaryHeroAnimController.InternalState state)
	{
		TimelineAsset asset;
		bool result = m_timelines.TryGetValue(state, out asset);
		PlayableDirector playableDirector = GetOrCeatePlayableDirector();
		if (asset != playableDirector.playableAsset)
		{
			playableDirector.playableAsset = asset;
			m_activeTimeline = state;
		}
		return result;
	}

	private PlayableDirector GetOrCeatePlayableDirector()
	{
		if (m_playableDirector == null && !base.gameObject.TryGetComponent<PlayableDirector>(out m_playableDirector))
		{
			m_playableDirector = base.gameObject.AddComponent<PlayableDirector>();
			m_playableDirector.timeUpdateMode = DirectorUpdateMode.Manual;
			m_playableDirector.playOnAwake = false;
		}
		return m_playableDirector;
	}

	private void Start()
	{
		m_animController = GetComponent<LegendaryHeroAnimController>();
		if (!m_animController)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		m_animController.OnRequestAnimTransition += OnRequestedAnimTransition;
		m_playableDirector = GetOrCeatePlayableDirector();
	}

	private void OnDestroy()
	{
		if ((bool)m_animController)
		{
			m_animController.OnRequestAnimTransition -= OnRequestedAnimTransition;
		}
	}

	private void OnRequestedAnimTransition(LegendaryHeroAnimController.InternalState state)
	{
		if (UpdatePlayableAsset(state))
		{
			m_playableDirector.Play();
		}
	}

	private void Update()
	{
		if (m_playableDirector.isActiveAndEnabled && m_playableDirector.playableAsset != null)
		{
			float animTime = m_animController.GetCurrentAnimTime();
			m_playableDirector.time = animTime;
			m_playableDirector.Evaluate();
		}
	}
}
