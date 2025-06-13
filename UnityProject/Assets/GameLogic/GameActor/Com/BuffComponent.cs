using System.Collections.Generic;
using AION.Config.Buff;

namespace AION.CoreFramework
{
    public class BuffComponent:GameActorCmp
    {
        private List<BaseBuff> buffs = new List<BaseBuff>();

        public void AddBuff(BaseBuff buff)
        {
            buffs.Add(buff);
            buff.OnStart();
        }

        public void RemoveBuff(BaseBuff buff)
        {
            if (buffs.Contains(buff))
            {
                buff.OnEnd();
                buffs.Remove(buff);
            }
        }

        public void Update(float deltaTime)
        {
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                BaseBuff buff = buffs[i] as BaseBuff;
                buff.OnUpdate(deltaTime);
                if (buff.CheckExpired())
                {
                    RemoveBuff(buff);
                }
            }
        }
        public override void OnInit()
        {
         
        }

        public override void OnUpdate()
        {
          
        }

        public override void OnDestroy()
        {
          
        }
    }
}