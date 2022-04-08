using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LinePoint : MonoBehaviour {

	public Color c1 = Color.yellow;
	public Color c2 = Color.red;
	public GameObject cube;
	List<Vector3> list = new List<Vector3>();
	LineRenderer lineRenderer;
	bool draw = false;
	int i = 0;
	int j = 0;
	bool move = false;

	Vector3 startMarker;
	Vector3 endMarker;
	public float speed = 1.0F;
	private float startTime;
	public float smooth = 5.0F;
	public int numberOfPoints = 50;

	public Vector3[] controlPoints = new Vector3[3];
	int k = 0;

//	void OnGUI() {
//		GUI.matrix = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, 
//		                            new Vector3 (Screen.width / 640, Screen.height / 960, 0f));
//	}

	void Start() {
		lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
		lineRenderer.SetColors(c1, c2);
		lineRenderer.SetWidth(0.05F, 0.05F);
//		Debug.Log (Mathf.Tan((-0.176f-2.68f)
//		                      /(1.378f+1.471f)));
		Debug.Log (Mathf.Atan(1)*180/Mathf.PI);
		Debug.Log (Mathf.Tan(0.5f));
	}
	void Update() {
//		Debug.Log (transform.name);
//		Debug.Log (Input.mousePosition);
		if(Input.GetMouseButtonDown(0)){
//			draw = true;
			Vector3 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			pz.z = 0;
			controlPoints[k++] = pz;
		}
		if(Input.GetMouseButtonUp(0)){
//			draw = false;
//			move = true;
//			for(int i = 0; i < list.Count; i++){
//				Debug.Log(list[i]);
//			}
//			list = new List<Vector3>();
		}
		if(k == 3 && !draw){
			draw =true;
			lineRenderer.SetVertexCount(numberOfPoints * (controlPoints.Length - 2) + 2);
			Vector3 p0;
			Vector3 p1;
			Vector3 p2;
			for (int j = 0; j < controlPoints.Length - 2; j++)
			{
				// determine control points of segment
				p0 = 0.5f * (controlPoints[j] 
				             + controlPoints[j + 1]);
				p1 = controlPoints[j + 1];
				p2 = 0.5f * (controlPoints[j + 1] 
				             + controlPoints[j + 2]);
				
				// set points of quadratic Bezier curve
				Vector3 position;
				float t; 
				float pointStep= 1.0f / numberOfPoints;
				if (j == controlPoints.Length - 3)
				{
					pointStep = 1.0f / (numberOfPoints - 1);
					// last point of last segment should reach p2
				}  
				lineRenderer.SetPosition(0, 
				                         controlPoints[0]);
				list.Add(controlPoints[0]);
				for(int i = 0; i < numberOfPoints; i++) 
				{
					t = i * pointStep;
					position = (1-t)*(1-t)*p0 + 2*(1-t)*t*p1+t*t*p2;
					lineRenderer.SetPosition(i +1+ j * numberOfPoints, 
					                         position);
					list.Add(position);
				}
				lineRenderer.SetPosition(numberOfPoints +1+ j * numberOfPoints, 
				                         controlPoints[2]);
				list.Add(controlPoints[2]);
				move =true;
			}
		}
	}

	void FixedUpdate(){
//		if(draw && CheckPosition()){
//			lineRenderer.SetVertexCount(i+1);
//			Vector3 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//			pz.z = 0;
//			lineRenderer.SetPosition(i, pz);
////			Debug.Log(Input.mousePosition);
//			i++;
////			list.Add(Input.mousePosition);
//			list.Add(pz);
//		}
		if(move){
			Debug.Log(startMarker);
			cube.transform.position = Vector3.Lerp(startMarker, endMarker, 1);
			if(Vector3.Distance(cube.transform.position, endMarker) == 0){
				SetMove();
			}
		}
	}

	bool CheckPosition(){
		if(list.Count == 0){
			return true;
		} else{
			if(Mathf.Abs(list[list.Count-1].x - Input.mousePosition.x) > 5 ||
			   Mathf.Abs(list[list.Count-1].y - Input.mousePosition.y) > 5){
				return true;
			} else{
				return false;
			}
		}
	}

	void SetMove(){
		if(j < list.Count-1){
			startTime = Time.time;
			startMarker = list [j];
			endMarker = list [j + 1];
			j++;
		}
	}
}
