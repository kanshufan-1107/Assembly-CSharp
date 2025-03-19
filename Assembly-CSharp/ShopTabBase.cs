using System;
using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

public abstract class ShopTabBase : MonoBehaviour
{
	[Header("Base")]
	[SerializeField]
	private Widget m_widget;

	[Header("Tooltip")]
	[SerializeField]
	private TooltipZone m_toolTip;

	[SerializeField]
	private Clickable m_toolTipClickable;

	[SerializeField]
	[Min(1f)]
	private float m_toolTipScale;

	private List<Action> m_onClickListeners = new List<Action>();

	private bool m_isShown;

	private bool m_isSelected;

	private HashSet<string> m_blockIds = new HashSet<string>();

	private const string SHOP_TAB_CLICKED_EVENT = "SHOP_TAB_CLICKED";

	private const string SHOW_TAB = "SHOW_TAB";

	private const string HIDE_TAB = "HIDE_TAB";

	private const string SELECT_TAB = "SELECTED";

	private const string DESELECT_TAB = "DESELECTED";

	[Overridable]
	public string BlockTab
	{
		set
		{
			Block(block: true, value);
		}
	}

	[Overridable]
	public string UnblockTab
	{
		set
		{
			Block(block: false, value);
		}
	}

	public Widget Widget => m_widget;

	public ShopTabDataModel CurrentData { get; private set; }

	protected bool m_isBlocked { get; private set; }

	private void Awake()
	{
		if (m_widget != null)
		{
			m_widget.RegisterEventListener(HandleEvent);
		}
		if (m_toolTipClickable != null)
		{
			m_toolTipClickable.AddEventListener(UIEventType.ROLLOVER, delegate
			{
				OnTooltipHover();
			});
			m_toolTipClickable.AddEventListener(UIEventType.ROLLOUT, delegate
			{
				OnTooltipRollout();
			});
		}
	}

	protected virtual void HandleEvent(string eventName)
	{
		if (eventName == "SHOP_TAB_CLICKED")
		{
			TriggerTabClick();
		}
	}

	public virtual void Show(ShopTabDataModel dataModel)
	{
		m_isShown = true;
		CurrentData = dataModel;
		Widget?.BindDataModel(dataModel);
		Widget?.TriggerEvent("SHOW_TAB");
	}

	public virtual void Hide()
	{
		m_isShown = false;
		if (m_isSelected)
		{
			Deselect();
		}
		Widget?.UnbindDataModel(842);
		Widget?.TriggerEvent("HIDE_TAB");
	}

	public virtual void Select()
	{
		if (m_isShown)
		{
			m_isSelected = true;
			Widget?.TriggerEvent("SELECTED");
		}
	}

	public virtual void Deselect()
	{
		if (m_isShown)
		{
			m_isSelected = false;
			Widget?.TriggerEvent("DESELECTED");
		}
	}

	public void RegisterTabClickListener(Action e)
	{
		if (e != null && !m_onClickListeners.Contains(e))
		{
			m_onClickListeners.Add(e);
		}
	}

	public void UnregisterTabClickListener(Action e)
	{
		if (e != null)
		{
			m_onClickListeners.Remove(e);
		}
	}

	protected void TriggerTabClick()
	{
		foreach (Action onClickListener in m_onClickListeners)
		{
			onClickListener?.Invoke();
		}
	}

	public void Block(bool block, string blockId)
	{
		if (block)
		{
			if (m_blockIds.Add(blockId) && m_blockIds.Count == 1)
			{
				OnBlockChanged(blocked: true);
			}
		}
		else if (m_blockIds.Remove(blockId) && m_blockIds.Count == 0)
		{
			OnBlockChanged(blocked: false);
		}
	}

	protected virtual void OnBlockChanged(bool blocked)
	{
		m_isBlocked = blocked;
	}

	protected abstract void GetTooltipData(out string title, out string body);

	private void OnTooltipHover()
	{
		GetTooltipData(out var title, out var body);
		if (!(m_toolTip == null) && !string.IsNullOrEmpty(body))
		{
			m_toolTip.LayerOverride = GameLayer.BattleNet;
			m_toolTip.ShowTooltip(title, body, m_toolTipScale);
		}
	}

	private void OnTooltipRollout()
	{
		if (m_toolTip != null)
		{
			m_toolTip.HideTooltip();
		}
	}
}
