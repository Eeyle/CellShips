using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandController : MonoBehaviour
{
    private RelativeJoint2D[] joints = new RelativeJoint2D[4];
    private LineRenderer lr;

    // Start is called before the first frame update
    void Start()
    {
        joints = GetComponents<RelativeJoint2D>();

        lr = gameObject.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.widthMultiplier = 0.2f;
        lr.positionCount = 1 + 2 * joints.Length;
    }

    // Update is called once per frame
    void Update()
    {

        if (joints.Length > 0) { 
        
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
        }
    }
}
