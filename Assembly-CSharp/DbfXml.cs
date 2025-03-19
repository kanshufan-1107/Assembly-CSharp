using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using UnityEngine;

public class DbfXml
{
	public static bool Load<T>(string xmlFile, Dbf<T> dbf) where T : DbfRecord, new()
	{
		if (File.Exists(xmlFile))
		{
			using (XmlReader reader = XmlReader.Create(xmlFile))
			{
				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element && reader.Name == "Record")
					{
						LoadRecord(reader.ReadSubtree(), dbf);
					}
				}
				return true;
			}
		}
		return false;
	}

	public static IEnumerator<IAsyncJobResult> Job_LoadAsync<T>(string xmlFile, Dbf<T> dbf) where T : DbfRecord, new()
	{
		string dbfName = dbf.GetName();
		Dbf<T> threadSafeDBF = new Dbf<T>(dbfName);
		yield return new JobDefinition($"DbfXml.LoadAsyncFromDisk[{dbfName}]", Job_LoadAsyncFromDisk(xmlFile, threadSafeDBF), JobFlags.StartImmediately | JobFlags.UseWorkerThread);
		lock (threadSafeDBF)
		{
			dbf.CopyRecords(threadSafeDBF);
		}
	}

	public static IEnumerator<IAsyncJobResult> Job_LoadAsyncFromDisk<T>(string xmlFile, Dbf<T> dbf) where T : DbfRecord, new()
	{
		lock (dbf)
		{
			Load(xmlFile, dbf);
		}
		yield break;
	}

	public static void LoadRecord<T>(XmlReader reader, Dbf<T> dbf, bool hideDbfLocDebugInfo = false) where T : DbfRecord, new()
	{
		DbfRecord newRecord = dbf.CreateNewRecord();
		while (reader.Read())
		{
			if (reader.NodeType != XmlNodeType.Element || reader.Name != "Field" || reader.IsEmptyElement)
			{
				continue;
			}
			string colName = reader["column"];
			Type varType = newRecord.GetVarType(colName);
			if (varType != null)
			{
				try
				{
					string strVal;
					if (varType == typeof(DbfLocValue))
					{
						newRecord.SetVar(colName, LoadLocalizedString(reader["loc_ID"], reader.ReadSubtree(), hideDbfLocDebugInfo));
					}
					else if (varType == typeof(bool))
					{
						string str = reader.ReadElementContentAsString();
						newRecord.SetVar(colName, GeneralUtils.ForceBool(str));
					}
					else if (varType.IsEnum)
					{
						strVal = reader.ReadElementContentAs(typeof(string), null) as string;
						Type underlyingType = Enum.GetUnderlyingType(varType);
						if (underlyingType == typeof(int))
						{
							if (!int.TryParse(strVal, out var val))
							{
								goto IL_01af;
							}
							newRecord.SetVar(colName, val);
						}
						else if (underlyingType == typeof(uint))
						{
							if (!uint.TryParse(strVal, out var val2))
							{
								goto IL_01af;
							}
							newRecord.SetVar(colName, val2);
						}
						else if (underlyingType == typeof(long))
						{
							if (!long.TryParse(strVal, out var val3))
							{
								goto IL_01af;
							}
							newRecord.SetVar(colName, val3);
						}
						else
						{
							if (!(underlyingType == typeof(ulong)) || !ulong.TryParse(strVal, out var val4))
							{
								goto IL_01af;
							}
							newRecord.SetVar(colName, val4);
						}
					}
					else if (varType == typeof(ulong))
					{
						newRecord.SetVar(colName, ulong.Parse(reader.ReadElementContentAsString()));
					}
					else
					{
						newRecord.SetVar(colName, reader.ReadElementContentAs(varType, null));
					}
					goto end_IL_0058;
					IL_01af:
					newRecord.SetVar(colName, strVal);
					end_IL_0058:;
				}
				catch (Exception ex)
				{
					Debug.LogErrorFormat("Failed to read record id={0} column={1} with varType={2} exception={3}", newRecord.ID, colName, varType, ex.ToString());
					throw;
				}
			}
			else
			{
				Debug.LogErrorFormat("Type is not defined for column {0}, dbf={1}. Try \"Build->Generate DBFs and Code\"", colName, newRecord.GetType().Name);
			}
		}
		dbf.AddRecord(newRecord);
	}

	public static DbfLocValue LoadLocalizedString(string locIdStr, XmlReader reader, bool hideDebugInfo = false)
	{
		reader.Read();
		DbfLocValue locStrings = new DbfLocValue(hideDebugInfo);
		if (!string.IsNullOrEmpty(locIdStr))
		{
			int locId = 0;
			if (int.TryParse(locIdStr, out locId))
			{
				locStrings.SetLocId(locId);
			}
		}
		while (reader.Read())
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localeName = reader.Name;
				string localeString = reader.ReadElementContentAsString();
				Locale locale;
				try
				{
					locale = EnumUtils.GetEnum<Locale>(localeName);
				}
				catch (ArgumentException)
				{
					continue;
				}
				locStrings.SetString(locale, TextUtils.DecodeWhitespaces(localeString));
			}
		}
		return locStrings;
	}
}
