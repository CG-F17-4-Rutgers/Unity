using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

// Handles NavMeshAgent logic
public class Locomotion_NavMeshAgent : MonoBehaviour {

	public Animator anim;
	public NavMeshAgent agent;
	Vector2 smoothDeltaPosition = Vector2.zero;
	Vector2 velocity = Vector2.zero;
	bool jumping = false;

	private readonly int m_HashHorizontalPara = Animator.StringToHash ("Horizontal");
	private readonly int m_HashVerticalPara = Animator.StringToHash ("Vertical");
	private readonly int m_HashMovingPara = Animator.StringToHash ("Moving");
	private readonly int m_HashRunningPara = Animator.StringToHash ("Running");
	private readonly int m_HashSpeedPara= Animator.StringToHash ("Speed");
	private readonly int m_HashJumpPara= Animator.StringToHash ("Jump");

	private Vector3 offset_y = new Vector3 (0.0f, 1.0f, 0.0f); // check presence of other agents at an offset height of 1
	public Slider slide;

	// Use this for initialization
	void Start () {
		// agent = GetComponent<NavMeshAgent> ();
		agent.speed = 2;

		agent.updatePosition = false;
	}
	
	// Update is called once per frame
	void Update () {

		// If agent is navigating, check that the destination is still unoccupied.
		if (!agent.isStopped)
		{
			checkDestination ();
		}

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
		anim.SetBool(m_HashMovingPara, shouldMove);
		anim.SetFloat (m_HashHorizontalPara, velocity.x);
		anim.SetFloat (m_HashSpeedPara, velocity.magnitude);
		anim.SetFloat (m_HashVerticalPara, velocity.y);

		if (anim.GetFloat (m_HashSpeedPara) > 3.5f) {
			anim.SetBool (m_HashRunningPara, true);
		} else {
			anim.SetBool (m_HashRunningPara, false);
		}
	}
		
	// Sets a destination for navmeshagent and starts the navigation
	public void MoveTo(Vector3 dest)
	{
		agent.destination = dest;
		agent.isStopped = false;
	}

	public void setMaxSpeed()
	{
		agent.speed = slide.value;
	}

	// Check the destination of agent and redirect destination if it is occupied by a stationary agent
	private void checkDestination()
	{
		Collider[] hitColliders = Physics.OverlapSphere (agent.destination + offset_y, 0.1f);

		// Check overlapping colliders
		for (int i = 0; i < hitColliders.Length; i++)
		{
			NavMeshAgent other = hitColliders [i].gameObject.GetComponent<NavMeshAgent> ();

			// If there is a near-stationary agent occupying the destination
			if (other != null && other != agent && other.velocity.magnitude < 0.1f) {
				Debug.Log ("Rerouting " + agent.gameObject.name);

				// Redirect to a destination just short of the old one.
				Vector3 opposite = agent.destination - agent.gameObject.transform.position;
				agent.destination = agent.destination - (0.25f * opposite.normalized);
				// agent.isStopped = false;

			}
		}
	}

	private IEnumerator WaitForAnimation ()
	{
		yield return new WaitForSeconds (0.5f);

	}

	void OnAnimatorMove ()
	{
		// Update position to agent position
		transform.position = agent.nextPosition;
	}
}
