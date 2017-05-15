/*
This plugin is meant to shut off fuel supplies when ElectricCharge is not applied, and allow access when it is. 
Credit goes to DarkenessHassFallen, LinuxGuruGamer, and Benjamin Kerman on the Kerbal Space Program Forums
*/

/*
 * Things to do: 
 *      Check the API documentation for fixing the fuel set values
 *      Implement ^^^
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
        public string Fuel2Name;

        //Last timestamp the BH was activated
        [KSPField(isPersistant = true)]
        public double LastUpdateTime = 0;

        //Whether or not BH is active
        [KSPField(isPersistant = true)]
        public bool BlackHoleEnabled = false; //This statement is also used for the EC draw, as you can't have one true without the other.

        //EC cost to keep fuel accessible
        [KSPField(isPersistant = false)]
        public static double BHECCost = 0.0f;

        //Private Values
        private double fuel1Amount = 0.0f;
        private double fuel2Amount = 0.0f;
        private double fuel1MaxAmount = 0.0f;
        private double fuel1LastAmount = 0.0f;
        private double fuel2MaxAmount = 0.0f;
        private double fuel2LastAmount = 0.0f;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Black Hole")]
        public string BlackHoleStatus;

        //RIP magic var, you served us well

        [KSPEvent(guiActive = false, guiName = "Activate Black Hole", active = true)]
        public void Enable()
        {
            BlackHoleEnabled = true;
        }

        [KSPEvent(guiActive = false, guiName = "Deactivate Black Hole", active = true)]
        public void Disable()
        {
            BlackHoleEnabled = false;
        }

        [KSPEvent(guiActive = true, guiName = "Toggle Black Hole", active = true)]
        public void ToggleBH()
        {
            BlackHoleEnabled = !BlackHoleEnabled;
        }

        //Actions
        [KSPAction("Activate Black Hole")]
        public void EnableAction(KSPActionParam param) { Enable(); }

        [KSPAction("Deactivate Black Hole")]
        public void DisableAction(KSPActionParam param) { Disable(); }

        [KSPAction("Toggle Black Hole")]
        public void ToggleResourcesAction(KSPActionParam param) { ToggleBH(); }

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
        

        private double Req(string res)
        {
            Debug.Log("BHT Requesting EC");
            return part.RequestResource(res, BHECCost);//Fix needed to make equal to BHECCost...
        }
        
        private bool Fuel2Active(bool fuel2State)
        {
            if(FuelTypes == 1)
            {
                return fuel2State = false;
            }
            else
            {
                return fuel2State = true;
            }
        }

        public void Start()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                fuel1MaxAmount = GetMaxResourceAmount(Fuel1Name);
                fuel1MaxAmount = fuel1LastAmount;
                Debug.Log("BHT Scene is flight, BHT has gotten fuel1 values");
                PartResourceList.Remove(Fuel1Name); //Removing fuel from tank
                //Need help with this. Create a variable up top, or is inline ok?
                Debug.Log("BHT Fuel1 amount set to 0.0, BH off");
                if (FuelTypes == 2)
                {
                    Fuel2Active(true);
                    fuel2MaxAmount = GetMaxResourceAmount(Fuel2Name);
                    fuel2MaxAmount = fuel2LastAmount;
                    Debug.Log("BHT has gotten fuel2 values");
                    PartResourceList.Remove(Fuel2Name); //Removing fuel from tank
                    Debug.Log("BHT Fuel2 amount set to 0.0, BH off");
                }
                else
                {
                    Fuel2Active(false);
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
                    if(Fuel2Active(true))
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
                if (Fuel2Active(false))
                {
                    fuel1Amount = GetResourceAmount(Fuel1Name);
                    if (BlackHoleEnabled == true)
                    {
                        BlackHoleStatus = "Enabled";
                        if (fuel1Amount == 0.0)
                        {
                            BlackHoleEnabled = false;
                            Debug.Log("BHT Black Hole is off porque fuel is out");
                        }
                        else
                        {
                            fuel1Amount = fuel1LastAmount;
                            ConsumeCharge();
                        }
                    }
                    else
                    {
                        BlackHoleStatus = "Disabled";
                        fuel1LastAmount = GetResourceAmount(Fuel1Name);
                        fuel1Amount = 0;
                    }
                }
                else
                {
                    fuel1Amount = GetResourceAmount(Fuel1Name);
                    fuel2Amount = GetResourceAmount(Fuel2Name);
                    if (BlackHoleEnabled == true)
                    {
                        BlackHoleStatus = "Enabled";
                        if (fuel1Amount == 0 && fuel2Amount == 0)
                        {
                            BlackHoleEnabled = false;
                            Debug.Log("BHT Black Hole is off porque fuel 1 and 2 are out");
                            BlackHoleStatus = "Disabled";
                            return;
                        }
                        else
                        {
                            fuel1Amount = fuel1LastAmount;
                            fuel2Amount = fuel2LastAmount;
                            ConsumeCharge();
                        }
                    }
                    else
                    {
                        BlackHoleStatus = "Disabled";
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
