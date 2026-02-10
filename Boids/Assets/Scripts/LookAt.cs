using UnityEngine;

namespace Boids
{
    /// <summary>
    /// https://www.youtube.com/watch?v=bqtqltqcQhw
    /// </summary>
    public class LookAt : MonoBehaviour
    {
        [SerializeField] private Transform target = default;
        private void FixedUpdate() => transform.LookAt(target);
    }
}
