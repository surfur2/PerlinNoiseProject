using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    float movementSpeed = 20.0f;
    float rotationSpeed = 35.0f;

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            var newRotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x + Input.GetAxisRaw("Mouse Y") * Time.deltaTime * -rotationSpeed,
                  transform.rotation.eulerAngles.y + Input.GetAxisRaw("Mouse X") * Time.deltaTime * rotationSpeed,
                  0.0f
            ));

            transform.rotation = newRotation;
        }

        var finalPosition = transform.position;
        if (Input.GetKey("a"))
        {
            finalPosition += transform.right * Time.deltaTime * -movementSpeed;
        }
        if (Input.GetKey("d"))
        {
            finalPosition += transform.right * Time.deltaTime * movementSpeed;
        }
        if (Input.GetKey("w"))
        {
            finalPosition += transform.forward * Time.deltaTime * movementSpeed;
        }
        if (Input.GetKey("s"))
        {
            finalPosition += transform.forward * Time.deltaTime * -movementSpeed;
        }

        transform.position = finalPosition;
    }
}
