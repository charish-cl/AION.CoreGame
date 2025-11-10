using AION.CoreFramework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameLogic
{
    public partial class CurrencyWidget : UIWidget
    {
        public Transform m_tfParent { get; set; }
        public GameObject currencyItem { get; set; }

        public override void ScriptGenerator()
        {

            m_tfParent = transform.Find("m_tfParent").GetComponent<Transform>();


            currencyItem = transform.Find("m_tfParent/CurrencyItem").gameObject;
            
        }
    }
}