using Hearthstone.UI.Scripting;

namespace Hearthstone.UI;

public class SendEventStateAction : StateActionImplementation
{
	public override void Run(bool loadSynchronously = false)
	{
		GetOverride(0).RegisterReadyListener(HandleReady);
	}

	private void HandleReady(object unused)
	{
		GetOverride(0).RemoveReadyOrInactiveListener(HandleReady);
		Override @override = GetOverride(0);
		string eventName = GetString(0);
		bool found = false;
		if (@override.Resolve(out var target))
		{
			string stateName = GetStateName();
			ScriptString script = GetValueScript(0);
			ScriptContext scriptContext = null;
			ScriptContext.EvaluationResults? evaluationResults = null;
			if (!string.IsNullOrEmpty(script.Script))
			{
				scriptContext = new ScriptContext();
				evaluationResults = scriptContext.Evaluate(script.Script, base.DataContext, base.StateCollectionContext);
			}
			TriggerEventParameters parameters = new TriggerEventParameters(stateName, evaluationResults.HasValue ? evaluationResults.Value.Value : null, noDownwardPropagation: true, ignorePlaymaker: true);
			found = EventFunctions.TriggerEvent(target.transform, eventName, parameters);
		}
		Complete(success: true);
	}
}
