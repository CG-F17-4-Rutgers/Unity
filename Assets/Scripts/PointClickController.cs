using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointClickController : MonoBehaviour {

	public float raycastDistance;

	private Ray shootRay;        // Ray cast from mouse position (at camera)
	private RaycastHit shootHit; // GameObject hit by shootRay
	private GameObject selectedGameObject;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		// Handle I/O Pointer Events
		Ray ray = new Ray (Camera.main.transform.position, Camera.main.transform.forward);
		RaycastHit hit;
	}
}
