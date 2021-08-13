using UnityEngine;
using UnityEngine.UI;

namespace _Scripts
{
    public class Unit : MonoBehaviour
    {
        public int Value { get; set; }
        public int PositionX {get; set; }
        public int PositionY { get; set; }
        public bool WasChanged { get; set; }
        public bool IsEmpty => isEmpty;

        [SerializeField] private bool isEmpty;

        #region Components

        [SerializeField] private RectTransform unitRectTransform;
        [SerializeField] private Image unitImage;
        [SerializeField] private GameObject textObject;
        [SerializeField] private Text unitText;
        
        public RectTransform UnitRectTransform => unitRectTransform;
        public Image UnitImage => unitImage;
        public GameObject TextObject => textObject;
        public Text UnitText => unitText;
        
        #endregion
    }
}
