public class TutorialInfoUIMediator : IngameBaseMediator {

	[Inject]
	public TutorialInfoUIView tutView { get; set; }
	
	public override void OnRegister()
	{
		base.OnRegister();

		tutView.onClick.AddListener(OnClick);
		tutView.cacheSignal.AddListener(CacheObject);
	}
}
