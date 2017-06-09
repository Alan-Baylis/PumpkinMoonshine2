using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class AnchorDeployer : MonoBehaviour {

    public float reach;
    private Vector2 deploymentDirection;
    public bool isDeploying;
    [SerializeField]
    private LayerMask AttachableLayer;
    public Transform AnchorPrefab;
    public RopeGrabber Grabber;
    public RopeDeployer Deployer;
    // Use this for initialization
    void Start () {
        deploymentDirection = Vector2.one;
	}
	
	// Update is called once per frame
	void Update () {
        UpdateDeploymentAngle();

        if (Input.GetKeyDown(KeyCode.LeftShift)||Input.GetKeyDown(KeyCode.CapsLock))
        {
            TryDeploy();
        }
	}

    void UpdateDeploymentAngle()
    {
        var input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (input == Vector2.zero)
            return;
        input = input.normalized;
        var inputAngle = Vector2.Angle(Vector2.right, input);
        if (input.y < 0)
            inputAngle = (-inputAngle)+360f;
        var deploymentAngle = Vector2.Angle(Vector2.right, deploymentDirection);
        if (deploymentDirection.y < 0)
            deploymentAngle = (-deploymentAngle) + 360f;

        var relativeInputDegree = (inputAngle-deploymentAngle) % 360f;
        if (relativeInputDegree < 0)
            relativeInputDegree += 360f;

        var difference = 0f;
        if (relativeInputDegree > 180f)
            difference = -(360f - relativeInputDegree);
        else
            difference = relativeInputDegree;

        var goalAngle = deploymentAngle + (difference / 6f);

        deploymentDirection = Quaternion.Euler(0f, 0f, goalAngle) * Vector2.right;
    }

    void OnDrawGizmos()
    {
        var oldColor = Gizmos.color;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, reach);

        Gizmos.color = Color.Lerp(Color.green, Color.cyan, .5f);
        Gizmos.DrawWireSphere((Vector3)GetLocalDeploymentPos()+transform.position, reach/4);

        Gizmos.color = oldColor;    
    }

    private Vector2 GetLocalDeploymentPos()
    {
        return deploymentDirection.normalized * reach;
    }

    private bool TryDeploy()
    {
        RaycastHit2D[] raycastHits = new RaycastHit2D[1];
        Vector2 localDeploymentPosition = GetLocalDeploymentPos();
        var contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(AttachableLayer);
        Physics2D.Raycast(transform.position, localDeploymentPosition, contactFilter, raycastHits, reach);

        var raycastHitsList = raycastHits.ToList();
        if (raycastHitsList.First().transform == null)
            return false;

        var anchor = Instantiate(AnchorPrefab, (Vector3)raycastHitsList.First().point, Quaternion.identity) as Transform;

        anchor.transform.GetComponent<RopeCreator>().holderGrabber = Grabber;

        if (Deployer.isDeploying)
            Deployer.StartConnect(anchor);
        else
            Deployer.StartDeploy(anchor);

        return true;
    }
}
