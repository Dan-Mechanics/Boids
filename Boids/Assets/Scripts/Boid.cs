using UnityEngine;

namespace Boids
{
    public class Boid
    {
        public Vector3 pos;
        public Vector3 vel;
        public int index;

        public Boid(Vector3 pos, Vector3 vel, int index)
        {
            this.pos = pos;
            this.vel = vel;
            this.index = index;
        }
    }
}
