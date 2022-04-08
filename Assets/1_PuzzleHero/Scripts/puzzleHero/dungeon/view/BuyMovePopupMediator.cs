public class BuyMovePopupMediator : IngameBaseMediator {

	[Inject]
	public BuyMovePopupView buyMoveView { get; set; }
	
	public override void OnRegister()
	{
		base.OnRegister();
		buyMoveView.onClick.AddListener(OnClick);
		buyMoveView.cacheSignal.AddListener(CacheObject);
	}
}
