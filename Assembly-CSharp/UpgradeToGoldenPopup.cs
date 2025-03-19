using System.Collections;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class UpgradeToGoldenPopup : MonoBehaviour
{
	private Widget m_widget;

	private CraftingDataModel m_craftingDataModel = new CraftingDataModel();

	private CraftingUI m_craftingUI;

	private const string CODE_CREATE_EVENT = "CODE_CREATE";

	private const string CODE_UPGRADE_EVENT = "CODE_UPGRADE";

	private const string CODE_HIDE_EVENT = "CODE_HIDE";

	private const string GROW_EVENT = "GROW";

	private const string SHRINK_EVENT = "SHRINK";

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(HandleEvent);
		m_widget.BindDataModel(m_craftingDataModel);
	}

	public void SetInfo(CraftingPendingTransaction pendingTransaction, CraftingUI craftingUI, Transform showBone)
	{
		m_craftingUI = craftingUI;
		m_widget.transform.position = showBone.position;
		m_widget.transform.localScale = showBone.localScale;
		CraftingManager.Get().SetCraftingRelatedActorsActiveForUpgradeToGoldenPopup(active: false);
		m_craftingDataModel.IsGolden = pendingTransaction.Premium == TAG_PREMIUM.GOLDEN;
		CraftingManager.Get().TryGetCardUpgradeValue(pendingTransaction.CardID, out var upgradeDustCost);
		m_craftingDataModel.UpgradeDustCost = upgradeDustCost;
		CraftingManager.Get().TryGetCardBuyValue(pendingTransaction.CardID, pendingTransaction.Premium, out var createDustCost);
		m_craftingDataModel.CreateDustCost = createDustCost;
		m_craftingDataModel.NumOwnedNormal = CraftingManager.Get().GetNumOwnedIncludePending(pendingTransaction.CardID, TAG_PREMIUM.NORMAL);
		m_craftingDataModel.NumOwnedGolden = CraftingManager.Get().GetNumOwnedIncludePending(pendingTransaction.CardID, TAG_PREMIUM.GOLDEN);
	}

	public void OnHide()
	{
		m_widget.TriggerEvent("SHRINK");
		CraftingManager.Get().SetCraftingRelatedActorsActiveForUpgradeToGoldenPopup(active: true);
	}

	public IEnumerator ShowWhenReadyRoutine()
	{
		while (m_widget.IsChangingStates)
		{
			yield return null;
		}
		m_widget.TriggerEvent("GROW");
		m_widget.Show();
	}

	private void HandleEvent(string eventName)
	{
		CraftingManager craftingManager = CraftingManager.Get();
		switch (eventName)
		{
		case "CODE_CREATE":
		{
			CraftingPendingTransaction.Operation type2 = ((craftingManager.GetShownActor().GetPremium() == TAG_PREMIUM.NORMAL) ? CraftingPendingTransaction.Operation.NormalCreate : CraftingPendingTransaction.Operation.GoldenCreate);
			craftingManager.TryGetCardBuyValue(craftingManager.GetShownActor().GetEntityDef().GetCardId(), craftingManager.GetShownActor().GetPremium(), out var uncommittedDustChanges2);
			craftingManager.AdjustUnCommitedArcaneDustChanges(-uncommittedDustChanges2);
			craftingManager.GetPendingClientTransaction().Add(type2);
			craftingManager.HideUpgradeToGoldenWidget();
			m_craftingUI.DoUpgradeToGoldenAnimations();
			break;
		}
		case "CODE_UPGRADE":
		{
			CraftingPendingTransaction.Operation type = ((craftingManager.GetShownActor().GetPremium() == TAG_PREMIUM.NORMAL) ? CraftingPendingTransaction.Operation.UpgradeToGoldenFromNormal : CraftingPendingTransaction.Operation.UpgradeToGoldenFromGolden);
			craftingManager.GetPendingClientTransaction().Add(type);
			craftingManager.TryGetCardUpgradeValue(craftingManager.GetShownActor().GetEntityDef().GetCardId(), out var uncommittedDustChanges);
			craftingManager.AdjustUnCommitedArcaneDustChanges(-uncommittedDustChanges);
			craftingManager.SwitchPremiumView(TAG_PREMIUM.GOLDEN);
			craftingManager.HideUpgradeToGoldenWidget();
			m_craftingUI.DoUpgradeToGoldenAnimations();
			break;
		}
		case "CODE_HIDE":
			craftingManager.HideUpgradeToGoldenWidget();
			break;
		}
	}
}
