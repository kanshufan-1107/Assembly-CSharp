using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Blizzard.Commerce;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Content.Delivery;
using Hearthstone.Core;
using Hearthstone.InGameMessage;
using Hearthstone.Streaming;
using Hearthstone.Util;
using MiniJSON;
using UnityEngine;
using UnityEngine.Networking;

namespace Hearthstone.MarketingImages;

public class MarketingImagesService : IService
{
	public class MarketingImageConfigSet
	{
		public MarketingImageConfig AnySize;

		public MarketingImageConfig FullRow;

		public MarketingImageConfig HalfRow;

		public IEnumerable<MarketingImageConfig> GetValidConfigs()
		{
			if (AnySize != null)
			{
				yield return AnySize;
			}
			if (FullRow != null)
			{
				yield return FullRow;
			}
			if (HalfRow != null)
			{
				yield return HalfRow;
			}
		}
	}

	private const string LOG_TAG = "[MarketingImagesService]";

	private const string CONTENT_STACK_CONTENT_TYPE = "marketing_image";

	private const int CONTENT_STACK_NO_AUTO_UPDATE_INTERVAL = 0;

	private const int MAX_NUM_CONCURRENT_OPS = 5;

	private static readonly Type[] s_serviceDependencies = new Type[4]
	{
		typeof(NetCache),
		typeof(GameDownloadManager),
		typeof(AssetLoader),
		typeof(SceneMgr)
	};

	private const int LOADING_TIMEOUT = 3;

	private ContentStackConnect m_contentConnect;

	private Coroutine m_queryCoroutine;

	private MarketingImagesContainer m_embeddedImagesInfo;

	private GameDownloadManager m_gameDownloadMgr;

	private Dictionary<long, MarketingImageConfigSet> m_imagesPerProductId = new Dictionary<long, MarketingImageConfigSet>();

	private bool m_initialized;

	private bool m_isDownloadingJobRunning;

	private bool m_assetsDownloaded;

	private bool m_isContentStackEnabled;

	private string m_mimgStorageDir;

	private string m_mimgJsonFile;

	public MarketingImagesService()
	{
		m_mimgStorageDir = Path.Combine(PlatformFilePaths.CachePath, "MarketingImages");
		m_mimgJsonFile = Path.Combine(m_mimgStorageDir, "mimg.json");
		try
		{
			Directory.CreateDirectory(m_mimgStorageDir);
		}
		catch (Exception ex)
		{
			Log.Services.PrintError("[MarketingImagesService] Failed to create directory '" + m_mimgStorageDir + "': " + ex.Message);
		}
	}

	IEnumerator<IAsyncJobResult> IService.Initialize(ServiceLocator serviceLocator)
	{
		LoadResource mimgLoader = new LoadResource("ServiceData/MarketingImagesContainer");
		yield return mimgLoader;
		MarketingImagesContainer embeddedImagesInfo = mimgLoader.LoadedAsset as MarketingImagesContainer;
		if ((bool)embeddedImagesInfo)
		{
			m_embeddedImagesInfo = embeddedImagesInfo;
		}
		else
		{
			Log.Services.PrintWarning("[MarketingImagesService] Failed to load ServiceData/MarketingImagesContainer");
			m_embeddedImagesInfo = ScriptableObject.CreateInstance<MarketingImagesContainer>();
		}
		foreach (MarketingImageConfig cfg in m_embeddedImagesInfo.Images)
		{
			if (cfg == null)
			{
				continue;
			}
			if (cfg.ProductId <= 0)
			{
				Log.Services.PrintWarning(string.Format("{0} Ignoring config for invalid product ID: {1}", "[MarketingImagesService]", cfg.ProductId));
				continue;
			}
			if (string.IsNullOrEmpty(cfg.TextureAsset))
			{
				Log.Services.PrintWarning(string.Format("{0} Ignoring config with no texture: {1}", "[MarketingImagesService]", cfg.ProductId));
				continue;
			}
			if (!m_imagesPerProductId.TryGetValue(cfg.ProductId, out var imgCfgSet))
			{
				imgCfgSet = new MarketingImageConfigSet();
				m_imagesPerProductId.Add(cfg.ProductId, imgCfgSet);
			}
			MarketingImageConfig prevConfg;
			switch (cfg.SlotCompatibility)
			{
			case MarketingImageSlot.Any:
				prevConfg = imgCfgSet.AnySize;
				imgCfgSet.AnySize = cfg;
				break;
			case MarketingImageSlot.FullRow:
				prevConfg = imgCfgSet.FullRow;
				imgCfgSet.FullRow = cfg;
				break;
			case MarketingImageSlot.HalfRow:
				prevConfg = imgCfgSet.HalfRow;
				imgCfgSet.HalfRow = cfg;
				break;
			default:
				Log.Services.PrintWarning(string.Format("{0} Ignoring image with invalid slot compatibility: {1}", "[MarketingImagesService]", cfg.SlotCompatibility));
				continue;
			}
			if (prevConfg != null)
			{
				Log.Services.PrintWarning(string.Format("{0} More than one marketing image set for product: {1}, slot: {2}", "[MarketingImagesService]", cfg.ProductId, cfg.SlotCompatibility));
			}
		}
		if (!ServiceManager.TryGet<GameDownloadManager>(out m_gameDownloadMgr))
		{
			yield return new JobFailedResult("[MarketingImagesService] failed to initialize as GameDownloadManager couldn't be found!");
		}
		if (!ServiceManager.TryGet<NetCache>(out var netCache))
		{
			yield return new JobFailedResult("[MarketingImagesService] failed to initialize as NetCache couldn't be found!");
		}
		netCache.RegisterUpdatedListener(typeof(NetCache.NetCacheFeatures), HandleNetCacheFeaturesChanged);
		HandleNetCacheFeaturesChanged();
		Processor.QueueJob("Register_Store_change", RegisterStoreManagerEventsWhenReady());
		m_initialized = true;
	}

	Type[] IService.GetDependencies()
	{
		return s_serviceDependencies;
	}

	void IService.Shutdown()
	{
		if (ServiceManager.TryGet<NetCache>(out var netCache))
		{
			netCache.RemoveUpdatedListener(typeof(NetCache.NetCacheFeatures), HandleNetCacheFeaturesChanged);
		}
		if (StoreManager.Get() != null)
		{
			StoreManager.Get().RemoveStoreHiddenListener(OnStoreHidden);
		}
		UnloadImages();
	}

	private IEnumerator<IAsyncJobResult> RegisterStoreManagerEventsWhenReady()
	{
		while (StoreManager.Get() == null)
		{
			yield return null;
		}
		StoreManager.Get().RegisterStoreHiddenListener(OnStoreHidden);
	}

	private void OnStoreHidden()
	{
		UnloadImages();
	}

	private void UnloadImages()
	{
		foreach (MarketingImageConfigSet value in m_imagesPerProductId.Values)
		{
			foreach (MarketingImageConfig validConfig in value.GetValidConfigs())
			{
				validConfig.DestroyCachedTexture();
			}
		}
	}

	public bool TryGetConfig(long productId, MarketingImageSlot slotSize, out MarketingImageConfig imgConfig)
	{
		imgConfig = null;
		if (m_initialized && m_imagesPerProductId.TryGetValue(productId, out var imgSet))
		{
			switch (slotSize)
			{
			case MarketingImageSlot.Any:
				imgConfig = imgSet.AnySize ?? imgSet.FullRow ?? imgSet.HalfRow;
				break;
			case MarketingImageSlot.FullRow:
				imgConfig = imgSet.FullRow ?? imgSet.AnySize;
				break;
			case MarketingImageSlot.HalfRow:
				imgConfig = imgSet.HalfRow ?? imgSet.AnySize;
				break;
			}
		}
		return imgConfig != null;
	}

	private void HandleNetCacheFeaturesChanged()
	{
		if (ServiceManager.TryGet<NetCache>(out var netCache))
		{
			NetCache.NetCacheFeatures featuresCache = netCache.GetNetObject<NetCache.NetCacheFeatures>();
			if (featuresCache != null)
			{
				ProcessFeatureStateChange(featuresCache);
			}
		}
	}

	private void ProcessFeatureStateChange(NetCache.NetCacheFeatures features)
	{
		if (features == null || !features.ContentstackEnabled)
		{
			m_isContentStackEnabled = false;
			UnloadImages();
		}
		else if (features.ContentstackEnabled != m_isContentStackEnabled)
		{
			m_isContentStackEnabled = features.ContentstackEnabled;
			if (m_contentConnect == null)
			{
				m_contentConnect = new ContentStackConnect();
				m_contentConnect.InitializeURL("marketing_image", Vars.Key("ContentStack.Env").GetStr("production"), Localization.GetBnetLocaleName(), BattleNet.GetCurrentRegion() == BnetRegion.REGION_CN, m_mimgJsonFile, Vars.Key("ContentStack.CacheAge").GetInt(0));
			}
			if (m_queryCoroutine != null)
			{
				Processor.CancelCoroutine(m_queryCoroutine);
			}
			m_queryCoroutine = Processor.RunCoroutine(m_contentConnect.Query(OnImageQueryComplete, null, BuildQueryString(), force: false));
		}
	}

	private static string BuildQueryString()
	{
		return InGameMessageUtils.QueryAnd(InGameMessageUtils.QueryNullOrLessEqual(InGameMessageAttributes.MIN_GAME_VERSION, InGameMessageUtils.ClientVersion), InGameMessageUtils.QueryNullOrGreaterEqual(InGameMessageAttributes.MAX_GAME_VERSION, InGameMessageUtils.ClientVersion));
	}

	private void OnImageQueryComplete(string json, object param)
	{
		m_queryCoroutine = null;
		List<MarketingImageContent> marketingImages = ParseMarketingImageContent(json);
		if (marketingImages != null && marketingImages.Count != 0 && !m_isDownloadingJobRunning)
		{
			if (!Processor.QueueJob(new JobDefinition("MarketingImagesService:DownloadTextures", DownloadTextures(marketingImages))))
			{
				Log.Services.PrintError("[MarketingImagesService] Failed to start DownloadTextures job");
			}
			m_isDownloadingJobRunning = true;
		}
	}

	private static List<MarketingImageContent> ParseMarketingImageContent(string json)
	{
		JsonNode jsonNode = null;
		try
		{
			jsonNode = Json.Deserialize(json) as JsonNode;
		}
		catch (Exception ex)
		{
			Log.Services.PrintWarning("[MarketingImagesService] Failed to parse marketing image json because of an invalid json response:\n" + json + " due exception: " + ex.Message);
			return null;
		}
		if (jsonNode == null)
		{
			return null;
		}
		List<MarketingImageContent> marketingImages = null;
		try
		{
			JsonList responseImages = InGameMessageUtils.GetRootListNode(jsonNode);
			if (responseImages == null)
			{
				return null;
			}
			marketingImages = new List<MarketingImageContent>();
			DateTime now = DateTime.UtcNow;
			foreach (object item in responseImages)
			{
				MarketingImageContent marketingImage = TryReadMarketingImageFromJson(item as JsonNode, now);
				if (marketingImage != null)
				{
					marketingImages.Add(marketingImage);
				}
			}
			return marketingImages;
		}
		catch (Exception ex2)
		{
			Log.Services.PrintError("[MarketingImagesService] Failed to deserialize marketing image json to MarketingImageContent due exception: " + ex2.Message);
			return null;
		}
	}

	private static MarketingImageContent TryReadMarketingImageFromJson(JsonNode node, DateTime now)
	{
		if (node == null)
		{
			return null;
		}
		IReadOnlyDictionary<InGameMessageAttributes, InGameMessageUtils.InGameMessageDef> attr = InGameMessageUtils.Attributes;
		if (attr == null)
		{
			return null;
		}
		string[] tags = InGameMessageUtils.GetAttribute<string[]>(node, attr[InGameMessageAttributes.TAGS]);
		DateTime beginningDate = InGameMessageUtils.GetAttribute<DateTime>(node, attr[InGameMessageAttributes.BEGINNING_DATE]);
		DateTime expiryDate = InGameMessageUtils.GetAttribute<DateTime>(node, attr[InGameMessageAttributes.EXPIRY_DATE]);
		string textureAssetUrl = InGameMessageUtils.GetAttribute<string>(node, attr[InGameMessageAttributes.TEXTURE_ASSET_URL]);
		string text = InGameMessageUtils.GetAttribute<string>(node, attr[InGameMessageAttributes.SLOT_COMPATIBILITY]);
		MarketingImageSlot slot = MarketingImageSlot.Any;
		if (text == null)
		{
			text = string.Empty;
		}
		if (!Enum.TryParse<MarketingImageSlot>(text, ignoreCase: true, out slot))
		{
			slot = MarketingImageSlot.Any;
		}
		if (beginningDate != default(DateTime) && now < beginningDate)
		{
			return null;
		}
		if (expiryDate != default(DateTime) && now > expiryDate)
		{
			return null;
		}
		if (string.IsNullOrWhiteSpace(textureAssetUrl))
		{
			Log.Services.PrintWarning("[MarketingImagesService] Ignoring marketing image without texture urls");
			return null;
		}
		MarketingImageContent marketingImageContent = new MarketingImageContent();
		marketingImageContent.UID = InGameMessageUtils.GetAttribute<string>(node, attr[InGameMessageAttributes.UID]);
		marketingImageContent.EntryTitle = InGameMessageUtils.GetAttribute<string>(node, attr[InGameMessageAttributes.ENTRYTITLE]);
		marketingImageContent.Title = InGameMessageUtils.GetAttribute<string>(node, attr[InGameMessageAttributes.TITLE]);
		marketingImageContent.TextureAssetUrl = textureAssetUrl.Trim();
		marketingImageContent.TextureOffsetX = InGameMessageUtils.GetAttribute<float>(node, attr[InGameMessageAttributes.TEXTURE_OFFSET_X]);
		marketingImageContent.TextureOffsetY = InGameMessageUtils.GetAttribute<float>(node, attr[InGameMessageAttributes.TEXTURE_OFFSET_Y]);
		marketingImageContent.PublishDate = InGameMessageUtils.GetAttribute<DateTime>(node, attr[InGameMessageAttributes.PUBLISH_DATE]);
		marketingImageContent.BeginningDate = beginningDate;
		marketingImageContent.ExpiryDate = expiryDate;
		marketingImageContent.MinGameVersion = InGameMessageUtils.GetAttribute<int>(node, attr[InGameMessageAttributes.MIN_GAME_VERSION]);
		marketingImageContent.MaxGameVersion = InGameMessageUtils.GetAttribute<int>(node, attr[InGameMessageAttributes.MAX_GAME_VERSION]);
		marketingImageContent.ProductId = InGameMessageUtils.GetAttribute<long>(node, attr[InGameMessageAttributes.PRODUCT_ID]);
		marketingImageContent.SlotCompatibility = slot;
		marketingImageContent.AutoFrame = InGameMessageUtils.GetAttribute<bool>(node, attr[InGameMessageAttributes.AUTO_FRAME]);
		marketingImageContent.Tags.UnionWith(tags);
		return marketingImageContent;
	}

	private IEnumerator<IAsyncJobResult> DownloadTextures(List<MarketingImageContent> images)
	{
		try
		{
			m_assetsDownloaded = false;
			string[] mimgFilePaths = Array.Empty<string>();
			HashSet<string> mimgFilesCached = new HashSet<string>();
			string[] array;
			try
			{
				if (Directory.Exists(m_mimgStorageDir))
				{
					mimgFilePaths = Directory.GetFiles(m_mimgStorageDir);
					array = mimgFilePaths;
					foreach (string mimgFilePath in array)
					{
						mimgFilesCached.Add(Path.GetFileName(mimgFilePath));
					}
				}
			}
			catch (Exception ex)
			{
				Log.Services.PrintError("[MarketingImagesService] Failed to enumerate directory '" + m_mimgStorageDir + "': " + ex.Message);
				yield break;
			}
			Dictionary<string, string> mimgsToDownload = new Dictionary<string, string>();
			HashSet<string> allRequiredMimgFiles = new HashSet<string>();
			List<MarketingImageContent> validImages = new List<MarketingImageContent>();
			foreach (MarketingImageContent image in images)
			{
				if (image == null || string.IsNullOrEmpty(image.TextureAssetUrl))
				{
					Log.Services.PrintError("[MarketingImagesService] Unable to download texture with uid=" + (image?.UID ?? "<null>") + ": missing url");
					continue;
				}
				if (!ProductId.IsValid(image.ProductId))
				{
					Log.Services.PrintError("[MarketingImagesService] Ignoring download texture request: missing product ID");
					continue;
				}
				string mimgCacheFile = FileUtils.GetMD5FromString(image.TextureAssetUrl);
				allRequiredMimgFiles.Add(mimgCacheFile);
				validImages.Add(image);
				if (!mimgFilesCached.Contains(mimgCacheFile))
				{
					mimgsToDownload[mimgCacheFile] = image.TextureAssetUrl;
				}
			}
			array = mimgFilePaths;
			foreach (string mimgFilePath2 in array)
			{
				if (mimgFilePath2.EndsWith(".json"))
				{
					continue;
				}
				string mimgFile = Path.GetFileName(mimgFilePath2);
				if (!allRequiredMimgFiles.Contains(mimgFile))
				{
					try
					{
						File.Delete(mimgFilePath2);
						mimgFilesCached.Remove(mimgFile);
					}
					catch (Exception ex2)
					{
						Log.Services.PrintError("[MarketingImagesService] Failed to delete file '" + mimgFilePath2 + "': " + ex2.Message);
					}
				}
			}
			List<string> mimgCacheFilesToDownload = new List<string>(mimgsToDownload.Keys);
			List<(string, string, UnityWebRequestAsyncOperation)> pendingOps = new List<(string, string, UnityWebRequestAsyncOperation)>();
			int numProcessed = 0;
			while (numProcessed < mimgCacheFilesToDownload.Count)
			{
				pendingOps.Clear();
				int num = numProcessed;
				int end = Math.Min(numProcessed + 5, mimgCacheFilesToDownload.Count);
				for (int j = num; j < end; j++)
				{
					string mimgCacheFile2 = mimgCacheFilesToDownload[j];
					string textureUrl = mimgsToDownload[mimgCacheFile2];
					string mimgCacheTempFilePath;
					UnityWebRequestAsyncOperation op;
					try
					{
						mimgCacheTempFilePath = Path.Combine(m_mimgStorageDir, mimgCacheFile2 + ".temp");
						UnityWebRequest unityWebRequest = UnityWebRequest.Get(textureUrl);
						unityWebRequest.downloadHandler = new DownloadHandlerFile(mimgCacheTempFilePath)
						{
							removeFileOnAbort = true
						};
						op = unityWebRequest.SendWebRequest();
					}
					catch (Exception ex3)
					{
						Log.Services.PrintError("[MarketingImagesService] Failed to download texture: url='" + textureUrl + "', error=" + ex3.Message);
						continue;
					}
					pendingOps.Add((mimgCacheFile2, mimgCacheTempFilePath, op));
					numProcessed++;
				}
				foreach (var (mimgCacheFile3, mimgCacheTempFilePath2, op2) in pendingOps)
				{
					while (!op2.isDone)
					{
						yield return null;
					}
					UnityWebRequest req = op2.webRequest;
					string textureUrl2 = req.url;
					if (req.result != UnityWebRequest.Result.Success)
					{
						Log.Services.PrintError(string.Format("{0} Failed to download texture: url='{1}', result={2}, error={3}", "[MarketingImagesService]", textureUrl2, req.result, req.error));
						continue;
					}
					string mimgCacheFilePath = mimgCacheTempFilePath2.Substring(0, mimgCacheTempFilePath2.Length - ".temp".Length);
					try
					{
						File.Move(mimgCacheTempFilePath2, mimgCacheFilePath);
					}
					catch (Exception ex4)
					{
						Log.Services.PrintError("[MarketingImagesService] Failed to rename download texture: url='" + textureUrl2 + "', srcFile='" + mimgCacheTempFilePath2 + "', dstFile='" + mimgCacheFilePath + "', error=" + ex4.Message);
						continue;
					}
					mimgFilesCached.Add(mimgCacheFile3);
				}
			}
			foreach (MarketingImageContent image2 in validImages)
			{
				string mimgCacheFile4 = FileUtils.GetMD5FromString(image2.TextureAssetUrl);
				if (mimgFilesCached.Contains(mimgCacheFile4))
				{
					string mimgCacheFilePath2 = Path.Combine(m_mimgStorageDir, mimgCacheFile4);
					MarketingImageConfig mimgConfig = new MarketingImageConfig
					{
						ProductId = image2.ProductId,
						TextureUrl = "file:///" + mimgCacheFilePath2.Replace('\\', '/'),
						TextureOffset = new Vector2(image2.TextureOffsetX, image2.TextureOffsetY),
						AutoFrame = image2.AutoFrame
					};
					mimgConfig.Tags.UnionWith(image2.Tags);
					if (!m_imagesPerProductId.TryGetValue(image2.ProductId, out var imgCfgSet))
					{
						imgCfgSet = new MarketingImageConfigSet();
						m_imagesPerProductId.Add(image2.ProductId, imgCfgSet);
					}
					switch (image2.SlotCompatibility)
					{
					case MarketingImageSlot.FullRow:
						imgCfgSet.FullRow = mimgConfig;
						continue;
					case MarketingImageSlot.HalfRow:
						imgCfgSet.HalfRow = mimgConfig;
						continue;
					}
					imgCfgSet.AnySize = mimgConfig;
					imgCfgSet.FullRow = mimgConfig;
					imgCfgSet.HalfRow = mimgConfig;
				}
			}
			m_assetsDownloaded = true;
			Log.Services.PrintInfo("[MarketingImagesService] Marketing images download complete");
		}
		finally
		{
			m_isDownloadingJobRunning = false;
		}
	}

	public IEnumerator LoadMarketingImage(long productID, MarketingImageSlot slotSize, Action<MarketingImageConfig> onComplete)
	{
		if (!m_initialized)
		{
			Log.Services.PrintError("[MarketingImagesService] MarketingImageService not initialized yet");
			onComplete?.Invoke(null);
			yield break;
		}
		MarketingImageConfig imgConfig = null;
		if (!TryGetConfig(productID, slotSize, out imgConfig))
		{
			Log.Services.PrintDebug(string.Format("{0} could not find image for product ID {1}, Service state: assets downloaded: {2}", "[MarketingImagesService]", productID, m_assetsDownloaded));
			onComplete?.Invoke(null);
			yield break;
		}
		if (imgConfig.Texture != null)
		{
			onComplete?.Invoke(imgConfig);
			yield break;
		}
		bool isDownloadedImage = false;
		bool assetFound = false;
		if (!string.IsNullOrEmpty(imgConfig.TextureUrl))
		{
			isDownloadedImage = true;
			assetFound = true;
		}
		else if (!string.IsNullOrEmpty(imgConfig.TextureAsset))
		{
			isDownloadedImage = false;
			assetFound = true;
		}
		if (!assetFound)
		{
			Log.Services.PrintError(string.Format("{0} could not find image ref in ImageConfig for product ID {1}", "[MarketingImagesService]", productID));
			onComplete?.Invoke(null);
			yield break;
		}
		Texture2D texture = null;
		if (isDownloadedImage)
		{
			bool isDone = false;
			Processor.RunCoroutine(LoadWebTexture(imgConfig.TextureUrl, delegate(Texture2D tex)
			{
				isDone = true;
				texture = tex;
			}));
			SimpleTimer timer = new SimpleTimer(new TimeSpan(0, 0, 3));
			while (!isDone && !timer.IsTimeout())
			{
				yield return null;
			}
		}
		else
		{
			bool isDone2 = false;
			Processor.RunCoroutine(LoadEmbeddedTexture(imgConfig.TextureAsset, delegate(Texture2D tex)
			{
				isDone2 = true;
				texture = tex;
			}));
			SimpleTimer timer = new SimpleTimer(new TimeSpan(0, 0, 3));
			while (!isDone2 && !timer.IsTimeout())
			{
				yield return null;
			}
		}
		imgConfig.Texture = texture;
		onComplete?.Invoke(imgConfig);
	}

	private IEnumerator LoadWebTexture(string textureUrl, Action<Texture2D> onComplete)
	{
		UnityWebRequest req;
		try
		{
			req = UnityWebRequest.Get(textureUrl);
			req.downloadHandler = new DownloadHandlerTexture(readable: false);
			req.timeout = 3;
			req.SendWebRequest();
		}
		catch (Exception ex)
		{
			Log.Services.PrintError("[MarketingImagesService] Failed to load texture: url='" + textureUrl + "', error=" + ex.Message);
			onComplete?.Invoke(null);
			yield break;
		}
		while (!req.isDone)
		{
			yield return null;
		}
		if (req.result != UnityWebRequest.Result.Success)
		{
			Log.Services.PrintError("[MarketingImagesService] Failed to load texture: " + $"url='{textureUrl}', result={req.result}, error={req.error}");
			onComplete?.Invoke(null);
			yield break;
		}
		Texture2D texture;
		try
		{
			texture = DownloadHandlerTexture.GetContent(req);
			texture.name = textureUrl;
		}
		catch (Exception ex2)
		{
			Log.Services.PrintError("[MarketingImagesService] Failed to load texture: url='" + textureUrl + "', error=" + ex2.Message);
			onComplete?.Invoke(null);
			yield break;
		}
		onComplete?.Invoke(texture);
	}

	private IEnumerator LoadEmbeddedTexture(AssetReference assetRef, Action<Texture2D> onComplete)
	{
		while (!m_gameDownloadMgr.IsReadyToPlay)
		{
			yield return null;
		}
		IAssetLoader assetLoader = AssetLoader.Get();
		if (assetLoader == null)
		{
			onComplete?.Invoke(null);
			yield break;
		}
		AssetHandleCallback<Texture2D> callback = delegate(AssetReference assetRef, AssetHandle<Texture2D> assetHandle, object cbData)
		{
			if ((bool)assetHandle)
			{
				onComplete?.Invoke(assetHandle.Asset);
			}
			else
			{
				Log.Services.PrintError(string.Format("{0} Failed to load texture with assetPath='{1}'", "[MarketingImagesService]", assetRef));
				onComplete?.Invoke(null);
			}
		};
		if (!assetLoader.LoadAsset(assetRef, callback))
		{
			callback(assetRef, null, null);
		}
	}
}
