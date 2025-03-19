using UnityEngine;

public class BeveledButton : PegUIElement
{
	public TextMesh m_label;

	public UberText m_uberLabel;

	public GameObject m_highlight;

	protected override void Awake()
	{
		base.Awake();
		SetOriginalLocalPosition();
		m_highlight.SetActive(value: false);
	}

	protected override void OnPress()
	{
		Vector3 origPos = GetOriginalLocalPosition();
		Vector3 destPos = new Vector3(origPos.x, origPos.y - 0.3f * base.transform.localScale.y, origPos.z);
		iTween.MoveTo(base.gameObject, iTween.Hash("position", destPos, "islocal", true, "time", 0.15f));
	}

	protected override void OnRelease()
	{
		iTween.MoveTo(base.gameObject, iTween.Hash("position", GetOriginalLocalPosition(), "islocal", true, "time", 0.15f));
	}

	protected override void OnOver(InteractionState oldState)
	{
		Vector3 origPos = GetOriginalLocalPosition();
		Vector3 destPos = new Vector3(origPos.x, origPos.y + 0.5f * base.transform.localScale.y, origPos.z);
		iTween.MoveTo(base.gameObject, iTween.Hash("position", destPos, "islocal", true, "time", 0.15f));
		m_highlight.SetActive(value: true);
	}

	protected override void OnOut(InteractionState oldState)
	{
		iTween.MoveTo(base.gameObject, iTween.Hash("position", GetOriginalLocalPosition(), "islocal", true, "time", 0.15f));
		m_highlight.SetActive(value: false);
	}

	public void SetText(string text)
	{
		if (m_uberLabel != null)
		{
			m_uberLabel.Text = text;
		}
		else
		{
			m_label.text = text;
		}
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
		m_highlight.SetActive(value: false);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public UberText GetUberText()
	{
		return m_uberLabel;
	}
}
