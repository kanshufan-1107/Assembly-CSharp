using System;
using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using Hearthstone.DataModels;
using Hearthstone.UI.Scripting;
using Unity.Profiling;
using UnityEngine;

namespace Hearthstone.UI;

[ExecuteAlways]
[AddComponentMenu("")]
public class AnimationOverrideWidgetBehaviour : CustomWidgetBehavior
{
	[Tooltip("The Animator component that owns the Animator Controller that will be overridden")]
	[SerializeField]
	private Animator m_animator;

	[SerializeField]
	[Tooltip("A data model script that needs to evaluate to the name and GUID of an Animator Controller.")]
	private ScriptString m_valueScript;

	private bool m_isLoading;

	private int m_lastDataVerion;

	private int m_asyncOperationId;

	private string m_currentControllerId;

	private int m_startedAssetLoadCount;

	private int m_failedAssetLoadCount;

	private float m_displayFrameNormalizedTime;

	private const string DisplayFrameAnimationEventName = "DisplayFrame";

	private static ProfilerMarker s_onUpdateProfilerMarker = new ProfilerMarker("AnimationOverrideWidgetBehaviour.OnUpdate");

	public override bool IsChangingStates => m_isLoading;

	protected override void OnInitialize()
	{
	}

	public override void OnUpdate()
	{
		using (s_onUpdateProfilerMarker.Auto())
		{
			int dataVersion = GetLocalDataVersion();
			if (m_lastDataVerion != dataVersion)
			{
				HandleDataChanged();
				m_lastDataVerion = dataVersion;
			}
		}
	}

	public override bool TryIncrementDataVersion(int id)
	{
		HashSet<int> dataModelIDs = null;
		dataModelIDs = m_valueScript.GetDataModelIDs();
		if (dataModelIDs == null || !dataModelIDs.Contains(id))
		{
			return false;
		}
		IncrementLocalDataVersion();
		return true;
	}

	public override bool GetIsChangingStates(Func<GameObject, bool> includeGameObject)
	{
		if (!includeGameObject(base.gameObject))
		{
			return false;
		}
		return IsChangingStates;
	}

	protected override void OnDestroy()
	{
		HandleCleanUp();
		m_asyncOperationId++;
		base.OnDestroy();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (m_animator != null && !string.IsNullOrEmpty(m_currentControllerId))
		{
			PauseAtNormalizedTime(m_displayFrameNormalizedTime);
		}
	}

	private void HandleDataChanged()
	{
		ScriptContext.EvaluationResults results = new ScriptContext().Evaluate(m_valueScript.Script, this);
		if (results.ErrorCode == ScriptContext.ErrorCodes.Success)
		{
			HandleItemChanged(results.Value);
		}
	}

	private void HandleItemChanged(object newController)
	{
		if (newController == null || newController is string)
		{
			CreateController((string)newController);
		}
		else
		{
			Error.AddDevFatal("AnimationOverrideWidgetBehaviour's script string produced a non-string, non-null value.");
		}
	}

	private void CreateController(string controllerId)
	{
		if (controllerId == m_currentControllerId)
		{
			return;
		}
		if (m_animator == null)
		{
			Debug.LogErrorFormat("Null animator for {0} in {1}!", base.name, base.Owner.name);
			HandleCleanUp();
			return;
		}
		HandleStartChangingStates();
		HandleCleanUp();
		m_isLoading = true;
		if (string.IsNullOrEmpty(controllerId))
		{
			m_isLoading = false;
			HandleDoneChangingStates();
			return;
		}
		m_startedAssetLoadCount++;
		m_asyncOperationId++;
		AssetHandleCallback<RuntimeAnimatorController> callback = delegate(AssetReference assetRef, AssetHandle<RuntimeAnimatorController> assetHandle, object asyncOperationId)
		{
			if (assetHandle == null)
			{
				HandleError(assetHandle);
			}
			else if (m_asyncOperationId != (int)asyncOperationId)
			{
				AssetHandle.SafeDispose(ref assetHandle);
			}
			else
			{
				HandleItemLoaded(assetHandle.Asset, controllerId);
			}
		};
		if (!AssetLoader.Get().LoadAsset(controllerId, callback, m_asyncOperationId, AssetLoadingOptions.DisableLocalization))
		{
			callback(controllerId, null, m_asyncOperationId);
		}
		m_startedAssetLoadCount++;
	}

	private void HandleItemLoaded(RuntimeAnimatorController controller, string controllerId)
	{
		if (!ValidateLoadSuccessful(controller))
		{
			HandleCleanUp();
			HandleDoneChangingStates();
			return;
		}
		m_animator.runtimeAnimatorController = null;
		m_animator.transform.localPosition = Vector3.zero;
		m_animator.transform.localRotation = Quaternion.identity;
		m_animator.transform.localScale = Vector3.one;
		m_currentControllerId = controllerId;
		m_animator.runtimeAnimatorController = controller;
		m_animator.fireEvents = false;
		ApplyEmoteImageOffset();
		ParseAnimationEvents(controller, out m_displayFrameNormalizedTime);
		PauseAtNormalizedTime(m_displayFrameNormalizedTime);
		m_failedAssetLoadCount = 0;
		m_startedAssetLoadCount = 0;
		m_isLoading = false;
		HandleDoneChangingStates();
	}

	private void ApplyEmoteImageOffset()
	{
		BattlegroundsEmoteDataModel emoteDataModel = base.Owner.GetDataModel<BattlegroundsEmoteDataModel>();
		if (emoteDataModel == null)
		{
			Debug.LogError("AnimationOverrideWidgetBehaviour: Null DataModel for " + base.name + " in " + base.Owner.name + ".");
			return;
		}
		Transform parentTransform = m_animator.transform.parent;
		if (parentTransform != null)
		{
			Vector3 offset = Vector3.zero;
			offset.x = emoteDataModel.XOffset;
			offset.z = emoteDataModel.ZOffset;
			offset.y = parentTransform.localPosition.y;
			parentTransform.localPosition = offset;
		}
	}

	private bool ValidateLoadSuccessful(RuntimeAnimatorController controller)
	{
		if (m_animator == null)
		{
			Debug.LogError("AnimationOverrideWidgetBehaviour: Null animator for " + base.name + " in " + base.Owner.name + "!");
			return false;
		}
		if (controller == null)
		{
			Debug.LogError("AnimationOverrideWidgetBehaviour: Failed to load RuntimeAnimatorController for " + base.name + " in " + base.Owner.name + ".");
			return false;
		}
		if (controller.animationClips == null || controller.animationClips.Length == 0)
		{
			Debug.LogError("AnimationOverrideWidgetBehaviour: Failed to find any animation clips on RuntimeAnimatorController for " + base.name + " in " + base.Owner.name + ".");
			return false;
		}
		return true;
	}

	private void ParseAnimationEvents(RuntimeAnimatorController controller, out float displayFrameNormalizedTime)
	{
		displayFrameNormalizedTime = 0f;
		AnimationClip[] animationClips = controller.animationClips;
		if (animationClips.Length > 1)
		{
			Debug.LogWarning(string.Format("{0}: Unexpected number of animation clips found. Expected 1, found {1}. Defaulting to first clip found", "AnimationOverrideWidgetBehaviour", animationClips.Length));
		}
		AnimationClip animationClip = animationClips[0];
		AnimationEvent[] events = animationClip.events;
		foreach (AnimationEvent animationEvent in events)
		{
			if (animationEvent.functionName == "DisplayFrame")
			{
				displayFrameNormalizedTime = ((!Mathf.Approximately(animationClip.length, 0f)) ? (animationEvent.time / animationClip.length) : 0f);
				continue;
			}
			Debug.LogError("AnimationOverrideWidgetBehaviour: Unrecognized FunctionName, " + animationEvent.functionName + ", in AnimationEvent for " + animationClip.name + ".");
		}
	}

	private void PauseAtNormalizedTime(float normalizedTime)
	{
		m_animator.Play(0, -1, normalizedTime);
		m_animator.Update(0f);
		m_animator.enabled = false;
	}

	private void HandleCleanUp()
	{
		m_isLoading = false;
		m_currentControllerId = null;
		m_failedAssetLoadCount = 0;
		m_startedAssetLoadCount = 0;
	}

	private void HandleError<T>(AssetHandle<T> assetHandle) where T : UnityEngine.Object
	{
		AssetHandle.SafeDispose(ref assetHandle);
		m_failedAssetLoadCount++;
		if (m_failedAssetLoadCount >= m_startedAssetLoadCount)
		{
			Debug.LogErrorFormat("Failed to load material icon for {0} in {1}!", base.name, base.Owner.name);
			HandleCleanUp();
			HandleDoneChangingStates();
		}
	}
}
