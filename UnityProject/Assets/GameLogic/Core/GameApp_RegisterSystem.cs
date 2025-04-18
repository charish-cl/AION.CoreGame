using System.Collections.Generic;
using GameBase;
using GameLogic;
using AION.CoreFramework;

public partial class GameApp : Singleton<GameApp>
{
    private List<ILogicSys> _listLogicMgr;

    public override void Active()
    {
        _listLogicMgr = new List<ILogicSys>();
        RegisterAllSystem();
        InitSystemSetting();
    }

    /// <summary>
    /// 设置一些通用的系统属性。
    /// </summary>
    private void InitSystemSetting()
    {

    }

    /// <summary>
    /// 注册所有逻辑系统
    /// </summary>
    private void RegisterAllSystem()
    {
        //带生命周期的单例系统。
        AddLogicSys(BehaviourSingleSystem.Instance);
        AddLogicSys(LevelSystem.Instance);
        
    }

    /// <summary>
    /// 注册逻辑系统。
    /// </summary>
    /// <param name="logicSys">ILogicSys</param>
    /// <returns></returns>
    private bool AddLogicSys(ILogicSys logicSys)
    {
        if (_listLogicMgr.Contains(logicSys))
        {
            Log.Fatal("Repeat add logic system: {0}", logicSys.GetType().Name);
            return false;
        }

        if (!logicSys.OnInit())
        {
            Log.Fatal("{0} Init failed", logicSys.GetType().Name);
            return false;
        }

        _listLogicMgr.Add(logicSys);

        return true;
    }
}