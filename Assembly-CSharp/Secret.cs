using UnityEngine;

public class Secret : MonoBehaviour
{
	public UberText secretLabelTop;

	public UberText secretLabelMiddle;

	public UberText secretLabelBottom;

	private void Start()
	{
		secretLabelTop.SetText(GameStrings.Get("GAMEPLAY_SECRET_BANNER_TITLE"));
		secretLabelMiddle.SetText(GameStrings.Get("GAMEPLAY_SECRET_BANNER_TITLE"));
		secretLabelBottom.SetText(GameStrings.Get("GAMEPLAY_SECRET_BANNER_TITLE"));
	}
}
