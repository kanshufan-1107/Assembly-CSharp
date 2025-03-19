using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class CollectionDeckInfo : MonoBehaviour
{
	public delegate void ShowListener();

	public delegate void HideListener();

	public GameObject m_root;

	public GameObject m_visualRoot;

	public GameObject m_heroPowerParent;

	public UberText m_heroPowerName;

	public UberText m_heroPowerDescription;

	public UberText m_manaCurveTooltipText;

	public PegUIElement m_offClicker;

	public List<DeckInfoManaBar> m_manaBars;

	private readonly float MANA_COST_TEXT_MIN_LOCAL_Z;

	private readonly float MANA_COST_TEXT_MAX_LOCAL_Z = 5.167298f;

	private DefLoader.DisposableCardDef m_heroCardDef;

	private Actor m_activeHeroPowerActor;

	private Actor m_defaultHeroPowerActor;

	private Actor m_defaultGoldenHeroPowerActor;

	private Dictionary<string, Actor> m_heroPowerActors = new Dictionary<string, Actor>();

	private bool m_wasTouchModeEnabled;

	protected bool m_shown = true;

	private string m_heroPowerID = "";

	private List<ShowListener> m_showListeners = new List<ShowListener>();

	private List<HideListener> m_hideListeners = new List<HideListener>();

	private void Awake()
	{
		m_manaCurveTooltipText.Text = GameStrings.Get("GLUE_COLLECTION_DECK_INFO_MANA_TOOLTIP");
		foreach (DeckInfoManaBar manaBar in m_manaBars)
		{
			manaBar.m_costText.Text = GetTextForManaCost(manaBar.m_manaCostID);
		}
		AssetLoader.Get().InstantiatePrefab("Card_Play_HeroPower.prefab:a3794839abb947146903a26be13e09af", OnDefaultHeroPowerActorLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		AssetLoader.Get().InstantiatePrefab("Card_Play_HeroPower_Premium.prefab:015ad985f9ec49e4db327d131fd79901", OnDefaultGoldenHeroPowerActorLoaded, null, AssetLoadingOptions.IgnorePrefabPosition);
		m_wasTouchModeEnabled = true;
	}

	private void Start()
	{
		m_offClicker.AddEventListener(UIEventType.RELEASE, OnClosePressed);
		m_offClicker.AddEventListener(UIEventType.ROLLOVER, OverOffClicker);
	}

	private void OnDestroy()
	{
		m_heroCardDef?.Dispose();
		m_heroCardDef = null;
	}

	private void Update()
	{
		if (m_wasTouchModeEnabled == UniversalInputManager.Get().IsTouchMode())
		{
			return;
		}
		m_wasTouchModeEnabled = UniversalInputManager.Get().IsTouchMode();
		if (UniversalInputManager.Get().IsTouchMode())
		{
			if (m_activeHeroPowerActor != null)
			{
				m_activeHeroPowerActor.TurnOffCollider();
			}
			m_offClicker.gameObject.SetActive(value: true);
		}
		else
		{
			if (m_activeHeroPowerActor != null)
			{
				m_activeHeroPowerActor.TurnOnCollider();
			}
			m_offClicker.gameObject.SetActive(value: true);
		}
	}

	public void Show()
	{
		if (!m_shown)
		{
			if (CollectionDeckTray.Get() == null)
			{
				m_visualRoot.SetActive(value: true);
			}
			else
			{
				CollectionDeck currentDeck = CollectionDeckTray.Get().GetCardsContent().GetEditingDeck();
				m_visualRoot.SetActive(!currentDeck.HasUIHeroOverride());
			}
			m_root.SetActive(value: true);
			m_shown = true;
			if (UniversalInputManager.Get().IsTouchMode())
			{
				Navigation.Push(GoBackImpl);
			}
			ShowListener[] array = m_showListeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i]();
			}
		}
	}

	private bool GoBackImpl()
	{
		Hide();
		return true;
	}

	public void Hide()
	{
		Navigation.RemoveHandler(GoBackImpl);
		if (m_shown)
		{
			m_root.SetActive(value: false);
			m_shown = false;
			HideListener[] array = m_hideListeners.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i]();
			}
		}
	}

	public void RegisterShowListener(ShowListener dlg)
	{
		m_showListeners.Add(dlg);
	}

	public void UnregisterShowListener(ShowListener dlg)
	{
		m_showListeners.Remove(dlg);
	}

	public void RegisterHideListener(HideListener dlg)
	{
		m_hideListeners.Add(dlg);
	}

	public void UnregisterHideListener(HideListener dlg)
	{
		m_hideListeners.Remove(dlg);
	}

	public bool IsShown()
	{
		return m_shown;
	}

	public void UpdateManaCurve()
	{
		CollectionDeck currentDeck = CollectionDeckTray.Get().GetCardsContent().GetEditingDeck();
		UpdateManaCurve(currentDeck);
	}

	public void UpdateManaCurve(CollectionDeck deck)
	{
		if (deck == null)
		{
			Debug.LogWarning($"CollectionDeckInfo.UpdateManaCurve(): deck is null.");
			return;
		}
		string heroCardID = deck.HeroCardID;
		CardPortraitQuality portraitQuality = new CardPortraitQuality(3, TAG_PREMIUM.NORMAL);
		DefLoader.Get().LoadCardDef(heroCardID, OnHeroCardDefLoaded, null, portraitQuality);
		foreach (DeckInfoManaBar manaBar3 in m_manaBars)
		{
			manaBar3.m_numCards = 0;
		}
		int maxNumCards = 0;
		foreach (CollectionDeckSlot slot in deck.GetSlots())
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(slot.CardID);
			int manaCost = entityDef.GetCost();
			if (manaCost > 7)
			{
				manaCost = 7;
			}
			DeckInfoManaBar manaBar = m_manaBars.Find((DeckInfoManaBar obj) => obj.m_manaCostID == manaCost);
			if (manaBar == null)
			{
				Debug.LogWarning($"CollectionDeckInfo.UpdateManaCurve(): Cannot update curve. Could not find mana bar for {entityDef} (cost {manaCost})");
				return;
			}
			manaBar.m_numCards += slot.Count;
			if (manaBar.m_numCards > maxNumCards)
			{
				maxNumCards = manaBar.m_numCards;
			}
		}
		foreach (DeckInfoManaBar manaBar2 in m_manaBars)
		{
			manaBar2.m_numCardsText.Text = Convert.ToString(manaBar2.m_numCards);
			float percentFill = ((maxNumCards == 0) ? 0f : ((float)manaBar2.m_numCards / (float)maxNumCards));
			Vector3 textLocalPos = manaBar2.m_numCardsText.transform.localPosition;
			textLocalPos.z = Mathf.Lerp(MANA_COST_TEXT_MIN_LOCAL_Z, MANA_COST_TEXT_MAX_LOCAL_Z, percentFill);
			manaBar2.m_numCardsText.transform.localPosition = textLocalPos;
			manaBar2.m_barFill.GetComponent<Renderer>().GetMaterial().SetFloat("_Percent", percentFill);
		}
	}

	public void SetDeck(CollectionDeck deck)
	{
		if (deck == null)
		{
			Debug.LogWarning($"CollectionDeckInfo.SetDeckID(): deck is null");
			return;
		}
		UpdateManaCurve(deck);
		if (!string.IsNullOrEmpty(deck.HeroPowerCardID))
		{
			m_heroPowerID = deck.HeroPowerCardID;
		}
		else
		{
			string newHeroPowerID = GameUtils.GetHeroPowerCardIdFromHero(deck.HeroCardID);
			if (string.IsNullOrEmpty(newHeroPowerID))
			{
				if (!deck.HeroCardID.Equals("None"))
				{
					Debug.LogWarning("CollectionDeckInfo.UpdateInfo(): invalid hero power ID with given hero card ID " + deck.HeroCardID);
				}
				m_heroPowerID = "";
				return;
			}
			if (newHeroPowerID.Equals(m_heroPowerID))
			{
				return;
			}
			m_heroPowerID = newHeroPowerID;
		}
		TAG_PREMIUM heroPremium = CollectionManager.Get().GetHeroPremium(deck.GetClass());
		if (!m_heroPowerActors.ContainsKey(m_heroPowerID))
		{
			SetupHeroPowerForHero(deck.GetDisplayHeroCardID(rerollFavoriteHero: false), m_heroPowerID, heroPremium);
		}
		DefLoader.Get().LoadFullDef(m_heroPowerID, OnHeroPowerFullDefLoaded, heroPremium);
	}

	private string GetTextForManaCost(int manaCostID)
	{
		if (manaCostID < 0 || manaCostID > 7)
		{
			Debug.LogWarning($"CollectionDeckInfo.GetTextForManaCost(): don't know how to handle mana cost ID {manaCostID}");
			return "";
		}
		string text = Convert.ToString(manaCostID);
		if (manaCostID == 7)
		{
			text += GameStrings.Get("GLUE_COLLECTION_PLUS");
		}
		return text;
	}

	private void SetupHeroPowerForHero(string heroCardId, string heroPowerId, TAG_PREMIUM premium)
	{
		if (!m_heroPowerActors.ContainsKey(heroCardId))
		{
			string actorName = ActorNames.GetNameWithPremiumType(ActorNames.ACTOR_ASSET.PLAY_HERO_POWER, premium, heroCardId);
			if (string.IsNullOrEmpty(actorName))
			{
				Debug.LogWarning($"CollectionDeckInfo.SetupHeroPower(): Hero Card {heroCardId} doesn't have a valid Play Hero Power");
			}
			if (actorName == "Card_Play_HeroPower.prefab:a3794839abb947146903a26be13e09af")
			{
				m_heroPowerActors[heroPowerId] = m_defaultHeroPowerActor;
			}
			else if (actorName == "Card_Play_HeroPower_Premium.prefab:015ad985f9ec49e4db327d131fd79901")
			{
				m_heroPowerActors[heroPowerId] = m_defaultGoldenHeroPowerActor;
			}
			else
			{
				AssetLoader.Get().InstantiatePrefab(actorName, OnHeroPowerActorOverrideLoaded, heroPowerId, AssetLoadingOptions.IgnorePrefabPosition);
			}
		}
	}

	private void OnHeroPowerActorOverrideLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		string heroPowerId = (string)callbackData;
		if (string.IsNullOrEmpty(heroPowerId))
		{
			Debug.LogWarning($"CollectionDeckInfo.OnHeroPowerActorOverrideLoaded() - FAILED to parse callbackData when loading actor \"{assetRef}\"");
		}
		if (go == null)
		{
			Debug.LogWarning($"CollectionDeckInfo.OnHeroPowerActorOverrideLoaded() - FAILED to load actor \"{assetRef}\"");
			return;
		}
		Actor actor = go.GetComponent<Actor>();
		if (actor == null)
		{
			Debug.LogWarning($"CollectionDeckInfo.OnHeroPowerActorOverrideLoaded() - ERROR actor \"{assetRef}\" has no Actor component");
			return;
		}
		actor.SetUnlit();
		actor.transform.parent = m_heroPowerParent.transform;
		actor.transform.localScale = Vector3.one;
		actor.transform.localPosition = Vector3.zero;
		if (UniversalInputManager.Get().IsTouchMode())
		{
			actor.TurnOffCollider();
		}
		m_heroPowerActors.Add(heroPowerId, actor);
	}

	private void OnDefaultHeroPowerActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogWarning($"CollectionDeckInfo.OnHeroPowerActorLoaded() - FAILED to load actor \"{assetRef}\"");
			return;
		}
		m_defaultHeroPowerActor = go.GetComponent<Actor>();
		if (m_defaultHeroPowerActor == null)
		{
			Debug.LogWarning($"CollectionDeckInfo.OnHeroPowerActorLoaded() - ERROR actor \"{assetRef}\" has no Actor component");
			return;
		}
		m_defaultHeroPowerActor.SetUnlit();
		m_defaultHeroPowerActor.transform.parent = m_heroPowerParent.transform;
		m_defaultHeroPowerActor.transform.localScale = Vector3.one;
		m_defaultHeroPowerActor.transform.localPosition = Vector3.zero;
		m_defaultHeroPowerActor.Hide();
		if (UniversalInputManager.Get().IsTouchMode())
		{
			m_defaultHeroPowerActor.TurnOffCollider();
		}
	}

	private void OnDefaultGoldenHeroPowerActorLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Debug.LogWarning($"CollectionDeckInfo.OnHeroPowerActorLoaded() - FAILED to load actor \"{assetRef}\"");
			return;
		}
		m_defaultGoldenHeroPowerActor = go.GetComponent<Actor>();
		if (m_defaultGoldenHeroPowerActor == null)
		{
			Debug.LogWarning($"CollectionDeckInfo.OnGoldenHeroPowerActorLoaded() - ERROR actor \"{assetRef}\" has no Actor component");
			return;
		}
		m_defaultGoldenHeroPowerActor.SetUnlit();
		m_defaultGoldenHeroPowerActor.transform.parent = m_heroPowerParent.transform;
		m_defaultGoldenHeroPowerActor.transform.localScale = Vector3.one;
		m_defaultGoldenHeroPowerActor.transform.localPosition = Vector3.zero;
		m_defaultGoldenHeroPowerActor.Hide();
		if (UniversalInputManager.Get().IsTouchMode())
		{
			m_defaultGoldenHeroPowerActor.TurnOffCollider();
		}
	}

	private void OnHeroPowerFullDefLoaded(string cardID, DefLoader.DisposableFullDef def, object userData)
	{
		TAG_PREMIUM premium = (TAG_PREMIUM)userData;
		StartCoroutine(SetHeroPowerInfoWhenReady(cardID, def, premium));
	}

	private IEnumerator SetHeroPowerInfoWhenReady(string heroPowerCardID, DefLoader.DisposableFullDef def, TAG_PREMIUM premium)
	{
		using (def)
		{
			while (m_defaultHeroPowerActor == null)
			{
				yield return null;
			}
			while (m_defaultGoldenHeroPowerActor == null)
			{
				yield return null;
			}
			while (!m_heroPowerActors.ContainsKey(heroPowerCardID))
			{
				yield return null;
			}
			SetHeroPowerInfo(heroPowerCardID, def, premium);
		}
	}

	private void SetHeroPowerInfo(string heroPowerCardID, DefLoader.DisposableFullDef def, TAG_PREMIUM premium)
	{
		if (heroPowerCardID.Equals(m_heroPowerID))
		{
			if (m_activeHeroPowerActor != null)
			{
				m_activeHeroPowerActor.Hide();
			}
			m_heroPowerActors.TryGetValue(heroPowerCardID, out m_activeHeroPowerActor);
			if (m_activeHeroPowerActor != null)
			{
				m_activeHeroPowerActor.Show();
				m_activeHeroPowerActor.SetFullDef(def);
				m_activeHeroPowerActor.SetUnlit();
				m_activeHeroPowerActor.SetPremium(premium);
				m_activeHeroPowerActor.UpdateAllComponents();
			}
			else
			{
				Debug.LogWarning($"CollectionDeckInfo.SetHeroPowerInfo(): hero power actor for {heroPowerCardID} is null. Call setup on that power before trying to use it");
			}
			string heroPowerName = def.EntityDef.GetName();
			m_heroPowerName.Text = heroPowerName;
			string heroPowerDescription = def.EntityDef.GetCardTextInHand();
			m_heroPowerDescription.Text = heroPowerDescription;
		}
	}

	private void OnHeroCardDefLoaded(string cardId, DefLoader.DisposableCardDef def, object userData)
	{
		m_heroCardDef?.Dispose();
		m_heroCardDef = def;
	}

	private void OnClosePressed(UIEvent e)
	{
		Hide();
	}

	private void OverOffClicker(UIEvent e)
	{
		Hide();
	}
}
