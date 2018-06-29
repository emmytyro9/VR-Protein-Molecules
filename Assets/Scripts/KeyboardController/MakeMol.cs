/***
 * This class is made to test class "Makemolecule".
 * Try to modify this class, if it works then you can replace the code in class "Makemolecule".
 * 
 ***/

using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using CurvedVRKeyboard;
using System.Collections;

public class MakeMol : MonoBehaviour
{
    public Text input;
    public float molScale;
    public Material molMat;
    public float softness;
    public GameObject atomModel;
    private GameObject Molecule;
    private List<Vector3> vertice = new List<Vector3>();
    /***
     * if the HttpWebRequest is used, this attribute should be uncomment.
     * 
     * private string apiUrl = "https://pdbj.org/rest/displayEFSiteFile?format=efvet&id="; 
    ***/

    /// <summary>
    /// this attribute is the list of colors which are generated for the molecules.
    /// </summary>
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

    /***
     * if the HttpWebRequest is used, this method should be uncomment.
     * 
     * public bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }
    ***/

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

            /***
             * To set position of the camera to the certain coordinate (Nearby the Molecule)
             *  ***/
            GameObject.FindGameObjectWithTag("Mol").transform.position = GameObject.Find("OVRPlayerController").transform.position - new Vector3(0f, 0f, -3f);
           
        }
    }

    /// <summary>
    /// Get the data from local file/ HttpWebserver.
    /// </summary>
    /// <param name="name">Name of the molecule which can be receive from VR keyboard</param>
    /// <returns>Array of data that read from file</returns>
    string[] _getEfvet(string name)
    {
        string[] lines = new string[] { };

        try
        {
            /***
             * This part is used to create the request and wait for response to get data from website.
             * 
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl + name);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();



             if (response.StatusCode == HttpStatusCode.OK)
              {
                  Stream dataStream = response.GetResponseStream();

             ***/

            /***
             * The attribute 'path' is used to read data from local file in folder Resources.
             * 'input' value is received from the VR Keyboard.
             ***/
            string path = "Assets/Resources/" + input.text + ".txt";
            Debug.Log(string.Format("DEBUG Path: " + path));
            StreamReader reader = new StreamReader(path);
            lines = reader.ReadToEnd().Split('\n');
            reader.Close();
            //If the HttpWebRequest and HttpWebResponse are used, the respond should close after being used.
            // }
            ///response.Close();
        }
        catch (WebException e)
        {
            Debug.Log(string.Format("Exception Raised : " + e.Status.ToString()));
            //error.text = "Exception Raised : " + e.Status.ToString();
        }

        return lines;

    }

    GameObject readEfvet(string name, float scale)
    {
        if (input.text == "a")
        {
            Molecule = new GameObject();
            Molecule.name = "Spacefill Molecule";
            //Molecule.transform.position = new Vector3(-93f, -4.5f, 7.92f);
            Molecule.transform.tag = "Spacefill";
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
                //error.text = "Very large molecule(" + verticesCount.ToString() + " vertices)";
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
                float temperatureFactor = float.Parse(stArrayData[8]);
                int isInside = int.Parse(stArrayData[11]);

                vertices.Add(new Vector3(-1 * x, y, z));
                GameObject ins = Instantiate(atomModel, new Vector3(-1 * x, y, z), Quaternion.identity);
                ins.transform.SetParent(Molecule.transform);
            }

            //Debug.Log(string.Format("Vertice Index: " + vertices.Count());


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
                //Debug.Log(string.Format("c: " + c + "| b: " + b + "| a: " + a));
                /***
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                cube.transform.position = new Vector3(c - 1, b - 1, a - 1);
                ***/
            }

            //Debug.Log(string.Format("triangles Index: " + triangles.Count());


            //Start The Coroutine
            //StartCoroutine("InstantiateDelay", 1000);
            Molecule.transform.localScale = new Vector3(.01f, .01f, .01f);

            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            Debug.Log(string.Format("Triangle: " + mesh.triangles.Count()));
            //mesh.colors = colors.ToArray();

            //Debug.Log(string.Format("DEBUG: MESH " + mesh.isReadable));

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            Molecule.GetComponent<MeshFilter>().sharedMesh = mesh;
            //Molecule.GetComponent<SkinnedMeshRenderer>().sharedMesh = mesh;

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
            

            return Molecule;
        }
        
        //***********************************************************************************
        else  
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
                //error.text = "Very large molecule(" + verticesCount.ToString() + " vertices)";
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
                    //Debug.Log(string.Format("Color Index: " + colors.Count());
                }

                if (isInside != 1)
                {
                    insiders.Add(i);
                }
            }

            //Debug.Log(string.Format("Vertice Index: " + vertices.Count());


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
                //Debug.Log(string.Format("c: " + c + "| b: " + b + "| a: " + a));
            }

            //Debug.Log(string.Format("triangles Index: " + triangles.Count());

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

            return mol;
        }
    }
    /*
    IEnumerator InstantiateDelay(int perframeCreateNum)
    {
        int count = 0;

        for (int i = 0; i < vertice.Count(); i++)
        {
            GameObject ins = Instantiate(atomModel, vertice[i], Quaternion.identity);
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