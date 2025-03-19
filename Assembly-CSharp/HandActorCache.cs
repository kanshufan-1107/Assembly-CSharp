using System;
using System.Collections.Generic;
using UnityEngine;

public class HandActorCache
{
	public delegate void ActorLoadedCallback(string name, Actor actor, object userData);

	private class ActorLoadedListener : EventListener<ActorLoadedCallback>
	{
		public void Fire(string name, Actor actor)
		{
			m_callback(name, actor, m_userData);
		}
	}

	private class ActorKey
	{
		public TAG_CARDTYPE m_cardType;

		public TAG_PREMIUM m_premiumType;

		public bool m_isHeroSkin;

		public int m_frameId;

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			ActorKey other = obj as ActorKey;
			return Equals(other);
		}

		public bool Equals(ActorKey other)
		{
			if (other == null)
			{
				return false;
			}
			if (m_cardType == other.m_cardType && m_premiumType == other.m_premiumType && m_isHeroSkin == other.m_isHeroSkin)
			{
				return m_frameId == other.m_frameId;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((23 * 17 + m_cardType.GetHashCode()) * 17 + m_premiumType.GetHashCode()) * 17 + m_isHeroSkin.GetHashCode();
		}
	}

	private readonly TAG_CARDTYPE[] ACTOR_CARD_TYPES = new TAG_CARDTYPE[6]
	{
		TAG_CARDTYPE.MINION,
		TAG_CARDTYPE.SPELL,
		TAG_CARDTYPE.WEAPON,
		TAG_CARDTYPE.HERO,
		TAG_CARDTYPE.HERO_POWER,
		TAG_CARDTYPE.LOCATION
	};

	private Dictionary<ActorKey, Actor> m_actorMap = new Dictionary<ActorKey, Actor>();

	private List<ActorLoadedListener> m_loadedListeners = new List<ActorLoadedListener>();

	public void Initialize()
	{
		TAG_CARDTYPE[] aCTOR_CARD_TYPES = ACTOR_CARD_TYPES;
		foreach (TAG_CARDTYPE cardType in aCTOR_CARD_TYPES)
		{
			foreach (TAG_PREMIUM premiumType in Enum.GetValues(typeof(TAG_PREMIUM)))
			{
				if (cardType == TAG_CARDTYPE.HERO)
				{
					string prefabPath = ActorNames.GetHeroSkinOrHandActor(cardType, premiumType);
					ActorKey key = MakeActorKey(cardType, premiumType, isHeroSkin: true);
					AssetLoader.Get().InstantiatePrefab(prefabPath, OnActorLoaded, key, AssetLoadingOptions.IgnorePrefabPosition);
					if (premiumType != TAG_PREMIUM.SIGNATURE)
					{
						prefabPath = ActorNames.GetHandActor(cardType, premiumType);
						key = MakeActorKey(cardType, premiumType);
						AssetLoader.Get().InstantiatePrefab(prefabPath, OnActorLoaded, key, AssetLoadingOptions.IgnorePrefabPosition);
					}
				}
				if (premiumType == TAG_PREMIUM.SIGNATURE)
				{
					foreach (KeyValuePair<int, string> signatureHandActorPairs in ActorNames.SignatureHandMinions)
					{
						ActorKey key = MakeActorKey(cardType, premiumType, isHeroSkin: false, signatureHandActorPairs.Key);
						AssetLoader.Get().InstantiatePrefab(signatureHandActorPairs.Value, OnActorLoaded, key, AssetLoadingOptions.IgnorePrefabPosition);
					}
				}
				else
				{
					string prefabPath = ActorNames.GetHeroSkinOrHandActor(cardType, premiumType);
					ActorKey key = MakeActorKey(cardType, premiumType);
					AssetLoader.Get().InstantiatePrefab(prefabPath, OnActorLoaded, key, AssetLoadingOptions.IgnorePrefabPosition);
				}
			}
		}
	}

	public bool IsInitializing()
	{
		TAG_CARDTYPE[] aCTOR_CARD_TYPES = ACTOR_CARD_TYPES;
		foreach (TAG_CARDTYPE cardType in aCTOR_CARD_TYPES)
		{
			foreach (TAG_PREMIUM premiumType in Enum.GetValues(typeof(TAG_PREMIUM)))
			{
				if (cardType == TAG_CARDTYPE.HERO && premiumType != TAG_PREMIUM.SIGNATURE)
				{
					ActorKey heroSkinKey = MakeActorKey(cardType, premiumType, isHeroSkin: true);
					ActorKey heroCardKey = MakeActorKey(cardType, premiumType);
					if (!m_actorMap.ContainsKey(heroSkinKey) || !m_actorMap.ContainsKey(heroCardKey))
					{
						return true;
					}
				}
				else if (premiumType == TAG_PREMIUM.SIGNATURE)
				{
					foreach (int frameId in ActorNames.GetSignatureFrameIds())
					{
						ActorKey key = MakeActorKey(cardType, premiumType, isHeroSkin: false, frameId);
						if (!m_actorMap.ContainsKey(key))
						{
							return true;
						}
					}
				}
				else
				{
					ActorKey key2 = MakeActorKey(cardType, premiumType);
					if (!m_actorMap.ContainsKey(key2))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public Actor GetActor(EntityDef entityDef, TAG_PREMIUM premium)
	{
		ActorKey key = MakeActorKey(entityDef, premium, entityDef.IsHeroSkin());
		if (!m_actorMap.TryGetValue(key, out var actor))
		{
			Debug.LogError($"HandActorCache.GetActor() - FAILED to get actor with cardType={entityDef.GetCardType()} premiumType={premium}");
			return null;
		}
		return actor;
	}

	public void AddActorLoadedListener(ActorLoadedCallback callback)
	{
		AddActorLoadedListener(callback, null);
	}

	public void AddActorLoadedListener(ActorLoadedCallback callback, object userData)
	{
		ActorLoadedListener listener = new ActorLoadedListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (!m_loadedListeners.Contains(listener))
		{
			m_loadedListeners.Add(listener);
		}
	}

	private ActorKey MakeActorKey(EntityDef entityDef, TAG_PREMIUM premium, bool isHeroSkin = false)
	{
		int frameId = 0;
		if (premium == TAG_PREMIUM.SIGNATURE)
		{
			frameId = ActorNames.GetSignatureFrameId(entityDef.GetCardId());
		}
		return MakeActorKey(entityDef.GetCardType(), premium, isHeroSkin, frameId);
	}

	private ActorKey MakeActorKey(TAG_CARDTYPE cardType, TAG_PREMIUM premiumType, bool isHeroSkin = false, int frameId = 0)
	{
		return new ActorKey
		{
			m_cardType = cardType,
			m_premiumType = premiumType,
			m_isHeroSkin = isHeroSkin,
			m_frameId = frameId
		};
	}

	private void OnActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogWarning($"HandActorCache.OnActorLoaded() - FAILED to load \"{assetRef}\"");
			return;
		}
		Actor actor = go.GetComponent<Actor>();
		if (actor == null)
		{
			Debug.LogWarning($"HandActorCache.OnActorLoaded() - ERROR \"{assetRef}\" has no Actor component");
			return;
		}
		go.transform.position = new Vector3(-99999f, -99999f, 99999f);
		ActorKey key = (ActorKey)callbackData;
		if (m_actorMap.ContainsKey(key))
		{
			Debug.LogWarning($"HandActorCache.OnActorLoaded() - ERROR \"{assetRef}\" key (cardtype={key.m_cardType} cardpremium={key.m_premiumType}) already exists in the dictionary");
			return;
		}
		m_actorMap.Add(key, actor);
		FireActorLoadedListeners(assetRef.ToString(), actor);
	}

	private void FireActorLoadedListeners(string assetRef, Actor actor)
	{
		ActorLoadedListener[] listeners = m_loadedListeners.ToArray();
		for (int i = 0; i < listeners.Length; i++)
		{
			listeners[i].Fire(assetRef, actor);
		}
	}
}
