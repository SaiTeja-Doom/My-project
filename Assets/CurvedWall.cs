using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class CurvedWall : MonoBehaviour
{
    public float radius = 5f;  // Radius of the curve
    public float height = 2f;  // Height of the wall
    public float angle = 90f;  // Angle of the curve (in degrees)
    public int segments = 10;  // Number of segments to make up the curve
    public float thickness = 0.5f;  // Thickness of the wall

    void Start()
    {
        GenerateWall();
    }

    void GenerateWall()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        Mesh mesh = new Mesh();

        // Double the number of vertices for inner and outer walls
        Vector3[] vertices = new Vector3[4 * (segments + 1)];
        int[] triangles = new int[12 * segments + 6 * segments];  // Add more triangles for caps (side faces)

        float segmentAngle = angle / segments * Mathf.Deg2Rad;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = i * segmentAngle;
            float xOuter = Mathf.Cos(currentAngle) * radius;
            float zOuter = Mathf.Sin(currentAngle) * radius;
            float xInner = Mathf.Cos(currentAngle) * (radius - thickness);
            float zInner = Mathf.Sin(currentAngle) * (radius - thickness);

            // Outer wall vertices
            vertices[i] = new Vector3(xOuter, 0, zOuter); // Bottom outer
            vertices[i + segments + 1] = new Vector3(xOuter, height, zOuter); // Top outer

            // Inner wall vertices
            vertices[i + 2 * (segments + 1)] = new Vector3(xInner, 0, zInner); // Bottom inner
            vertices[i + 3 * (segments + 1)] = new Vector3(xInner, height, zInner); // Top inner
        }

        for (int i = 0; i < segments; i++)
        {
            // Outer wall (front)
            triangles[12 * i] = i;
            triangles[12 * i + 1] = i + segments + 1;
            triangles[12 * i + 2] = i + 1;

            triangles[12 * i + 3] = i + 1;
            triangles[12 * i + 4] = i + segments + 1;
            triangles[12 * i + 5] = i + segments + 2;

            // Inner wall (back)
            triangles[12 * i + 6] = i + 2 * (segments + 1);
            triangles[12 * i + 7] = i + 3 * (segments + 1);
            triangles[12 * i + 8] = i + 2 * (segments + 1) + 1;

            triangles[12 * i + 9] = i + 2 * (segments + 1) + 1;
            triangles[12 * i + 10] = i + 3 * (segments + 1);
            triangles[12 * i + 11] = i + 3 * (segments + 1) + 1;
        }

        // Cap the sides (connect inner and outer walls)
        for (int i = 0; i < segments; i++)
        {
            // Connect front face
            triangles[12 * segments + 6 * i] = i; // Outer bottom
            triangles[12 * segments + 6 * i + 1] = i + segments + 1; // Outer top
            triangles[12 * segments + 6 * i + 2] = i + 2 * (segments + 1); // Inner bottom

            triangles[12 * segments + 6 * i + 3] = i + segments + 1; // Outer top
            triangles[12 * segments + 6 * i + 4] = i + 3 * (segments + 1); // Inner top
            triangles[12 * segments + 6 * i + 5] = i + 2 * (segments + 1); // Inner bottom
        }

        // Assign vertices and triangles
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

        // Assign the mesh to the collider
        meshCollider.sharedMesh = mesh;
    }
}
