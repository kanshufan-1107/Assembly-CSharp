using System.Collections.Generic;
using Assets;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class BattlegroundsEmoteDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 638;

	private int m_EmoteDbiId;

	private string m_DisplayName;

	private string m_Description;

	private bool m_IsOwned;

	private bool m_IsNew;

	private string m_Animation;

	private bool m_IsEquipped;

	private bool m_IsAnimating;

	private BattlegroundsEmote.Bordertype m_BorderType;

	private float m_XOffset;

	private float m_ZOffset;

	private string m_Rarity;

	private DataModelProperty[] m_properties = new DataModelProperty[12]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "emote_dbi_id",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "display_name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "description",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "is_owned",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "is_new",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "animation",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "is_equipped",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "is_animating",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "border_type",
			Type = typeof(BattlegroundsEmote.Bordertype)
		},
		new DataModelProperty
		{
			PropertyId = 9,
			PropertyDisplayName = "x_offset",
			Type = typeof(float)
		},
		new DataModelProperty
		{
			PropertyId = 10,
			PropertyDisplayName = "z_offset",
			Type = typeof(float)
		},
		new DataModelProperty
		{
			PropertyId = 11,
			PropertyDisplayName = "rarity",
			Type = typeof(string)
		}
	};

	public int DataModelId => 638;

	public string DataModelDisplayName => "battlegrounds_emote";

	public int EmoteDbiId
	{
		get
		{
			return m_EmoteDbiId;
		}
		set
		{
			if (m_EmoteDbiId != value)
			{
				m_EmoteDbiId = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string DisplayName
	{
		get
		{
			return m_DisplayName;
		}
		set
		{
			if (!(m_DisplayName == value))
			{
				m_DisplayName = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string Description
	{
		get
		{
			return m_Description;
		}
		set
		{
			if (!(m_Description == value))
			{
				m_Description = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsOwned
	{
		get
		{
			return m_IsOwned;
		}
		set
		{
			if (m_IsOwned != value)
			{
				m_IsOwned = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsNew
	{
		get
		{
			return m_IsNew;
		}
		set
		{
			if (m_IsNew != value)
			{
				m_IsNew = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string Animation
	{
		get
		{
			return m_Animation;
		}
		set
		{
			if (!(m_Animation == value))
			{
				m_Animation = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsEquipped
	{
		get
		{
			return m_IsEquipped;
		}
		set
		{
			if (m_IsEquipped != value)
			{
				m_IsEquipped = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsAnimating
	{
		get
		{
			return m_IsAnimating;
		}
		set
		{
			if (m_IsAnimating != value)
			{
				m_IsAnimating = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public BattlegroundsEmote.Bordertype BorderType
	{
		get
		{
			return m_BorderType;
		}
		set
		{
			if (m_BorderType != value)
			{
				m_BorderType = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public float XOffset
	{
		get
		{
			return m_XOffset;
		}
		set
		{
			if (m_XOffset != value)
			{
				m_XOffset = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public float ZOffset
	{
		get
		{
			return m_ZOffset;
		}
		set
		{
			if (m_ZOffset != value)
			{
				m_ZOffset = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string Rarity
	{
		get
		{
			return m_Rarity;
		}
		set
		{
			if (!(m_Rarity == value))
			{
				m_Rarity = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int num = 17 * 31;
		_ = m_EmoteDbiId;
		int num2 = (((num + m_EmoteDbiId.GetHashCode()) * 31 + ((m_DisplayName != null) ? m_DisplayName.GetHashCode() : 0)) * 31 + ((m_Description != null) ? m_Description.GetHashCode() : 0)) * 31;
		_ = m_IsOwned;
		int num3 = (num2 + m_IsOwned.GetHashCode()) * 31;
		_ = m_IsNew;
		int num4 = ((num3 + m_IsNew.GetHashCode()) * 31 + ((m_Animation != null) ? m_Animation.GetHashCode() : 0)) * 31;
		_ = m_IsEquipped;
		int num5 = (num4 + m_IsEquipped.GetHashCode()) * 31;
		_ = m_IsAnimating;
		int num6 = (num5 + m_IsAnimating.GetHashCode()) * 31;
		_ = m_BorderType;
		int num7 = (num6 + m_BorderType.GetHashCode()) * 31;
		_ = m_XOffset;
		int num8 = (num7 + m_XOffset.GetHashCode()) * 31;
		_ = m_ZOffset;
		return (num8 + m_ZOffset.GetHashCode()) * 31 + ((m_Rarity != null) ? m_Rarity.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_EmoteDbiId;
			return true;
		case 1:
			value = m_DisplayName;
			return true;
		case 2:
			value = m_Description;
			return true;
		case 3:
			value = m_IsOwned;
			return true;
		case 4:
			value = m_IsNew;
			return true;
		case 5:
			value = m_Animation;
			return true;
		case 6:
			value = m_IsEquipped;
			return true;
		case 7:
			value = m_IsAnimating;
			return true;
		case 8:
			value = m_BorderType;
			return true;
		case 9:
			value = m_XOffset;
			return true;
		case 10:
			value = m_ZOffset;
			return true;
		case 11:
			value = m_Rarity;
			return true;
		default:
			value = null;
			return false;
		}
	}

	public bool SetPropertyValue(int id, object value)
	{
		switch (id)
		{
		case 0:
			EmoteDbiId = ((value != null) ? ((int)value) : 0);
			return true;
		case 1:
			DisplayName = ((value != null) ? ((string)value) : null);
			return true;
		case 2:
			Description = ((value != null) ? ((string)value) : null);
			return true;
		case 3:
			IsOwned = value != null && (bool)value;
			return true;
		case 4:
			IsNew = value != null && (bool)value;
			return true;
		case 5:
			Animation = ((value != null) ? ((string)value) : null);
			return true;
		case 6:
			IsEquipped = value != null && (bool)value;
			return true;
		case 7:
			IsAnimating = value != null && (bool)value;
			return true;
		case 8:
			BorderType = ((value != null) ? ((BattlegroundsEmote.Bordertype)value) : BattlegroundsEmote.Bordertype.NONE);
			return true;
		case 9:
			XOffset = ((value != null) ? ((float)value) : 0f);
			return true;
		case 10:
			ZOffset = ((value != null) ? ((float)value) : 0f);
			return true;
		case 11:
			Rarity = ((value != null) ? ((string)value) : null);
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 0:
			info = Properties[0];
			return true;
		case 1:
			info = Properties[1];
			return true;
		case 2:
			info = Properties[2];
			return true;
		case 3:
			info = Properties[3];
			return true;
		case 4:
			info = Properties[4];
			return true;
		case 5:
			info = Properties[5];
			return true;
		case 6:
			info = Properties[6];
			return true;
		case 7:
			info = Properties[7];
			return true;
		case 8:
			info = Properties[8];
			return true;
		case 9:
			info = Properties[9];
			return true;
		case 10:
			info = Properties[10];
			return true;
		case 11:
			info = Properties[11];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
