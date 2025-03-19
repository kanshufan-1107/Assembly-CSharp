public struct ScreenEffectParameters
{
	public static ScreenEffectParameters BlurVignetteDesaturatePerspective = new ScreenEffectParameters(ScreenEffectType.BLUR | ScreenEffectType.VIGNETTE | ScreenEffectType.DESATURATE, ScreenEffectPassLocation.PERSPECTIVE, 0.5f, iTween.EaseType.easeOutCirc, BlurParameters.Default, VignetteParameters.Default, DesaturateParameters.Default, null);

	public static ScreenEffectParameters BlurVignetteDesaturateOrthographic = new ScreenEffectParameters(ScreenEffectType.BLUR | ScreenEffectType.VIGNETTE | ScreenEffectType.DESATURATE, ScreenEffectPassLocation.ORTHOGRAPHIC, 0.5f, iTween.EaseType.easeOutCirc, BlurParameters.Default, VignetteParameters.Default, DesaturateParameters.Default, null);

	public static ScreenEffectParameters BlurDesaturatePerspective;

	public static ScreenEffectParameters BlurVignettePerspective;

	public static ScreenEffectParameters BlurVignetteOrthographic;

	public static ScreenEffectParameters VignettePerspective;

	public static ScreenEffectParameters VignetteOrthographic;

	public static ScreenEffectParameters VignetteDesaturatePerspective;

	public static ScreenEffectParameters DesaturatePerspective;

	public static ScreenEffectParameters BlendToColorPerspective;

	public static ScreenEffectParameters None;

	public const ScreenEffectType BlurVignetteType = ScreenEffectType.BLUR | ScreenEffectType.VIGNETTE;

	public const ScreenEffectType BlurVignetteDesaturateType = ScreenEffectType.BLUR | ScreenEffectType.VIGNETTE | ScreenEffectType.DESATURATE;

	public const iTween.EaseType DefaultEaseType = iTween.EaseType.easeOutCirc;

	public const float DefaultTweenTime = 0.5f;

	public ScreenEffectType Type;

	public ScreenEffectPassLocation PassLocation;

	public float Time;

	public iTween.EaseType EaseType;

	public BlurParameters Blur;

	public VignetteParameters Vignette;

	public DesaturateParameters Desaturate;

	public BlendToColorParameters BlendToColor;

	public ScreenEffectParameters(ScreenEffectType type = ScreenEffectType.BLUR | ScreenEffectType.VIGNETTE, ScreenEffectPassLocation pass = ScreenEffectPassLocation.PERSPECTIVE, float time = 0.5f, iTween.EaseType easeType = iTween.EaseType.easeOutCirc, BlurParameters? blur = null, VignetteParameters? vignette = null, DesaturateParameters? desaturate = null, BlendToColorParameters? blendToColor = null)
	{
		Type = type;
		PassLocation = pass;
		Time = time;
		EaseType = easeType;
		if (!blur.HasValue)
		{
			blur = BlurParameters.None;
		}
		if (!vignette.HasValue)
		{
			vignette = VignetteParameters.None;
		}
		if (!desaturate.HasValue)
		{
			desaturate = DesaturateParameters.None;
		}
		if (!blendToColor.HasValue)
		{
			blendToColor = BlendToColorParameters.None;
		}
		Blur = blur.Value;
		Vignette = vignette.Value;
		Desaturate = desaturate.Value;
		BlendToColor = blendToColor.Value;
	}

	static ScreenEffectParameters()
	{
		BlurParameters? blur = BlurParameters.Default;
		DesaturateParameters? desaturate = DesaturateParameters.Default;
		BlurDesaturatePerspective = new ScreenEffectParameters(ScreenEffectType.BLUR | ScreenEffectType.DESATURATE, ScreenEffectPassLocation.PERSPECTIVE, 0.5f, iTween.EaseType.easeOutCirc, blur, null, desaturate, null);
		BlurVignettePerspective = new ScreenEffectParameters(ScreenEffectType.BLUR | ScreenEffectType.VIGNETTE, ScreenEffectPassLocation.PERSPECTIVE, 0.5f, iTween.EaseType.easeOutCirc, BlurParameters.Default, VignetteParameters.Default, null, null);
		BlurVignetteOrthographic = new ScreenEffectParameters(ScreenEffectType.BLUR | ScreenEffectType.VIGNETTE, ScreenEffectPassLocation.ORTHOGRAPHIC, 0.5f, iTween.EaseType.easeOutCirc, BlurParameters.Default, VignetteParameters.Default, null, null);
		VignetteParameters? vignette = VignetteParameters.Default;
		VignettePerspective = new ScreenEffectParameters(ScreenEffectType.VIGNETTE, ScreenEffectPassLocation.PERSPECTIVE, 0.5f, iTween.EaseType.easeInOutCubic, null, vignette, null, null);
		vignette = VignetteParameters.Default;
		VignetteOrthographic = new ScreenEffectParameters(ScreenEffectType.VIGNETTE, ScreenEffectPassLocation.ORTHOGRAPHIC, 0.5f, iTween.EaseType.easeInOutCubic, null, vignette, null, null);
		vignette = VignetteParameters.Default;
		desaturate = DesaturateParameters.Default;
		VignetteDesaturatePerspective = new ScreenEffectParameters(ScreenEffectType.VIGNETTE | ScreenEffectType.DESATURATE, ScreenEffectPassLocation.PERSPECTIVE, 0.5f, iTween.EaseType.easeInOutCubic, null, vignette, desaturate, null);
		desaturate = new DesaturateParameters(0.9f);
		DesaturateParameters? desaturate2 = desaturate;
		BlendToColorParameters? blendToColor = null;
		DesaturatePerspective = new ScreenEffectParameters(ScreenEffectType.DESATURATE, ScreenEffectPassLocation.PERSPECTIVE, 0.4f, iTween.EaseType.easeInOutQuad, null, null, desaturate2, blendToColor);
		blendToColor = default(BlendToColorParameters);
		BlendToColorPerspective = new ScreenEffectParameters(ScreenEffectType.BLENDTOCOLOR, ScreenEffectPassLocation.PERSPECTIVE, 0.5f, iTween.EaseType.easeInOutCubic, null, null, null, blendToColor);
		None = new ScreenEffectParameters(ScreenEffectType.NONE, ScreenEffectPassLocation.NONE, 0.5f, iTween.EaseType.easeOutCirc, null, null, null, null);
	}
}
