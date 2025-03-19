using System;
using System.Collections.Generic;
using System.Linq;
using Blizzard.MobileAuth;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.Telemetry.WTCG.Client;
using Hearthstone.Core;
using HearthstoneTelemetry;

namespace Hearthstone.Login;

public class LegacyCloudGuestAccountStrategy : IAsyncMobileLoginStrategy
{
	private class ImportCloudAccounts : IJobDependency, IAsyncJobResult
	{
		private readonly IEnumerator<GuestAccountInfo> m_accountsEnumerator;

		private bool m_readyToProcess = true;

		private IMobileAuth MobileAuth { get; set; }

		private ILogger Logger { get; set; }

		private ITelemetryClient TelemetryClient { get; set; }

		public ImportCloudAccounts(ILegacyGuestAccountStorage accountStorage, IMobileAuth mobileAuth, ILogger logger, ITelemetryClient telemetryClient)
		{
			Logger = logger;
			MobileAuth = mobileAuth;
			TelemetryClient = telemetryClient;
			IEnumerable<GuestAccountInfo> accounts = accountStorage.GetStoredGuestAccounts();
			m_accountsEnumerator = accounts?.GetEnumerator();
			Logger?.Log(LogLevel.Information, "Got {0} stored accounts to import", accounts?.Count());
		}

		public bool IsReady()
		{
			if (!m_readyToProcess)
			{
				return false;
			}
			if (m_accountsEnumerator.MoveNext())
			{
				ProcessNextAccount();
				return false;
			}
			return true;
		}

		private void ProcessNextAccount()
		{
			GuestAccountInfo account = m_accountsEnumerator.Current;
			m_readyToProcess = false;
			ImportCallbackWrapper callbackWrapper = new ImportCallbackWrapper(delegate(bool success)
			{
				Logger?.Log(LogLevel.Debug, "Got callback import result {0}", success);
				m_readyToProcess = true;
			}, Logger, TelemetryClient);
			Logger?.Log(LogLevel.Debug, "Attempting import account of id {0} : {1}", account.GuestId, account.RegionId);
			MobileAuth.ImportGuestAccount(account.GuestId, callbackWrapper);
		}
	}

	private class ImportCallbackWrapper : IImportAccountCallback
	{
		private ILogger Logger { get; set; }

		private ITelemetryClient TelemetryClient { get; set; }

		private Action<bool> ResultCallback { get; set; }

		public ImportCallbackWrapper(Action<bool> callback, ILogger logger, ITelemetryClient telemetryClient)
		{
			Logger = logger;
			ResultCallback = callback;
			TelemetryClient = telemetryClient;
		}

		public void OnImportAccountError(BlzMobileAuthError error)
		{
			Logger?.Log(LogLevel.Error, "Import error [{0}] {1}\nMessage: {2}", error.errorCode, error.errorContext, error.errorMessage);
			SendTelemetryResult(MASDKImportResult.ImportResult.FAILURE, error.errorCode);
			ResultCallback?.Invoke(obj: false);
		}

		public void OnImportAccountSuccess(Account account)
		{
			Logger?.Log(LogLevel.Information, "Import success for account {0}", account.displayName);
			SendTelemetryResult(MASDKImportResult.ImportResult.SUCCESS);
			ResultCallback?.Invoke(obj: true);
		}

		private void SendTelemetryResult(MASDKImportResult.ImportResult result, int errorCode = 0)
		{
			TelemetryClient?.SendMASDKImportResult(result, MASDKImportResult.ImportType.GUEST_ACCOUNT_ID, errorCode);
		}
	}

	private ILegacyGuestAccountStorage AccountStorage { get; }

	private ISwitchAccountMenuController SwitchAccountController { get; }

	private ILogger Logger { get; }

	private ITelemetryClient TelemetryClient { get; }

	private TokenPromise Promise { get; set; }

	public LegacyCloudGuestAccountStrategy(ILegacyGuestAccountStorage accountStorage, ISwitchAccountMenuController switchAccountMenuController, ILogger logger, ITelemetryClient telemetryClient)
	{
		AccountStorage = accountStorage;
		SwitchAccountController = switchAccountMenuController;
		Logger = logger;
		TelemetryClient = telemetryClient;
	}

	public bool MeetsRequirements(LoginStrategyParameters parameters)
	{
		IEnumerable<GuestAccountInfo> storedGuestAccounts = AccountStorage.GetStoredGuestAccounts();
		if (storedGuestAccounts == null)
		{
			return false;
		}
		return storedGuestAccounts.Count() > 0;
	}

	public void StartExecution(LoginStrategyParameters parameters, TokenPromise promise)
	{
		Promise = promise;
		Logger?.Log(LogLevel.Debug, "Starting Legacy guest account import job");
		Processor.QueueJob("ImportAndSelectGuestAccount", Job_ImportAndPresentOptions(parameters.MobileAuth));
	}

	private IEnumerator<IAsyncJobResult> Job_ImportAndPresentOptions(IMobileAuth mobileAuth)
	{
		IEnumerable<GuestAccountInfo> storedGuestAccounts = AccountStorage.GetStoredGuestAccounts();
		if (storedGuestAccounts != null && storedGuestAccounts.Count() == 0)
		{
			Logger?.Log(LogLevel.Information, "No soft accounts found to import, failing the legacy guest account import");
			Promise.SetResult(TokenPromise.ResultType.Failure);
			yield break;
		}
		yield return new ImportCloudAccounts(AccountStorage, mobileAuth, Logger, TelemetryClient);
		string selectedGuestId = AccountStorage.GetSelectedGuestAccountId();
		AccountStorage.ClearGuestAccounts();
		List<Account> authAccounts = mobileAuth.GetSoftAccounts();
		Logger?.Log(LogLevel.Information, "Got {0} soft accounts from mobile auth", authAccounts?.Count);
		if (authAccounts == null || authAccounts.Count == 0)
		{
			Logger?.Log(LogLevel.Information, "No soft accounts found, failing the legacy guest account import");
			Promise.SetResult(TokenPromise.ResultType.Failure);
			yield break;
		}
		foreach (Account account in authAccounts)
		{
			if (account.bnetGuestId.Equals(selectedGuestId))
			{
				Logger?.Log(LogLevel.Information, "Found a selected guest account, authenticating it");
				AuthenticateAccount(mobileAuth, account);
				yield break;
			}
		}
		ShowAccountSelector(mobileAuth, authAccounts);
	}

	private void ShowAccountSelector(IMobileAuth mobileAuth, List<Account> authAccounts)
	{
		Logger?.Log(LogLevel.Information, "Showing switch account selection for {0} accounts", authAccounts.Count);
		SwitchAccountController.ShowSwitchAccount(authAccounts, delegate(Account? selectedAccount)
		{
			if (!selectedAccount.HasValue)
			{
				Logger?.Log(LogLevel.Information, "Login chosen for switch account. Failing so we can show login");
				Promise.SetResult(TokenPromise.ResultType.Failure);
			}
			else
			{
				AuthenticateAccount(mobileAuth, selectedAccount.Value);
			}
		});
	}

	private void AuthenticateAccount(IMobileAuth mobileAuth, Account selectedAccount)
	{
		Logger?.Log(LogLevel.Information, "Attempting to authenticate account {0}", selectedAccount.displayName);
		mobileAuth.AuthenticateAccount(selectedAccount, HandleAuthenticationCallback);
	}

	private void HandleAuthenticationCallback(Account? account, BlzMobileAuthError? error)
	{
		if (account.HasValue)
		{
			Authenticated(account.Value);
		}
		else if (error.HasValue)
		{
			AuthenticationError(error.Value);
		}
		else
		{
			AuthenticationCancelled();
		}
	}

	private void Authenticated(Account authenticatedAccount)
	{
		Logger?.Log(LogLevel.Information, "Successfully authenticated account {0}", authenticatedAccount.displayName);
		SendAuthResultTelemetry(MASDKAuthResult.AuthResult.SUCCESS);
		Promise.SetResult(TokenPromise.ResultType.Success, authenticatedAccount.authenticationToken);
	}

	private void AuthenticationCancelled()
	{
		Logger?.Log(LogLevel.Information, "Authentication Cancelled");
		SendAuthResultTelemetry(MASDKAuthResult.AuthResult.CANCELED);
		Promise.SetResult(TokenPromise.ResultType.Canceled);
	}

	public void AuthenticationError(BlzMobileAuthError error)
	{
		Logger?.Log(LogLevel.Error, "Error authenticating account [{0}] {1}\nMessage: {2}", error.errorCode, error.errorContext, error.errorMessage);
		SendAuthResultTelemetry(MASDKAuthResult.AuthResult.FAILURE, error.errorCode);
		Promise.SetResult(TokenPromise.ResultType.Failure);
	}

	private void SendAuthResultTelemetry(MASDKAuthResult.AuthResult result, int errorCode = 0)
	{
		TelemetryClient?.SendMASDKAuthResult(result, errorCode, "LegacyCloudGuestAccount");
	}
}
