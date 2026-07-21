using UnityEngine;

public class ParticleRemove : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private float time;
    void Start()
    {
        Destroy(gameObject, time);
    }
}
