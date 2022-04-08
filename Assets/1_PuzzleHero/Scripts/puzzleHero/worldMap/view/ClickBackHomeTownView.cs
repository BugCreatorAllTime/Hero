using UnityEngine;
using System.Collections;
using strange.extensions.signal.impl;
using strange.extensions.mediation.impl;

public class ClickBackHomeTownView : View {
	
	internal Signal clickSignal = new Signal();

	public void Click(){
		clickSignal.Dispatch();
	}
}
