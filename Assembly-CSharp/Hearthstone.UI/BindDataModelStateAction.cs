using Blizzard.T5.Core.Utils;
using Hearthstone.UI.Scripting;

namespace Hearthstone.UI;

public class BindDataModelStateAction : StateActionImplementation
{
	private ScriptContext m_valueContext;

	public override void Run(bool loadSynchronously = false)
	{
		GetOverride(0).RegisterReadyListener(HandleReady);
	}

	private void HandleReady(object unused)
	{
		GetOverride(0).RemoveReadyOrInactiveListener(HandleReady);
		if (!GetOverride(0).Resolve(out var go))
		{
			Complete(success: false);
			return;
		}
		ScriptString valueScript = GetValueScript(0);
		if (string.IsNullOrEmpty(valueScript.Script))
		{
			Complete(success: false);
			return;
		}
		if (m_valueContext == null)
		{
			m_valueContext = new ScriptContext();
		}
		ScriptContext.EvaluationResults valueResults = m_valueContext.Evaluate(valueScript.Script, base.DataContext);
		if (valueResults.ErrorCode != 0 || valueResults.Value == null)
		{
			Complete(success: false);
			return;
		}
		IDataModel dataModelValue = ((!(valueResults.Value is IDataModelList list)) ? (valueResults.Value as IDataModel) : (list.GetElementAtIndex(0) as IDataModel));
		WidgetTemplate targetTemplate = go.GetComponent<WidgetTemplate>() ?? GameObjectUtils.FindComponentInParents<WidgetTemplate>(go);
		if (dataModelValue == null || targetTemplate == null)
		{
			Complete(success: false);
			return;
		}
		bool success = targetTemplate.BindDataModel(dataModelValue, go);
		Complete(success);
	}
}
