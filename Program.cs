using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // Name of the group of the station's batteries
        public string myBatteryGroupName = "StationBatteriesGroup1";

        // Name of the group of the station's hydrogen tanks
        public string myHydrogenTankGroupName = "StationHydrogenTankGroup1";

        // Name of the group of the station's hydrogen engines
        public string myHydrogenEngineGroupName = "StationHydrogenEngineGroup1";

        // Percent from 0.0 to 1.0 of the minimum average battery level amoungst all batteries.
        public float minBatteryLevel = 0.25f;

        // Percent from 0.0 to 1.0 of the max average battery level amoungst all batteries.
        public float maxBatteryLevel = 0.95f;

        // Percent from 0.0 to 1.0 of the minimum average hydrogen tank filled level.
        // -1 if you don't want to keep track on hydrogen tank levels
        public double minHydrogenTankLevel = 0.25;

        // ===========================
        // DO NOT EDIT BELOW THIS LINE
        // ===========================

        MyCommandLine _commandLine = new MyCommandLine();
        List<IMyBatteryBlock> StationBatteryBlocks = new List<IMyBatteryBlock>();
        List<IMyGasTank> StationHydrogenTanks = new List<IMyGasTank>();
        List<IMyPowerProducer> StationHydrogenEngines = new List<IMyPowerProducer>();
        string State = "Battery.Empty";

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            GetComponents();
        }

        public void Save()
        {
            if (Me.CustomData == "Battery.Empty" || Me.CustomData == "Battery.Full")
            {
                Me.CustomData = State;
            }
        }

        public void GetComponents()
        {
            IMyBlockGroup BatteryGroup = GridTerminalSystem.GetBlockGroupWithName(myBatteryGroupName);
            if (BatteryGroup != null) BatteryGroup.GetBlocksOfType(StationBatteryBlocks, x => x.BlockDefinition.SubtypeName.Contains("Battery"));
            IMyBlockGroup HydrogenTankGroup = GridTerminalSystem.GetBlockGroupWithName(myHydrogenTankGroupName);
            if (HydrogenTankGroup != null) HydrogenTankGroup.GetBlocksOfType(StationHydrogenTanks, x => x.BlockDefinition.SubtypeName.Contains("Hydro"));
            IMyBlockGroup HydrogenEngineGroup = GridTerminalSystem.GetBlockGroupWithName(myHydrogenEngineGroupName);
            if (HydrogenEngineGroup != null) HydrogenEngineGroup.GetBlocksOfType(StationHydrogenEngines, x => x.BlockDefinition.SubtypeName.EndsWith("HydrogenEngine"));
        }

        public void OutputInfo()
        {
            List<float> batteryLevels = (from b in StationBatteryBlocks
                                         let a = (b.CurrentStoredPower / b.MaxStoredPower)
                                         select a).ToList();

            float percent = ComputeLevels(batteryLevels);

            List<double> hydrogenTankLevels = (from g in StationHydrogenTanks
                                               let a = g.FilledRatio
                                               select a).ToList();

            double average = ComputeLevels(hydrogenTankLevels);

            List<bool> hydrogenEnginesStates = (from e in StationHydrogenEngines
                                                let a = e.Enabled
                                                select a).ToList();

            string hydrogenEngineHeader = "Hydrogen Engine ";

            Echo("Batteries: " + Math.Round(percent * 100, 2) + "%");
            Echo("Charge: " + ComputeRange(batteryLevels).ToString() + "%");
            Echo("Hydrogen Tanks: " + Math.Round(average * 100, 2) + "%");
            foreach (bool b in hydrogenEnginesStates)
            {
                Echo(hydrogenEngineHeader + (hydrogenEnginesStates.IndexOf(b) + 1).ToString() + ": " + (b.Equals(true) ? "true" : "false"));
            }
            Echo("State: " + State);
        }

        public static float ComputeLevels(List<float> levels)
        {
            if (levels.Count == 0) return 0f;
            return levels.Average();
        }
        public static double ComputeLevels(List<double> levels)
        {
            if (levels.Count == 0) return 0D;
            return levels.Average();
        }

        public float ComputeRange(List<float> levels)
        {
            float average = ComputeLevels(levels);
            float range = maxBatteryLevel - minBatteryLevel;
            float correctedStartValue = average / 3.0f - minBatteryLevel;
            return correctedStartValue * 100 / range;
        }

        public bool CheckHydrogenLevels()
        {
            List<double> levels = (from g in StationHydrogenTanks
                                   let a = g.FilledRatio
                                   select a).ToList();

            double average = ComputeLevels(levels);

            if (average <= minHydrogenTankLevel)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool CheckBatteryLevels()
        {
            List<float> levels = (from b in StationBatteryBlocks
                                  let a = (b.CurrentStoredPower / b.MaxStoredPower)
                                  select a).ToList();

            float percent = ComputeLevels(levels);

            if (percent <= minBatteryLevel)
            {
                State = "Battery.Empty";
                return true;
            }
            else if (percent >= maxBatteryLevel)
            {
                State = "Battery.Full";
                return false;
            }
            else
            {
                return false;
            }
        }

        public void LoadData()
        {
            string[] storedData = new string[] { };
            if (Me.CustomData == "Battery.Full" || Me.CustomData == "Battery.Empty")
            {
                storedData = Me.CustomData.Split(';');
            }
            if (storedData.Length >= 1 && storedData[0] != "")
            {
                State = storedData[0];
            }
        }

        public void StartStopHydrogenEngine()
        {
            CheckBatteryLevels();
            bool hydrogenState = CheckHydrogenLevels();

            if (State == "Battery.Empty" && hydrogenState)
            {
                foreach (IMyPowerProducer p in StationHydrogenEngines)
                {
                    p.Enabled = true;
                }
            }
            else if (State == "Battery.Full" || !hydrogenState)
            {
                foreach (IMyPowerProducer p in StationHydrogenEngines)
                {
                    p.Enabled = false;
                }
            }
            OutputInfo();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            Echo(updateSource.Equals(UpdateFrequency.Update10) ? "Stopped" : "Running");

            if (_commandLine.TryParse(argument).Equals("stop"))
            {
                Runtime.UpdateFrequency = UpdateFrequency.None;
            }
            if (_commandLine.TryParse(argument).Equals("fix"))
            {
                Me.CustomData = "Battery.Empty";
            }
            LoadData();
            if ((updateSource & UpdateType.Update10) != 0 && !_commandLine.TryParse(argument).Equals("stop"))
            {
                StartStopHydrogenEngine();
            }
        }
    }
}
