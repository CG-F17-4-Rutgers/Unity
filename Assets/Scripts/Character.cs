using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

// Handles NavMeshAgent logic
public class Character : MonoBehaviour {

	public static bool selected = false;
	public GameObject SelectionIndicator;
	public GameObject Avatar;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {

		if (selected)
		{
			// Debug.Log ("HELLO");
			SelectionIndicator.transform.Rotate (new Vector3 (15, 30, 45) * Time.deltaTime);
			SelectionIndicator.transform.position = Avatar.transform.position + new Vector3 (0.0f, 2.1f, 0.0f);
		}

	}

	// Selects this agent.
	public void toggleSelection()
	{
		selected = !selected;
		SelectionIndicator.SetActive (selected); // display/hide selected marker
	}
		
	// Sets a destination for navmeshagent and starts the navigation
	public void moveTo(Vector3 dest)
	{
		Avatar.SendMessage ("MoveTo", dest);
	}

}
