using System;
using System.Collections;
using UnityEngine;

public class OutstandingDraftTicketDialog : DialogBase
{
	public class Info
	{
		public Action m_callbackOnEnter;

		public Action m_callbackOnCancel;

		public int m_outstandingTicketCount;
	}

	[CustomEditField(Sections = "Object Links")]
	public UIBButton m_enterButton;

	[CustomEditField(Sections = "Object Links")]
	public UIBButton m_cancelButton;

	public UberText m_ticketCount;

	public UberText m_description;

	public GameObject m_plusSign;

	[CustomEditField(Sections = "Sounds", T = EditType.SOUND_PREFAB)]
	public string m_showAnimationSound = "Expand_Up.prefab:775d97ea42498c044897f396362b9db3";

	[CustomEditField(Sections = "Sounds", T = EditType.SOUND_PREFAB)]
	public string m_hideAnimationSound = "Shrink_Down_Quicker.prefab:2fe963b171811ca4b8d544fa53e3330c";

	private Info m_info;

	private bool m_isConfirmed;

	protected override void Awake()
	{
		base.Awake();
		m_enterButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			HandleDraftTicketResponse(isConfirmed: true);
		});
		m_cancelButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			HandleDraftTicketResponse(isConfirmed: false);
		});
		m_plusSign.SetActive(value: false);
		AddHideListener(OnHideComplete);
	}

	public void SetInfo(Info info)
	{
		m_info = info;
	}

	public override void Show()
	{
		Vector3 popUpScale = base.transform.localScale;
		base.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
		EnableFullScreenEffects(enable: true);
		base.Show();
		bool showPlus = false;
		int outstandingTicketCount = m_info.m_outstandingTicketCount;
		if (outstandingTicketCount > 99)
		{
			m_ticketCount.SetText(GameStrings.Get("99"));
			showPlus = true;
		}
		else
		{
			m_ticketCount.SetText(GameStrings.Get(outstandingTicketCount.ToString()));
			showPlus = false;
		}
		GameStrings.PluralNumber[] pluralNumbers = new GameStrings.PluralNumber[1]
		{
			new GameStrings.PluralNumber
			{
				m_index = 0,
				m_number = outstandingTicketCount
			}
		};
		m_description.Text = GameStrings.FormatPlurals("GLUE_OUTSTANDING_DRAFT_TICKET_DIALOG_DESC", pluralNumbers);
		if (m_plusSign != null)
		{
			m_plusSign.SetActive(showPlus);
		}
		if (!string.IsNullOrEmpty(m_showAnimationSound))
		{
			SoundManager.Get().LoadAndPlay(m_showAnimationSound);
		}
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("scale", popUpScale);
		args.Add("time", 0.3f);
		args.Add("easetype", iTween.EaseType.easeOutBack);
		iTween.ScaleTo(base.gameObject, args);
		UniversalInputManager.Get().SetSystemDialogActive(active: true);
	}

	protected void EnableFullScreenEffects(bool enable)
	{
		if (enable)
		{
			ScreenEffectParameters screenEffectParameters = ScreenEffectParameters.BlurVignetteDesaturatePerspective;
			screenEffectParameters.Time = 1f;
			DialogBase.m_screenEffectsHandle.StartEffect(screenEffectParameters);
		}
		else
		{
			DialogBase.m_screenEffectsHandle.StopEffect();
		}
	}

	protected override void DoHideAnimation()
	{
		if (!string.IsNullOrEmpty(m_hideAnimationSound))
		{
			SoundManager.Get().LoadAndPlay(m_hideAnimationSound);
		}
		base.DoHideAnimation();
	}

	private void HandleDraftTicketResponse(bool isConfirmed)
	{
		m_isConfirmed = isConfirmed;
		EnableFullScreenEffects(enable: false);
		Hide();
	}

	private void OnHideComplete(DialogBase dialog, object userdata)
	{
		if (m_isConfirmed)
		{
			m_info?.m_callbackOnEnter?.Invoke();
		}
		else
		{
			m_info?.m_callbackOnCancel?.Invoke();
		}
	}
}
