using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using PegasusLettuce;
using UnityEngine;

[CustomEditClass]
public class LettuceLobbyChooserTray : AccordionMenuTray, IPopupRendering
{
	public class SelectedOptionInfo
	{
		public SceneMgr.Mode Mode;

		public LettuceBountySetDbfRecord BountySetRecord;

		public MercenariesDataUtil.MercenariesBountyLockedReason LockedReason;

		public LettuceBounty.MercenariesBountyDifficulty Difficulty;

		public string CustomLockedText;

		public bool Locked => LockedReason != MercenariesDataUtil.MercenariesBountyLockedReason.UNLOCKED;

		public void SetInfo(SceneMgr.Mode mode, LettuceBountySetDbfRecord record, LettuceBounty.MercenariesBountyDifficulty difficulty, MercenariesDataUtil.MercenariesBountyLockedReason lockedReason = MercenariesDataUtil.MercenariesBountyLockedReason.UNLOCKED, string customLockedText = null)
		{
			Mode = mode;
			BountySetRecord = record;
			Difficulty = difficulty;
			LockedReason = lockedReason;
			CustomLockedText = customLockedText;
		}
	}

	private class BountySetButtonInfo
	{
		public LettuceLobbyChooserButton ChooserButton;

		public LettuceBountySetDbfRecord BountySet;

		private List<LettuceLobbyChooserSubButton> m_difficultySubButtons = new List<LettuceLobbyChooserSubButton>();

		public LettuceLobbyChooserSubButton GetSubButtonForDifficulty(LettuceBounty.MercenariesBountyDifficulty difficulty)
		{
			return m_difficultySubButtons.Find((LettuceLobbyChooserSubButton subButton) => subButton.GetDifficulty() == difficulty);
		}

		public void ForEachDifficultySubButton(Action<LettuceLobbyChooserSubButton> fn)
		{
			foreach (LettuceLobbyChooserSubButton subButton in m_difficultySubButtons)
			{
				fn(subButton);
			}
		}

		public void AddSubButton(LettuceLobbyChooserSubButton subButton)
		{
			m_difficultySubButtons.Add(subButton);
		}
	}

	private enum EventStatus
	{
		ACTIVE,
		DISABLED,
		NOT_YET_STARTED,
		ENDED
	}

	private class BountySetBountyCounts
	{
		public int Total { get; set; }

		public int New { get; set; }
	}

	public string m_pvpChooserButtonPrefab;

	public Material m_activeRunGlowMaterial;

	private SelectedOptionInfo m_selectedOption = new SelectedOptionInfo();

	private WidgetTemplate m_owningWidget;

	private List<WidgetInstance> m_buttonWidgets = new List<WidgetInstance>();

	private int m_numButtonsLoading;

	private bool m_okayToCreateButtons;

	private IPopupRoot m_popupRoot;

	private HashSet<IPopupRendering> m_popupRenderers = new HashSet<IPopupRendering>();

	public event Action OnModeSelected;

	public SelectedOptionInfo GetSelectedModeInfo()
	{
		if (m_selectedOption.Mode == SceneMgr.Mode.INVALID)
		{
			Debug.LogError("LettuceLobbyChooserTray:GetSelectedModeInfo m_selectedOption is being accessed before initialization.");
			return null;
		}
		return m_selectedOption;
	}

	private void Awake()
	{
		m_owningWidget = GetComponentInParent<WidgetTemplate>();
	}

	private void OnEnable()
	{
		if (!m_okayToCreateButtons)
		{
			m_okayToCreateButtons = true;
			return;
		}
		RemoveChooserButtons(destroyObjects: true);
		StartCoroutine(InitTrayWhenReady());
	}

	private void OnDestroy()
	{
		RemoveChooserButtons(destroyObjects: false);
	}

	protected IEnumerator InitTrayWhenReady()
	{
		if (m_ChooseFrameScroller == null || m_ChooseFrameScroller.ScrollObject == null)
		{
			Debug.LogError("m_ChooseFrameScroller or m_ChooseFrameScroller.m_ScrollObject cannot be null. Unable to create button.", this);
			yield break;
		}
		InitNormalLayout();
		while (!m_buttonWidgets.TrueForAll((WidgetInstance w) => w.IsReady && !w.IsChangingStates) || m_ChooseFrameScroller.GetPolledScrollHeight() <= 0f)
		{
			yield return null;
		}
		if (m_SelectedSubButton != null && m_ChooseFrameScroller != null)
		{
			m_ChooseFrameScroller.CenterObjectInView(m_SelectedSubButton.gameObject, 0f, null, iTween.EaseType.easeOutCubic, 0f);
		}
	}

	private int CompareBountySets(LettuceBountySetDbfRecord a, LettuceBountySetDbfRecord b, bool hasActiveBountySet, int previousBountySetId)
	{
		if (hasActiveBountySet)
		{
			if (a.ID == previousBountySetId)
			{
				return -1;
			}
			if (b.ID == previousBountySetId)
			{
				return 1;
			}
		}
		bool aIsUnlocked = MercenariesDataUtil.GetBountySetUnlockStatus(a) == MercenariesDataUtil.MercenariesBountyLockedReason.UNLOCKED;
		bool bIsUnlocked = MercenariesDataUtil.GetBountySetUnlockStatus(b) == MercenariesDataUtil.MercenariesBountyLockedReason.UNLOCKED;
		if (aIsUnlocked == bIsUnlocked)
		{
			return a.SortOrder.CompareTo(b.SortOrder);
		}
		if (!aIsUnlocked)
		{
			return 1;
		}
		return -1;
	}

	private void InitNormalLayout()
	{
		NetCache.NetCacheMercenariesPlayerInfo playerInfo = NetCache.Get().GetNetObject<NetCache.NetCacheMercenariesPlayerInfo>();
		List<LettuceBountySetDbfRecord> bountySets = GameDbf.LettuceBountySet.GetRecords((LettuceBountySetDbfRecord r) => !r.IsTutorial);
		int previousMapSetId = -1;
		LettuceBounty.MercenariesBountyDifficulty previousMapDifficulty = LettuceBounty.MercenariesBountyDifficulty.NORMAL;
		bool hasActiveMap = false;
		PegasusLettuce.LettuceMap previousMap = NetCache.Get().GetNetObject<NetCache.NetCacheLettuceMap>()?.Map;
		BnetBar.Get().TryGetServerTimeUTC(out var currentTime);
		if (previousMap != null)
		{
			LettuceBountyDbfRecord bountyRecord = GameDbf.LettuceBounty.GetRecord((int)previousMap.BountyId);
			if (bountyRecord != null && bountySets.Find((LettuceBountySetDbfRecord bountySet) => bountySet.ID == bountyRecord.BountySetId) != null)
			{
				hasActiveMap = previousMap.Active;
				previousMapSetId = bountyRecord.BountySetId;
				previousMapDifficulty = ((!bountyRecord.Heroic) ? LettuceBounty.MercenariesBountyDifficulty.NORMAL : LettuceBounty.MercenariesBountyDifficulty.HEROIC);
			}
		}
		bountySets.Sort((LettuceBountySetDbfRecord a, LettuceBountySetDbfRecord b) => CompareBountySets(a, b, hasActiveMap, previousMapSetId));
		bool firstTimeThroughLoop = true;
		foreach (LettuceBountySetDbfRecord lettuceBountySetRecord in bountySets)
		{
			bool thisIsFirstButtonInList = firstTimeThroughLoop;
			Dictionary<LettuceBounty.MercenariesBountyDifficulty, BountySetBountyCounts> numBountiesPerDifficulty = new Dictionary<LettuceBounty.MercenariesBountyDifficulty, BountySetBountyCounts>();
			bool setHasAccessibleBounties = false;
			if (playerInfo != null && playerInfo.BountyInfoMap != null)
			{
				foreach (LettuceBountyDbfRecord bounty in GameDbf.LettuceBounty.GetRecords((LettuceBountyDbfRecord r) => r.BountySetId == lettuceBountySetRecord.ID))
				{
					LettuceBounty.MercenariesBountyDifficulty bountyDifficulty = bounty.DifficultyMode;
					if (bountyDifficulty == LettuceBounty.MercenariesBountyDifficulty.NONE)
					{
						bountyDifficulty = ((!bounty.Heroic) ? LettuceBounty.MercenariesBountyDifficulty.NORMAL : LettuceBounty.MercenariesBountyDifficulty.HEROIC);
					}
					if (!numBountiesPerDifficulty.TryGetValue(bountyDifficulty, out var difficultyCounts))
					{
						difficultyCounts = new BountySetBountyCounts();
						numBountiesPerDifficulty[bountyDifficulty] = difficultyCounts;
					}
					BountySetBountyCounts bountySetBountyCounts = difficultyCounts;
					int total = bountySetBountyCounts.Total + 1;
					bountySetBountyCounts.Total = total;
					if (LettuceVillageDataUtil.IsDifficultyUnlocked(bounty.DifficultyMode))
					{
						setHasAccessibleBounties = true;
					}
					if (!playerInfo.BountyInfoMap.TryGetValue(bounty.ID, out var bountyInfo))
					{
						if (MercenariesDataUtil.GetBountyUnlockStatus(bounty) == MercenariesDataUtil.MercenariesBountyLockedReason.UNLOCKED && MercenariesDataUtil.GetBountySetUnlockStatus(lettuceBountySetRecord) == MercenariesDataUtil.MercenariesBountyLockedReason.UNLOCKED)
						{
							BountySetBountyCounts bountySetBountyCounts2 = difficultyCounts;
							total = bountySetBountyCounts2.New + 1;
							bountySetBountyCounts2.New = total;
						}
					}
					else if (!bountyInfo.IsAcknowledged && (!bountyInfo.UnlockTime.HasValue || bountyInfo.UnlockTime < currentTime))
					{
						BountySetBountyCounts bountySetBountyCounts3 = difficultyCounts;
						total = bountySetBountyCounts3.New + 1;
						bountySetBountyCounts3.New = total;
					}
				}
			}
			if (!setHasAccessibleBounties)
			{
				continue;
			}
			WidgetInstance buttonWidget = CreateChooserButton(m_DefaultChooserButtonPrefab, lettuceBountySetRecord.Name, delegate(LettuceLobbyChooserButton button)
			{
				BountySetButtonInfo bountySetButtonInfo = CreateBountySetButtons(button, lettuceBountySetRecord, numBountiesPerDifficulty);
				if (hasActiveMap)
				{
					ConfigureButtonWhenPreviousBountyIsUnfinished(bountySetButtonInfo, previousMapSetId, previousMapDifficulty);
				}
				else if (lettuceBountySetRecord.IsComingSoon)
				{
					button.SetDesaturate(desaturate: true);
					bountySetButtonInfo.ForEachDifficultySubButton(delegate(LettuceLobbyChooserSubButton subButton)
					{
						subButton.LockButton(MercenariesDataUtil.MercenariesBountyLockedReason.COMING_SOON);
					});
				}
				else
				{
					MercenariesDataUtil.MercenariesBountyLockedReason bountySetLockReason = MercenariesDataUtil.GetBountySetUnlockStatus(lettuceBountySetRecord);
					if (bountySetLockReason == MercenariesDataUtil.MercenariesBountyLockedReason.EVENT_ENDED || bountySetLockReason == MercenariesDataUtil.MercenariesBountyLockedReason.EVENT_NOT_ACTIVE || bountySetLockReason == MercenariesDataUtil.MercenariesBountyLockedReason.EVENT_NOT_COMPLETE || bountySetLockReason == MercenariesDataUtil.MercenariesBountyLockedReason.EVENT_NOT_STARTED)
					{
						ConfigureButtonFromEventStatus(bountySetButtonInfo, bountySetLockReason);
						if (!string.IsNullOrEmpty(lettuceBountySetRecord.EventComingSoonText))
						{
							bountySetButtonInfo.ForEachDifficultySubButton(delegate(LettuceLobbyChooserSubButton subButton)
							{
								subButton.SetCustomLockedText(lettuceBountySetRecord.EventComingSoonText);
							});
						}
					}
					else if (bountySetLockReason == MercenariesDataUtil.MercenariesBountyLockedReason.PREVIOUS_ZONES_INCOMPLETE)
					{
						button.SetDesaturate(desaturate: true);
						bountySetButtonInfo.ForEachDifficultySubButton(delegate(LettuceLobbyChooserSubButton subButton)
						{
							subButton.LockButton(bountySetLockReason);
						});
					}
					else
					{
						bountySetButtonInfo.ForEachDifficultySubButton(delegate(LettuceLobbyChooserSubButton subButton)
						{
							if (!LettuceVillageDataUtil.IsDifficultyUnlocked(subButton.GetDifficulty()))
							{
								subButton.LockButton(MercenariesDataUtil.MercenariesBountyLockedReason.PVE_BUILDING_NEEDS_UPGRADE);
							}
						});
					}
					HandleDefaultButtonExpansion(bountySetButtonInfo, previousMapSetId, previousMapDifficulty, thisIsFirstButtonInList);
				}
			});
			m_buttonWidgets.Add(buttonWidget);
			firstTimeThroughLoop = false;
		}
	}

	private BountySetButtonInfo CreateBountySetButtons(LettuceLobbyChooserButton button, LettuceBountySetDbfRecord bountySet, Dictionary<LettuceBounty.MercenariesBountyDifficulty, BountySetBountyCounts> counts)
	{
		BountySetButtonInfo result = new BountySetButtonInfo
		{
			ChooserButton = button,
			BountySet = bountySet
		};
		button.m_visualController.SetState(bountySet.ShortGuid);
		if (!string.IsNullOrEmpty(bountySet.TileArtTexture))
		{
			AssetLoader.Get().LoadTexture(bountySet.TileArtTexture, delegate(AssetReference assetRef, UnityEngine.Object obj, object callbackData)
			{
				Renderer portraitRenderer = button.m_PortraitRenderer;
				if (portraitRenderer != null)
				{
					portraitRenderer.GetMaterial().mainTexture = obj as Texture;
				}
			});
		}
		int totalNewCount = 0;
		foreach (LettuceBounty.MercenariesBountyDifficulty difficulty in Enum.GetValues(typeof(LettuceBounty.MercenariesBountyDifficulty)))
		{
			if (difficulty != 0 && counts.TryGetValue(difficulty, out var bountyCountForDifficulty) && bountyCountForDifficulty.Total != 0)
			{
				int newCount = ((counts != null && counts.ContainsKey(difficulty) && (difficulty < LettuceBounty.MercenariesBountyDifficulty.HEROIC || LettuceVillageDataUtil.IsDifficultyUnlocked(difficulty))) ? bountyCountForDifficulty.New : 0);
				totalNewCount += newCount;
				string glueString = $"GLUE_LETTUCE_{difficulty.ToString()}_SUB_BUTTON";
				result.AddSubButton(button.CreateLettuceLobbySubButton(GameStrings.Get(glueString), SceneMgr.Mode.LETTUCE_BOUNTY_BOARD, bountySet, difficulty, m_DefaultChooserSubButtonPrefab, useAsLastSelected: false, newCount));
			}
		}
		button.SetNewCount(totalNewCount);
		return result;
	}

	private void ConfigureButtonWhenPreviousBountyIsUnfinished(BountySetButtonInfo buttonInfo, int activeSetId, LettuceBounty.MercenariesBountyDifficulty activeDifficulty)
	{
		if (buttonInfo.BountySet.ID == activeSetId)
		{
			buttonInfo.ChooserButton.ToggleButton(toggle: true);
			buttonInfo.ChooserButton.GetComponent<UIBHighlight>().AlwaysOver = true;
			buttonInfo.ChooserButton.m_glowRenderer.SetMaterial(m_activeRunGlowMaterial);
			buttonInfo.ForEachDifficultySubButton(delegate(LettuceLobbyChooserSubButton subButton)
			{
				if (activeDifficulty == subButton.GetDifficulty())
				{
					subButton.TriggerRelease();
					m_SelectedSubButton = subButton;
				}
				else
				{
					subButton.LockButton(MercenariesDataUtil.MercenariesBountyLockedReason.CURRENT_BOUNTY_UNFINISHED);
				}
			});
		}
		else
		{
			buttonInfo.ChooserButton.SetDesaturate(desaturate: true);
			buttonInfo.ForEachDifficultySubButton(delegate(LettuceLobbyChooserSubButton subButton)
			{
				subButton.LockButton(MercenariesDataUtil.MercenariesBountyLockedReason.CURRENT_BOUNTY_UNFINISHED);
			});
		}
	}

	private void ConfigureButtonFromEventStatus(BountySetButtonInfo buttonInfo, MercenariesDataUtil.MercenariesBountyLockedReason lockReason)
	{
		buttonInfo.ChooserButton.SetDesaturate(desaturate: true);
		buttonInfo.ForEachDifficultySubButton(delegate(LettuceLobbyChooserSubButton subButton)
		{
			subButton.LockButton(lockReason);
		});
	}

	private void HandleDefaultButtonExpansion(BountySetButtonInfo buttonInfo, int previousSetId, LettuceBounty.MercenariesBountyDifficulty previousDifficulty, bool thisIsTheFirstButtonInList)
	{
		if (previousSetId != -1)
		{
			if (buttonInfo.BountySet.ID == previousSetId)
			{
				buttonInfo.ChooserButton.ToggleButton(toggle: true);
				LettuceLobbyChooserSubButton subButton = buttonInfo.GetSubButtonForDifficulty(previousDifficulty);
				if (subButton != null)
				{
					subButton.TriggerRelease();
					m_SelectedSubButton = subButton;
				}
			}
		}
		else if (thisIsTheFirstButtonInList)
		{
			buttonInfo.ChooserButton.ToggleButton(toggle: true);
			LettuceLobbyChooserSubButton subButton2 = buttonInfo.GetSubButtonForDifficulty(LettuceBounty.MercenariesBountyDifficulty.NORMAL);
			if (subButton2 != null)
			{
				subButton2.TriggerRelease();
				m_SelectedSubButton = subButton2;
			}
		}
	}

	private void RemoveChooserButtons(bool destroyObjects)
	{
		foreach (WidgetInstance widget in m_buttonWidgets)
		{
			if (widget != null)
			{
				UnregisterButtonFromOwningWidget(widget);
				if (destroyObjects)
				{
					UnityEngine.Object.Destroy(widget.gameObject);
				}
			}
		}
		m_SelectedSubButton = null;
		m_ChooserButtons.Clear();
		m_buttonWidgets.Clear();
	}

	private WidgetInstance CreateChooserButton(string prefab, string chooserButtonName, Action<LettuceLobbyChooserButton> OnButtonReadyCallback)
	{
		WidgetInstance widget = WidgetInstance.Create(prefab);
		m_numButtonsLoading++;
		widget.RegisterReadyListener(delegate
		{
			LettuceLobbyChooserButton newbutton = widget.transform.GetComponentInChildren<LettuceLobbyChooserButton>();
			if (!(newbutton == null))
			{
				GameUtils.SetParent(widget, m_ChooseFrameScroller.ScrollObject);
				if (m_popupRoot != null)
				{
					m_popupRoot.ApplyPopupRendering(widget.transform, m_popupRenderers, overrideLayer: true, 31);
				}
				newbutton.SetButtonText(chooserButtonName);
				newbutton.AddVisualUpdatedListener(base.OnButtonVisualUpdated);
				int index = m_ChooserButtons.Count;
				newbutton.AddToggleListener(delegate(bool toggle)
				{
					OnChooserButtonToggled(newbutton, toggle, index);
				});
				newbutton.AddModeSelectionListener(ButtonModeSelected);
				newbutton.AddExpandedListener(ButtonExpanded);
				m_ChooserButtons.Add(newbutton);
				if (OnButtonReadyCallback != null)
				{
					OnButtonReadyCallback(newbutton);
				}
				newbutton.FireVisualUpdatedEvent();
				m_numButtonsLoading--;
			}
		});
		RegisterButtonWithOwningWidget(widget);
		return widget;
	}

	private void RegisterButtonWithOwningWidget(WidgetInstance instance)
	{
		if (m_owningWidget != null)
		{
			m_owningWidget.AddNestedInstance(instance);
		}
	}

	private void UnregisterButtonFromOwningWidget(WidgetInstance instance)
	{
		if (m_owningWidget != null)
		{
			m_owningWidget.RemoveNestedInstance(instance);
		}
	}

	private void ButtonModeSelected(ChooserSubButton btn)
	{
		foreach (ChooserButton chooserButton in m_ChooserButtons)
		{
			chooserButton.DisableSubButtonHighlights();
		}
		LettuceLobbyChooserSubButton lettuceBtn = btn as LettuceLobbyChooserSubButton;
		m_selectedOption.SetInfo(lettuceBtn.GetMode(), lettuceBtn.GetBountySetRecord(), lettuceBtn.GetDifficulty(), lettuceBtn.GetLockedReason(), lettuceBtn.GetCustomLockedText());
		if (this.OnModeSelected != null)
		{
			this.OnModeSelected();
		}
	}

	protected void ButtonExpanded(ChooserButton button, bool expand)
	{
		if (expand)
		{
			ToggleScrollable(enable: true);
		}
	}

	private EventStatus GetEventStatus(string eventName)
	{
		EventTimingManager eventTimingManager = EventTimingManager.Get();
		EventTimingType eventType = eventTimingManager.GetEventType(eventName);
		if (eventType == EventTimingType.UNKNOWN)
		{
			return EventStatus.DISABLED;
		}
		if (eventTimingManager.IsEventActive(eventType))
		{
			return EventStatus.ACTIVE;
		}
		if (eventType == EventTimingType.SPECIAL_EVENT_NEVER || eventTimingManager.IsEventForcedInactive(eventType))
		{
			return EventStatus.DISABLED;
		}
		if (eventTimingManager.HasEventEnded(eventType))
		{
			return EventStatus.ENDED;
		}
		return EventStatus.NOT_YET_STARTED;
	}

	public void EnablePopupRendering(IPopupRoot popupRoot)
	{
		m_popupRoot = popupRoot;
	}

	public void DisablePopupRendering()
	{
		if (m_popupRoot != null)
		{
			m_popupRoot.CleanupPopupRendering(m_popupRenderers);
		}
	}

	public bool HandlesChildPropagation()
	{
		return true;
	}
}
