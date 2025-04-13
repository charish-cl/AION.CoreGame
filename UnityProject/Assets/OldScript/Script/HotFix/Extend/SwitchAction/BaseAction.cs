using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AION.CoreFramework.SwitchAction
{
    [Serializable]
    public abstract class BaseAction
    {
        [Button("Execute", ButtonSizes.Large)]
        public abstract void Execute();
    }
}