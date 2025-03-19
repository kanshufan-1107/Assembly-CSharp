using System;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class CollectionCardLock : MonoBehaviour
{
	[Serializable]
	public class SignatureLock
	{
		public int frameId;

		public GameObject m_lockPlate;

		public GameObject m_background;

		public void SetActive(bool active)
		{
			m_lockPlate.SetActive(active);
			m_background.SetActive(active);
		}

		public bool IsValid()
		{
			if (m_lockPlate != null)
			{
				return m_background != null;
			}
			return false;
		}
	}

	[SerializeField]
	private GameObject m_allyBg;

	[SerializeField]
	private GameObject m_spellBg;

	[SerializeField]
	private GameObject m_weaponBg;

	[SerializeField]
	private GameObject m_locationBg;

	[SerializeField]
	private GameObject m_lockPlate;

	[SerializeField]
	private List<SignatureLock> m_signatureLocks;

	[SerializeField]
	private GameObject m_bannedRibbon;

	[SerializeField]
	private UberText m_lockText;

	[SerializeField]
	private GameObject m_lockPlateBone;

	[SerializeField]
	private GameObject m_weaponLockPlateBone;

	[SerializeField]
	private GameObject m_heroLockPlateBone;

	private EntityDef m_entityDef;

	private string m_lockReason;

	public void UpdateLockVisual(Actor actor, CollectionCardVisual.LockType lockType, string reason)
	{
		m_entityDef = actor.GetEntityDef();
		if (m_entityDef == null || lockType == CollectionCardVisual.LockType.NONE)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		m_lockReason = reason;
		base.gameObject.SetActive(value: true);
		m_bannedRibbon.SetActive(value: false);
		m_allyBg.SetActive(value: false);
		m_spellBg.SetActive(value: false);
		m_weaponBg.SetActive(value: false);
		m_locationBg.SetActive(value: false);
		foreach (SignatureLock signatureLock in m_signatureLocks)
		{
			signatureLock.m_background.SetActive(value: false);
		}
		GameObject currentBg;
		switch (m_entityDef.GetCardType())
		{
		case TAG_CARDTYPE.SPELL:
			currentBg = m_spellBg;
			m_lockPlate.transform.localPosition = m_lockPlateBone.transform.localPosition;
			break;
		case TAG_CARDTYPE.MINION:
			currentBg = m_allyBg;
			m_lockPlate.transform.localPosition = m_lockPlateBone.transform.localPosition;
			break;
		case TAG_CARDTYPE.WEAPON:
			currentBg = m_weaponBg;
			m_lockPlate.transform.localPosition = m_weaponLockPlateBone.transform.localPosition;
			break;
		case TAG_CARDTYPE.HERO:
			currentBg = m_allyBg;
			m_lockPlate.transform.localPosition = m_heroLockPlateBone.transform.localPosition;
			break;
		case TAG_CARDTYPE.LOCATION:
			currentBg = m_locationBg;
			m_lockPlate.transform.localPosition = m_lockPlateBone.transform.localPosition;
			break;
		default:
			currentBg = m_spellBg;
			break;
		}
		float desatAmount = 0f;
		switch (lockType)
		{
		case CollectionCardVisual.LockType.MAX_COPIES_IN_DECK:
		{
			desatAmount = 0f;
			int maxCardCopies = (m_entityDef.IsElite() ? 1 : 2);
			SetLockText(GameStrings.Format("GLUE_COLLECTION_LOCK_MAX_DECK_COPIES", maxCardCopies));
			break;
		}
		case CollectionCardVisual.LockType.NO_MORE_INSTANCES:
			desatAmount = 1f;
			SetLockText(GameStrings.Get("GLUE_COLLECTION_LOCK_NO_MORE_INSTANCES"));
			break;
		case CollectionCardVisual.LockType.NOT_PLAYABLE:
			desatAmount = 1f;
			SetLockText(GameStrings.Get("GLUE_COLLECTION_LOCK_CARD_NOT_PLAYABLE"));
			break;
		case CollectionCardVisual.LockType.BANNED:
			m_bannedRibbon.SetActive(value: true);
			m_lockPlate.SetActive(value: false);
			foreach (SignatureLock signatureLock2 in m_signatureLocks)
			{
				signatureLock2.SetActive(active: false);
			}
			currentBg.SetActive(value: false);
			return;
		}
		m_lockPlate.SetActive(value: true);
		m_lockPlate.GetComponent<Renderer>().GetMaterial().SetFloat("_Desaturate", desatAmount);
		foreach (SignatureLock signatureLock3 in m_signatureLocks)
		{
			signatureLock3.m_lockPlate.SetActive(value: false);
		}
		if (actor.GetPremium() == TAG_PREMIUM.SIGNATURE && TrySetSignatureLock(desatAmount))
		{
			currentBg.SetActive(value: false);
		}
		else
		{
			currentBg.SetActive(value: true);
			currentBg.GetComponent<Renderer>().GetMaterial().SetFloat("_Desaturate", desatAmount);
		}
		SetLockText(m_lockReason);
	}

	public bool TrySetSignatureLock(float desatAmount)
	{
		if (m_signatureLocks == null && m_signatureLocks.Count <= 0)
		{
			Debug.LogError("CollectionCardLock::TrySetSignatureLock - No signature lock found.");
			return false;
		}
		int signatureFrameId = ActorNames.GetSignatureFrameId(m_entityDef.GetCardId());
		SignatureLock currentSignatureLock = m_signatureLocks.Find((SignatureLock signatureLock) => (signatureFrameId > 2) ? (signatureLock.frameId == 2) : (signatureLock.frameId == signatureFrameId));
		if (currentSignatureLock == null || !currentSignatureLock.IsValid())
		{
			Debug.LogWarning($"CollectionCardLock::TrySetSignatureLock - Signature FrameId {signatureFrameId} " + "for " + m_entityDef.GetCardId() + " not found.");
			currentSignatureLock = m_signatureLocks[0];
			if (currentSignatureLock == null || !currentSignatureLock.IsValid())
			{
				Debug.LogError("CollectionCardLock::TrySetSignatureLock - Failed to set default signature lock");
				return false;
			}
		}
		currentSignatureLock.m_lockPlate.SetActive(value: true);
		currentSignatureLock.m_lockPlate.GetComponent<Renderer>().GetMaterial().SetFloat("_Desaturate", desatAmount);
		currentSignatureLock.m_background.GetComponent<Renderer>().GetMaterial().SetFloat("_Desaturate", desatAmount);
		currentSignatureLock.m_background.SetActive(value: true);
		return true;
	}

	public void SetLockText(string text)
	{
		m_lockText.Text = text;
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
