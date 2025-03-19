using System;
using System.Collections.Generic;
using UnityEngine;

public class MercenaryRoleGemObject : GemObject
{
	[Serializable]
	public class RoleGemObjectMapping
	{
		[SerializeField]
		public TAG_ROLE m_role;

		[SerializeField]
		public GameObject m_roleGemObject;
	}

	public List<RoleGemObjectMapping> m_roleGemObjects;

	public void SetRole(TAG_ROLE role)
	{
		foreach (RoleGemObjectMapping gemObject in m_roleGemObjects)
		{
			gemObject.m_roleGemObject.SetActive(gemObject.m_role == role);
		}
	}
}
