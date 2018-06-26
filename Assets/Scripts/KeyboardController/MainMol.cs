using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using CurvedVRKeyboard;
using System.Collections;

public class MainMol : MonoBehaviour
{
    public Text input;
    public float molScale;
    public GameObject obj;
    private List<Vector3> vertices = new List<Vector3>();
    private GameObject Molecule;

    private List<Color> vertexColor = new List<Color>(){
        new Color(1.00f, 1.00f, 1.00f, 1.00f),
        new Color(1.00f, 0.00f, 0.00f, 1.00f),
        new Color(1.00f, 0.25f, 0.25f, 1.00f),
        new Color(1.00f, 0.50f, 0.50f, 1.00f),
        new Color(1.00f, 0.75f, 0.75f, 1.00f),
        new Color(1.00f, 1.00f, 1.00f, 1.00f),
        new Color(0.80f, 0.80f, 1.00f, 1.00f),
        new Color(0.60f, 0.60f, 1.00f, 1.00f),
        new Color(0.40f, 0.40f, 1.00f, 1.00f),
        new Color(0.00f, 0.00f, 1.00f, 1.00f),
        new Color(1.00f, 1.00f, 1.00f, 1.00f),
        new Color(1.00f, 0.500f, 0.000f, 1.00f),
        new Color(1.00f, 0.625f, 0.125f, 1.00f),
        new Color(1.00f, 0.750f, 0.250f, 1.00f),
        new Color(1.00f, 0.875f, 0.375f, 1.00f),
        new Color(1.00f, 1.000f, 0.375f, 1.00f),
        new Color(0.80f, 1.00f, 0.60f, 1.00f),
        new Color(0.60f, 1.00f, 0.50f, 1.00f),
        new Color(0.40f, 1.00f, 0.40f, 1.00f),
        new Color(0.00f, 1.00f, 0.00f, 1.00f),
        new Color(1.00f, 1.00f, 1.00f, 1.00f),
        new Color(1.00f, 0.00f, 0.00f, 1.00f),
        new Color(0.00f, 0.00f, 1.00f, 1.00f),
        new Color(0.00f, 1.00f, 0.00f, 1.00f),
        new Color(1.00f, 1.00f, 0.00f, 1.00f),
        new Color(1.00f, 0.00f, 1.00f, 1.00f)
    };

    private List<string> flaggedAminos = new List<string>() { "LEU", "ILE", "VAL", "MET", "PRO", "PHE", "TRP", "TYR", "ALA" };
    private List<string> flaggedAtoms = new List<string>() { "N", "HN", "CA", "C", "O", "OT", "OH", "HH" };

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            SpawnMolecule();
            Destroy(this);
        }
    }

    public void SpawnMolecule()
    {
        GameObject mol = readEfvet(input.text, molScale);
        if (mol != null)
        {
            mol.transform.tag = "Mol";
            input.text = "";
            GameObject.FindGameObjectWithTag ("Mol").transform.position = GameObject.Find("OVRPlayerController").transform.position - new Vector3(0f, 0f, -3f);

        }
    }
    
    string[] _getEfvet(string name)
    {
        string[] lines = new string[] { };

        try
        {
            string path = "Assets/Resources/" + input.text + ".txt";
            Debug.Log(string.Format("DEBUG Path: " + path));
            StreamReader reader = new StreamReader(path);
            lines = reader.ReadToEnd().Split('\n');
            reader.Close();
        }
        catch (WebException e)
        {
            Debug.Log(string.Format("Exception Raised : " + e.Status.ToString()));
        }

        return lines;

    }

    GameObject readEfvet(string name, float scale)
    {
        GameObject mol = new GameObject(name);
        mol.transform.localScale = new Vector3(scale, scale, scale);

        Regex sepReg = new Regex(@"\s+");

        string[] lines = _getEfvet(name);
        if (!lines.Any())
        {
            Destroy(mol);
            mol = null;
            return mol;
        }

        string line = lines[0].TrimStart(' ');
        string[] stArrayData = sepReg.Split(line);

        int verticesCount = int.Parse(stArrayData[0]);

        if (verticesCount > 65000)
        {
            Debug.Log(string.Format("This molecule is too large(more than 65,000 vertices), we are going to destroy it!"));
            Destroy(mol);
            mol = null;
            return mol;
        }

        Molecule = new GameObject();
        Molecule.name = "Spacefill Mollecule";
        Molecule.transform.position = new Vector3(-93f, -4.5f, 7.92f);
        Molecule.transform.tag = "Spacefill";

        for (int i = 0; i < verticesCount; i++)
        {
            line = lines[i + 1].TrimStart(' ');
            stArrayData = sepReg.Split(line);

              float x = float.Parse(stArrayData[18]);
              float y = float.Parse(stArrayData[19]);
              float z = float.Parse(stArrayData[20]);

            vertices.Add(new Vector3(-1 * x, y, z));
            GameObject ins = Instantiate(obj, vertices[i], Quaternion.identity);
            ins.transform.SetParent(Molecule.transform);
        }

        //Start The Coroutine
        //StartCoroutine("InstantiateDelay", 1000);

        

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();

        Debug.Log(string.Format("Vertices: " + vertices.Count()));

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

        //Molecule.transform.localScale = new Vector3(.01f, .01f, .01f);

        return Molecule;
    }
   /*
    IEnumerator InstantiateDelay(int perframeCreateNum)
    {
        int count = 0;

        for (int i = 0; i < vertices.Count(); i++)
        {
            GameObject ins = Instantiate(obj, vertices[i], Quaternion.identity);
            ins.transform.SetParent(Molecule.transform);
            //Debug.Log(string.Format("Sphere scale: " + ins.transform.localScale.x + ", " + ins.transform.localScale.y + ", " + ins.transform.localScale.z));
            if (count < perframeCreateNum)
            {
                count++;
            }
            else
            {
                count = 0;
                yield return 0;
            }
        }
        yield return 0;
        Molecule.transform.localScale = new Vector3(.01f, .01f, .01f);
    }*/
}