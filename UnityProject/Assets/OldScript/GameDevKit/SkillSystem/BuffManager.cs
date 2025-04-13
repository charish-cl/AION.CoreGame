using System.Collections.Generic;

namespace GameDevKit.SkillSystem
{
    public class BuffManager
    {
        private List<IBuff> buffs = new List<IBuff>();

        public void AddBuff(IBuff buff)
        {
            buffs.Add(buff);
            buff.OnStart();
        }

        public void RemoveBuff(IBuff buff)
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
                IBuff buff = buffs[i] as IBuff;
                buff.Update(deltaTime);
                if (buff.IsExpired())
                {
                    RemoveBuff(buff);
                }
            }
        }
    }
}