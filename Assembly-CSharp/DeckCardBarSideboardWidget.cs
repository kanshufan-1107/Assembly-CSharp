using System;
using Hearthstone.UI;
using UnityEngine;

public class DeckCardBarSideboardWidget : MonoBehaviour
{
	[SerializeField]
	private GameObject m_root;

	[SerializeField]
	private UIBButton m_sideboardButton;

	[SerializeField]
	private Clickable m_clickable;

	[SerializeField]
	private VisualController m_mainVC;

	[SerializeField]
	private VisualController m_premiumVC;

	[SerializeField]
	private VisualController m_buttonVC;

	public event Action SideboardButtonPressed;

	public event Action SideboardButtonOver;

	public event Action SideboardButtonOut;

	private void OnEnable()
	{
		m_sideboardButton.AddEventListener(UIEventType.RELEASE, OnSideboardButtonPress);
		m_sideboardButton.AddEventListener(UIEventType.ROLLOVER, OnSideboardButtonOver);
		m_sideboardButton.AddEventListener(UIEventType.ROLLOUT, OnSideboardButtonOut);
	}

	private void OnDisable()
	{
		m_sideboardButton.RemoveEventListener(UIEventType.RELEASE, OnSideboardButtonPress);
		m_sideboardButton.RemoveEventListener(UIEventType.ROLLOVER, OnSideboardButtonOver);
		m_sideboardButton.RemoveEventListener(UIEventType.ROLLOUT, OnSideboardButtonOut);
	}

	public void Show()
	{
		m_root.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		m_root.gameObject.SetActive(value: false);
	}

	private void OnSideboardButtonPress(UIEvent e)
	{
		if (CollectionManager.Get().GetCollectibleDisplay().GetViewMode() == CollectionUtils.ViewMode.CARDS)
		{
			CollectionDeck editedDeck = CollectionManager.Get().GetEditedDeck();
			if (editedDeck != null && !editedDeck.Locked)
			{
				this.SideboardButtonPressed?.Invoke();
			}
		}
	}

	private void OnSideboardButtonOver(UIEvent e)
	{
		if (CollectionManager.Get().GetCollectibleDisplay().GetViewMode() == CollectionUtils.ViewMode.CARDS)
		{
			HighlightState highlight = m_sideboardButton.GetComponentInChildren<HighlightState>();
			if (highlight != null)
			{
				highlight.ChangeState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
			}
			SoundManager.Get().LoadAndPlay("Small_Mouseover.prefab:692610296028713458ea58bc34adb4c9", base.gameObject);
			this.SideboardButtonOver?.Invoke();
		}
	}

	private void OnSideboardButtonOut(UIEvent e)
	{
		if (CollectionManager.Get().GetCollectibleDisplay().GetViewMode() == CollectionUtils.ViewMode.CARDS)
		{
			HighlightState highlight = m_sideboardButton.GetComponentInChildren<HighlightState>();
			if (highlight != null)
			{
				highlight.ChangeState(ActorStateType.NONE);
			}
			this.SideboardButtonOut?.Invoke();
		}
	}

	public void Initialize(SideboardDeck sideboard)
	{
		bool shouldEnableSideboardButton = !TavernBrawlDisplay.IsTavernBrawlViewing();
		SetButtonEnabled(enable: false);
		if (sideboard == null)
		{
			return;
		}
		foreach (IDataModel sideboardDataModel in sideboard.UIDataModels)
		{
			m_mainVC.BindDataModel(sideboardDataModel);
			m_premiumVC.BindDataModel(sideboardDataModel);
			m_buttonVC.BindDataModel(sideboardDataModel);
		}
		sideboard.InitCardCount();
		sideboard.DataModel.EnableEditButton = shouldEnableSideboardButton;
		sideboard.UpdateInvalidCardsData();
		SetButtonEnabled(shouldEnableSideboardButton);
	}

	public void SetButtonEnabled(bool enable)
	{
		m_sideboardButton.SetEnabled(enable);
		if (m_clickable != null)
		{
			m_clickable.Active = enable;
		}
	}
}
