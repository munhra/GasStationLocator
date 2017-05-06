using System;
namespace GasStationLocator
{
	public class PlaceGasStation
	{
		public String name { get; set; }
		public String vicinity { get; set; }
		public Geometry geometry { get; set; }

		public PlaceGasStation()
		{
		}
	}

	public class Geometry
	{
		public Location location { get; set;}

		public Geometry()
		{
		
		}
	}

	public class Location
	{
		public double lat { get; set; }
		public double lng { get; set; }

		public Location()
		{
		
		}
	}
}
