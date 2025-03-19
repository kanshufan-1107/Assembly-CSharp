using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
[CreateAssetMenu(menuName = "ScriptableObjects/Bacon Cosmetic Preview Config")]
public class BaconCosmeticPreviewRunnerConfig : ScriptableObject
{
	public int boardId;

	public int strikeId;

	public string friendlyHeroCardId;

	public string opposingHeroCardId;

	public TAG_BOARD_VISUAL_STATE initialState;

	public List<BaconCosmeticPreviewAction> actions;
}
