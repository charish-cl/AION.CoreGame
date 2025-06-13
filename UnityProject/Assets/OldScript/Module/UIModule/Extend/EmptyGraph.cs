namespace AION.CoreFramework
{
    using UnityEngine;
    using UnityEngine.UI;

    public class EmptyGraph : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }    
}