using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {
    GameObject ball;

	// Use this for initialization
	void Start () {
        ball = GameObject.Find("volleyball");
	}
	
	// Update is called once per frame
	void Update () {
	}
}
