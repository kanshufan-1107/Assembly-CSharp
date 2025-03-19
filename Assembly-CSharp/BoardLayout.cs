using Assets;
using UnityEngine;

public class BoardLayout : MonoBehaviour
{
	public Transform m_BoneParent;

	public Transform m_ColliderParent;

	public virtual void Awake()
	{
		if (LoadingScreen.Get() != null)
		{
			LoadingScreen.Get().NotifyMainSceneObjectAwoke(base.gameObject);
		}
	}

	public Transform FindBone(string name)
	{
		return m_BoneParent.Find(name);
	}

	public Collider FindCollider(string name)
	{
		Transform t = m_ColliderParent.Find(name);
		if (!(t == null))
		{
			return t.GetComponent<Collider>();
		}
		return null;
	}

	public static string GetBoardLayoutPrefab(Scenario.BoardLayout boardLayout)
	{
		return boardLayout switch
		{
			Scenario.BoardLayout.STANDARD => "BoardStandardGame.prefab:b87d693f752160b43a25b7cec3787122", 
			Scenario.BoardLayout.LETTUCE => "BoardLettuceGame.prefab:9e87f54ccdfb2d848b82dbba40b52df4", 
			_ => null, 
		};
	}
}
