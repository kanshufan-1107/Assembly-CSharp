using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.MaterialService.Extensions;
using PegasusUtil;
using UnityEngine;

[CustomEditClass]
public class AdventureWing : MonoBehaviour
{
	[Serializable]
	public class BackgroundRandomization
	{
		public MeshRenderer m_backgroundRenderer;

		public string m_materialTextureName = "_MainTex";
	}

	public delegate void BossSelected(AdventureBossCoin coin, ScenarioDbId mission);

	public delegate void OpenPlateStart(AdventureWing wing);

	public delegate void OpenPlateEnd(AdventureWing wing);

	public delegate void ShowRewards(List<RewardData> rewards, Vector3 origin);

	public delegate void HideRewards(List<RewardData> rewards);

	public delegate void ShowRewardsPreview();

	public delegate void TryPurchaseWing();

	public delegate void DelOnCoinAnimateCallback(Vector3 coinPosition);

	public delegate void BringToFocusCallback(AdventureWing wing);

	protected class Boss
	{
		public ScenarioDbId m_MissionId;

		public AdventureBossCoin m_Coin;

		public AdventureRewardsChest m_Chest;
	}

	[CustomEditField(Sections = "Wing Event Table")]
	public AdventureWingEventTable m_WingEventTable;

	[CustomEditField(Sections = "Containers & Bones")]
	public GameObject m_ContentsContainer;

	[CustomEditField(Sections = "Containers & Bones")]
	public GameObject m_CoinContainer;

	[CustomEditField(Sections = "Containers & Bones")]
	public GameObject m_WallAccentContainer;

	[CustomEditField(Sections = "Containers & Bones")]
	public GameObject m_PlateAccentContainer;

	[CustomEditField(Sections = "Lock Plate")]
	public GameObject m_LockPlate;

	[CustomEditField(Sections = "Lock Plate")]
	public GameObject m_LockPlateFXContainer;

	[CustomEditField(Sections = "UI")]
	public List<UberText> m_WingTitles = new List<UberText>();

	[CustomEditField(Sections = "UI")]
	public PegUIElement m_UnlockButton;

	[CustomEditField(Sections = "UI")]
	public PegUIElement m_BuyButton;

	[CustomEditField(Sections = "UI")]
	public MeshRenderer m_BuyButtonMesh;

	[CustomEditField(Sections = "UI")]
	public UberText m_BuyButtonText;

	[CustomEditField(Sections = "UI")]
	public UberText m_ReleaseLabelText;

	[CustomEditField(Sections = "UI")]
	public PegUIElement m_RewardsPreviewButton;

	[CustomEditField(Sections = "UI")]
	public GameObject m_PurchasedBanner;

	[CustomEditField(Sections = "Wing Rewards")]
	public PegUIElement m_BigChest;

	[CustomEditField(Sections = "Random Background Properties", ListTable = true)]
	public List<BackgroundRandomization> m_BackgroundRenderers = new List<BackgroundRandomization>();

	[CustomEditField(Sections = "Random Background Properties")]
	public List<float> m_BackgroundOffsets = new List<float>();

	[CustomEditField(Sections = "Special UI")]
	public bool m_BuyButtonOnOppositeSideOfKey;

	[CustomEditField(Sections = "Special UI/LOE")]
	public MeshRenderer m_UnlockButtonHighlightMesh_LOE;

	[CustomEditField(Sections = "Special UI/LOE")]
	public float m_UnlockButtonHighlightIntensityOut = 1.52f;

	[CustomEditField(Sections = "Special UI/LOE")]
	public float m_UnlockButtonHighlightIntensityOver = 2f;

	[CustomEditField(Sections = "Special UI/KARA")]
	public PlayMakerFSM m_prologueLoadingPlayMakerFSM_KARA;

	[SerializeField]
	private float m_CoinSpacing = 25f;

	[SerializeField]
	private Vector3 m_CoinsOffset = Vector3.zero;

	[SerializeField]
	private Vector3 m_CoinsChestOffset = Vector3.zero;

	protected AdventureWingDef m_WingDef;

	private Spell m_UnlockSpell;

	private GameObject m_WallAccentObject;

	private GameObject m_PlateAccentObject;

	private List<Boss> m_BossCoins = new List<Boss>();

	private BringToFocusCallback m_BringToFocusCallback;

	private bool m_Owned;

	private bool m_Playable;

	private bool m_Locked;

	private bool m_EventStartDetected;

	private bool m_HasJustAckedProgress;

	private List<BossSelected> m_BossSelectedListeners = new List<BossSelected>();

	private List<OpenPlateStart> m_OpenPlateStartListeners = new List<OpenPlateStart>();

	private List<OpenPlateEnd> m_OpenPlateEndListeners = new List<OpenPlateEnd>();

	private List<ShowRewards> m_ShowRewardsListeners = new List<ShowRewards>();

	private List<HideRewards> m_HideRewardsListeners = new List<HideRewards>();

	private List<ShowRewardsPreview> m_ShowRewardsPreviewListeners = new List<ShowRewardsPreview>();

	private List<TryPurchaseWing> m_TryPurchaseWingListeners = new List<TryPurchaseWing>();

	private static List<int> s_LastRandomNumbers = new List<int>();

	public float CoinSpacing
	{
		get
		{
			return m_CoinSpacing;
		}
		set
		{
			m_CoinSpacing = value;
			UpdateCoinPositions();
		}
	}

	public Vector3 CoinsOffset
	{
		get
		{
			return m_CoinsOffset;
		}
		set
		{
			m_CoinsOffset = value;
			UpdateCoinPositions();
		}
	}

	public Vector3 CoinsChestOffset
	{
		get
		{
			return m_CoinsChestOffset;
		}
		set
		{
			m_CoinsChestOffset = value;
			UpdateCoinPositions();
		}
	}

	public bool IsDevMode { get; set; }

	protected virtual void Awake()
	{
		IsDevMode = AdventureScene.Get() == null || AdventureScene.Get().IsDevMode;
	}

	private void Start()
	{
		if (m_UnlockButton != null)
		{
			m_UnlockButton.AddEventListener(UIEventType.RELEASE, UnlockButtonPressed);
			m_UnlockButton.AddEventListener(UIEventType.ROLLOUT, OnUnlockButtonOut);
			m_UnlockButton.AddEventListener(UIEventType.ROLLOVER, OnUnlockButtonOver);
		}
		if (m_BuyButton != null)
		{
			m_BuyButton.AddEventListener(UIEventType.RELEASE, delegate
			{
				if (IsDevMode)
				{
					SetPlateKeyEvent(initial: false);
				}
				else
				{
					FireTryPurchaseWingEvent();
				}
			});
		}
		if (m_RewardsPreviewButton != null)
		{
			m_RewardsPreviewButton.AddEventListener(UIEventType.RELEASE, delegate
			{
				FireShowRewardsPreviewEvent();
			});
		}
		if (m_BigChest != null)
		{
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				m_BigChest.AddEventListener(UIEventType.RELEASE, ShowBigChestRewards);
				return;
			}
			m_BigChest.AddEventListener(UIEventType.ROLLOVER, ShowBigChestRewards);
			m_BigChest.AddEventListener(UIEventType.ROLLOUT, HideBigChestRewards);
		}
	}

	private void OnDestroy()
	{
		if (StoreManager.Get() != null)
		{
			StoreManager.Get().RemoveStatusChangedListener(UpdateBuyButton);
		}
	}

	private void Update()
	{
		if (m_WingEventTable.IsPlateInOrGoingToAnActiveState() && !IsDevMode)
		{
			if (!m_EventStartDetected && AdventureProgressMgr.IsWingEventActive((int)m_WingDef.GetWingId()))
			{
				UpdatePlateState();
			}
			else if (!m_Owned && AdventureProgressMgr.Get().OwnsWing((int)m_WingDef.GetWingId()))
			{
				UpdatePlateState();
			}
		}
		if (!IsDevMode)
		{
			return;
		}
		if (InputCollection.GetKeyDown(KeyCode.Alpha1))
		{
			m_LockPlate.SetActive(value: true);
			SetPlateKeyEvent(initial: true);
		}
		if (InputCollection.GetKeyDown(KeyCode.Alpha2))
		{
			m_LockPlate.SetActive(value: true);
			m_WingEventTable.DoStatePlateBuy(initial: true);
		}
		if (InputCollection.GetKeyDown(KeyCode.Alpha3))
		{
			m_LockPlate.SetActive(value: true);
			m_WingEventTable.DoStatePlateInitialText();
		}
		if (InputCollection.GetKeyDown(KeyCode.Alpha4))
		{
			m_WingEventTable.DoStatePlateDeactivate();
		}
		if (InputCollection.GetKeyDown(KeyCode.Alpha5))
		{
			m_LockPlate.SetActive(value: true);
			UnlockPlate();
		}
		if (InputCollection.GetKeyDown(KeyCode.Alpha6))
		{
			m_WingEventTable.DoStatePlateDeactivate();
			foreach (Boss bossCoin in m_BossCoins)
			{
				bossCoin.m_Chest.SlamInCheckmark();
			}
		}
		if (InputCollection.GetKeyDown(KeyCode.Alpha7))
		{
			m_WingEventTable.DoStatePlateDeactivate();
			OpenBigChest();
		}
		if (InputCollection.GetKeyDown(KeyCode.Alpha8))
		{
			m_WingEventTable.DoStatePlateDeactivate();
			StartCoroutine(AnimateCoinsAndChests(m_BossCoins, 0f, null));
		}
		if (InputCollection.GetKeyDown(KeyCode.Alpha9))
		{
			m_LockPlate.SetActive(value: true);
			m_WingEventTable.DoStatePlateReset();
		}
	}

	public void Initialize(AdventureWingDef wingDef)
	{
		m_WingDef = wingDef;
		base.gameObject.name = $"{base.gameObject.name}_{(int)wingDef.GetWingId()}";
		foreach (UberText titleObj in m_WingTitles)
		{
			if (titleObj != null)
			{
				titleObj.Text = m_WingDef.GetWingName();
			}
		}
		if (!string.IsNullOrEmpty(wingDef.m_UnlockSpellPrefab) && m_LockPlateFXContainer != null)
		{
			m_UnlockSpell = SpellManager.Get().GetSpell(wingDef.m_UnlockSpellPrefab);
			GameUtils.SetParent(m_UnlockSpell, m_LockPlateFXContainer);
			m_UnlockSpell.gameObject.SetActive(value: false);
		}
		SetAccent(wingDef.m_AccentPrefab);
		m_Owned = AdventureProgressMgr.Get().OwnsWing((int)wingDef.GetWingId());
		m_EventStartDetected = AdventureProgressMgr.IsWingEventActive((int)wingDef.GetWingId());
		m_Playable = m_Owned && m_EventStartDetected;
		m_Locked = AdventureProgressMgr.Get().IsWingLocked(wingDef);
		UpdatePurchasedBanner();
		bool isHeroic = AdventureConfig.Get().GetSelectedMode() == AdventureModeDbId.LINEAR_HEROIC;
		bool isPlateFullyAcked = HasAckedAllPlateOpenEvents();
		AdventureWingKarazhanHelper karaHelper = GetComponent<AdventureWingKarazhanHelper>();
		if (karaHelper != null)
		{
			karaHelper.Initialize();
		}
		if (IsDevMode)
		{
			return;
		}
		if (m_Playable && (isPlateFullyAcked || isHeroic))
		{
			m_WingEventTable.DoStatePlateDeactivate();
			return;
		}
		UpdateBuyButton(StoreManager.Get().IsOpen());
		StoreManager.Get().RegisterStatusChangedListener(UpdateBuyButton);
		m_WingEventTable.DoStatePlateActivate();
		if (m_Locked && m_EventStartDetected)
		{
			m_WingEventTable.DoStatePlateInitialText();
			if (m_ReleaseLabelText != null)
			{
				m_ReleaseLabelText.Text = (m_Owned ? GameStrings.Get(m_WingDef.m_LockedLocString) : GameStrings.Get(m_WingDef.m_LockedPurchaseLocString));
			}
			return;
		}
		bool ownershipPrereqIsOwned = AdventureProgressMgr.Get().OwnershipPrereqWingIsOwned(m_WingDef);
		if (!m_EventStartDetected)
		{
			m_WingEventTable.DoStatePlateInitialText();
			if (m_ReleaseLabelText != null)
			{
				m_ReleaseLabelText.Text = m_WingDef.GetComingSoonLabel();
			}
		}
		else if (!m_Owned && ownershipPrereqIsOwned)
		{
			if (m_prologueLoadingPlayMakerFSM_KARA != null)
			{
				m_prologueLoadingPlayMakerFSM_KARA.SendEvent("on");
			}
			m_WingEventTable.DoStatePlateBuy(initial: true);
		}
		else if (m_Owned && !isPlateFullyAcked)
		{
			SetPlateKeyEvent(initial: true);
		}
		else
		{
			m_WingEventTable.DoStatePlateInitialText();
			if (m_ReleaseLabelText != null)
			{
				m_ReleaseLabelText.Text = m_WingDef.GetRequiresLabel();
			}
		}
	}

	public void InitializeDevMode()
	{
		if (IsDevMode)
		{
			int devModeSetting = AdventureScene.Get().DevModeSetting;
			m_WingEventTable.DoStatePlateActivate();
			switch (devModeSetting)
			{
			case 1:
				SetPlateKeyEvent(initial: true);
				break;
			case 2:
				m_WingEventTable.DoStatePlateInitialText();
				break;
			}
			if (m_ReleaseLabelText != null)
			{
				m_ReleaseLabelText.Text = m_WingDef.GetComingSoonLabel();
			}
		}
	}

	public AdventureWingDef GetWingDef()
	{
		return m_WingDef;
	}

	public WingDbId GetWingId()
	{
		return m_WingDef.GetWingId();
	}

	public List<AdventureRewardsChest> GetChests()
	{
		List<AdventureRewardsChest> chests = new List<AdventureRewardsChest>();
		foreach (Boss coin in m_BossCoins)
		{
			chests.Add(coin.m_Chest);
		}
		return chests;
	}

	public AdventureDbId GetAdventureId()
	{
		return m_WingDef.GetAdventureId();
	}

	public ProductType GetProductType()
	{
		return StoreManager.GetAdventureProductType(GetAdventureId());
	}

	public int GetProductData()
	{
		return (int)GetWingId();
	}

	public string GetWingName()
	{
		return m_WingDef.GetWingName();
	}

	public void AddBossSelectedListener(BossSelected dlg)
	{
		m_BossSelectedListeners.Add(dlg);
	}

	public void AddOpenPlateStartListener(OpenPlateStart dlg)
	{
		m_OpenPlateStartListeners.Add(dlg);
	}

	public void AddOpenPlateEndListener(OpenPlateEnd dlg)
	{
		m_OpenPlateEndListeners.Add(dlg);
	}

	public void AddShowRewardsListener(ShowRewards dlg)
	{
		m_ShowRewardsListeners.Add(dlg);
	}

	public void AddHideRewardsListener(HideRewards dlg)
	{
		m_HideRewardsListeners.Add(dlg);
	}

	public void AddShowRewardsPreviewListeners(ShowRewardsPreview dlg)
	{
		m_ShowRewardsPreviewListeners.Add(dlg);
	}

	public void AddTryPurchaseWingListener(TryPurchaseWing dlg)
	{
		m_TryPurchaseWingListeners.Add(dlg);
	}

	public bool ContainsBossCoin(AdventureBossCoin coin)
	{
		foreach (Boss bossCoin in m_BossCoins)
		{
			if (bossCoin.m_Coin == coin)
			{
				return true;
			}
		}
		return false;
	}

	public AdventureBossCoin CreateBoss(string coinPrefab, string rewardsPrefab, ScenarioDbId mission, bool enabled)
	{
		AdventureBossCoin newcoin = GameUtils.LoadGameObjectWithComponent<AdventureBossCoin>(coinPrefab);
		AdventureRewardsChest newchest = GameUtils.LoadGameObjectWithComponent<AdventureRewardsChest>(rewardsPrefab);
		newcoin.gameObject.name = $"{newcoin.gameObject.name}_{mission}";
		if (newchest != null)
		{
			newchest.gameObject.name = $"{newchest.gameObject.name}_{mission}";
			UpdateBossChest(newchest, mission);
		}
		if (m_CoinContainer != null)
		{
			GameUtils.SetParent(newcoin, m_CoinContainer);
			if (newchest != null)
			{
				GameUtils.SetParent(newchest, m_CoinContainer);
				TransformUtil.SetLocalPosY(newchest.transform, 0.01f);
			}
		}
		newcoin.Enable(enabled, animate: false);
		newcoin.AddEventListener(UIEventType.RELEASE, delegate
		{
			FireBossSelectedEvent(newcoin, mission);
		});
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			newchest.Enable(enable: false);
			if (newcoin.m_DisabledCollider != null)
			{
				newcoin.m_DisabledCollider.AddEventListener(UIEventType.PRESS, delegate
				{
					ShowBossRewards(mission, newcoin.transform.position);
				});
			}
		}
		else
		{
			newchest.AddChestEventListener(UIEventType.ROLLOVER, delegate
			{
				ShowBossRewards(mission, newchest.transform.position);
			});
			newchest.AddChestEventListener(UIEventType.ROLLOUT, delegate
			{
				HideBossRewards(mission);
			});
		}
		if (m_BossCoins.Count == 0)
		{
			newcoin.ShowConnector(show: false);
		}
		m_BossCoins.Add(new Boss
		{
			m_MissionId = mission,
			m_Coin = newcoin,
			m_Chest = newchest
		});
		UpdateCoinPositions();
		return newcoin;
	}

	public void UpdateAllBossCoinChests()
	{
		foreach (Boss coin in m_BossCoins)
		{
			UpdateBossChest(coin.m_Chest, coin.m_MissionId);
		}
	}

	public void SetAccent(string accentPrefab)
	{
		if (m_WallAccentObject != null)
		{
			UnityEngine.Object.Destroy(m_WallAccentObject);
		}
		if (m_PlateAccentObject != null)
		{
			UnityEngine.Object.Destroy(m_PlateAccentObject);
		}
		if (!string.IsNullOrEmpty(accentPrefab))
		{
			if (m_WallAccentContainer != null)
			{
				m_WallAccentObject = AssetLoader.Get().InstantiatePrefab(accentPrefab);
				GameUtils.SetParent(m_WallAccentObject, m_WallAccentContainer);
			}
			if (m_PlateAccentContainer != null)
			{
				m_PlateAccentObject = UnityEngine.Object.Instantiate(m_WallAccentObject);
				GameUtils.SetParent(m_PlateAccentObject, m_PlateAccentContainer);
			}
		}
	}

	public void SetBringToFocusCallback(BringToFocusCallback dlg)
	{
		m_BringToFocusCallback = dlg;
	}

	public void OpenBigChest()
	{
		if (m_BigChest != null)
		{
			m_WingEventTable.DoStateBigChestOpen();
			BringToFocus();
			m_BigChest.RemoveEventListener(UIEventType.PRESS, ShowBigChestRewards);
			m_BigChest.RemoveEventListener(UIEventType.ROLLOVER, ShowBigChestRewards);
			m_BigChest.RemoveEventListener(UIEventType.ROLLOUT, HideBigChestRewards);
		}
	}

	public void HideBigChest()
	{
		m_WingEventTable.DoStateBigChestCover();
	}

	public void BigChestStayOpen()
	{
		m_WingEventTable.DoStateBigChestStayOpen();
	}

	public void SetBigChestRewards(WingDbId wingId)
	{
		if (AdventureConfig.Get().GetSelectedMode() == AdventureModeDbId.LINEAR)
		{
			HashSet<Assets.Achieve.RewardTiming> rewardTimings = new HashSet<Assets.Achieve.RewardTiming> { Assets.Achieve.RewardTiming.ADVENTURE_CHEST };
			List<RewardData> wingRewards = AdventureProgressMgr.GetRewardsForWing((int)wingId, rewardTimings);
			if (m_BigChest != null)
			{
				m_BigChest.SetData(wingRewards);
			}
			AdventureWingFrozenThroneHelper frozenThroneHelper = GetComponent<AdventureWingFrozenThroneHelper>();
			if (frozenThroneHelper != null)
			{
				frozenThroneHelper.SetBigChestRewards(wingId);
			}
		}
	}

	public List<RewardData> GetBigChestRewards()
	{
		if (!(m_BigChest != null))
		{
			return null;
		}
		return (List<RewardData>)m_BigChest.GetData();
	}

	public bool HasBigChestRewards()
	{
		if (m_BigChest != null)
		{
			return m_BigChest.GetData() != null;
		}
		return false;
	}

	public bool UpdateAndAnimateCoinsAndChests(float startDelay, bool forceCoinAnimation, DelOnCoinAnimateCallback dlg)
	{
		if (m_WingEventTable.IsPlateInOrGoingToAnActiveState())
		{
			return false;
		}
		List<Boss> thingsToFlip = new List<Boss>();
		List<KeyValuePair<int, int>> updateProgress = new List<KeyValuePair<int, int>>();
		foreach (Boss boss in m_BossCoins)
		{
			int wingId = 0;
			int missionReqProgress = 0;
			bool newlyAvailable = AdventureConfig.IsMissionNewlyAvailableAndGetReqs((int)boss.m_MissionId, ref wingId, ref missionReqProgress);
			if ((forceCoinAnimation || newlyAvailable) && (!forceCoinAnimation || AdventureProgressMgr.Get().CanPlayScenario((int)boss.m_MissionId)) && !AdventureProgressMgr.Get().HasDefeatedScenario((int)boss.m_MissionId))
			{
				updateProgress.Add(new KeyValuePair<int, int>(wingId, missionReqProgress));
				Boss flip = new Boss();
				flip.m_MissionId = boss.m_MissionId;
				flip.m_Coin = boss.m_Coin;
				if (AdventureProgressMgr.Get().ScenarioHasRewardData((int)boss.m_MissionId))
				{
					flip.m_Chest = boss.m_Chest;
				}
				thingsToFlip.Add(flip);
			}
		}
		foreach (KeyValuePair<int, int> kvpair in updateProgress)
		{
			if (AdventureConfig.SetWingAckIfGreater(kvpair.Key, kvpair.Value))
			{
				AdventureMissionDisplay.Get().SetWingHasJustAckedProgress(kvpair.Key, hasJustAckedProgress: true);
			}
		}
		if (thingsToFlip.Count > 0)
		{
			StartCoroutine(AnimateCoinsAndChests(thingsToFlip, startDelay, dlg));
			return true;
		}
		return false;
	}

	public void UpdatePlateState()
	{
		UpdatePurchasedBanner();
		bool locked = AdventureProgressMgr.Get().IsWingLocked(m_WingDef);
		bool owned = AdventureProgressMgr.Get().OwnsWing((int)m_WingDef.GetWingId());
		bool wingEventActive = AdventureProgressMgr.IsWingEventActive((int)m_WingDef.GetWingId());
		bool playable = owned && wingEventActive;
		TryToUnlockAutomatically();
		if (playable && m_Playable && !locked && !m_Locked)
		{
			return;
		}
		if (playable && !locked && !m_WingEventTable.IsPlateKey())
		{
			if (m_BuyButtonOnOppositeSideOfKey && !m_WingEventTable.IsPlateBuy())
			{
				m_WingEventTable.DoStatePlateBuy();
			}
			if (m_prologueLoadingPlayMakerFSM_KARA != null)
			{
				m_prologueLoadingPlayMakerFSM_KARA.SendEvent("off");
			}
			SetPlateKeyEvent(initial: false);
		}
		else if (!m_WingEventTable.IsPlateBuy())
		{
			if (locked && wingEventActive)
			{
				m_WingEventTable.DoStatePlateInitialText();
				if (m_ReleaseLabelText != null)
				{
					m_ReleaseLabelText.Text = (owned ? GameStrings.Get(m_WingDef.m_LockedLocString) : GameStrings.Get(m_WingDef.m_LockedPurchaseLocString));
				}
			}
			else
			{
				bool ownershipPrereqIsOwned = AdventureProgressMgr.Get().OwnershipPrereqWingIsOwned(m_WingDef);
				if (!wingEventActive)
				{
					m_WingEventTable.DoStatePlateInitialText();
					if (m_ReleaseLabelText != null)
					{
						m_ReleaseLabelText.Text = m_WingDef.GetComingSoonLabel();
					}
				}
				else if (!owned && ownershipPrereqIsOwned)
				{
					m_WingEventTable.DoStatePlateBuy();
				}
				else
				{
					m_WingEventTable.DoStatePlateInitialText();
					if (m_ReleaseLabelText != null)
					{
						m_ReleaseLabelText.Text = m_WingDef.GetRequiresLabel();
					}
				}
			}
		}
		m_EventStartDetected = wingEventActive;
		m_Playable = playable;
		m_Owned = owned;
		m_Locked = locked;
	}

	public void UpdateRewardsPreviewCover()
	{
		if (!HasRewards())
		{
			m_WingEventTable.DoStatePlateCoverPreviewChest();
		}
	}

	public bool HasRewards()
	{
		List<RewardData> bigChestRewards = GetBigChestRewards();
		if (bigChestRewards != null && bigChestRewards.Count > 0)
		{
			return true;
		}
		foreach (Boss boss in m_BossCoins)
		{
			if (AdventureProgressMgr.Get().ScenarioHasRewardData((int)boss.m_MissionId))
			{
				return true;
			}
		}
		return false;
	}

	public void RandomizeBackground()
	{
		if (m_BackgroundOffsets.Count == 0)
		{
			return;
		}
		int rand;
		do
		{
			rand = UnityEngine.Random.Range(0, m_BackgroundOffsets.Count);
		}
		while (s_LastRandomNumbers.Contains(rand));
		s_LastRandomNumbers.Add(rand);
		if (s_LastRandomNumbers.Count >= m_BackgroundOffsets.Count)
		{
			s_LastRandomNumbers.RemoveAt(0);
		}
		foreach (BackgroundRandomization bg in m_BackgroundRenderers)
		{
			if (!(bg.m_backgroundRenderer == null) && !string.IsNullOrEmpty(bg.m_materialTextureName))
			{
				Material material = bg.m_backgroundRenderer.GetMaterial();
				Vector2 newOffset = material.GetTextureOffset(bg.m_materialTextureName);
				newOffset.y = m_BackgroundOffsets[rand];
				material.SetTextureOffset(bg.m_materialTextureName, newOffset);
			}
		}
	}

	public void BringToFocus()
	{
		if (m_BringToFocusCallback != null)
		{
			m_BringToFocusCallback(this);
		}
	}

	public void HideBossChests()
	{
		foreach (Boss boss in m_BossCoins)
		{
			if (boss.m_Chest != null)
			{
				boss.m_Chest.FadeOutChestImmediate();
			}
		}
	}

	public void NavigateBackCleanup()
	{
		if (m_prologueLoadingPlayMakerFSM_KARA != null)
		{
			m_prologueLoadingPlayMakerFSM_KARA.SendEvent("cancel");
		}
	}

	public void GetCompleteQuoteAssetsFromTargetWingEventTiming(int targetWingId, out string completeQuotePrefab, out string completeQuoteVOLine)
	{
		completeQuotePrefab = m_WingDef.m_CompleteQuotePrefab;
		completeQuoteVOLine = m_WingDef.m_CompleteQuoteVOLine;
		if (targetWingId != 0 && !AdventureProgressMgr.IsWingEventActive(targetWingId) && !string.IsNullOrEmpty(m_WingDef.m_CompleteQuoteNextWingLockedPrefab) && !string.IsNullOrEmpty(m_WingDef.m_CompleteQuoteNextWingLockedVOLine))
		{
			completeQuotePrefab = m_WingDef.m_CompleteQuoteNextWingLockedPrefab;
			completeQuoteVOLine = m_WingDef.m_CompleteQuoteNextWingLockedVOLine;
		}
	}

	private IEnumerator AnimateCoinsAndChests(List<Boss> thingsToFlip, float delaySeconds, DelOnCoinAnimateCallback dlg)
	{
		if (delaySeconds > 0f)
		{
			yield return new WaitForSeconds(delaySeconds);
		}
		dlg?.Invoke(thingsToFlip[0].m_Coin.transform.position);
		int i = 0;
		while (i < thingsToFlip.Count)
		{
			Boss boss = thingsToFlip[i];
			StartCoroutine(AnimateOneCoinAndChest(boss));
			yield return new WaitForSeconds(0.2f);
			int num = i + 1;
			i = num;
		}
		yield return new WaitForSeconds(0.5f);
	}

	private IEnumerator AnimateOneCoinAndChest(Boss boss)
	{
		if (boss.m_Chest != null && !AdventureProgressMgr.Get().HasDefeatedScenario((int)boss.m_MissionId))
		{
			boss.m_Chest.BlinkChest();
		}
		yield return new WaitForSeconds(0.5f);
		boss.m_Coin.Enable(flag: true);
		yield return new WaitForSeconds(1f);
		if (boss.m_Chest != null && boss.m_Chest.m_fadedOut)
		{
			boss.m_Chest.FadeInChest();
		}
		boss.m_Coin.ShowNewLookGlow();
	}

	private void UpdateCoinPositions()
	{
		int i = 0;
		foreach (Boss boss in m_BossCoins)
		{
			boss.m_Coin.transform.localPosition = m_CoinsOffset;
			TransformUtil.SetLocalPosX(boss.m_Coin, m_CoinsOffset.x + (float)i * m_CoinSpacing);
			if (boss.m_Chest != null)
			{
				boss.m_Chest.transform.localPosition = m_CoinsOffset;
				TransformUtil.SetLocalPosX(boss.m_Chest, m_CoinsOffset.x + (float)i * m_CoinSpacing);
				boss.m_Chest.transform.localPosition += m_CoinsChestOffset;
			}
			i++;
		}
	}

	private void FireBossSelectedEvent(AdventureBossCoin coin, ScenarioDbId mission)
	{
		BossSelected[] array = m_BossSelectedListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](coin, mission);
		}
	}

	private void FireOpenPlateStartEvent()
	{
		OpenPlateStart[] array = m_OpenPlateStartListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](this);
		}
	}

	protected void FireOpenPlateEndEvent(Spell s)
	{
		if (m_UnlockSpell != null)
		{
			m_UnlockSpell.gameObject.SetActive(value: false);
		}
		OpenPlateEnd[] array = m_OpenPlateEndListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](this);
		}
	}

	private void OnUnlockButtonOut(UIEvent e)
	{
		if (!(m_UnlockButtonHighlightMesh_LOE == null))
		{
			m_UnlockButtonHighlightMesh_LOE.GetMaterial().SetFloat("_Intensity", m_UnlockButtonHighlightIntensityOut);
		}
	}

	private void OnUnlockButtonOver(UIEvent e)
	{
		if (!(m_UnlockButtonHighlightMesh_LOE == null))
		{
			m_UnlockButtonHighlightMesh_LOE.GetMaterial().SetFloat("_Intensity", m_UnlockButtonHighlightIntensityOver);
		}
	}

	private void UnlockButtonPressed(UIEvent e)
	{
		if (m_WingDef != null && !string.IsNullOrEmpty(m_WingDef.GetOpeningNotRecommendedWarning()) && !IsWingRecommendedToOpen())
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
			info.m_text = m_WingDef.GetOpeningNotRecommendedWarning();
			info.m_showAlertIcon = true;
			info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
			info.m_responseCallback = OnConfirmWingUnlockResponse;
			DialogManager.Get().ShowPopup(info);
		}
		else
		{
			UnlockPlate();
		}
	}

	private void OnConfirmWingUnlockResponse(AlertPopup.Response response, object userData)
	{
		if (response == AlertPopup.Response.CONFIRM)
		{
			UnlockPlate();
		}
	}

	public bool TryToUnlockAutomatically()
	{
		if (IsDevMode)
		{
			return false;
		}
		if (!m_WingDef.GetUnlocksAutomatically())
		{
			return false;
		}
		if (!m_WingEventTable.IsPlateKey() && (!m_WingEventTable.IsPlatePartiallyOpen() || !HasDependentWingJustAckedRequiredProgress()))
		{
			return false;
		}
		if (AdventureProgressMgr.Get().IsWingLocked(m_WingDef))
		{
			return false;
		}
		if (!AdventureProgressMgr.Get().OwnsWing((int)m_WingDef.GetWingId()) || !AdventureProgressMgr.IsWingEventActive((int)m_WingDef.GetWingId()))
		{
			return false;
		}
		if (!CanPlayAtLeastOneScenario())
		{
			return false;
		}
		UnlockPlate();
		return true;
	}

	public void UnlockPlate()
	{
		if (m_UnlockButton != null)
		{
			m_UnlockButton.RemoveEventListener(UIEventType.RELEASE, UnlockButtonPressed);
		}
		float animTime = 0f;
		if (m_BringToFocusCallback != null)
		{
			animTime = 0.5f;
			m_BringToFocusCallback(this);
		}
		StartCoroutine(DoUnlockPlate(animTime));
	}

	private IEnumerator DoUnlockPlate(float startDelay)
	{
		FireOpenPlateStartEvent();
		if (startDelay > 0f)
		{
			yield return new WaitForSeconds(startDelay);
		}
		m_WingEventTable.AddOpenPlateEndEventListener(FireOpenPlateEndEvent, once: true);
		if (m_UnlockButton != null)
		{
			m_UnlockButton.GetComponent<Collider>().enabled = false;
		}
		float unlockDelay = 0f;
		if (m_UnlockSpell != null)
		{
			AdventureWingUnlockSpell wingUnlockSpell = m_UnlockSpell.GetComponent<AdventureWingUnlockSpell>();
			unlockDelay = ((wingUnlockSpell != null) ? wingUnlockSpell.m_UnlockDelay : 0f);
		}
		DoOpenPlate(unlockDelay);
		m_ContentsContainer.SetActive(value: true);
		if (m_UnlockSpell != null)
		{
			m_UnlockSpell.gameObject.SetActive(value: true);
			m_UnlockSpell.AddFinishedCallback(OnUnlockSpellFinished);
			m_UnlockSpell.Activate();
		}
		else
		{
			OnUnlockSpellFinished(null, null);
		}
		if (m_UnlockButton != null)
		{
			m_UnlockButton.AddEventListener(UIEventType.RELEASE, UnlockButtonPressed);
		}
	}

	protected virtual void DoOpenPlate(float unlockDelay)
	{
		int progress = AdventureProgressMgr.Get().GetProgressValueForWing((int)m_WingDef.GetWingId());
		int plateOpenIndex = progress - 1;
		AdventureProgressMgr.Get().GetWingAck((int)m_WingDef.GetWingId(), out var wingProgressAck);
		if (wingProgressAck < progress || m_HasJustAckedProgress || IsDevMode)
		{
			if (m_WingEventTable.SupportsIncrementalOpening() || wingProgressAck == 0)
			{
				if (IsDevMode)
				{
					plateOpenIndex = 0;
				}
				m_WingEventTable.DoStatePlateOpen(plateOpenIndex, unlockDelay);
			}
		}
		else
		{
			FireOpenPlateEndEvent(null);
		}
	}

	protected bool HasDependentWingJustAckedRequiredProgress()
	{
		foreach (AdventureMissionDbfRecord record in GameDbf.AdventureMission.GetRecords((AdventureMissionDbfRecord r) => r.GrantsWingId == (int)m_WingDef.GetWingId()))
		{
			if (AdventureMissionDisplay.Get().HasWingJustAckedRequiredProgress(record.ReqWingId, record.ReqProgress))
			{
				return true;
			}
		}
		return false;
	}

	protected bool HasDependentWingJustAckedRequiredProgress(AdventureMissionDbfRecord record)
	{
		return AdventureMissionDisplay.Get().HasWingJustAckedRequiredProgress(record.ReqWingId, record.ReqProgress);
	}

	public bool HasJustAckedRequiredProgress(int requiredProgress)
	{
		int progress = AdventureProgressMgr.Get().GetProgressValueForWing((int)m_WingDef.GetWingId());
		if (m_HasJustAckedProgress)
		{
			return progress == requiredProgress;
		}
		return false;
	}

	public void SetHasJustAckedProgress(bool hasJustAckedProgress)
	{
		m_HasJustAckedProgress = hasJustAckedProgress;
	}

	private bool CanPlayAtLeastOneScenario()
	{
		foreach (Boss boss in m_BossCoins)
		{
			if (AdventureProgressMgr.Get().CanPlayScenario((int)boss.m_MissionId))
			{
				return true;
			}
		}
		return false;
	}

	protected virtual bool InitializePlateOpenState()
	{
		AdventureProgressMgr.Get().GetWingAck((int)m_WingDef.GetWingId(), out var wingProgressAck);
		int plateOpenIndex = wingProgressAck - 1;
		if (wingProgressAck >= 1)
		{
			m_WingEventTable.DoStatePlateAlreadyOpen(plateOpenIndex);
			return true;
		}
		return false;
	}

	private void OnUnlockSpellFinished(Spell spell, object userData)
	{
		if (AdventureUtils.CanPlayWingOpenQuote(m_WingDef))
		{
			StartCoroutine(PlayOpenQuoteAfterDelay());
		}
	}

	private IEnumerator PlayOpenQuoteAfterDelay()
	{
		if (!(m_WingDef == null))
		{
			yield return new WaitForSeconds(m_WingDef.m_OpenQuoteDelay);
			string notifierPrefab = m_WingDef.m_OpenQuotePrefab;
			if (string.IsNullOrEmpty(notifierPrefab))
			{
				notifierPrefab = AdventureScene.Get().GetAdventureDef(GetAdventureId()).m_DefaultQuotePrefab;
			}
			Vector3 quotePosition = (UniversalInputManager.UsePhoneUI ? NotificationManager.PHONE_CHARACTER_POS : NotificationManager.ALT_ADVENTURE_SCREEN_POS);
			string gameString = new AssetReference(m_WingDef.m_OpenQuoteVOLine).GetLegacyAssetName();
			NotificationManager.Get().CreateCharacterQuote(notifierPrefab, quotePosition, GameStrings.Get(gameString), m_WingDef.m_OpenQuoteVOLine, IsDevMode);
		}
	}

	protected void ShowBigChestRewards(UIEvent e)
	{
		List<RewardData> rewards = GetBigChestRewards();
		if (rewards != null)
		{
			FireShowRewardsEvent(rewards, m_BigChest.transform.position);
		}
	}

	protected void HideBigChestRewards(UIEvent e)
	{
		List<RewardData> rewards = GetBigChestRewards();
		if (rewards != null)
		{
			FireHideRewardsEvent(rewards);
		}
	}

	private void ShowBossRewards(ScenarioDbId mission, Vector3 origin)
	{
		List<RewardData> rewardList = AdventureProgressMgr.Get().GetImmediateRewardsForDefeatingScenario((int)mission);
		FireShowRewardsEvent(rewardList, origin);
	}

	private void HideBossRewards(ScenarioDbId mission)
	{
		List<RewardData> rewardList = AdventureProgressMgr.Get().GetImmediateRewardsForDefeatingScenario((int)mission);
		FireHideRewardsEvent(rewardList);
	}

	public void FireShowRewardsEvent(List<RewardData> rewards, Vector3 origin)
	{
		ShowRewards[] array = m_ShowRewardsListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](rewards, origin);
		}
	}

	public void FireHideRewardsEvent(List<RewardData> rewards)
	{
		HideRewards[] array = m_HideRewardsListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i](rewards);
		}
	}

	private void FireShowRewardsPreviewEvent()
	{
		ShowRewardsPreview[] array = m_ShowRewardsPreviewListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i]();
		}
	}

	private void FireTryPurchaseWingEvent()
	{
		TryPurchaseWing[] array = m_TryPurchaseWingListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i]();
		}
	}

	private void UpdateBossChest(AdventureRewardsChest chest, ScenarioDbId mission)
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (ac.IsScenarioDefeatedAndInitCache(mission))
		{
			if (ac.IsScenarioJustDefeated(mission))
			{
				chest.SlamInCheckmark();
			}
			else
			{
				chest.ShowCheckmark();
			}
		}
		else if (AdventureProgressMgr.ScenarioUsesGameSaveDataProgress((int)mission) && AdventureProgressMgr.Get().CanPlayScenario((int)mission))
		{
			if (AdventureProgressMgr.GetGameSaveDataProgressForScenario((int)mission, out var progress, out var maxProgress))
			{
				chest.ShowGameSaveDataProgress(progress, maxProgress);
			}
		}
		else if (!AdventureProgressMgr.Get().ScenarioHasRewardData((int)mission))
		{
			chest.HideAll();
		}
	}

	private void UpdatePurchasedBanner()
	{
		if (m_PurchasedBanner != null)
		{
			bool owned = AdventureProgressMgr.Get().OwnsWing((int)m_WingDef.GetWingId());
			bool isWingEventActive = AdventureProgressMgr.IsWingEventActive((int)m_WingDef.GetWingId());
			m_PurchasedBanner.SetActive(owned && !isWingEventActive);
		}
	}

	private void UpdateBuyButton(bool isStoreOpen)
	{
		if (!(m_BuyButton == null))
		{
			float desaturate = 0f;
			bool colliderEnabled = true;
			string buyText = "GLUE_STORE_MONEY_BUTTON_TOOLTIP_HEADLINE";
			if (!isStoreOpen)
			{
				desaturate = 1f;
				colliderEnabled = false;
				buyText = "GLUE_ADVENTURE_LABEL_SHOP_CLOSED";
			}
			m_BuyButtonMesh.GetMaterial().SetFloat("_Desaturate", desaturate);
			m_BuyButton.GetComponent<Collider>().enabled = colliderEnabled;
			m_BuyButtonText.SetText(GameStrings.Get(buyText));
		}
	}

	private bool HasAckedAllPlateOpenEvents()
	{
		AdventureProgressMgr.Get().GetWingAck((int)m_WingDef.GetWingId(), out var ackProgress);
		if (!m_WingEventTable.SupportsIncrementalOpening())
		{
			return ackProgress >= 1;
		}
		return m_WingEventTable.m_PlateOpenEvents.Count == ackProgress;
	}

	private bool IsWingRecommendedToOpen()
	{
		if (m_WingDef.GetOpenPrereqId() == WingDbId.INVALID)
		{
			return true;
		}
		AdventureWingDef openPrereqWingDef = AdventureScene.Get().GetWingDef(m_WingDef.GetOpenPrereqId());
		if (openPrereqWingDef == null)
		{
			return true;
		}
		return AdventureProgressMgr.Get().IsWingComplete(openPrereqWingDef.GetAdventureId(), AdventureConfig.Get().GetSelectedMode(), openPrereqWingDef.GetWingId());
	}

	private void SetPlateKeyEvent(bool initial)
	{
		bool recommended = IsWingRecommendedToOpen();
		m_WingEventTable.DoStatePlateKey(recommended, initial);
		if (m_ReleaseLabelText != null)
		{
			if (!recommended && !string.IsNullOrEmpty(m_WingDef.GetOpeningNotRecommendedLabel()))
			{
				m_ReleaseLabelText.Text = m_WingDef.GetOpeningNotRecommendedLabel();
			}
			else if (m_WingDef.GetUnlocksAutomatically())
			{
				m_ReleaseLabelText.Text = m_WingDef.GetWingName();
			}
			else
			{
				m_ReleaseLabelText.Text = GameStrings.Get("GLUE_ADVENTURE_READY_TO_OPEN");
			}
		}
		InitializePlateOpenState();
		TryToUnlockAutomatically();
	}
}
