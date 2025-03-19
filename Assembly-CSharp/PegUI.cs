using System;
using System.Collections.Generic;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core;
using UnityEngine;

public class PegUI : MonoBehaviour
{
	public enum SWIPE_DIRECTION
	{
		RIGHT,
		LEFT
	}

	public delegate void DelSwipeListener(SWIPE_DIRECTION direction);

	public const float DEFAULT_SCREEN_DPI = 96f;

	public Camera orthographicUICam;

	private static readonly GameLayer[] HIT_TEST_PRIORITY = new GameLayer[11]
	{
		GameLayer.IgnoreFullScreenEffects,
		GameLayer.BackgroundUI,
		GameLayer.PerspectiveUI,
		GameLayer.CameraMask,
		GameLayer.UI,
		GameLayer.BattleNet,
		GameLayer.BattleNetFriendList,
		GameLayer.BattleNetChat,
		GameLayer.BattleNetDialog,
		GameLayer.HighPriorityUI,
		GameLayer.Reserved29
	};

	private List<Camera> m_UICams = new List<Camera>();

	private PegUIElement m_prevElement;

	private PegUIElement m_currentElement;

	private PegUIElement m_mouseDownElement;

	private static PegUI s_instance;

	private float m_mouseDownTimer;

	private float m_lastClickTimer;

	private Vector3 m_lastClickPosition;

	private const float PRESS_VS_TAP_TOLERANCE = 0.4f;

	private const float HOLD_TOLERANCE = 0.45f;

	private const float DOUBLECLICK_TOLERANCE = 0.7f;

	private const float DOUBLECLICK_COUNT_DISABLED = -1f;

	private const float MOUSEDOWN_COUNT_DISABLED = -1f;

	private List<PegUICustomBehavior> m_customBehaviors = new List<PegUICustomBehavior>();

	private HashSet<Component> m_newHitDetectionComponents = new HashSet<Component>();

	private DelSwipeListener m_swipeListener;

	private bool m_hasFocus = true;

	private bool m_uguiActive;

	private float m_dragToleranceDpiAdjustmentFactor = 1f;

	private Camera m_cameraPriorityHitCamera;

	public bool IsUsingRenderPassPriorityHitTest => m_newHitDetectionComponents.Count > 0;

	public Camera LastCameraPriorityHitCamera => m_cameraPriorityHitCamera;

	public static event Action<PegUIElement> OnReleasePreTrigger;

	private void Awake()
	{
		s_instance = this;
		m_lastClickPosition = Vector3.zero;
		base.gameObject.AddComponent<HSDontDestroyOnLoad>();
		if (Screen.dpi > 0f)
		{
			m_dragToleranceDpiAdjustmentFactor = Screen.dpi / 96f;
		}
	}

	private void OnDestroy()
	{
		if (UniversalInputManager.Get() != null)
		{
			UniversalInputManager.Get().UnregisterMouseOnOrOffScreenListener(OnMouseOnOrOffScreen);
		}
		HearthstoneApplication hearthstoneApplication = HearthstoneApplication.Get();
		if (hearthstoneApplication != null)
		{
			hearthstoneApplication.RemoveFocusChangedListener(OnAppFocusChanged);
		}
		s_instance = null;
	}

	private void Start()
	{
		Processor.QueueJob(HearthstoneJobs.CreateJobFromAction("PegUI.RegisterMouseOnOrOffScreenListener", RegisterMouseListener, ServiceManager.CreateServiceDependency(typeof(UniversalInputManager))));
		HearthstoneApplication hearthstoneApplication = HearthstoneApplication.Get();
		if (hearthstoneApplication != null)
		{
			hearthstoneApplication.AddFocusChangedListener(OnAppFocusChanged);
		}
	}

	private void Update()
	{
		CleanUpStaleRenderPassHitComponents();
		MouseInputUpdate();
	}

	public static PegUI Get()
	{
		return s_instance;
	}

	public static bool IsInitialized()
	{
		return s_instance != null;
	}

	public PegUIElement GetMousedOverElement()
	{
		return m_currentElement;
	}

	public PegUIElement GetMouseDownElement()
	{
		return m_mouseDownElement;
	}

	public PegUIElement GetPrevMousedOverElement()
	{
		return m_prevElement;
	}

	public void AddInputCamera(Camera cam)
	{
		if (cam == null)
		{
			Debug.Log("Trying to add a null camera!");
		}
		else
		{
			m_UICams.Add(cam);
		}
	}

	public void RemoveInputCamera(Camera cam)
	{
		if (cam != null)
		{
			m_UICams.Remove(cam);
		}
	}

	public PegUIElement FindHitElement()
	{
		RaycastHit unused;
		return FindHitElement(out unused);
	}

	public PegUIElement FindHitElement(out RaycastHit hit)
	{
		UniversalInputManager inputManager = UniversalInputManager.Get();
		if (inputManager.IsTouchMode() && !InputCollection.GetMouseButton(0) && !InputCollection.GetMouseButtonUp(0))
		{
			hit = default(RaycastHit);
			return null;
		}
		SceneDebugger sceneDebugger = null;
		if (ServiceManager.TryGet<SceneDebugger>(out sceneDebugger) && sceneDebugger.IsMouseOverGui())
		{
			hit = default(RaycastHit);
			return null;
		}
		if (m_newHitDetectionComponents.Count > 0 && inputManager.GetInputHitInfoByRenderPass(out hit, out m_cameraPriorityHitCamera))
		{
			return TryGetPegUIElementFromHit(hit);
		}
		if (inputManager.GetInputHitInfo(HIT_TEST_PRIORITY, out hit))
		{
			return TryGetPegUIElementFromHit(hit);
		}
		for (int i = m_UICams.Count - 1; i >= 0; i--)
		{
			Camera cam = m_UICams[i];
			if (cam == null)
			{
				m_UICams.RemoveAt(i);
			}
			else if (inputManager.GetInputHitInfo(cam, out hit))
			{
				return TryGetPegUIElementFromHit(hit);
			}
		}
		hit = default(RaycastHit);
		return null;
	}

	private PegUIElement TryGetPegUIElementFromHit(RaycastHit hit)
	{
		PegUIElement pegUiElement = hit.transform.GetComponent<PegUIElement>();
		if (pegUiElement != null)
		{
			return pegUiElement;
		}
		PegUIElementProxy pegUiElementProxy = hit.transform.GetComponent<PegUIElementProxy>();
		if (pegUiElementProxy != null)
		{
			return pegUiElementProxy.Owner;
		}
		return null;
	}

	public void DoMouseDown(PegUIElement element, Vector3 mouseDownPos)
	{
		m_currentElement = element;
		m_mouseDownElement = element;
		m_currentElement.TriggerPress();
		m_lastClickPosition = mouseDownPos;
		if (IsDragAmountAboveDragTolerance(m_currentElement))
		{
			m_currentElement.TriggerDrag();
		}
	}

	public void RemoveAsMouseDownElement(PegUIElement element)
	{
		if (!(m_mouseDownElement == null) && !(element != m_mouseDownElement))
		{
			m_mouseDownElement.TriggerReleaseAll(m_currentElement == m_mouseDownElement);
			m_mouseDownElement = null;
		}
	}

	public Vector3 GetDragDelta()
	{
		return InputCollection.GetMousePosition() - m_lastClickPosition;
	}

	private void MouseInputUpdate()
	{
		if (UniversalInputManager.Get() == null || !m_hasFocus || m_uguiActive)
		{
			return;
		}
		bool customResponse = false;
		foreach (PegUICustomBehavior customBehavior in m_customBehaviors)
		{
			if (customBehavior.UpdateUI())
			{
				customResponse = true;
				break;
			}
		}
		if (customResponse)
		{
			if (m_mouseDownElement != null)
			{
				m_mouseDownElement.TriggerOut();
			}
			m_mouseDownElement = null;
			return;
		}
		if (InputCollection.GetMouseButton(0) && m_mouseDownElement != null && m_lastClickPosition != Vector3.zero && IsDragAmountAboveDragTolerance(m_mouseDownElement))
		{
			m_mouseDownElement.TriggerDrag();
		}
		if (m_lastClickTimer != -1f)
		{
			m_lastClickTimer += Time.deltaTime;
		}
		if (m_mouseDownTimer != -1f)
		{
			m_mouseDownTimer += Time.deltaTime;
		}
		PegUIElement hitElement = FindHitElement();
		if (hitElement != null && HearthstoneApplication.IsInternal() && Options.Get().GetInt(Option.PEGUI_DEBUG) >= 3)
		{
			Debug.Log(string.Format("{0,-7} {1}", "HIT:", DebugUtils.GetHierarchyPath(hitElement, '/')));
		}
		bool inMouseModeOrHasFingerOnScreen = !UniversalInputManager.Get().IsTouchMode() || InputCollection.GetMouseButton(0) || InputCollection.GetMouseButtonUp(0);
		if (inMouseModeOrHasFingerOnScreen)
		{
			m_prevElement = m_currentElement;
		}
		if ((bool)hitElement && hitElement.IsEnabled())
		{
			m_currentElement = hitElement;
		}
		else if (inMouseModeOrHasFingerOnScreen)
		{
			m_currentElement = null;
		}
		if ((bool)m_prevElement && m_currentElement != m_prevElement)
		{
			if (PegCursor.Get() != null)
			{
				PegCursor.Get().SetMode(PegCursor.Mode.UP);
			}
			m_prevElement.TriggerOut();
			m_lastClickTimer = -1f;
		}
		if (InputCollection.GetMouseButton(0) && m_mouseDownElement != null && m_currentElement == m_mouseDownElement && m_mouseDownTimer > 0.45f)
		{
			m_mouseDownElement.TriggerHold();
		}
		if (m_currentElement == null)
		{
			if (PegCursor.Get() != null)
			{
				if (InputCollection.GetMouseButtonDown(0))
				{
					PegCursor.Get().SetMode(PegCursor.Mode.DOWN);
				}
				else if (!InputCollection.GetMouseButton(0))
				{
					PegCursor.Get().SetMode(PegCursor.Mode.UP);
				}
			}
			if ((bool)m_mouseDownElement && InputCollection.GetMouseButtonUp(0))
			{
				m_mouseDownElement.TriggerReleaseAll(mouseIsOver: false);
				m_mouseDownElement = null;
			}
		}
		else
		{
			if (!UpdateMouseLeftClick())
			{
				UpdateMouseLeftHold();
			}
			UpdateMouseRightClick();
			UpdateMouseOver();
		}
	}

	private void RegisterMouseListener()
	{
		UniversalInputManager.Get().RegisterMouseOnOrOffScreenListener(OnMouseOnOrOffScreen);
	}

	private bool UpdateMouseLeftClick()
	{
		bool handled = false;
		if (InputCollection.GetMouseButtonDown(0))
		{
			handled = true;
			if (PegCursor.Get() != null)
			{
				if (m_currentElement.GetCursorDown() != PegCursor.Mode.NONE)
				{
					PegCursor.Get().SetMode(m_currentElement.GetCursorDown());
				}
				else
				{
					PegCursor.Get().SetMode(PegCursor.Mode.DOWN);
				}
			}
			m_mouseDownTimer = 0f;
			if (UniversalInputManager.Get().IsTouchMode() && m_currentElement.GetReceiveOverWithMouseDown())
			{
				m_currentElement.TriggerOver();
			}
			m_currentElement.TriggerPress();
			m_lastClickPosition = InputCollection.GetMousePosition();
			m_mouseDownElement = m_currentElement;
		}
		if (InputCollection.GetMouseButtonUp(0))
		{
			handled = true;
			if (m_lastClickTimer > 0f && m_lastClickTimer <= 0.7f && m_currentElement.DoubleClickEnabled)
			{
				m_currentElement.TriggerDoubleClick();
				m_lastClickTimer = -1f;
			}
			else
			{
				if (m_mouseDownElement == m_currentElement || m_currentElement.GetReceiveReleaseWithoutMouseDown())
				{
					if (m_mouseDownTimer <= 0.4f)
					{
						m_currentElement.TriggerTap();
					}
					if (PegUI.OnReleasePreTrigger != null)
					{
						PegUI.OnReleasePreTrigger(m_currentElement);
					}
					m_currentElement.TriggerRelease();
				}
				if ((bool)m_mouseDownElement)
				{
					m_lastClickTimer = 0f;
					m_mouseDownElement.TriggerReleaseAll(m_currentElement == m_mouseDownElement);
					m_mouseDownElement = null;
				}
			}
			if (m_currentElement.GetReceiveOverWithMouseDown())
			{
				m_currentElement.TriggerOut();
			}
			if (PegCursor.Get() != null)
			{
				PegCursor.Get().SetMode((m_currentElement.GetCursorOver() != PegCursor.Mode.NONE) ? m_currentElement.GetCursorOver() : PegCursor.Mode.OVER);
			}
			m_mouseDownTimer = -1f;
			m_lastClickPosition = Vector3.zero;
			if (UniversalInputManager.Get().IsTouchMode())
			{
				m_currentElement = null;
				m_prevElement = null;
			}
		}
		return handled;
	}

	private bool UpdateMouseLeftHold()
	{
		if (!InputCollection.GetMouseButton(0))
		{
			return false;
		}
		if (m_currentElement.GetReceiveOverWithMouseDown() && m_currentElement != m_prevElement)
		{
			if (PegCursor.Get() != null)
			{
				if (m_currentElement.GetCursorOver() != PegCursor.Mode.NONE)
				{
					PegCursor.Get().SetMode(m_currentElement.GetCursorOver());
				}
				else
				{
					PegCursor.Get().SetMode(PegCursor.Mode.OVER);
				}
			}
			m_currentElement.TriggerOver();
		}
		return true;
	}

	private bool UpdateMouseRightClick()
	{
		bool handled = false;
		if (InputCollection.GetMouseButtonDown(1))
		{
			handled = true;
			if (m_currentElement != null)
			{
				m_currentElement.TriggerRightClick();
			}
		}
		return handled;
	}

	private void UpdateMouseOver()
	{
		if (m_currentElement == null || (UniversalInputManager.Get().IsTouchMode() && (!InputCollection.GetMouseButton(0) || !m_currentElement.GetReceiveOverWithMouseDown())) || m_currentElement == m_prevElement)
		{
			return;
		}
		if (PegCursor.Get() != null)
		{
			if (m_currentElement.GetCursorOver() != PegCursor.Mode.NONE)
			{
				PegCursor.Get().SetMode(m_currentElement.GetCursorOver());
			}
			else
			{
				PegCursor.Get().SetMode(PegCursor.Mode.OVER);
			}
		}
		m_currentElement.TriggerOver();
	}

	private void OnAppFocusChanged(bool focus, object userData)
	{
		m_hasFocus = focus;
	}

	public void OnUGUIActiveChanged(bool active)
	{
		m_uguiActive = active;
	}

	private void OnMouseOnOrOffScreen(bool onScreen)
	{
		if (!onScreen)
		{
			m_lastClickPosition = Vector3.zero;
			if (m_currentElement != null)
			{
				m_currentElement.TriggerOut();
				m_currentElement = null;
			}
			if (PegCursor.Get() != null)
			{
				PegCursor.Get().SetMode(PegCursor.Mode.UP);
			}
			if (m_prevElement != null)
			{
				m_prevElement.TriggerOut();
				m_prevElement = null;
			}
			m_lastClickTimer = -1f;
		}
	}

	private bool IsDragAmountAboveDragTolerance(PegUIElement draggedElement)
	{
		Vector3 dragVector = InputCollection.GetMousePosition() - m_lastClickPosition;
		Vector3 dragTolerance = draggedElement.GetDragTolerance();
		if (dragTolerance.x == 0f || !(Mathf.Abs(dragVector.x) > Mathf.Abs(dragTolerance.x * m_dragToleranceDpiAdjustmentFactor)))
		{
			if (dragTolerance.y != 0f)
			{
				return Mathf.Abs(dragVector.y) > Mathf.Abs(dragTolerance.y * m_dragToleranceDpiAdjustmentFactor);
			}
			return false;
		}
		return true;
	}

	public void EnableSwipeDetection(Rect swipeRect, DelSwipeListener listener)
	{
		m_swipeListener = listener;
	}

	public void CancelSwipeDetection(DelSwipeListener listener)
	{
		if (listener == m_swipeListener)
		{
			m_swipeListener = null;
		}
	}

	public void RegisterCustomBehavior(PegUICustomBehavior behavior)
	{
		m_customBehaviors.Add(behavior);
	}

	public void UnregisterCustomBehavior(PegUICustomBehavior behavior)
	{
		m_customBehaviors.Remove(behavior);
	}

	public void RegisterForRenderPassPriorityHitTest(Component component)
	{
		m_newHitDetectionComponents.Add(component);
	}

	public void UnregisterFromRenderPassPriorityHitTest(Component component)
	{
		m_newHitDetectionComponents.Remove(component);
	}

	private void CleanUpStaleRenderPassHitComponents()
	{
		m_newHitDetectionComponents.RemoveWhere((Component c) => c == null);
	}
}
