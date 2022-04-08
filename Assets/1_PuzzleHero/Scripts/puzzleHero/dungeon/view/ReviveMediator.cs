public class ReviveMediator : IngameBaseMediator
{
	[Inject]
	public ReviveView reviveView { get; set; }

	public override void OnRegister()
	{
		base.OnRegister();

//		reviveView.Init();
		reviveView.onClick.AddListener(OnClick);
		reviveView.cacheSignal.AddListener(CacheObject);
//		reviveView.Setup();
	}
}