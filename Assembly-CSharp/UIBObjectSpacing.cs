using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CustomEditClass]
public class UIBObjectSpacing : MonoBehaviour
{
	[Serializable]
	public class SpacedObject
	{
		public GameObject m_Object;

		public Vector3 m_Offset;

		public bool m_CountIfNull;
	}

	private class AnimationPosition
	{
		public Vector3 m_targetPos;

		public GameObject m_object;
	}

	public List<SpacedObject> m_Objects = new List<SpacedObject>();

	[SerializeField]
	private Vector3 m_LocalOffset;

	[SerializeField]
	private Vector3 m_LocalSpacing;

	[SerializeField]
	private Vector3 m_Alignment = new Vector3(0.5f, 0.5f, 0.5f);

	public bool m_reverse;

	public Vector3 LocalOffset
	{
		get
		{
			return m_LocalOffset;
		}
		set
		{
			m_LocalOffset = value;
			UpdatePositions();
		}
	}

	public Vector3 LocalSpacing
	{
		get
		{
			return m_LocalSpacing;
		}
		set
		{
			m_LocalSpacing = value;
			UpdatePositions();
		}
	}

	[CustomEditField(Range = "0 - 1")]
	public Vector3 Alignment
	{
		get
		{
			return m_Alignment;
		}
		set
		{
			m_Alignment = value;
			m_Alignment.x = Mathf.Clamp01(m_Alignment.x);
			m_Alignment.y = Mathf.Clamp01(m_Alignment.y);
			m_Alignment.z = Mathf.Clamp01(m_Alignment.z);
			UpdatePositions();
		}
	}

	public void AddSpace(int index)
	{
		m_Objects.Insert(index, new SpacedObject
		{
			m_CountIfNull = true
		});
	}

	public void AddSpace(int index, Vector3 offset)
	{
		m_Objects.Insert(index, new SpacedObject
		{
			m_Offset = offset,
			m_CountIfNull = true
		});
	}

	public void AddObject(GameObject obj, bool countIfNull = true)
	{
		AddObject(obj, Vector3.zero, countIfNull);
	}

	public void AddObject(Component comp, bool countIfNull = true)
	{
		AddObject(comp, Vector3.zero, countIfNull);
	}

	public void AddObject(Component comp, Vector3 offset, bool countIfNull = true)
	{
		AddObject(comp.gameObject, offset, countIfNull);
	}

	public void AddObject(GameObject obj, Vector3 offset, bool countIfNull = true)
	{
		m_Objects.Add(new SpacedObject
		{
			m_Object = obj,
			m_Offset = offset,
			m_CountIfNull = countIfNull
		});
	}

	public void ClearObjects()
	{
		m_Objects.Clear();
	}

	public void AnimateUpdatePositions(float animTime, iTween.EaseType tweenType = iTween.EaseType.easeInOutQuad)
	{
		List<AnimationPosition> objsToAnim = new List<AnimationPosition>();
		List<SpacedObject> validObjects = m_Objects.FindAll((SpacedObject o) => o.m_CountIfNull || (o.m_Object != null && o.m_Object.activeInHierarchy));
		if (m_reverse)
		{
			validObjects.Reverse();
		}
		Vector3 position = m_LocalOffset;
		Vector3 increment = m_LocalSpacing;
		Vector3 alignment = Vector3.zero;
		for (int i = 0; i < validObjects.Count; i++)
		{
			SpacedObject spacedObj = validObjects[i];
			GameObject obj = spacedObj.m_Object;
			if (obj != null)
			{
				objsToAnim.Add(new AnimationPosition
				{
					m_targetPos = position + spacedObj.m_Offset,
					m_object = obj
				});
			}
			Vector3 step = spacedObj.m_Offset;
			if (i < validObjects.Count - 1)
			{
				step += increment;
			}
			position += step;
			alignment += step;
		}
		alignment.x *= m_Alignment.x;
		alignment.y *= m_Alignment.y;
		alignment.z *= m_Alignment.z;
		for (int j = 0; j < objsToAnim.Count; j++)
		{
			AnimationPosition animObj = objsToAnim[j];
			Hashtable args = iTweenManager.Get().GetTweenHashTable();
			args.Add("position", animObj.m_targetPos - alignment);
			args.Add("islocal", true);
			args.Add("easetype", tweenType);
			args.Add("time", animTime);
			iTween.MoveTo(animObj.m_object, args);
		}
	}

	public void UpdatePositions()
	{
		List<SpacedObject> validObjects = m_Objects.FindAll((SpacedObject o) => o.m_CountIfNull || (o.m_Object != null && o.m_Object.activeInHierarchy));
		if (m_reverse)
		{
			validObjects.Reverse();
		}
		Vector3 position = m_LocalOffset;
		Vector3 increment = m_LocalSpacing;
		Vector3 alignment = Vector3.zero;
		for (int i = 0; i < validObjects.Count; i++)
		{
			SpacedObject spacedObj = validObjects[i];
			GameObject obj = spacedObj.m_Object;
			if (obj != null)
			{
				obj.transform.localPosition = position + spacedObj.m_Offset;
			}
			Vector3 step = spacedObj.m_Offset;
			if (i < validObjects.Count - 1)
			{
				step += increment;
			}
			position += step;
			alignment += step;
		}
		alignment.x *= m_Alignment.x;
		alignment.y *= m_Alignment.y;
		alignment.z *= m_Alignment.z;
		for (int j = 0; j < validObjects.Count; j++)
		{
			GameObject obj2 = validObjects[j].m_Object;
			if (obj2 != null)
			{
				obj2.transform.localPosition -= alignment;
			}
		}
	}
}
