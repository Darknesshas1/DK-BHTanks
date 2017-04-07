using System;
using KSP;

namespace BHTKSP
{
	public class ModuleBlackHoleLFO : PartModule
	{		
		//Last timestamp the BH was activated
		[KSPField(isPersistant = true)]
		public double LastUpdateTime = 0;
		
		//Whether or not BH is active
		[KSPField(isPersistant = true)]
		public bool BlackHoleEnabled = True //Could we condense these two statements into one by saying <public bool BlackHoleEnabled = FuelAccessible = true;> ?
		
		//Whether or not fuel is accessible
		[KSPField(isPersistant = true)]
		public bool FuelAccessible = BlackHoleEnabled;
		
		//EC cost to keep fuel accessible
		[KSPField(isPersistant = false)]
		public float BHECCost = 0.0f;
		
		//Private Values
		private double fuelAmount = 0.0;
		private double maxFuelAmount = 0.0;
		private double oxidizerAmount = 0.0;
		private double maxOxidizerAmount = 0.0;
		private double blackHoleECCost = 0.0;
		
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
				
				
				
			}
		}
		
		public void DoCatchup()
		{
			if (part.vessel.missionTime > 0.0)
			{
				if (part.RequestResource("ElectricCharge", BHECCost * TimeWarp.fixedDeltaTime) < BHECCost * TimeWarp.fixedDeltaTime)
				{
					double elapsedTime = part.vessel.missionTime - LastUpdateTime;
					
					double 
				}
			}
		}
	}
}






