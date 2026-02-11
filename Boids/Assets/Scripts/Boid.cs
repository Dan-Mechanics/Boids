using UnityEngine;

namespace Boids
{
    public class Boid
    {
        public Vector3 pos;
        public Vector3 vel;
        public int index;

        public void Constrain(float bounds)
        {
            if (pos.x < 0f)
                pos.x = 0f;

            if (pos.x > bounds)
                pos.x = bounds;

            if (pos.y < 0f)
                pos.y = 0f;

            if (pos.y > bounds)
                pos.y = bounds;

            if (pos.z < 0f)
                pos.z = 0f;

            if (pos.z > bounds)
                pos.z = bounds;
        }
    }
}
