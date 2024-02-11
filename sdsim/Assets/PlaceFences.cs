using UnityEngine;

public class PlaceFences : MonoBehaviour
{
    public GameObject fencePrefab;
    public int numberOfFences = 100;

    void Start()
    {
        PlaceFencesAlongRoad();
    }


    void PlaceFencesAlongRoad()
    {
        Debug.LogError("entering place fences along road");
        Transform roadTransform = transform.Find("Roads"); // Change "Road" to the actual name of your road GameObject
        if (roadTransform == null)
        {
            Debug.LogError("Make sure the road GameObject is a child of the current GameObject.");
            return;
        }
        Debug.LogError("Found road: " + roadTransform.name);

        // Find the "road_0001" GameObject within "Road Objects"
        Transform roadObject = roadTransform.Find("road_0001");

        if (roadObject == null)
        {
            Debug.LogError("Road not found! Make sure the road_0001 GameObject is a child of the Road Objects GameObject.");
            return;
        }
        Debug.LogError("Found road: " + roadObject.name);


        // Determine the length of the road
        float roadLength = CalculateRoadLength(roadObject);

        // Calculate positions along the road and instantiate fences
        for (int i = 0; i < numberOfFences; i++)
        {
            float t = i / (float)(numberOfFences - 1);
            Vector3 position = CalculatePositionAlongRoad(roadObject, roadLength, t);

            // Instantiate the fence prefab at the calculated position
            GameObject fence = Instantiate(fencePrefab, position, Quaternion.identity);
            
            // Check if fence was created successfully
            if (fence != null)
            {
                Debug.Log("Fence created at position: " + position);
            }
            else
            {
                Debug.LogError("Failed to create fence at position: " + position);
            }
        }
    }

    float CalculateRoadLength(Transform roadTransform)
    {
        // Add your logic to calculate the length of the road
        // You might need to traverse the road and sum up the distances between points

        return 0f; // Placeholder, replace with actual calculation
    }

    Vector3 CalculatePositionAlongRoad(Transform roadTransform, float roadLength, float t)
    {
        // Add your logic to calculate the position along the road based on t
        // You might use t to interpolate between points along the road

        return roadTransform.position; // Placeholder, replace with actual calculation
    }
}
