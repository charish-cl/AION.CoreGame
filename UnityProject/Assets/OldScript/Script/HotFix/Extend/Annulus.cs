using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AION.CoreFramework
{
    public class Annulus : MaskableGraphic
    {
        public enum ShapeType
        {
            Annulus,
            Ellipse,
        }

        [SerializeField] private Sprite m_image;

        public ShapeType shapeType;

        public float innerRadius = 10;
        public float outerRadius = 20;

        public float xRadius = 20;
        public float yRadius = 15;

        [Range(0, 1)] [SerializeField] private float m_fillAmount;

        [Range(0, 36000)] public int segments = 360;

        [SerializeField] private Image.Origin360 m_originType;

        public bool m_isClockwise = true;

        [SerializeReference] public List<(Color color, float proportion)> segmentColors = new List<(Color, float)>();

        public override Texture mainTexture => m_image == null ? s_WhiteTexture : m_image.texture;

        private float m_originRadian = -1;

        public float fillAmount
        {
            get => m_fillAmount;
            set
            {
                m_fillAmount = value;
                SetVerticesDirty();
            }
        }

        public Sprite image
        {
            get => m_image;
            set
            {
                if (m_image == value)
                    return;
                m_image = value;
                SetVerticesDirty();
                SetMaterialDirty();
            }
        }

        public Image.Origin360 originType
        {
            get => m_originType;
            set
            {
                if (m_originType == value)
                    return;
                m_originType = value;
                SetOriginRadian();
                SetVerticesDirty();
            }
        }

        public bool isClockwise
        {
            get => m_isClockwise;
            set
            {
                if (m_isClockwise != value)
                {
                    m_isClockwise = value;
                    SetVerticesDirty();
                }
            }
        }

        private UIVertex[] m_vertexes = new UIVertex[4];
        private Vector2[] m_uvs = new Vector2[4];
        private Vector2[] m_positions = new Vector2[4];

        protected override void Start()
        {
            if (m_originRadian == -1)
                SetOriginRadian();

            m_uvs[0] = new Vector2(0, 1);
            m_uvs[1] = new Vector2(1, 1);
            m_uvs[2] = new Vector2(1, 0);
            m_uvs[3] = new Vector2(0, 0);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (m_fillAmount == 0 || segmentColors.Count == 0) return;

#if UNITY_EDITOR
            SetOriginRadian();
#endif

            float degrees = 360f / segments;
            int totalSegmentsToDraw = Mathf.RoundToInt(segments * m_fillAmount);

            int segmentIndex = 0;

            foreach (var (color, proportion) in segmentColors)
            {
                int colorSegmentCount = Mathf.RoundToInt(totalSegmentsToDraw * proportion);

                for (int i = 0; i < colorSegmentCount && segmentIndex < totalSegmentsToDraw; i++, segmentIndex++)
                {
                    float startRadian = segmentIndex * degrees * Mathf.Deg2Rad * (isClockwise ? 1 : -1) + m_originRadian;
                    float cosStart = Mathf.Cos(startRadian);
                    float sinStart = Mathf.Sin(startRadian);

                    m_positions[0] = new Vector2(-outerRadius * cosStart * xRadius, outerRadius * sinStart * yRadius);
                    m_positions[3] = new Vector2(-innerRadius * cosStart * xRadius, innerRadius * sinStart * yRadius);

                    float endRadian = (segmentIndex + 1) * degrees * Mathf.Deg2Rad * (isClockwise ? 1 : -1) + m_originRadian;
                    float cosEnd = Mathf.Cos(endRadian);
                    float sinEnd = Mathf.Sin(endRadian);

                    m_positions[1] = new Vector2(-outerRadius * cosEnd * xRadius, outerRadius * sinEnd * yRadius);
                    m_positions[2] = shapeType == ShapeType.Annulus
                        ? new Vector2(-innerRadius * cosEnd * xRadius, innerRadius * sinEnd * yRadius)
                        : Vector2.zero;

                    for (int j = 0; j < 4; j++)
                    {
                        m_vertexes[j].color = color;
                        m_vertexes[j].position = m_positions[j];
                        m_vertexes[j].uv0 = m_uvs[j];
                    }

                    int vertCount = vh.currentVertCount;

                    // Add the vertices for this segment
                    vh.AddVert(m_vertexes[0]);
                    vh.AddVert(m_vertexes[1]);
                    vh.AddVert(m_vertexes[2]);
                    vh.AddTriangle(vertCount, vertCount + 2, vertCount + 1);

                    if (shapeType == ShapeType.Annulus)
                    {
                        vh.AddVert(m_vertexes[3]);
                        vh.AddTriangle(vertCount, vertCount + 3, vertCount + 2);
                    }
                }
            }
        }
        
        public int GetIndexAtPoint(Vector2 point)
        {
            // Calculate the normalized polar angle for the point
            float angleRadian = Mathf.Atan2(point.y / yRadius, point.x / xRadius);
            if (angleRadian < 0)
                angleRadian += 2 * Mathf.PI; // Ensure angle is positive for comparison

            // Adjust the angle based on the origin and direction
            float adjustedAngle = (isClockwise ? 1 : -1) * angleRadian - m_originRadian;
            if (adjustedAngle < 0)
                adjustedAngle += 2 * Mathf.PI;

            // Normalize the angle to a 0-1 range (as a proportion of 360 degrees)
            float angleProportion = adjustedAngle / (2 * Mathf.PI);

            // Determine the cumulative fill of each color segment and find the segment index
            float cumulativeProportion = 0f;
            for (int i = 0; i < segmentColors.Count; i++)
            {
                cumulativeProportion += segmentColors[i].proportion * m_fillAmount;
                if (angleProportion <= cumulativeProportion)
                    return segmentColors.Count - 1 - i;
            }

            // If no segment matched, return -1 indicating point is out of bounds
            return -1;
        }


        private void SetOriginRadian()
        {
            switch (m_originType)
            {
                case Image.Origin360.Left:
                    m_originRadian = 0 * Mathf.Deg2Rad;
                    break;
                case Image.Origin360.Top:
                    m_originRadian = 90 * Mathf.Deg2Rad;
                    break;
                case Image.Origin360.Right:
                    m_originRadian = 180 * Mathf.Deg2Rad;
                    break;
                case Image.Origin360.Bottom:
                    m_originRadian = 270 * Mathf.Deg2Rad;
                    break;
            }
        }
    }
}