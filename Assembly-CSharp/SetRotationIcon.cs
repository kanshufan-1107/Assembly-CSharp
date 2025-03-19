using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using UnityEngine;

public class SetRotationIcon : MonoBehaviour
{
	public GameObject m_YearIconQuad;

	private static Vector2 SET_ROTATION_ODD_YEAR_TEXTURE_OFFSET = new Vector2(0f, 0.5f);

	private static Vector2 SET_ROTATION_EVEN_YEAR_TEXTURE_OFFSET = new Vector2(0.5f, 0.5f);

	public const string CORE_SET_FILTER_ICON = "Filter_Icons_Core.tif:effec2b862f39224bac756f4a498164a";

	public const string CORE_SET_WATERMARK_EVEN = "CoreIcon_Even.tif:8f398522346ce634bb1e26b2f556403f";

	public const string CORE_SET_WATERMARK_ODD = "CoreIcon_Odd.tif:66255e7e42828b94c828861986fa68f8";

	private void Awake()
	{
		m_YearIconQuad.GetComponent<Renderer>().GetMaterial().mainTextureOffset = GetYearIconTextureOffset();
	}

	public static Vector2 GetYearIconTextureOffset()
	{
		if (!ServiceManager.TryGet<SetRotationManager>(out var setRotationManager) || setRotationManager.GetActiveSetRotationYear() % 2 != 0)
		{
			return SET_ROTATION_ODD_YEAR_TEXTURE_OFFSET;
		}
		return SET_ROTATION_EVEN_YEAR_TEXTURE_OFFSET;
	}

	public static string GetYearIconWatermark()
	{
		if (!ServiceManager.TryGet<SetRotationManager>(out var setRotationManager) || setRotationManager.GetActiveSetRotationYear() % 2 != 0)
		{
			return "CoreIcon_Odd.tif:66255e7e42828b94c828861986fa68f8";
		}
		return "CoreIcon_Even.tif:8f398522346ce634bb1e26b2f556403f";
	}
}
