using System.Collections.Generic;
using UnityEngine;

public class Row : Rectangle
{
	private List<Rectangle> columns;

	private int widthAvailable
	{
		get
		{
			if (used)
			{
				return 0;
			}
			if (columns == null)
			{
				return position.width;
			}
			int width = position.width;
			foreach (Rectangle column in columns)
			{
				width -= column.position.width;
			}
			return width;
		}
	}

	public Row(int x, int y, int w, int h)
		: base(x, y, w, h)
	{
	}

	public override Rectangle Insert(int w, int h)
	{
		if (used)
		{
			return null;
		}
		if (h > position.height)
		{
			return null;
		}
		if (columns == null && w == position.width && h == position.height)
		{
			used = true;
			return this;
		}
		if (columns == null)
		{
			columns = new List<Rectangle>();
		}
		foreach (Rectangle column in columns)
		{
			Rectangle rect = column.Insert(w, h);
			if (rect != null)
			{
				return rect;
			}
		}
		if (h > widthAvailable)
		{
			return null;
		}
		Column newColumn = new Column(position.x + position.width - widthAvailable, position.y, w, position.height);
		columns.Add(newColumn);
		return newColumn.Insert(w, h);
	}

	public override bool Remove(RectInt rect)
	{
		if (!Contains(rect))
		{
			return false;
		}
		if (used && position.Equals(rect))
		{
			used = false;
			return true;
		}
		if (columns != null)
		{
			foreach (Rectangle column in columns)
			{
				if (column.Remove(rect))
				{
					return true;
				}
			}
		}
		return false;
	}
}
