using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    /// <summary>
    /// Todo: enforce cosntant speed, debug all the bullshit, make more normalized and predictable
    /// </summary>
    public class BoidManager : MonoBehaviour
    {
        [SerializeField] private GameObject boidPrefab = default;
        [SerializeField] private Transform cam = default;
        [SerializeField] private Transform food = default;
        [SerializeField, Range(5, 50)] private int cellCountAcross = default;
        [SerializeField, Min(5)] private int boidsCount = default;
        [SerializeField, Min(0f)] private float bounds = default;
        [SerializeField, Min(0f)] private float speed = default;

        [SerializeField, Min(0f)] private float cohesion = default;
        [SerializeField, Min(0f)] private float separation = default;
        [SerializeField, Min(0f)] private float viewingRange = default;
        [SerializeField, Min(0f)] private float alignment = default;
        [SerializeField, Min(0f)] private float motivation = default;
        [SerializeField, Min(0f)] private float constraint = default;

        private readonly Dictionary<Vector3Int, List<Boid>> cells = new Dictionary<Vector3Int, List<Boid>>();
        private Transform[] transforms;

        private void Start()
        {
            for (int x = 0; x < cellCountAcross; x++)
            {
                for (int y = 0; y < cellCountAcross; y++)
                {
                    for (int z = 0; z < cellCountAcross; z++)
                    {
                        cells[new Vector3Int(x, y, z)] = new List<Boid>();
                    }
                }
            }

            transforms = new Transform[boidsCount];
            for (int i = 0; i < boidsCount; i++)
            {
                Vector3 pos = new Vector3(Random.Range(0f, bounds), Random.Range(0f, bounds), Random.Range(0f, bounds));
                Vector3 vel = Random.insideUnitSphere.normalized * speed;
                cells[Vector3Int.zero].Add(new Boid() { pos = pos, vel = vel, index = i });

                Transform newBoid = Instantiate(boidPrefab).transform;
                LookAt[] lookAts = newBoid.GetComponentsInChildren<LookAt>();
                for (int j = 0; j < lookAts.Length; j++)
                {
                    lookAts[j].SetTarget(food);
                }

                transforms[i] = newBoid;
            }
        }

        private void FixedUpdate()
        {
            foreach (KeyValuePair<Vector3Int, List<Boid>> cell in cells)
            {
                // FIND FRIENDS.
                List<Boid> friends = new List<Boid>();
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        for (int z = -1; z <= 1; z++)
                        {
                            Vector3Int cellPos = cell.Key + new Vector3Int(x, y, z);
                            if (!cells.ContainsKey(cellPos))
                                continue;

                            friends.AddRange(cells[cellPos]);
                        }
                    }
                }

                // APPLY RULES.
                foreach (Boid boid in cell.Value)
                {
                   // boid.vel += GetConstrainForce(boid);
                    //boid.vel += GetMotivationForce(boid);

                    friends.Remove(boid);
                    if (friends.Count > 0)
                    {
                        // YOU COULD IMPLEMENT INTERFACES HERE.
                        // MAY AFFECT PERFORMANCE.
                        boid.vel += GetCohesionForce(boid, friends);
                        boid.vel += GetSeparationForce(boid, friends);
                        boid.vel += GetAlignmentForce(boid, friends);
                    }

                    friends.Add(boid);

                    // RENDER.
                    boid.vel.Normalize();
                    boid.pos += boid.vel * Time.fixedDeltaTime;
                    transforms[boid.index].position = boid.pos;
                    transforms[boid.index].forward = boid.vel.normalized;
                }
            }

            // SORT.
            /*foreach (KeyValuePair<Vector3Int, List<Boid>> cell in cells)
            {
                List<Boid> boids = cell.Value;
                for (int i = boids.Count - 1; i >= 0; i--)
                {
                    boids[i].Constrain(bounds);
                    Vector3Int correctCell = GetCellPos(boids[i].pos, bounds);
                    if (correctCell == cell.Key)
                        continue;

                    cells[correctCell].Add(boids[i]);
                    boids.RemoveAt(i);
                }
            }*/
        }

        public Vector3Int GetCellPos(Vector3 pos, float size)
        {
            return new Vector3Int(
                Mathf.FloorToInt(pos.x / size),
                Mathf.FloorToInt(pos.y / size),
                Mathf.FloorToInt(pos.z / size));
        }

        private Vector3 GetCohesionForce(Boid boid, List<Boid> friends)
        {
            Vector3 avPos = friends[0].pos;
            for (int i = 1; i < friends.Count; i++)
            {
                avPos += friends[i].pos;
            }

            avPos /= friends.Count;
            return (avPos - boid.pos) * cohesion;
        }

        private Vector3 GetSeparationForce(Boid boid, List<Boid> friends)
        {
            Vector3 force = Vector3.zero;
            for (int i = 0; i < friends.Count; i++)
            {
                Boid friend = friends[i];
                if (Vector3.Distance(boid.pos, friend.pos) < viewingRange)
                    force -= friend.pos - boid.pos;
            }

            force /= friends.Count;
            return (force) * separation;
        }

        private Vector3 GetAlignmentForce(Boid boid, List<Boid> friends)
        {
            Vector3 avVel = friends[0].vel;
            for (int i = 1; i < friends.Count; i++)
            {
                avVel += friends[i].vel;
            }

            avVel /= friends.Count;
            return (avVel - boid.vel) * alignment;
        }

        private Vector3 GetMotivationForce(Boid boid)
        {
            return (food.position - boid.pos).normalized * motivation;
        }

        private Vector3 GetConstrainForce(Boid boid)
        {
            Vector3 force = Vector3.zero;
            if (boid.pos.x < 0f)
                force.x = 1f;

            if (boid.pos.x > bounds)
                force.x = -1f;

            if (boid.pos.y < 0f)
                force.y = 1f;

            if (boid.pos.y > bounds)
                force.y = -1f;

            if (boid.pos.z < 0f)
                force.z = 1f;

            if (boid.pos.z > bounds)
                force.z = -1f;

            return force * constraint;
        }

        /// <summary>
        /// https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Gizmos.DrawWireCube.html
        /// </summary>
        private void OnDrawGizmos()
        {
            float cellSize = bounds / cellCountAcross;
            Gizmos.color = new Color(0f, 1f, 0f, 0.05f);

            for (int x = 0; x < cellCountAcross; x++)
            {
                for (int y = 0; y < cellCountAcross; y++)
                {
                    for (int z = 0; z < cellCountAcross; z++)
                    {
                        Gizmos.DrawWireCube(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f) * cellSize, Vector3.one * cellSize);
                    }
                }
            }

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(0.5f * bounds * Vector3.one, Vector3.one * bounds);
        }

        private void OnValidate()
        {
            cam.position = 0.5f * bounds * Vector3.one - cam.forward * bounds;
        }
    }
}
