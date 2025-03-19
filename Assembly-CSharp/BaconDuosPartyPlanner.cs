using Blizzard.GameService.SDK.Client.Integration;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class BaconDuosPartyPlanner : MonoBehaviour
{
	[SerializeField]
	private AsyncReference[] m_widgetReferences;

	[SerializeField]
	private AsyncReference m_dragWidgetReference;

	private Widget[] m_widgets;

	private Widget m_dragWidget;

	private BattlegroundsDuosTeamBuilderPlayerDataModel[] m_dataModels;

	private readonly string STATE_OPEN = "OPEN";

	private readonly string STATE_CLOSED = "CLOSED";

	private readonly string STATE_HIDDEN = "HIDDEN";

	private readonly string STATE_DRAGGING = "DRAGGING";

	private readonly string STATE_NOT_DRAGGING = "NOT_DRAGGING";

	private void Start()
	{
		m_widgets = new Widget[m_widgetReferences.Length];
		m_dataModels = new BattlegroundsDuosTeamBuilderPlayerDataModel[m_widgetReferences.Length];
		for (int i = 0; i < m_widgetReferences.Length; i++)
		{
			int index = i;
			m_widgets[i] = null;
			m_dataModels[i] = null;
			m_widgetReferences[i].RegisterReadyListener(delegate(Widget w)
			{
				OnWidgetReady(w, index);
			});
		}
		m_dragWidgetReference.RegisterReadyListener<Widget>(OnDragWidgetReady);
	}

	public void ShowDragWidget(BattlegroundsDuosTeamBuilderPlayerDataModel dataModel)
	{
		if (!(m_dragWidget == null))
		{
			m_dragWidget.BindDataModel(dataModel);
			m_dragWidget.transform.Find("BaconDuosPartyBuilderMember/Root/DragFX").GetComponent<VisualController>().SetState(STATE_DRAGGING);
		}
	}

	public void HideDragWidget()
	{
		if (!(m_dragWidget == null))
		{
			m_dragWidget.UnbindDataModel(937);
			m_dragWidget.transform.Find("BaconDuosPartyBuilderMember/Root/DragFX").GetComponent<VisualController>().SetState(STATE_NOT_DRAGGING);
			m_dragWidget.transform.position = new Vector3(0f, 0f, 1000f);
		}
	}

	public void UpdateDragWidgetPosition(Vector3 position)
	{
		if (!(m_dragWidget == null) && !(m_dragWidget.transform.Find("BaconDuosPartyBuilderMember/Root/DragFX").GetComponent<VisualController>().State != STATE_DRAGGING))
		{
			m_dragWidget.transform.position = position;
		}
	}

	public bool IsOpen()
	{
		return GetComponent<VisualController>().State == STATE_OPEN;
	}

	public void ToggleOpenState()
	{
		ToggleOpenState(GetComponent<VisualController>().State != STATE_OPEN);
	}

	public void ToggleOpenState(bool isOpen)
	{
		if (!BaconLobbyMgr.Get().IsInDuosMode() || !PartyManager.Get().IsInBattlegroundsParty() || !PartyManager.Get().IsPartyLeader())
		{
			GetComponent<VisualController>().SetState(STATE_HIDDEN);
		}
		else
		{
			GetComponent<VisualController>().SetState(isOpen ? STATE_OPEN : STATE_CLOSED);
		}
	}

	private void OnDragWidgetReady(Widget widget)
	{
		m_dragWidget = widget;
		m_dragWidget.transform.Find("BaconDuosPartyBuilderMember/Root/DragFX").GetComponent<VisualController>().SetState("NOT_DRAGGING");
		Object.Destroy(m_dragWidget.transform.Find("BaconDuosPartyBuilderMember").GetComponent<Clickable>());
		Object.Destroy(m_dragWidget.transform.Find("BaconDuosPartyBuilderMember").GetComponent<BoxCollider>());
	}

	private void OnWidgetReady(Widget widget, int index)
	{
		m_widgets[index] = widget;
		if (m_dataModels[index] != null)
		{
			m_widgets[index].BindDataModel(m_dataModels[index]);
		}
	}

	private int GetNextAvailableIndex()
	{
		for (int i = 0; i < m_dataModels.Length; i++)
		{
			if (m_dataModels[i] == null)
			{
				return i;
			}
		}
		return -1;
	}

	private int GetIndexByGameAccountId(BnetGameAccountId gameAccountId)
	{
		for (int i = 0; i < m_dataModels.Length; i++)
		{
			if (m_dataModels[i] != null && new BnetGameAccountId(m_dataModels[i].GameAccountId.High, m_dataModels[i].GameAccountId.Low) == gameAccountId)
			{
				return i;
			}
		}
		return -1;
	}

	private BnetGameAccountId FromDataModel(BattlegroundsDuosTeamBuilderPlayerDataModel dataModel)
	{
		if (dataModel == null || dataModel.GameAccountId == null)
		{
			return new BnetGameAccountId(0uL, 0uL);
		}
		return new BnetGameAccountId(dataModel.GameAccountId.High, dataModel.GameAccountId.Low);
	}

	public bool AddMember(BattlegroundsDuosTeamBuilderPlayerDataModel dataModel)
	{
		if (GetIndexByGameAccountId(FromDataModel(dataModel)) >= 0)
		{
			return false;
		}
		int index = GetNextAvailableIndex();
		if (index < 0)
		{
			return false;
		}
		m_dataModels[index] = dataModel;
		if (m_widgets[index] != null)
		{
			m_widgets[index].BindDataModel(m_dataModels[index]);
		}
		return true;
	}

	public bool RemoveMember(BnetGameAccountId gameAccountId)
	{
		int index = GetIndexByGameAccountId(gameAccountId);
		if (index < 0)
		{
			return false;
		}
		m_dataModels[index] = null;
		if (m_widgets[index] != null)
		{
			m_widgets[index].UnbindDataModel(937);
		}
		return true;
	}

	public bool UpdateMemberRating(BnetGameAccountId gameAccountId, int rating)
	{
		int index = GetIndexByGameAccountId(gameAccountId);
		if (index < 0)
		{
			return false;
		}
		m_dataModels[index].BattlegroundsDuosRating = rating;
		return true;
	}

	public void ClearPartyData()
	{
		for (int i = 0; i < m_dataModels.Length; i++)
		{
			if (m_widgets[i] != null)
			{
				m_widgets[i].UnbindDataModel(937);
			}
			if (m_dataModels[i] != null)
			{
				m_dataModels[i] = null;
			}
		}
	}
}
