// LocomotionSimpleAgent.cs
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections.Generic;
[RequireComponent (typeof (NavMeshAgent))]
[RequireComponent (typeof (Animator))]


/* This class enables animation-physics managed navigation. (B2 Extra Credit 2)
 * The NavMeshAgent is disabled on the prefab, the animations' physics do all the work of moving
 * the agent, while following a list of points designated by the NavMeshPath
 * Attach as a component of an animated character with a NavMeshAgent. */

public class AnimationPhysicsLocomotionAgent : MonoBehaviour {
	public static bool active = false;  // is character selected?

	Animator anim;
	private NavMeshAgent agent;
	bool moving;
	List<Vector3> path;                 // store points from navmeshpath
	int indexOfNextPathPoint;
	Vector3 lastPosition;               // character's last position
	float RECALCULATE_THRESHOLD = 5.0f; // threshold for character divergence from linear path
	private readonly float ANGLE_PI_DIV_4 = 45.0f * Mathf.Deg2Rad; // 45deg
	private readonly float ANGLE_TWO_PI = 2.0f * Mathf.PI;

	public Slider slide;                // adjustable speed of character
	float userInputSpeed = 1.0f;

	// Animator Controller parameters
	private readonly int m_HashHorizontalPara = Animator.StringToHash ("Horizontal");
	private readonly int m_HashVerticalPara = Animator.StringToHash ("Vertical");
	private readonly int m_HashMovingPara = Animator.StringToHash ("Moving");
	private readonly int m_HashRunningPara = Animator.StringToHash ("Running");
	private readonly int m_HashSpeedPara = Animator.StringToHash ("Speed");
	private readonly int m_HashTurningPara = Animator.StringToHash ("Turning");


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

		if (moving)
		{
			agent.enabled = true;
			float remainingDistance = Vector3.Magnitude(agent.destination - transform.position);
			bool atDestination = remainingDistance < agent.radius;
			if (agent.pathPending)
			{
				Debug.Log ("Path Pending.");
			}
			else if (atDestination)
			{
				Debug.Log ("Stopping");
				anim.SetBool (m_HashMovingPara, false);
				anim.SetBool (m_HashTurningPara, false);
				moving = false;
			}
			else
			{
				if (needNewPath ())
				{
					calculateNewPath (agent.destination);
				}
				else
				{
					lastPosition = transform.position;
					Vector3 nextDirection = Vector3.Normalize(path[indexOfNextPathPoint] - transform.position);
					Quaternion rotationToNextPoint = Quaternion.FromToRotation (transform.forward, nextDirection);
					Vector3 nextDirectionLocal = rotationToNextPoint * Vector3.forward;
					float angleToNextPoint = rotationToNextPoint.eulerAngles.y; // [0.0f, 360.0f]
					if (angleToNextPoint > 180.0f)
						angleToNextPoint = -(360 - angleToNextPoint);
					Debug.Log (angleToNextPoint);
					bool turning = Mathf.Abs (angleToNextPoint) >= 45.0f;
					// need to rotate nextDirection by negative of agent's facing direction.

					// Update animation parameters
					anim.SetBool  (m_HashMovingPara, !turning);
					anim.SetBool  (m_HashTurningPara, turning);
					anim.SetFloat (m_HashHorizontalPara, nextDirectionLocal.x);
					anim.SetFloat (m_HashVerticalPara, nextDirectionLocal.z);
					anim.SetFloat (m_HashSpeedPara, userInputSpeed);

					if (anim.GetFloat (m_HashSpeedPara) > 3.5f)
						anim.SetBool (m_HashRunningPara, true);
					else
						anim.SetBool (m_HashRunningPara, false);
					
				}
			}

			// agent.enabled = false;
		}

	}

	public void moveTo(Vector3 destination)
	{
		Debug.Log ("MoveTo message received");
		agent.enabled = true;
		agent.transform.position = transform.position;
		calculateNewPath (destination);
		Debug.Log ("agent destination: " + agent.destination);
		agent.speed = slide.value;
		moving = true;
		agent.isStopped = true;
	}

	private void calculateNewPath(Vector3 targetPosition)
	{
		Debug.Log ("Calculating new path");
		agent.destination = targetPosition;
		NavMeshPath navMeshPath = new NavMeshPath();
		agent.CalculatePath(targetPosition, navMeshPath);
		path = new List<Vector3>(navMeshPath.corners);
		Debug.Log ("Path of " + path.Count + " points generated.");
		printPath ();
		indexOfNextPathPoint = 1;
	}

	private void printPath()
	{
		Debug.Log ("Path points: ");
		foreach (Vector3 point in path)
			Debug.Log(point);
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
		// Update position based on animation movement
		Vector3 position = anim.rootPosition;
		Quaternion rotation = anim.rootRotation;
		transform.position = position;
		transform.rotation = rotation;
		// position.y = agent.nextPosition.y; // may not look good for jumps

		if (moving)
		{
			// Pull agent towards character
			Vector3 agentDeltaPosition = agent.nextPosition - transform.position;
			if (agentDeltaPosition.magnitude > agent.radius)
				agent.nextPosition = transform.position + 0.9f*agentDeltaPosition;

			// Test if we're close to the next corner point or have passed it in the last movement

			if (Vector3.Magnitude(transform.position - lastPosition) > Vector3.Magnitude(path[indexOfNextPathPoint] - lastPosition))
				indexOfNextPathPoint++;
		}

	}

	bool needNewPath()
	{
		if (agent.pathPending)
			return false;
		
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
			 * a new path from the character's current position. */
			Vector3 straightPath = path [indexOfNextPathPoint] - path [indexOfNextPathPoint - 1];
			Vector3 dirCharFromLastPoint = transform.position - path [indexOfNextPathPoint - 1];
			float angleBetweenCharAndLastPoint = Vector3.Angle (straightPath, dirCharFromLastPoint);
			float distanceFromStraightLine = Mathf.Sin (angleBetweenCharAndLastPoint) * dirCharFromLastPoint.magnitude;
			return distanceFromStraightLine >= RECALCULATE_THRESHOLD;
		}
	}
}