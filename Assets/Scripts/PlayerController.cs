using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private List<GameObject> engines;
    private List<GameObject> controls;
    private List<GameObject> guns;
    private GameObject command; 

    public float accScaleFactor; // how strongly each engine pushes the ship forward
    public float turnScaleFactor; // how strongly each control cell turns the ship
    public float recoilStrength; // how strongly the guns recoil when shot
    public float shotSpeed; // how strong the initial force on a shot is
    public GameObject Shot; // which prefab shot to use
    public float shotTime; // how many frames to wait between each shot

    private GameObject selectedGO; // which cell the player has selected
    private GameObject emptyGO; // switch to an empty one when no selecting anything

    private float counter;
    private float shotTimer;
    

    // Start is called before the first frame update
    void Start()
    {
        // find the subservient cells
        UpdateSublists();
        foreach (Transform child in transform) { if (child.tag == "Command") command = child.gameObject; }

        emptyGO = new GameObject("empty go");
        selectedGO = emptyGO;

        counter = 0;
        shotTimer = 0;
    }
    
    // Update is called once per frame
    void Update()
    {
        // collect inputs
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        // don't care about negative y values
        if (y < 0) y = 0; 

        float fire = Input.GetAxis("Jump");

        //float slow = Input.GetAxis("Fire2");

        // if the player is holding down w, fire all engines
        foreach (GameObject engine in engines)
        {
            Rigidbody2D rb = engine.GetComponent<Rigidbody2D>();
            rb.AddForce(rotate(new Vector2(0, y * accScaleFactor), rb.rotation));
        }

        // if the player is turning, fire all controls
        foreach (GameObject control in controls)
        {
            Rigidbody2D rb = control.GetComponent<Rigidbody2D>();
            rb.AddForce(rotate(new Vector2(x * turnScaleFactor, 0), rb.rotation));
        }

        // if the player is shooting, fire the guns
        if (fire > 0 && shotTimer > shotTime)
        {
            shotTimer = 0;
            foreach (GameObject gun in guns)
            {
                // recoil the gun
                Rigidbody2D rb = gun.GetComponent<Rigidbody2D>();
                rb.AddForce(rotate(new Vector2(0, -recoilStrength * fire), rb.rotation));

                // shoot the shot
                var shot = Instantiate(Shot, rb.position + rotate(new Vector2(0, 1.28f), rb.rotation), gun.transform.rotation) as GameObject;
                shot.GetComponent<Rigidbody2D>().AddForce(rotate(new Vector2(0, shotSpeed * 20), rb.rotation));
            }
        }

        // check if the player has clicked on a cell
        if (Input.GetMouseButtonDown(0))
        {
            // raycast to find the cell
            //Ray2D ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            ContactFilter2D cf2d = new ContactFilter2D(); cf2d.maxDepth = -5; cf2d.minDepth = 5;
            RaycastHit2D[] hit = new RaycastHit2D[16];
            if (Physics2D.Raycast(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
                                              Camera.main.ScreenToWorldPoint(Input.mousePosition).y),
                                 Vector2.zero,
                                 cf2d, hit)
                > 0)
            {
                if (hit != null && (
                    hit[0].transform.CompareTag("Control") || 
                    hit[0].transform.CompareTag("Engine") || 
                    hit[0].transform.CompareTag("Gun") || 
                    hit[0].transform.CompareTag("Structure")))
                {
                    // remove its joints if it has any on the player
                    if (hit[0].transform.parent == null)
                    {
                        // set it to be selected
                        hit[0].transform.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                        hit[0].transform.gameObject.BroadcastMessage("SetSelected", true);
                        selectedGO = hit[0].transform.gameObject;
                    }
                    else if (hit[0].transform.parent.tag != "Enemy")
                    {
                        foreach (RelativeJoint2D joint in hit[0].transform.GetComponents<RelativeJoint2D>())
                        {
                            joint.connectedBody = joint.attachedRigidbody;
                        }
                        // remove others' joints if they are connected to it
                        if (hit[0].transform.parent && !hit[0].transform.parent.name.Equals("Debris"))
                        {
                            foreach (Transform child in hit[0].transform.parent)
                            {
                                foreach (RelativeJoint2D joint in child.GetComponents<RelativeJoint2D>())
                                {
                                    if (joint.connectedBody.Equals(hit[0].transform.GetComponent<Rigidbody2D>()))
                                    {
                                        joint.connectedBody = joint.attachedRigidbody;
                                    }
                                }
                            }
                        }

                        // set it to be selected
                        hit[0].transform.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                        hit[0].transform.gameObject.BroadcastMessage("SetSelected", true);
                        selectedGO = hit[0].transform.gameObject;
                    }

                }
            }
        }
        // check if the player has released the cell from their click
        if (Input.GetMouseButtonUp(0))
        {
            if (selectedGO != emptyGO)
            {
                selectedGO.BroadcastMessage("SetSelected", false);
                selectedGO.BroadcastMessage("ReleaseSelected");
                selectedGO = emptyGO;
            }
        }

        counter += Time.deltaTime;
        shotTimer += Time.deltaTime;
    }

    private Vector2 rotate(Vector2 v, float angle)
    {
        angle *= (float)Math.PI / 180;
        return new Vector2(v.x * (float)Math.Cos(angle) - v.y * (float)Math.Sin(angle),
                           v.x * (float)Math.Sin(angle) + v.y * (float)Math.Cos(angle));
    }

    void UpdateSublists()
    {
        engines = new List<GameObject>();
        controls = new List<GameObject>();
        guns = new List<GameObject>();
        foreach (Transform child in transform)
        {
            UpdateSublist(child.tag, child.gameObject); 
        }
    }

    void UpdateSublist(String name, GameObject go)
    {
        switch(name)
        {
            case "Engine": engines.Add(go); break;
            case "Control": controls.Add(go); break;
            case "Gun": guns.Add(go); break;
        }
    }
}
