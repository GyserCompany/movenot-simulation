using System.Collections.Generic;
using UnityEngine;

// Необходим для простого получения объектов, находящихся внутри коллайдера
public class TriggerDetector : MonoBehaviour
{
    private HashSet<GameObject> collidingObjects = new HashSet<GameObject>();
    
    void OnTriggerEnter(Collider other)
    {
        collidingObjects.Add(other.gameObject);
    }
    
    void OnTriggerExit(Collider other)
    {
        collidingObjects.Remove(other.gameObject);
    }
    
    public HashSet<GameObject> CollidingObjects => collidingObjects;
}