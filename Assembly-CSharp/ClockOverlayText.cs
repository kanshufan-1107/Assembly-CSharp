using System;
using System.Collections;
using UnityEngine;

public class ClockOverlayText : MonoBehaviour
{
	[SerializeField]
	private GameObject m_bannerStandard;

	[SerializeField]
	private GameObject m_bannerYear;

	[SerializeField]
	private UberText m_detailsText;

	[SerializeField]
	private UberText m_detailsTextStandard;

	[SerializeField]
	private UberText m_bannerHeadlineText;

	[SerializeField]
	private Vector3 m_maxScale;

	[SerializeField]
	private Vector3 m_maxScale_phone;

	private Vector3 m_minScale = new Vector3(0.01f, 0.01f, 0.01f);

	public void Show()
	{
		Vector3 scale = m_maxScale;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			scale = m_maxScale_phone;
		}
		iTween.Stop(base.gameObject);
		base.gameObject.SetActive(value: true);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("scale", scale);
		args.Add("time", 0.4f);
		args.Add("easetype", iTween.EaseType.easeOutQuad);
		iTween.ScaleTo(base.gameObject, args);
		if (m_bannerHeadlineText == null)
		{
			Debug.LogError("ClockOverlayText: Banner Headline Text is not set!");
		}
		else
		{
			m_bannerHeadlineText.Text = SetRotationManager.Get().GetActiveSetRotationYearLocalizedString();
		}
	}

	public void Hide()
	{
		iTween.Stop(base.gameObject);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("scale", m_minScale);
		args.Add("time", 0.15f);
		args.Add("easetype", iTween.EaseType.easeInQuad);
		args.Add("oncomplete", (Action<object>)delegate
		{
			base.gameObject.SetActive(value: false);
		});
		iTween.ScaleTo(base.gameObject, args);
	}

	public void HideImmediate()
	{
		base.gameObject.SetActive(value: false);
		base.transform.localScale = m_minScale;
	}

	public void UpdateText(int step)
	{
		if (step == 0)
		{
			m_bannerYear.SetActive(value: false);
			m_detailsText.gameObject.SetActive(value: false);
			m_bannerStandard.SetActive(value: true);
			m_detailsTextStandard.gameObject.SetActive(value: true);
		}
		else
		{
			m_bannerStandard.SetActive(value: false);
			m_detailsTextStandard.gameObject.SetActive(value: false);
			m_bannerYear.SetActive(value: true);
			m_detailsText.gameObject.SetActive(value: true);
		}
	}
}
