using UnityEngine;


public class AwakeToSpawnMolecule : MonoBehaviour {

    private void Awake()
    {
        MainMol.SpawnMolecule("surface");
    }
}
