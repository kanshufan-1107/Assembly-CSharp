using System;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using UnityEngine;

public class MercenariesBoard : Board
{
	[Serializable]
	public class LightingSettings
	{
		public Color m_lightColor;

		public Color m_ambientColor;

		public float m_maxIntensity;
	}

	[Serializable]
	public class DecorationCountWeighting
	{
		public int m_numberOfDecorations;

		public int m_weight;
	}

	[Serializable]
	public class DecorationLayerWeighting
	{
		public MercenariesBoardDecorationLayer m_decorationLayer;

		public int m_weight;
	}

	public List<GameObject> m_baseVisuals;

	public List<GameObject> m_pathVisuals;

	public List<DecorationLayerWeighting> m_substrateVisuals;

	public List<DecorationLayerWeighting> m_clickableVisuals;

	public List<DecorationLayerWeighting> m_capVisuals;

	public List<LightingSettings> m_lighting;

	public List<GameObject> m_weatherEffects;

	public GameObject m_bossDecorations;

	public List<DecorationCountWeighting> m_cornerCountWeightList;

	public UIBButton m_debugRandomizeButton;

	public MusicPlaylistType m_bossMusic = MusicPlaylistType.InGame_MERCBOSS1;

	private bool m_isFinalBoss;

	private bool m_allowLightingChanges;

	public override void Start()
	{
		base.Start();
		if (m_debugRandomizeButton != null)
		{
			m_debugRandomizeButton.AddEventListener(UIEventType.RELEASE, OnRandomizeButtonPressed);
		}
	}

	protected override void ValidateInspectorReferences()
	{
	}

	public void RandomizeVisuals(bool isFinalBoss, bool allowLightingChanges, int seed = 0)
	{
		Log.Lettuce.PrintDebug("MercenariesBoard.RandomizeVisuals: seed={0}, isFinalBoss={1}", seed, isFinalBoss);
		m_isFinalBoss = isFinalBoss;
		m_allowLightingChanges = allowLightingChanges;
		if (seed > 0)
		{
			UnityEngine.Random.InitState(seed);
		}
		ActivateRandomElementFromList(m_baseVisuals);
		ActivateRandomElementFromList(m_pathVisuals, allowNone: true);
		ActivateRandomCornerLayerFromList(m_substrateVisuals);
		ActivateRandomCornerLayerFromList(m_clickableVisuals);
		ActivateRandomCapLayerFromList(m_capVisuals);
		if (m_allowLightingChanges)
		{
			ActivateRandomLightingFromList(m_lighting);
		}
		ActivateRandomElementFromList(m_weatherEffects, allowNone: true);
		SetupFinalBossState(isFinalBoss);
	}

	private void ActivateRandomElementFromList(List<GameObject> list, bool allowNone = false)
	{
		if (list == null || list.Count == 0)
		{
			return;
		}
		foreach (GameObject item in list)
		{
			item.SetActive(value: false);
		}
		int elementsToRoll = list.Count;
		if (allowNone)
		{
			elementsToRoll++;
		}
		int rolledIndex = UnityEngine.Random.Range(0, elementsToRoll);
		if (rolledIndex < list.Count)
		{
			list[rolledIndex].SetActive(value: true);
		}
	}

	private void ActivateRandomCornerLayerFromList(List<DecorationLayerWeighting> list)
	{
		if (list == null || list.Count == 0)
		{
			return;
		}
		SetupWeightedDecorations(list);
		List<MercenariesBoardDecorationLayer.DecorationPosition> cornerTypes = new List<MercenariesBoardDecorationLayer.DecorationPosition>
		{
			MercenariesBoardDecorationLayer.DecorationPosition.TOP_LEFT,
			MercenariesBoardDecorationLayer.DecorationPosition.TOP_RIGHT,
			MercenariesBoardDecorationLayer.DecorationPosition.BOTTOM_LEFT,
			MercenariesBoardDecorationLayer.DecorationPosition.BOTTOM_RIGHT
		};
		GeneralUtils.Shuffle(cornerTypes);
		int numberOfCorners = PickNumberOfCornersToShow();
		for (int i = 0; i < numberOfCorners; i++)
		{
			GeneralUtils.RollElementFromWeightedList(list, (DecorationLayerWeighting e) => e.m_weight)?.m_decorationLayer.SetDecorationVisible(cornerTypes[i]);
		}
	}

	private void ActivateRandomCapLayerFromList(List<DecorationLayerWeighting> list)
	{
		if (list == null || list.Count == 0)
		{
			return;
		}
		SetupWeightedDecorations(list);
		List<MercenariesBoardDecorationLayer.DecorationPosition> capTypes = new List<MercenariesBoardDecorationLayer.DecorationPosition>
		{
			MercenariesBoardDecorationLayer.DecorationPosition.TOP_CENTER,
			MercenariesBoardDecorationLayer.DecorationPosition.BOTTOM_CENTER
		};
		for (int i = 0; i < capTypes.Count; i++)
		{
			GeneralUtils.RollElementFromWeightedList(list, (DecorationLayerWeighting e) => e.m_weight)?.m_decorationLayer.SetDecorationVisible(capTypes[i]);
		}
	}

	private void SetupWeightedDecorations(List<DecorationLayerWeighting> list)
	{
		foreach (DecorationLayerWeighting item in list)
		{
			item.m_decorationLayer.gameObject.SetActive(value: true);
			item.m_decorationLayer.HideAllDecorations();
		}
	}

	private void ActivateRandomLightingFromList(List<LightingSettings> list)
	{
		if (list != null && list.Count != 0)
		{
			int rolledIndex = UnityEngine.Random.Range(0, list.Count);
			LightingSettings selectedLightingSettings = list[rolledIndex];
			m_AmbientColor = selectedLightingSettings.m_ambientColor;
			m_DirectionalLight.color = selectedLightingSettings.m_lightColor;
			m_DirectionalLight.intensity = selectedLightingSettings.m_maxIntensity;
			m_DirectionalLightIntensity = selectedLightingSettings.m_maxIntensity;
			ResetAmbientColor();
		}
	}

	private int PickNumberOfCornersToShow()
	{
		return GeneralUtils.RollElementFromWeightedList(m_cornerCountWeightList, (DecorationCountWeighting e) => e.m_weight)?.m_numberOfDecorations ?? 0;
	}

	private void SetupFinalBossState(bool isFinalBoss)
	{
		if (m_bossDecorations != null)
		{
			m_bossDecorations.SetActive(isFinalBoss);
		}
		if (!isFinalBoss)
		{
			return;
		}
		foreach (DecorationLayerWeighting clickableVisual in m_clickableVisuals)
		{
			clickableVisual.m_decorationLayer.HideTopDecorations();
		}
		foreach (DecorationLayerWeighting capVisual in m_capVisuals)
		{
			capVisual.m_decorationLayer.HideTopDecorations();
		}
		foreach (GameObject pathVisual in m_pathVisuals)
		{
			pathVisual.SetActive(value: false);
		}
		m_BoardMusic = m_bossMusic;
	}

	private void OnRandomizeButtonPressed(UIEvent e)
	{
		RandomizeVisuals(m_isFinalBoss, m_allowLightingChanges);
	}
}
