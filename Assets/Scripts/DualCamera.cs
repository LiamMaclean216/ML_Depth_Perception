using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualCamera : MonoBehaviour
{
    public Camera leftCam;
    public Camera rightCam;
    public Camera mainCam;

    List<Vector3> points = new List<Vector3>();
    List<Vector3> triangles = new List<Vector3>();

    public ScreenRecorder recorder;
    void Start()
    {

    }

    void Update()
    {
        points = new List<Vector3>();
        triangles = new List<Vector3>();

        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

        foreach (GameObject go in allObjects)
        {
        
            MeshFilter meshFilter = go.GetComponent<MeshFilter>();

            if (go.tag != "MainCamera" && meshFilter != null)
            {

                if (go.activeInHierarchy && go.GetComponent<MeshRenderer>().GetComponent<Renderer>().isVisible == true)
                {
                    Mesh mesh = meshFilter.mesh;
                    if (mesh.isReadable)
                    {
                        Vector3[] vertices = mesh.vertices;

                        Edge[] edges = BuildManifoldEdges(mesh);
                        for (int i = 0; i < edges.Length; i++)
                        {
                            Vector3 first = vertices[edges[i].vertexIndex[0]], second = vertices[edges[i].vertexIndex[1]];

                            Vector3 dir = (go.transform.TransformPoint(first) - mainCam.transform.position).normalized;
                            float distance = Vector3.Distance(go.transform.TransformPoint(first), mainCam.transform.position);

                            Vector3 dir2 = (go.transform.TransformPoint(second) - mainCam.transform.position).normalized;
                            float distance2 = Vector3.Distance(go.transform.TransformPoint(second), mainCam.transform.position);

                            if (!Physics.Raycast(mainCam.transform.position, dir, distance - 0.5f)
                            || !Physics.Raycast(mainCam.transform.position, dir2, distance2 - 0.5f))
                            {
                                first = mainCam.WorldToScreenPoint(go.transform.TransformPoint(first));
                                second = mainCam.WorldToScreenPoint(go.transform.TransformPoint(second));

                                bool firstInScreen = (first.x > 0 && first.x < Screen.width && first.y > 0 && first.y < Screen.height);
                                bool secondInScreen = (second.x > 0 && first.x < Screen.width && second.y > 0 && second.y < Screen.height);

                                

                                if (firstInScreen || secondInScreen)
                                {
                                    /*if (first.x <= 0) first = Vector3.MoveTowards(second, first, Vector3.Distance(second, new Vector3(second.x, 0, second.z)));
                                    if (first.x >= Screen.width) first = Vector3.MoveTowards(second, first, Vector3.Distance(second, new Vector3(second.x, Screen.width, second.z)));
                                    if (first.y <= 0) first = Vector3.MoveTowards(second, first, Vector3.Distance(second, new Vector3(second.x, 0, second.z)));
                                    if (first.y >= Screen.height) first = Vector3.MoveTowards(second, first, Vector3.Distance(second, new Vector3(second.x, Screen.height, second.z)));

                                    if (second.x <= 0) second = Vector3.MoveTowards(first, second, Vector3.Distance(second, new Vector3(second.x, 0, second.z)));
                                    if (second.x >= Screen.width) second = Vector3.MoveTowards(first, second, Vector3.Distance(second, new Vector3(second.x, Screen.width, second.z)));
                                    if (second.y <= 0) second = Vector3.MoveTowards(first, second, Vector3.Distance(second, new Vector3(second.x, 0, second.z)));
                                    if (second.y >= Screen.height) second = Vector3.MoveTowards(first, second, Vector3.Distance(second, new Vector3(second.x, Screen.height, second.z)));*/

                                    triangles.Add(first);
                                    triangles.Add(second);
                                }
                            }
                        }

                       /* for (int i = 0; i < vertices.Length; i++)
                        {
                            RaycastHit[] hits;
                            Vector3 dir = (go.transform.TransformPoint(vertices[i]) - cam1.transform.position).normalized;
                            float distance = Vector3.Distance(go.transform.TransformPoint(vertices[i]), cam1.transform.position);
                            hits = Physics.RaycastAll(cam1.transform.position, dir, distance - 0.05f);


                            if (hits.Length < 2)
                            {
                                if (hits.Length != 0)
                                {
                                    if (Vector3.Distance(transform.position, hits[0].point) + 0.01 > Vector3.Distance(transform.position, go.transform.TransformPoint(vertices[i])))
                                    {
                                        Debug.DrawLine(cam1.transform.position, hits[0].point, Color.red, Mathf.Infinity, false);
                                        points.Add(cam1.WorldToScreenPoint(go.transform.TransformPoint(vertices[i])));

                                    }
                                }
                                else
                                {
                                    points.Add(cam1.WorldToScreenPoint(go.transform.TransformPoint(vertices[i])));

                                }
                            }
                            if (hits.Length == 0)
                            {
                                points.Add(cam1.WorldToScreenPoint(go.transform.TransformPoint(vertices[i])));
                                // Debug.DrawLine(cam1.transform.position, go.transform.TransformPoint(vertices[i]), Color.red, Mathf.Infinity, false);
                            }
                        }*/


                        
                    }
                }
            }
        }
    }

    void OnGUI()
    {
        /* for (int i = 0; i < points.Count; i++)
         {
             float z = 10;
             float x = points[i].x - 5, y = -points[i].y + Screen.height - 5;

             if (x > 0 && x < Screen.width && y > 0 && y < Screen.height)
                 GUI.Box(new Rect(x, y, z, z), "This is a box");

         }*/
       for (int i = 0; i < triangles.Count; i += 2)
        {
            if (triangles[i].z > 0 && triangles[i + 1].z > 0)
            {
                Vector2 first = new Vector2(triangles[i].x, triangles[i].y);
                Vector2 second = new Vector2(triangles[i + 1].x, triangles[i + 1].y);
                DrawLine(first, second, new Color(1/triangles[i].z, 10 / triangles[i].z, 10 / triangles[i].z,10));
            }
        }
    }

    void DrawLine(Vector2 pointA, Vector2 pointB, Color color)
    {
    
        pointA.x = (int)pointA.x;
        pointB.x = (int)pointB.x;

        pointA.y = Screen.height - (int)pointA.y;
        pointB.y = Screen.height - (int)pointB.y;

        Texture2D lineTex = new Texture2D(1, 1);
        Matrix4x4 matrixBackup = GUI.matrix;
        float width = 2.0f;
        GUI.color = color;
        float angle = Mathf.Atan2(pointB.y - pointA.y, pointB.x - pointA.x) * 180f / Mathf.PI;
        GUIUtility.ScaleAroundPivot(new Vector2((pointB - pointA).magnitude, width), new Vector2(pointA.x, pointA.y + 0.5f));

        GUIUtility.RotateAroundPivot(angle, pointA);
        GUI.DrawTexture(new Rect(pointA.x, pointA.y, 1, 1), lineTex);
        GUI.matrix = matrixBackup;
        Destroy(lineTex);
    }

    //https://answers.unity.com/questions/443633/finding-the-vertices-of-each-edge-on-mesh.html
    public static Edge[] BuildManifoldEdges(Mesh mesh)
    {
        // Build a edge list for all unique edges in the mesh
        Edge[] edges = BuildEdges(mesh.vertexCount, mesh.triangles);

        // We only want edges that connect to a single triangle
        ArrayList culledEdges = new ArrayList();
        foreach (Edge edge in edges)
        {
            if (edge.faceIndex[0] == edge.faceIndex[1])
            {
                culledEdges.Add(edge);
            }
        }

        return culledEdges.ToArray(typeof(Edge)) as Edge[];
    }

    public static Edge[] BuildEdges(int vertexCount, int[] triangleArray)
    {
        int maxEdgeCount = triangleArray.Length;
        int[] firstEdge = new int[vertexCount + maxEdgeCount];
        int nextEdge = vertexCount;
        int triangleCount = triangleArray.Length / 3;

        for (int a = 0; a < vertexCount; a++)
            firstEdge[a] = -1;

        // First pass over all triangles. This finds all the edges satisfying the
        // condition that the first vertex index is less than the second vertex index
        // when the direction from the first vertex to the second vertex represents
        // a counterclockwise winding around the triangle to which the edge belongs.
        // For each edge found, the edge index is stored in a linked list of edges
        // belonging to the lower-numbered vertex index i. This allows us to quickly
        // find an edge in the second pass whose higher-numbered vertex index is i.
        Edge[] edgeArray = new Edge[maxEdgeCount];

        int edgeCount = 0;
        for (int a = 0; a < triangleCount; a++)
        {
            int i1 = triangleArray[a * 3 + 2];
            for (int b = 0; b < 3; b++)
            {
                int i2 = triangleArray[a * 3 + b];
                if (i1 < i2)
                {
                    Edge newEdge = new Edge();
                    newEdge.vertexIndex[0] = i1;
                    newEdge.vertexIndex[1] = i2;
                    newEdge.faceIndex[0] = a;
                    newEdge.faceIndex[1] = a;
                    edgeArray[edgeCount] = newEdge;

                    int edgeIndex = firstEdge[i1];
                    if (edgeIndex == -1)
                    {
                        firstEdge[i1] = edgeCount;
                    }
                    else
                    {
                        while (true)
                        {
                            int index = firstEdge[nextEdge + edgeIndex];
                            if (index == -1)
                            {
                                firstEdge[nextEdge + edgeIndex] = edgeCount;
                                break;
                            }

                            edgeIndex = index;
                        }
                    }

                    firstEdge[nextEdge + edgeCount] = -1;
                    edgeCount++;
                }

                i1 = i2;
            }
        }

        // Second pass over all triangles. This finds all the edges satisfying the
        // condition that the first vertex index is greater than the second vertex index
        // when the direction from the first vertex to the second vertex represents
        // a counterclockwise winding around the triangle to which the edge belongs.
        // For each of these edges, the same edge should have already been found in
        // the first pass for a different triangle. Of course we might have edges with only one triangle
        // in that case we just add the edge here
        // So we search the list of edges
        // for the higher-numbered vertex index for the matching edge and fill in the
        // second triangle index. The maximum number of comparisons in this search for
        // any vertex is the number of edges having that vertex as an endpoint.

        for (int a = 0; a < triangleCount; a++)
        {
            int i1 = triangleArray[a * 3 + 2];
            for (int b = 0; b < 3; b++)
            {
                int i2 = triangleArray[a * 3 + b];
                if (i1 > i2)
                {
                    bool foundEdge = false;
                    for (int edgeIndex = firstEdge[i2]; edgeIndex != -1; edgeIndex = firstEdge[nextEdge + edgeIndex])
                    {
                        Edge edge = edgeArray[edgeIndex];
                        if ((edge.vertexIndex[1] == i1) && (edge.faceIndex[0] == edge.faceIndex[1]))
                        {
                            edgeArray[edgeIndex].faceIndex[1] = a;
                            foundEdge = true;
                            break;
                        }
                    }

                    if (!foundEdge)
                    {
                        Edge newEdge = new Edge();
                        newEdge.vertexIndex[0] = i1;
                        newEdge.vertexIndex[1] = i2;
                        newEdge.faceIndex[0] = a;
                        newEdge.faceIndex[1] = a;
                        edgeArray[edgeCount] = newEdge;
                        edgeCount++;
                    }
                }

                i1 = i2;
            }
        }

        Edge[] compactedEdges = new Edge[edgeCount];
        for (int e = 0; e < edgeCount; e++)
            compactedEdges[e] = edgeArray[e];

        return compactedEdges;
    }
}

public class Edge
{
    // The indiex to each vertex
    public int[] vertexIndex = new int[2];
    // The index into the face.
    // (faceindex[0] == faceindex[1] means the edge connects to only one triangle)
    public int[] faceIndex = new int[2];
}




