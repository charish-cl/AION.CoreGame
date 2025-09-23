using AION.CoreFramework;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    
    public class HPBarLogic : ObjectBase
    {
        public GameObject HPBarPrefab;
        public Transform HeroTransform;
        
        Slider m_Slider;
        public HPBarLogic(GameObject go = null, Transform heroTransform = null)
        {
            HPBarPrefab = go;
            HeroTransform = heroTransform;

            if (go)
            {
                m_Slider = go.GetComponent<Slider>();
            }
        }
        public void SetParent(Transform parent)
        {
            HeroTransform = parent;
        }
        public override void OnSpawn()
        {
            HPBarPrefab.gameObject.SetActive(true);
        }

        public override void OnUnspawn()
        {
            HPBarPrefab.gameObject.SetActive(false);
        }
        
        public void SynPos()
        {
            if (HeroTransform == null)
            {
                return;
            }
            // 将世界坐标转换为屏幕坐标
            Vector3 screenPoint = GameModule.UI.UICamera.WorldToScreenPoint(HeroTransform.position);         
            
              
            // 将屏幕坐标转换为Canvas内的局部坐标
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)HPBarPrefab.transform.parent.transform, 
                screenPoint, 
                GameModule.UI.UICamera, 
                out localPoint
            );
        
            // 应用偏移并设置位置
            HPBarPrefab.transform.localPosition = localPoint + new Vector2(0, 50f);
        }

        public void Init(float HP)
        {
            m_Slider.maxValue = HP;
            m_Slider.value = HP;
        }
        public void SetHp(float currentHp)
        {
            m_Slider.value = currentHp;
            // DOTween.To(()=>m_Slider.value,()=>m_Slider.value,currentHp,0.1f);
        }
    }
    public class HPBarLogicSystem : BaseLogicSys<HPBarLogicSystem>
    {
        public Transform HPBarParent;
        
        private ObjectPool<HPBarLogic>  HPBarPool;
        
        public override bool OnInit()
        {
            HPBarParent =SceneMgr.Instance.SceneBehavior.HPBarCanvas.transform;
            HPBarPool = GameModule.ObjectPool.CreateObjectPool<HPBarLogic>();
            return base.OnInit();
        }

        public HPBarLogic CreateHPBar( Transform heroTransform)
        {
            
            var hpBar = HPBarPool.Spawn("HPBar");
            if (hpBar == null)
            {
                GameObject hpBarPrefab = GameModule.Resource.LoadAsset<GameObject>("Assets/Game/UIComponent/HPBar.prefab");
          
                GameObject hpBarGameObject = GameObject.Instantiate(hpBarPrefab, HPBarParent);
                
                hpBarGameObject.name = "HPBar";
                
                hpBar = new HPBarLogic(hpBarGameObject,heroTransform);
                HPBarPool.Register( hpBar);
            }
            hpBar.SetParent(heroTransform);
            return hpBar;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            foreach (var keyValuePair in HPBarPool.objMap)
            {
                var hpBar = keyValuePair.Value;
                if (hpBar == null) continue;
                hpBar.m_Object.SynPos();
            }
        }

        public void ReleaseHPBar(HPBarLogic hpBarLogic)
        {
            HPBarPool.UnSpawn(hpBarLogic);
        }
    }
}