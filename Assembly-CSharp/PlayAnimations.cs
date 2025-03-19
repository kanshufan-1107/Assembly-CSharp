using UnityEngine;

public class PlayAnimations : MonoBehaviour
{
	private Animation m_coinDropAnimation;

	public void Awake()
	{
		m_coinDropAnimation = GetComponent<Animation>();
	}

	public void Update()
	{
		m_coinDropAnimation.PlayQueued("CoinDropA");
		m_coinDropAnimation.PlayQueued("CoinDropB");
	}
}
