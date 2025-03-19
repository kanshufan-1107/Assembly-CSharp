using System.Collections.Generic;
using UnityEngine;

public class Column : Rectangle
{
	private List<Rectangle> rows;

	private int heightAvailable
	{
		get
		{
			if (used)
			{
				return 0;
			}
			if (rows == null)
			{
				return position.height;
			}
			int height = position.height;
			foreach (Rectangle row in rows)
			{
				height -= row.position.height;
			}
			return height;
		}
	}

	public Column(int x, int y, int w, int h)
		: base(x, y, w, h)
	{
	}

	public override Rectangle Insert(int w, int h)
	{
		if (used)
		{
			return null;
		}
		if (w > position.width)
		{
			return null;
		}
		if (rows == null && position.width == w && position.height == h)
		{
			used = true;
			return this;
		}
		if (rows == null)
		{
			rows = new List<Rectangle>();
		}
		foreach (Rectangle row in rows)
		{
			Rectangle rect = row.Insert(w, h);
			if (rect != null)
			{
				return rect;
			}
		}
		if (h > heightAvailable)
		{
			return null;
		}
		Row newRow = new Row(position.x, position.y + position.height - heightAvailable, position.width, h);
		rows.Add(newRow);
		rows.Sort((Rectangle lhs, Rectangle rhs) => lhs.position.height.CompareTo(rhs.position.height));
		return newRow.Insert(w, h);
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
		if (rows != null)
		{
			foreach (Rectangle row in rows)
			{
				if (row.Remove(rect))
				{
					return true;
				}
			}
		}
		return false;
	}
}
