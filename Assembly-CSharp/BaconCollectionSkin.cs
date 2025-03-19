using UnityEngine;

public class BaconCollectionSkin : CollectibleSkin
{
	protected bool m_showingFavorited;

	public GameObject m_nameWrapper;

	public GameObject m_phoneUINameWrapper;

	public GameObject m_favoriteStateTextWrapper;

	public UberText m_favoriteStateUberText;

	public GameObject m_favoriteNameBackground;

	public GameObject m_nonFavoriteNameBackground;

	public GameObject m_favoriteIcon;

	protected virtual string GetFavoritedText()
	{
		return GameStrings.Get("GLUE_BACON_COLLECTION_FAVORITE");
	}

	public override void ShowFavoriteBanner(bool show)
	{
		m_showingFavorited = show;
		PopulateNameText();
	}

	protected override void PopulateNameText()
	{
		if (null != m_favoriteNameBackground)
		{
			m_favoriteNameBackground.SetActive(m_showingFavorited && base.ShowName);
		}
		if (null != m_nonFavoriteNameBackground)
		{
			m_nonFavoriteNameBackground.SetActive(!m_showingFavorited && base.ShowName);
		}
		if (m_showingFavorited)
		{
			ShowFavoriteUI();
			return;
		}
		HideFavoriteUI();
		base.PopulateNameText();
	}

	protected virtual void ShowFavoriteUI()
	{
		GameObject platformNameWrapper = GetActiveNameWrapper();
		if (platformNameWrapper != null)
		{
			platformNameWrapper.SetActive(value: false);
		}
		if (m_favoriteStateTextWrapper != null)
		{
			m_favoriteStateTextWrapper.SetActive(value: true);
		}
		if (m_favoriteIcon != null)
		{
			m_favoriteIcon.SetActive(value: false);
		}
		if (m_favoriteStateUberText != null)
		{
			m_favoriteStateUberText.Text = GetFavoritedText();
		}
	}

	protected virtual void HideFavoriteUI()
	{
		if (m_favoriteIcon != null)
		{
			m_favoriteIcon.SetActive(value: false);
		}
		GameObject platformNameWrapper = GetActiveNameWrapper();
		if (platformNameWrapper != null)
		{
			platformNameWrapper.SetActive(value: true);
		}
		if (m_favoriteStateTextWrapper != null)
		{
			m_favoriteStateTextWrapper.SetActive(value: false);
		}
	}

	protected GameObject GetActiveNameWrapper()
	{
		if (!UniversalInputManager.UsePhoneUI)
		{
			return m_nameWrapper;
		}
		return m_phoneUINameWrapper;
	}
}
