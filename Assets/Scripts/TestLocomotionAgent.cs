﻿// LocomotionSimpleAgent.cs
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof (NavMeshAgent))]
[RequireComponent (typeof (Animator))]

// Handles animation logic for an animated character. Requires a separate script for NavMeshAgent logic.
public class TestLocomotionAgent : MonoBehaviour {
	Animator anim;
	NavMeshAgent agent;
	Vector2 smoothDeltaPosition = Vector2.zero;
	Vector2 velocity = Vector2.zero;

	void Start ()
	{
		anim = GetComponent<Animator> ();
		agent = GetComponent<NavMeshAgent> ();
		// Don’t update position automatically
		agent.updatePosition = false;
	}

	void Update ()
	{
		Vector3 worldDeltaPosition = agent.nextPosition - transform.position;

		// Map 'worldDeltaPosition' to local space
		float dx = Vector3.Dot (transform.right, worldDeltaPosition);
		float dy = Vector3.Dot (transform.forward, worldDeltaPosition);
		Vector2 deltaPosition = new Vector2 (dx, dy);

		// Low-pass filter the deltaMove
		float smooth = Mathf.Min(1.0f, Time.deltaTime/0.15f);
		smoothDeltaPosition = Vector2.Lerp (smoothDeltaPosition, deltaPosition, smooth);

		// Update velocity if time advances
		if (Time.deltaTime > 1e-5f)
			velocity = smoothDeltaPosition / Time.deltaTime;

		bool shouldMove = velocity.magnitude > 0.5f && agent.remainingDistance > agent.radius;

		// Update animation parameters
		anim.SetBool("move", shouldMove);
		Debug.Log (velocity.magnitude);
		anim.SetFloat ("Speed", velocity.magnitude);
		anim.SetFloat ("Horizontal", velocity.x);
		anim.SetFloat ("Vertical", velocity.y);

		// GetComponent<LookAt>().lookAtTargetPosition = agent.steeringTarget + transform.forward;
	}

	void OnAnimatorMove ()
	{
		// Update position to agent position
		transform.position = agent.nextPosition;
	}
}