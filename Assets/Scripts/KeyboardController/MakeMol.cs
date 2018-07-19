/***
 * This class is made to test class "Makemolecule".
 * Try to modify this class, if it works then you can replace the code in class "Makemolecule".
 * 
 ***/

using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using CurvedVRKeyboard;


public class MakeMol : MonoBehaviour
{
    private static string input = "1atn-A";
    private static float molScale = 0.01f;
    private static Material molMat;
    private static float softness = 1f;
    private static GameObject atomModel;
    private static GameObject Molecule;

    private static List<Color> vertexColor = new List<Color>(){
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

    private static List<string> flaggedAminos = new List<string>() { "LEU", "ILE", "VAL", "MET", "PRO", "PHE", "TRP", "TYR", "ALA" };
    private static List<string> flaggedAtoms = new List<string>() { "N", "HN", "CA", "C", "O", "OT", "OH", "HH" };
    private static string keepInput;

    public static void SpawnMolecule(string type)
    {
        GameObject mol = readEfvet(input, molScale, type);
        if (mol != null)
        {
            mol.transform.tag = "Mol";
            mol.GetComponent<Transform>().transform.position = GameObject.Find("OVRPlayerController").transform.position - new Vector3(0f, 0f, -3f);
            Debug.Log(string.Format("Mol x,y,z: ") + GameObject.FindGameObjectWithTag("Mol").transform.position.x + ", " + GameObject.FindGameObjectWithTag("Mol").transform.position.y + ", " + GameObject.FindGameObjectWithTag("Mol").transform.position.z);
            Debug.Log(string.Format("OVRPlayerController x,y,z: ") + GameObject.Find("OVRPlayerController").transform.position.x + ", " + GameObject.Find("OVRPlayerController").transform.position.y + ", " + GameObject.Find("OVRPlayerController").transform.position.z);

        }
    }

    public static string[] _getEfvet(string name)
    {
        string[] lines = new string[] { };
        try
        {

            string path = "Assets/Resources/" + input + ".txt";
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

    public static GameObject readEfvet(string name, float scale, string type)
    {
        Debug.Log(string.Format("Debug: Type = ") + type);
        Material newMat = Resources.Load("AAA", typeof(Material)) as Material;
        molMat = newMat;
        atomModel = GameObject.Find("AtomModel");

        if (type == "spacefill")
        {
            Molecule = new GameObject();
            Molecule.name = "Spacefill Molecule";
            Molecule.AddComponent<MeshFilter>();

            Regex sepReg = new Regex(@"\s+");
            //		Regex numReg = new Regex(@"[^0-9]");

            string[] lines = _getEfvet(name);

            if (!lines.Any())
            {
                Destroy(Molecule);
                Molecule = null;
                return Molecule;
            }

            string line = lines[0].TrimStart(' ');
            string[] stArrayData = sepReg.Split(line);

            int verticesCount = int.Parse(stArrayData[0]);
            int edgesCount = int.Parse(stArrayData[1]);
            int triangleArrayCount = int.Parse(stArrayData[2]);

            if (verticesCount > 65000)
            {
                Debug.Log(string.Format("This molecule is too large(more than 65,000 vertices), we are going to destroy it!"));
                Destroy(Molecule);
                Molecule = null;
                return Molecule;
            }

            var vertices = new List<Vector3>();

            var insiders = new List<int>();

            for (int i = 0; i < verticesCount; i++)
            {
                line = lines[i + 1].TrimStart(' ');
                stArrayData = sepReg.Split(line);

                float x = float.Parse(stArrayData[18]);
                float y = float.Parse(stArrayData[19]);
                float z = float.Parse(stArrayData[20]);

                vertices.Add(new Vector3(-1 * x, y, z));
                //Generate each sphere for the protein molecule.
                GameObject ins = Instantiate(atomModel, new Vector3(-1 * x, y, z), Quaternion.identity);
                ins.transform.SetParent(Molecule.transform);
            }

            var triangles = new List<int>();
            for (int i = 0; i < triangleArrayCount; i++)
            {
                line = lines[verticesCount + edgesCount + i + 1].TrimStart(' ');
                stArrayData = sepReg.Split(line);
                int a = int.Parse(stArrayData[3]);
                int b = int.Parse(stArrayData[4]);
                int c = int.Parse(stArrayData[5]);

                if (!insiders.Contains(a - 1) && !insiders.Contains(b - 1) && !insiders.Contains(c - 1))
                {
                    triangles.Add(c - 1);
                    triangles.Add(b - 1);
                    triangles.Add(a - 1);
                }
            }

            Molecule.transform.localScale = new Vector3(.01f, .01f, .01f);

            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            //Debug.Log(string.Format("Triangle: " + mesh.triangles.Count()));

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            Molecule.GetComponent<MeshFilter>().sharedMesh = mesh;

            Molecule.AddComponent<MeshCollider>();
            MeshCollider molCollider = Molecule.GetComponent<MeshCollider>();
            molCollider.convex = true;
            molCollider.inflateMesh = true;
            Molecule.GetComponent<MeshCollider>().sharedMesh = mesh;

            Molecule.AddComponent<OVRGrabbable>();
            OVRGrabbable g = Molecule.GetComponent<OVRGrabbable>();
            Collider col = Molecule.GetComponent<MeshCollider>();
            g.GetComponent<OVRGrabbable>().setGrabPoint(col);

            Molecule.AddComponent<Rigidbody>();
            Rigidbody r = Molecule.GetComponent<Rigidbody>();
            r.isKinematic = true;
            r.useGravity = false;

            //Debug.Log(string.Format("Position in readvet(Spacefill): " + Molecule.transform.position.x + "," + Molecule.transform.position.y + ", " + Molecule.transform.position.z));

            return Molecule;
        }

        //***********************************************************************************
        else if (type == "surface")
        {

            GameObject mol = new GameObject(name);
            mol.transform.localScale = new Vector3(scale, scale, scale);

            mol.AddComponent<MeshFilter>();
            mol.AddComponent<SkinnedMeshRenderer>();
            mol.AddComponent<Cloth>();

            Regex sepReg = new Regex(@"\s+");
            //		Regex numReg = new Regex(@"[^0-9]");

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
            int edgesCount = int.Parse(stArrayData[1]);
            int triangleArrayCount = int.Parse(stArrayData[2]);

            if (verticesCount > 65000)
            {
                Debug.Log(string.Format("This molecule is too large(more than 65,000 vertices), we are going to destroy it!"));
                Destroy(mol);
                mol = null;
                return mol;
            }

            var vertices = new List<Vector3>();
            var colors = new List<Color>();
            var temperatures = new List<float>();
            var insiders = new List<int>();

            for (int i = 0; i < verticesCount; i++)
            {
                line = lines[i + 1].TrimStart(' ');
                stArrayData = sepReg.Split(line);

                float x = float.Parse(stArrayData[0]);
                float y = float.Parse(stArrayData[1]);
                float z = float.Parse(stArrayData[2]);
                float temperatureFactor = float.Parse(stArrayData[8]);
                int isInside = int.Parse(stArrayData[11]);
                int colorIndex = int.Parse(stArrayData[13]);
                string atomName = stArrayData[14];
                string residueName = stArrayData[15];

                vertices.Add(new Vector3(-1 * x, y, z));
                temperatures.Add(temperatureFactor);

                if (!flaggedAtoms.Contains(atomName) && flaggedAminos.Contains(residueName))
                {
                    colors.Add(vertexColor[colorIndex + 10]);
                }
                else
                {
                    colors.Add(vertexColor[colorIndex]);
                }

                if (isInside != 1)
                {
                    insiders.Add(i);
                }
            }

            var triangles = new List<int>();
            for (int i = 0; i < triangleArrayCount; i++)
            {
                line = lines[verticesCount + edgesCount + i + 1].TrimStart(' ');
                stArrayData = sepReg.Split(line);
                int a = int.Parse(stArrayData[3]);
                int b = int.Parse(stArrayData[4]);
                int c = int.Parse(stArrayData[5]);

                if (!insiders.Contains(a - 1) && !insiders.Contains(b - 1) && !insiders.Contains(c - 1))
                {
                    triangles.Add(c - 1);
                    triangles.Add(b - 1);
                    triangles.Add(a - 1);
                }
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.colors = colors.ToArray();

            Debug.Log(string.Format("DEBUG: MESH " + mesh.isReadable));

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            mol.GetComponent<MeshFilter>().sharedMesh = mesh;
            mol.GetComponent<SkinnedMeshRenderer>().sharedMesh = mesh;
            mol.GetComponent<SkinnedMeshRenderer>().sharedMaterial = molMat;

            mol.AddComponent<MeshCollider>();
            MeshCollider molCollider = mol.GetComponent<MeshCollider>();
            molCollider.convex = true;
            molCollider.inflateMesh = true;
            mol.GetComponent<MeshCollider>().sharedMesh = mesh;

            mol.AddComponent<OVRGrabbable>();
            OVRGrabbable g = mol.GetComponent<OVRGrabbable>();
            Collider col = mol.GetComponent<MeshCollider>();
            g.GetComponent<OVRGrabbable>().setGrabPoint(col);

            mol.AddComponent<Rigidbody>();
            Rigidbody r = mol.GetComponent<Rigidbody>();
            r.isKinematic = true;
            r.useGravity = false;

            mol.GetComponent<Cloth>().useGravity = false;
            float maxTemparature = temperatures.Max();
            ClothSkinningCoefficient[] constrants;
            constrants = mol.GetComponent<Cloth>().coefficients;
            for (int i = 0; i < constrants.Length; i++)
            {
                constrants[i].maxDistance = softness * temperatures[i] / maxTemparature;
            }
            mol.GetComponent<Cloth>().coefficients = constrants;
            //Debug.Log(string.Format("Position in readvet(Surface): " + mol.transform.position.x + "," + mol.transform.position.y + ", " + mol.transform.position.z));

            return mol;
        }
        return null;
    }
}