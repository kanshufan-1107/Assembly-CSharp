using System.Collections.Generic;
using System.Linq;

namespace Hearthstone.UI;

public static class DataModelListExtensions
{
	public static DataModelList<T> ToDataModelList<T>(this IEnumerable<T> source) where T : class
	{
		return source.Aggregate(new DataModelList<T>(), delegate(DataModelList<T> acc, T element)
		{
			acc.Add(element);
			return acc;
		});
	}

	public static void OverwriteDataModels<T>(this DataModelList<T> inputDataModel, DataModelList<T> newDataModels) where T : IDataModel
	{
		for (int i = 0; i < newDataModels.Count; i++)
		{
			if (i < inputDataModel.Count)
			{
				inputDataModel[i] = newDataModels[i];
			}
			else
			{
				inputDataModel.Add(newDataModels[i]);
			}
		}
		while (inputDataModel.Count > newDataModels.Count)
		{
			inputDataModel.RemoveAt(inputDataModel.Count - 1);
		}
	}
}
