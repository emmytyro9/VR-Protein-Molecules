using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateSurface : MonoBehaviour {

    // Use this for initialization
    private void Awake()
    {
        MainMol.SpawnMolecule("surface");
    }

    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
