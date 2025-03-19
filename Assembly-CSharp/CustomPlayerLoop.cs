using Cysharp.Threading.Tasks;
using UnityEngine.LowLevel;

public class CustomPlayerLoop
{
	public static void SetupCustomPlayerLoop()
	{
		PlayerLoopSystem playerLoopSystem = PlayerLoop.GetDefaultPlayerLoop();
		PlayerLoopHelper.Initialize(ref playerLoopSystem);
		PlayerLoop.SetPlayerLoop(playerLoopSystem);
	}
}
