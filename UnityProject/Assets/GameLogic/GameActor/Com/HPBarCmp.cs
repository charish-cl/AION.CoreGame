namespace GameLogic
{
    public class HPBarCmp : GameActorCmp
    {
       
        HPBarLogic hpBarLogic;
        public override void OnInit()
        {
            base.OnInit();
            
            Actor.EventDispatcher.AddEventListener<NumericType,float,float>(IActorEvent_Event.NumbericChange, OnNumbericChange,this);

            hpBarLogic = HPBarLogicSystem.Instance.CreateHPBar(Actor.Transform);


            var numericComponent = Actor.GetComponent<NumericComponent>();
            
            hpBarLogic.Init(numericComponent.GetAsInt(NumericType.Hp));
        }

        private void OnNumbericChange(NumericType arg1, float oldValue, float newValue)
        {
            if (arg1 == NumericType.Hp)
            {
                hpBarLogic.SetHp(newValue);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            HPBarLogicSystem.Instance.ReleaseHPBar(hpBarLogic);
        }
        
    }
}