using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.UI;
using Hearthstone.Util;
using PegasusShared;
using UnityEngine;

public class FriendListFrame : MonoBehaviour
{
	public enum FriendListEditMode
	{
		NONE,
		REMOVE_FRIENDS
	}

	[Flags]
	public enum FriendListFlyoutButton
	{
		None = 0,
		AddFriend = 1,
		RemoveFriend = 2,
		RecruitFriend = 4,
		PrivacyPolicy = 8,
		All = 0xF
	}

	[Serializable]
	public class FlyoutMenuButtonPair
	{
		public FriendListFlyoutButton ButtonType;

		public PegUIElement PegUIElement;
	}

	private class PlayerUpdate
	{
		public enum ChangeType
		{
			Added,
			Removed
		}

		public ChangeType Change;

		public BnetPlayer Player;

		public PlayerUpdate(ChangeType c, BnetPlayer p)
		{
			Change = c;
			Player = p;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is PlayerUpdate other))
			{
				return false;
			}
			if (Change != other.Change)
			{
				return false;
			}
			return Player.GetAccountId() == other.Player.GetAccountId();
		}

		public override int GetHashCode()
		{
			return Player.GetHashCode();
		}
	}

	[Serializable]
	public class Me
	{
		public UberText nameText;

		public AsyncReference m_rankedMedalWidgetReference;
	}

	[Serializable]
	public class Prefabs
	{
		public FriendListItemHeader headerItem;

		public FriendListItemFooter footerItem;

		public FriendListNearbyPlayersHeader nearbyPlayersHeaderItem;

		public FriendListRequestFrame requestItem;

		public FriendListFriendFrame friendItem;

		public AddFriendFrame addFriendFrame;
	}

	[Serializable]
	public class ListInfo
	{
		public Transform topLeft;

		public Transform bottomRight;

		public Transform bottomRightWithScrollbar;

		public HeaderBackgroundInfo requestBackgroundInfo;

		public HeaderBackgroundInfo currentGameBackgroundInfo;
	}

	[Serializable]
	public class HeaderBackgroundInfo
	{
		public Mesh mesh;

		public Material material;
	}

	public struct FriendListItem
	{
		private object m_item;

		public MobileFriendListItem.TypeFlags ItemFlags { get; private set; }

		public bool IsHeader => MobileFriendListItem.ItemIsHeader(ItemFlags);

		public MobileFriendListItem.TypeFlags ItemMainType
		{
			get
			{
				if (IsHeader)
				{
					return MobileFriendListItem.TypeFlags.Header;
				}
				return SubType;
			}
		}

		public MobileFriendListItem.TypeFlags SubType => ItemFlags & ~MobileFriendListItem.TypeFlags.Header;

		public BnetPlayer GetFriend()
		{
			if ((ItemFlags & MobileFriendListItem.TypeFlags.Friend) == 0)
			{
				return null;
			}
			return (BnetPlayer)m_item;
		}

		public BnetPlayer GetRecentPlayer()
		{
			if ((ItemFlags & MobileFriendListItem.TypeFlags.RecentPlayer) == 0)
			{
				return null;
			}
			return (BnetPlayer)m_item;
		}

		public BnetPlayer GetNearbyPlayer()
		{
			if ((ItemFlags & MobileFriendListItem.TypeFlags.NearbyPlayer) == 0)
			{
				return null;
			}
			return (BnetPlayer)m_item;
		}

		public BnetInvitation GetInvite()
		{
			if ((ItemFlags & MobileFriendListItem.TypeFlags.Request) == 0)
			{
				return null;
			}
			return (BnetInvitation)m_item;
		}

		public override string ToString()
		{
			if (IsHeader)
			{
				return $"[{SubType}]Header";
			}
			return $"[{ItemMainType}]{m_item}";
		}

		public Type GetFrameType()
		{
			switch (ItemMainType)
			{
			case MobileFriendListItem.TypeFlags.Header:
				return typeof(FriendListItemHeader);
			case MobileFriendListItem.TypeFlags.Request:
				return typeof(FriendListRequestFrame);
			case MobileFriendListItem.TypeFlags.Friend:
			case MobileFriendListItem.TypeFlags.CurrentGame:
			case MobileFriendListItem.TypeFlags.NearbyPlayer:
			case MobileFriendListItem.TypeFlags.RecentPlayer:
				return typeof(FriendListFriendFrame);
			default:
				throw new Exception("Unknown ItemType: " + ItemFlags.ToString() + " (" + (int)ItemFlags + ")");
			}
		}

		public FriendListItem(bool isHeader, MobileFriendListItem.TypeFlags itemType, object itemData)
		{
			this = default(FriendListItem);
			if (!isHeader && itemData == null)
			{
				Log.All.Print("FriendListItem: itemData is null! itemType=" + itemType);
			}
			m_item = itemData;
			ItemFlags = itemType;
			if (isHeader)
			{
				ItemFlags |= MobileFriendListItem.TypeFlags.Header;
			}
			else
			{
				ItemFlags &= ~MobileFriendListItem.TypeFlags.Header;
			}
		}
	}

	private class VirtualizedFriendsListBehavior : TouchList.ILongListBehavior
	{
		private FriendListFrame m_friendList;

		private int m_cachedMaxVisibleItems = -1;

		private const int MAX_FREELIST_ITEMS = 20;

		private List<MobileFriendListItem> m_freelist;

		private HashSet<MobileFriendListItem> m_acquiredItems = new HashSet<MobileFriendListItem>();

		private Bounds[] m_boundsByType;

		public List<MobileFriendListItem> FreeList => m_freelist;

		public int AllItemsCount => m_friendList.m_allItems.Count;

		public int MaxVisibleItems
		{
			get
			{
				if (m_cachedMaxVisibleItems >= 0)
				{
					return m_cachedMaxVisibleItems;
				}
				m_cachedMaxVisibleItems = 0;
				UnityEngine.Vector2 viewableBox = m_friendList.items.ClipSize;
				Bounds inviteBounds = GetPrefabBounds(m_friendList.prefabs.requestItem.gameObject);
				Bounds friendBounds = GetPrefabBounds(m_friendList.prefabs.friendItem.gameObject);
				float a = inviteBounds.max.z - inviteBounds.min.z;
				float friendLength = friendBounds.max.z - friendBounds.min.z;
				float smallestItemLength = Mathf.Min(a, friendLength);
				if (smallestItemLength > 0f)
				{
					int countViewable = Mathf.CeilToInt(viewableBox.y / smallestItemLength);
					m_cachedMaxVisibleItems = countViewable + 3;
				}
				return m_cachedMaxVisibleItems;
			}
		}

		public int MinBuffer => 2;

		public int MaxAcquiredItems => MaxVisibleItems + 2 * MinBuffer;

		private bool HasCollapsedHeaders
		{
			get
			{
				foreach (KeyValuePair<MobileFriendListItem.TypeFlags, FriendListItemHeader> header2 in m_friendList.m_headers)
				{
					if (!header2.Value.IsShowingContents)
					{
						return true;
					}
				}
				return false;
			}
		}

		public VirtualizedFriendsListBehavior(FriendListFrame friendList)
		{
			m_friendList = friendList;
		}

		private static Bounds GetPrefabBounds(GameObject prefabGameObject)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(prefabGameObject);
			gameObject.SetActive(value: true);
			Bounds bounds = TransformUtil.ComputeSetPointBounds(gameObject);
			UnityEngine.Object.DestroyImmediate(gameObject);
			return bounds;
		}

		public bool IsItemShowable(int allItemsIndex)
		{
			if (allItemsIndex < 0 || allItemsIndex >= AllItemsCount)
			{
				return false;
			}
			FriendListItem item = m_friendList.m_allItems[allItemsIndex];
			if (item.IsHeader && !m_friendList.IsInEditMode)
			{
				return true;
			}
			FriendListItemHeader header = m_friendList.FindHeader(item.SubType);
			if (header == null || !header.IsShowingContents)
			{
				return false;
			}
			if (m_friendList.EditMode == FriendListEditMode.REMOVE_FRIENDS)
			{
				if (item.ItemMainType != MobileFriendListItem.TypeFlags.Header)
				{
					if (item.ItemFlags == MobileFriendListItem.TypeFlags.RecentPlayer)
					{
						return false;
					}
					if (item.ItemFlags == MobileFriendListItem.TypeFlags.NearbyPlayer)
					{
						return false;
					}
				}
				else
				{
					if (item.ItemFlags == (MobileFriendListItem.TypeFlags.RecentPlayer | MobileFriendListItem.TypeFlags.Header))
					{
						return false;
					}
					if (item.ItemFlags == (MobileFriendListItem.TypeFlags.NearbyPlayer | MobileFriendListItem.TypeFlags.Header))
					{
						return false;
					}
				}
			}
			return true;
		}

		public Vector3 GetItemSize(int allItemsIndex)
		{
			if (allItemsIndex < 0 || allItemsIndex >= AllItemsCount)
			{
				return Vector3.zero;
			}
			FriendListItem item = m_friendList.m_allItems[allItemsIndex];
			if (m_boundsByType == null)
			{
				InitializeBoundsByTypeArray();
			}
			int boundsByTypeIndex = GetBoundsByTypeIndex(item.ItemMainType);
			return m_boundsByType[boundsByTypeIndex].size;
		}

		public void ReleaseAllItems()
		{
			if (m_acquiredItems.Count == 0)
			{
				return;
			}
			if (m_freelist == null)
			{
				m_freelist = new List<MobileFriendListItem>();
			}
			m_freelist.Clear();
			foreach (MobileFriendListItem visualItem in m_acquiredItems)
			{
				if (visualItem.IsHeader)
				{
					visualItem.gameObject.SetActive(value: false);
				}
				else if (m_freelist.Count >= 20)
				{
					UnityEngine.Object.Destroy(visualItem.gameObject);
				}
				else
				{
					m_freelist.Add(visualItem);
					visualItem.gameObject.SetActive(value: false);
				}
				visualItem.Unselected();
			}
			m_acquiredItems.Clear();
		}

		public void ReleaseItem(ITouchListItem item)
		{
			MobileFriendListItem visualItem = item as MobileFriendListItem;
			if (visualItem == null)
			{
				throw new ArgumentException("given item is not MobileFriendListItem: " + item);
			}
			if (m_freelist == null)
			{
				m_freelist = new List<MobileFriendListItem>();
			}
			if (visualItem.IsHeader)
			{
				visualItem.gameObject.SetActive(value: false);
			}
			else if (m_freelist.Count >= 20)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
			else
			{
				m_freelist.Add(visualItem);
				visualItem.gameObject.SetActive(value: false);
			}
			if (!m_acquiredItems.Remove(visualItem))
			{
				Log.All.Print("VirtualizedFriendsListBehavior.ReleaseItem item not found in m_acquiredItems: {0}", visualItem);
			}
			visualItem.Unselected();
		}

		public ITouchListItem AcquireItem(int index)
		{
			if (m_acquiredItems.Count >= MaxAcquiredItems)
			{
				throw new Exception("Bug in ILongListBehavior? there are too many acquired items! index=" + index + " max=" + MaxAcquiredItems + " maxVisible=" + MaxVisibleItems + " minBuffer=" + MinBuffer + " acquiredItems.Count=" + m_acquiredItems.Count + " hasCollapsedHeaders=" + HasCollapsedHeaders);
			}
			if (index < 0 || index >= m_friendList.m_allItems.Count)
			{
				throw new IndexOutOfRangeException($"Invalid index, {DebugUtils.GetHierarchyPathAndType(m_friendList)} has {m_friendList.m_allItems.Count} elements.");
			}
			FriendListItem item = m_friendList.m_allItems[index];
			MobileFriendListItem.TypeFlags itemType = item.ItemMainType;
			Type frameType = item.GetFrameType();
			if (m_freelist != null && !item.IsHeader)
			{
				int freeIndex = m_freelist.FindLastIndex((MobileFriendListItem m) => (!item.IsHeader) ? (m.GetComponent(frameType) != null) : m.IsHeader);
				if (freeIndex >= 0 && m_freelist[freeIndex] == null)
				{
					for (int i = 0; i < m_freelist.Count; i++)
					{
						if (m_freelist[i] == null)
						{
							m_freelist.RemoveAt(i);
							i--;
						}
					}
					freeIndex = m_freelist.FindLastIndex((MobileFriendListItem m) => (!item.IsHeader) ? (m.GetComponent(frameType) != null) : m.IsHeader);
				}
				if (freeIndex >= 0)
				{
					MobileFriendListItem reusedItem = m_freelist[freeIndex];
					m_freelist.RemoveAt(freeIndex);
					reusedItem.gameObject.SetActive(value: true);
					switch (itemType)
					{
					case MobileFriendListItem.TypeFlags.Friend:
					case MobileFriendListItem.TypeFlags.NearbyPlayer:
					case MobileFriendListItem.TypeFlags.RecentPlayer:
					{
						FriendListFriendFrame frame = reusedItem.GetComponent<FriendListFriendFrame>();
						frame.gameObject.SetActive(value: true);
						BnetPlayer player = null;
						bool isRecentPlayer = false;
						switch (itemType)
						{
						case MobileFriendListItem.TypeFlags.Friend:
							player = item.GetFriend();
							break;
						case MobileFriendListItem.TypeFlags.RecentPlayer:
							player = item.GetRecentPlayer();
							isRecentPlayer = true;
							break;
						case MobileFriendListItem.TypeFlags.NearbyPlayer:
							player = item.GetNearbyPlayer();
							break;
						}
						frame.Initialize(player, isRecentPlayer);
						FriendListItemHeader header = m_friendList.FindHeader(itemType);
						m_friendList.FinishCreateVisualItem(frame, itemType, header, frame.gameObject);
						break;
					}
					case MobileFriendListItem.TypeFlags.Request:
					{
						FriendListRequestFrame requestFrame = reusedItem.GetComponent<FriendListRequestFrame>();
						requestFrame.gameObject.SetActive(value: true);
						requestFrame.SetInvite(item.GetInvite());
						m_friendList.FinishCreateVisualItem(requestFrame, itemType, m_friendList.FindHeader(itemType), requestFrame.gameObject);
						requestFrame.UpdateInvite();
						break;
					}
					default:
						throw new NotImplementedException("VirtualizedFriendsListBehavior.AcquireItem[reuse] frameType=" + frameType.FullName + " itemType=" + itemType);
					}
					m_acquiredItems.Add(reusedItem);
					return reusedItem;
				}
			}
			MobileFriendListItem newVisualItem = null;
			newVisualItem = itemType switch
			{
				MobileFriendListItem.TypeFlags.Header => m_friendList.FindHeader(item.SubType).GetComponent<MobileFriendListItem>(), 
				MobileFriendListItem.TypeFlags.Friend => m_friendList.CreatePlayerFrame(item.GetFriend(), MobileFriendListItem.TypeFlags.Friend), 
				MobileFriendListItem.TypeFlags.Request => m_friendList.CreateRequestFrame(item.GetInvite()), 
				MobileFriendListItem.TypeFlags.RecentPlayer => m_friendList.CreatePlayerFrame(item.GetRecentPlayer(), MobileFriendListItem.TypeFlags.RecentPlayer), 
				MobileFriendListItem.TypeFlags.NearbyPlayer => m_friendList.CreatePlayerFrame(item.GetNearbyPlayer(), MobileFriendListItem.TypeFlags.NearbyPlayer), 
				_ => throw new NotImplementedException("VirtualizedFriendsListBehavior.AcquireItem[new] type=" + frameType.FullName), 
			};
			m_acquiredItems.Add(newVisualItem);
			return newVisualItem;
		}

		private void InitializeBoundsByTypeArray()
		{
			Array values = Enum.GetValues(typeof(MobileFriendListItem.TypeFlags));
			m_boundsByType = new Bounds[values.Length];
			for (int i = 0; i < values.Length; i++)
			{
				MobileFriendListItem.TypeFlags val = (MobileFriendListItem.TypeFlags)values.GetValue(i);
				Component prefab = GetPrefab(val);
				int index = GetBoundsByTypeIndex(val);
				m_boundsByType[index] = ((prefab == null) ? default(Bounds) : GetPrefabBounds(prefab.gameObject));
			}
		}

		private int GetBoundsByTypeIndex(MobileFriendListItem.TypeFlags itemType)
		{
			switch (itemType)
			{
			case MobileFriendListItem.TypeFlags.Header:
				return 0;
			case MobileFriendListItem.TypeFlags.Request:
				return 1;
			case MobileFriendListItem.TypeFlags.RecentPlayer:
				return 2;
			case MobileFriendListItem.TypeFlags.NearbyPlayer:
				return 3;
			case MobileFriendListItem.TypeFlags.CurrentGame:
				return 4;
			case MobileFriendListItem.TypeFlags.Friend:
				return 5;
			default:
			{
				string[] obj = new string[5]
				{
					"Unknown ItemType: ",
					itemType.ToString(),
					" (",
					null,
					null
				};
				int num = (int)itemType;
				obj[3] = num.ToString();
				obj[4] = ")";
				throw new Exception(string.Concat(obj));
			}
			}
		}

		private Component GetPrefab(MobileFriendListItem.TypeFlags itemType)
		{
			switch (itemType)
			{
			case MobileFriendListItem.TypeFlags.Header:
				return m_friendList.prefabs.headerItem;
			case MobileFriendListItem.TypeFlags.Request:
				return m_friendList.prefabs.requestItem;
			case MobileFriendListItem.TypeFlags.Friend:
			case MobileFriendListItem.TypeFlags.CurrentGame:
			case MobileFriendListItem.TypeFlags.NearbyPlayer:
			case MobileFriendListItem.TypeFlags.RecentPlayer:
				return m_friendList.prefabs.friendItem;
			default:
			{
				string[] obj = new string[5]
				{
					"Unknown ItemType: ",
					itemType.ToString(),
					" (",
					null,
					null
				};
				int num = (int)itemType;
				obj[3] = num.ToString();
				obj[4] = ")";
				throw new Exception(string.Concat(obj));
			}
			}
		}
	}

	public Me me;

	public Prefabs prefabs;

	public ListInfo listInfo;

	public TouchList items;

	public PlayerPortrait myPortrait;

	public List<FlyoutMenuButtonPair> flyoutMenuButtonsMap = new List<FlyoutMenuButtonPair>();

	public PegUIElement addFriendButton;

	public PegUIElement removeFriendButton;

	public GameObject removeFriendButtonEnabledVisual;

	public GameObject removeFriendButtonDisabledVisual;

	public GameObject removeFriendButtonButtonGlow;

	public PegUIElement rafButton;

	public GameObject rafButtonEnabledVisual;

	public GameObject rafButtonDisabledVisual;

	public GameObject rafButtonButtonGlow;

	public GameObject rafButtonGlowBone;

	public PegUIElement privacySettingsButton;

	public TouchListScrollbar scrollbar;

	public NineSliceElement window;

	public GameObject portraitBackground;

	public Material unrankedBackground;

	public Material rankedBackground;

	public GameObject innerShadow;

	public GameObject outerShadow;

	public GameObject temporaryAccountPaper;

	public GameObject temporaryAccountCover;

	public GameObject temporaryAccountDrawingBone;

	public GameObject temporaryAccountDrawing;

	public UIBButton temporaryAccountSignUpButton;

	public PegUIElement flyoutMenuButton;

	public GameObject flyoutMenu;

	public GameObject flyoutMiddleFrame;

	public GameObject flyoutMiddleShadow;

	public MultiSliceElement flyoutFrameContainer;

	public MultiSliceElement flyoutShadowContainer;

	public HighlightState flyoutButtonGlow;

	public GameObject flyoutButtonHighlightObject;

	[Tooltip("Added space between buttons, proportionate to button size")]
	public float flyoutPaddingBetweenButtons;

	[Tooltip("Added space at the top of middle frame, proportionate to button size")]
	public float flyoutMiddleFrameTopMargin;

	[Tooltip("Added space at the bottom of the middle frame, proportionate to button size")]
	public float flyoutMiddleFrameBottomMargin;

	[Tooltip("Added vertical length at the bottom of the middle frame, proportionate to button size")]
	public float flyoutMiddleShadowBottomPadding;

	public GameObject friendFlyoutBone;

	public GameObject addAFriendCallout;

	public PegUIElement addFriendCalloutButton;

	public GameObject[] flyoutButtonBones;

	private const float UpdateItemsAfterScrollDelay = 0.5f;

	private float m_timeSinceLastScroll;

	private bool m_updateItemsAfterScroll;

	private AddFriendFrame m_addFriendFrame;

	private AlertPopup m_removeFriendPopup;

	private CameraOverridePass m_itemsCameraOverridePass;

	private FriendListEditMode m_editMode;

	private BnetPlayer m_friendToRemove;

	private bool m_flyoutOpen;

	private SelectableMedal m_mySelectableMedal;

	private Coroutine m_updateFriendItemsWhenAvailableCoroutine;

	private bool m_isRemoveFriendsShowing = true;

	private FriendListFlyoutButton m_buttonsToShow = (FriendListFlyoutButton)(-1);

	private bool m_isFlyoutMenuLayoutDirty = true;

	private bool m_isFlyoutButtonHovered;

	private Vector3 m_flyoutButtonWorldSize;

	private Renderer m_flyoutMiddleFrameRenderer;

	private Bounds m_flyoutMiddleFrameBounds;

	private Vector3 m_flyoutMiddleFrameBaseWorldSize;

	private Vector3 m_flyoutMiddleShadowBaseWorldSize;

	private List<PlayerUpdate> m_nearbyPlayerUpdates = new List<PlayerUpdate>();

	private List<PlayerUpdate> m_recentPlayerUpdates = new List<PlayerUpdate>();

	private BnetPlayerChangelist m_playersChangeList = new BnetPlayerChangelist();

	private float m_lastNearbyPlayersUpdate;

	private bool m_nearbyPlayersNeedUpdate;

	private const float NEARBY_PLAYERS_UPDATE_TIME = 10f;

	private bool m_recentPlayersNeedUpdate;

	private bool m_hasNearbyPlayers;

	private bool m_isRAFButtonEnabled = true;

	private List<FriendListItem> m_allItems = new List<FriendListItem>();

	private VirtualizedFriendsListBehavior m_longListBehavior;

	private Dictionary<MobileFriendListItem.TypeFlags, FriendListItemHeader> m_headers = new Dictionary<MobileFriendListItem.TypeFlags, FriendListItemHeader>();

	public Action FlyoutOpened;

	public Action FlyoutClosed;

	public bool IsStarted { get; private set; }

	public bool ShowingAddFriendFrame => m_addFriendFrame != null;

	public bool IsInEditMode => m_editMode != FriendListEditMode.NONE;

	public FriendListEditMode EditMode => m_editMode;

	public bool IsFlyoutOpen => m_flyoutOpen;

	public event Action OnStarted;

	public event Action AddFriendFrameOpened;

	public event Action AddFriendFrameClosed;

	public event Action RemoveFriendPopupOpened;

	public event Action RemoveFriendPopupClosed;

	private void Awake()
	{
		InitFlyoutMenu();
		RegisterFriendEvents();
		CreateItemsView();
		UpdateBackgroundCollider();
		bool showScrollbar = !UniversalInputManager.Get().IsTouchMode() || PlatformSettings.OS == OSCategory.PC;
		if (scrollbar != null)
		{
			scrollbar.gameObject.SetActive(showScrollbar);
		}
		if (BnetFriendMgr.Get().HasOnlineFriends() || BnetNearbyPlayerMgr.Get().HasNearbyStrangers())
		{
			CollectionManager.Get().RequestDeckContentsForDecksWithoutContentsLoaded();
		}
		if (TemporaryAccountManager.IsTemporaryAccount())
		{
			items.GetComponent<BoxCollider>().enabled = false;
			temporaryAccountPaper.SetActive(value: true);
			temporaryAccountCover.SetActive(value: true);
			temporaryAccountDrawing.SetActive(value: true);
			temporaryAccountSignUpButton.AddEventListener(UIEventType.RELEASE, OnTemporaryAccountSignUpButtonPressed);
		}
		m_itemsCameraOverridePass.Schedule(CustomViewEntryPoint.BattleNetFriendList);
		Network.Get().OnDisconnectedFromBattleNet += OnDisconnectedFromBattleNet;
		FlyoutOpened = (Action)Delegate.Combine(FlyoutOpened, new Action(OnFlyoutOpened));
		FlyoutClosed = (Action)Delegate.Combine(FlyoutClosed, new Action(OnFlyoutClosed));
	}

	private void Start()
	{
		UpdateMyself();
		InitItems();
		UpdateRAFState();
		TelemetryManager.Client().SendFriendsListView(SceneMgr.Get().GetMode().ToString());
		me.m_rankedMedalWidgetReference.RegisterReadyListener<Widget>(OnMySelectableMedalWidgetReady);
		IsStarted = true;
		if (this.OnStarted != null)
		{
			this.OnStarted();
		}
	}

	private void OnDestroy()
	{
		m_itemsCameraOverridePass.Unschedule();
		UnregisterFriendEvents();
		CloseAddFriendFrame();
		if (PegUI.Get() != null)
		{
			PegUI.Get().UnregisterFromRenderPassPriorityHitTest(this);
		}
		if (m_longListBehavior != null && m_longListBehavior.FreeList != null)
		{
			foreach (MobileFriendListItem obj in m_longListBehavior.FreeList)
			{
				if (obj != null)
				{
					UnityEngine.Object.Destroy(obj.gameObject);
				}
			}
		}
		foreach (FriendListItemHeader obj2 in m_headers.Values)
		{
			if (obj2 != null)
			{
				UnityEngine.Object.Destroy(obj2.gameObject);
			}
		}
		Network network = Network.Get();
		if (network != null)
		{
			network.OnDisconnectedFromBattleNet -= OnDisconnectedFromBattleNet;
		}
		FlyoutOpened = (Action)Delegate.Remove(FlyoutOpened, new Action(OnFlyoutOpened));
		FlyoutClosed = (Action)Delegate.Remove(FlyoutClosed, new Action(OnFlyoutClosed));
	}

	private void Update()
	{
		HandleKeyboardInput();
		if (m_isFlyoutMenuLayoutDirty)
		{
			UpdateFlyoutMenuLayout();
		}
		bool itemsUpdated = false;
		if (m_recentPlayersNeedUpdate)
		{
			HandleRecentPlayersChanged();
			itemsUpdated = true;
		}
		if (m_nearbyPlayersNeedUpdate && Time.realtimeSinceStartup >= m_lastNearbyPlayersUpdate + 10f)
		{
			HandleNearbyPlayersChanged();
			itemsUpdated = true;
		}
		if (m_updateItemsAfterScroll && Time.realtimeSinceStartup - m_timeSinceLastScroll > 0.5f)
		{
			if (!itemsUpdated)
			{
				UpdateFriendItems();
			}
			m_updateItemsAfterScroll = false;
		}
	}

	private void OnEnable()
	{
		if (m_recentPlayersNeedUpdate)
		{
			HandleRecentPlayersChanged();
		}
		if (m_nearbyPlayersNeedUpdate)
		{
			HandleNearbyPlayersChanged();
		}
		if (m_playersChangeList.GetChanges().Count > 0)
		{
			DoPlayersChanged(m_playersChangeList);
			m_playersChangeList.GetChanges().Clear();
		}
		if (items.IsInitialized)
		{
			ResumeItemsLayout();
			SortAndRefreshTouchList();
		}
		UpdateMyself();
		items.ResetState();
		m_editMode = FriendListEditMode.NONE;
		m_friendToRemove = null;
	}

	private void OnDisconnectedFromBattleNet(BattleNetErrors error)
	{
		m_longListBehavior.ReleaseAllItems();
		m_allItems.Clear();
	}

	public void SetWorldRect(float x, float z, float width, float height)
	{
		bool wasActive = base.gameObject.activeSelf;
		base.gameObject.SetActive(value: true);
		window.SetEntireSize(width, height);
		Vector3 topLeft = TransformUtil.ComputeWorldPoint(TransformUtil.ComputeSetPointBounds(window), new Vector3(0f, 0f, 1f));
		Vector3 delta = new Vector3(x, topLeft.y, z) - topLeft;
		base.transform.Translate(delta, Space.World);
		UpdateItemsList();
		UpdateItemsView();
		UpdateBackgroundCollider();
		UpdateDropShadow();
		base.gameObject.SetActive(wasActive);
		if (temporaryAccountDrawingBone != null && TemporaryAccountManager.IsTemporaryAccount())
		{
			temporaryAccountDrawing.transform.position = temporaryAccountDrawingBone.transform.position;
		}
	}

	public void SetWorldPosition(float x, float y, float z)
	{
		SetWorldPosition(new Vector3(x, y, z));
	}

	public void SetWorldPosition(Vector3 pos)
	{
		bool wasActive = base.gameObject.activeSelf;
		base.gameObject.SetActive(value: true);
		base.transform.position = pos;
		UpdateItemsList();
		UpdateItemsView();
		UpdateBackgroundCollider();
		base.gameObject.SetActive(wasActive);
		if (temporaryAccountDrawingBone != null && TemporaryAccountManager.IsTemporaryAccount())
		{
			temporaryAccountDrawing.transform.position = temporaryAccountDrawingBone.transform.position;
		}
	}

	public void SetWorldHeight(float height)
	{
		bool wasActive = base.gameObject.activeSelf;
		base.gameObject.SetActive(value: true);
		float oldHeight = window.GetEntireHeight();
		window.SetEntireHeight(height);
		float newHeight = window.GetEntireHeight();
		Vector3 newFlyoutPos = friendFlyoutBone.transform.position;
		newFlyoutPos.z -= oldHeight - newHeight;
		friendFlyoutBone.transform.position = newFlyoutPos;
		UpdateItemsList();
		UpdateItemsView();
		UpdateBackgroundCollider();
		UpdateDropShadow();
		base.gameObject.SetActive(wasActive);
		if (temporaryAccountDrawingBone != null && TemporaryAccountManager.IsTemporaryAccount())
		{
			temporaryAccountDrawing.transform.position = temporaryAccountDrawingBone.transform.position;
		}
	}

	public void ShowAddFriendFrame(BnetPlayer player = null)
	{
		m_addFriendFrame = UnityEngine.Object.Instantiate(prefabs.addFriendFrame);
		m_addFriendFrame.Closed += CloseAddFriendFrame;
		if (player != null && !BnetRecentPlayerMgr.Get().IsCurrentOpponent(player))
		{
			m_addFriendFrame.SetPlayer(player);
		}
	}

	public void CloseAddFriendFrame()
	{
		if (!(m_addFriendFrame == null))
		{
			m_addFriendFrame.Close();
			if (this.AddFriendFrameClosed != null)
			{
				this.AddFriendFrameClosed();
			}
			m_addFriendFrame = null;
		}
	}

	public void ShowRemoveFriendPopup(BnetPlayer friend)
	{
		m_friendToRemove = friend;
		if (m_friendToRemove != null)
		{
			string friendName = FriendUtils.GetUniqueName(m_friendToRemove);
			AlertPopup.PopupInfo popupInfo = new AlertPopup.PopupInfo();
			popupInfo.m_text = GameStrings.Format("GLOBAL_FRIENDLIST_REMOVE_FRIEND_ALERT_MESSAGE", friendName);
			popupInfo.m_showAlertIcon = true;
			popupInfo.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
			popupInfo.m_responseCallback = OnRemoveFriendPopupResponse;
			AlertPopup.PopupInfo info = popupInfo;
			DialogManager.Get().ShowPopup(info, OnRemoveFriendDialogShown, m_friendToRemove);
			if (this.RemoveFriendPopupOpened != null)
			{
				this.RemoveFriendPopupOpened();
			}
		}
	}

	public void SelectFriend(BnetPlayer player)
	{
		foreach (FriendListFriendFrame friendFrame in GetRenderedItems<FriendListFriendFrame>())
		{
			if (friendFrame.GetFriend() == player)
			{
				friendFrame.ForceHighlight = true;
			}
			else
			{
				friendFrame.ForceHighlight = false;
			}
		}
	}

	public void ClearHighlights()
	{
		foreach (FriendListFriendFrame renderedItem in GetRenderedItems<FriendListFriendFrame>())
		{
			renderedItem.ForceHighlight = false;
		}
	}

	public void UpdateRAFButtonGlow()
	{
		bool hasSeenRAF = Options.Get().GetBool(Option.HAS_SEEN_RAF);
		rafButtonButtonGlow.SetActive(!hasSeenRAF && m_isRAFButtonEnabled);
		UpdateFlyoutButtonGlow();
	}

	private void UpdateFlyoutButtonGlow()
	{
		if (flyoutButtonGlow != null)
		{
			flyoutButtonGlow.ChangeState(rafButtonButtonGlow.activeSelf ? ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE : ActorStateType.NONE);
		}
		UpdateFlyoutButtonHighlight();
	}

	private void UpdateFlyoutButtonHighlight()
	{
		if (flyoutButtonHighlightObject != null)
		{
			flyoutButtonHighlightObject.SetActive(IsFlyoutOpen || m_isFlyoutButtonHovered);
		}
	}

	public OrientedBounds ComputeFrameWorldBounds()
	{
		GameObject go = base.gameObject;
		List<GameObject> ignoreMeshes = new List<GameObject> { items.gameObject };
		return TransformUtil.ComputeOrientedWorldBounds(go, default(Vector3), default(Vector3), ignoreMeshes);
	}

	public void SetRAFButtonEnabled(bool enabled)
	{
		if (m_isRAFButtonEnabled != enabled)
		{
			m_isRAFButtonEnabled = enabled;
			rafButton.GetComponent<UIBHighlight>().EnableResponse = m_isRAFButtonEnabled;
			rafButtonEnabledVisual.SetActive(enabled);
			rafButtonDisabledVisual.SetActive(!enabled);
			if (m_isRAFButtonEnabled)
			{
				rafButton.AddEventListener(UIEventType.RELEASE, OnRAFButtonReleased);
			}
			else
			{
				rafButton.RemoveEventListener(UIEventType.RELEASE, OnRAFButtonReleased);
			}
			UpdateRAFButtonGlow();
		}
	}

	public void SetFlyoutMenuButtonVisible(FriendListFlyoutButton button, bool isVisible)
	{
		bool num = (m_buttonsToShow & button) > FriendListFlyoutButton.None;
		m_buttonsToShow = (isVisible ? (m_buttonsToShow | button) : (m_buttonsToShow & ~button));
		if (num != isVisible)
		{
			SetFlyoutMenuDirty();
		}
	}

	private void SetFlyoutMenuDirty()
	{
		m_isFlyoutMenuLayoutDirty = true;
	}

	public void OpenFlyoutMenu()
	{
		if (!(flyoutMenu == null))
		{
			m_flyoutOpen = true;
			flyoutMenu.SetActive(value: true);
			UpdateFlyoutButtonGlow();
			FlyoutOpened?.Invoke();
		}
	}

	public void CloseFlyoutMenu()
	{
		if (!(flyoutMenu == null))
		{
			m_flyoutOpen = false;
			flyoutMenu.SetActive(value: false);
			UpdateFlyoutButtonGlow();
			FlyoutClosed?.Invoke();
		}
	}

	private void CreateItemsView()
	{
		if (m_itemsCameraOverridePass == null)
		{
			m_itemsCameraOverridePass = new CameraOverridePass("FriendListFrameItemsView", GameLayer.BattleNetFriendList.LayerBit());
			UpdateItemsView();
			if (PegUI.Get() != null)
			{
				PegUI.Get().RegisterForRenderPassPriorityHitTest(this);
			}
		}
	}

	private void UpdateItemsList()
	{
		Transform bottomRightBone = GetBottomRightBone();
		Vector3 topLeftPosition = listInfo.topLeft.position;
		Vector3 bottomRightPosition = bottomRightBone.position;
		items.transform.position = (topLeftPosition + bottomRightPosition) / 2f + Vector3.up * 5f;
		Vector3 listClipSize3D = bottomRightPosition - topLeftPosition;
		items.ClipSize = new UnityEngine.Vector2(listClipSize3D.x, Math.Abs(listClipSize3D.z)) * 4f;
		if (innerShadow != null)
		{
			innerShadow.transform.position = items.transform.position;
			Vector3 listSize = GetBottomRightBone().position - listInfo.topLeft.position;
			TransformUtil.SetLocalScaleToWorldDimension(innerShadow, new WorldDimensionIndex(Mathf.Abs(listSize.x), 0), new WorldDimensionIndex(Mathf.Abs(listSize.z), 2));
		}
	}

	private void UpdateItemsView()
	{
		Camera bnetCamera = BaseUI.Get().GetBnetCamera();
		Transform brTransform = GetBottomRightBone();
		Vector3 min = bnetCamera.WorldToScreenPoint(listInfo.topLeft.position);
		Vector3 max = bnetCamera.WorldToScreenPoint(brTransform.position);
		GeneralUtils.Swap(ref min.y, ref max.y);
		m_itemsCameraOverridePass.OverrideScissor(new Rect(min.x, min.y, max.x - min.x, max.y - min.y));
	}

	private void UpdateBackgroundCollider()
	{
		Renderer[] componentsInChildren = window.GetComponentsInChildren<Renderer>();
		Bounds bounds = new Bounds(base.transform.position, Vector3.zero);
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			if (renderer.bounds.size.x != 0f && renderer.bounds.size.y != 0f && renderer.bounds.size.z != 0f)
			{
				bounds.Encapsulate(renderer.bounds);
			}
		}
		Vector3 min = base.transform.InverseTransformPoint(bounds.min);
		Vector3 max = base.transform.InverseTransformPoint(bounds.max);
		BoxCollider collider = GetComponent<BoxCollider>();
		if (collider == null)
		{
			collider = base.gameObject.AddComponent<BoxCollider>();
		}
		collider.center = (min + max) / 2f + Vector3.forward;
		collider.size = max - min;
	}

	private void UpdateDropShadow()
	{
		if (!(outerShadow == null))
		{
			outerShadow.SetActive(!UniversalInputManager.Get().IsTouchMode());
		}
	}

	private void UpdateMyself()
	{
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		if (myself != null && myself.IsDisplayable())
		{
			BnetBattleTag battleTag = myself.GetBattleTag();
			if (Options.Get().GetBool(Option.STREAMER_MODE))
			{
				me.nameText.Text = string.Format("<color=#{0}>{1}</color>", "5ecaf0ff", GameStrings.Get("GAMEPLAY_HIDDEN_PLAYER_NAME"));
			}
			else
			{
				me.nameText.Text = string.Format("<color=#{0}>{1}</color> <size=32><color=#{2}>#{3}</color></size>", "5ecaf0ff", battleTag.GetName(), "999999ff", battleTag.GetNumber().ToString());
			}
			bool rankedMedalDisplayable = RankMgr.Get().GetRankedMedalFromRankPresenceField(BnetPresenceMgr.Get().GetMyPlayer())?.IsDisplayable() ?? false;
			int bgRating;
			GameType gameType;
			bool bgMedalDisplayable = RankMgr.Get().GetBattlegroundsMedalFromRankPresenceField(BnetPresenceMgr.Get().GetMyPlayer().GetHearthstoneGameAccount(), out bgRating, out gameType);
			UpdatePortrait(rankedMedalDisplayable || bgMedalDisplayable);
			UpdateMySelectableMedalWidget();
		}
		else
		{
			me.nameText.Text = string.Empty;
		}
	}

	private void InitItems()
	{
		BnetFriendMgr friendMgr = BnetFriendMgr.Get();
		BnetRecentPlayerMgr bnetRecentPlayerMgr = BnetRecentPlayerMgr.Get();
		BnetNearbyPlayerMgr nearbyPlayerMgr = BnetNearbyPlayerMgr.Get();
		items.SelectionEnabled = true;
		items.SelectedIndexChanging += (int index) => index != -1;
		items.Scrolled += delegate
		{
			m_timeSinceLastScroll = Time.realtimeSinceStartup;
			m_updateItemsAfterScroll = true;
		};
		SuspendItemsLayout();
		UpdateRequests(friendMgr.GetReceivedInvites(), null);
		UpdateAllFriends(friendMgr.GetFriends(), null);
		foreach (BnetPlayer player in bnetRecentPlayerMgr.GetRecentPlayers())
		{
			FriendListItem item = new FriendListItem(isHeader: false, MobileFriendListItem.TypeFlags.RecentPlayer, player);
			AddItem(item);
		}
		foreach (BnetPlayer player2 in nearbyPlayerMgr.GetNearbyPlayers())
		{
			FriendListItem item2 = new FriendListItem(isHeader: false, MobileFriendListItem.TypeFlags.NearbyPlayer, player2);
			AddItem(item2);
		}
		UpdateAllHeaders();
		ResumeItemsLayout();
		SortAndRefreshTouchList();
		UpdateAllHeaderBackgrounds();
		UpdateSelectedItem();
		UpdateRAFButtonGlow();
		items.ScrollValue = 0f;
	}

	public void UpdateItems()
	{
		foreach (FriendListRequestFrame renderedItem in GetRenderedItems<FriendListRequestFrame>())
		{
			renderedItem.UpdateInvite();
		}
		UpdateFriendItems();
	}

	public void UpdateFriendItems()
	{
		if (m_updateFriendItemsWhenAvailableCoroutine != null)
		{
			StopCoroutine(m_updateFriendItemsWhenAvailableCoroutine);
		}
		foreach (FriendListFriendFrame renderedItem in GetRenderedItems<FriendListFriendFrame>())
		{
			renderedItem.UpdateFriend();
		}
	}

	public void UpdateFriendItemsWhenAvailable()
	{
		if (m_updateFriendItemsWhenAvailableCoroutine != null)
		{
			StopCoroutine(m_updateFriendItemsWhenAvailableCoroutine);
		}
		m_updateFriendItemsWhenAvailableCoroutine = StartCoroutine(UpdateFriendItemsWhenAvailableCoroutine());
	}

	private IEnumerator UpdateFriendItemsWhenAvailableCoroutine()
	{
		while (!FriendChallengeMgr.Get().AmIAvailable())
		{
			yield return null;
		}
		m_updateFriendItemsWhenAvailableCoroutine = null;
		UpdateFriendItems();
	}

	private void UpdateRequests(List<BnetInvitation> addedList, List<BnetInvitation> removedList)
	{
		if (removedList == null && addedList == null)
		{
			return;
		}
		if (removedList != null)
		{
			foreach (BnetInvitation invite in removedList)
			{
				RemoveItem(isHeader: false, MobileFriendListItem.TypeFlags.Request, invite);
			}
		}
		foreach (FriendListRequestFrame renderedItem in GetRenderedItems<FriendListRequestFrame>())
		{
			renderedItem.UpdateInvite();
		}
		if (addedList == null)
		{
			return;
		}
		foreach (BnetInvitation invite2 in addedList)
		{
			FriendListItem item = new FriendListItem(isHeader: false, MobileFriendListItem.TypeFlags.Request, invite2);
			AddItem(item);
		}
	}

	private void UpdateAllFriends(List<BnetPlayer> addedList, List<BnetPlayer> removedList)
	{
		if (removedList == null && addedList == null)
		{
			return;
		}
		if (removedList != null)
		{
			foreach (BnetPlayer friend in removedList)
			{
				RemoveItem(isHeader: false, MobileFriendListItem.TypeFlags.Friend, friend);
			}
		}
		UpdateFriendItems();
		if (addedList != null)
		{
			foreach (BnetPlayer friend2 in addedList)
			{
				friend2.GetPersistentGameId();
				FriendListItem item = new FriendListItem(isHeader: false, MobileFriendListItem.TypeFlags.Friend, friend2);
				AddItem(item);
			}
		}
		SortAndRefreshTouchList();
		UpdateRemoveFriendVisibility();
	}

	private FriendListFriendFrame FindRenderedBaseFriendFrame(BnetPlayer friend)
	{
		return FindFirstRenderedItem((FriendListFriendFrame frame) => frame.GetFriend() == friend);
	}

	private void UpdateFriendFrame(BnetPlayer friend)
	{
		FriendListFriendFrame friendFrame = FindRenderedBaseFriendFrame(friend);
		if (friendFrame != null)
		{
			friendFrame.UpdateFriend();
		}
	}

	private MobileFriendListItem CreatePlayerFrame(BnetPlayer player, MobileFriendListItem.TypeFlags typeFlag)
	{
		FriendListFriendFrame playerFrame = UnityEngine.Object.Instantiate(prefabs.friendItem);
		(playerFrame.GetWidget() as WidgetTemplate).OnInstantiated();
		UberText[] elems = UberText.EnableAllTextInObject(playerFrame.gameObject, enable: false);
		playerFrame.Initialize(player, typeFlag == MobileFriendListItem.TypeFlags.RecentPlayer);
		MobileFriendListItem result = FinishCreateVisualItem(playerFrame, typeFlag, FindHeader(typeFlag), playerFrame.gameObject);
		UberText.EnableAllTextObjects(elems, enable: true);
		return result;
	}

	private MobileFriendListItem CreateRequestFrame(BnetInvitation invite)
	{
		FriendListRequestFrame requestFrame = UnityEngine.Object.Instantiate(prefabs.requestItem);
		UberText[] elems = UberText.EnableAllTextInObject(requestFrame.gameObject, enable: false);
		requestFrame.SetInvite(invite);
		MobileFriendListItem result = FinishCreateVisualItem(requestFrame, MobileFriendListItem.TypeFlags.Request, FindHeader(MobileFriendListItem.TypeFlags.Request), requestFrame.gameObject);
		UberText.EnableAllTextObjects(elems, enable: true);
		return result;
	}

	private void UpdateAllHeaders()
	{
		UpdateRequestsHeader();
		if (NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().RecentFriendListDisplayEnabled)
		{
			UpdatePlayersHeader(MobileFriendListItem.TypeFlags.RecentPlayer);
		}
		UpdatePlayersHeader(MobileFriendListItem.TypeFlags.NearbyPlayer);
		UpdateFriendsHeader();
		UpdateAddAFriendCallout();
	}

	private void UpdateAllHeaderBackgrounds()
	{
		UpdateHeaderBackground(FindHeader(MobileFriendListItem.TypeFlags.Request));
	}

	private void UpdateRequestsHeader(FriendListItemHeader header = null)
	{
		int count = 0;
		foreach (FriendListItem allItem in m_allItems)
		{
			if (allItem.ItemMainType == MobileFriendListItem.TypeFlags.Request)
			{
				count++;
			}
		}
		if (count > 0)
		{
			string text = GameStrings.Format("GLOBAL_FRIENDLIST_REQUESTS_HEADER", count);
			if (header == null)
			{
				header = FindOrCreateHeader(MobileFriendListItem.TypeFlags.Request);
				if (!DoesHeaderExist(MobileFriendListItem.TypeFlags.Request))
				{
					FriendListItem item = new FriendListItem(isHeader: true, MobileFriendListItem.TypeFlags.Request, null);
					AddItem(item);
				}
			}
			header.SetText(text);
		}
		else if (header == null)
		{
			RemoveItem(isHeader: true, MobileFriendListItem.TypeFlags.Request, null);
		}
	}

	private void UpdatePlayersHeader(MobileFriendListItem.TypeFlags typeFlag)
	{
		int count = 0;
		foreach (FriendListItem allItem in m_allItems)
		{
			if (allItem.ItemMainType == typeFlag)
			{
				count++;
			}
		}
		if ((typeFlag == MobileFriendListItem.TypeFlags.RecentPlayer || typeFlag == MobileFriendListItem.TypeFlags.NearbyPlayer) && count == 0)
		{
			RemoveItem(isHeader: true, typeFlag, null);
			return;
		}
		FriendListItemHeader header = FindOrCreateHeader(typeFlag);
		if (!DoesHeaderExist(typeFlag))
		{
			FriendListItem item = new FriendListItem(isHeader: true, typeFlag, null);
			AddItem(item);
		}
		switch (typeFlag)
		{
		case MobileFriendListItem.TypeFlags.RecentPlayer:
			header.SetText(GameStrings.Format("GLOBAL_FRIENDLIST_RECENT_PLAYERS_HEADER", count));
			break;
		case MobileFriendListItem.TypeFlags.NearbyPlayer:
		{
			m_hasNearbyPlayers = count > 0;
			string text = GameStrings.Format("GLOBAL_FRIENDLIST_NEARBY_PLAYERS_HEADER", count);
			FriendListNearbyPlayersHeader nearbyPlayersHeader = header as FriendListNearbyPlayersHeader;
			if (nearbyPlayersHeader != null)
			{
				nearbyPlayersHeader.SetText(count);
				break;
			}
			header.SetText(text);
			Debug.LogError("FriendListFrame: Could not cast header to type FriendListNearbyPlayersHeader.");
			break;
		}
		}
		if (header != null)
		{
			header.SetToggleEnabled(enabled: false);
		}
	}

	private List<FriendListItem> GetFriendItems()
	{
		List<FriendListItem> friendItems = new List<FriendListItem>();
		foreach (FriendListItem item in m_allItems)
		{
			if (item.ItemMainType == MobileFriendListItem.TypeFlags.Friend)
			{
				friendItems.Add(item);
			}
		}
		return friendItems;
	}

	private void UpdateFriendsHeader()
	{
		List<FriendListItem> friendItems = GetFriendItems();
		int onlineCount = 0;
		foreach (FriendListItem item2 in friendItems)
		{
			if (item2.GetFriend().IsOnline())
			{
				onlineCount++;
			}
		}
		int totalCount = friendItems.Count;
		if (totalCount > 0)
		{
			string text = null;
			text = ((onlineCount != totalCount) ? GameStrings.Format("GLOBAL_FRIENDLIST_FRIENDS_HEADER", onlineCount, totalCount) : GameStrings.Format("GLOBAL_FRIENDLIST_FRIENDS_HEADER_ALL_ONLINE", onlineCount));
			FriendListItemHeader friendListItemHeader = FindOrCreateHeader(MobileFriendListItem.TypeFlags.Friend);
			if (!DoesHeaderExist(MobileFriendListItem.TypeFlags.Friend))
			{
				FriendListItem item = new FriendListItem(isHeader: true, MobileFriendListItem.TypeFlags.Friend, null);
				AddItem(item);
			}
			friendListItemHeader.SetText(text);
			friendListItemHeader.SetToggleEnabled(enabled: false);
		}
		else if (DoesHeaderExist(MobileFriendListItem.TypeFlags.Friend))
		{
			RemoveItem(isHeader: true, MobileFriendListItem.TypeFlags.Friend, null);
		}
	}

	private void UpdateHeaderBackground(FriendListItemHeader itemHeader)
	{
		if (itemHeader == null)
		{
			return;
		}
		MobileFriendListItem header = itemHeader.GetComponent<MobileFriendListItem>();
		if (header == null)
		{
			return;
		}
		TiledBackground background = null;
		if (itemHeader.Background == null)
		{
			GameObject backgroundObj = new GameObject("ItemsBackground");
			backgroundObj.transform.parent = header.transform;
			TransformUtil.Identity(backgroundObj);
			backgroundObj.layer = 24;
			HeaderBackgroundInfo bgInfo = (((header.Type & MobileFriendListItem.TypeFlags.Request) != 0) ? listInfo.requestBackgroundInfo : listInfo.currentGameBackgroundInfo);
			backgroundObj.AddComponent<MeshFilter>().mesh = bgInfo.mesh;
			backgroundObj.AddComponent<MeshRenderer>().SetMaterial(bgInfo.material);
			background = backgroundObj.AddComponent<TiledBackground>();
			itemHeader.Background = backgroundObj;
		}
		else
		{
			background = itemHeader.Background.GetComponent<TiledBackground>();
		}
		background.transform.parent = null;
		MobileFriendListItem.TypeFlags type = header.Type ^ MobileFriendListItem.TypeFlags.Header;
		Bounds bounds = new Bounds(header.transform.position, Vector3.zero);
		foreach (ITouchListItem renderedItem in items.RenderedItems)
		{
			MobileFriendListItem mobileItem = renderedItem as MobileFriendListItem;
			if (mobileItem != null && (mobileItem.Type & type) != 0)
			{
				bounds.Encapsulate(mobileItem.ComputeWorldBounds());
			}
		}
		background.transform.parent = header.transform.parent.transform;
		bounds.center = background.transform.parent.transform.InverseTransformPoint(bounds.center);
		background.SetBounds(bounds);
		TransformUtil.SetPosZ(background.transform, 2f);
		background.gameObject.SetActive(itemHeader.IsShowingContents);
	}

	private FriendListItemHeader FindHeader(MobileFriendListItem.TypeFlags type)
	{
		type |= MobileFriendListItem.TypeFlags.Header;
		m_headers.TryGetValue(type, out var header);
		return header;
	}

	private bool DoesHeaderExist(MobileFriendListItem.TypeFlags type)
	{
		foreach (FriendListItem item in m_allItems)
		{
			if (item.IsHeader && item.SubType == type)
			{
				return true;
			}
		}
		return false;
	}

	private FriendListItemHeader FindOrCreateHeader(MobileFriendListItem.TypeFlags type)
	{
		type |= MobileFriendListItem.TypeFlags.Header;
		FriendListItemHeader header = FindHeader(type);
		if (header == null)
		{
			FriendListItem item = new FriendListItem(isHeader: true, type, null);
			if (type == (MobileFriendListItem.TypeFlags.NearbyPlayer | MobileFriendListItem.TypeFlags.Header))
			{
				header = UnityEngine.Object.Instantiate(prefabs.nearbyPlayersHeaderItem);
				header.SetToggleEnabled(enabled: false);
			}
			else
			{
				header = UnityEngine.Object.Instantiate(prefabs.headerItem);
			}
			m_headers[type] = header;
			Option setoption = Option.FRIENDS_LIST_FRIEND_SECTION_HIDE;
			switch (item.SubType)
			{
			case MobileFriendListItem.TypeFlags.Request:
				setoption = Option.FRIENDS_LIST_REQUEST_SECTION_HIDE;
				break;
			case MobileFriendListItem.TypeFlags.NearbyPlayer:
				setoption = Option.FRIENDS_LIST_NEARBYPLAYER_SECTION_HIDE;
				break;
			case MobileFriendListItem.TypeFlags.Friend:
			case MobileFriendListItem.TypeFlags.CurrentGame:
				setoption = Option.FRIENDS_LIST_FRIEND_SECTION_HIDE;
				break;
			}
			header.SubType = item.SubType;
			header.Option = setoption;
			bool showContents = GetShowHeaderSection(setoption);
			header.SetInitialShowContents(showContents);
			header.ClearToggleListeners();
			header.AddToggleListener(OnHeaderSectionToggle, header);
			UberText[] objs = UberText.EnableAllTextInObject(header.gameObject, enable: false);
			FinishCreateVisualItem(header, type, null, null);
			UberText.EnableAllTextObjects(objs, enable: true);
		}
		return header;
	}

	private void OnHeaderSectionToggle(bool show, object userdata)
	{
		FriendListItemHeader header = (FriendListItemHeader)userdata;
		SetShowHeaderSection(header.Option, show);
		int headerIndex = m_allItems.FindIndex((FriendListItem item) => item.IsHeader && item.SubType == header.SubType);
		items.RefreshList(headerIndex, preserveScrolling: true);
		UpdateHeaderBackground(header);
	}

	public T FindFirstRenderedItem<T>(Predicate<T> predicate = null) where T : MonoBehaviour
	{
		foreach (ITouchListItem renderedItem in items.RenderedItems)
		{
			T component = renderedItem.GetComponent<T>();
			if (component != null && (predicate == null || predicate(component)))
			{
				return component;
			}
		}
		return null;
	}

	private List<T> GetRenderedItems<T>() where T : MonoBehaviour
	{
		List<T> allItems = new List<T>();
		foreach (ITouchListItem renderedItem in items.RenderedItems)
		{
			T component = renderedItem.GetComponent<T>();
			if (component != null)
			{
				allItems.Add(component);
			}
		}
		return allItems;
	}

	private MobileFriendListItem FinishCreateVisualItem<T>(T obj, MobileFriendListItem.TypeFlags type, ITouchListItem parent, GameObject showObj) where T : MonoBehaviour
	{
		MobileFriendListItem item = obj.gameObject.GetComponent<MobileFriendListItem>();
		if (item == null)
		{
			item = obj.gameObject.AddComponent<MobileFriendListItem>();
			if (obj is FriendListFriendFrame)
			{
				((FriendListFriendFrame)(object)obj).InitializeMobileFriendListItem(this, item);
			}
			BoxCollider collider = item.GetComponent<BoxCollider>();
			if (collider != null)
			{
				collider.size = new Vector3(collider.size.x, collider.size.y, collider.size.z + items.elementSpacing);
			}
		}
		item.Type = type;
		item.SetShowObject(showObj);
		item.SetParent(parent);
		if (item.Selectable)
		{
			BnetPlayer selectedFriend = FriendMgr.Get().GetSelectedFriend();
			if (selectedFriend != null)
			{
				BnetPlayer thisFriend = null;
				if (obj is FriendListFriendFrame)
				{
					thisFriend = ((FriendListFriendFrame)(object)obj).GetFriend();
				}
				if (thisFriend != null && selectedFriend == thisFriend)
				{
					item.Selected();
				}
			}
		}
		return item;
	}

	private bool RemoveItem(bool isHeader, MobileFriendListItem.TypeFlags type, object itemToRemove)
	{
		int index = m_allItems.FindIndex(delegate(FriendListItem item)
		{
			if (item.IsHeader != isHeader || item.SubType != type)
			{
				return false;
			}
			if (itemToRemove == null)
			{
				return true;
			}
			switch (type)
			{
			case MobileFriendListItem.TypeFlags.Request:
				return item.GetInvite() == (BnetInvitation)itemToRemove;
			case MobileFriendListItem.TypeFlags.RecentPlayer:
				return item.GetRecentPlayer() == (BnetPlayer)itemToRemove;
			case MobileFriendListItem.TypeFlags.NearbyPlayer:
				return item.GetNearbyPlayer() == (BnetPlayer)itemToRemove;
			case MobileFriendListItem.TypeFlags.Friend:
			case MobileFriendListItem.TypeFlags.CurrentGame:
				return item.GetFriend() == (BnetPlayer)itemToRemove;
			default:
				return false;
			}
		});
		if (index < 0)
		{
			return false;
		}
		m_allItems.RemoveAt(index);
		return true;
	}

	private void AddItem(FriendListItem itemToAdd)
	{
		m_allItems.Add(itemToAdd);
	}

	private void SuspendItemsLayout()
	{
		items.SuspendLayout();
	}

	private void ResumeItemsLayout()
	{
		items.ResumeLayout(repositionItems: false);
	}

	public void ToggleRemoveFriendsMode()
	{
		FriendListEditMode newMode;
		switch (m_editMode)
		{
		case FriendListEditMode.NONE:
			newMode = FriendListEditMode.REMOVE_FRIENDS;
			break;
		case FriendListEditMode.REMOVE_FRIENDS:
			newMode = FriendListEditMode.NONE;
			break;
		default:
			Log.All.PrintError("FriendListFrame: Should not be toggling Remove Friends mode when in mode {0}!", m_editMode);
			return;
		}
		SetEditFriendsMode(newMode);
		removeFriendButtonDisabledVisual.SetActive(m_editMode == FriendListEditMode.REMOVE_FRIENDS);
		removeFriendButtonEnabledVisual.SetActive(m_editMode == FriendListEditMode.NONE);
		removeFriendButtonButtonGlow.SetActive(m_editMode == FriendListEditMode.REMOVE_FRIENDS);
		UpdateRemoveFriendVisibility();
	}

	private void UpdateRemoveFriendVisibility()
	{
		bool wasRemoveFriendButtonVisible = m_isRemoveFriendsShowing;
		m_isRemoveFriendsShowing = BnetFriendMgr.Get().GetFriendCount() > 0 || m_editMode == FriendListEditMode.REMOVE_FRIENDS;
		if (m_isRemoveFriendsShowing != wasRemoveFriendButtonVisible)
		{
			SetFlyoutMenuButtonVisible(FriendListFlyoutButton.RemoveFriend, m_isRemoveFriendsShowing);
		}
	}

	private void UpdateFlyoutMenuLayout()
	{
		List<Transform> buttonsToLayout = new List<Transform>();
		for (int i = 0; i < flyoutMenuButtonsMap.Count; i++)
		{
			FlyoutMenuButtonPair btnPair = flyoutMenuButtonsMap[i];
			if (btnPair.PegUIElement == null)
			{
				continue;
			}
			if (!IsFlyoutMenuButtonVisible(btnPair.ButtonType))
			{
				btnPair.PegUIElement.gameObject.SetActive(value: false);
				continue;
			}
			btnPair.PegUIElement.gameObject.SetActive(value: true);
			if (flyoutButtonBones.Length <= i)
			{
				Log.All.PrintError("[FriendListFrame] Flyout menu layout failed - mismatch with buttons and bones.");
				return;
			}
			buttonsToLayout.Add(btnPair.PegUIElement.transform);
		}
		float buttonHeight = m_flyoutButtonWorldSize.z;
		float buttonPaddingBetweenButtons = buttonHeight * flyoutPaddingBetweenButtons;
		float topMarginHeight = buttonHeight * flyoutMiddleFrameTopMargin;
		float bottomMarginHeight = buttonHeight * flyoutMiddleFrameBottomMargin;
		float num = (float)buttonsToLayout.Count * buttonHeight + (float)(buttonsToLayout.Count - 1) * buttonPaddingBetweenButtons + topMarginHeight + bottomMarginHeight;
		float newFrameScale = num / m_flyoutMiddleFrameBaseWorldSize.z;
		flyoutMiddleFrame.transform.localScale = new Vector3(flyoutMiddleFrame.transform.localScale.x, newFrameScale, flyoutMiddleFrame.transform.localScale.z);
		float shadowBottomPadding = buttonHeight * flyoutMiddleShadowBottomPadding;
		float newShadowScale = (num + shadowBottomPadding) / m_flyoutMiddleShadowBaseWorldSize.z;
		flyoutMiddleShadow.transform.localScale = new Vector3(flyoutMiddleShadow.transform.localScale.x, newShadowScale, flyoutMiddleShadow.transform.localScale.z);
		flyoutFrameContainer.UpdateSlices();
		flyoutShadowContainer.UpdateSlices();
		Transform referenceAnchor = flyoutMiddleFrame.transform;
		float topExtent = m_flyoutMiddleFrameRenderer.bounds.center.z + m_flyoutMiddleFrameRenderer.bounds.extents.z;
		Vector3 nextButtonPos = new Vector3(referenceAnchor.position.x, referenceAnchor.position.y, topExtent - topMarginHeight - buttonHeight / 2f);
		foreach (Transform item in buttonsToLayout)
		{
			item.position = nextButtonPos;
			nextButtonPos.z -= buttonHeight + buttonPaddingBetweenButtons;
		}
		m_isFlyoutMenuLayoutDirty = false;
	}

	private bool IsFlyoutMenuButtonVisible(FriendListFlyoutButton button)
	{
		return (m_buttonsToShow & button) > FriendListFlyoutButton.None;
	}

	private bool SetEditFriendsMode(FriendListEditMode mode)
	{
		m_editMode = mode;
		SortAndRefreshTouchList();
		UpdateFriendItems();
		return true;
	}

	public void ExitRemoveFriendsMode()
	{
		if (m_editMode == FriendListEditMode.REMOVE_FRIENDS)
		{
			ToggleRemoveFriendsMode();
		}
	}

	private void UpdateAddAFriendCallout()
	{
		if (addAFriendCallout != null)
		{
			bool shouldShowAddAFriendCallout = m_allItems.Count == 0 && !TemporaryAccountManager.IsTemporaryAccount();
			addAFriendCallout.SetActive(shouldShowAddAFriendCallout);
		}
	}

	private void SortAndRefreshTouchList()
	{
		if (!items.IsLayoutSuspended)
		{
			m_allItems.Sort(ItemsSortCompare);
			if (m_longListBehavior == null)
			{
				m_longListBehavior = new VirtualizedFriendsListBehavior(this);
				items.LongListBehavior = m_longListBehavior;
			}
			else
			{
				items.RefreshList(0, preserveScrolling: true);
			}
		}
	}

	private int ItemsSortCompare(FriendListItem item1, FriendListItem item2)
	{
		if (!m_hasNearbyPlayers)
		{
			if ((item1.ItemFlags & MobileFriendListItem.TypeFlags.NearbyPlayer) != 0)
			{
				return 1;
			}
			if ((item2.ItemFlags & MobileFriendListItem.TypeFlags.NearbyPlayer) != 0)
			{
				return -1;
			}
		}
		int result = item2.ItemFlags.CompareTo(item1.ItemFlags);
		if (result != 0)
		{
			return result;
		}
		switch (item1.ItemFlags)
		{
		case MobileFriendListItem.TypeFlags.Friend:
		case MobileFriendListItem.TypeFlags.CurrentGame:
			return FriendUtils.FriendSortCompare(item1.GetFriend(), item2.GetFriend());
		case MobileFriendListItem.TypeFlags.RecentPlayer:
			return FriendUtils.RecentFriendSortCompare(item1.GetRecentPlayer(), item2.GetRecentPlayer());
		case MobileFriendListItem.TypeFlags.NearbyPlayer:
			return FriendUtils.FriendSortCompare(item1.GetNearbyPlayer(), item2.GetNearbyPlayer());
		case MobileFriendListItem.TypeFlags.Request:
		{
			BnetInvitation invite1 = item1.GetInvite();
			BnetInvitation invite2 = item2.GetInvite();
			int nameCompareResult = string.Compare(invite1.GetInviterName(), invite2.GetInviterName(), ignoreCase: true);
			if (nameCompareResult != 0)
			{
				return nameCompareResult;
			}
			ulong low = invite1.GetInviterId().Low;
			long invite2AccountIdLo = (long)invite2.GetInviterId().Low;
			return (int)((long)low - invite2AccountIdLo);
		}
		default:
			return 0;
		}
	}

	private void RegisterFriendEvents()
	{
		BnetFriendMgr.Get().AddChangeListener(OnFriendsChanged);
		BnetPresenceMgr.Get().AddPlayersChangedListener(OnPlayersChanged);
		FriendChallengeMgr.Get().AddChangedListener(OnFriendChallengeChanged);
		BnetRecentPlayerMgr.Get().AddChangeListener(OnRecentPlayersChanged);
		BnetNearbyPlayerMgr.Get().AddChangeListener(OnNearbyPlayersChanged);
		SceneMgr.Get().RegisterScenePreUnloadEvent(OnScenePreUnload);
		SpectatorManager.Get().OnInviteReceived += SpectatorManager_OnInviteReceivedOrSent;
		SpectatorManager.Get().OnInviteSent += SpectatorManager_OnInviteReceivedOrSent;
	}

	private void UnregisterFriendEvents()
	{
		BnetFriendMgr.RemoveChangeListenerFromInstance(OnFriendsChanged);
		BnetPresenceMgr.RemovePlayersChangedListenerFromInstance(OnPlayersChanged);
		FriendChallengeMgr.RemoveChangedListenerFromInstance(OnFriendChallengeChanged);
		BnetRecentPlayerMgr.Get()?.RemoveChangeListenerFromInstance(OnRecentPlayersChanged);
		BnetNearbyPlayerMgr.RemoveChangeListenerFromInstance(OnNearbyPlayersChanged);
		SceneMgr.Get()?.UnregisterScenePreUnloadEvent(OnScenePreUnload);
		if (SpectatorManager.InstanceExists())
		{
			SpectatorManager spectatorManager = SpectatorManager.Get();
			spectatorManager.OnInviteReceived -= SpectatorManager_OnInviteReceivedOrSent;
			spectatorManager.OnInviteSent -= SpectatorManager_OnInviteReceivedOrSent;
		}
	}

	private void OnFriendsChanged(BnetFriendChangelist changelist, object userData)
	{
		SuspendItemsLayout();
		UpdateRequests(changelist.GetAddedReceivedInvites(), changelist.GetRemovedReceivedInvites());
		UpdateAllFriends(changelist.GetAddedFriends(), changelist.GetRemovedFriends());
		UpdateAllHeaders();
		ResumeItemsLayout();
		SortAndRefreshTouchList();
		UpdateAllHeaderBackgrounds();
		UpdateSelectedItem();
	}

	private void OnRecentPlayersChanged(BnetRecentOrNearbyPlayerChangelist changelist, object userData)
	{
		m_recentPlayersNeedUpdate = true;
		OnPlayerChangedCommon(changelist, m_recentPlayerUpdates);
		if (base.gameObject.activeInHierarchy)
		{
			HandleRecentPlayersChanged();
		}
	}

	private void OnNearbyPlayersChanged(BnetRecentOrNearbyPlayerChangelist changelist, object userData)
	{
		m_nearbyPlayersNeedUpdate = true;
		OnPlayerChangedCommon(changelist, m_nearbyPlayerUpdates);
		if (base.gameObject.activeInHierarchy && Time.realtimeSinceStartup >= m_lastNearbyPlayersUpdate + 10f)
		{
			HandleNearbyPlayersChanged();
		}
	}

	private void OnPlayerChangedCommon(BnetRecentOrNearbyPlayerChangelist changelist, List<PlayerUpdate> playerUpdates)
	{
		if (changelist.GetAddedStrangers() != null)
		{
			foreach (BnetPlayer player in changelist.GetAddedStrangers())
			{
				PlayerUpdate update = new PlayerUpdate(PlayerUpdate.ChangeType.Added, player);
				playerUpdates.Remove(update);
				playerUpdates.Add(update);
			}
		}
		if (changelist.GetRemovedStrangers() != null)
		{
			foreach (BnetPlayer player2 in changelist.GetRemovedStrangers())
			{
				PlayerUpdate update2 = new PlayerUpdate(PlayerUpdate.ChangeType.Removed, player2);
				playerUpdates.Remove(update2);
				playerUpdates.Add(update2);
			}
		}
		if (changelist.GetAddedFriends() != null)
		{
			foreach (BnetPlayer player3 in changelist.GetAddedFriends())
			{
				PlayerUpdate update3 = new PlayerUpdate(PlayerUpdate.ChangeType.Added, player3);
				playerUpdates.Remove(update3);
				playerUpdates.Add(update3);
			}
		}
		if (changelist.GetRemovedFriends() != null)
		{
			foreach (BnetPlayer player4 in changelist.GetRemovedFriends())
			{
				PlayerUpdate update4 = new PlayerUpdate(PlayerUpdate.ChangeType.Removed, player4);
				playerUpdates.Remove(update4);
				playerUpdates.Add(update4);
			}
		}
		if (changelist.GetAddedPlayers() != null)
		{
			foreach (BnetPlayer player5 in changelist.GetAddedPlayers())
			{
				PlayerUpdate update5 = new PlayerUpdate(PlayerUpdate.ChangeType.Added, player5);
				playerUpdates.Remove(update5);
				playerUpdates.Add(update5);
			}
		}
		if (changelist.GetRemovedPlayers() == null)
		{
			return;
		}
		foreach (BnetPlayer player6 in changelist.GetRemovedPlayers())
		{
			PlayerUpdate update6 = new PlayerUpdate(PlayerUpdate.ChangeType.Removed, player6);
			playerUpdates.Remove(update6);
			playerUpdates.Add(update6);
		}
	}

	private void HandleRecentPlayersChanged()
	{
		if (m_recentPlayersNeedUpdate)
		{
			HandlePlayersChangedCommon(m_recentPlayerUpdates, MobileFriendListItem.TypeFlags.RecentPlayer);
			m_recentPlayersNeedUpdate = false;
		}
	}

	private void HandleNearbyPlayersChanged()
	{
		if (m_nearbyPlayersNeedUpdate)
		{
			HandlePlayersChangedCommon(m_nearbyPlayerUpdates, MobileFriendListItem.TypeFlags.NearbyPlayer);
			m_nearbyPlayersNeedUpdate = false;
			m_lastNearbyPlayersUpdate = Time.realtimeSinceStartup;
		}
	}

	private void HandlePlayersChangedCommon(List<PlayerUpdate> playerUpdates, MobileFriendListItem.TypeFlags typeFlag)
	{
		UpdateFriendItems();
		BnetFriendChangelist friendsChangeList = null;
		if (playerUpdates.Count > 0)
		{
			SuspendItemsLayout();
			foreach (PlayerUpdate update in playerUpdates)
			{
				if (update.Change == PlayerUpdate.ChangeType.Added)
				{
					FriendListItem item = new FriendListItem(isHeader: false, typeFlag, update.Player);
					AddItem(item);
				}
				else
				{
					RemoveItem(isHeader: false, typeFlag, update.Player);
				}
			}
			playerUpdates.Clear();
			UpdateAllHeaders();
			ResumeItemsLayout();
			SortAndRefreshTouchList();
			UpdateAllHeaderBackgrounds();
			UpdateSelectedItem();
		}
		if (friendsChangeList != null)
		{
			OnFriendsChanged(friendsChangeList, null);
		}
	}

	private void DoPlayersChanged(BnetPlayerChangelist changelist)
	{
		SuspendItemsLayout();
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		bool myFriendChallengeChanged = false;
		bool friendNameChanged = false;
		foreach (BnetPlayerChange change in changelist.GetChanges())
		{
			BnetPlayer oldPlayer = change.GetOldPlayer();
			BnetPlayer newPlayer = change.GetNewPlayer();
			if (newPlayer == myself)
			{
				UpdateMyself();
				BnetGameAccount hsGameAccount = newPlayer.GetHearthstoneGameAccount();
				myFriendChallengeChanged = ((oldPlayer != null && !(oldPlayer.GetHearthstoneGameAccount() == null)) ? (oldPlayer.GetHearthstoneGameAccount().CanBeInvitedToGame() != hsGameAccount.CanBeInvitedToGame()) : hsGameAccount.CanBeInvitedToGame());
				continue;
			}
			if (oldPlayer == null || oldPlayer.GetBestName() != newPlayer.GetBestName())
			{
				friendNameChanged = true;
			}
			UpdateFriendFrame(newPlayer);
		}
		if (myFriendChallengeChanged)
		{
			UpdateItems();
		}
		else if (friendNameChanged)
		{
			UpdateFriendItems();
		}
		UpdateAllHeaders();
		UpdateAllHeaderBackgrounds();
		ResumeItemsLayout();
	}

	private void OnPlayersChanged(BnetPlayerChangelist changelist, object userData)
	{
		if (base.gameObject.activeInHierarchy)
		{
			DoPlayersChanged(changelist);
			return;
		}
		List<BnetPlayerChange> changes = changelist.GetChanges();
		m_playersChangeList.GetChanges().AddRange(changes);
	}

	private void OnScenePreUnload(SceneMgr.Mode prevMode, PegasusScene prevScene, object userData)
	{
		SceneMgr.Mode mode = SceneMgr.Get().GetMode();
		if ((uint)(mode - 8) <= 1u)
		{
			if (ChatMgr.Get() != null)
			{
				ChatMgr.Get().CloseFriendsList();
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	private void SpectatorManager_OnInviteReceivedOrSent(OnlineEventType evt, BnetPlayer inviter)
	{
		UpdateFriendFrame(inviter);
	}

	private void OnFriendChallengeChanged(FriendChallengeEvent challengeEvent, BnetPlayer player, FriendlyChallengeData challengeData, object userData)
	{
		if (player == BnetPresenceMgr.Get().GetMyPlayer())
		{
			UpdateFriendItems();
		}
		else
		{
			UpdateFriendFrame(player);
		}
	}

	private void OnFlyoutClosed()
	{
		items.SetScrollingEnabled(enable: true);
	}

	private void OnFlyoutOpened()
	{
		items.SetScrollingEnabled(enable: false);
	}

	private bool HandleKeyboardInput()
	{
		FatalErrorMgr.Get().HasError();
		return false;
	}

	private void OnAddFriendButtonReleased(UIEvent e)
	{
		CloseFlyoutMenu();
		if (!BnetFriendMgr.Get().IsFriendInviteFeatureEnabled)
		{
			GameObject popupGO = AssetLoader.Get().InstantiatePrefab("PrivacyPopups.prefab:99a8f571a8a35a54e90790c904bc94f8");
			PrivacyFeaturesPopup privacyPopup = popupGO.GetComponent<PrivacyFeaturesPopup>();
			privacyPopup.Set(PrivacyFeatures.CHAT, BnetFriendMgr.Get().IsFriendInviteFeatureEnabled, delegate
			{
				PrivacyGate.Get().SetFeature(PrivacyFeatures.CHAT, isEnabled: true);
			}, delegate
			{
				ClosePrivacyPopup(privacyPopup);
				OnAddFriendAllowed();
			}, delegate
			{
				ClosePrivacyPopup(privacyPopup);
			});
			privacyPopup.Show();
		}
		else
		{
			OnAddFriendAllowed();
		}
	}

	private void ClosePrivacyPopup(PrivacyFeaturesPopup privacyPopup)
	{
		privacyPopup.Hide();
		UnityEngine.Object.Destroy(privacyPopup.gameObject, 1f);
	}

	private void OnAddFriendAllowed()
	{
		if (m_addFriendFrame != null)
		{
			CloseAddFriendFrame();
			return;
		}
		if (this.AddFriendFrameOpened != null)
		{
			this.AddFriendFrameOpened();
		}
		BnetPlayer selectedFriend = FriendMgr.Get().GetSelectedFriend();
		ShowAddFriendFrame(selectedFriend);
	}

	private void OnEditFriendsButtonReleased(UIEvent e)
	{
		ToggleRemoveFriendsMode();
	}

	private void OnRAFButtonReleased(UIEvent e)
	{
		if (m_isRAFButtonEnabled)
		{
			RAFManager.Get().ShowRAFFrame();
			TelemetryManager.Client().SendClickRecruitAFriend();
		}
	}

	private void OnRAFButtonOver(UIEvent e)
	{
		TooltipZone tooltipZone = rafButton.GetComponent<TooltipZone>();
		if (!(tooltipZone == null))
		{
			string tooltipDesc = ((GameUtils.GetNextTutorial() != 0) ? GameStrings.Get("GLUE_RAF_TOOLTIP_LOCKED_DESC") : GameStrings.Get("GLUE_RAF_TOOLTIP_DESC"));
			tooltipZone.ShowSocialTooltip(rafButton, GameStrings.Get("GLUE_RAF_TOOLTIP_HEADLINE"), tooltipDesc, 75f, GameLayer.BattleNetDialog);
		}
	}

	private void OnRAFButtonOut(UIEvent e)
	{
		TooltipZone tooltipZone = rafButton.GetComponent<TooltipZone>();
		if (tooltipZone != null)
		{
			tooltipZone.HideTooltip();
		}
	}

	private void OnPrivacySettingsButtonReleased(UIEvent e)
	{
		PrivacySettingsMenu privacySettingsMenu = ((!PlatformSettings.IsMobile()) ? AssetLoader.Get().InstantiatePrefab(PrivacyMenu.OPTIONS_MENU_PRIVACY_SETTINGS).GetComponent<PrivacySettingsMenu>() : AssetLoader.Get().InstantiatePrefab(PrivacyMenu.OPTIONS_MENU_PRIVACY_SETTINGS_PHONE).GetComponent<PrivacySettingsMenu>());
		privacySettingsMenu.Show();
	}

	private bool OnRemoveFriendDialogShown(DialogBase dialog, object userData)
	{
		BnetPlayer friend = (BnetPlayer)userData;
		if (!BnetFriendMgr.Get().IsFriend(friend))
		{
			return false;
		}
		m_removeFriendPopup = (AlertPopup)dialog;
		return true;
	}

	private void OnRemoveFriendPopupResponse(AlertPopup.Response response, object userData)
	{
		if (response == AlertPopup.Response.CONFIRM && m_friendToRemove != null)
		{
			BnetFriendMgr.Get().RemoveFriend(m_friendToRemove);
		}
		m_friendToRemove = null;
		m_removeFriendPopup = null;
		if (this.RemoveFriendPopupClosed != null)
		{
			this.RemoveFriendPopupClosed();
		}
	}

	private void OnFlyoutButtonReleased(UIEvent e)
	{
		if (IsInEditMode)
		{
			ExitRemoveFriendsMode();
		}
		else if (m_flyoutOpen)
		{
			CloseFlyoutMenu();
		}
		else
		{
			OpenFlyoutMenu();
		}
		if (ChatMgr.Get().IsChatLogUIShowing())
		{
			ChatMgr.Get().CloseChatUI(closeFriendList: false);
		}
	}

	private void UpdateSelectedItem()
	{
		BnetPlayer selectedFriend = FriendMgr.Get().GetSelectedFriend();
		FriendListFriendFrame frame = FindRenderedBaseFriendFrame(selectedFriend);
		if (frame == null)
		{
			if (items.SelectedIndex == -1)
			{
				return;
			}
			items.SelectedIndex = -1;
			if (m_removeFriendPopup != null)
			{
				m_removeFriendPopup.Hide();
				m_removeFriendPopup = null;
				if (this.RemoveFriendPopupClosed != null)
				{
					this.RemoveFriendPopupClosed();
				}
			}
		}
		else
		{
			items.SelectedIndex = items.IndexOf(frame.GetComponent<MobileFriendListItem>());
		}
	}

	private void UpdateRAFState()
	{
		if (SetRotationManager.Get().ShouldShowSetRotationIntro() || SceneMgr.Get().GetMode() == SceneMgr.Mode.LOGIN || WelcomeQuests.Get() != null || TemporaryAccountManager.IsTemporaryAccount() || GameUtils.GetNextTutorial() != 0)
		{
			SetRAFButtonEnabled(enabled: false);
		}
		if (RegionUtils.IsCNLegalRegion)
		{
			SetFlyoutMenuButtonVisible(FriendListFlyoutButton.RecruitFriend, isVisible: false);
		}
	}

	private void InitFlyoutMenu()
	{
		addFriendButton.AddEventListener(UIEventType.RELEASE, OnAddFriendButtonReleased);
		addFriendCalloutButton.AddEventListener(UIEventType.RELEASE, OnAddFriendButtonReleased);
		removeFriendButton.AddEventListener(UIEventType.RELEASE, OnEditFriendsButtonReleased);
		rafButton.AddEventListener(UIEventType.RELEASE, OnRAFButtonReleased);
		rafButton.AddEventListener(UIEventType.ROLLOVER, OnRAFButtonOver);
		rafButton.AddEventListener(UIEventType.ROLLOUT, OnRAFButtonOut);
		privacySettingsButton.AddEventListener(UIEventType.RELEASE, OnPrivacySettingsButtonReleased);
		flyoutMenuButton.AddEventListener(UIEventType.RELEASE, OnFlyoutButtonReleased);
		flyoutMenuButton.AddEventListener(UIEventType.ROLLOVER, OnFlyoutButtonOver);
		flyoutMenuButton.AddEventListener(UIEventType.ROLLOUT, OnFlyoutButtonOut);
		if (addFriendButton is UIBButton exampleUIBButton)
		{
			Renderer buttonRenderer = exampleUIBButton.m_RootObject.GetComponent<Renderer>();
			m_flyoutButtonWorldSize = buttonRenderer.bounds.size;
		}
		flyoutMiddleFrame.transform.localScale = new Vector3(flyoutMiddleFrame.transform.localScale.x, 1f, flyoutMiddleFrame.transform.localScale.z);
		m_flyoutMiddleFrameRenderer = flyoutMiddleFrame.GetComponent<Renderer>();
		m_flyoutMiddleFrameBaseWorldSize = m_flyoutMiddleFrameRenderer.bounds.size;
		flyoutMiddleShadow.transform.localScale = new Vector3(flyoutMiddleShadow.transform.localScale.x, 1f, flyoutMiddleShadow.transform.localScale.z);
		Renderer middleShadowRenderer = flyoutMiddleShadow.GetComponent<Renderer>();
		m_flyoutMiddleShadowBaseWorldSize = middleShadowRenderer.bounds.size;
		UpdateRemoveFriendVisibility();
	}

	private void OnFlyoutButtonOver(UIEvent e)
	{
		m_isFlyoutButtonHovered = true;
		UpdateFlyoutButtonHighlight();
	}

	private void OnFlyoutButtonOut(UIEvent e)
	{
		m_isFlyoutButtonHovered = false;
		UpdateFlyoutButtonHighlight();
	}

	private bool GetShowHeaderSection(Option setoption)
	{
		return !(bool)Options.Get().GetOption(setoption, false);
	}

	private void SetShowHeaderSection(Option sectionoption, bool show)
	{
		if (GetShowHeaderSection(sectionoption) != show)
		{
			Options.Get().SetOption(sectionoption, !show);
		}
	}

	private Transform GetBottomRightBone()
	{
		if (!(scrollbar != null) || !scrollbar.gameObject.activeSelf)
		{
			return listInfo.bottomRight;
		}
		return listInfo.bottomRightWithScrollbar;
	}

	private void OnTemporaryAccountSignUpButtonPressed(UIEvent e)
	{
		ChatMgr.Get().CloseFriendsList();
		TemporaryAccountManager.Get().ShowHealUpPage(TemporaryAccountManager.HealUpReason.FRIENDS_LIST);
	}

	private void OnMySelectableMedalWidgetReady(Widget widget)
	{
		m_mySelectableMedal = widget.GetComponentInChildren<SelectableMedal>();
		UpdateMySelectableMedalWidget();
	}

	private void UpdatePortrait(bool medalIsDisplayable)
	{
		if (medalIsDisplayable)
		{
			myPortrait.gameObject.SetActive(value: false);
			if (portraitBackground != null)
			{
				portraitBackground.GetComponent<Renderer>().SetMaterial(rankedBackground);
			}
			return;
		}
		myPortrait.SetProgramId(BnetProgramId.HEARTHSTONE);
		myPortrait.gameObject.SetActive(value: true);
		if (portraitBackground != null)
		{
			portraitBackground.GetComponent<Renderer>().SetMaterial(unrankedBackground);
		}
	}

	private void UpdateMySelectableMedalWidget()
	{
		m_mySelectableMedal?.UpdateWidget(BnetPresenceMgr.Get().GetMyPlayer(), delegate
		{
			UpdatePortrait(medalIsDisplayable: true);
		}, delegate
		{
			UpdatePortrait(medalIsDisplayable: true);
		}, delegate
		{
			UpdatePortrait(medalIsDisplayable: false);
		});
	}
}
