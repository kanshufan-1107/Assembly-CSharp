using System.Collections;
using UnityEngine;

[CustomEditClass]
public class UIBInfoButton : PegUIElement
{
	private const float RAISE_TIME = 0.1f;

	private const float DEPRESS_TIME = 0.1f;

	[SerializeField]
	[CustomEditField(Sections = "Button Objects")]
	public GameObject m_RootObject;

	[CustomEditField(Sections = "Button Objects")]
	[SerializeField]
	public Transform m_UpBone;

	[SerializeField]
	[CustomEditField(Sections = "Button Objects")]
	public Transform m_DownBone;

	[SerializeField]
	[CustomEditField(Sections = "Highlight")]
	public GameObject m_Highlight;

	private UIBHighlight m_UIBHighlight;

	protected override void Awake()
	{
		base.Awake();
		UIBHighlight highlight = GetComponent<UIBHighlight>();
		if (highlight == null)
		{
			highlight = base.gameObject.AddComponent<UIBHighlight>();
		}
		m_UIBHighlight = highlight;
		if (m_UIBHighlight != null)
		{
			m_UIBHighlight.m_MouseOverHighlight = m_Highlight;
			m_UIBHighlight.m_HideMouseOverOnPress = false;
		}
	}

	public void Select()
	{
		Depress();
	}

	public void Deselect()
	{
		Raise();
	}

	private void Raise()
	{
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("position", m_UpBone.localPosition);
		args.Add("time", 0.1f);
		args.Add("easetype", iTween.EaseType.linear);
		args.Add("islocal", true);
		iTween.MoveTo(m_RootObject, args);
	}

	private void Depress()
	{
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("position", m_DownBone.localPosition);
		args.Add("time", 0.1f);
		args.Add("easetype", iTween.EaseType.linear);
		args.Add("islocal", true);
		iTween.MoveTo(m_RootObject, args);
	}
}
