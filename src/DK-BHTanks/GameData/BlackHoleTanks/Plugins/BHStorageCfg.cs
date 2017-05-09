/*
This plugin is meant to shut off fuel supplies when ElectricCharge is not applied, and allow access when it is. 
Credit goes to DarkenessHassFallen, LinuxGuruGamer, and Benjamin Kerman on the Kerbal Space Program Forums
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP;

namespace BHTKSP
{
    public class ModuleBlackHole : PartModule
    {
        [KSPField(isPersistant = false)]
        public double FuelTypes; //FuelTypes is either equal to one or two to say how many types of fuel there are stored in the tank. 

        [KSPField(isPersistant = false)]
        public string Fuel1Name;

        [KSPField(isPersistant = false)]
        public bool Fuel2Active;

        [KSPField(isPersistant = false)]
        public string Fuel2Name;

        //Last timestamp the BH was activated
        [KSPField(isPersistant = true)]
        public double LastUpdateTime = 0;

        //Whether or not BH is active
        [KSPField(isPersistant = true)]
        public bool BlackHoleEnabled = false; //This statement is also used for the EC draw, as you can't have one true without the other.

        //EC cost to keep fuel accessible
        [KSPField(isPersistant = false)]
        public double BHECCost = 0.0f;

        //Private Values
        private double fuel1Amount = 0.0f;
        private double fuel2Amount = 0.0f;
        private double fuel1MaxAmount = 0.0f;
        private double fuel1LastAmount = 0.0f;
        private double fuel2MaxAmount = 0.0f;
        private double fuel2LastAmount = 0.0f;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Black Hole")]
        public string BlackHoleStatus = "N/A";

        //RIP magic var, you served us well

        [KSPEvent(guiActive = false, guiName = "Activate Black Hole", active = true)]//Without the above variable, this is not recognized...
        public void Enable()
        {
            BlackHoleEnabled = true;
        }

        [KSPEvent(guiActive = false, guiName = "Deactivate Black Hole", active = false)]
        public void Disable()
        {
            BlackHoleEnabled = false;
        }

        //Actions
        [KSPAction("Activate Black Hole")]
        public void EnableAction(KSPActionParam param) { Enable(); }

        [KSPAction("Deactivate Black Hole")]
        public void DisableAction(KSPActionParam param) { Disable(); }

        [KSPAction("Toggle Black Hole")]
        public void ToggleResourcesAction(KSPActionParam param)
        {
            BlackHoleEnabled = !BlackHoleEnabled;
        }

        //Credit for the next two sections goes to Nertea, used with his permission.
        protected double GetResourceAmount(string nm)
        {
            PartResource res = this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id);
            return res.amount;
        }
        protected double GetMaxResourceAmount(string nm)
        {
            int id = PartResourceLibrary.Instance.GetDefinition(nm).id;

            PartResource res = this.part.Resources.Get(id);

            return res.maxAmount;
        }
        //Thanks Nertea!!!

        static double Req(string res)
        {
            return Part.RequestResource(res, 0);//Fix needed to make equal to BHECCost...
            Debug.Log("BHT Requesting EC");
        }

        public void Start()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                fuel1MaxAmount = GetMaxResourceAmount(Fuel1Name);
                fuel1MaxAmount = fuel1LastAmount;

                if (FuelTypes == 2)
                {
                    Fuel2Active = true;
                    fuel2MaxAmount = fuel2LastAmount;
                }
                else
                {
                    Fuel2Active = false;
                }
                DoCatchup();
            }
        }

        public void DoCatchup()
        {
            if (part.vessel.missionTime > 0.0)
            {
                if (part.RequestResource("ElectricCharge", BHECCost * TimeWarp.fixedDeltaTime) < BHECCost * TimeWarp.fixedDeltaTime)
                {
                    double elapsedTime = part.vessel.missionTime - LastUpdateTime;
                    Debug.Log("BHT Now doing catchup");
                    part.RequestResource(Fuel1Name, BHECCost * elapsedTime);//For fuel 1
                    if(Fuel2Active == true)
                    {
                        part.RequestResource(Fuel2Name, BHECCost * elapsedTime);//Only happens if Fuel2 is active
                    }
                }
            }
        }

        //Got rid of public void Update, uneeded, as we don't have a custom GUI.

        protected void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (Fuel2Active == false)
                {
                    fuel1Amount = GetResourceAmount(Fuel1Name);
                    if (BlackHoleEnabled == true)
                        if (fuel1Amount == 0.0)
                        {
                            BlackHoleEnabled = false;
                        }
                        else
                        {
                            fuel1Amount = fuel1LastAmount;
                            ConsumeCharge();
                        }
                    else
                    {
                        fuel1LastAmount = GetResourceAmount(Fuel1Name);
                        fuel1Amount = 0;
                    }
                }
                else
                {
                    fuel1Amount = GetResourceAmount(Fuel1Name);
                    fuel2Amount = GetResourceAmount(Fuel2Name);
                    if (BlackHoleEnabled == true)
                        if (fuel1Amount == 0 && fuel2Amount == 0)
                        {
                            BlackHoleEnabled = false;
                            BHECCost = 0;
                            return;
                        }
                        else
                        {
                            fuel1Amount = fuel1LastAmount;
                            fuel2Amount = fuel2LastAmount;
                            ConsumeCharge();
                        }
                    else
                    {
                        fuel1LastAmount = GetResourceAmount(Fuel1Name);
                        fuel2LastAmount = GetResourceAmount(Fuel2Name);
                        fuel1Amount = fuel2Amount = 0;
                    }
                }
            }
        }

        protected void ConsumeCharge()
        {
            if (TimeWarp.CurrentRate > 1f)
            {
                if (BlackHoleEnabled == true)
                {
                    Debug.Log("BHT Black hole enabled");
                    double Ec = GetResourceAmount("ElectricCharge");
                    if (Ec <= BHECCost)
                    {
                        BlackHoleEnabled = false;
                        Debug.Log("BHT not enough EC");
                        Debug.Log("BHT Black hole disabled");
                    }
                    else if (Ec == 0.0)
                    {
                        BlackHoleEnabled = false;
                        Debug.Log("BHT no EC");
                        Debug.Log("BHT Black hole disabled");
                    }
                    else
                    {
                        Req("ElectricCharge");
                        Debug.Log("BHT using electric charge!");
                    }
                }
            }
            else
            {
                DoCatchup(); //Used during timewarp, alternativly could just have the BH turn off if timewarp is higher than 1x
            }
        }
    }
}
