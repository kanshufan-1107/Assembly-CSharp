using System.Collections.Generic;
using Assets;
using UnityEngine;

public class SoundUtils
{
	public static readonly AssetReference SquarePanelSlideOnSFX = new AssetReference("UI_SquarePanel_slide_on.prefab:777a4a40258158040ad5bc27596ba51e");

	public static readonly AssetReference SquarePanelSlideOffSFX = new AssetReference("UI_SquarePanel_slide_off.prefab:9e10f244ba0586e44beca5b547684d3f");

	public static PlatformDependentValue<bool> PlATFORM_CAN_DETECT_VOLUME = new PlatformDependentValue<bool>(PlatformCategory.OS)
	{
		PC = true,
		Mac = true,
		iOS = false,
		Android = false
	};

	public static bool IsDeviceBackgroundMusicPlaying()
	{
		return false;
	}

	public static Option GetCategoryEnabledOption(Global.SoundCategory cat)
	{
		Option option = Option.INVALID;
		SoundDataTables.s_categoryEnabledOptionMap.TryGetValue(cat, out option);
		return option;
	}

	public static Option GetCategoryVolumeOption(Global.SoundCategory cat)
	{
		Option option = Option.INVALID;
		SoundDataTables.s_categoryVolumeOptionMap.TryGetValue(cat, out option);
		return option;
	}

	public static float GetOptionVolume(Option option)
	{
		float num = Mathf.Clamp01(Options.Get().GetFloat(option));
		float maxVolume = (SoundDataTables.s_optionVolumeMaxMap.ContainsKey(option) ? SoundDataTables.s_optionVolumeMaxMap[option] : 1f);
		return num * maxVolume;
	}

	public static float GetCategoryVolume(Global.SoundCategory cat)
	{
		Cheats cheatsService = Cheats.Get();
		if (cheatsService != null && !cheatsService.IsSoundCategoryEnabled(cat))
		{
			return 0f;
		}
		Option option = GetCategoryVolumeOption(cat);
		if (option == Option.INVALID)
		{
			return 1f;
		}
		return GetOptionVolume(option);
	}

	public static bool IsMusicCategory(Global.SoundCategory cat)
	{
		return cat switch
		{
			Global.SoundCategory.MUSIC => true, 
			Global.SoundCategory.SPECIAL_MUSIC => true, 
			_ => false, 
		};
	}

	public static void SetSourceVolumes(Component c, float volume, bool includeInactive = false)
	{
		if ((bool)c)
		{
			SetSourceVolumes(c.gameObject, volume);
		}
	}

	public static void SetSourceVolumes(GameObject go, float volume, bool includeInactive = false)
	{
		if ((bool)go)
		{
			AudioSource[] componentsInChildren = go.GetComponentsInChildren<AudioSource>(includeInactive);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].volume = volume;
			}
		}
	}

	public static string GetRandomClipFromDef(SoundDef def)
	{
		if (def == null)
		{
			return null;
		}
		List<RandomAudioClip> randomClips = def.m_RandomClips;
		if (def is IMultipleRandomClipSoundDef)
		{
			randomClips = ((IMultipleRandomClipSoundDef)def).GetRandomAudioClips();
		}
		if (randomClips == null)
		{
			return null;
		}
		if (randomClips.Count == 0)
		{
			return null;
		}
		float totalWeight = 0f;
		foreach (RandomAudioClip randomClip in randomClips)
		{
			totalWeight += randomClip.m_Weight;
		}
		float roll = Random.Range(0f, totalWeight);
		float runningWeight = 0f;
		int lastClipIndex = randomClips.Count - 1;
		for (int i = 0; i < lastClipIndex; i++)
		{
			RandomAudioClip randomClip2 = randomClips[i];
			runningWeight += randomClip2.m_Weight;
			if (roll <= runningWeight)
			{
				return randomClip2.m_Clip;
			}
		}
		return randomClips[lastClipIndex].m_Clip;
	}

	public static float GetRandomVolumeFromDef(SoundDef def)
	{
		if (def == null)
		{
			return 1f;
		}
		return Random.Range(def.m_RandomVolumeMin, def.m_RandomVolumeMax);
	}

	public static float GetRandomPitchFromDef(SoundDef def)
	{
		if (def == null)
		{
			return 1f;
		}
		return Random.Range(def.m_RandomPitchMin, def.m_RandomPitchMax);
	}

	public static void CopyDuckedCategoryDef(SoundDuckedCategoryDef src, SoundDuckedCategoryDef dst)
	{
		dst.m_Category = src.m_Category;
		dst.m_Volume = src.m_Volume;
		dst.m_BeginSec = src.m_BeginSec;
		dst.m_BeginEaseType = src.m_BeginEaseType;
		dst.m_RestoreSec = src.m_RestoreSec;
		dst.m_RestoreEaseType = src.m_RestoreEaseType;
	}

	public static void CopyAudioSource(AudioSource src, AudioSource dst)
	{
		dst.clip = src.clip;
		dst.outputAudioMixerGroup = src.outputAudioMixerGroup;
		dst.bypassEffects = src.bypassEffects;
		dst.loop = src.loop;
		dst.priority = src.priority;
		dst.volume = src.volume;
		dst.pitch = src.pitch;
		dst.panStereo = src.panStereo;
		dst.spatialBlend = src.spatialBlend;
		dst.reverbZoneMix = src.reverbZoneMix;
		dst.rolloffMode = src.rolloffMode;
		dst.dopplerLevel = src.dopplerLevel;
		dst.minDistance = src.minDistance;
		dst.maxDistance = src.maxDistance;
		dst.spread = src.spread;
		SoundDef srcDef = src.GetComponent<SoundDef>();
		if (srcDef == null)
		{
			SoundDef dstDef = dst.GetComponent<SoundDef>();
			if (dstDef != null)
			{
				Object.DestroyImmediate(dstDef);
			}
			return;
		}
		SoundDef dstDef2 = dst.GetComponent<SoundDef>();
		if (dstDef2 == null)
		{
			dstDef2 = dst.gameObject.AddComponent<SoundDef>();
		}
		CopySoundDef(srcDef, dstDef2);
	}

	public static void CopySoundDef(SoundDef src, SoundDef dst)
	{
		dst.m_Category = src.m_Category;
		dst.m_RandomClips = new List<RandomAudioClip>();
		if (src.m_RandomClips != null)
		{
			for (int i = 0; i < src.m_RandomClips.Count; i++)
			{
				dst.m_RandomClips.Add(src.m_RandomClips[i]);
			}
		}
		dst.m_RandomPitchMin = src.m_RandomPitchMin;
		dst.m_RandomPitchMax = src.m_RandomPitchMax;
		dst.m_RandomVolumeMin = src.m_RandomVolumeMin;
		dst.m_RandomVolumeMax = src.m_RandomVolumeMax;
		dst.m_IgnoreDucking = src.m_IgnoreDucking;
	}

	public static bool ChangeAudioSourceSettings(AudioSource source, AudioSourceSettings settings)
	{
		bool changed = false;
		if (settings.m_changeMixerOutput && source.outputAudioMixerGroup != settings.m_mixerOutput)
		{
			source.outputAudioMixerGroup = settings.m_mixerOutput;
			changed = true;
		}
		if (settings.m_changeBypassEffects && source.bypassEffects != settings.m_bypassEffects)
		{
			source.bypassEffects = settings.m_bypassEffects;
			changed = true;
		}
		if (settings.m_changeLoop && source.loop != settings.m_loop)
		{
			source.loop = settings.m_loop;
			changed = true;
		}
		if (settings.m_changePriority && source.priority != settings.m_priority)
		{
			source.priority = settings.m_priority;
			changed = true;
		}
		if (settings.m_changeVolume && !Mathf.Approximately(source.volume, settings.m_volume))
		{
			source.volume = settings.m_volume;
			changed = true;
		}
		if (settings.m_changePitch && !Mathf.Approximately(source.pitch, settings.m_pitch))
		{
			source.pitch = settings.m_pitch;
			changed = true;
		}
		if (settings.m_changeStereoPan && !Mathf.Approximately(source.panStereo, settings.m_stereoPan))
		{
			source.panStereo = settings.m_stereoPan;
			changed = true;
		}
		if (settings.m_changeSpatialBlend && !Mathf.Approximately(source.spatialBlend, settings.m_spatialBlend))
		{
			source.spatialBlend = settings.m_spatialBlend;
			changed = true;
		}
		if (settings.m_changeReverbZoneMix && !Mathf.Approximately(source.reverbZoneMix, settings.m_reverbZoneMix))
		{
			source.reverbZoneMix = settings.m_reverbZoneMix;
			changed = true;
		}
		if (settings.m_changeRolloffMode && source.rolloffMode != settings.m_rolloffMode)
		{
			source.rolloffMode = settings.m_rolloffMode;
			changed = true;
		}
		if (settings.m_changeDopplerLevel && !Mathf.Approximately(source.dopplerLevel, settings.m_dopplerLevel))
		{
			source.dopplerLevel = settings.m_dopplerLevel;
			changed = true;
		}
		if (settings.m_changeMinDistance && !Mathf.Approximately(source.minDistance, settings.m_minDistance))
		{
			source.minDistance = settings.m_minDistance;
			changed = true;
		}
		if (settings.m_changeMaxDistance && !Mathf.Approximately(source.maxDistance, settings.m_maxDistance))
		{
			source.maxDistance = settings.m_maxDistance;
			changed = true;
		}
		if (settings.m_changeSpread && !Mathf.Approximately(source.spread, settings.m_spread))
		{
			source.spread = settings.m_spread;
			changed = true;
		}
		return changed;
	}

	public static bool AddAudioSourceComponents(GameObject go)
	{
		bool modified = false;
		AudioSource source = go.GetComponent<AudioSource>();
		if (source == null)
		{
			source = go.AddComponent<AudioSource>();
			ChangeAudioSourceSettings(source, new AudioSourceSettings());
			modified = true;
		}
		if (source.playOnAwake)
		{
			source.playOnAwake = false;
			modified = true;
		}
		if (go.GetComponent<SoundDef>() == null)
		{
			modified = true;
			go.AddComponent<SoundDef>();
		}
		return modified;
	}
}
