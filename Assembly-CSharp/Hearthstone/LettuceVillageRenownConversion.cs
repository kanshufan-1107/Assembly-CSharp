using System.Collections;
using Blizzard.T5.Core.Utils;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using Hearthstone.UI;
using PegasusLettuce;
using UnityEngine;

namespace Hearthstone;

public class LettuceVillageRenownConversion : MonoBehaviour
{
	[SerializeField]
	private AsyncReference m_convertButtonReference;

	private UIBButton m_convertButton;

	private bool m_isConvertButtonReady;

	private Widget m_widget;

	private RewardPresenter m_rewardPresenter = new RewardPresenter();

	private const string MAKE_VISIBLE = "MAKE_VISIBLE";

	private const string MAKE_HIDDEN = "MAKE_HIDDEN";

	private const string RENOWN_CONVERT_IDLE = "ON_RENOWN_CONVERT_IDLE";

	private const string RENOWN_CONVERT_INPROGRESS = "ON_RENOWN_CONVERT_INPROGRESS";

	private const string RENOWN_CONVERT_DONE = "ON_RENOWN_CONVERT_DONE";

	private const string RENOWN_CONVERT_ACK = "ON_RENOWN_CONVERT_ACKNOWLEDGED";

	private Coroutine m_waitUntilReadyCoroutine;

	public bool IsProcessing { get; private set; }

	private void Awake()
	{
		m_widget = GetComponent<Widget>();
		m_widget.RegisterEventListener(HandleEvent);
	}

	private void Start()
	{
		m_convertButtonReference.RegisterReadyListener<UIBButton>(OnConvertButtonReady);
		m_waitUntilReadyCoroutine = StartCoroutine(WaitUntilReady());
	}

	private void OnDestroy()
	{
		if (m_waitUntilReadyCoroutine != null)
		{
			StopCoroutine(m_waitUntilReadyCoroutine);
			m_waitUntilReadyCoroutine = null;
		}
	}

	private IEnumerator WaitUntilReady()
	{
		while (!m_isConvertButtonReady)
		{
			yield return null;
		}
		m_waitUntilReadyCoroutine = null;
	}

	private void HandleEvent(string eventName)
	{
		if (eventName == "MAKE_VISIBLE")
		{
			m_widget.BindDataModel(LettuceRenownUtil.GetCurrentRenownTradeData(updateConversionValues: true));
			m_widget.TriggerEvent("ON_RENOWN_CONVERT_IDLE");
		}
	}

	private void OnConvertExcessCoinsReceived()
	{
		IsProcessing = false;
		Network.Get().RemoveNetHandler(MercenariesConvertExcessCoinsResponse.PacketID.ID, OnConvertExcessCoinsReceived);
		MercenariesConvertExcessCoinsResponse response = Network.Get().ConvertExcessCoinsToRenownResponse();
		if (response.Success && response.RenownGenerated > 0)
		{
			m_widget.TriggerEvent("ON_RENOWN_CONVERT_DONE");
			ShowRenownConversionReward(response.RenownGenerated);
			return;
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_LETTUCE_RENOWN_CONVERSION_CLAIM_FAILED_TITLE"),
			m_text = GameStrings.Get("GLUE_LETTUCE_RENOWN_CONVERSION_CLAIM_FAILED_BODY"),
			m_showAlertIcon = true,
			m_responseDisplay = AlertPopup.ResponseDisplay.OK,
			m_responseCallback = delegate
			{
				m_widget.TriggerEvent("ON_RENOWN_CONVERT_IDLE");
			}
		};
		DialogManager.Get().ShowPopup(info);
	}

	private void OnConvertButtonReady(UIBButton button)
	{
		m_isConvertButtonReady = true;
		if (button == null)
		{
			Log.Lettuce.PrintError("PreviousTaskButton could not be found.");
			return;
		}
		m_convertButton = button;
		m_convertButton.AddEventListener(UIEventType.RELEASE, OnConvertButtonReleased);
	}

	private void OnConvertButtonReleased(UIEvent e)
	{
		IsProcessing = true;
		m_widget.TriggerEvent("ON_RENOWN_CONVERT_INPROGRESS");
		Network.Get().RegisterNetHandler(MercenariesConvertExcessCoinsResponse.PacketID.ID, OnConvertExcessCoinsReceived);
		LettuceRenownUtil.ConvertAllExcessMercCoins();
	}

	private void ShowRenownConversionReward(long amount)
	{
		RewardItemDataModel renownItem = new RewardItemDataModel
		{
			ItemType = RewardItemType.MERCENARY_RENOWN,
			Quantity = 1,
			Currency = new PriceDataModel
			{
				Currency = CurrencyType.RENOWN,
				Amount = amount,
				DisplayText = amount.ToString()
			}
		};
		RewardListDataModel rewards = new RewardListDataModel();
		rewards.Items.Add(renownItem);
		RewardScrollDataModel rewardScrollDataModel = new RewardScrollDataModel();
		rewardScrollDataModel.DisplayName = GameStrings.Get("GLUE_LETTUCE_RENOWN_CONVERSION_CLAIMED_TITLE");
		rewardScrollDataModel.Description = GameStrings.Format("GLUE_LETTUCE_RENOWN_CONVERSION_CLAIMED_REWARD", amount);
		rewardScrollDataModel.RewardList = rewards;
		RewardScrollDataModel rewardScrollDataModel2 = rewardScrollDataModel;
		m_rewardPresenter.EnqueueReward(rewardScrollDataModel2, GeneralUtils.noOp);
		m_rewardPresenter.ShowNextReward(OnRenownRewardHidden);
	}

	private void OnRenownRewardHidden()
	{
		m_widget.TriggerEvent("ON_RENOWN_CONVERT_ACKNOWLEDGED");
		LettuceVillagePopupManager popupManager = LettuceVillagePopupManager.Get();
		if (popupManager != null)
		{
			LettuceVillagePopupManager.PopupType targetPopup = popupManager.PreviouslyOpenPopup;
			if (targetPopup == LettuceVillagePopupManager.PopupType.TASKBOARD)
			{
				popupManager.Show(targetPopup);
				return;
			}
		}
		popupManager.Hide(LettuceVillagePopupManager.PopupType.RENOWNCONVERSION);
	}
}
