using System;

public class DownloadPermissionManager
{
	private static bool s_cellularEnabledSession;

	public static bool CellularEnabled
	{
		get
		{
			return s_cellularEnabledSession;
		}
		set
		{
			s_cellularEnabledSession = value;
		}
	}

	public static bool DownloadEnabled
	{
		get
		{
			return Options.Get().GetBool(Option.ASSET_DOWNLOAD_ENABLED);
		}
		set
		{
			Options.Get().SetBool(Option.ASSET_DOWNLOAD_ENABLED, value);
		}
	}

	public static event Action OnCellularPermissionChanged;
}
