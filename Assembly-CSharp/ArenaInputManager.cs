using UnityEngine;

public class ArenaInputManager : MonoBehaviour
{
	private static ArenaInputManager s_instance;

	private void Awake()
	{
		s_instance = this;
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	public static ArenaInputManager Get()
	{
		return s_instance;
	}
}
