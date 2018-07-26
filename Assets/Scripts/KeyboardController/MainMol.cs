using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CurvedVRKeyboard;
using System;

public class MainMol : MonoBehaviour
{

    //user should be able to grab small molecules.
    //Bigger molecule.
    //test other molecules.



    private static string input = ChainSelectingTimer.molecule_name;
    private static float molScale = 0.015f;
    private static Material molMat;
    private static float softness = 0f;
    private static GameObject Molecule;

    private static List<string> flaggedAminos = new List<string>() { "LEU", "ILE", "VAL", "MET", "PRO", "PHE", "TRP", "TYR", "ALA" };
    private static List<string> flaggedAtoms = new List<string>() { "N", "HN", "CA", "C", "O", "OT", "OH", "HH" };

    private static char GetChain()
    {
        return input[5];
    }

    public static void SpawnMolecule(string type)
    {
        Debug.Log(string.Format("Chain: " + GetChain()));  // Debugging if the system is able to get the correctly chain of molecule.

        GameObject mol = readEfvet(input.ToLower(), molScale, type);
        if (mol != null)
        {
            type = "";
            mol.transform.tag = "Mol";
            mol.GetComponent<Transform>().transform.position = GameObject.Find("OVRPlayerController").transform.position - new Vector3(0f, 1f, -3f);
            Debug.Log(string.Format("Mol x,y,z: ") + GameObject.FindGameObjectWithTag("Mol").transform.position.x + ", " + GameObject.FindGameObjectWithTag("Mol").transform.position.y + ", " + GameObject.FindGameObjectWithTag("Mol").transform.position.z);
            Debug.Log(string.Format("OVRPlayerController x,y,z: ") + GameObject.Find("OVRPlayerController").transform.position.x + ", " + GameObject.Find("OVRPlayerController").transform.position.y + ", " + GameObject.Find("OVRPlayerController").transform.position.z);

        }
    }

    public static string[] _getMoleculeData()
    {
        string[] data = new string[] { };

        try
        {
            string pdbFile = input.Substring(0, input.Length - 2);
            string pathHetatm = "Assets/Resources/" + pdbFile + ".pdb";
            StreamReader reader1 = new StreamReader(pathHetatm);
            data = reader1.ReadToEnd().Split('\n');
            Debug.Log(string.Format("DEBUG: PDB file are read Successfully!!!"));
            reader1.Close();
        }
        catch(Exception e)
        {
            Debug.Log(string.Format("Exception Raised : " + e.ToString()));
        }


        return data;
    }

    public static string[] _getEfvet(string name)
    {
        string[] lines = new string[] { };
        try
        {
            string path = "Assets/Resources/" + input + ".txt";
            Debug.Log(string.Format("DEBUG: the file " + input + " is read successfully"));
            StreamReader reader = new StreamReader(path);
            lines = reader.ReadToEnd().Split('\n');
            reader.Close();
        }
        catch (Exception e)
        {
            Debug.Log(string.Format("Exception Raised : " + e.ToString()));
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
            Molecule = new GameObject("Spacefill: " + input);
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

            if (verticesCount > 50000)
            {
                Debug.Log(string.Format("This molecule is too large(more than 50,000 vertices), we are going to destroy it!"));
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
                float atomSize = CheckSizeOfElenemts(atomName, residueName);
                GameObject instantiateObj = null;
                Renderer rend = null;
                Color atomColor = GetColor(atomName);

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
                    //modify here - grabbable atom.
                    instantiateObj.transform.SetParent(Molecule.transform);
                    instantiateObj.isStatic = true;
                    insCollection.Add(instantiateObj);
                }
            }

            Molecule.transform.localScale = new Vector3(.015f, .015f, .015f);
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

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            Molecule.GetComponent<MeshFilter>().sharedMesh = mesh;

            Molecule.AddComponent<MeshCollider>();
            MeshCollider molCollider = Molecule.GetComponent<MeshCollider>();
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
        else if(type == "surface")
        {

            GameObject surface = new GameObject("Surface: " + input);
            

            GameObject mol = new GameObject(name);
            mol.transform.localScale = new Vector3(scale, scale, scale);
            mol.transform.SetParent(surface.transform);

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

                colors.Add(GetColor(atomName));

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
            //Debug.Log(string.Format("DEBUG: Position (z , y , z) = (") + mol.transform.position.x + ", " + mol.transform.position.y + ", " + mol.transform.position.z + ")");

            

            ///**************************************************************
            ///To Generate the HETATM (Small Molecule) in surface molecule.
            
            Regex sepReg1 = new Regex(@"\s+");
            string[] h = _getMoleculeData();
            print("Array Length " + h.Length);
            GameObject HETATM = new GameObject("HETATM");
            HETATM.transform.tag = "Surface";

            for (int i = 0; i < h.Length; i++)
            {
                string line1 = h[i].TrimStart(' ');
                string[] stArrayData1 = sepReg1.Split(line1);
               

                if (stArrayData1[0] == "HETATM")
                {
                    float Hx = float.Parse(stArrayData1[6]);
                    float Hy = float.Parse(stArrayData1[7]);
                    float Hz = float.Parse(stArrayData1[8]);
                    char[] chain = stArrayData1[4].ToCharArray();

                    //Debug.Log(string.Format("Chain: " + chain[0])); // Debugging - To see the chain of protein molecule.

                    if (chain[0] == GetChain())
                    {
                        //print("This is: " + stArrayData1[0] + " of " + GetChain() + " chain.");

                        string atom = stArrayData1[2];
                        string residue = stArrayData1[3];

                        float atomSize = CheckSizeOfElenemts(atom, residue);
                        GameObject instantiateObj = null;
                        Renderer rend = null;
                        Color atomColor = GetColor(atom);
                        //Debug.Log(string.Format("DEBUG: HETATM  Atom size: ") + atomSize);
                        if (atom != "OXT")
                        {
                            if (atomSize == 1.4f)
                            {
                                instantiateObj = Instantiate(atomModel_14, new Vector3(-1 * Hx, Hy, Hz), Quaternion.identity);
                                rend = instantiateObj.GetComponent<Renderer>();
                                rend.material.color = atomColor;
                            }
                            else if (atomSize == 1.5f)
                            {
                                instantiateObj = Instantiate(atomModel_15, new Vector3(-1 * Hx, Hy, Hz), Quaternion.identity);
                                rend = instantiateObj.GetComponent<Renderer>();
                                rend.material.color = atomColor;
                            }
                            else if (atomSize == 1.85f)
                            {
                                instantiateObj = Instantiate(atomModel_185, new Vector3(-1 * Hx, Hy, Hz), Quaternion.identity);
                                rend = instantiateObj.GetComponent<Renderer>();
                                rend.material.color = atomColor;
                            }
                            else if (atomSize == 2f)
                            {
                                instantiateObj = Instantiate(atomModel_2, new Vector3(-1 * Hx, Hy, Hz), Quaternion.identity);
                                rend = instantiateObj.GetComponent<Renderer>();
                                rend.material.color = atomColor;
                            }

                            instantiateObj.transform.SetParent(HETATM.transform);
                        }
                    }
                }
            }

            

            HETATM.GetComponent<Transform>().transform.position = GameObject.Find("OVRPlayerController").transform.position - new Vector3(0f, 1f, -3f);

            HETATM.transform.localScale = new Vector3(.015f, .015f, .015f);
            HETATM.transform.SetParent(surface.transform);

            HETATM.AddComponent<MeshFilter>();
            HETATM.GetComponent<MeshFilter>().sharedMesh = mesh;


            HETATM.AddComponent<MeshCollider>();
            MeshCollider HETATMCollider = HETATM.GetComponent<MeshCollider>();
            HETATMCollider.inflateMesh = true;
            HETATM.GetComponent<MeshCollider>().sharedMesh = mesh;

            HETATM.AddComponent<OVRGrabbable>();
            OVRGrabbable hetatm = HETATM.GetComponent<OVRGrabbable>();
            Collider collider = HETATM.GetComponent<MeshCollider>();
            hetatm.GetComponent<OVRGrabbable>().setGrabPoint(collider);

            HETATM.AddComponent<Rigidbody>();
            Rigidbody rigid = HETATM.GetComponent<Rigidbody>();
            rigid.isKinematic = true;
            rigid.useGravity = false;

            
            
            ///End of generating HETATM (Small Molecule) in surface molecule.
            ///**************************************************************


            string[] data = GetAllChain();
            print("DEBUG == There are: " + data.Length +  " chain of this molecule.");
            for (int i = 0; i < data.Length; i++)
            {
                print("DEBUG == Consist of: " + data[i]);
            }
            

            return mol;
        }
     return null;
    }

    private static string[] GetAllChain()
    {
        Regex sepReg2 = new Regex(@"\s+");
        string[] atom_data = _getMoleculeData();
        var allChain = new List<string>();

        for (int i = 0; i < atom_data.Length; i++)
         {
            string line1 = atom_data[i].TrimStart(' ');
            string[] data = sepReg2.Split(line1);

            if(data[0] == "ATOM" || data[0] == "HETATM")
            {
                if(!allChain.Contains(data[4]))
                {
                    allChain.Add(data[4]);
                }
            }

         }

        return allChain.ToArray();
     }

    private static Color GetColor(string atom)
    {

        if (atom[0] == 'C')
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
        else if(atom[0] == 'P')
        {
            return new Color(255f, 165f, 0f);
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