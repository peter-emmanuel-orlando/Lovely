using UnityEngine;
using UnityEngine.AI;

// Use physics raycast hit from mouse click to set agent destination
[RequireComponent(typeof(UnifiedController))]
public class ClickToMove : MonoBehaviour
{
    [SerializeField]
    bool asTheCrowFlies = false;

    UnifiedController cont;
    RaycastHit m_HitInfo = new RaycastHit();


    void Start()
    {
        cont = GetComponent<UnifiedController>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction, out m_HitInfo, Mathf.Infinity, -1, QueryTriggerInteraction.Ignore) )
            {
                //var dir = transform.InverseTransformDirection(m_HitInfo.point - transform.position).normalized;
                if(asTheCrowFlies)
                {
                    cont.TurnToFace(m_HitInfo.point);
                    cont.Move(0, 1);
                }
                else
                    cont.MoveToDestination(m_HitInfo.point);

            }
        }
    }
}
