using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class ActiveFilterButton : MonoBehaviour
{
	public SlidingTray m_manaFilterTray;

	public SlidingTray m_setFilterTray;

	public UberText m_searchText;

	public GameObject m_manaFilterIcon;

	public UberText m_manaFilterText;

	public PegUIElement m_activeFilterButton;

	public PegUIElement m_inactiveFilterButton;

	public ManaFilterTabManager m_manaFilter;

	public SetFilterTray m_setFilter;

	public NestedPrefab m_setFilterContainer;

	public CollectionSearch m_search;

	public PegUIElement m_offClickCatcher;

	public UIBButton m_doneButton;

	public Material m_enabledMaterial;

	public Material m_disabledMaterial;

	public MeshRenderer m_inactiveFilterButtonRenderer;

	public GameObject m_inactiveFilterButtonText;

	public Transform m_manaFilterIconCenterBone;

	public Transform m_setFilterIconCenterBone;

	private bool m_filtersShown;

	private bool m_manaFilterActive;

	private string m_manaFilterValue = "";

	private bool m_searchFilterActive;

	private string m_searchFilterValue = "";

	[SerializeField]
	private Transform m_manaFilterIconDefaultBone;

	[SerializeField]
	private Transform m_setFilterIconDefaultBone;

	protected void Awake()
	{
		if (m_inactiveFilterButton != null)
		{
			m_inactiveFilterButton.AddEventListener(UIEventType.RELEASE, delegate
			{
				ShowFilters();
			});
		}
		if (m_activeFilterButton != null)
		{
			m_activeFilterButton.AddEventListener(UIEventType.RELEASE, delegate
			{
				ActiveFilterPressed();
			});
		}
		if (m_doneButton != null)
		{
			m_doneButton.AddEventListener(UIEventType.RELEASE, delegate
			{
				OffClickPressed();
			});
		}
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null)
		{
			cmd.RegisterManaFilterListener(ManaFilterUpdate);
			cmd.RegisterSearchFilterListener(SearchFilterUpdate);
		}
		if (m_manaFilter != null)
		{
			m_manaFilter.OnFilterCleared += ManaFilter_OnFilterCleared;
		}
		CollectibleDisplay cd = CollectionManager.Get()?.GetCollectibleDisplay();
		if (cd != null)
		{
			cd.OnViewModeChanged += OnCollectionManagerViewModeChanged;
		}
	}

	protected void Start()
	{
		if (m_setFilterContainer != null)
		{
			m_setFilter = m_setFilterContainer.PrefabGameObject().GetComponent<SetFilterTray>();
			m_setFilter.m_toggleButton.transform.parent = base.transform;
			if (!UniversalInputManager.UsePhoneUI)
			{
				m_setFilterIconDefaultBone = m_setFilter.m_toggleButton.transform;
			}
		}
		if (!UniversalInputManager.UsePhoneUI)
		{
			m_manaFilterIconDefaultBone = m_manaFilterIcon.transform;
		}
		UpdateFilterView();
	}

	protected void OnDestroy()
	{
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null)
		{
			cmd.UnregisterManaFilterListener(ManaFilterUpdate);
			cmd.UnregisterSearchFilterListener(SearchFilterUpdate);
		}
		if (m_manaFilter != null)
		{
			m_manaFilter.OnFilterCleared -= ManaFilter_OnFilterCleared;
		}
	}

	private void OnCollectionManagerViewModeChanged(CollectionUtils.ViewMode prevMode, CollectionUtils.ViewMode mode, CollectionUtils.ViewModeData userdata, bool triggerResponse)
	{
		if (triggerResponse)
		{
			UpdateFilterView();
		}
	}

	private void ShowFilters()
	{
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null)
		{
			cmd.HideDeckHelpPopup();
		}
		Navigation.Push(HideFilters);
		if (m_manaFilterTray != null)
		{
			m_manaFilterTray.ToggleTraySlider(show: true);
		}
		m_setFilterTray.ToggleTraySlider(show: true);
		m_setFilter.Show(show: true);
		if (m_manaFilter != null)
		{
			m_manaFilter.m_manaCrystalContainer.UpdateSlices();
		}
	}

	private bool HideFilters()
	{
		if (m_manaFilterTray != null)
		{
			m_manaFilterTray.ToggleTraySlider(show: false);
		}
		m_setFilterTray.ToggleTraySlider(show: false);
		CollectibleDisplay cd = CollectionManager.Get().GetCollectibleDisplay();
		if (cd != null)
		{
			cd.m_search.Deactivate();
		}
		m_setFilter.Show(show: false);
		return true;
	}

	private void OffClickPressed()
	{
		Navigation.GoBack();
		UpdateFilterView();
	}

	public void ActiveFilterPressed()
	{
		bool isViewingHeroClass = false;
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd != null)
		{
			isViewingHeroClass = cmd.GetHeroSkinClass().HasValue;
		}
		if (isViewingHeroClass)
		{
			ClearHeroSkinClass();
		}
		else
		{
			ClearFilters();
		}
	}

	public void ClearHeroSkinClass()
	{
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (!(cmd == null))
		{
			cmd.SetHeroSkinClass(null);
			if (!CollectionManager.Get().IsInEditMode())
			{
				cmd.SetViewMode(CollectionUtils.ViewMode.HERO_PICKER);
			}
		}
	}

	public void ClearFilters()
	{
		if (m_manaFilter != null)
		{
			m_manaFilter.ClearFilter(transitionPage: false);
		}
		m_setFilter.ClearFilter(transitionPage: false);
		m_search.ClearFilter();
	}

	public void SetEnabled(bool enabled)
	{
		m_inactiveFilterButton.SetEnabled(enabled);
		m_inactiveFilterButtonText.SetActive(enabled);
		m_inactiveFilterButtonRenderer.SetSharedMaterial(enabled ? m_enabledMaterial : m_disabledMaterial);
	}

	private void ManaFilter_OnFilterCleared(bool transitionPage)
	{
		ManaFilterUpdate(state: false, string.Empty);
	}

	private void ManaFilterUpdate(bool state, object description)
	{
		m_manaFilterActive = state;
		if (description == null)
		{
			m_manaFilterValue = "";
		}
		else
		{
			m_manaFilterValue = (string)description;
		}
		UpdateFilterView();
	}

	private void SearchFilterUpdate(bool state, object description)
	{
		m_searchFilterActive = state;
		if (description == null)
		{
			m_searchFilterValue = "";
		}
		else
		{
			m_searchFilterValue = (string)description;
		}
		UpdateFilterView();
	}

	public void UpdateFilterView()
	{
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (cmd == null)
		{
			return;
		}
		bool showSearchTextLabel = m_searchFilterActive;
		string textSearchTextLabel = m_searchFilterValue;
		bool isCardsView = cmd.GetViewMode() == CollectionUtils.ViewMode.CARDS;
		bool showHeroClassFilter = cmd.GetHeroSkinClass().HasValue;
		bool showSetFilter = m_setFilter.HasActiveFilter() && isCardsView && !m_searchFilterActive && !showHeroClassFilter;
		bool showManaIcon = m_manaFilterActive && isCardsView && !m_searchFilterActive && !showHeroClassFilter;
		string textManaIcon = m_manaFilterValue;
		if (showHeroClassFilter)
		{
			textSearchTextLabel = GameStrings.GetClassName(cmd.GetHeroSkinClass().Value);
			showSearchTextLabel = true;
		}
		else if (m_manaFilter != null && m_manaFilter.IsFilterOddOrEvenValues)
		{
			showManaIcon = false;
			showSetFilter = false;
			showSearchTextLabel = true;
			textManaIcon = string.Empty;
		}
		bool areFiltersActive = showManaIcon || showSearchTextLabel || showSetFilter;
		if (m_inactiveFilterButton != null)
		{
			m_activeFilterButton.gameObject.SetActive(areFiltersActive);
			m_inactiveFilterButton.gameObject.SetActive(!areFiltersActive);
		}
		else
		{
			if (m_filtersShown != areFiltersActive)
			{
				Vector3 start = (areFiltersActive ? new Vector3(180f, 0f, 0f) : new Vector3(0f, 0f, 0f));
				float target = (areFiltersActive ? 0.5f : (-0.5f));
				iTween.Stop(m_activeFilterButton.gameObject);
				m_activeFilterButton.gameObject.transform.localRotation = Quaternion.Euler(start);
				iTween.RotateBy(m_activeFilterButton.gameObject, iTween.Hash("x", target, "time", 0.25f, "easetype", iTween.EaseType.easeInOutExpo));
			}
			m_filtersShown = areFiltersActive;
		}
		if (showSearchTextLabel)
		{
			m_searchText.gameObject.SetActive(value: true);
			m_searchText.Text = textSearchTextLabel;
		}
		else
		{
			m_searchText.gameObject.SetActive(value: false);
			m_searchText.Text = string.Empty;
		}
		m_manaFilterIcon.SetActive(showManaIcon);
		m_manaFilterText.Text = textManaIcon;
		m_setFilter.SetButtonShown(showSetFilter);
		if (m_manaFilterIcon.activeSelf && !showSetFilter)
		{
			m_manaFilterIcon.transform.localPosition = m_manaFilterIconCenterBone.localPosition;
			return;
		}
		if (!m_manaFilterIcon.activeSelf && showSetFilter)
		{
			m_setFilter.m_toggleButton.gameObject.transform.localPosition = m_setFilterIconCenterBone.localPosition;
			return;
		}
		if (m_manaFilterIconDefaultBone != null)
		{
			m_manaFilterIcon.transform.localPosition = m_manaFilterIconDefaultBone.localPosition;
		}
		if (m_setFilterIconDefaultBone != null)
		{
			m_setFilter.m_toggleButton.gameObject.transform.localPosition = m_setFilterIconDefaultBone.localPosition;
		}
	}
}
