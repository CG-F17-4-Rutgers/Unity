using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointClickController : MonoBehaviour {

	public float raycastDistance;

	private Ray shootRay;        // Ray cast from mouse position (at camera)
	private RaycastHit shootHit; // GameObject hit by shootRay
	private GameObject activeGameObject;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		// ignore clicks on Event System
		if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject ())
			return;

		// Handle I/O Pointer Events
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//		Ray ray = new Ray (Camera.main.transform.position, Camera.main.transform.forward);
		RaycastHit hit;

		// Select
		if (Input.GetButtonDown ("Fire2"))
		{
			if (Physics.Raycast (ray, out hit, raycastDistance))
			{
				if (hit.collider.CompareTag ("Character")) {
					GameObject selectedGameObject = hit.transform.gameObject;
					//selectedGameObject.SendMessage ("toggleActive");
					activeGameObject = (activeGameObject == null ? selectedGameObject : null);
					print ("SELECTED CHARACTER");
				} else {
					print ("Missed");
				}
			}
		}

		// Act
		if (Input.GetButtonDown ("Fire1"))
		{
			if (Physics.Raycast (ray, out hit, raycastDistance))
			{
				if (activeGameObject != null)
				{

					activeGameObject.SendMessage ("moveTo", hit.point);
				}
			}
		}
	}
}
