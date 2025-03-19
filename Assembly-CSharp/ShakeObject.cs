using UnityEngine;

public class ShakeObject : MonoBehaviour
{
	public float amount = 1f;

	private Vector3 orgPos;

	private void Start()
	{
		orgPos = base.transform.position;
	}

	private void Update()
	{
		float x = Random.value * amount;
		float y = Random.value * amount;
		float z = Random.value * amount;
		x *= amount;
		y *= amount;
		z *= amount;
		base.transform.position = orgPos + new Vector3(x, y, z);
	}
}
