using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Hearthstone.DataModels;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using PegasusShared;
using UnityEngine;

[CustomEditClass]
public abstract class AbsHeroPickerButtons : MonoBehaviour
{
	protected class HeroFullDefLoadedCallbackData
	{
		[CompilerGenerated]
		private TAG_PREMIUM _003CPremium_003Ek__BackingField;

		public HeroPickerButton HeroPickerButton { get; private set; }

		private TAG_PREMIUM Premium
		{
			[CompilerGenerated]
			set
			{
				_003CPremium_003Ek__BackingField = value;
			}
		}

		public HeroFullDefLoadedCallbackData(HeroPickerButton button, TAG_PREMIUM premium)
		{
			HeroPickerButton = button;
			Premium = premium;
		}
	}

	public GameObject m_rootObject;

	public GameObject m_buttonContainer;

	public List<GameObject> m_heroPickerButtonBonesByHeroCount;

	[SerializeField]
	protected bool m_isMobileLayout;

	[CustomEditField(T = EditType.GAME_OBJECT)]
	public string m_heroButtonWidgetPrefab;

	protected int m_HeroPickerButtonCount;

	protected List<HeroPickerButton> m_heroButtons = new List<HeroPickerButton>();

	protected HeroPickerButton m_selectedHeroButton;

	protected List<Transform> m_heroBones;

	protected List<TAG_CLASS> m_validClasses = new List<TAG_CLASS>();

	protected int m_heroDefsLoading = int.MaxValue;

	private WidgetTemplate m_widget;

	protected bool m_hasLoaded;

	[Overridable]
	public bool IsMobileLayout
	{
		get
		{
			return m_isMobileLayout;
		}
		set
		{
			m_isMobileLayout = value;
		}
	}

	public virtual void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterDoneChangingStatesListener(HandleDoneChangingStates);
	}

	private void HandleDoneChangingStates(object unused)
	{
		if (!m_hasLoaded)
		{
			StartCoroutine(LoadHeroButtons(null));
			m_hasLoaded = true;
		}
	}

	protected virtual void OnHeroButtonReleased(UIEvent e)
	{
	}

	protected virtual void OnHeroMouseOver(UIEvent e)
	{
	}

	protected virtual void OnHeroMouseOut(UIEvent e)
	{
	}

	protected virtual IEnumerator WaitForHeroPickerButtonsLoaded()
	{
		while (m_heroButtons.Count < m_HeroPickerButtonCount)
		{
			yield return null;
		}
		foreach (HeroPickerButton button in m_heroButtons)
		{
			while (button.GetComponent<WidgetTemplate>().IsChangingStates)
			{
				yield return null;
			}
		}
	}

	protected IEnumerator LoadHeroButtons(int? m_cheatOverrideHeroPickerButtonCount = null)
	{
		if (m_cheatOverrideHeroPickerButtonCount.HasValue)
		{
			m_HeroPickerButtonCount = m_cheatOverrideHeroPickerButtonCount.Value;
		}
		else
		{
			m_HeroPickerButtonCount = ValidateHeroCount();
		}
		SetupHeroLayout();
		foreach (HeroPickerButton heroButton in m_heroButtons)
		{
			Object.Destroy(heroButton.gameObject);
		}
		m_heroButtons.Clear();
		HeroPickerDataModel dataModel = GetHeroPickerDataModel();
		for (int i = 0; i < m_HeroPickerButtonCount; i++)
		{
			WidgetInstance heroPickerButtonWidget = WidgetInstance.Create(m_heroButtonWidgetPrefab);
			if (dataModel != null)
			{
				heroPickerButtonWidget.BindDataModel(dataModel);
			}
			heroPickerButtonWidget.RegisterReadyListener(delegate
			{
				OnHeroPickerButtonWidgetReady(heroPickerButtonWidget);
			});
		}
		yield return StartCoroutine(WaitForHeroPickerButtonsLoaded());
		InitHeroPickerButtons();
	}

	protected virtual void InitHeroPickerButtons()
	{
	}

	protected void SetupHeroLayout()
	{
		if (m_HeroPickerButtonCount <= 0 || m_HeroPickerButtonCount > m_heroPickerButtonBonesByHeroCount.Count || m_heroPickerButtonBonesByHeroCount[m_HeroPickerButtonCount] == null)
		{
			Log.Adventures.PrintWarning("Deck/Class Picker Instantiated with an unsupported amount of heroes: " + m_HeroPickerButtonCount);
			return;
		}
		GameObject layout = m_heroPickerButtonBonesByHeroCount[m_HeroPickerButtonCount];
		List<Transform> boneLocations = GetBoneLocationsFromLayout(layout);
		m_heroBones = new List<Transform>();
		m_heroBones.AddRange(boneLocations);
		if (m_heroBones.Count != m_HeroPickerButtonCount)
		{
			Log.Adventures.PrintWarning("Layout for {0} heroes yielded an incorrect amount of transforms. This will result in errors when displaying heroes!", m_HeroPickerButtonCount);
		}
	}

	private List<Transform> GetBoneLocationsFromLayout(GameObject layout)
	{
		List<Transform> locations = new List<Transform>();
		Transform[] componentsInChildren = layout.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.childCount == 0)
			{
				locations.Add(transform);
			}
		}
		return locations;
	}

	protected void OnHeroPickerButtonWidgetReady(WidgetInstance widget)
	{
		HeroPickerButton button = widget.GetComponentInChildren<HeroPickerButton>();
		m_heroButtons.Add(button);
		SetUpHeroPickerButton(button, m_heroButtons.Count - 1);
		button.Lock();
		button.Activate(enable: false);
		button.AddEventListener(UIEventType.TAP, OnHeroButtonReleased);
		button.AddEventListener(UIEventType.RELEASE, OnHeroButtonReleased);
		button.AddEventListener(UIEventType.ROLLOVER, OnHeroMouseOver);
		button.AddEventListener(UIEventType.ROLLOUT, OnHeroMouseOut);
		Vector3 originalLocalPos = ((button.m_raiseAndLowerRoot != null) ? button.m_raiseAndLowerRoot.transform.localPosition : base.transform.localPosition);
		button.SetOriginalLocalPosition(originalLocalPos);
	}

	protected void SetUpHeroPickerButton(HeroPickerButton button, int heroCount)
	{
		GameObject go = button.gameObject;
		Transform parent = go.transform.parent;
		go.name = $"{go.name}_{heroCount}";
		parent.transform.SetParent(m_heroBones[heroCount], worldPositionStays: false);
		parent.transform.localScale = Vector3.one;
		parent.transform.localPosition = Vector3.zero;
		parent.SetParent(m_buttonContainer.transform, worldPositionStays: true);
	}

	protected virtual void UpdateValidHeroClasses()
	{
		m_validClasses = new List<TAG_CLASS>(GameUtils.ORDERED_HERO_CLASSES);
		if (SceneMgr.Get() == null)
		{
			return;
		}
		SceneMgr.Mode mode = SceneMgr.Get().GetMode();
		if (Options.GetFormatType() == FormatType.FT_CLASSIC && mode != SceneMgr.Mode.ADVENTURE)
		{
			m_validClasses = new List<TAG_CLASS>(GameUtils.CLASSIC_ORDERED_HERO_CLASSES);
		}
		ScenarioDbId? scenarioId = null;
		if (mode == SceneMgr.Mode.ADVENTURE)
		{
			scenarioId = AdventureConfig.Get().GetMission();
		}
		if (mode == SceneMgr.Mode.TAVERN_BRAWL || FriendChallengeMgr.Get().IsChallengeTavernBrawl())
		{
			scenarioId = (ScenarioDbId)TavernBrawlManager.Get().CurrentMission().missionId;
		}
		if (scenarioId.HasValue && scenarioId != ScenarioDbId.INVALID)
		{
			ScenarioDbfRecord scenarioIdRecord = GameDbf.Scenario.GetRecord((int)scenarioId.Value);
			for (int i = 0; i < scenarioIdRecord.ClassExclusions.Count; i++)
			{
				m_validClasses.Remove((TAG_CLASS)scenarioIdRecord.ClassExclusions[i].ClassId);
			}
		}
	}

	protected virtual int ValidateHeroCount()
	{
		UpdateValidHeroClasses();
		return m_validClasses.Count;
	}

	protected virtual IEnumerator SetHeroButtonsEnabled(bool enable)
	{
		yield return StartCoroutine(WaitForHeroPickerButtonsLoaded());
		foreach (HeroPickerButton button in m_heroButtons)
		{
			if (!button.IsLocked() || !enable)
			{
				button.SetEnabled(enable);
			}
		}
	}

	protected virtual void OnHeroFullDefLoaded(string cardId, DefLoader.DisposableFullDef fullDef, object userData)
	{
		using (fullDef)
		{
			if (fullDef?.EntityDef != null)
			{
				HeroFullDefLoadedCallbackData callbackData = userData as HeroFullDefLoadedCallbackData;
				TAG_PREMIUM premium = ((!GameUtils.IsVanillaHero(cardId)) ? TAG_PREMIUM.GOLDEN : CollectionManager.Get().GetBestCardPremium(cardId));
				callbackData.HeroPickerButton.UpdateDisplay(fullDef, premium);
				if (!m_hasLoaded)
				{
					Vector3 originalLocalPos = ((callbackData.HeroPickerButton.m_raiseAndLowerRoot != null) ? callbackData.HeroPickerButton.m_raiseAndLowerRoot.transform.localPosition : callbackData.HeroPickerButton.transform.localPosition);
					callbackData.HeroPickerButton.SetOriginalLocalPosition(originalLocalPos);
				}
			}
			m_heroDefsLoading--;
		}
	}

	public void Show()
	{
		m_rootObject.SetActive(value: true);
	}

	public void Hide()
	{
		m_rootObject.SetActive(value: false);
	}

	public HeroPickerDataModel GetHeroPickerDataModel()
	{
		VisualController visualController = GetComponent<VisualController>();
		if (visualController == null)
		{
			return null;
		}
		Widget owner = visualController.Owner;
		if (!owner.GetDataModel(13, out var dataModel))
		{
			dataModel = new HeroPickerDataModel();
			owner.BindDataModel(dataModel);
		}
		return dataModel as HeroPickerDataModel;
	}

	public List<Transform> GetHeroButtonTransforms()
	{
		return m_heroBones;
	}
}
