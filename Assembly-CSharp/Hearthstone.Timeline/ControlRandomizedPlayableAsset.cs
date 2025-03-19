using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[Serializable]
[NotKeyable]
public class ControlRandomizedPlayableAsset : PlayableAsset, IPropertyPreview, ITimelineClipAsset
{
	private const int k_MaxRandInt = 10000;

	private static readonly List<PlayableDirector> k_EmptyDirectorsList = new List<PlayableDirector>(0);

	private static readonly List<ParticleSystem> k_EmptyParticlesList = new List<ParticleSystem>(0);

	[SerializeField]
	public ExposedReference<GameObject> sourceGameObject;

	[SerializeField]
	public GameObject prefabGameObject;

	[SerializeField]
	public bool updateParticle = true;

	[SerializeField]
	public bool updateDirector = true;

	[SerializeField]
	public bool updateITimeControl = true;

	[SerializeField]
	public bool searchHierarchy = true;

	[SerializeField]
	public bool active = true;

	[SerializeField]
	public ActivationControlPlayable.PostPlaybackState postPlayback = ActivationControlPlayable.PostPlaybackState.Revert;

	private PlayableAsset m_ControlDirectorAsset;

	private double m_Duration = PlayableBinding.DefaultDuration;

	private bool m_SupportLoop;

	private static HashSet<PlayableDirector> s_ProcessedDirectors = new HashSet<PlayableDirector>();

	private static HashSet<GameObject> s_CreatedPrefabs = new HashSet<GameObject>();

	public bool controllingDirectors { get; private set; }

	public bool controllingParticles { get; private set; }

	public override double duration => m_Duration;

	public ClipCaps clipCaps => (ClipCaps)(0xC | (m_SupportLoop ? 1 : 0));

	public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
	{
		if (prefabGameObject != null)
		{
			if (s_CreatedPrefabs.Contains(prefabGameObject))
			{
				Debug.LogWarningFormat("Control Track Clip ({0}) is causing a prefab to instantiate itself recursively. Aborting further instances.", base.name);
				ScriptPlayable<ControlRandomizedBehaviour> playable = ScriptPlayable<ControlRandomizedBehaviour>.Create(graph);
				playable.GetBehaviour().PlayableAsset = this;
				playable.GetBehaviour().SetParticleSystemsBelongingToThisTrack(null);
				return playable;
			}
			s_CreatedPrefabs.Add(prefabGameObject);
		}
		ScriptPlayable<ControlRandomizedBehaviour> root = ScriptPlayable<ControlRandomizedBehaviour>.Null;
		List<Playable> playables = new List<Playable>();
		GameObject sourceObject = sourceGameObject.Resolve(graph.GetResolver());
		if (prefabGameObject != null)
		{
			Transform parenTransform = ((sourceObject != null) ? sourceObject.transform : null);
			ScriptPlayable<PrefabControlPlayable> controlPlayable = PrefabControlPlayable.Create(graph, prefabGameObject, parenTransform);
			sourceObject = controlPlayable.GetBehaviour().prefabInstance;
			playables.Add(controlPlayable);
		}
		m_Duration = PlayableBinding.DefaultDuration;
		m_SupportLoop = false;
		controllingParticles = false;
		controllingDirectors = false;
		List<ParticleSystem> particleSystems = new List<ParticleSystem>();
		if (sourceObject != null)
		{
			IList<PlayableDirector> list2;
			if (!updateDirector)
			{
				IList<PlayableDirector> list = k_EmptyDirectorsList;
				list2 = list;
			}
			else
			{
				list2 = GetComponent<PlayableDirector>(sourceObject);
			}
			IList<PlayableDirector> directors = list2;
			particleSystems = (updateParticle ? ((List<ParticleSystem>)GetParticleSystemRoots(sourceObject)) : k_EmptyParticlesList);
			UpdateDurationAndLoopFlag(directors, particleSystems);
			PlayableDirector director = go.GetComponent<PlayableDirector>();
			if (director != null)
			{
				m_ControlDirectorAsset = director.playableAsset;
			}
			if (go == sourceObject && prefabGameObject == null)
			{
				Debug.LogWarningFormat("Control Playable ({0}) is referencing the same PlayableDirector component than the one in which it is playing.", base.name);
				active = false;
				if (!searchHierarchy)
				{
					updateDirector = false;
				}
			}
			if (active)
			{
				CreateActivationPlayable(sourceObject, graph, playables);
			}
			if (updateDirector)
			{
				SearchHierarchyAndConnectDirector(directors, graph, playables, prefabGameObject != null);
			}
			if (updateParticle)
			{
				SearchHiearchyAndConnectParticleSystem(particleSystems, graph, playables);
			}
			if (updateITimeControl)
			{
				SearchHierarchyAndConnectControlableScripts(GetControlableScripts(sourceObject), graph, playables);
			}
			root = ConnectPlayablesToMixer(graph, playables);
		}
		if (prefabGameObject != null)
		{
			s_CreatedPrefabs.Remove(prefabGameObject);
		}
		if (!root.IsValid())
		{
			root = ScriptPlayable<ControlRandomizedBehaviour>.Create(graph);
		}
		root.GetBehaviour().PlayableAsset = this;
		root.GetBehaviour().SetParticleSystemsBelongingToThisTrack(particleSystems);
		return root;
	}

	private static ScriptPlayable<ControlRandomizedBehaviour> ConnectPlayablesToMixer(PlayableGraph graph, List<Playable> playables)
	{
		ScriptPlayable<ControlRandomizedBehaviour> mixer = ScriptPlayable<ControlRandomizedBehaviour>.Create(graph, playables.Count);
		for (int i = 0; i != playables.Count; i++)
		{
			ConnectMixerAndPlayable(graph, mixer, playables[i], i);
		}
		mixer.SetPropagateSetTime(value: true);
		return mixer;
	}

	private void CreateActivationPlayable(GameObject root, PlayableGraph graph, List<Playable> outplayables)
	{
		ScriptPlayable<ActivationControlPlayable> activation = ActivationControlPlayable.Create(graph, root, postPlayback);
		if (activation.IsValid())
		{
			outplayables.Add(activation);
		}
	}

	private void SearchHiearchyAndConnectParticleSystem(IEnumerable<ParticleSystem> particleSystems, PlayableGraph graph, List<Playable> outplayables)
	{
		foreach (ParticleSystem particleSystem in particleSystems)
		{
			if (particleSystem != null)
			{
				controllingParticles = true;
				outplayables.Add(ControlRandomizedParticleControlPlayable.Create(graph, particleSystem));
			}
		}
	}

	private void SearchHierarchyAndConnectDirector(IEnumerable<PlayableDirector> directors, PlayableGraph graph, List<Playable> outplayables, bool disableSelfReferences)
	{
		foreach (PlayableDirector director in directors)
		{
			if (director != null)
			{
				if (director.playableAsset != m_ControlDirectorAsset)
				{
					outplayables.Add(DirectorControlPlayable.Create(graph, director));
					controllingDirectors = true;
				}
				else if (disableSelfReferences)
				{
					director.enabled = false;
				}
			}
		}
	}

	private static void SearchHierarchyAndConnectControlableScripts(IEnumerable<MonoBehaviour> controlableScripts, PlayableGraph graph, List<Playable> outplayables)
	{
		foreach (MonoBehaviour script in controlableScripts)
		{
			outplayables.Add(TimeControlPlayable.Create(graph, (ITimeControl)script));
		}
	}

	private static void ConnectMixerAndPlayable(PlayableGraph graph, Playable mixer, Playable playable, int portIndex)
	{
		graph.Connect(playable, 0, mixer, portIndex);
		mixer.SetInputWeight(playable, 1f);
	}

	public IList<T> GetComponent<T>(GameObject gameObject)
	{
		List<T> components = new List<T>();
		if (gameObject != null)
		{
			if (searchHierarchy)
			{
				gameObject.GetComponentsInChildren(includeInactive: true, components);
			}
			else
			{
				gameObject.GetComponents(components);
			}
		}
		return components;
	}

	private static IEnumerable<MonoBehaviour> GetControlableScripts(GameObject root)
	{
		if (root == null)
		{
			yield break;
		}
		MonoBehaviour[] componentsInChildren = root.GetComponentsInChildren<MonoBehaviour>();
		foreach (MonoBehaviour script in componentsInChildren)
		{
			if (script is ITimeControl)
			{
				yield return script;
			}
		}
	}

	public void UpdateDurationAndLoopFlag(IList<PlayableDirector> directors, IList<ParticleSystem> particleSystems)
	{
		if (directors.Count == 0 && particleSystems.Count == 0)
		{
			return;
		}
		double maxDuration = double.NegativeInfinity;
		bool supportsLoop = false;
		foreach (PlayableDirector director in directors)
		{
			if (director.playableAsset != null)
			{
				double assetDuration = director.playableAsset.duration;
				if (director.playableAsset is TimelineAsset && assetDuration > 0.0)
				{
					assetDuration = (double)((DiscreteTime)assetDuration).OneTickAfter();
				}
				maxDuration = Math.Max(maxDuration, assetDuration);
				supportsLoop = supportsLoop || director.extrapolationMode == DirectorWrapMode.Loop;
			}
		}
		foreach (ParticleSystem particleSystem in particleSystems)
		{
			maxDuration = Math.Max(maxDuration, particleSystem.main.duration);
			supportsLoop = supportsLoop || particleSystem.main.loop;
		}
		m_Duration = (double.IsNegativeInfinity(maxDuration) ? PlayableBinding.DefaultDuration : maxDuration);
		m_SupportLoop = supportsLoop;
	}

	private IList<ParticleSystem> GetParticleSystemRoots(GameObject go)
	{
		if (searchHierarchy)
		{
			List<ParticleSystem> roots = new List<ParticleSystem>();
			GetParticleSystemRoots(go.transform, roots);
			return roots;
		}
		return GetComponent<ParticleSystem>(go);
	}

	private static void GetParticleSystemRoots(Transform t, ICollection<ParticleSystem> roots)
	{
		ParticleSystem ps = t.GetComponent<ParticleSystem>();
		if (ps != null)
		{
			roots.Add(ps);
			return;
		}
		for (int i = 0; i < t.childCount; i++)
		{
			GetParticleSystemRoots(t.GetChild(i), roots);
		}
	}

	public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
	{
		if (director == null || s_ProcessedDirectors.Contains(director))
		{
			return;
		}
		s_ProcessedDirectors.Add(director);
		GameObject gameObject = sourceGameObject.Resolve(director);
		if (gameObject != null)
		{
			if (updateParticle)
			{
				ParticleSystem[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
				foreach (ParticleSystem ps in componentsInChildren)
				{
					driver.AddFromName<ParticleSystem>(ps.gameObject, "randomSeed");
					driver.AddFromName<ParticleSystem>(ps.gameObject, "autoRandomSeed");
				}
			}
			if (active)
			{
				driver.AddFromName(gameObject, "m_IsActive");
			}
			if (updateITimeControl)
			{
				foreach (MonoBehaviour script in GetControlableScripts(gameObject))
				{
					if (script is IPropertyPreview propertyPreview)
					{
						propertyPreview.GatherProperties(director, driver);
					}
					else
					{
						driver.AddFromComponent(script.gameObject, script);
					}
				}
			}
			if (updateDirector)
			{
				foreach (PlayableDirector childDirector in GetComponent<PlayableDirector>(gameObject))
				{
					if (!(childDirector == null))
					{
						TimelineAsset timeline = childDirector.playableAsset as TimelineAsset;
						if (!(timeline == null))
						{
							timeline.GatherProperties(childDirector, driver);
						}
					}
				}
			}
		}
		s_ProcessedDirectors.Remove(director);
	}
}
