using UnityEngine;

public class WebcamFeed : MonoBehaviour
{
    [Tooltip("The Renderer of the Quad (FPVScreen). We'll set its material.mainTexture at runtime.")]
    public Renderer targetRenderer;

    [Tooltip("Optional: force a specific camera name. Leave empty to auto-pick first camera.")]
    public string preferredDeviceName = "";

    // We'll store the webcam texture so we can stop it later if needed
    private WebCamTexture camTexture;

    void Start()
    {
        // Get list of all connected camera devices
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            Debug.LogError("No webcam devices found.");
            return;
        }

        // Pick device
        string deviceNameToUse = "";
        if (!string.IsNullOrEmpty(preferredDeviceName))
        {
            // Try to find a device that matches preferredDeviceName (partial match allowed)
            foreach (var d in devices)
            {
                if (d.name.Contains(preferredDeviceName))
                {
                    deviceNameToUse = d.name;
                    break;
                }
            }

            if (deviceNameToUse == "")
            {
                Debug.LogWarning("Preferred device not found. Falling back to first device.");
            }
        }

        if (deviceNameToUse == "")
        {
            // default: first camera Unity sees
            deviceNameToUse = devices[0].name;
        }

        Debug.Log("Using webcam: " + deviceNameToUse);

        // Create webcam texture
        // You can request a size here. Unity will try, similar to cap.set in OpenCV.
        int requestedWidth  = 1920;
        int requestedHeight = 540;  // remember: we only want the *top half* long-term, but for now grab full
        int requestedFPS    = 30;

        camTexture = new WebCamTexture(deviceNameToUse, requestedWidth, requestedHeight, requestedFPS);

        // Assign texture to the Quad's material so it shows up in world/VR
        if (targetRenderer != null)
        {
            targetRenderer.material.mainTexture = camTexture;
        }
        else
        {
            Debug.LogWarning("No targetRenderer assigned on WebcamFeed.");
        }

        // Start streaming
        camTexture.Play();
    }

    void OnDestroy()
    {
        if (camTexture != null && camTexture.isPlaying)
        {
            camTexture.Stop();
        }
    }
}
