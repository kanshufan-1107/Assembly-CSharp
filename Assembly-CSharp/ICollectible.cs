using System;
using System.Collections.Generic;

public interface ICollectible : IComparable
{
	int OwnedCount { get; }

	bool IsNewCollectible { get; }

	HashSet<string> GetSearchableTokens();

	SearchableString GetSearchableString();
}
