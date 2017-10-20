using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsoCameraController : MonoBehaviour {

	public Camera cam;
	private Transform g_cam;

	private Vector3 isoForward = new Vector3(1.0f, 0.0f, 1.0f);
	private float ANGLE_PI_DIV_4 = 45 * Mathf.Deg2Rad;

	public int panSpeed;
	public int scrollSpeed;

	// Use this for initialization
	void Start () {
        panSpeed = 6;
        scrollSpeed = 100;

		g_cam = cam.transform;
		g_cam.LookAt(Vector3.zero); // Default look at origin
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		// Panning
		float inputX = Input.GetAxis ("Horizontal") * panSpeed;
		float inputZ = Input.GetAxis ("Vertical") * panSpeed;
		float moveX = Mathf.Cos (-ANGLE_PI_DIV_4) * inputX - Mathf.Sin (-ANGLE_PI_DIV_4) * inputZ;
		float moveZ = Mathf.Sin (-ANGLE_PI_DIV_4) * inputX + Mathf.Cos (-ANGLE_PI_DIV_4) * inputZ;
		g_cam.Translate (new Vector3 (moveX, 0, moveZ) * Time.deltaTime, Space.World);

		// Zooming
		float inputScroll = Input.GetAxis ("Mouse ScrollWheel") * scrollSpeed;
		g_cam.Translate (new Vector3 (0, 0, inputScroll) * Time.deltaTime, Space.Self);

		// WARNING: Floating point errors may result in accumulating errors of camera angle.
		// TODO: Regularly adjust camera transform LookAt to maintain isometric angle.
	}
}
