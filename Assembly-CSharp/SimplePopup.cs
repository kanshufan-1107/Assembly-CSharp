using UnityEngine;

public class SimplePopup : MonoBehaviour
{
	public GameObject m_targetPopup;

	public GameObject m_targetCollider;

	private bool m_enabled;

	private bool m_mobile;

	private void Awake()
	{
		if (UniversalInputManager.Get().IsTouchMode())
		{
			m_mobile = true;
		}
	}

	private void LateUpdate()
	{
		if (m_enabled || m_mobile)
		{
			RaycastHit hitInfo;
			if (m_mobile && !InputCollection.GetMouseButton(0))
			{
				SetPopupState(enabled: false);
			}
			else if (!UniversalInputManager.Get().GetInputHitInfo(GameLayer.CardRaycast.LayerBit(), out hitInfo))
			{
				SetPopupState(enabled: false);
			}
			else if (hitInfo.transform != null && hitInfo.transform.gameObject == m_targetCollider)
			{
				SetPopupState(enabled: true);
			}
		}
	}

	private void OnMouseEnter()
	{
		if (!m_enabled && !m_mobile)
		{
			SetPopupState(enabled: true);
		}
	}

	private void OnMouseExit()
	{
		if (m_enabled)
		{
			SetPopupState(enabled: false);
		}
	}

	public void SetPopupState(bool enabled)
	{
		if (m_targetPopup != null)
		{
			m_targetPopup.SetActive(enabled);
		}
		m_enabled = enabled;
	}
}
