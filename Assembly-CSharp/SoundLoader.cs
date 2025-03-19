using System;
using Blizzard.T5.AssetManager;
using Blizzard.T5.Services;
using UnityEngine;

public static class SoundLoader
{
	private class LoadSoundCallbackData
	{
		public PrefabCallback<GameObject> callback;

		public object callbackData;

		public GameObject fallback;
	}

	public static bool LoadSound(AssetReference assetRef, PrefabCallback<GameObject> callback, object callbackData = null, GameObject fallback = null)
	{
		LoadSoundCallbackData soundCallbackData = new LoadSoundCallbackData
		{
			callback = callback,
			callbackData = callbackData,
			fallback = fallback
		};
		bool num = AssetLoader.Get().InstantiatePrefab(assetRef, OnLoadSoundAttempted, soundCallbackData);
		if (!num)
		{
			OnLoadSoundAttempted(assetRef, null, soundCallbackData);
		}
		return num;
	}

	public static GameObject LoadSound(AssetReference assetRef)
	{
		if (assetRef == null)
		{
			Error.AddDevFatal("SoundLoader.LoadSound() - An asset request was made but no file name was given.");
			return null;
		}
		GameObject go = AssetLoader.Get().InstantiatePrefab(assetRef);
		if (!LocalizeSoundPrefab(go))
		{
			UnityEngine.Object.Destroy(go);
			return null;
		}
		return go;
	}

	public static void GetAudioDataForObject(GameObject go, out AudioSource audioSource, out SoundDef soundDef)
	{
		CardSoundSpell soundSpell = go.GetComponent<CardSoundSpell>();
		if (soundSpell != null)
		{
			audioSource = soundSpell.DetermineBestAudioSource();
			if (audioSource == null)
			{
				Debug.LogError(" No audio source in Object" + go.name + " Please check the object to make sure it has an Audio Source Component");
				soundDef = null;
			}
			else
			{
				soundDef = audioSource.gameObject.GetComponent<SoundDef>();
			}
		}
		else
		{
			soundDef = go.GetComponent<SoundDef>();
			audioSource = go.GetComponent<AudioSource>();
		}
	}

	private static bool LocalizeSoundPrefab(GameObject go)
	{
		if (go == null)
		{
			return false;
		}
		GetAudioDataForObject(go, out var source, out var soundDef);
		if (soundDef == null)
		{
			Log.Asset.PrintInfo("LocalizeSoundPrefab: trying to load sound prefab with no SoundDef components: \"{0}\"", go.name);
			return false;
		}
		if (string.IsNullOrEmpty(soundDef.m_AudioClip))
		{
			Log.Asset.PrintInfo("LocalizeSoundPrefab: trying to load sound prefab with an SoundDef that contains no AudoClip: \"{0}\"", go.name);
			return false;
		}
		AssetHandle<AudioClip> clip = null;
		LoadAudioClipWithFallback(ref clip, source, soundDef.m_AudioClip);
		if (clip != null)
		{
			ServiceManager.Get<DisposablesCleaner>()?.Attach(go, clip);
			source.clip = clip;
			return true;
		}
		return false;
	}

	public static void LoadAudioClipWithFallback(ref AssetHandle<AudioClip> clip, AudioSource source, AssetReference clipAsset)
	{
		AssetLoader.Get().LoadAsset(ref clip, clipAsset);
		if (clip == null)
		{
			source.volume = 0f;
			Log.Sound.PrintWarning("LoadAudioClipWithFallback failed to load {0}. Falling back to muted enUS asset", clipAsset?.ToString());
			AssetLoader.Get().LoadAsset(ref clip, clipAsset, AssetLoadingOptions.DisableLocalization);
		}
		if (clip == null)
		{
			Log.Sound.PrintWarning("LoadAudioClipWithFallback failed to load enUS variant of {0}. Falling back to general fallback sound", clipAsset?.ToString());
			AssetLoader.Get().LoadAsset(ref clip, SoundManager.FallbackSound);
		}
	}

	private static void OnLoadSoundAttempted(AssetReference assetRef, GameObject go, object callbackData)
	{
		LoadSoundCallbackData soundCallbackData = (LoadSoundCallbackData)callbackData;
		try
		{
			if (go == null && soundCallbackData.fallback != null)
			{
				go = UnityEngine.Object.Instantiate(soundCallbackData.fallback);
			}
			if (go != null && !LocalizeSoundPrefab(go))
			{
				UnityEngine.Object.Destroy(go);
				go = null;
			}
			soundCallbackData.callback(assetRef, go, soundCallbackData.callbackData);
		}
		catch (Exception ex)
		{
			Error.AddDevFatal("LoadSoundCallback failed - assetRef={0}: {1}", assetRef?.ToString(), ex);
			soundCallbackData.callback(assetRef, null, soundCallbackData.callbackData);
		}
	}
}
