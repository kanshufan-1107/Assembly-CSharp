using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class ArcEndSphere : MonoBehaviour
{
	private void Update()
	{
		Material material = GetComponent<Renderer>().GetMaterial();
		Vector2 offset = material.mainTextureOffset;
		offset.x += Time.deltaTime * 1f;
		material.mainTextureOffset = offset;
	}
}
