using UnityEngine;

public class FollowerController : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] float mouvementSpeed;
    private string state = "walking";
    [SerializeField] Vector2[] road;
    private int nextRoadNode;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {

    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        for (int i = 0; i < road.Length; i++)
        {
            try
            {
                Gizmos.DrawLine(road[i], road[i - 1]);
            }
            catch { }
        }
    }
    private void FixedUpdate()
    {
        if (state == "carrying" || state == "walking")
        {
            rb.velocity = getDirection().normalized * mouvementSpeed;
        }
    }
    private Vector2 getDirection()
    {
        if (state == "carrying")
        {
            if ((road[0] - (Vector2)transform.position).magnitude < 0.1f)
                state = "walking";

            else if ((road[nextRoadNode] - (Vector2)transform.position).magnitude < 0.1f)
                nextRoadNode++;

        }
        if (state == "walking" && (road[0] - (Vector2)transform.position).magnitude < 0.1f)
        {

        }
        return (Vector2)transform.position - road[nextRoadNode];
    }
}