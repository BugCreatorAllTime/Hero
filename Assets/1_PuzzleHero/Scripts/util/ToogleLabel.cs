	using UnityEngine;

public class ToogleLabel : MonoBehaviour
{
	public string onLabel;
	public string offLabel;

	private UILabel label;
	private UIToggle toggle;

	void Awake()
	{
		label = gameObject.transform.Find("Label").GetComponent<UILabel>();
		toggle = gameObject.GetComponent<UIToggle>();

		EventDelegate eventDelegate = new EventDelegate(this, "OnToggled");
		toggle.onChange.Add(eventDelegate);
	}

	private void OnToggled()
	{
//		Logger.Trace("toggled value", toggle.value);
		switch (toggle.value)
		{
			case true:
				label.text = onLabel;
				break;
			case false:
				label.text = offLabel;
				break;
		}
	}
}
