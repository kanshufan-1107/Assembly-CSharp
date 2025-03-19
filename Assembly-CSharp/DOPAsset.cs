using UnityEngine;

public class DOPAsset : ScriptableObject
{
	public int DataVersion;

	public static DOPAsset GenerateDOPAsset()
	{
		DOPAsset dOPAsset = ScriptableObject.CreateInstance<DOPAsset>();
		dOPAsset.DataVersion = 31600;
		return dOPAsset;
	}
}
