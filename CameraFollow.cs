using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 15f;
    [SerializeField] private float height = 4f;
    [SerializeField] private float rotationDamping = 2f;
    [SerializeField] private float heightDamping = 10f;
    [SerializeField] private float pitchAngle = 50f;
    [SerializeField] private bool followFromFront = false;


    private void Start()
    {
        GameObject carAgent = GameObject.Find("CarAgent");
        if (carAgent != null)
        {
            target = carAgent.transform;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        float wantedRotationAngle = target.eulerAngles.y;
        float wantedHeight = target.position.y + height;
        float currentRotationAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;

        if (followFromFront)
        {
            pitchAngle = -50f; // look upwards towards the front of the car
            wantedRotationAngle += 180f; // flip the rotation to look at the car from the front
        }
        else
        {
            pitchAngle = 50f; // look downwards towards the back of the car
        }

        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

        Quaternion currentRotation = Quaternion.Euler(pitchAngle, currentRotationAngle, 0);

        transform.position = target.position;
        transform.position -= currentRotation * Vector3.forward * distance;
        transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

        transform.LookAt(target);
    }
}
