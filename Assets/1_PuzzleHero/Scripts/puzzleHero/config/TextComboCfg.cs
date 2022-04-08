using System.Collections;
using System.Collections.Generic;
using System;

public class TextComboCfg {

	public Dictionary<string, TextComboData> combo = new Dictionary<string, TextComboData>();
	
	public TextComboData GetTextCombo (int id)
	{
		return this.combo [id.ToString ()];
	}
}
