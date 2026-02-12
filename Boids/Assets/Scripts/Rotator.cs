using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public class Rotator : MonoBehaviour
    {
        [SerializeField] private Vector3 rotation = default;

        private void FixedUpdate()
        {
            transform.Rotate(rotation * Time.fixedDeltaTime, Space.World);
        }
    }
}
