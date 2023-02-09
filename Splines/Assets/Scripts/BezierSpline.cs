using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// https://www.youtube.com/watch?v=jvPPXbo87ds&ab_channel=FreyaHolm%C3%A9r

namespace Splines
{
    [ExecuteInEditMode]
    public class BezierSpline : MonoBehaviour
    {
        [Header("Gizmo Controls")]
        [Range(0f, 0.5f)] public float pointSize = 0;
        public bool showTrackingPoint = false;
        public bool showPoints = false;
        public bool showLines = false;
        public bool showCurve = false;

        [Space(10)]
        [Header("Anchor Controls")]
        // current number of bezier curves
        [SerializeField] private int numOfCurves = 0;
        // number of bezier curves on last frame
        private int numOfCurvesLastFrame = 0;

        // tracker point control value
        [SerializeField] [Range(0f, 3f)] private float tValue = 0f;

        // the point that moves
        [SerializeField] private Vector3 trackingPoint = Vector3.zero;

        // list of the all the bezier curves
        [SerializeField] private List<Vector3> points = new List<Vector3>();
        private Vector3 endPoint = Vector3.zero;
        
        void Start()
        {
            points.Add(endPoint);
        }

        void Update()
        {
            if (numOfCurves > numOfCurvesLastFrame)
                AddCurve();
            if (numOfCurvesLastFrame > numOfCurves)
                RemoveCurve();

            if (points.Count != 0)
                PolynomialCoefficients();

            // updates for next frame
            numOfCurvesLastFrame = numOfCurves;
        }

        public void AddCurve()
        {
            if (points.Count != 0)
            {
                endPoint = points[points.Count - 1];
                points.RemoveAt(points.Count - 1);
            }

            Vector3 startPos = endPoint;
            for (int i = 0; i < 3; i++)
            {
                points.Add(startPos);
            }

            points.Add(endPoint);
        }

        public void RemoveCurve()
        {
            if (points.Count == 0)
            {
                points.Clear();
                return;
            }

            endPoint = points[points.Count - 1];

            for (int i = 0; i < 4; i++)
            {
                points.RemoveAt(points.Count - 1);
            }

            points.Add(endPoint);
        }

        public void Bernstein()
        {
            int localCurve = (int)tValue;

            // more readable anchor points
            Vector3 p0 = points[0 + localCurve];
            Vector3 p1 = points[1 + localCurve];
            Vector3 p2 = points[2 + localCurve];
            Vector3 p3 = points[3 + localCurve];

            // split into 4 lines
            Vector3 con1 = p0 * (Mathf.Pow(-tValue, 3) + 3 * Mathf.Pow(tValue, 2) - 3 * tValue + 1);
            Vector3 con2 = p1 * (3 * Mathf.Pow(tValue, 3) - 6 * Mathf.Pow(tValue, 2) + 3 * tValue);
            Vector3 con3 = p2 * (-3 * Mathf.Pow(tValue, 3) + 3 * Mathf.Pow(tValue, 2));
            Vector3 con4 = p3 * Mathf.Pow(tValue, 3);

            trackingPoint = con1 + con2 + con3 + con4;
        }

        public void PolynomialCoefficients()
        {
            // gets what curve the tracking point is on
            int localCurve = (int)tValue + 2 * (int)tValue;
            // t value local to the local curve
            float localT = tValue - (int)tValue;

            // more readable anchor points
            Vector3 p0 = points[0 + localCurve];
            Vector3 p1 = points[1 + localCurve];
            Vector3 p2 = points[2 + localCurve];
            Vector3 p3 = points[3 + localCurve];

            // split exquation into 3 separate lines
            Vector3 con1 = localT * (-3 * p0 + 3 * p1);
            Vector3 con2 = localT * localT * (3 * p0 - 6 * p1 + 3 * p2);
            Vector3 con3 = localT * localT * localT * (-p0 + 3 * p1 - 3 * p2 + p3);

            // add all together
            trackingPoint = p0 + con1 + con2 + con3;
        }

        private void OnDrawGizmos()
        {
            if (showLines && points.Count != 0)
            {
                // draws the straight lines connecting the anchor points
                Gizmos.color = Color.white;
                for (int i = 0; i < points.Count - 3; i += 3)
                {
                    Gizmos.DrawLine(points[i], points[i + 1]);
                    Gizmos.DrawLine(points[i + 2], points[i + 3]);
                }
            }

            if (showPoints && points.Count != 0)
            {
                // draws the anchor points
                Gizmos.color = Color.blue;
                for (int i = 0; i < points.Count; i++)
                {
                    Gizmos.DrawWireSphere(points[i], pointSize);

                    UnityEditor.Handles.Label(points[i], "P" + i.ToString());
                }
            }

            if (showTrackingPoint)
            {
                // draws the tracking point
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(trackingPoint, pointSize);
            }

            if (showCurve)
            {
                Texture2D texture = null;

                for (int i = 0; i < points.Count - 3; i += 3)
                {
                    UnityEditor.Handles.DrawBezier(points[i], points[i + 3], points[i + 1], points[i + 2], Color.white, texture, 2f);
                }
            }
        }
    }
}