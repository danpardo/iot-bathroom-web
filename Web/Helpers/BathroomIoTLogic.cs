using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IoT.Bathroom.Web.Hubs;
using Microsoft.AspNet.SignalR;
using Microsoft.ServiceBus.Messaging;

namespace IoT.Bathroom.Web.Helpers
{
	public class BathroomIoTLogic
	{
		//Prod
		static string connectionString = "HostName=bathroom-project.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=LHSlyHcccfvraewWGt0V4+p8vEqFtsyw8EIg/stZo9Q=";

		//Test
		//static string connectionString = "HostName=simulated.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=jjqgjBG5clnFFGBxzJVdTaa6nDEjMO0ZaDBfdG0Rw9s=";

		static string iotHubD2cEndpoint = "messages/events";
	    EventHubClient eventHubClient;

		private static BathroomIoTLogic instance;

		private List<string> latestUpdates = new List<string>();

		private string connectionId;

		private BathroomIoTLogic()
		{
			eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint);

			var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;

			CancellationTokenSource cts = new CancellationTokenSource();

			var tasks = new List<Task>();
			foreach (string partition in d2cPartitions)
			{
				tasks.Add(ReceiveMessagesFromDeviceAsync(partition, cts.Token));
			}
		}

		public void NotifyLatestData(string clientId)
		{
			connectionId = clientId;
			//GlobalHost.ConnectionManager.GetHubContext<BathroomHub>()
			//	.Clients.Client(clientId)
			//	.listLatests(this.latestUpdates);
		}

		public static BathroomIoTLogic Instance {
			get
			{
				if (instance == null)
				instance = new BathroomIoTLogic();
				return instance;
			}
		}	

		private async Task ReceiveMessagesFromDeviceAsync(string partition, CancellationToken ct)
		{
			var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.UtcNow);
			while (true)
			{
				if (ct.IsCancellationRequested) break;
				EventData eventData = await eventHubReceiver.ReceiveAsync();
				if (eventData == null) continue;

				string data = Encoding.UTF8.GetString(eventData.GetBytes());

				if (latestUpdates.Count == 20)
				{
					latestUpdates.RemoveAt(19);
					latestUpdates.Insert(0, data);
				}
				else
				{
					latestUpdates.Add(data);
				}

				GlobalHost.ConnectionManager.GetHubContext<BathroomHub>().Clients.All.updateStatus(data);
			}
		}
	}
}