using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float followSpeed;
    [SerializeField] private float minSize, maxSize;
    [SerializeField] private float minSpeed, maxSpeed;
    [SerializeField] private Camera m_camera;
    private Transform tr;

    private void Start()
    {
        tr = transform;
    }

    private void Update()
    {
        Vector3 targetPos = target.position + offset;
        tr.position = Vector3.MoveTowards(tr.position, targetPos, followSpeed*Time.deltaTime);
    }

    public void SetSize(float speed)
    {
        m_camera.orthographicSize = Mathf.Lerp(minSize, maxSize, Mathf.InverseLerp(minSpeed, maxSpeed, speed));
    }
}
