using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	Animator anim;
	private readonly int movingHash = Animator.StringToHash("Moving");
	private readonly int jumpHash = Animator.StringToHash("Jump");
	private readonly int turningHash = Animator.StringToHash("Turning");


	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		float vertical = Input.GetAxis ("Vertical");
		float horizontal = Input.GetAxis ("Horizontal");

		if (Mathf.Abs(vertical) > 0.1f) {
			anim.SetBool (movingHash, true);
		}

		if (horizontal != 0.0f && Mathf.Abs(vertical) < 0.1f) {
			anim.SetBool (turningHash, true);
		} else {
			anim.SetBool (turningHash, false);
		}

		if (Input.GetKeyDown (KeyCode.Space)) {
			if (!Input.GetKey (KeyCode.S)) {
				anim.SetTrigger (jumpHash);
			}
		}
	}
}
