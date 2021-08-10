using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts
{
    public class MessageBoxController : MonoBehaviour
    {
        //[SerializeField] private GameObject MessageBoxPanel;
        [SerializeField] private RectTransform MessageBoxPanel;
        [SerializeField] private Text questionText;
        private Action agreeAction, disagreeAction;

        public void OpenNewMessageBox(Action agree, Action disagree, string question)
        {
            agreeAction = agree;
            disagreeAction = disagree;
            questionText.text = question;
            //MessageBoxPanel.SetActive(true);
            MessageBoxPanel.localPosition = Vector3.zero;
        }

        public void Agree()
        {
            agreeAction();
            //MessageBoxPanel.SetActive(false);
            MessageBoxPanel.localPosition = new Vector3(1500,0,0);
        }

        public void Disagree()
        {
            disagreeAction();
            //MessageBoxPanel.SetActive(false);
            MessageBoxPanel.localPosition = new Vector3(1500,0,0);
        }
    }
}