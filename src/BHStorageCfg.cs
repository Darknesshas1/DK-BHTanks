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
        public int FuelTypes; //FuelTypes is either equal to one or two to say how many types of fuel there are stored in the tank. 

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
        
        [KSPField(isPersistant = true)]
        public bool BlackHoleRefueling = false;

        //EC cost to keep fuel accessible
        [KSPField(isPersistant = false)]
        public static double BHECCost = 0.0f;

        //Private Values
        private float fuel1Amount = 0.0f;
        private float fuel2Amount = 0.0f;
        private float fuel1MaxAmount = 0.0f;
        private float fuel2MaxAmount = 0.0f;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Black Hole")]
        public string BlackHoleStatus;

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
        
        [KSPEvent(guiActive = true, guiName = "Refuel Tank", active = true)]
        public void Refuel()
        {
            if (BlackHoleRefueling == true)
            {
                //Add function to be able to add fuel without being able to pull out. 
                if (Fuel2Active(false))
                {
                    part.Resources.Add(Fuel1Name, FloatToDouble(fuel1Amount), FloatToDouble(fuel1MaxAmount), true, false, false, true, (PartResource.FlowMode)2);
                    //Need to put MassAdding in here
                }
                else
                {
                    part.Resources.Add(Fuel1Name, FloatToDouble(fuel1Amount), FloatToDouble(fuel1MaxAmount), true, false, false, true, (PartResource.FlowMode)2);
                    part.Resources.Add(Fuel2Name, FloatToDouble(fuel2Amount), FloatToDouble(fuel2MaxAmount), true, false, false, true, (PartResource.FlowMode)2);
                }
            }
        }
            
        //Actions
        [KSPAction("Activate Black Hole")]
        public void EnableAction(KSPActionParam param) { Enable(); }

        [KSPAction("Deactivate Black Hole")]
        public void DisableAction(KSPActionParam param) { Disable(); }

        [KSPAction("Toggle Black Hole")]
        public void ToggleResourcesAction(KSPActionParam param) { ToggleBH(); }
        
        [KSPAction("Refuel Black Hole")]
        public void RefuelAction(KSPActionParam param) { Refuel(); }

        //Credit for the next two sections goes to Nertea, used with his permission.
        public double GetResourceAmount(string nm)
        {
          if (this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id) != null)
            return this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id).amount;
          else
            return 0.0;
        }
        public double GetResourceAmount(string nm,bool max)
        {
            if (max)
                if (this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id) != null)
                  return this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id).maxAmount;
                else
                  return 0.0;
            else
                return GetResourceAmount(nm);
        }
        //Thanks Nertea!!!

        //Used to convert floats to doubles and back again for the different types of values used
        public static float DoubleToFloat(double dValue)
        {
            if (float.IsPositiveInfinity(Convert.ToSingle(dValue)))
            {
                return float.MaxValue;
            }
            if (float.IsNegativeInfinity(Convert.ToSingle(dValue)))
            {
                return float.MinValue;
            }
            return Convert.ToSingle(dValue);
        }

        public static double FloatToDouble(float fValue)
        {
            if (double.IsPositiveInfinity(Convert.ToSingle(fValue)))
            {
                return double.MaxValue;
            }
            if (double.IsNegativeInfinity(Convert.ToSingle(fValue)))
            {
                return double.MinValue;
            }
            return Convert.ToSingle(fValue);
        }
        //End of conversions

        /*
        //Look over this section
        //Will it add mass to the tanks, and how do I activate it?
        public float GetModuleMass(float defaultMass, ModifierStagingSituation sit) //Used to add weight to the tanks
        {
            if ((Fuel1Name == "LiquidFuel") || (Fuel1Name == "Oxidizer") || (Fuel2Name == "LiquidFuel") || (Fuel2Name == "Oxidizer")){
                if (Fuel2Active(false))//Isnt able to access Fuel2Active: 
                {
                    return fuel1LastAmount / 200;
                }
                else
                {
                    float FuelBothLastAmount = fuel1LastAmount + fuel2LastAmount;
                    return FuelBothLastAmount / 200;
                }
            }//Doesnt recingize curly brace...
            else if (Fuel1Name == "Ore") //else if statement has "unreachable code" warning
            {
                if ((Fuel2Name == "LiquidFuel") || (Fuel2Name == "Oxidizer") || (Fuel2Name == "XenonGas"))
                {
                    return 0.0f;
                }
                else
                {
                    return fuel1LastAmount / 100;
                }
            }
            else
            {
                return 0;
            }
        }
        //^^^ doesn't make sense. I don't know how to implement it. 
        */
        
        //Makes Black Hole use EC
        private double Req(string res)
        {
            Debug.Log("BHT Requesting EC");
            return part.RequestResource(res, BHECCost);//Fix needed to make equal to BHECCost...
        }

        //Determines if Fuel2 is active or not. 
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

        //Runs on launch of vessel
        public void Start()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                fuel1MaxAmount = DoubleToFloat(GetResourceAmount(Fuel1Name, true));
                fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                Debug.Log("BHT Scene is flight, BHT has gotten Fuel1 values");
                if (FuelTypes == 2)
                {
                    Fuel2Active(true);
                    fuel2MaxAmount = DoubleToFloat(GetResourceAmount(Fuel2Name, true));
                    fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name));
                    Debug.Log("BHT has gotten fuel2 values");
                }
                else
                {
                    Fuel2Active(false);
                    Debug.Log("Fuel2 is not active");
                }
                DoCatchup();
            }
        }

        //Got rid of public void Update, uneeded, as we don't have a custom GUI.

        public void FixedUpdate()//If needed this can be Update()... I think...
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (Fuel2Active(false))//Only occurs if Fuel2 is non-active
                {
                    if (BlackHoleEnabled == true)
                    {
                        part.Resources.Add(Fuel1Name, FloatToDouble(fuel1Amount), FloatToDouble(fuel1MaxAmount), true, false, false, true, (PartResource.FlowMode)3);
                        Debug.Log("BHT Black Hole has been enabled");
                        BlackHoleStatus = "Enabled";
                        if (fuel1Amount == 0.0)
                        {
                            BlackHoleEnabled = false;
                            Debug.Log("BHT Black Hole is off porque fuel is out");
                            part.Resources.Remove(Fuel1Name);
                        }
                        else
                        {
                            fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                            ConsumeCharge();
                        }
                    }
                    else
                    {
                        BlackHoleStatus = "Disabled";
                        Debug.Log("BHT Black Hole has been disabled");
                        fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                        if(part.Resources.Contains(Fuel1Name))
                        {
                            fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                            part.Resources.Remove(Fuel1Name);
                        }
                    }
                }
                else//If fuel 2 is active
                {
                    part.Resources.Add(Fuel1Name, FloatToDouble(fuel1Amount), FloatToDouble(fuel1MaxAmount), true, false, false, true, (PartResource.FlowMode)3);
                    part.Resources.Add(Fuel2Name, FloatToDouble(fuel2Amount), FloatToDouble(fuel2MaxAmount), true, false, false, true, (PartResource.FlowMode)3);
                    Debug.Log("BHT Black Hole has been enabled");
                    fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                    fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name));
                    BlackHoleStatus = "Enabled";
                    if (BlackHoleEnabled == true)
                    {
                        BlackHoleStatus = "Enabled";
                        if (fuel1Amount == 0.0 && fuel2Amount == 0.0)
                        {
                            BlackHoleEnabled = false;
                            Debug.Log("BHT Black Hole is off porque fuel 1 and 2 are out");
                            BlackHoleStatus = "Disabled";
                            part.Resources.Remove(Fuel1Name);
                            part.Resources.Remove(Fuel2Name);
                        }
                        else
                        {
                            fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                            fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name));
                            ConsumeCharge();
                        }
                    }
                    else
                    {
                        BlackHoleStatus = "Disabled";
                        fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                        fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name));
                        if (part.Resources.Contains(Fuel1Name))
                        {
                            fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                            part.Resources.Remove(Fuel1Name);
                        }
                        if (part.Resources.Contains(Fuel2Name))
                        {
                            fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name));
                            part.Resources.Remove(Fuel2Name);
                        }
                    }
                }
            }
        }

        protected void ConsumeCharge()
        {
            if (TimeWarp.CurrentRate > 1f)
            {
                if (Fuel2Active(false))
                {
                    Debug.Log("BHT Black hole enabled");
                    double Ec = GetResourceAmount("ElectricCharge");
                    if (Ec <= BHECCost)
                    {
                        BlackHoleEnabled = false;
                        fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                        part.Resources.Remove(Fuel1Name);
                        Debug.Log("BHT not enough EC");
                        Debug.Log("BHT Black hole disabled");
                    }
                    else if (Ec == 0.0)
                    {
                        BlackHoleEnabled = false;
                        fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                        part.Resources.Remove(Fuel1Name);
                        Debug.Log("BHT no EC");
                        Debug.Log("BHT Black hole disabled");
                    }
                    else
                    {
                        Req("ElectricCharge");
                        Debug.Log("BHT using electric charge!");
                    }
                }
                else
                {
                    Debug.Log("BHT Black hole enabled");
                    double Ec = GetResourceAmount("ElectricCharge");
                    if (Ec <= BHECCost)
                    {
                        BlackHoleEnabled = false;
                        fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                        fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name));
                        part.Resources.Remove(Fuel1Name);
                        part.Resources.Remove(Fuel2Name);
                        Debug.Log("BHT not enough EC");
                        Debug.Log("BHT Black hole disabled");
                    }
                    else if (Ec == 0.0)
                    {
                        BlackHoleEnabled = false;
                        fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                        fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name));
                        part.Resources.Remove(Fuel1Name);
                        part.Resources.Remove(Fuel2Name);
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
            else { DoCatchup(); }
        }
        
        public void DoCatchup() //For timewarping and on load after launch
        {
            if (part.vessel.missionTime > 0.0)
            {
                if (part.RequestResource("ElectricCharge", BHECCost * TimeWarp.fixedDeltaTime) < BHECCost * TimeWarp.fixedDeltaTime)
                {
                    double elapsedTime = part.vessel.missionTime - LastUpdateTime;
                    Debug.Log("BHT Now doing catchup");
                    part.RequestResource("ElectricCharge", BHECCost * elapsedTime);
                    //No need for another request because of fuel2, otherwise it would just be double the cost
                }
            }
        }
    }
}

