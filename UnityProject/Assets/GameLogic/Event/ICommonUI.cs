using AION.CoreFramework;
using GameConfig.item;

namespace GameLogic
{
    [EventInterface(EEventGroup.GroupUI)]
    //一般这种事件都是不影响当前流程的和执行顺序的，需要确认取消的，不调用这个
    public interface ICommonUI
    {
        //提示
        void ShowTip(string tip);
        
        //错误
        void ShowError(string error);

        //奖励
        void ShowReward(RewardData reward);
        
        //公告
        void ShowAnnouncement(string announcement);
    }
}