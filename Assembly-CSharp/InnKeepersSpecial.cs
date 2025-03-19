using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone;
using Hearthstone.Http;
using MiniJSON;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

public class InnKeepersSpecial : MonoBehaviour
{
	public GameObject adImage;

	public GameObject adBackground;

	public PegUIElement adButton;

	public UberText adButtonText;

	public UberText adTitle;

	public UberText adSubtitle;

	public GameObject content;

	private Vector3 m_titleOrgPos;

	private Vector3 m_subtitleOrgPos;

	private List<InnKeepersSpecialAd> m_allAdsFromServer;

	private const char KEY_VALUE_PAIR_OPTIONS_SEPARATOR = ';';

	private const char HASH_COUNT_OPTIONS_SEPARATOR = ',';

	private const int DEFAULT_MAX_MESSAGE_VIEW_COUNT = 3;

	private string m_url;

	private AdventureDbId m_adventureDbId;

	private AdventureModeDbId m_adventureModeDbId;

	private GameSaveKeyId m_adventureClientGameSaveKey;

	private static InnKeepersSpecial s_instance;

	private bool m_loadedSuccessfully;

	private bool m_forceShowIks;

	private bool m_forceOnetime;

	private bool m_calledOnInit;

	private bool m_isShown;

	private bool m_wasInteractedWith;

	private bool m_adsDependOnAdventureGameSaveData;

	private bool m_adsDependOnTavernBrawlProgress;

	private bool m_adsDependOnRecruitProgress;

	private bool m_adsDependOnAccountLicenseInfo;

	private bool m_adsDependOnCollectionProgress;

	private bool m_adventureGameSaveDataReceived;

	private bool m_tavernBrawlInfoReceived;

	private bool m_tavernBrawlPlayerRecordReceived;

	private bool m_recruitProgressReceived;

	private bool m_accountLicenseInfoReceived;

	private bool m_collectionProgressReceived;

	private bool m_bnetButtonsLocked;

	private bool m_readyToDisplay;

	private List<Action> m_readyToDisplayListeners = new List<Action>();

	private List<Action> m_loadedSuccessfullyListeners = new List<Action>();

	private BaseIKSContentProvider m_contentHandler = new ContentStackIKSContentProvider();

	private Action m_OnClickCallback;

	public InnKeepersSpecialAd AdToDisplay
	{
		get
		{
			if (!m_allAdsFromServer.Any())
			{
				return new InnKeepersSpecialAd();
			}
			return m_allAdsFromServer[0];
		}
	}

	public bool IsShown => m_isShown;

	public bool ProcessingResponse { get; set; }

	public static InnKeepersSpecial Get()
	{
		Init();
		return s_instance;
	}

	public static void Init()
	{
		if (s_instance == null)
		{
			s_instance = AssetLoader.Get().InstantiatePrefab("InnKeepersSpecial.prefab:fe19b8065e74440e4bf42d73cbbf3662").GetComponent<InnKeepersSpecial>();
			OverlayUI.Get().AddGameObject(s_instance.gameObject);
			s_instance.m_forceShowIks = Options.Get().GetBool(Option.FORCE_SHOW_IKS);
			s_instance.m_titleOrgPos = s_instance.adTitle.transform.localPosition;
			s_instance.m_subtitleOrgPos = s_instance.adSubtitle.transform.localPosition;
		}
	}

	public bool LoadedSuccessfully()
	{
		return m_loadedSuccessfully;
	}

	public void InitializeURLAndUpdate()
	{
		Hide();
		MigrationIKSOptions();
		InitializeJsonURL(string.Empty);
		adButton.AddEventListener(UIEventType.RELEASE, Click);
		RegisterAllDependencyListeners();
		Update();
	}

	public void InitializeJsonURL(string customURL)
	{
		m_contentHandler.InitializeJsonURL(customURL);
	}

	public void ResetAdUrl()
	{
		m_forceOnetime = true;
	}

	private void Start()
	{
		Hide();
	}

	private static void MigrationIKSOptions()
	{
		Options.Get().DeleteOption(Option.IKS_LAST_DOWNLOAD_TIME);
		Options.Get().DeleteOption(Option.IKS_CACHE_AGE);
		Options.Get().DeleteOption(Option.IKS_LAST_DOWNLOAD_RESPONSE);
	}

	private void RegisterAllDependencyListeners()
	{
		Network net = Network.Get();
		if (net != null)
		{
			net.RegisterNetHandler(TavernBrawlInfo.PacketID.ID, TavernBrawlInfoReceivedCallback);
			net.RegisterNetHandler(TavernBrawlPlayerRecordResponse.PacketID.ID, TavernBrawlPlayerRecordReceivedCallback);
			net.RegisterNetHandler(RecruitAFriendDataResponse.PacketID.ID, RecruitProgressReceivedCallback);
			net.RegisterNetHandler(AccountLicensesInfoResponse.PacketID.ID, AccountLicensesInfoResponseReceivedCallback);
			CollectionManager.Get().RegisterOnInitialCollectionReceivedListener(CollectionProgressReceivedCallback);
		}
	}

	private void RemoveAllDependencyListeners()
	{
		Network net = Network.Get();
		if (net != null)
		{
			net.RemoveNetHandler(TavernBrawlInfo.PacketID.ID, TavernBrawlInfoReceivedCallback);
			net.RemoveNetHandler(TavernBrawlPlayerRecordResponse.PacketID.ID, TavernBrawlPlayerRecordReceivedCallback);
			net.RemoveNetHandler(RecruitAFriendDataResponse.PacketID.ID, RecruitProgressReceivedCallback);
			net.RemoveNetHandler(AccountLicensesInfoResponse.PacketID.ID, AccountLicensesInfoResponseReceivedCallback);
			CollectionManager.Get().RemoveOnInitialCollectionReceivedListener(CollectionProgressReceivedCallback);
		}
	}

	private void RequestDataForDependencies()
	{
		Network network = Network.Get();
		if (m_adsDependOnTavernBrawlProgress && !m_tavernBrawlInfoReceived)
		{
			network.RequestTavernBrawlInfo(BrawlType.BRAWL_TYPE_TAVERN_BRAWL);
		}
		if (m_adsDependOnTavernBrawlProgress && !m_tavernBrawlPlayerRecordReceived)
		{
			network.RequestTavernBrawlPlayerRecord(BrawlType.BRAWL_TYPE_TAVERN_BRAWL);
		}
		if (m_adsDependOnRecruitProgress && !m_recruitProgressReceived)
		{
			network.RequestRecruitAFriendData();
		}
		if (m_adsDependOnAccountLicenseInfo && !m_accountLicenseInfoReceived)
		{
			NetCache.Get().RefreshNetObject<NetCache.NetCacheAccountLicenses>();
		}
		if (m_adventureClientGameSaveKey != 0)
		{
			GameSaveDataManager.Get().Request(m_adventureClientGameSaveKey, AdventureGameSaveDataReceivedCallback);
		}
	}

	private void AdventureGameSaveDataReceivedCallback(bool success)
	{
		m_adventureGameSaveDataReceived = true;
		if (m_adsDependOnAdventureGameSaveData)
		{
			CheckReadyToDisplay();
		}
	}

	private void TavernBrawlInfoReceivedCallback()
	{
		m_tavernBrawlInfoReceived = true;
		Network.Get().RemoveNetHandler(TavernBrawlInfo.PacketID.ID, TavernBrawlInfoReceivedCallback);
		if (m_adsDependOnTavernBrawlProgress)
		{
			CheckReadyToDisplay();
		}
	}

	private void TavernBrawlPlayerRecordReceivedCallback()
	{
		m_tavernBrawlPlayerRecordReceived = true;
		Network.Get().RemoveNetHandler(TavernBrawlPlayerRecordResponse.PacketID.ID, TavernBrawlPlayerRecordReceivedCallback);
		if (m_adsDependOnTavernBrawlProgress)
		{
			CheckReadyToDisplay();
		}
	}

	private void RecruitProgressReceivedCallback()
	{
		m_recruitProgressReceived = true;
		Network.Get().RemoveNetHandler(RecruitAFriendDataResponse.PacketID.ID, RecruitProgressReceivedCallback);
		if (m_adsDependOnRecruitProgress)
		{
			CheckReadyToDisplay();
		}
	}

	private void AccountLicensesInfoResponseReceivedCallback()
	{
		m_accountLicenseInfoReceived = true;
		Network.Get().RemoveNetHandler(AccountLicensesInfoResponse.PacketID.ID, AccountLicensesInfoResponseReceivedCallback);
		if (m_adsDependOnAccountLicenseInfo)
		{
			CheckReadyToDisplay();
		}
	}

	private void CollectionProgressReceivedCallback()
	{
		m_collectionProgressReceived = true;
		CollectionManager.Get().RemoveOnInitialCollectionReceivedListener(CollectionProgressReceivedCallback);
		if (m_adsDependOnCollectionProgress)
		{
			CheckReadyToDisplay();
		}
	}

	private void CheckReadyToDisplay()
	{
		m_readyToDisplay = (!m_adsDependOnAdventureGameSaveData || m_adventureGameSaveDataReceived) && (!m_adsDependOnAccountLicenseInfo || m_accountLicenseInfoReceived) && (!m_adsDependOnRecruitProgress || m_recruitProgressReceived) && (!m_adsDependOnTavernBrawlProgress || (m_tavernBrawlInfoReceived && m_tavernBrawlPlayerRecordReceived)) && (!m_adsDependOnCollectionProgress || m_collectionProgressReceived);
		if (m_readyToDisplay)
		{
			Action[] array = m_readyToDisplayListeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i]();
			}
		}
	}

	public void RegisterReadyToDisplayCallback(Action callback)
	{
		if (!m_readyToDisplayListeners.Contains(callback))
		{
			m_readyToDisplayListeners.Add(callback);
		}
		if (m_readyToDisplay)
		{
			callback();
		}
	}

	public void RegisterLoadedSuccessfullyCallback(Action callback)
	{
		if (!m_loadedSuccessfullyListeners.Contains(callback))
		{
			m_loadedSuccessfullyListeners.Add(callback);
		}
		if (m_loadedSuccessfully)
		{
			callback();
		}
	}

	public static void RegisterClickCallback(Action callback)
	{
		if (!(s_instance == null))
		{
			InnKeepersSpecial innKeepersSpecial = s_instance;
			innKeepersSpecial.m_OnClickCallback = (Action)Delegate.Remove(innKeepersSpecial.m_OnClickCallback, callback);
			InnKeepersSpecial innKeepersSpecial2 = s_instance;
			innKeepersSpecial2.m_OnClickCallback = (Action)Delegate.Combine(innKeepersSpecial2.m_OnClickCallback, callback);
		}
	}

	public static void UnregisterClickCallback(Action callback)
	{
		if (!(s_instance == null))
		{
			InnKeepersSpecial innKeepersSpecial = s_instance;
			innKeepersSpecial.m_OnClickCallback = (Action)Delegate.Remove(innKeepersSpecial.m_OnClickCallback, callback);
		}
	}

	public static bool CheckShow()
	{
		if (s_instance == null)
		{
			return false;
		}
		if (!s_instance.LoadedSuccessfully())
		{
			Log.InnKeepersSpecial.Print("Skipping IKS! IKS Views not incremented. loadedSuccessfully={0}", Get().LoadedSuccessfully());
			return false;
		}
		int countOfWelcomeQuestViews = Options.Get().GetInt(Option.IKS_VIEW_ATTEMPTS, 0);
		countOfWelcomeQuestViews++;
		Options.Get().SetInt(Option.IKS_VIEW_ATTEMPTS, countOfWelcomeQuestViews);
		bool hasViewedWelcomeQuestsEnough = countOfWelcomeQuestViews > 3;
		int lastShownIksViews = 0;
		bool forceShowIks = Options.Get().GetBool(Option.FORCE_SHOW_IKS);
		if (ReturningPlayerMgr.Get().SuppressOldPopups)
		{
			Log.InnKeepersSpecial.Print("Skipping IKS! ReturningPlayerMgr.Get().SuppressOldPopups={1}!", ReturningPlayerMgr.Get().SuppressOldPopups);
			return false;
		}
		if (!(hasViewedWelcomeQuestsEnough || forceShowIks))
		{
			Log.InnKeepersSpecial.Print("Skipping IKS! views={0} lastShownViews={1}", countOfWelcomeQuestViews, lastShownIksViews);
			return false;
		}
		Log.InnKeepersSpecial.Print("Showing IKS!");
		s_instance.LockBnetButtons();
		s_instance.ShowAdAndIncrementViewCountWhenReady();
		return true;
	}

	public void ShowAdAndIncrementViewCountWhenReady()
	{
		if (m_allAdsFromServer == null || !m_allAdsFromServer.Any())
		{
			Hide();
			return;
		}
		RegisterReadyToDisplayCallback(delegate
		{
			if (m_allAdsFromServer.Any())
			{
				RegisterLoadedSuccessfullyCallback(delegate
				{
					IncremenetViewCountOfDisplayedAdInStorage();
					Show();
				});
			}
		});
	}

	public void Show()
	{
		float fadeTime = 0.5f;
		content.SetActive(value: true);
		Material material = adImage.gameObject.GetComponent<Renderer>().GetMaterial();
		Color c = material.color;
		c.a = 0f;
		material.color = c;
		Hashtable args = iTweenManager.Get().GetTweenHashTable();
		args.Add("amount", 1f);
		args.Add("time", fadeTime);
		args.Add("easetype", iTween.EaseType.linear);
		iTween.FadeTo(adImage.gameObject, args);
		adTitle.Show();
		Hashtable titleArgs = iTweenManager.Get().GetTweenHashTable();
		titleArgs.Add("from", 0f);
		titleArgs.Add("to", 1f);
		titleArgs.Add("time", fadeTime);
		titleArgs.Add("easetype", iTween.EaseType.linear);
		titleArgs.Add("onupdate", (Action<object>)delegate(object newVal)
		{
			adTitle.TextAlpha = (float)newVal;
		});
		iTween.ValueTo(adTitle.gameObject, titleArgs);
		adSubtitle.Show();
		Hashtable subtitleArgs = iTweenManager.Get().GetTweenHashTable();
		subtitleArgs.Add("from", 0f);
		subtitleArgs.Add("to", 1f);
		subtitleArgs.Add("time", fadeTime);
		subtitleArgs.Add("easetype", iTween.EaseType.linear);
		subtitleArgs.Add("onupdate", (Action<object>)delegate(object newVal)
		{
			adSubtitle.TextAlpha = (float)newVal;
		});
		iTween.ValueTo(adSubtitle.gameObject, subtitleArgs);
		m_isShown = true;
		m_wasInteractedWith = false;
	}

	public void Hide()
	{
		content.SetActive(value: false);
		adTitle.Hide();
		adSubtitle.Hide();
		m_isShown = false;
	}

	public static void Close()
	{
		if (s_instance != null)
		{
			s_instance.CloseInternal();
		}
	}

	private void CloseInternal()
	{
		if (m_isShown && !m_wasInteractedWith)
		{
			TelemetryManager.Client().SendIKSIgnored(AdToDisplay.CampaignName, AdToDisplay.ImageUrl);
		}
		Hide();
		UnlockBnetButtons();
		RemoveAllDependencyListeners();
		m_readyToDisplayListeners.Clear();
		m_loadedSuccessfullyListeners.Clear();
		UnityEngine.Object.Destroy(base.gameObject);
		s_instance = null;
	}

	private void Click(UIEvent e)
	{
		Log.InnKeepersSpecial.Print("IKS on release! Link: " + AdToDisplay.Link + " Game Action: " + AdToDisplay.GameAction);
		m_wasInteractedWith = true;
		TelemetryManager.Client().SendIKSClicked(AdToDisplay.CampaignName, AdToDisplay.ImageUrl);
		SetAdViewCountInStorage(AdToDisplay.GetHash(), AdToDisplay.MaxViewCount + 1);
		if (!string.IsNullOrEmpty(AdToDisplay.GameAction))
		{
			DeepLinkManager.ExecuteDeepLink(AdToDisplay.GameAction.Split(' '), DeepLinkManager.DeepLinkSource.INNKEEPERS_SPECIAL, 0);
			WelcomeQuests.OnNavigateBack();
			Hide();
		}
		else if (!string.IsNullOrEmpty(AdToDisplay.Link))
		{
			if (PlatformSettings.IsMobile())
			{
				AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
				info.m_showAlertIcon = false;
				info.m_headerText = GameStrings.Format("GLUE_INNKEEPERS_SPECIAL_CONFIRM_POPUP_HEADER");
				info.m_text = GameStrings.Get("GLUE_INNKEEPERS_SPECIAL_CONFIRM_POPUP_MESSAGE");
				info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
				info.m_disableBnetBar = true;
				AlertPopup.ResponseCallback callback = delegate(AlertPopup.Response response, object userdata)
				{
					if (response == AlertPopup.Response.CONFIRM)
					{
						Application.OpenURL(AdToDisplay.Link);
					}
				};
				info.m_responseCallback = callback;
				DialogManager.Get().ShowPopup(info);
			}
			else
			{
				Application.OpenURL(AdToDisplay.Link);
			}
		}
		else
		{
			Debug.LogWarning("InnKeepersSpecial Ad has no Game Action and Link is null or empty.");
		}
		m_OnClickCallback?.Invoke();
	}

	private void UpdateAdJson(string jsonResponse, object param)
	{
		if (!string.IsNullOrEmpty(jsonResponse))
		{
			JsonNode jsonNode;
			try
			{
				jsonNode = Json.Deserialize(jsonResponse) as JsonNode;
			}
			catch (Exception ex)
			{
				jsonNode = null;
				Log.ContentConnect.PrintWarning("Aborting because of an invalid json response:\n{0}", jsonResponse);
				Debug.LogError(ex.StackTrace);
			}
			m_allAdsFromServer = GetAllAdsFromJsonResponse(jsonNode);
			if (m_allAdsFromServer.Any())
			{
				CheckAdDependenciesAndRequestData(AdToDisplay.GameAction);
				RegisterReadyToDisplayCallback(VerifyAdToDisplayBasedOnResponses);
			}
		}
		ProcessingResponse = false;
	}

	private JsonList GetRootListNode(JsonNode response)
	{
		return m_contentHandler.GetRootListNode(response);
	}

	private List<InnKeepersSpecialAd> GetAllAdsFromJsonResponse(JsonNode response)
	{
		if (response == null)
		{
			return new List<InnKeepersSpecialAd>();
		}
		List<InnKeepersSpecialAd> innKeepersSpecialAds = new List<InnKeepersSpecialAd>();
		try
		{
			JsonList responseAds = GetRootListNode(response);
			if (responseAds == null)
			{
				return new List<InnKeepersSpecialAd>();
			}
			if (m_contentHandler == null)
			{
				return new List<InnKeepersSpecialAd>();
			}
			Dictionary<string, int> oldViewCountMap = GetViewCountOfAdsFromStorage();
			Dictionary<string, int> newViewCountMap = new Dictionary<string, int>();
			foreach (object item in responseAds)
			{
				JsonNode adNode = item as JsonNode;
				InnKeepersSpecialAd innKeepersSpecialAd = m_contentHandler.ReadInnKeepersSpecialAd(adNode);
				string adHash = innKeepersSpecialAd.GetHash();
				int adViewCount = (innKeepersSpecialAd.CurrentViewCount = (newViewCountMap[adHash] = (oldViewCountMap.TryGetValue(adHash, out adViewCount) ? adViewCount : 0)));
				if (!m_forceShowIks && adViewCount >= innKeepersSpecialAd.MaxViewCount)
				{
					continue;
				}
				if (!string.IsNullOrEmpty(innKeepersSpecialAd.ClientVersion) && !m_forceShowIks && !StringUtils.CompareIgnoreCase(innKeepersSpecialAd.ClientVersion, "31.6"))
				{
					Log.InnKeepersSpecial.Print("Skipping IKS: {0}, mis-matched client version {0} != {1}", innKeepersSpecialAd.CampaignName, innKeepersSpecialAd.ClientVersion, "31.6");
					continue;
				}
				if (!string.IsNullOrEmpty(innKeepersSpecialAd.Platform))
				{
					string[] array = innKeepersSpecialAd.Platform.Trim().Split(',');
					bool supported = false;
					string[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						if (StringUtils.CompareIgnoreCase(array2[i].Trim(), PlatformSettings.OS.ToString()))
						{
							supported = true;
						}
					}
					if (!m_forceShowIks && !supported)
					{
						Log.InnKeepersSpecial.Print("Skipping IKS: {0}, supported on: {1}; current platform is {2}", innKeepersSpecialAd.CampaignName, innKeepersSpecialAd.Platform, PlatformSettings.OS.ToString());
						continue;
					}
				}
				if (!string.IsNullOrEmpty(innKeepersSpecialAd.AndroidStore))
				{
					string[] array3 = innKeepersSpecialAd.AndroidStore.Trim().Split(',');
					bool supported2 = false;
					string androidStoreString = AndroidDeviceSettings.Get().GetAndroidStore().ToString();
					string[] array2 = array3;
					for (int i = 0; i < array2.Length; i++)
					{
						if (StringUtils.CompareIgnoreCase(array2[i].Trim(), androidStoreString))
						{
							supported2 = true;
						}
					}
					if (!m_forceShowIks && !supported2)
					{
						Log.InnKeepersSpecial.Print("Skipping IKS: {0}, supported on: {1}; current android store is {2}", innKeepersSpecialAd.CampaignName, innKeepersSpecialAd.AndroidStore, androidStoreString);
						continue;
					}
				}
				if (!m_forceShowIks && HearthstoneApplication.IsPublic() && !innKeepersSpecialAd.Visibility)
				{
					Log.InnKeepersSpecial.Print("Skipping IKS: {0}, not flagged as publicly visible", (string)adNode["campaignName"]);
				}
				else
				{
					innKeepersSpecialAds.Add(innKeepersSpecialAd);
				}
			}
			WriteViewCountOfAdsToStorage(newViewCountMap);
			innKeepersSpecialAds.Sort(InnKeepersSpecialAd.ComparisonDescending);
			return innKeepersSpecialAds;
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to get correct advertisement: " + ex);
			return new List<InnKeepersSpecialAd>();
		}
	}

	private void VerifyAdToDisplayBasedOnResponses()
	{
		if (!(this == null) && m_allAdsFromServer.Any())
		{
			if (!m_forceShowIks && HasInteractedWithAdvertisedProduct(AdToDisplay.GameAction))
			{
				Log.InnKeepersSpecial.Print("Player has interacted with the advertised product. Skipping ad: " + AdToDisplay.GameAction);
				DiscardCurrentAdAndRequestNextAdData();
			}
			else
			{
				Log.InnKeepersSpecial.Print("Ad to display :" + AdToDisplay.Link);
				StartCoroutine(UpdateAdTexture());
			}
		}
	}

	private void DiscardCurrentAdAndRequestNextAdData()
	{
		if (m_allAdsFromServer.Any())
		{
			m_allAdsFromServer.RemoveAt(0);
			if (m_allAdsFromServer.Any())
			{
				CheckAdDependenciesAndRequestData(AdToDisplay.GameAction);
			}
		}
	}

	private void Update()
	{
		if ((!m_calledOnInit || m_forceOnetime) && m_contentHandler.Ready)
		{
			Hide();
			ProcessingResponse = true;
			StartCoroutine(m_contentHandler.GetQuery(UpdateAdJson, null, m_forceOnetime));
			m_forceOnetime = false;
			m_calledOnInit = true;
		}
	}

	private IEnumerator UpdateAdTexture()
	{
		if (!string.IsNullOrEmpty(AdToDisplay.Title))
		{
			adTitle.Text = AdToDisplay.Title.Replace("\\n", "\n");
		}
		if (!string.IsNullOrEmpty(AdToDisplay.SubTitle))
		{
			adSubtitle.Text = AdToDisplay.SubTitle.Replace("\\n", "\n");
		}
		string imageUrl = AdToDisplay.ImageUrl;
		if (!string.IsNullOrEmpty(AdToDisplay.ImageUrl) && AdToDisplay.ImageUrl.StartsWith("//"))
		{
			imageUrl = "http:" + AdToDisplay.ImageUrl;
		}
		Log.InnKeepersSpecial.Print("image url is " + imageUrl);
		IHttpRequest textureHttpRequest = HttpRequestFactory.Get().CreateGetTextureRequest(imageUrl);
		yield return textureHttpRequest.SendRequest();
		if (textureHttpRequest.IsNetworkError || textureHttpRequest.IsHttpError)
		{
			Debug.LogError("Failed to download image for Innkeeper's Special: " + imageUrl);
			Debug.LogError(textureHttpRequest.ErrorString);
			DiscardCurrentAdAndRequestNextAdData();
			yield break;
		}
		Texture adTexture = textureHttpRequest.ResponseAsTexture;
		if (adTexture.width == 8 && adTexture.height == 8)
		{
			Debug.LogError("Failed to download image for Innkeeper's Special (got 8x8 dummy image): " + imageUrl);
			DiscardCurrentAdAndRequestNextAdData();
			yield break;
		}
		Material material = adImage.GetComponent<Renderer>().GetMaterial();
		material.mainTexture = adTexture;
		material.mainTexture.wrapMode = TextureWrapMode.Clamp;
		UpdateText();
		m_loadedSuccessfully = true;
		Action[] array = m_loadedSuccessfullyListeners.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i]();
		}
	}

	private void UpdateText()
	{
		if (!string.IsNullOrEmpty(AdToDisplay.ButtonText))
		{
			adButtonText.GameStringLookup = false;
			adButtonText.Text = AdToDisplay.ButtonText;
		}
		Vector3 titlePos = m_titleOrgPos;
		titlePos.x += AdToDisplay.TitleOffsetX;
		titlePos.y += AdToDisplay.TitleOffsetY;
		adTitle.transform.localPosition = titlePos;
		Vector3 subtitlePos = m_subtitleOrgPos;
		subtitlePos.x += AdToDisplay.SubTitleOffsetX;
		subtitlePos.y += AdToDisplay.SubTitleOffsetY;
		adSubtitle.transform.localPosition = subtitlePos;
		adTitle.FontSize = AdToDisplay.TitleFontSize;
		adSubtitle.FontSize = AdToDisplay.SubTitleFontSize;
	}

	public bool HasInteractedWithAdvertisedProduct(string gameAction)
	{
		if (string.IsNullOrEmpty(gameAction))
		{
			Log.InnKeepersSpecial.Print("IKS unable to check interaction for product with null gameAction.");
			return false;
		}
		string[] actionTokens = gameAction.Split(' ');
		if (actionTokens[0].Equals("store", StringComparison.OrdinalIgnoreCase))
		{
			if (actionTokens.Length > 1)
			{
				string str = actionTokens[1];
				AdventureDbId adventureDbId = EnumUtils.SafeParse(str, AdventureDbId.INVALID, ignoreCase: true);
				HeroDbId heroDbId = EnumUtils.SafeParse(str, HeroDbId.INVALID, ignoreCase: true);
				DeepLinkManager.GetBoosterAndStorePackTypeFromGameAction(actionTokens, out var boosterId, out var storePackType);
				if (boosterId != 0)
				{
					if (storePackType == StorePackType.BOOSTER && boosterId == 181)
					{
						return StoreManager.IsFirstPurchaseBundleOwned();
					}
				}
				else
				{
					if (adventureDbId != 0)
					{
						if (m_adventureClientGameSaveKey != 0)
						{
							GameSaveDataManager.Get().GetSubkeyValue(m_adventureClientGameSaveKey, GameSaveKeySubkeyId.ADVENTURE_HAS_SEEN_ADVENTURE, out long hasSeenAdventure);
							return hasSeenAdventure == 1;
						}
						return false;
					}
					if (heroDbId != 0)
					{
						string heroDesignerId = GameUtils.GetCardIdFromHeroDbId((int)heroDbId);
						return CollectionManager.Get().IsCardInCollection(heroDesignerId, TAG_PREMIUM.NORMAL);
					}
				}
				return false;
			}
			return false;
		}
		if (actionTokens[0].Equals("recruitafriend", StringComparison.OrdinalIgnoreCase))
		{
			return RAFManager.Get().GetTotalRecruitCount() != 0;
		}
		if (actionTokens[0].Equals("tavernbrawl", StringComparison.OrdinalIgnoreCase))
		{
			if (TavernBrawlManager.Get().GamesPlayed <= 0)
			{
				return !TavernBrawlManager.Get().IsTavernBrawlActive(BrawlType.BRAWL_TYPE_TAVERN_BRAWL);
			}
			return true;
		}
		if (actionTokens[0].Equals("adventure", StringComparison.OrdinalIgnoreCase))
		{
			if (actionTokens.Length > 1)
			{
				AdventureDbId adventureId = EnumUtils.SafeParse(actionTokens[1], AdventureDbId.INVALID, ignoreCase: true);
				if (adventureId != 0)
				{
					return AdventureProgressMgr.Get().OwnsOneOrMoreAdventureWings(adventureId);
				}
				return false;
			}
			return false;
		}
		Log.InnKeepersSpecial.Print("IKS unrecognized game action: " + gameAction + " Unable to determine if the player has interacted with it previously. ");
		return false;
	}

	private void CheckAdDependenciesAndRequestData(string gameAction)
	{
		if (string.IsNullOrEmpty(gameAction))
		{
			CheckReadyToDisplay();
			return;
		}
		string[] actionTokens = gameAction.Split(' ');
		if (actionTokens[0].Equals("store", StringComparison.OrdinalIgnoreCase))
		{
			if (actionTokens.Length > 1)
			{
				string str = actionTokens[1];
				AdventureDbId adventureDbId = EnumUtils.SafeParse(str, AdventureDbId.INVALID, ignoreCase: true);
				HeroDbId heroDbId = EnumUtils.SafeParse(str, HeroDbId.INVALID, ignoreCase: true);
				DeepLinkManager.GetBoosterAndStorePackTypeFromGameAction(actionTokens, out var boosterId, out var storePackType);
				if (boosterId != 0)
				{
					if (storePackType == StorePackType.BOOSTER && boosterId == 181)
					{
						m_adsDependOnAccountLicenseInfo = true;
					}
				}
				else if (adventureDbId != 0)
				{
					m_adsDependOnAdventureGameSaveData = true;
				}
				else if (heroDbId != 0)
				{
					m_adsDependOnCollectionProgress = true;
				}
			}
		}
		else if (actionTokens[0].Equals("recruitafriend", StringComparison.OrdinalIgnoreCase))
		{
			m_adsDependOnRecruitProgress = true;
		}
		else if (actionTokens[0].Equals("tavernbrawl", StringComparison.OrdinalIgnoreCase))
		{
			m_adsDependOnTavernBrawlProgress = true;
		}
		else if (actionTokens[0].Equals("adventure", StringComparison.OrdinalIgnoreCase))
		{
			m_adsDependOnAdventureGameSaveData = true;
			if (actionTokens.Length > 1)
			{
				string adventureAction = actionTokens[1];
				m_adventureDbId = EnumUtils.SafeParse(adventureAction, AdventureDbId.INVALID, ignoreCase: true);
				AdventureDataDbfRecord adventureDataRecord = GameDbf.AdventureData.GetRecord((AdventureDataDbfRecord r) => r.AdventureId == (int)m_adventureDbId);
				if (adventureDataRecord != null)
				{
					m_adventureClientGameSaveKey = (GameSaveKeyId)adventureDataRecord.GameSaveDataClientKey;
				}
			}
		}
		RequestDataForDependencies();
		CheckReadyToDisplay();
	}

	public void IncremenetViewCountOfDisplayedAdInStorage()
	{
		if (m_allAdsFromServer.Any())
		{
			SetAdViewCountInStorage(AdToDisplay.GetHash(), ++AdToDisplay.CurrentViewCount);
		}
	}

	private void SetAdViewCountInStorage(string adHash, int count)
	{
		if (!string.IsNullOrEmpty(adHash))
		{
			Dictionary<string, int> viewCountMap = GetViewCountOfAdsFromStorage();
			viewCountMap[adHash] = count;
			WriteViewCountOfAdsToStorage(viewCountMap);
		}
	}

	private Dictionary<string, int> GetViewCountOfAdsFromStorage()
	{
		Dictionary<string, int> viewCountMap = new Dictionary<string, int>();
		string viewCountStorageString = Options.Get().GetString(Option.IKS_LAST_SHOWN_AD);
		if (string.IsNullOrEmpty(viewCountStorageString))
		{
			return viewCountMap;
		}
		string[] array = viewCountStorageString.Split(';');
		for (int i = 0; i < array.Length; i++)
		{
			string[] keyValueSections = array[i].Split(',');
			if (keyValueSections.Length == 2)
			{
				string iksHash = keyValueSections[0];
				int iksCount = (int.TryParse(keyValueSections[1], out iksCount) ? iksCount : 0);
				if (!viewCountMap.ContainsKey(iksHash))
				{
					viewCountMap.Add(iksHash, iksCount);
				}
			}
		}
		return viewCountMap;
	}

	private void WriteViewCountOfAdsToStorage(Dictionary<string, int> values)
	{
		string result = string.Join(';'.ToString(), values.Select((KeyValuePair<string, int> kvp) => kvp.Key + "," + kvp.Value).ToArray());
		if (!string.IsNullOrEmpty(result))
		{
			Options.Get().SetString(Option.IKS_LAST_SHOWN_AD, result);
		}
		else
		{
			Options.Get().DeleteOption(Option.IKS_LAST_SHOWN_AD);
		}
	}

	private void LockBnetButtons()
	{
		if (!(BaseUI.Get() == null) && !m_bnetButtonsLocked)
		{
			BaseUI.Get().m_BnetBar.RequestDisableButtons();
			m_bnetButtonsLocked = true;
		}
	}

	private void UnlockBnetButtons()
	{
		if (!(BaseUI.Get() == null) && m_bnetButtonsLocked)
		{
			BaseUI.Get().m_BnetBar.CancelRequestToDisableButtons();
			m_bnetButtonsLocked = false;
		}
	}
}
