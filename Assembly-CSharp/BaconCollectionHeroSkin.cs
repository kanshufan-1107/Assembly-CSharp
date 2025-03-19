using System;
using System.Collections;
using UnityEngine;

public class BaconCollectionHeroSkin : BaconCollectionSkin
{
	private BaconHeroSkinUtils.RotationType m_rotationType;

	private bool m_playerHasEarlyAccessHeroes;

	public GameObject m_heroPowerParent;

	private Actor m_heroPowerActor;

	private Action<Actor> m_heroPowerActorLoaded;

	public GameObject m_unownedStateTextWrapper;

	public UberText m_unownedStateUberText;

	private const float MOBILE_FAVORITE_X_POSITION = 0.94f;

	private const float PC_FAVORITE_X_POSITION = 0.97f;

	public override void Awake()
	{
		base.Awake();
		if (base.gameObject.GetComponent<Actor>() != null)
		{
			AssetLoader.Get().InstantiatePrefab("Card_Bacon_Collection_HeroPower.prefab:cba9305dae5005f45814f741f72e532d", OnHeroPowerActorLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_favoriteIcon.transform.position = new Vector3(0.94f, m_favoriteIcon.transform.position.y, m_favoriteIcon.transform.position.z);
		}
		else
		{
			m_favoriteIcon.transform.position = new Vector3(0.97f, m_favoriteIcon.transform.position.y, m_favoriteIcon.transform.position.z);
		}
	}

	private void OnDestroy()
	{
		m_heroPowerActorLoaded = null;
	}

	public void SetHeroPower(string heroPowerCardId)
	{
		DefLoader.Get().LoadFullDef(heroPowerCardId, OnHeroPowerFullDefLoaded);
	}

	public void RegisterHeroPowerActorLoaded(Action<Actor> loaded)
	{
		if (m_heroPowerActor != null)
		{
			loaded?.Invoke(m_heroPowerActor);
			return;
		}
		m_heroPowerActorLoaded = (Action<Actor>)Delegate.Remove(m_heroPowerActorLoaded, loaded);
		m_heroPowerActorLoaded = (Action<Actor>)Delegate.Combine(m_heroPowerActorLoaded, loaded);
	}

	public void UnregisterHeroPowerActorLoaded(Action<Actor> loaded)
	{
		m_heroPowerActorLoaded = (Action<Actor>)Delegate.Remove(m_heroPowerActorLoaded, loaded);
	}

	private void OnHeroPowerActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogWarning($"CollectionDeckInfo.OnHeroPowerActorLoaded() - FAILED to load actor \"{assetRef}\"");
			return;
		}
		m_heroPowerActor = go.GetComponent<Actor>();
		if (m_heroPowerActor == null)
		{
			Debug.LogWarning($"BaconCollectionHeroSkin.OnHeroPowerActorLoaded() - ERROR actor \"{assetRef}\" has no Actor component");
			return;
		}
		m_heroPowerActor.SetUnlit();
		m_heroPowerActor.transform.parent = m_heroPowerParent.transform;
		m_heroPowerActor.transform.localScale = Vector3.one;
		m_heroPowerActor.transform.localPosition = Vector3.zero;
		if (UniversalInputManager.Get().IsTouchMode())
		{
			m_heroPowerActor.TurnOffCollider();
		}
		m_heroPowerActorLoaded?.Invoke(m_heroPowerActor);
		m_heroPowerActorLoaded = null;
	}

	private void OnHeroPowerFullDefLoaded(string cardID, DefLoader.DisposableFullDef def, object userData)
	{
		StartCoroutine(SetHeroPowerInfoWhenReady(cardID, def, TAG_PREMIUM.NORMAL));
	}

	private IEnumerator SetHeroPowerInfoWhenReady(string heroPowerCardID, DefLoader.DisposableFullDef def, TAG_PREMIUM premium)
	{
		using (def)
		{
			while (m_heroPowerActor == null)
			{
				yield return null;
			}
			SetHeroPowerInfo(heroPowerCardID, def, premium);
		}
	}

	public void AddHeroPowerGhosting(EntityDef def)
	{
		StartCoroutine(AddHeroPowerGhostingWhenReady(def));
	}

	private IEnumerator AddHeroPowerGhostingWhenReady(EntityDef def)
	{
		while (m_heroPowerActor == null)
		{
			yield return null;
		}
		Actor actor = base.gameObject.GetComponent<Actor>();
		m_heroPowerActor.gameObject.GetComponent<BaconCollectionHeroPower>()?.HideItemsForGhostView();
		if (def.IsHeroSkin() && HeroSkinUtils.CanBuyHeroSkinFromCollectionManager(def.GetCardId()))
		{
			actor.GhostCardEffect(GhostCard.Type.MISSING);
		}
		else
		{
			actor.MissingCardEffect();
		}
	}

	private void SetHeroPowerInfo(string heroPowerCardID, DefLoader.DisposableFullDef def, TAG_PREMIUM premium)
	{
		m_heroPowerActor.Show();
		m_heroPowerActor.SetFullDef(def);
		m_heroPowerActor.SetUnlit();
		m_heroPowerActor.UpdateAllComponents();
		m_heroPowerActor.GetCostTextObject()?.SetActive(value: false);
		m_heroPowerActor.m_manaObject?.SetActive(value: false);
	}

	protected override void PopulateNameText()
	{
		if (m_rotationType == BaconHeroSkinUtils.RotationType.Resting)
		{
			GetActiveNameWrapper().SetActive(value: false);
			m_favoriteStateTextWrapper.SetActive(value: false);
			m_unownedStateTextWrapper.SetActive(value: true);
			m_unownedStateUberText.Text = GameStrings.Get("GLUE_BACON_COLLECTION_RESTING");
		}
		else if (!m_playerHasEarlyAccessHeroes && m_rotationType == BaconHeroSkinUtils.RotationType.Preview)
		{
			GetActiveNameWrapper().SetActive(value: false);
			m_favoriteStateTextWrapper.SetActive(value: false);
			m_unownedStateTextWrapper.SetActive(value: true);
			m_unownedStateUberText.Text = GameStrings.Get("GLUE_BACON_COLLECTION_PREVIEWING");
		}
		else
		{
			m_unownedStateTextWrapper.SetActive(value: false);
			base.PopulateNameText();
		}
	}

	protected override void ShowFavoriteUI()
	{
		GetActiveNameWrapper().SetActive(value: true);
		m_favoriteStateTextWrapper.SetActive(value: false);
		m_favoriteIcon.SetActive(value: true);
		if (m_favoriteNameBackground != null)
		{
			m_favoriteNameBackground.SetActive(value: false);
		}
		if (m_nonFavoriteNameBackground != null)
		{
			m_nonFavoriteNameBackground.SetActive(base.ShowName);
		}
	}

	protected override void HideFavoriteUI()
	{
		GetActiveNameWrapper().SetActive(value: true);
		m_favoriteStateTextWrapper.SetActive(value: false);
		m_favoriteIcon.SetActive(value: false);
		if (m_favoriteNameBackground != null)
		{
			m_favoriteNameBackground.SetActive(value: false);
		}
		if (m_nonFavoriteNameBackground != null)
		{
			m_nonFavoriteNameBackground.SetActive(base.ShowName);
		}
	}

	public void SetCardStateDisplay(CollectibleCard card, EntityDef entityDef, bool playerHasEarlyAccessHeroes)
	{
		string baseHeroCardId = CollectionManager.Get().GetBattlegroundsBaseHeroCardId(card.CardId);
		CardDbfRecord baseCardRecord = GameUtils.GetCardRecord(baseHeroCardId);
		EntityDef baseEntityDef = DefLoader.Get().GetEntityDef(baseHeroCardId);
		m_rotationType = BaconHeroSkinUtils.GetBattleGroundsHeroRotationType(baseCardRecord, baseEntityDef);
		m_playerHasEarlyAccessHeroes = playerHasEarlyAccessHeroes;
		CollectionManager.Get().GetCollectibleDisplay();
		bool isResting = m_rotationType == BaconHeroSkinUtils.RotationType.Resting;
		bool isPreviewing = !playerHasEarlyAccessHeroes && m_rotationType == BaconHeroSkinUtils.RotationType.Preview;
		if (card.OwnedCount == 0 || isResting || isPreviewing)
		{
			AddHeroPowerGhosting(entityDef);
		}
		PopulateNameText();
	}
}
