using System.Collections.Generic;
using Blizzard.T5.Core;
using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
public class CollectionHeroPickerButtons : AbsHeroPickerButtons
{
	public GameObject m_heroCountersContainer;

	[CustomEditField(T = EditType.GAME_OBJECT)]
	public string m_heroCounterPrefab;

	private const float LABEL_Z_OFFSET = 6.35f;

	private List<TAG_CLASS> m_heroClasses;

	private Map<TAG_CLASS, WidgetInstance> m_heroCounters;

	private int m_loadedCounters;

	private int[] m_allHeroCounts;

	private int[] m_ownedHeroCounts;

	public override void Awake()
	{
		base.Awake();
		GenerateHeroCounters();
	}

	private void GenerateHeroCounters()
	{
		m_heroClasses = new List<TAG_CLASS>(GameUtils.ORDERED_HERO_CLASSES);
		m_heroCounters = new Map<TAG_CLASS, WidgetInstance>();
		m_loadedCounters = 0;
		foreach (TAG_CLASS heroClass in m_heroClasses)
		{
			WidgetInstance heroCounterWidget = WidgetInstance.Create(m_heroCounterPrefab);
			heroCounterWidget.transform.SetParent(m_heroCountersContainer.transform, worldPositionStays: true);
			heroCounterWidget.RegisterReadyListener(delegate
			{
				OnHeroPickerCounterWidgetReady();
			});
			m_heroCounters[heroClass] = heroCounterWidget;
		}
	}

	protected override void InitHeroPickerButtons()
	{
		base.InitHeroPickerButtons();
		LoadHeroButtonsForFavoriteHeroes();
	}

	protected override void UpdateValidHeroClasses()
	{
		m_validClasses = new List<TAG_CLASS>(GameUtils.ORDERED_HERO_CLASSES);
	}

	protected void OnHeroPickerCounterWidgetReady()
	{
		m_loadedCounters++;
		if (AllCountersLoaded())
		{
			if (CollectionManager.Get().GetCollectibleDisplay().GetViewMode() != CollectionUtils.ViewMode.HERO_PICKER)
			{
				Hide();
			}
			else
			{
				UpdateHeroTotalLabels();
			}
		}
	}

	private void PositionHeroTotalLabel(WidgetInstance label, Transform targetTransform)
	{
		_ = label.transform.parent;
		label.transform.SetParent(targetTransform, worldPositionStays: false);
		label.transform.localPosition = new Vector3(0f, 0f, 6.35f);
		label.transform.SetParent(m_heroCountersContainer.transform, worldPositionStays: true);
		label.transform.localScale = new Vector3(1f, 1f, 1f);
		label.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
	}

	private void UpdateHeroTotalLabels()
	{
		if (!HasCounters())
		{
			return;
		}
		List<Transform> heroButtonTransforms = GetHeroButtonTransforms();
		List<TAG_CLASS> validClasses = new List<TAG_CLASS>(GameUtils.ORDERED_HERO_CLASSES);
		for (int i = 0; i < validClasses.Count; i++)
		{
			if (i >= m_allHeroCounts.Length || i >= m_ownedHeroCounts.Length || i >= heroButtonTransforms.Count)
			{
				Debug.LogWarning("UpdateHeroTotalLabels: mismatch between collectible hero classes and currently 'valid' classes.");
				continue;
			}
			TAG_CLASS heroClass = validClasses[i];
			WidgetInstance label = GetCounterForClass(heroClass);
			if (!(label == null))
			{
				PositionHeroTotalLabel(label, heroButtonTransforms[i]);
				UberText labelText = label.GetComponentInChildren<UberText>(includeInactive: true);
				if (!(labelText == null))
				{
					int classTotal = m_allHeroCounts[i];
					int ownedTotal = m_ownedHeroCounts[i];
					labelText.Text = ownedTotal + "/" + classTotal;
				}
			}
		}
	}

	protected override void OnHeroButtonReleased(UIEvent e)
	{
		if (e != null)
		{
			SoundManager.Get().LoadAndPlay("Card_Transition_Out.prefab:aecf5b5837772844b9d2db995744df82");
			HeroPickerButton button = (HeroPickerButton)e.GetElement();
			button.SetHighlightState(ActorStateType.HIGHLIGHT_OFF);
			CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
			if (cmd != null)
			{
				cmd.SetHeroSkinClass(button.m_heroClass);
				cmd.SetViewMode(CollectionUtils.ViewMode.HERO_SKINS);
			}
		}
	}

	protected override void OnHeroMouseOver(UIEvent e)
	{
		if (e != null)
		{
			SoundManager.Get().LoadAndPlay("collection_manager_card_mouse_over.prefab:0d4e20bc78956bc48b5e2963ec39211c");
			((HeroPickerButton)e.GetElement()).SetHighlightState(ActorStateType.HIGHLIGHT_MOUSE_OVER);
		}
	}

	protected override void OnHeroMouseOut(UIEvent e)
	{
		if (e != null)
		{
			HeroPickerButton button = (HeroPickerButton)e.GetElement();
			if (!UniversalInputManager.UsePhoneUI || !button.IsSelected())
			{
				button.SetHighlightState(ActorStateType.HIGHLIGHT_OFF);
			}
		}
	}

	public void LoadHeroButtonsForFavoriteHeroes()
	{
		CollectionManager collectionManager = CollectionManager.Get();
		if (collectionManager == null)
		{
			return;
		}
		m_heroDefsLoading = m_validClasses.Count;
		for (int i = 0; i < m_validClasses.Count; i++)
		{
			if (i >= m_heroButtons.Count || m_heroButtons[i] == null)
			{
				Debug.LogWarning("LoadHeroButtonsForFavoriteHeroes: not enough buttons for total heroes.");
				break;
			}
			HeroPickerButton button = m_heroButtons[i];
			button.Unlock();
			button.Raise();
			button.Activate(enable: true);
			TAG_CLASS heroClass = m_validClasses[i];
			CardPortraitQuality heroPortraitQuality = new CardPortraitQuality(3, collectionManager.GetHeroPremium(heroClass));
			NetCache.CardDefinition heroCardDef = collectionManager.GetRandomFavoriteHero(heroClass, null);
			if (heroCardDef == null)
			{
				Debug.LogWarning("LoadHeroButtonsForFavoriteHeroes: CCouldn't find Favorite Hero for hero class: " + heroClass.ToString() + " defaulting to Vanilla Hero!");
				string heroCardID = CollectionManager.GetVanillaHero(heroClass);
				HeroFullDefLoadedCallbackData callbackData = new HeroFullDefLoadedCallbackData(button, heroPortraitQuality.PremiumType);
				DefLoader.Get().LoadFullDef(heroCardID, OnHeroFullDefLoaded, callbackData, heroPortraitQuality);
			}
			else
			{
				HeroFullDefLoadedCallbackData callbackData2 = new HeroFullDefLoadedCallbackData(button, heroCardDef.Premium);
				DefLoader.Get().LoadFullDef(heroCardDef.Name, OnHeroFullDefLoaded, callbackData2, heroPortraitQuality);
			}
			button.SetDivotVisible(visible: false);
		}
	}

	public WidgetInstance GetCounterForClass(TAG_CLASS heroClass)
	{
		if (m_heroCounters == null)
		{
			return null;
		}
		return m_heroCounters[heroClass];
	}

	public bool AllCountersLoaded()
	{
		return m_loadedCounters == m_heroClasses.Count;
	}

	public bool HasCounters()
	{
		if (m_heroCountersContainer != null)
		{
			return m_heroCountersContainer.activeSelf;
		}
		return false;
	}

	public bool IsReady()
	{
		if (HasCounters())
		{
			return AllCountersLoaded();
		}
		return true;
	}

	public void UpdateHeroClassTotals(int[] allHeroCounts, int[] ownedHeroCounts)
	{
		m_allHeroCounts = allHeroCounts;
		m_ownedHeroCounts = ownedHeroCounts;
		UpdateHeroTotalLabels();
	}
}
