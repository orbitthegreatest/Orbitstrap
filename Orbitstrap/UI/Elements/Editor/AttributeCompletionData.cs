using System;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Orbitstrap.UI.Elements.Editor;

public class AttributeCompletionData : ICompletionData
{
	private Action _openValueAutoCompleteAction;

	public ImageSource? Image => null;

	public string Text { get; private set; }

	public object Content => Text;

	public object? Description => null;

	public double Priority { get; }

	public AttributeCompletionData(string text, Action openValueAutoCompleteAction)
	{
		_openValueAutoCompleteAction = openValueAutoCompleteAction;
		Text = text;
	}

	public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
	{
		textArea.Document.Replace(completionSegment, Text + "=\"\"");
		textArea.Caret.Offset = textArea.Caret.Offset - 1;
		_openValueAutoCompleteAction();
	}
}
