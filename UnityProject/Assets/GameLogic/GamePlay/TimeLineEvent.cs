using System;

namespace GameLogic.GamePlay
{
    public class TimeLineEvent
    {
        public string EventName { get; set; }
        public float Time { get; set; }
        public Action Action { get; set; }

        public TimeLineEvent(string eventName, float time, Action action)
        {
            EventName = eventName;
            Time = time;
            Action = action;
        }
    }
}