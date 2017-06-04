using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RopeCreator : MonoBehaviour {

    public Transform RopePart;
    public float RopePartDistance;
    public int NumberOfRopeParts;
    public List<Transform> ropes;
    void Start()
    {
        ropes = new List<Transform>();
        for(var i = 0; i < NumberOfRopeParts; i++)
        {
            addRopePart();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            var goalCount = Mathf.Ceil((float)NumberOfRopeParts / 2f);
            while(ropes.Count() > goalCount)
                ropes.RemoveAt(ropes.Count() - 1);
            for(var i = 0; i<NumberOfRopeParts;i++)
                addRopePart();
        }
    }

    private void addRopePart()
    {
        
        var ropeTransform = Instantiate(RopePart) as Transform;
        ropeTransform.position = new Vector2(transform.position.x, transform.position.y - ((ropes.Count() + 1) * (RopePartDistance*2f)));


        Rigidbody2D prevRigidBody = null;
        if (ropes.Count() == 0)
            prevRigidBody = transform.GetComponent<Rigidbody2D>();
        else
            prevRigidBody = ropes[ropes.Count - 1].GetComponent<Rigidbody2D>();
        Rigidbody2D newRigidBody = ropeTransform.GetComponent<Rigidbody2D>();

        var newHinge = ropeTransform.GetComponent<HingeJoint2D>();

        newHinge.connectedBody = prevRigidBody;

        newHinge.anchor = new Vector2((prevRigidBody.transform.position - newRigidBody.transform.position).x, (prevRigidBody.transform.position - newRigidBody.transform.position).y);

        newHinge.connectedAnchor = Vector2.zero;

        newHinge.breakForce = 999999f;
        newHinge.breakTorque = 999999f;

        ropes.Add(ropeTransform);
        
    }
}
