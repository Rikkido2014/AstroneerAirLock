using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRageMath;
using VRage.Game;
using VRage.Library;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Ingame;
using Sandbox.Common;
using Sandbox.Game;
using VRage.Collections;
using VRage.Game.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace AstroneerAirLock
{
    class Light_Test: MyGridProgram
    {
        void Main(string argument)
        {
            var light1 = GridTerminalSystem.GetBlockWithName("AirLock1-CornerLight") as IMyLightingBlock;
            var light2 = GridTerminalSystem.GetBlockWithName("AirLock1-CornerLigh-D") as IMyLightingBlock;

            light1.Color = Color.Red;
            light2.Color = Color.Red;
        }
    }
}
