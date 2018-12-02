using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class GestureDone : UnityEvent {}

public class CircularGesture : MonoBehaviour
{
    private const int NUMS_OF_CIRCLES_TO_SHOW = 2;

    public GestureDone OnGestureDone = new GestureDone();

    private List<Vector2> mGestureDetector = new List<Vector2>();
    private Vector2 mGestureSum = Vector2.zero;
    private float mGestureLength = 0;
    private int mGestureCount = 0;

    void Update()
    {
        if (IsGestureDone() && OnGestureDone != null)
            OnGestureDone.Invoke();
    }

    private bool IsGestureDone()
    {
        if (Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (Input.touches.Length != 1)
            {
                mGestureDetector.Clear();
                mGestureCount = 0;
            }
            else
            {
                if (Input.touches[0].phase == TouchPhase.Canceled || Input.touches[0].phase == TouchPhase.Ended)
                    mGestureDetector.Clear();
                else if (Input.touches[0].phase == TouchPhase.Moved)
                {
                    Vector2 p = Input.touches[0].position;
                    if (mGestureDetector.Count == 0 || (p - mGestureDetector[mGestureDetector.Count - 1]).magnitude > 10)
                        mGestureDetector.Add(p);
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonUp(0))
            {
                mGestureDetector.Clear();
                mGestureCount = 0;
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    Vector2 p = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                    if (mGestureDetector.Count == 0 || (p - mGestureDetector[mGestureDetector.Count - 1]).magnitude > 10)
                        mGestureDetector.Add(p);
                }
            }
        }

        if (mGestureDetector.Count < 10)
            return false;

        mGestureSum = Vector2.zero;
        mGestureLength = 0;
        Vector2 prevDelta = Vector2.zero;
        for (int i = 0; i < mGestureDetector.Count - 2; i++)
        {
            Vector2 delta = mGestureDetector[i + 1] - mGestureDetector[i];
            float deltaLength = delta.magnitude;
            mGestureSum += delta;
            mGestureLength += deltaLength;

            float dot = Vector2.Dot(delta, prevDelta);
            if (dot < 0f)
            {
                mGestureDetector.Clear();
                mGestureCount = 0;
                return false;
            }

            prevDelta = delta;
        }

        int gestureBase = (Screen.width + Screen.height) / 4;

        if (mGestureLength > gestureBase && mGestureSum.magnitude < gestureBase / 2)
        {
            mGestureDetector.Clear();
            mGestureCount++;
            if (mGestureCount >= NUMS_OF_CIRCLES_TO_SHOW)
                return true;
        }

        return false;
    }
}
