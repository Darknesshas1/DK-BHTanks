using System;
using KSP;

namespace BHTKSP
{
	public class BHTLFO : PartModule{
	[KSPAction("Toggle Fuel Avalibility")]
		public void ToggleResourcesAction(KSPActionParam param{
			this.toggleResources();
		}
	}
}