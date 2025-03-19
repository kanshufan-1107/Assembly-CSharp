using System;
using System.Runtime.CompilerServices;

public class CurrencyBalanceChangedEventArgs : EventArgs
{
	[CompilerGenerated]
	private readonly long _003COldAmount_003Ek__BackingField;

	[CompilerGenerated]
	private readonly long _003CNewAmount_003Ek__BackingField;

	public CurrencyType Currency { get; }

	public CurrencyBalanceChangedEventArgs(CurrencyType type, long oldAmount, long newAmount)
	{
		Currency = type;
		_003COldAmount_003Ek__BackingField = oldAmount;
		_003CNewAmount_003Ek__BackingField = newAmount;
	}
}
