using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core;
using Unity.Profiling;
using UnityEngine;

public class FullScreenFXMgr : IHasLateUpdate, IService
{
	public class ScreenEffectsInstance : IComparable
	{
		public object Owner;

		public FullScreenEffects EffectsComponent;

		public ScreenEffectParameters Parameters;

		public Action OnFinishedCallback;

		public bool Released;

		public Camera Camera
		{
			get
			{
				if (!(EffectsComponent != null))
				{
					return null;
				}
				return EffectsComponent.Camera;
			}
		}

		public ScreenEffectsInstance(object owner)
		{
			Owner = owner;
			EffectsComponent = null;
			Parameters = ScreenEffectParameters.None;
		}

		public void Initialize(ScreenEffectParameters effectParameters)
		{
			Parameters = effectParameters;
			Released = false;
		}

		public void Reset()
		{
			Parameters = ScreenEffectParameters.None;
			EffectsComponent = null;
			Released = false;
		}

		public int CompareTo(object obj)
		{
			if (!(obj is ScreenEffectsInstance other))
			{
				return -1;
			}
			if (Camera == null && other.Camera == null)
			{
				return 0;
			}
			if (Camera != null && other.Camera == null)
			{
				return -1;
			}
			if (Camera == null && other.Camera != null)
			{
				return 1;
			}
			if (Camera.depth > other.Camera.depth)
			{
				return -1;
			}
			if (Camera.depth < other.Camera.depth)
			{
				return 1;
			}
			return 0;
		}
	}

	private FullScreenEffects m_ActiveCameraFullScreenEffects;

	private FullScreenEffects m_SecondaryCameraFullScreenEffects;

	private List<ScreenEffectsInstance> m_screenEffects = new List<ScreenEffectsInstance>(10);

	private List<ScreenEffectsInstance> m_screenEffectsForImmediateRemoval = new List<ScreenEffectsInstance>(10);

	private bool m_stackDirty;

	private ProfilerMarker m_updateProfilerMarker;

	private ProfilerMarker m_resolveProfilerMarker;

	private ProfilerMarker m_addEffectProfilerMarker;

	private ProfilerMarker m_releaseEffectProfilerMarker;

	public FullScreenEffects ActiveCameraFullScreenEffects
	{
		get
		{
			if (m_ActiveCameraFullScreenEffects == null)
			{
				Camera mainCamera = CameraUtils.GetMainCamera();
				if (mainCamera == null)
				{
					Log.FullScreenFX.PrintError("Could not find Box Camera");
					return null;
				}
				if (!mainCamera.TryGetComponent<FullScreenEffects>(out var fse))
				{
					Log.FullScreenFX.PrintError("Could not find Perspective/Active FullScreen Effects component");
					return null;
				}
				m_ActiveCameraFullScreenEffects = fse;
			}
			return m_ActiveCameraFullScreenEffects;
		}
	}

	public FullScreenEffects SecondaryCameraFullScreenEffects
	{
		get
		{
			if (m_SecondaryCameraFullScreenEffects == null)
			{
				Camera secondaryCamera = CameraUtils.FindFirstByLayer(GameLayer.BattleNet);
				if (secondaryCamera == null)
				{
					Log.FullScreenFX.PrintError("Could not find secondary camera");
					return null;
				}
				if (!secondaryCamera.TryGetComponent<FullScreenEffects>(out var fse))
				{
					Log.FullScreenFX.PrintError("Could not find Orthographic/Secondary FullScreen Effects component");
					return null;
				}
				m_SecondaryCameraFullScreenEffects = fse;
			}
			return m_SecondaryCameraFullScreenEffects;
		}
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		m_updateProfilerMarker = new ProfilerMarker("FullscreenFXMgr.LateUpdate");
		m_resolveProfilerMarker = new ProfilerMarker("FullscreenFXMgr.ResolveEffects");
		m_addEffectProfilerMarker = new ProfilerMarker("FullscreenFXMgr.AddEffect");
		m_releaseEffectProfilerMarker = new ProfilerMarker("FullscreenFXMgr.ReleaseEffect");
		if (HearthstoneApplication.Get() != null)
		{
			HearthstoneApplication.Get().WillReset += OnHSReset;
		}
		yield return new ServiceSoftDependency(typeof(SceneMgr), serviceLocator);
		if (serviceLocator.TryGetService<SceneMgr>(out var sceneMgr))
		{
			sceneMgr.RegisterScenePreLoadEvent(OnScenePreLoad);
		}
	}

	public Type[] GetDependencies()
	{
		return null;
	}

	public void Shutdown()
	{
		if (HearthstoneApplication.Get() != null)
		{
			HearthstoneApplication.Get().WillReset -= OnHSReset;
		}
		if (ServiceManager.TryGet<SceneMgr>(out var sceneMgr))
		{
			sceneMgr.UnregisterScenePreLoadEvent(OnScenePreLoad);
		}
	}

	public static FullScreenFXMgr Get()
	{
		return ServiceManager.Get<FullScreenFXMgr>();
	}

	public void LateUpdate()
	{
		using (m_updateProfilerMarker.Auto())
		{
			for (int i = m_screenEffects.Count - 1; i >= 0; i--)
			{
				if (!m_screenEffects[i].Released && (m_screenEffects[i].Owner == null || (m_screenEffects[i].Owner is UnityEngine.Object && (UnityEngine.Object)m_screenEffects[i].Owner == null)))
				{
					StopEffect(m_screenEffects[i]);
					if (m_screenEffects[i].EffectsComponent == null)
					{
						m_screenEffectsForImmediateRemoval.Add(m_screenEffects[i]);
					}
				}
				if (m_screenEffects.Count > 1 && m_screenEffects[i].Released)
				{
					m_stackDirty = true;
				}
			}
			if (m_screenEffectsForImmediateRemoval.Count > 0)
			{
				foreach (ScreenEffectsInstance instance in m_screenEffectsForImmediateRemoval)
				{
					RemoveFxInstanceFromStack(instance);
				}
				m_screenEffectsForImmediateRemoval.Clear();
			}
			if (m_stackDirty)
			{
				ResolveEffects();
			}
		}
	}

	public void AddEffect(ScreenEffectsHandle handle, ScreenEffectParameters effectParameters, Action onFinishedCallback = null)
	{
		using (m_addEffectProfilerMarker.Auto())
		{
			if (handle == null)
			{
				Log.FullScreenFX.PrintError("Invalid handle passed in to AddEffect! You should store and pass in a valid reference.");
				return;
			}
			if (handle.Owner == null)
			{
				Log.FullScreenFX.PrintError("Handle has an invalid owner! This effect request will be ignored!");
				return;
			}
			if (!m_screenEffects.Contains(handle.ScreenEffectsInstance))
			{
				m_screenEffects.Add(handle.ScreenEffectsInstance);
			}
			handle.ScreenEffectsInstance.Initialize(effectParameters);
			handle.SetFinishedCallback(onFinishedCallback);
			m_stackDirty = true;
		}
	}

	public void StopEffect(ScreenEffectsInstance fxInstance, bool immediate = false)
	{
		using (m_releaseEffectProfilerMarker.Auto())
		{
			if (!m_screenEffects.Contains(fxInstance))
			{
				Log.FullScreenFX.PrintWarning("Attempted to release a fullscreen effect that was not added!");
				return;
			}
			if (fxInstance.Released && !immediate)
			{
				Log.FullScreenFX.PrintWarning("Attempted to release a fullscreen effect that was already released!");
				return;
			}
			fxInstance.Parameters.Blur = BlurParameters.None;
			fxInstance.Parameters.Vignette = VignetteParameters.None;
			fxInstance.Parameters.Desaturate = DesaturateParameters.None;
			fxInstance.Parameters.BlendToColor = BlendToColorParameters.None;
			fxInstance.Released = true;
			if (immediate)
			{
				RemoveFxInstanceFromStack(fxInstance);
			}
			m_stackDirty = true;
		}
	}

	private void RemoveFxInstanceFromStack(ScreenEffectsInstance screenEffectsInstance)
	{
		if (m_screenEffects.Contains(screenEffectsInstance))
		{
			if (screenEffectsInstance.EffectsComponent != null && screenEffectsInstance.EffectsComponent.ActiveEffectsInstance == screenEffectsInstance)
			{
				screenEffectsInstance.EffectsComponent.CleanupEffects(screenEffectsInstance.Parameters.Time);
			}
			screenEffectsInstance.Reset();
			m_screenEffects.Remove(screenEffectsInstance);
			m_stackDirty = true;
		}
	}

	private void ResolveEffects()
	{
		using (m_resolveProfilerMarker.Auto())
		{
			m_screenEffects.Sort();
			if (m_screenEffects.Count > 1)
			{
				for (int i = m_screenEffects.Count - 1; i >= 0; i--)
				{
					if (m_screenEffects[i].Released)
					{
						RemoveFxInstanceFromStack(m_screenEffects[i]);
					}
				}
			}
			if (m_screenEffects.Count == 0)
			{
				m_stackDirty = false;
				return;
			}
			ScreenEffectsInstance effect = m_screenEffects[m_screenEffects.Count - 1];
			if (effect == null)
			{
				Log.FullScreenFX.PrintError("Could not find a ScreenEffectsInstance!");
				return;
			}
			FullScreenEffects effectsComponent = ((effect.Parameters.PassLocation == ScreenEffectPassLocation.PERSPECTIVE) ? ActiveCameraFullScreenEffects : SecondaryCameraFullScreenEffects);
			if (effectsComponent == null)
			{
				Log.FullScreenFX.PrintError("Could not find a FullScreenEffects component!");
				return;
			}
			effect.EffectsComponent = effectsComponent;
			effectsComponent.StartEffect(effect);
			m_stackDirty = false;
			UpdateInputManager();
		}
	}

	public void OnFinishedEffect(ScreenEffectsInstance screenEffectsInstance)
	{
		if (screenEffectsInstance.Released)
		{
			RemoveFxInstanceFromStack(screenEffectsInstance);
		}
	}

	public void ForceReset()
	{
		if (ActiveCameraFullScreenEffects != null)
		{
			ActiveCameraFullScreenEffects.Disable();
		}
	}

	private void OnHSReset()
	{
		for (int i = m_screenEffects.Count - 1; i >= 0; i--)
		{
			StopEffect(m_screenEffects[i]);
		}
		ForceReset();
	}

	private void OnScenePreLoad(SceneMgr.Mode prevMode, SceneMgr.Mode nextMode, object userData)
	{
		if (prevMode == SceneMgr.Mode.GAMEPLAY && nextMode != SceneMgr.Mode.HUB)
		{
			StopAllEffects();
		}
	}

	public void StopAllEffects(float delay = 0f)
	{
		FullScreenEffects activeCameraFullScreenFX = ActiveCameraFullScreenEffects;
		if (!(activeCameraFullScreenFX == null) && activeCameraFullScreenFX.IsActive)
		{
			Log.FullScreenFX.Print("StopAllEffects");
			Processor.RunCoroutine(StopAllEffectsCoroutine(activeCameraFullScreenFX, delay));
		}
	}

	private IEnumerator StopAllEffectsCoroutine(FullScreenEffects effects, float delay)
	{
		float stopEffectsTime = 0.25f;
		if (delay > 0f)
		{
			yield return new WaitForSeconds(delay);
		}
		Log.FullScreenFX.Print("StopAllEffectsCoroutine stopping effects now");
		foreach (ScreenEffectsInstance effectsInstance in m_screenEffects)
		{
			StopEffect(effectsInstance);
		}
		yield return new WaitForSeconds(stopEffectsTime);
		if (!(effects == null))
		{
			effects.Disable();
		}
	}

	private void UpdateInputManager()
	{
		if (UniversalInputManager.Get() != null)
		{
			FullScreenEffects effect = GetHighestActiveEffect();
			UniversalInputManager.Get().SetCurrentFullScreenEffect(effect);
		}
	}

	private FullScreenEffects GetHighestActiveEffect()
	{
		if (m_screenEffects.Count > 0)
		{
			for (int i = 0; i < m_screenEffects.Count; i++)
			{
				ScreenEffectsInstance fxInstance = m_screenEffects[i];
				if (!(fxInstance.EffectsComponent == null) && fxInstance.EffectsComponent.HasActiveEffects)
				{
					return fxInstance.EffectsComponent;
				}
			}
		}
		if (ActiveCameraFullScreenEffects != null && ActiveCameraFullScreenEffects.HasActiveEffects)
		{
			return ActiveCameraFullScreenEffects;
		}
		return null;
	}
}
