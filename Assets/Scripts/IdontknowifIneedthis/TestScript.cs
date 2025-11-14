using UnityEngine;

public class ListWebcams : MonoBehaviour
{
    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.Log("No webcam devices found!");
        }
        else
        {
            Debug.Log("=== Available Webcams ===");
            for (int i = 0; i < devices.Length; i++)
            {
                Debug.Log($"{i}: {devices[i].name}");
            }
        }
    }
}
