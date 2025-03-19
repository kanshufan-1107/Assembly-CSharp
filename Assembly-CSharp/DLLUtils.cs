using System;
using System.IO;
using System.Runtime.InteropServices;

public class DLLUtils
{
	[DllImport("kernel32.dll")]
	public static extern IntPtr LoadLibrary(string filename);

	[DllImport("kernel32.dll")]
	public static extern IntPtr GetProcAddress(IntPtr module, string funcName);

	[DllImport("kernel32.dll")]
	public static extern bool FreeLibrary(IntPtr module);

	public static string GetPluginPath(string fileName)
	{
		return $"Hearthstone_Data/Plugins/{fileName}";
	}

	public static IntPtr LoadPlugin(string fileName, bool handleError = true)
	{
		try
		{
			string relativePath = GetPluginPath(fileName);
			IntPtr dll = LoadLibrary(relativePath);
			string workingDir = Directory.GetCurrentDirectory().Replace("\\", "/");
			if (dll == IntPtr.Zero && handleError)
			{
				string fullPath = $"{workingDir}/{relativePath}";
				Error.AddDevFatal("Failed to load plugin from '{0}'", fullPath);
				Error.AddFatal(FatalErrorReason.LOAD_PLUGIN, "GLOBAL_ERROR_ASSET_LOAD_FAILED", fileName);
			}
			return dll;
		}
		catch (Exception ex)
		{
			Error.AddDevFatal("FileUtils.LoadPlugin() - Exception occurred. message={0} stackTrace={1}", ex.Message, ex.StackTrace);
			return IntPtr.Zero;
		}
	}
}
