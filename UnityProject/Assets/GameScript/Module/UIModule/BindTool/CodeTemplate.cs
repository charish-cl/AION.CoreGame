namespace AION.CoreFramework
{
    public class CodeTemplate
    {
      public const string uiWindowClass = @"
    public  class #类名# : MonoBehaviour
    {   
        #绑定变量#
        public void Awake()
        {
            #绑定代码#
        }
        #绑定方法#
    }
";

      public  const string uiFormClass = @"
using AION.CoreFramework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace #命名空间#
{
    public partial class #类名# : UIWindow
    {
        #绑定方法#
        public override void RegisterEvent()
        {
        }
        public override void OnCreate()
        {
        }
    }
}";
      public    const   string uiFormBindClass = @"
using AION.CoreFramework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace #命名空间#
{
    public partial class #类名# 
    {
        #绑定变量#
    
        public override void ScriptGenerator()
        {
            #绑定代码#
        }
    }
}";

      public  const   string UIWidgetClass = @"
using AION.CoreFramework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace #命名空间#
{
    public partial class #类名# : UIWidget
    {
        #绑定方法#
    }
}";

 

      public  const   string UIWidgetBindClass = @"
using AION.CoreFramework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace #命名空间#
{
    public partial class #类名# 
    {
        #绑定变量#
        public override void OnInit(object userData)
        {
            base.OnInit(userData);
            #绑定代码#
            BindOtherData(userData);
        }
    }
}";
    }
}