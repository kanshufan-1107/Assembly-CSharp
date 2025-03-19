using UnityEngine;

public class MobileHitBox : MonoBehaviour
{
	public BoxCollider m_boxCollider;

	public float m_scaleX = 1f;

	public float m_scaleY = 1f;

	public float m_scaleZ;

	public Vector3 m_offset;

	public bool m_phoneOnly;

	private bool m_hasExecuted;

	private PlatformDependentValue<bool> m_isMobile = new PlatformDependentValue<bool>(PlatformCategory.Screen)
	{
		Tablet = true,
		MiniTablet = true,
		Phone = true,
		PC = false
	};

	private void Start()
	{
		if (m_boxCollider != null && (bool)m_isMobile && (!m_phoneOnly || (bool)UniversalInputManager.UsePhoneUI))
		{
			Vector3 newSize = default(Vector3);
			newSize.x = m_boxCollider.size.x * m_scaleX;
			newSize.y = m_boxCollider.size.y * m_scaleY;
			if (m_scaleZ == 0f)
			{
				newSize.z = m_boxCollider.size.z * m_scaleY;
			}
			else
			{
				newSize.z = m_boxCollider.size.z * m_scaleZ;
			}
			m_boxCollider.size = newSize;
			m_boxCollider.center += m_offset;
			m_hasExecuted = true;
		}
	}

	public bool HasExecuted()
	{
		return m_hasExecuted;
	}
}
