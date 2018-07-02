using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

namespace ButtonTimers
{


    public class ButtonTimer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
            Debug.Log(string.Format("Debug: Clicked."));

            if (GameObject.FindGameObjectWithTag("Spacefill").GetComponent<ButtonTimer>().getIsClick())
            {
                /*
                Destroy(GameObject.FindGameObjectWithTag("Mol"));
                MainMol.SpawnMolecule("spacefill");
                */
                Debug.Log(string.Format("Spacefill is clicked"));
            }

            else if (GameObject.FindGameObjectWithTag("Surface").GetComponent<ButtonTimer>().getIsClick())
            {
                /*
                Destroy(GameObject.FindGameObjectWithTag("Mol"));
                MainMol.SpawnMolecule("surface");
               */
                Debug.Log(string.Format("Spacefill is clicked"));
            }
            else
            {
                Debug.Log(string.Format("Debug: None is clicked."));
            }
        }

        public bool getIsClick()
        {
            return isClick;
        }
    }
}