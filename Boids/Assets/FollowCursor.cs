using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public class FollowCursor : MonoBehaviour
    {
        [SerializeField] private Camera cam = default;
        [SerializeField] private float offset = default;

        private void FixedUpdate()
        {
            Vector3 pos = Input.mousePosition;
            pos.z = offset;

            transform.position = cam.ScreenToWorldPoint(pos);
        }
    }
}
