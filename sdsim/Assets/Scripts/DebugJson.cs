using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DebugJson : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
        AddLocationMarkers(json);
        Debug.LogError(json.ToString());
    }

    // Update is called once per frame
    void Update()
    {

    }

    void AddLocationMarkers(JSONObject json)
    {
        LocationMarker[] locationMarkers = GameObject.FindObjectsOfType<LocationMarker>();
        if (locationMarkers.Length > 0)
        {
            // Order location markers by id
            locationMarkers = locationMarkers.OrderBy(marker => marker.id).ToArray();

            // Create a JSONObject for positions
            JSONObject positionsJson = new JSONObject(JSONObject.Type.ARRAY);

            foreach (LocationMarker marker in locationMarkers)
            {
                Vector3 coordinates = marker.transform.position / 8.0f;

                // Create a JSONArray for current position
                JSONObject positionJson = new JSONObject(JSONObject.Type.ARRAY);
                positionJson.Add(coordinates.x.ToString("F2", System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                positionJson.Add(coordinates.y.ToString("F2", System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                positionJson.Add(coordinates.z.ToString("F2", System.Globalization.CultureInfo.InvariantCulture.NumberFormat));

                // Add current position to positionsJson
                positionsJson.Add(positionJson);
            }

            // Add positionsJson to the main json object
            json.AddField("LocationMarker", positionsJson);
            Debug.Log("Location markers added");
        }
        else
        {
            Debug.LogWarning("No location markers found.");
        }
    }

    // void AddLocationMarkers(JSONObject json)
    // {
    //     LocationMarker[] locationMarkers = GameObject.FindObjectsOfType<LocationMarker>();
    //     if (locationMarkers.Length > 0)
    //     {
    //         // Order location markers by id
    //         locationMarkers = locationMarkers.OrderBy(marker => marker.id).ToArray();

    //         List<List<float>> positions = new List<List<float>>();

    //         foreach (LocationMarker marker in locationMarkers)
    //         {
    //             Vector3 coordinates = marker.transform.position / 8.0f;
    //             List<float> position = new List<float>() { coordinates.x, coordinates.y, coordinates.z };
    //             positions.Add(position);
    //         }

    //         // Create a JSONObject and add positions to it
    //         JSONObject positionsJson = new JSONObject(JSONObject.Type.ARRAY);
    //         foreach (List<float> position in positions)
    //         {
    //             JSONObject positionJson = new JSONObject(JSONObject.Type.ARRAY);
    //             foreach (float coord in position)
    //             {
    //                 positionJson.Add(coord);
    //             }
    //             positionsJson.Add(positionJson);
    //         }

    //         // Add positionsJson to the main json object
    //         json.AddField("locationmarker", positionsJson);
    //         Debug.LogError(positionsJson.ToString());
    //         Debug.Log("Location markers added");
    //     }
    //     else
    //     {
    //         Debug.LogWarning("No location markers found.");
    //     }
    // }

}



