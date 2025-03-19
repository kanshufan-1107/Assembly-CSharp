using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class CraftingTray : CraftingTrayBase
{
	public UIBButton m_doneButton;

	public PegUIElement m_massDisenchantButton;

	public UberText m_potentialDustAmount;

	public UberText m_massDisenchantText;

	public CheckBox m_normalOwnedCheckbox;

	public CheckBox m_normalMissingCheckbox;

	public CheckBox m_premiumOwnedCheckbox;

	public CheckBox m_premiumMissingCheckbox;

	public CheckBox m_includeUncraftableCheckbox;

	public HighlightState m_highlight;

	public GameObject m_massDisenchantMesh;

	public Material m_massDisenchantMaterial;

	public Material m_massDisenchantDisabledMaterial;

	private int m_dustAmount;

	private bool m_shown;

	private CollectionUtils.ViewMode m_previousViewMode;

	private List<CollectibleCard> m_disenchantCards = new List<CollectibleCard>();

	private static CraftingTray s_instance;

	private static PlatformDependentValue<int> MASS_DISENCHANT_MATERIAL_TO_SWITCH = new PlatformDependentValue<int>(PlatformCategory.Screen)
	{
		PC = 0,
		Phone = 1
	};

	public static event Action CraftingTrayShown;

	public static event Action CraftingTrayHidden;

	private void Awake()
	{
		s_instance = this;
	}

	private void Start()
	{
		m_doneButton.AddEventListener(UIEventType.RELEASE, OnDoneButtonReleased);
		m_massDisenchantButton.AddEventListener(UIEventType.RELEASE, OnMassDisenchantButtonReleased);
		m_massDisenchantButton.AddEventListener(UIEventType.ROLLOVER, OnMassDisenchantButtonOver);
		m_massDisenchantButton.AddEventListener(UIEventType.ROLLOUT, OnMassDisenchantButtonOut);
		SetMassDisenchantAmount();
		m_normalOwnedCheckbox.AddEventListener(UIEventType.RELEASE, delegate
		{
			CheckboxChanged(m_normalOwnedCheckbox.IsChecked());
		});
		m_normalOwnedCheckbox.SetChecked(isChecked: true);
		m_normalMissingCheckbox.AddEventListener(UIEventType.RELEASE, delegate
		{
			CheckboxChanged(m_normalMissingCheckbox.IsChecked());
		});
		m_normalMissingCheckbox.SetChecked(isChecked: true);
		m_premiumOwnedCheckbox.AddEventListener(UIEventType.RELEASE, delegate
		{
			CheckboxChanged(m_premiumOwnedCheckbox.IsChecked());
		});
		m_premiumOwnedCheckbox.SetChecked(isChecked: true);
		m_premiumMissingCheckbox.AddEventListener(UIEventType.RELEASE, delegate
		{
			CheckboxChanged(m_premiumMissingCheckbox.IsChecked());
		});
		m_premiumMissingCheckbox.SetChecked(isChecked: false);
		m_includeUncraftableCheckbox.AddEventListener(UIEventType.RELEASE, delegate
		{
			CheckboxChanged(m_includeUncraftableCheckbox.IsChecked());
		});
		m_includeUncraftableCheckbox.SetChecked(isChecked: true);
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	public static CraftingTray Get()
	{
		return s_instance;
	}

	public void UpdateMassDisenchantAmount()
	{
		bool massDisenchantEnabled = m_dustAmount > 0;
		SetMassDisenchantEnabled(massDisenchantEnabled);
	}

	private void SetMassDisenchantEnabled(bool enabled)
	{
		m_massDisenchantButton.SetEnabled(enabled);
		m_massDisenchantText.gameObject.SetActive(enabled);
		m_potentialDustAmount.gameObject.SetActive(enabled);
		m_highlight.gameObject.SetActive(enabled);
		Renderer massDisenchantRenderer = m_massDisenchantMesh.GetComponent<Renderer>();
		if (enabled)
		{
			massDisenchantRenderer.SetMaterial(MASS_DISENCHANT_MATERIAL_TO_SWITCH, m_massDisenchantMaterial);
			m_highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
		}
		else
		{
			massDisenchantRenderer.SetMaterial(MASS_DISENCHANT_MATERIAL_TO_SWITCH, m_massDisenchantDisabledMaterial);
		}
	}

	public void SetMassDisenchantAmount()
	{
		if (base.gameObject.activeSelf)
		{
			StartCoroutine(SetMassDisenchantAmountWhenReady());
		}
	}

	private IEnumerator SetMassDisenchantAmountWhenReady()
	{
		while (MassDisenchant.Get() == null)
		{
			yield return null;
		}
		CollectionManager.Get().GetMassDisenchantCards(m_disenchantCards);
		MassDisenchant.Get().UpdateContents(m_disenchantCards);
		int amount = (m_dustAmount = MassDisenchant.Get().GetTotalAmount());
		m_potentialDustAmount.Text = amount.ToString();
		UpdateMassDisenchantAmount();
	}

	public override void Show(bool? overrideIncludeUncraftable = null, bool? overrideNormalOwned = null, bool? overrideNormalMissing = null, bool? overridePremiumOwned = null, bool? overridePremiumMissing = null, bool updatePage = true)
	{
		if (!m_shown)
		{
			m_shown = true;
			PresenceMgr.Get().SetStatus(Global.PresenceStatus.CRAFTING);
			if (overrideIncludeUncraftable.HasValue)
			{
				m_includeUncraftableCheckbox.SetChecked(overrideIncludeUncraftable.Value);
			}
			if (overrideNormalOwned.HasValue)
			{
				m_normalOwnedCheckbox.SetChecked(overrideNormalOwned.Value);
			}
			if (overrideNormalMissing.HasValue)
			{
				m_normalMissingCheckbox.SetChecked(overrideNormalMissing.Value);
			}
			if (overridePremiumOwned.HasValue)
			{
				m_premiumOwnedCheckbox.SetChecked(overridePremiumOwned.Value);
			}
			if (overridePremiumMissing.HasValue)
			{
				m_premiumMissingCheckbox.SetChecked(overridePremiumMissing.Value);
			}
			SetMassDisenchantAmount();
			(CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as CollectionPageManager).ShowCraftingModeCards(null, null, m_includeUncraftableCheckbox.IsChecked(), m_normalOwnedCheckbox.IsChecked(), m_normalMissingCheckbox.IsChecked(), m_premiumOwnedCheckbox.IsChecked(), m_premiumMissingCheckbox.IsChecked(), updatePage);
			CraftingTray.CraftingTrayShown?.Invoke();
		}
	}

	public override void Hide()
	{
		Hide();
		CraftingTray.CraftingTrayHidden?.Invoke();
	}

	public void Hide(bool updatePage = true)
	{
		if (!m_shown)
		{
			return;
		}
		m_shown = false;
		PresenceMgr.Get().SetPrevStatus();
		CollectibleDisplay cd = CollectionManager.Get().GetCollectibleDisplay();
		if (!(cd != null))
		{
			return;
		}
		cd.HideCraftingTray();
		if (updatePage)
		{
			bool num = CollectionManager.Get().GetCollectibleDisplay().GetViewMode() == CollectionUtils.ViewMode.MASS_DISENCHANT;
			BookPageManager.PageTransitionType pageTransitionType = (num ? BookPageManager.PageTransitionType.MANY_PAGE_LEFT : BookPageManager.PageTransitionType.NONE);
			cd.GetPageManager().HideCraftingModeCards(pageTransitionType);
			if (num)
			{
				CollectionManager.Get().GetCollectibleDisplay().SetViewMode(m_previousViewMode);
			}
		}
	}

	public override bool IsShown()
	{
		return m_shown;
	}

	public void EnableCraftingInBackground(bool enable = true)
	{
		CollectionPageManager cpm = CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as CollectionPageManager;
		if (enable)
		{
			cpm.ShowCraftingModeCards(null, null, m_includeUncraftableCheckbox.IsChecked(), m_normalOwnedCheckbox.IsChecked(), m_normalMissingCheckbox.IsChecked(), m_premiumOwnedCheckbox.IsChecked(), m_premiumMissingCheckbox.IsChecked(), updatePage: false);
		}
		else
		{
			cpm.HideCraftingModeCards(BookPageManager.PageTransitionType.NONE, updatePage: false);
		}
	}

	private void OnDoneButtonReleased(UIEvent e)
	{
		Hide();
	}

	private void OnMassDisenchantButtonReleased(UIEvent e)
	{
		if (!CollectionManager.Get().GetCollectibleDisplay().GetPageManager()
			.ArePagesTurning())
		{
			if (CollectionManager.Get().GetCollectibleDisplay().GetViewMode() == CollectionUtils.ViewMode.MASS_DISENCHANT)
			{
				CollectionManager.Get().GetCollectibleDisplay().SetViewMode(m_previousViewMode);
				m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
			}
			else
			{
				m_previousViewMode = CollectionManager.Get().GetCollectibleDisplay().GetViewMode();
				CollectionManager.Get().GetCollectibleDisplay().SetViewMode(CollectionUtils.ViewMode.MASS_DISENCHANT);
				StartCoroutine(MassDisenchant.Get().StartHighlight());
			}
			SoundManager.Get().LoadAndPlay("Hub_Click.prefab:cc2cf2b5507827149b13d12210c0f323");
		}
	}

	private void OnMassDisenchantButtonOver(UIEvent e)
	{
		m_highlight.ChangeState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
		SoundManager.Get().LoadAndPlay("Hub_Mouseover.prefab:40130da7b734190479c527d6bca1a4a8");
	}

	private void OnMassDisenchantButtonOut(UIEvent e)
	{
		if (CollectionManager.Get().GetCollectibleDisplay().GetViewMode() != CollectionUtils.ViewMode.MASS_DISENCHANT)
		{
			int potentialDustAmount = 0;
			try
			{
				potentialDustAmount = int.Parse(m_potentialDustAmount.Text);
			}
			catch (Exception ex)
			{
				Log.All.PrintWarning("Exception when attempting to parse CraftingTray's m_potentialDustAmount! Exception: {0}", ex);
			}
			if (potentialDustAmount > 0)
			{
				m_highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
			}
			else
			{
				m_highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
			}
		}
	}

	private void CheckboxChanged(bool isChecked)
	{
		bool updatePage = CollectionManager.Get().GetCollectibleDisplay().GetViewMode() == CollectionUtils.ViewMode.CARDS;
		(CollectionManager.Get().GetCollectibleDisplay().GetPageManager() as CollectionPageManager).ShowCraftingModeCards(null, null, m_includeUncraftableCheckbox.IsChecked(), m_normalOwnedCheckbox.IsChecked(), m_normalMissingCheckbox.IsChecked(), m_premiumOwnedCheckbox.IsChecked(), m_premiumMissingCheckbox.IsChecked(), updatePage, toggleChanged: true);
		if (isChecked)
		{
			SoundManager.Get().LoadAndPlay("checkbox_toggle_on.prefab:8be4c59e7387600468ac88787943da8b", base.gameObject);
		}
		else
		{
			SoundManager.Get().LoadAndPlay("checkbox_toggle_off.prefab:fa341d119cee1d14c941b63dba112af3", base.gameObject);
		}
	}
}
