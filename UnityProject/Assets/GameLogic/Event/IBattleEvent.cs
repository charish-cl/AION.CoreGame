using AION.CoreFramework;

namespace GameLogic
{
    [EventInterface(EEventGroup.GroupLogic)]
    public interface IBattleEvent
    {
        void OnBattleStart();
        void OnBattleEnd();
    }
}