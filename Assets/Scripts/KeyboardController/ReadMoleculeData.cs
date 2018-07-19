using CurvedVRKeyboard;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class ReadMoleculeData : MonoBehaviour {

    private string input = KeyboardStatus.sentOutput;
    public GameObject buttonModel;
    public GameObject textError;
    public Transform raycast_transform;
    private Ray ray;
    private string buttonName;
    private float rayLength = Mathf.Infinity;


    private void Start()
    {
        buttonModel = GetComponent<GameObject>();
        textError = GetComponent<GameObject>();
        raycast_transform = GetComponent<Transform>();
        //timer = GetComponent<ChainSelectingTimer>();
    }

    private void Awake()
    {
        string[] data = GetAllChain();
        Debug.Log(string.Format("DEBUG == There are: " + data.Length + " chains of this molecule."));
        Vector3 oldPos = new Vector3(0, 3.45f, -6.529f);

        if (data.Length > 0)
        {
            for (int i = 0; i < data.Length; i++)
            {
                Debug.Log(string.Format("DEBUG == Consist of: " + data[i]));

                GameObject button = (GameObject)Instantiate(buttonModel);
                button.transform.name = input + "-" + data[i]; 
                
                //button.GetComponent<Button>().onClick.AddListener(OnClick);
                button.transform.GetChild(0).GetComponent<Text>().text = input + "-" + data[i];
                button.transform.SetParent(GameObject.Find("ChainsList").transform);
   
                //button.AddComponent<ChainSelectingTimer>();

                /*
                GameObject gazePrefab = Resources.Load("GazePointerRing", typeof(GameObject)) as GameObject;

                gazePrefab.transform.SetParent(GameObject.Find("Canvas").transform);
                GameObject.Find("Quad").SetActive(false);
                GameObject.Find("Half").SetActive(false);
                GameObject.Find("Full").SetActive(false);
                */

                //ColorBlock cb = button.GetComponent<Button>().colors;
                //cb.pressedColor = new Color(255, 0, 0);
                // button.GetComponent<Button>().colors = cb;

                //raycastingSource.transform.position = GameObject.Find("OVRCameraRig").transform.position;
                //Debug.DrawRay(GameObject.Find("OVRCameraRig").transform.position, GameObject.Find("OVRCameraRig").transform.TransformDirection(Vector3.forward * 1000), Color.green);

                Vector3 newPos = oldPos + new Vector3(0f, -0.55f, 0f);
                button.transform.position = newPos;
                oldPos = newPos;
            }
        }
        else
        { 
            //Error message.
            textError.GetComponent<Text>().text = "Sorry, this molecule was not found in our data. Please try again.";
        }

    }

    void Update()
    {

        var fw = GameObject.Find("CenterEyeAnchor").transform.TransformDirection(Vector3.forward);

        Debug.DrawRay(GameObject.Find("CenterEyeAnchor").transform.position, fw * 100, Color.green);


        ray = new Ray(raycast_transform.position, raycast_transform.forward);
    }
   

    void OnClick() 
    {
        Debug.Log("The button is clicked!");

    }



    public string[] _getMoleculeData()
    {
        string[] data = new string[] { };

        try
        {
            string pathHetatm = "Assets/Resources/" + input + ".pdb";
            StreamReader reader1 = new StreamReader(pathHetatm);
            data = reader1.ReadToEnd().Split('\n');
            Debug.Log(string.Format("DEBUG: PDB file is read Successfully!!!"));
            reader1.Close();
        }
        catch (Exception e)
        {
            Debug.Log(string.Format("Exception Raised : " + e.ToString()));
        }
        return data;
    }

    private string[] GetAllChain()
    {
        Regex sepReg2 = new Regex(@"\s+");
        string[] atom_data = _getMoleculeData();
        var allChain = new List<string>();

        for (int i = 0; i < atom_data.Length; i++)
        {
            string line1 = atom_data[i].TrimStart(' ');
            string[] data = sepReg2.Split(line1);
   
            if (data[0] == "ATOM" || data[0] == "HETATM")
            {
                char[] chain = data[4].ToCharArray();
                
                if (!allChain.Contains(chain[0].ToString()))
                {
                    allChain.Add(chain[0].ToString());
                }
            }

        }

        return allChain.ToArray();
    }
}
