using System;
using System.Collections.Generic;
using Hearthstone.UI;
using UnityEngine;

public class MassPackOpeningHighlights : MonoBehaviour
{
	private Queue<List<NetCache.BoosterCard>> m_highlights = new Queue<List<NetCache.BoosterCard>>();

	private const float LEGENDARY_WEIGHT = 50f;

	private const float EPIC_WEIGHT = 20f;

	private const float RARE_WEIGHT = 5f;

	private const float COMMON_WEIGHT = 1f;

	private const float SIGNATURE_WEIGHT = 10f;

	private const float GOLDEN_WEIGHT = 2f;

	private const float NORMAL_WEIGHT = 1f;

	private const float NEW_WEIGHT = 1.5f;

	private const float NOT_NEW_WEIGHT = 1f;

	private const string CODE_TRIGGER_EXPLOSION = "DoAnim";

	private const string CODE_HIDE_EXPLOSION = "None";

	private const string CODE_LOOPING_STATE_NAME = "Play Looping FX";

	[SerializeField]
	private GameObject m_rootPC;

	[SerializeField]
	private GameObject m_buttonRevealPC;

	[SerializeField]
	private GameObject m_buttonContinuePC;

	[SerializeField]
	private GameObject m_rootPhone;

	[SerializeField]
	private GameObject m_buttonRevealPhone;

	[SerializeField]
	private GameObject m_buttonContinuePhone;

	[SerializeField]
	private GameObject m_banner;

	[SerializeField]
	private UberText m_numPacksOpened;

	[SerializeField]
	private PlayMakerFSM m_explosionAnimationPlayMakerOver25;

	[SerializeField]
	private PlayMakerFSM m_explosionAnimationPlayMakerOver10;

	[SerializeField]
	private PlayMakerFSM m_explosionAnimationPlayMakerUnder10;

	private WidgetInstance m_cardsWidget;

	private MassPackOpeningHighlightsCards m_massPackOpeningHighlightsCards;

	private List<PackOpeningCard> m_cards;

	private GameObject m_buttonReveal;

	private GameObject m_buttonContinue;

	private PlayMakerFSM m_explosionAnimationPlayMaker;

	private static readonly AssetReference HIGHLIGHT_CARDS_PC = new AssetReference("MassPackOpeningHighlights_Cards_PC.prefab:162290527edeeb54182aa51e1768476e");

	private static readonly AssetReference HIGHLIGHT_CARDS_PHONE = new AssetReference("MassPackOpeningHighlights_Cards_Phone.prefab:671887496dde9854f93a5396998176bb");

	[SerializeField]
	[Tooltip("The PlayMaker script variable for the card back material")]
	[Header("Highlights Explosion PlayMaker Variable Names")]
	private string m_explosionCardBack;

	private CameraOverridePass m_explosionCustomPass;

	public WidgetInstance SpawnNewHighlightCards()
	{
		if (m_cardsWidget != null)
		{
			m_cardsWidget.Hide();
			UnityEngine.Object.Destroy(m_cardsWidget);
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_cardsWidget = WidgetInstance.Create(HIGHLIGHT_CARDS_PHONE);
			m_buttonReveal = m_buttonRevealPhone;
			m_buttonContinue = m_buttonContinuePhone;
		}
		else
		{
			m_cardsWidget = WidgetInstance.Create(HIGHLIGHT_CARDS_PC);
			m_buttonReveal = m_buttonRevealPC;
			m_buttonContinue = m_buttonContinuePC;
		}
		m_cardsWidget.RegisterReadyListener(delegate
		{
			GameObject gameObject = (UniversalInputManager.UsePhoneUI ? m_rootPhone : m_rootPC);
			if (gameObject != null)
			{
				GameUtils.SetParent(m_cardsWidget, gameObject);
			}
			m_massPackOpeningHighlightsCards = m_cardsWidget.GetComponentInChildren<MassPackOpeningHighlightsCards>();
			if (m_massPackOpeningHighlightsCards != null)
			{
				m_cards = m_massPackOpeningHighlightsCards.GetPackOpeningCards();
			}
		});
		return m_cardsWidget;
	}

	public void SetNumPacksOpened(int numPacks)
	{
		if (numPacks >= 25)
		{
			m_explosionAnimationPlayMaker = m_explosionAnimationPlayMakerOver25;
		}
		else if (numPacks >= 10)
		{
			m_explosionAnimationPlayMaker = m_explosionAnimationPlayMakerOver10;
		}
		else
		{
			m_explosionAnimationPlayMaker = m_explosionAnimationPlayMakerUnder10;
		}
		if (m_numPacksOpened != null)
		{
			m_numPacksOpened.Text = GameStrings.Format("GLUE_MASS_PACK_OPEN_PACKS_OPENED", numPacks);
		}
	}

	public Queue<List<NetCache.BoosterCard>> GetHighights()
	{
		return m_highlights;
	}

	public void ShowContinueButton(bool show)
	{
		if (m_buttonContinue != null && m_buttonReveal != null)
		{
			m_buttonContinue.SetActive(show);
			m_buttonReveal.SetActive(!show);
		}
	}

	public bool IsContinueButtonShowing()
	{
		if (m_buttonContinue != null)
		{
			return m_buttonContinue.activeInHierarchy;
		}
		return false;
	}

	public bool IsRevealButtonShowing()
	{
		if (m_buttonReveal != null)
		{
			return m_buttonReveal.activeInHierarchy;
		}
		return false;
	}

	public void DetermineMassPackOpeningHighlights(List<NetCache.BoosterCard> cards)
	{
		List<NetCache.BoosterCard> highlightsList = new List<NetCache.BoosterCard>();
		List<NetCache.BoosterCard> fillerList = new List<NetCache.BoosterCard>();
		foreach (NetCache.BoosterCard card in cards)
		{
			EntityDef entity = DefLoader.Get().GetEntityDef(card.Def.Name);
			if (entity == null)
			{
				Log.All.PrintWarning("Card has no entity [" + card.Def.Name + "]");
			}
			else if (IsCardAboveTheFold(card, entity))
			{
				highlightsList.Add(card);
			}
			else
			{
				fillerList.Add(card);
			}
		}
		FillHighlightsQueue(highlightsList, fillerList);
		Queue<NetCache.BoosterCard> highlightsQueue = ShuffleListIntoQueue(highlightsList);
		int numberOfHighlights = highlightsList.Count / 5;
		for (int i = 0; i < numberOfHighlights; i++)
		{
			List<NetCache.BoosterCard> highlightSet = new List<NetCache.BoosterCard>();
			while (highlightSet.Count < 5)
			{
				if (highlightsQueue.Count <= 0)
				{
					Log.All.PrintWarning("Ran out of highlights to show, this should never happen");
					return;
				}
				highlightSet.Add(highlightsQueue.Dequeue());
			}
			m_highlights.Enqueue(highlightSet);
		}
	}

	public void TriggerCardExplosionAnimation()
	{
		if (!(m_explosionAnimationPlayMaker == null))
		{
			CustomViewEntryPoint entryPoint = CustomViewEntryPoint.PerspectivePostFullscreenFX;
			GameLayer targetLayer = GameLayer.Default;
			if (m_explosionCustomPass == null)
			{
				m_explosionCustomPass = CameraPassProvider.RequestPass("MPOCardExplosionFX", 1 << (int)targetLayer, entryPoint);
			}
			if (!m_explosionCustomPass.isScheduled)
			{
				m_explosionCustomPass.Schedule(entryPoint);
			}
			Transform explosionFXRoot = m_explosionAnimationPlayMaker.transform;
			ApplySettingsToRenderers(explosionFXRoot, targetLayer, m_explosionCustomPass.renderLayerMaskOverride);
			m_explosionAnimationPlayMaker.SendEvent("DoAnim");
		}
	}

	private void ApplySettingsToRenderers(Transform fxRoot, GameLayer layer, uint renderingLayerMask)
	{
		Renderer[] componentsInChildren = fxRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
		foreach (Renderer obj in componentsInChildren)
		{
			obj.gameObject.layer = (int)layer;
			obj.renderingLayerMask = renderingLayerMask;
		}
	}

	public void HideCardExplosionAnimation()
	{
		if (!(m_explosionAnimationPlayMaker == null))
		{
			m_explosionAnimationPlayMaker.SendEvent("None");
		}
	}

	public bool IsInitialCardExplosionComplete()
	{
		if (m_explosionAnimationPlayMaker == null)
		{
			return false;
		}
		return m_explosionAnimationPlayMaker.ActiveStateName == "Play Looping FX";
	}

	public void SetBannerActive(bool active)
	{
		if (m_banner != null)
		{
			m_banner.SetActive(active);
		}
	}

	public List<PackOpeningCard> GetPackOpeningCards()
	{
		return m_cards;
	}

	private void SetExplosionCardback()
	{
		if (!(m_explosionAnimationPlayMaker == null))
		{
			Material material = CardBackManager.Get().GetCardBackMaterialFromSlot(CardBackManager.CardBackSlot.FAVORITE);
			if (material != null)
			{
				m_explosionAnimationPlayMaker.FsmVariables.GetFsmMaterial(m_explosionCardBack).Value = material;
			}
		}
	}

	private bool IsCardAboveTheFold(NetCache.BoosterCard card, EntityDef entity)
	{
		TAG_RARITY rarity = entity.GetRarity();
		if (rarity == TAG_RARITY.LEGENDARY)
		{
			return true;
		}
		TAG_PREMIUM premium = card.Def.Premium;
		if (premium == TAG_PREMIUM.SIGNATURE)
		{
			return true;
		}
		if (rarity == TAG_RARITY.EPIC)
		{
			if (premium == TAG_PREMIUM.GOLDEN)
			{
				return true;
			}
			CollectibleCard collectibleCard = CollectionManager.Get().GetCard(entity.GetCardId(), premium);
			if (collectibleCard.SeenCount < 1 && collectibleCard.OwnedCount < 2)
			{
				return true;
			}
		}
		return false;
	}

	private Queue<NetCache.BoosterCard> ShuffleListIntoQueue(List<NetCache.BoosterCard> list)
	{
		Queue<NetCache.BoosterCard> queue = new Queue<NetCache.BoosterCard>();
		System.Random rng = new System.Random();
		for (int i = list.Count - 1; i > 1; i--)
		{
			int randomIndex = rng.Next(i + 1);
			NetCache.BoosterCard card = list[randomIndex];
			list[randomIndex] = list[i];
			list[i] = card;
		}
		foreach (NetCache.BoosterCard card2 in list)
		{
			queue.Enqueue(card2);
		}
		return queue;
	}

	private void FillHighlightsQueue(List<NetCache.BoosterCard> highlightsList, List<NetCache.BoosterCard> fillerList)
	{
		int numFillerNeeded = 5 - highlightsList.Count % 5;
		if (numFillerNeeded == 0)
		{
			return;
		}
		if (fillerList.Count < numFillerNeeded)
		{
			Log.All.PrintWarning("Ran out of filler cards, this should never happen");
			return;
		}
		fillerList.Sort(CompareFillerCards);
		for (int i = 0; i < numFillerNeeded; i++)
		{
			highlightsList.Add(fillerList[i]);
		}
	}

	public static int CompareFillerCards(NetCache.BoosterCard x, NetCache.BoosterCard y)
	{
		float xImportance = GetImportance(x);
		float yImportance = GetImportance(y);
		if (xImportance < yImportance)
		{
			return 1;
		}
		if (xImportance > yImportance)
		{
			return -1;
		}
		return 1;
	}

	public static float GetImportance(NetCache.BoosterCard card)
	{
		EntityDef entity = DefLoader.Get().GetEntityDef(card.Def.Name);
		TAG_RARITY rarity = entity.GetRarity();
		TAG_PREMIUM premium = card.Def.Premium;
		CollectibleCard collectibleCard = CollectionManager.Get().GetCard(entity.GetCardId(), premium);
		bool num = collectibleCard.SeenCount < 1 && collectibleCard.OwnedCount < 2;
		float importance = 1f;
		switch (rarity)
		{
		case TAG_RARITY.LEGENDARY:
			importance = 50f;
			break;
		case TAG_RARITY.EPIC:
			importance = 20f;
			break;
		case TAG_RARITY.RARE:
			importance = 5f;
			break;
		}
		switch (premium)
		{
		case TAG_PREMIUM.SIGNATURE:
			importance *= 10f;
			break;
		case TAG_PREMIUM.GOLDEN:
			importance *= 2f;
			break;
		case TAG_PREMIUM.NORMAL:
			importance *= 1f;
			break;
		}
		if (num)
		{
			return importance * 1.5f;
		}
		return importance * 1f;
	}

	private void OnDisable()
	{
		if (m_explosionCustomPass != null)
		{
			m_explosionCustomPass.Unschedule();
			CameraPassProvider.ReleasePass(m_explosionCustomPass);
			m_explosionCustomPass = null;
		}
	}
}
