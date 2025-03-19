using UnityEngine;

public class ReturnBoardDustVFX : MonoBehaviour
{
	private void OnParticleSystemStopped()
	{
		Board.Get().ReturnDisabledDustVFX(base.gameObject);
	}
}
