using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class UndoableInputField : MonoBehaviour
{
	private InputField inputField;

	private UndoableText text;

	private bool IsModifierKeyDown
	{
		get
		{
			if (!Input.GetKey(KeyCode.RightControl) && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightMeta))
			{
				return Input.GetKey(KeyCode.LeftMeta);
			}
			return true;
		}
	}

	private void Awake()
	{
		inputField = GetComponent<InputField>();
		text = new UndoableText();
	}

	private void Update()
	{
		if (inputField != null && inputField.isFocused)
		{
			if (IsModifierKeyDown && Input.GetKeyDown(KeyCode.Z))
			{
				inputField.text = text.Undo();
			}
			else if (IsModifierKeyDown && Input.GetKeyDown(KeyCode.Y))
			{
				inputField.text = text.Redo();
			}
			else if (IsModifierKeyDown && Input.GetKeyDown(KeyCode.B))
			{
				HandleBoldCommand();
			}
			else if (IsModifierKeyDown && Input.GetKeyDown(KeyCode.T))
			{
				HandleItalicsCommand();
			}
			else if (Input.anyKeyDown)
			{
				text.ProcessChange(inputField.text);
			}
		}
	}

	private void HandleItalicsCommand()
	{
		string italicsStart = "<i>";
		string italicsEnd = "</i>";
		HandleCommandHelper(italicsStart, italicsEnd);
	}

	private void HandleBoldCommand()
	{
		string boldStart = "<b>";
		string boldEnd = "</b>";
		HandleCommandHelper(boldStart, boldEnd);
	}

	private void HandleCommandHelper(string startFormater, string endFormater)
	{
		int start = ((inputField.selectionAnchorPosition < inputField.selectionFocusPosition) ? inputField.selectionAnchorPosition : inputField.selectionFocusPosition);
		int length = ((inputField.selectionAnchorPosition > inputField.selectionFocusPosition) ? inputField.selectionAnchorPosition : inputField.selectionFocusPosition) - start;
		string text = inputField.text;
		string selection = inputField.text.Substring(start, length);
		string a = GetText(text, start - startFormater.Length, startFormater.Length);
		string suffix = GetText(text, start + length, endFormater.Length);
		if (string.Equals(a, startFormater) && string.Equals(suffix, endFormater))
		{
			text = text.Remove(start + length, endFormater.Length);
			text = text.Remove(start - startFormater.Length, startFormater.Length);
		}
		else if (selection.StartsWith(startFormater) && selection.EndsWith(endFormater))
		{
			text = text.Remove(start + length - endFormater.Length, endFormater.Length);
			text = text.Remove(start, startFormater.Length);
		}
		else
		{
			text = text.Insert(start + length, endFormater);
			text = text.Insert(start, startFormater);
		}
		inputField.text = text;
		inputField.selectionAnchorPosition = 0;
		inputField.selectionFocusPosition = 0;
	}

	private string GetText(string givenString, int offset, int length)
	{
		if (length < 0 || offset < 0 || offset + length > givenString.Length)
		{
			return string.Empty;
		}
		return givenString.Substring(offset, length);
	}
}
