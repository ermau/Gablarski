using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Client
{
	public class CurrentUser
		: ClientUser
	{
		internal CurrentUser()
		{
		}

		//private void OnChangeChannelResultMessage (MessageReceivedEventArgs e)
		//{
		//    var msg = (ChannelChangeResultMessage)e.Message;

		//    if (msg.Result != ChannelChangeResult.Success)
		//        return;

		//    lock (this.playerLock)
		//    {
		//        if (!this.players.ContainsKey (msg.MoveInfo.TargetUserId))
		//            return;

		//        this.players[msg.MoveInfo.TargetUserId].CurrentChannelId = msg.MoveInfo.TargetChannelId;
		//    }

		//    this.OnPlayerChangedChannnel (new ChannelChangedEventArgs (msg.MoveInfo));
		//}	
	
		// TODO: Deserialize
		//private void OnLoginResultMessage (MessageReceivedEventArgs e)
		//{
		//    var msg = (LoginResultMessage)e.Message;

		//    var args = new ReceivedLoginResultEventArgs (msg.Result, msg.UserInfo);

		//    if (!msg.Result.Succeeded || (msg.Result.Succeeded && msg.UserInfo.Nickname == this.nickname))
		//    {
		//        this.playerId = msg.UserInfo.UserId;
		//        OnLoginResult (args);
		//    }
		//    else
		//    {
		//        lock (playerLock)
		//        {
		//            this.players.Add (msg.Result.UserId, msg.UserInfo);
		//        }

		//        OnPlayerLoggedIn (args);
		//    }
		//}
	}
}