using System;

namespace Hearthstone.UI;

public static class DataModelExtensions
{
	public static T CloneDataModel<T>(this T inputDataModel) where T : IDataModel
	{
		if (inputDataModel == null)
		{
			return default(T);
		}
		T outputDataModel = (T)Activator.CreateInstance(inputDataModel.GetType());
		DataModelProperty[] properties = inputDataModel.Properties;
		for (int i = 0; i < properties.Length; i++)
		{
			DataModelProperty inputProperty = properties[i];
			if (inputDataModel.GetPropertyValue(inputProperty.PropertyId, out var value))
			{
				outputDataModel.SetPropertyValue(inputProperty.PropertyId, value);
			}
		}
		return outputDataModel;
	}

	public static T CopyFromDataModel<T>(this T inputDataModel, T copiedDataModel) where T : IDataModel
	{
		if (inputDataModel == null)
		{
			return default(T);
		}
		if (copiedDataModel == null)
		{
			return inputDataModel;
		}
		DataModelProperty[] properties = inputDataModel.Properties;
		for (int i = 0; i < properties.Length; i++)
		{
			DataModelProperty inputProperty = properties[i];
			if (copiedDataModel.GetPropertyValue(inputProperty.PropertyId, out var value))
			{
				inputDataModel.SetPropertyValue(inputProperty.PropertyId, value);
			}
		}
		return inputDataModel;
	}
}
