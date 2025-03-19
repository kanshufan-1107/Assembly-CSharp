using UnityEngine;

public class CollectibleSkin : MonoBehaviour
{
	public GameObject m_favoriteBanner;

	public UberText m_favoriteBannerText;

	public GameObject m_shadow;

	public UberText m_name;

	public GameObject m_nameShadow;

	public UberText m_collectionManagerName;

	private bool m_showName = true;

	public bool ShowName
	{
		get
		{
			return m_showName;
		}
		set
		{
			m_showName = value;
			PopulateNameText();
			if (m_nameShadow != null)
			{
				m_nameShadow.gameObject.SetActive(m_showName && !UniversalInputManager.UsePhoneUI);
			}
		}
	}

	public virtual void Awake()
	{
		Actor actor = base.gameObject.GetComponent<Actor>();
		if (actor != null)
		{
			actor.SetUseShortName(useShortName: true);
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				actor.OverrideNameText(null);
			}
		}
		ShowName = m_showName;
	}

	protected virtual void PopulateNameText()
	{
		Actor component = base.gameObject.GetComponent<Actor>();
		UberText textField = GetActiveNameText();
		component.OverrideNameText(textField);
	}

	public void ShowShadow(bool show)
	{
		if (!(m_shadow == null))
		{
			m_shadow.SetActive(show);
		}
	}

	public virtual void ShowFavoriteBanner(bool show)
	{
		if (!(m_favoriteBanner == null))
		{
			m_favoriteBanner.SetActive(show);
		}
	}

	public void ShowCollectionManagerText()
	{
		Actor actor = base.gameObject.GetComponent<Actor>();
		if (actor != null)
		{
			PopulateNameText();
			if (actor.isMissingCard())
			{
				actor.UpdateMissingCardArt();
			}
		}
	}

	protected UberText GetActiveNameText()
	{
		if (!m_showName)
		{
			return null;
		}
		if (!UniversalInputManager.UsePhoneUI)
		{
			return m_name;
		}
		return m_collectionManagerName;
	}

	[ContextMenu("Toggle Missing Card Effect")]
	private void ToggleMissingCardEffect()
	{
		Actor actor = base.gameObject.GetComponent<Actor>();
		if (actor != null)
		{
			if (actor.isMissingCard())
			{
				actor.DisableMissingCardEffect();
			}
			else
			{
				actor.MissingCardEffect();
			}
			actor.UpdateAllComponents();
		}
	}
}
