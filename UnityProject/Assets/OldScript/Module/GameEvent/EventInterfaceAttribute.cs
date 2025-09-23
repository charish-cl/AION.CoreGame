using System;

namespace AION.CoreFramework
{
    /// <summary>
    /// 事件分组枚举。
    /// </summary>
    public enum EEventGroup
    {
        /// <summary>
        /// UI相关的交互。
        /// </summary>
        GroupUI,   

        /// <summary>
        /// 逻辑层内部相关的交互。
        /// </summary>
        GroupLogic,
        
        
        /// <summary>
        /// 战斗相关的交互。
        /// </summary>
        BattleLogic,
    }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class EventInterfaceAttribute : Attribute
     {
         private EEventGroup _eGroup;
         public EEventGroup EventGroup => _eGroup;
         public EventInterfaceAttribute(EEventGroup group)
         {
             _eGroup = group;
         }
    }
}
