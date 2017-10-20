// LocomotionSimpleAgent.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent (typeof (NavMeshAgent))]
[RequireComponent (typeof (Animator))]

// This class enables animation-physics managed navigation. (B2 Extra Credit 2)
// The NavMeshAgent is disabled on the prefab, the animations' physics do all the work of moving
// the agent, while following a list of points designated by the NavMeshPath
// Attach as a component of an animated character with a NavMeshAgent.
public class AnimationPhysicsLocomotionAgent : MonoBehaviour {
	Animator anim;
	private NavMeshAgent agent;
//	Vector2 smoothDeltaPosition = Vector2.zero;
//	Vector2 velocity = Vector2.zero;

	// Threshold for character divergence from linear path.
	// If distance of character from the line connecting the previous path point and next path point
	// exceeds this threshold, a new path will be calculated, as encouraged by needNewPath().
	float RECALCULATE_THRESHOLD = 5.0f;

	// Adjustable speed of character
	float userInputSpeed = 1.0f; // TODO: For catmull-rom we will know this based on time curve

	List<Vector3> path;
	// keep track of how many points in the path have been passed. store index of next path point
	// we have to hold onto these points to derive splines.
	// and we have to know from where to delete when we find a new path.
	int indexOfNextPathPoint;

	Vector3 lastPosition; // keep track of last position to check if we have passed a path point

	public static bool active = false;

	void Start ()
	{
		anim = GetComponent<Animator> ();
		agent = GetComponent<NavMeshAgent> ();
		// Don’t update position automatically
		agent.updatePosition = false;
		lastPosition = transform.position;
	}

	void Update ()
	{
//		Vector3 worldDeltaPosition = agent.nextPosition - transform.position;
//
//		// Map 'worldDeltaPosition' to local space
//		float dx = Vector3.Dot (transform.right, worldDeltaPosition);
//		float dy = Vector3.Dot (transform.forward, worldDeltaPosition);
//		Vector2 deltaPosition = new Vector2 (dx, dy);
//
//		// Low-pass filter the deltaMove
//		float smooth = Mathf.Min(1.0f, Time.deltaTime/0.15f);
//		smoothDeltaPosition = Vector2.Lerp (smoothDeltaPosition, deltaPosition, smooth);
//
//		// Update velocity if time advances
//		if (Time.deltaTime > 1e-5f)
//			velocity = smoothDeltaPosition / Time.deltaTime;
//
//		bool shouldMove = velocity.magnitude > 0.5f && agent.remainingDistance > agent.radius;
//
//		// Update animation parameters
//		anim.SetBool("move", shouldMove);
//		anim.SetFloat ("speed", velocity.magnitude);
//		anim.SetFloat ("Horizontal", velocity.x);
//		anim.SetFloat ("Vertical", velocity.y);

		// GetComponent<LookAt>().lookAtTargetPosition = agent.steeringTarget + transform.forward;

		agent.enabled = true;

		bool shouldMove = agent.remainingDistance > agent.radius;
		if (shouldMove)
		{
			// Recalculate new path to destination if necessary
			if (needNewPath ())
			{
				NavMeshPath navMeshPath = new NavMeshPath();
				agent.CalculatePath(agent.destination, navMeshPath);
				List<Vector3> path = new List<Vector3>(navMeshPath.corners);
				indexOfNextPathPoint = 1;
			}

			lastPosition = transform.position;
			Vector3 nextDirection = Vector3.Normalize(path[indexOfNextPathPoint] - transform.position);

			// anim.SetBool ("Move", shouldMove);
			anim.SetFloat ("Speed", userInputSpeed);
			anim.SetFloat ("Horizontal", nextDirection.x);
			anim.SetFloat ("Vertical", nextDirection.z);
		}

		agent.enabled = false;


	}

	public void moveTo(Vector3 destination)
	{
		agent.enabled = true;
		agent.destination = destination;
		agent.enabled = false;
	}

	// Selects this agent.
	public void toggleActive()
	{
		active = !active;
		Debug.Log (active);
		// display/hide marker for selected character
	}

	public void setMaxSpeed(float speed)
	{
		userInputSpeed = speed;
	}


	void OnAnimatorMove ()
	{
		// Update position to agent position
		// transform.position = agent.nextPosition;

		// Update position based on animation movement using navigation surface height
//		Vector3 position = anim.rootPosition;
//		position.y = agent.nextPosition.y; // may not look good for jumps
//		transform.position = position;

		// Test if we've passed the next corner point in the last movement
		if (Vector3.Magnitude(transform.position - lastPosition) > Vector3.Magnitude(path[indexOfNextPathPoint] - lastPosition))
		{
			indexOfNextPathPoint++;
		}
	}

	bool needNewPath()
	{
		if (path == null)
			return true; // trivially need a new path
		else if (indexOfNextPathPoint >= path.Count)
			return true; // presumably, we overshot the last control point (destination).
		else
		{
			/*            C         
			 * p_i ---------------> p_i+1
			 * 
			 * It's possible that the character has gone too far from the straight line connecting
			 * the previous path point and the next path point. In this case, we'd like to calculate
			 * a new path. */
			Vector3 straightPath = path [indexOfNextPathPoint] - path [indexOfNextPathPoint - 1];
			Vector3 dirCharFromLastPoint = transform.position - path [indexOfNextPathPoint - 1];
			float angleBetweenCharAndLastPoint = Vector3.Angle (straightPath, dirCharFromLastPoint);
			float distanceFromStraightLine = Mathf.Sin (angleBetweenCharAndLastPoint) * dirCharFromLastPoint.magnitude;
			return distanceFromStraightLine >= RECALCULATE_THRESHOLD;
		}
	}
}