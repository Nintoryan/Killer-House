using UnityEngine;

public class Swipe : MonoBehaviour
{
    public bool Tap { get; private set; }
    public bool SwipeLeft { get; private set; }
    public bool SwipeRight { get; private set; }
    public bool SwipeUp { get; private set; }
    public bool SwipeDown { get; private set; }
    public Vector2 SwipeDelta { get; private set; }

    private Vector2 startTouch;
    private bool isDraging;
    

    private void Update()
    {
        Tap = SwipeUp = SwipeDown = SwipeLeft = SwipeRight = false;

        #region Standalone Input
        if (Input.GetMouseButtonDown(0))
        {
            Tap = true;
            isDraging = true;
            startTouch = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Reset();
        }
        #endregion

        #region MobileInput

        if (Input.touches.Length > 0)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                Tap = true;
                isDraging = true;
                startTouch = Input.touches[0].position;
            }
            else if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
            {
                Reset();
            }
        }

        #endregion

        SwipeDelta = Vector2.zero;
        if (isDraging)
        {
            if (Input.touches.Length > 0)
            {
                SwipeDelta = Input.touches[0].position - startTouch;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                SwipeDelta = (Vector2) Input.mousePosition - startTouch;
            }
        }

        if (SwipeDelta.magnitude > 125)
        {
            float x = SwipeDelta.x;
            float y = SwipeDelta.y;
            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                if (x < 0)
                {
                    SwipeLeft = true;
                }
                else
                {
                    SwipeRight = true;
                }
            }
            else
            {
                if (y < 0)
                {
                    SwipeDown = true;
                }
                else
                {
                    SwipeUp = true;
                }
                
            }
            Reset();
        }
    }

    private void Reset()
    {
        isDraging = false;
        startTouch = SwipeDelta = Vector2.zero;
    }
}
