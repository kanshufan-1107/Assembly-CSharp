using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Blizzard.T5.AssetManager;
using Hearthstone;
using Hearthstone.Core;
using MiniJSON;
using UnityEngine;
using UnityEngine.Networking;

public class JiraBugDebugDisplay : MonoBehaviour
{
	private const string s_loadingText = "Loading...";

	private const string s_jiraUrl = "https://jira.blizzard.com/";

	private const string s_searchQuery = "summary ~ \"{0}\" and status != closed and issuetype=Bug";

	private const string s_jiraAuth = "";

	private static JiraBugDebugDisplay s_instance = null;

	private static readonly AssetReference s_backgroundTexture = new AssetReference("tilable_background_grey_vertical.tif:2069edef921936f4db7eaeb542bcf5f1");

	private ConcurrentDictionary<string, string> m_bugcache = new ConcurrentDictionary<string, string>();

	private int m_remoteRequestCount;

	private string m_currentCard = "";

	private bool m_isEnabled;

	private GUIStyle m_debugTextStyle;

	private AssetHandle<Texture> m_loadedBackgroundTexture;

	public static JiraBugDebugDisplay Get()
	{
		if (s_instance == null)
		{
			GameObject obj = new GameObject();
			s_instance = obj.AddComponent<JiraBugDebugDisplay>();
			obj.name = "JIRABugDebugDisplay (Dynamically created)";
			AssetLoader.Get().LoadAsset<Texture>(s_backgroundTexture, s_instance.OnTextureLoad);
			s_instance.m_debugTextStyle = new GUIStyle("box");
			s_instance.m_debugTextStyle.fontSize = 16;
			s_instance.m_debugTextStyle.normal.textColor = Color.white;
			s_instance.m_debugTextStyle.alignment = TextAnchor.MiddleLeft;
		}
		return s_instance;
	}

	private void OnTextureLoad(AssetReference assetRef, AssetHandle<Texture> loadedTexture, object callbackData)
	{
		AssetHandle.Take(ref m_loadedBackgroundTexture, loadedTexture);
		s_instance.m_debugTextStyle.normal.background = (Texture2D)m_loadedBackgroundTexture.Asset;
	}

	private void OnDestroy()
	{
		AssetHandle.SafeDispose(ref m_loadedBackgroundTexture);
	}

	private void LoadBugsInBrowser()
	{
		Application.OpenURL(GetSearchURL(m_currentCard, useApiEndpoint: false));
	}

	private bool GetBugsForCard(string cardid, out string bugs)
	{
		m_bugcache.TryGetValue(cardid, out bugs);
		if (string.IsNullOrWhiteSpace(bugs))
		{
			bugs = "No Issues Found";
			return false;
		}
		if (bugs == "Loading...")
		{
			return false;
		}
		return true;
	}

	private string GetSearchURL(string search, bool useApiEndpoint = true)
	{
		string url = ((!useApiEndpoint) ? "https://jira.blizzard.com/issues/?jql=" : "https://jira.blizzard.com/rest/api/2/search/?jql=");
		string jql = $"summary ~ \"{search}\" and status != closed and issuetype=Bug";
		return url + UnityWebRequest.EscapeURL(jql);
	}

	private IEnumerator SearchJira(string search)
	{
		if (!m_bugcache.ContainsKey(search))
		{
			m_remoteRequestCount++;
			m_bugcache.TryAdd(search, "Loading...");
			string url = GetSearchURL(search);
			UnityWebRequest request = new UnityWebRequest(url, "GET");
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Authorization", "");
			request.useHttpContinue = false;
			yield return request.SendWebRequest();
			m_bugcache.TryUpdate(search, ParseJiraSearchResults(request), "Loading...");
		}
		yield return null;
	}

	private string ParseJiraSearchResults(UnityWebRequest request)
	{
		StringBuilder result = new StringBuilder();
		if (Json.Deserialize(request.downloadHandler.text) is JsonNode { Count: >0 } getResponse)
		{
			if (!getResponse.ContainsKey("total"))
			{
				return string.Empty;
			}
			long issuecount = (long)getResponse["total"];
			if (issuecount == 0L)
			{
				return string.Empty;
			}
			JsonList issues = getResponse["issues"] as JsonList;
			for (int x = 0; x < issuecount; x++)
			{
				JsonNode obj = issues[x] as JsonNode;
				string key = obj["key"] as string;
				JsonNode fields = obj["fields"] as JsonNode;
				result.Append(key.PadRight(11));
				result.Append(" - ");
				result.AppendLine(fields["summary"] as string);
			}
			result.Length--;
		}
		return result.ToString();
	}

	public bool EnableDebugDisplay(string func, string[] args, string rawArgs)
	{
		m_isEnabled = true;
		return true;
	}

	public bool DisableDebugDisplay(string func, string[] args, string rawArgs)
	{
		m_isEnabled = false;
		m_bugcache.Clear();
		return true;
	}

	private void Update()
	{
		if (HearthstoneApplication.IsPublic() || !m_isEnabled)
		{
			return;
		}
		GameState currentGame = GameState.Get();
		if (currentGame == null)
		{
			return;
		}
		currentGame.GetEntityMap();
		Card mousedOverCard = InputManager.Get().GetMousedOverCard();
		Entity mousedOverEntity = null;
		if (mousedOverCard != null && mousedOverCard.GetEntity() != null)
		{
			mousedOverEntity = mousedOverCard.GetEntity();
		}
		List<Zone> zones = ZoneMgr.Get().GetZones();
		for (int i = 0; i < zones.Count; i++)
		{
			Zone zone = zones[i];
			if (zone.m_ServerTag != TAG_ZONE.HAND && zone.m_ServerTag != TAG_ZONE.PLAY && zone.m_ServerTag != TAG_ZONE.SECRET)
			{
				continue;
			}
			foreach (Card card in zone.GetCards())
			{
				Entity ent = card.GetEntity();
				if (mousedOverEntity != null && mousedOverEntity != ent)
				{
					continue;
				}
				Vector3 drawPos = card.transform.position;
				if (zone.m_ServerTag == TAG_ZONE.HAND)
				{
					Vector3 offset = card.transform.forward;
					if (card.GetControllerSide() == Player.Side.OPPOSING)
					{
						offset *= -1.5f;
						if (card.GetController().IsRevealed())
						{
							offset = -offset;
						}
					}
					drawPos += offset;
				}
				if (mousedOverEntity != null)
				{
					string cardid = card.GetEntity().GetCardId();
					if (!string.IsNullOrEmpty(cardid))
					{
						Processor.RunCoroutine(SearchJira(cardid));
						SetCurrentCard(cardid);
						DrawDebugTextForHighlightedCard(ent, DebugTextManager.WorldPosToScreenPos(drawPos));
					}
					return;
				}
			}
		}
	}

	private void SetCurrentCard(string cardid)
	{
		s_instance.m_currentCard = cardid;
	}

	private void DrawDebugTextForHighlightedCard(Entity ent, Vector3 pos, bool screenSpace = false, bool forceShowZeroTags = false)
	{
		if (GetBugsForCard(ent.GetCardId(), out var bugs))
		{
			bugs = "Press ALT+F2 to view in JIRA\n" + bugs;
		}
		DebugTextManager.Get().DrawDebugText(bugs, pos, 0f, screenSpace, "", m_debugTextStyle);
	}
}
