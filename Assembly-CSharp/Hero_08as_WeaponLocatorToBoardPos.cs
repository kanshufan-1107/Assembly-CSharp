using UnityEngine;

public class Hero_08as_WeaponLocatorToBoardPos : MonoBehaviour
{
	public Spell m_spell;

	public string m_weaponLocName;

	public GameObject m_targetObject;

	private Hero_08as_WeaponLocator m_weaponLocator;

	private Transform m_portraitTransform;

	private Vector3 m_lastWorldP;

	private Mesh m_portraitMesh;

	private void Start()
	{
		if (m_targetObject == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		Actor actor = m_spell.GetSourceCard()?.GetHeroCard()?.GetActor();
		m_weaponLocator = (actor?.LegendaryHeroPortrait)?.FindGameObjectInLegendaryPortraitPrefab(m_weaponLocName)?.GetComponent<Hero_08as_WeaponLocator>();
		if (m_weaponLocator == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		m_portraitMesh = actor.m_portraitMesh.GetComponent<MeshFilter>()?.sharedMesh;
		if (m_portraitMesh == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		m_portraitTransform = actor.m_portraitMesh?.transform;
		if (m_portraitTransform == null)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		m_targetObject.transform.position = CalculateWeaponLoc();
	}

	public Vector3 CalculateWeaponLoc()
	{
		Vector3 weaponLocCoords = m_weaponLocator.GetWeaponLocatorPos();
		if (ConvertUVToWorld(weaponLocCoords, out var worldP))
		{
			m_lastWorldP = worldP;
		}
		return m_lastWorldP;
	}

	private bool ConvertUVToWorld(Vector2 p, out Vector3 worldP)
	{
		Vector2[] uvs = m_portraitMesh.uv2;
		int[] triangles = m_portraitMesh.triangles;
		for (int i = 0; i < triangles.Length; i += 3)
		{
			Vector2 a = uvs[triangles[i]];
			Vector2 b = uvs[triangles[i + 1]];
			Vector2 vector = uvs[triangles[i + 2]];
			Vector2 uv0 = b - a;
			Vector2 uv1 = vector - a;
			Vector2 lhs = p - a;
			float d00 = Vector2.Dot(uv0, uv0);
			float d1 = Vector2.Dot(uv0, uv1);
			float d11 = Vector2.Dot(uv1, uv1);
			float d20 = Vector2.Dot(lhs, uv0);
			float d21 = Vector2.Dot(lhs, uv1);
			float det = d00 * d11 - d1 * d1;
			if (det != 0f)
			{
				float oneOverDet = 1f / det;
				float v = (d11 * d20 - d1 * d21) * oneOverDet;
				float w = (d00 * d21 - d1 * d20) * oneOverDet;
				if (v >= 0f && w >= 0f && v + w <= 1f)
				{
					float u = 1f - v - w;
					Vector3[] vertices = m_portraitMesh.vertices;
					Vector3 v2 = m_portraitTransform.TransformPoint(vertices[triangles[i]]);
					Vector3 v3 = m_portraitTransform.TransformPoint(vertices[triangles[i + 1]]);
					Vector3 v4 = m_portraitTransform.TransformPoint(vertices[triangles[i + 2]]);
					worldP = u * v2 + v * v3 + w * v4;
					return true;
				}
			}
		}
		worldP = Vector3.zero;
		return false;
	}
}
