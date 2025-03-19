using System;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using UnityEngine;

public class UniversalInputManager : IHasUpdate, IService
{
	public delegate void MouseOnOrOffScreenCallback(bool onScreen);

	public delegate void TextInputUpdatedCallback(string input);

	public delegate bool TextInputPreprocessCallback();

	public delegate void TextInputCompletedCallback(string input);

	public delegate void TextInputCanceledCallback(bool userRequested, GameObject requester);

	public delegate void TextInputUnfocusedCallback();

	public class TextInputParams
	{
		public GameObject m_owner;

		public bool m_password;

		public bool m_number;

		public bool m_multiLine;

		public Rect m_rect;

		public GameLayer? m_gameLayer;

		public int? m_orderInLayer;

		public TextInputUpdatedCallback m_updatedCallback;

		public TextInputPreprocessCallback m_preprocessCallback;

		public TextInputCompletedCallback m_completedCallback;

		public TextInputCanceledCallback m_canceledCallback;

		public TextInputUnfocusedCallback m_unfocusedCallback;

		public int m_maxCharacters;

		public Font m_font;

		public TextAnchor? m_alignment;

		public string m_text;

		public bool m_touchScreenKeyboardHideInput;

		public int m_touchScreenKeyboardType;

		public bool m_inputKeepFocusOnComplete;

		public Color? m_color;

		public bool m_showVirtualKeyboard = true;

		public bool m_hideVirtualKeyboardOnComplete = true;

		public bool m_useNativeKeyboard;

		public bool m_showBackground;
	}

	private enum TextInputIgnoreState
	{
		INVALID,
		COMPLETE_KEY_UP,
		CANCEL_KEY_UP,
		NEXT_CALL
	}

	private static UniversalInputManager s_instance;

	private const int RAYCAST_MAXHITNUMBER = 20;

	private RaycastHit[] m_cachedRaycastHits = new RaycastHit[20];

	private static readonly PlatformDependentValue<bool> IsTouchDevice = new PlatformDependentValue<bool>(PlatformCategory.Input)
	{
		Mouse = false,
		Touch = true
	};

	private const int MAX_CAMERAS = 20;

	private const float TEXT_INPUT_RECT_HEIGHT_OFFSET = 3f;

	private const int TEXT_INPUT_MAX_FONT_SIZE = 96;

	private const int TEXT_INPUT_MIN_FONT_SIZE = 4;

	private const int TEXT_INPUT_FONT_SIZE_INSET = 4;

	private const int TEXT_INPUT_IME_FONT_SIZE_INSET = 4;

	private const string TEXT_INPUT_NAME = "UniversalInputManagerTextInput";

	private static readonly GameLayer[] s_hitTestPriorityOrder = new GameLayer[11]
	{
		GameLayer.IgnoreFullScreenEffects,
		GameLayer.Reserved29,
		GameLayer.BackgroundUI,
		GameLayer.PerspectiveUI,
		GameLayer.CameraMask,
		GameLayer.UI,
		GameLayer.BattleNet,
		GameLayer.BattleNetFriendList,
		GameLayer.BattleNetDialog,
		GameLayer.BattleNetChat,
		GameLayer.HighPriorityUI
	};

	private static readonly GameLayer[] s_ignoreHitTestLayers = new GameLayer[10]
	{
		GameLayer.TransparentFX,
		GameLayer.IgnoreRaycast,
		GameLayer.Water,
		GameLayer.Tooltip,
		GameLayer.NoLight,
		GameLayer.Effects,
		GameLayer.FXCollide,
		GameLayer.ScreenEffects,
		GameLayer.InvisibleRender,
		GameLayer.CameraFade
	};

	private static readonly LayerMask s_cameraMaskLayer = GameLayer.CameraMask.LayerBit();

	private static readonly LayerMask s_friendsListMaskLayer = GameLayer.BattleNetFriendList.LayerBit();

	private static readonly LayerMask s_bnetChatMaskLayer = GameLayer.BattleNetChat.LayerBit();

	private static readonly LayerMask s_ignoreFullScreenEffectsLayer = GameLayer.IgnoreFullScreenEffects.LayerBit();

	private static Map<int, int> s_hitTestPriorityMap;

	private static int s_hitTestLayerBits = 0;

	private static bool IsIMEEverUsed = false;

	private bool m_mouseOnScreen;

	private List<MouseOnOrOffScreenCallback> m_mouseOnOrOffScreenListeners = new List<MouseOnOrOffScreenCallback>();

	private bool m_gameDialogActive;

	private bool m_systemDialogActive;

	private Camera m_mainEffectsCamera;

	private FullScreenEffects m_currentFullScreenEffect;

	private List<Camera> m_cameraMaskCameras = new List<Camera>();

	private Camera[] m_allCameras = new Camera[20];

	private int m_numCameras = 20;

	private Vector3 m_mousePosition;

	private List<Camera> m_ignoredCameras = new List<Camera>();

	private GameObject m_inputOwner;

	private TextInputUpdatedCallback m_inputUpdatedCallback;

	private TextInputPreprocessCallback m_inputPreprocessCallback;

	private TextInputCompletedCallback m_inputCompletedCallback;

	private TextInputCanceledCallback m_inputCanceledCallback;

	private TextInputUnfocusedCallback m_inputUnfocusedCallback;

	private bool m_inputPassword;

	private bool m_inputNumber;

	private bool m_inputMultiLine;

	private bool m_inputActive;

	private bool m_inputFocused;

	private bool m_inputKeepFocusOnComplete;

	private string m_inputText;

	private Rect m_inputNormalizedRect;

	private Vector2 m_inputInitialScreenSize;

	private int m_inputMaxCharacters;

	private TextAnchor? m_inputAlignment;

	private Color? m_inputColor;

	private bool m_inputNeedsFocus;

	private bool m_tabKeyDown;

	private bool m_inputNeedsFocusFromTabKeyDown;

	private TextInputIgnoreState m_inputIgnoreState;

	private GameObject m_sceneObject;

	private bool m_hideVirtualKeyboardOnComplete = true;

	private InputFieldUI m_inputFieldUI;

	private HearthstoneCheckout m_commerce;

	private bool m_shouldHandleCheats;

	private SceneMgr m_sceneMgr;

	private bool m_isTouchMode;

	private bool m_inputFieldReady;

	public static readonly PlatformDependentValue<bool> UsePhoneUI = new PlatformDependentValue<bool>(PlatformCategory.Screen)
	{
		Phone = true,
		Tablet = false,
		PC = false
	};

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		CreateHitTestPriorityMap();
		m_mouseOnScreen = InputUtil.IsMouseOnScreen();
		m_shouldHandleCheats = !HearthstoneApplication.IsPublic();
		UpdateIsTouchMode();
		Options.Get().RegisterChangedListener(Option.TOUCH_MODE, OnTouchModeChangedCallback);
		new GameObject("RaycastCache", typeof(RaycastCache), typeof(HSDontDestroyOnLoad));
		yield break;
	}

	public Type[] GetDependencies()
	{
		return null;
	}

	public void Shutdown()
	{
		s_instance = null;
		m_cachedRaycastHits = null;
		Options.Get().UnregisterChangedListener(Option.TOUCH_MODE, OnTouchModeChangedCallback);
	}

	public void Update()
	{
		UpdateAllCamerasArray();
		UpdateMouseOnOrOffScreen();
		UpdateInput();
		CleanDeadCameras();
		if (m_inputFieldReady)
		{
			IgnoreGUIInput();
			HandleGUIInputInactive();
			HandleGUIInputActive();
		}
	}

	public static UniversalInputManager Get()
	{
		if (s_instance == null)
		{
			s_instance = ServiceManager.Get<UniversalInputManager>();
		}
		return s_instance;
	}

	public bool IsTouchMode()
	{
		return m_isTouchMode;
	}

	private void UpdateIsTouchMode()
	{
		m_isTouchMode = (bool)IsTouchDevice || Options.Get().GetBool(Option.TOUCH_MODE);
	}

	public bool UseWindowsTouch()
	{
		if (IsTouchMode())
		{
			return !PlatformSettings.IsEmulating;
		}
		return false;
	}

	public bool WasTouchCanceled()
	{
		if (!IsTouchDevice)
		{
			return false;
		}
		int i = 0;
		for (int iMax = Input.touchCount; i < iMax; i++)
		{
			if (Input.GetTouch(i).phase == TouchPhase.Canceled)
			{
				return true;
			}
		}
		return false;
	}

	public bool RegisterMouseOnOrOffScreenListener(MouseOnOrOffScreenCallback listener)
	{
		if (m_mouseOnOrOffScreenListeners.Contains(listener))
		{
			return false;
		}
		m_mouseOnOrOffScreenListeners.Add(listener);
		return true;
	}

	public bool UnregisterMouseOnOrOffScreenListener(MouseOnOrOffScreenCallback listener)
	{
		return m_mouseOnOrOffScreenListeners.Remove(listener);
	}

	public void SetGameDialogActive(bool active)
	{
		m_gameDialogActive = active;
	}

	public void SetSystemDialogActive(bool active)
	{
		m_systemDialogActive = active;
	}

	public bool IsDialogActive()
	{
		if (!m_gameDialogActive)
		{
			return m_systemDialogActive;
		}
		return true;
	}

	public void UseTextInput(TextInputParams parms, bool force = false)
	{
		if (m_inputFieldReady && (force || !(parms.m_owner == m_inputOwner)))
		{
			if (m_inputOwner != null && m_inputOwner != parms.m_owner)
			{
				ObjectCancelTextInput(parms.m_owner);
			}
			m_inputOwner = parms.m_owner;
			m_inputUpdatedCallback = parms.m_updatedCallback;
			m_inputPreprocessCallback = parms.m_preprocessCallback;
			m_inputCompletedCallback = parms.m_completedCallback;
			m_inputCanceledCallback = parms.m_canceledCallback;
			m_inputUnfocusedCallback = parms.m_unfocusedCallback;
			m_inputFieldUI.SetTextInputParams(parms);
			m_inputPassword = parms.m_password;
			m_inputNumber = parms.m_number;
			m_inputMultiLine = parms.m_multiLine;
			m_inputActive = true;
			m_inputFocused = false;
			m_inputText = parms.m_text ?? string.Empty;
			m_inputNormalizedRect = parms.m_rect;
			m_inputInitialScreenSize.x = Screen.width;
			m_inputInitialScreenSize.y = Screen.height;
			m_inputMaxCharacters = parms.m_maxCharacters;
			m_inputColor = parms.m_color;
			m_inputAlignment = parms.m_alignment;
			m_inputNeedsFocus = true;
			m_inputIgnoreState = TextInputIgnoreState.INVALID;
			m_inputKeepFocusOnComplete = parms.m_inputKeepFocusOnComplete;
			if (IsTextInputPassword())
			{
				Input.imeCompositionMode = IMECompositionMode.Off;
			}
			else
			{
				Input.imeCompositionMode = IMECompositionMode.On;
			}
			m_hideVirtualKeyboardOnComplete = parms.m_hideVirtualKeyboardOnComplete;
			if (UseWindowsTouch() && parms.m_showVirtualKeyboard)
			{
				ServiceManager.Get<ITouchScreenService>().ShowKeyboard();
			}
		}
	}

	public void CancelTextInput(GameObject requester, bool force = false)
	{
		if (IsTextInputActive() && (force || !(requester != m_inputOwner)))
		{
			ObjectCancelTextInput(requester);
		}
	}

	public void FocusTextInput(GameObject owner)
	{
		if (!(owner != m_inputOwner))
		{
			if (!m_tabKeyDown)
			{
				m_inputNeedsFocus = true;
			}
			else
			{
				m_inputNeedsFocusFromTabKeyDown = true;
			}
		}
	}

	public bool IsTextInputPassword()
	{
		return m_inputPassword;
	}

	public bool IsTextInputActive()
	{
		return m_inputActive;
	}

	public string GetInputText()
	{
		return m_inputText;
	}

	public void SetInputText(string text, bool moveCursorToEnd = false)
	{
		m_inputText = text ?? string.Empty;
		m_inputFieldUI.Text = m_inputText;
		if (moveCursorToEnd)
		{
			m_inputFieldUI.MoveCursorToEnd();
		}
	}

	public bool InputIsOver(GameObject gameObj)
	{
		RaycastHit hitInfo;
		return InputIsOver(gameObj, out hitInfo);
	}

	public bool InputIsOver(GameObject gameObj, out RaycastHit hitInfo)
	{
		LayerMask layerMask = ((GameLayer)gameObj.layer).LayerBit();
		if (!Raycast(null, layerMask, out var _, out hitInfo))
		{
			return false;
		}
		return hitInfo.collider.gameObject == gameObj;
	}

	public bool InputIsOver(Camera camera, GameObject gameObj)
	{
		RaycastHit hitInfo;
		return InputIsOver(camera, gameObj, out hitInfo);
	}

	public bool InputIsOver(Camera camera, GameObject gameObj, out RaycastHit hitInfo)
	{
		LayerMask layerMask = ((GameLayer)gameObj.layer).LayerBit();
		if (!Raycast(camera, layerMask, out var _, out hitInfo))
		{
			return false;
		}
		return hitInfo.collider.gameObject == gameObj;
	}

	public bool InputIsOverByRenderPass(GameObject gameObj, out RaycastHit hitInfo)
	{
		if (!GetInputHitInfoByRenderPass(out hitInfo, out var _))
		{
			return false;
		}
		return hitInfo.collider.gameObject == gameObj;
	}

	public bool ForcedInputIsOver(Camera camera, GameObject gameObj)
	{
		RaycastHit hitInfo;
		return ForcedInputIsOver(camera, gameObj, out hitInfo);
	}

	public bool ForcedInputIsOver(Camera camera, GameObject gameObj, out RaycastHit hitInfo, CameraOverridePass cameraOverride = null)
	{
		LayerMask layerMask = ((GameLayer)gameObj.layer).LayerBit();
		if (!CameraUtils.Raycast(camera, m_mousePosition, layerMask, out hitInfo, cameraOverride))
		{
			return false;
		}
		return hitInfo.collider.gameObject == gameObj;
	}

	public bool ForcedUnblockableInputIsOver(Camera camera, GameObject gameObj, out RaycastHit hitInfo)
	{
		LayerMask layerMask = ((GameLayer)gameObj.layer).LayerBit();
		hitInfo = default(RaycastHit);
		int hitNumber = CameraUtils.RaycastAll(camera, m_mousePosition, layerMask, ref m_cachedRaycastHits);
		if (hitNumber == 0)
		{
			return false;
		}
		for (int i = 0; i < hitNumber; i++)
		{
			if (m_cachedRaycastHits[i].collider.gameObject == gameObj)
			{
				hitInfo = m_cachedRaycastHits[i];
				return true;
			}
		}
		return false;
	}

	public bool InputHitAnyObject(GameLayer layer)
	{
		RaycastHit hitInfo;
		return GetInputHitInfo(layer, out hitInfo);
	}

	public bool InputHitAnyObject(Camera requestedCamera, GameLayer layer)
	{
		RaycastHit hitInfo;
		return GetInputHitInfo(requestedCamera, layer, out hitInfo);
	}

	public bool GetInputHitInfo(GameLayer[] gameLayers, out RaycastHit hitInfo)
	{
		bool ignorePriority = false;
		int i = 0;
		for (int iMax = gameLayers.Length; i < iMax; i++)
		{
			int mask = gameLayers[i].LayerBit();
			Camera requestedCamera = GuessBestHitTestCamera(mask);
			if (Raycast(requestedCamera, mask, out var _, out hitInfo, ignorePriority))
			{
				return true;
			}
			ignorePriority = true;
		}
		hitInfo = default(RaycastHit);
		return false;
	}

	public bool GetInputHitInfo(out RaycastHit hitInfo)
	{
		return GetInputHitInfo(GameLayer.Default, out hitInfo);
	}

	public bool GetInputHitInfo(GameLayer layer, out RaycastHit hitInfo)
	{
		return GetInputHitInfo(layer.LayerBit(), out hitInfo);
	}

	public bool GetInputHitInfo(LayerMask mask, out RaycastHit hitInfo)
	{
		Camera requestedCamera = GuessBestHitTestCamera(mask);
		return GetInputHitInfo(requestedCamera, mask, out hitInfo);
	}

	public bool GetInputHitInfo(Camera requestedCamera, out RaycastHit hitInfo)
	{
		if (requestedCamera == null)
		{
			return GetInputHitInfo(out hitInfo);
		}
		return GetInputHitInfo(requestedCamera, requestedCamera.cullingMask, out hitInfo);
	}

	public bool GetInputHitInfo(Camera requestedCamera, GameLayer layer, out RaycastHit hitInfo)
	{
		Camera camera;
		return Raycast(requestedCamera, layer.LayerBit(), out camera, out hitInfo);
	}

	public bool GetInputHitInfo(Camera requestedCamera, LayerMask mask, out RaycastHit hitInfo)
	{
		Camera camera;
		return Raycast(requestedCamera, mask, out camera, out hitInfo);
	}

	public int GetAllInputHitInfo(LayerMask mask, ref RaycastHit[] hitInfo)
	{
		Camera requestedCamera = GuessBestHitTestCamera(mask);
		return GetAllInputHitInfo(requestedCamera, mask, ref hitInfo);
	}

	public int GetAllInputHitInfo(Camera requestedCamera, LayerMask mask, ref RaycastHit[] hitInfo)
	{
		return CameraUtils.RaycastAll(requestedCamera, m_mousePosition, mask, ref hitInfo);
	}

	public bool GetInputHitInfoByRenderPass(out RaycastHit hitInfo, out Camera hitCamera)
	{
		Camera orthoCamera = GuessBestHitTestCamera(GameLayer.UI.LayerBit());
		if (orthoCamera != null)
		{
			for (int i = s_hitTestPriorityOrder.Length - 1; i >= 0; i--)
			{
				GameLayer layer = s_hitTestPriorityOrder[i];
				int layerBit = layer.LayerBit();
				List<CustomViewPass> customPasses = null;
				if ((layerBit & (int)s_friendsListMaskLayer) != 0)
				{
					customPasses = CustomViewPass.GetQueue(CustomViewEntryPoint.BattleNetFriendList);
				}
				else if ((layerBit & (int)s_bnetChatMaskLayer) != 0)
				{
					customPasses = CustomViewPass.GetQueue(CustomViewEntryPoint.BattleNetChat);
				}
				CameraOverridePass cameraOverride = null;
				if (customPasses != null && customPasses.Count > 0)
				{
					cameraOverride = customPasses[0] as CameraOverridePass;
				}
				if (RaycastAgainstBlockingLayers(orthoCamera, layerBit, out hitInfo, cameraOverride))
				{
					hitCamera = orthoCamera;
					return true;
				}
				if (layer == GameLayer.UI)
				{
					break;
				}
			}
			LayerMask primaryLayerMask = orthoCamera.cullingMask & s_hitTestLayerBits & ~(int)s_cameraMaskLayer;
			for (int i2 = 3; i2 >= 2; i2--)
			{
				List<CustomViewPass> renderPasses = CustomViewPass.GetQueue((CustomViewEntryPoint)i2);
				if (renderPasses != null && RaycastAgainstMaskableLayers(orthoCamera, primaryLayerMask, renderPasses, out hitInfo))
				{
					hitCamera = orthoCamera;
					return true;
				}
			}
			if (RaycastAgainstBlockingLayers(orthoCamera, primaryLayerMask, out hitInfo))
			{
				hitCamera = orthoCamera;
				return true;
			}
		}
		Camera perspectiveCamera = GuessBestHitTestCamera(GameLayer.Default.LayerBit());
		if (perspectiveCamera != null)
		{
			LayerMask primaryLayerMask2 = perspectiveCamera.cullingMask & s_hitTestLayerBits & ~(int)s_cameraMaskLayer;
			for (int i3 = 1; i3 >= 0; i3--)
			{
				List<CustomViewPass> renderPasses2 = CustomViewPass.GetQueue((CustomViewEntryPoint)i3);
				if (renderPasses2 != null && RaycastAgainstMaskableLayers(perspectiveCamera, primaryLayerMask2, renderPasses2, out hitInfo))
				{
					hitCamera = perspectiveCamera;
					return true;
				}
			}
			if (RaycastAgainstBlockingLayers(perspectiveCamera, primaryLayerMask2, out hitInfo))
			{
				hitCamera = perspectiveCamera;
				return true;
			}
		}
		hitCamera = null;
		hitInfo = default(RaycastHit);
		return false;
	}

	public bool GetInputPointOnPlane(Vector3 origin, out Vector3 point)
	{
		return GetInputPointOnPlane(GameLayer.Default, origin, out point);
	}

	public bool GetInputPointOnPlane(GameLayer layer, Vector3 origin, out Vector3 point)
	{
		point = Vector3.zero;
		LayerMask layerMask = layer.LayerBit();
		if (!Raycast(null, layerMask, out var camera, out var _))
		{
			return false;
		}
		Ray ray = camera.ScreenPointToRay(m_mousePosition);
		Vector3 normal = -camera.transform.forward;
		if (!new Plane(normal, origin).Raycast(ray, out var distance))
		{
			return false;
		}
		point = ray.GetPoint(distance);
		return true;
	}

	public Ray MousePositionToRay(Camera camera)
	{
		return camera.ScreenPointToRay(m_mousePosition);
	}

	public void SetCurrentFullScreenEffect(FullScreenEffects effect)
	{
		m_currentFullScreenEffect = effect;
	}

	public bool AddIgnoredCamera(Camera camera)
	{
		if (m_ignoredCameras.Contains(camera))
		{
			return false;
		}
		m_ignoredCameras.Add(camera);
		return true;
	}

	private static void CreateHitTestPriorityMap()
	{
		s_hitTestPriorityMap = new Map<int, int>();
		int priority = 1;
		for (int i = 0; i < s_hitTestPriorityOrder.Length; i++)
		{
			GameLayer layer = s_hitTestPriorityOrder[i];
			s_hitTestPriorityMap.Add(layer.LayerBit(), priority++);
		}
		foreach (GameLayer value in Enum.GetValues(typeof(GameLayer)))
		{
			int layerBit = value.LayerBit();
			s_hitTestLayerBits |= layerBit;
			if (!s_hitTestPriorityMap.ContainsKey(layerBit))
			{
				s_hitTestPriorityMap.Add(layerBit, 0);
			}
		}
		int ignoreBits = 0;
		GameLayer[] array = s_ignoreHitTestLayers;
		foreach (GameLayer ignoreLayer in array)
		{
			ignoreBits |= ignoreLayer.LayerBit();
		}
		s_hitTestLayerBits &= ~ignoreBits;
	}

	private void UpdateAllCamerasArray()
	{
		int num = m_allCameras.Length;
		int allCameras = Camera.allCamerasCount;
		if (num < allCameras)
		{
			m_allCameras = new Camera[allCameras];
		}
		m_numCameras = Camera.GetAllCameras(m_allCameras);
	}

	private void CleanDeadCameras()
	{
		GeneralUtils.CleanDeadObjectsFromList(m_cameraMaskCameras);
		GeneralUtils.CleanDeadObjectsFromList(m_ignoredCameras);
	}

	public void SetTextInputField(InputFieldUI inputFieldObject)
	{
		m_inputFieldUI = inputFieldObject;
		m_inputFieldUI.SetCanvasActive(active: false);
		m_inputFieldUI.SetEndEditFunction(EndEdit);
		m_inputFieldReady = true;
	}

	private void EndEdit(string text)
	{
		m_inputText = text;
	}

	private Camera GuessBestHitTestCamera(LayerMask mask)
	{
		Camera orthoCam = null;
		BaseUI baseUI = BaseUI.Get();
		if (baseUI != null)
		{
			orthoCam = baseUI.GetBnetCamera();
		}
		if (orthoCam != null && (orthoCam.cullingMask & (int)mask) != 0)
		{
			return orthoCam;
		}
		Camera perspectiveCam = CameraUtils.GetMainCamera();
		if (perspectiveCam != null && (perspectiveCam.cullingMask & (int)mask) != 0)
		{
			return perspectiveCam;
		}
		for (int i = 0; i < m_numCameras; i++)
		{
			Camera camera = m_allCameras[i];
			if (!(camera == null) && (camera.cullingMask & (int)mask) != 0 && !m_ignoredCameras.Contains(camera))
			{
				return camera;
			}
		}
		return null;
	}

	private bool Raycast(Camera requestedCamera, LayerMask mask, out Camera camera, out RaycastHit hitInfo, bool ignorePriority = false)
	{
		hitInfo = default(RaycastHit);
		if (!ignorePriority)
		{
			foreach (Camera maskCamera in m_cameraMaskCameras)
			{
				if (RaycastWithPriority(maskCamera, s_cameraMaskLayer, out hitInfo))
				{
					camera = maskCamera;
					return true;
				}
			}
			if (m_mainEffectsCamera == null)
			{
				m_mainEffectsCamera = CameraUtils.FindFullScreenEffectsCamera(activeOnly: false);
			}
			if (m_mainEffectsCamera != null && RaycastWithPriority(m_mainEffectsCamera, s_ignoreFullScreenEffectsLayer, out hitInfo))
			{
				camera = m_mainEffectsCamera;
				return true;
			}
		}
		camera = requestedCamera;
		if (camera != null)
		{
			return RaycastWithPriority(camera, mask, out hitInfo);
		}
		camera = Camera.main;
		return RaycastWithPriority(camera, mask, out hitInfo);
	}

	private bool RaycastWithPriority(Camera camera, LayerMask mask, out RaycastHit hitInfo, CameraOverridePass cameraOverride = null)
	{
		hitInfo = default(RaycastHit);
		if (camera == null)
		{
			return false;
		}
		if (!FilteredRaycast(camera, m_mousePosition, mask, out hitInfo, cameraOverride))
		{
			return false;
		}
		GameLayer hitLayer = (GameLayer)hitInfo.collider.gameObject.layer;
		if (HigherPriorityCollisionExists(hitLayer.LayerBit()))
		{
			return false;
		}
		return true;
	}

	private bool RaycastAgainstMaskableLayers(Camera camera, LayerMask primaryLayerMask, List<CustomViewPass> renderPasses, out RaycastHit hitInfo)
	{
		if (renderPasses == null)
		{
			hitInfo = default(RaycastHit);
			return false;
		}
		for (int j = renderPasses.Count - 1; j >= 0; j--)
		{
			if (renderPasses[j] is CameraOverridePass cameraOverride)
			{
				LayerMask targetLayerMask = ((((int)cameraOverride.layerMask & (int)s_cameraMaskLayer) != 0) ? s_cameraMaskLayer : primaryLayerMask);
				if (RaycastAgainstBlockingLayers(camera, targetLayerMask, out hitInfo, cameraOverride))
				{
					return true;
				}
			}
		}
		hitInfo = default(RaycastHit);
		return false;
	}

	private bool RaycastAgainstBlockingLayers(Camera camera, LayerMask mask, out RaycastHit hitInfo, CameraOverridePass cameraOverride = null)
	{
		hitInfo = default(RaycastHit);
		if (camera == null)
		{
			return false;
		}
		if (!FilteredRaycast(camera, m_mousePosition, mask, out hitInfo, cameraOverride))
		{
			return false;
		}
		int layerBit = ((GameLayer)hitInfo.collider.gameObject.layer).LayerBit();
		if (m_systemDialogActive && s_hitTestPriorityMap[layerBit] < s_hitTestPriorityMap[GameLayer.UI.LayerBit()])
		{
			return false;
		}
		if (m_gameDialogActive && s_hitTestPriorityMap[layerBit] < s_hitTestPriorityMap[GameLayer.IgnoreFullScreenEffects.LayerBit()])
		{
			return false;
		}
		if (m_currentFullScreenEffect != null && m_currentFullScreenEffect.HasActiveEffects && camera.depth < m_currentFullScreenEffect.Camera.depth)
		{
			return false;
		}
		return true;
	}

	public void UpdateCachedValues()
	{
		m_mousePosition = InputCollection.GetMousePosition();
	}

	private bool FilteredRaycast(Camera camera, Vector3 screenPoint, LayerMask mask, out RaycastHit hitInfo, CameraOverridePass cameraOverride = null)
	{
		if (!CameraUtils.Raycast(camera, screenPoint, mask, out hitInfo, cameraOverride))
		{
			return false;
		}
		return true;
	}

	private bool HigherPriorityCollisionExists(int layerBit)
	{
		if (m_systemDialogActive && s_hitTestPriorityMap[layerBit] < s_hitTestPriorityMap[GameLayer.UI.LayerBit()])
		{
			return true;
		}
		if (m_gameDialogActive && s_hitTestPriorityMap[layerBit] < s_hitTestPriorityMap[GameLayer.IgnoreFullScreenEffects.LayerBit()])
		{
			return true;
		}
		LayerMask higherPriorityLayerMask = GetHigherPriorityLayerMask(layerBit);
		for (int i = 0; i < m_numCameras; i++)
		{
			Camera camera = m_allCameras[i];
			if (!(camera == null) && (camera.cullingMask & (int)higherPriorityLayerMask) != 0 && !m_ignoredCameras.Contains(camera) && FilteredRaycast(camera, m_mousePosition, higherPriorityLayerMask, out var hitInfo))
			{
				GameLayer hitLayer = (GameLayer)hitInfo.collider.gameObject.layer;
				if ((camera.cullingMask & hitLayer.LayerBit()) != 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	private LayerMask GetHigherPriorityLayerMask(int layerBit)
	{
		int priority = s_hitTestPriorityMap[layerBit];
		LayerMask mask = 0;
		foreach (KeyValuePair<int, int> pair in s_hitTestPriorityMap)
		{
			if (pair.Value > priority)
			{
				mask = (int)mask | pair.Key;
			}
		}
		return mask;
	}

	private void UpdateMouseOnOrOffScreen()
	{
		bool onScreen = InputUtil.IsMouseOnScreen();
		if (onScreen != m_mouseOnScreen)
		{
			m_mouseOnScreen = onScreen;
			MouseOnOrOffScreenCallback[] array = m_mouseOnOrOffScreenListeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i](onScreen);
			}
		}
	}

	private void UpdateInput()
	{
		if (UpdateTextInput())
		{
			return;
		}
		InputManager gameInputManager = InputManager.Get();
		if ((gameInputManager != null && gameInputManager.HandleKeyboardInput()) || HearthstoneCheckoutBlocksInput())
		{
			return;
		}
		if (m_shouldHandleCheats)
		{
			CheatMgr cheatMgr = CheatMgr.Get();
			if (cheatMgr != null && cheatMgr.HandleKeyboardInput())
			{
				return;
			}
			Cheats cheats = Cheats.Get();
			if (cheats != null && cheats.HandleKeyboardInput())
			{
				return;
			}
		}
		DialogManager dialogs = DialogManager.Get();
		if (dialogs != null && dialogs.HandleKeyboardInput())
		{
			return;
		}
		InputMgr inputMgr = InputMgr.Get();
		if (inputMgr != null && inputMgr.HandleKeyboardInput())
		{
			return;
		}
		DraftInputManager draftInputMgr = DraftInputManager.Get();
		if (draftInputMgr != null && draftInputMgr.HandleKeyboardInput())
		{
			return;
		}
		PackOpening packOpening = PackOpening.Get();
		if (packOpening != null && packOpening.HandleKeyboardInput())
		{
			return;
		}
		if (m_sceneMgr != null || ServiceManager.TryGet<SceneMgr>(out m_sceneMgr))
		{
			PegasusScene scene = m_sceneMgr.GetScene();
			if (scene != null && scene.HandleKeyboardInput())
			{
				return;
			}
		}
		BaseUI ui = BaseUI.Get();
		if (ui != null)
		{
			ui.HandleKeyboardInput();
		}
	}

	private bool UpdateTextInput()
	{
		if (Input.imeIsSelected || !string.IsNullOrEmpty(Input.compositionString))
		{
			IsIMEEverUsed = true;
		}
		if (m_inputNeedsFocusFromTabKeyDown)
		{
			m_inputNeedsFocusFromTabKeyDown = false;
			m_inputNeedsFocus = true;
		}
		if (!m_inputActive)
		{
			return false;
		}
		return m_inputFocused;
	}

	private void UserCancelTextInput()
	{
		CancelTextInput(userRequested: true, null);
	}

	private void ObjectCancelTextInput(GameObject requester)
	{
		CancelTextInput(userRequested: false, requester);
	}

	private void CancelTextInput(bool userRequested, GameObject requester)
	{
		if (IsTextInputPassword())
		{
			Input.imeCompositionMode = IMECompositionMode.Auto;
		}
		if (requester != null && requester == m_inputOwner)
		{
			ClearTextInputVars();
		}
		else
		{
			TextInputCanceledCallback callback = m_inputCanceledCallback;
			ClearTextInputVars();
			callback?.Invoke(userRequested, requester);
		}
		if (UseWindowsTouch())
		{
			ServiceManager.Get<ITouchScreenService>().HideKeyboard();
		}
	}

	private void ResetKeyboard()
	{
		if (UseWindowsTouch() && m_hideVirtualKeyboardOnComplete)
		{
			ServiceManager.Get<ITouchScreenService>().HideKeyboard();
		}
	}

	private void CompleteTextInput()
	{
		if (IsTextInputPassword())
		{
			Input.imeCompositionMode = IMECompositionMode.Auto;
		}
		TextInputCompletedCallback callback = m_inputCompletedCallback;
		if (!m_inputKeepFocusOnComplete)
		{
			ClearTextInputVars();
		}
		try
		{
			callback?.Invoke(m_inputText);
			m_inputText = string.Empty;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex);
			ResetKeyboard();
			throw new Exception("Error completing text input", ex);
		}
		ResetKeyboard();
	}

	private void ClearTextInputVars()
	{
		m_inputActive = false;
		m_inputFocused = false;
		m_inputOwner = null;
		m_inputMaxCharacters = 0;
		m_inputUpdatedCallback = null;
		m_inputCompletedCallback = null;
		m_inputCanceledCallback = null;
		m_inputUnfocusedCallback = null;
		_ = Application.isEditor;
	}

	private bool IgnoreGUIInput()
	{
		if (m_inputIgnoreState == TextInputIgnoreState.INVALID)
		{
			return false;
		}
		if (Input.GetKeyDown(KeyCode.Return))
		{
			if (m_inputIgnoreState == TextInputIgnoreState.COMPLETE_KEY_UP)
			{
				m_inputIgnoreState = TextInputIgnoreState.NEXT_CALL;
			}
			return true;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (m_inputIgnoreState == TextInputIgnoreState.CANCEL_KEY_UP)
			{
				m_inputIgnoreState = TextInputIgnoreState.NEXT_CALL;
			}
			return true;
		}
		return false;
	}

	private void HandleGUIInputInactive()
	{
		if (m_inputActive)
		{
			return;
		}
		m_inputFieldUI.SetCanvasActive(active: false);
		if (m_inputIgnoreState != 0)
		{
			if (m_inputIgnoreState == TextInputIgnoreState.NEXT_CALL)
			{
				m_inputIgnoreState = TextInputIgnoreState.INVALID;
			}
		}
		else if (!HearthstoneCheckoutBlocksInput())
		{
			ChatMgr.Get()?.HandleGUIInput();
		}
	}

	private void HandleGUIInputActive()
	{
		if (!m_inputActive || !PreprocessGUITextInput())
		{
			return;
		}
		Vector2 screenSize = new Vector2(Screen.width, Screen.height);
		Rect inputScreenRect = ComputeTextInputRect(screenSize);
		string input = ShowTextInput(inputScreenRect);
		ITouchScreenService touchScreenService = ServiceManager.Get<ITouchScreenService>();
		if (UseWindowsTouch() && !touchScreenService.IsVirtualKeyboardVisible() && InputCollection.GetMouseButtonDown(0) && inputScreenRect.Contains(touchScreenService.GetTouchPositionForGUI()))
		{
			touchScreenService.ShowKeyboard();
		}
		UpdateTextInputFocus();
		if (m_inputFocused && m_inputText != input)
		{
			if (m_inputNumber)
			{
				input = StringUtils.StripNonNumbers(input);
			}
			if (!m_inputMultiLine)
			{
				input = StringUtils.StripNewlines(input);
			}
			m_inputText = input;
			m_inputFieldUI.Text = input;
			if (m_inputUpdatedCallback != null)
			{
				m_inputUpdatedCallback(input);
			}
		}
	}

	private bool PreprocessGUITextInput()
	{
		UpdateTabKeyDown();
		if (m_inputPreprocessCallback != null)
		{
			m_inputPreprocessCallback();
			if (!m_inputActive)
			{
				return false;
			}
		}
		if (ProcessTextInputFinishKeys())
		{
			return false;
		}
		return true;
	}

	private void UpdateTabKeyDown()
	{
		m_tabKeyDown = Input.GetKeyDown(KeyCode.Tab);
	}

	private bool ProcessTextInputFinishKeys()
	{
		if (!m_inputFocused)
		{
			return false;
		}
		if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
		{
			m_inputIgnoreState = TextInputIgnoreState.COMPLETE_KEY_UP;
			CompleteTextInput();
			return true;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			m_inputIgnoreState = TextInputIgnoreState.CANCEL_KEY_UP;
			UserCancelTextInput();
			return true;
		}
		return false;
	}

	private string ShowTextInput(Rect inputScreenRect)
	{
		Rect normalizedOverlayRect = OverlayUI.Get().GetInputFieldRect(m_inputNormalizedRect);
		normalizedOverlayRect.y -= 1.5f;
		normalizedOverlayRect.height += 1.5f;
		int fontSize = ComputeTextInputFontSize(inputScreenRect.height);
		m_inputFieldUI.SetupTextProperties(fontSize, m_inputColor, m_inputAlignment);
		m_inputFieldUI.SetCanvasActive(active: true);
		m_inputFieldUI.SetInputRect(normalizedOverlayRect);
		return m_inputFieldUI.Text;
	}

	private void UpdateTextInputFocus()
	{
		if (m_inputNeedsFocus)
		{
			m_inputFieldUI.ActivateInputField();
			m_inputFocused = m_inputFieldUI.IsFocused;
			m_inputNeedsFocus = !m_inputFocused;
			return;
		}
		bool wasInputFocused = m_inputFocused;
		m_inputFocused = m_inputFieldUI.IsFocused;
		TextInputUnfocusedCallback callback = m_inputUnfocusedCallback;
		if (!m_inputFocused && wasInputFocused)
		{
			callback?.Invoke();
		}
	}

	private Rect ComputeTextInputRect(Vector2 screenSize)
	{
		float screenAspect = screenSize.x / screenSize.y;
		float aspectScalar = m_inputInitialScreenSize.x / m_inputInitialScreenSize.y / screenAspect;
		float heightScalar = screenSize.y / m_inputInitialScreenSize.y;
		float screenDistToCenter = (0.5f - m_inputNormalizedRect.x) * m_inputInitialScreenSize.x * heightScalar;
		return new Rect(screenSize.x * 0.5f - screenDistToCenter, m_inputNormalizedRect.y * screenSize.y - 1.5f, m_inputNormalizedRect.width * screenSize.x * aspectScalar, m_inputNormalizedRect.height * screenSize.y + 1.5f);
	}

	private int ComputeTextInputFontSize(float rectHeight)
	{
		int fontSize = Mathf.CeilToInt(rectHeight);
		fontSize = ((!Localization.IsIMELocale() && !IsIMEEverUsed) ? (fontSize - 4) : (fontSize - 4));
		return Mathf.Clamp(fontSize, 4, 96);
	}

	private bool HearthstoneCheckoutBlocksInput()
	{
		if (m_commerce == null && !ServiceManager.TryGet<HearthstoneCheckout>(out m_commerce))
		{
			return false;
		}
		return m_commerce.ShouldBlockInput;
	}

	private void OnTouchModeChangedCallback(Option option, object prevvalue, bool existed, object userdata)
	{
		UpdateIsTouchMode();
	}
}
