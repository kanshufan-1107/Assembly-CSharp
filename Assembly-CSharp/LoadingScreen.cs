using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Services;
using Hearthstone;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
	public delegate void PreviousSceneDestroyedCallback(object userData);

	public delegate void FinishedTransitionCallback(bool cutoff, object userData);

	public enum Phase
	{
		INVALID,
		WAITING_FOR_SCENE_UNLOAD,
		WAITING_FOR_SCENE_LOAD,
		WAITING_FOR_BLOCKERS,
		FADING_OUT,
		FADING_IN
	}

	private class PreviousSceneDestroyedListener : EventListener<PreviousSceneDestroyedCallback>
	{
		public void Fire()
		{
			m_callback(m_userData);
		}
	}

	private class FinishedTransitionListener : EventListener<FinishedTransitionCallback>
	{
		public void Fire(bool cutoff)
		{
			m_callback(cutoff, m_userData);
		}
	}

	private class TransitionParams
	{
		private List<GameObject> m_objects = new List<GameObject>();

		private List<Camera> m_cameras = new List<Camera>();

		private List<Light> m_lights = new List<Light>();

		private Camera m_freezeFrameCamera;

		private AudioListener m_audioListener;

		private int m_blockerCount;

		private bool m_fadeOut = true;

		private bool m_fadeIn = true;

		private Color m_fadeColor = Color.black;

		private bool m_clearPreviousAssets = true;

		public bool ClearPreviousAssets
		{
			get
			{
				return m_clearPreviousAssets;
			}
			set
			{
				m_clearPreviousAssets = value;
			}
		}

		public void AddObject(Component c)
		{
			if (!(c == null))
			{
				AddObject(c.gameObject);
			}
		}

		public void AddObject(GameObject go)
		{
			if (go == null)
			{
				return;
			}
			Transform t = go.transform;
			while (t != null)
			{
				if (m_objects.Contains(t.gameObject))
				{
					return;
				}
				t = t.parent;
			}
			Camera[] componentsInChildren = go.GetComponentsInChildren<Camera>();
			foreach (Camera camera in componentsInChildren)
			{
				if (!m_cameras.Contains(camera))
				{
					m_cameras.Add(camera);
				}
			}
			m_objects.Add(go);
		}

		public void AddBlocker()
		{
			m_blockerCount++;
		}

		public void AddBlocker(int count)
		{
			m_blockerCount += count;
		}

		public void RemoveBlocker()
		{
			m_blockerCount--;
		}

		public void RemoveBlocker(int count)
		{
			m_blockerCount -= count;
		}

		public int GetBlockerCount()
		{
			return m_blockerCount;
		}

		public void SetFreezeFrameCamera(Camera camera)
		{
			if (!(camera == null))
			{
				m_freezeFrameCamera = camera;
				AddObject(camera.gameObject);
			}
		}

		public Camera GetFreezeFrameCamera()
		{
			return m_freezeFrameCamera;
		}

		public AudioListener GetAudioListener()
		{
			return m_audioListener;
		}

		public void SetAudioListener(AudioListener listener)
		{
			if (!(listener == null))
			{
				m_audioListener = listener;
				AddObject(listener);
			}
		}

		public void EnableFadeOut(bool enable)
		{
			m_fadeOut = enable;
		}

		public bool IsFadeOutEnabled()
		{
			return m_fadeOut;
		}

		public void EnableFadeIn(bool enable)
		{
			m_fadeIn = enable;
		}

		public bool IsFadeInEnabled()
		{
			return m_fadeIn;
		}

		public void SetFadeColor(Color color)
		{
			m_fadeColor = color;
		}

		public Color GetFadeColor()
		{
			return m_fadeColor;
		}

		public void FixCameraTagsAndDepths(float fxCameraDepth)
		{
			if (m_cameras.Count != 0)
			{
				UntagCameras();
				BoostCamerasToJustBelowDepth(fxCameraDepth);
			}
		}

		private void UntagCameras()
		{
			foreach (Camera camera in m_cameras)
			{
				if (!(camera == null))
				{
					camera.tag = "Untagged";
				}
			}
		}

		private void BoostCamerasToJustBelowDepth(float targetDepth)
		{
			float highestDepth = GetHighestCameraDepth();
			float depthBoost = targetDepth - 1f - highestDepth;
			for (int i = 0; i < m_cameras.Count; i++)
			{
				Camera camera = m_cameras[i];
				if (!(camera == null))
				{
					camera.depth += depthBoost;
				}
			}
		}

		public float GetHighestCameraDepth()
		{
			if (m_cameras.Count == 0)
			{
				return 0f;
			}
			float highestDepth = 0f;
			for (int i = 0; i < m_cameras.Count; i++)
			{
				if (!(m_cameras[i] == null))
				{
					float cameraDepth = m_cameras[i].depth;
					if (highestDepth < cameraDepth)
					{
						highestDepth = cameraDepth;
					}
				}
			}
			return highestDepth;
		}

		public void AutoAddObjects()
		{
			Light[] array = (Light[])UnityEngine.Object.FindObjectsOfType(typeof(Light));
			foreach (Light light in array)
			{
				AddObject(light.gameObject);
				m_lights.Add(light);
			}
		}

		public void PreserveObjects(Transform parent)
		{
			foreach (GameObject go in m_objects)
			{
				if (!(go == null))
				{
					go.transform.parent = parent;
				}
			}
		}

		public void DestroyObjects()
		{
			foreach (GameObject @object in m_objects)
			{
				UnityEngine.Object.DestroyImmediate(@object);
			}
		}
	}

	private class TransitionUnfriendlyData
	{
		private AudioListener m_audioListener;

		private List<Light> m_lights = new List<Light>();

		public void Clear()
		{
			m_audioListener = null;
			m_lights.Clear();
		}

		public void SetAudioListener(AudioListener listener)
		{
			if (!(listener == null) && listener.enabled)
			{
				m_audioListener = listener;
				m_audioListener.enabled = false;
			}
		}

		public void AddLights(Light[] lights)
		{
			foreach (Light light in lights)
			{
				if (light.enabled)
				{
					light.enabled = false;
					Transform lightRoot = light.transform;
					while (lightRoot.parent != null)
					{
						lightRoot = lightRoot.parent;
					}
					m_lights.Add(light);
				}
			}
		}

		public void Restore()
		{
			for (int i = 0; i < m_lights.Count; i++)
			{
				Light light = m_lights[i];
				if (light == null)
				{
					Debug.LogError($"TransitionUnfriendlyData.Restore() - light {i} is null!");
					continue;
				}
				Transform lightRoot = light.transform;
				while (lightRoot.parent != null)
				{
					lightRoot = lightRoot.parent;
				}
				light.enabled = true;
			}
			if (m_audioListener != null)
			{
				m_audioListener.enabled = true;
			}
		}
	}

	public float m_FadeOutSec = 1f;

	public iTween.EaseType m_FadeOutEaseType = iTween.EaseType.linear;

	public float m_FadeInSec = 1f;

	public iTween.EaseType m_FadeInEaseType = iTween.EaseType.linear;

	private const float MIDDLE_OF_NOWHERE_X = 5000f;

	private Phase m_phase;

	private bool m_previousSceneActive;

	private TransitionParams m_prevTransitionParams;

	private TransitionParams m_transitionParams = new TransitionParams();

	private TransitionUnfriendlyData m_transitionUnfriendlyData = new TransitionUnfriendlyData();

	private Camera m_fxCamera;

	private List<PreviousSceneDestroyedListener> m_prevSceneDestroyedListeners = new List<PreviousSceneDestroyedListener>();

	private List<FinishedTransitionListener> m_finishedTransitionListeners = new List<FinishedTransitionListener>();

	private float m_originalPosX;

	private long m_assetLoadStartTimestamp;

	private long m_assetLoadEndTimestamp;

	private long m_assetLoadNextStartTimestamp;

	public event Action OnFadeInStart;

	private void Awake()
	{
		InitializeFxCamera();
	}

	public static LoadingScreen Get()
	{
		if (ServiceManager.TryGet<SceneMgr>(out var sceneMgr))
		{
			return sceneMgr.LoadingScreen;
		}
		return null;
	}

	public Camera GetFxCamera()
	{
		return m_fxCamera;
	}

	public CameraFade GetCameraFade()
	{
		return GetComponent<CameraFade>();
	}

	public void RegisterSceneListeners(SceneMgr sceneMgr)
	{
		sceneMgr.RegisterScenePreUnloadEvent(OnScenePreUnload);
		sceneMgr.RegisterSceneUnloadedEvent(OnSceneUnloaded);
		sceneMgr.RegisterSceneLoadedEvent(OnSceneLoaded);
	}

	public void UnregisterSceneListeners(SceneMgr sceneMgr)
	{
		sceneMgr.UnregisterScenePreUnloadEvent(OnScenePreUnload);
		sceneMgr.UnregisterSceneUnloadedEvent(OnSceneUnloaded);
		sceneMgr.UnregisterSceneLoadedEvent(OnSceneLoaded);
	}

	public static bool DoesShowLoadingScreen(SceneMgr.Mode prevMode, SceneMgr.Mode nextMode)
	{
		if (prevMode == SceneMgr.Mode.GAMEPLAY)
		{
			return true;
		}
		if (nextMode == SceneMgr.Mode.GAMEPLAY)
		{
			return true;
		}
		return false;
	}

	public Phase GetPhase()
	{
		return m_phase;
	}

	public bool IsTransitioning()
	{
		return m_phase != Phase.INVALID;
	}

	public bool IsWaiting()
	{
		Phase phase = m_phase;
		if ((uint)(phase - 1) <= 2u)
		{
			return true;
		}
		return false;
	}

	public bool IsFadingOut()
	{
		return m_phase == Phase.FADING_OUT;
	}

	public bool IsFadingIn()
	{
		return m_phase == Phase.FADING_IN;
	}

	public bool IsFading()
	{
		if (IsFadingOut())
		{
			return true;
		}
		if (IsFadingIn())
		{
			return true;
		}
		return false;
	}

	public bool IsPreviousSceneActive()
	{
		return m_previousSceneActive;
	}

	public void AddTransitionObject(GameObject go)
	{
		m_transitionParams.AddObject(go);
	}

	public void AddTransitionObject(Component c)
	{
		m_transitionParams.AddObject(c.gameObject);
	}

	public void AddTransitionBlocker()
	{
		m_transitionParams.AddBlocker();
	}

	public void AddTransitionBlocker(int count)
	{
		m_transitionParams.AddBlocker(count);
	}

	public Camera GetFreezeFrameCamera()
	{
		return m_transitionParams.GetFreezeFrameCamera();
	}

	public void SetFreezeFrameCamera(Camera camera)
	{
		m_transitionParams.SetFreezeFrameCamera(camera);
	}

	public AudioListener GetTransitionAudioListener()
	{
		return m_transitionParams.GetAudioListener();
	}

	public void SetTransitionAudioListener(AudioListener listener)
	{
		Log.LoadingScreen.Print("LoadingScreen.SetTransitionAudioListener() - {0}", listener);
		m_transitionParams.SetAudioListener(listener);
	}

	public void EnableFadeOut(bool enable)
	{
		m_transitionParams.EnableFadeOut(enable);
	}

	public void EnableFadeIn(bool enable)
	{
		m_transitionParams.EnableFadeIn(enable);
	}

	public Color GetFadeColor()
	{
		return m_transitionParams.GetFadeColor();
	}

	public void SetFadeColor(Color color)
	{
		m_transitionParams.SetFadeColor(color);
	}

	public void NotifyTransitionBlockerComplete()
	{
		if (m_prevTransitionParams != null)
		{
			m_prevTransitionParams.RemoveBlocker();
			TransitionIfPossible();
		}
	}

	public void NotifyTransitionBlockerComplete(int count)
	{
		if (m_prevTransitionParams != null)
		{
			m_prevTransitionParams.RemoveBlocker(count);
			TransitionIfPossible();
		}
	}

	public void NotifyMainSceneObjectAwoke(GameObject mainObject)
	{
		if (IsPreviousSceneActive())
		{
			DisableTransitionUnfriendlyStuff(mainObject);
		}
	}

	public long GetAssetLoadStartTimestamp()
	{
		return m_assetLoadStartTimestamp;
	}

	public void SetAssetLoadStartTimestamp(long timestamp)
	{
		m_assetLoadStartTimestamp = Math.Min(m_assetLoadStartTimestamp, timestamp);
		Log.LoadingScreen.Print("LoadingScreen.SetAssetLoadStartTimestamp() - m_assetLoadStartTimestamp={0}", m_assetLoadStartTimestamp);
	}

	public bool RegisterPreviousSceneDestroyedListener(PreviousSceneDestroyedCallback callback)
	{
		return RegisterPreviousSceneDestroyedListener(callback, null);
	}

	public bool RegisterPreviousSceneDestroyedListener(PreviousSceneDestroyedCallback callback, object userData)
	{
		PreviousSceneDestroyedListener listener = new PreviousSceneDestroyedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_prevSceneDestroyedListeners.Contains(listener))
		{
			return false;
		}
		m_prevSceneDestroyedListeners.Add(listener);
		return true;
	}

	public bool UnregisterPreviousSceneDestroyedListener(PreviousSceneDestroyedCallback callback)
	{
		return UnregisterPreviousSceneDestroyedListener(callback, null);
	}

	public bool UnregisterPreviousSceneDestroyedListener(PreviousSceneDestroyedCallback callback, object userData)
	{
		PreviousSceneDestroyedListener listener = new PreviousSceneDestroyedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_prevSceneDestroyedListeners.Remove(listener);
	}

	public bool RegisterFinishedTransitionListener(FinishedTransitionCallback callback)
	{
		return RegisterFinishedTransitionListener(callback, null);
	}

	public bool RegisterFinishedTransitionListener(FinishedTransitionCallback callback, object userData)
	{
		FinishedTransitionListener listener = new FinishedTransitionListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (m_finishedTransitionListeners.Contains(listener))
		{
			return false;
		}
		m_finishedTransitionListeners.Add(listener);
		return true;
	}

	public bool UnregisterFinishedTransitionListener(FinishedTransitionCallback callback)
	{
		return UnregisterFinishedTransitionListener(callback, null);
	}

	public bool UnregisterFinishedTransitionListener(FinishedTransitionCallback callback, object userData)
	{
		FinishedTransitionListener listener = new FinishedTransitionListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_finishedTransitionListeners.Remove(listener);
	}

	private void OnScenePreUnload(SceneMgr.Mode prevMode, PegasusScene prevScene, object userData)
	{
		Log.LoadingScreen.Print("LoadingScreen.OnScenePreUnload() - prevMode={0} nextMode={1} m_phase={2}", prevMode, SceneMgr.Get().GetMode(), m_phase);
		if (!DoesShowLoadingScreen(prevMode, SceneMgr.Get().GetMode()))
		{
			CutoffTransition();
			return;
		}
		if (IsTransitioning())
		{
			DoInterruptionCleanUp();
		}
		m_assetLoadNextStartTimestamp = TimeUtils.BinaryStamp();
		if (IsTransitioning())
		{
			FireFinishedTransitionListeners(cutoff: true);
			if (IsPreviousSceneActive())
			{
				return;
			}
		}
		m_phase = Phase.WAITING_FOR_SCENE_UNLOAD;
		m_previousSceneActive = true;
		ShowFreezeFrame(m_transitionParams.GetFreezeFrameCamera());
	}

	private void OnSceneUnloaded(SceneMgr.Mode prevMode, PegasusScene prevScene, object userData)
	{
		Log.LoadingScreen.Print("LoadingScreen.OnSceneUnloaded() - prevMode={0} nextMode={1} m_phase={2}", prevMode, SceneMgr.Get().GetMode(), m_phase);
		if (m_phase != Phase.WAITING_FOR_SCENE_UNLOAD)
		{
			return;
		}
		m_assetLoadEndTimestamp = m_assetLoadNextStartTimestamp;
		Log.LoadingScreen.Print("LoadingScreen.OnSceneUnloaded() - m_assetLoadEndTimestamp={0}", m_assetLoadEndTimestamp);
		m_phase = Phase.WAITING_FOR_SCENE_LOAD;
		m_prevTransitionParams = m_transitionParams;
		m_transitionParams = new TransitionParams();
		m_transitionParams.ClearPreviousAssets = prevMode != SceneMgr.Get().GetMode();
		m_prevTransitionParams.AutoAddObjects();
		if (m_fxCamera != null)
		{
			float highestCameraDepth = m_prevTransitionParams.GetHighestCameraDepth();
			if (highestCameraDepth >= m_fxCamera.depth - 1f)
			{
				Debug.LogWarning("FX Camera's depth was less than or equal to previous transition's camera stack. Moving to a higher depth");
				m_fxCamera.depth = highestCameraDepth + 2f;
			}
			m_prevTransitionParams.FixCameraTagsAndDepths(m_fxCamera.depth);
		}
		m_prevTransitionParams.PreserveObjects(base.transform);
		m_originalPosX = base.transform.position.x;
		TransformUtil.SetPosX(base.gameObject, 5000f);
	}

	private void OnSceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		Log.LoadingScreen.Print("LoadingScreen.OnSceneLoaded() - prevMode={0} currMode={1}", SceneMgr.Get().GetPrevMode(), mode);
		if (mode == SceneMgr.Mode.FATAL_ERROR)
		{
			Log.LoadingScreen.Print("LoadingScreen.OnSceneLoaded() - calling CutoffTransition()", mode);
			CutoffTransition();
			return;
		}
		if (SceneMgr.Get().GetPrevMode() == SceneMgr.Mode.STARTUP)
		{
			m_assetLoadStartTimestamp = TimeUtils.BinaryStamp();
			Log.LoadingScreen.Print("LoadingScreen.OnSceneLoaded() - m_assetLoadStartTimestamp={0}", m_assetLoadStartTimestamp);
		}
		if (m_phase != Phase.WAITING_FOR_SCENE_LOAD)
		{
			Log.LoadingScreen.Print("LoadingScreen.OnSceneLoaded() - END - {0} != Phase.WAITING_FOR_SCENE_LOAD", m_phase);
		}
		else
		{
			m_phase = Phase.WAITING_FOR_BLOCKERS;
			TransitionIfPossible();
		}
	}

	private bool TransitionIfPossible()
	{
		if (m_prevTransitionParams.GetBlockerCount() > 0)
		{
			return false;
		}
		StartCoroutine("HackWaitThenStartTransitionEffects");
		return true;
	}

	private IEnumerator HackWaitThenStartTransitionEffects()
	{
		Log.LoadingScreen.Print("LoadingScreen.HackWaitThenStartTransitionEffects() - START");
		yield return new WaitForEndOfFrame();
		if (m_phase != Phase.WAITING_FOR_BLOCKERS)
		{
			Log.LoadingScreen.Print("LoadingScreen.HackWaitThenStartTransitionEffects() - END - {0} != Phase.WAITING_FOR_BLOCKERS", m_phase);
		}
		else
		{
			FadeOut();
		}
	}

	private void FirePreviousSceneDestroyedListeners()
	{
		PreviousSceneDestroyedListener[] listeners = m_prevSceneDestroyedListeners.ToArray();
		for (int i = 0; i < listeners.Length; i++)
		{
			listeners[i].Fire();
		}
	}

	private void FireFinishedTransitionListeners(bool cutoff)
	{
		FinishedTransitionListener[] listeners = m_finishedTransitionListeners.ToArray();
		for (int i = 0; i < listeners.Length; i++)
		{
			listeners[i].Fire(cutoff);
		}
	}

	private void FadeOut()
	{
		Log.LoadingScreen.Print("LoadingScreen.FadeOut()");
		m_phase = Phase.FADING_OUT;
		if (!m_prevTransitionParams.IsFadeOutEnabled())
		{
			OnFadeOutComplete();
			return;
		}
		CameraFade cameraFade = GetComponent<CameraFade>();
		if (cameraFade == null)
		{
			Debug.LogError("LoadingScreen FadeOut(): Failed to find CameraFade component");
			return;
		}
		cameraFade.m_Color = m_prevTransitionParams.GetFadeColor();
		Action<object> OnFadeUpdate = delegate(object amount)
		{
			cameraFade.m_Fade = (float)amount;
		};
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("time", m_FadeOutSec);
		args.Add("from", cameraFade.m_Fade);
		args.Add("to", 1f);
		args.Add("onupdate", OnFadeUpdate);
		args.Add("onupdatetarget", base.gameObject);
		args.Add("oncomplete", "OnFadeOutComplete");
		args.Add("oncompletetarget", base.gameObject);
		args.Add("name", "Fade");
		iTween.ValueTo(base.gameObject, args);
	}

	private void OnFadeOutComplete()
	{
		Log.LoadingScreen.Print("LoadingScreen.OnFadeOutComplete()");
		FinishPreviousScene();
		FadeIn();
	}

	private void FadeIn()
	{
		Log.LoadingScreen.Print("LoadingScreen.FadeIn()");
		m_phase = Phase.FADING_IN;
		this.OnFadeInStart?.Invoke();
		if (!m_prevTransitionParams.IsFadeInEnabled())
		{
			OnFadeInComplete();
			return;
		}
		CameraFade cameraFade = GetComponent<CameraFade>();
		if (cameraFade == null)
		{
			Debug.LogError("LoadingScreen FadeIn(): Failed to find CameraFade component");
			return;
		}
		cameraFade.m_Color = m_prevTransitionParams.GetFadeColor();
		Action<object> OnFadeUpdate = delegate(object amount)
		{
			cameraFade.m_Fade = (float)amount;
		};
		OnFadeUpdate(1f);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("time", m_FadeInSec);
		args.Add("from", 1f);
		args.Add("to", 0f);
		args.Add("onupdate", OnFadeUpdate);
		args.Add("onupdatetarget", base.gameObject);
		args.Add("oncomplete", "OnFadeInComplete");
		args.Add("oncompletetarget", base.gameObject);
		args.Add("name", "Fade");
		iTween.ValueTo(base.gameObject, args);
	}

	private void OnFadeInComplete()
	{
		Log.LoadingScreen.Print("LoadingScreen.OnFadeInComplete()");
		FinishFxCamera();
		m_prevTransitionParams = null;
		m_phase = Phase.INVALID;
		FireFinishedTransitionListeners(cutoff: false);
	}

	private void InitializeFxCamera()
	{
		m_fxCamera = GetComponent<Camera>();
		if (m_fxCamera != null)
		{
			m_fxCamera.allowMSAA = ServiceManager.Get<IGraphicsManager>().AllowMSAA();
		}
	}

	private void FinishFxCamera()
	{
		CameraFade cameraFade = GetComponent<CameraFade>();
		if (cameraFade == null)
		{
			Debug.LogError("LoadingScreen.FinishFxCamera(): Failed to find CameraFade component");
		}
		else if (cameraFade.m_Fade > 0f)
		{
			Action<object> OnFadeUpdate = delegate(object amount)
			{
				cameraFade.m_Fade = (float)amount;
			};
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("time", 0.3f);
			args.Add("from", cameraFade.m_Fade);
			args.Add("to", 0f);
			args.Add("onupdate", OnFadeUpdate);
			args.Add("onupdatetarget", base.gameObject);
			args.Add("oncompletetarget", base.gameObject);
			args.Add("delay", 0.5f);
			args.Add("name", "Fade");
			iTween.ValueTo(base.gameObject, args);
		}
	}

	private FreezeFrame GetFreezeFrameEffect(Camera camera)
	{
		FreezeFrame freezeFrame = camera.GetComponent<FreezeFrame>();
		if (freezeFrame != null)
		{
			return freezeFrame;
		}
		return camera.gameObject.AddComponent<FreezeFrame>();
	}

	private void ShowFreezeFrame(Camera camera)
	{
		if (!(camera == null))
		{
			FreezeFrame freezeFrameEffect = GetFreezeFrameEffect(camera);
			freezeFrameEffect.enabled = true;
			freezeFrameEffect.Freeze();
		}
	}

	private void CutoffTransition()
	{
		if (!IsTransitioning())
		{
			m_transitionParams = new TransitionParams();
			return;
		}
		StopFading();
		FinishPreviousScene();
		FinishFxCamera();
		m_prevTransitionParams = null;
		m_transitionParams = new TransitionParams();
		m_phase = Phase.INVALID;
		FireFinishedTransitionListeners(cutoff: true);
	}

	private void StopFading()
	{
		iTween.Stop(base.gameObject);
	}

	private void DoInterruptionCleanUp()
	{
		bool previousSceneActive = IsPreviousSceneActive();
		Log.LoadingScreen.Print("LoadingScreen.DoInterruptionCleanUp() - m_phase={0} previousSceneActive={1}", m_phase, previousSceneActive);
		if (m_phase == Phase.WAITING_FOR_BLOCKERS)
		{
			StopCoroutine("HackWaitThenStartTransitionEffects");
		}
		if (IsFading())
		{
			StopFading();
			if (IsFadingIn())
			{
				m_prevTransitionParams = null;
			}
		}
		if (previousSceneActive)
		{
			long startTimestamp = m_assetLoadNextStartTimestamp;
			long endTimestamp = TimeUtils.BinaryStamp();
			ClearAssets(startTimestamp, endTimestamp);
			m_transitionUnfriendlyData.Clear();
			m_transitionParams = new TransitionParams();
			m_phase = Phase.WAITING_FOR_SCENE_LOAD;
		}
	}

	private void FinishPreviousScene()
	{
		Log.LoadingScreen.Print("LoadingScreen.FinishPreviousScene()");
		if (m_prevTransitionParams != null)
		{
			m_prevTransitionParams.DestroyObjects();
			TransformUtil.SetPosX(base.gameObject, m_originalPosX);
		}
		if (m_transitionParams.ClearPreviousAssets)
		{
			ClearPreviousSceneAssets();
		}
		m_transitionUnfriendlyData.Restore();
		m_transitionUnfriendlyData.Clear();
		m_previousSceneActive = false;
		FirePreviousSceneDestroyedListeners();
	}

	private void ClearPreviousSceneAssets()
	{
		Log.LoadingScreen.Print("LoadingScreen.ClearPreviousSceneAssets() - START m_assetLoadStartTimestamp={0} m_assetLoadEndTimestamp={1}", m_assetLoadStartTimestamp, m_assetLoadEndTimestamp);
		ClearAssets(m_assetLoadStartTimestamp, m_assetLoadEndTimestamp);
		m_assetLoadStartTimestamp = m_assetLoadNextStartTimestamp;
		m_assetLoadEndTimestamp = 0L;
		m_assetLoadNextStartTimestamp = 0L;
		Log.LoadingScreen.Print("LoadingScreen.ClearPreviousSceneAssets() - END m_assetLoadStartTimestamp={0} m_assetLoadEndTimestamp={1}", m_assetLoadStartTimestamp, m_assetLoadEndTimestamp);
	}

	private void ClearAssets(long startTimestamp, long endTimestamp)
	{
		HearthstoneApplication hearthstoneApplication = HearthstoneApplication.Get();
		if (hearthstoneApplication != null)
		{
			hearthstoneApplication.UnloadUnusedAssets();
		}
	}

	private void DisableTransitionUnfriendlyStuff(GameObject mainObject)
	{
		Log.LoadingScreen.Print("LoadingScreen.DisableTransitionUnfriendlyStuff() - {0}", mainObject);
		AudioListener[] componentsInChildren = GetComponentsInChildren<AudioListener>();
		bool atLeastOneListener = false;
		AudioListener[] array = componentsInChildren;
		foreach (AudioListener existingListener in array)
		{
			atLeastOneListener |= existingListener != null && existingListener.enabled;
		}
		if (atLeastOneListener)
		{
			AudioListener listener = mainObject.GetComponentInChildren<AudioListener>();
			m_transitionUnfriendlyData.SetAudioListener(listener);
		}
		Light[] lights = mainObject.GetComponentsInChildren<Light>();
		m_transitionUnfriendlyData.AddLights(lights);
	}
}
