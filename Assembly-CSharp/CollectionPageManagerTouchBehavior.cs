using System.Collections;
using Blizzard.T5.Services;
using UnityEngine;

public class CollectionPageManagerTouchBehavior : PegUICustomBehavior
{
	private enum SwipeState
	{
		None,
		Update,
		Success
	}

	private float TurnDist = 0.07f;

	private PegUIElement m_pageLeftRegion;

	private PegUIElement m_pageRightRegion;

	private PegUIElement m_pageDragRegion;

	private SwipeState m_swipeState;

	private Vector2 m_swipeStartPosition;

	protected override void Awake()
	{
		base.Awake();
		BookPageManager collectionPageManager = GetComponent<BookPageManager>();
		m_pageLeftRegion = collectionPageManager.m_pageLeftClickableRegion;
		m_pageRightRegion = collectionPageManager.m_pageRightClickableRegion;
		m_pageDragRegion = collectionPageManager.m_pageDraggableRegion;
		m_pageDragRegion.gameObject.SetActive(value: true);
		m_pageDragRegion.AddEventListener(UIEventType.PRESS, OnPageDraggableRegionDown);
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null)
		{
			cmd.OnViewModeChanged += OnViewModeChanged;
		}
	}

	protected override void OnDestroy()
	{
		m_pageDragRegion.gameObject.SetActive(value: false);
		m_pageDragRegion.RemoveEventListener(UIEventType.PRESS, OnPageDraggableRegionDown);
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null)
		{
			cmd.OnViewModeChanged -= OnViewModeChanged;
		}
		base.OnDestroy();
	}

	public override bool UpdateUI()
	{
		if ((CollectionInputMgr.Get() != null && CollectionInputMgr.Get().HasHeldCard()) || (CraftingManager.Get() != null && CraftingManager.Get().IsCardShowing()))
		{
			return false;
		}
		bool captureInput = false;
		if (InputCollection.GetMouseButtonUp(0))
		{
			captureInput = m_swipeState == SwipeState.Success;
			m_swipeState = SwipeState.None;
		}
		if (m_swipeState != 0)
		{
			return true;
		}
		return captureInput;
	}

	protected void OnViewModeChanged(CollectionUtils.ViewMode prevMode, CollectionUtils.ViewMode mode, CollectionUtils.ViewModeData userdata, bool triggerResponse)
	{
		bool canDragPage = mode != CollectionUtils.ViewMode.HERO_PICKER && mode != CollectionUtils.ViewMode.DECK_TEMPLATE && mode != CollectionUtils.ViewMode.MASS_DISENCHANT;
		m_pageDragRegion.gameObject.SetActive(canDragPage);
	}

	private void OnPageDraggableRegionDown(UIEvent e)
	{
		if (!(base.gameObject == null))
		{
			TryStartPageTurnGesture();
		}
	}

	private void TryStartPageTurnGesture()
	{
		if (m_swipeState != SwipeState.Update)
		{
			StartCoroutine(HandlePageTurnGesture());
		}
	}

	private Vector2 GetTouchPosition()
	{
		Vector3 vec3 = ServiceManager.Get<ITouchScreenService>().GetTouchPosition();
		return new Vector2(vec3.x, vec3.y);
	}

	private IEnumerator HandlePageTurnGesture()
	{
		if (!UniversalInputManager.Get().IsTouchMode())
		{
			yield return null;
		}
		m_swipeStartPosition = GetTouchPosition();
		m_swipeState = SwipeState.Update;
		float pixelTurnDist = Mathf.Clamp(TurnDist * (float)Screen.currentResolution.width, 2f, 300f);
		PegUIElement touchDownPageTurnRegion = HitTestPageTurnRegions();
		while (!InputCollection.GetMouseButtonUp(0))
		{
			float pixelDist = (GetTouchPosition() - m_swipeStartPosition).x;
			if (pixelDist <= 0f - pixelTurnDist && m_pageRightRegion.enabled)
			{
				m_pageRightRegion.TriggerRelease();
				m_swipeState = SwipeState.Success;
				yield break;
			}
			if (pixelDist >= pixelTurnDist && m_pageLeftRegion.enabled)
			{
				m_pageLeftRegion.TriggerRelease();
				m_swipeState = SwipeState.Success;
				yield break;
			}
			yield return null;
		}
		if (touchDownPageTurnRegion != null && touchDownPageTurnRegion == HitTestPageTurnRegions())
		{
			touchDownPageTurnRegion.TriggerRelease();
		}
		m_swipeState = SwipeState.None;
	}

	private PegUIElement HitTestPageTurnRegions()
	{
		PegUIElement element = null;
		Collider component = m_pageDragRegion.GetComponent<Collider>();
		component.enabled = false;
		if (UniversalInputManager.Get().GetInputHitInfo(out var hitInfo))
		{
			element = hitInfo.collider.GetComponent<PegUIElement>();
			if (element != m_pageLeftRegion && element != m_pageRightRegion)
			{
				element = null;
			}
		}
		component.enabled = true;
		return element;
	}
}
