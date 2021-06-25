using UnityEngine;
using UnityEngine.UI;

namespace _Scripts
{
    public class Unit : MonoBehaviour
    {
        public int Value { get; set; }
        public int PositionX {get; set; }
        public int PositionY { get; set; }
        
        public  bool ReadyToDeath { get; set; }
        public bool WasChanged { get; set; }
        
        public RectTransform UnitRectTransform;
        public Image UnitImage;
        public GameObject TextObject;
        public Text UnitText;
    }
}
