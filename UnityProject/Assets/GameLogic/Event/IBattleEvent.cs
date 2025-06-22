namespace AION.CoreFramework
{
    [EventInterface(EEventGroup.GroupLogic)]
    public interface IBattleEvent
    {
        void OnBattleStart();
        void OnBattleEnd();
    }
}