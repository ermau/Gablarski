using System;
using Gtk;
using Gablarski.Client;
using Gablarski.Network;

public partial class MainWindow : Gtk.Window
{
	public MainWindow (string host, int port, string nickname) : base(Gtk.WindowType.Toplevel)
	{
		Build ();
		
		this.nickname = nickname;
			
		gablarski.Connected += GablarskiConnected;
		gablarski.ConnectionRejected += GablarskiConnectionRejected;
		
		gablarski.Connect (host, port);
	}

	void GablarskiConnectionRejected (object sender, RejectedConnectionEventArgs e)
	{
		MessageDialog dlg = new MessageDialog (this, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, "Connection rejected: {0}", e.Reason);
		dlg.Show();
	}

	void GablarskiConnected (object sender, EventArgs e)
	{
		gablarski.CurrentUser.Join (this.nickname, null);
	}
	
	private string nickname;
	private GablarskiClient gablarski = new GablarskiClient (new NetworkClientConnection());

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
}