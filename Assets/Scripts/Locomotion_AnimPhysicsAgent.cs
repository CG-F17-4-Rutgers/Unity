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

public class Locomotion_AnimPhysicsAgent : MonoBehaviour {
	public GameObject CurveVisualizer;
	public GameObject PathPointPlot;
	public GameObject NextCurvePointPlot;
	private List<GameObject> PointPlots;
	public GameObject CurveLinePlot;
	private LineRenderer lineRenderer;

	// threshold for character divergence from linear path
	// if character is farther from linear path than this value, recalculate path
	public float RECALCULATE_THRESHOLD;

	private bool active = false;  // is character selected?

	private Animator anim;
	private NavMeshAgent agent;
	private bool moving;
	private List<Vector3> path;                 // store points from navmeshpath
	private int indexOfNextPathPoint;
	private Vector3 nextCurvePoint;             // the target point on the curve, between the current path point and the next path point
	private Vector3 lastPosition;               // character's last position
	private readonly float ANGLE_PI_DIV_4 = 45.0f * Mathf.Deg2Rad; // 45deg
	private readonly float ANGLE_TWO_PI = 2.0f * Mathf.PI;

	private float time; // time in curve (catmull-rom spline)

	// private GameObject CurvePointVisualizer;

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
		lineRenderer = CurveLinePlot.GetComponent<LineRenderer> ();
		// Don’t update position automatically
		agent.updatePosition = false;
		lastPosition = transform.position;
	}


	void Update ()
	{

		if (moving)
		{
			agent.enabled = true;
			float remainingDistance = Vector3.Magnitude(agent.destination - transform.position);
			bool atDestination = remainingDistance < agent.radius;

			if (agent.pathPending)
			{
				Debug.Log ("Path Pending."); // Still waiting for path
			}
			else if (atDestination)
			{
				Debug.Log ("Stopping"); // You have reached your destination
				anim.SetBool (m_HashMovingPara, false);
				anim.SetBool (m_HashTurningPara, false);
				anim.SetBool (m_HashRunningPara, false);
				anim.SetFloat (m_HashHorizontalPara, 0.0f);
				anim.SetFloat (m_HashVerticalPara, 0.0f);
				anim.SetFloat (m_HashSpeedPara, 0.0f);
				moving = false;
			}
			else
			{
				// We still have a ways to go.

				// Update next curve point if we're already on it or have passed it
				if (Vector3.Magnitude (nextCurvePoint - lastPosition) < agent.radius || Vector3.Magnitude (transform.position - lastPosition) > Vector3.Magnitude (nextCurvePoint - lastPosition))
				{
					nextCurvePoint = GetCurvePoint ();
				}

				if (needNewPath ())
				{
					calculateNewPath (agent.destination);
				}
				else // animate
				{
					lastPosition = transform.position; // update lastPosition

					Vector3 target = nextCurvePoint; // we are aiming our animation for this point

					Vector3 nextDirectionWorld = Vector3.Normalize(target - transform.position);
					Quaternion rotationToNextPoint = Quaternion.FromToRotation (transform.forward, nextDirectionWorld);
					Vector3 nextDirectionLocal = rotationToNextPoint * Vector3.forward;
					float angleToNextPoint = rotationToNextPoint.eulerAngles.y; // [0.0f, 360.0f]
					if (angleToNextPoint > 180.0f)  // Fix angle to (-180.0f, 180.0f]
						angleToNextPoint = -(360 - angleToNextPoint);
					bool turning = Mathf.Abs (angleToNextPoint) >= 45.0f;

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
		}

	}

	// Selects this agent.
	public void toggleActive()
	{
		active = !active;
		Debug.Log (active);
		// display/hide marker for selected character
	}

	public void MoveTo(Vector3 destination)
	{
		agent.enabled = true;
		agent.transform.position = transform.position;
		calculateNewPath (destination);
		Debug.Log ("Agent destination: " + agent.destination);
		agent.speed = slide.value;
		moving = true;
		agent.isStopped = true;
	}

	// Determines if we need to calculate a new path
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
			float distanceFromStraightLine = Mathf.Sin (angleBetweenCharAndLastPoint * Mathf.Deg2Rad) * dirCharFromLastPoint.magnitude;
//			if (distanceFromStraightLine >= RECALCULATE_THRESHOLD)
//			{
//				Debug.Log (angleBetweenCharAndLastPoint);
//			}
//			return false;
			return distanceFromStraightLine >= RECALCULATE_THRESHOLD;
			// return false;
		}
	}

	private void calculateNewPath(Vector3 dest)
	{
		Debug.Log ("Calculating new path");

		// Get Path Points from NavMeshAgent
		agent.destination = dest;
		NavMeshPath navMeshPath = new NavMeshPath();
		agent.CalculatePath(dest, navMeshPath);
		path = new List<Vector3>(navMeshPath.corners); // store path points

		// Get next curve point from Catmull-Rom spline connecting path points
		indexOfNextPathPoint = 0; // will be properly incremented at the beginning of the first GetCurvePoint call
		ResetCurve();
		nextCurvePoint = GetCurvePoint ();

		// Testing
		Debug.Assert (path.Count >= 2);
		Debug.Log ("Path of " + path.Count + " points generated.");
		printPath ();
		deletePathPoints ();
		plotPathPoints ();
	}

	private void plotPathPoints()
	{
		PointPlots = new List<GameObject> (path.Count + 3);

		for (int i = -1; i <= path.Count; i++)
		{
			Vector3 position = GetPathPoint (i);
			GameObject pointPlotObject = Instantiate (PathPointPlot, position, Quaternion.identity);
			PointPlots.Add (pointPlotObject);
			pointPlotObject.transform.parent = CurveVisualizer.transform;
		}
	}

	private void deletePathPoints()
	{
		if (PointPlots != null)
		{
			foreach (GameObject go in PointPlots)
			{
				Destroy (go);
				PointPlots = null;
			}
		}
		lineRenderer.positionCount = 0;
	}

	// Method returns path point from the list path.
	// For splines, auxiliary path points are needed beyond each end of the path (index = -1 and index = path.Count)
	private Vector3 GetPathPoint(int index)
	{
		Debug.Assert (index >= -1 && index <= path.Count);

		if (index == -1) // create auxiliary path point before start
		{
			return path [0] - Vector3.Normalize (path [1] - path [0]); // return point nearby point0 in the opposite direction from point1
		}
		else if (index == path.Count) // create auxiliary path point after end
		{
			return path [path.Count - 1] + Vector3.Normalize (path [path.Count-1] - path [path.Count-2]);
		}
		else // return path point from navmeshpath
		{
			return path [index];
		}
	}

	private int currentControlPoint = 0;
	private float currentT = 0;
	private int velocity = 4;
	private int calls = 0;

	private void ResetCurve()
	{
		calls = 0;
	}

	// Returns the next point on the curve at interval t=time away.
	// We can insert this point into the path and tell the character to navigate toward it.
	// When we reach that point or pass it, we can generate a new one. 
	private Vector3 GetCurvePoint()
	{
		if (calls == 0)
			indexOfNextPathPoint++;

		Vector3 point = Vector3.zero;

		int i = indexOfNextPathPoint - 1, // index of current control point
			iMinusOne = i - 1,
			iPlusOne = i + 1,
			iPlusTwo = i + 2;
		currentT = calls / (float)velocity;
		currentT = Mathf.Clamp01(currentT);
		point = GetCurvePoint(currentT, GetPathPoint(iMinusOne), GetPathPoint(i), GetPathPoint(iPlusOne), GetPathPoint(iPlusTwo));

		if (calls == velocity) {
			calls = 0;
		} else {
			calls++;
		}

		NextCurvePointPlot.transform.position = point;
		lineRenderer.positionCount = lineRenderer.positionCount + 1;
		lineRenderer.SetPosition (lineRenderer.positionCount - 1, point + new Vector3(0.0f, 1.0f, 0.0f));

		return point;
	}

	public Vector3 GetCurvePoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
		Vector3 a = 0.5f * (2f * p1);
		Vector3 b = 0.5f * (p2 - p0);
		Vector3 c = 0.5f * (2f * p0 - 5f * p1 + 4f * p2 - p3);
		Vector3 d = 0.5f * (-p0 + 3f * p1 - 3f * p2 + p3);

		Vector3 pos = a + (b * t) + (c * t * t) + (d * t * t * t);
		// first derivative: b + 2ct + 3dt^2
		return pos;
	}

	// Output path points to console.
	private void printPath()
	{
		Debug.Log ("Path points: ");
		foreach (Vector3 point in path)
			Debug.Log(point);
	}

	// Set max speed
	public void setMaxSpeed(float speed)
	{
		userInputSpeed = speed;
	}
}