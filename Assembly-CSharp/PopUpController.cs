using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class PopUpController : MonoBehaviour
{
	public float LifeTime = 4f;

	public GameObject Portrait;

	public GameObject Banner;

	private void Update()
	{
		if (LifeTime >= 0f)
		{
			LifeTime -= Time.deltaTime;
			if (LifeTime <= 0f)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	public void Populate(int currentValue, int totalValue, int cardID, TAG_PREMIUM premium)
	{
		DefLoader.DisposableCardDef def = DefLoader.Get().GetCardDef(cardID);
		if (def != null)
		{
			Texture texture = def.CardDef.GetPortraitTexture(premium);
			if (texture != null)
			{
				Portrait.GetComponent<Renderer>().GetMaterial().SetTexture("_MainTex", texture);
			}
			Banner.GetComponent<RewardBanner>().SetText(currentValue + " out of " + totalValue, "Entity: " + DefLoader.Get().GetEntityDef(cardID).GetName(), "");
		}
	}
}
