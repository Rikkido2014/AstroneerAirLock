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

using System.Threading.Tasks;


namespace AstroneerAirLock
{
    class program : MyGridProgram
    {
        AirLockComponets componets = new AirLockComponets();

        public void Main(string argument)
        {

            List<string> arguments = argument.Split(',').ToList();
            if (arguments.Count == 1) arguments.Add("NON");

            const float AIRLOCK_OXYGEN_DEPURESSUR = 5;
            const float AIRLOCK_OXYGEN_PURESSUR = 95;

            //定義部 
            componets.airLockDoorIn = GridTerminalSystem.GetBlockWithName($"{arguments[0]}-Door-In") as IMyAirtightSlideDoor;

            componets.airLockDoorOut = GridTerminalSystem.GetBlockWithName($"{arguments[0]}-Door-Out") as IMyAirtightSlideDoor;

            componets.airLockVent = GridTerminalSystem.GetBlockWithName($"{arguments[0]}-Vent") as IMyAirVent;

            componets.lcds = GridTerminalSystem.GetBlockGroupWithName($"{arguments[0]}-lcds") as List<IMyTextPanel>;

            componets.lights = GridTerminalSystem.GetBlockGroupWithName($"{arguments[0]}-lights") as List<IMyInteriorLight>;

            float airLockOxtgenLevel = GetOxygenLevel();
            Echo($"Oxy GenLevel:{airLockOxtgenLevel}");

            if (componets.lcds != null)
            {
                componets.lcds[0].WritePublicText($"{airLockOxtgenLevel}");
            }


            //処理部******************************************************
            //OUT "外"に出る Go "Out" Side 
            //IN　"中"に入る Go "In" Side

            //ドア呼び出し用スイッチの処理
            if (arguments[1] == "SWITCH")
            {
                //ボタン呼び出し　中から外へ
                if (airLockOxtgenLevel <= AIRLOCK_OXYGEN_DEPURESSUR)
                {
                    ToggleDoors(DoorState.On);

                    componets.airLockDoorIn.CloseDoor();
                    componets.airLockDoorOut.CloseDoor();


                    componets.airLockVent.Depressurize = false;
                    componets.airLockVent.CustomData = "IN";


                }
                //外から中へ
                if (airLockOxtgenLevel >= AIRLOCK_OXYGEN_PURESSUR)
                {
                    ToggleDoors(DoorState.On);

                    componets.airLockDoorIn.CloseDoor();
                    componets.airLockDoorOut.CloseDoor();

                    componets.airLockVent.Depressurize = true;
                    componets.airLockVent.CustomData = "OUT";

                }


                return;
            }
            //外から中へ入ろうと外側のドアを閉めようとした時の処理
            if (airLockOxtgenLevel <= AIRLOCK_OXYGEN_DEPURESSUR && componets.airLockVent.Depressurize == true && componets.airLockVent.CustomData == "OUT")
            {
                if (componets.airLockDoorIn.Status == DoorStatus.Open || componets.airLockDoorIn.Status == DoorStatus.Opening) componets.airLockDoorIn.CloseDoor();

                if (componets.airLockDoorOut.Status == DoorStatus.Closed || componets.airLockDoorOut.Status == DoorStatus.Closing)
                {
                    if (componets.airLockVent.CanPressurize == false)
                    {

                        return;
                    }

                    if (componets.airLockDoorIn.Status != DoorStatus.Closed || componets.airLockDoorOut.Status != DoorStatus.Closed) return;

                    componets.airLockDoorIn.Enabled = true;
                    componets.airLockDoorOut.Enabled = false;

                    componets.airLockVent.Depressurize = false;


                }

                return;
            }
            //中から外へ出ようと内側のドアを閉めたのを検出した時
            if (airLockOxtgenLevel >= AIRLOCK_OXYGEN_PURESSUR && componets.airLockVent.Depressurize == false && componets.airLockVent.CustomData == "IN")
            {
                if (componets.airLockDoorOut.Status == DoorStatus.Open || componets.airLockDoorOut.Status == DoorStatus.Opening) componets.airLockDoorOut.CloseDoor();

                if (componets.airLockDoorIn.Status == DoorStatus.Closed || componets.airLockDoorIn.Status == DoorStatus.Closing)
                {
                    if (componets.airLockDoorIn.Status != DoorStatus.Closed || componets.airLockDoorOut.Status != DoorStatus.Closed) return;

                    componets.airLockDoorIn.Enabled = false;
                    componets.airLockDoorOut.Enabled = true;

                    componets.airLockVent.Depressurize = true;


                }

                return;
            }
            //プレイヤーが外から中へ入ろうとしている時
            if (airLockOxtgenLevel >= AIRLOCK_OXYGEN_PURESSUR && componets.airLockVent.Depressurize == false && componets.airLockVent.CustomData == "OUT")
            {
                if (componets.airLockDoorOut.Status == DoorStatus.Open || componets.airLockDoorOut.Status == DoorStatus.Opening) componets.airLockDoorOut.CloseDoor();
                if (componets.airLockDoorIn.Status == DoorStatus.Closed || componets.airLockDoorIn.Status == DoorStatus.Closing)
                {
                    if (componets.airLockDoorOut.Status != DoorStatus.Closed) return;

                    componets.airLockDoorIn.Enabled = true;
                    componets.airLockDoorOut.Enabled = false;
                    componets.airLockDoorIn.OpenDoor();


                    componets.airLockVent.CustomData = "IN";


                }

                return;
            }

            //プレイヤーが中から外へ出ようとしている時1
            if (airLockOxtgenLevel <= AIRLOCK_OXYGEN_DEPURESSUR && componets.airLockVent.Depressurize == true && componets.airLockVent.CustomData == "IN")
            {
                if (componets.airLockDoorIn.Status == DoorStatus.Open || componets.airLockDoorIn.Status == DoorStatus.Opening) componets.airLockDoorOut.CloseDoor();
                if (componets.airLockDoorOut.Status == DoorStatus.Closed || componets.airLockDoorOut.Status == DoorStatus.Closing)
                {
                    if (componets.airLockDoorIn.Status != DoorStatus.Closed) return;
                    componets.airLockDoorOut.Enabled = true;

                    componets.airLockDoorOut.OpenDoor();
                    componets.airLockDoorIn.Enabled = false;

                    componets.airLockVent.CustomData = "OUT";


                }

                return;

            }
        }
        public float GetOxygenLevel()
        {
            return componets.airLockVent.GetOxygenLevel() * 100;
        }

        public void ToggleDoors(DoorState state)
        {
            bool useDoor = true;
            if (state == DoorState.Off) useDoor = false;
            componets.airLockDoorIn.Enabled = useDoor;
            componets.airLockDoorOut.Enabled = useDoor;
        }

        void ChangeColor(List<IMyLightingBlock> lights, Color color)
        {
            foreach (var light in lights)
            {
                light.Color = color;
            }
        }
        public enum DoorState
        {
            On, Off
        }
    }
    struct AirLockComponets
    {
        public IMyAirtightSlideDoor airLockDoorIn { get; set; }//Inside Door
        public IMyAirtightSlideDoor airLockDoorOut { get; set; }//OutSide Door
        public IMyAirVent airLockVent { get; set; }// Air Vent
        public List<IMyInteriorLight> lights { get; set; }
        public List<IMyTextPanel> lcds { get; set; }
    }
}
