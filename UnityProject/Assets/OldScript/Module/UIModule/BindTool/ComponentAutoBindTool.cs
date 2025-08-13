using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Scriban;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace AION.CoreFramework
{
    public class ComponentAutoBindTool : SerializedMonoBehaviour
    {
        [Title("UI控件")] [TableList] public List<BindData> BindDatas = new List<BindData>();


        [ValueDropdown("GetBindDatas")] [LabelText("添加数据")] [OnValueChanged("SimpleAdd")]
        public BindData AddData;

        public void SimpleAdd()
        {
            if (AddData == null || AddData.BindCom == null)
            {
                return;
            }

            BindDatas.Add(AddData);
            AddData = null;
        }

        public IEnumerable GetBindDatas()
        {
            return UISelectTool.GetBindGo(transform);
        }

        private void OnEnable()
        {
            var selectedTransform = Selection.activeTransform;
            if (selectedTransform == null)
            {
                return;
            }
        }
#if UNITY_EDITOR
        [ButtonGroup("Tools")]
        [Button("生成代码", ButtonHeight = 40)]
        public string GenerateUIBindings()
        {
            var selectedTransform = Selection.activeTransform;
            if (selectedTransform == null)
            {
                Debug.LogError("Please select a valid UI element.");
                return string.Empty;
            }

            //移除空引用
            BindDatas = BindDatas.Where(e => e.BindCom != null).ToList();

            EditorUtility.SetDirty(this);

            UIConfigDefine.BuildBindCode(BindDatas, transform.gameObject);
            return "";
        }
#endif

        public bool HashComponent(Object selectobj)
        {
            return BindDatas.Exists(e => e.BindCom == selectobj);
        }

        public void RemoveData(Object selectobj)
        {
            BindDatas.RemoveAll(e => e.BindCom == selectobj);
            SetDirty();
        }

        void SetDirty()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
#endif
        }


        public void UpdateData(GameObject selectObj, string uiType)
        {
            var bindData = BindDatas.Find(e => e.BindCom == selectObj);
            if (bindData == null)
            {
                bindData = new BindData();
                bindData.BindCom = selectObj;
                BindDatas.Add(bindData);
            }

            bindData.TypeName = uiType;
            SetDirty();
        }

        
        public int GetIndex(Object bindCom)
        {
            var bindData = BindDatas.Find(e => e.BindCom == bindCom);
            if (bindData == null)
            {
                return -1;
            }
            return UIConfigDefine.dicWidgetIndex.TryGetValue(bindData.TypeName, out var index)? index : -1;
        }
    }
}