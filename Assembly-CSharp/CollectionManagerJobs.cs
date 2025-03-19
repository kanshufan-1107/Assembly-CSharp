using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Hearthstone.Core;

public class CollectionManagerJobs
{
	public static void OpenToCardPageWhenReady(string cardId, TAG_PREMIUM premium, bool craftingMode = false, bool craftingDialog = false)
	{
		Processor.QueueJob("CollectionManager.OpenToCardWhenReady", Job_OpenToCardPage(cardId, premium, craftingMode, craftingDialog));
	}

	private static IEnumerator<IAsyncJobResult> Job_OpenToCardPage(string cardId, TAG_PREMIUM premium, bool craftingMode, bool craftingDialog)
	{
		CollectionManager collectionManager = CollectionManager.Get();
		if (collectionManager == null)
		{
			yield return new JobFailedResult("[CollectionManager.OpenToCardPage] Cannot open card because CollectionManager is unavailable");
		}
		CollectibleCard card = collectionManager.GetCard(cardId, premium);
		if (card == null)
		{
			yield return new JobFailedResult("[CollectionManager.OpenToCardPage] Cannot open card because CollectibleCard(" + cardId + ") is unavailable");
		}
		SceneMgr sceneMgr = SceneMgr.Get();
		if (sceneMgr == null)
		{
			yield return new JobFailedResult("[CollectionManager.OpenToCardPage] Cannot open card because SceneMgr is unavailable");
		}
		while (sceneMgr.IsTransitioning())
		{
			yield return null;
		}
		if (sceneMgr.GetMode() != SceneMgr.Mode.COLLECTIONMANAGER)
		{
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.COLLECTIONMANAGER);
			if (sceneMgr.IsTransitioning())
			{
				yield return null;
			}
		}
		if (sceneMgr.GetMode() != SceneMgr.Mode.COLLECTIONMANAGER)
		{
			yield return new JobFailedResult("[CollectionManager.OpenToCardPage] Did not transition to the CollectionManager");
		}
		CollectionManagerDisplay managerDisplay = CollectionManager.Get().GetCollectibleDisplay() as CollectionManagerDisplay;
		if (managerDisplay == null)
		{
			yield return new JobFailedResult("[CollectionManager.OpenToCardPage] Cannot open card because CollectionManagerDisplay is unavailable");
		}
		while (!managerDisplay.IsReady())
		{
			yield return null;
		}
		while (!managerDisplay.IsBookOpened())
		{
			yield return null;
		}
		CollectionPageManager pageManager = managerDisplay.GetPageManager() as CollectionPageManager;
		if (pageManager == null)
		{
			yield return new JobFailedResult("[CollectionManager.OpenToCardPage] Cannot open card because CollectionPageManager is unavailable");
		}
		while (!pageManager.IsFullyLoaded())
		{
			yield return null;
		}
		if (craftingDialog || craftingMode)
		{
			managerDisplay.ShowCraftingTray(null, null, null, null, null);
		}
		managerDisplay.GoToPageWithCard(card.CardId, card.PremiumType);
		CollectionCardVisual cardVisual = pageManager.GetCardVisual(cardId, card.PremiumType);
		if (cardVisual == null)
		{
			List<TAG_CARD_SET> cardSets = new List<TAG_CARD_SET> { card.Set };
			pageManager.FilterByCardSets(cardSets);
			managerDisplay.GoToPageWithCard(card.CardId, card.PremiumType);
			cardVisual = pageManager.GetCardVisual(cardId, card.PremiumType);
			if (cardVisual == null)
			{
				yield return new JobFailedResult("[CollectionManager.OpenToCardPage] Cannot open card because CollectionCardVisual(" + cardId + ") is unavailable");
			}
		}
		if (craftingDialog)
		{
			CraftingManager cmgr = CraftingManager.Get();
			if (cmgr != null)
			{
				cmgr.EnterCraftMode(cardVisual.GetActor());
			}
		}
	}
}
