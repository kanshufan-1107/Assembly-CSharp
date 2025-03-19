using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Hearthstone.MarketingImages;

public class MarketingImageContent
{
	[CompilerGenerated]
	private string _003CEntryTitle_003Ek__BackingField;

	[CompilerGenerated]
	private string _003CTitle_003Ek__BackingField;

	[CompilerGenerated]
	private DateTime _003CPublishDate_003Ek__BackingField;

	[CompilerGenerated]
	private DateTime _003CBeginningDate_003Ek__BackingField;

	[CompilerGenerated]
	private DateTime _003CExpiryDate_003Ek__BackingField;

	[CompilerGenerated]
	private int _003CMinGameVersion_003Ek__BackingField;

	[CompilerGenerated]
	private int _003CMaxGameVersion_003Ek__BackingField;

	public string UID { get; set; }

	public string EntryTitle
	{
		[CompilerGenerated]
		set
		{
			_003CEntryTitle_003Ek__BackingField = value;
		}
	}

	public HashSet<string> Tags { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	public string Title
	{
		[CompilerGenerated]
		set
		{
			_003CTitle_003Ek__BackingField = value;
		}
	}

	public string TextureAssetUrl { get; set; }

	public float TextureOffsetX { get; set; }

	public float TextureOffsetY { get; set; }

	public DateTime PublishDate
	{
		[CompilerGenerated]
		set
		{
			_003CPublishDate_003Ek__BackingField = value;
		}
	}

	public DateTime BeginningDate
	{
		[CompilerGenerated]
		set
		{
			_003CBeginningDate_003Ek__BackingField = value;
		}
	}

	public DateTime ExpiryDate
	{
		[CompilerGenerated]
		set
		{
			_003CExpiryDate_003Ek__BackingField = value;
		}
	}

	public int MinGameVersion
	{
		[CompilerGenerated]
		set
		{
			_003CMinGameVersion_003Ek__BackingField = value;
		}
	}

	public int MaxGameVersion
	{
		[CompilerGenerated]
		set
		{
			_003CMaxGameVersion_003Ek__BackingField = value;
		}
	}

	public long ProductId { get; set; }

	public MarketingImageSlot SlotCompatibility { get; set; }

	public bool AutoFrame { get; set; } = true;
}
