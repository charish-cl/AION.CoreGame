using System;
using UnityEngine;

namespace GameDevKit.Utility
{
    public class ParabolaDemo : MonoBehaviour
    {
        public Transform startPoint;
        public float horizontalSpeed = 1.0f;
        public float verticalSpeed = 10f;
        public float gravity = -9.8f;
        public float speed = 1.0f;
        public float Duration => Mathf.Abs(verticalSpeed / gravity)*2 ;
        private IParametricCurve m_Parabola;
        private float m_T = 0.0f;

        private void Start()
        {
            m_Parabola = new ParabolaWithVelocity(startPoint.position, horizontalSpeed, verticalSpeed, gravity);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }
           
            Gizmos.color = Color.red;
            float timeStep = 0.1f;
            float maxTime = Duration;
            Vector3 prevPos = transform.position;

            for (float t = 0f; t < maxTime; t += timeStep)
            {
                Vector3 pos = m_Parabola.GetPoint(t);   
                Gizmos.DrawLine(prevPos, pos);
                prevPos = pos;
            }
        }

        private void Update()
        {
            m_T += Time.deltaTime * speed;
            if (m_T > Duration)
            {
                m_T = 0.0f;
            }

            transform.position = m_Parabola.GetPoint(m_T);
        }
    }

}