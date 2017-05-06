using Xamarin.Forms;
using Xamarin.Forms.Maps;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System;
using System.Globalization;

using Plugin.Geolocator;

namespace GasStationLocator
{


	public partial class GasStationLocatorPage : ContentPage
	{

		private Position lastGPSPosition = new Position(-23.108631, -47.226678);
		private String searchRadius = "500";

		private List<Pin> pinList = new List<Pin>(); 



		public GasStationLocatorPage()
		{
			System.Diagnostics.Debug.WriteLine("Initialize component");
			InitializeComponent();
			getUserPosition().ContinueWith((t) =>
			{
				System.Diagnostics.Debug.WriteLine("Task call back " + t.Result.Latitude + ", " + t.Result.Longitude);
				Device.BeginInvokeOnMainThread(() =>
				{
					lastGPSPosition = t.Result;
					setMapUserPosition(t.Result);

				});

				fetchNearGasStations(t.Result,searchRadius).ContinueWith((arg) =>
				{
					System.Diagnostics.Debug.WriteLine("fetchNearGasStations result " + arg.Result.results.Count);
					Device.BeginInvokeOnMainThread(() =>
					{
						for (int i = 0; i < arg.Result.results.Count; i++)
						{
							PlaceGasStation gasStation = (PlaceGasStation)arg.Result.results[i];
							var lat = gasStation.geometry.location.lat;
							var lng = gasStation.geometry.location.lng;
							addPin(lat, lng, gasStation.name, gasStation.vicinity);
						}	

					});
				});
			});
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

		public void addPin(double lat, double lng, String name, String address)
		{
			var position = new Position(lat, lng);
			var pin = new Pin
			{
				Type = PinType.Place,
				Position = position,
				Label = name,
				Address = address
			};
			pinList.Add(pin);
			this.MyMap.Pins.Add(pin);
		}

		public void OnButtonClearAllClicked(object sender, EventArgs args)
		{
			//for (int i = 0; i < pinList.Count; i++)

			Device.BeginInvokeOnMainThread(() => 
			{
				System.Diagnostics.Debug.WriteLine("Remove all pins " + pinList.Count);
				foreach (Pin pin in pinList)
				{
					MyMap.Pins.Remove(pin);
				}
			});
		}

		public void OnPickerSelectedIndexChanged(object sender, EventArgs args)
		{
			System.Diagnostics.Debug.WriteLine("Picker selected");

			Picker picker = (Picker)sender;
			int selectedIndex = picker.SelectedIndex;

			if (selectedIndex == -1)
			{
				return;
			}

			searchRadius = picker.Items[selectedIndex];


			fetchNearGasStations(lastGPSPosition, searchRadius).ContinueWith((arg) =>
				{
					System.Diagnostics.Debug.WriteLine("fetchNearGasStations result " + arg.Result.results.Count);
					
					Device.BeginInvokeOnMainThread(() =>
					{
					
						for (int i = 0; i<arg.Result.results.Count; i++)
						{
							PlaceGasStation gasStation = (PlaceGasStation)arg.Result.results[i];
							var lat = gasStation.geometry.location.lat;
							var lng = gasStation.geometry.location.lng;
							addPin(lat, lng, gasStation.name, gasStation.vicinity);
						}	

					});
				});

		}

		public async Task<PlacesGasStationList> fetchNearGasStations(Position position, String searchRadius)
		{

			NumberFormatInfo nfi = new NumberFormatInfo();
			nfi.NumberGroupSeparator = ".";
				

			double latitude = Convert.ToDouble(position.Latitude, CultureInfo.InvariantCulture);
			double longitude = Convert.ToDouble(position.Longitude, CultureInfo.InvariantCulture);

			var requestUrlString = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?" +
				"location="+latitude.ToString(nfi)+","+longitude.ToString(nfi)+
			                        "&radius="+searchRadius+
			                        "&type=gas_station&key=AIzaSyBm6WWkdlH2_yundiSdg0bUmeE0nJqLGS8";

			System.Diagnostics.Debug.WriteLine("Request String" + requestUrlString);

			HttpWebRequest request = 
				(HttpWebRequest)HttpWebRequest.Create(
					new Uri(requestUrlString));

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
					var gasStationList = (PlacesGasStationList)JsonConvert.DeserializeObject<PlacesGasStationList>(jsonStr);
					//System.Diagnostics.Debug.WriteLine("First element " + gasStationList.gasStation[0].name);
					System.Diagnostics.Debug.WriteLine("Element count " + gasStationList.results.Count);
					//}

					return gasStationList;
					//JsonSerializer serializer = new JsonSerializer();
					//serializer.Deserialize<T>()
				}
			}
		}
	}
}
