using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class RarityGem : MonoBehaviour
{
	public void SetRarityGem(TAG_RARITY rarity, TAG_CARD_SET cardSet, TAG_PREMIUM premium)
	{
		Renderer renderer = GetComponent<Renderer>();
		if (rarity == TAG_RARITY.FREE && premium != TAG_PREMIUM.SIGNATURE)
		{
			renderer.enabled = false;
			return;
		}
		renderer.enabled = true;
		switch (rarity)
		{
		default:
			renderer.GetMaterial().mainTextureOffset = new Vector2(0f, 0f);
			break;
		case TAG_RARITY.RARE:
			renderer.GetMaterial().mainTextureOffset = new Vector2(0.118f, 0f);
			break;
		case TAG_RARITY.EPIC:
			renderer.GetMaterial().mainTextureOffset = new Vector2(0.239f, 0f);
			break;
		case TAG_RARITY.LEGENDARY:
			renderer.GetMaterial().mainTextureOffset = new Vector2(0.3575f, 0f);
			break;
		}
	}
}
