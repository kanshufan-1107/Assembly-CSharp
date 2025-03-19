using System;
using Blizzard.MobileAuth;
using Blizzard.T5.Core;
using Blizzard.Telemetry.WTCG.Client;
using HearthstoneTelemetry;

namespace Hearthstone.Login;

public class CreateGuestAccountStrategy : IAsyncMobileLoginStrategy
{
	private TokenPromise Promise { get; set; }

	private ILogger Logger { get; set; }

	private ITelemetryClient TelemetryClient { get; set; }

	public CreateGuestAccountStrategy(ILogger logger, ITelemetryClient telemetryClient)
	{
		Logger = logger;
		TelemetryClient = telemetryClient;
	}

	public bool MeetsRequirements(LoginStrategyParameters parameters)
	{
		if (PlatformSettings.LocaleVariant == LocaleVariant.China)
		{
			return false;
		}
		bool num = parameters.ChallengeUrl.Contains("externalChallenge=login", StringComparison.InvariantCulture);
		bool isNewUser = Options.Get().GetBool(Option.NEW_USER_LOGIN, defaultVal: false);
		return num && isNewUser;
	}

	public void StartExecution(LoginStrategyParameters parameters, TokenPromise promise)
	{
		Promise = promise;
		Logger?.Log(LogLevel.Information, "Attempting to create guest account");
		new GuestCreateAndAuthFlow(parameters.MobileAuth, Logger).Success(Authenticated).Cancelled(AuthenticationCancelled).Error(AuthenticationError)
			.Begin();
	}

	private void Authenticated(Account authenticatedAccount)
	{
		Options.Get().SetBool(Option.NEW_USER_LOGIN, val: false);
		Logger?.Log(LogLevel.Information, "Successfully created and authenticated new guest account {0}", authenticatedAccount.displayName);
		AdTrackingManager.Get()?.TrackHeadlessAccountCreated(authenticatedAccount.accountId);
		SendAuthResultTelemetry(MASDKAuthResult.AuthResult.SUCCESS);
		CreateSkipHelper.RequestShowCreateSkip();
		Promise.SetResult(TokenPromise.ResultType.Success, authenticatedAccount.authenticationToken);
	}

	private void AuthenticationCancelled()
	{
		Logger?.Log(LogLevel.Information, "New soft account authentication canceled");
		SendAuthResultTelemetry(MASDKAuthResult.AuthResult.CANCELED);
		Promise.SetResult(TokenPromise.ResultType.Canceled);
	}

	private void AuthenticationError(BlzMobileAuthError error)
	{
		Logger?.Log(LogLevel.Error, "Error authenticating new soft account: {0} {1} {2}", error.errorCode, error.errorContext, error.errorMessage);
		SendAuthResultTelemetry(MASDKAuthResult.AuthResult.FAILURE, error.errorCode);
		Promise.SetResult(TokenPromise.ResultType.Failure);
	}

	private void SendAuthResultTelemetry(MASDKAuthResult.AuthResult result, int errorCode = 0)
	{
		TelemetryClient?.SendMASDKAuthResult(result, errorCode, "CreateGuestAccountStrategy");
	}
}
