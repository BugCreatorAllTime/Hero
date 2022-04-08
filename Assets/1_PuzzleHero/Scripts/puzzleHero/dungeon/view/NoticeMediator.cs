public class NoticeMediator : IngameBaseMediator {

	[Inject]
	public NoticeView noticeView { get; set; }
	
	public override void OnRegister()
	{
		base.OnRegister();
		
//		noticeView.Init();
		noticeView.onClick.AddListener(OnClick);
		noticeView.cacheSignal.AddListener(CacheObject);
//		noticeView.Setup();
	}
}
