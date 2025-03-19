using System;
using System.Collections.Generic;
using Blizzard.MobileAuth;

namespace Hearthstone.Login;

internal class MobileAuthAdapter : IMobileAuth
{
	public bool IsAuthenticated => MobileAuth.IsAuthenticated;

	public void AuthenticateSoftAccount(GuestSoftAccountId softAccountId, IGuestSoftAccountCallback callback)
	{
		MobileAuth.AuthenticateSoftAccount(softAccountId, callback);
	}

	public void GenerateGuestSoftAccount(IGuestSoftAccountCallback callback)
	{
		MobileAuth.GenerateGuestSoftAccount(callback);
	}

	public void GenerateGuestSoftAccount(string queueStamp, IGuestSoftAccountCallback callback)
	{
		MobileAuth.GenerateGuestSoftAccount(queueStamp, callback);
	}

	public Account? GetAuthenticatedAccount()
	{
		return MobileAuth.GetAuthenticatedAccount();
	}

	public List<Account> GetSoftAccounts()
	{
		List<Account> accounts;
		try
		{
			accounts = MobileAuth.GetSoftAccounts();
		}
		catch (Exception ex)
		{
			Log.Login.PrintDebug("Unexepcted exceception from mobile auth {0}:\n{1}", ex.Message, ex.StackTrace);
			accounts = new List<Account>();
		}
		if (accounts == null)
		{
			Log.Login.PrintDebug("Unexpected null soft account list");
			accounts = new List<Account>();
		}
		return accounts;
	}

	public void AuthenticateAccount(Account account, AuthenticationCallback authenticationCallback)
	{
		MobileAuth.SetAuthenticatedAccount(account.accountId, authenticationCallback);
	}

	public void ImportGuestAccount(string guestId, IImportAccountCallback importCallback)
	{
		MobileAuth.ImportAccount(ImportAccount.CreateWithSoftAccountBnetGuestID(guestId), importCallback);
	}

	public void StartHealUpFlow(Account account, IAuthenticationFlowCallback authenticationFlowCallback)
	{
		MobileAuth.StartHealUpFlow(account, authenticationFlowCallback);
	}

	public void StartMergeFlow(Account account, IAuthenticationFlowCallback authenticationFlowCallback)
	{
		MobileAuth.StartMergeFlow(account, authenticationFlowCallback, showMergeSummary: true);
	}

	public void StartWebQueueFlow(QueueInfo info, IWebQueueFlowCallback callback)
	{
		MobileAuth.StartWebQueueFlow(info, callback);
	}

	public void RemoveAccountById(string accountId, IOnAccountRemovedCallback removedCallback)
	{
		MobileAuth.RemoveAccountById(accountId, removedCallback);
	}
}
