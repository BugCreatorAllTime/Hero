
using System;
using System.Collections;
using UnityEngine;
using Nfury.Base;
using strange.examples.strangerocks;

public class GameLoop : ILoop
{
	public const float FRAME_TIME = 0.03f;
	private float lastTime;
	private float lag;

	[Inject]
	public IRoutineRunner routinerunner { get; set; }

	[Inject(DungeonContext.BATTLE_LOGIC)]
	public AbstractGameLogic battleLogic { get; set; }

	[Inject(DungeonContext.MATCH_LOGIC)]
	public AbstractGameLogic matchLogic { get; set; }

	[Inject]
	public ConfigManager configMgr {get;set;}

	[PostConstruct]
	public void PostConstruct ()
	{
		routinerunner.StartCoroutine (run ());
	}
	public GameLoop ()
	{
		lastTime = Time.time;
	}

	protected IEnumerator run ()
	{
		while (true) {
			float curTime = Time.time;
			float elapsed = curTime - lastTime;
			lag += elapsed;
			lastTime = curTime;
			while (lag > FRAME_TIME) {
				lag -= FRAME_TIME;
				FixedUpdate (FRAME_TIME);
			}
			Update (elapsed);
			yield return new WaitForEndOfFrame ();
		}
	}

	public void Update(float dt) {
		battleLogic.Update(dt);
		matchLogic.Update(dt);
	}

	public void FixedUpdate(float dt) {
		battleLogic.FixedUpdate(dt);
		matchLogic.FixedUpdate(dt);
	}


}


