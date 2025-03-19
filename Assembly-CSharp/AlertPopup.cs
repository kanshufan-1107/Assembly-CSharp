using System;
using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

[CustomEditClass]
public class AlertPopup : DialogBase
{
	[Serializable]
	public class Header
	{
		public MultiSliceElement m_container;

		public GameObject m_middle;

		public UberText m_text;
	}

	public enum Response
	{
		OK,
		CONFIRM,
		CANCEL
	}

	public enum ResponseDisplay
	{
		NONE,
		OK,
		CONFIRM,
		CANCEL,
		CONFIRM_CANCEL,
		YES_NO
	}

	public delegate void ResponseCallback(Response response, object userData);

	public class PopupInfo
	{
		public enum IconSet
		{
			Default,
			Alternate,
			None
		}

		public UserAttentionBlocker m_attentionCategory = UserAttentionBlocker.ALL_EXCEPT_FATAL_ERROR_SCENE;

		public string m_id;

		public string m_headerText;

		public string m_text;

		public string m_okText;

		public string m_confirmText;

		public string m_cancelText;

		public bool m_showAlertIcon = true;

		public AssetReference m_iconTexture;

		public ResponseDisplay m_responseDisplay = ResponseDisplay.OK;

		public ResponseCallback m_responseCallback;

		public object m_responseUserData;

		public Vector3 m_offset = Vector3.zero;

		public float m_padding;

		public Vector3? m_scaleOverride;

		public bool m_richTextEnabled = true;

		public bool m_disableBlocker;

		public IconSet m_iconSet;

		public UberText.AlignmentOptions m_alertTextAlignment;

		public UberText.AnchorOptions m_alertTextAlignmentAnchor;

		public GameLayer? m_layerToUse;

		public bool m_keyboardEscIsCancel = true;

		public bool m_disableBnetBar;

		public bool m_blurWhenShown;
	}

	public Header m_header;

	public NineSliceElement m_body;

	public GameObject m_alertIcon;

	public MultiSliceElement m_buttonContainer;

	public UIBButton m_okayButton;

	public UIBButton m_confirmButton;

	public UIBButton m_cancelButton;

	public GameObject m_clickCatcher;

	public UberText m_alertText;

	public Vector3 m_alertIconOffset;

	public float m_padding;

	public Vector3 m_loadPosition;

	public Vector3 m_showPosition;

	public List<GameObject> m_buttonIconsSet1 = new List<GameObject>();

	public List<GameObject> m_buttonIconsSet2 = new List<GameObject>();

	private const float BUTTON_FRAME_WIDTH = 80f;

	private PopupInfo m_popupInfo;

	private PopupInfo m_updateInfo;

	private float m_alertTextInitialWidth;

	private bool m_wasPressed;

	private AssetHandle<Texture> m_loadedIconTexture;

	public string BodyText
	{
		get
		{
			return m_alertText.Text;
		}
		set
		{
			m_alertText.Text = value;
			if (m_popupInfo != null)
			{
				UpdateLayout();
			}
		}
	}

	protected override void Awake()
	{
		m_alertTextInitialWidth = m_alertText.Width;
		base.Awake();
		m_okayButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			ButtonPress(Response.OK);
		});
		m_confirmButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			ButtonPress(Response.CONFIRM);
		});
		m_cancelButton.AddEventListener(UIEventType.RELEASE, delegate
		{
			ButtonPress(Response.CANCEL);
		});
	}

	private void Start()
	{
		if (string.IsNullOrEmpty(m_alertText.Text))
		{
			m_alertText.Text = GameStrings.Get("GLOBAL_OKAY");
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (UniversalInputManager.Get() != null)
		{
			UniversalInputManager.Get().SetSystemDialogActive(active: false);
		}
		AssetHandle.SafeDispose(ref m_loadedIconTexture);
	}

	public override bool HandleKeyboardInput()
	{
		if (InputCollection.GetKeyUp(KeyCode.Escape) && m_popupInfo != null && m_popupInfo.m_keyboardEscIsCancel && m_cancelButton.enabled && m_cancelButton.gameObject.activeSelf)
		{
			GoBack();
			return true;
		}
		return false;
	}

	public override void GoBack()
	{
		ButtonPress(Response.CANCEL);
	}

	public void SetInfo(PopupInfo info)
	{
		m_popupInfo = info;
	}

	public PopupInfo GetInfo()
	{
		return m_popupInfo;
	}

	public override void Show()
	{
		base.Show();
		InitInfo();
		UpdateAll(m_popupInfo);
		base.transform.localPosition += m_popupInfo.m_offset;
		if (m_popupInfo.m_layerToUse.HasValue)
		{
			LayerUtils.SetLayer(this, m_popupInfo.m_layerToUse.Value);
		}
		if (m_popupInfo.m_disableBlocker)
		{
			m_clickCatcher.SetActive(value: false);
		}
		if (m_popupInfo.m_disableBnetBar)
		{
			BnetBar.Get().DisableButtonsByDialog(this);
		}
		if (m_popupInfo.m_blurWhenShown)
		{
			DialogBase.DoBlur();
		}
		DoShowAnimation();
		bool isSystemDialog = m_popupInfo == null || !m_popupInfo.m_layerToUse.HasValue || m_popupInfo.m_layerToUse.Value == GameLayer.UI || m_popupInfo.m_layerToUse.Value == GameLayer.HighPriorityUI;
		UniversalInputManager.Get().SetSystemDialogActive(isSystemDialog);
	}

	public override void Hide()
	{
		base.Hide();
		if (m_popupInfo.m_blurWhenShown)
		{
			DialogBase.EndBlur();
		}
	}

	public void UpdateInfo(PopupInfo info)
	{
		m_updateInfo = info;
		UpdateButtons(m_updateInfo.m_responseDisplay);
		if (m_showAnimState != ShowAnimState.IN_PROGRESS)
		{
			UpdateInfoAfterAnim();
		}
	}

	protected override void OnHideAnimFinished()
	{
		UniversalInputManager.Get().SetSystemDialogActive(active: false);
		base.OnHideAnimFinished();
	}

	protected override void OnShowAnimFinished()
	{
		base.OnShowAnimFinished();
		if (m_updateInfo != null)
		{
			UpdateInfoAfterAnim();
		}
	}

	private void InitInfo()
	{
		if (m_popupInfo == null)
		{
			m_popupInfo = new PopupInfo();
		}
		if (m_popupInfo.m_headerText == null)
		{
			m_popupInfo.m_headerText = GameStrings.Get("GLOBAL_DEFAULT_ALERT_HEADER");
		}
	}

	private void UpdateButtons(ResponseDisplay displayType)
	{
		m_confirmButton.gameObject.SetActive(value: false);
		m_cancelButton.gameObject.SetActive(value: false);
		m_okayButton.gameObject.SetActive(value: false);
		switch (displayType)
		{
		case ResponseDisplay.OK:
			m_okayButton.gameObject.SetActive(value: true);
			break;
		case ResponseDisplay.CONFIRM:
			m_confirmButton.gameObject.SetActive(value: true);
			break;
		case ResponseDisplay.CANCEL:
			m_cancelButton.gameObject.SetActive(value: true);
			break;
		case ResponseDisplay.CONFIRM_CANCEL:
			m_confirmButton.gameObject.SetActive(value: true);
			m_confirmButton.SetText("GLOBAL_CONFIRM");
			m_cancelButton.gameObject.SetActive(value: true);
			m_cancelButton.SetText("GLOBAL_CANCEL");
			break;
		case ResponseDisplay.YES_NO:
			m_confirmButton.gameObject.SetActive(value: true);
			m_confirmButton.SetText("GLUE_COLLECTION_DECK_COMPLETE_POPUP_CONFIRM");
			m_cancelButton.gameObject.SetActive(value: true);
			m_cancelButton.SetText("GLUE_COLLECTION_DECK_COMPLETE_POPUP_CANCEL");
			break;
		}
		m_buttonContainer.UpdateSlices();
	}

	private void UpdateTexts(PopupInfo popupInfo)
	{
		m_alertText.RichText = m_popupInfo.m_richTextEnabled;
		m_alertText.Alignment = m_popupInfo.m_alertTextAlignment;
		m_alertText.Anchor = m_popupInfo.m_alertTextAlignmentAnchor;
		if (popupInfo.m_headerText == null)
		{
			popupInfo.m_headerText = GameStrings.Get("GLOBAL_DEFAULT_ALERT_HEADER");
		}
		m_alertText.Text = popupInfo.m_text;
		m_okayButton.SetText((popupInfo.m_okText == null) ? GameStrings.Get("GLOBAL_OKAY") : popupInfo.m_okText);
		m_confirmButton.SetText((popupInfo.m_confirmText == null) ? GameStrings.Get("GLOBAL_CONFIRM") : popupInfo.m_confirmText);
		m_cancelButton.SetText((popupInfo.m_cancelText == null) ? GameStrings.Get("GLOBAL_CANCEL") : popupInfo.m_cancelText);
	}

	private void UpdateIcons(PopupInfo popupInfo)
	{
		m_alertIcon.SetActive(popupInfo.m_showAlertIcon);
		if (AssetLoader.Get() != null && !string.IsNullOrEmpty(popupInfo.m_iconTexture))
		{
			MeshRenderer alertIconMesh = m_alertIcon.GetComponent<MeshRenderer>();
			if (alertIconMesh != null)
			{
				AssetLoader.Get().LoadAsset(ref m_loadedIconTexture, popupInfo.m_iconTexture);
				if (m_loadedIconTexture != null)
				{
					alertIconMesh.GetMaterial().SetTexture("_MainTex", m_loadedIconTexture);
				}
			}
		}
		bool useFirstIconSet = popupInfo.m_iconSet == PopupInfo.IconSet.Default;
		bool useSecondIconSet = popupInfo.m_iconSet == PopupInfo.IconSet.Alternate;
		for (int i = 0; i < m_buttonIconsSet1.Count; i++)
		{
			m_buttonIconsSet1[i].SetActive(useFirstIconSet);
		}
		for (int j = 0; j < m_buttonIconsSet2.Count; j++)
		{
			m_buttonIconsSet2[j].SetActive(useSecondIconSet);
		}
	}

	private void UpdateInfoAfterAnim()
	{
		m_popupInfo = m_updateInfo;
		m_updateInfo = null;
		UpdateAll(m_popupInfo);
	}

	private void UpdateAll(PopupInfo popupInfo)
	{
		UpdateIcons(popupInfo);
		UpdateHeaderText(popupInfo.m_headerText);
		UpdateTexts(popupInfo);
		UpdateLayout();
	}

	private void UpdateLayout()
	{
		bool activeSelf = m_alertIcon.activeSelf;
		Bounds bounds = m_alertText.GetTextBounds();
		float width = bounds.size.x;
		float height = bounds.size.y + m_padding + m_popupInfo.m_padding;
		float iconAddWidth = 0f;
		float iconHeight = 0f;
		if (activeSelf)
		{
			OrientedBounds orientedBounds = TransformUtil.ComputeOrientedWorldBounds(m_alertIcon);
			iconAddWidth = orientedBounds.Extents[0].magnitude * 2f;
			iconHeight = orientedBounds.Extents[1].magnitude * 2f;
		}
		UpdateButtons(m_popupInfo.m_responseDisplay);
		width = Mathf.Max(TransformUtil.GetBoundsOfChildren(m_confirmButton).size.x * 2f, width);
		m_body.SetSize(width + iconAddWidth, Mathf.Max(height, iconHeight));
		Vector3 offset = new Vector3(0f, 0.01f, 0f);
		TransformUtil.SetPoint(m_alertIcon, Anchor.TOP_LEFT_XZ, m_body.m_middle, Anchor.TOP_LEFT_XZ, offset);
		m_alertIcon.transform.localPosition += m_alertIconOffset;
		Anchor textAnchor = Anchor.TOP_LEFT_XZ;
		if (m_popupInfo.m_alertTextAlignment == UberText.AlignmentOptions.Center)
		{
			textAnchor = Anchor.TOP_XZ;
			if (m_popupInfo.m_showAlertIcon)
			{
				textAnchor = Anchor.TOP_LEFT_XZ;
			}
		}
		if (m_alertText.Anchor == UberText.AnchorOptions.Middle)
		{
			switch (textAnchor)
			{
			case Anchor.TOP_LEFT_XZ:
				textAnchor = Anchor.LEFT_XZ;
				break;
			case Anchor.TOP_XZ:
				textAnchor = Anchor.CENTER_XZ;
				break;
			case Anchor.TOP_RIGHT_XZ:
				textAnchor = Anchor.RIGHT_XZ;
				break;
			}
		}
		TransformUtil.SetPoint(m_alertText, textAnchor, m_body.m_middle, textAnchor, offset);
		Vector3 setTextPos = m_alertText.transform.position;
		setTextPos.x += iconAddWidth + m_alertIconOffset.x;
		setTextPos.y += 1f;
		m_alertText.transform.position = setTextPos;
		if (m_popupInfo.m_alertTextAlignment == UberText.AlignmentOptions.Center)
		{
			m_alertText.Width = m_alertTextInitialWidth - iconAddWidth * m_alertText.transform.localScale.x;
		}
		m_header.m_container.transform.position = m_body.m_top.m_slice.transform.position;
		m_buttonContainer.transform.position = m_body.m_bottom.m_slice.transform.position;
		if (m_popupInfo.m_scaleOverride.HasValue)
		{
			m_originalScale = m_popupInfo.m_scaleOverride.Value;
		}
	}

	private void ButtonPress(Response response)
	{
		if (!m_wasPressed)
		{
			m_wasPressed = true;
			if (m_popupInfo.m_responseCallback != null)
			{
				m_popupInfo.m_responseCallback(response, m_popupInfo.m_responseUserData);
			}
			Hide();
		}
	}

	private void UpdateHeaderText(string text)
	{
		bool emptyHeader = string.IsNullOrEmpty(text);
		m_header.m_container.gameObject.SetActive(!emptyHeader);
		if (!emptyHeader)
		{
			m_header.m_text.ResizeToFit = false;
			m_header.m_text.Text = text;
			m_header.m_text.UpdateNow();
			MeshRenderer middle = m_body.m_middle.m_slice.GetComponent<MeshRenderer>();
			float headerTextWidthWorld = m_header.m_text.GetTextBounds().size.x;
			float headerTextWidth = m_header.m_text.transform.worldToLocalMatrix.MultiplyVector(m_header.m_text.GetTextBounds().size).x;
			float maxHeaderWidth = 0.8f * m_header.m_text.transform.worldToLocalMatrix.MultiplyVector(middle.GetComponent<Renderer>().bounds.size).x;
			if (headerTextWidth > maxHeaderWidth)
			{
				m_header.m_text.Width = maxHeaderWidth;
				m_header.m_text.ResizeToFit = true;
				m_header.m_text.UpdateNow();
				headerTextWidthWorld = m_header.m_text.GetTextBounds().size.x;
			}
			else
			{
				m_header.m_text.Width = headerTextWidth;
			}
			TransformUtil.SetLocalScaleToWorldDimension(m_header.m_middle, new WorldDimensionIndex(headerTextWidthWorld, 0));
			m_header.m_container.UpdateSlices();
		}
	}
}
