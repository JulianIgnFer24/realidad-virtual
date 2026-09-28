using UnityEngine;

public class RotationController : MonoBehaviour
{
    public float rotationSpeed = 1.2f;
    public bool isAnimating = true;

    private float angleY = 0f;
    private float angleX = 0f;
    private int activeAxis = 0; // 0 = vertical (Y), 1 = horizontal (X)
    private int direction = 1;  // +1 or -1

    public Transform figureRoot;
    private Quaternion initialRotation;

    void Start()
    {
        if (figureRoot == null)
            figureRoot = GameObject.Find("Root")?.transform;

        initialRotation = figureRoot.rotation;

        // Start with vertical axis, positive direction
        activeAxis = 0;
        direction = 1;
    }

    void Update()
    {
        HandleInput();

        if (!isAnimating) return;

        // Accumulate rotation on active axis
        if (activeAxis == 0)
            angleY += direction * rotationSpeed * Time.deltaTime;
        else
            angleX += direction * rotationSpeed * Time.deltaTime;

        angleX = Mathf.Clamp(angleX, -Mathf.PI / 2 + 0.1f, Mathf.PI / 2 - 0.1f);

        if (figureRoot != null)
        {
            // Compose: initial * additional
            var additional = Quaternion.Euler(angleX * Mathf.Rad2Deg, angleY * Mathf.Rad2Deg, 0f);
            figureRoot.rotation = initialRotation * additional;
        }
    }

    void HandleInput()
    {
        // Arrow keys: set axis and direction, resume if stopped
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            activeAxis = 0; direction = -1; isAnimating = true;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            activeAxis = 0; direction = +1; isAnimating = true;
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            activeAxis = 1; direction = -1; isAnimating = true;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            activeAxis = 1; direction = +1; isAnimating = true;
        }

        // D: stop animation (freeze)
        if (Input.GetKeyDown(KeyCode.D))
            isAnimating = false;

        // R: reset position and restart
        if (Input.GetKeyDown(KeyCode.R))
        {
            angleX = 0f;
            angleY = 0f;
            activeAxis = 0;
            direction = 1;
            isAnimating = true;
            if (figureRoot != null)
                figureRoot.rotation = initialRotation;
        }
    }

    public string GetStatus()
    {
        string axisName = (activeAxis == 0) ? "VERTICAL" : "HORIZONTAL";
        string dirName = (direction > 0) ? "+" : "-";
        string stateName = isAnimating ? "en movimiento" : "detenida";
        return $"Eje: {axisName}   Sentido: {dirName}   Estado: {stateName}";
    }
}