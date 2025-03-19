using Hearthstone.UI;
using UnityEngine;

public class StoreQuantityPrompt : UIBPopup
{
	private enum QuantityResponse
	{
		Success,
		NotNumber,
		BelowMin,
		AboveMax
	}

	public delegate void OkayListener(int quantity);

	public delegate void CancelListener();

	public GameObject m_root;

	public Clickable m_clickCatcherClickable;

	public UberText m_instructionText;

	public UberText m_inputTextFieldBounds;

	private string m_currentInputText;

	private int m_currentMaxQuantity;

	private OkayListener m_currentOkayListener;

	private CancelListener m_currentCancelListener;

	protected override void Awake()
	{
		base.Awake();
		m_clickCatcherClickable.AddEventListener(UIEventType.RELEASE, OnCancelRequest);
	}

	public bool Show(int maxQuantity, OkayListener delOkay = null, CancelListener delCancel = null)
	{
		if (m_shown)
		{
			return false;
		}
		m_currentMaxQuantity = maxQuantity;
		m_shown = true;
		m_currentOkayListener = delOkay;
		m_currentCancelListener = delCancel;
		m_instructionText.Text = GameStrings.Format("GLUE_STORE_PACK_QUANTITY_BODY", m_currentMaxQuantity);
		UpdateQuantityText(string.Empty);
		m_root.SetActive(value: true);
		DoShowAnimation(ShowInput);
		return true;
	}

	protected override void Hide(bool animate)
	{
		if (m_shown)
		{
			m_shown = false;
			HideInput();
			DoHideAnimation(!animate, delegate
			{
				m_root.SetActive(value: false);
			});
		}
	}

	private QuantityResponse GetQuantity(out int quantity)
	{
		if (!int.TryParse(m_currentInputText, out quantity))
		{
			Debug.LogWarning($"GeneralStore.OnStoreQuantityOkayPressed: invalid quantity='{m_currentInputText}'");
			return QuantityResponse.NotNumber;
		}
		if (quantity <= 0)
		{
			Log.Store.Print($"GeneralStore.OnStoreQuantityOkayPressed: quantity {quantity} must be positive");
			return QuantityResponse.BelowMin;
		}
		if (quantity > m_currentMaxQuantity)
		{
			Log.Store.Print($"GeneralStore.OnStoreQuantityOkayPressed: quantity {quantity} is larger than max allowed quantity ({m_currentMaxQuantity})");
			return QuantityResponse.AboveMax;
		}
		return QuantityResponse.Success;
	}

	private void Submit()
	{
		Hide(animate: true);
		int quantity;
		switch (GetQuantity(out quantity))
		{
		case QuantityResponse.Success:
			FireOkayEvent(quantity);
			return;
		case QuantityResponse.AboveMax:
			UIStatus.Get().AddError(GameStrings.Get("GLUE_STORE_PACK_QUANTITY_WARN_MAX"));
			break;
		}
		FireCancelEvent();
	}

	public void Cancel()
	{
		Hide(animate: true);
		FireCancelEvent();
	}

	private void FireOkayEvent(int quantity)
	{
		m_currentOkayListener?.Invoke(quantity);
		m_currentOkayListener = null;
	}

	private void FireCancelEvent()
	{
		m_currentCancelListener?.Invoke();
		m_currentCancelListener = null;
	}

	private void OnSubmitRequest(UIEvent e)
	{
		Submit();
	}

	private void OnCancelRequest(UIEvent e)
	{
		Cancel();
	}

	private void ShowInput()
	{
		Camera camera = CameraUtils.FindProjectionCameraForObject(base.gameObject);
		Bounds textBounds = m_inputTextFieldBounds.GetBounds();
		Rect rect = CameraUtils.CreateGUIViewportRect(camera, textBounds.min, textBounds.max);
		UniversalInputManager.TextInputParams inputParams = new UniversalInputManager.TextInputParams
		{
			m_owner = base.gameObject,
			m_number = true,
			m_rect = rect,
			m_updatedCallback = OnInputUpdated,
			m_completedCallback = OnInputComplete,
			m_canceledCallback = OnInputCanceled,
			m_font = m_inputTextFieldBounds.GetLocalizedFont(),
			m_alignment = TextAnchor.MiddleLeft,
			m_maxCharacters = 2,
			m_touchScreenKeyboardHideInput = true,
			m_touchScreenKeyboardType = 0
		};
		UniversalInputManager.Get().UseTextInput(inputParams);
	}

	private void HideInput()
	{
		UniversalInputManager.Get().CancelTextInput(base.gameObject);
	}

	private void OnInputUpdated(string input)
	{
		UpdateQuantityText(input);
	}

	private void OnInputComplete(string input)
	{
		UpdateQuantityText(input);
		Submit();
	}

	private void OnInputCanceled(bool userRequested, GameObject requester)
	{
		UpdateQuantityText(string.Empty);
		Cancel();
	}

	private void UpdateQuantityText(string input)
	{
		m_instructionText.gameObject.SetActive(string.IsNullOrEmpty(input));
		m_currentInputText = input;
	}
}
