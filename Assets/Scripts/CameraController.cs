using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
  public float movementSpeed = 10f; // Adjust this value to change movement speed
  public float zoomSensitivity = 10f; // Adjust this value to change zoom speed
  public float minZoom = 1f; // Define minimum zoom distance (camera gets closer)
  public float maxZoom = 10f; // Define maximum zoom distance (camera gets farther)
  public float minY, maxY; // Define camera boundary values
  public float minZ = -12f;
  public float maxZ = 4f;
  public float minX = -8f;
  public float maxX = 8f;

  //rotation control
  private float x;
  private float y;
  public float sensitivity = -3f;
  private Vector3 rotate;

  void Update()
  {
      float horizontalInput = Input.GetAxis("Horizontal");
      float verticalInput = Input.GetAxis("Vertical");
      float zoomInput = Input.GetAxis("Mouse ScrollWheel");

      Vector3 forwardMovement = transform.forward * verticalInput * movementSpeed * Time.deltaTime;
      Vector3 rightMovement = transform.right * horizontalInput * movementSpeed * Time.deltaTime;

      Vector3 projectedForwardMovement = Vector3.ProjectOnPlane(forwardMovement, Vector3.up);
      Vector3 projectedRightMovement = Vector3.ProjectOnPlane(rightMovement, Vector3.up);

      transform.position += projectedForwardMovement + projectedRightMovement;

      float newYPosition = Mathf.Clamp(transform.localPosition.y - zoomInput * zoomSensitivity, minZoom, maxZoom);
      float newZPosition = Mathf.Clamp(transform.localPosition.z - zoomInput * zoomSensitivity, minZ, maxZ);
      transform.localPosition = new Vector3(transform.localPosition.x, newYPosition, newZPosition);

    //rotation control
    if (Input.GetMouseButton(1))
    {
      y = Input.GetAxis("Mouse X");
      x = Input.GetAxis("Mouse Y");
      if (x > maxX)
      {
        x = maxX;
      }
      else if (x < minX)
      {
        x = minX;
      }
      rotate = new Vector3(x, y * sensitivity, 0);
      transform.eulerAngles = transform.eulerAngles - rotate;
    }
  }
}