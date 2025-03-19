using System.Collections.Generic;

public class CraftingPendingTransaction
{
	public enum Operation
	{
		Invalid,
		NormalDisenchant,
		NormalCreate,
		GoldenDisenchant,
		GoldenCreate,
		UpgradeToGoldenFromNormal,
		UpgradeToGoldenFromNothing,
		UpgradeToGoldenFromGolden,
		SignatureDisenchant,
		DiamondDisenchant
	}

	public string CardID;

	public TAG_PREMIUM Premium;

	public bool CardValueOverridden;

	public int NormalDisenchantCount;

	public int NormalCreateCount;

	public int GoldenDisenchantCount;

	public int GoldenCreateCount;

	public int GoldenUpgradeFromNormalCount;

	public int GoldenUpgradeFromNothingCount;

	public int SignatureDisenchantCount;

	public int DiamondDisenchantCount;

	private Stack<Operation> m_transactionOrder = new Stack<Operation>();

	public Operation Undo()
	{
		if (m_transactionOrder == null)
		{
			return Operation.Invalid;
		}
		if (m_transactionOrder.Count > 0)
		{
			Operation top = m_transactionOrder.Pop();
			switch (top)
			{
			case Operation.NormalDisenchant:
				NormalDisenchantCount--;
				break;
			case Operation.NormalCreate:
				NormalCreateCount--;
				break;
			case Operation.GoldenDisenchant:
				GoldenDisenchantCount--;
				break;
			case Operation.GoldenCreate:
				GoldenCreateCount--;
				break;
			case Operation.UpgradeToGoldenFromNormal:
			case Operation.UpgradeToGoldenFromGolden:
				GoldenUpgradeFromNormalCount--;
				break;
			case Operation.UpgradeToGoldenFromNothing:
				GoldenUpgradeFromNothingCount--;
				break;
			case Operation.SignatureDisenchant:
				SignatureDisenchantCount--;
				break;
			case Operation.DiamondDisenchant:
				DiamondDisenchantCount--;
				break;
			}
			return top;
		}
		return Operation.Invalid;
	}

	public void Add(Operation transaction)
	{
		if (m_transactionOrder != null)
		{
			switch (transaction)
			{
			case Operation.NormalDisenchant:
				NormalDisenchantCount++;
				break;
			case Operation.NormalCreate:
				NormalCreateCount++;
				break;
			case Operation.GoldenDisenchant:
				GoldenDisenchantCount++;
				break;
			case Operation.GoldenCreate:
				GoldenCreateCount++;
				break;
			case Operation.UpgradeToGoldenFromNormal:
			case Operation.UpgradeToGoldenFromGolden:
				GoldenUpgradeFromNormalCount++;
				break;
			case Operation.UpgradeToGoldenFromNothing:
				GoldenUpgradeFromNothingCount++;
				break;
			case Operation.SignatureDisenchant:
				SignatureDisenchantCount++;
				break;
			case Operation.DiamondDisenchant:
				DiamondDisenchantCount++;
				break;
			}
			m_transactionOrder.Push(transaction);
		}
	}

	public CraftingPendingTransaction ShallowCopy()
	{
		return MemberwiseClone() as CraftingPendingTransaction;
	}

	public int GetTransactionAmount(TAG_PREMIUM premium)
	{
		return premium switch
		{
			TAG_PREMIUM.NORMAL => NormalCreateCount - (NormalDisenchantCount + GoldenUpgradeFromNormalCount), 
			TAG_PREMIUM.GOLDEN => GoldenCreateCount + GoldenUpgradeFromNormalCount + GoldenUpgradeFromNothingCount - GoldenDisenchantCount, 
			TAG_PREMIUM.SIGNATURE => -SignatureDisenchantCount, 
			TAG_PREMIUM.DIAMOND => -DiamondDisenchantCount, 
			_ => 0, 
		};
	}

	public bool HasPendingTransactions()
	{
		return m_transactionOrder.Count != 0;
	}

	public void ResetTransactionAmount()
	{
		NormalDisenchantCount = 0;
		NormalCreateCount = 0;
		GoldenDisenchantCount = 0;
		GoldenCreateCount = 0;
		GoldenUpgradeFromNormalCount = 0;
		GoldenUpgradeFromNothingCount = 0;
		SignatureDisenchantCount = 0;
		DiamondDisenchantCount = 0;
	}

	public int GetExpectedTransactionCost(string cardId)
	{
		NetCache.CardValue normalValue = CraftingManager.GetCardValue(cardId, TAG_PREMIUM.NORMAL);
		NetCache.CardValue goldenValue = CraftingManager.GetCardValue(cardId, TAG_PREMIUM.GOLDEN);
		NetCache.CardValue signatureValue = CraftingManager.GetCardValue(cardId, TAG_PREMIUM.SIGNATURE);
		NetCache.CardValue diamondValue = CraftingManager.GetCardValue(cardId, TAG_PREMIUM.DIAMOND);
		if (normalValue == null || goldenValue == null || signatureValue == null || diamondValue == null)
		{
			return 0;
		}
		return -(NormalDisenchantCount * normalValue.GetSellValue()) - GoldenDisenchantCount * goldenValue.GetSellValue() + NormalCreateCount * normalValue.GetBuyValue() + GoldenCreateCount * goldenValue.GetBuyValue() + GoldenUpgradeFromNormalCount * normalValue.GetUpgradeValue() + GoldenUpgradeFromNothingCount * (normalValue.GetBuyValue() + normalValue.GetUpgradeValue()) - SignatureDisenchantCount * signatureValue.GetSellValue() - DiamondDisenchantCount * diamondValue.GetSellValue();
	}

	public bool GetLastTransactionWasDisenchant()
	{
		if (m_transactionOrder == null || m_transactionOrder.Count == 0)
		{
			return false;
		}
		Operation lastOperation = GetLastOperation();
		if (lastOperation != Operation.NormalDisenchant && lastOperation != Operation.GoldenDisenchant && lastOperation != Operation.SignatureDisenchant)
		{
			return lastOperation == Operation.DiamondDisenchant;
		}
		return true;
	}

	public bool GetLastTransactionWasCrafting()
	{
		if (m_transactionOrder == null || m_transactionOrder.Count == 0)
		{
			return false;
		}
		Operation lastOperation = GetLastOperation();
		if (lastOperation != Operation.NormalDisenchant && lastOperation != Operation.GoldenDisenchant && lastOperation != Operation.SignatureDisenchant)
		{
			return lastOperation != Operation.DiamondDisenchant;
		}
		return false;
	}

	public bool GetLastTransactionWasUpgrade()
	{
		if (m_transactionOrder == null || m_transactionOrder.Count == 0)
		{
			return false;
		}
		Operation lastOperation = m_transactionOrder.Peek();
		if (lastOperation != Operation.UpgradeToGoldenFromNormal && lastOperation != Operation.UpgradeToGoldenFromNothing)
		{
			return lastOperation == Operation.UpgradeToGoldenFromGolden;
		}
		return true;
	}

	public Operation GetLastOperation()
	{
		if (m_transactionOrder == null || m_transactionOrder.Count == 0)
		{
			return Operation.Invalid;
		}
		return m_transactionOrder.Peek();
	}
}
