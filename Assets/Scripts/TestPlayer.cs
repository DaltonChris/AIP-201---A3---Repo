using UnityEngine;

// Renamed to Dalton_ as stefan used testPlayer to build from
public class TestPlayer : MonoBehaviour
{
    public float speed = 5f; // movementspeed
    public Camera PlayCamera; // players camera
    public Camera GridCamera; //camera to view entire grid

    private Camera currentCamera;

    void Start()
    {
        // Set the Player camera as the default camera
        currentCamera = PlayCamera;
        SetCameraActive(PlayCamera);
    }

    void Update()
    {
        // Movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0f).normalized;// basic input to movement vector

        transform.position += movement * speed * Time.deltaTime; // move

        // Camera switch
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            SwitchCamera();
        }
    }

    // method to switch between cameras (dogey implementation)
    void SwitchCamera()
    {
        if (currentCamera == PlayCamera)
        {
            SetCameraActive(GridCamera);
            currentCamera = GridCamera;
        }
        else
        {
            SetCameraActive(PlayCamera);
            currentCamera = PlayCamera;
        }
    }

    // used when switching cameras
    void SetCameraActive(Camera camera)
    {
        PlayCamera.enabled = false;
        GridCamera.enabled = false;

        camera.enabled = true;
    }
}
