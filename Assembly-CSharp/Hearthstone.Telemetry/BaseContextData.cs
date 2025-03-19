using System;
using Blizzard.T5.Configuration;
using Hearthstone.Util;
using UnityEngine;

namespace Hearthstone.Telemetry;

internal abstract class BaseContextData
{
	private string m_programVersion;

	private ulong? m_battleNetId;

	private int? m_battleNetRegion;

	private bool m_telemetryDisabled;

	private Uri m_ingestUri;

	private TelemetryConnectionType m_connectionType;

	private string m_currentTelemetryHost;

	private string ProductionUrl
	{
		get
		{
			if (!RegionUtils.IsCNLegalRegion)
			{
				return "https://telemetry-in.battle.net";
			}
			return "https://telemetry-in.battlenet.com.cn";
		}
	}

	private string GetTelemetryHost
	{
		get
		{
			if (!string.IsNullOrEmpty(m_currentTelemetryHost))
			{
				return m_currentTelemetryHost;
			}
			string url = Vars.Key("Telemetry.Host").GetStr(string.Empty);
			if (string.IsNullOrEmpty(url))
			{
				m_currentTelemetryHost = ProductionUrl;
			}
			else
			{
				Uri uriResult;
				bool isUrlType = Uri.TryCreate(url, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
				if (isUrlType)
				{
					m_currentTelemetryHost = url;
				}
				else
				{
					Debug.LogError("[Telemetry] Ingest input URL('" + url + "') is wrong!");
					m_currentTelemetryHost = "https://telemetry-in.battle.net";
				}
				m_currentTelemetryHost = (isUrlType ? url : "https://telemetry-in.battle.net");
			}
			return m_currentTelemetryHost;
		}
	}

	public string ProgramId => "WTCG";

	public string ProgramName => "Hearthstone";

	public Uri IngestUri => m_ingestUri;

	public int? BattleNetRegion => m_battleNetRegion;

	public abstract string ApplicationID { get; }

	public string ProgramVersion => m_programVersion;

	public TelemetryConnectionType ConnectionType => m_connectionType;

	private void PopulateIngestUrl()
	{
		try
		{
			m_ingestUri = new Uri(GetTelemetryHost);
		}
		catch (Exception ex)
		{
			Debug.LogErrorFormat("[Telemetry] Ingest host assignment had an error! (Host: '{0}')\nException: {1}", GetTelemetryHost, ex);
			m_ingestUri = new Uri(ProductionUrl);
		}
	}

	public BaseContextData()
	{
		m_connectionType = ((!GetTelemetryHost.Equals(ProductionUrl, StringComparison.InvariantCultureIgnoreCase)) ? TelemetryConnectionType.Internal : TelemetryConnectionType.Production);
		m_programVersion = $"{31}.{6}.{0}.{216423}";
		m_battleNetId = BnetUtils.TryGetBnetAccountId();
		m_battleNetRegion = (int?)BnetUtils.TryGetGameRegion();
		m_telemetryDisabled = Vars.Key("Telemetry.Enabled").GetBool(def: true);
		PopulateIngestUrl();
	}
}
