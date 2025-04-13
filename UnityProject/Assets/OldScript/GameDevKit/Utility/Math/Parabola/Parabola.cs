namespace GameDevKit.Utility
{
    using UnityEngine;
    public interface IParametricCurve
    {
        Vector3 GetPoint(float t);
    }
    public class ParabolaWithPoint:IParametricCurve
    {
        private Vector3 m_StartPoint;
        private float m_HorizontalSpeed;
        private float m_VerticalSpeed;
        private float m_Gravity;

        public ParabolaWithPoint(Vector3 startPoint, float horizontalSpeed, float verticalSpeed, float gravity)
        {
            m_StartPoint = startPoint;
            m_HorizontalSpeed = horizontalSpeed;
            m_VerticalSpeed = verticalSpeed;
            m_Gravity = gravity;
        }

        public Vector3 GetPoint(float t)
        {
            float x = m_StartPoint.x + m_HorizontalSpeed * t;
            float y = m_StartPoint.y + m_VerticalSpeed * t + 0.5f * m_Gravity * t * t;
            float z = m_StartPoint.z;
            return new Vector3(x, y, z);
        }
    }


    public class ParabolaWithVelocity:IParametricCurve
    {
        private Vector3 m_StartPoint;
        private float m_HorizontalSpeed;
        private float m_VerticalSpeed;
        private float m_Gravity;

        public ParabolaWithVelocity(Vector3 startPoint, float horizontalSpeed, float verticalSpeed, float gravity)
        {
            m_StartPoint = startPoint;
            m_HorizontalSpeed = horizontalSpeed;
            m_VerticalSpeed = verticalSpeed;
            m_Gravity = gravity;
        }

        public Vector3 GetPoint(float t)
        {
            float x = m_StartPoint.x + m_HorizontalSpeed * t;
            float y = m_StartPoint.y + m_VerticalSpeed * t + 0.5f * m_Gravity * t * t;
            float z = m_StartPoint.z;
            return new Vector3(x, y, z);
        }
    }

}