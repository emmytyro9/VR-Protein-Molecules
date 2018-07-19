using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackTimer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool isClick = false;
    public int timeremain = 3;
    Button _button;

    // Use this for initialization
    void Start()
    {

        _button = GetComponent<Button>();

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
        timeremain = 2;
        isClick = false;
    }

    void countDown()
    {
        if (timeremain <= 0)
        {
            NotificationCenter.DefaultCenter().PostNotification(this, "EnNada");
            _button.onClick.Invoke();
            CancelInvoke("countDown");
            timeremain = 2;

        }

        timeremain--;
    }

    public void Clicked()
    {
        Debug.Log(string.Format("Surface is clicked."));
        Debug.Log(string.Format("Debug: Back button was Clicked."));
        SceneManager.LoadScene("KeyboardScene");

    }
}
