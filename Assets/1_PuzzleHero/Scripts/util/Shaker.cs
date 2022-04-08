using System.Collections;
using System.Collections.Generic;
using strange.examples.strangerocks;
using UnityEngine;

public class Shaker
{
	private static List<Transform> camTransforms = new List<Transform>();
	// How long the object should shake for.
	public static float shake = 0f;
	// Amplitude of the shake. A larger value shakes the camera harder.
	public static float shakeAmount = 10f;
	public static float decreaseFactor = 1.0f;
	private static List<Vector3> originalPositions = new List<Vector3>();
	private static bool shaking = false;

	private static IEnumerator Update() {
		while (shake > 0)
		{
			shaking = true;
			Vector3 r = Random.insideUnitSphere * shakeAmount;
			for (int i = 0; i < camTransforms.Count; i++)
			{
				Vector3 p = originalPositions[i] + r;
				p.z = originalPositions[i].z;
				Transform camTransform = camTransforms[i];
				camTransform.localPosition = p;
			}
			shake -= Time.deltaTime * decreaseFactor;
			yield return new WaitForEndOfFrame();
		}
		if (shake <= 0)
		{
			shaking = false;
			shake = 0f;
			for (int i = 0; i < camTransforms.Count; i++)
			{
				camTransforms[i].localPosition = originalPositions[i];
			}

			camTransforms = null;
			originalPositions.Clear();
		}
		
	}

	public static void Shake(RoutineRunner routineRunner, List<Transform> cams, float shake = 0.15f)
	{
		if (shaking) return;
		Shaker.shake = shake;
		camTransforms = cams;
		for (int i = 0; i < cams.Count; i++)
		{
			originalPositions.Add(cams[i].localPosition);
		}

		routineRunner.StartCoroutine(Update());
	}

}