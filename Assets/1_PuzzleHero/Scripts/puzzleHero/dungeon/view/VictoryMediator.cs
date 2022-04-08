public class VictoryMediator : IngameBaseMediator {

	[Inject]
	public VictoryView victoryView { get; set; }
	
	public override void OnRegister()
	{
		base.OnRegister();
		
//		victoryView.Init();
		victoryView.onClick.AddListener(OnClick);
		victoryView.cacheSignal.AddListener(CacheObject);
//		victoryView.Setup();
	}
}
