using UnityEngine;

public class RotatingController : MonoBehaviour
{
    public float rotationSpeed = 90f;
    public bool isAnimating = true;

    private float horizontalRotation = 0f;
    private float verticalRotation = 0f;

    void Update()
    {
        if (!isAnimating) return;

        float inputHorizontal = 0f;
        float inputVertical = 0f;

        if (Input.GetKey(KeyCode.LeftArrow)) inputHorizontal -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) inputHorizontal += 1f;
        if (Input.GetKey(KeyCode.UpArrow)) inputVertical -= 1f;
        if (Input.GetKey(KeyCode.DownArrow)) inputVertical += 1f;

        horizontalRotation += inputHorizontal * rotationSpeed * Time.deltaTime;
        verticalRotation += inputVertical * rotationSpeed * Time.deltaTime;

        verticalRotation = Mathf.Clamp(verticalRotation, -89f, 89f);

        transform.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
    }

    void OnGUI()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            isAnimating = false;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            isAnimating = true;
            horizontalRotation = 0f;
            verticalRotation = 0f;
            transform.rotation = Quaternion.identity;
        }
    }
}