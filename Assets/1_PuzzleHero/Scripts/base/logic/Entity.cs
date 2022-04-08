using System.Collections;
using Nfury.Base;
using System;
using System.Collections.Generic;

public class Entity : ILoop {

	public enum STATE {
		USED,
		NOT_USE,
		DESTROY
	}

	public AbstractGameLogic gLogic;
	private string name;
	private long id;
	private STATE state;

	public STATE State {
		get {
			return state;
		}
		set {
			state = value;
		}
	}

	public long Id {
		get {
			return id;
		}
		set {
			id = value;
		}
	}

	public string Name {
		get {
			return name;
		}
		set {
			name = value;
		}
	}

	
	public void Update(float dt) {
		OnUpdate(dt);

	}

	public void FixedUpdate(float dt) {
		OnFixedUpdate(dt);

	}

	public void BeAdded(AbstractGameLogic gLogic) {
		this.gLogic = gLogic;
		State = STATE.USED;
		OnBeAdded();
	}

	public void BeRemoved() {
		gLogic = null;
		Destroy();
	}



	protected virtual void Destroy(){}
	protected virtual void OnBeAdded(){}
	protected virtual void OnUpdate(float dt){}
	protected virtual void OnFixedUpdate(float dt){}

}
