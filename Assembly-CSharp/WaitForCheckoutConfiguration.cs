using Blizzard.T5.Jobs;
using Blizzard.T5.Services;

public class WaitForCheckoutConfiguration : IJobDependency, IAsyncJobResult
{
	public bool IsReady()
	{
		if (ServiceManager.TryGet<HearthstoneCheckout>(out var hearthstoneCheckout))
		{
			if (hearthstoneCheckout.HasProductCatalog && hearthstoneCheckout.HasClientID)
			{
				return hearthstoneCheckout.HasCurrencyCode;
			}
			return false;
		}
		return false;
	}
}
