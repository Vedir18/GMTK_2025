using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private float startingSpeed;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float startingDeceleration;
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private float pointsToSpeedRatio;
    public int Score;
    private Transform tr;
    private float currentSpeed;
    private float currentDeceleration;
    private float currentRotateSpeed;

    private void Start()
    {
        tr = transform;
        currentSpeed = startingSpeed;
        currentDeceleration = startingDeceleration;
        currentRotateSpeed = rotateSpeed;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        int input = (int)Input.GetAxisRaw("Horizontal");
        tr.Rotate(0, 0, deltaTime * input * currentRotateSpeed);
        tr.position += tr.up * currentSpeed * deltaTime;
        ModifySpeed(currentDeceleration * deltaTime);
        mapManager.ProcessPlayerPos(tr.position);
        cameraManager.SetSize(currentSpeed);
    }

    public void AddPoints(int points)
    {
        Score += points;
        float percentIncrease = points * pointsToSpeedRatio / currentSpeed;
        ModifySpeed(percentIncrease);
    }

    private void ModifySpeed(float percentChange)
    {
        currentSpeed *= 1 + percentChange;
        currentRotateSpeed *= 1 + percentChange;
    }
}
