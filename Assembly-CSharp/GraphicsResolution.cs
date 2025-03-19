using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone;
using UnityEngine;

public class GraphicsResolution : IComparable
{
	private const int MIN_ASPECT_RATIO_WIDTH = 4;

	private const int MIN_ASPECT_RATIO_HEIGHT = 3;

	private const int MAX_ASPECT_RATIO_WIDTH = 16;

	private const int MAX_ASPECT_RATIO_HEIGHT = 9;

	private const int MIN_WINDOW_WIDTH = 400;

	private const int MIN_WINDOW_HEIGHT = 400;

	private const int TASKBAR_HEIGHT = 63;

	private const float ASPECT_RATIO_ERROR_ALLOWANCE = 0.051f;

	public static readonly List<GraphicsResolution> resolutions_ = new List<GraphicsResolution>();

	public static List<GraphicsResolution> list
	{
		get
		{
			if (resolutions_.Count == 0)
			{
				lock (resolutions_)
				{
					Resolution[] resolutions = Screen.resolutions;
					for (int i = 0; i < resolutions.Length; i++)
					{
						Resolution res = resolutions[i];
						if (IsAspectRatioWithinLimit(res.width, res.height, isWindowedMode: false))
						{
							add(res.width, res.height);
						}
					}
					resolutions_.Reverse();
				}
			}
			return resolutions_;
		}
	}

	public static GraphicsResolution current => create(Screen.currentResolution);

	public int x { get; private set; }

	public int y { get; private set; }

	public float aspectRatio { get; private set; }

	private GraphicsResolution()
	{
	}

	private GraphicsResolution(int width, int height)
	{
		x = width;
		y = height;
		aspectRatio = (float)x / (float)y;
	}

	public static GraphicsResolution create(Resolution res)
	{
		return new GraphicsResolution(res.width, res.height);
	}

	public static GraphicsResolution create(int width, int height)
	{
		return new GraphicsResolution(width, height);
	}

	private static bool add(int width, int height)
	{
		GraphicsResolution res = new GraphicsResolution(width, height);
		if (resolutions_.BinarySearch(res) >= 0)
		{
			return false;
		}
		resolutions_.Add(res);
		resolutions_.Sort();
		return true;
	}

	public int CompareTo(object obj)
	{
		if (!(obj is GraphicsResolution that))
		{
			return 1;
		}
		if (x < that.x)
		{
			return -1;
		}
		if (x > that.x)
		{
			return 1;
		}
		if (y < that.y)
		{
			return -1;
		}
		if (y > that.y)
		{
			return 1;
		}
		return 0;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is GraphicsResolution other))
		{
			return false;
		}
		if (x == other.x)
		{
			return y == other.y;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (23 * 17 + x.GetHashCode()) * 17 + y.GetHashCode();
	}

	public static GraphicsResolution GetLargestResolution()
	{
		return list.First();
	}

	public static bool IsAspectRatioWithinLimit(int width, int height, bool isWindowedMode)
	{
		if (HearthstoneApplication.IsInternal())
		{
			return true;
		}
		if (isWindowedMode && (float)width / (float)height > 1.5f)
		{
			height += 63;
		}
		if (width >= 400 && height >= 400 && CompareAspectRatio(16, 9, width, height) >= 0)
		{
			return CompareAspectRatio(width, height, 4, 3) >= 0;
		}
		return false;
	}

	public static int CompareAspectRatio(int lWidth, int lHeight, int rWidth, int rHeight)
	{
		float lAspectRatio = (float)lWidth / (float)lHeight;
		float rAspectRatio = (float)rWidth / (float)rHeight;
		if (Mathf.Abs(lAspectRatio - rAspectRatio) < 0.051f)
		{
			return 0;
		}
		if (lAspectRatio > rAspectRatio)
		{
			return 1;
		}
		return -1;
	}

	public static int[] CalcAspectRatioLimit(int x, int y)
	{
		int width = Mathf.Max(x, 400);
		int height = Mathf.Max(y, 400);
		if (CompareAspectRatio(width, height, 16, 9) > 0)
		{
			width = (int)((float)height * 16f / 9f);
		}
		else if (CompareAspectRatio(width, height, 4, 3) < 0)
		{
			width = (int)((float)height * 4f / 3f);
		}
		return new int[2] { width, height };
	}
}
