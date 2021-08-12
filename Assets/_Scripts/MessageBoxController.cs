using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts
{
    public class MessageBoxController : MonoBehaviour
    {
        [SerializeField] private RectTransform MessageBoxPanel;
        [SerializeField] private Animator animator;
        [SerializeField] private Text questionText;
        [SerializeField] private Vector3 hidePosition;
        [SerializeField] private float outAnimDuration;
        private Action agreeAction, disagreeAction;
        private static readonly int Any = Animator.StringToHash("Any");

        public void OpenNewMessageBox(Action agree, Action disagree, string question)
        {
            agreeAction = agree;
            disagreeAction = disagree;
            questionText.text = question;
            MessageBoxPanel.localPosition = Vector3.zero;
            animator.SetTrigger(Any);
        }

        public void Agree()
        {
            agreeAction();
            Close();
        }

        public void Disagree()
        {
            disagreeAction();
            Close();
        }

        private void Close()
        {
            animator.SetTrigger(Any);
            Invoke(nameof(Hide), outAnimDuration);
        }

        private void Hide()
        {
            MessageBoxPanel.localPosition = hidePosition;
        }
    }
}