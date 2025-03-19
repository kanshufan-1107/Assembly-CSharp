using System.Collections;
using UnityEngine;

[CustomEditClass]
public class TGTTargetDummy : MonoBehaviour
{
	private const int SPIN_PERCENT = 5;

	public GameObject m_Body;

	public GameObject m_Shield;

	public GameObject m_Sword;

	public Animation m_Animation;

	public GameObject m_BodyMesh;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_HitBodySoundPrefab;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_HitShieldSoundPrefab;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_HitSwordSoundPrefab;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_HitSpinSoundPrefab;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_SqueakSoundPrefab;

	private static TGTTargetDummy s_instance;

	private float m_squeakSoundVelocity;

	private float m_lastSqueakSoundVol;

	private Quaternion m_lastFrameSqueakAngle;

	private AudioSource m_squeakSound;

	private void Awake()
	{
		s_instance = this;
	}

	private void Start()
	{
		StartCoroutine(RegisterBoardEventLargeShake());
		GameObject squeakSoundGO = SoundLoader.LoadSound(m_SqueakSoundPrefab);
		if (squeakSoundGO != null)
		{
			squeakSoundGO.transform.position = m_Body.transform.position;
			m_squeakSound = squeakSoundGO.GetComponent<AudioSource>();
		}
	}

	private void Update()
	{
		HandleHits();
	}

	public static TGTTargetDummy Get()
	{
		return s_instance;
	}

	public void ArrowHit()
	{
		m_Animation.CrossFade("TGT_GrandStand_Dummy_ArrowHit", 0.1f);
	}

	private void BodyHit()
	{
		PlaySqueakSound();
		if (!string.IsNullOrEmpty(m_HitBodySoundPrefab))
		{
			SoundManager.Get().LoadAndPlay(m_HitBodySoundPrefab, m_Body);
		}
		m_Animation.CrossFade("TGT_GrandStand_Dummy_Hit", 0.1f);
	}

	private void ShieldHit()
	{
		PlaySqueakSound();
		if (Random.Range(0, 100) < 5)
		{
			Spin(reverse: false);
			return;
		}
		if (!string.IsNullOrEmpty(m_HitShieldSoundPrefab))
		{
			SoundManager.Get().LoadAndPlay(m_HitShieldSoundPrefab, m_Body);
		}
		m_Animation.CrossFade("TGT_GrandStand_Dummy_ShieldHit", 0.1f);
	}

	private void SwordHit()
	{
		PlaySqueakSound();
		if (Random.Range(0, 100) < 5)
		{
			Spin(reverse: true);
			return;
		}
		if (!string.IsNullOrEmpty(m_HitSwordSoundPrefab))
		{
			SoundManager.Get().LoadAndPlay(m_HitSwordSoundPrefab, m_Body);
		}
		m_Animation.CrossFade("TGT_GrandStand_Dummy_SwordHit", 0.1f);
	}

	private IEnumerator RegisterBoardEventLargeShake()
	{
		while (BoardEvents.Get() == null)
		{
			yield return null;
		}
		yield return new WaitForSeconds(2f);
		BoardEvents.Get().RegisterLargeShakeEvent(BodyHit);
	}

	private void HandleHits()
	{
		if (InputCollection.GetMouseButtonDown(0))
		{
			if (IsOver(m_Body))
			{
				BodyHit();
			}
			if (IsOver(m_Shield))
			{
				ShieldHit();
			}
			if (IsOver(m_Sword))
			{
				SwordHit();
			}
		}
	}

	private void Spin(bool reverse)
	{
		float spin = 1080f;
		if (reverse)
		{
			spin = -1080f;
		}
		if (!string.IsNullOrEmpty(m_HitSpinSoundPrefab))
		{
			SoundManager.Get().LoadAndPlay(m_HitSpinSoundPrefab, m_Body);
		}
		m_BodyMesh.transform.localEulerAngles = Vector3.zero;
		Vector3 rotTo = new Vector3(0f, m_BodyMesh.transform.localEulerAngles.y + spin, 0f);
		Hashtable hash = iTweenManager.Get().GetTweenHashTable();
		hash.Add("rotation", rotTo);
		hash.Add("islocal", true);
		hash.Add("time", 3f);
		hash.Add("easetype", iTween.EaseType.easeOutElastic);
		iTween.RotateTo(m_BodyMesh, hash);
	}

	private void PlaySqueakSound()
	{
		StopCoroutine(SqueakSound());
		m_lastSqueakSoundVol = 0f;
		StartCoroutine(SqueakSound());
	}

	private IEnumerator SqueakSound()
	{
		if (!(m_squeakSound == null))
		{
			if (m_squeakSound != null && m_squeakSound.isPlaying)
			{
				SoundManager.Get().Stop(m_squeakSound);
			}
			SoundManager.Get().PlayPreloaded(m_squeakSound, 0f);
			while (m_squeakSound != null && m_squeakSound.isPlaying)
			{
				float difAngle = Mathf.Clamp01(Quaternion.Angle(m_Body.transform.rotation, m_lastFrameSqueakAngle) * 0.1f);
				m_lastFrameSqueakAngle = m_Body.transform.rotation;
				difAngle = (m_lastSqueakSoundVol = Mathf.SmoothDamp(difAngle, m_lastSqueakSoundVol, ref m_squeakSoundVelocity, 0.5f));
				SoundManager.Get().SetVolume(m_squeakSound, Mathf.Clamp01(difAngle));
				yield return null;
			}
		}
	}

	private bool IsOver(GameObject go)
	{
		if (!go)
		{
			return false;
		}
		if (!InputUtil.IsPlayMakerMouseInputAllowed(go))
		{
			return false;
		}
		if (!UniversalInputManager.Get().InputIsOver(go))
		{
			return false;
		}
		return true;
	}
}
