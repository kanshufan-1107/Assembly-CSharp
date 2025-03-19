using Hearthstone.UI;
using UnityEngine;

public class LettuceTaskCollectionListRow : MonoBehaviour
{
	[SerializeField]
	private Listable m_listable;

	private void Awake()
	{
		m_listable.SetLayerOverride(GameLayer.CameraMask);
	}
}
