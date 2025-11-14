using UnityEngine;

public class Insta360FeedSplit : MonoBehaviour
{
    [Header("Assign the mesh (Quad / Curved Mesh) you want to display on")]
    public Renderer targetRenderer;

    [Header("Optional: match part of the device name, e.g. 'Insta360'")]
    public string preferredDeviceName = "Insta360";

    [Header("Debug")]
    public bool showRearInstead = false; // toggle to test bottom half

    private WebCamTexture webcamTex;
    private Texture2D croppedTex;
    private Color32[] fullBuffer;
    private Color32[] cropBuffer;

    void Start()
    {
        // Pick device
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogError("No webcams found.");
            return;
        }

        string chosen = devices[0].name;
        foreach (var d in devices)
        {
            if (d.name.Contains(preferredDeviceName))
            {
                chosen = d.name;
                break;
            }
        }
        Debug.Log("Using webcam: " + chosen);

        // Start webcam
        // Request high res. Unity will negotiate what it can actually get.
        webcamTex = new WebCamTexture(chosen, 1920, 1080, 30);
        webcamTex.Play();

        // We don't know exact size until it starts playing, so we finish setup in LateUpdate once we know dimensions
    }

    void LateUpdate()
    {
        if (webcamTex == null || !webcamTex.isPlaying)
            return;

        int fullW = webcamTex.width;
        int fullH = webcamTex.height;

        if (fullW < 16 || fullH < 16)
        {
            // WebCamTexture sometimes reports 16x16 before first real frame arrives.
            return;
        }

        // Create buffers/textures once when sizes become valid
        if (croppedTex == null)
        {
            int halfH = fullH / 2;
            croppedTex = new Texture2D(fullW, halfH, TextureFormat.RGBA32, false);

            fullBuffer = new Color32[fullW * fullH];
            cropBuffer = new Color32[fullW * halfH];

            if (targetRenderer != null)
            {
                targetRenderer.material.mainTexture = croppedTex;
            }

            Debug.Log($"Initialized crop texture: {fullW}x{halfH} (full={fullW}x{fullH})");
        }

        // Get full frame pixels
        webcamTex.GetPixels32(fullBuffer);

        int halfHeight = fullH / 2;

        // Copy either top half or bottom half into cropBuffer
        // remember Unity texture origin is bottom-left
        // webcamTex pixel array is also bottom-up
        // So we need to pick correct rows.

        // We'll define:
        // - front lens = TOP half of the physical frame
        // but in pixel memory that might correspond to either upper rows or lower rows.
        // We'll assume:
        //   Top half visually = rows [halfHeight .. fullH-1]
        //   Bottom half visually = rows [0 .. halfHeight-1]
        //
        // If it's flipped for you, just flip showRearInstead logic or swap calculations.

        if (!showRearInstead)
        {
            // FRONT VIEW (top half of the actual video frame)
            // Copy rows halfHeight -> fullH into a 0->halfHeight buffer
            for (int y = 0; y < halfHeight; y++)
            {
                int srcY = y + halfHeight; // take from upper half of source
                // block copy one row
                System.Array.Copy(
                    fullBuffer,
                    srcY * fullW,
                    cropBuffer,
                    y * fullW,
                    fullW
                );
            }
        }
        else
        {
            // REAR VIEW (bottom half of the frame)
            // Copy rows 0 -> halfHeight straight into cropBuffer
            for (int y = 0; y < halfHeight; y++)
            {
                int srcY = y; // lower half of source
                System.Array.Copy(
                    fullBuffer,
                    srcY * fullW,
                    cropBuffer,
                    y * fullW,
                    fullW
                );
            }
        }

        // Push cropped pixels into the Texture2D
        croppedTex.SetPixels32(cropBuffer);
        croppedTex.Apply(false);
    }

    void OnDestroy()
    {
        if (webcamTex != null && webcamTex.isPlaying)
        {
            webcamTex.Stop();
        }
    }
}
