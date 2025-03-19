using System;
using System.Collections.Generic;
using Blizzard.T5.Jobs;
using Hearthstone.Core;
using Hearthstone.Http;
using Hearthstone.InGameMessage.UI;
using UnityEngine;

namespace Hearthstone.InGameMessage;

public class InGameMessageExternalAssetDownloader
{
	public void DownloadMessagesExternalAssets(List<MessageUIData> messageList, Action OnDownloadDone)
	{
		Processor.QueueJob("IGM_DownloadAllExternalAssets", DownloadExternalAssets(messageList, OnDownloadDone));
	}

	private IEnumerator<IAsyncJobResult> DownloadExternalAssets(List<MessageUIData> messageList, Action OnDownloadDone)
	{
		List<WaitForJob> allDownloads = new List<WaitForJob>();
		foreach (MessageUIData message in messageList)
		{
			if (message.LayoutType == MessageLayoutType.SHOP)
			{
				ShopMessageContent data = message.MessageData as ShopMessageContent;
				if (!string.IsNullOrEmpty(data.TextureAssetUrl) && data.ImageTexture == null)
				{
					JobDefinition job = new JobDefinition("IGM_DownloadExternalAsset", DownloadShopExternalTextureAsset(message));
					Processor.QueueJob(job);
					allDownloads.Add(job.CreateDependency());
				}
			}
		}
		if (allDownloads.Count == 0)
		{
			OnDownloadDone?.Invoke();
			yield break;
		}
		IEnumerator<IAsyncJobResult> jobAction = OnAllDownloadDone(OnDownloadDone);
		IJobDependency[] dependencies = allDownloads.ToArray();
		Processor.QueueJob("IGM_WaitAllExternalAssetsDownload", jobAction, dependencies);
	}

	private IEnumerator<IAsyncJobResult> DownloadShopExternalTextureAsset(MessageUIData message)
	{
		ShopMessageContent data = message.MessageData as ShopMessageContent;
		IHttpRequest textureHttpRequest = HttpRequestFactory.Get().CreateGetTextureRequest(data.TextureAssetUrl);
		AsyncOperation asyncOp = textureHttpRequest.SendRequest();
		while (!asyncOp.isDone)
		{
			yield return null;
		}
		if (textureHttpRequest.IsNetworkError || textureHttpRequest.IsHttpError)
		{
			Log.InGameMessage.PrintError("Failed to download image for message Title " + data.Title + ", URL : " + data.TextureAssetUrl);
			Log.InGameMessage.PrintError(textureHttpRequest.ErrorString);
		}
		else
		{
			Texture tex = textureHttpRequest.ResponseAsTexture;
			tex.wrapMode = TextureWrapMode.Clamp;
			data.ImageTexture = tex;
		}
	}

	private IEnumerator<IAsyncJobResult> OnAllDownloadDone(Action OnDownloadDone)
	{
		Log.InGameMessage.PrintDebug("External assets downloaded");
		OnDownloadDone?.Invoke();
		yield break;
	}
}
