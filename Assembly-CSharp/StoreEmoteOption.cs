using Hearthstone.UI;
using UnityEngine;

public class StoreEmoteOption : MonoBehaviour
{
	[SerializeField]
	private StoreEmoteHandler m_emoteHandler;

	[SerializeField]
	private EmoteType m_EmoteType;

	[SerializeField]
	private GameObject m_bubbleMeshObj;

	[SerializeField]
	private GameObject m_uberTextObj;

	[SerializeField]
	private AsyncReference m_asyncWidgetClickableReference;

	private Vector3 m_startingScale;

	private bool m_shouldBeShowing;

	private bool m_isInitialized;

	private void Awake()
	{
		Initialize();
	}

	private void Initialize()
	{
		if (!m_isInitialized)
		{
			if (m_emoteHandler == null)
			{
				Debug.LogError("StoreEmoteOption: Missing a required reference to an StoreEmoteHandler component");
			}
			if (m_asyncWidgetClickableReference == null)
			{
				Debug.LogError("StoreEmoteOption: Missing a required AsyncReference to an clickable widget component");
			}
			if (m_bubbleMeshObj != null)
			{
				m_bubbleMeshObj.SetActive(value: false);
			}
			if (m_uberTextObj != null)
			{
				m_uberTextObj.SetActive(value: false);
			}
			m_startingScale = base.transform.localScale;
			base.transform.localScale = Vector3.zero;
			m_isInitialized = true;
		}
	}

	private void Start()
	{
		m_asyncWidgetClickableReference.RegisterReadyListener<Clickable>(OnWidgetClickableReady);
	}

	public void Enable()
	{
		Initialize();
		m_shouldBeShowing = true;
		if (m_bubbleMeshObj != null)
		{
			m_bubbleMeshObj.SetActive(value: true);
		}
		if (m_uberTextObj != null)
		{
			m_uberTextObj.SetActive(value: true);
		}
		iTween.Stop(base.gameObject);
		base.transform.localScale = Vector3.zero;
		iTween.ScaleTo(base.gameObject, iTween.Hash("scale", m_startingScale, "time", 0.5f, "ignoretimescale", true, "easetype", iTween.EaseType.easeOutElastic));
	}

	public void Disable(bool isImmediateHide = false)
	{
		Initialize();
		m_shouldBeShowing = false;
		iTween.Stop(base.gameObject);
		if (isImmediateHide)
		{
			if (m_bubbleMeshObj != null)
			{
				m_bubbleMeshObj.SetActive(value: false);
			}
			if (m_uberTextObj != null)
			{
				m_uberTextObj.SetActive(value: false);
			}
			base.transform.localScale = Vector3.zero;
		}
		else
		{
			iTween.ScaleTo(base.gameObject, iTween.Hash("scale", Vector3.zero, "time", 0.1f, "ignoretimescale", true, "easetype", iTween.EaseType.linear, "oncompletetarget", base.gameObject, "oncomplete", "OnFinishHiding"));
		}
	}

	private void OnFinishHiding()
	{
		if (!m_shouldBeShowing)
		{
			if (m_bubbleMeshObj != null)
			{
				m_bubbleMeshObj.SetActive(value: false);
			}
			if (m_uberTextObj != null)
			{
				m_uberTextObj.SetActive(value: false);
			}
			base.transform.localScale = Vector3.zero;
		}
	}

	private void OnWidgetClickableReady(Clickable widgetClickable)
	{
		if (widgetClickable == null)
		{
			Debug.LogError("StoreEmoteOption: Failed to load clickable by reference.");
			return;
		}
		widgetClickable.AddEventListener(UIEventType.RELEASE, OnClickableTriggered);
		widgetClickable.AddEventListener(UIEventType.ROLLOUT, OnClickableMouseOut);
		widgetClickable.AddEventListener(UIEventType.ROLLOVER, OnClickableMouseOver);
	}

	private void OnClickableMouseOut(UIEvent e)
	{
		if (e != null && e.GetEventType() == UIEventType.ROLLOUT)
		{
			iTween.ScaleTo(base.gameObject, iTween.Hash("scale", m_startingScale, "time", 0.2f, "ignoretimescale", true));
		}
	}

	private void OnClickableMouseOver(UIEvent e)
	{
		if (e != null && e.GetEventType() == UIEventType.ROLLOVER)
		{
			iTween.ScaleTo(base.gameObject, iTween.Hash("scale", m_startingScale * 1.1f, "time", 0.2f, "ignoretimescale", true));
		}
	}

	private void OnClickableTriggered(UIEvent e)
	{
		if (e != null && e.GetEventType() == UIEventType.RELEASE && m_EmoteType != 0)
		{
			if (m_emoteHandler == null)
			{
				Debug.LogError("StoreEmoteOption: Failed to trigger emote " + m_EmoteType.ToString() + " as missing StoreEmoteHandler reference.");
			}
			else
			{
				m_emoteHandler.PlayEmote(m_EmoteType);
			}
		}
	}
}
