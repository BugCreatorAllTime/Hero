public class SurrenderConfirmMediator : IngameBaseMediator
{
	[Inject]
	public SurrenderConfirmView surrenderConfirmView { get; set; }

	public override void OnRegister()
	{
		base.OnRegister();

//		surrenderConfirmView.Init();
		surrenderConfirmView.onClick.AddListener(OnClick);
		surrenderConfirmView.cacheSignal.AddListener(CacheObject);
//		surrenderConfirmView.Setup();
	}
}