using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float zoomSpeed;

    private float targetZoom;

    // Start is called before the first frame update
    void Start()
    {
        targetZoom = Camera.main.orthographicSize * 2.5f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.identity;

        targetZoom -= zoomSpeed * Input.GetAxis("Mouse ScrollWheel");
        Camera.main.orthographicSize = Mathf.MoveTowards(Camera.main.orthographicSize, Mathf.Clamp(targetZoom, 5, 50), 10*Time.deltaTime);
    }
}
