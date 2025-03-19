using System;
using Assets;
using Hearthstone.Core.Streaming;
using Hearthstone.Streaming;
using UnityEngine;

public class UnopenedPack : PegUIElement
{
	public UnopenedPackStack m_SingleStack;

	public UnopenedPackStack m_MultipleStack;

	public GameObject m_LockRibbon;

	public GameObject m_AmountBanner;

	public UberText m_AmountText;

	public UberText m_LockedRibbonText;

	public Spell m_AlertEvent;

	public Spell m_DragStartEvent;

	public Spell m_DragStopEvent;

	public DragRotatorInfo m_DragRotatorInfo = new DragRotatorInfo
	{
		m_PitchInfo = new DragRotatorAxisInfo
		{
			m_ForceMultiplier = 3f,
			m_MinDegrees = -55f,
			m_MaxDegrees = 55f,
			m_RestSeconds = 2f
		},
		m_RollInfo = new DragRotatorAxisInfo
		{
			m_ForceMultiplier = 4.5f,
			m_MinDegrees = -60f,
			m_MaxDegrees = 60f,
			m_RestSeconds = 2f
		}
	};

	private int m_boosterDbId;

	private int m_count;

	private UnopenedPack m_draggedPack;

	private UnopenedPack m_creatorPack;

	private bool m_isRotatedPack;

	private bool m_isCenterPack;

	protected override void Awake()
	{
		base.Awake();
		UpdateState();
	}

	public int GetBoosterId()
	{
		return m_boosterDbId;
	}

	public int GetCount()
	{
		return m_count;
	}

	public void SetBoosterId(int boosterDbId)
	{
		m_boosterDbId = boosterDbId;
		if (GameDbf.Booster.GetRecord(boosterDbId) != null)
		{
			m_isRotatedPack = GameUtils.IsBoosterRotated((BoosterDbId)boosterDbId, DateTime.UtcNow);
		}
		UpdateState();
	}

	public void SetCount(int count)
	{
		m_count = count;
		UpdateState();
	}

	public void AddBoosters(int numNewBoosters)
	{
		m_count += numNewBoosters;
		UpdateState();
	}

	public void AddBooster()
	{
		AddBoosters(1);
	}

	public void RemoveBooster(int numPacks)
	{
		m_count -= numPacks;
		if (m_count < 0)
		{
			Debug.LogWarning("UnopenedPack.RemoveBooster(): Removed a booster pack from a stack with no boosters");
			m_count = 0;
		}
		UpdateState();
	}

	public UnopenedPack AcquireDraggedPack()
	{
		if (m_draggedPack != null)
		{
			return m_draggedPack;
		}
		Vector3 spawnLocation = base.transform.position;
		spawnLocation.y -= 5000f;
		m_draggedPack = UnityEngine.Object.Instantiate(this, spawnLocation, base.transform.rotation);
		TransformUtil.CopyWorldScale(m_draggedPack, this);
		m_draggedPack.transform.parent = base.transform.parent;
		UIBScrollableItem scrollableItem = m_draggedPack.GetComponent<UIBScrollableItem>();
		if (scrollableItem != null)
		{
			scrollableItem.m_active = UIBScrollableItem.ActiveState.Inactive;
		}
		m_draggedPack.m_creatorPack = this;
		m_draggedPack.gameObject.AddComponent<DragRotator>().SetInfo(m_DragRotatorInfo);
		m_draggedPack.m_DragStartEvent.Activate();
		return m_draggedPack;
	}

	public void ReleaseDraggedPack()
	{
		if (!(m_draggedPack == null))
		{
			UnopenedPack draggedPack = m_draggedPack;
			m_draggedPack = null;
			draggedPack.m_DragStopEvent.AddStateFinishedCallback(OnDragStopSpellStateFinished, draggedPack);
			draggedPack.m_DragStopEvent.Activate();
			UpdateState();
		}
	}

	public UnopenedPack GetDraggedPack()
	{
		return m_draggedPack;
	}

	public UnopenedPack GetCreatorPack()
	{
		return m_creatorPack;
	}

	public void PlayAlert()
	{
		m_AlertEvent.ActivateState(SpellStateType.BIRTH);
	}

	public void StopAlert()
	{
		m_AlertEvent.ActivateState(SpellStateType.DEATH);
	}

	public bool CanOpenPack()
	{
		string lockedReason = string.Empty;
		return CanOpenPack(out lockedReason);
	}

	public bool CanOpenPack(out string packLockedReason)
	{
		BoosterDbfRecord boosterDBFRecord = GameDbf.Booster.GetRecord(m_boosterDbId);
		packLockedReason = string.Empty;
		if (boosterDBFRecord == null)
		{
			return false;
		}
		if (m_boosterDbId == 629)
		{
			return CanOpenMercenariesPack(boosterDBFRecord, out packLockedReason);
		}
		return CanOpenTraditionalHearthstonePack(boosterDBFRecord, out packLockedReason);
	}

	public void UpdateState()
	{
		if (!m_isCenterPack)
		{
			string packLockedReason = string.Empty;
			bool openable = CanOpenPack(out packLockedReason);
			if (m_LockRibbon != null)
			{
				m_LockRibbon.SetActive(!openable);
			}
			if (!openable && m_LockedRibbonText != null && !string.IsNullOrEmpty(packLockedReason))
			{
				m_LockedRibbonText.Text = packLockedReason;
			}
			bool empty = GetCount() == 0;
			bool multi = GetCount() > 1;
			m_SingleStack.m_RootObject.SetActive((!multi || !openable) && !empty);
			m_MultipleStack.m_RootObject.SetActive(multi && !empty && openable);
			m_AmountBanner.SetActive(multi);
			m_AmountText.enabled = multi;
			if (multi)
			{
				m_AmountText.Text = GetCount().ToString();
			}
		}
	}

	public void SetUpToBeCenterPack(int bannerCount, Transform centerBone)
	{
		m_isCenterPack = true;
		base.gameObject.transform.position = centerBone.position;
		base.gameObject.transform.rotation = centerBone.rotation;
		if (m_LockRibbon != null)
		{
			m_LockRibbon.SetActive(value: false);
			m_LockedRibbonText = null;
		}
		if (bannerCount == 1)
		{
			m_SingleStack.m_RootObject.SetActive(value: true);
			m_MultipleStack.m_RootObject.SetActive(value: false);
			return;
		}
		m_SingleStack.m_RootObject.SetActive(value: false);
		m_MultipleStack.m_RootObject.SetActive(value: true);
		m_AmountBanner.SetActive(value: true);
		m_AmountText.enabled = true;
		m_AmountText.Text = bannerCount.ToString();
	}

	private void OnDragStopSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (spell.GetActiveState() == SpellStateType.NONE)
		{
			UnityEngine.Object.Destroy(((UnopenedPack)userData).gameObject);
		}
	}

	private bool CanOpenPackBasedOnEventTiming(BoosterDbfRecord boosterDBFRecord)
	{
		EventTimingType openPackEvent = boosterDBFRecord.OpenPackEvent;
		switch (openPackEvent)
		{
		case EventTimingType.UNKNOWN:
			return false;
		case EventTimingType.IGNORE:
			return true;
		default:
			if (EventTimingManager.Get().IsEventActive(openPackEvent))
			{
				return true;
			}
			return false;
		}
	}

	private bool CanOpenTraditionalHearthstonePack(BoosterDbfRecord boosterDBFRecord, out string packLockedReason)
	{
		packLockedReason = string.Empty;
		if (m_isRotatedPack && !RankMgr.Get().WildCardsAllowedInCurrentLeague())
		{
			packLockedReason = GameStrings.Get("GLUE_NEW_PLAYER_AVAILABLE_AT_LEAGUE_PROMO");
			return false;
		}
		return CanOpenPackBasedOnEventTiming(boosterDBFRecord);
	}

	private bool CanOpenMercenariesPack(BoosterDbfRecord boosterDBFRecord, out string packLockedReason)
	{
		packLockedReason = string.Empty;
		if (!GameDownloadManagerProvider.Get().IsModuleReadyToPlay(DownloadTags.Content.Merc))
		{
			packLockedReason = GameStrings.Get("GLUE_MERCENARY_PACK_UNAVAILABLE_DOWNLOAD_REQUIRED");
			return false;
		}
		if (!NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().MercenariesPackOpeningEnabled)
		{
			packLockedReason = GameStrings.Get("GLUE_MERCENARY_PACK_UNAVAILABLE");
			return false;
		}
		if (!LettuceTutorialUtils.IsEventTypeComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_SHOP_CLAIM_PACK_POPUP))
		{
			packLockedReason = GameStrings.Get("GLUE_MERCENARY_TUTORIAL_INCOMPLETE_FOR_PACK");
			return false;
		}
		return CanOpenPackBasedOnEventTiming(boosterDBFRecord);
	}
}
