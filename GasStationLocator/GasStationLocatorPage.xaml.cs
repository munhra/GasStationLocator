using Xamarin.Forms;
using Xamarin.Forms.Maps;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System;

using Plugin.Geolocator;

namespace GasStationLocator
{


	public partial class GasStationLocatorPage : ContentPage
	{
		public GasStationLocatorPage()
		{
			System.Diagnostics.Debug.WriteLine("Initialize component");
			InitializeComponent();
			getUserPosition().ContinueWith((t) =>
			{
				System.Diagnostics.Debug.WriteLine("Task call back " + t.Result.Latitude + ", " + t.Result.Longitude);
				Device.BeginInvokeOnMainThread(() =>
				{
					setMapUserPosition(t.Result);
				});
			});

			fetchNearGasStations();
			//testJsonSerialization();
			addPin();
		}

		public void testJsonSerialization()
		{
			var jsonTest = "{\n\t\"gasStation\": [{\n\t\t\"name\": \"Shell Gas Station 1\",\n\t\t\"latitude\": -23.099231,\n\t\t\"longitude\": -47.227770\n\t}, {\n\t\t\"name\": \"Shell from hell 1\",\n\t\t\"latitude\": -23.099231,\n\t\t\"longitude\": -47.227770\n\t}, {\n\t\t\"name\": \"Texaco from texas\",\n\t\t\"latitude\": -23.099231,\n\t\t\"longitude\": -47.227770\n\t}, {\n\t\t\"name\": \"Elf 1\",\n\t\t\"latitude\": -23.099231,\n\t\t\"longitude\": -47.227770\n\t}, {\n\t\t\"name\": \"Ipiranga 1\",\n\t\t\"latitude\": -23.099231,\n\t\t\"longitude\": -47.227770\n\t}]\n}";
			//var gasStationTest = (GasStation)JsonConvert.DeserializeObject<GasStation>(jsonTest);
			var gasStationList = (GasStationList)JsonConvert.DeserializeObject<GasStationList>(jsonTest);

			System.Diagnostics.Debug.WriteLine("Number of gas stations "+gasStationList.gasStation.Count);
		}
			
			

		public async Task<Position> getUserPosition()
		{
			System.Diagnostics.Debug.WriteLine("getUserPosition");
			Plugin.Geolocator.Abstractions.Position geoPosition;
			var userPosition = new Position(0, 0);
			try
			{
				var locator = CrossGeolocator.Current;
				locator.DesiredAccuracy = 100;
				geoPosition = await locator.GetPositionAsync(2000);
				userPosition = new Position(geoPosition.Latitude, geoPosition.Longitude);
				System.Diagnostics.Debug.WriteLine("user position " + userPosition.Latitude + ", " + userPosition.Longitude);

			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Failed to get user position " + ex);
			}
			return userPosition;
		}

		public void setMapUserPosition(Position position)
		{
			this.MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(position.Latitude, position.Longitude),
																	Distance.FromKilometers(1)));
		}

		public void setInitialPosition()
		{
			this.MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(-23.108631, -47.226678), Distance.FromKilometers(1)));
		}

		public void addPin()
		{
			var position = new Position(-23.099231, -47.227770);
			var pin = new Pin
			{
				Type = PinType.Place,
				Position = position,
				Label = "Gas Station XXXX",
				Address = "Oil Avenue"
			};
			this.MyMap.Pins.Add(pin);
		}

		public async Task fetchNearGasStations()
		{
			HttpWebRequest request = 
				(HttpWebRequest)HttpWebRequest.Create(
					new Uri("https://quarkbackend.com/getfile/rafael-munhoz/gasstation-json"));

			request.ContentType = "application/json";
			request.Method = "GET";


			using (WebResponse response = await request.GetResponseAsync())
			{
				using (Stream stream = response.GetResponseStream())
				{

					TextReader textReader = new StreamReader(stream);

					//using (JsonTextReader jsonReader = new JsonTextReader(textReader))
					//{
						JsonSerializer serializer = new JsonSerializer();
						//serializer.Deserialize<MapPage>(jsonReader);
						var jsonStr = textReader.ReadToEnd();
						System.Diagnostics.Debug.WriteLine(jsonStr);
						var gasStationList = (GasStationList)JsonConvert.DeserializeObject<GasStationList>(jsonStr);
						//System.Diagnostics.Debug.WriteLine("First element " + gasStationList.gasStation[0].name);
						System.Diagnostics.Debug.WriteLine("Element count " + gasStationList.gasStation.Count);
					//}


					//JsonSerializer serializer = new JsonSerializer();
					//serializer.Deserialize<T>()



				}
			
			
			}

		
		}
	}
}
