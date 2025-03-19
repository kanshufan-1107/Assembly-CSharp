using Hearthstone.UI;
using UnityEngine;

public class ShopTab : ShopTabBase
{
	[SerializeField]
	[Header("Tab Components")]
	private Clickable m_clickable;

	private const string GLUE_STORE_UNAVAILABLE_REASON_TITLE_LOCKED = "GLUE_STORE_UNAVAILABLE_REASON_TITLE_LOCKED";

	private const string GLUE_STORE_UNAVAILABLE_REASON_UNKNOWN = "GLUE_STORE_UNAVAILABLE_REASON_UNKNOWN";

	private const string GLUE_STORE_UNAVAILABLE_REASON_TUTORIAL = "GLUE_STORE_UNAVAILABLE_REASON_TUTORIAL";

	private const string GLUE_STORE_UNAVAILABLE_REASON_MISSING_ASSETS = "GLUE_STORE_UNAVAILABLE_REASON_MISSING_ASSETS";

	protected override void OnBlockChanged(bool blocked)
	{
		base.OnBlockChanged(blocked);
		m_clickable.Active = !blocked;
	}

	protected override void GetTooltipData(out string title, out string body)
	{
		title = string.Empty;
		body = string.Empty;
		if (base.CurrentData != null && base.CurrentData.Locked && (base.CurrentData.LockedReason != 0 || base.CurrentData.LockedReason != IGamemodeAvailabilityService.Status.READY))
		{
			title = GameStrings.Get("GLUE_STORE_UNAVAILABLE_REASON_TITLE_LOCKED");
			body = GameStrings.Format(GetLocStrFromGamemodeStatus(base.CurrentData.LockedReason), GetDisplayableStringForGamemodeName(base.CurrentData.LockedMode));
		}
	}

	private static string GetDisplayableStringForGamemodeName(IGamemodeAvailabilityService.Gamemode mode)
	{
		if (GamemodeAvailabilityService.TryGetGamemodeLocalizedString(mode, out var locStr))
		{
			return GameStrings.Get(locStr);
		}
		Log.Store.PrintError(string.Format("{0} failed to get gamemode localized string for mode {1}!", "ShopTab", mode));
		return string.Empty;
	}

	private static string GetLocStrFromGamemodeStatus(IGamemodeAvailabilityService.Status status)
	{
		switch (status)
		{
		case IGamemodeAvailabilityService.Status.READY:
			return string.Empty;
		case IGamemodeAvailabilityService.Status.WAITING_FOR_TUTORIAL:
		case IGamemodeAvailabilityService.Status.TUTORIAL_INCOMPLETE:
			return "GLUE_STORE_UNAVAILABLE_REASON_TUTORIAL";
		case IGamemodeAvailabilityService.Status.NOT_DOWNLOADED:
			return "GLUE_STORE_UNAVAILABLE_REASON_MISSING_ASSETS";
		default:
			return "GLUE_STORE_UNAVAILABLE_REASON_UNKNOWN";
		}
	}
}
