using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Hearthstone;
using UnityEngine;

public class Dbf<T> : IDbf where T : DbfRecord, new()
{
	public delegate void RecordAddedListener(T record);

	public delegate void RecordsRemovedListener(List<T> removedRecords);

	private string m_name;

	private List<T> m_records = new List<T>();

	private Dictionary<int, T> m_recordsById = new Dictionary<int, T>();

	private event RecordAddedListener m_recordAddedListener;

	private event RecordsRemovedListener m_recordsRemovedListener;

	public Dbf(string name)
	{
		m_name = name;
	}

	public void CopyRecords(Dbf<T> other)
	{
		m_records.AddRange(other.m_records);
		foreach (KeyValuePair<int, T> kvp in other.m_recordsById)
		{
			m_recordsById.Add(kvp.Key, kvp.Value);
		}
	}

	public void AddListeners(RecordAddedListener addedListener, RecordsRemovedListener removedListener)
	{
		if (addedListener != null)
		{
			m_recordAddedListener += addedListener;
		}
		if (removedListener != null)
		{
			m_recordsRemovedListener += removedListener;
		}
	}

	public DbfRecord CreateNewRecord()
	{
		return new T();
	}

	public void AddRecord(DbfRecord record)
	{
		T rec = (T)record;
		m_records.Add(rec);
		m_recordsById[record.ID] = rec;
		if (this.m_recordAddedListener != null)
		{
			this.m_recordAddedListener(rec);
		}
	}

	public List<T> GetRecords()
	{
		return m_records;
	}

	public List<T> GetRecords(Predicate<T> predicate, int limit = -1)
	{
		if (limit >= 0)
		{
			return m_records.FindAll(predicate).GetRange(0, limit);
		}
		return m_records.FindAll(predicate);
	}

	public static Dbf<T> Load(string name, DbfFormat format)
	{
		string assetPath = ((format == DbfFormat.XML) ? GetXmlPath(name) : GetAssetPath(name));
		return Load(name, assetPath, format);
	}

	public static Dbf<T> Load(string name, string assetPath, DbfFormat format)
	{
		Dbf<T> dbf = new Dbf<T>(name);
		dbf.Clear();
		bool success = false;
		bool useXmlLoading = format == DbfFormat.XML;
		if (!((!useXmlLoading) ? dbf.LoadScriptableObject(assetPath) : DbfXml.Load(assetPath, dbf)))
		{
			dbf.Clear();
			Log.Dbf.Print(string.Format("Dbf.Load[{0}] - failed to load {1} at {2}", useXmlLoading ? "Xml" : "ScriptableObject", name, assetPath));
		}
		GameDbf.RegisterDbf(dbf);
		return dbf;
	}

	public static JobDefinition CreateLoadAsyncJob(string name, DbfFormat format, ref Dbf<T> dbf)
	{
		string assetPath = ((format == DbfFormat.XML) ? GetXmlPath(name) : GetAssetPath(name));
		return CreateLoadAsyncJob(name, assetPath, format, ref dbf);
	}

	public static JobDefinition CreateLoadAsyncJob(string name, string assetPath, DbfFormat format, ref Dbf<T> dbf)
	{
		dbf = new Dbf<T>(name);
		dbf.Clear();
		GameDbf.RegisterDbf(dbf);
		if (format == DbfFormat.XML)
		{
			return new JobDefinition(MakeJobName(typeof(T)), DbfXml.Job_LoadAsync(assetPath, dbf), JobFlags.StartImmediately);
		}
		return new JobDefinition(MakeJobName(typeof(T)), dbf.Job_LoadScriptableObjectAsync(assetPath));
	}

	private static string MakeJobName(Type t)
	{
		return $"Dbf.LoadAsync[{t.ToString()}]";
	}

	public string GetName()
	{
		return m_name;
	}

	public void Clear()
	{
		m_records.Clear();
		m_recordsById.Clear();
	}

	public T GetRecord(int id)
	{
		m_recordsById.TryGetValue(id, out var record);
		return record;
	}

	public T GetRecord(Predicate<T> match)
	{
		return m_records.Find(match);
	}

	public bool HasRecord(int id)
	{
		T record = null;
		m_recordsById.TryGetValue(id, out record);
		return record != null;
	}

	public bool HasRecord(Predicate<T> match)
	{
		return GetRecord(match) != null;
	}

	public void ReplaceRecordByRecordId(T record)
	{
		int index = m_records.FindIndex((T r) => r.ID == record.ID);
		if (index == -1)
		{
			AddRecord(record);
			return;
		}
		T prevRecord = m_records[index];
		bool num = prevRecord != record;
		if (num && this.m_recordsRemovedListener != null)
		{
			List<T> removedRecords = new List<T> { prevRecord };
			this.m_recordsRemovedListener(removedRecords);
		}
		m_records[index] = record;
		m_recordsById[record.ID] = record;
		if (num && this.m_recordAddedListener != null)
		{
			this.m_recordAddedListener(record);
		}
	}

	public void RemoveRecordsWhere(Predicate<T> match)
	{
		List<int> indexesToRemove = null;
		int i = 0;
		for (int iMax = m_records.Count; i < iMax; i++)
		{
			if (match(m_records[i]))
			{
				if (indexesToRemove == null)
				{
					indexesToRemove = new List<int>();
				}
				indexesToRemove.Add(i);
			}
		}
		if (indexesToRemove == null)
		{
			return;
		}
		List<T> removedRecords = null;
		if (this.m_recordsRemovedListener != null)
		{
			removedRecords = new List<T>(indexesToRemove.Count);
		}
		for (int i2 = indexesToRemove.Count - 1; i2 >= 0; i2--)
		{
			int indexToRemove = indexesToRemove[i2];
			T recordInArray = m_records[indexToRemove];
			if (removedRecords != null && recordInArray != null)
			{
				removedRecords.Add(recordInArray);
			}
			if (m_recordsById.TryGetValue(recordInArray.ID, out var recordInMap))
			{
				m_recordsById.Remove(recordInMap.ID);
			}
			m_records.RemoveAt(indexToRemove);
		}
		if (this.m_recordsRemovedListener != null)
		{
			this.m_recordsRemovedListener(removedRecords);
		}
	}

	public override string ToString()
	{
		return m_name;
	}

	private static string GetXmlPath(string name)
	{
		string localPath = $"UnimportedAssets/DBF/{name}.xml";
		if (HearthstoneApplication.TryGetStandaloneLocalDataPath(localPath, out var path))
		{
			return path;
		}
		if (!Application.isEditor)
		{
			localPath = $"DBF/{name}.xml";
		}
		return localPath;
	}

	private static string GetAssetPath(string name)
	{
		return $"Assets/Game/DBF-Asset/{name}.asset";
	}

	public bool LoadScriptableObject(string resourcePath)
	{
		if (!new T().LoadRecordsFromAsset(resourcePath, out m_records))
		{
			return false;
		}
		if (m_records.Count < 1 && m_name != "SUBSET_CARD")
		{
			Debug.LogErrorFormat("{0} DBF Asset has 0 records! Something went wrong generating it. Try checking the generated XMLs in the DBF folder.", m_name);
		}
		int i = 0;
		for (int iMax = m_records.Count; i < iMax; i++)
		{
			T recordVal = m_records[i];
			m_recordsById[recordVal.ID] = recordVal;
			if (this.m_recordAddedListener != null)
			{
				this.m_recordAddedListener(recordVal);
			}
		}
		return true;
	}

	public IEnumerator<IAsyncJobResult> Job_LoadScriptableObjectAsync(string resourcePath)
	{
		Action<List<T>> recordsResultHanlder = delegate(List<T> records)
		{
			m_records = records ?? new List<T>();
			if (m_records.Count < 1 && m_name != "SUBSET_CARD")
			{
				Debug.LogErrorFormat("{0} DBF Asset has 0 records! Something went wrong generating it. Try checking the generated XMLs in the DBF folder.", m_name);
			}
			int i = 0;
			for (int count = m_records.Count; i < count; i++)
			{
				T val = m_records[i];
				m_recordsById[val.ID] = val;
				if (this.m_recordAddedListener != null)
				{
					this.m_recordAddedListener(val);
				}
			}
		};
		return new T().Job_LoadRecordsFromAssetAsync(resourcePath, recordsResultHanlder);
	}
}
