using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Common {

	/// <summary>
	/// Command is structure which is sent from client to server.
	/// </summary>
	public struct UserCmd {

		/// <summary>
		/// Server time got from last received snapshot
		/// </summary>
		public int	ServerTime;

		/// <summary>
		/// Command opcode. Flags allowed.
		/// </summary>
		public int Command;

		/// <summary>
		/// Command parameter.
		/// </summary>
		public int Parameter;

		/// <summary>
		/// Spatial user orientation (yaw).
		/// </summary>
		public int	Yaw;

		/// <summary>
		/// Spatial user orientation (pitch).
		/// </summary>
		public int	Pitch;

		/// <summary>
		/// Spatial user orientation (roll).
		/// </summary>
		public int	Roll;

		/// <summary>
		/// Two-dimensional X
		/// For clicks.
		/// </summary>
		public short X;

		/// <summary>
		/// Two-dimensional Y
		/// For clicks.
		/// </summary>
		public short Y;
	}
}
