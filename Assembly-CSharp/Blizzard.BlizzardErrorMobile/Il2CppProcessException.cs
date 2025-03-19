using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Blizzard.BlizzardErrorMobile;

public class Il2CppProcessException
{
	public static void Process(Exception incomingException, ExceptionSettings.ReportType reportType)
	{
		IEnumerable<Exception> enumerable = EnumerateChainedExceptions(incomingException);
		StringBuilder stringBuilder = new StringBuilder();
		string summary = null;
		foreach (Exception exception in enumerable)
		{
			string message = "";
			if (exception.Message != null)
			{
				message = exception.Message.Trim();
			}
			string exceptionType = exception.GetType().Name;
			message = ((!string.IsNullOrEmpty(message)) ? (exceptionType + ": " + message) : exceptionType);
			if (summary == null)
			{
				summary = message;
			}
			else
			{
				stringBuilder.Append("Rethrow as ");
			}
			stringBuilder.AppendLine(exception.Message);
			StackTrace stackTrace = new StackTrace(exception, fNeedFileInfo: true);
			NativeStackTrace nativeStackTrace = GetNativeStackTrace(exception);
			DebugImage mainLibImage = null;
			for (int i = 0; i < stackTrace.FrameCount; i++)
			{
				StackFrame frame = stackTrace.GetFrame(i);
				string nativeFrameText = null;
				if (nativeStackTrace != null)
				{
					IntPtr nativeFrame = nativeStackTrace.Frames[i];
					string mainImageUUID = NormalizeUUID(nativeStackTrace.ImageUuid);
					ulong instructionAddress = (ulong)nativeFrame.ToInt64();
					bool isMainLibFrame = false;
					string package = "";
					MethodBase method = frame.GetMethod();
					if ((object)method != null)
					{
						package = method.DeclaringType?.Assembly.FullName;
						isMainLibFrame = package != null && (package.StartsWith("UnityEngine.", StringComparison.InvariantCultureIgnoreCase) || package.StartsWith("Assembly-CSharp", StringComparison.InvariantCultureIgnoreCase));
						_ = method.Name;
					}
					string notes = null;
					DebugImage image2 = null;
					bool? isRelativeAddress = null;
					if (!isMainLibFrame)
					{
						image2 = FindDebugImageContainingAddress(instructionAddress);
						if (image2 == null)
						{
							isRelativeAddress = true;
							notes = "because it looks like a relative address.";
						}
						else
						{
							isRelativeAddress = false;
							notes = "because it looks like an absolute address inside the range of this debug image.";
						}
					}
					if (image2 == null)
					{
						if (mainImageUUID == null)
						{
							Log.ExceptionReporter.PrintWarning("Couldn't process stack trace - main image UUID reported as NULL by Unity");
							continue;
						}
						if (Application.platform == RuntimePlatform.Android && mainLibImage == null)
						{
							mainLibImage = Il2CppModules.DebugImages.Value.Find((DebugImage image) => image.CodeFile == nativeStackTrace.ImageName);
						}
						if (mainLibImage == null)
						{
							mainLibImage = new DebugImage
							{
								CodeFile = (string.IsNullOrEmpty(nativeStackTrace.ImageName) ? "GameAssembly.fallback" : nativeStackTrace.ImageName),
								Start = 9223372036854775808uL
							};
						}
						image2 = mainLibImage;
						if (isMainLibFrame && notes == null)
						{
							notes = "based on frame package name '" + package + "'.";
						}
					}
					ulong imageAddress = image2.Start;
					bool valueOrDefault = isRelativeAddress == true;
					if (!isRelativeAddress.HasValue)
					{
						valueOrDefault = instructionAddress < imageAddress;
						isRelativeAddress = valueOrDefault;
					}
					if (isRelativeAddress ?? false)
					{
						instructionAddress += imageAddress;
					}
					else
					{
						int ilOffset = frame.GetILOffset();
						if (ilOffset != -1)
						{
							instructionAddress = (ulong)ilOffset;
						}
					}
					bool isWarning = false;
					if (image2.End != 0L && (instructionAddress < imageAddress || instructionAddress >= image2.End))
					{
						isWarning = true;
						if (notes == null)
						{
							notes = ".";
						}
						notes += " However, the instruction address falls out of the range of the debug image.";
					}
					int lastSlashIndex = image2.CodeFile.LastIndexOf('/');
					string imageName = image2.CodeFile.Substring(lastSlashIndex + 1);
					nativeFrameText = string.Format(" ({0} 0x{1:X8} 0x{2:X8} + {3}) [originally {4:X8}] belongs to {5} {6}", imageName, instructionAddress, imageAddress, instructionAddress - imageAddress, nativeFrame.ToInt64(), image2.CodeFile, notes ?? "");
					if (isWarning)
					{
						Log.ExceptionReporter.PrintWarning(nativeFrameText);
					}
					else
					{
						Log.ExceptionReporter.PrintDebug(nativeFrameText);
					}
				}
				AppendNoneIl2CppInfo(stringBuilder, frame);
				if (nativeFrameText != null)
				{
					stringBuilder.Append(" ");
					stringBuilder.AppendLine(nativeFrameText);
				}
			}
		}
		ExceptionReporter.Get().RecordException(summary, stringBuilder.ToString(), recordOnly: false, reportType, happenedBefore: false);
	}

	private static void AppendNoneIl2CppInfo(StringBuilder stringBuilder, StackFrame frame)
	{
		MethodBase methodBase = frame.GetMethod();
		if (methodBase == null)
		{
			return;
		}
		Type classType = methodBase.DeclaringType;
		if (classType == null)
		{
			return;
		}
		string nameSpace = classType.Namespace;
		if (!string.IsNullOrEmpty(nameSpace))
		{
			stringBuilder.Append(nameSpace);
			stringBuilder.Append(".");
		}
		stringBuilder.Append(classType.Name);
		stringBuilder.Append(":");
		stringBuilder.Append(methodBase.Name);
		stringBuilder.Append("(");
		int i = 0;
		ParameterInfo[] parameterInfo = methodBase.GetParameters();
		bool fFirstParam = true;
		for (; i < parameterInfo.Length; i++)
		{
			if (!fFirstParam)
			{
				stringBuilder.Append(", ");
			}
			else
			{
				fFirstParam = false;
			}
			stringBuilder.Append(parameterInfo[i].ParameterType.Name);
		}
		stringBuilder.Append(")");
		string path = frame.GetFileName();
		if (path != null && (!(classType.Name == "Debug") || !(classType.Namespace == "UnityEngine")) && (!(classType.Name == "Logger") || !(classType.Namespace == "UnityEngine")) && (!(classType.Name == "DebugLogHandler") || !(classType.Namespace == "UnityEngine")) && (!(classType.Name == "Assert") || !(classType.Namespace == "UnityEngine.Assertions")) && (!(methodBase.Name == "print") || !(classType.Name == "MonoBehaviour") || !(classType.Namespace == "UnityEngine")))
		{
			stringBuilder.Append(" (at ");
			stringBuilder.Append(path);
			stringBuilder.Append(":");
			stringBuilder.Append(frame.GetFileLineNumber().ToString());
			stringBuilder.Append(")");
		}
	}

	private static string NormalizeUUID(string value)
	{
		return value?.ToLowerInvariant().Replace("-", "").TrimEnd(new char[1] { '0' });
	}

	private static DebugImage FindDebugImageContainingAddress(ulong instructionAddress)
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return null;
		}
		List<DebugImage> list = Il2CppModules.DebugImages.Value;
		int lowerBound = 0;
		int upperBound = list.Count - 1;
		while (lowerBound <= upperBound)
		{
			int mid = (lowerBound + upperBound) / 2;
			DebugImage image = list[mid];
			if (image.Start <= instructionAddress)
			{
				if (instructionAddress <= image.End)
				{
					return image;
				}
				lowerBound = mid + 1;
			}
			else
			{
				upperBound = mid - 1;
			}
		}
		return null;
	}

	private static IEnumerable<Exception> EnumerateChainedExceptions(Exception exception)
	{
		if (exception is AggregateException ae)
		{
			foreach (Exception item in ae.InnerExceptions.SelectMany(EnumerateChainedExceptions))
			{
				yield return item;
			}
		}
		else if (exception.InnerException != null)
		{
			foreach (Exception item2 in EnumerateChainedExceptions(exception.InnerException))
			{
				yield return item2;
			}
		}
		yield return exception;
	}

	private static NativeStackTrace GetNativeStackTrace(Exception e)
	{
		if (Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.Android)
		{
			return null;
		}
		GCHandle gch = GCHandle.Alloc(e);
		IntPtr addresses = IntPtr.Zero;
		try
		{
			IntPtr exc = Il2CppMethods.Il2CppGcHandleGetTarget(GCHandle.ToIntPtr(gch).ToInt32());
			int numFrames = 0;
			string imageUuid = null;
			string imageName = null;
			Il2CppMethods.Il2CppNativeStackTrace(exc, out addresses, out numFrames, out imageUuid, out imageName);
			IntPtr[] frames = new IntPtr[numFrames];
			Marshal.Copy(addresses, frames, 0, numFrames);
			return new NativeStackTrace
			{
				Frames = frames,
				ImageUuid = imageUuid,
				ImageName = imageName
			};
		}
		finally
		{
			gch.Free();
			if (addresses != IntPtr.Zero)
			{
				Il2CppMethods.Il2CppFree(addresses);
			}
		}
	}
}
