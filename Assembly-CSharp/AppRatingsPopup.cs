using System;
using UnityEngine;

public class AppRatingsPopup : UIBPopup
{
	public UIBButton m_acceptButton;

	public UIBButton m_declineButton;

	[CustomEditField(Hide = true)]
	public Action m_OnHideCallback;

	public Vector3 m_showPosPhone = new Vector3(0f, 15f, -2f);

	public Vector3 m_showScalePhone = new Vector3(85f, 85f, 85f);

	private PegUIElement m_inputBlockerPegUIElement;

	protected override void Awake()
	{
		base.Awake();
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_showPosition = m_showPosPhone;
			m_showScale = m_showScalePhone;
		}
		m_acceptButton.AddEventListener(UIEventType.RELEASE, OnAcceptPressed);
		m_declineButton.AddEventListener(UIEventType.RELEASE, OnDeclinePress);
	}

	private void OnDestroy()
	{
		Hide(animate: false);
	}

	public override void Show()
	{
		if (!IsShown())
		{
			Navigation.Push(OnNavigateBack);
			base.Show(useOverlayUI: true);
			base.gameObject.SetActive(value: true);
			if (m_inputBlockerPegUIElement != null)
			{
				UnityEngine.Object.Destroy(m_inputBlockerPegUIElement.gameObject);
				m_inputBlockerPegUIElement = null;
			}
			GameObject inputBlockerObject = CameraUtils.CreateInputBlocker(CameraUtils.FindFirstByLayer(base.gameObject.layer), "AppRatingsPopUpInputBlocker");
			LayerUtils.SetLayer(inputBlockerObject, base.gameObject.layer, null);
			m_inputBlockerPegUIElement = inputBlockerObject.AddComponent<PegUIElement>();
			TransformUtil.SetPosY(m_inputBlockerPegUIElement, base.gameObject.transform.position.y - 5f);
			DarkenInputBlocker(inputBlockerObject, 0.5f);
		}
	}

	protected override void Hide(bool animate)
	{
		if (IsShown())
		{
			Navigation.RemoveHandler(OnNavigateBack);
			if (m_inputBlockerPegUIElement != null)
			{
				UnityEngine.Object.Destroy(m_inputBlockerPegUIElement.gameObject);
				m_inputBlockerPegUIElement = null;
			}
			base.gameObject.SetActive(value: false);
			base.Hide(animate);
			m_OnHideCallback?.Invoke();
			m_OnHideCallback = null;
		}
	}

	private void OnAcceptPressed(UIEvent e)
	{
		TelemetryManager.Client().SendButtonPressed("AppReviewAccept");
		Options.Get().SetBool(Option.APP_RATING_AGREED, val: true);
		MobileCallbackManager.ShowNativeAppRatingPopup();
		Hide(animate: true);
	}

	private void OnDeclinePress(UIEvent e)
	{
		TelemetryManager.Client().SendButtonPressed("AppReviewReject");
		Hide(animate: false);
	}

	private bool OnNavigateBack()
	{
		Hide(animate: true);
		return true;
	}
}
