using PegasusShared;

internal static class VisualsFormatTypeExtensions
{
	public static VisualsFormatType GetCurrentVisualsFormatType()
	{
		return ToVisualsFormatType(Options.GetFormatType(), Options.GetInRankedPlayMode());
	}

	public static VisualsFormatType ToVisualsFormatType(FormatType formatType, bool inRankedPlayMode)
	{
		if (!inRankedPlayMode)
		{
			return VisualsFormatType.VFT_CASUAL;
		}
		return formatType switch
		{
			FormatType.FT_WILD => VisualsFormatType.VFT_WILD, 
			FormatType.FT_STANDARD => VisualsFormatType.VFT_STANDARD, 
			FormatType.FT_CLASSIC => VisualsFormatType.VFT_CLASSIC, 
			FormatType.FT_TWIST => VisualsFormatType.VFT_TWIST, 
			_ => VisualsFormatType.VFT_UNKNOWN, 
		};
	}

	public static FormatType ToFormatType(this VisualsFormatType visualsFormatType)
	{
		return visualsFormatType switch
		{
			VisualsFormatType.VFT_CASUAL => DeckPickerTrayDisplay.Get().GetSelectedCollectionDeck()?.FormatType ?? FormatType.FT_STANDARD, 
			VisualsFormatType.VFT_WILD => FormatType.FT_WILD, 
			VisualsFormatType.VFT_STANDARD => FormatType.FT_STANDARD, 
			VisualsFormatType.VFT_CLASSIC => FormatType.FT_CLASSIC, 
			VisualsFormatType.VFT_TWIST => FormatType.FT_TWIST, 
			_ => FormatType.FT_UNKNOWN, 
		};
	}

	public static bool IsRanked(this VisualsFormatType visualsFormatType)
	{
		if (visualsFormatType == VisualsFormatType.VFT_UNKNOWN || visualsFormatType == VisualsFormatType.VFT_CASUAL)
		{
			return false;
		}
		return true;
	}
}
