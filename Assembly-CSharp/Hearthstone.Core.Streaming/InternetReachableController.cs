using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Blizzard.T5.Services;

namespace Hearthstone.Core.Streaming;

public class InternetReachableController
{
	private readonly Dictionary<string, DownloadModeForInternetUnreachable> m_availableModes = new Dictionary<string, DownloadModeForInternetUnreachable>
	{
		{
			"apk",
			DownloadModeForInternetUnreachable.APK_DOWNLOAD
		},
		{
			"i",
			DownloadModeForInternetUnreachable.INITIAL_DOWNLOAD
		},
		{
			"o",
			DownloadModeForInternetUnreachable.OPTIONAL_DOWNLOAD
		},
		{
			"li",
			DownloadModeForInternetUnreachable.LOCALE_INITIAL_DOWNLOAD
		},
		{
			"lo",
			DownloadModeForInternetUnreachable.LOCALE_OPTIONAL_DOWNLOAD
		}
	};

	private string m_optionOverride;

	[CompilerGenerated]
	private DownloadModeForInternetUnreachable _003CMode_003Ek__BackingField = DownloadModeForInternetUnreachable.NONE;

	[CompilerGenerated]
	private float _003CDuration_003Ek__BackingField;

	[CompilerGenerated]
	private long _003CBytes_003Ek__BackingField;

	private NetworkReachabilityManager m_networkReachabilityManager;

	protected string OptionStorage
	{
		get
		{
			if (string.IsNullOrEmpty(m_optionOverride))
			{
				return Options.Get().GetString(Option.INTERNET_UNREACHABLE);
			}
			return m_optionOverride;
		}
	}

	public bool InternetAvailable => m_networkReachabilityManager?.InternetAvailable_Cached ?? NetworkReachabilityManager.InternetAvailable;

	protected DownloadModeForInternetUnreachable Mode
	{
		[CompilerGenerated]
		set
		{
			_003CMode_003Ek__BackingField = value;
		}
	}

	protected float Duration
	{
		[CompilerGenerated]
		set
		{
			_003CDuration_003Ek__BackingField = value;
		}
	}

	protected long Bytes
	{
		[CompilerGenerated]
		set
		{
			_003CBytes_003Ek__BackingField = value;
		}
	}

	public bool Initialize()
	{
		SetModeDuration();
		return ServiceManager.TryGet<NetworkReachabilityManager>(out m_networkReachabilityManager);
	}

	protected long ConvertBytesFromMB(int mbValue)
	{
		return mbValue * 1000000;
	}

	protected bool SetModeDuration()
	{
		string unreachableSettingStr = OptionStorage;
		Mode = DownloadModeForInternetUnreachable.NONE;
		Duration = 5f;
		Bytes = ConvertBytesFromMB(5);
		if (string.IsNullOrEmpty(unreachableSettingStr))
		{
			return false;
		}
		string[] values = unreachableSettingStr.Split('|');
		if (!m_availableModes.TryGetValue(values[0], out var mode))
		{
			Log.Downloader.PrintError("Unknown mode string: " + values[0]);
			return false;
		}
		float duration = 5f;
		if (values.Length >= 2 && !float.TryParse(values[1], out duration))
		{
			Log.Downloader.PrintError("Invalid duration value: " + values[1]);
			return false;
		}
		if (duration < 5f)
		{
			Log.Downloader.PrintInfo("The duration value is too small({0}), set the duration to default '{1}'", duration, 5f);
			duration = 5f;
		}
		int mbytes = 5;
		if (values.Length >= 3 && !int.TryParse(values[2], out mbytes))
		{
			Log.Downloader.PrintError("Invalid downloaded bytes value: " + values[2]);
			return false;
		}
		if (mbytes < 5)
		{
			Log.Downloader.PrintInfo("The downloaded bytes is too small({0}), set the downloaded btyes to default '{1}'MB", mbytes, 5);
			mbytes = 5;
		}
		Log.Downloader.PrintInfo("Internet unreachable setting: {0} {1} seconds after downloading {2}MB", mode, duration, mbytes);
		Mode = mode;
		Duration = duration;
		Bytes = ConvertBytesFromMB(mbytes);
		return true;
	}
}
