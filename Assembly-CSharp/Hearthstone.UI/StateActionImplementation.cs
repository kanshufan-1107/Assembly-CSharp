using System;
using System.Diagnostics;
using Hearthstone.UI.Internal;
using Hearthstone.UI.Logging;
using Hearthstone.UI.Scripting;
using UnityEngine;

namespace Hearthstone.UI;

public abstract class StateActionImplementation
{
	private struct RunOnInstanceOrTargetGameObjectArgs
	{
		public NestedReference NestedReference;

		public Action<WidgetInstance> WidgetInstanceCallback;

		public Action<GameObject> GameObjectCallback;

		public bool EnableInstance;

		public WidgetInstance WidgetInstance;
	}

	public StateAction StateAction { private get; set; }

	protected IDataModelProvider DataContext => StateAction.DataContext;

	protected IWidgetStateCollection StateCollectionContext => StateAction.StateCollectionContext;

	protected AnimationClip AnimationClip => StateAction.AnimationClip;

	protected double SecondsSinceRun => StateAction.SecondsSinceRun;

	protected string GetString(int index)
	{
		return StateAction.GetString(index);
	}

	protected bool GetBool(int index)
	{
		return StateAction.GetBool(index);
	}

	protected Override GetOverride(int index)
	{
		return StateAction.GetOverride(index);
	}

	protected ScriptString GetValueScript(int index)
	{
		return StateAction.GetValueScript(index);
	}

	protected int GetIntValueAtIndex(int index)
	{
		StateAction.TryGetIntValueAtIndex(index, out var value);
		return value;
	}

	protected float GetFloatValueAtIndex(int index)
	{
		StateAction.TryGetFloatValueAtIndex(index, out var value);
		return value;
	}

	protected bool GetBoolValueAtIndex(int index)
	{
		StateAction.TryGetBoolValueAtIndex(index, out var value);
		return value;
	}

	protected WeakAssetReference GetAssetAtIndex(int index)
	{
		StateAction.TryGetAssetAtIndex(index, out var value);
		return value;
	}

	protected string GetStateName()
	{
		VisualController visualController = StateAction.DataContext as VisualController;
		if (visualController != null && !string.IsNullOrEmpty(visualController.RequestedState))
		{
			return visualController.RequestedState;
		}
		return "UNKNOWN";
	}

	protected void Complete(bool success)
	{
		StateAction.CompleteAsyncOperation((!success) ? AsyncOperationResult.Failure : AsyncOperationResult.Success);
	}

	protected void Complete(AsyncOperationResult result)
	{
		StateAction.CompleteAsyncOperation(result);
	}

	[Conditional("UNITY_EDITOR")]
	protected void Log(string message, string type, LogLevel level = LogLevel.Info)
	{
		Hearthstone.UI.Logging.Log.Get().AddMessage(message, StateAction, level, type);
	}

	protected void RunOnInstanceOrTargetGameObject(NestedReference nestedReference, bool enableInstance, Action<WidgetInstance> instanceCallback, Action<GameObject> gameObjectCallback)
	{
		nestedReference.RegisterReadyListener(HandleNestedReferenceReady, new RunOnInstanceOrTargetGameObjectArgs
		{
			NestedReference = nestedReference,
			WidgetInstanceCallback = instanceCallback,
			GameObjectCallback = gameObjectCallback,
			EnableInstance = enableInstance
		});
	}

	private void HandleNestedReferenceReady(object payload)
	{
		RunOnInstanceOrTargetGameObjectArgs args = (RunOnInstanceOrTargetGameObjectArgs)payload;
		args.NestedReference.RemoveReadyOrInactiveListener();
		if (!args.NestedReference.Resolve(out GameObject gameObject).IsSuccessful)
		{
			Complete(success: false);
			return;
		}
		WidgetInstance instance = gameObject.GetComponent<WidgetInstance>();
		if (instance != null)
		{
			if (args.EnableInstance)
			{
				instance.enabled = true;
				instance.gameObject.SetActive(value: true);
			}
			if (instance.IsReady)
			{
				if (instance.Widget == null)
				{
					Complete(success: false);
				}
				else
				{
					args.WidgetInstanceCallback(instance);
				}
			}
			else
			{
				args.WidgetInstance = instance;
				instance.RegisterReadyListener(HandleWidgetInstanceReady, args);
			}
		}
		else
		{
			args.GameObjectCallback(gameObject);
		}
	}

	private void HandleWidgetInstanceReady(object payload)
	{
		RunOnInstanceOrTargetGameObjectArgs args = (RunOnInstanceOrTargetGameObjectArgs)payload;
		args.WidgetInstanceCallback(args.WidgetInstance);
	}

	public abstract void Run(bool loadSynchronously = false);

	public virtual void Update()
	{
	}
}
