using Hearthstone.Store;
using UnityEngine;

public class StoreRadioButton : FramedRadioButton
{
	public class Data
	{
		public ProductInfo m_bundle;

		public NoGTAPPTransactionData m_noGTAPPTransactionData;
	}

	public SaleBanner m_saleBanner;

	public UberText m_cost;

	public GameObject m_bonusFrame;

	public UberText m_bonusText;

	public GameObject m_realMoneyTextRoot;

	public GameObject m_goldRoot;

	public UberText m_goldCostText;

	public UberText m_goldButtonText;

	private static readonly Color NO_SALE_TEXT_COLOR = new Color(0.239f, 0.184f, 0.098f);

	private static readonly Color ON_SALE_TEXT_COLOR = new Color(0.702f, 0.114f, 0.153f);

	private void Awake()
	{
		ActivateSale(active: false);
	}

	public override void Init(FrameType frameType, string text, int buttonID, object userData)
	{
		base.Init(frameType, text, buttonID, userData);
		if (!(userData is Data storeRadioButtonData))
		{
			Debug.LogWarning(string.Format("StoreRadioButton.Init(): storeRadioButtonData is null (frameType={0}, text={1}, buttonID={2)", frameType, text, buttonID));
		}
		else if (storeRadioButtonData.m_bundle != null)
		{
			InitMoneyOption(storeRadioButtonData.m_bundle);
		}
		else if (storeRadioButtonData.m_noGTAPPTransactionData != null)
		{
			InitGoldOptionNoGTAPP(storeRadioButtonData.m_noGTAPPTransactionData);
		}
		else
		{
			Debug.LogWarning(string.Format("StoreRadioButton.Init(): storeRadioButtonData has neither gold price nor bundle data! (frameType={0}, text={1}, buttonID={2)", frameType, text, buttonID));
		}
	}

	public void ActivateSale(bool active)
	{
		m_saleBanner.m_root.SetActive(active);
		m_text.TextColor = (active ? ON_SALE_TEXT_COLOR : NO_SALE_TEXT_COLOR);
	}

	private void InitMoneyOption(ProductInfo bundle)
	{
		m_goldRoot.SetActive(value: false);
		m_realMoneyTextRoot.SetActive(value: true);
		m_bonusFrame.SetActive(value: false);
		if (bundle.TryGetRMPriceInfo(out var priceInfo))
		{
			m_cost.Text = string.Format(GameStrings.Get("GLUE_STORE_PRODUCT_PRICE"), priceInfo.CurrentPrice.DisplayPrice);
		}
	}

	private void InitGoldOptionNoGTAPP(NoGTAPPTransactionData noGTAPPTransactionData)
	{
		string costText = string.Empty;
		if (StoreManager.Get().GetGoldCostNoGTAPP(noGTAPPTransactionData, out var goldCost))
		{
			costText = goldCost.ToString();
		}
		m_goldRoot.SetActive(value: true);
		m_realMoneyTextRoot.SetActive(value: false);
		m_goldButtonText.Text = m_text.Text;
		m_goldCostText.Text = costText;
	}
}
