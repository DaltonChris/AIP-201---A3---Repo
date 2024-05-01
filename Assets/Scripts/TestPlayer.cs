using UnityEngine;

public class TestPlayer : MonoBehaviour
{
    public float speed = 5f;
    public Camera PlayCamera;
    public Camera GridCamera;

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

        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0f).normalized;

        transform.position += movement * speed * Time.deltaTime;

        // Camera switch
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            SwitchCamera();
        }
    }

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

    void SetCameraActive(Camera camera)
    {
        PlayCamera.enabled = false;
        GridCamera.enabled = false;

        camera.enabled = true;
    }
}
