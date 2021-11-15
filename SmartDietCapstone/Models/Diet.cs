using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartDietCapstone.Models
{
	public class Diet
	{
		public string DietId { get; set; }
		public string UserId { get; set; }
		public string SerializedDiet { get; set; }
		public string Name { get; set; }
		public Diet(string DietId_, string UserId_, string SerializedDiet_, string Name_)
		{
			this.DietId = DietId_;
			this.UserId = UserId_;
			this.SerializedDiet = SerializedDiet_;
			Name = Name_;
		}
	}

}
