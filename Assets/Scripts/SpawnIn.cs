using UnityEngine;

public class SpawnIn : MonoBehaviour
{

    public Transform target;
    public float speed = 25f;

    void Update()
    {
        if (target != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target.position,
                speed * Time.deltaTime
            );
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

}
