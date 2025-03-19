using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using UnityEngine;

public class LegendaryHeroRenderToTextureService : IHasUpdate, IService
{
	private struct ObjectSkinPair
	{
		public GameObject Object;

		public LegendarySkin Skin;
	}

	private class HeroPortraitHandleInternal : ILegendaryHeroPortrait, IDisposable
	{
		private readonly LegendaryHeroRenderToTextureService m_service;

		private readonly string m_assetPath;

		private readonly Player.Side m_playerSide;

		private readonly HashSet<LegendarySkinDynamicResController> m_dynamicResControllers;

		private GameObject m_legendaryPrefabInstance;

		private LegendarySkin m_legendarySkin;

		private LegendaryHeroAnimController m_animController;

		private LegendaryHeroActorCache m_actorCache;

		Texture ILegendaryHeroPortrait.PortraitTexture => m_legendarySkin?.PortraitTexture;

		public HeroPortraitHandleInternal(LegendaryHeroRenderToTextureService service, string assetPath, Player.Side playerSide)
		{
			m_service = service;
			m_assetPath = assetPath;
			m_playerSide = playerSide;
			m_dynamicResControllers = new HashSet<LegendarySkinDynamicResController>();
			m_legendaryPrefabInstance = m_service.GetOrCreateGameObject(assetPath, playerSide);
			m_legendarySkin = m_legendaryPrefabInstance.GetComponentInChildren<LegendarySkin>();
			m_animController = m_legendaryPrefabInstance.GetComponentInChildren<LegendaryHeroAnimController>();
			m_actorCache = m_legendaryPrefabInstance.GetComponentInChildren<LegendaryHeroActorCache>();
		}

		bool ILegendaryHeroPortrait.IsValidForPath(string assetPath, Player.Side playerSide)
		{
			if (m_assetPath == assetPath)
			{
				return m_playerSide == playerSide;
			}
			return false;
		}

		void ILegendaryHeroPortrait.AttachToActor(Actor actor)
		{
			if (m_animController != null)
			{
				m_animController.OnAttachedToActor(actor);
			}
			if (m_actorCache != null)
			{
				m_actorCache.Actor = actor;
			}
		}

		void ILegendaryHeroPortrait.RaiseAnimationEvent(LegendaryHeroAnimations animation)
		{
			if (m_animController != null)
			{
				m_animController.RaiseAnimEvent(animation);
			}
		}

		void ILegendaryHeroPortrait.RaiseEmoteAnimationEvent(EmoteType emote)
		{
			if (m_animController != null)
			{
				m_animController.EmotePlayCallback(emote);
			}
		}

		void ILegendaryHeroPortrait.RaiseGenericEvent(string eventName, object eventData)
		{
			LegendaryHeroGenericEventHandler[] componentsInChildren = m_legendaryPrefabInstance.GetComponentsInChildren<LegendaryHeroGenericEventHandler>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].HandleEvent(eventName, eventData);
			}
		}

		void ILegendaryHeroPortrait.AddSlaveAnimator(Animator slaveAnimator, float transitionTimeMultiplier)
		{
			if (m_animController != null)
			{
				m_animController.AddSlaveAnimator(slaveAnimator, transitionTimeMultiplier);
			}
		}

		void ILegendaryHeroPortrait.RemoveSlaveAnimator(Animator slaveAnimator)
		{
			if (m_animController != null)
			{
				m_animController.RemoveSlaveAnimator(slaveAnimator);
			}
		}

		void ILegendaryHeroPortrait.ClearDynamicResolutionControllers()
		{
			foreach (LegendarySkinDynamicResController dynamicResController in m_dynamicResControllers)
			{
				dynamicResController.Skin = null;
			}
			m_dynamicResControllers.Clear();
		}

		void ILegendaryHeroPortrait.ConnectDynamicResolutionController(LegendarySkinDynamicResController controller)
		{
			if (controller != null)
			{
				m_dynamicResControllers.Add(controller);
				controller.Skin = m_legendarySkin;
				m_legendarySkin.SetDirty();
			}
		}

		void ILegendaryHeroPortrait.UpdateDynamicResolutionControllers()
		{
			m_legendarySkin.UpdateDynamicResControllers();
		}

		GameObject ILegendaryHeroPortrait.FindGameObjectInLegendaryPortraitPrefab(string objectName)
		{
			if (m_legendaryPrefabInstance == null)
			{
				Debug.LogError("Cannot FindGameObjectInLegendaryPortraitPrefab when prefab instance is null.");
				return null;
			}
			Stack<GameObject> gameObjects = new Stack<GameObject>();
			gameObjects.Push(m_legendaryPrefabInstance);
			while (gameObjects.Count > 0)
			{
				GameObject top = gameObjects.Pop();
				for (int i = 0; i < top.transform.childCount; i++)
				{
					GameObject child = top.transform.GetChild(i).gameObject;
					if (child.name == objectName)
					{
						return child;
					}
					gameObjects.Push(child);
				}
			}
			return null;
		}

		void IDisposable.Dispose()
		{
			((ILegendaryHeroPortrait)this).ClearDynamicResolutionControllers();
			if (m_legendaryPrefabInstance != null)
			{
				m_service.ReleaseGameObject(m_legendaryPrefabInstance);
				m_legendaryPrefabInstance = null;
			}
		}
	}

	private readonly Dictionary<(string, Player.Side), ObjectSkinPair> m_assetPathToObjectMap = new Dictionary<(string, Player.Side), ObjectSkinPair>();

	private readonly Dictionary<int, (string, Player.Side)> m_objectIDToAssetPathMap = new Dictionary<int, (string, Player.Side)>();

	private readonly Dictionary<int, int> m_objectIDReferenceCounts = new Dictionary<int, int>();

	private readonly List<LegendarySkin> m_activeList = new List<LegendarySkin>();

	private readonly List<LegendarySkin> m_inactiveList = new List<LegendarySkin>();

	Type[] IService.GetDependencies()
	{
		return null;
	}

	IEnumerator<IAsyncJobResult> IService.Initialize(ServiceLocator serviceLocator)
	{
		SceneMgr sceneMgr = null;
		while (!serviceLocator.TryGetService<SceneMgr>(out sceneMgr))
		{
			yield return null;
		}
		LegendarySkin.DynamicResolutionEnabled = true;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			LegendarySkin.DynamicResolutionScale = 1.1f;
		}
		else
		{
			LegendarySkin.DynamicResolutionScale = 1.5f;
		}
		sceneMgr.RegisterSceneLoadedEvent(OnSceneLoaded);
	}

	void IHasUpdate.Update()
	{
		int currentPriority = -1;
		m_activeList.Clear();
		m_inactiveList.Clear();
		foreach (KeyValuePair<(string, Player.Side), ObjectSkinPair> item in m_assetPathToObjectMap)
		{
			LegendarySkin skin = item.Value.Skin;
			if (!skin.isActiveAndEnabled || !skin.HasObservers)
			{
				skin.ClearSuspendFlag(LegendarySkin.SuspendUpdateReason.UpdatePriority);
				continue;
			}
			int skinPriority = skin.UpdatePriority;
			if (skinPriority > currentPriority)
			{
				m_inactiveList.AddRange(m_activeList);
				m_activeList.Clear();
				m_activeList.Add(skin);
				currentPriority = skinPriority;
			}
			else if (skinPriority == currentPriority)
			{
				m_activeList.Add(skin);
			}
			else
			{
				m_inactiveList.Add(skin);
			}
		}
		foreach (LegendarySkin inactive in m_inactiveList)
		{
			inactive.SetSuspendFlag(LegendarySkin.SuspendUpdateReason.UpdatePriority);
		}
		foreach (LegendarySkin active in m_activeList)
		{
			active.ClearSuspendFlag(LegendarySkin.SuspendUpdateReason.UpdatePriority);
		}
	}

	void IService.Shutdown()
	{
		foreach (KeyValuePair<(string, Player.Side), ObjectSkinPair> item in m_assetPathToObjectMap)
		{
			UnityEngine.Object.Destroy(item.Value.Object);
		}
		m_assetPathToObjectMap.Clear();
		m_objectIDToAssetPathMap.Clear();
		m_objectIDReferenceCounts.Clear();
	}

	private void OnSceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		foreach (KeyValuePair<(string, Player.Side), ObjectSkinPair> item in m_assetPathToObjectMap)
		{
			GameObject prefabInstance = item.Value.Object;
			if ((bool)prefabInstance)
			{
				prefabInstance.SetActive(value: false);
				prefabInstance.SetActive(value: true);
			}
		}
	}

	private GameObject GetOrCreateGameObject(string assetPath, Player.Side playerSide)
	{
		if (string.IsNullOrEmpty(assetPath))
		{
			return null;
		}
		if (m_assetPathToObjectMap.TryGetValue((assetPath, playerSide), out var objectSkinPair))
		{
			int id = objectSkinPair.Object.GetInstanceID();
			int refCount = m_objectIDReferenceCounts[id];
			m_objectIDReferenceCounts[id] = refCount + 1;
			return objectSkinPair.Object;
		}
		GameObject gameObject = AssetLoader.Get().InstantiatePrefab(assetPath);
		if (gameObject != null)
		{
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			int id2 = gameObject.GetInstanceID();
			m_assetPathToObjectMap[(assetPath, playerSide)] = new ObjectSkinPair
			{
				Object = gameObject,
				Skin = gameObject.GetComponentInChildren<LegendarySkin>()
			};
			m_objectIDReferenceCounts[id2] = 1;
			m_objectIDToAssetPathMap[id2] = (assetPath, playerSide);
		}
		return gameObject;
	}

	private void ReleaseGameObject(GameObject gameObject)
	{
		int instanceID = gameObject.GetInstanceID();
		if (m_objectIDReferenceCounts.TryGetValue(instanceID, out var refCount))
		{
			if (refCount > 1)
			{
				m_objectIDReferenceCounts[instanceID] = refCount - 1;
				return;
			}
			(string, Player.Side) key = m_objectIDToAssetPathMap[instanceID];
			m_objectIDReferenceCounts.Remove(instanceID);
			m_objectIDToAssetPathMap.Remove(instanceID);
			m_assetPathToObjectMap.Remove(key);
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	public ILegendaryHeroPortrait CreatePortrait(string assetPath, Player.Side playerSide)
	{
		return new HeroPortraitHandleInternal(this, assetPath, playerSide);
	}
}
