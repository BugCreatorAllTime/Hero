using UnityEngine;
using System.Collections;

public class TextInput : MonoBehaviour {

	UIInput mInput;

	void Start ()
	{
		mInput = GetComponent<UIInput>();
		mInput.label.maxLineCount = 1;
	}

	public void OnChat()
	{
		string text = NGUIText.StripSymbols(mInput.value);

		if (!string.IsNullOrEmpty(text))
		{
			GameObject.Find("YourName").GetComponent<UILabel>().text = text;
			mInput.value = "";
			mInput.isSelected = false;
		}
	}
}
