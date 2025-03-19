using System;
using Blizzard.T5.Services;
using UnityEngine;

public interface ITouchScreenService : IService
{
	void ShowKeyboard();

	void HideKeyboard();

	string GetIntelDeviceName();

	PowerSource GetBatteryMode();

	bool IsVirtualKeyboardVisible();

	Vector3 GetTouchPosition();

	Vector3 GetTouchPositionForGUI();

	bool IsTouchSupported();

	void AddOnVirtualKeyboardShowListener(Action listener);

	void RemoveOnVirtualKeyboardShowListener(Action listener);

	void AddOnVirtualKeyboardHideListener(Action listener);

	void RemoveOnVirtualKeyboardHideListener(Action listener);
}
