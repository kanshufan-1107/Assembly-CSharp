using System;

namespace Hearthstone.Login;

public static class CreateSkipHelper
{
	public static bool ShouldShowSkipScreenAtBox { get; private set; }

	public static void QueueSkipScreenAtBox()
	{
		if (TemporaryAccountManager.IsTemporaryAccount())
		{
			ShouldShowSkipScreenAtBox = true;
		}
	}

	public static bool IsCreateSkipScreenSupported()
	{
		return PlatformSettings.IsMobileRuntimeOS;
	}

	public static bool ShouldShowCreateSkip()
	{
		if (TemporaryAccountManager.IsTemporaryAccount() && IsCreateSkipScreenSupported())
		{
			return Options.Get().GetBool(Option.SHOW_CREATE_SKIP_ACCT, defaultVal: false);
		}
		return false;
	}

	public static void RequestShowCreateSkip()
	{
		if (IsCreateSkipScreenSupported())
		{
			Options.Get().SetBool(Option.SHOW_CREATE_SKIP_ACCT, val: true);
		}
	}

	public static void ClearShowCreateSkip()
	{
		Options.Get().SetBool(Option.SHOW_CREATE_SKIP_ACCT, val: false);
	}

	public static bool ShowCreateSkipDialog(Action onCanceled)
	{
		Log.Login.PrintInfo("Requesting showing create/Skip");
		TemporaryAccountSignUpPopUp.PopupTextParameters popupTextParameters = default(TemporaryAccountSignUpPopUp.PopupTextParameters);
		popupTextParameters.Header = "GLUE_TEMPORARY_ACCOUNT_DIALOG_HEADER_02";
		popupTextParameters.Body = "GLUE_TEMPORARY_ACCOUNT_DIALOG_BODY_07";
		popupTextParameters.CancelButton = "GLUE_TEMPORARY_ACCOUNT_SKIP";
		TemporaryAccountSignUpPopUp.PopupTextParameters popupParams = popupTextParameters;
		bool num = TemporaryAccountManager.Get().ShowHealUpDialog(popupParams, TemporaryAccountManager.HealUpReason.UNKNOWN, userTriggered: true, delegate
		{
			onCanceled?.Invoke();
		});
		if (num)
		{
			ClearShowCreateSkip();
			ShouldShowSkipScreenAtBox = false;
		}
		return num;
	}
}
