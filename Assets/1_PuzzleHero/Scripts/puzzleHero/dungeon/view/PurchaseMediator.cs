public class PurchaseMediator : IngameBaseMediator {

	[Inject]
	public PurchaseView purchaseView { get; set;}

	public override void OnRegister()
	{
		base.OnRegister();
		
//		purchaseView.Init();
		purchaseView.onClick.AddListener(OnClick);
		purchaseView.cacheSignal.AddListener(CacheObject);
//		purchaseView.Setup();
	}
}
