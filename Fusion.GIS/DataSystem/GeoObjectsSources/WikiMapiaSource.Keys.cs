using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.GIS.DataSystem.GeoObjectsSources
{
	public partial class WikiMapiaSource
	{
		public class APIKey
		{
			string key;
			public readonly TimeSpan TimeOut = new TimeSpan(0, 5, 0);
			public DateTime FirstTimeUse;

			public int useCounter;


			public APIKey(string key)
			{
				this.key = key;
			}

			/// <summary>
			/// Return value could be null if counter higher than 100
			/// </summary>
			public string Key
			{
				get
				{
					if (useCounter == 0) FirstTimeUse = DateTime.Now;

					useCounter++;
					if (useCounter > 100) {
						return null;
					} else {
						return key;
					}
				}
			}
		}

		string key00 = "DCD14FF0-22702A33-C1762636-5E6F5F08-E74383ED-BB465C11-0CF6E3A8-DBDD2C83";
		string key01 = "DCD14FF0-DF0B429B-414FC51D-C13A1818-EEAD965B-40520049-DF235498-5DDC79D";

		List<APIKey> Keys = new List<APIKey>(); 


		public void InitKeys()
		{
			Keys.Add(new APIKey(key00));
			Keys.Add(new APIKey(key01));
		}


		public void UpdateKeys(GameTime gameTime)
		{
			Keys.ForEach( x =>
				{
					if ( x.useCounter != 0 && (DateTime.Now - x.FirstTimeUse) > x.TimeOut ) {
						x.useCounter = 0;
					}
				});
		}


		public string GetKey()
		{
			string	ret = null;
			int		ind = 0;

			ret = Keys[ind].Key;
			while (ret == null && ++ind < Keys.Count) {
				ret = Keys[ind].Key;
			}

			return ret;
		}
	}
}
