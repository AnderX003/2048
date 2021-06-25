using UnityEngine;

namespace _Scripts
{
    public class SwipeController : MonoBehaviour
    {
        private bool isDragging;
        private Vector2 StartTouch, SwipeDelta;
        public bool Enabled = true;
        public float Sensitivity = 50;
        public bool SwipeRight { get; private set; }
        public bool SwipeUp { get; private set; }
        public bool SwipeLeft { get; private set; }
        public bool SwipeDown { get; private set; }

        private void Update()
        {
            if (Enabled)
            {
                SwipeLeft = SwipeRight = SwipeUp = SwipeDown = false;

                #region PC Inputs

                if (Input.GetMouseButtonDown(0))
                {
                    isDragging = true;
                    StartTouch = Input.mousePosition;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    isDragging = false;
                    Reset();
                }

                #endregion

                #region Mobile Inputs

                if (Input.touches.Length != 0)
                {
                    if (Input.touches[0].phase == TouchPhase.Began)
                    {
                        isDragging = true;
                        StartTouch = Input.touches[0].position;
                    }
                    else if (Input.touches[0].phase == TouchPhase.Ended ||
                             Input.touches[0].phase == TouchPhase.Canceled)
                    {
                        isDragging = false;
                        Reset();
                    }
                }

                #endregion

                // Calculating the distance
                SwipeDelta = Vector2.zero;
                if (isDragging)
                {
                    if (Input.touches.Length > 0) SwipeDelta = Input.touches[0].position - StartTouch;
                    else if (Input.GetMouseButton(0)) SwipeDelta = (Vector2) Input.mousePosition - StartTouch;
                }

                // Making a swipe
                if (SwipeDelta.magnitude > Sensitivity)
                {
                    float x = SwipeDelta.x, y = SwipeDelta.y;
                    if (Mathf.Abs(x) > Mathf.Abs(y))
                    {
                        if (x < 0) SwipeLeft = true;
                        else SwipeRight = true;
                    }
                    else
                    {
                        if (y < 0) SwipeDown = true;
                        else SwipeUp = true;
                    }

                    Reset();
                }
            }
        }

        private void Reset()
        {
            isDragging = false;
            StartTouch = SwipeDelta = Vector2.zero;
        }
    }
}
