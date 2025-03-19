using System.Collections.Generic;

public class UndoableText
{
	public class TextChange
	{
		public enum Type
		{
			Addition,
			Deletion
		}

		public Type changeType;

		public int index;

		public string text;

		public TextChange(Type changeType, int index, string text)
		{
			this.changeType = changeType;
			this.index = index;
			this.text = text;
		}
	}

	private string currentText = string.Empty;

	private Stack<TextChange> UndoStack = new Stack<TextChange>();

	private Stack<TextChange> RedoStack = new Stack<TextChange>();

	public void ProcessChange(string newText)
	{
		TextChange newChange = SimpleDiff(currentText, newText);
		if (newChange == null)
		{
			return;
		}
		TextChange currentChange = ((UndoStack.Count == 0) ? null : UndoStack.Peek());
		currentText = newText;
		if (currentChange != null && newChange.changeType == currentChange.changeType)
		{
			if (newChange.changeType == TextChange.Type.Addition && currentChange.index + currentChange.text.Length == newChange.index)
			{
				currentChange.text += newChange.text;
				return;
			}
			if (newChange.changeType == TextChange.Type.Deletion && currentChange.index - newChange.text.Length == newChange.index)
			{
				currentChange.index = newChange.index;
				currentChange.text = newChange.text + currentChange.text;
				return;
			}
		}
		currentChange = newChange;
		UndoStack.Push(currentChange);
		RedoStack.Clear();
	}

	private TextChange SimpleDiff(string text1, string text2)
	{
		if (text1 == text2)
		{
			return null;
		}
		int text1StartIndex = -1;
		int text2StartIndex = -1;
		int text1EndIndex = -1;
		int text2EndIndex = -1;
		for (int index = 0; index < text1.Length && index < text2.Length; index++)
		{
			if (text1[index] != text2[index])
			{
				text1StartIndex = index;
				text2StartIndex = index;
				break;
			}
		}
		if (text1StartIndex == -1)
		{
			if (text1.Length < text2.Length)
			{
				return new TextChange(TextChange.Type.Addition, text1.Length, text2.Substring(text1.Length));
			}
			if (text1.Length > text2.Length)
			{
				return new TextChange(TextChange.Type.Deletion, text2.Length, text1.Substring(text2.Length));
			}
			return null;
		}
		for (int i = 0; i < text1.Length - text1StartIndex && i < text2.Length - text2StartIndex; i++)
		{
			if (text1[text1.Length - i - 1] != text2[text2.Length - i - 1])
			{
				text1EndIndex = text1.Length - i;
				text2EndIndex = text2.Length - i;
				break;
			}
		}
		if (text1EndIndex == -1)
		{
			if (text1.Length < text2.Length)
			{
				text1EndIndex = text1StartIndex;
				text2EndIndex = text1StartIndex + (text2.Length - text1.Length);
			}
			else if (text1.Length > text2.Length)
			{
				text1EndIndex = text2StartIndex + (text1.Length - text2.Length);
				text2EndIndex = text2StartIndex;
			}
		}
		string text1Change = text1.Substring(text1StartIndex, text1EndIndex - text1StartIndex);
		string text2Change = text2.Substring(text2StartIndex, text2EndIndex - text2StartIndex);
		if (string.IsNullOrEmpty(text1Change))
		{
			return new TextChange(TextChange.Type.Addition, text1StartIndex, text2Change);
		}
		if (string.IsNullOrEmpty(text2Change))
		{
			return new TextChange(TextChange.Type.Deletion, text1StartIndex, text1Change);
		}
		TextChange deletionChange = new TextChange(TextChange.Type.Deletion, text1StartIndex, text1Change);
		UndoStack.Push(deletionChange);
		return new TextChange(TextChange.Type.Addition, text1StartIndex, text2Change);
	}

	public string Undo()
	{
		if (UndoStack.Count > 0)
		{
			TextChange currentChange = UndoStack.Pop();
			RedoStack.Push(currentChange);
			switch (currentChange.changeType)
			{
			case TextChange.Type.Addition:
				DeletionOperation(currentChange);
				break;
			case TextChange.Type.Deletion:
				AdditionOperation(currentChange);
				break;
			}
		}
		return currentText;
	}

	public string Redo()
	{
		if (RedoStack.Count > 0)
		{
			TextChange currentChange = RedoStack.Pop();
			UndoStack.Push(currentChange);
			switch (currentChange.changeType)
			{
			case TextChange.Type.Addition:
				AdditionOperation(currentChange);
				break;
			case TextChange.Type.Deletion:
				DeletionOperation(currentChange);
				break;
			}
		}
		return currentText;
	}

	private void AdditionOperation(TextChange textChange)
	{
		currentText = currentText.Insert(textChange.index, textChange.text);
	}

	private void DeletionOperation(TextChange textChange)
	{
		currentText = currentText.Remove(textChange.index, textChange.text.Length);
	}
}
