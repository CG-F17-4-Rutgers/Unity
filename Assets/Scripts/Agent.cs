using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour {

	private NavMeshAgent agent;
	private Vector3 offset_y = new Vector3 (0.0f, 1.0f, 0.0f); // check presence of other agents at an offset height of 1
	public static bool active = false;
	private bool running = false;

	// Use this for initialization
	void Start () {
		agent = GetComponent<NavMeshAgent> ();
	}
	
	// Update is called once per frame
	void Update () {

		// If agent is navigating, check that the destination is still unoccupied.
		if (!agent.isStopped)
		{
			checkDestination ();
		}
	}

	// Selects this agent.
	public void toggleActive()
	{
		active = !active;
		Debug.Log (active);
		// display/hide marker for selected character
	}

	public bool isActive()
	{
		return active;
	}
		
	// Sets a destination for navmeshagent and starts the navigation
	public void moveTo(Vector3 dest)
	{
		agent.destination = dest;
		agent.isStopped = false;
	}

	public void setMaxSpeed(float speed)
	{
		agent.speed = speed;
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
				agent.destination = agent.destination - (0.8f * opposite.normalized);
				agent.Resume ();

			}
		}
	}
}
