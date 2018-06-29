﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

public class SpacefillTimer : MonoBehaviour
{
    public static bool isClick = false;
    public int timeremain = 3;
    Button _button;
    private MainMol mol;

    // Use this for initialization
    void Start()
    {

        _button = GetComponent<Button>();

    }

    void Update()
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        NotificationCenter.DefaultCenter().PostNotification(this, "EnBoton");
        InvokeRepeating("countDown", 1, 1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        NotificationCenter.DefaultCenter().PostNotification(this, "EnNada");
        Debug.Log(string.Format("Debug: Cancel."));
        CancelInvoke("countDown");
        timeremain = 3;
        isClick = false;
    }

    void countDown()
    {
        if (timeremain <= 0)
        {
            NotificationCenter.DefaultCenter().PostNotification(this, "EnNada");
            _button.onClick.Invoke();
            CancelInvoke("countDown");
            timeremain = 3;

        }

        timeremain--;
    }

    public void Clicked()
    {
        isClick = true;
        Debug.Log(string.Format("Debug: Clicked."));
        Destroy(GameObject.FindGameObjectWithTag("Mol"));
        MainMol.SpawnMolecule("spacefill");

    }

    public bool getIsClick()
    {
        return isClick;
    }
}

