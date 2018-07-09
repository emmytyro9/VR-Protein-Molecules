using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using CurvedVRKeyboard;

public class MainMol : MonoBehaviour
{
    private static string input = KeyboardStatus.sentOutput;
    private static float molScale = 0.01f;
    private static Material molMat;
    private static float softness = 1f;
    private static GameObject Molecule;
    private static List<Vector3> vertice = new List<Vector3>();

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
            type = "";
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

    public static GameObject readEfvet(string name, float scale , string type)
    {
        Debug.Log(string.Format("Debug: Type = ") + type);
        Material newMat = Resources.Load("AAA", typeof(Material)) as Material;
        molMat = newMat;
        GameObject atomModel_14 = GameObject.Find("AtomModel_14");
        GameObject atomModel_15 = GameObject.Find("AtomModel_15");
        GameObject atomModel_185 = GameObject.Find("AtomModel_185");
        GameObject atomModel_2 = GameObject.Find("AtomModel_2");

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
            var insCollection = new List<GameObject>();

            for (int i = 0; i < verticesCount; i++)
            {
                line = lines[i + 1].TrimStart(' ');
                stArrayData = sepReg.Split(line);

                float x = float.Parse(stArrayData[18]);
                float y = float.Parse(stArrayData[19]);
                float z = float.Parse(stArrayData[20]);
                string atomName = stArrayData[14];
                string residueName = stArrayData[15];

                vertices.Add(new Vector3(-1 * x, y, z));
                //Assign radius to each atom.
                float atomSize = CheckSizeOfElenemts(atomName, residueName);
                GameObject instantiateObj = null;
                Renderer rend = null;
                Color atomColor = GetColor(atomName);
                //Debug.Log(string.Format("DEBUG: Atom size: ") + atomSize);
                if (atomName != "OXT")
                {
                    if (atomSize == 1.4f)
                    {
                        instantiateObj = Instantiate(atomModel_14, new Vector3(-1 * x, y, z), Quaternion.identity);
                        rend = instantiateObj.GetComponent<Renderer>();
                        rend.material.color = atomColor;
                    }
                    else if (atomSize == 1.5f)
                    {
                        instantiateObj = Instantiate(atomModel_15, new Vector3(-1 * x, y, z), Quaternion.identity);
                        rend = instantiateObj.GetComponent<Renderer>();
                        rend.material.color = atomColor;
                    }
                    else if (atomSize == 1.85f)
                    {
                        instantiateObj = Instantiate(atomModel_185, new Vector3(-1 * x, y, z), Quaternion.identity);
                        rend = instantiateObj.GetComponent<Renderer>();
                        rend.material.color = atomColor;
                    }
                    else if (atomSize == 2f)
                    {
                        instantiateObj = Instantiate(atomModel_2, new Vector3(-1 * x, y, z), Quaternion.identity);
                        rend = instantiateObj.GetComponent<Renderer>();
                        rend.material.color = atomColor;
                    }


                    //Debug.Log(string.Format("DEBUG: Color = " + atomColor + " || Atom Name = " + atomName));
                    instantiateObj.transform.SetParent(Molecule.transform);
                    instantiateObj.isStatic = true;
                    insCollection.Add(instantiateObj);
                }
            }

            Molecule.transform.localScale = new Vector3(.01f, .01f, .01f);
            StaticBatchingUtility.Combine(insCollection.ToArray(), Molecule);

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
            //Debug.Log(string.Format("Triangle: " + mesh.triangles.Count()));

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            Molecule.GetComponent<MeshFilter>().sharedMesh = mesh;

            /***
             * Conbine MeshFilter
            MeshFilter[] meshFilters = Molecule.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];
            int j = 0;
            while (j < meshFilters.Length)
            {
                combine[j].mesh = meshFilters[j].sharedMesh;
                combine[j].transform = meshFilters[j].transform.localToWorldMatrix;
                meshFilters[j].gameObject.SetActive(false);
                j++;
            }
            Molecule.transform.GetComponent<MeshFilter>().mesh = mesh;
            Molecule.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
            Molecule.transform.gameObject.SetActive(true);
            ***/

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
        else if(type == "surface")
        {

            GameObject mol = new GameObject(name);
            mol.transform.localScale = new Vector3(scale, scale, scale);

            mol.AddComponent<MeshFilter>();
            mol.AddComponent<SkinnedMeshRenderer>();
            mol.AddComponent<Cloth>();

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

    private static Color GetColor(string atom)
    {
        if(atom[0] == 'C')
        {
            return Color.gray;
        }
        else if(atom[0] == 'O')
        {
            return Color.red;
        }
        else if(atom[0] == 'N')
        {
            return Color.blue;
        }
        else if(atom[0] == 'S')
        {
            return Color.yellow;
        }

        return Color.black;
    }

    private static float CheckSizeOfElenemts(string atom, string residue)
    {
        if (atom == "N" || atom == "C")
        {
            return 1.5f;
        } else if (atom == "CA")
        {
            return 2f;
        } else if (atom == "O")
        {
            return 1.4f;
        } else if (atom == "OXT")
        {
            return 1.5f;
        }

        if (residue == "ALA")
        {
            if (atom == "CB")
            {
                return 2f;
            }
        }
        else if (residue == "CYS")
        {
            if (atom == "CB")
            {
                return 2f;
            } else if (residue == "SG")
            {
                return 1.85f;
            }
        }
        else if (residue == "ASP")
        {
            if (atom == "CB")
            {
                return 2f;
            } else if (atom == "CG")
            {
                return 1.5f;
            } else if (atom == "OD1" || atom == "OD2")
            {
                return 1.4f;
            }
        }
        else if (residue == "GLU")
        {
            if (atom == "CB" || atom == "CG")
            {
                return 2f;
            } else if (atom == "CD")
            {
                return 1.5f;
            } else if (atom == "OE1" || atom == "OE2")
            {
                return 1.4f;
            }
        }
        else if (residue == "PHE")
        {
            if (atom == "CB" || atom == "CG" || atom == "CD1" || atom == "CD2" || atom == "CE1" || atom == "CE2" || atom == "CZ")
            {
                return 1.85f;
            }
        }
        else if (residue == "GLY")
        {
            if (atom == "OXT")
            {
                return 1.5f;
            }
        }
        else if (residue == "HIS")
        {
            if (atom == "CB" || atom == "CG" || atom == "CD2" || atom == "CE1")
            {
                return 1.85f;
            } else if (atom == "ND1" || atom == "NE2")
            {
                return 1.5f;
            }
        }
        else if (residue == "ILE")
        {
            if (atom == "CB" || atom == "CG1" || atom == "CG2" || atom == "CD1")
            {
                return 2.0f;
            }
        }
        else if (residue == "LYS")
        {
            if (atom == "CB" || atom == "CG" || atom == "CD" || atom == "CE")
            {
                return 2f;
            } else if (atom == "NZ")
            {
                return 1.5f;
            }
        }
        else if (residue == "LEU")
        {
            if (atom == "CB" || atom == "CG" || atom == "CD1" || atom == "CD2")
            {
                return 2f;
            }
        }
        else if (residue == "MET")
        {
            if (atom == "CB" || atom == "CG" || atom == "CE")
            {
                return 2f;
            }
            else if (atom == "SD")
            {
                return 1.85f;
            }
        }
        else if (residue == "MSE")
        {
            if (atom == "CB" || atom == "CG" || atom == "CE")
            {
                return 2f;
            }
            else if (atom == "SE")
            {
                return 1.85f;
            }
        }
        else if (residue == "ASN")
        {
            if (atom == "CB" || atom == "CG")
            {
                return 2f;
            } else if (atom == "OD1")
            {
                return 1.4f;
            } else if (atom == "ND2")
            {
                return 1.5f;
            }
        }
        else if (residue == "PRO")
        {
            if (atom == "CB" || atom == "CG" || atom == "CD")
            {
                return 1.85f;
            }
        }
        else if(residue == "GLN")
        {
            if(atom == "CB" || atom == "CG")
            {
                return 2f;
            }else if(atom == "CD" || atom == "NE2")
            {
                return 1.5f;
            }else if (atom == "OE1")
            {
                return 1.4f;
            }
        }
        else if(residue == "ARG")
        {
            if (atom == "CB" || atom == "CG" || atom == "CD" || atom == "CZ")
            {
                return 2f;
            }
            else if(atom == "NE" || atom == "NH1" || atom == "NH2")
            {
                return 1.5f;
            }
        }
        else if(residue == "SER")
        {
            if(atom == "CB")
            {
                return 2f;
            }else if (atom == "OG")
            {
                return 1.4f;
            }
        }
        else if(residue == " THR")
        {
            if(atom == "CB")
            {
                return 2f;
            }else if(atom == "OG1")
            {
                return 1.4f;
            }else if(atom == "OG2")
            {
                return 1.5f;
            }
        }
        else if(residue == "VAL")
        {
            if(atom == "CB" || atom == "CG1" || atom == "CG2")
            {
                return 2f;
            }
        }
        else if (residue == "TRP")
        {
            if(atom == "CB")
            {
                return 2f;
            }else if(atom == "NE1")
            {
                return 1.5f;
            }else if(atom == "CG" || atom == "CD1" || atom == "CD2" || atom == "CE2" ||
                atom == "CE3" || atom == "CZ2" || atom == "CZ3" || atom == "CH2")
            {
                return 1.85f;
            }
        }
        else if(residue == "TRY")
        {
            if(atom == "CB")
            {
                return 2.0f;
            }else if(atom == "OH")
            {
                return 1.4f;
            }else if(atom == "CG" || atom == "CD1" || atom == "CD2" || atom == "CE1"
                || atom == "CE3" || atom == "CZ")
            {
                return 1.85f;
            }
        }
        else if(residue == "UKN")
        {
            if (atom == "OXT")
            {
                return 1.5f;
            }
        }

        return 1.4f;
    }
}