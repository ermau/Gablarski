using System;
using Gablarski.Messages;

namespace Gablarski.Client
{
	public class ReceivedChannelChannelResultEventArgs
		: EventArgs
	{
		public ReceivedChannelChannelResultEventArgs (ChannelChangeInfo moveInfo, ChannelChangeResult result)
		{
			if (moveInfo == null)
				throw new ArgumentNullException ("moveInfo");

			this.MoveInfo = moveInfo;
			this.Result = result;
		}

		/// <summary>
		/// Gets information about the move this result is for.
		/// </summary>
		public ChannelChangeInfo MoveInfo
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the result of the change channel request.
		/// </summary>
		public ChannelChangeResult Result
		{
			get;
			private set;
		}
	}
}