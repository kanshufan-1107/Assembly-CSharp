using System;
using System.Collections.Generic;
using System.IO;
using Hearthstone.Util;
using UnityEngine;

namespace Hearthstone.InGameMessage;

public class ViewCountController
{
	[Serializable]
	private struct ViewCountStructSave
	{
		public string m_uid;

		public int m_viewCount;

		public ulong m_lastUpdate;
	}

	[Serializable]
	private class ViewCountClassSave
	{
		public List<ViewCountStructSave> m_items = new List<ViewCountStructSave>();
	}

	private struct ViewCountStruct
	{
		public int m_viewCount;

		public ulong m_lastUpdate;
	}

	private const string VIEW_COUNT_FILENAME = "ViewCountsPath.json";

	private const double IGM_VALID_VIEWCOUNTS_SECONDS = 4838400.0;

	private Dictionary<string, ViewCountStruct> m_viewCountDict = new Dictionary<string, ViewCountStruct>();

	private static string ViewCountsPath => Path.Combine(PlatformFilePaths.PersistentDataPath, "ViewCountsPath.json");

	public ViewCountController()
	{
		Deserialize();
	}

	public int GetViewCount(string uid)
	{
		if (m_viewCountDict.TryGetValue(uid, out var view))
		{
			return view.m_viewCount;
		}
		return 0;
	}

	public void IncreaseViewCount(string uid)
	{
		if (m_viewCountDict.TryGetValue(uid, out var view))
		{
			view.m_viewCount++;
			view.m_lastUpdate = TimeUtils.DateTimeToUnixTimeStamp(DateTime.Now);
			m_viewCountDict[uid] = view;
		}
		else
		{
			ViewCountStruct viewCountStruct = default(ViewCountStruct);
			viewCountStruct.m_viewCount = 1;
			viewCountStruct.m_lastUpdate = TimeUtils.DateTimeToUnixTimeStamp(DateTime.Now);
			view = viewCountStruct;
			m_viewCountDict.Add(uid, view);
		}
		Serialize();
	}

	public void ClearViewCounts()
	{
		m_viewCountDict.Clear();
		Serialize();
	}

	private void Deserialize()
	{
		if (!File.Exists(ViewCountsPath))
		{
			return;
		}
		ViewCountClassSave viewCounts;
		try
		{
			viewCounts = JsonUtility.FromJson<ViewCountClassSave>(File.ReadAllText(ViewCountsPath));
		}
		catch (Exception ex)
		{
			Log.InGameMessage.PrintError("Unable to deserialize {0}: {1}", "ViewCountsPath.json", ex);
			return;
		}
		if (viewCounts == null || viewCounts.m_items == null)
		{
			Log.InGameMessage.PrintError("Deserialized viewCounts is empty");
			return;
		}
		if (m_viewCountDict == null)
		{
			m_viewCountDict = new Dictionary<string, ViewCountStruct>();
		}
		try
		{
			viewCounts.m_items.ForEach(delegate(ViewCountStructSave x)
			{
				if ((double)(TimeUtils.DateTimeToUnixTimeStamp(DateTime.Now) - x.m_lastUpdate) < 4838400.0)
				{
					m_viewCountDict.Add(x.m_uid, new ViewCountStruct
					{
						m_viewCount = x.m_viewCount,
						m_lastUpdate = x.m_lastUpdate
					});
				}
			});
		}
		catch (Exception ex2)
		{
			Log.InGameMessage.PrintError("Unable to deserialize {0}: {1}", "ViewCountsPath.json", ex2);
		}
	}

	private void Serialize()
	{
		ViewCountClassSave viewCounts = new ViewCountClassSave();
		foreach (KeyValuePair<string, ViewCountStruct> view in m_viewCountDict)
		{
			viewCounts.m_items.Add(new ViewCountStructSave
			{
				m_uid = view.Key,
				m_viewCount = view.Value.m_viewCount,
				m_lastUpdate = view.Value.m_lastUpdate
			});
		}
		try
		{
			string json = JsonUtility.ToJson(viewCounts, !HearthstoneApplication.IsPublic());
			File.WriteAllText(ViewCountsPath, json);
		}
		catch (Exception ex)
		{
			Log.InGameMessage.PrintError("Unable to serialize {0}: {1}", "ViewCountsPath.json", ex);
		}
	}
}
