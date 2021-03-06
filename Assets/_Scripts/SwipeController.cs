using UnityEngine;

namespace _Scripts
{
    public class SwipeController : MonoBehaviour
    {
        private bool isDragging;
        private Vector2 startTouch, swipeDelta;
        [SerializeField] private float sensitivity = 50;
        [SerializeField] private int borderTop;
        [SerializeField] private int borderBottom;
        [SerializeField] private int borderLeft;
        [SerializeField] private int borderRight;
        private Vector2 canvasSize;
        public bool Enabled { get; set; } = true;
        public bool SwipeRight { get; private set; }
        public bool SwipeUp { get; private set; }
        public bool SwipeLeft { get; private set; }
        public bool SwipeDown { get; private set; }

        private void Start()
        {
            canvasSize = new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight);
        }

        private void Update()
        {
            if (!Enabled) return;
            SwipeLeft = SwipeRight = SwipeUp = SwipeDown = false;

            #region Mobile Inputs

            if (Input.touches.Length != 0)
            {
                switch (Input.touches[0].phase)
                {
                    case TouchPhase.Began:
                        isDragging = true;
                        startTouch = Input.touches[0].position;
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        isDragging = false;
                        Reset();
                        break;
                }
            }

            #endregion

            // Calculating the distance
            swipeDelta = Vector2.zero;
            if (isDragging)
            {
                if (Input.touches.Length > 0) swipeDelta = Input.touches[0].position - startTouch;
                else if (Input.GetMouseButton(0)) swipeDelta = (Vector2)Input.mousePosition - startTouch;
            }

            // Making a swipe
            if (!(swipeDelta.magnitude > sensitivity)) return;
            float x = swipeDelta.x, y = swipeDelta.y;
            if (startTouch.y > borderBottom)
            {
                if (Mathf.Abs(x) > Mathf.Abs(y))
                {
                    if (x < 0)
                    {
                        if (startTouch.x < canvasSize.x - borderRight)
                        {
                            SwipeLeft = true;
                        }
                    }
                    else
                    {
                        if (startTouch.x > borderLeft)
                        {
                            SwipeRight = true;
                        }
                    }
                }
                else
                {
                    if (y < 0)
                    {
                        if (startTouch.y < canvasSize.y - borderTop) SwipeDown = true;
                    }
                    else
                    {
                        /*if (startTouch.y > borderBottom)*/
                        SwipeUp = true;
                    }
                }
            }

            Reset();
        }

        private void Reset()
        {
            isDragging = false;
            startTouch = swipeDelta = Vector2.zero;
        }
    }
}
