using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureController : MonoBehaviour
{
    RelativeJoint2D[] joints = new RelativeJoint2D[4];
    private LineRenderer lr;

    private bool isSelected; // player is holding down the mouse on this cell

    private GameObject player;
    private List<GameObject> possibleSelections; // what it could attach to while selected

    // Start is called before the first frame update
    void Start()
    {
        joints = GetComponents<RelativeJoint2D>();

        lr = gameObject.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.widthMultiplier = 0.2f;
        lr.positionCount = 1 + 2 * joints.Length;

        player = GameObject.FindGameObjectsWithTag("Player")[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (isSelected)
        {
            // follow the mouse
            transform.position = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

            // find a new vector pointing from the cell's position to each of the player's cells
            possibleSelections = new List<GameObject>(); // store the cell's position vectors
            foreach (Transform child in player.transform)
            {
                if (child.tag == "Structure" ||
                    child.tag == "Gun" ||
                    child.tag == "Engine" ||
                    child.tag == "Control" ||
                    child.tag == "Command")
                {
                    if (Vector2.Distance(child.position, transform.position) < 3)
                    {
                        possibleSelections.Add(child.gameObject);
                    }
                }

                if (child.tag == "Command")
                {
                    // rotate to face the command module of the ship
                    transform.rotation = child.rotation;
                }
            }
            // the line renderer is either nonexistant or referencing itself.
            lr.positionCount = (possibleSelections.Count > 0) ? 3 + 2 * (possibleSelections.Count - 1) : 0;
            if (possibleSelections.Count > 0)
            {
                lr.SetPosition(0, transform.position);
                for (int i = 0; i < possibleSelections.Count; i++)
                {
                    lr.SetPosition(2 * i + 1, possibleSelections[i].transform.position);
                    lr.SetPosition(2 * i + 2, transform.position);
                }
            }
        }
        else
        {
            if (joints.Length > 0)
            {
                // Draw the lines between all joints
                lr.SetPosition(0, joints[0].attachedRigidbody.position);
                for (int i = 0; i < joints.Length; i++)
                {
                    lr.SetPosition(2 * i + 1, joints[i].connectedBody.position);
                    lr.SetPosition(2 * i + 2, joints[i].attachedRigidbody.position);

                    // Check if any lines need to break
                    if (Vector2.Distance(joints[i].connectedBody.position, joints[i].attachedRigidbody.position) > 3)
                    {
                        joints[i].connectedBody = joints[i].attachedRigidbody; // poof, the joint is gone
                    }
                }
                // If all joints are broken, remove it from its parent and update the parent
                bool noJoints = true;
                foreach (var joint in joints) { if (!joint.connectedBody.Equals(joint.attachedRigidbody)) noJoints = false; }
                if (noJoints)
                {
                    if (transform.parent)
                    {
                        var parent = transform.parent;
                        transform.parent = null;
                        if (parent.gameObject.tag == "Player" || parent.gameObject.tag == "Enemy") parent.gameObject.BroadcastMessage("UpdateSublists");
                    }
                }
            }
            else
            {
                bool noConnections = true;
                if (transform.parent)
                {
                    var j = transform.parent.GetComponentsInChildren<RelativeJoint2D>();
                    if (j.Length > 0)
                    {
                        foreach (RelativeJoint2D joint in j)
                        {
                            if (joint.connectedBody.Equals(GetComponent<Rigidbody2D>())) noConnections = false;
                        }
                        if (noConnections)
                        {
                            var parent = transform.parent;
                            transform.parent = null;
                            if (parent.gameObject.tag == "Player" || parent.gameObject.tag == "Enemy") parent.gameObject.BroadcastMessage("UpdateSublists");
                        }
                    }
                }
            }
        }
    }

    void SetSelected(bool val)
    {
        isSelected = val;

        if (!val) lr.positionCount = 1 + 2 * joints.Length;
    }

    void ReleaseSelected()
    {
        foreach (GameObject selection in possibleSelections)
        {
            var go = transform.gameObject.AddComponent<RelativeJoint2D>();
            go.connectedBody = selection.GetComponent<Rigidbody2D>();
            go.maxForce = 4f;
            go.maxTorque = 5f;
            go.correctionScale = 0.1f;
            go.enableCollision = true;
        }

        transform.parent = player.transform; // make it subservient to player
        player.gameObject.BroadcastMessage("UpdateSublists");

        joints = GetComponents<RelativeJoint2D>();
        lr.positionCount = 1 + 2 * joints.Length;
    }
}