using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{

    public string sceneName;
    // Use this for initialization
    void Start()
    {
        sceneName = GetComponent<string>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SceneLoader()
    {
        SceneManager.LoadScene(sceneName);
    }
}
