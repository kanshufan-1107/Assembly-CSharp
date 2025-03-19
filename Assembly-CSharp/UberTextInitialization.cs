public class UberTextInitialization
{
	public static void InitializeUberText()
	{
		UberTextSetup.SetConfig(CreateUberTextConfig());
	}

	private static UberTextConfig CreateUberTextConfig()
	{
		return new UberTextConfig(28, new UberTextRenderTextureTracker(), new UberTextShhadetUtil(), Log.UberText);
	}
}
