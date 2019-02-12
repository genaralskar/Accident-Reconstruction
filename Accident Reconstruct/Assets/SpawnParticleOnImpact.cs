using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnParticleOnImpact : MonoBehaviour
{
    public GameObject explosionParticles;
    public float requiredImpactSpeed = 5;

    private void OnCollisionEnter(Collision other)
    {
        if (other.relativeVelocity.magnitude > requiredImpactSpeed)
        {
            Instantiate(explosionParticles, transform);
        }
    }
}
