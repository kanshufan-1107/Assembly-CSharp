public class LayoutMapEntry
{
	public int UnitWidth { get; set; }

	public int UnitHeight { get; set; }

	public int OriginX { get; set; } = -1;

	public int OriginY { get; set; } = -1;

	public override string ToString()
	{
		return $"Slot {UnitWidth}/{UnitHeight}, Origin {OriginX}/{OriginY}";
	}

	public LayoutMapEntry Clone()
	{
		return new LayoutMapEntry
		{
			UnitWidth = UnitWidth,
			UnitHeight = UnitHeight,
			OriginX = OriginX,
			OriginY = OriginY
		};
	}
}
