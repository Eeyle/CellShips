using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private List<GameObject> engines;
    private List<GameObject> controls;
    private List<GameObject> guns;
    private GameObject command;

    private GameObject player;
    private GameObject playerCommand;

    public GameObject Shot;
    public float shotSpeed;

    public float recoilStrength; // strength of recoil
    public float shotTime; // time between shots (frames)
    private float shotTimer;

    public float accelSpeed;
    public float rotateSpeed;

    public float aggroDistance;

    // Start is called before the first frame update
    void Start()
    {
        UpdateSublists();
        foreach (Transform child in transform) { if (child.tag == "Command") command = child.gameObject; }

        player = GameObject.FindGameObjectWithTag("Player");
        foreach (Transform child in player.transform) { if (child.tag == "Command") playerCommand = child.gameObject; }

        shotTimer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector2.Distance(command.transform.position, playerCommand.transform.position) < aggroDistance)
        {
            //Vector2 diff = command.transform.position - playerCommand.transform.position;
            var cross = Vector3.Cross(command.transform.rotation * Vector3.up,
                                      new Vector3(command.transform.position.x - playerCommand.transform.position.x,
                                                  command.transform.position.y - playerCommand.transform.position.y,
                                                  0));
            var dot = Vector3.Dot(command.transform.rotation * Vector3.up,
                                  new Vector3(command.transform.position.x - playerCommand.transform.position.x,
                                                  command.transform.position.y - playerCommand.transform.position.y,
                                                  0));
            // rotate towards the player
            foreach (GameObject control in controls)
            {
                Rigidbody2D rb = control.GetComponent<Rigidbody2D>();
                rb.AddForce(rotate(new Vector2(cross.z * rotateSpeed, 0), rb.rotation));
            }
            // if the rotation is close enough, fire guns and engines
            if (Mathf.Abs(cross.z) < 1 && shotTimer > shotTime)
            {
                if (shotTimer > shotTime)
                {
                    shotTimer = 0;
                    foreach (GameObject gun in guns)
                    {
                        // recoil the gun
                        Rigidbody2D rb = gun.GetComponent<Rigidbody2D>();
                        rb.AddForce(rotate(new Vector2(0, -recoilStrength), rb.rotation));

                        // shoot the shot
                        var shot = Instantiate(Shot, rb.position + rotate(new Vector2(0, 1.28f), rb.rotation), gun.transform.rotation) as GameObject;
                        shot.GetComponent<Rigidbody2D>().AddForce(rotate(new Vector2(0, shotSpeed * 20), rb.rotation));
                    }
                }

                // fly towards the player
                foreach (GameObject engine in engines)
                {
                    Rigidbody2D rb = engine.GetComponent<Rigidbody2D>();
                    rb.AddForce(rotate(new Vector2(0, -25 * dot * accelSpeed), rb.rotation));
                }
            }
        }

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
        switch (name)
        {
            case "Engine": engines.Add(go); break;
            case "Control": controls.Add(go); break;
            case "Gun": guns.Add(go); break;
        }
    }
}
