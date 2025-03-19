using System;
using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Services;
using Hearthstone;
using UnityEngine;

public class DefLoader
{
	public class DisposableCardDef : IDisposable
	{
		private AssetHandle<GameObject> m_cardPrefabInstance;

		public CardDef CardDef { get; private set; }

		public DisposableCardDef(AssetHandle<GameObject> cardPrefabInstance)
		{
			m_cardPrefabInstance = cardPrefabInstance;
			CardDef = (m_cardPrefabInstance ? m_cardPrefabInstance.Asset.GetComponent<CardDef>() : null);
		}

		public void Dispose()
		{
			m_cardPrefabInstance?.Dispose();
		}

		public DisposableCardDef Share()
		{
			return new DisposableCardDef(m_cardPrefabInstance.Share());
		}
	}

	public class DisposableFullDef : IDisposable
	{
		public CardDef CardDef => DisposableCardDef?.CardDef;

		public DisposableCardDef DisposableCardDef { get; }

		public EntityDef EntityDef { get; private set; }

		public DisposableFullDef(EntityDef entityDef, DisposableCardDef cardDef)
		{
			EntityDef = entityDef;
			DisposableCardDef = cardDef;
		}

		public void Dispose()
		{
			DisposableCardDef?.Dispose();
		}

		public DisposableFullDef Share()
		{
			return new DisposableFullDef(EntityDef, DisposableCardDef?.Share());
		}
	}

	public delegate void LoadDefCallback<T>(string cardId, T def, object userData);

	private static DefLoader s_instance;

	private bool m_loadedEntityDefs;

	private Dictionary<string, EntityDef> m_entityDefCache = new Dictionary<string, EntityDef>();

	private bool m_isPlaying;

	public static DefLoader Get()
	{
		if (s_instance != null && s_instance.m_isPlaying != Application.isPlaying)
		{
			s_instance = null;
		}
		if (s_instance == null)
		{
			s_instance = new DefLoader();
			s_instance.m_isPlaying = Application.isPlaying;
			HearthstoneApplication hearthstoneApplication = HearthstoneApplication.Get();
			if (hearthstoneApplication != null)
			{
				hearthstoneApplication.WillReset += s_instance.WillReset;
			}
			else if (Application.isPlaying)
			{
				Log.All.PrintWarning("DefLoader being initialized before HearthstoneApplication is initialized! This is very bad if you're running the game!");
			}
		}
		return s_instance;
	}

	public void Initialize()
	{
		LoadAllEntityDefs();
	}

	public void Clear()
	{
		ClearEntityDefs();
	}

	public Dictionary<string, EntityDef> GetAllEntityDefs()
	{
		return m_entityDefCache;
	}

	public void ClearEntityDefs()
	{
		m_entityDefCache.Clear();
		m_loadedEntityDefs = false;
	}

	public EntityDef GetEntityDef(string cardId)
	{
		if (string.IsNullOrEmpty(cardId))
		{
			return null;
		}
		if (cardId.Equals("None"))
		{
			return null;
		}
		EntityDef entityDef = null;
		m_entityDefCache.TryGetValue(cardId, out entityDef);
		if (entityDef == null)
		{
			if (HearthstoneApplication.UseDevWorkarounds())
			{
				Debug.LogErrorFormat("DefLoader.GetEntityDef() - Failed to load {0}. Loading {1} instead.", cardId, "PlaceholderCard");
				m_entityDefCache.TryGetValue("PlaceholderCard", out var placeholderDef);
				if (placeholderDef == null)
				{
					Error.AddDevFatal("DefLoader.GetEntityDef() - Failed to load {0} in place of {1}", "PlaceholderCard", cardId);
					return null;
				}
				entityDef = placeholderDef.Clone();
				entityDef.SetCardId(cardId);
				m_entityDefCache[cardId] = entityDef;
			}
			else
			{
				Error.AddDevFatal("DefLoader.GetEntityDef() - Failed to load {0}", cardId);
			}
		}
		return entityDef;
	}

	public EntityDef GetEntityDef(int dbId, bool displayError = true)
	{
		string cardId = GameUtils.TranslateDbIdToCardId(dbId);
		if (cardId == null && displayError)
		{
			Debug.LogErrorFormat("DefLoader.GetEntityDef() - dbId {0} does not map to a cardId", dbId);
			return null;
		}
		return GetEntityDef(cardId);
	}

	public void LoadAllEntityDefs()
	{
		List<string> cardIds = GameUtils.GetAllCardIds();
		if (!cardIds.Contains("PlaceholderCard"))
		{
			cardIds.Add("PlaceholderCard");
		}
		m_entityDefCache = EntityDef.LoadBatchCardEntityDefs(cardIds, out var failedCardIds);
		m_loadedEntityDefs = true;
		if (failedCardIds.Count > 0)
		{
			if (Application.isEditor)
			{
				Debug.LogWarningFormat("LoadAllEntityDefs: Missing Cards! Proceed with caution. - Failed to load {0} card(s) on startup - {1}", failedCardIds.Count, string.Join(", ", failedCardIds.ToArray()));
			}
			else
			{
				Error.AddDevWarning("Missing Cards", "Failed to load {0} card(s) on startup!\n\n{1}", failedCardIds.Count, string.Join(", ", failedCardIds.ToArray()));
			}
		}
	}

	public bool HasLoadedEntityDefs()
	{
		return m_loadedEntityDefs;
	}

	public void LoadCardDef(string cardId, LoadDefCallback<DisposableCardDef> callback, object userData = null, CardPortraitQuality quality = null)
	{
		DisposableCardDef cardDef = GetCardDef(cardId, quality);
		callback(cardId, cardDef, userData);
	}

	public DisposableCardDef GetCardDef(int dbId)
	{
		string cardId = GameUtils.TranslateDbIdToCardId(dbId);
		if (cardId == null)
		{
			Debug.LogError($"DefLoader.GetCardDef() - dbId {dbId} does not map to a cardId");
			return null;
		}
		return GetCardDef(cardId);
	}

	public DisposableCardDef GetCardDef(string cardId, TAG_PREMIUM premiumType)
	{
		CardPortraitQuality quality = CardPortraitQuality.GetDefault();
		quality.PremiumType = premiumType;
		return GetCardDef(cardId, quality);
	}

	public DisposableCardDef GetCardDef(string cardId, CardPortraitQuality quality = null)
	{
		if (string.IsNullOrEmpty(cardId) || AssetLoader.Get() == null)
		{
			return null;
		}
		if (cardId.Equals("None"))
		{
			return null;
		}
		if (quality == null)
		{
			quality = CardPortraitQuality.GetDefault();
		}
		if (PlatformSettings.ShouldFallbackToLowRes && quality.TextureQuality > 1)
		{
			quality.TextureQuality = 1;
		}
		AssetReference prefabRef = ServiceManager.Get<IAliasedAssetResolver>().GetCardDefAssetRefFromCardId(cardId);
		AssetHandle<GameObject> cardPrefabInstance = AssetLoader.Get().GetOrInstantiateSharedPrefab(prefabRef);
		CardDef cardDef = (cardPrefabInstance ? cardPrefabInstance.Asset.GetComponent<CardDef>() : null);
		if (cardDef == null)
		{
			cardPrefabInstance?.Dispose();
			if (!HearthstoneApplication.UseDevWorkarounds())
			{
				Error.AddDevFatal("DefLoader.GetCardDef() - Failed to load {0}", cardId);
				return null;
			}
			Debug.LogErrorFormat("DefLoader.GetCardDef() - Failed to load {0}. Using {1} instead.", cardId, "PlaceholderCard");
			cardPrefabInstance = LoadPlaceholderCardPrefab();
			if (!cardPrefabInstance)
			{
				Error.AddDevFatal("DefLoader.GetCardDef() - Failed to load {0} in place of {1}", "PlaceholderCard", cardId);
				return null;
			}
			cardDef = cardPrefabInstance.Asset.GetComponent<CardDef>();
		}
		if (CardPortraitQuality.GetFromDef(cardDef).TextureQuality < quality.TextureQuality || !cardDef.IsPremiumLoaded(quality.PremiumType))
		{
			CardTextureLoader.Load(cardDef, quality);
		}
		return new DisposableCardDef(cardPrefabInstance);
	}

	private AssetHandle<GameObject> LoadPlaceholderCardPrefab()
	{
		AssetReference prefabRef = ServiceManager.Get<IAliasedAssetResolver>().GetCardDefAssetRefFromCardId("PlaceholderCard");
		AssetHandle<GameObject> placeholderCardPrefab = AssetLoader.Get().GetOrInstantiateSharedPrefab(prefabRef);
		if (!placeholderCardPrefab)
		{
			Debug.LogErrorFormat("DefLoader.LoadPlaceholderCardPrefab() - Failed to load {0}", "PlaceholderCard");
			return null;
		}
		return placeholderCardPrefab;
	}

	private void WillReset()
	{
		ClearEntityDefs();
	}

	public DisposableFullDef GetFullDef(int dbId)
	{
		string cardId = GameUtils.TranslateDbIdToCardId(dbId);
		if (cardId == null)
		{
			Debug.LogError($"DefLoader.GetCardDef() - dbId {dbId} does not map to a cardId");
			return null;
		}
		return GetFullDef(cardId);
	}

	public DisposableFullDef GetFullDef(string cardId, CardPortraitQuality quality = null)
	{
		EntityDef entityDef = GetEntityDef(cardId);
		DisposableCardDef cardDef = GetCardDef(cardId, quality);
		return new DisposableFullDef(entityDef, cardDef);
	}

	public void LoadFullDef(string cardId, LoadDefCallback<DisposableFullDef> callback, object userData = null, CardPortraitQuality quality = null)
	{
		callback(cardId, GetFullDef(cardId, quality), userData);
	}
}
