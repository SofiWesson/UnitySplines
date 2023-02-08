using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// https://www.youtube.com/watch?v=jvPPXbo87ds&ab_channel=FreyaHolm%C3%A9r

namespace Splines
{
    // To move and visualise the anchor points
    [Serializable]
    public class AnchorPoints
    {
        public List<Vector3> anchorPoints = new List<Vector3>();
    }

    public class BezierCurve
    {
        // First 4 are control points, will have to change to 3 and an end point
        // Last 5 are lerping points
        private List<Vector3> points;
        private AnchorPoints anchorPoints = new AnchorPoints();

        // creates the bezier curve
        public void Initialise(int a_points)
        {
            points = new List<Vector3>();
            for (int i = 0; i < a_points; i++)
            {
                points.Add(Vector3.zero);
            }

            for (int i = 0; i < 4; i++)
            {
                anchorPoints.anchorPoints.Add(points[i]);
            }
        }

        // get all the points of a bezier curve
        public List<Vector3> GetPoints()
        {
            return points;
        }

        // set all the points of a bezier curve
        public void SetPoints(List<Vector3> a_points)
        {
            points = a_points;
        }

        // get a specific point
        public Vector3 GetPoint(int a_index)
        {
            return points[a_index];
        }

        // set a specific point
        public void SetPoint(int a_index, Vector3 a_value)
        {
            points[a_index] = a_value;
        }

        // get anchor points
        public AnchorPoints GetAnchorPoints()
        {
            return anchorPoints;
        }
    }

    [ExecuteInEditMode]
    public class BezierSpline : MonoBehaviour
    {
        [Header("Gizmo Controls")]
        [Range(0f, 0.5f)] public float pointSize = 0;
        public bool showTrackingPoint = false;
        public bool showPoints = false;
        public bool showLines = false;

        [Space(10)]
        [Header("Anchor Controls")]
        // current number of bezier curves
        [SerializeField] private int numOfCurves = 0;
        // number of bezier curves on last frame
        private int numOfCurvesLastFrame = 0;
        [SerializeField] private List<AnchorPoints> anchorPoints = new List<AnchorPoints>();

        // tracker point control value
        [SerializeField] [Range(0f, 1f)] private float tValue = 0f;

        // the point that moves
        private Vector3 trackingPoint = Vector3.zero;

        // static number of how many points are in a curve
        static private int numPointsInCurve = 9;
        // list of the all the bezier curves
        private List<BezierCurve> curves = new List<BezierCurve>();
        
        void Start()
        {

        }

        void Update()
        {
            if (numOfCurves > numOfCurvesLastFrame)
                AddCurve();
            if (numOfCurvesLastFrame > numOfCurves)
                RemoveCurve();

            UpdatePoints();

            if (curves.Count != 0 && anchorPoints.Count != 0)
                PolynomialCoefficients();

            // updates for next frame
            numOfCurvesLastFrame = numOfCurves;
        }

        public void AddCurve()
        {
            // creates new bezier curve
            BezierCurve curve = new BezierCurve();
            curve.Initialise(numPointsInCurve);
            // adds new bezier curve to lists
            curves.Add(curve);
            anchorPoints.Add(curve.GetAnchorPoints());
        }

        public void RemoveCurve()
        {


            
                curves.Clear();
                anchorPoints.Clear();
                return;

            // removes bezier curve at end of lists
            curves.Remove(curves[curves.Count - 1]);
            anchorPoints.Remove(anchorPoints[anchorPoints.Count - 1]);
        }

        public void UpdatePoints()
        {
            // allows the anchor points to be moved from the inspector
            for (int i = 0; i < curves.Count; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    curves[i].SetPoint(j, anchorPoints[i].anchorPoints[j]);
                }
            }
        }

        public void Bernstein()
        {
            // more readable anchor points
            Vector3 p0 = curves[0].GetPoint(0);
            Vector3 p1 = curves[0].GetPoint(1);
            Vector3 p2 = curves[0].GetPoint(2);
            Vector3 p3 = curves[0].GetPoint(3);

            // split into 4 lines
            Vector3 con1 = p0 * (Mathf.Pow(-tValue, 3) + 3 * Mathf.Pow(tValue, 2) - 3 * tValue + 1);
            Vector3 con2 = p1 * (3 * Mathf.Pow(tValue, 3) - 6 * Mathf.Pow(tValue, 2) + 3 * tValue);
            Vector3 con3 = p2 * (-3 * Mathf.Pow(tValue, 3) + 3 * Mathf.Pow(tValue, 2));
            Vector3 con4 = p3 * Mathf.Pow(tValue, 3);

            trackingPoint = con1 + con2 + con3 + con4;
        }

        public void PolynomialCoefficients()
        {
            // more readable anchor points
            Vector3 p0 = curves[0].GetPoint(0);
            Vector3 p1 = curves[0].GetPoint(1);
            Vector3 p2 = curves[0].GetPoint(2);
            Vector3 p3 = curves[0].GetPoint(3);

            // split exquation into 3 separate lines
            Vector3 con1 = tValue * (-3 * p0 + 3 * p1);
            Vector3 con2 = tValue * tValue * (3 * p0 - 6 * p1 + 3 * p2);
            Vector3 con3 = tValue * tValue * tValue * (-p0 + 3 * p1 - 3 * p2 + p3);

            // add all together
            trackingPoint = p0 + con1 + con2 + con3;
        }

        public Vector3 V3Lerp(Vector3 a, Vector3 b, float t)
        {
            return a + (b - a) * t;
        }

        private void OnDrawGizmos()
        {
            if (showTrackingPoint)
            {
                // draws the tracking point
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(trackingPoint, pointSize);
            }

            if (showPoints && curves.Count != 0)
            {
                // draws the anchor points
                Gizmos.color = Color.blue;
                for (int i = 0; i < curves.Count; i++)
                {
                    for (int j = 0; j < curves[i].GetPoints().Count; j++)
                    {
                        Gizmos.DrawWireSphere(curves[i].GetPoint(j), pointSize);
                    }
                }
            }

            if (showLines && curves.Count != 0)
            {
                // draws the straight lines connecting the anchor points
                Gizmos.color = Color.white;
                for (int i = 0; i < curves.Count; i++)
                {
                    Gizmos.DrawLine(curves[i].GetPoint(0), curves[i].GetPoint(1));
                    Gizmos.DrawLine(curves[i].GetPoint(2), curves[i].GetPoint(3));
                }
            }
        }
    }
}