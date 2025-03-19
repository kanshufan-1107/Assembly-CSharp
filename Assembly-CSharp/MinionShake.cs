using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Services;
using UnityEngine;

public class MinionShake : MonoBehaviour
{
	private readonly Vector3 OFFSCREEN_POSITION = new Vector3(-400f, -400f, -400f);

	private const float INTENSITY_SMALL = 0.1f;

	private const float INTENSITY_MEDIUM = 0.5f;

	private const float INTENSITY_LARGE = 1f;

	private const float DISABLE_HEIGHT = 0.1f;

	public GameObject m_MinionShakeAnimator;

	private bool m_Animating;

	private Animator m_Animator;

	private Vector2 m_ImpactDirection;

	private Vector3 m_ImpactPosition;

	private float m_Angle;

	private ShakeMinionIntensity m_ShakeIntensityType = ShakeMinionIntensity.MediumShake;

	private float m_IntensityValue = 0.5f;

	private ShakeMinionType m_ShakeType = ShakeMinionType.RandomDirection;

	private GameObject m_MinionShakeInstance;

	private Transform m_CardPlayAllyTransform;

	private Vector3 m_MinionOrgPos;

	private Quaternion m_MinionOrgRot;

	private float m_StartDelay;

	private float m_Radius;

	private bool m_IgnoreAnimationPlaying;

	private bool m_IgnoreHeight;

	private IGraphicsManager m_graphicsManager;

	private static int s_IdleState = Animator.StringToHash("Base.Idle");

	private void Awake()
	{
		m_graphicsManager = ServiceManager.Get<IGraphicsManager>();
	}

	private void LateUpdate()
	{
		if ((m_graphicsManager == null || m_graphicsManager.RenderQualityLevel != 0) && m_Animating && !(m_Animator == null) && !(m_MinionShakeInstance == null))
		{
			if (m_Animator.GetCurrentAnimatorStateInfo(0).fullPathHash == s_IdleState && !m_Animator.GetBool("shake"))
			{
				base.transform.localPosition = m_MinionOrgPos;
				base.transform.localRotation = m_MinionOrgRot;
				m_Animating = false;
			}
			else
			{
				base.transform.localPosition = m_CardPlayAllyTransform.localPosition + m_MinionOrgPos;
				base.transform.localRotation = m_MinionOrgRot;
				base.transform.Rotate(m_CardPlayAllyTransform.localRotation.eulerAngles);
			}
		}
	}

	private void OnDestroy()
	{
		if ((bool)m_MinionShakeInstance)
		{
			UnityEngine.Object.Destroy(m_MinionShakeInstance);
		}
	}

	public bool isShaking()
	{
		return m_Animating;
	}

	public static void ShakeAllMinions(GameObject shakeTrigger, ShakeMinionType shakeType, Vector3 impactPoint, ShakeMinionIntensity intensityType, float intensityValue, float radius, float startDelay)
	{
		MinionShake[] array = FindAllMinionShakers(shakeTrigger);
		foreach (MinionShake obj in array)
		{
			obj.m_StartDelay = startDelay;
			obj.m_ShakeType = shakeType;
			obj.m_ImpactPosition = impactPoint;
			obj.m_ShakeIntensityType = intensityType;
			obj.m_IntensityValue = intensityValue;
			obj.m_Radius = radius;
			obj.ShakeMinion();
			BoardEvents boardEvents = BoardEvents.Get();
			if (boardEvents != null)
			{
				boardEvents.MinionShakeEvent(intensityType, intensityValue);
			}
		}
	}

	public static void ShakeTargetMinion(GameObject shakeTarget, ShakeMinionType shakeType, Vector3 impactPoint, ShakeMinionIntensity intensityType, float intensityValue, float radius, float startDelay)
	{
		Spell triggerSpell = GameObjectUtils.FindComponentInThisOrParents<Spell>(shakeTarget);
		if (triggerSpell == null)
		{
			Debug.LogWarning("MinionShake: failed to locate Spell component");
			return;
		}
		GameObject go = triggerSpell.GetVisualTarget();
		if (go == null)
		{
			Debug.LogWarning("MinionShake: failed to Spell GetVisualTarget");
			return;
		}
		MinionShake shaker = go.GetComponentInChildren<MinionShake>();
		if (shaker == null)
		{
			Debug.LogWarning("MinionShake: failed to locate MinionShake component");
			return;
		}
		shaker.m_StartDelay = startDelay;
		shaker.m_ShakeType = shakeType;
		shaker.m_ImpactPosition = impactPoint;
		shaker.m_ShakeIntensityType = intensityType;
		shaker.m_IntensityValue = intensityValue;
		shaker.m_Radius = radius;
		shaker.ShakeMinion();
	}

	public static void ShakeObject(GameObject shakeObject, ShakeMinionType shakeType, Vector3 impactPoint, ShakeMinionIntensity intensityType, float intensityValue, float radius, float startDelay)
	{
		ShakeObject(shakeObject, shakeType, impactPoint, intensityType, intensityValue, radius, startDelay, ignoreAnimationPlaying: false);
	}

	public static void ShakeObject(GameObject shakeObject, ShakeMinionType shakeType, Vector3 impactPoint, ShakeMinionIntensity intensityType, float intensityValue, float radius, float startDelay, bool ignoreAnimationPlaying)
	{
		ShakeObject(shakeObject, shakeType, impactPoint, intensityType, intensityValue, radius, startDelay, ignoreAnimationPlaying: false, ignoreAnimationPlaying);
	}

	public static void ShakeObject(GameObject shakeObject, ShakeMinionType shakeType, Vector3 impactPoint, ShakeMinionIntensity intensityType, float intensityValue, float radius, float startDelay, bool ignoreAnimationPlaying, bool ignoreHeight)
	{
		MinionShake shaker = shakeObject.GetComponentInChildren<MinionShake>();
		if (shaker == null)
		{
			Actor actor = GameObjectUtils.FindComponentInParents<Actor>(shakeObject);
			if (actor == null)
			{
				return;
			}
			shaker = actor.gameObject.GetComponentInChildren<MinionShake>();
			if (shaker == null)
			{
				return;
			}
		}
		shaker.m_StartDelay = startDelay;
		shaker.m_ShakeType = shakeType;
		shaker.m_ImpactPosition = impactPoint;
		shaker.m_ShakeIntensityType = intensityType;
		shaker.m_IntensityValue = intensityValue;
		shaker.m_Radius = radius;
		shaker.m_IgnoreAnimationPlaying = ignoreAnimationPlaying;
		shaker.ShakeMinion();
	}

	public void ShakeAllMinionsRandomMedium()
	{
		Vector3 center = Vector3.zero;
		Board board = Board.Get();
		if (board != null)
		{
			Transform centerBone = board.FindBone("CenterPointBone");
			if (centerBone != null)
			{
				center = centerBone.position;
			}
		}
		ShakeAllMinions(base.gameObject, ShakeMinionType.RandomDirection, center, ShakeMinionIntensity.MediumShake, 0.5f, 0f, 0f);
	}

	public void ShakeAllMinionsRandomLarge()
	{
		Vector3 center = Vector3.zero;
		Board board = Board.Get();
		if (board != null)
		{
			Transform centerBone = board.FindBone("CenterPointBone");
			if (centerBone != null)
			{
				center = centerBone.position;
			}
		}
		ShakeAllMinions(base.gameObject, ShakeMinionType.RandomDirection, center, ShakeMinionIntensity.LargeShake, 1f, 0f, 0f);
	}

	public void RandomShake(float impact)
	{
		m_ShakeIntensityType = ShakeMinionIntensity.Custom;
		m_IntensityValue = impact;
		m_ShakeType = ShakeMinionType.Angle;
		m_ShakeType = ShakeMinionType.RandomDirection;
		ShakeMinion();
	}

	private void ShakeMinion()
	{
		if (m_graphicsManager == null || m_graphicsManager.RenderQualityLevel == GraphicsQuality.Low)
		{
			return;
		}
		if (m_MinionShakeAnimator == null)
		{
			Debug.LogWarning("MinionShake: failed to locate MinionShake Animator");
			return;
		}
		Animation anim = GetComponent<Animation>();
		if (anim != null && anim.isPlaying && !m_IgnoreAnimationPlaying)
		{
			return;
		}
		Vector3 center = Vector3.zero;
		Board board = Board.Get();
		if (board != null)
		{
			Transform centerBone = board.FindBone("CenterPointBone");
			if (centerBone != null)
			{
				center = centerBone.position;
			}
		}
		if (center.y - base.transform.position.y > 0.1f && !m_IgnoreHeight)
		{
			return;
		}
		if (m_MinionShakeInstance == null)
		{
			m_MinionShakeInstance = UnityEngine.Object.Instantiate(m_MinionShakeAnimator, OFFSCREEN_POSITION, base.transform.rotation);
			m_CardPlayAllyTransform = m_MinionShakeInstance.transform.Find("Card_Play_Ally").gameObject.transform;
		}
		if (m_Animator == null)
		{
			m_Animator = m_MinionShakeInstance.GetComponent<Animator>();
		}
		if (!m_Animating)
		{
			m_MinionOrgPos = base.transform.localPosition;
			m_MinionOrgRot = base.transform.localRotation;
		}
		if (m_ShakeType == ShakeMinionType.Angle)
		{
			m_ImpactDirection = AngleToVector(m_Angle);
		}
		else if (m_ShakeType == ShakeMinionType.ImpactDirection)
		{
			m_ImpactDirection = Vector3.Normalize(base.transform.position - m_ImpactPosition);
		}
		else if (m_ShakeType == ShakeMinionType.RandomDirection)
		{
			m_ImpactDirection = AngleToVector(UnityEngine.Random.Range(0f, 360f));
		}
		float intensity = m_IntensityValue;
		if (m_ShakeIntensityType == ShakeMinionIntensity.SmallShake)
		{
			intensity = 0.1f;
		}
		else if (m_ShakeIntensityType == ShakeMinionIntensity.MediumShake)
		{
			intensity = 0.5f;
		}
		else if (m_ShakeIntensityType == ShakeMinionIntensity.LargeShake)
		{
			intensity = 1f;
		}
		m_ImpactDirection *= intensity;
		m_Animator.SetFloat("posx", m_ImpactDirection.x);
		m_Animator.SetFloat("posy", m_ImpactDirection.y);
		if (!(m_Radius > 0f) || !(Vector3.Distance(base.transform.position, m_ImpactPosition) > m_Radius))
		{
			if (m_StartDelay > 0f)
			{
				StartCoroutine(StartShakeAnimation());
			}
			else
			{
				m_Animating = true;
				m_Animator.SetBool("shake", value: true);
			}
			StartCoroutine(ResetShakeAnimator());
		}
	}

	private Vector2 AngleToVector(float angle)
	{
		Vector3 v = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0f, -1f);
		return new Vector2(v.x, v.z);
	}

	private IEnumerator StartShakeAnimation()
	{
		yield return new WaitForSeconds(m_StartDelay);
		m_Animating = true;
		m_Animator.SetBool("shake", value: true);
	}

	private IEnumerator ResetShakeAnimator()
	{
		yield return new WaitForSeconds(m_StartDelay);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		m_Animator.SetBool("shake", value: false);
	}

	private static MinionShake[] FindAllMinionShakers(GameObject shakeTrigger)
	{
		Card triggerCard = null;
		Spell triggerSpell = GameObjectUtils.FindComponentInThisOrParents<Spell>(shakeTrigger);
		if (triggerSpell != null)
		{
			triggerCard = triggerSpell.GetSourceCard();
		}
		List<MinionShake> minionShakers = new List<MinionShake>();
		ZoneMgr zoneManager = ZoneMgr.Get();
		if (zoneManager == null)
		{
			return minionShakers.ToArray();
		}
		foreach (Zone playZone in zoneManager.FindZonesForTag(TAG_ZONE.PLAY))
		{
			if (playZone.GetType() == typeof(ZoneHero))
			{
				continue;
			}
			foreach (Card card in playZone.GetCards())
			{
				if (card == triggerCard || !card.IsActorReady())
				{
					continue;
				}
				try
				{
					MinionShake minionShake = card.GetComponentInChildren<MinionShake>();
					if (minionShake != null)
					{
						minionShakers.Add(minionShake);
					}
				}
				catch (Exception ex)
				{
					Debug.LogWarning($"MinionShake: failed to get component - card = {card}, ignored: {ex.Message}");
				}
			}
		}
		return minionShakers.ToArray();
	}
}
