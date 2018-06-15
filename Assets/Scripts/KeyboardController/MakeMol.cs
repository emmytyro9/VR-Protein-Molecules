using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
public class MakeMol : MonoBehaviour {
    public static Text input;
    public static Text error;

    public static float molScale;
    public static Material molMat;
    public static float softness;
    private static float scale = 20;


    private static string apiUrl = "http://ipr.pdbj.org/rest/displayEFSiteFile?format=efvet&id=2itz-A";

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

    public static void SpawnMolecule()
    {
        error.text = "";
        GameObject mol = readEfvet();
        if (mol != null)
        {
            mol.transform.tag = "Mol";
            input.text = "";
        }
    }

    static string[] _getEfvet()
    {
        string[] lines = new string[] { };

        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                lines = reader.ReadToEnd().Split('\n');
                reader.Close();
            }
            response.Close();
        }
        catch (WebException e)
        {
            error.text = "WebException Raised : " + e.Status.ToString();
        }

        return lines;

    }

    public static GameObject readEfvet()
    {
        GameObject mol = new GameObject("2itz-A");
        mol.transform.localScale = new Vector3(scale, scale, scale);

        mol.AddComponent<MeshFilter>();
        mol.AddComponent<SkinnedMeshRenderer>();
        mol.AddComponent<Cloth>();

        Regex sepReg = new Regex(@"\s+");
        //		Regex numReg = new Regex(@"[^0-9]");

        string[] lines = _getEfvet();
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
            error.text = "Very large molecule(" + verticesCount.ToString() + " vertices)";
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

            if (!insiders.Contains(a - 1) && !insiders.Contains(a - 1) && !insiders.Contains(c - 1))
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

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        mol.GetComponent<MeshFilter>().sharedMesh = mesh;
        mol.GetComponent<SkinnedMeshRenderer>().sharedMesh = mesh;
        mol.GetComponent<SkinnedMeshRenderer>().sharedMaterial = molMat;

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
