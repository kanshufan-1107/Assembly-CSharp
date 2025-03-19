using System;
using System.IO;
using UnityEngine;

namespace Blizzard.BlizzardErrorMobile;

internal static class Screenshot
{
	private static string m_screenshotPath;

	public static string ScreenshotPath
	{
		get
		{
			if (string.IsNullOrEmpty(m_screenshotPath))
			{
				m_screenshotPath = Path.Combine(Application.persistentDataPath, "Screenshot-0.png");
			}
			return m_screenshotPath;
		}
	}

	public static void RemoveScreenshot()
	{
		if (File.Exists(ScreenshotPath))
		{
			File.Delete(ScreenshotPath);
		}
	}

	public static bool CaptureScreenshot(int maxWidth, string screenshotPath = null)
	{
		m_screenshotPath = screenshotPath;
		RemoveScreenshot();
		if (maxWidth < 0)
		{
			ExceptionLogger.LogDebug("Skip generating Screenshot");
			return false;
		}
		ExceptionLogger.LogDebug("Making Screenshot");
		Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, mipChain: true);
		screenshot.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
		screenshot.Apply();
		if (maxWidth > 0)
		{
			ExceptionLogger.LogDebug("Resizing Screenshot");
			int resizedWidth = screenshot.width;
			int resizedHeight = screenshot.height;
			float rate = (float)maxWidth / (float)screenshot.width;
			resizedWidth = maxWidth;
			resizedHeight = Convert.ToInt32((float)screenshot.height * rate);
			screenshot = ScaleTexture(screenshot, resizedWidth, resizedHeight);
		}
		byte[] bytes = screenshot.EncodeToPNG();
		File.WriteAllBytes(ScreenshotPath, bytes);
		ExceptionLogger.LogDebug("Captured " + ScreenshotPath);
		return true;
	}

	private static Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
	{
		Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, mipChain: true);
		Color[] rpixels = result.GetPixels(0);
		float incX = 1f / (float)source.width * ((float)source.width / (float)targetWidth);
		float incY = 1f / (float)source.height * ((float)source.height / (float)targetHeight);
		for (int px = 0; px < rpixels.Length; px++)
		{
			rpixels[px] = source.GetPixelBilinear(incX * ((float)px % (float)targetWidth), incY * Mathf.Floor(px / targetWidth));
		}
		result.SetPixels(rpixels, 0);
		result.Apply();
		return result;
	}
}
