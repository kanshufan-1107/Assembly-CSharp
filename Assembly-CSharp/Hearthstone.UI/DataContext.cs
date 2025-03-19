using System;
using System.Collections.Generic;
using System.Linq;

namespace Hearthstone.UI;

public class DataContext : IDataModelProvider
{
	public delegate void DataChangedDelegate(IDataModel dataModel);

	public static int DataVersion;

	private int m_localDataVersion = 1;

	private readonly Dictionary<int, IDataModel> m_modelMap = new Dictionary<int, IDataModel>();

	private DataChangedDelegate m_onDataChanged;

	private Action<object> m_handleDataModelChangedMethod;

	private Action<object> HandleDataModelChangedMethod
	{
		get
		{
			if (m_handleDataModelChangedMethod == null)
			{
				m_handleDataModelChangedMethod = HandleDataModelChanged;
			}
			return m_handleDataModelChangedMethod;
		}
	}

	public void BindDataModel(IDataModel model)
	{
		if (model == null)
		{
			Log.All.PrintError("DataContext::BindDataModel() Attempted to bind null datamodel");
		}
		else if (!HasDataModelInstance(model))
		{
			if (m_modelMap.ContainsKey(model.DataModelId))
			{
				m_modelMap[model.DataModelId].RemoveChangedListener(HandleDataModelChangedMethod);
			}
			m_modelMap[model.DataModelId] = model;
			model.RegisterChangedListener(HandleDataModelChangedMethod, model);
			HandleDataChange(model);
			DataVersion++;
		}
	}

	public void UnbindDataModel(int id)
	{
		if (m_modelMap.ContainsKey(id))
		{
			IDataModel model = m_modelMap[id];
			model.RemoveChangedListener(HandleDataModelChangedMethod);
			HandleDataChange(model);
			m_modelMap.Remove(id);
			DataVersion++;
		}
	}

	public void UnbindAllDataModels()
	{
		foreach (KeyValuePair<int, IDataModel> item in m_modelMap)
		{
			IDataModel model = item.Value;
			model.RemoveChangedListener(HandleDataModelChangedMethod);
			HandleDataChange(model);
		}
		m_modelMap.Clear();
		DataVersion++;
	}

	public bool GetDataModel(int id, out IDataModel model)
	{
		return m_modelMap.TryGetValue(id, out model);
	}

	public bool HasDataModel(int id)
	{
		return m_modelMap.ContainsKey(id);
	}

	public bool HasDataModelInstance(IDataModel dataModel)
	{
		IDataModel bindingDataModel = null;
		if (dataModel == null || !GetDataModel(dataModel.DataModelId, out bindingDataModel))
		{
			return false;
		}
		return bindingDataModel == dataModel;
	}

	public ICollection<IDataModel> GetDataModels()
	{
		return m_modelMap.Values;
	}

	public void Clear()
	{
		IDataModel[] array = m_modelMap.Values.ToArray();
		foreach (IDataModel model in array)
		{
			model.RemoveChangedListener(HandleDataModelChangedMethod);
			HandleDataChange(model);
		}
		m_modelMap.Clear();
		DataVersion++;
	}

	private void HandleDataChange(IDataModel dataModel)
	{
		m_localDataVersion++;
		m_onDataChanged?.Invoke(dataModel);
	}

	public int GetLocalDataVersion()
	{
		return m_localDataVersion;
	}

	public void RegisterChangedListener(DataChangedDelegate listener)
	{
		m_onDataChanged = (DataChangedDelegate)Delegate.Remove(m_onDataChanged, listener);
		m_onDataChanged = (DataChangedDelegate)Delegate.Combine(m_onDataChanged, listener);
	}

	public void RemoveChangedListener(DataChangedDelegate listener)
	{
		m_onDataChanged = (DataChangedDelegate)Delegate.Remove(m_onDataChanged, listener);
	}

	private void HandleDataModelChanged(object payload)
	{
		HandleDataChange((IDataModel)payload);
	}
}
