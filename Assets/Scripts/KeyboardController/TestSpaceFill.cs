using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSpaceFill : MonoBehaviour {

    public void BuildSphere()
    {
        GameObject Molecule = new GameObject();
        Molecule.name = "Molecule";
        for (int i = 0; i < 5; i++)
        {
            GameObject atom = new GameObject();
            atom.name = "Atom";
            atom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            atom.transform.localScale = new Vector3(1f, 1f, 1f);
            atom.transform.position += new Vector3(i - 1.5f, i - 1.5f, i - 1.5f);
            atom.transform.SetParent(Molecule.transform);
        }

        GameObject.Find("Molecule").transform.position = GameObject.Find("OVRPlayerController").transform.position - new Vector3(0f, 0f, -5f);

        Mesh mesh = new Mesh();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        Molecule.AddComponent<MeshFilter>();


        Molecule.AddComponent<MeshCollider>();
        MeshCollider molCollider = Molecule.GetComponent<MeshCollider>();
        molCollider.convex = true;
        molCollider.inflateMesh = true;
        //Molecule.GetComponent<MeshCollider>().sharedMesh = mesh;
        Molecule.transform.GetComponent<MeshCollider>().sharedMesh = mesh;

        Molecule.AddComponent<OVRGrabbable>();
        OVRGrabbable g = Molecule.GetComponent<OVRGrabbable>();
        Collider col = Molecule.GetComponent<MeshCollider>();
        g.GetComponent<OVRGrabbable>().setGrabPoint(col);

        Molecule.AddComponent<Rigidbody>();
        Rigidbody r = Molecule.GetComponent<Rigidbody>();
        r.isKinematic = true;
        r.useGravity = false;

        Molecule.GetComponent<MeshFilter>().sharedMesh = mesh;


    }


}
