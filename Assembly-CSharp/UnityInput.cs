using UnityEngine;

public class UnityInput : IInput
{
	public bool GetMousePosition(out Vector3 position)
	{
		position = Input.mousePosition;
		if (Input.touchSupported && Input.touchCount > 0)
		{
			position = Input.GetTouch(0).position;
		}
		return true;
	}

	public bool GetAnyKey(out bool value)
	{
		value = Input.anyKey;
		return value;
	}

	public bool GetKey(KeyCode keycode, out bool value)
	{
		value = Input.GetKey(keycode);
		return value;
	}

	public bool GetKeyDown(KeyCode keycode, out bool value)
	{
		value = Input.GetKeyDown(keycode);
		return value;
	}

	public bool GetKeyUp(KeyCode keycode, out bool value)
	{
		value = Input.GetKeyUp(keycode);
		return value;
	}

	public bool GetMouseButton(int button, out bool value)
	{
		value = false;
		if (Input.touchSupported)
		{
			if (Input.touchCount > 0)
			{
				TouchPhase touchPhase = Input.GetTouch(0).phase;
				if ((touchPhase == TouchPhase.Began || touchPhase == TouchPhase.Stationary || touchPhase == TouchPhase.Moved) && button == 0)
				{
					value = true;
				}
			}
			else
			{
				value = Input.GetMouseButton(button);
			}
		}
		else
		{
			value = Input.GetMouseButton(button);
		}
		return value;
	}

	public bool GetMouseButtonDown(int button, out bool value)
	{
		value = false;
		if (Input.touchSupported)
		{
			if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
			{
				if (button == 0)
				{
					value = true;
				}
			}
			else
			{
				value = Input.GetMouseButtonDown(button);
			}
		}
		else
		{
			value = Input.GetMouseButtonDown(button);
		}
		return value;
	}

	public bool GetMouseButtonUp(int button, out bool value)
	{
		value = false;
		if (Input.touchSupported)
		{
			if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
			{
				if (button == 0)
				{
					value = true;
				}
			}
			else
			{
				value = Input.GetMouseButtonUp(button);
			}
		}
		else
		{
			value = Input.GetMouseButtonUp(button);
		}
		return value;
	}
}
