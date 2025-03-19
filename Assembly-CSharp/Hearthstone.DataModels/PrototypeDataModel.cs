using System.Collections.Generic;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.DataModels;

public class PrototypeDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 1;

	private bool m_Bool1;

	private int m_Int1;

	private float m_Float1;

	private string m_String1;

	private Texture m_Texture1;

	private Material m_Material1;

	private DataModelList<PrototypeDataModel> m_List1 = new DataModelList<PrototypeDataModel>();

	private DataModelProperty[] m_properties = new DataModelProperty[7]
	{
		new DataModelProperty
		{
			PropertyId = 0,
			PropertyDisplayName = "bool1",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 1,
			PropertyDisplayName = "int1",
			Type = typeof(int)
		},
		new DataModelProperty
		{
			PropertyId = 2,
			PropertyDisplayName = "float1",
			Type = typeof(float)
		},
		new DataModelProperty
		{
			PropertyId = 3,
			PropertyDisplayName = "string1",
			Type = typeof(string)
		},
		new DataModelProperty
		{
			PropertyId = 4,
			PropertyDisplayName = "texture1",
			Type = typeof(Texture)
		},
		new DataModelProperty
		{
			PropertyId = 5,
			PropertyDisplayName = "material1",
			Type = typeof(Material)
		},
		new DataModelProperty
		{
			PropertyId = 6,
			PropertyDisplayName = "list1",
			Type = typeof(DataModelList<PrototypeDataModel>)
		}
	};

	public int DataModelId => 1;

	public string DataModelDisplayName => "prototype";

	public bool Bool1
	{
		get
		{
			return m_Bool1;
		}
		set
		{
			if (m_Bool1 != value)
			{
				m_Bool1 = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public int Int1
	{
		get
		{
			return m_Int1;
		}
		set
		{
			if (m_Int1 != value)
			{
				m_Int1 = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public float Float1
	{
		get
		{
			return m_Float1;
		}
		set
		{
			if (m_Float1 != value)
			{
				m_Float1 = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string String1
	{
		get
		{
			return m_String1;
		}
		set
		{
			if (!(m_String1 == value))
			{
				m_String1 = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public Texture Texture1
	{
		get
		{
			return m_Texture1;
		}
		set
		{
			if (!(m_Texture1 == value))
			{
				m_Texture1 = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public Material Material1
	{
		get
		{
			return m_Material1;
		}
		set
		{
			if (!(m_Material1 == value))
			{
				m_Material1 = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelList<PrototypeDataModel> List1
	{
		get
		{
			return m_List1;
		}
		set
		{
			if (m_List1 != value)
			{
				RemoveNestedDataModel(m_List1);
				RegisterNestedDataModel(value);
				m_List1 = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public DataModelProperty[] Properties => m_properties;

	public PrototypeDataModel()
	{
		RegisterNestedDataModel(m_List1);
	}

	public int GetPropertiesHashCode(HashSet<int> inspectedDataModels = null)
	{
		if (inspectedDataModels == null)
		{
			inspectedDataModels = new HashSet<int>();
		}
		int hash = 17;
		int num = hash * 31;
		_ = m_Bool1;
		hash = num + m_Bool1.GetHashCode();
		int num2 = hash * 31;
		_ = m_Int1;
		hash = num2 + m_Int1.GetHashCode();
		int num3 = hash * 31;
		_ = m_Float1;
		hash = num3 + m_Float1.GetHashCode();
		hash = hash * 31 + ((m_String1 != null) ? m_String1.GetHashCode() : 0);
		hash = hash * 31 + ((m_Texture1 != null) ? m_Texture1.GetHashCode() : 0);
		hash = hash * 31 + ((m_Material1 != null) ? m_Material1.GetHashCode() : 0);
		if (m_List1 != null && !inspectedDataModels.Contains(m_List1.GetHashCode()))
		{
			inspectedDataModels.Add(m_List1.GetHashCode());
			return hash * 31 + m_List1.GetPropertiesHashCode(inspectedDataModels);
		}
		return hash * 31;
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 0:
			value = m_Bool1;
			return true;
		case 1:
			value = m_Int1;
			return true;
		case 2:
			value = m_Float1;
			return true;
		case 3:
			value = m_String1;
			return true;
		case 4:
			value = m_Texture1;
			return true;
		case 5:
			value = m_Material1;
			return true;
		case 6:
			value = m_List1;
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
			Bool1 = value != null && (bool)value;
			return true;
		case 1:
			Int1 = ((value != null) ? ((int)value) : 0);
			return true;
		case 2:
			Float1 = ((value != null) ? ((float)value) : 0f);
			return true;
		case 3:
			String1 = ((value != null) ? ((string)value) : null);
			return true;
		case 4:
			Texture1 = ((value != null) ? ((Texture)value) : null);
			return true;
		case 5:
			Material1 = ((value != null) ? ((Material)value) : null);
			return true;
		case 6:
			List1 = ((value != null) ? ((DataModelList<PrototypeDataModel>)value) : null);
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
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
