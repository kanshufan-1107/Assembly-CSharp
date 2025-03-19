using System;
using System.Collections.Generic;
using UnityEngine;

public class VolumeSlidersUtils
{
	private static Dictionary<Option, WeakReference<AudioSource>> s_PlayingFeedbackAudioSources = new Dictionary<Option, WeakReference<AudioSource>>();

	public static void InitializeVolumeSlider(ScrollbarControl volumeSlider, Option volumeOption, AssetReference onReleasedAudioAsset, GameObject gameObject)
	{
		volumeSlider.SetValue(Options.Get().GetFloat(volumeOption));
		volumeSlider.SetUpdateHandler(delegate(float newVolume)
		{
			OnNewVolumeOption(volumeOption, newVolume);
		});
		Options.Get().RegisterChangedListener(volumeOption, OnVolumeOptionChanged, volumeSlider);
		if (onReleasedAudioAsset != null)
		{
			volumeSlider.SetFinishHandler(delegate
			{
				OnVolumeReleased(onReleasedAudioAsset, gameObject, volumeOption);
			});
		}
	}

	private static void OnNewVolumeOption(Option volumeOption, float newVolume)
	{
		Options.Get().SetFloat(volumeOption, newVolume);
	}

	private static void OnVolumeOptionChanged(Option option, object prevValue, bool existed, object userData)
	{
		ScrollbarControl volumeControl = userData as ScrollbarControl;
		if (volumeControl != null)
		{
			volumeControl.SetValue(Options.Get().GetFloat(option));
		}
	}

	private static void OnVolumeReleased(AssetReference onReleasedAudioAsset, GameObject gameObject, Option volumeOption)
	{
		bool didPlay = false;
		if (s_PlayingFeedbackAudioSources.ContainsKey(volumeOption) && s_PlayingFeedbackAudioSources[volumeOption].TryGetTarget(out var audioSourceToStop) && audioSourceToStop != null)
		{
			audioSourceToStop.Stop();
			audioSourceToStop.Play();
			didPlay = true;
		}
		if (!didPlay)
		{
			SoundManager.LoadedCallback loadedCallback = delegate(AudioSource source, object userData)
			{
				SoundManager.Get().Set3d(source, enable: false);
				s_PlayingFeedbackAudioSources[volumeOption] = new WeakReference<AudioSource>(source);
			};
			SoundManager.Get().LoadAndPlay(onReleasedAudioAsset, gameObject, 1f, loadedCallback, volumeOption);
		}
	}
}
