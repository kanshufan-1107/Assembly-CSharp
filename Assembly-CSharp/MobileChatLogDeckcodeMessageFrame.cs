using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate), typeof(WidgetTransform))]
public class MobileChatLogDeckcodeMessageFrame : MobileChatLogMessageFrame
{
	private const float DeckClassIconOffset = 50f;

	[SerializeField]
	private GameObject m_iconGameObject;

	[SerializeField]
	private GameObject m_defaultIcon;

	[SerializeField]
	private GameObject m_mercenariesIcon;

	[SerializeField]
	private string m_hintColor;

	[SerializeField]
	private string m_hintSize;

	private WidgetTemplate m_widget;

	private WidgetTransform m_widgetTransform;

	public string DeckName { get; set; }

	public string DeckcodeString { get; set; }

	public override float Width
	{
		get
		{
			return base.LocalBounds.size.x;
		}
		set
		{
			float offset = value / 2f;
			Vector3 center = base.transform.localPosition;
			m_widgetTransform.Left = center.x - offset;
			m_widgetTransform.Right = center.x + offset;
		}
	}

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widgetTransform = GetComponent<WidgetTransform>();
		m_widget.RegisterReadyListener(OnDeckcodeMessageFrameReady);
		m_widget.RegisterEventListener(OnClickEventListener);
	}

	private void Start()
	{
		UpdateLocalBounds();
	}

	private void OnDeckcodeMessageFrameReady(object _)
	{
		m_widget.SetLayerOverride(GameLayer.BattleNetChat);
		UpdateLocalBounds();
	}

	private void UpdateIconPosition()
	{
		Vector3 iconPosition = base.transform.position;
		Bounds widgetBounds = WidgetTransform.GetWorldBoundsOfWidgetTransforms(base.transform);
		iconPosition.x = base.transform.position.x - widgetBounds.extents.x + 6.25f;
		m_iconGameObject.transform.position = iconPosition;
	}

	public void BindClassData(ShareableDeck shareableDeck)
	{
		DeckName = shareableDeck.DeckName;
		UpdateIconPosition();
		string copyHintRaw = (UniversalInputManager.Get().IsTouchMode() ? GameStrings.Get("GLOBAL_CHAT_DECK_CODE_HINT_TOUCH") : GameStrings.Get("GLOBAL_CHAT_DECK_CODE_HINT"));
		string copyHint = "<color=" + m_hintColor + "><size=" + m_hintSize + ">" + copyHintRaw + "</size></color>";
		if (shareableDeck is ShareableMercenariesTeam)
		{
			m_mercenariesIcon.SetActive(value: true);
			base.Message = ((!string.IsNullOrWhiteSpace(DeckName)) ? GameStrings.Format("GLOBAL_CHAT_MERCENARIES_PARTY_CODE_WITH_NAME_MESSAGE", DeckName, copyHint) : GameStrings.Format("GLOBAL_CHAT_MERCENARIES_PARTY_CODE_MESSAGE", copyHint));
			return;
		}
		TAG_CLASS heroClass = ShareableDeck.ExtractClassFromDeck(shareableDeck);
		if (heroClass == TAG_CLASS.INVALID)
		{
			m_defaultIcon.SetActive(value: true);
		}
		PrototypeDataModel dataModel = new PrototypeDataModel();
		dataModel.String1 = heroClass.ToString();
		m_widget.BindDataModel(dataModel);
		string className = ((heroClass == TAG_CLASS.INVALID) ? string.Empty : GameStrings.GetClassName(heroClass));
		base.Message = ((!string.IsNullOrWhiteSpace(DeckName)) ? GameStrings.Format("GLOBAL_CHAT_DECK_CODE_WITH_NAME_MESSAGE", className, DeckName, copyHint) : GameStrings.Format("GLOBAL_CHAT_DECK_CODE_MESSAGE", className, copyHint));
	}

	private void OnClickEventListener(string eventName)
	{
		if (eventName == "BUTTON_CLICKED")
		{
			ClipboardUtils.CopyToClipboard(ShareableDeck.GenerateDeckCodeMessage(DeckcodeString, DeckName));
			UIStatus.Get().AddInfo(GameStrings.Get("GLUE_COLLECTION_DECK_COPIED_TOAST"));
		}
	}

	public override void RebuildUberText()
	{
		Bounds textBounds = default(Bounds);
		textBounds.extents = new Vector3((m_widgetTransform.Right - m_widgetTransform.Left) / 2f - 50f, (m_widgetTransform.Top - m_widgetTransform.Bottom) / 2f, base.LocalBounds.extents.z);
		textBounds.center = new Vector3(text.transform.localPosition.x + 25f, text.transform.localPosition.y, text.transform.localPosition.z);
		if (text.TryGetComponent<WidgetTransform>(out var m_textWidgetTransform))
		{
			m_textWidgetTransform.Left = textBounds.min.x;
			m_textWidgetTransform.Right = textBounds.max.x;
			m_textWidgetTransform.Bottom = textBounds.min.y;
			m_textWidgetTransform.Top = textBounds.max.y;
		}
		text.UpdateNow(updateIfInactive: true);
	}

	public override void OnPositionUpdate()
	{
		UpdateLocalBounds();
	}

	public override void UpdateLocalBounds()
	{
		float widthOffset = (m_widgetTransform.Right - m_widgetTransform.Left) / 2f;
		float heightOffset = (m_widgetTransform.Top - m_widgetTransform.Bottom) / 2f;
		Bounds newLocalBounds = default(Bounds);
		newLocalBounds.center = Vector3.zero;
		newLocalBounds.extents = new Vector3(widthOffset, heightOffset, 0f);
		m_widgetTransform.Right = widthOffset;
		m_widgetTransform.Left = 0f - widthOffset;
		m_widgetTransform.Top = heightOffset;
		m_widgetTransform.Bottom = 0f - heightOffset;
		if (m_widget.TryGetComponent<BoxCollider>(out var collider))
		{
			collider.center = new Vector3(0f, 0f, -0.15f);
		}
		RebuildUberText();
		base.LocalBounds = newLocalBounds;
		UpdateIconPosition();
	}
}
