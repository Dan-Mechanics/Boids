using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public class BoidManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject boidPrefab = default;
        [SerializeField] private Transform pivot = default;
        [SerializeField] private Transform target = default;
        [SerializeField] private int cellCountAcross = default;
        [SerializeField] private int boidsCount = default;
        [SerializeField] private float boundsSize = default;

        [Header("Settings")]
        [SerializeField] private float impulseSpeed = default;
        [SerializeField] private float maxSpeed = default;
        [SerializeField] private float cohesion = default;
        [SerializeField] private float separation = default;
        [SerializeField] private float viewingRange = default;
        [SerializeField] private float alignment = default;
        [SerializeField] private float constraint = default;
        [SerializeField] private float constraintMargin = default;
        [SerializeField] private float motivation = default;

        private Dictionary<Vector3Int, List<Boid>> cells;
        private Transform[] transforms;

        private void Start()
        {
            // SPATIAL HASH.
            cells = new Dictionary<Vector3Int, List<Boid>>();
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

            // SETUP.
            transforms = new Transform[boidsCount];
            for (int i = 0; i < boidsCount; i++)
            {
                Vector3 pos = new Vector3(Random.Range(0f, boundsSize), Random.Range(0f, boundsSize), Random.Range(0f, boundsSize));
                Vector3 vel = Random.insideUnitSphere.normalized * impulseSpeed;
                Boid boid = new Boid(pos, vel, i);

                GameObject go = Instantiate(boidPrefab);
                LookAt[] lookAts = go.GetComponentsInChildren<LookAt>();
                for (int j = 0; j < lookAts.Length; j++)
                {
                    lookAts[j].SetTarget(target);
                }

                cells[Vector3Int.zero].Add(boid);
                transforms[i] = go.transform;
            }
        }

        private void FixedUpdate()
        {
            foreach (KeyValuePair<Vector3Int, List<Boid>> cell in cells)
            {
                // FIND NEIGHBOURS.
                List<Boid> neighbours = new List<Boid>();
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        for (int z = -1; z <= 1; z++)
                        {
                            Vector3Int cellPos = cell.Key + new Vector3Int(x, y, z);
                            if (!cells.ContainsKey(cellPos))
                                continue;

                            neighbours.AddRange(cells[cellPos]);
                        }
                    }
                }

                // APPLY RULES.
                foreach (Boid boid in cell.Value)
                {
                    boid.vel += GetConstrainForce(boid);
                    boid.vel += GetMotivationForce(boid);

                    neighbours.Remove(boid);
                    if (neighbours.Count > 0)
                    {
                        boid.vel += GetCohesionForce(boid, neighbours);
                        boid.vel += GetSeparationForce(boid, neighbours);
                        boid.vel += GetAlignmentForce(boid, neighbours);
                    }

                    neighbours.Add(boid);

                    boid.vel = Vector3.ClampMagnitude(boid.vel, maxSpeed);
                    boid.pos += boid.vel * Time.fixedDeltaTime;
                    transforms[boid.index].position = boid.pos;
                    transforms[boid.index].forward = boid.vel;
                }
            }

            // SORT CELLS.
            foreach (KeyValuePair<Vector3Int, List<Boid>> cell in cells)
            {
                List<Boid> boids = cell.Value;
                for (int i = boids.Count - 1; i >= 0; i--)
                {
                    Constrain(boids[i]);

                    Vector3Int correctCell = GetCellPos(boids[i].pos, boundsSize);
                    if (correctCell == cell.Key)
                        continue;

                    cells[correctCell].Add(boids[i]);
                    boids.RemoveAt(i);
                }
            }
        }

        public Vector3Int GetCellPos(Vector3 worldPos, float size)
        {
            return new Vector3Int(
                Mathf.FloorToInt(worldPos.x / size),
                Mathf.FloorToInt(worldPos.y / size),
                Mathf.FloorToInt(worldPos.z / size));
        }

        private Vector3 GetCohesionForce(Boid self, List<Boid> neighbours)
        {
            Vector3 avPos = neighbours[0].pos;
            for (int i = 1; i < neighbours.Count; i++)
            {
                avPos += neighbours[i].pos;
            }

            avPos /= neighbours.Count;
            return (avPos - self.pos) * cohesion;
        }

        private Vector3 GetSeparationForce(Boid self, List<Boid> neighbours)
        {
            Vector3 force = Vector3.zero;
            for (int i = 0; i < neighbours.Count; i++)
            {
                if (Vector3.Distance(self.pos, neighbours[i].pos) <= viewingRange)
                    force -= neighbours[i].pos - self.pos;
            }

            force /= neighbours.Count;
            return force * separation;
        }

        private Vector3 GetAlignmentForce(Boid self, List<Boid> neighbours)
        {
            Vector3 avVel = neighbours[0].vel;
            for (int i = 1; i < neighbours.Count; i++)
            {
                avVel += neighbours[i].vel;
            }

            avVel /= neighbours.Count;
            return (avVel - self.vel) * alignment;
        }

        private Vector3 GetMotivationForce(Boid boid)
        {
            return (target.position - boid.pos).normalized * motivation;
        }

        private Vector3 GetConstrainForce(Boid boid)
        {
            Vector3 force = Vector3.zero;
            if (boid.pos.x < constraintMargin)
                force.x = constraintMargin - boid.pos.x;
            else if (boid.pos.x > boundsSize - constraintMargin)
                force.x = boundsSize - constraintMargin - boid.pos.x;

            if (boid.pos.y < constraintMargin)
                force.y = constraintMargin - boid.pos.y;
            else if (boid.pos.y > boundsSize - constraintMargin)
                force.y = boundsSize - constraintMargin - boid.pos.y;

            if (boid.pos.z < constraintMargin)
                force.z = constraintMargin - boid.pos.z;
            else if (boid.pos.z > boundsSize - constraintMargin)
                force.z = boundsSize - constraintMargin - boid.pos.z;

            return force * constraint;
        }

        private void Constrain(Boid boid)
        {
            if (boid.pos.x < 0f)
                boid.pos.x = 0f;

            if (boid.pos.x > boundsSize)
                boid.pos.x = boundsSize;

            if (boid.pos.y < 0f)
                boid.pos.y = 0f;

            if (boid.pos.y > boundsSize)
                boid.pos.y = boundsSize;

            if (boid.pos.z < 0f)
                boid.pos.z = 0f;

            if (boid.pos.z > boundsSize)
                boid.pos.z = boundsSize;
        }

        /// <summary>
        /// https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Gizmos.DrawWireCube.html
        /// </summary>
        private void OnDrawGizmos()
        {
            float cellSize = boundsSize / cellCountAcross;
            Gizmos.color = new Color(0f, 1f, 0f, 0.01f);

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
            Gizmos.DrawWireCube(0.5f * boundsSize * Vector3.one, Vector3.one * boundsSize);
        }

        private void OnValidate()
        {
            pivot.position = 0.5f * boundsSize * Vector3.one;
            pivot.GetChild(0).localPosition = Vector3.back * boundsSize;
        }
    }
}
