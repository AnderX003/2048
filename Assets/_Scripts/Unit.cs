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
        public UnitAnimationQueue AnimationQueue { get; } = new UnitAnimationQueue();
        public bool IsEmpty => isEmpty;
        public RectTransform UnitRectTransform => unitRectTransform;
        public Image UnitImage => unitImage;
        public Text UnitText => unitText;

        [SerializeField] private bool isEmpty;
        [SerializeField] private RectTransform unitRectTransform;
        [SerializeField] private Image unitImage;
        [SerializeField] private Text unitText;
    }
}
