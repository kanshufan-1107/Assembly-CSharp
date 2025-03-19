using System;
using System.Runtime.InteropServices;

namespace Blizzard.BlizzardErrorMobile;

public class Il2CppMethods
{
	public static IntPtr Il2CppGcHandleGetTarget(int gchandle)
	{
		return il2cpp_gchandle_get_target(gchandle);
	}

	public static void Il2CppNativeStackTrace(IntPtr exc, out IntPtr addresses, out int numFrames, out string imageUUID, out string imageName)
	{
		Il2CppNativeStackTraceShim(exc, out addresses, out numFrames, out imageUUID, out imageName);
	}

	public static void Il2CppFree(IntPtr ptr)
	{
		il2cpp_free(ptr);
	}

	private static string? SanitizeDebugId(IntPtr debugIdPtr)
	{
		if (debugIdPtr == IntPtr.Zero)
		{
			return null;
		}
		return Marshal.PtrToStringAnsi(debugIdPtr);
	}

	[DllImport("__Internal")]
	private static extern IntPtr il2cpp_gchandle_get_target(int gchandle);

	[DllImport("__Internal")]
	private static extern void il2cpp_free(IntPtr ptr);

	private static void Il2CppNativeStackTraceShim(IntPtr exc, out IntPtr addresses, out int numFrames, out string imageUUID, out string imageName)
	{
		IntPtr uuidBuffer = IntPtr.Zero;
		IntPtr imageNameBuffer = IntPtr.Zero;
		il2cpp_native_stack_trace(exc, out addresses, out numFrames, out uuidBuffer, out imageNameBuffer);
		try
		{
			imageUUID = SanitizeDebugId(uuidBuffer);
			imageName = ((imageNameBuffer == IntPtr.Zero) ? null : Marshal.PtrToStringAnsi(imageNameBuffer));
		}
		finally
		{
			il2cpp_free(uuidBuffer);
			il2cpp_free(imageNameBuffer);
		}
	}

	[DllImport("__Internal")]
	private static extern void il2cpp_native_stack_trace(IntPtr exc, out IntPtr addresses, out int numFrames, out IntPtr imageUUID, out IntPtr imageName);
}
