using System;
using Blizzard.T5.Game.Spells;
using Blizzard.T5.Game.Spells.SuperSpells;
using UnityEngine;

[Serializable]
public class SpellMissileInfo : ISpellMissileInfo
{
	[SerializeField]
	private bool m_Enabled = true;

	[SerializeField]
	private Spell m_Prefab;

	[SerializeField]
	private Spell m_ReversePrefab;

	[SerializeField]
	private float m_reverseDelay;

	[SerializeField]
	private bool m_UseSuperSpellLocation = true;

	[SerializeField]
	private float m_SpawnDelaySecMin;

	[SerializeField]
	private float m_SpawnDelaySecMax;

	[SerializeField]
	private bool m_SpawnInSequence;

	[SerializeField]
	private float m_SpawnOffset;

	[SerializeField]
	private float m_PathDurationMin = 0.5f;

	[SerializeField]
	private float m_PathDurationMax = 1f;

	[SerializeField]
	private iTween.EaseType m_PathEaseType = iTween.EaseType.linear;

	[SerializeField]
	private bool m_OrientToPath;

	[SerializeField]
	private float m_CenterOffsetPercent = 50f;

	[SerializeField]
	private float m_CenterPointHeightMin;

	[SerializeField]
	private float m_CenterPointHeightMax;

	[SerializeField]
	private float m_RightMin;

	[SerializeField]
	private float m_RightMax;

	[SerializeField]
	private float m_LeftMin;

	[SerializeField]
	private float m_LeftMax;

	[SerializeField]
	private bool m_DebugForceMax;

	[SerializeField]
	private float m_DistanceScaleFactor = 8f;

	[SerializeField]
	private string m_TargetJoint = "TargetJoint";

	[SerializeField]
	private float m_TargetHeightOffset = 0.5f;

	[SerializeField]
	private Vector3 m_JointUpVector = Vector3.up;

	[SerializeField]
	private bool m_UseTargetCardPositionInsteadOfHandSlot;

	[SerializeField]
	private int m_TimesToHitSameTarget;

	public bool Enabled => m_Enabled;

	public ISpell Prefab => m_Prefab;

	public ISpell ReversePrefab => m_ReversePrefab;

	public float ReverseDelay => m_reverseDelay;

	public bool UseSuperSpellLocation => m_UseSuperSpellLocation;

	public float SpawnDelaySecMin => m_SpawnDelaySecMin;

	public float SpawnDelaySecMax => m_SpawnDelaySecMax;

	public bool SpawnInSequence => m_SpawnInSequence;

	public float SpawnOffset => m_SpawnOffset;

	public float PathDurationMin => m_PathDurationMin;

	public float PathDurationMax => m_PathDurationMax;

	public iTween.EaseType PathEaseType => m_PathEaseType;

	public bool OrientToPath => m_OrientToPath;

	public float CenterOffsetPercent => m_CenterOffsetPercent;

	public float CenterPointHeightMin => m_CenterPointHeightMin;

	public float CenterPointHeightMax => m_CenterPointHeightMax;

	public float RightMin => m_RightMin;

	public float RightMax => m_RightMax;

	public float LeftMin => m_LeftMin;

	public float LeftMax => m_LeftMax;

	public bool DebugForceMax => m_DebugForceMax;

	public float DistanceScaleFactor => m_DistanceScaleFactor;

	public string TargetJoint => m_TargetJoint;

	public float TargetHeightOffset => m_TargetHeightOffset;

	public Vector3 JointUpVector => m_JointUpVector;

	public bool UseTargetCardPositionInsteadOfHandSlot => m_UseTargetCardPositionInsteadOfHandSlot;

	public int TimesToHitSameTarget => m_TimesToHitSameTarget;
}
