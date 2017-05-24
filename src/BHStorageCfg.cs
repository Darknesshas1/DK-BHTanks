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

        [KSPField(isPersistant = false, guiActive = true, guiName = "Refueling Status")]
        public string RefuelingStatus;

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
            BlackHoleRefueling = !BlackHoleRefueling;
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
            Debug.Log("51");
            return part.RequestResource(res, BHECCost);//Fix needed to make equal to BHECCost...
        }

        //Determines if Fuel2 is active or not. 
        private bool Fuel2Active(bool fuel2State)
        {
            Debug.Log("52");
            if (FuelTypes == 1)
            {
                Debug.Log("53");
                return fuel2State = false;
            }
            else
            {
                Debug.Log("54");
                return fuel2State = true;
            }
        }

        private void BHRefuel()
        {
            Debug.Log("41");
            if (BlackHoleRefueling == true)
            {
                Debug.Log("42");
                //Add function to be able to add fuel without being able to pull out. 
                if (Fuel2Active(false))
                {
                    Debug.Log("43");
                    if (Fuel1Present(true)) { Debug.Log("44"); }
                    //Need to put MassAdding in here
                    else
                    {
                        Debug.Log("45");
                        part.Resources.Add(Fuel1Name, FloatToDouble(fuel1Amount), FloatToDouble(fuel1MaxAmount), true, false, false, true, (PartResource.FlowMode)2);
                    }
                }
                else
                {
                    Debug.Log("46");
                    if ((Fuel2Present(true)) && (Fuel1Present(true))){ Debug.Log("47"); }
                    else if ((Fuel1Present(true)) && (Fuel2Present(false)))
                    {
                        Debug.Log("48");
                        part.Resources.Add(Fuel2Name, FloatToDouble(fuel2Amount), FloatToDouble(fuel2MaxAmount), true, false, false, true, (PartResource.FlowMode)2);
                    }
                    else if ((Fuel1Present(false)) && (Fuel2Present(true)))
                    {
                        Debug.Log("49");
                        part.Resources.Add(Fuel1Name, FloatToDouble(fuel1Amount), FloatToDouble(fuel1MaxAmount), true, false, false, true, (PartResource.FlowMode)2);
                    }
                    else
                    {
                        Debug.Log("50");
                        part.Resources.Add(Fuel1Name, FloatToDouble(fuel1Amount), FloatToDouble(fuel1MaxAmount), true, false, false, true, (PartResource.FlowMode)2);
                        part.Resources.Add(Fuel2Name, FloatToDouble(fuel2Amount), FloatToDouble(fuel2MaxAmount), true, false, false, true, (PartResource.FlowMode)2);
                    }
                }
            }
        }

        private bool Fuel1Present(bool present)
        {
            if(part.Resources.Contains(Fuel1Name))
            {
                return present = true;
            }
            else
            {
                return present = false;
            }
        }

        private bool Fuel2Present(bool present)
        {
            if(part.Resources.Contains(Fuel2Name))
            {
                return present = true;
            }
            else
            {
                return present = false;
            }
        }

        //Runs on launch of vessel
        public void Start()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                fuel1MaxAmount = DoubleToFloat(GetResourceAmount(Fuel1Name, true));
                fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                Debug.Log("0");
                if (FuelTypes == 2)
                {
                    Fuel2Active(true);
                    fuel2MaxAmount = DoubleToFloat(GetResourceAmount(Fuel2Name, true));
                    fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name));
                    Debug.Log("0.1");
                }
                else
                {
                    Fuel2Active(false);
                    Debug.Log("0.2");
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
                    Debug.Log("1");
                    if (BlackHoleEnabled == true)
                    {
                        Debug.Log("2");
                        if (Fuel1Present(true))
                        {
                            Debug.Log("3");
                            BlackHoleStatus = "Enabled";
                            if (fuel1Amount == 0.0)
                            {
                                BlackHoleEnabled = false;
                                Debug.Log("4");
                                part.Resources.Remove(Fuel1Name);
                            }
                            else
                            {
                                Debug.Log("5");
                                fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                                ConsumeCharge();
                            }
                        }
                        else
                        {
                            Debug.Log("6");
                            part.Resources.Add(Fuel1Name, FloatToDouble(fuel1Amount), FloatToDouble(fuel1MaxAmount), true, false, false, true, (PartResource.FlowMode)3);
                            BlackHoleStatus = "Enabled";
                            if (fuel1Amount == 0.0)
                            {
                                Debug.Log("7");
                                BlackHoleEnabled = false;
                                BlackHoleStatus = "Disabled";
                                part.Resources.Remove(Fuel1Name);
                            }
                            else
                            {
                                Debug.Log("8");
                                fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                                ConsumeCharge();
                            }
                        }
                    }
                    else if (BlackHoleRefueling == true)
                    {
                        Debug.Log("9");
                        BlackHoleEnabled = false;
                        BHRefuel();
                    }
                    else
                    {
                        BlackHoleStatus = "Disabled";
                        Debug.Log("10");
                        fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                        if(part.Resources.Contains(Fuel1Name))
                        {
                            Debug.Log("11");
                            fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                            part.Resources.Remove(Fuel1Name);
                        }
                    }
                }
                else//If fuel 2 is active
                {
                    part.Resources.Add(Fuel1Name, FloatToDouble(fuel1Amount), FloatToDouble(fuel1MaxAmount), true, false, false, true, (PartResource.FlowMode)3);
                    part.Resources.Add(Fuel2Name, FloatToDouble(fuel2Amount), FloatToDouble(fuel2MaxAmount), true, false, false, true, (PartResource.FlowMode)3);
                    Debug.Log("12");
                    fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                    fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name));
                    BlackHoleStatus = "Enabled";
                    if (BlackHoleEnabled == true)
                    {
                        Debug.Log("13");
                        if ((Fuel1Present(true)) && (Fuel2Present(true)))
                        {
                            Debug.Log("14");
                            BlackHoleStatus = "Enabled";
                            if ((fuel1Amount == 0.0) && (fuel2Amount == 0.0))
                            {
                                BlackHoleEnabled = false;
                                Debug.Log("15");
                                part.Resources.Remove(Fuel1Name);
                                part.Resources.Remove(Fuel2Name);
                            }
                            else
                            {
                                Debug.Log("16");
                                fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                                fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name));
                                ConsumeCharge();
                            }
                        }
                        else if ((Fuel1Present(false)) && (Fuel2Present(true)))
                        {
                            part.Resources.Add(Fuel1Name, FloatToDouble(fuel1Amount), FloatToDouble(fuel1MaxAmount), true, false, false, true, (PartResource.FlowMode)3);
                            Debug.Log("17");
                            BlackHoleStatus = "Enabled";
                            if ((fuel1Amount == 0.0) && (fuel2Amount == 0.0))
                            {
                                BlackHoleEnabled = false;
                                BlackHoleStatus = "Disabled";
                                Debug.Log("18");
                                part.Resources.Remove(Fuel1Name);
                                part.Resources.Remove(Fuel2Name);
                            }
                            else
                            {
                                Debug.Log("19");
                                fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                                fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name));
                                ConsumeCharge();
                            }
                        }
                        else if ((Fuel1Present(true)) && (Fuel2Present(false)))
                        {
                            part.Resources.Add(Fuel2Name, FloatToDouble(fuel2Amount), FloatToDouble(fuel2MaxAmount), true, false, false, true, (PartResource.FlowMode)3);
                            Debug.Log("20");
                            if ((fuel1Amount == 0.0) && (fuel2Amount == 0.0))
                            {
                                BlackHoleEnabled = false;
                                BlackHoleStatus = "Disabled";
                                Debug.Log("21");
                                part.Resources.Remove(Fuel1Name);
                                part.Resources.Remove(Fuel2Name);
                            }
                            else
                            {
                                Debug.Log("22");
                                fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                                fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name));
                                ConsumeCharge();
                            }
                        }
                        else
                        {
                            part.Resources.Add(Fuel1Name, FloatToDouble(fuel1Amount), FloatToDouble(fuel1MaxAmount), true, false, false, true, (PartResource.FlowMode)3);
                            part.Resources.Add(Fuel2Name, FloatToDouble(fuel2Amount), FloatToDouble(fuel2MaxAmount), true, false, false, true, (PartResource.FlowMode)3);
                            Debug.Log("23");
                            BlackHoleStatus = "Enabled";
                            if ((fuel1Amount == 0.0) && (fuel2Amount == 0.0))
                            {
                                BlackHoleEnabled = false;
                                BlackHoleStatus = "Disabled";
                                Debug.Log("24");
                                part.Resources.Remove(Fuel1Name);
                                part.Resources.Remove(Fuel2Name);
                            }
                            else
                            {
                                Debug.Log("25");
                                fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                                fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name));
                                ConsumeCharge();
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("26");
                        BlackHoleStatus = "Disabled";
                        fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                        fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name));
                        if (part.Resources.Contains(Fuel1Name))
                        {
                            Debug.Log("27");
                            fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                            part.Resources.Remove(Fuel1Name);
                        }
                        if (part.Resources.Contains(Fuel2Name))
                        {
                            Debug.Log("28");
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
                    Debug.Log("29");
                    double Ec = GetResourceAmount("ElectricCharge");
                    if (Ec <= BHECCost)
                    {
                        BlackHoleEnabled = false;
                        fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                        part.Resources.Remove(Fuel1Name);
                        Debug.Log("30");
                    }
                    else if (Ec == 0.0)
                    {
                        BlackHoleEnabled = false;
                        fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                        part.Resources.Remove(Fuel1Name);
                        Debug.Log("31");
                    }
                    else
                    {
                        Req("ElectricCharge");
                        Debug.Log("32");
                    }
                }
                else
                {
                    Debug.Log("33");
                    double Ec = GetResourceAmount("ElectricCharge");
                    if (Ec <= BHECCost)
                    {
                        BlackHoleEnabled = false;
                        fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                        fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name));
                        part.Resources.Remove(Fuel1Name);
                        part.Resources.Remove(Fuel2Name);
                        Debug.Log("34");
                    }
                    else if (Ec == 0.0)
                    {
                        BlackHoleEnabled = false;
                        fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name));
                        fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name));
                        part.Resources.Remove(Fuel1Name);
                        part.Resources.Remove(Fuel2Name);
                        Debug.Log("35");
                    }
                    else
                    {
                        Req("ElectricCharge");
                        Debug.Log("36");
                    }
                }
            }
            else
            {
                DoCatchup();
                Debug.Log("37");
            }
        }
        
        public void DoCatchup() //For timewarping and on load after launch
        {
            Debug.Log("38");
            if (part.vessel.missionTime > 0.0)
            {
                Debug.Log("39");
                if (part.RequestResource("ElectricCharge", BHECCost * TimeWarp.fixedDeltaTime) < BHECCost * TimeWarp.fixedDeltaTime)
                {
                    double elapsedTime = part.vessel.missionTime - LastUpdateTime;
                    Debug.Log("40");
                    part.RequestResource("ElectricCharge", BHECCost * elapsedTime);
                    //No need for another request because of fuel2, otherwise it would just be double the cost
                }
            }
        }
    }
}

