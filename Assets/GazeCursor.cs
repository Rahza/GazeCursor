using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GazeCursor : MonoBehaviour {

    public GameObject cursorPrefab;

    public int samples = 10;
    public float threshold = 20.0f;

    private List<Vector2> points;
    private Vector2 lastPoint;

    private float accuracy = 0.0f;

    private GameObject cursor;

    private GazePointDataComponent gazePointDataComponent;

    // Use this for initialization
    void Start () {
        gazePointDataComponent = GetComponent<GazePointDataComponent>();
        points = new List<Vector2>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        var lastGazePoint = gazePointDataComponent.LastGazePoint;

        if (lastGazePoint.IsValid)
        {
            if (Vector2.Distance(lastPoint, lastGazePoint.Screen) > threshold)
            {
                // Different possibilities?
                points.Clear();
            }

            if (points.Count > samples)
            {
                points.RemoveAt(0);
            }

            lastPoint = lastGazePoint.Screen;
            points.Add(lastPoint);
        }

        /*
        Vector3 direction = Camera.main.ScreenPointToRay(Input.mousePosition).direction;

        if (lastGazePoint.IsValid)
        {
            Vector3 gazePointInScreenSpace = lastGazePoint.Screen;
            Vector3 gazePointInWorldSpace = Camera.main.ScreenToWorldPoint(new Vector3(gazePointInScreenSpace.x, gazePointInScreenSpace.y, -transform.position.x));
            direction = gazePointInWorldSpace - Camera.main.transform.position;
        }
        */

        DrawCursor();
    }

    private void DrawCursor()
    {
        if (cursor != null) Destroy(cursor);

        Vector2 position = new Vector2();

        // Different possibilities? 90% circle?
        foreach (Vector2 p in points)
        {
            position += p;
        }

        position /= points.Count;

        accuracy = CalculateAccuracy(position);

        Vector3 gazePointInWorldSpace = Camera.main.ScreenToWorldPoint(new Vector3(position.x, position.y, Camera.main.farClipPlane));
        Vector3 direction = gazePointInWorldSpace - Camera.main.transform.position;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, direction, out hit))
        {
            cursor = Instantiate(cursorPrefab, hit.point, Quaternion.identity) as GameObject;

            cursor.transform.localScale = Vector3.Lerp(cursor.transform.localScale, Vector3.one * accuracy, Time.deltaTime * 3.0f);
        }
    }

    private float CalculateAccuracy(Vector3 position)
    {
        float mean = 0;
        float variance = 0;

        foreach (Vector3 p in points)
        {
            mean += Vector3.Distance(position, p);
        }

        mean /= points.Count;

        foreach (Vector3 p in points)
        {
            variance += Mathf.Pow((Vector3.Distance(position, p) - mean), 2);
        }

        variance /= points.Count;

        return Mathf.Sqrt(variance);
    }

}
