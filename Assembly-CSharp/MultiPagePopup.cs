using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

[CustomEditClass]
public class MultiPagePopup : DialogBase
{
	public enum PageType
	{
		CARD_LIST,
		DUST_JAR
	}

	public class PageInfo
	{
		public PageType m_pageType;

		public string m_customPrefabAssetRef;

		public string m_headerText;

		public string m_bodyText;

		public string m_footerText;

		public List<int> m_cards;

		public int m_dustAmount;
	}

	public class Info
	{
		public HideCallback m_callbackOnHide;

		public bool m_blurWhenShown;

		public List<PageInfo> m_pages = new List<PageInfo>();
	}

	private readonly Map<PageType, string> m_pagePrefabRefs = new Map<PageType, string>
	{
		{
			PageType.CARD_LIST,
			"CardListPage.prefab:e48c89787318c4d49bd21abc51901bf8"
		},
		{
			PageType.DUST_JAR,
			"DustJarPage.prefab:9d96713c54a11764691eb73236976680"
		}
	};

	[CustomEditField(Sections = "Sounds", T = EditType.SOUND_PREFAB)]
	public string m_showAnimationSound = "Expand_Up.prefab:775d97ea42498c044897f396362b9db3";

	[CustomEditField(Sections = "Sounds", T = EditType.SOUND_PREFAB)]
	public string m_hideAnimationSound = "Shrink_Down_Quicker.prefab:2fe963b171811ca4b8d544fa53e3330c";

	private Info m_info = new Info();

	private int m_currentPageIdx;

	private Map<int, GameObject> m_pageObjects = new Map<int, GameObject>();

	private int m_numPagesLoaded;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (UniversalInputManager.Get() != null)
		{
			UniversalInputManager.Get().SetSystemDialogActive(active: false);
		}
	}

	public void SetInfo(Info info)
	{
		m_info = info;
		if (m_info.m_callbackOnHide != null)
		{
			AddHideListener(m_info.m_callbackOnHide);
		}
	}

	public override void Show()
	{
		if (m_info.m_blurWhenShown)
		{
			DialogBase.DoBlur();
		}
		UniversalInputManager.Get().SetSystemDialogActive(active: true);
		int idx = 0;
		foreach (PageInfo pageInfo in m_info.m_pages)
		{
			string prefabAssetRef = GetPageAssetRef(pageInfo);
			m_pageObjects[idx] = null;
			AssetLoader.Get().InstantiatePrefab(prefabAssetRef, OnPageLoaded, idx, AssetLoadingOptions.IgnorePrefabPosition);
			idx++;
		}
		StartCoroutine(ShowWhenReady());
	}

	public override void Hide()
	{
		base.Hide();
		if (m_info.m_blurWhenShown)
		{
			DialogBase.EndBlur();
		}
	}

	private string GetPageAssetRef(PageInfo pageInfo)
	{
		if (!string.IsNullOrEmpty(pageInfo.m_customPrefabAssetRef))
		{
			return pageInfo.m_customPrefabAssetRef;
		}
		m_pagePrefabRefs.TryGetValue(pageInfo.m_pageType, out var prefabAssetRef);
		return prefabAssetRef;
	}

	private void OnPageLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		int pageIdx = (int)callbackData;
		m_pageObjects[pageIdx] = go;
		GameUtils.SetParent(go, base.gameObject);
		LayerUtils.SetLayer(go, base.gameObject.layer, null);
		go.SetActive(value: false);
		m_numPagesLoaded++;
	}

	private IEnumerator ShowWhenReady()
	{
		while (m_numPagesLoaded < m_pageObjects.Count)
		{
			yield return null;
		}
		base.Show();
		if (!string.IsNullOrEmpty(m_showAnimationSound))
		{
			SoundManager.Get().LoadAndPlay(m_showAnimationSound);
		}
		Vector3 popUpScale = base.transform.localScale;
		base.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("scale", popUpScale);
		args.Add("time", 0.3f);
		args.Add("easetype", iTween.EaseType.easeOutBack);
		iTween.ScaleTo(base.gameObject, args);
		UniversalInputManager.Get().SetSystemDialogActive(active: true);
		if (!ShowPage(m_currentPageIdx))
		{
			Hide();
		}
	}

	protected override void DoHideAnimation()
	{
		if (!string.IsNullOrEmpty(m_hideAnimationSound))
		{
			SoundManager.Get().LoadAndPlay(m_hideAnimationSound);
		}
		base.DoHideAnimation();
	}

	private void PressNext()
	{
		GameObject currentPage = null;
		if (m_pageObjects.TryGetValue(m_currentPageIdx, out currentPage))
		{
			currentPage.gameObject.SetActive(value: false);
		}
		m_currentPageIdx++;
		if (!ShowPage(m_currentPageIdx))
		{
			Hide();
		}
	}

	private bool ShowPage(int pageIdx)
	{
		if (pageIdx >= m_info.m_pages.Count)
		{
			return false;
		}
		PageInfo pageInfo = m_info.m_pages[pageIdx];
		if (pageInfo == null)
		{
			return false;
		}
		GameObject go = null;
		if (!m_pageObjects.TryGetValue(pageIdx, out go))
		{
			return false;
		}
		MultiPagePopupPage page = go.GetComponent<MultiPagePopupPage>();
		if (page == null)
		{
			return false;
		}
		go.SetActive(value: true);
		page.m_button.AddEventListener(UIEventType.RELEASE, delegate
		{
			PressNext();
		});
		if (pageIdx == m_info.m_pages.Count - 1)
		{
			page.m_buttonText.Text = GameStrings.Get("GLOBAL_DONE");
		}
		else
		{
			page.m_buttonText.Text = GameStrings.Get("GLOBAL_BUTTON_NEXT");
		}
		if (page.m_headerText != null && pageInfo.m_headerText != null)
		{
			page.m_headerText.Text = pageInfo.m_headerText;
		}
		if (page.m_bodyText != null && pageInfo.m_bodyText != null)
		{
			page.m_bodyText.Text = pageInfo.m_bodyText;
		}
		if (page.m_footerText != null && pageInfo.m_footerText != null)
		{
			page.m_footerText.Text = pageInfo.m_footerText;
		}
		CardListPanel cardListPanel = go.GetComponentInChildren<CardListPanel>();
		if (cardListPanel != null)
		{
			cardListPanel.Show(pageInfo.m_cards);
		}
		if (pageInfo.m_dustAmount > 0)
		{
			DustJarPanel dustJarPanel = go.GetComponentInChildren<DustJarPanel>();
			if (dustJarPanel != null)
			{
				dustJarPanel.Show(pageInfo.m_dustAmount);
			}
		}
		return true;
	}
}
