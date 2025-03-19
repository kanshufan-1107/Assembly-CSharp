using System.Collections.Generic;

public static class TierAttributes
{
	public struct LayoutStyleData
	{
		public List<string> LayoutMap { get; set; }

		public int LayoutWidth { get; set; }

		public int LayoutHeight { get; set; }

		public int MaxLayoutCount { get; set; }

		public List<string> TierOptionTags { get; set; }

		public List<string> DividerOptionTags { get; set; }

		public List<string> AdditionalTags { get; set; }
	}

	public enum LayoutStyle
	{
		Default,
		XL,
		BigSmall,
		SmallBig,
		Standard,
		StandardSingle,
		Battlegrounds,
		Expansion,
		YearExpansion
	}

	private static Dictionary<LayoutStyle, LayoutStyleData> s_layoutStyleData = new Dictionary<LayoutStyle, LayoutStyleData>
	{
		{
			LayoutStyle.Default,
			new LayoutStyleData
			{
				LayoutMap = new List<string> { "6x1", "6x1" },
				LayoutWidth = 12,
				LayoutHeight = 1,
				MaxLayoutCount = 1,
				TierOptionTags = new List<string>(),
				DividerOptionTags = new List<string>(),
				AdditionalTags = new List<string>()
			}
		},
		{
			LayoutStyle.XL,
			new LayoutStyleData
			{
				LayoutMap = new List<string> { "12x1" },
				LayoutWidth = 12,
				LayoutHeight = 1,
				MaxLayoutCount = 1,
				TierOptionTags = new List<string>(),
				DividerOptionTags = new List<string>(),
				AdditionalTags = new List<string>()
			}
		},
		{
			LayoutStyle.BigSmall,
			new LayoutStyleData
			{
				LayoutMap = new List<string> { "7x1", "5x1" },
				LayoutWidth = 12,
				LayoutHeight = 1,
				MaxLayoutCount = 1,
				TierOptionTags = new List<string>(),
				DividerOptionTags = new List<string>(),
				AdditionalTags = new List<string>()
			}
		},
		{
			LayoutStyle.SmallBig,
			new LayoutStyleData
			{
				LayoutMap = new List<string> { "5x1", "7x1" },
				LayoutWidth = 12,
				LayoutHeight = 1,
				MaxLayoutCount = 1,
				TierOptionTags = new List<string>(),
				DividerOptionTags = new List<string>(),
				AdditionalTags = new List<string>()
			}
		},
		{
			LayoutStyle.Standard,
			new LayoutStyleData
			{
				LayoutMap = new List<string> { "6x1", "6x1" },
				LayoutWidth = 12,
				LayoutHeight = 1,
				MaxLayoutCount = 3,
				TierOptionTags = new List<string>(),
				DividerOptionTags = new List<string>(),
				AdditionalTags = new List<string>()
			}
		},
		{
			LayoutStyle.StandardSingle,
			new LayoutStyleData
			{
				LayoutMap = new List<string> { "3x1", "3x1", "3x1", "3x1" },
				LayoutWidth = 12,
				LayoutHeight = 1,
				MaxLayoutCount = 3,
				TierOptionTags = new List<string>(),
				DividerOptionTags = new List<string>(),
				AdditionalTags = new List<string>()
			}
		},
		{
			LayoutStyle.Battlegrounds,
			new LayoutStyleData
			{
				LayoutMap = new List<string> { "3x1", "3x1", "3x1", "3x1" },
				LayoutWidth = 12,
				LayoutHeight = 1,
				MaxLayoutCount = 3,
				TierOptionTags = new List<string>(),
				DividerOptionTags = new List<string>(),
				AdditionalTags = new List<string>()
			}
		},
		{
			LayoutStyle.Expansion,
			new LayoutStyleData
			{
				LayoutMap = new List<string> { "3x1", "3x1", "3x1", "3x1" },
				LayoutWidth = 12,
				LayoutHeight = 1,
				MaxLayoutCount = 3,
				TierOptionTags = new List<string> { "pack_logo", "available_currencies" },
				DividerOptionTags = new List<string> { "empty" },
				AdditionalTags = new List<string> { "expansion_row" }
			}
		},
		{
			LayoutStyle.YearExpansion,
			new LayoutStyleData
			{
				LayoutMap = new List<string> { "4x1", "4x1", "4x1" },
				LayoutWidth = 12,
				LayoutHeight = 1,
				MaxLayoutCount = 3,
				TierOptionTags = new List<string>(),
				DividerOptionTags = new List<string> { "watermark", "header_plate" },
				AdditionalTags = new List<string> { "expansion_row" }
			}
		}
	};

	public static bool TryGetLayoutStyleData(LayoutStyle style, out LayoutStyleData maps)
	{
		if (!s_layoutStyleData.TryGetValue(style, out maps))
		{
			maps = s_layoutStyleData[LayoutStyle.Default];
			return false;
		}
		return true;
	}

	public static bool TryParseLayoutStyle(string style, out LayoutStyle layoutStyle)
	{
		switch (style.ToLower())
		{
		case "default":
			layoutStyle = LayoutStyle.Default;
			return true;
		case "xl":
		case "s":
			layoutStyle = LayoutStyle.XL;
			return true;
		case "bigsmall":
			layoutStyle = LayoutStyle.BigSmall;
			return true;
		case "smallbig":
			layoutStyle = LayoutStyle.SmallBig;
			return true;
		case "d":
		case "standard":
			layoutStyle = LayoutStyle.Standard;
			return true;
		case "standardsingle":
			layoutStyle = LayoutStyle.StandardSingle;
			return true;
		case "battlegrounds":
			layoutStyle = LayoutStyle.Battlegrounds;
			return true;
		case "expansion":
		case "3x1_expansion":
			layoutStyle = LayoutStyle.Expansion;
			return true;
		case "year_expansion":
		case "dragon":
		case "gryphon":
		case "hydra":
		case "pegasus":
		case "phoenix":
		case "raven":
		case "wolf":
		case "mammoth":
		case "kraken":
		case "years1and2":
			layoutStyle = LayoutStyle.YearExpansion;
			return true;
		default:
			layoutStyle = LayoutStyle.Default;
			return false;
		}
	}
}
