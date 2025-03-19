using System.Collections.Generic;
using Hearthstone.UI;

namespace Hearthstone.DataModels;

public class TutorialPreviewDataModel : DataModelEventDispatcher, IDataModel, IDataModelProperties
{
	public const int ModelId = 476;

	private bool m_IsNewPlayer;

	private string m_SelectedMode;

	private DataModelProperty[] m_properties = new DataModelProperty[2]
	{
		new DataModelProperty
		{
			PropertyId = 477,
			PropertyDisplayName = "is_new_player",
			Type = typeof(bool)
		},
		new DataModelProperty
		{
			PropertyId = 549,
			PropertyDisplayName = "selected_mode",
			Type = typeof(string)
		}
	};

	public int DataModelId => 476;

	public string DataModelDisplayName => "tutorial_preview";

	public bool IsNewPlayer
	{
		get
		{
			return m_IsNewPlayer;
		}
		set
		{
			if (m_IsNewPlayer != value)
			{
				m_IsNewPlayer = value;
				DispatchChangedListeners();
				DataContext.DataVersion++;
			}
		}
	}

	public string SelectedMode
	{
		get
		{
			return m_SelectedMode;
		}
		set
		{
			if (!(m_SelectedMode == value))
			{
				m_SelectedMode = value;
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
		_ = m_IsNewPlayer;
		return (num + m_IsNewPlayer.GetHashCode()) * 31 + ((m_SelectedMode != null) ? m_SelectedMode.GetHashCode() : 0);
	}

	public bool GetPropertyValue(int id, out object value)
	{
		switch (id)
		{
		case 477:
			value = m_IsNewPlayer;
			return true;
		case 549:
			value = m_SelectedMode;
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
		case 477:
			IsNewPlayer = value != null && (bool)value;
			return true;
		case 549:
			SelectedMode = ((value != null) ? ((string)value) : null);
			return true;
		default:
			return false;
		}
	}

	public bool GetPropertyInfo(int id, out DataModelProperty info)
	{
		switch (id)
		{
		case 477:
			info = Properties[0];
			return true;
		case 549:
			info = Properties[1];
			return true;
		default:
			info = default(DataModelProperty);
			return false;
		}
	}
}
