using UnityEngine;

namespace Boids
{
    public class LookAt : MonoBehaviour
    {
        [SerializeField] private Transform target = default;
        
        private void FixedUpdate() => transform.LookAt(target);

        public void SetTarget(Transform target) => this.target = target;
    }
}
