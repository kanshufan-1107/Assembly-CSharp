using System.Collections.Generic;
using Blizzard.MobileAuth;

namespace Hearthstone.Login;

public interface IMobileAuth
{
	bool IsAuthenticated { get; }

	Account? GetAuthenticatedAccount();

	void AuthenticateSoftAccount(GuestSoftAccountId softAccountId, IGuestSoftAccountCallback callback);

	void GenerateGuestSoftAccount(IGuestSoftAccountCallback callback);

	void GenerateGuestSoftAccount(string queueStamp, IGuestSoftAccountCallback callback);

	List<Account> GetSoftAccounts();

	void ImportGuestAccount(string guestId, IImportAccountCallback importCallback);

	void AuthenticateAccount(Account account, AuthenticationCallback authenticationCallback);

	void StartHealUpFlow(Account account, IAuthenticationFlowCallback authenticationFlowCallback);

	void RemoveAccountById(string accountId, IOnAccountRemovedCallback removedListener);

	void StartMergeFlow(Account account, IAuthenticationFlowCallback authenticationFlowCallback);

	void StartWebQueueFlow(QueueInfo info, IWebQueueFlowCallback callback);
}
