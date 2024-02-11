using PathCreation;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CreatePlaneFromPath : MonoBehaviour {
    public PathCreator pathCreator;
    public int samples = 100; // Adjust this based on the level of detail you need
    private MeshFilter meshFilter;

    void Start() {
        meshFilter = GetComponent<MeshFilter>();

        if (pathCreator != null) {
            CreatePlaneAlongPath();
        }
    }

    void CreatePlaneAlongPath() {
        VertexPath path = pathCreator.path;
        int vertexCount = samples * 2; // For a plane, you need two points for each sample
        Vector3[] vertices = new Vector3[vertexCount];

        for (int i = 0; i < samples; i++) {
            float percentage = i / (float)(samples - 1); // Use (samples - 1) to get accurate percentage

            Vector3 pathPoint = path.GetPointAtTime(percentage, EndOfPathInstruction.Stop);
            Vector3 nextPercentagePoint = path.GetPointAtTime(Mathf.Min((i + 1) / (float)(samples - 1), 1f), EndOfPathInstruction.Stop);

            Vector3 pathTangent = (nextPercentagePoint - pathPoint).normalized;
            Vector3 pathNormal = Vector3.Cross(Vector3.up, pathTangent).normalized;

            float planeWidth = 1.0f; // Half width of the plane

            // Calculate vertices above and below the path to form the plane
            vertices[i * 2] = pathPoint - Vector3.Cross(pathNormal, pathTangent).normalized * planeWidth;
            vertices[i * 2 + 1] = pathPoint + Vector3.Cross(pathNormal, pathTangent).normalized * planeWidth;
        }

        // Rest of the mesh creation and assignment remains unchanged
        int[] triangles = new int[(samples - 1) * 6];
        // Create a new mesh and assign the vertices and triangles
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        meshFilter.mesh = mesh;
    }
}
