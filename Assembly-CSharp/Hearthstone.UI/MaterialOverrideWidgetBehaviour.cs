using System;
using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.UI.Scripting;
using Unity.Profiling;
using UnityEngine;

namespace Hearthstone.UI;

[ExecuteAlways]
[AddComponentMenu("")]
public class MaterialOverrideWidgetBehaviour : CustomWidgetBehavior
{
	[Tooltip("The index of the material that will be overridden")]
	[SerializeField]
	private int m_materialIndex;

	[Tooltip("The renderer that owns the material that will be overridden")]
	[SerializeField]
	private MeshRenderer m_renderer;

	[Tooltip("Optional material. If specified, while the final material is loading this material will be displayed instead.")]
	[SerializeField]
	private Material m_loadingMaterial;

	[Tooltip("A data model script that needs to evaluate to the name and GUID of a material.")]
	[SerializeField]
	private ScriptString m_valueScript;

	[Tooltip("Optional link to a set render que component. If present, render que will often apply its settings too early, before materials are loaded. Set this field to make the render que apply its settings after load.")]
	[SerializeField]
	private SetRenderQue m_optionalSetRenderQue;

	private bool m_isLoading;

	private int m_lastDataVerion;

	private int m_materialAsyncOperationId;

	private string m_CurrentMaterial;

	private int m_startedAssetLoadCount;

	private int m_failedAssetLoadCount;

	private static ProfilerMarker s_onUpdateProfilerMarker = new ProfilerMarker("MaterialOverrideWidgetBehaviour.OnUpdate");

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
		m_materialAsyncOperationId++;
		base.OnDestroy();
	}

	private void HandleDataChanged()
	{
		ScriptContext.EvaluationResults results = new ScriptContext().Evaluate(m_valueScript.Script, this);
		if (results.ErrorCode == ScriptContext.ErrorCodes.Success)
		{
			HandleItemChanged(results.Value);
		}
	}

	private void HandleItemChanged(object newMaterial)
	{
		if (newMaterial == null || newMaterial is string)
		{
			CreateMaterial((string)newMaterial);
		}
		else
		{
			Error.AddDevFatal("MaterialOverrideWidgetBehaviour's script string produced a non-string, non-null value.");
		}
	}

	private void CreateMaterial(string materialId)
	{
		if (materialId == m_CurrentMaterial)
		{
			return;
		}
		if (m_renderer == null || m_materialIndex < 0 || m_materialIndex >= m_renderer.GetSharedMaterials().Count)
		{
			Debug.LogErrorFormat("Renderer and material index setup incorrect for {0} in {1}!", base.name, base.Owner.name);
			HandleCleanUp();
			return;
		}
		if (m_loadingMaterial != null)
		{
			m_renderer.SetMaterial(m_materialIndex, new Material(m_loadingMaterial));
			m_optionalSetRenderQue?.Run();
		}
		HandleStartChangingStates();
		HandleCleanUp();
		m_isLoading = true;
		if (string.IsNullOrEmpty(materialId))
		{
			m_isLoading = false;
			HandleDoneChangingStates();
			return;
		}
		m_startedAssetLoadCount++;
		m_materialAsyncOperationId++;
		AssetHandleCallback<Material> callback = delegate(AssetReference assetRef, AssetHandle<Material> assetHandle, object asyncOperationId)
		{
			if (assetHandle == null)
			{
				HandleError(assetHandle);
			}
			else if (m_materialAsyncOperationId != (int)asyncOperationId)
			{
				AssetHandle.SafeDispose(ref assetHandle);
			}
			else
			{
				HandleItemLoaded(assetHandle.Asset, materialId);
			}
		};
		if (!AssetLoader.Get().LoadAsset(materialId, callback, m_materialAsyncOperationId, AssetLoadingOptions.DisableLocalization))
		{
			callback(materialId, null, m_materialAsyncOperationId);
		}
		m_startedAssetLoadCount++;
	}

	private void HandleItemLoaded(Material material, string materialId)
	{
		if (m_renderer == null || m_materialIndex < 0 || m_materialIndex >= m_renderer.GetSharedMaterials().Count)
		{
			Debug.LogErrorFormat("Renderer and material index setup incorrect for {0} in {1}!", base.name, base.Owner.name);
			HandleCleanUp();
			HandleDoneChangingStates();
			return;
		}
		m_CurrentMaterial = materialId;
		m_renderer.SetMaterial(m_materialIndex, material);
		if (m_optionalSetRenderQue != null)
		{
			m_optionalSetRenderQue.Run();
		}
		m_failedAssetLoadCount = 0;
		m_startedAssetLoadCount = 0;
		m_isLoading = false;
		HandleDoneChangingStates();
	}

	private void HandleCleanUp()
	{
		m_isLoading = false;
		m_CurrentMaterial = null;
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
