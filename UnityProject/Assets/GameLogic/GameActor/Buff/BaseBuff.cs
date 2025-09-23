using System;
using GameLogic;

namespace AION.Config.Buff
{
    public class BaseBuff
    {
        public bool IsExpired = false;
        public string Id;
        public float Duration;
        public AttributeModifier Modifier;
        public Action OnBuffExpired;

        public BaseBuff(string id, float duration, AttributeModifier modifier)
        {
            Id = id;
            Duration = duration;
            Modifier = modifier;
        }

        public void OnStart()
        {
        }

        public void OnUpdate(float deltaTime)
        {
            if (deltaTime >= Duration)
            {
                IsExpired = true;
                OnBuffExpired?.Invoke();
            }
        }

        public void OnEnd()
        {
        }

        public bool CheckExpired()
        {
            return IsExpired;
        }
    }
}