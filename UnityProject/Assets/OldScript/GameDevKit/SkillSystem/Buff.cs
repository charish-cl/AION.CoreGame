using System;
using UnityEngine;

namespace GameDevKit.SkillSystem
{
    public interface IBuff
    {
        public  void Update(float deltaTime);
        
        public void OnStart();
        public void OnEnd();
        public bool IsExpired();

    }
 
    public class Buff<T> where T:ScriptableObject,IBuff
    {
        public T data;
        public string Name { get; set; }
        public BuffType Type { get; set; }
        public int Value { get; set; }
        public float Duration { get; set; }

        private float remainingDuration;

        public Buff(string name, BuffType type, int value, float duration)
        {
            this.Name = name;
            this.Type = type;
            this.Value = value;
            this.Duration = duration;
            this.remainingDuration = Math.Abs(Duration - (-1)) > float.Epsilon ? Mathf.Infinity : duration;
        }

        public virtual bool IsExpired()
        {
            return remainingDuration <= 0f && Math.Abs(Duration - (-1)) > float.Epsilon;
        }

        public virtual void Update(float deltaTime)
        {
            if (remainingDuration > 0f && Math.Abs(Duration - (-1)) > float.Epsilon)
            {
                remainingDuration -= deltaTime;
            }
            else if (remainingDuration <= 0f && !IsExpired())
            {
                // Buff just expired, end it now
                OnEnd();
                remainingDuration = 0f;
            }
        }

        public virtual void OnStart()
        {
            // Do something when the buff starts
        }

        public virtual void OnEnd()
        {
            // Do something when the buff ends
        }
    }

    public enum BuffType
    {
        Attack,
        Defense,
        Speed,
        // ...
    }
}