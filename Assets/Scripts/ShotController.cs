using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotController : MonoBehaviour
{
    public float shotStrength;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        collision.attachedRigidbody.AddForce(new Vector2(
            -20 * shotStrength * Mathf.Sin(transform.eulerAngles.z * Mathf.PI / 180),
            20 * shotStrength * Mathf.Cos(transform.eulerAngles.z * Mathf.PI / 180)
            ));

        gameObject.SetActive(false);
    }
}