using System;
using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
public class CollectionManagerSpinnerPopup : UIBPopup
{
	private enum PopupState
	{
		Init,
		Waiting,
		Success,
		Failure,
		Timeout,
		Finished
	}

	private const float MAX_WAITING_TIME = 10f;

	[CustomEditField(Sections = "Base UI")]
	public MultiSliceElement m_root;

	[CustomEditField(Sections = "Swirly Animation")]
	public Spell m_spell;

	[CustomEditField(Sections = "Base UI")]
	public UIBButton m_okButton;

	[CustomEditField(Sections = "Text")]
	public UberText m_waitingText;

	[CustomEditField(Sections = "Text")]
	public UberText m_successHeadlineText;

	[CustomEditField(Sections = "Text")]
	public UberText m_failHeadlineText;

	[CustomEditField(Sections = "Text")]
	public UberText m_failDetailsText;

	[CustomEditField(Sections = "Scale")]
	public Vector3 m_pcScale;

	[CustomEditField(Sections = "Scale")]
	public Vector3 m_phoneScale;

	private Action m_OkListener;

	private PopupState m_currentState;

	private float m_currentWaitingTime;

	protected override void Awake()
	{
		base.Awake();
		m_okButton.AddEventListener(UIEventType.RELEASE, OnOkButtonPressed);
		m_showScale = (UniversalInputManager.UsePhoneUI ? m_phoneScale : m_pcScale);
		SetState(PopupState.Init);
	}

	public void Show(Action OnPlayerClicksOk)
	{
		if (!m_shown)
		{
			m_OkListener = OnPlayerClicksOk;
			SetState(PopupState.Waiting);
			m_shown = true;
			m_spell.ActivateState(SpellStateType.BIRTH);
			m_root.UpdateSlices();
			Navigation.PushBlockBackingOut();
			if (!OverlayUI.Get().HasObject(base.gameObject))
			{
				OverlayUI.Get().AddGameObject(base.gameObject);
			}
			UIContext.GetRoot().ShowPopup(base.gameObject);
			DoShowAnimation();
		}
	}

	public void UpdateSuccessOrFail(bool isSuccess)
	{
		if (m_currentState == PopupState.Waiting)
		{
			SetState(isSuccess ? PopupState.Success : PopupState.Failure);
		}
	}

	public override void Hide()
	{
		if (m_shown)
		{
			m_shown = false;
			Navigation.PopBlockBackingOut();
			DoHideAnimation(delegate
			{
				m_spell.ActivateState(SpellStateType.NONE);
				UIContext.GetRoot().DismissPopup(base.gameObject);
			});
		}
	}

	private void Update()
	{
		if (m_currentState == PopupState.Waiting)
		{
			m_currentWaitingTime += Time.deltaTime;
			if (m_currentWaitingTime >= 10f)
			{
				m_currentWaitingTime = 0f;
				SetState(PopupState.Timeout);
			}
		}
	}

	private void SetState(PopupState state)
	{
		switch (state)
		{
		case PopupState.Init:
			StartInitState();
			break;
		case PopupState.Waiting:
			StartWaitingState();
			break;
		case PopupState.Success:
			StartSuccessState();
			break;
		case PopupState.Failure:
			StartFailureState();
			break;
		case PopupState.Timeout:
			StartTimeoutState();
			break;
		case PopupState.Finished:
			StartFinishedState();
			break;
		}
		m_currentState = state;
	}

	private void StartInitState()
	{
		SetupButton();
		SetupPopupText();
		m_waitingText.gameObject.SetActive(value: false);
		m_successHeadlineText.gameObject.SetActive(value: false);
		m_failHeadlineText.gameObject.SetActive(value: false);
		m_failDetailsText.gameObject.SetActive(value: false);
		m_okButton.gameObject.SetActive(value: false);
		m_currentWaitingTime = 0f;
	}

	private void StartWaitingState()
	{
		m_waitingText.gameObject.SetActive(value: true);
		m_successHeadlineText.gameObject.SetActive(value: false);
		m_failHeadlineText.gameObject.SetActive(value: false);
		m_failDetailsText.gameObject.SetActive(value: false);
		m_okButton.gameObject.SetActive(value: false);
		m_currentWaitingTime = 0f;
	}

	private void StartSuccessState()
	{
		m_waitingText.gameObject.SetActive(value: false);
		m_successHeadlineText.gameObject.SetActive(value: true);
		m_failHeadlineText.gameObject.SetActive(value: false);
		m_failDetailsText.gameObject.SetActive(value: false);
		m_okButton.gameObject.SetActive(value: true);
		m_spell.ActivateState(SpellStateType.ACTION);
	}

	private void StartFailureState()
	{
		m_failDetailsText.Text = GameStrings.Get("GLUE_COLLECTION_SPINNER_FAIL_BODY");
		m_waitingText.gameObject.SetActive(value: false);
		m_successHeadlineText.gameObject.SetActive(value: false);
		m_failHeadlineText.gameObject.SetActive(value: true);
		m_failDetailsText.gameObject.SetActive(value: true);
		m_okButton.gameObject.SetActive(value: true);
		m_spell.ActivateState(SpellStateType.DEATH);
	}

	private void StartTimeoutState()
	{
		m_failDetailsText.Text = GameStrings.Get("GLUE_COLLECTION_SPINNER_TIMEOUT_BODY");
		m_waitingText.gameObject.SetActive(value: false);
		m_successHeadlineText.gameObject.SetActive(value: false);
		m_failHeadlineText.gameObject.SetActive(value: true);
		m_failDetailsText.gameObject.SetActive(value: true);
		m_okButton.gameObject.SetActive(value: true);
		m_spell.ActivateState(SpellStateType.DEATH);
	}

	private void StartFinishedState()
	{
		m_OkListener = null;
		m_okButton.gameObject.SetActive(value: false);
		Hide();
	}

	private void SetupButton()
	{
		m_okButton.SetText("GLOBAL_OKAY");
		m_okButton.gameObject.SetActive(value: false);
	}

	private void SetupPopupText()
	{
		m_waitingText.Text = GameStrings.Get("GLUE_COLLECTION_SPINNER_WAITING");
		m_successHeadlineText.Text = GameStrings.Get("GLUE_COLLECTION_SPINNER_SUCCESS_HEADLINE");
		m_failHeadlineText.Text = GameStrings.Get("GLUE_COLLECTION_SPINNER_FAIL_HEADLINE");
		m_failDetailsText.Text = GameStrings.Get("GLUE_COLLECTION_SPINNER_FAIL_BODY");
	}

	private void OnOkButtonPressed(UIEvent e)
	{
		if (m_OkListener != null)
		{
			m_OkListener();
		}
		SetState(PopupState.Finished);
	}
}
