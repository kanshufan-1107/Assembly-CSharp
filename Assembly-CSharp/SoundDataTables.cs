using Assets;
using Blizzard.T5.Core;

public class SoundDataTables
{
	public static readonly Map<Global.SoundCategory, Option> s_categoryEnabledOptionMap = new Map<Global.SoundCategory, Option>
	{
		{
			Global.SoundCategory.MUSIC,
			Option.MUSIC
		},
		{
			Global.SoundCategory.SPECIAL_MUSIC,
			Option.MUSIC
		},
		{
			Global.SoundCategory.HERO_MUSIC,
			Option.MUSIC
		}
	};

	public static readonly Map<Global.SoundCategory, Option> s_categoryVolumeOptionMap = new Map<Global.SoundCategory, Option>
	{
		{
			Global.SoundCategory.MUSIC,
			Option.MUSIC_VOLUME
		},
		{
			Global.SoundCategory.SPECIAL_MUSIC,
			Option.MUSIC_VOLUME
		},
		{
			Global.SoundCategory.HERO_MUSIC,
			Option.MUSIC_VOLUME
		},
		{
			Global.SoundCategory.AMBIENCE,
			Option.AMBIENCE_VOLUME
		},
		{
			Global.SoundCategory.VO,
			Option.DIALOG_VOLUME
		},
		{
			Global.SoundCategory.SPECIAL_VO,
			Option.DIALOG_VOLUME
		},
		{
			Global.SoundCategory.TRIGGER_VO,
			Option.DIALOG_VOLUME
		},
		{
			Global.SoundCategory.BOSS_VO,
			Option.DIALOG_VOLUME
		},
		{
			Global.SoundCategory.FX,
			Option.SOUND_EFFECT_VOLUME
		},
		{
			Global.SoundCategory.SPECIAL_CARD,
			Option.SOUND_EFFECT_VOLUME
		},
		{
			Global.SoundCategory.TRIGGER_SFX,
			Option.SOUND_EFFECT_VOLUME
		},
		{
			Global.SoundCategory.SPECIAL_SFX,
			Option.SOUND_EFFECT_VOLUME
		}
	};

	public static readonly Map<Option, float> s_optionVolumeMaxMap = new Map<Option, float> { 
	{
		Option.MUSIC_VOLUME,
		0.5f
	} };
}
