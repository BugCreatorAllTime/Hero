using UnityEngine;
using System.Collections;

public interface SubjectOs {

	void Notify();
	void AddObserver(Observer server);
	void RemoveObserver(Observer server);
}
