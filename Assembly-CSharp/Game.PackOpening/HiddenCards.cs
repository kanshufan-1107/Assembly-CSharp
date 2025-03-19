using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hearthstone.Extensions;
using PegasusLettuce;
using Shared.UI.Scripts.Carousel;
using UnityEngine;

namespace Game.PackOpening;

public class HiddenCards
{
	protected List<PackOpeningCard> m_cards = new List<PackOpeningCard>();

	public int Count => m_cards.Count;

	public event EventHandler OnCardRevealedEvent;

	public event EventHandler OnCardSpellFinishedEvent;

	public event EventHandler<Spell> OnCardSpellStateFinishedEvent;

	public void InitializeCards(PackOpeningCard template, int numCards)
	{
		CardBackManager.Get().LoadRandomCardBackIntoFavoriteSlot(updateScene: true);
		CreateCards(template, numCards);
		SetUpCards();
	}

	public void DeactivateCards(int startingIndex = 0)
	{
		foreach (var (card, index) in m_cards.WithIndex())
		{
			if (index >= startingIndex)
			{
				card.name = $"DisabledCard_Hidden{index + 1}";
				card.gameObject.SetActive(value: false);
			}
		}
	}

	public void ActivateCards(int startingIndex = 0)
	{
		foreach (var (card, index) in m_cards.WithIndex())
		{
			if (index >= startingIndex)
			{
				card.name = $"Card_Hidden{index + 1}";
				card.gameObject.SetActive(value: true);
			}
		}
	}

	public void SetCardsPosition(Vector3 newPosition, int startingIndex = 0)
	{
		foreach (var (card, index) in m_cards.WithIndex())
		{
			if (index >= startingIndex)
			{
				card.name = $"DisabledCard_Hidden{index + 1}";
				card.gameObject.transform.position = newPosition;
			}
		}
	}

	public void Dissipate()
	{
		foreach (var item in m_cards.WithIndex())
		{
			PackOpeningCard card = item.item;
			card.Dissipate();
		}
	}

	public void EnableCollision()
	{
		foreach (PackOpeningCard card in m_cards)
		{
			card.GetComponent<Collider>().enabled = true;
		}
	}

	public void SetInputEnabled(bool enable)
	{
		foreach (PackOpeningCard card in m_cards)
		{
			card.EnableInput(enable);
		}
	}

	public void EnableReveal()
	{
		foreach (PackOpeningCard card in m_cards)
		{
			card.EnableReveal(enable: true);
		}
	}

	public void ShowRarityGlow()
	{
		foreach (PackOpeningCard card in m_cards)
		{
			card.ShowRarityGlow();
		}
	}

	public IEnumerable<Carousel.Item> ToCarouselItems()
	{
		return m_cards.Select((PackOpeningCard card) => new PackOpeningCardCarouselItem(card));
	}

	public void ForceRevealRandomCard()
	{
		PackOpeningCard[] cardsToReveal = m_cards.Where(CardCanBeRevealed).ToArray();
		if (cardsToReveal.Length != 0)
		{
			int hiddenCardIndex = UnityEngine.Random.Range(0, cardsToReveal.Length);
			cardsToReveal[hiddenCardIndex].ForceReveal();
		}
	}

	public void ForceRevealAllCards()
	{
		m_cards.Sort(PackOpeningCard.ComparePackOpeningCards);
		bool hasRevealedFirstCard = false;
		foreach (PackOpeningCard card in m_cards)
		{
			if (!card.IsRevealed())
			{
				if (!hasRevealedFirstCard)
				{
					card.ForceReveal();
					hasRevealedFirstCard = true;
				}
				else
				{
					card.ForceReveal(suppressVO: true);
				}
			}
		}
	}

	public IEnumerator AttachBoosterCards(List<NetCache.BoosterCard> items)
	{
		foreach (var (card2, item2) in m_cards.Zip(items, (PackOpeningCard card, NetCache.BoosterCard item) => (card: card, item: item)))
		{
			yield return null;
			card2.AttachBoosterCard(item2);
		}
		SetHiddenCardMeshVisible();
	}

	public IEnumerator AttachBoosterMercenaries(List<LettucePackComponent> items)
	{
		items = items.OrderBy((LettucePackComponent x) => Guid.NewGuid()).Take(items.Count).ToList();
		foreach (var (card2, item2) in m_cards.Zip(items, (PackOpeningCard card, LettucePackComponent item) => (card: card, item: item)))
		{
			yield return null;
			card2.AttachBoosterMercenary(item2);
		}
	}

	public void RemoveOnOverWhileFlippedListeners()
	{
		foreach (PackOpeningCard card in m_cards)
		{
			card.RemoveOnOverWhileFlippedListeners();
		}
	}

	private static bool CardCanBeRevealed(PackOpeningCard card)
	{
		if (card.IsReady() && card.IsRevealEnabled())
		{
			return !card.IsRevealed();
		}
		return false;
	}

	protected void CreateCards(PackOpeningCard prefab, int numCards)
	{
		m_cards.Add(prefab);
		prefab.ClearEventListeners();
		for (int index = 1; index < numCards; index++)
		{
			PackOpeningCard card = UnityEngine.Object.Instantiate(prefab.gameObject).GetComponent<PackOpeningCard>();
			card.transform.parent = prefab.transform.parent;
			TransformUtil.CopyLocal(card, prefab);
			m_cards.Add(card);
		}
	}

	protected void SetUpCards()
	{
		foreach (var item in m_cards.WithIndex())
		{
			PackOpeningCard card = item.item;
			int index = item.index;
			card.name = $"Card_Hidden{index + 1}";
			card.SetCardNumber(index + 1);
			card.EnableInput(enable: false);
			card.AddRevealedListener(OnCardRevealed);
			card.OnSpellFinishedEvent += this.OnCardSpellFinishedEvent;
			card.OnSpellStateFinishedEvent += this.OnCardSpellStateFinishedEvent;
		}
		void OnCardRevealed(object sender)
		{
			this.OnCardRevealedEvent?.Invoke(sender, EventArgs.Empty);
		}
	}

	public void SetHiddenCardMeshVisible()
	{
		foreach (PackOpeningCard card in m_cards)
		{
			LayerUtils.SetLayer(card.gameObject, GameLayer.IgnoreFullScreenEffects);
		}
	}
}
