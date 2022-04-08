public class DefeatedMediator : IngameBaseMediator
{
	[Inject]
	public DefeatedView defeatedView { get; set; }

	public override void OnRegister()
	{
		base.OnRegister();

//		defeatedView.Init();
		defeatedView.onClick.AddListener(OnClick);
		defeatedView.cacheSignal.AddListener(CacheObject);
//		defeatedView.Setup();
	}
}