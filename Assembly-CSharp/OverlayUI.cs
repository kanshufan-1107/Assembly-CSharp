using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Hearthstone;
using Hearthstone.Core;
using UnityEngine;

public class OverlayUI : MonoBehaviour
{
	public CanvasAnchors m_heightScale;

	public CanvasAnchors m_widthScale;

	public RectTransform m_inputFieldOverlayRect;

	public InputFieldUI m_inputFieldUI;

	public Transform m_BoneParent;

	public GameObject m_clickBlocker;

	public GameObject m_QuestProgressToastBone;

	public Camera m_UICamera;

	private const string REGISTER_ONSCENECHANGE_JOB_NAME = "OverlayUI.OnSceneChange";

	private static OverlayUI s_instance;

	private HashSet<GameObject> m_destroyOnSceneLoad = new HashSet<GameObject>();

	private bool m_clickBlockerRequested;

	private void Awake()
	{
		s_instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
		HearthstoneApplication.Get().WillReset += WillReset;
		UniversalInputManager.Get().SetTextInputField(m_inputFieldUI);
		m_UICamera = CameraUtils.FindFirstByLayer(GameLayer.BattleNet);
		Processor.QueueJobIfNotExist("OverlayUI.OnSceneChange", Job_RegisterOnSceneChange());
	}

	private void Update()
	{
		if (m_clickBlocker != null)
		{
			m_clickBlocker.SetActive(m_clickBlockerRequested);
		}
		m_clickBlockerRequested = false;
	}

	private void OnDestroy()
	{
		if (HearthstoneApplication.Get() != null)
		{
			HearthstoneApplication.Get().WillReset -= WillReset;
		}
		s_instance = null;
	}

	public static OverlayUI Get()
	{
		return s_instance;
	}

	public void AddGameObject(GameObject go, CanvasAnchor anchor = CanvasAnchor.CENTER, bool destroyOnSceneLoad = false, CanvasScaleMode scaleMode = CanvasScaleMode.HEIGHT)
	{
		CanvasAnchors scaleType = ((scaleMode == CanvasScaleMode.HEIGHT) ? m_heightScale : m_widthScale);
		TransformUtil.AttachAndPreserveLocalTransform(go.transform, scaleType.GetAnchor(anchor));
		if (destroyOnSceneLoad)
		{
			DestroyOnSceneLoad(go);
		}
	}

	public bool HasObject(GameObject gameObject)
	{
		if (gameObject == null)
		{
			return false;
		}
		return gameObject.transform.IsChildOf(base.transform);
	}

	public Vector3 GetRelativePosition(Vector3 worldPosition, Camera camera = null, Transform bone = null, float depth = 0f)
	{
		if (camera == null)
		{
			camera = ((SceneMgr.Get() == null || SceneMgr.Get().GetMode() != SceneMgr.Mode.GAMEPLAY) ? Box.Get().GetBoxCamera().GetComponent<Camera>() : BoardCameras.Get().GetComponentInChildren<Camera>());
		}
		if (bone == null)
		{
			bone = m_heightScale.m_Center;
		}
		Vector3 screenPoint = camera.WorldToScreenPoint(worldPosition);
		Vector3 overlayPoint = m_UICamera.ScreenToWorldPoint(screenPoint);
		overlayPoint.y = depth;
		return bone.InverseTransformPoint(overlayPoint);
	}

	public Rect GetInputFieldRect(Rect normalizedInputRect)
	{
		Vector2 min = normalizedInputRect.min;
		Vector2 max = normalizedInputRect.max;
		min = Rect.NormalizedToPoint(m_inputFieldOverlayRect.rect, min);
		max = Rect.NormalizedToPoint(m_inputFieldOverlayRect.rect, max);
		return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
	}

	public void DestroyOnSceneLoad(GameObject go)
	{
		if (!m_destroyOnSceneLoad.Contains(go))
		{
			m_destroyOnSceneLoad.Add(go);
		}
	}

	public void DontDestroyOnSceneLoad(GameObject go)
	{
		if (m_destroyOnSceneLoad.Contains(go))
		{
			m_destroyOnSceneLoad.Remove(go);
		}
	}

	public Transform FindBone(string name)
	{
		if (m_BoneParent != null)
		{
			Transform bone = m_BoneParent.Find(name);
			if (bone != null)
			{
				return bone;
			}
		}
		return base.transform;
	}

	public void RequestActivateClickBlocker()
	{
		m_clickBlockerRequested = true;
	}

	private void OnSceneChange(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		m_destroyOnSceneLoad?.RemoveWhere(delegate(GameObject go)
		{
			if (go != null)
			{
				Object.Destroy(go);
				return true;
			}
			return false;
		});
	}

	private void WillReset()
	{
		m_widthScale.WillReset();
		m_heightScale.WillReset();
	}

	private IEnumerator<IAsyncJobResult> Job_RegisterOnSceneChange()
	{
		while (SceneMgr.Get() == null)
		{
			yield return new WaitForDurationForWorker(500.0);
		}
		SceneMgr.Get().RegisterSceneLoadedEvent(OnSceneChange);
	}
}
