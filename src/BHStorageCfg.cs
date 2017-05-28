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
        [KSPField(isPersistant = true)]
        public int FuelTypes; //FuelTypes is either equal to one or two to say how many types of fuel there are stored in the tank. 

        [KSPField(isPersistant = true)]
        public string Fuel1Name;

        [KSPField(isPersistant = true)]
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
        [KSPField]
        private float fuel1Amount = 0.0f;

        [KSPField]
        private float fuel2Amount = 0.0f;

        [KSPField]
        private float fuel1MaxAmount = 0.0f;

        [KSPField]
        private float fuel2MaxAmount = 0.0f;

        [KSPField(isPersistant = true, guiActive = true, guiName = "Black Hole")]
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
        public double GetResourceAmount(string nm, bool max)
        {
            if (max)
            {
                if (this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id) != null)
                {
                    Debug.Log("[BHT] GMRA");
                    return this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id).maxAmount;
                }
                else
                {
                    return 0.0;
                }
            }
            else
            {
                if (this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id) != null)
                {
                    Debug.Log("[BHT] GRA1");
                    return this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id).amount;
                }
                else
                {
                    return 0.0;
                }
            }
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
            Debug.Log("[BHT]51");
            return part.RequestResource(res, BHECCost);//Fix needed to make equal to BHECCost...
        }

        //Determines if Fuel2 is active or not. 
        private bool Fuel2Active(bool active)
        {
            Debug.Log("[BHT]52");
            if (active == false)
            {
                Debug.Log("[BHT]53");
                return false;
            }
            else
            {
                Debug.Log("[BHT]54");
                return true;
            }
        }

        private void BHRefuel()
        {
            Debug.Log("[BHT]41");
            if (BlackHoleRefueling == true)
            {
                Debug.Log("[BHT]42");
                //Add function to be able to add fuel without being able to pull out. 
                if (Fuel2Active(false))
                {
                    Debug.Log("[BHT]43");
                    if (Fuel1Present(true)) { Debug.Log("[BHT]44"); }
                    //Need to put MassAdding in here
                    else
                    {
                        Debug.Log("[BHT]45");
                        part.Resources.Add(Fuel1Name, FloatToDouble(fuel1Amount), FloatToDouble(fuel1MaxAmount), true, false, false, true, (PartResource.FlowMode)2);
                    }
                }
                else
                {
                    Debug.Log("[BHT]46");
                    if ((Fuel2Present(true)) && (Fuel1Present(true))) { Debug.Log("[BHT]47"); }
                    else if ((Fuel1Present(true)) && (Fuel2Present(false)))
                    {
                        Debug.Log("[BHT]48");
                        part.Resources.Add(Fuel2Name, FloatToDouble(fuel2Amount), FloatToDouble(fuel2MaxAmount), true, false, false, true, (PartResource.FlowMode)2);
                    }
                    else if ((Fuel1Present(false)) && (Fuel2Present(true)))
                    {
                        Debug.Log("[BHT]49");
                        part.Resources.Add(Fuel1Name, FloatToDouble(fuel1Amount), FloatToDouble(fuel1MaxAmount), true, false, false, true, (PartResource.FlowMode)2);
                    }
                    else
                    {
                        Debug.Log("[BHT]50");
                        part.Resources.Add(Fuel1Name, FloatToDouble(fuel1Amount), FloatToDouble(fuel1MaxAmount), true, false, false, true, (PartResource.FlowMode)2);
                        part.Resources.Add(Fuel2Name, FloatToDouble(fuel2Amount), FloatToDouble(fuel2MaxAmount), true, false, false, true, (PartResource.FlowMode)2);
                    }
                }
            }
        }

        private bool Fuel1Present(bool present)
        {
            if (part.Resources.Contains(Fuel1Name))
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
            if (part.Resources.Contains(Fuel2Name))
            {
                return present = true;
            }
            else
            {
                return present = false;
            }
        }
        
        PartResource[] GetResourceList(PartResourceList resourceList)
        {
            DictionaryValueList<int, PartResource> prl = resourceList.dict;
            PartResource[] resources = new PartResource[prl.Count];
            prl.Values.CopyTo(resources, 0);
            return resources;
        }
        
        //Runs on launch of vessel
        public void Start()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                PartResourceList BHT = this.part.Resources;
                PartResource[] res = GetResourceList(BHT);
                Fuel1Name = res[0].resourceName;
                if (Fuel2Active(true))
                {
                    FuelTypes = 2;
                }
                Debug.Log("[BHT]0.0 " + Fuel1Name);
                Debug.Log("[BHT]0.2 " + FuelTypes);
                fuel1MaxAmount = DoubleToFloat(GetResourceAmount(Fuel1Name, true));
                fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name, false));
                Debug.Log("[BHT]0");
                if (FuelTypes == 2)
                {
                    Fuel2Name = res[1].resourceName;
                    fuel2MaxAmount = DoubleToFloat(GetResourceAmount(Fuel2Name, true));
                    fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name, false));
                    Debug.Log("[BHT]0.1" + Fuel2Name);
                }
                BlackHoleStatus = "Disabled";
                DoCatchup();
            }
        }

        public void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (Fuel2Active(false))//Only occurs if Fuel2 is non-active
                {
                    Debug.Log("[BHT]1");
                    if (BlackHoleEnabled == true)
                    {
                        BlackHoleStatus = "Enabled";
                        Debug.Log("[BHT]2");
                        part.Resources.Add(Fuel1Name, FloatToDouble(fuel1Amount), FloatToDouble(fuel1MaxAmount), true, false, false, true, (PartResource.FlowMode)3);
                        if (Fuel1Present(true))
                        {
                            Debug.Log("[BHT]3");
                            if (fuel1Amount == 0.0)
                            {
                                BlackHoleEnabled = false;
                                BlackHoleStatus = "Disabled";
                                Debug.Log("[BHT]4");
                                part.Resources.Remove(Fuel1Name);
                            }
                            else
                            {
                                Debug.Log("[BHT]5");
                                fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name, false));
                                ConsumeCharge();
                            }
                        }
                        else
                        {
                            Debug.Log("[BHT]6");
                            if (fuel1Amount == 0.0)
                            {
                                Debug.Log("[BHT]7");
                                BlackHoleEnabled = false;
                                BlackHoleStatus = "Disabled";
                                part.Resources.Remove(Fuel1Name);
                            }
                            else
                            {
                                Debug.Log("[BHT]8");
                                fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name, false));
                                ConsumeCharge();
                            }
                        }
                    }
                    else if (BlackHoleRefueling == true)
                    {
                        Debug.Log("[BHT]9");
                        BlackHoleEnabled = false;
                        BlackHoleStatus = "Refueling";
                        if (Fuel1Present(true)) { Debug.Log("[BHT]44"); }
                        //Need to put MassAdding in here
                        else
                        {
                            Debug.Log("[BHT]45");
                            part.Resources.Add(Fuel1Name, FloatToDouble(fuel1Amount), FloatToDouble(fuel1MaxAmount), true, false, false, true, (PartResource.FlowMode)2);
                        }
                    }
                    else
                    {
                        BlackHoleEnabled = false;
                        BlackHoleStatus = "Disabled";
                        Debug.Log("[BHT]10");
                        fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name, false));
                        if (part.Resources.Contains(Fuel1Name))
                        {
                            Debug.Log("[BHT]11");
                            fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name, false));
                            part.Resources.Remove(Fuel1Name);
                        }
                    }
                }
                else//If fuel 2 is active
                {
                    Debug.Log("[BHT]1.5");
                    if (BlackHoleEnabled == true)
                    {
                        BlackHoleStatus = "Enabled";
                        Debug.Log("[BHT]13");
                        if ((Fuel1Present(true)) && (Fuel2Present(true)))
                        {
                            Debug.Log("[BHT]14");
                            if ((fuel1Amount == 0.0) && (fuel2Amount == 0.0))
                            {
                                BlackHoleEnabled = false;
                                BlackHoleStatus = "Disabled";
                                Debug.Log("[BHT]15");
                                part.Resources.Remove(Fuel1Name);
                                part.Resources.Remove(Fuel2Name);
                            }
                            else
                            {
                                Debug.Log("[BHT]16");
                                fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name, false));
                                fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name, false));
                                ConsumeCharge();
                            }
                        }
                        else if ((Fuel1Present(false)) && (Fuel2Present(true)))
                        {
                            part.Resources.Add(Fuel1Name, FloatToDouble(fuel1Amount), FloatToDouble(fuel1MaxAmount), true, false, false, true, (PartResource.FlowMode)3);
                            Debug.Log("[BHT]17");
                            if ((fuel1Amount == 0.0) && (fuel2Amount == 0.0))
                            {
                                BlackHoleEnabled = false;
                                BlackHoleStatus = "Disabled";
                                Debug.Log("[BHT]18");
                                part.Resources.Remove(Fuel1Name);
                                part.Resources.Remove(Fuel2Name);
                            }
                            else
                            {
                                Debug.Log("[BHT]19");
                                fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name, false));
                                fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name, false));
                                ConsumeCharge();
                            }
                        }
                        else if ((Fuel1Present(true)) && (Fuel2Present(false)))
                        {
                            part.Resources.Add(Fuel2Name, FloatToDouble(fuel2Amount), FloatToDouble(fuel2MaxAmount), true, false, false, true, (PartResource.FlowMode)3);
                            Debug.Log("[BHT]20");
                            if ((fuel1Amount == 0.0) && (fuel2Amount == 0.0))
                            {
                                BlackHoleEnabled = false;
                                BlackHoleStatus = "Disabled";
                                Debug.Log("[BHT]21");
                                part.Resources.Remove(Fuel1Name);
                                part.Resources.Remove(Fuel2Name);
                            }
                            else
                            {
                                Debug.Log("[BHT]22");
                                fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name, false));
                                fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name, false));
                                ConsumeCharge();
                            }
                        }
                        else
                        {
                            part.Resources.Add(Fuel1Name, FloatToDouble(fuel1Amount), FloatToDouble(fuel1MaxAmount), true, false, false, true, (PartResource.FlowMode)3);
                            part.Resources.Add(Fuel2Name, FloatToDouble(fuel2Amount), FloatToDouble(fuel2MaxAmount), true, false, false, true, (PartResource.FlowMode)3);
                            Debug.Log("[BHT]23");
                            if ((fuel1Amount == 0.0) && (fuel2Amount == 0.0))
                            {
                                BlackHoleEnabled = false;
                                BlackHoleStatus = "Disabled";
                                Debug.Log("[BHT]24");
                                part.Resources.Remove(Fuel1Name);
                                part.Resources.Remove(Fuel2Name);
                            }
                            else
                            {
                                Debug.Log("[BHT]25");
                                fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name, false));
                                fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name, false));
                                ConsumeCharge();
                            }
                        }
                    }
                    else if(BlackHoleRefueling == true)
                    {
                        Debug.Log("[BHT]46");
                        BlackHoleEnabled = false;
                        BlackHoleStatus = "Refueling";
                        if ((Fuel2Present(true)) && (Fuel1Present(true))) { Debug.Log("[BHT]47"); }
                        else if ((Fuel1Present(true)) && (Fuel2Present(false)))
                        {
                            Debug.Log("[BHT]48");
                            part.Resources.Add(Fuel2Name, FloatToDouble(fuel2Amount), FloatToDouble(fuel2MaxAmount), true, false, false, true, (PartResource.FlowMode)2);
                        }
                        else if ((Fuel1Present(false)) && (Fuel2Present(true)))
                        {
                            Debug.Log("[BHT]49");
                            part.Resources.Add(Fuel1Name, FloatToDouble(fuel1Amount), FloatToDouble(fuel1MaxAmount), true, false, false, true, (PartResource.FlowMode)2);
                        }
                        else
                        {
                            Debug.Log("[BHT]50");
                            part.Resources.Add(Fuel1Name, FloatToDouble(fuel1Amount), FloatToDouble(fuel1MaxAmount), true, false, false, true, (PartResource.FlowMode)2);
                            part.Resources.Add(Fuel2Name, FloatToDouble(fuel2Amount), FloatToDouble(fuel2MaxAmount), true, false, false, true, (PartResource.FlowMode)2);
                        }
                    }
                    else
                    {
                        BlackHoleEnabled = false;
                        BlackHoleStatus = "Disabled";
                        Debug.Log("[BHT]26");
                        fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name, false));
                        fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name, false));
                        if (part.Resources.Contains(Fuel1Name))
                        {
                            Debug.Log("[BHT]27");
                            fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name, false));
                            part.Resources.Remove(Fuel1Name);
                        }
                        if (part.Resources.Contains(Fuel2Name))
                        {
                            Debug.Log("[BHT]28");
                            fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name, false));
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
                    Debug.Log("[BHT]29");
                    double Ec = GetResourceAmount("ElectricCharge", false);
                    if (Ec <= BHECCost)
                    {
                        BlackHoleEnabled = false;
                        fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name, false));
                        part.Resources.Remove(Fuel1Name);
                        Debug.Log("[BHT]30");
                    }
                    else if (Ec == 0.0)
                    {
                        BlackHoleEnabled = false;
                        fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name, false));
                        part.Resources.Remove(Fuel1Name);
                        Debug.Log("[BHT]31");
                    }
                    else
                    {
                        Req("ElectricCharge");
                        Debug.Log("[BHT]32");
                    }
                }
                else
                {
                    Debug.Log("[BHT]33");
                    double Ec = GetResourceAmount("ElectricCharge", false);
                    if (Ec <= BHECCost)
                    {
                        BlackHoleEnabled = false;
                        fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name, false));
                        fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name, false));
                        part.Resources.Remove(Fuel1Name);
                        part.Resources.Remove(Fuel2Name);
                        Debug.Log("[BHT]34");
                    }
                    else if (Ec == 0.0)
                    {
                        BlackHoleEnabled = false;
                        fuel1Amount = DoubleToFloat(GetResourceAmount(Fuel1Name, false));
                        fuel2Amount = DoubleToFloat(GetResourceAmount(Fuel2Name, false));
                        part.Resources.Remove(Fuel1Name);
                        part.Resources.Remove(Fuel2Name);
                        Debug.Log("[BHT]35");
                    }
                    else
                    {
                        Req("ElectricCharge");
                        Debug.Log("[BHT]36");
                    }
                }
            }
            else
            {
                DoCatchup();
                Debug.Log("[BHT]37");
            }
        }

        public void DoCatchup() //For timewarping and on load after launch
        {
            Debug.Log("[BHT]38");
            if (part.vessel.missionTime > 0.0)
            {
                Debug.Log("[BHT]39");
                if (part.RequestResource("ElectricCharge", BHECCost * TimeWarp.fixedDeltaTime) < BHECCost * TimeWarp.fixedDeltaTime)
                {
                    double elapsedTime = part.vessel.missionTime - LastUpdateTime;
                    Debug.Log("[BHT]40");
                    part.RequestResource("ElectricCharge", BHECCost * elapsedTime);
                    //No need for another request because of fuel2, otherwise it would just be double the cost
                }
            }
        }
    }
}

