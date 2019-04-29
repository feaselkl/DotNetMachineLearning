using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetMachineLearning.Models
{
	public enum Location
	{
		Home,
		Away
	}

	public class BillsViewModel
	{
		[Display(Name = "Quarterback")]
		public string Quarterback { get; set; }
		[Display(Name = "Location")]
		public Location Location { get; set; }
		[Display(Name = "Number of Points Scored")]
		public float NumberOfPointsScored { get; set; }
		[Display(Name = "Top Receiver")]
		public string TopReceiver { get; set; }
		[Display(Name = "Top Runner")]
		public string TopRunner { get; set; }
	}
}
