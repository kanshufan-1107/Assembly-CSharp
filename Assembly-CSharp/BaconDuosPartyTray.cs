using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class BaconDuosPartyTray : MonoBehaviour
{
	public BaconParty m_baconParty;

	[SerializeField]
	private AsyncReference[] m_dropTargetWidgetReferences;

	[SerializeField]
	private AsyncReference[] m_dragWidgetReferences;

	private Widget[] m_dropTargetWidgets;

	private Widget[] m_dragWidgets;

	private BattlegroundsDuosTeamBuilderPlayerDataModel m_draggedDataModel;

	private static int INVALID_HOVER_INDEX = -1;

	private int m_hoveredIndex = INVALID_HOVER_INDEX;

	private readonly Vector3 INACTIVE_DROP_TARGET_COLLIDER = new Vector3(0f, -5f, 0f);

	private readonly Vector3 ACTIVE_DROP_TARGET_COLLIDER = new Vector3(0f, 5f, 0f);

	public static readonly string HOVERED_STATE = "HOVERED";

	public static readonly string NOT_HOVERED_STATE = "NOT_HOVERED";

	public static readonly string COMPLETION_CONFIRMATION_STATE = "GLOW_COMPLETE";

	public static readonly string ALLOWED_DROP_STATE = "ALLOWED";

	private void Start()
	{
		m_dropTargetWidgets = new Widget[m_dropTargetWidgetReferences.Length];
		m_dragWidgets = new Widget[m_dragWidgetReferences.Length];
		for (int i = 0; i < m_dropTargetWidgetReferences.Length; i++)
		{
			int index = i;
			m_dropTargetWidgets[i] = null;
			m_dropTargetWidgetReferences[i].RegisterReadyListener(delegate(Widget w)
			{
				OnDropTargetWidgetReady(w, index);
			});
		}
		for (int j = 0; j < m_dragWidgetReferences.Length; j++)
		{
			int index2 = j;
			m_dragWidgets[j] = null;
			m_dragWidgetReferences[j].RegisterReadyListener(delegate(Widget w)
			{
				OnDragWidgetReady(w, index2);
			});
		}
		UniversalInputManager.Get().RegisterMouseOnOrOffScreenListener(OnMouseOnOrOffScreen);
	}

	private void Update()
	{
		if (IsDragging())
		{
			if (!InputUtil.IsMouseOnScreen())
			{
				OnDragRelease();
				return;
			}
			if (!UniversalInputManager.Get().GetInputHitInfo(Box.Get().GetCamera(), GameLayer.DragPlane.LayerBit(), out var hit) || !InputCollection.GetMouseButton(0))
			{
				OnDragRelease();
				return;
			}
			Vector3 newPos = hit.point;
			m_baconParty.m_partyPlanner.UpdateDragWidgetPosition(newPos);
		}
	}

	private void OnDropTargetWidgetReady(Widget widget, int index)
	{
		m_dropTargetWidgets[index] = widget;
		widget.RegisterEventListener(DragDisplayEventListener);
	}

	private void OnDragWidgetReady(Widget widget, int index)
	{
		m_dragWidgets[index] = widget;
		widget.RegisterEventListener(DragDisplayEventListener);
	}

	public void OnDestroy()
	{
		Unload();
	}

	public void ClearPartyData()
	{
		Widget[] dropTargetWidgets = m_dropTargetWidgets;
		for (int i = 0; i < dropTargetWidgets.Length; i++)
		{
			dropTargetWidgets[i].UnbindDataModel(937);
		}
	}

	public void Unload()
	{
		if (m_dropTargetWidgets != null)
		{
			Widget[] dropTargetWidgets = m_dropTargetWidgets;
			foreach (Widget widget in dropTargetWidgets)
			{
				if (widget != null)
				{
					widget.RemoveEventListener(DragDisplayEventListener);
				}
			}
		}
		if (m_dragWidgets != null)
		{
			Widget[] dropTargetWidgets = m_dragWidgets;
			foreach (Widget widget2 in dropTargetWidgets)
			{
				if (widget2 != null)
				{
					widget2.RemoveEventListener(DragDisplayEventListener);
				}
			}
		}
		if (UniversalInputManager.Get() != null)
		{
			UniversalInputManager.Get().UnregisterMouseOnOrOffScreenListener(OnMouseOnOrOffScreen);
		}
	}

	private void DragDisplayEventListener(string eventName)
	{
		switch (eventName)
		{
		case "PLAYER_drag_started":
			OnDragStart();
			break;
		case "PLAYER_drag_released":
			OnDragRelease();
			break;
		case "PLAYER_mouse_over":
			OnMouseOver();
			break;
		case "PLAYER_mouse_out":
			OnMouseOut();
			break;
		}
	}

	public bool IsDragging()
	{
		return m_draggedDataModel != null;
	}

	private void OnDragRelease()
	{
		for (int i = 0; i < m_dropTargetWidgets.Length; i++)
		{
			m_dropTargetWidgets[i].transform.Find("BaconPartyMember").GetComponent<VisualController>().SetState(NOT_HOVERED_STATE);
		}
		if (m_hoveredIndex != INVALID_HOVER_INDEX)
		{
			m_baconParty.MovePlayerToSlot(m_draggedDataModel, m_dropTargetWidgets[m_hoveredIndex]);
		}
		m_draggedDataModel = null;
		m_hoveredIndex = INVALID_HOVER_INDEX;
		m_baconParty.m_partyPlanner.HideDragWidget();
		ToggleDropTargetColliders(enabled: false);
	}

	private void ToggleDropTargetColliders(bool enabled)
	{
		Widget[] dropTargetWidgets = m_dropTargetWidgets;
		for (int i = 0; i < dropTargetWidgets.Length; i++)
		{
			dropTargetWidgets[i].transform.Find("BaconPartyMember").GetComponent<BoxCollider>().center = (enabled ? ACTIVE_DROP_TARGET_COLLIDER : INACTIVE_DROP_TARGET_COLLIDER);
		}
	}

	private void OnDragStart()
	{
		if (PegUI.Get().GetMousedOverElement() == null)
		{
			return;
		}
		EventDataModel eventDataModel = PegUI.Get().GetMousedOverElement().GetComponentInParent<Widget>()
			.GetDataModel<EventDataModel>();
		if (eventDataModel == null)
		{
			Log.CollectionManager.PrintError("No event data model attached to to moused over element");
			return;
		}
		m_draggedDataModel = (BattlegroundsDuosTeamBuilderPlayerDataModel)eventDataModel.Payload;
		if (m_draggedDataModel == null)
		{
			return;
		}
		for (int i = 0; i < m_dropTargetWidgets.Length; i++)
		{
			BattlegroundsDuosTeamBuilderPlayerDataModel destinationDataModel = m_dropTargetWidgets[i].GetDataModel<BattlegroundsDuosTeamBuilderPlayerDataModel>();
			if (m_baconParty.IsValidDropTarget(i + 1) && (destinationDataModel?.GameAccountId.High != m_draggedDataModel.GameAccountId.High || destinationDataModel?.GameAccountId.Low != m_draggedDataModel.GameAccountId.Low))
			{
				m_dropTargetWidgets[i].transform.Find("BaconPartyMember").GetComponent<VisualController>().SetState(ALLOWED_DROP_STATE);
			}
		}
		m_baconParty.m_partyPlanner.ShowDragWidget(m_draggedDataModel);
		ToggleDropTargetColliders(enabled: true);
	}

	private void OnMouseOver()
	{
		Widget hoveredWidget = PegUI.Get().GetMousedOverElement().GetComponentInParent<Widget>();
		if (hoveredWidget == null)
		{
			return;
		}
		BattlegroundsDuosTeamBuilderPlayerDataModel destinationDataModel = null;
		for (int i = 0; i < m_dropTargetWidgets.Length; i++)
		{
			if (m_dropTargetWidgets[i].transform == hoveredWidget.transform.parent && m_baconParty.IsValidDropTarget(i + 1))
			{
				m_hoveredIndex = i;
				destinationDataModel = m_dropTargetWidgets[i].GetDataModel<BattlegroundsDuosTeamBuilderPlayerDataModel>();
			}
		}
		if (m_draggedDataModel != null && m_hoveredIndex != INVALID_HOVER_INDEX && (destinationDataModel == null || destinationDataModel?.GameAccountId.High != m_draggedDataModel.GameAccountId.High || destinationDataModel?.GameAccountId.Low != m_draggedDataModel.GameAccountId.Low))
		{
			hoveredWidget.GetComponent<VisualController>().SetState(HOVERED_STATE);
		}
	}

	private void OnMouseOut()
	{
		if (m_hoveredIndex != INVALID_HOVER_INDEX)
		{
			BattlegroundsDuosTeamBuilderPlayerDataModel destinationDataModel = m_dropTargetWidgets[m_hoveredIndex].GetDataModel<BattlegroundsDuosTeamBuilderPlayerDataModel>();
			GameObject hoveredWidget = m_dropTargetWidgets[m_hoveredIndex].transform.Find("BaconPartyMember").gameObject;
			if (m_draggedDataModel != null && m_baconParty.IsValidDropTarget(m_hoveredIndex + 1) && (destinationDataModel == null || destinationDataModel?.GameAccountId.High != m_draggedDataModel.GameAccountId.High || destinationDataModel?.GameAccountId.Low != m_draggedDataModel.GameAccountId.Low))
			{
				hoveredWidget.GetComponent<VisualController>().SetState(ALLOWED_DROP_STATE);
			}
			else
			{
				hoveredWidget.GetComponent<VisualController>().SetState(NOT_HOVERED_STATE);
			}
		}
		m_hoveredIndex = INVALID_HOVER_INDEX;
	}

	private void OnMouseOnOrOffScreen(bool onScreen)
	{
		if (m_draggedDataModel != null && !onScreen)
		{
			OnDragRelease();
		}
	}
}
