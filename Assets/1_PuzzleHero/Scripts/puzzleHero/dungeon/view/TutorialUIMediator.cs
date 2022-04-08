public class TutorialUIMediator : IngameBaseMediator {

	[Inject]
	public TutorialUIView tutView { get; set; }
	
	public override void OnRegister()
	{
		base.OnRegister();

		tutView.onClick.AddListener(OnClick);
		tutView.cacheSignal.AddListener(CacheObject);
	}
}
