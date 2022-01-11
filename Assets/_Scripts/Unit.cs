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

        public UnitAnimationQueue AnimationQueue { get; private set; } = new UnitAnimationQueue();
        public bool IsEmpty => isEmpty;

        [SerializeField] private bool isEmpty;


        #region Components

        [SerializeField] private RectTransform unitRectTransform;
        [SerializeField] private Image unitImage;
        [SerializeField] private Text unitText;
        
        public RectTransform UnitRectTransform => unitRectTransform;
        public Image UnitImage => unitImage;
        public Text UnitText => unitText;
        
        #endregion
    }
}
