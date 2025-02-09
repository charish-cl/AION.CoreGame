using System;
using UnityEngine;

namespace GameLogic.Entrace
{
    public class TestProcedure:MonoBehaviour
    {
        private void Start()
        {
            GameApp.Instance.Entrance();
        }
    }
}