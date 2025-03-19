using System;
using System.Collections.Generic;
using System.Linq;

namespace Hearthstone.Core.Streaming;

public static class DownloadTags
{
	public enum Quality
	{
		Unknown,
		Dbf,
		Strings,
		Essential,
		Initial,
		Fonts,
		Videos,
		SoundSpell,
		CardDef,
		CardAsset,
		CardTexture,
		SoundMission,
		PlaySounds,
		SoundLegend,
		PortPremium,
		MusicExpansion,
		SoundOtherMinion,
		PortHigh,
		HeroMusic
	}

	public enum Content
	{
		Unknown,
		Base,
		Drga,
		Ulda,
		Dala,
		Trla,
		Bota,
		Gila,
		Loota,
		Icca,
		Naxa,
		Brma,
		Loea,
		Kara,
		Bta,
		Boh,
		Bom,
		Rotlk,
		Tb,
		Bgs,
		Prog,
		Merc,
		Adventure,
		Duels,
		Arena
	}

	public enum Locale
	{
		EnUS,
		EnGB,
		RuRU,
		EsES,
		DeDE,
		PlPL,
		PtBR,
		EsMX,
		FrFR,
		ItIT,
		KoKR,
		JaJP,
		ThTH,
		ZhTW,
		ZhCN
	}

	public enum TagGroup
	{
		Quality,
		Content,
		Locale
	}

	public static Dictionary<Quality, string> QualityTags = new Dictionary<Quality, string>
	{
		{
			Quality.Unknown,
			string.Empty
		},
		{
			Quality.Dbf,
			"dbf"
		},
		{
			Quality.Strings,
			"strings"
		},
		{
			Quality.Essential,
			"essential"
		},
		{
			Quality.Initial,
			"initial"
		},
		{
			Quality.Fonts,
			"font"
		},
		{
			Quality.Videos,
			"video"
		},
		{
			Quality.SoundSpell,
			"soundspell"
		},
		{
			Quality.CardDef,
			"carddef"
		},
		{
			Quality.CardAsset,
			"cardasset"
		},
		{
			Quality.CardTexture,
			"cardtexture"
		},
		{
			Quality.SoundMission,
			"soundmission"
		},
		{
			Quality.PlaySounds,
			"playsound"
		},
		{
			Quality.SoundLegend,
			"soundlegend"
		},
		{
			Quality.PortPremium,
			"portpremium"
		},
		{
			Quality.MusicExpansion,
			"musicexpansion"
		},
		{
			Quality.SoundOtherMinion,
			"soundotherminion"
		},
		{
			Quality.PortHigh,
			"porthigh"
		},
		{
			Quality.HeroMusic,
			"heromusic"
		}
	};

	public static Dictionary<Content, string> ContentTags = new Dictionary<Content, string>
	{
		{
			Content.Unknown,
			string.Empty
		},
		{
			Content.Base,
			"base"
		},
		{
			Content.Drga,
			"drga"
		},
		{
			Content.Ulda,
			"ulda"
		},
		{
			Content.Dala,
			"dala"
		},
		{
			Content.Trla,
			"trla"
		},
		{
			Content.Bota,
			"bota"
		},
		{
			Content.Gila,
			"gila"
		},
		{
			Content.Loota,
			"loota"
		},
		{
			Content.Icca,
			"icca"
		},
		{
			Content.Naxa,
			"naxa"
		},
		{
			Content.Brma,
			"brma"
		},
		{
			Content.Loea,
			"loea"
		},
		{
			Content.Kara,
			"kara"
		},
		{
			Content.Bta,
			"bta"
		},
		{
			Content.Boh,
			"boh"
		},
		{
			Content.Bom,
			"bom"
		},
		{
			Content.Rotlk,
			"rotlk"
		},
		{
			Content.Tb,
			"tb"
		},
		{
			Content.Bgs,
			"bgs"
		},
		{
			Content.Prog,
			"prog"
		},
		{
			Content.Merc,
			"merc"
		},
		{
			Content.Adventure,
			"adventure"
		},
		{
			Content.Duels,
			"duels"
		},
		{
			Content.Arena,
			"arena"
		}
	};

	public static TEnum GetLastEnum<TEnum>() where TEnum : IComparable, IConvertible, IFormattable
	{
		return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Last();
	}

	public static Quality GetQualityTag(string tagStr)
	{
		foreach (Quality tag in Enum.GetValues(typeof(Quality)))
		{
			if (tagStr == QualityTags[tag])
			{
				return tag;
			}
		}
		return Quality.Unknown;
	}

	public static Content GetContentTag(string tagStr)
	{
		foreach (Content tag in Enum.GetValues(typeof(Content)))
		{
			if (tagStr == ContentTags[tag])
			{
				return tag;
			}
		}
		return Content.Unknown;
	}

	public static string GetTagString(Quality tag)
	{
		return QualityTags[tag];
	}

	public static string GetTagString(Content tag)
	{
		return ContentTags[tag];
	}

	public static string GetTagString(Locale tag)
	{
		return tag switch
		{
			Locale.EnUS => "enUS", 
			Locale.EnGB => "enGB", 
			Locale.RuRU => "ruRU", 
			Locale.EsES => "esES", 
			Locale.DeDE => "deDE", 
			Locale.PlPL => "plPL", 
			Locale.PtBR => "ptBR", 
			Locale.EsMX => "esMX", 
			Locale.FrFR => "frFR", 
			Locale.ItIT => "itIT", 
			Locale.KoKR => "koKR", 
			Locale.JaJP => "jaJP", 
			Locale.ThTH => "thTH", 
			Locale.ZhTW => "zhTW", 
			Locale.ZhCN => "zhCN", 
			_ => string.Empty, 
		};
	}

	public static string GetTagGroupString(TagGroup tagGroup)
	{
		return tagGroup switch
		{
			TagGroup.Quality => "quality", 
			TagGroup.Content => "content", 
			TagGroup.Locale => "locale", 
			_ => string.Empty, 
		};
	}
}
