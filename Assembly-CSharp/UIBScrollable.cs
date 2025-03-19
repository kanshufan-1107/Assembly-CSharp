using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

[CustomEditClass]
public class UIBScrollable : PegUICustomBehavior
{
	public enum ScrollDirection
	{
		X,
		Y,
		Z
	}

	public enum HeightMode
	{
		UseHeightCallback,
		UseScrollableItem,
		UseBoxCollider
	}

	public enum ScrollWheelMode
	{
		ScaledToScrollSize,
		FixedRate
	}

	public interface IContent
	{
		UIBScrollable Scrollable { get; }
	}

	private class ContentComponent : MonoBehaviour, IContent, IScrollingVisibilityProvider
	{
		public UIBScrollable Scrollable { get; set; }

		public void AddFastVisibleAffectedObject(GameObject topObj, Vector3 extents, bool visible, float buffer, Action<int, int, bool> callback = null)
		{
			Scrollable.AddFastVisibleAffectedObject(topObj, extents, visible, buffer, callback);
		}

		public void RemoveFastVisibleAffectedObject(GameObject obj)
		{
			Scrollable.RemoveFastVisibleAffectedObject(obj);
		}

		public void ChangeExtentsOnFastVisibleObject(GameObject topObj, Vector3 extents)
		{
			Scrollable.ChangeExtentsOnFastVisibleObject(topObj, extents);
		}

		public void UpdateVisibility(GameObject topObj)
		{
			Scrollable.UpdateAndFireVisibleAffectedObjects();
		}
	}

	public delegate void EnableScroll(bool enabled);

	public delegate float ScrollHeightCallback();

	public delegate void OnScrollComplete(float percentage);

	public delegate void OnTouchScrollStarted();

	public delegate void OnTouchScrollEnded();

	public delegate void VisibleAffected(GameObject obj, bool visible);

	public class VisibleAffectedObject
	{
		public GameObject Obj;

		public Vector3 Extents;

		public bool Visible;

		public VisibleAffected Callback;
	}

	protected class FastVisibleAffectedObject
	{
		public GameObject TopObj;

		public Vector3 Extents;

		public Action<int, int, bool> Callback;

		public float Buffer;
	}

	[CustomEditField(Sections = "Camera Settings")]
	public bool m_UseCameraFromLayer;

	[CustomEditField(Sections = "Preferences")]
	public float m_ScrollWheelAmount = 0.1f;

	[CustomEditField(Sections = "Preferences")]
	public ScrollWheelMode m_ScrollWheelMode;

	[CustomEditField(Sections = "Preferences")]
	public float m_ScrollBottomPadding;

	[CustomEditField(Sections = "Preferences")]
	public iTween.EaseType m_ScrollEaseType = iTween.Defaults.easeType;

	[CustomEditField(Sections = "Preferences")]
	public float m_ScrollTweenTime = 0.2f;

	[CustomEditField(Sections = "Preferences")]
	public ScrollDirection m_ScrollPlane = ScrollDirection.Z;

	[CustomEditField(Sections = "Preferences")]
	public bool m_ScrollDirectionReverse;

	[CustomEditField(Sections = "Preferences")]
	[Tooltip("If scrolling is active, all PegUI calls will be suppressed")]
	public bool m_OverridePegUI;

	[CustomEditField(Sections = "Preferences")]
	public bool m_ForceScrollAreaHitTest;

	[CustomEditField(Sections = "Preferences")]
	public bool m_ScrollOnMouseDrag;

	[CustomEditField(Sections = "Bounds Settings")]
	public BoxCollider m_ScrollBounds;

	[Tooltip("Determines full area finger is allowed continue scrolling once it has started. Position this behind/below the ScrollBounds.")]
	[CustomEditField(Sections = "Optional Bounds Settings")]
	public BoxCollider m_TouchDragFullArea;

	[CustomEditField(Sections = "Thumb Settings")]
	public BoxCollider m_ScrollTrack;

	[CustomEditField(Sections = "Thumb Settings")]
	public ScrollBarThumb m_ScrollThumb;

	[CustomEditField(Sections = "Thumb Settings")]
	public bool m_HideThumbWhenDisabled;

	[CustomEditField(Sections = "Thumb Settings")]
	public GameObject m_scrollTrackCover;

	[CustomEditField(Sections = "Bounds Settings")]
	[SerializeField]
	private GameObject m_ScrollObject;

	[CustomEditField(Sections = "Bounds Settings")]
	public float m_VisibleObjectThreshold;

	[CustomEditField(Sections = "Preferences")]
	public bool m_UseScrollContentsInHitTest = true;

	[CustomEditField(Sections = "Touch Settings")]
	[Tooltip("Drag distance required to initiate deck tile dragging (inches)")]
	public float m_DeckTileDragThreshold = 0.04f;

	[CustomEditField(Sections = "Touch Settings")]
	[Tooltip("Drag distance required to initiate scroll dragging (inches)")]
	public float m_ScrollDragThreshold = 0.04f;

	[Tooltip("Stopping speed for scrolling after the user has let go")]
	[CustomEditField(Sections = "Touch Settings")]
	public float m_MinKineticScrollSpeed = 0.01f;

	[CustomEditField(Sections = "Touch Settings")]
	[Tooltip("Resistance for slowing down scrolling after the user has let go")]
	public float m_KineticScrollFriction = 6f;

	[CustomEditField(Sections = "Touch Settings")]
	[Tooltip("Strength of the boundary springs")]
	public float m_ScrollBoundsSpringK = 700f;

	[Tooltip("Distance at which the out-of-bounds scroll value will snapped to 0 or 1")]
	[CustomEditField(Sections = "Touch Settings")]
	public float m_MinOutOfBoundsScrollValue = 0.001f;

	[Tooltip("Use this to match scaling issues.")]
	[CustomEditField(Sections = "Touch Settings")]
	public float m_ScrollDeltaMultiplier = 1f;

	[CustomEditField(Sections = "Touch Settings")]
	public List<BoxCollider> m_TouchScrollBlockers = new List<BoxCollider>();

	public HeightMode m_HeightMode = HeightMode.UseScrollableItem;

	private bool m_Enabled = true;

	private float m_ScrollValue;

	private float m_LastTouchScrollValue;

	private bool m_InputBlocked;

	private bool m_Pause;

	private bool m_PauseUpdateScrollHeight;

	private bool m_overrideHideThumb;

	private Vector2? m_TouchBeginScreenPos;

	private Vector3? m_TouchDragBeginWorldPos;

	private float m_TouchDragBeginScrollValue;

	private float m_prevScrollValue;

	private Vector3 m_ScrollAreaStartPos;

	private float m_ScrollThumbStartYPos;

	private ScrollHeightCallback m_ScrollHeightCallback;

	private List<EnableScroll> m_EnableScrollListeners = new List<EnableScroll>();

	private float m_LastScrollHeightRecorded;

	private float m_PolledScrollHeight;

	private List<VisibleAffectedObject> m_VisibleAffectedObjects = new List<VisibleAffectedObject>();

	private List<FastVisibleAffectedObject> m_fastVisibleAffectedObjects = new List<FastVisibleAffectedObject>();

	private List<OnTouchScrollStarted> m_TouchScrollStartedListeners = new List<OnTouchScrollStarted>();

	private List<OnTouchScrollEnded> m_TouchScrollEndedListeners = new List<OnTouchScrollEnded>();

	private bool m_ForceShowVisibleAffectedObjects;

	private List<UIBScrollableItem> m_scrollableItems = new List<UIBScrollableItem>();

	private int m_currentHierarchyCount;

	private Camera m_scrollTrackCamera;

	private CameraOverridePass m_scrollTrackCameraOverridePass;

	private static Map<string, float> s_SavedScrollValues = new Map<string, float>();

	[CustomEditField(Sections = "Scroll")]
	public float ScrollValue
	{
		get
		{
			return m_ScrollValue;
		}
		set
		{
			if (!Application.isEditor)
			{
				SetScroll(value, blockInputWhileScrolling: false, clamp: false);
			}
		}
	}

	[Overridable]
	public float ImmediateScrollValue
	{
		get
		{
			return m_ScrollValue;
		}
		set
		{
			SetScrollImmediate(value);
		}
	}

	[Overridable]
	public float ScrollBottomPadding
	{
		get
		{
			return m_ScrollBottomPadding;
		}
		set
		{
			m_ScrollBottomPadding = value;
		}
	}

	[Overridable]
	public Vector3 ScrollBoundsCenter
	{
		get
		{
			if (!(m_ScrollBounds != null))
			{
				return Vector3.zero;
			}
			return m_ScrollBounds.center;
		}
		set
		{
			if (m_ScrollBounds != null)
			{
				m_ScrollBounds.center = value;
			}
		}
	}

	[Overridable]
	public Vector3 ScrollBoundsSize
	{
		get
		{
			if (!(m_ScrollBounds != null))
			{
				return Vector3.zero;
			}
			return m_ScrollBounds.size;
		}
		set
		{
			if (m_ScrollBounds != null)
			{
				m_ScrollBounds.size = value;
			}
		}
	}

	public GameObject ScrollObject
	{
		get
		{
			return m_ScrollObject;
		}
		set
		{
			m_ScrollObject = value;
			SetupScrollObject();
		}
	}

	public static void DefaultVisibleAffectedCallback(GameObject obj, bool visible)
	{
		if (obj.activeSelf != visible)
		{
			obj.SetActive(visible);
		}
	}

	protected override void Awake()
	{
		ResetScrollStartPosition();
		SaveScrollThumbStartHeight();
		if (m_ScrollTrack != null && !UniversalInputManager.UsePhoneUI)
		{
			PegUIElement pegelem = m_ScrollTrack.GetComponent<PegUIElement>();
			if (pegelem != null)
			{
				pegelem.AddEventListener(UIEventType.PRESS, delegate
				{
					StartDragging();
				});
			}
			PegUIElement thumbPegelem = m_ScrollThumb.GetComponent<PegUIElement>();
			if (thumbPegelem != null)
			{
				thumbPegelem.AddEventListener(UIEventType.PRESS, delegate
				{
					StartDragging();
				});
			}
		}
		if (m_OverridePegUI)
		{
			base.Awake();
		}
		if (m_ScrollObject != null)
		{
			SetupScrollObject();
		}
	}

	public void RegisterScrollableItem(UIBScrollableItem scrollableItem)
	{
		if (!m_scrollableItems.Contains(scrollableItem))
		{
			m_scrollableItems.Add(scrollableItem);
		}
	}

	public void RemoveScrollableItem(UIBScrollableItem scrollableItem)
	{
		m_scrollableItems.Remove(scrollableItem);
	}

	public void Start()
	{
		if (m_scrollTrackCover != null)
		{
			m_scrollTrackCover.SetActive(value: false);
		}
	}

	protected override void OnDestroy()
	{
		if (m_OverridePegUI)
		{
			base.OnDestroy();
		}
	}

	private void Update()
	{
		int count = base.transform.hierarchyCount;
		if (count != m_currentHierarchyCount)
		{
			m_currentHierarchyCount = count;
			SetupScrollObject();
		}
		UpdateScroll();
		if (!m_Enabled || m_InputBlocked || m_Pause || GetScrollCamera() == null)
		{
			return;
		}
		if (IsInputOverScrollableArea(m_ScrollBounds, out var _))
		{
			float scrollwheelScroll = Input.GetAxis("Mouse ScrollWheel");
			if (scrollwheelScroll != 0f)
			{
				float magnitude = 0f;
				magnitude = ((m_ScrollWheelMode != ScrollWheelMode.FixedRate) ? (m_ScrollWheelAmount * 10f) : (m_ScrollWheelAmount / GetTotalWorldScrollHeight()));
				AddScroll(0f - scrollwheelScroll * magnitude);
			}
		}
		if (m_ScrollThumb != null && m_ScrollThumb.IsDragging())
		{
			DragThumb();
		}
		else if (UniversalInputManager.Get().IsTouchMode() || m_ScrollOnMouseDrag)
		{
			DragContent();
		}
	}

	private void SetupScrollObject()
	{
		if (m_ScrollObject == null)
		{
			m_scrollableItems.Clear();
			return;
		}
		ContentComponent contentComponent = m_ScrollObject.GetComponent<ContentComponent>();
		if (contentComponent == null)
		{
			contentComponent = m_ScrollObject.AddComponent<ContentComponent>();
		}
		else if (!contentComponent.hideFlags.HasFlag(HideFlags.DontSave))
		{
			if (Application.IsPlaying(this))
			{
				UnityEngine.Object.Destroy(contentComponent);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(contentComponent);
			}
			contentComponent = m_ScrollObject.AddComponent<ContentComponent>();
		}
		contentComponent.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
		contentComponent.Scrollable = this;
		m_ScrollObject.GetComponentsInChildren(includeInactive: true, m_scrollableItems);
		int i = 0;
		for (int iMax = m_scrollableItems.Count; i < iMax; i++)
		{
			m_scrollableItems[i].SetScrollableParent(contentComponent);
		}
	}

	private bool IsInputOverScrollableArea(BoxCollider scrollableBounds, out RaycastHit hitInfo)
	{
		Camera scrollCamera = GetScrollCamera();
		if (UniversalInputManager.Get() == null || scrollCamera == null || m_ScrollBounds == null)
		{
			hitInfo = default(RaycastHit);
			return false;
		}
		bool isInputOver = false;
		isInputOver = (m_ForceScrollAreaHitTest ? UniversalInputManager.Get().ForcedInputIsOver(scrollCamera, scrollableBounds.gameObject, out hitInfo) : ((!PegUI.IsInitialized() || !PegUI.Get().IsUsingRenderPassPriorityHitTest) ? UniversalInputManager.Get().InputIsOver(scrollCamera, scrollableBounds.gameObject, out hitInfo) : UniversalInputManager.Get().InputIsOverByRenderPass(scrollableBounds.gameObject, out hitInfo)));
		if (m_UseScrollContentsInHitTest && m_ScrollObject != null)
		{
			isInputOver |= hitInfo.collider != null && hitInfo.collider.transform.IsChildOf(m_ScrollObject.transform);
		}
		return isInputOver;
	}

	public override bool UpdateUI()
	{
		if (IsTouchDragging())
		{
			return m_Enabled;
		}
		return false;
	}

	public void ResetScrollStartPosition()
	{
		if (m_ScrollObject != null)
		{
			m_ScrollAreaStartPos = m_ScrollObject.transform.localPosition;
		}
	}

	public void ResetScrollStartPosition(Vector3 position)
	{
		if (m_ScrollObject != null)
		{
			m_ScrollAreaStartPos = position;
		}
	}

	public void AddVisibleAffectedObject(GameObject obj, Vector3 extents, bool visible, VisibleAffected callback = null)
	{
		m_VisibleAffectedObjects.Add(new VisibleAffectedObject
		{
			Obj = obj,
			Extents = extents,
			Visible = visible,
			Callback = ((callback == null) ? new VisibleAffected(DefaultVisibleAffectedCallback) : callback)
		});
	}

	public void AddFastVisibleAffectedObject(GameObject obj, Vector3 extents, bool visible, float buffer, Action<int, int, bool> callback = null)
	{
		m_fastVisibleAffectedObjects.Add(new FastVisibleAffectedObject
		{
			TopObj = obj,
			Extents = extents,
			Buffer = buffer,
			Callback = callback
		});
	}

	public void ChangeExtentsOnFastVisibleObject(GameObject topObj, Vector3 extents)
	{
		if (topObj == null)
		{
			Log.UIFramework.PrintError("Null TopObj passed into ChangeExtentsOnFastVisibleObject");
			return;
		}
		foreach (FastVisibleAffectedObject obj in m_fastVisibleAffectedObjects)
		{
			if (obj.TopObj == topObj)
			{
				obj.Extents = extents;
				return;
			}
		}
		Log.UIFramework.PrintError("Fast visible object {0} not registered", topObj.gameObject.name);
	}

	public void RemoveVisibleAffectedObject(GameObject obj, VisibleAffected callback)
	{
		m_VisibleAffectedObjects.RemoveAll((VisibleAffectedObject o) => (object)o.Obj == obj && o.Callback == callback);
	}

	public void RemoveFastVisibleAffectedObject(GameObject obj)
	{
		for (int i = 0; i < m_fastVisibleAffectedObjects.Count; i++)
		{
			if (m_fastVisibleAffectedObjects[i].TopObj == obj)
			{
				m_fastVisibleAffectedObjects.RemoveAt(i);
				break;
			}
		}
	}

	public void ClearVisibleAffectObjects()
	{
		m_VisibleAffectedObjects.Clear();
	}

	public IEnumerable<VisibleAffectedObject> GetVisibleAffectedObjects()
	{
		return m_VisibleAffectedObjects;
	}

	public void ForceVisibleAffectedObjectsShow(bool show)
	{
		if (m_ForceShowVisibleAffectedObjects != show)
		{
			m_ForceShowVisibleAffectedObjects = show;
			UpdateAndFireVisibleAffectedObjects();
		}
	}

	public void AddEnableScrollListener(EnableScroll dlg)
	{
		m_EnableScrollListeners.Add(dlg);
	}

	public void RemoveEnableScrollListener(EnableScroll dlg)
	{
		m_EnableScrollListeners.Remove(dlg);
	}

	public void AddTouchScrollStartedListener(OnTouchScrollStarted dlg)
	{
		m_TouchScrollStartedListeners.Add(dlg);
	}

	public void RemoveTouchScrollStartedListener(OnTouchScrollStarted dlg)
	{
		m_TouchScrollStartedListeners.Remove(dlg);
	}

	public void AddTouchScrollEndedListener(OnTouchScrollEnded dlg)
	{
		m_TouchScrollEndedListeners.Add(dlg);
	}

	public void RemoveTouchScrollEndedListener(OnTouchScrollEnded dlg)
	{
		m_TouchScrollEndedListeners.Remove(dlg);
	}

	public void Pause(bool pause)
	{
		m_Pause = pause;
	}

	public void PauseUpdateScrollHeight(bool pause)
	{
		m_PauseUpdateScrollHeight = pause;
	}

	public void Enable(bool enable)
	{
		if (m_Enabled != enable)
		{
			m_Enabled = enable;
			if (m_scrollTrackCover != null)
			{
				m_scrollTrackCover.SetActive(!enable);
			}
			RefreshShowThumb();
			if (m_Enabled || UniversalInputManager.Get().IsTouchMode())
			{
				ResetTouchDrag();
			}
			FireEnableScrollEvent();
		}
	}

	public bool IsEnabled()
	{
		return m_Enabled;
	}

	public bool IsEnabledAndScrollable()
	{
		if (m_Enabled)
		{
			return IsScrollNeeded();
		}
		return false;
	}

	public float GetScroll()
	{
		return m_ScrollValue;
	}

	public void SaveScroll(string savedName)
	{
		s_SavedScrollValues[savedName] = m_ScrollValue;
	}

	public void LoadScroll(string savedName, bool snap)
	{
		float percentage = 0f;
		if (s_SavedScrollValues.TryGetValue(savedName, out percentage))
		{
			if (snap)
			{
				SetScrollSnap(percentage);
			}
			else
			{
				SetScroll(percentage);
			}
			ResetTouchDrag();
		}
	}

	public bool EnableIfNeeded()
	{
		bool scrollNeeded = IsScrollNeeded();
		Enable(scrollNeeded);
		return scrollNeeded;
	}

	public bool IsScrollNeeded()
	{
		return GetTotalWorldScrollHeight() > 0.16f;
	}

	public float PollScrollHeight()
	{
		switch (m_HeightMode)
		{
		case HeightMode.UseHeightCallback:
			if (m_ScrollHeightCallback == null)
			{
				return m_PolledScrollHeight;
			}
			return m_ScrollHeightCallback();
		case HeightMode.UseScrollableItem:
			return GetScrollableItemsHeight();
		default:
			return 0f;
		}
	}

	public float GetPolledScrollHeight()
	{
		return m_PolledScrollHeight;
	}

	public void SetScroll(float percentage, bool blockInputWhileScrolling = false, bool clamp = true)
	{
		SetScroll(percentage, null, blockInputWhileScrolling, clamp);
	}

	public void SetScroll(float percentage, iTween.EaseType tweenType, float tweenTime, bool blockInputWhileScrolling = false, bool clamp = true)
	{
		SetScroll(percentage, null, tweenType, tweenTime, blockInputWhileScrolling, clamp);
	}

	public void SetScrollSnap(float percentage, bool clamp = true)
	{
		SetScrollSnap(percentage, null, clamp);
	}

	public void SetScroll(float percentage, OnScrollComplete scrollComplete, bool blockInputWhileScrolling = false, bool clamp = true)
	{
		StartCoroutine(SetScrollWait(percentage, scrollComplete, blockInputWhileScrolling, tween: true, null, null, clamp));
	}

	public void SetScroll(float percentage, OnScrollComplete scrollComplete, iTween.EaseType tweenType, float tweenTime, bool blockInputWhileScrolling = false, bool clamp = true)
	{
		StartCoroutine(SetScrollWait(percentage, scrollComplete, blockInputWhileScrolling, tween: true, tweenType, tweenTime, clamp));
	}

	public void SetScrollSnap(float percentage, OnScrollComplete scrollComplete, bool clamp = true)
	{
		m_PolledScrollHeight = PollScrollHeight();
		m_LastScrollHeightRecorded = m_PolledScrollHeight;
		ScrollTo(percentage, scrollComplete, blockInputWhileScrolling: false, tween: false, null, null, clamp);
		ResetTouchDrag();
	}

	public void StopScroll()
	{
		Vector3 startpos = m_ScrollAreaStartPos;
		Vector3 endpos = startpos;
		Vector3 scrollVec = GetTotalScrollHeightVector(convertToLocalSpace: true);
		endpos += scrollVec * (m_ScrollDirectionReverse ? (-1f) : 1f);
		Vector3 position = m_ScrollObject.transform.localPosition;
		float currentActualScroll = (((endpos - startpos).magnitude > float.Epsilon) ? ((position - startpos).magnitude / (endpos - startpos).magnitude) : 0f);
		iTween.Stop(m_ScrollObject);
		SetScrollImmediate(currentActualScroll);
	}

	public void SetScrollHeightCallback(ScrollHeightCallback dlg, bool refresh = false, bool resetScroll = false)
	{
		float? setScroll = null;
		if (resetScroll)
		{
			setScroll = 0f;
		}
		SetScrollHeightCallback(dlg, setScroll, refresh);
	}

	public void SetScrollHeightCallback(ScrollHeightCallback dlg, float? setResetScroll, bool refresh = false)
	{
		m_VisibleAffectedObjects.Clear();
		m_ScrollHeightCallback = dlg;
		if (setResetScroll.HasValue)
		{
			m_ScrollValue = setResetScroll.Value;
			ResetTouchDrag();
		}
		if (refresh)
		{
			UpdateScroll();
			UpdateThumbPosition();
			UpdateScrollObjectPosition(tween: true, null, null, null);
		}
		m_PolledScrollHeight = PollScrollHeight();
		m_LastScrollHeightRecorded = m_PolledScrollHeight;
	}

	public void SetHeight(float height)
	{
		m_ScrollHeightCallback = null;
		m_PolledScrollHeight = height;
		UpdateHeight();
	}

	public void UpdateScroll()
	{
		if (!m_PauseUpdateScrollHeight)
		{
			m_PolledScrollHeight = PollScrollHeight();
			UpdateHeight();
		}
	}

	public void CenterWorldPosition(Vector3 position)
	{
		float percentage = m_ScrollObject.transform.InverseTransformPoint(position)[(int)m_ScrollPlane] / (0f - (m_PolledScrollHeight + m_ScrollBottomPadding)) * 2f - 0.5f;
		StartCoroutine(BlockInput(m_ScrollTweenTime));
		SetScroll(percentage);
	}

	public bool IsObjectVisibleInScrollArea(GameObject obj, Vector3 extents, bool fullyVisible = false)
	{
		int comp = (int)m_ScrollPlane;
		float min = obj.transform.position[comp] - extents[comp];
		float max = obj.transform.position[comp] + extents[comp];
		Bounds scrollBounds = m_ScrollBounds.bounds;
		float scrollMinThreshold = scrollBounds.min[comp] - m_VisibleObjectThreshold;
		float scrollMaxThreshold = scrollBounds.max[comp] + m_VisibleObjectThreshold;
		bool minEdgeInArea = min >= scrollMinThreshold && min <= scrollMaxThreshold;
		bool maxEdgeInArea = max >= scrollMinThreshold && max <= scrollMaxThreshold;
		bool minEdgeBeforeMin = min <= scrollMinThreshold;
		bool maxEdgeAfterMax = max >= scrollMaxThreshold;
		if (fullyVisible)
		{
			return minEdgeInArea && maxEdgeInArea;
		}
		if (!(minEdgeInArea || maxEdgeInArea))
		{
			return minEdgeBeforeMin && maxEdgeAfterMax;
		}
		return true;
	}

	private void GetFastVisibleRange(FastVisibleAffectedObject item, out int startIndex, out int endIndex, out bool isVisible)
	{
		int scrollDirection = (int)m_ScrollPlane;
		Bounds scrollBounds = m_ScrollBounds.bounds;
		float num = scrollBounds.size[scrollDirection];
		float topBounds = scrollBounds.max[scrollDirection];
		float scrollDistance = item.TopObj.transform.position[scrollDirection] - item.Extents[scrollDirection] - topBounds;
		float individualSize = item.Extents[scrollDirection] * 2f;
		int amountInBounds = Mathf.CeilToInt(num / individualSize);
		int startBuffer = Mathf.CeilToInt((float)amountInBounds * item.Buffer);
		int endBuffer = startBuffer * 2;
		startIndex = Mathf.FloorToInt(scrollDistance / individualSize) - startBuffer + 1;
		endIndex = startIndex + amountInBounds + endBuffer;
		isVisible = endIndex > 0;
	}

	public bool CenterObjectInView(GameObject obj, float positionOffset, OnScrollComplete scrollComplete, iTween.EaseType tweenType, float tweenTime, bool blockInputWhileScrolling = false)
	{
		float axisExtent = m_ScrollBounds.bounds.extents.z;
		return ScrollObjectIntoView(obj, positionOffset, axisExtent, scrollComplete, tweenType, tweenTime, blockInputWhileScrolling);
	}

	public bool ScrollObjectIntoView(GameObject obj, float positionOffset, float axisExtent, OnScrollComplete scrollComplete, iTween.EaseType tweenType, float tweenTime, bool blockInputWhileScrolling = false)
	{
		int comp = (int)m_ScrollPlane;
		float min = obj.transform.position[comp] + positionOffset - axisExtent;
		float max = obj.transform.position[comp] + positionOffset + axisExtent;
		Bounds scrollBounds = m_ScrollBounds.bounds;
		float scrollMinThreshold = scrollBounds.min[comp] - m_VisibleObjectThreshold;
		float scrollMaxThreshold = scrollBounds.max[comp] + m_VisibleObjectThreshold;
		bool minEdgeInArea = min >= scrollMinThreshold;
		bool maxEdgeInArea = max <= scrollMaxThreshold;
		if (minEdgeInArea && maxEdgeInArea)
		{
			return false;
		}
		float percentage = 0f;
		if (!minEdgeInArea)
		{
			float scrollHeight = GetTotalScrollHeightVector().z;
			if (scrollHeight == 0f)
			{
				Debug.LogWarning("UIBScrollable.ScrollObjectIntoView() - scrollHeight calculated as 0, cannot calculate scroll percentage!");
			}
			else
			{
				float percentToMove = Mathf.Abs(Math.Abs(scrollMinThreshold - min) / scrollHeight);
				percentage = m_ScrollValue + percentToMove;
			}
		}
		else if (!maxEdgeInArea)
		{
			float scrollHeight2 = GetTotalScrollHeightVector().z;
			if (scrollHeight2 == 0f)
			{
				Debug.LogWarning("UIBScrollable.ScrollObjectIntoView() - scrollHeight calculated as 0, cannot calculate scroll percentage!");
			}
			else
			{
				float percentToMove2 = Mathf.Abs(Math.Abs(scrollMaxThreshold - max) / scrollHeight2);
				percentage = m_ScrollValue - percentToMove2;
			}
		}
		SetScroll(percentage, scrollComplete, tweenType, tweenTime, blockInputWhileScrolling);
		return true;
	}

	public bool IsDragging()
	{
		if (!(m_ScrollThumb != null) || !m_ScrollThumb.IsDragging())
		{
			return m_TouchBeginScreenPos.HasValue;
		}
		return true;
	}

	public bool IsTouchDragging()
	{
		if (!m_TouchBeginScreenPos.HasValue || m_PolledScrollHeight == 0f)
		{
			m_TouchBeginScreenPos = null;
			return false;
		}
		float verticalDelta = Mathf.Abs(InputCollection.GetMousePosition().y - m_TouchBeginScreenPos.Value.y);
		return m_ScrollDragThreshold * ((Screen.dpi > 0f) ? Screen.dpi : 96f) <= verticalDelta;
	}

	public void SetScrollImmediate(float percentage)
	{
		ScrollTo(percentage, null, blockInputWhileScrolling: false, tween: false, null, 0f, clamp: true);
		ResetTouchDrag();
	}

	public void SetScrollImmediate(float percentage, OnScrollComplete scrollComplete, bool blockInputWhileScrolling, bool tween, iTween.EaseType? tweenType, float? tweenTime, bool clamp)
	{
		ScrollTo(percentage, scrollComplete, blockInputWhileScrolling, tween, tweenType, tweenTime, clamp);
		ResetTouchDrag();
	}

	public void SetHideThumb(bool value)
	{
		m_overrideHideThumb = value;
		RefreshShowThumb();
	}

	private void RefreshShowThumb()
	{
		if (!(m_ScrollThumb != null))
		{
			return;
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_ScrollThumb.gameObject.SetActive(value: false);
			return;
		}
		bool isThumbShown = (m_Enabled || !m_HideThumbWhenDisabled) && !m_overrideHideThumb;
		if (isThumbShown != m_ScrollThumb.gameObject.activeSelf)
		{
			m_ScrollThumb.gameObject.SetActive(isThumbShown);
			if (isThumbShown)
			{
				UpdateThumbPosition();
			}
		}
	}

	private void StartDragging()
	{
		if (!m_InputBlocked && !m_Pause && m_Enabled)
		{
			m_ScrollThumb.StartDragging();
		}
	}

	private void UpdateHeight()
	{
		if (Mathf.Abs(m_PolledScrollHeight - m_LastScrollHeightRecorded) > 0.001f)
		{
			if (!EnableIfNeeded())
			{
				m_ScrollValue = 0f;
			}
			UpdateThumbPosition();
			UpdateScrollObjectPosition(tween: false, null, null, null);
			ResetTouchDrag();
		}
		m_LastScrollHeightRecorded = m_PolledScrollHeight;
	}

	private void DragContent()
	{
		if (InputCollection.GetMouseButtonDown(0))
		{
			if (GetWorldTouchPosition().HasValue)
			{
				m_TouchBeginScreenPos = InputCollection.GetMousePosition();
				return;
			}
		}
		else if (InputCollection.GetMouseButtonUp(0))
		{
			m_TouchBeginScreenPos = null;
			m_TouchDragBeginWorldPos = null;
			FireTouchEndEvent();
		}
		if (m_TouchDragBeginWorldPos.HasValue)
		{
			Vector3? worldTouchPos = GetWorldTouchPositionOnDragArea();
			if (worldTouchPos.HasValue)
			{
				int scrollPlane = (int)m_ScrollPlane;
				m_LastTouchScrollValue = m_ScrollValue;
				float worldDelta = worldTouchPos.Value[scrollPlane] - m_TouchDragBeginWorldPos.Value[scrollPlane];
				float scrollDelta = GetScrollValueDelta(worldDelta);
				float scrollValue = m_TouchDragBeginScrollValue + scrollDelta;
				float outOfBoundsDist = GetOutOfBoundsDist(scrollValue);
				if (outOfBoundsDist != 0f)
				{
					outOfBoundsDist = Mathf.Log10(Mathf.Abs(outOfBoundsDist) + 1f) * Mathf.Sign(outOfBoundsDist);
					scrollValue = ((outOfBoundsDist < 0f) ? outOfBoundsDist : (outOfBoundsDist + 1f));
				}
				ScrollTo(Mathf.Lerp(m_prevScrollValue, scrollValue, 0.9f), null, blockInputWhileScrolling: false, tween: false, null, null, clamp: false);
			}
			return;
		}
		if (m_TouchBeginScreenPos.HasValue)
		{
			float horizontalDelta = Mathf.Abs(InputCollection.GetMousePosition().x - m_TouchBeginScreenPos.Value.x);
			float verticalDelta = Mathf.Abs(InputCollection.GetMousePosition().y - m_TouchBeginScreenPos.Value.y);
			bool num = horizontalDelta > m_DeckTileDragThreshold * ((Screen.dpi > 0f) ? Screen.dpi : 96f);
			bool canStartScroll = verticalDelta > m_ScrollDragThreshold * ((Screen.dpi > 0f) ? Screen.dpi : 96f);
			if (num && (horizontalDelta >= verticalDelta || !canStartScroll))
			{
				m_TouchBeginScreenPos = null;
			}
			else if (canStartScroll)
			{
				m_TouchDragBeginWorldPos = GetWorldTouchPositionOnDragArea();
				m_TouchDragBeginScrollValue = m_ScrollValue;
				m_LastTouchScrollValue = m_ScrollValue;
				FireTouchStartEvent();
			}
			return;
		}
		float speed = (m_ScrollValue - m_LastTouchScrollValue) / Time.fixedDeltaTime;
		float outOfBoundsDist2 = GetOutOfBoundsDist(m_ScrollValue);
		if (outOfBoundsDist2 != 0f)
		{
			if (Mathf.Abs(outOfBoundsDist2) >= m_MinOutOfBoundsScrollValue)
			{
				float springForce = (0f - m_ScrollBoundsSpringK) * outOfBoundsDist2 - Mathf.Sqrt(4f * m_ScrollBoundsSpringK) * speed;
				speed += springForce * Time.fixedDeltaTime;
				m_LastTouchScrollValue = m_ScrollValue;
				ScrollTo(m_ScrollValue + speed * Time.fixedDeltaTime, null, blockInputWhileScrolling: false, tween: false, null, null, clamp: false);
			}
			if (Mathf.Abs(GetOutOfBoundsDist(m_ScrollValue)) < m_MinOutOfBoundsScrollValue)
			{
				ScrollTo(Mathf.Round(m_ScrollValue), null, blockInputWhileScrolling: false, tween: false, null, null, clamp: false);
				m_LastTouchScrollValue = m_ScrollValue;
			}
		}
		else if (m_LastTouchScrollValue != m_ScrollValue)
		{
			float direction = Mathf.Sign(speed);
			speed -= direction * m_KineticScrollFriction * Time.fixedDeltaTime;
			m_LastTouchScrollValue = m_ScrollValue;
			if (Mathf.Abs(speed) >= m_MinKineticScrollSpeed && Mathf.Sign(speed) == direction)
			{
				ScrollTo(m_ScrollValue + speed * Time.fixedDeltaTime, null, blockInputWhileScrolling: false, tween: false, null, null, clamp: false);
			}
		}
	}

	private void DragThumb()
	{
		Vector3 planeOrigin = m_ScrollTrack.bounds.min;
		if (m_scrollTrackCamera == null)
		{
			m_scrollTrackCamera = CameraUtils.FindProjectionCameraForObject(m_ScrollTrack.gameObject);
			m_scrollTrackCameraOverridePass = GetCameraPassInParentHierarchy();
		}
		Plane trackPlane = new Plane(-m_scrollTrackCamera.transform.forward, planeOrigin);
		Ray mouseRay = CameraUtils.ScreenPointToRayWithCameraPass(m_scrollTrackCamera, InputCollection.GetMousePosition(), m_scrollTrackCameraOverridePass);
		if (trackPlane.Raycast(mouseRay, out var dist))
		{
			Vector3 worldIntersectPoint = mouseRay.GetPoint(dist);
			float start = GetScrollTrackTop1D();
			float end = GetScrollTrackBtm1D();
			float scrollValue = Mathf.Clamp01((worldIntersectPoint[(int)m_ScrollPlane] - start) / (end - start));
			if (Mathf.Abs(m_ScrollValue - scrollValue) > Mathf.Epsilon)
			{
				m_ScrollValue = scrollValue;
				UpdateThumbPosition();
				UpdateScrollObjectPosition(tween: false, null, null, null);
			}
		}
		ResetTouchDrag();
	}

	private CameraOverridePass GetCameraPassInParentHierarchy()
	{
		Transform transfromToCheck = base.transform;
		while (transfromToCheck != null)
		{
			PopupRoot root = transfromToCheck.GetComponent<PopupRoot>();
			if (root != null && root.PrimaryRenderPass != null)
			{
				return root.PrimaryRenderPass;
			}
			transfromToCheck = transfromToCheck.parent;
		}
		return null;
	}

	private void ResetTouchDrag()
	{
		bool hasValue = m_TouchDragBeginWorldPos.HasValue;
		m_TouchBeginScreenPos = null;
		m_TouchDragBeginWorldPos = null;
		m_TouchDragBeginScrollValue = m_ScrollValue;
		m_LastTouchScrollValue = m_ScrollValue;
		if (hasValue)
		{
			FireTouchEndEvent();
		}
	}

	private float GetScrollTrackTop1D()
	{
		return GetScrollTrackTop()[(int)m_ScrollPlane];
	}

	private float GetScrollTrackBtm1D()
	{
		return GetScrollTrackBtm()[(int)m_ScrollPlane];
	}

	private Vector3 GetScrollTrackTop()
	{
		if (m_ScrollTrack == null)
		{
			return Vector3.zero;
		}
		if (m_ScrollPlane == ScrollDirection.X)
		{
			return m_ScrollTrack.bounds.min;
		}
		return m_ScrollTrack.bounds.max;
	}

	private Vector3 GetScrollTrackBtm()
	{
		if (m_ScrollTrack == null)
		{
			return Vector3.zero;
		}
		if (m_ScrollPlane == ScrollDirection.X)
		{
			return m_ScrollTrack.bounds.max;
		}
		return m_ScrollTrack.bounds.min;
	}

	private void AddScroll(float amount)
	{
		ScrollTo(m_ScrollValue + amount, null, blockInputWhileScrolling: false, tween: true, null, null, clamp: true);
		ResetTouchDrag();
	}

	private void ScrollTo(float percentage, OnScrollComplete scrollComplete, bool blockInputWhileScrolling, bool tween, iTween.EaseType? tweenType, float? tweenTime, bool clamp)
	{
		m_ScrollValue = (clamp ? Mathf.Clamp01(percentage) : percentage);
		UpdateThumbPosition();
		UpdateScrollObjectPosition(tween, scrollComplete, tweenType, tweenTime, blockInputWhileScrolling);
		m_prevScrollValue = percentage;
	}

	private void UpdateThumbPosition()
	{
		if (!(m_ScrollThumb == null))
		{
			Vector3 start = GetScrollTrackTop();
			Vector3 end = GetScrollTrackBtm();
			float start1D = start[(int)m_ScrollPlane];
			float end1D = end[(int)m_ScrollPlane];
			Vector3 pos = start + (end - start) * 0.5f;
			pos[(int)m_ScrollPlane] = start1D + (end1D - start1D) * Mathf.Clamp01(m_ScrollValue);
			m_ScrollThumb.transform.position = pos;
			if (m_ScrollPlane == ScrollDirection.Z)
			{
				Vector3 currentLocalThumbPosition = m_ScrollThumb.transform.localPosition;
				m_ScrollThumb.transform.localPosition = new Vector3(currentLocalThumbPosition.x, m_ScrollThumbStartYPos, currentLocalThumbPosition.z);
			}
		}
	}

	private void UpdateScrollObjectPosition(bool tween, OnScrollComplete scrollComplete, iTween.EaseType? tweenType, float? tweenTime, bool blockInputWhileScrolling = false)
	{
		if (m_ScrollObject == null)
		{
			return;
		}
		Vector3 startpos = m_ScrollAreaStartPos;
		Vector3 endpos = startpos;
		Vector3 scrollVec = GetTotalScrollHeightVector(convertToLocalSpace: true);
		endpos += scrollVec * (m_ScrollDirectionReverse ? (-1f) : 1f);
		Vector3 position = startpos + m_ScrollValue * (endpos - startpos);
		if (float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z))
		{
			return;
		}
		if (tween)
		{
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("position", position);
			args.Add("time", tweenTime.HasValue ? tweenTime.Value : m_ScrollTweenTime);
			args.Add("islocal", true);
			args.Add("easetype", tweenType.HasValue ? tweenType.Value : m_ScrollEaseType);
			args.Add("onupdate", (Action<object>)delegate
			{
				UpdateAndFireVisibleAffectedObjects();
			});
			args.Add("oncomplete", (Action<object>)delegate
			{
				UpdateAndFireVisibleAffectedObjects();
				if (scrollComplete != null)
				{
					scrollComplete(m_ScrollValue);
				}
			});
			iTween.MoveTo(m_ScrollObject, args);
		}
		else
		{
			if (m_ScrollPlane == ScrollDirection.Z)
			{
				m_ScrollObject.transform.localPosition = new Vector3(position.x, m_ScrollObject.transform.localPosition.y, position.z);
			}
			else
			{
				m_ScrollObject.transform.localPosition = position;
			}
			UpdateAndFireVisibleAffectedObjects();
			if (scrollComplete != null)
			{
				scrollComplete(m_ScrollValue);
			}
		}
	}

	private IEnumerator SetScrollWait(float percentage, OnScrollComplete scrollComplete, bool blockInputWhileScrolling, bool tween, iTween.EaseType? tweenType, float? tweenTime, bool clamp)
	{
		yield return null;
		ScrollTo(percentage, scrollComplete, blockInputWhileScrolling, tween, tweenType, tweenTime, clamp);
		ResetTouchDrag();
	}

	private IEnumerator BlockInput(float blockTime)
	{
		m_InputBlocked = true;
		yield return new WaitForSeconds(blockTime);
		m_InputBlocked = false;
	}

	private Vector3 GetTotalScrollHeightVector(bool convertToLocalSpace = false)
	{
		if (m_ScrollObject == null)
		{
			return Vector3.zero;
		}
		float scrollHeight = m_PolledScrollHeight - GetScrollBoundsHeight();
		if (scrollHeight < 0f)
		{
			return Vector3.zero;
		}
		Vector3 heightVec = Vector3.zero;
		heightVec[(int)m_ScrollPlane] = scrollHeight;
		if (convertToLocalSpace)
		{
			heightVec = m_ScrollObject.transform.parent.worldToLocalMatrix * heightVec;
		}
		if (m_ScrollBottomPadding > 0f)
		{
			heightVec += heightVec.normalized * m_ScrollBottomPadding;
		}
		return heightVec;
	}

	private float GetTotalWorldScrollHeight()
	{
		return GetTotalScrollHeightVector().magnitude;
	}

	private Vector3? GetWorldTouchPosition()
	{
		return GetWorldTouchPosition(m_ScrollBounds);
	}

	private Vector3? GetWorldTouchPositionOnDragArea()
	{
		Vector3? touchPos = null;
		if (m_TouchDragFullArea != null)
		{
			touchPos = GetWorldTouchPosition(m_TouchDragFullArea);
		}
		if (!touchPos.HasValue && m_ScrollBounds != null)
		{
			touchPos = GetWorldTouchPosition(m_ScrollBounds);
		}
		return touchPos;
	}

	private Vector3? GetWorldTouchPosition(BoxCollider bounds)
	{
		Camera camera = GetScrollCamera();
		if (camera == null)
		{
			return null;
		}
		Ray touchRay = camera.ScreenPointToRay(InputCollection.GetMousePosition());
		RaycastHit hitInfo;
		foreach (BoxCollider touchScrollBlocker in m_TouchScrollBlockers)
		{
			if (touchScrollBlocker.Raycast(touchRay, out hitInfo, float.MaxValue))
			{
				return null;
			}
		}
		if (IsInputOverScrollableArea(bounds, out hitInfo))
		{
			return touchRay.GetPoint(hitInfo.distance);
		}
		return null;
	}

	private float GetScrollValueDelta(float worldDelta)
	{
		return m_ScrollDeltaMultiplier * worldDelta / GetTotalWorldScrollHeight();
	}

	private float GetOutOfBoundsDist(float scrollValue)
	{
		if (scrollValue < 0f)
		{
			return scrollValue;
		}
		if (scrollValue > 1f)
		{
			return scrollValue - 1f;
		}
		return 0f;
	}

	private void FireEnableScrollEvent()
	{
		EnableScroll[] array = m_EnableScrollListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](m_Enabled);
		}
	}

	public void UpdateAndFireVisibleAffectedObjects()
	{
		foreach (VisibleAffectedObject item in m_VisibleAffectedObjects)
		{
			bool visible = IsObjectVisibleInScrollArea(item.Obj, item.Extents) || m_ForceShowVisibleAffectedObjects;
			if (visible != item.Visible)
			{
				item.Visible = visible;
				item.Callback(item.Obj, visible);
			}
		}
		foreach (FastVisibleAffectedObject item2 in m_fastVisibleAffectedObjects)
		{
			if (!(item2.TopObj == null))
			{
				FireFastVisibleCallback(item2);
			}
		}
	}

	private void FireFastVisibleCallback(FastVisibleAffectedObject item)
	{
		GetFastVisibleRange(item, out var startIndex, out var endIndex, out var isVisible);
		if (item.Callback != null)
		{
			item.Callback(startIndex, endIndex, isVisible);
		}
	}

	public void UpdateVisibilityOnObject(GameObject topObj)
	{
		if (topObj == null)
		{
			return;
		}
		foreach (FastVisibleAffectedObject item in m_fastVisibleAffectedObjects)
		{
			if (item.TopObj == topObj)
			{
				FireFastVisibleCallback(item);
			}
		}
	}

	private float GetScrollBoundsHeight()
	{
		if (m_ScrollBounds == null)
		{
			return 0f;
		}
		return m_ScrollBounds.bounds.size[(int)m_ScrollPlane];
	}

	private void FireTouchStartEvent()
	{
		OnTouchScrollStarted[] arr = m_TouchScrollStartedListeners.ToArray();
		for (int i = 0; i < arr.Length; i++)
		{
			arr[i]();
		}
	}

	private void FireTouchEndEvent()
	{
		OnTouchScrollEnded[] arr = m_TouchScrollEndedListeners.ToArray();
		for (int i = 0; i < arr.Length; i++)
		{
			arr[i]();
		}
	}

	private float GetScrollableItemsHeight()
	{
		Vector3 currMin = Vector3.zero;
		Vector3 currMax = Vector3.zero;
		if (GetScrollableItemsMinMax(ref currMin, ref currMax).Count == 0)
		{
			return 0f;
		}
		int scrollDir = (int)m_ScrollPlane;
		return currMax[scrollDir] - currMin[scrollDir];
	}

	private List<UIBScrollableItem> GetScrollableItemsMinMax(ref Vector3 min, ref Vector3 max)
	{
		if (m_ScrollObject == null)
		{
			return m_scrollableItems;
		}
		int itemsCount = m_scrollableItems.Count;
		if (itemsCount == 0)
		{
			return m_scrollableItems;
		}
		min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		for (int i = 0; i < itemsCount; i++)
		{
			UIBScrollableItem scrollable = m_scrollableItems[i];
			if (!(scrollable == null) && scrollable.IsActive())
			{
				scrollable.GetWorldBounds(out var currMin, out var currMax);
				min.x = Math.Min(min.x, Math.Min(currMin.x, currMax.x));
				min.y = Math.Min(min.y, Math.Min(currMin.y, currMax.y));
				min.z = Math.Min(min.z, Math.Min(currMin.z, currMax.z));
				max.x = Math.Max(max.x, Math.Max(currMin.x, currMax.x));
				max.y = Math.Max(max.y, Math.Max(currMin.y, currMax.y));
				max.z = Math.Max(max.z, Math.Max(currMin.z, currMax.z));
			}
		}
		return m_scrollableItems;
	}

	private BoxCollider[] GetBoxCollidersMinMax(ref Vector3 min, ref Vector3 max)
	{
		return null;
	}

	private Camera GetScrollCamera()
	{
		if (m_UseCameraFromLayer)
		{
			return CameraUtils.FindFirstByLayer(base.gameObject.layer);
		}
		Box box = Box.Get();
		if (box == null)
		{
			return null;
		}
		return box.GetCamera();
	}

	private void SaveScrollThumbStartHeight()
	{
		if (m_ScrollThumb != null)
		{
			m_ScrollThumbStartYPos = m_ScrollThumb.transform.localPosition.y;
		}
	}
}
