using System.Collections;
using Assets;
using Hearthstone.Store;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

[CustomEditClass]
public class TavernBrawlScene : PegasusScene
{
	[CustomEditField(T = EditType.GAME_OBJECT)]
	public String_MobileOverride m_CollectionManagerPrefab;

	[CustomEditField(T = EditType.GAME_OBJECT)]
	public String_MobileOverride m_TavernBrawlPrefab;

	[CustomEditField(T = EditType.GAME_OBJECT)]
	public String_MobileOverride m_TavernBrawlNoDeckPrefab;

	private bool m_unloading;

	private GameObject m_tavernBrawlRoot;

	private bool m_collectionManagerNeeded;

	private GameObject m_collectionManager;

	private bool m_pendingSessionBegin;

	protected override void Awake()
	{
		base.Awake();
	}

	private void Start()
	{
		Network.Get().RegisterNetHandler(TavernBrawlRequestSessionBeginResponse.PacketID.ID, OnSessionBeginResponse);
		TavernBrawlManager.Get().EnsureAllDataReady(OnServerDataReady);
	}

	private void Update()
	{
		Network.Get().ProcessNetwork();
	}

	public override bool IsUnloading()
	{
		return m_unloading;
	}

	public override void Unload()
	{
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			BnetBar.Get().ToggleActive(active: true);
		}
		m_unloading = true;
		if (CollectionManager.Get().GetCollectibleDisplay() != null)
		{
			CollectionManager.Get().GetCollectibleDisplay().Unload();
		}
		if (TavernBrawlDisplay.Get() != null)
		{
			TavernBrawlDisplay.Get().Unload();
		}
		Network.Get().SendAckCardsSeen();
		StoreManager.Get().RemoveSuccessfulPurchaseAckListener(OnTavernBrawlTicketPurchaseAck);
		if (m_tavernBrawlRoot != null)
		{
			Object.Destroy(m_tavernBrawlRoot);
		}
		if (m_collectionManager != null)
		{
			Object.Destroy(m_collectionManager);
		}
		m_unloading = false;
	}

	private void OnServerDataReady()
	{
		if (TavernBrawlManager.Get().PlayerStatus == TavernBrawlStatus.TB_STATUS_INVALID)
		{
			if (!SceneMgr.Get().IsModeRequested(SceneMgr.Mode.HUB))
			{
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
			}
			if (TavernBrawlManager.Get().IsCurrentBrawlTypeActive)
			{
				AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
				if (TavernBrawlManager.Get().CurrentMission().brawlMode == TavernBrawlMode.TB_MODE_HEROIC)
				{
					info.m_headerText = GameStrings.Get("GLUE_HEROIC_BRAWL_SESSION_ERROR_TITLE");
					info.m_text = GameStrings.Get("GLUE_HEROIC_BRAWL_SESSION_ERROR");
				}
				else
				{
					info.m_headerText = GameStrings.Get("GLUE_BRAWLISEUM_SESSION_ERROR_TITLE");
					info.m_text = GameStrings.Get("GLUE_BRAWLISEUM_SESSION_ERROR");
				}
				info.m_responseCallback = delegate
				{
					TavernBrawlManager.Get().RefreshServerData();
				};
				info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
				info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
				DialogManager.Get().ShowPopup(info);
			}
			return;
		}
		CollectionDeck deck = TavernBrawlManager.Get().CurrentDeck();
		if (TavernBrawlManager.Get().CurrentSession != null && deck != null)
		{
			deck.Locked = TavernBrawlManager.Get().CurrentSession.DeckLocked;
		}
		m_collectionManagerNeeded = TavernBrawlManager.Get().CurrentMission() != null && TavernBrawlManager.Get().CurrentMission().canEditDeck;
		if (m_collectionManagerNeeded)
		{
			if (!AssetLoader.Get().InstantiatePrefab((string)m_TavernBrawlPrefab, OnTavernBrawlLoadAttempted, null, AssetLoadingOptions.IgnorePrefabPosition))
			{
				OnTavernBrawlLoadAttempted((string)m_TavernBrawlPrefab, null, null);
			}
			if (!AssetLoader.Get().InstantiatePrefab((string)m_CollectionManagerPrefab, OnCollectionManagerLoadAttempted))
			{
				OnCollectionManagerLoadAttempted((string)m_CollectionManagerPrefab, null, null);
			}
		}
		else if (!AssetLoader.Get().InstantiatePrefab((string)m_TavernBrawlNoDeckPrefab, OnTavernBrawlLoadAttempted, null, AssetLoadingOptions.IgnorePrefabPosition))
		{
			OnTavernBrawlLoadAttempted((string)m_TavernBrawlNoDeckPrefab, null, null);
		}
		if (TavernBrawlManager.Get().PlayerStatus == TavernBrawlStatus.TB_STATUS_TICKET_REQUIRED && !TavernBrawlManager.Get().IsEligibleForFreeTicket())
		{
			int ticketType = TavernBrawlManager.Get().CurrentMission().ticketType;
			if (GameDbf.TavernBrawlTicket.GetRecord(ticketType).TicketBehaviorType != TavernBrawlTicket.TicketBehaviorType.ARENA_TAVERN_TICKET)
			{
				m_pendingSessionBegin = true;
				Network.Get().RequestTavernBrawlSessionBegin();
			}
		}
		StartCoroutine(NotifySceneLoadedWhenReady());
	}

	private IEnumerator NotifySceneLoadedWhenReady()
	{
		while (m_tavernBrawlRoot == null)
		{
			yield return 0;
		}
		while (m_collectionManagerNeeded && (m_collectionManager == null || !CollectionManager.Get().GetCollectibleDisplay().IsReady()))
		{
			yield return 0;
		}
		while (m_pendingSessionBegin)
		{
			yield return 0;
		}
		TavernBrawlMission currentMission = TavernBrawlManager.Get().CurrentMission();
		CollectionDeck currentDeck = TavernBrawlManager.Get().CurrentDeck();
		CollectionManagerDisplay cmd = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (currentMission != null && currentMission.canCreateDeck && currentDeck != null && cmd != null)
		{
			cmd.ShowTavernBrawlDeck(currentDeck.ID);
		}
		StoreManager.Get().RegisterSuccessfulPurchaseAckListener(OnTavernBrawlTicketPurchaseAck);
		SceneMgr.Get().NotifySceneLoaded();
	}

	private void OnCollectionManagerLoadAttempted(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_collectionManager = go;
		if (go == null)
		{
			Debug.LogError($"TavernBrawlScene.OnCollectionManagerLoadAttempted() - failed to load screen {assetRef}");
		}
	}

	private void OnTavernBrawlLoadAttempted(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_tavernBrawlRoot = go;
		if (go == null)
		{
			Debug.LogError($"TavernBrawlScene.OnTavernBrawlLoadAttempted() - failed to load screen {assetRef}");
		}
		else
		{
			go.transform.position = Vector3.zero;
		}
	}

	private void OnSessionBeginResponse()
	{
		m_pendingSessionBegin = false;
	}

	private void OnTavernBrawlTicketPurchaseAck(ProductInfo bundle, PaymentMethod paymentMethod)
	{
		Log.TavernBrawl.Print("TavernBrawlScene.OnTavernBrawlTicketPurchaseAck");
		if (paymentMethod == PaymentMethod.GOLD_NO_GTAPP)
		{
			TavernBrawlManager.Get().RequestSessionBegin();
		}
		else
		{
			if (bundle == null || bundle.Items == null)
			{
				return;
			}
			foreach (Network.BundleItem item in bundle.Items)
			{
				if (item != null && (item.ItemType == ProductType.PRODUCT_TYPE_TAVERN_BRAWL_TICKET || item.ItemType == ProductType.PRODUCT_TYPE_DRAFT) && SceneMgr.Get().IsModeRequested(SceneMgr.Mode.TAVERN_BRAWL))
				{
					TavernBrawlManager.Get().RequestSessionBegin();
					return;
				}
			}
			Log.TavernBrawl.PrintError("TavernBrawlScene.OnTavernBrawlTicketPurchaseAck ERROR: Got a purchase ack in the Tavern Brawl scene for a product we don't recognize");
		}
	}
}
