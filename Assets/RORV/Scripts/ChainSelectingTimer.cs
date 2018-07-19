using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChainSelectingTimer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public static bool isClick = false;
    public int timeremain = 3;
    public static string molecule_name;
    Button _button;

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
        isClick = true;
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
            isClick = true;

        }

        timeremain--;
    }

    public void Clicked()
    {
        isClick = true;
        Debug.Log(string.Format("Debug: The button " + _button.transform.name + " is Clicked."));
        molecule_name = _button.transform.name;
        SceneManager.LoadScene("NewRaycast");
    }

    public bool getIsClick()
    {
        return isClick;
    }
}
