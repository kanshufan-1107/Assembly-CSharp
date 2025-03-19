using System;
using UnityEngine;

public class ClassFilterButtonContainer : MonoBehaviour
{
	public TAG_CLASS[] m_classTags;

	public ClassFilterButton[] m_classButtons;

	public Material[] m_classMaterials;

	public Material[] m_runeMaterials;

	public Material m_inactiveMaterial;

	public Material m_templateMaterial;

	public PegUIElement m_cardBacksButton;

	public PegUIElement m_heroSkinsButton;

	public PegUIElement m_coinsButton;

	public ClassFilterButton m_zilliaxModulesButton;

	public ClassFilterButton m_zilliaxVersionsButton;

	private const int NUM_ZILLIAX_BUTTONS = 2;

	public GameObject m_cardBacksDisabled;

	public GameObject m_heroSkinsDisabled;

	public GameObject m_coinsDisabled;

	public event Action<TAG_CLASS> OnClassFilterButtonPressed;

	private void OnEnable()
	{
		CollectionManagerDisplay.HideLockedRunesCheckboxToggled += OnHideLockedRunesCheckboxToggled;
		if (m_zilliaxModulesButton != null)
		{
			m_zilliaxModulesButton.AddEventListener(UIEventType.RELEASE, OnZilliaxModulesPressed);
		}
		if (m_zilliaxVersionsButton != null)
		{
			m_zilliaxVersionsButton.AddEventListener(UIEventType.RELEASE, OnZilliaxVersionsPressed);
		}
		ClassFilterButton[] classButtons = m_classButtons;
		for (int i = 0; i < classButtons.Length; i++)
		{
			classButtons[i].AddEventListener(UIEventType.RELEASE, OnClassButtonPressed);
		}
	}

	private void OnDisable()
	{
		CollectionManagerDisplay.HideLockedRunesCheckboxToggled -= OnHideLockedRunesCheckboxToggled;
		if (m_zilliaxModulesButton != null)
		{
			m_zilliaxModulesButton.RemoveEventListener(UIEventType.RELEASE, OnZilliaxModulesPressed);
		}
		if (m_zilliaxVersionsButton != null)
		{
			m_zilliaxVersionsButton.RemoveEventListener(UIEventType.RELEASE, OnZilliaxVersionsPressed);
		}
		ClassFilterButton[] classButtons = m_classButtons;
		for (int i = 0; i < classButtons.Length; i++)
		{
			classButtons[i].RemoveEventListener(UIEventType.RELEASE, OnClassButtonPressed);
		}
	}

	private void OnClassButtonPressed(UIEvent e)
	{
		ClassFilterButton classFilterButton = e.GetElement() as ClassFilterButton;
		if (classFilterButton != null)
		{
			this.OnClassFilterButtonPressed?.Invoke(classFilterButton.GetTagClass());
		}
	}

	private void OnHideLockedRunesCheckboxToggled(bool isChecked)
	{
		UpdateClassButtons();
	}

	private void SetCardBacksEnabled(bool enabled)
	{
		m_cardBacksButton.SetEnabled(enabled);
		m_cardBacksDisabled.SetActive(!enabled);
	}

	private void SetHeroSkinsEnabled(bool enabled)
	{
		m_heroSkinsButton.SetEnabled(enabled);
		m_heroSkinsDisabled.SetActive(!enabled);
	}

	private void SetCoinsEnabled(bool enabled)
	{
		m_coinsButton.SetEnabled(enabled);
		m_coinsDisabled.SetActive(!enabled);
	}

	private void UpdateCosmeticButtons()
	{
		CollectionManager collectionManager = CollectionManager.Get();
		bool isViewingZilliax = collectionManager.GetCollectibleDisplay().GetViewSubmode() == CollectionUtils.ViewSubmode.CARD_ZILLIAX_MODULES;
		bool shouldShowCardBacks;
		bool shouldShowHeroSkins;
		bool shouldShowCoins = (shouldShowHeroSkins = (shouldShowCardBacks = (collectionManager.GetCollectibleDisplay().GetPageManager() as CollectionPageManager).HasAnyCardsAvailable()));
		CollectionDeck editedDeck = collectionManager.GetEditedDeck();
		if (editedDeck != null)
		{
			int cardBacksOwned = CardBackManager.Get().GetCardBacksOwned().Count;
			int countOfOwnedHeroesForClass = collectionManager.GetCountOfOwnedHeroesForClass(editedDeck.GetClass());
			bool editedDeckHasHeroOverride = editedDeck.HasUIHeroOverride();
			shouldShowCardBacks = cardBacksOwned > 1;
			shouldShowHeroSkins = countOfOwnedHeroesForClass > 1 && !editedDeckHasHeroOverride;
			shouldShowCoins = CosmeticCoinManager.Get().GetTotalCoinsOwned() > 1;
		}
		shouldShowCardBacks = shouldShowCardBacks && !isViewingZilliax;
		shouldShowHeroSkins = shouldShowHeroSkins && !isViewingZilliax;
		shouldShowCoins = shouldShowCoins && !isViewingZilliax;
		SetCardBacksEnabled(shouldShowCardBacks);
		SetHeroSkinsEnabled(shouldShowHeroSkins);
		SetCoinsEnabled(shouldShowCoins);
	}

	private static bool SetupButton(ClassFilterButton button, CollectionTabInfo tabInfo, CollectionPageManager pageManager, Material material)
	{
		if (!pageManager.HasClassCardsAvailable(tabInfo.tagClass))
		{
			return false;
		}
		int newCardsCount = pageManager.GetNumNewCardsForClass(tabInfo.tagClass);
		button.SetTabInfo(tabInfo, material);
		button.SetNewCardCount(newCardsCount);
		return true;
	}

	private void UpdateClassButtons()
	{
		for (int i = 0; i < m_classTags.Length; i++)
		{
			m_classButtons[i].SetTabInfo(default(CollectionTabInfo), m_inactiveMaterial);
			m_classButtons[i].SetNewCardCount(0);
		}
		CollectionPageManager pageManager = CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as CollectionPageManager;
		if (pageManager == null)
		{
			Debug.Log("ClassFilterButtonContainer: UpdateClassButtons: pageManager is null");
			return;
		}
		bool isViewingZilliax = CollectionManager.Get().GetCollectibleDisplay().GetViewSubmode() == CollectionUtils.ViewSubmode.CARD_ZILLIAX_MODULES;
		if (!isViewingZilliax)
		{
			int currentIndex = 0;
			for (int j = 0; j < m_classTags.Length; j++)
			{
				if (pageManager.HasClassCardsAvailable(m_classTags[j]))
				{
					CollectionTabInfo collectionTabInfo = default(CollectionTabInfo);
					collectionTabInfo.tagClass = m_classTags[j];
					CollectionTabInfo tabInfo = collectionTabInfo;
					m_classButtons[currentIndex].SetTabInfo(tabInfo, m_classMaterials[j]);
					int numNewCardsForClass = pageManager.GetNumNewCardsForClass(m_classTags[j]);
					m_classButtons[currentIndex].SetNewCardCount(numNewCardsForClass);
					currentIndex++;
				}
			}
		}
		for (int k = 0; k < 2; k++)
		{
			m_classButtons[k].gameObject.SetActive(!isViewingZilliax);
		}
		if (m_zilliaxModulesButton != null)
		{
			m_zilliaxModulesButton.SetFixedClassTabEnabled(isViewingZilliax);
		}
		if (m_zilliaxVersionsButton != null)
		{
			m_zilliaxVersionsButton.SetFixedClassTabEnabled(isViewingZilliax);
		}
	}

	public void UpdateButtons()
	{
		UpdateCosmeticButtons();
		UpdateClassButtons();
	}

	private void OnZilliaxModulesPressed(UIEvent e)
	{
		CollectionManager.Get().GetCollectibleDisplay().GetPageManager()
			.FlipToPage(1, null, null);
	}

	private void OnZilliaxVersionsPressed(UIEvent e)
	{
		CollectionManager.Get().GetCollectibleDisplay().GetPageManager()
			.FlipToPage(2, null, null);
	}
}
