using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlicedDestroyer : MonoBehaviour
{
    void Update()
    {
        if (gameObject.transform.position.y < -10)
        {
            Destroy(gameObject);
        }
    }
}
