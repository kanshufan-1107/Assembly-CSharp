using System;
using System.Collections;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PageTurn : MonoBehaviour
{
	public delegate void DelOnPageTurnComplete(object callbackData);

	public delegate void DelPositionPages(object callbackData);

	private class PageTurningData
	{
		public float m_secondsToWait;

		public DelOnPageTurnComplete m_pageTurnCompleteCallback;

		public object m_callbackData;

		public AnimationState m_animation;

		public DelPositionPages m_positionPagesCallback;
	}

	private class TurnPageData
	{
		public GameObject flippingPage;

		public GameObject otherPage;

		public DelOnPageTurnComplete pageTurnCompleteCallback;

		public DelPositionPages positionPagesCallback;

		public object callbackData;
	}

	private readonly string FRONT_PAGE_NAME = "PageTurnFront";

	private readonly string BACK_PAGE_NAME = "PageTurnBack";

	private readonly string WAIT_THEN_COMPLETE_PAGE_TURN_RIGHT_COROUTINE = "WaitThenCompletePageTurnRight";

	private readonly string PAGE_TURN_LEFT_ANIM = "PageTurnLeft";

	private readonly string PAGE_TURN_RIGHT_ANIM = "PageTurnRight";

	private IGraphicsManager m_graphicsManager;

	public Shader m_MaskShader;

	public float m_TurnLeftSpeed = 1.65f;

	public float m_TurnRightSpeed = 1.65f;

	public float m_TurnLeftDelayBeforePositioningPages = 0.44f;

	private Bounds m_RenderBounds;

	private Camera m_OffscreenPageTurnCamera;

	private GameObject m_OffscreenPageTurnCameraGO;

	private RenderTexture m_TempRenderBuffer;

	private GameObject m_MeshGameObject;

	private GameObject m_FrontPageGameObject;

	private GameObject m_BackPageGameObject;

	private GameObject m_TheBoxOuterFrame;

	private float m_RenderOffset = 500f;

	private Vector3 m_initialPosition;

	private bool m_RenderRequested;

	private TurnPageData m_LeftTurnRenderData;

	private void RequestRightTurnRender()
	{
		m_RenderRequested = true;
	}

	private void RequestLeftTurnRender(TurnPageData data)
	{
		m_LeftTurnRenderData = data;
		m_RenderRequested = true;
	}

	private bool RenderingIsDone()
	{
		return !m_RenderRequested;
	}

	private void OnBeginFrameRendering(ScriptableRenderContext context, Camera[] cameras)
	{
		if (m_RenderRequested)
		{
			Vector3 pagePos = Vector3.zero;
			Vector3 otherPagePos = Vector3.zero;
			if (m_LeftTurnRenderData != null)
			{
				GameObject flippingPage = m_LeftTurnRenderData.flippingPage;
				GameObject otherPage = m_LeftTurnRenderData.otherPage;
				pagePos = flippingPage.transform.position;
				otherPagePos = otherPage.transform.position;
				flippingPage.transform.position = otherPagePos;
				otherPage.transform.position = pagePos;
			}
			Show(show: true);
			m_FrontPageGameObject.SetActive(value: true);
			m_BackPageGameObject.SetActive(value: true);
			SetCameraSize(m_OffscreenPageTurnCamera);
			m_OffscreenPageTurnCameraGO.transform.position = base.transform.position;
			Renderer frontPageRenderer = m_FrontPageGameObject.GetComponent<Renderer>();
			_ = frontPageRenderer.enabled;
			Renderer component = m_BackPageGameObject.GetComponent<Renderer>();
			_ = component.enabled;
			frontPageRenderer.enabled = false;
			component.enabled = false;
			bool outerBox = m_TheBoxOuterFrame.activeSelf;
			m_TheBoxOuterFrame.SetActive(value: false);
			UniversalRenderPipeline.RenderSingleCamera(context, m_OffscreenPageTurnCamera);
			m_TheBoxOuterFrame.SetActive(outerBox);
			if (m_LeftTurnRenderData != null)
			{
				m_LeftTurnRenderData.flippingPage.transform.position = pagePos;
				m_LeftTurnRenderData.otherPage.transform.position = otherPagePos;
			}
			m_RenderRequested = false;
			m_LeftTurnRenderData = null;
		}
	}

	private void Awake()
	{
		m_graphicsManager = ServiceManager.Get<IGraphicsManager>();
		m_initialPosition = base.transform.localPosition;
		Transform childTransform = base.transform.Find(FRONT_PAGE_NAME);
		if (childTransform != null)
		{
			m_FrontPageGameObject = childTransform.gameObject;
		}
		if (m_FrontPageGameObject == null)
		{
			Debug.LogError("Failed to find " + FRONT_PAGE_NAME + " Object.");
		}
		childTransform = base.transform.Find(BACK_PAGE_NAME);
		if (childTransform != null)
		{
			m_BackPageGameObject = childTransform.gameObject;
		}
		if (m_BackPageGameObject == null)
		{
			Debug.LogError("Failed to find " + BACK_PAGE_NAME + " Object.");
		}
		Show(show: false);
		m_TheBoxOuterFrame = Box.Get().m_OuterFrame;
		CreateCamera();
		CreateRenderTexture();
		SetupMaterial();
	}

	protected void OnEnable()
	{
		if (m_OffscreenPageTurnCameraGO != null)
		{
			CreateCamera();
		}
		if (m_TempRenderBuffer != null)
		{
			CreateRenderTexture();
			SetupMaterial();
		}
		RenderPipelineManager.beginFrameRendering += OnBeginFrameRendering;
	}

	protected void OnDisable()
	{
		if (m_TempRenderBuffer != null)
		{
			RenderTextureTracker.Get().DestroyRenderTexture(m_TempRenderBuffer);
		}
		if (m_OffscreenPageTurnCameraGO != null)
		{
			UnityEngine.Object.Destroy(m_OffscreenPageTurnCameraGO);
		}
		if (m_OffscreenPageTurnCamera != null)
		{
			UnityEngine.Object.Destroy(m_OffscreenPageTurnCamera);
		}
		RenderPipelineManager.beginFrameRendering -= OnBeginFrameRendering;
	}

	public void TurnRight(GameObject flippingPage, GameObject otherPage, DelOnPageTurnComplete pageTurnCompleteCallback, DelPositionPages positionPagesCallback, object callbackData)
	{
		RequestRightTurnRender();
		if (m_graphicsManager.RenderQualityLevel == GraphicsQuality.Low)
		{
			Time.captureFramerate = 18;
		}
		else if (m_graphicsManager.RenderQualityLevel == GraphicsQuality.Medium)
		{
			Time.captureFramerate = 24;
		}
		else
		{
			Time.captureFramerate = 30;
		}
		Animation component = GetComponent<Animation>();
		component.Stop(PAGE_TURN_RIGHT_ANIM);
		m_FrontPageGameObject.GetComponent<Renderer>().GetMaterial().SetFloat("_Alpha", 1f);
		m_BackPageGameObject.GetComponent<Renderer>().GetMaterial().SetFloat("_Alpha", 1f);
		float actualAnimLength = component[PAGE_TURN_RIGHT_ANIM].length / m_TurnRightSpeed;
		PageTurningData pageTurningData = new PageTurningData
		{
			m_secondsToWait = actualAnimLength,
			m_pageTurnCompleteCallback = pageTurnCompleteCallback,
			m_callbackData = callbackData,
			m_positionPagesCallback = positionPagesCallback
		};
		StopCoroutine(WAIT_THEN_COMPLETE_PAGE_TURN_RIGHT_COROUTINE);
		StartCoroutine(WAIT_THEN_COMPLETE_PAGE_TURN_RIGHT_COROUTINE, pageTurningData);
	}

	public void TurnLeft(GameObject flippingPage, GameObject otherPage, DelOnPageTurnComplete pageTurnCompleteCallback, DelPositionPages positionPagesCallback, object callbackData)
	{
		TurnPageData pageData = new TurnPageData();
		pageData.flippingPage = flippingPage;
		pageData.otherPage = otherPage;
		pageData.pageTurnCompleteCallback = pageTurnCompleteCallback;
		pageData.positionPagesCallback = positionPagesCallback;
		pageData.callbackData = callbackData;
		StopCoroutine("TurnLeftPage");
		StartCoroutine("TurnLeftPage", pageData);
	}

	private IEnumerator TurnLeftPage(TurnPageData pageData)
	{
		yield return null;
		yield return null;
		yield return null;
		_ = pageData.flippingPage;
		_ = pageData.otherPage;
		DelOnPageTurnComplete pageTurnCompleteCallback = pageData.pageTurnCompleteCallback;
		DelPositionPages positionPagesCallback = pageData.positionPagesCallback;
		object callbackData = pageData.callbackData;
		RequestLeftTurnRender(pageData);
		while (!RenderingIsDone())
		{
			yield return null;
		}
		if (m_graphicsManager.RenderQualityLevel == GraphicsQuality.Low)
		{
			Time.captureFramerate = 18;
		}
		else if (m_graphicsManager.RenderQualityLevel == GraphicsQuality.Medium)
		{
			Time.captureFramerate = 24;
		}
		else
		{
			Time.captureFramerate = 30;
		}
		Renderer component = m_FrontPageGameObject.GetComponent<Renderer>();
		Renderer backPageRenderer = m_BackPageGameObject.GetComponent<Renderer>();
		component.enabled = true;
		component.GetMaterial().SetFloat("_Alpha", 1f);
		backPageRenderer.enabled = true;
		backPageRenderer.GetMaterial().SetFloat("_Alpha", 1f);
		Animation pageAnimation = GetComponent<Animation>();
		pageAnimation.Stop(PAGE_TURN_LEFT_ANIM);
		pageAnimation[PAGE_TURN_LEFT_ANIM].time = 0.22f;
		pageAnimation[PAGE_TURN_LEFT_ANIM].speed = m_TurnLeftSpeed;
		pageAnimation.Play(PAGE_TURN_LEFT_ANIM);
		while (pageAnimation[PAGE_TURN_LEFT_ANIM].time < Math.Min(pageAnimation[PAGE_TURN_LEFT_ANIM].length, m_TurnLeftDelayBeforePositioningPages))
		{
			yield return null;
		}
		positionPagesCallback?.Invoke(callbackData);
		PageTurningData pageTurningData = new PageTurningData
		{
			m_secondsToWait = 0f,
			m_pageTurnCompleteCallback = pageTurnCompleteCallback,
			m_callbackData = callbackData,
			m_animation = pageAnimation[PAGE_TURN_LEFT_ANIM]
		};
		StartCoroutine(WaitThenCompletePageTurnLeft(pageTurningData));
	}

	private IEnumerator WaitThenCompletePageTurnLeft(PageTurningData pageTurningData)
	{
		while (GetComponent<Animation>().isPlaying)
		{
			yield return null;
		}
		Time.captureFramerate = 0;
		Show(show: false);
		if (pageTurningData.m_pageTurnCompleteCallback != null)
		{
			pageTurningData.m_pageTurnCompleteCallback(pageTurningData.m_callbackData);
		}
	}

	private void CreateCamera()
	{
		if (m_OffscreenPageTurnCameraGO == null)
		{
			if (m_OffscreenPageTurnCamera != null)
			{
				UnityEngine.Object.DestroyImmediate(m_OffscreenPageTurnCamera);
			}
			m_OffscreenPageTurnCameraGO = new GameObject();
			m_OffscreenPageTurnCamera = m_OffscreenPageTurnCameraGO.AddComponent<Camera>();
			m_OffscreenPageTurnCameraGO.name = base.name + "_OffScreenPageTurnCamera";
			SetupCamera(m_OffscreenPageTurnCamera);
		}
	}

	private void SetupCamera(Camera camera)
	{
		camera.orthographic = true;
		camera.transform.parent = base.transform;
		camera.nearClipPlane = -20f;
		camera.farClipPlane = 20f;
		camera.depth = ((Camera.main == null) ? 0f : (Camera.main.depth + 100f));
		camera.backgroundColor = Color.clear;
		camera.clearFlags = CameraClearFlags.Color;
		camera.cullingMask = GameLayer.Default.LayerBit() | GameLayer.CardRaycast.LayerBit();
		camera.enabled = false;
		camera.renderingPath = RenderingPath.Forward;
		camera.allowHDR = false;
		camera.TryGetComponent<UniversalAdditionalCameraData>(out var baseCameraAdditionalData);
		if (baseCameraAdditionalData == null)
		{
			baseCameraAdditionalData = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
		}
		baseCameraAdditionalData.SetRenderer(3);
		camera.transform.Rotate(90f, 0f, 0f);
		GameObjectUtils.SetHideFlags(camera, HideFlags.HideAndDontSave);
	}

	private void CreateRenderTexture()
	{
		int resolution = 512;
		GraphicsQuality quality = m_graphicsManager.RenderQualityLevel;
		switch (quality)
		{
		case GraphicsQuality.Medium:
			resolution = 1024;
			break;
		case GraphicsQuality.High:
		{
			int current = Math.Max(Screen.currentResolution.width, Screen.currentResolution.height);
			if (current >= 4096)
			{
				resolution = 4096;
			}
			else if (current >= 2048)
			{
				resolution = 2048;
			}
			else if (current >= 1024)
			{
				resolution = 1024;
			}
			break;
		}
		}
		if (m_TempRenderBuffer == null)
		{
			if (quality == GraphicsQuality.High)
			{
				m_TempRenderBuffer = RenderTextureTracker.Get().CreateNewTexture(resolution, resolution, 16, RenderTextureFormat.ARGB32);
			}
			else
			{
				bool num = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB1555) && PlatformSettings.RuntimeOS != OSCategory.Mac;
				bool supportsARGS4444 = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB4444) && PlatformSettings.RuntimeOS != OSCategory.PC;
				if (num)
				{
					m_TempRenderBuffer = RenderTextureTracker.Get().CreateNewTexture(resolution, resolution, 16, RenderTextureFormat.ARGB1555);
				}
				else if (quality == GraphicsQuality.Low && supportsARGS4444)
				{
					m_TempRenderBuffer = RenderTextureTracker.Get().CreateNewTexture(resolution, resolution, 16, RenderTextureFormat.ARGB4444);
				}
				else
				{
					m_TempRenderBuffer = RenderTextureTracker.Get().CreateNewTexture(resolution, resolution, 16);
				}
			}
			m_TempRenderBuffer.Create();
		}
		if (m_OffscreenPageTurnCamera != null)
		{
			m_OffscreenPageTurnCamera.targetTexture = m_TempRenderBuffer;
		}
	}

	private void SetCameraSize(Camera camera)
	{
		camera.orthographicSize = GetWorldScale(m_FrontPageGameObject.transform).x / 2f;
	}

	public void SetBackPageMaterial(Material material)
	{
		m_BackPageGameObject.GetComponent<Renderer>().SetMaterial(material);
	}

	private void SetupMaterial()
	{
		Material material = m_FrontPageGameObject.GetComponent<Renderer>().GetMaterial();
		material.mainTexture = m_TempRenderBuffer;
		material.renderQueue = 3001;
		m_BackPageGameObject.GetComponent<Renderer>().GetMaterial().renderQueue = 3002;
	}

	private void Show(bool show)
	{
		base.transform.localPosition = (show ? m_initialPosition : (Vector3.right * m_RenderOffset));
	}

	private IEnumerator WaitThenCompletePageTurnRight(PageTurningData pageTurningData)
	{
		while (!RenderingIsDone())
		{
			yield return null;
		}
		m_FrontPageGameObject.GetComponent<Renderer>().enabled = true;
		m_BackPageGameObject.GetComponent<Renderer>().enabled = true;
		Animation component = GetComponent<Animation>();
		component[PAGE_TURN_RIGHT_ANIM].time = 0f;
		component[PAGE_TURN_RIGHT_ANIM].speed = m_TurnRightSpeed;
		component.Play(PAGE_TURN_RIGHT_ANIM);
		if (pageTurningData.m_positionPagesCallback != null)
		{
			pageTurningData.m_positionPagesCallback(pageTurningData.m_callbackData);
		}
		yield return new WaitForSeconds(pageTurningData.m_secondsToWait);
		Time.captureFramerate = 0;
		Show(show: false);
		if (pageTurningData.m_pageTurnCompleteCallback != null)
		{
			pageTurningData.m_pageTurnCompleteCallback(pageTurningData.m_callbackData);
		}
	}

	public static Vector3 GetWorldScale(Transform transform)
	{
		Vector3 worldScale = transform.localScale;
		Transform parent = transform.parent;
		while (parent != null)
		{
			worldScale = Vector3.Scale(worldScale, parent.localScale);
			parent = parent.parent;
		}
		return worldScale;
	}
}
