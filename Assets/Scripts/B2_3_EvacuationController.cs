using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class B2_3_EvacuationController : MonoBehaviour {
	public GameObject goal;

	// Use this for initialization
	void Start () {
		GameObject[] characters;
		characters = GameObject.FindGameObjectsWithTag("Character");
		foreach (GameObject character in characters)
		{
			character.SendMessage ("MoveTo", goal.transform.position);
		}
	}
	
	// Update is called once per frame
	void Update () {
	}
}
