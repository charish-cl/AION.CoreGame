namespace GameLogic
{
    public class HPBarCmp : GameActorCmp
    {
       
        HPBarLogic hpBarLogic;
        public override void OnInit()
        {
            base.OnInit();
            
            Actor.EventDispatcher.AddEventListener<NumericType,float,float>(IActorEvent_Event.NumbericChange, OnNumbericChange,this);

            hpBarLogic = HPBarLogicSystem.Instance.CreateHPBar(Actor.m_transform);


            var numericComponent = Actor.GetComponent<NumericComponent>();
            
            hpBarLogic.Init(numericComponent.GetAsInt(NumericType.Hp));
        }

        private void OnNumbericChange(NumericType arg1, float arg2, float arg3)
        {
            if (arg1 == NumericType.Hp)
            {
                hpBarLogic.SetHp(arg2);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            HPBarLogicSystem.Instance.ReleaseHPBar(hpBarLogic);
        }
        
    }
}