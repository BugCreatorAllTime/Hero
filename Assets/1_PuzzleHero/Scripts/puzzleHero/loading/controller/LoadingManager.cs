using strange.examples.strangerocks;
using System.Collections;
using UnityEngine;
public class LoadingManager {

	[Inject]
	public IRoutineRunner routineRunner{ get; set; }

	[Inject]
	public AssetMgr aseetMng { set; get;}

	[Inject]
	public GoPlayFlushData goPlayFlushData { get; set;}

	private string nameScreen;
	private AsyncOperation async;
	static int count = 0;
	NGUIAnimation animation;

	public void LoadScreen()
	{
		routineRunner.StartCoroutine (Load ());
		count++;

	}

	IEnumerator Load(){
		yield return new WaitForSeconds(Time.deltaTime);
		async = Application.LoadLevelAsync (nameScreen);

		async.allowSceneActivation = false;
		while( !async.isDone ) 
		{

			if( async.progress >= 0.9f )
			{
				// Almost done.
				yield return new WaitForSeconds(1.0f);
				break;
			}
			yield return null;
		}
		async.allowSceneActivation = true;

		//yield return status;
	}


	public void SetScreen(string nameScreen, bool isSave = true)
	{
		if(isSave)
			goPlayFlushData.Save ();
		this.nameScreen = nameScreen;
	}
}
