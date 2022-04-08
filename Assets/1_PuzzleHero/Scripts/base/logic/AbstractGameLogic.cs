using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Nfury.Base
{
	public abstract class AbstractGameLogic
	{
		public enum GAME_STATE
		{
			INIT,
			RUN,
			PAUSE,
			END
		}
		protected float gameSpeed = 1;
		protected GAME_STATE gState;
		private float dt;
		public GAME_STATE GState {
			get {
				return gState;
			}
			protected set {
				gState = value;
			}
			
		}
		
		public float GameSpeed {
			get {
				return gameSpeed;
			}
			set {
				gameSpeed = value;
			}
		}

		protected List<Entity> objList = new List<Entity> ();

		public AbstractGameLogic ()
		{
		}

		public void Init ()
		{
			GState = GAME_STATE.INIT;
			OnInit ();
		}

		public void Start ()
		{
			GState = GAME_STATE.RUN;
			OnStart ();
		}
		
		public void Pause ()
		{
			GState = GAME_STATE.PAUSE;
			OnPause ();
		}
		
		public void End ()
		{
			GState = GAME_STATE.END;
			OnEnd ();
		}

		public void FixedUpdate (float dt)
		{
			if (GState == GAME_STATE.RUN) {
				int len = objList.Count;
				Entity e;
				for (int i = 0; i< len; ++ i) {
					e = objList [i];
					switch (e.State) {
					case Entity.STATE.USED:
						e.FixedUpdate (dt);
						break;
					case Entity.STATE.DESTROY:
						e.BeRemoved ();
						objList.RemoveAt (i);
						OnRemoveObject (e);
						--len;
						--i;
						break;
					}
				}
				OnFixedUpdate (dt);
			}
		}

		public void Update (float dt)
		{
			if (GState == GAME_STATE.RUN) {
				int len = objList.Count;
				Entity e;
				for (int i = 0; i< len; ++ i) {
					e = objList [i];
					if (e.State == Entity.STATE.USED) {
						e.Update (dt);
					}
				}
				OnUpdate (dt);
				this.dt = dt;
			}
		}

		public float getDelta ()
		{
			return this.dt;
		}
		public void AddObject (Entity obj)
		{
			objList.Add (obj);
			OnAddObject (obj);
			obj.BeAdded (this);
		}
		
		public void RemoveObject (Entity obj)
		{
			obj.State = Entity.STATE.DESTROY;
		}
		
		protected virtual void OnInit ()
		{
		}
		protected virtual void OnStart ()
		{
		}
		protected virtual void OnPause ()
		{
		}
		protected virtual void OnEnd ()
		{
		}
		protected abstract void OnFixedUpdate (float dt);
		protected abstract void OnUpdate (float dt);
		public abstract void OnAddObject (Entity obj);
		public abstract void OnRemoveObject (Entity obj) ;
	}
}



