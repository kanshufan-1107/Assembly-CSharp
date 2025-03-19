using Hearthstone.DataModels;
using PegasusUtil;

public class WizardDuels : StandardGameEntity
{
	private PVPDRLobbyDataModel m_pvpdrDataModel;

	public override void OnCreate()
	{
		Network.Get().RegisterNetHandler(PVPDRSessionInfoResponse.PacketID.ID, OnPVPDRSessionInfoResponse);
		GetPVPDRDataModel();
	}

	public override void StartGameplaySoundtracks()
	{
		if (m_pvpdrDataModel != null && m_pvpdrDataModel.Wins >= 9)
		{
			MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_SCH_FinalLevels);
		}
		else
		{
			base.StartGameplaySoundtracks();
		}
	}

	public override void StartMulliganSoundtracks(bool soft)
	{
		MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_SCH_Mulligan);
	}

	private PVPDRLobbyDataModel GetPVPDRDataModel()
	{
		if (m_pvpdrDataModel != null)
		{
			return m_pvpdrDataModel;
		}
		Network.Get().SendPVPDRSessionInfoRequest();
		return null;
	}

	private void OnPVPDRSessionInfoResponse()
	{
		PVPDRSessionInfoResponse response = Network.Get().GetPVPDRSessionInfoResponse();
		if (response.HasSession)
		{
			m_pvpdrDataModel = new PVPDRLobbyDataModel();
			m_pvpdrDataModel.Wins = (int)response.Session.Wins;
			m_pvpdrDataModel.Losses = (int)response.Session.Losses;
			m_pvpdrDataModel.HasSession = response.Session.HasSession;
			m_pvpdrDataModel.IsSessionActive = response.Session.IsActive;
			m_pvpdrDataModel.IsPaidEntry = response.Session.IsPaidEntry;
			m_pvpdrDataModel.IsSessionRolledOver = response.Session.DidSeasonRollover;
		}
		Network.Get().RemoveNetHandler(PVPDRSessionInfoResponse.PacketID.ID, OnPVPDRSessionInfoResponse);
	}
}
