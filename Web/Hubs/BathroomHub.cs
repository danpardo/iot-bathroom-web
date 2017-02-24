using IoT.Bathroom.Web.Helpers;
using Microsoft.AspNet.SignalR;

namespace IoT.Bathroom.Web.Hubs
{
	public class BathroomHub : Hub
	{

		public BathroomHub()
		{
		}

		public void Get()
		{
			BathroomIoTLogic.Instance.NotifyLatestData(this.Context.ConnectionId);
		}
	}
}