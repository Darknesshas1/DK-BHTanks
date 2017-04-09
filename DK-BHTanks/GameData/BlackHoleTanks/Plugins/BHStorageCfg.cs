using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BHTKSP
{
	public class ModuleBlackHoleLFO : PartModule
	{	
		[KSPField(isPersistant = false)]
		public string Fuel1Name;
		
		[KSPField(isPersistant = false)]
		public string Fuel2Name;
		
		//Last timestamp the BH was activated
		[KSPField(isPersistant = true)]
		public double LastUpdateTime = 0;
		
		//Whether or not BH is active
		[KSPField(isPersistant = true)]
		public bool BlackHoleEnabled = true; //This statement is also used for the EC draw, as you can't have one true without the other.
		
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
		
		public void Start()
		{
			if(HighLogic.LoadedSceneIsFlight)
			{
				fuel1MaxAmount = GetMaxResourceAmount(Fuel1Name);
				fuel2MaxAmount = GetMaxResourceAmount(Fuel2Name);
				fuel1MaxAmount = fuel1LastAmount;
				fuel2MaxAmount = fuel2LastAmount;
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
			}
		}
		
		public void Update()
		{
			if (HighLogic.LoadedSceneIsFlight)
			{
				//Linux: Not sure what to put in here. 
			}
		}
		
		protected void FixedUpdate()
		{
			if(HighLogic.LoadedSceneIsFlight)
			{
				fuel1Amount = GetResourceAmount(fuel1ResourceName);
				fuel2Amount = GetResourceAmount(fuel2ResourceName);
				if (BlackHoleEnabled = true)
					if (fuel1Amount == fuel2Amount == 0.0)
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
		
		protected void ConsumeCharge()
		{
			if(TimeWarp.CurrentRate > 5f)
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
				DoCatchup();
			}
		}
	}
}
