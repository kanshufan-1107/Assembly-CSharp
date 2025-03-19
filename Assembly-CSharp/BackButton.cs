using UnityEngine;

public class BackButton : PegUIElement
{
	public GameObject m_highlight;

	public UberText m_backText;

	public static KeyCode backKey;

	protected override void Awake()
	{
		base.Awake();
		SetOriginalLocalPosition();
		m_highlight.SetActive(value: false);
		if ((bool)m_backText)
		{
			m_backText.Text = GameStrings.Get("GLOBAL_BACK");
		}
	}

	protected override void OnPress()
	{
		Vector3 origPos = GetOriginalLocalPosition();
		Vector3 destPos = new Vector3(origPos.x, origPos.y - 0.3f, origPos.z);
		iTween.MoveTo(base.gameObject, iTween.Hash("position", destPos, "islocal", true, "time", 0.15f));
	}

	protected override void OnRelease()
	{
		iTween.MoveTo(base.gameObject, iTween.Hash("position", GetOriginalLocalPosition(), "islocal", true, "time", 0.15f));
	}

	protected override void OnOver(InteractionState oldState)
	{
		Vector3 origPos = GetOriginalLocalPosition();
		Vector3 destPos = new Vector3(origPos.x, origPos.y + 0.5f, origPos.z);
		iTween.MoveTo(base.gameObject, iTween.Hash("position", destPos, "islocal", true, "time", 0.15f));
		m_highlight.SetActive(value: true);
	}

	protected override void OnOut(InteractionState oldState)
	{
		iTween.MoveTo(base.gameObject, iTween.Hash("position", GetOriginalLocalPosition(), "islocal", true, "time", 0.15f));
		m_highlight.SetActive(value: false);
	}
}
