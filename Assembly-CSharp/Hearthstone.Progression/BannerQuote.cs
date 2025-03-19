using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

public class BannerQuote : MonoBehaviour, IWidgetEventListener
{
	[SerializeField]
	private WeakAssetReference m_bannerQuotePrefab;

	[SerializeField]
	private string m_text;

	[SerializeField]
	private WeakAssetReference m_voiceoverAsset;

	[SerializeField]
	private float m_durationSeconds;

	[SerializeField]
	private CanvasAnchor m_anchorPoint = CanvasAnchor.BOTTOM_LEFT;

	[SerializeField]
	private bool m_allowRepeatDuringSession = true;

	[SerializeField]
	private bool m_blockAllOtherInput;

	private const string CODE_SHOW_QUOTE = "CODE_SHOW_QUOTE";

	private WidgetTemplate m_widget;

	private Notification m_quote;

	public WidgetTemplate OwningWidget => m_widget;

	private void Awake()
	{
		m_widget = GetComponentInParent<WidgetTemplate>();
		if (m_widget != null)
		{
			m_widget.RegisterDeactivatedListener(OnDeactivate);
			return;
		}
		Log.All.PrintWarning("[BannerQuote] ({0}) missing owning widget object", base.name);
	}

	private void OnDeactivate(object unused)
	{
		if (m_quote != null)
		{
			NotificationManager.Get()?.DestroyQuote(m_quote, 0f);
		}
	}

	public WidgetEventListenerResponse EventReceived(string eventName, TriggerEventParameters eventParams)
	{
		if (eventName == "CODE_SHOW_QUOTE")
		{
			ShowQuote();
		}
		return default(WidgetEventListenerResponse);
	}

	public void ShowQuote()
	{
		m_quote = NotificationManager.Get().CreateCharacterQuote(m_bannerQuotePrefab.AssetString, GameStrings.Get(m_text.Trim()), m_voiceoverAsset.AssetString, m_allowRepeatDuringSession, m_durationSeconds, m_anchorPoint, m_blockAllOtherInput);
	}
}
