using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core;
using PegasusUtil;
using UnityEngine;

public class RAFManager : IService
{
	public class RecruitData
	{
		public PegasusUtil.RecruitData m_recruit;

		public string m_recruitBattleTag;
	}

	private bool m_isRAFLoading;

	private RAFFrame m_RAFFrame;

	private string m_rafDisplayURL;

	private string m_rafFullURL;

	private bool m_hasRAFData;

	private uint m_totalRecruitCount;

	private List<RecruitData> m_topRecruits;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		Network network = serviceLocator.Get<Network>();
		network.RegisterNetHandler(ProcessRecruitAFriendResponse.PacketID.ID, OnProcessRecruitResponse);
		network.RegisterNetHandler(RecruitAFriendURLResponse.PacketID.ID, OnURLResponse);
		network.RegisterNetHandler(RecruitAFriendDataResponse.PacketID.ID, OnDataResponse);
		HearthstoneApplication.Get().WillReset += WillReset;
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[2]
		{
			typeof(Network),
			typeof(SoundManager)
		};
	}

	public void Shutdown()
	{
		HearthstoneApplication application = HearthstoneApplication.Get();
		if (application != null)
		{
			application.WillReset -= WillReset;
		}
		Network network = null;
		if (ServiceManager.TryGet<Network>(out network))
		{
			network.RemoveNetHandler(ProcessRecruitAFriendResponse.PacketID.ID, OnProcessRecruitResponse);
			network.RemoveNetHandler(RecruitAFriendURLResponse.PacketID.ID, OnURLResponse);
			network.RemoveNetHandler(RecruitAFriendDataResponse.PacketID.ID, OnDataResponse);
		}
	}

	public static RAFManager Get()
	{
		return ServiceManager.Get<RAFManager>();
	}

	public void WillReset()
	{
		BnetPresenceMgr.Get().OnGameAccountPresenceChange -= OnPresenceChanged;
		m_RAFFrame = null;
		m_rafDisplayURL = null;
		m_rafFullURL = null;
		m_hasRAFData = false;
		m_totalRecruitCount = 0u;
		m_topRecruits = null;
	}

	public void InitializeRequests()
	{
		Network.Get().RequestProcessRecruitAFriend();
	}

	public void ShowRAFFrame()
	{
		if (!m_hasRAFData)
		{
			Log.RAF.Print("Network.RequestRecruitAFriendData");
			Network.Get().RequestRecruitAFriendData();
		}
		Processor.CancelCoroutine(ShowRAFFrameWhenReady());
		Processor.RunCoroutine(ShowRAFFrameWhenReady());
	}

	public RAFFrame GetRAFFrame()
	{
		return m_RAFFrame;
	}

	public void ShowRAFHeroFrame()
	{
		if (m_RAFFrame != null)
		{
			m_RAFFrame.ShowHeroFrame();
		}
	}

	public void ShowRAFProgressFrame()
	{
		if (m_RAFFrame != null)
		{
			m_RAFFrame.ShowProgressFrame();
		}
	}

	public void SetRAFProgress(int progress)
	{
		if (m_RAFFrame != null)
		{
			m_RAFFrame.SetProgress(progress);
		}
	}

	public string GetRecruitDisplayURL()
	{
		if (m_rafDisplayURL != null)
		{
			return m_rafDisplayURL;
		}
		Log.RAF.Print("Network.RequestRecruitAFriendURL");
		Network.Get().RequestRecruitAFriendUrl();
		return null;
	}

	public string GetRecruitFullURL()
	{
		if (m_rafFullURL != null)
		{
			return m_rafFullURL;
		}
		return null;
	}

	public void GotoRAFWebsite()
	{
		Processor.CancelCoroutine(SendToRAFWebsiteThenHide());
		Processor.RunCoroutine(SendToRAFWebsiteThenHide());
	}

	public uint GetTotalRecruitCount()
	{
		return m_totalRecruitCount;
	}

	private IEnumerator ShowRAFFrameWhenReady()
	{
		if (m_RAFFrame == null && !m_isRAFLoading)
		{
			m_isRAFLoading = true;
			AssetLoader.Get().InstantiatePrefab("RAF_main.prefab:5fa2642eb52ae469dbe27e96a7570e08", OnRAFLoaded);
		}
		while (m_RAFFrame == null)
		{
			yield return null;
		}
		while (!m_hasRAFData)
		{
			yield return null;
		}
		m_RAFFrame.Show();
		ChatMgr.Get().CloseFriendsList();
	}

	private void OnRAFLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		m_isRAFLoading = false;
		if (go == null)
		{
			Log.RAF.PrintError("RAFManager.OnRAFLoaded() - FAILED to load RAFManager GameObject");
			return;
		}
		m_RAFFrame = go.GetComponent<RAFFrame>();
		if (m_RAFFrame == null)
		{
			Log.RAF.PrintError("RAFManager.OnRAFLoaded() - ERROR RAFManager GameObject has no " + typeof(RAFFrame)?.ToString() + " component");
		}
		else if (m_hasRAFData)
		{
			if (m_totalRecruitCount != 0)
			{
				m_RAFFrame.SetProgressData(m_totalRecruitCount, m_topRecruits);
				m_RAFFrame.ShowProgressFrame();
			}
			else
			{
				m_RAFFrame.ShowHeroFrame();
			}
		}
	}

	private void OnProcessRecruitResponse()
	{
	}

	private void OnURLResponse()
	{
		RecruitAFriendURLResponse responseData = Network.Get().GetRecruitAFriendUrlResponse();
		if (responseData == null || responseData.RafServiceStatus == RAFServiceStatus.RAFServiceStatus_NotAvailable || string.IsNullOrEmpty(responseData.RafUrl))
		{
			string errorString = "RAFManager.OnURLResponse() - Response not valid!";
			if (responseData != null)
			{
				errorString += ((" " + responseData.RafServiceStatus.ToString() + ", " + responseData.RafUrl == null) ? "null" : responseData.RafUrl);
			}
			Log.RAF.PrintError(errorString);
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
			{
				m_headerText = GameStrings.Get("GLUE_RAF_ERROR_HEADER"),
				m_showAlertIcon = true,
				m_text = GameStrings.Get("GLUE_RAF_ERROR_BODY"),
				m_responseDisplay = AlertPopup.ResponseDisplay.OK,
				m_responseCallback = null
			};
			DialogManager.Get().ShowPopup(info);
		}
		else
		{
			m_rafDisplayURL = responseData.RafUrl;
			Log.RAF.Print("Recruit URL = " + m_rafDisplayURL);
			if (m_RAFFrame != null)
			{
				m_rafFullURL = responseData.RafUrlFull;
				m_RAFFrame.ShowLinkFrame(m_rafDisplayURL, m_rafFullURL);
			}
		}
	}

	private void OnDataResponse()
	{
		RecruitAFriendDataResponse data = Network.Get().GetRecruitAFriendDataResponse();
		if (data == null)
		{
			Log.RAF.PrintError("RAFManager.OnDataResponse() - Recruit Data is NULL!");
			return;
		}
		m_hasRAFData = true;
		m_totalRecruitCount = data.TotalRecruitCount;
		m_topRecruits = new List<RecruitData>();
		BnetPresenceMgr.Get().OnGameAccountPresenceChange -= OnPresenceChanged;
		BnetPresenceMgr.Get().OnGameAccountPresenceChange += OnPresenceChanged;
		for (int i = 0; i < data.TopRecruits.Count; i++)
		{
			RecruitData recruitData = new RecruitData();
			m_topRecruits.Add(recruitData);
			recruitData.m_recruit = data.TopRecruits[i];
			if (recruitData.m_recruit.GameAccountId == null)
			{
				Log.RAF.PrintWarning("RAFManager.OnDataResponse() - GameAccountId is NULL for recruit!");
				continue;
			}
			BnetGameAccountId gameAccountId = new BnetGameAccountId(recruitData.m_recruit.GameAccountId.Hi, recruitData.m_recruit.GameAccountId.Lo);
			List<PresenceFieldKey> list = new List<PresenceFieldKey>();
			PresenceFieldKey battleTag = new PresenceFieldKey
			{
				programId = BnetProgramId.BNET.GetValue(),
				groupId = 2u,
				fieldId = 7u,
				uniqueId = 0uL
			};
			list.Add(battleTag);
			battleTag.programId = BnetProgramId.BNET.GetValue();
			battleTag.groupId = 2u;
			battleTag.fieldId = 3u;
			battleTag.uniqueId = 0uL;
			list.Add(battleTag);
			battleTag.programId = BnetProgramId.BNET.GetValue();
			battleTag.groupId = 2u;
			battleTag.fieldId = 5u;
			battleTag.uniqueId = 0uL;
			list.Add(battleTag);
			PresenceFieldKey[] fieldList = list.ToArray();
			BattleNet.RequestPresenceFields(isGameAccountEntityId: true, gameAccountId, fieldList);
		}
		if (m_RAFFrame != null)
		{
			if (m_totalRecruitCount != 0)
			{
				m_RAFFrame.SetProgressData(m_totalRecruitCount, m_topRecruits);
				m_RAFFrame.ShowProgressFrame();
			}
			else
			{
				m_RAFFrame.ShowHeroFrame();
			}
		}
	}

	private void OnPresenceChanged(PresenceUpdate[] updates)
	{
		if (m_topRecruits == null)
		{
			return;
		}
		BnetPlayer myself = BnetPresenceMgr.Get().GetMyPlayer();
		for (int i = 0; i < updates.Length; i++)
		{
			PresenceUpdate update = updates[i];
			if (update.programId != BnetProgramId.BNET || update.groupId != 2 || update.fieldId != 5)
			{
				continue;
			}
			BnetGameAccountId gameAccountId = new BnetGameAccountId(update.entityId?.EntityId);
			BnetPlayer player = BnetUtils.GetPlayer(gameAccountId);
			if (player == null || player == myself || player.GetBattleTag() == null)
			{
				continue;
			}
			foreach (RecruitData recruitData in m_topRecruits)
			{
				if (recruitData.m_recruit.GameAccountId.Lo == gameAccountId.Low && recruitData.m_recruit.GameAccountId.Hi == gameAccountId.High)
				{
					recruitData.m_recruitBattleTag = player.GetBattleTag().GetString();
					Log.RAF.Print("Found Battle Tag for Game Account ID: " + recruitData.m_recruitBattleTag);
					if (m_RAFFrame != null)
					{
						m_RAFFrame.UpdateBattleTag(recruitData.m_recruit.GameAccountId, recruitData.m_recruitBattleTag);
					}
					break;
				}
			}
		}
	}

	private IEnumerator SendToRAFWebsiteThenHide()
	{
		m_RAFFrame.m_infoFrame.m_okayButton.SetEnabled(enabled: false);
		string url = ExternalUrlService.Get().GetRecruitAFriendLink();
		if (!string.IsNullOrEmpty(url))
		{
			Application.OpenURL(url);
		}
		m_RAFFrame.m_infoFrame.Hide();
		yield break;
	}
}
