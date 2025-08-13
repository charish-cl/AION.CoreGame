using System;
using System.Collections.Generic;

namespace GameLogic
{
    public class CommonStateButton :BaseTabBindTool
    {
        public override void OnInit()
        {
            base.OnInit();

            Create();

            int index = 0;
            foreach (var group in BindGo)
            {
                group.TitleName  = ((EnumCommonProcessState)index++).ToString();
            }
        }
        
        
    }

}