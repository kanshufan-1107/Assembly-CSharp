using System.Linq;
using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class RewardTrackDisplay : MonoBehaviour
{
	public const string TURN_PAGE_LEFT = "CODE_TURN_PAGE_LEFT";

	public const string TURN_PAGE_RIGHT = "CODE_TURN_PAGE_RIGHT";

	public const string TURN_PAGE_LEFT_CLICKED = "CODE_TURN_PAGE_LEFT_CLICKED";

	public const string TURN_PAGE_LEFT_RELEASED = "CODE_TURN_PAGE_LEFT_RELEASED";

	public const string TURN_PAGE_RIGHT_CLICKED = "CODE_TURN_PAGE_RIGHT_CLICKED";

	public const string TURN_PAGE_RIGHT_RELEASED = "CODE_TURN_PAGE_RIGHT_RELEASED";

	public const string TURN_PAGE_LEFT_DRAG_END = "CODE_TURN_PAGE_LEFT_DRAG_END";

	public const string TURN_PAGE_RIGHT_DRAG_END = "CODE_TURN_PAGE_RIGHT_DRAG_END";

	public const string TURN_ANIM_FINISHED = "CODE_TURN_ANIM_FINISHED";

	public const string NEXT_LEFT_PAGE_FINISHED = "CODE_NEXT_LEFT_PAGE_FINISHED";

	public const string NEXT_RIGHT_PAGE_FINISHED = "CODE_NEXT_RIGHT_PAGE_FINISHED";

	public const string HIDE_LEFT_LINE = "CODE_HIDE_LEFT_LINE";

	public const string HIDE_RIGHT_LINE = "CODE_HIDE_RIGHT_LINE";

	public const string SHOW_LEFT_LINE = "CODE_SHOW_LEFT_LINE";

	public const string SHOW_RIGHT_LINE = "CODE_SHOW_RIGHT_LINE";

	private const string APPRENTICE_MAGE_NODE_CLICKED = "CODE_APPRENTICE_MAGE_NODE_CLICKED";

	private const string REWARD_TRACK_ITEM_CLICKED = "REWARD_CLICKED";

	private const float PAGE_TURN_LEFT = -1f;

	private const float PAGE_TURN_RIGHT = 1f;

	[SerializeField]
	private Assets.Achievement.RewardTrackType m_rewardTrackType = Assets.Achievement.RewardTrackType.GLOBAL;

	[SerializeField]
	private WidgetInstance m_mapSegment1;

	[SerializeField]
	private WidgetInstance m_mapSegment2;

	[SerializeField]
	private int m_numberOfItemsPerPage = 5;

	[SerializeField]
	public float m_fastPagingDelay = 2f;

	[SerializeField]
	public int m_fastPagingPagesPerSecond = 5;

	private WidgetTemplate m_widget;

	private WidgetInstance m_activePageInstance;

	private bool m_isChangingPage;

	private float m_pageTurnDirection = -1f;

	private float m_fastPagingStartSeconds;

	private bool m_fastPagingCountingPages;

	private bool m_fastPagingLeft;

	private RewardTrack m_rewardTrack;

	[Overridable]
	public int CurrentPageNumber
	{
		get
		{
			return m_rewardTrack.CurrentPageNumber;
		}
		set
		{
			int totalPages = m_rewardTrack.TotalPages;
			int newPageNumber = Mathf.Clamp(value, 1, totalPages);
			if (CurrentPageNumber > 0 && newPageNumber != CurrentPageNumber)
			{
				TurnToPage(newPageNumber);
			}
		}
	}

	public float WidgetTransformWidth
	{
		get
		{
			WidgetTransform widgetTransform = GetComponent<WidgetTransform>();
			if (widgetTransform == null)
			{
				return 0f;
			}
			return (base.transform.localToWorldMatrix * widgetTransform.Rect.size).x;
		}
	}

	private WidgetInstance NextPageInstance
	{
		get
		{
			if (m_activePageInstance == m_mapSegment1)
			{
				return m_mapSegment2;
			}
			return m_mapSegment1;
		}
	}

	private void Awake()
	{
		if (m_mapSegment1 == null || m_mapSegment2 == null)
		{
			Debug.LogError("RewardTrackDisplay: Map Instances not set and the reward track will not load.");
			return;
		}
		m_rewardTrack = RewardTrackManager.Get().GetRewardTrack(m_rewardTrackType);
		if (m_rewardTrack == null || !m_rewardTrack.IsValid)
		{
			Debug.LogError($"RewardTrackDisplay: Reward track for type {m_rewardTrackType} not found.");
			return;
		}
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.BindDataModel(m_rewardTrack.TrackDataModel);
		m_widget.BindDataModel(m_rewardTrack.PageDataModel);
		m_widget.RegisterEventListener(delegate(string eventName)
		{
			switch (eventName)
			{
			case "CODE_TURN_PAGE_LEFT":
				CurrentPageNumber--;
				break;
			case "CODE_TURN_PAGE_RIGHT":
				CurrentPageNumber++;
				break;
			case "CODE_TURN_PAGE_LEFT_CLICKED":
				m_fastPagingStartSeconds = Time.time;
				m_fastPagingCountingPages = true;
				m_fastPagingLeft = true;
				break;
			case "CODE_TURN_PAGE_LEFT_RELEASED":
				TurnPages(left: true);
				break;
			case "CODE_TURN_PAGE_RIGHT_CLICKED":
				m_fastPagingStartSeconds = Time.time;
				m_fastPagingCountingPages = true;
				m_fastPagingLeft = false;
				break;
			case "CODE_TURN_PAGE_RIGHT_RELEASED":
				TurnPages(left: false);
				break;
			case "CODE_TURN_ANIM_FINISHED":
				m_activePageInstance = NextPageInstance;
				NextPageInstance.gameObject.SetActive(value: false);
				m_isChangingPage = false;
				break;
			case "CODE_TURN_PAGE_LEFT_DRAG_END":
				if (m_fastPagingCountingPages)
				{
					TurnPages(left: true);
				}
				break;
			case "CODE_TURN_PAGE_RIGHT_DRAG_END":
				if (m_fastPagingCountingPages)
				{
					TurnPages(left: false);
				}
				break;
			case "CODE_APPRENTICE_MAGE_NODE_CLICKED":
			{
				RewardListDataModel rewardListDataModel = new RewardListDataModel
				{
					ChooseOne = false
				};
				RewardItemDataModel item = new RewardItemDataModel
				{
					ItemType = RewardItemType.HERO_CLASS,
					HeroClassId = 4
				};
				rewardListDataModel.Items.Add(item);
				RewardTrackNodeRewardsDataModel payload = new RewardTrackNodeRewardsDataModel
				{
					RewardTrackType = 8,
					Items = rewardListDataModel
				};
				SendEventUpwardStateAction.SendEventUpward(base.gameObject, "REWARD_CLICKED", new EventDataModel
				{
					Payload = payload,
					SourceName = "CODE_APPRENTICE_MAGE_NODE_CLICKED"
				});
				break;
			}
			}
		});
		m_numberOfItemsPerPage = Mathf.Max(1, m_numberOfItemsPerPage);
		m_activePageInstance = m_mapSegment1;
	}

	private void OnEnable()
	{
		InitializePages();
	}

	private void Update()
	{
		if ((m_isChangingPage && m_fastPagingCountingPages) || InputCollection.GetMouseButtonUp(0))
		{
			m_fastPagingCountingPages = false;
		}
		if (m_fastPagingCountingPages)
		{
			int totalPages = m_rewardTrack.TotalPages;
			int pageNumber = CurrentPageNumber;
			if (Time.time - m_fastPagingStartSeconds > m_fastPagingDelay)
			{
				pageNumber += FastPagingPagesSinceClick() * ((!m_fastPagingLeft) ? 1 : (-1));
			}
			pageNumber = Mathf.Max(pageNumber, 1);
			RewardTrackDbfRecord rewardTrackAsset = m_rewardTrack.RewardTrackAsset;
			int maxLevel = GameUtils.GetRewardTrackLevelsForRewardTrack(rewardTrackAsset.ID).Count;
			int maxPages = Mathf.CeilToInt((float)Mathf.Min(((rewardTrackAsset.LevelCapSoft > 0) ? rewardTrackAsset.LevelCapSoft : maxLevel) + 1, maxLevel) / (float)m_numberOfItemsPerPage);
			pageNumber = Mathf.Min(pageNumber, maxPages);
			m_rewardTrack.PageDataModel.InfoText = GameStrings.Format("GLUE_PROGRESSION_REWARD_TRACK_PAGE_NUMBER", pageNumber, totalPages);
		}
	}

	private void InitializePages()
	{
		int trackLevel = m_rewardTrack.TrackDataModel.Level;
		int softCap = m_rewardTrack.RewardTrackAsset.LevelCapSoft;
		int unclaimedMinLevel = int.MaxValue;
		foreach (RewardTrackLevelDbfRecord record in GameUtils.GetRewardTrackLevelsForRewardTrack(m_rewardTrack.RewardTrackId))
		{
			int level = record.Level;
			bool num = level <= trackLevel;
			bool isLevelPastSoftCap = softCap > 0 && level > softCap;
			if (num && !isLevelPastSoftCap && m_rewardTrack.HasUnclaimedRewardsForLevel(record) && level < unclaimedMinLevel)
			{
				unclaimedMinLevel = level;
			}
		}
		int startingLevel = Mathf.Min(unclaimedMinLevel, trackLevel + 1);
		if (m_rewardTrack.TrackDataModel.RewardTrackType == Global.RewardTrackType.APPRENTICE)
		{
			startingLevel++;
		}
		if (softCap > 0)
		{
			startingLevel = Mathf.Min(startingLevel, softCap);
		}
		SetPageData(Mathf.CeilToInt((float)startingLevel / (float)m_numberOfItemsPerPage), m_activePageInstance);
	}

	private int FastPagingPagesSinceClick()
	{
		return Mathf.FloorToInt((Time.time - m_fastPagingStartSeconds - m_fastPagingDelay) * (float)m_fastPagingPagesPerSecond);
	}

	private void TurnPages(bool left)
	{
		int pagesFlipped = 1;
		if (Time.time - m_fastPagingStartSeconds > m_fastPagingDelay)
		{
			pagesFlipped = Mathf.Max(pagesFlipped, FastPagingPagesSinceClick());
		}
		CurrentPageNumber += (left ? (pagesFlipped * -1) : pagesFlipped);
		m_fastPagingCountingPages = false;
	}

	private void SetPageData(int pageNumber, WidgetInstance widgetInstance)
	{
		ClearPageData(widgetInstance);
		if (m_rewardTrack.TrackDataModel.RewardTrackType == Global.RewardTrackType.APPRENTICE)
		{
			m_rewardTrack.SetApprenticeTrackNodePage(pageNumber, m_numberOfItemsPerPage);
		}
		else
		{
			m_rewardTrack.SetRewardTrackNodePage(pageNumber, m_numberOfItemsPerPage);
		}
		widgetInstance.BindDataModel(m_rewardTrack.NodesDataModel.CloneDataModel());
		widgetInstance.BindDataModel(m_rewardTrack.PageDataModel.CloneDataModel());
	}

	private void ClearPageData(WidgetInstance widgetInstance)
	{
		widgetInstance.UnbindDataModel(231);
		widgetInstance.UnbindDataModel(255);
	}

	private void TurnToPage(int pageNumber)
	{
		if (m_isChangingPage)
		{
			return;
		}
		m_isChangingPage = true;
		WidgetInstance currentPageInstance = m_activePageInstance;
		WidgetInstance nextPageInstance = NextPageInstance;
		int delta = pageNumber - CurrentPageNumber;
		m_pageTurnDirection = ((delta < 0) ? (-1f) : 1f);
		Vector3 currentPagePosition = currentPageInstance.transform.position;
		Vector3 nextPagePosition = nextPageInstance.transform.position;
		nextPagePosition.x = currentPagePosition.x + m_pageTurnDirection * WidgetTransformWidth;
		nextPageInstance.transform.position = nextPagePosition;
		nextPageInstance.gameObject.SetActive(value: true);
		nextPageInstance.Hide();
		SetPageData(pageNumber, nextPageInstance);
		nextPageInstance.RegisterDoneChangingStatesListener(delegate
		{
			string eventName = ((delta < 0) ? "CODE_NEXT_LEFT_PAGE_FINISHED" : "CODE_NEXT_RIGHT_PAGE_FINISHED");
			m_widget.TriggerEvent(eventName, new TriggerEventParameters(null, null, noDownwardPropagation: true));
			nextPageInstance.Show();
			Listable componentInChildren = currentPageInstance.GetComponentInChildren<Listable>();
			if (componentInChildren != null && componentInChildren.WidgetItems.Count() > 0)
			{
				if (m_pageTurnDirection == -1f)
				{
					componentInChildren.WidgetItems.First().TriggerEvent("CODE_HIDE_LEFT_LINE");
				}
				else
				{
					componentInChildren.WidgetItems.Last().TriggerEvent("CODE_HIDE_RIGHT_LINE");
				}
			}
			componentInChildren = nextPageInstance.GetComponentInChildren<Listable>();
			if (componentInChildren != null && componentInChildren.WidgetItems.Count() > 0)
			{
				if (CurrentPageNumber != 1 && m_pageTurnDirection == 1f)
				{
					componentInChildren.WidgetItems.First().TriggerEvent("CODE_SHOW_LEFT_LINE");
				}
				else
				{
					componentInChildren.WidgetItems.Last().TriggerEvent("CODE_SHOW_RIGHT_LINE");
				}
			}
		}, null, callImmediatelyIfSet: true, doOnce: true);
	}
}
