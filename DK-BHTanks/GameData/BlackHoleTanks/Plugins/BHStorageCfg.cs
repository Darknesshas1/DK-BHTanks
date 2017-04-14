/*
Hey Linux, I think I have finished the plugin. 
I just have a couple of questions for you, if you search the document, I have added comments addressed to you.
If you have any suggestions, just PM me on the current thread between Darkness, you, and me. 
Thank you for all your advice! 

This plugin is meant to shut off fuel supplies when ElectricCharge is not applied, and allow access when it is. 
Credit goes to DarkenessHassFallen, LinuxGuruGamer, and Benjamin Kerman on the Kerbal Space Program Forums
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BHTKSP
{
	public class ModuleBlackHole : PartModule
	{	
		[KSPField(isPersistant = false)]
		public string FuelTypes; //FuelTypes is either equal to one or two to say what type of fuel there is stored in the tank. 
		
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
		public float BHECCost = 0.0f;
		
		//Private Values
		private double fuel1MaxAmount = 0.0f;
		private double fuel1LastAmount = 0.0f;
		private double fuel2MaxAmount = 0.0f;
		private double fuel2LastAmount = 0.0f;

		
		//UI
		[KSPField(isPersistant = false, guiActive = true, guiName = "Black Hole")]
		public string BlackHoleStatus = "N/A"
		
		[KSPEvent(guiActive = false, guiName = "Activate Black Hole", active = true)]
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
		public void ActivateBlackHole(KSPActionParam param){
			this.activateBlackHole;
		}
		
		[KSPAction("Deactivate Black Hole")]
		public void DeactivateBlackHole(KSPActionParam param){
			this.deactivateBlackHole;
		}
		
		[KSPAction("Toggle Black Hole")]
		public void ToggleResourcesAction(KSPActionParam param){
			BlackHoleEnabled = !BlackHoleEnabled
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
		
		public void Start()
		{
			if(HighLogic.LoadedSceneIsFlight)
			{
				if(FuelTypes = 1)
				{
					Fuel2Active = false;
					fuel1MaxAmount = GetMaxResourceAmount(Fuel1Name);
					fuel1MaxAmount = fuel1LastAmount;
				}
				else
				{
					Fuel2Active = true;
					fuel1MaxAmount = GetMaxResourceAmount(Fuel1Name);
					fuel2MaxAmount = GetMaxResourceAmount(Fuel2Name);
					fuel1MaxAmount = fuel1LastAmount;
					fuel2MaxAmount = fuel2LastAmount;
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
					
					part.RequestResource(BHECCost * elapsedTime);
				}
				//Linux: Is this the way for using EC during TimeWarp? 
				//We could also just have the BH deactivate during timewarp, which might be easier. 
			}
		}
		
		//Got rid of public void Update, uneeded, as we don't have a custom GUI.
		
		protected void FixedUpdate()
		{
			if(HighLogic.LoadedSceneIsFlight)
			{
				if(Fuel2Active = false)
				{
					fuel1Amount = GetResourceAmount(fuel1ResourceName);
					if (BlackHoleEnabled = true)
						if (fuel1Amount == 0.0)
						{
							BlackHoleEnabled = false;
							BHECCost = 0.0;
							return;
						}
						else
						{
							fuel1Amount = fuel1LastAmount;
							BHECCost = ConsumeCharge();
						}
					else
					{
						fuel1LastAmount = GetResourceAmount(fuel1ResourceName);
					}
				}
				else
				{
					fuel1Amount = GetResourceAmount(fuel1ResourceName); //Linux: Will this command only check for the fuel amount in the part?
					fuel2Amount = GetResourceAmount(fuel2ResourceName);
					if (BlackHoleEnabled = true)
						if (fuel1Amount == fuel2Amount == 0.0) //Can you do x == y == 1 like in here?
						{
							BlackHoleEnabled = false;
							BHECCost = 0.0;
							return;
						}
						else
						{
							fuel1Amount = fuel1LastAmount;
							fuel2Amount = fuel2LastAmount;
							BHECCost = ConsumeCharge();
						}
					else
					{
						fuel1LastAmount = GetResourceAmount(fuel1ResourceName);
						fuel2LastAmount = GetResourceAmount(fuel2ResourceName);
					}
				}
			}
		}
		
		protected void ConsumeCharge()
		{
			if(TimeWarp.CurrentRate > 1f)
			{
				if(BlackHoleEnabled = true)
				{
					double Ec = GetResourceAmount("ElectricCharge");
					double req = part.RequestResource("ElectricCharge", Ec);
					if (Ec <= BHECCost)
					{
						BlackHoleEnabled = false;
					}
					else if(Ec == 0.0)
					{
						BlackHoleEnabled = false;
					}
					else
					{
						req;
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
