using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HSDontDestroyOnLoad))]
public class CustomFrameDef : MonoBehaviour
{
	[Header("Mesh replacement")]
	public Mesh FrameMesh;

	public Material FrameMat;

	public Material PortraitMat;

	public int FrameMatIdx;

	public int PortraitMatIdx;

	[Header("Diamond Assets")]
	public GameObject ExtraDiamondPrefab;

	public Material MissingDiamondPortraitMat;

	[Header("Texture replacement")]
	public Texture2D Silhouette;

	[Header("Highlight")]
	public HighlightRenderOverrides HighlightOverrides;

	public HighlightRenderOverrides CollectionOverrides;

	public HighlightRenderOverrides CardOverrides;

	public HighlightRenderOverrides CardPlayOverrides;

	[Header("Calibration")]
	public float DecorationRootOffset;

	public float HeroClassIconOffset;

	public float AvoidShadowPlaneOffset;

	public float HeroZonePositionOffset;

	public float HeroPickerRaiseAndLowerLimit = -1f;

	public float HeroPowerContainerOffset;

	public List<Vector3> SecretZoneOffsetsPc = new List<Vector3>();

	public float SecretZoneScalePc = 1f;

	public List<Vector3> SecretZoneOffsetsMobile = new List<Vector3>();

	public float SecretZoneScaleMobile = 1f;

	public Vector3 AttackOffset;

	public Vector3 HealthOffset;

	public Vector3 ArmorOffset;

	public Vector3 MulliganHeroOffsetFriendly;

	public Vector3 MulliganHeroOffsetEnemy;

	public Vector3 MulliganHeroNameOffsetFriendly;

	public Vector3 MulliganHeroNameOffsetEnemy;

	[Header("Animation")]
	public GameObject m_SkinnedFrameRoot;

	public Animator m_FrameAnimator;

	[Header("Custom Name Shadow")]
	public Texture m_CustomNameShadowTexture;

	public Vector3 m_CustomNameShadowOffset;

	[Header("Meta Calibration")]
	public bool UseMetaCalibration;

	[Header("Meta Calibration - Collection")]
	public Vector3 MetaCollectionHeroPortraitScale;

	public Vector3 MetaCollectionPositionOffset;

	[Header("Meta Calibration - Deck Picker")]
	public Vector3 MetaDeckPickerHeroPortraitScale;

	public Vector3 MetaDeckPickerPositionOffset;

	public Vector3 MetaDeckPickerXPBarPosition;

	public Vector3 MetaDeckPickerXPBarScale;

	[Header("Meta Calibration - Rewards")]
	public Vector3 MetaRewardsHeroPortraitScalePC;

	public Vector3 MetaRewardsHeroPortraitScalePhone;

	[Header("Meta Calibration - Rewards")]
	public Vector3 MetaEndGameXPBarScale;

	public Vector3 MetaEndGameXPBarVictoryPosition;
}
