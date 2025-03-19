using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using PegasusShared;
using UnityEngine;

[CustomEditClass]
public class Rune : MonoBehaviour
{
	public MeshRenderer m_runeRenderer;

	public MeshRenderer m_highlightRenderer;

	public Material m_runeMaterialEmpty;

	public List<RuneAssetPaths> m_runeAssetTable = new List<RuneAssetPaths>();

	private RuneType m_runeType;

	private RuneState m_state = RuneState.Uninit;

	public RuneType GetRuneType()
	{
		return m_runeType;
	}

	public void ShowRune(RuneType type, RuneState state)
	{
		ShowRune();
		SetMaterial(type, state);
	}

	public void ShowRune()
	{
		if (!(m_runeRenderer == null))
		{
			m_runeRenderer.enabled = true;
		}
	}

	public void SetHighlighted(bool highlighted)
	{
		if (m_highlightRenderer == null)
		{
			return;
		}
		m_highlightRenderer.enabled = highlighted;
		if (m_runeType != 0)
		{
			Material highlightMaterial = GetHighlightMaterial();
			if ((bool)highlightMaterial)
			{
				m_highlightRenderer.SetMaterial(highlightMaterial);
			}
		}
	}

	private void SetMaterial(RuneType type, RuneState state)
	{
		m_runeType = type;
		SetState(state);
	}

	public void SetState(RuneState state)
	{
		if (state != RuneState.Uninit)
		{
			m_state = state;
		}
		else if (m_state == RuneState.Uninit)
		{
			m_state = RuneState.Default;
		}
		Material runeMaterial = GetRuneMaterial();
		if (runeMaterial != null)
		{
			m_runeRenderer.SetMaterial(runeMaterial);
		}
	}

	public void HideRune()
	{
		if (!(m_runeRenderer == null))
		{
			m_runeRenderer.enabled = false;
		}
	}

	private Material GetHighlightMaterial()
	{
		if (m_runeType == RuneType.RT_NONE)
		{
			return m_runeMaterialEmpty;
		}
		foreach (RuneAssetPaths assetPath in m_runeAssetTable)
		{
			if (assetPath.m_assetRuneType == m_runeType)
			{
				return assetPath.m_runeMaterialHighlighted;
			}
		}
		return null;
	}

	private Material GetRuneMaterial()
	{
		if (m_runeType == RuneType.RT_NONE)
		{
			return m_runeMaterialEmpty;
		}
		foreach (RuneAssetPaths assetPath in m_runeAssetTable)
		{
			if (assetPath.m_assetRuneType == m_runeType)
			{
				return m_state switch
				{
					RuneState.Default => assetPath.m_runeMaterial, 
					RuneState.Highlighted => assetPath.m_runeMaterialHighlighted, 
					RuneState.Disabled => assetPath.m_runeMaterialGhosted, 
					RuneState.Red => assetPath.m_runeMaterialRed, 
					RuneState.Blue => assetPath.m_runeMaterialBlue, 
					_ => assetPath.m_runeMaterial, 
				};
			}
		}
		return null;
	}
}
