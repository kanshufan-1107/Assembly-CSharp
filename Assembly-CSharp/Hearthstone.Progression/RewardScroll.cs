using System;
using Blizzard.T5.Services;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class RewardScroll : MonoBehaviour
{
	private const string HIDE = "CODE_HIDE";

	private const string SHOW_REWARD = "CODE_SHOW_REWARD";

	private Widget m_widget;

	private GameObject m_owner;

	private bool m_isSignatureCard;

	private event Action OnRewardScrollHidden;

	private event Action OnRewardScrollShown;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(delegate(string eventName)
		{
			if (eventName == "CODE_HIDE")
			{
				if (m_isSignatureCard)
				{
					m_widget.TriggerEvent("CODE_HIDE_SIGNATURE_TOOLTIP", new TriggerEventParameters(ToString(), null, noDownwardPropagation: false, ignorePlaymaker: true));
				}
				Hide();
			}
		});
		m_widget.RegisterDoneChangingStatesListener(delegate
		{
			if (m_widget.GetDataModel(257, out var model))
			{
				RewardListDataModel rewardList = (model as RewardScrollDataModel).RewardList;
				if (rewardList != null && !rewardList.ChooseOne && rewardList.Items.Count == 1 && rewardList.Items[0].ItemType == RewardItemType.CARD && rewardList.Items[0].Card.Premium == TAG_PREMIUM.SIGNATURE)
				{
					m_widget.TriggerEvent("CODE_SHOW_SIGNATURE_TOOLTIP", new TriggerEventParameters(ToString(), null, noDownwardPropagation: false, ignorePlaymaker: true));
					m_isSignatureCard = true;
				}
			}
		});
		m_owner = base.gameObject;
		if (base.transform.parent != null && base.transform.parent.GetComponent<WidgetInstance>() != null)
		{
			m_owner = base.transform.parent.gameObject;
		}
		FatalErrorMgr.Get().AddErrorListener(OnFatalError);
	}

	private void OnDestroy()
	{
		if (FatalErrorMgr.IsInitialized())
		{
			FatalErrorMgr.Get().RemoveErrorListener(OnFatalError);
		}
	}

	public void Initialize(Action onHiddenCallback, Action onShownCallback = null)
	{
		this.OnRewardScrollHidden = onHiddenCallback;
		this.OnRewardScrollShown = onShownCallback;
	}

	public void Show()
	{
		OverlayUI.Get().AddGameObject(m_owner);
		if (SceneMgr.Get().GetMode() != SceneMgr.Mode.GAMEPLAY)
		{
			UIContext.GetRoot().ShowPopup(base.gameObject);
		}
		m_widget.TriggerEvent("CODE_SHOW_REWARD", new TriggerEventParameters(null, null, noDownwardPropagation: true));
		HandleCurrencyReward();
		this.OnRewardScrollShown?.Invoke();
	}

	private void Hide()
	{
		if (SceneMgr.Get().GetMode() != SceneMgr.Mode.GAMEPLAY)
		{
			UIContext.GetRoot().DismissPopup(base.gameObject);
		}
		this.OnRewardScrollHidden?.Invoke();
		m_isSignatureCard = false;
		UnityEngine.Object.Destroy(m_owner);
	}

	public static Widget ShowFakeRewardWidget(RewardScrollDataModel dataModel)
	{
		Widget widget = WidgetInstance.Create(RewardPresenter.REWARD_PREFAB);
		widget.BindDataModel(dataModel);
		widget.RegisterDoneChangingStatesListener(delegate
		{
			widget.GetComponentInChildren<RewardScroll>().Show();
		}, null, callImmediatelyIfSet: true, doOnce: true);
		return widget;
	}

	private void HandleCurrencyReward()
	{
		if (Shop.Get() == null || m_widget == null || !m_widget.GetDataModel(257, out var dataModel) || !(dataModel is RewardScrollDataModel rewardScrollDataModel))
		{
			return;
		}
		foreach (RewardItemDataModel item in rewardScrollDataModel.RewardList.Items)
		{
			if (item.ItemType == RewardItemType.CN_ARCANE_ORBS && ServiceManager.TryGet<CurrencyManager>(out var currencyManager))
			{
				currencyManager.MarkCurrencyDirty(CurrencyType.CN_ARCANE_ORBS);
				break;
			}
		}
	}

	private void OnFatalError(FatalErrorMessage message, object userData)
	{
		Hide();
	}

	public static void DebugShowFake(RewardScrollDataModel dataModel)
	{
		ShowFakeRewardWidget(dataModel);
	}
}
