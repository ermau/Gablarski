//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4927
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------



public partial class MainWindow {
    
    private Gtk.UIManager UIManager;
    
    private Gtk.Action DisconnectedAction;
    
    private Gtk.Action SettingsAction;
    
    private Gtk.VBox vbox1;
    
    private Gtk.Toolbar toolbar;
    
    private Gtk.ScrolledWindow GtkScrolledWindow;
    
    private Gtk.TreeView treeview1;
    
    protected virtual void Build() {
        Stetic.Gui.Initialize(this);
        // Widget MainWindow
        this.UIManager = new Gtk.UIManager();
        Gtk.ActionGroup w1 = new Gtk.ActionGroup("Default");
        this.DisconnectedAction = new Gtk.Action("DisconnectedAction", null, null, "Disconnected");
        this.DisconnectedAction.Sensitive = false;
        w1.Add(this.DisconnectedAction, null);
        this.SettingsAction = new Gtk.Action("SettingsAction", null, null, "Settings");
        w1.Add(this.SettingsAction, null);
        this.UIManager.InsertActionGroup(w1, 0);
        this.AddAccelGroup(this.UIManager.AccelGroup);
        this.Name = "MainWindow";
        this.Title = Mono.Unix.Catalog.GetString("Gablarski");
        this.Icon = Gdk.Pixbuf.LoadFromResource("GablarskiGtk.Headphones.ico");
        this.WindowPosition = ((Gtk.WindowPosition)(4));
        // Container child MainWindow.Gtk.Container+ContainerChild
        this.vbox1 = new Gtk.VBox();
        this.vbox1.Name = "vbox1";
        this.vbox1.Spacing = 6;
        // Container child vbox1.Gtk.Box+BoxChild
        this.UIManager.AddUiFromString("<ui><toolbar name=\'toolbar\'><toolitem name=\'DisconnectedAction\' action=\'Disconnec" +
                "tedAction\'/><toolitem name=\'SettingsAction\' action=\'SettingsAction\'/></toolbar><" +
                "/ui>");
        this.toolbar = ((Gtk.Toolbar)(this.UIManager.GetWidget("/toolbar")));
        this.toolbar.Name = "toolbar";
        this.toolbar.ShowArrow = false;
        this.toolbar.ToolbarStyle = ((Gtk.ToolbarStyle)(0));
        this.toolbar.IconSize = ((Gtk.IconSize)(2));
        this.vbox1.Add(this.toolbar);
        Gtk.Box.BoxChild w2 = ((Gtk.Box.BoxChild)(this.vbox1[this.toolbar]));
        w2.Position = 0;
        w2.Expand = false;
        w2.Fill = false;
        // Container child vbox1.Gtk.Box+BoxChild
        this.GtkScrolledWindow = new Gtk.ScrolledWindow();
        this.GtkScrolledWindow.Name = "GtkScrolledWindow";
        this.GtkScrolledWindow.ShadowType = ((Gtk.ShadowType)(1));
        // Container child GtkScrolledWindow.Gtk.Container+ContainerChild
        this.treeview1 = new Gtk.TreeView();
        this.treeview1.CanFocus = true;
        this.treeview1.Name = "treeview1";
        this.GtkScrolledWindow.Add(this.treeview1);
        this.vbox1.Add(this.GtkScrolledWindow);
        Gtk.Box.BoxChild w4 = ((Gtk.Box.BoxChild)(this.vbox1[this.GtkScrolledWindow]));
        w4.Position = 1;
        this.Add(this.vbox1);
        if ((this.Child != null)) {
            this.Child.ShowAll();
        }
        this.DefaultWidth = 269;
        this.DefaultHeight = 433;
        this.Show();
        this.DeleteEvent += new Gtk.DeleteEventHandler(this.OnDeleteEvent);
    }
}
