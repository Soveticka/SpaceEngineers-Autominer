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
        /*###################################################################
        #   Script Name: Automatic Mining Rig - Script v1
        #   Author: Soveticka / Alias: Matěj Kománek
        #   My Twitch: https://twitch.tv/soveticka
        #   My Webpage: https://soveticka.cz/

        #####################################################################
        #   Description:
        #   Basic script for automining 

        #####################################################################
        */

        // ##  Basic settings ## //

        //Block name tags - Names cant be same, so if u have 2 drills the names should look like - [Drill-Auto] Drill1 and [Drill-Auto] Drill2
        string drillNameTag = "[Drill-Auto]";
        string pistonNameTag = "[Piston-Auto]";
        string rotorNameTag = "[Rotor-Auto]";

        //Self updating - needed for piston state checking. RECOMMENDED!
        bool selfUpdating = true;

        //How often will the script auto update >> 1 = 1second || 2 = 2seconds
        int selfUpdatingDelay = 2;
        public Program()
        {
            if (selfUpdating)
            {
                Runtime.UpdateFrequency = UpdateFrequency.Update10;
            }
        }

        //Essential settings for autoUpdate - don't change this!
        int selfUpSysCountTimes = 5;
        int selfUpSysCounter = 0;

        //Progress Checking - don't change this!
        bool isMiningDown = false;
        bool finishedMining = false;
        bool isExtended = false;
        bool workFinished = false;

        // Dont change anything below!

        // Methods
        public float getMaxPistonExtend(List<IMyTerminalBlock> pistonList, bool extender)
        {
            float maxPistonExtend = 0f;
            if (!extender)
            {
                for (int i = 0; i < pistonList.Count; i++)
                {
                    IMyPistonBase pistonList_X = GridTerminalSystem.GetBlockWithName(pistonList[i].CustomName) as IMyPistonBase;
                    if (pistonList_X.CustomName.Contains(pistonNameTag) && !pistonList_X.CustomName.Contains("Extender"))
                    {
                        maxPistonExtend += pistonList_X.HighestPosition;
                    }
                }
            }
            else
            {
                for (int i = 0; i < pistonList.Count; i++)
                {
                    IMyPistonBase pistonList_X = GridTerminalSystem.GetBlockWithName(pistonList[i].CustomName) as IMyPistonBase;
                    if (pistonList_X.CustomName.Contains(pistonNameTag) && pistonList_X.CustomName.Contains("Extender"))
                    {
                        maxPistonExtend += pistonList_X.HighestPosition;
                    }
                }
            }
            return maxPistonExtend;
        }

        public float getActualPistonExtend(List<IMyTerminalBlock> pistonList, bool extender)
        {
            float actualPistonExtend = 0f;
            if (!extender)
            {
                for (int i = 0; i < pistonList.Count; i++)
                {
                    IMyPistonBase pistonList_X = GridTerminalSystem.GetBlockWithName(pistonList[i].CustomName) as IMyPistonBase;
                    if (pistonList_X.CustomName.Contains(pistonNameTag) && !pistonList_X.CustomName.Contains("Extender"))
                    {
                        actualPistonExtend += pistonList_X.CurrentPosition;
                    }
                }
            }
            else
            {
                for (int i = 0; i < pistonList.Count; i++)
                {
                    IMyPistonBase pistonList_X = GridTerminalSystem.GetBlockWithName(pistonList[i].CustomName) as IMyPistonBase;
                    if (pistonList_X.CustomName.Contains(pistonNameTag) && pistonList_X.CustomName.Contains("Extender"))
                    {
                        actualPistonExtend += pistonList_X.CurrentPosition;
                    }
                }
            }
            return actualPistonExtend;
        }

        public void startMining(List<IMyTerminalBlock> drillList, List<IMyTerminalBlock> pistonList, List<IMyTerminalBlock> rotorList)
        {
            if (!isMiningDown && !workFinished)
            {
                //Piston settings
                float maxPistonExtend = getMaxPistonExtend(pistonList, false);
                float pistonSpeed = 0.003f;

                //Rotor settings
                float rotorTorque = 33599988f;
                float rotorVelocityArm = 1f;
                float rotorVelocityHead = 3f;

                isMiningDown = true;

                //Pasting drill settings & enabling them
                for (int i = 0; i < drillList.Count; i++)
                {
                    IMyShipDrill drillList_X = GridTerminalSystem.GetBlockWithName(drillList[i].CustomName) as IMyShipDrill;
                    if (drillList_X.CustomName.Contains(drillNameTag))
                    {
                        drillList_X.Enabled = true;
                    }
                }

                //Pasting piston settings & enabling them
                for (int i = 0; i < pistonList.Count; i++)
                {
                    IMyPistonBase pistonList_X = GridTerminalSystem.GetBlockWithName(pistonList[i].CustomName) as IMyPistonBase;
                    if (pistonList_X.CustomName.Contains(pistonNameTag) && !pistonList_X.CustomName.Contains("Extender"))
                    {
                        pistonList_X.SetValue("Velocity", pistonSpeed);
                        pistonList_X.Enabled = true;
                    }
                }

                //Pasting advanced rotor settings & enabling them
                for (int i = 0; i < rotorList.Count; i++)
                {
                    IMyMotorAdvancedStator rotorList_X = GridTerminalSystem.GetBlockWithName(rotorList[i].CustomName) as IMyMotorAdvancedStator;
                    if (rotorList_X.CustomName.Contains(rotorNameTag))
                    {
                        if (rotorList_X.CustomName.Contains("Arm"))
                        {
                            rotorList_X.TargetVelocityRPM = rotorVelocityArm;
                            rotorList_X.Torque = rotorTorque;
                            rotorList_X.BrakingTorque = rotorTorque;
                            rotorList_X.Enabled = true;
                        }
                        if (rotorList_X.CustomName.Contains("Head"))
                        {
                            rotorList_X.Torque = rotorTorque;
                            rotorList_X.BrakingTorque = rotorTorque;
                            rotorList_X.TargetVelocityRPM = rotorVelocityHead;
                            rotorList_X.Enabled = true;
                        }
                        Echo("Rotor: " + i.ToString());
                    }
                }
            }
        }

        public void retractPistons(List<IMyTerminalBlock> pistonList)
        {
            if (getActualPistonExtend(pistonList, false) == getMaxPistonExtend(pistonList, false) && !finishedMining) { }
            {
                finishedMining = true;
                for (int i = 0; i < pistonList.Count; i++)
                {
                    IMyPistonBase pistonList_X = GridTerminalSystem.GetBlockWithName(pistonList[i].CustomName) as IMyPistonBase;
                    if (pistonList_X.CustomName.Contains(pistonNameTag) && !pistonList_X.CustomName.Contains("Extender"))
                    {
                        pistonList_X.Reverse();
                    }
                }
            }
        }

        public void startMininingStage2(List<IMyTerminalBlock> drillList, List<IMyTerminalBlock> pistonList, List<IMyTerminalBlock> rotorList)
        {
            if (!workFinished && isMiningDown && finishedMining)
            {
                if (getActualPistonExtend(pistonList, false) == 0f)
                {
                    for (int i = 0; i < pistonList.Count; i++)
                    {
                        IMyPistonBase pistonList_X = GridTerminalSystem.GetBlockWithName(pistonList[i].CustomName) as IMyPistonBase;
                        if (pistonList_X.CustomName.Contains(pistonNameTag) && pistonList_X.CustomName.Contains("Extender"))
                        {
                            pistonList_X.SetValue("Velocity", 1f);
                            pistonList_X.Enabled = true;
                        }
                    }
                }
                if (getActualPistonExtend(pistonList, true) == getMaxPistonExtend(pistonList, true))
                {
                    isMiningDown = false;
                    finishedMining = false;
                    startMining(drillList, pistonList, rotorList);
                    isExtended = true;
                }
            }
        }

        public void workIsFinished(List<IMyTerminalBlock> drillList, List<IMyTerminalBlock> pistonList, List<IMyTerminalBlock> rotorList)
        {
            if (getActualPistonExtend(pistonList, false) == 0 && isExtended && finishedMining)
            {
                for (int i = 0; i < drillList.Count; i++)
                {
                    IMyShipDrill drillList_X = GridTerminalSystem.GetBlockWithName(drillList[i].CustomName) as IMyShipDrill;
                    if (drillList_X.CustomName.Contains(drillNameTag))
                    {
                        drillList_X.Enabled = false;
                    }
                }

                for (int i = 0; i < pistonList.Count; i++)
                {
                    IMyPistonBase pistonList_X = GridTerminalSystem.GetBlockWithName(pistonList[i].CustomName) as IMyPistonBase;
                    if (pistonList_X.CustomName.Contains(pistonNameTag) && !pistonList_X.CustomName.Contains("Extender"))
                    {
                        pistonList_X.Enabled = false;
                    }
                }

                for (int i = 0; i < rotorList.Count; i++)
                {
                    IMyMotorAdvancedStator rotorList_X = GridTerminalSystem.GetBlockWithName(rotorList[i].CustomName) as IMyMotorAdvancedStator;
                    if (rotorList_X.CustomName.Contains(rotorNameTag))
                    {
                        rotorList_X.Enabled = false;
                    }
                }
                Echo("Work is finished");
                workFinished = true;
            }
        }





        public void Main(string argument, UpdateType updateSource)
        {
            bool Run_ThisScript = false;
            if (selfUpdating)
            {
                if (selfUpSysCounter == 0)
                {
                    selfUpSysCounter = selfUpSysCountTimes * selfUpdatingDelay;
                    Run_ThisScript = true;
                }
                if (!Run_ThisScript)
                {
                    selfUpSysCounter -= 1;
                }
            }
            else
            {
                Run_ThisScript = true;
            }

            if (Run_ThisScript && !workFinished)
            {
                var drillList = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(drillList);

                var pistonList = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyPistonBase>(pistonList);

                var rotorList = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyMotorAdvancedStator>(rotorList);

                startMining(drillList, pistonList, rotorList);
                retractPistons(pistonList);
                startMininingStage2(drillList, pistonList, rotorList);
                workIsFinished(drillList, pistonList, rotorList);
            }
        }//End of the Code
    }
}
