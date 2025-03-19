using System.Collections.Generic;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.DataModels;

public class AdventureLoadoutOptionDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 4;

	private bool m_Locked;

	private bool m_Completed;

	private bool m_NewlyUnlocked;

	private string m_LockedText;

	private string m_Name;

	private bool m_IsSelectedOption;

	private Material m_DisplayTexture;

	private Color m_DisplayColor;

	private bool m_IsUpgraded;

	private DataModelList<ShopDeckPouchDisplay.DKRuneTypes> m_DKRunes = new DataModelList<ShopDeckPouchDisplay.DKRuneTypes>();

	private DataModelProperty[] m_properties = new DataModelProperty[10]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "locked",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "completed",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "newly_unlocked",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "locked_text",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "name",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "is_selected_option",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "display_texture",
			Type = typeof(Material)
		},
		new DataModelProperty
		{
			PropertyId = 7,
			PropertyDisplayName = "display_color",
			Type = typeof(Color)
		},
		new DataModelProperty
		{
			PropertyId = 8,
			PropertyDisplayName = "is_upgraded",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 737,
			PropertyDisplayName = "dk_runes",
			Type = typeof(DataModelList<ShopDeckPouchDisplay.DKRuneTypes>)
		}
	};

	public int DataModelId => 4;

	public string DataModelDisplayName => "adventure_loadout_option";

	public bool Locked
	{
		get
		{
			return m_Locked;
		}
		set
		{
			if (m_Locked != value)
			{
				m_Locked = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool Completed
	{
		get
		{
			return m_Completed;
		}
		set
		{
			if (m_Completed != value)
			{
				m_Completed = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool NewlyUnlocked
	{
		get
		{
			return m_NewlyUnlocked;
		}
		set
		{
			if (m_NewlyUnlocked != value)
			{
				m_NewlyUnlocked = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string LockedText
	{
		get
		{
			return m_LockedText;
		}
		set
		{
			if (!(m_LockedText == value))
			{
				m_LockedText = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string Name
	{
		get
		{
			return m_Name;
		}
		set
		{
			if (!(m_Name == value))
			{
				m_Name = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsSelectedOption
	{
		get
		{
			return m_IsSelectedOption;
		}
		set
		{
			if (m_IsSelectedOption != value)
			{
				m_IsSelectedOption = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public Material DisplayTexture
	{
		get
		{
			return m_DisplayTexture;
		}
		set
		{
			if (!(m_DisplayTexture == value))
			{
				m_DisplayTexture = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public Color DisplayColor
	{
		get
		{
			return m_DisplayColor;
		}
		set
		{
			if (!(m_DisplayColor == value))
			{
				m_DisplayColor = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public bool IsUpgraded
	{
		get
		{
			return m_IsUpgraded;
		}
		set
		{
			if (m_IsUpgraded != value)
			{
				m_IsUpgraded = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<ShopDeckPouchDisplay.DKRuneTypes> DKRunes
	{
		get
		{
			return m_DKRunes;
		}
		set
		{
			if (m_DKRunes != value)
			{
				RemoveNestedDataModel(m_DKRunes);
				RegisterNestedDataModel(value);
				m_DKRunes = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public AdventureLoadoutOptionDataModel()
	{
		RegisterNestedDataModel(m_DKRunes);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		int num = hash * 31;
		_ = m_Locked;
		hash = num + m_Locked.GetHashCode();
		int num2 = hash * 31;
		_ = m_Completed;
		hash = num2 + m_Completed.GetHashCode();
		int num3 = hash * 31;
		_ = m_NewlyUnlocked;
		hash = num3 + m_NewlyUnlocked.GetHashCode();
		hash = hash * 31 + ((m_LockedText != null) ? m_LockedText.GetHashCode() : 0);
		hash = hash * 31 + ((m_Name != null) ? m_Name.GetHashCode() : 0);
		int num4 = hash * 31;
		_ = m_IsSelectedOption;
		hash = num4 + m_IsSelectedOption.GetHashCode();
		hash = hash * 31 + ((m_DisplayTexture != null) ? m_DisplayTexture.GetHashCode() : 0);
		int num5 = hash * 31;
		_ = m_DisplayColor;
		hash = num5 + m_DisplayColor.GetHashCode();
		int num6 = hash * 31;
		_ = m_IsUpgraded;
		hash = num6 + m_IsUpgraded.GetHashCode();
		if (m_DKRunes != null && !inspectedDataModels.Contains(m_DKRunes.GetHashCode()))
		{
			inspectedDataModels.Add(m_DKRunes.GetHashCode());
			return hash * 31 + m_DKRunes.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Locked;
			return true;
		case 1:
			value = m_Completed;
			return true;
		case 2:
			value = m_NewlyUnlocked;
			return true;
		case 3:
			value = m_LockedText;
			return true;
		case 4:
			value = m_Name;
			return true;
		case 5:
			value = m_IsSelectedOption;
			return true;
		case 6:
			value = m_DisplayTexture;
			return true;
		case 7:
			value = m_DisplayColor;
			return true;
		case 8:
			value = m_IsUpgraded;
			return true;
		case 737:
			value = m_DKRunes;
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
			Locked = value != null && (bool)value;
			return true;
		case 1:
			Completed = value != null && (bool)value;
			return true;
		case 2:
			NewlyUnlocked = value != null && (bool)value;
			return true;
		case 3:
			LockedText = ((value != null) ? ((string)value) : null);
			return true;
		case 4:
			Name = ((value != null) ? ((string)value) : null);
			return true;
		case 5:
			IsSelectedOption = value != null && (bool)value;
			return true;
		case 6:
			DisplayTexture = ((value != null) ? ((Material)value) : null);
			return true;
		case 7:
			DisplayColor = ((value != null) ? ((Color)value) : default(Color));
			return true;
		case 8:
			IsUpgraded = value != null && (bool)value;
			return true;
		case 737:
			DKRunes = ((value != null) ? ((DataModelList<ShopDeckPouchDisplay.DKRuneTypes>)value) : null);
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
		case 737:
			info = Properties[9];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
