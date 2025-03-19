using System;
using System.Collections.Generic;
using System.Linq;
using Blizzard.Commerce;
using Blizzard.T5.Services;
using Hearthstone.Store;
using PegasusShared;
using PegasusUtil;

[CustomEditClass]
public class StorePurchaseAuth : UIBPopup
{
	public delegate void AckPurchaseResultListener(bool success, MoneyOrGTAPPTransaction moneyOrGTAPPTransaction);

	public delegate void PurchaseLockedDialogCallback(bool showHelp);

	private enum InternalButtonStyle
	{
		Unset,
		NoButton,
		Ok,
		Back,
		BackWithOkText,
		Cancel
	}

	public enum ButtonStyle
	{
		NoButton = 1,
		Back = 3,
		Cancel = 5
	}

	private const string s_OkButtonText = "GLOBAL_OKAY";

	private const string s_BackButtonText = "GLOBAL_BACK";

	private const string s_CancelButtonText = "GLOBAL_CANCEL";

	[CustomEditField(Sections = "Base UI")]
	public MultiSliceElement m_root;

	[CustomEditField(Sections = "Swirly Animation")]
	public Spell m_spell;

	[CustomEditField(Sections = "Base UI")]
	public UIBButton m_okButton;

	[CustomEditField(Sections = "Text")]
	public UberText m_waitingForAuthText;

	[CustomEditField(Sections = "Text")]
	public UberText m_successHeadlineText;

	[CustomEditField(Sections = "Text")]
	public UberText m_failHeadlineText;

	[CustomEditField(Sections = "Text")]
	public UberText m_failDetailsText;

	[CustomEditField(Sections = "Base UI")]
	public StoreMiniSummary m_miniSummary;

	private bool m_showingSuccess;

	private MoneyOrGTAPPTransaction m_moneyOrGTAPPTransaction;

	private List<AckPurchaseResultListener> m_ackPurchaseResultListeners = new List<AckPurchaseResultListener>();

	private List<Action> m_cancelButtonListeners = new List<Action>();

	private List<Action> m_exitListeners = new List<Action>();

	private InternalButtonStyle m_buttonStyle;

	protected override void Awake()
	{
		base.Awake();
		m_miniSummary.gameObject.SetActive(value: false);
		m_okButton.AddEventListener(UIEventType.RELEASE, OnOkayButtonPressed);
		SetButtonStyle(InternalButtonStyle.NoButton);
	}

	public void Show(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, bool isZeroCostLicense, ButtonStyle waitButtonStyle = ButtonStyle.NoButton)
	{
		if (!m_shown)
		{
			m_shown = true;
			StartNewTransaction(moneyOrGTAPPTransaction, isZeroCostLicense, waitButtonStyle);
			m_spell.ActivateState(SpellStateType.BIRTH);
			if (m_moneyOrGTAPPTransaction != null && m_moneyOrGTAPPTransaction.ShouldShowMiniSummary())
			{
				ShowMiniSummary();
			}
			else
			{
				m_root.UpdateSlices();
			}
			Navigation.PushBlockBackingOut();
			DoShowAnimation();
		}
	}

	public void StartNewTransaction(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, bool isZeroCostLicense, ButtonStyle waitButtonStyle = ButtonStyle.NoButton)
	{
		m_moneyOrGTAPPTransaction = moneyOrGTAPPTransaction;
		m_showingSuccess = false;
		if (isZeroCostLicense)
		{
			m_waitingForAuthText.Text = GameStrings.Get("GLUE_STORE_AUTH_ZERO_COST_WAITING");
			m_successHeadlineText.Text = GameStrings.Get("GLUE_STORE_AUTH_ZERO_COST_SUCCESS_HEADLINE");
			m_failHeadlineText.Text = GameStrings.Get("GLUE_STORE_AUTH_ZERO_COST_FAIL_HEADLINE");
		}
		else
		{
			m_waitingForAuthText.Text = GameStrings.Get("GLUE_STORE_AUTH_WAITING");
			m_successHeadlineText.Text = GameStrings.Get("GLUE_STORE_AUTH_SUCCESS_HEADLINE");
			m_failHeadlineText.Text = GameStrings.Get("GLUE_STORE_AUTH_FAIL_HEADLINE");
		}
		SetButtonStyle((InternalButtonStyle)waitButtonStyle);
		m_waitingForAuthText.gameObject.SetActive(value: true);
		m_successHeadlineText.gameObject.SetActive(value: false);
		m_failHeadlineText.gameObject.SetActive(value: false);
		m_failDetailsText.gameObject.SetActive(value: false);
		if (moneyOrGTAPPTransaction != null && moneyOrGTAPPTransaction.PMTProductID.HasValue && (bool)m_miniSummary && m_miniSummary.gameObject.activeSelf && ProductId.IsValid(moneyOrGTAPPTransaction.PMTProductID.Value))
		{
			m_miniSummary.SetDetails(ProductId.CreateFrom(moneyOrGTAPPTransaction.PMTProductID.Value), 1);
		}
	}

	public void ShowPurchaseLocked(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, bool isZeroCostLicense, ButtonStyle waitButtonStyle, PurchaseLockedDialogCallback purchaseLockedCallback)
	{
		Show(moneyOrGTAPPTransaction, isZeroCostLicense, waitButtonStyle);
		string storeName = string.Empty;
		if (moneyOrGTAPPTransaction.Provider.HasValue)
		{
			storeName = moneyOrGTAPPTransaction.Provider.Value switch
			{
				BattlePayProvider.BP_PROVIDER_APPLE => GameStrings.Get("GLOBAL_STORE_MOBILE_NAME_APPLE"), 
				BattlePayProvider.BP_PROVIDER_GOOGLE_PLAY => GameStrings.Get("GLOBAL_STORE_MOBILE_NAME_GOOGLE"), 
				BattlePayProvider.BP_PROVIDER_AMAZON => GameStrings.Get("GLOBAL_STORE_MOBILE_NAME_AMAZON"), 
				_ => GameStrings.Get("GLOBAL_STORE_MOBILE_NAME_DEFAULT"), 
			};
		}
		string lockDescription = GameStrings.Format("GLUE_STORE_PURCHASE_LOCK_DESCRIPTION", storeName);
		DialogManager.Get().ShowPopup(new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_STORE_PURCHASE_LOCK_HEADER"),
			m_confirmText = GameStrings.Get("GLOBAL_CANCEL"),
			m_cancelText = GameStrings.Get("GLOBAL_HELP"),
			m_text = lockDescription,
			m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
			m_iconSet = AlertPopup.PopupInfo.IconSet.Alternate,
			m_responseCallback = delegate(AlertPopup.Response response, object data)
			{
				if (purchaseLockedCallback != null)
				{
					purchaseLockedCallback(response == AlertPopup.Response.CANCEL);
				}
			}
		});
	}

	public override void Hide()
	{
		if (m_shown)
		{
			m_shown = false;
			Navigation.PopBlockBackingOut();
			DoHideAnimation(delegate
			{
				SetButtonStyle(InternalButtonStyle.NoButton);
				m_miniSummary.gameObject.SetActive(value: false);
				m_spell.ActivateState(SpellStateType.NONE);
			});
		}
	}

	public bool CompletePurchaseSuccess(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return false;
		}
		bool shouldShowSummary = false;
		if (moneyOrGTAPPTransaction != null)
		{
			shouldShowSummary = moneyOrGTAPPTransaction.ShouldShowMiniSummary();
		}
		ShowPurchaseSuccess(moneyOrGTAPPTransaction, shouldShowSummary);
		return true;
	}

	public bool CompletePurchaseFailure(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, string failDetails, Network.PurchaseErrorInfo.ErrorType error)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return false;
		}
		bool shouldShowSummary = false;
		if (moneyOrGTAPPTransaction != null)
		{
			shouldShowSummary = moneyOrGTAPPTransaction.ShouldShowMiniSummary();
		}
		ShowPurchaseFailure(moneyOrGTAPPTransaction, failDetails, shouldShowSummary, error);
		return true;
	}

	public void ShowPreviousPurchaseSuccess(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, ButtonStyle buttonStyle = ButtonStyle.NoButton)
	{
		Show(moneyOrGTAPPTransaction, isZeroCostLicense: false, buttonStyle);
		ShowPurchaseSuccess(moneyOrGTAPPTransaction, showMiniSummary: true);
	}

	public void ShowPreviousPurchaseFailure(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, string failDetails, ButtonStyle buttonStyle, Network.PurchaseErrorInfo.ErrorType error)
	{
		Show(moneyOrGTAPPTransaction, isZeroCostLicense: false, buttonStyle);
		ShowPurchaseFailure(moneyOrGTAPPTransaction, failDetails, showMiniSummary: true, error);
	}

	public void ShowPurchaseMethodFailure(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, string failDetails, ButtonStyle buttonStyle, Network.PurchaseErrorInfo.ErrorType error)
	{
		Show(moneyOrGTAPPTransaction, isZeroCostLicense: false, buttonStyle);
		ShowPurchaseFailure(moneyOrGTAPPTransaction, failDetails, showMiniSummary: false, error);
	}

	public void RegisterAckPurchaseResultListener(AckPurchaseResultListener listener)
	{
		if (!m_ackPurchaseResultListeners.Contains(listener))
		{
			m_ackPurchaseResultListeners.Add(listener);
		}
	}

	public void RemoveAckPurchaseResultListener(AckPurchaseResultListener listener)
	{
		m_ackPurchaseResultListeners.Remove(listener);
	}

	public void RegisterCancelButtonListener(Action listener)
	{
		if (!m_cancelButtonListeners.Contains(listener))
		{
			m_cancelButtonListeners.Add(listener);
		}
	}

	public void RemoveCancelButtonListener(Action listener)
	{
		m_cancelButtonListeners.Remove(listener);
	}

	public void RegisterExitListener(Action listener)
	{
		if (!m_exitListeners.Contains(listener))
		{
			m_exitListeners.Add(listener);
		}
	}

	public void RemoveExitListener(Action listener)
	{
		m_exitListeners.Remove(listener);
	}

	public bool HideCancelButton()
	{
		if (m_showingSuccess)
		{
			return false;
		}
		if (m_buttonStyle != InternalButtonStyle.Cancel)
		{
			return false;
		}
		SetButtonStyle(InternalButtonStyle.NoButton);
		return true;
	}

	private void SetButtonStyle(InternalButtonStyle buttonStyle)
	{
		if (buttonStyle != m_buttonStyle)
		{
			m_buttonStyle = buttonStyle;
			string buttonText;
			switch (buttonStyle)
			{
			case InternalButtonStyle.Ok:
			case InternalButtonStyle.BackWithOkText:
				buttonText = "GLOBAL_OKAY";
				break;
			case InternalButtonStyle.Back:
				buttonText = "GLOBAL_BACK";
				break;
			case InternalButtonStyle.Cancel:
				buttonText = "GLOBAL_CANCEL";
				break;
			default:
				buttonText = null;
				break;
			}
			if (buttonText == null)
			{
				m_okButton.gameObject.SetActive(value: false);
				return;
			}
			m_okButton.SetText(buttonText);
			m_okButton.gameObject.SetActive(value: true);
			LayerUtils.SetLayer(m_okButton, GameLayer.HighPriorityUI);
		}
	}

	private void OnOkayButtonPressed(UIEvent e)
	{
		if (m_showingSuccess)
		{
			string message = null;
			if (m_moneyOrGTAPPTransaction != null && ServiceManager.TryGet<IProductDataService>(out var dataService) && dataService.TryGetProduct(m_moneyOrGTAPPTransaction.PMTProductID, out var bundle) && bundle.Items != null)
			{
				Network.BundleItem heroLicense = bundle.Items.FirstOrDefault((Network.BundleItem i) => i.ItemType == ProductType.PRODUCT_TYPE_HERO);
				if (heroLicense != null)
				{
					CardHeroDbfRecord cardHeroDbf = GameUtils.GetCardHeroRecordForCardId(heroLicense.ProductData);
					if (cardHeroDbf != null)
					{
						message = cardHeroDbf.PurchaseCompleteMsg;
					}
				}
			}
			if (!string.IsNullOrEmpty(message))
			{
				Hide();
				AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
				{
					m_headerText = GameStrings.Get("GLUE_STORE_AUTH_SUCCESS_HEADLINE"),
					m_text = message,
					m_showAlertIcon = false,
					m_responseDisplay = AlertPopup.ResponseDisplay.OK,
					m_responseCallback = delegate
					{
						OnOkayButtonPressed_Finish();
					}
				};
				DialogManager.Get().ShowPopup(info);
				return;
			}
		}
		OnOkayButtonPressed_Finish();
	}

	private void OnOkayButtonPressed_Finish()
	{
		switch (m_buttonStyle)
		{
		case InternalButtonStyle.Ok:
		{
			Hide();
			AckPurchaseResultListener[] array2 = m_ackPurchaseResultListeners.ToArray();
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i](m_showingSuccess, m_moneyOrGTAPPTransaction);
			}
			break;
		}
		case InternalButtonStyle.Back:
		case InternalButtonStyle.BackWithOkText:
		{
			Action[] array = m_exitListeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i]();
			}
			break;
		}
		case InternalButtonStyle.Cancel:
		{
			Hide();
			Action[] array = m_cancelButtonListeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i]();
			}
			break;
		}
		default:
			Hide();
			break;
		}
	}

	private void ShowPurchaseSuccess(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, bool showMiniSummary)
	{
		m_showingSuccess = true;
		m_moneyOrGTAPPTransaction = moneyOrGTAPPTransaction;
		SetButtonStyle(InternalButtonStyle.Ok);
		if (showMiniSummary)
		{
			ShowMiniSummary();
		}
		m_waitingForAuthText.gameObject.SetActive(value: false);
		m_successHeadlineText.gameObject.SetActive(value: true);
		m_failHeadlineText.gameObject.SetActive(value: false);
		m_failDetailsText.gameObject.SetActive(value: false);
		m_spell.ActivateState(SpellStateType.ACTION);
	}

	private void ShowPurchaseFailure(MoneyOrGTAPPTransaction moneyOrGTAPPTransaction, string failDetails, bool showMiniSummary, Network.PurchaseErrorInfo.ErrorType error)
	{
		m_showingSuccess = false;
		m_moneyOrGTAPPTransaction = moneyOrGTAPPTransaction;
		if (error == Network.PurchaseErrorInfo.ErrorType.PRODUCT_EVENT_HAS_ENDED && (SceneMgr.Get().IsModeRequested(SceneMgr.Mode.TAVERN_BRAWL) || SceneMgr.Get().IsModeRequested(SceneMgr.Mode.DRAFT)))
		{
			SetButtonStyle(InternalButtonStyle.BackWithOkText);
		}
		else
		{
			SetButtonStyle(InternalButtonStyle.Ok);
		}
		if (showMiniSummary)
		{
			ShowMiniSummary();
		}
		m_failDetailsText.Text = failDetails;
		m_waitingForAuthText.gameObject.SetActive(value: false);
		m_successHeadlineText.gameObject.SetActive(value: false);
		m_failHeadlineText.gameObject.SetActive(value: true);
		m_failDetailsText.gameObject.SetActive(value: true);
		m_spell.ActivateState(SpellStateType.DEATH);
	}

	private void ShowMiniSummary()
	{
		MoneyOrGTAPPTransaction moneyOrGTAPPTransaction = m_moneyOrGTAPPTransaction;
		if (moneyOrGTAPPTransaction != null && moneyOrGTAPPTransaction.PMTProductID.HasValue && ProductId.IsValid(m_moneyOrGTAPPTransaction.PMTProductID.Value))
		{
			m_miniSummary.SetDetails(ProductId.CreateFrom(m_moneyOrGTAPPTransaction.PMTProductID.Value), 1);
			m_miniSummary.gameObject.SetActive(value: true);
			m_root.UpdateSlices();
		}
	}
}
