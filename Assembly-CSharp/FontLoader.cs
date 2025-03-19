using System;
using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Core;
using Blizzard.T5.Fonts;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone.Core;
using UnityEngine;

public class FontLoader : IFontLoader
{
	private LoadResource m_resourceData;

	private Map<string, AssetHandle<FontDefinition>> m_defs;

	private Logger m_logger;

	public FontTableData ResourceData => m_resourceData.LoadedAsset as FontTableData;

	public FontLoader(Logger logger)
	{
		m_logger = logger;
	}

	public IUnreliableJobDependency LoadFontTableData()
	{
		m_resourceData = new LoadResource("ServiceData/FontTableData", LoadResourceFlags.FailOnError);
		return m_resourceData;
	}

	public void LoadFontDefinition(Action<Map<string, AssetHandle<FontDefinition>>> onLoadDone = null)
	{
		Processor.QueueJob("loadDefs", Job_LoadFontDefinition(onLoadDone), new WaitForGameDownloadManagerAvailable(), ServiceManager.CreateServiceDependency(typeof(IAssetLoader)));
	}

	private IEnumerator<IAsyncJobResult> Job_LoadFontDefinition(Action<Map<string, AssetHandle<FontDefinition>>> onLoadDone = null)
	{
		m_defs = new Map<string, AssetHandle<FontDefinition>>();
		JobResultCollection loadFontDefJobs = new JobResultCollection();
		foreach (FontTableData.FontTableEntry entry in ResourceData.m_Entries)
		{
			loadFontDefJobs.Add(new LoadFontDef($"{entry.m_FontName}:{entry.m_FontGuid}"));
		}
		yield return loadFontDefJobs;
		int i = 0;
		while (i < loadFontDefJobs.Results.Count)
		{
			LoadFontDef loadFontDefJob = loadFontDefJobs.Results[i] as LoadFontDef;
			string assetName = loadFontDefJob.AssetRef.GetLegacyAssetName();
			m_logger.Log(LogLevel.Debug, "OnFontDefLoaded " + assetName);
			if (loadFontDefJob.loadedAsset == null)
			{
				ServiceManager.Get<IErrorService>()?.AddFatal(FatalErrorReason.ASSET_INCORRECT_DATA, "GLOBAL_ERROR_ASSET_INCORRECT_DATA", assetName);
				string internalErrorMessage = string.Format("FontLoader.Job_LoadFontDefinition() - name={0} message={1}", assetName, ServiceManager.Get<IGameStringsService>().Format("GLOBAL_ERROR_ASSET_INCORRECT_DATA", assetName));
				Debug.LogError(internalErrorMessage);
				yield return new JobFailedResult(internalErrorMessage);
			}
			m_defs.SetOrReplaceDisposable(assetName, loadFontDefJob.loadedAsset);
			int num = i + 1;
			i = num;
		}
		onLoadDone?.Invoke(m_defs);
	}
}
