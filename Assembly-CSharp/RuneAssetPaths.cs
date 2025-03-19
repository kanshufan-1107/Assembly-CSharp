using System;
using PegasusShared;
using UnityEngine;

[Serializable]
public class RuneAssetPaths
{
	public RuneType m_assetRuneType;

	[CustomEditField(T = EditType.MATERIAL)]
	public Material m_runeMaterial;

	[CustomEditField(T = EditType.MATERIAL)]
	public Material m_runeMaterialHighlighted;

	[CustomEditField(T = EditType.MATERIAL)]
	public Material m_runeMaterialGhosted;

	[CustomEditField(T = EditType.MATERIAL)]
	public Material m_runeMaterialRed;

	[CustomEditField(T = EditType.MATERIAL)]
	public Material m_runeMaterialBlue;
}
