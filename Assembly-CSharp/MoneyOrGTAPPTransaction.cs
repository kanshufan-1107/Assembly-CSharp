using PegasusShared;

public class MoneyOrGTAPPTransaction
{
	public static readonly BattlePayProvider? UNKNOWN_PROVIDER;

	public const int LOCKED_THIRD_PARTY_QUANTITY = 1;

	public long ID { get; }

	public long? PMTProductID { get; }

	public bool IsGTAPP { get; }

	public BattlePayProvider? Provider { get; }

	public bool ClosedStore { get; set; }

	public MoneyOrGTAPPTransaction(long id, long? pmtProductID, BattlePayProvider? provider, bool isGTAPP)
	{
		ID = id;
		PMTProductID = pmtProductID;
		IsGTAPP = isGTAPP;
		Provider = provider;
		ClosedStore = false;
	}

	public override int GetHashCode()
	{
		return ID.GetHashCode() * PMTProductID.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is MoneyOrGTAPPTransaction other))
		{
			return false;
		}
		bool providersMatch = false;
		providersMatch = !Provider.HasValue || !other.Provider.HasValue || Provider.Value == other.Provider.Value;
		return other.ID == ID && other.PMTProductID == PMTProductID && providersMatch;
	}

	public override string ToString()
	{
		return string.Format("[MoneyOrGTAPPTransaction: ID={0}, PmtProductID='{1}', IsGTAPP={2}, Provider={3}]", ID, PMTProductID, IsGTAPP, Provider.HasValue ? Provider.Value.ToString() : "UNKNOWN");
	}

	public bool ShouldShowMiniSummary()
	{
		if (StoreManager.HasExternalStore)
		{
			return true;
		}
		return ClosedStore;
	}
}
