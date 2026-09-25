using Microsoft.Maui.Layouts;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Globalization;
using System.Text.RegularExpressions;
using static Scada.ScadaClasses;

namespace Scada;
public partial class MainPage : ContentPage
{
    ScadaClasses ScadaGlobals = new ScadaClasses();
    SystemColors ScadaColor = new SystemColors();

    public System.Timers.Timer bcktimer = new System.Timers.Timer();
    public System.Timers.Timer uxtimer = new System.Timers.Timer();

    //private static WebSocket client;
    public static string ConnectionString = "";

    Editor edtSvgEditor = new Editor { Placeholder = "Paste your SVG text", Text = "Paste your SVG text" };
    Editor edtSvgName = new Editor { Placeholder = "SvgName", Text = "SvgName" };
    Editor edtInputText = new Editor { Placeholder = "InputText", Text = "" };

    bool bStartup = true;
    bool Designing = false;
    bool Editing = false;
    bool Moving = false;
    bool bLoginSuccess = false;
    int infoType = ScadaClasses.infoStartup;
    double Xpos = 0, Ypos = 0, Xwidth = 0, Yheight = 0;
    double OldXpos = 0, OldYpos = 0;
    double LastXpos = 0, LastYpos = 0;

    string sFilter = "";
    int iMenuOffsetRows = 0;
    float RampDirection = 2.0F;
    float StepSignal = 25F;


    // List<ContentView> ContentViews = new List<ContentView>();
    List<SKCanvasView> SKCanvasViews = new List<SKCanvasView>();
    List<SKCanvasView> SKCanvasPopupViews = new List<SKCanvasView>();

    AbsoluteLayout absoluteLayout = new AbsoluteLayout
    {
        Margin = new Thickness(0)
    };

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height); 
        Window.MinimumWidth = 1100;
        Window.MinimumHeight = 600;

        ScadaGlobals.Previouspage = -1;
        RefreshGui();      
    }

    public MainPage()
    {
        InitializeComponent();
        //Local SQL
        //Preferences.Default.Set("ConnectionString", "Data Source=PC-5CG5125C24; Initial Catalog=SCADA; Integrated Security=true; TrustServerCertificate=true");

        //Local SQLExpress
        //ConnectionString = Preferences.Default.Get("ConnectionString", "");

        //ConnectionString = "Server=tcp:scada.database.windows.net,1433;Initial Catalog=scada; Encrypt=True;TrustServerCertificate=False;Connection Timeout = 30; Authentication=Active Directory Default";
        //ConnectionString = "Server = tcp:scada.database.windows.net,1433;Initial Catalog = scada; Persist Security Info = False; User ID =freydr; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = False; Authentication = Active Directory Integrated";

        //Azure SQL Autentication
        //ConnectionString = "Server = tcp:scada.database.windows.net,1433; Initial Catalog = scada; Persist Security Info = False; User ID = freydr; Password =Admin123>; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = False; Connection Timeout = 30";

        ConnectionString = "Data Source = .\\SqlExpress; Initial Catalog = SCADA; Integrated Security = true; TrustServerCertificate=true";
        //ConnectionString = "Server=tcp:scada.database.windows.net,1433;Initial Catalog = scada; Encrypt=True;TrustServerCertificate=False;Connection Timeout = 30; Authentication=Active Directory Default";

        //SQL
        // ConnectionString = "Data Source =PC-5CG5125C24; Initial Catalog = SCADA; Integrated Security = true; TrustServerCertificate = true";

        /*
         ConnectionString = "Server =tcp:scada.database.windows.net,1433;" +
                            "Database=Scada;User ID=freydr;" +
                            "Password=Admin123>;Encrypt=True;" +
                            "TrustServerCertificate=False;Connection Timeout=30;";
         */
        //ConnectionString = "Server = tcp:scada.database.windows.net,1433; Initial Catalog = scada; Persist Security Info = False; User ID = operator1; Password = KopparGruvanSmalter137081>; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = False; Connection Timeout = 30";

        /*ConnectionString =  "Server=tcp:scada.database.windows.net,1433;" +
                            "Initial Catalog=scada;" +
                            "Encrypt=True;" +
                            "TrustServerCertificate=False;" +
                            "Connection Timeout=30;" +
                            "Authentication=SharedTokenCacheCredential;" +
                            "User ID=fredrik655@hotmail.com;"
        */
        /*
        ConnectionString = "Server=tcp:scada.database.windows.net,1433;" +
                            "Initial Catalog=scada;" +
                            "Encrypt=True;" +
                            "TrustServerCertificate=False;" +
                            "Connection Timeout=30;" +
                            "Authentication=Active Directory Integrated;" +
                            "User ID=fredrik655@hotmail.com;";
        */

        /*Ok 2026-04-20
        ConnectionString =  "Server=tcp:scada.database.windows.net,1433;" +
                            "Initial Catalog=scada;" +
                            "Encrypt=True;" +
                            "TrustServerCertificate=False;" +
                            "Connection Timeout=30;" +
                            "Authentication=Active Directory Interactive;" +
                            "User ID=fredrik655@hotmail.com;";
       */

        /* ConnectionString = "Server=scada.database.windows.net;" +
                              "Authentication=Active Directory Password; Encrypt=True; Database=scada;" +
                              "User Id=fredrik655@hotmail.com; Password=MQTTBroker0407>";
        */
        /*
        ConnectionString = @"Server=scada.database.windows.net;"+
                                   "Authentication=Active Directory Managed Identity; Encrypt=True;" +
                                   "Database=scada";
         */
        // Use your own server, database, user ID, and password.

        /*
          ConnectionString =  "Server=scada.database.windows.net;" +
                            "Authentication=Active Directory Password; Encrypt=True; Database=scada;"+
                            "User Id=fredrik655@hotmail.com; Password=MQTTBroker0407>";
        */
        //Active Directory Default

        List<ScadaClasses.Telegram> ScadaItems = new List<ScadaClasses.Telegram>();

        bcktimer.Interval = 10000;
        bcktimer.Elapsed += bckUpdate;
        bcktimer.Start();
        bcktimer.Enabled = true;

        uxtimer.Interval = 5000;
        uxtimer.Elapsed += uxUpdate;
        uxtimer.Start();

        uxtimer.Enabled = true;
        ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxLoginMenu;

    }


    static public SKColor RGBStringToColor(string RGBColor)
    {
        SKColor color = SKColors.White;
        if (RGBColor != null)
        {
            var regex = new Regex(@"rgba\((?<r>\d{1,3}),(?<g>\d{1,3}),(?<b>\d{1,3}),(?<a>\d{1,3})\)");
            Match match = regex.Match(RGBColor);
            if (match.Success)
            {
                byte r = byte.Parse(match.Groups["r"].Value);
                byte g = byte.Parse(match.Groups["g"].Value);
                byte b = byte.Parse(match.Groups["b"].Value);
                byte a = byte.Parse(match.Groups["a"].Value);
                color = new SKColor(r, g, b, a);
            }
        }
        return color;
    }


    void bckUpdate(object sender, EventArgs e)
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DataAccessLayer MyDataAccessLayer = new DataAccessLayer(ConnectionString);

                if (MyDataAccessLayer.ReadParameterByName("Enable ADAM") == "1")
                {
                    Adam AdamProtocol = new Adam(MyDataAccessLayer.ReadParameterByName("PortName"), MyDataAccessLayer.ReadParameterByName("Baudrate"));
                    AdamProtocol.ReadADAM();
                }
                if (MyDataAccessLayer.ReadParameterByName("Enable MQTT") == "1")
                {

                }
                if (MyDataAccessLayer.ReadParameterByName("Enable OpcUA") == "1")
                {

                }


                if (MyDataAccessLayer.ReadParameterByName("Enable simulation") == "1")
                {
                    float RampSignal = MyDataAccessLayer.GetTagValueByName("RampSignal");
                    if ((RampSignal > 75F) && (RampDirection > 0))
                    {
                        RampDirection = -5.0F;
                        StepSignal = 75F;
                    }

                    if ((RampSignal < 25F) && (RampDirection < 0))
                    {
                        RampDirection = 5.0F;
                        StepSignal = 25F;
                    }
                    RampSignal = RampSignal + RampDirection;

                    MyDataAccessLayer.SetTagValueByName("RampSignal", RampSignal);
                    var rnd = new Random();
                    var d = rnd.NextDouble() / 5F; ;
                    MyDataAccessLayer.SetTagValueByName("StepSignal", StepSignal + (float)(d));

                    var rnd1 = new Random();
                    var d1 = (rnd1.NextDouble() * 15F);
                    MyDataAccessLayer.SetTagValueByName("RandomSignal", RampSignal + (float)(d1));
                }


                var ListOfAnalogTagsToStore = MyDataAccessLayer.GetAnalogTagsToStore();
                foreach (var Tag in ListOfAnalogTagsToStore)
                {
                    MyDataAccessLayer.StoreTagValue(Tag.TagID, Tag.Value);
                }

                var ListOfDigitalTagsToStore = MyDataAccessLayer.GetDigitalTagsToStore(1);
                foreach (var Tag in ListOfDigitalTagsToStore)
                {
                    MyDataAccessLayer.StoreTagValue(Tag.TagID, Tag.Value);
                }

                //ScadaClasses.Refresh = true;
                bcktimer.Interval = 10000;
            });
        }
        catch
        {
        }
    }

    public void RefreshGui()
    {
        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(ConnectionString);
        var Items = MyDataAccessLayer.getItems(ScadaGlobals.Currentpage);
        UpdateGui(Items);

    }

    void uxUpdate(object sender, EventArgs e)
    {
        try
        {

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (!Moving)
                {
                    DataAccessLayer MyDataAccessLayer = new DataAccessLayer(ConnectionString);
                    if (bStartup)
                    {
                        //DataAccessLayer MyDataAccessLayer = new DataAccessLayer(ConnectionString);

                        MyDataAccessLayer.SetTagStatus(1, 1);
                        var ListOfDigitalTagsToStore = MyDataAccessLayer.GetDigitalTagsToStore(-1);
                        foreach (var Tag in ListOfDigitalTagsToStore)
                        {
                            MyDataAccessLayer.StoreTagValue(Tag.TagID, Tag.Value);
                        }

                        bStartup = false;

                    }
                    uxtimer.Interval = 5000;
                    RefreshGui();
                    //var Items = new List<ScadaClasses.Telegram> { };              
                    //var Items = MyDataAccessLayer.getItems(ScadaClasses.Currentpage);                    
                    //  UpdateGui(Items);


                    /*
                    if (ScadaClasses.Refresh == true)
                    {
                        ScadaClasses.Refresh = false;
                        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(ConnectionString);
                        bLoginSuccess = true;
                        var Items = new List<ScadaClasses.Telegram> {};
                        if (bLoginSuccess == true)
                        {
                            Items = MyDataAccessLayer.getItems(ScadaClasses.Currentpage);
                        }
                        
                        //Items = MyPopups.AddCurrentPopup(ScadaClasses.CurrentScadaPopup, ScadaClasses.Currentpage, ScadaClasses.CurrentTag, ScadaClasses.CurrentItem, Width, Height, Xpos, Ypos, Xwidth, Yheight, Items);
                        UpdateGui(Items);
                       
                    }
                    */
                }
            });
        }
        catch
        {
        }
    }

    static SKColor GetStatusColor(int status)
    {
        var c = SKColors.Black;
        switch (status)
        {
            case -1:
                c = new SKColor(64, 64, 64, 255);
                break;
            case 0:
                c = new SKColor(100, 100, 100, 255);
                break;
            case 1:
                c = new SKColor(32, 32, 215, 255);
                break;
            case 2:
                c = new SKColor(215, 215, 32, 255);
                break;
            case 3:
                c = new SKColor(215, 32, 32, 255);
                break;
            case 4:
                c = new SKColor(175, 175, 175, 255);
                break;
            case 5:
                c = new SKColor(3, 156, 35, 255);
                break;
        }
        return c;
    }

    private void ScadaLogin(int iType)
    {
        double panelWith = 0.4;
        double panelHeight = 0.55;
        double x = (Width / 2) - ((panelWith * Width)) / 2;
        double y = (Height / 2) - ((panelHeight * Height)) / 2;
        double w = (panelWith * Width);
        double h = (panelHeight * Height);

        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(ConnectionString);

        var gp = new ScadaButton();
        gp.AnchorX = 0;
        gp.AnchorY = 0;
        gp.CornerRadius = 10;
        gp.BarBackgroundColor = ScadaColor.uxPanelColor;
        gp.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        gp.GradientStartColor = ScadaColor.uxItemColor;
        gp.GradientEndColor = ScadaColor.uxItemColor;
        gp.IndicatorColor = ScadaColor.uxItemColor;
        gp.IndicatorType = 0;
        gp.WidthRequest = w;
        gp.HeightRequest = h;
        gp.AlternativeTextColor = ScadaColor.uxTextColor;
        gp.TextColor = ScadaColor.uxTextColor;
        gp.IsEnabled = true;
        gp.IsVisible = true;
        AbsoluteLayout.SetLayoutBounds(gp, new Rect(x, y, w, h));
        AbsoluteLayout.SetLayoutFlags(gp, AbsoluteLayoutFlags.None);

        SKCanvasPopupViews.Clear();
        SKCanvasPopupViews.Add(gp);

        var s1 = new ScadaSvg();
        s1.AnchorX = 0;
        s1.AnchorY = 0;
        s1.CornerRadius = 1;
        s1.BarBackgroundColor = ScadaColor.uxPanelColor;
        s1.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        s1.GradientStartColor = ScadaColor.uxPanelColor;
        s1.GradientEndColor = ScadaColor.uxPanelColor;
        s1.SvgBase64 = MyDataAccessLayer.LoadLibItem("Applelogo", 1);
        s1.WidthRequest = 70;
        s1.HeightRequest = 70;
        s1.IsEnabled = true;
        s1.IsVisible = true;
        AbsoluteLayout.SetLayoutBounds(s1, new Rect(x - 100, y, w, h));
        AbsoluteLayout.SetLayoutFlags(s1, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(s1);

        var s2 = new ScadaSvg();
        s2.AnchorX = 0;
        s2.AnchorY = 0;
        s2.CornerRadius = 1;
        s2.BarBackgroundColor = ScadaColor.uxPanelColor;
        s2.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        s2.GradientStartColor = ScadaColor.uxPanelColor;
        s2.GradientEndColor = ScadaColor.uxPanelColor;
        s2.SvgBase64 = MyDataAccessLayer.LoadLibItem("Androidlogo", 1);
        s2.WidthRequest = 70;
        s2.HeightRequest = 70;
        s2.IsEnabled = true;
        s2.IsVisible = true;
        AbsoluteLayout.SetLayoutBounds(s2, new Rect(x, y, w, h));
        AbsoluteLayout.SetLayoutFlags(s2, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(s2);

        var s3 = new ScadaSvg();
        s3.AnchorX = 0;
        s3.AnchorY = 0;
        s3.CornerRadius = 1;
        s3.BarBackgroundColor = ScadaColor.uxPanelColor;
        s3.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        s3.GradientStartColor = ScadaColor.uxPanelColor;
        s3.GradientEndColor = ScadaColor.uxPanelColor;
        s3.SvgBase64 = MyDataAccessLayer.LoadLibItem("Windows11logo", 1);
        s3.WidthRequest = 70;
        s3.HeightRequest = 70;
        s3.IsEnabled = true;
        s3.IsVisible = true;
        AbsoluteLayout.SetLayoutBounds(s3, new Rect(x + 100, y, w, h));
        AbsoluteLayout.SetLayoutFlags(s3, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(s3);

        var t1 = new ScadaText();
        t1.CornerRadius = 1;
        t1.BarBackgroundColor = ScadaColor.uxPanelColor;
        t1.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        t1.GradientStartColor = ScadaColor.uxPanelColor;
        t1.GradientEndColor = ScadaColor.uxPanelColor;
        t1.TextColor = ScadaColor.uxTextColor;
        t1.TheText = "uScada";
        t1.WidthRequest = 500;
        t1.HeightRequest = 70;
        t1.FontSize = 18;
        AbsoluteLayout.SetLayoutBounds(t1, new Rect(x + 30, y - 80, 500, h));
        AbsoluteLayout.SetLayoutFlags(t1, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(t1);

        var btnSettings = new ScadaButton();
        btnSettings.GradientStartColor = ScadaColor.uxItemColor;
        btnSettings.GradientEndColor = ScadaColor.uxItemColor;
        btnSettings.CornerRadius = 15;
        btnSettings.ItemID = 1;
        btnSettings.EnableTouchEvents = true;
        btnSettings.InputTransparent = false;
        btnSettings.HeightRequest = 25;
        btnSettings.WidthRequest = 25;
        btnSettings.SvgBase64 = MyDataAccessLayer.LoadLibItem("CogWheel", 1);
        btnSettings.IndicatorType = 3;
        btnSettings.Margin = 0.15F;
        btnSettings.ButtonText = "";
        btnSettings.Touch += (sender, args) =>
        {
            switch (args.ActionType)
            {
                case SKTouchAction.Released:
                    ScadaGlobals.Previouspage = -1;
                    ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxEditParameterText;
                    ScadaGlobals.PreviousScadaPopup = ScadaClasses.uxLoginMenu;
                    edtInputText.Text = ConnectionString;
                    ScadaGlobals.CurrentType = ScadaClasses.pmComputer;
                    //ScadaClasses.Refresh = true;
                    RefreshGui();
                    break;

                case SKTouchAction.Pressed:
                    break;

                case SKTouchAction.Moved:
                    btnSettings.GradientStartColor = ScadaColor.uxHoverColor;
                    btnSettings.GradientEndColor = ScadaColor.uxHoverColor;
                    btnSettings.IndicatorColor = ScadaColor.uxHoverColor;
                    break;

                case SKTouchAction.Exited:
                    btnSettings.GradientStartColor = ScadaColor.uxItemColor;
                    btnSettings.GradientEndColor = ScadaColor.uxItemColor;
                    btnSettings.IndicatorColor = ScadaColor.uxItemColor;
                    break;
            }
            args.Handled = true;
        };
        AbsoluteLayout.SetLayoutBounds(btnSettings, new Rect(x + btnSettings.WidthRequest / 5, y + btnSettings.HeightRequest / 5, 25, 25));
        AbsoluteLayout.SetLayoutFlags(btnSettings, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(btnSettings);


        CreateCloseButton(x, y, w, h, ScadaClasses.uxLoginMenu);

        /*
        if (iType == 0)
        {
            var btnClose = new ScadaButton();
            btnClose.GradientStartColor = ScadaColor.uxItemColor;
            btnClose.GradientEndColor = ScadaColor.uxItemColor;
            btnClose.CornerRadius = 15;
            btnClose.ItemID = 1;
            btnClose.EnableTouchEvents = true;
            btnClose.InputTransparent = false;
            btnClose.HeightRequest = 25;
            btnClose.WidthRequest = 25;
            btnClose.SvgBase64 = MyDataAccessLayer.LoadLibItem("Closecross", 1);
            btnClose.IndicatorType = 3;
            btnClose.Margin = 0.15F;
            btnClose.ButtonText = "";
            btnClose.Touch += (sender, args) =>
            {
                switch (args.ActionType)
                {
                    case SKTouchAction.Released:
                        //ScadaClasses.Previouspage = -1;
                        //ScadaClasses.CurrentScadaPopup = -1;
                        //ScadaClasses.Refresh = true;
                        //RefreshGui();
                        break;

                    case SKTouchAction.Pressed:
                        break;

                    case SKTouchAction.Moved:
                        btnClose.GradientStartColor = ScadaColor.uxHoverColor;
                        btnClose.GradientEndColor = ScadaColor.uxHoverColor;
                        btnClose.IndicatorColor = ScadaColor.uxHoverColor;
                        break;

                    case SKTouchAction.Exited:
                        btnClose.GradientStartColor = ScadaColor.uxItemColor;
                        btnClose.GradientEndColor = ScadaColor.uxItemColor;
                        btnClose.IndicatorColor = ScadaColor.uxItemColor;
                        break;
                }
                args.Handled = true;
            };
            AbsoluteLayout.SetLayoutBounds(btnClose, new Rect(x + gp.WidthRequest - btnClose.WidthRequest - (btnClose.WidthRequest / 4), y + btnClose.HeightRequest / 4, 25, 25));
            AbsoluteLayout.SetLayoutFlags(btnClose, AbsoluteLayoutFlags.None);
            SKCanvasPopupViews.Add(btnClose);
        }
        */
        var a1 = new ScadaButton();
        a1.CornerRadius = 10;
        a1.IndicatorType = 1;
        a1.BarBackgroundColor = ScadaColor.uxPanelColor;
        a1.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        a1.GradientStartColor = ScadaColor.uxPopupItemColor;
        a1.GradientEndColor = ScadaColor.uxPopupItemColor;
        a1.IndicatorColor = ScadaColor.uxItemColor;
        a1.TextColor = ScadaColor.uxTextColor;
        a1.ButtonText = "Login";
        a1.WidthRequest = 70;
        a1.HeightRequest = 40;
        a1.FontSize = 18;
        a1.IsEnabled = true;
        a1.IsVisible = true;
        a1.EnableTouchEvents = true;
        a1.InputTransparent = false;

        a1.Touch += (sender, args) =>
        {
            var pt = args.Location;
            switch (args.ActionType)
            {
                case SKTouchAction.Released:
                    //ScadaClasses.Previouspage = -1;
                    ScadaGlobals.CurrentScadaPopup = -1;
                    //ScadaGlobals.Refresh = true;
                    bLoginSuccess = true;
                    //infoType = ScadaGlobals.infoStartup;
                    //Go
                    //uxtimer.Enabled = true;
                    //bcktimer.Enabled = true;
                    foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                    {
                        absoluteLayout.Remove(viewItem);
                    }
                    SKCanvasPopupViews.Clear();
                    //RefreshGui();
                    break;

                case SKTouchAction.Pressed:
                    break;

                case SKTouchAction.Moved:
                    a1.GradientStartColor = ScadaColor.uxHoverColor;
                    a1.GradientEndColor = ScadaColor.uxHoverColor;
                    break;

                case SKTouchAction.Exited:
                    a1.GradientStartColor = ScadaColor.uxPopupItemColor;
                    a1.GradientEndColor = ScadaColor.uxPopupItemColor;
                    break;
            }
            args.Handled = true;
        };
        AbsoluteLayout.SetLayoutBounds(a1, new Rect(x + gp.WidthRequest - (gp.WidthRequest / 2) - (a1.WidthRequest / 2), y + gp.HeightRequest - a1.HeightRequest * 2, a1.WidthRequest, a1.HeightRequest));
        AbsoluteLayout.SetLayoutFlags(a1, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(a1);
    }



    private void ScadaUpload()
    {
        double panelWith = 0.4;
        double panelHeight = 0.55;
        double x = (Width / 2) - panelWith * Width / 2;
        double y = (Height / 2) - panelHeight * Height / 2;
        double w = (panelWith * Width);
        double h = (panelHeight * Height);

        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(ConnectionString);
        SKCanvasPopupViews.Clear();
        var gp = new ScadaButton();
        gp.AnchorX = 0;
        gp.AnchorY = 0;
        gp.CornerRadius = 10;
        gp.IndicatorType = 1;
        gp.BarBackgroundColor = ScadaColor.uxPanelColor;
        gp.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        gp.GradientStartColor = ScadaColor.uxItemColor;
        gp.GradientEndColor = ScadaColor.uxItemColor;
        gp.IndicatorColor = ScadaColor.uxPanelColor;
        gp.TextColor = ScadaColor.uxTextColor;

        gp.WidthRequest = w;
        gp.HeightRequest = h;
        gp.AlternativeTextColor = ScadaColor.uxTextColor;
        gp.TextColor = ScadaColor.uxTextColor;
        gp.IsEnabled = true;
        gp.IsVisible = true;
        AbsoluteLayout.SetLayoutBounds(gp, new Rect(x, y, w, h));
        AbsoluteLayout.SetLayoutFlags(gp, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(gp);

        var s2 = new ScadaSvg();
        s2.AnchorX = 0;
        s2.AnchorY = 0;
        s2.CornerRadius = 1;
        s2.BarBackgroundColor = ScadaColor.uxPanelColor;
        s2.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        s2.GradientStartColor = ScadaColor.uxPanelColor;
        s2.GradientEndColor = ScadaColor.uxPanelColor;
        s2.SvgBase64 = MyDataAccessLayer.LoadLibItem(edtSvgName.Text, 1);
        s2.WidthRequest = 50;
        s2.HeightRequest = 50;
        s2.IsEnabled = true;
        s2.IsVisible = true;
        AbsoluteLayout.SetLayoutBounds(s2, new Rect(x+w/2-25, y+5, 50, 50));
        AbsoluteLayout.SetLayoutFlags(s2, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(s2);
        /*
         var t1 = new ScadaText();
         t1.CornerRadius = 1;
         t1.BarBackgroundColor = ScadaColor.uxPanelColor;
         t1.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
         t1.GradientStartColor = ScadaColor.uxPanelColor;
         t1.GradientEndColor = ScadaColor.uxPanelColor;
         t1.TextColor = ScadaColor.uxTextColor;
         t1.TheText = "Name : ";
         t1.WidthRequest = 500;
         t1.HeightRequest = 70;
         t1.FontSize = 18;
         AbsoluteLayout.SetLayoutBounds(t1, new Rect(x - 100, y - 100, 500, h));
         AbsoluteLayout.SetLayoutFlags(t1, AbsoluteLayoutFlags.None);
         SKCanvasPopupViews.Add(t1);
         */

        CreateCloseButton(x, y, w, h, ScadaClasses.uxParameters);
      
        var a1 = new ScadaButton();
        a1.CornerRadius = 10;
        a1.IndicatorType = 1;
        a1.BarBackgroundColor = ScadaColor.uxPanelColor;
        a1.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        a1.GradientStartColor = ScadaColor.uxPopupItemColor;
        a1.GradientEndColor = ScadaColor.uxPopupItemColor;
        a1.IndicatorColor = ScadaColor.uxItemColor;
        a1.TextColor = ScadaColor.uxTextColor;
        a1.ButtonText = "Upload";
        a1.WidthRequest = 70;
        a1.HeightRequest = 40;
        a1.FontSize = 18;
        a1.IsEnabled = true;
        a1.IsVisible = true;
        a1.EnableTouchEvents = true;
        a1.InputTransparent = false;

        a1.Touch += (sender, args) =>
        {
            var pt = args.Location;
            switch (args.ActionType)
            {
                case SKTouchAction.Pressed:
                    break;

                case SKTouchAction.Released:
                    ScadaGlobals.Previouspage = -1;
                    ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxParameters;
                    string sBase64 = Base64Encode(edtSvgEditor.Text);
                    MyDataAccessLayer.DeleteLibItem(edtSvgName.Text);
                    MyDataAccessLayer.SaveLibItem(edtSvgName.Text, 1, sBase64);
                    uxtimer.Interval = 100;
                    break;

                case SKTouchAction.Moved:
                    a1.GradientStartColor = ScadaColor.uxHoverColor;
                    a1.GradientEndColor = ScadaColor.uxHoverColor;
                    break;

                case SKTouchAction.Exited:
                    a1.GradientStartColor = ScadaColor.uxPopupItemColor;
                    a1.GradientEndColor = ScadaColor.uxPopupItemColor;
                    break;
            }
            args.Handled = true;
        };
        AbsoluteLayout.SetLayoutBounds(a1, new Rect(x + gp.WidthRequest - (gp.WidthRequest / 2) - (a1.WidthRequest / 2), y + gp.HeightRequest - a1.HeightRequest * 1.5, a1.WidthRequest, a1.HeightRequest));
        AbsoluteLayout.SetLayoutFlags(a1, AbsoluteLayoutFlags.None);

        SKCanvasPopupViews.Add(a1); 
        foreach (SKCanvasView viewItem in SKCanvasPopupViews)
        {
            absoluteLayout.Add(viewItem);
        }
        edtSvgName.Placeholder = "SvgName";
        edtSvgName.WidthRequest = w-20;
        edtSvgName.HeightRequest = 28;
        edtSvgName.TextColor = ScadaColor.uxTextColor.ToMauiColor();
        edtSvgName.BackgroundColor = ScadaColor.uxPopupItemColor.ToMauiColor();
        edtSvgName.PlaceholderColor = ScadaColor.uxPopupItemColor.ToMauiColor();
        edtSvgName.FontAttributes = FontAttributes.None;
        edtSvgName.IsVisible = true;
        AbsoluteLayout.SetLayoutBounds(edtSvgName, new Rect(x+10, y+45, w-20, 28));
        AbsoluteLayout.SetLayoutFlags(edtSvgName, AbsoluteLayoutFlags.None);
        edtSvgName.TextChanged += OnEditorTextChanged;
        edtSvgName.Completed += OnEditorCompleted;
        absoluteLayout.Add(edtSvgName);

        edtSvgEditor.Placeholder = "Paste the SVG text(xml)";
        edtSvgEditor.WidthRequest = w-20;
        edtSvgEditor.HeightRequest = h/2;
        edtSvgEditor.TextColor = ScadaColor.uxTextColor.ToMauiColor();
        edtSvgEditor.BackgroundColor = ScadaColor.uxPopupItemColor.ToMauiColor();
        edtSvgEditor.PlaceholderColor = ScadaColor.uxItemColor.ToMauiColor();
        edtSvgEditor.FontAttributes = FontAttributes.None;
        edtSvgEditor.IsVisible = true;
        AbsoluteLayout.SetLayoutBounds(edtSvgEditor, new Rect(x+10, y+85, w-20, h/2));
        AbsoluteLayout.SetLayoutFlags(edtSvgEditor, AbsoluteLayoutFlags.None);
        edtSvgEditor.TextChanged += OnEditorTextChanged;
        edtSvgEditor.Completed += OnEditorCompleted;
        absoluteLayout.Add(edtSvgEditor);

    }




    private void ScadaProtocolSettings()
    {
        double panelWith = 0.4;
        double panelHeight = 0.55;

        double x = (Width / 2) - panelWith * Width / 2;
        double y = (Height / 2) - panelHeight * Height / 2;
        double w = (panelWith * Width);
        double h = (panelHeight * Height);
        var popupParameters = new ScadaButton();
        popupParameters.AnchorX = 0;
        popupParameters.AnchorY = 0;
        popupParameters.CornerRadius = 10;
        popupParameters.BarBackgroundColor = ScadaColor.uxPanelColor;
        popupParameters.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        popupParameters.GradientStartColor = ScadaColor.uxItemColor;
        popupParameters.GradientEndColor = ScadaColor.uxItemColor;
        popupParameters.IndicatorColor = ScadaColor.uxPanelColor;
        popupParameters.IndicatorType = 0;
        popupParameters.WidthRequest = w;
        popupParameters.HeightRequest = h;
        popupParameters.AlternativeTextColor = ScadaColor.uxTextColor;
        popupParameters.TextColor = ScadaColor.uxTextColor;
        popupParameters.IsEnabled = true;
        popupParameters.IsVisible = true;
        AbsoluteLayout.SetLayoutBounds(popupParameters, new Rect(x, y, w, h));
        AbsoluteLayout.SetLayoutFlags(popupParameters, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(popupParameters);
        CreateCloseButton(x, y, w, h, ScadaClasses.uxProtocols);
        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);

        var gridRows = MyDataAccessLayer.ReadParameters("", 2, ScadaGlobals.CurrentCategory, ScadaGlobals.CurrentCategory, 0, 10);

        int r = 0;
        y = y + popupParameters.CornerRadius;
        var TheColor = ScadaColor.uxItemColor;
        y = y + 30;
        foreach (gridRow myRow in gridRows)
        {
            if ((r % 2) == 0)
            {
                TheColor = ScadaColor.uxLightColor;
            }
            else
            {
                TheColor = ScadaColor.uxItemColor;
            }

            var myRowButton = new ScadaButton
            {
                ItemID = 1,
                StyleId = r.ToString(),
                WidthRequest = w,
                HeightRequest = 26,
                Background = ScadaColor.uxItemColor.ToMauiColor(),
                BarBackgroundColor = ScadaColor.uxItemColor,
                IndicatorType = 2,

                IndicatorColor = TheColor,
                GradientStartColor = TheColor,
                GradientEndColor = TheColor,

                CornerRadius = 0,
                TextColor = ScadaColor.uxTextColor,
                FontSize = 18.5F,
                SvgBase64 = "",
            };

            myRowButton.ButtonRow = myRow;
            myRowButton.ButtonText = "";
            myRowButton.EnableIndicatorBlink();
            myRowButton.InputTransparent = true;
            myRowButton.EnableTouchEvents = false;
            AbsoluteLayout.SetLayoutBounds(myRowButton, new Rect(x, y, myRowButton.WidthRequest, 26));
            AbsoluteLayout.SetLayoutFlags(myRowButton, AbsoluteLayoutFlags.None);
            SKCanvasPopupViews.Add(myRowButton);

            if (myRow.DataType == 0)
            {
                //Add Toggle
                var tgRow = new Toggle();
                tgRow.ItemID = -1;
                tgRow.StyleId = myRow.Row.ToString();
                tgRow.CornerRadius = 1;
                tgRow.BarBackgroundColor = ScadaColor.uxItemColor;
                tgRow.BackgroundColor = ScadaColor.uxItemColor.ToMauiColor();
                tgRow.GradientStartColor = ScadaColor.uxPopupItemColor;
                tgRow.GradientEndColor = ScadaColor.uxPopupItemColor;
                tgRow.TextColor = ScadaColor.uxTextColor;
                tgRow.WidthRequest = 50;
                tgRow.HeightRequest = 22;
                tgRow.FontSize = 18;
                tgRow.PV = new ItemValue();
                tgRow.SV = new ItemValue();
                tgRow.IsEnabled = true;
                tgRow.IsVisible = true;
                tgRow.EnableTouchEvents = true;
                tgRow.InputTransparent = false;
                tgRow.StyleId = r.ToString();
                tgRow.PV.TagID = myRowButton.ButtonRow.Id;
                tgRow.SV.TagID = myRowButton.ButtonRow.Id;
                tgRow.PV.Value = myRowButton.ButtonRow.Value;
                tgRow.SV.Value = myRowButton.ButtonRow.Value;

                tgRow.Touch += (sender, args) =>
                {
                    switch (args.ActionType)
                    {
                        case SKTouchAction.Pressed:
                            if (tgRow.SV.Value > 0.5F)
                            {
                                tgRow.SV.Value = 0;
                                tgRow.PV.Value = 0;
                                MyDataAccessLayer.UpdateParameterValue(myRowButton.ButtonRow.Id, tgRow.SV.Value.ToString());
                            }
                            else
                            {
                                tgRow.SV.Value = 1;
                                tgRow.PV.Value = 1;
                                MyDataAccessLayer.UpdateParameterValue(myRowButton.ButtonRow.Id, tgRow.SV.Value.ToString());
                            }
                            tgRow.InvalidateSurface();
                            RefreshGui();
                            break;
                    }
                    args.Handled = true;
                };
                AbsoluteLayout.SetLayoutBounds(tgRow, new Rect(x + w - 80, y + 2, 50, 22));
                AbsoluteLayout.SetLayoutFlags(tgRow, AbsoluteLayoutFlags.None);
                SKCanvasPopupViews.Add(tgRow);
            }
            if (myRow.DataType == 1)
            {
                var btnDots = new ScadaButton();
                btnDots.GradientStartColor = ScadaColor.uxPopupItemColor;
                btnDots.GradientEndColor = ScadaColor.uxPopupItemColor;
                btnDots.CornerRadius = 15;
                btnDots.ItemID = 1;
                btnDots.EnableTouchEvents = true;
                btnDots.InputTransparent = false;
                btnDots.HeightRequest = 22;
                btnDots.WidthRequest = 22;
                btnDots.SvgBase64 = MyDataAccessLayer.LoadLibItem("Threedots", 1);
                btnDots.IndicatorType = 9;
                btnDots.Margin = 0.15F;
                btnDots.ButtonText = "";
                btnDots.Touch += (sender, args) =>
                {
                    switch (args.ActionType)
                    {
                        case SKTouchAction.Released:
                            ScadaGlobals.Previouspage = -1;
                            ScadaGlobals.PreviousScadaPopup = ScadaGlobals.CurrentScadaPopup;
                            ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxEditParameterText;
                            edtInputText.Text = myRowButton.ButtonRow.col2text;
                            ScadaGlobals.CurrentID = myRowButton.ButtonRow.Id;
                            ScadaGlobals.CurrentType = ScadaClasses.pmParameter;
                            //RefreshGui();
                            break;

                        case SKTouchAction.Pressed:
                            break;

                        case SKTouchAction.Moved:
                            btnDots.GradientStartColor = ScadaColor.uxHoverColor;
                            btnDots.GradientEndColor = ScadaColor.uxHoverColor;
                            btnDots.IndicatorColor = ScadaColor.uxHoverColor;
                            break;

                        case SKTouchAction.Exited:
                            btnDots.GradientStartColor = ScadaColor.uxPopupItemColor;
                            btnDots.GradientEndColor = ScadaColor.uxPopupItemColor;
                            btnDots.IndicatorColor = ScadaColor.uxPopupItemColor;
                            break;
                    }
                    args.Handled = true;
                };
                AbsoluteLayout.SetLayoutBounds(btnDots, new Rect(x + w - 80 + 15, y + 2, 50, 22));
                AbsoluteLayout.SetLayoutFlags(btnDots, AbsoluteLayoutFlags.None);
                SKCanvasPopupViews.Add(btnDots);
            }
            y = y + 26;
            r++;
        }
    }



    private void ScadaTagSettings()
    {
        double panelWith = 0.4;
        double panelHeight = 0.55;

        double x = (Width / 2) - panelWith * Width / 2;
        double y = (Height / 2) - panelHeight * Height / 2;
        double w = (panelWith * Width);
        double h = (panelHeight * Height);
        double yPlaceHolder = y + 30;

        var TagsPropPanel = new ScadaButton();
        TagsPropPanel.AnchorX = 0;
        TagsPropPanel.AnchorY = 0;
        TagsPropPanel.ItemID = -1;
        TagsPropPanel.StyleId = "-1";
        TagsPropPanel.CornerRadius = 10;
        TagsPropPanel.BarBackgroundColor = ScadaColor.uxPanelColor;
        TagsPropPanel.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        TagsPropPanel.GradientStartColor = ScadaColor.uxItemBackGroundColor;
        TagsPropPanel.GradientEndColor = ScadaColor.uxItemBackGroundColor;
        TagsPropPanel.IndicatorColor = ScadaColor.uxPanelColor; ;
        TagsPropPanel.IndicatorType = 0;
        TagsPropPanel.WidthRequest = w;
        TagsPropPanel.HeightRequest = h;
        TagsPropPanel.AlternativeTextColor = ScadaColor.uxTextColor;
        TagsPropPanel.TextColor = ScadaColor.uxTextColor;
        TagsPropPanel.IsEnabled = true;
        TagsPropPanel.IsVisible = true;

        AbsoluteLayout.SetLayoutBounds(TagsPropPanel, new Rect(x, y, w, h));
        AbsoluteLayout.SetLayoutFlags(TagsPropPanel, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(TagsPropPanel);
        CreateCloseButton(x, y, w, h, ScadaClasses.uxTagsGrid);
        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);
        var gridRows = MyDataAccessLayer.GetTagParams(ScadaGlobals.CurrentTag);

        int r = 0;
        y = y + TagsPropPanel.CornerRadius;
        var TheColor = ScadaColor.uxItemColor;
        y = y + 30;
        foreach (gridRow myRow in gridRows)
        {
            if ((r % 2) == 0)
            {
                TheColor = ScadaColor.uxLightColor;
            }
            else
            {
                TheColor = ScadaColor.uxItemColor;
            }

            var myRowButton = new ScadaButton
            {
                ItemID = 1,
                StyleId = r.ToString(),
                WidthRequest = w,
                HeightRequest = 26,
                Background = ScadaColor.uxItemColor.ToMauiColor(),
                BarBackgroundColor = ScadaColor.uxItemColor,
                IndicatorType = 2,

                IndicatorColor = TheColor,
                GradientStartColor = TheColor,
                GradientEndColor = TheColor,

                CornerRadius = 0,
                TextColor = ScadaColor.uxTextColor,
                FontSize = 18.5F,
                SvgBase64 = "",
            };


            myRowButton.ButtonRow = myRow;
            myRowButton.ButtonText = "";
            myRowButton.EnableIndicatorBlink();
            myRowButton.InputTransparent = true;
            myRowButton.EnableTouchEvents = false;
            AbsoluteLayout.SetLayoutBounds(myRowButton, new Rect(x, y, myRowButton.WidthRequest, 26));
            AbsoluteLayout.SetLayoutFlags(myRowButton, AbsoluteLayoutFlags.None);
            SKCanvasPopupViews.Add(myRowButton);

            if (myRow.DataType == 0)
            {
                //Add Toggle
                var tgRow = new Toggle();
                tgRow.ItemID = -1;
                tgRow.StyleId = myRow.Row.ToString();
                tgRow.CornerRadius = 1;
                tgRow.BarBackgroundColor = ScadaColor.uxItemColor;
                tgRow.BackgroundColor = ScadaColor.uxItemColor.ToMauiColor();
                tgRow.GradientStartColor = ScadaColor.uxPopupItemColor;
                tgRow.GradientEndColor = ScadaColor.uxPopupItemColor;
                tgRow.TextColor = ScadaColor.uxTextColor;
                tgRow.WidthRequest = 50;
                tgRow.HeightRequest = 22;
                tgRow.FontSize = 18;
                tgRow.PV = new ItemValue();
                tgRow.SV = new ItemValue();
                tgRow.IsEnabled = true;
                tgRow.IsVisible = true;
                tgRow.EnableTouchEvents = true;
                tgRow.InputTransparent = false;
                tgRow.StyleId = r.ToString();
                tgRow.PV.TagID = myRowButton.ButtonRow.TagID;
                tgRow.SV.TagID = myRowButton.ButtonRow.TagID;
                tgRow.PV.Value = Int32.Parse(myRowButton.ButtonRow.col2text);
                tgRow.SV.Value = Int32.Parse(myRowButton.ButtonRow.col2text);

                tgRow.Touch += (sender, args) =>
                {
                    switch (args.ActionType)
                    {
                        case SKTouchAction.Released:
                            if (tgRow.SV.Value > 0.5F)
                            {
                                tgRow.SV.Value = 0;
                                tgRow.PV.Value = 0;
                                MyDataAccessLayer.UpdateAlarmEnable(ScadaGlobals.CurrentTag, tgRow.SV.Value.ToString());
                            }
                            else
                            {
                                tgRow.SV.Value = 1;
                                tgRow.PV.Value = 1;
                                MyDataAccessLayer.UpdateAlarmEnable(ScadaGlobals.CurrentTag, tgRow.SV.Value.ToString());
                            }
                            tgRow.InvalidateSurface();
                            uxtimer.Interval = 50;
                            break;

                        case SKTouchAction.Pressed:
                            break;

                        case SKTouchAction.Moved:
                            break;

                        case SKTouchAction.Exited:
                            break;
                    }
                    args.Handled = true;
                };
                AbsoluteLayout.SetLayoutBounds(tgRow, new Rect(x + w - 80, y + 2, 50, 22));
                AbsoluteLayout.SetLayoutFlags(tgRow, AbsoluteLayoutFlags.None);
                SKCanvasPopupViews.Add(tgRow);
            }
            if (myRow.DataType == 1)
            {
                var btnDots = new ScadaButton();
                btnDots.GradientStartColor = ScadaColor.uxPopupItemColor;
                btnDots.GradientEndColor = ScadaColor.uxPopupItemColor;
                btnDots.CornerRadius = 15;
                btnDots.ItemID = 1;
                btnDots.EnableTouchEvents = true;
                btnDots.InputTransparent = false;
                btnDots.HeightRequest = 22;
                btnDots.WidthRequest = 22;
                btnDots.SvgBase64 = MyDataAccessLayer.LoadLibItem("Threedots", 1);
                btnDots.IndicatorType = 9;
                btnDots.Margin = 0.15F;
                btnDots.ButtonText = "";
                btnDots.Touch += (sender, args) =>
                {
                    switch (args.ActionType)
                    {
                        case SKTouchAction.Released:
                            foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                            {
                                absoluteLayout.Remove(viewItem);
                            }
                            SKCanvasPopupViews.Clear();
                            edtInputText.Text = myRow.col2text;
                            ScadaGlobals.CurrentScadaPopup = -1;
                            ScadaGlobals.CurrentType = ScadaClasses.pmTagSetting;
                            ScadaGlobals.CurrentID = myRow.Id;
                            SKCanvasPopupViews.Clear();
                            ScadaEditParameterText();
                            /*
                            foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                            {
                                absoluteLayout.Add(viewItem);
                            }
                            edtInputText.Placeholder = "EditText";
                            edtInputText.WidthRequest = w - 20;
                            edtInputText.HeightRequest = h / 2;
                            edtInputText.TextColor = ScadaColor.uxTextColor.ToMauiColor();
                            edtInputText.BackgroundColor = ScadaColor.uxItemColor.ToMauiColor();
                            edtInputText.PlaceholderColor = ScadaColor.uxPopupItemColor.ToMauiColor();
                            edtInputText.FontAttributes = FontAttributes.Bold;
                            edtInputText.IsVisible = true;
                            AbsoluteLayout.SetLayoutBounds(edtInputText, new Rect(x + 10, yPlaceHolder, w - 20, h / 2));
                            AbsoluteLayout.SetLayoutFlags(edtInputText, AbsoluteLayoutFlags.None);
                            edtInputText.TextChanged += OnEditorTextChanged;
                            edtInputText.Completed += OnEditorCompleted;
                            absoluteLayout.Add(edtInputText);
                            */
                            Content = absoluteLayout;
                            break;

                        case SKTouchAction.Pressed:
                            break;

                        case SKTouchAction.Moved:
                            btnDots.GradientStartColor = ScadaColor.uxHoverColor;
                            btnDots.GradientEndColor = ScadaColor.uxHoverColor;
                            btnDots.IndicatorColor = ScadaColor.uxHoverColor;
                            break;

                        case SKTouchAction.Exited:
                            btnDots.GradientStartColor = ScadaColor.uxPopupItemColor;
                            btnDots.GradientEndColor = ScadaColor.uxPopupItemColor;
                            btnDots.IndicatorColor = ScadaColor.uxPopupItemColor;
                            break;
                    }
                    args.Handled = true;
                };
                AbsoluteLayout.SetLayoutBounds(btnDots, new Rect(x + w - 80 + 15, y + 2, 50, 22));
                AbsoluteLayout.SetLayoutFlags(btnDots, AbsoluteLayoutFlags.None);
                SKCanvasPopupViews.Add(btnDots);
            }


            y = y + 26;
            r++;
        }




    }





    

    public static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }

    public static string Base64Decode(string base64EncodedData)
    {
        var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }

    void OnEditorTextChanged(object sender, TextChangedEventArgs e)
    {

    }

    void OnEditorCompleted(object sender, EventArgs e)
    {

    }

    private void ScadaAlarmGrid()
    {
        double panelWith = 0.7;
        double panelHeight = 0.6;

        double x = (Width / 2) - panelWith * Width / 2;
        double y = (Height / 2) - panelHeight * Height / 2;
        double w = (panelWith * Width);
        double h = (panelHeight * Height);
        var popupAlarm = new ScadaButton();
        popupAlarm.StyleId = "-1";
        popupAlarm.AnchorX = 0;
        popupAlarm.AnchorY = 0;
        popupAlarm.CornerRadius = 10;
        popupAlarm.BarBackgroundColor = ScadaColor.uxPanelColor;
        popupAlarm.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        popupAlarm.GradientStartColor = ScadaColor.uxItemColor;
        popupAlarm.GradientEndColor = ScadaColor.uxItemColor;
        popupAlarm.IndicatorColor = ScadaColor.uxPanelColor;
        popupAlarm.IndicatorType = 0;
        popupAlarm.WidthRequest = w;
        popupAlarm.HeightRequest = h;
        popupAlarm.AlternativeTextColor = ScadaColor.uxTextColor;
        popupAlarm.TextColor = ScadaColor.uxTextColor;
        popupAlarm.IsEnabled = true;
        popupAlarm.IsVisible = true;

        AbsoluteLayout.SetLayoutBounds(popupAlarm, new Rect(x, y, w, h));
        AbsoluteLayout.SetLayoutFlags(popupAlarm, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(popupAlarm);
        CreateCloseButton(x, y, w, h, -1);


        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);

        double HeaderHeight = 40F;
        y = y + HeaderHeight;
        var TheColor = ScadaColor.uxItemColor;

        for (int r = 0; r < 6; r++)
        {
            if ((r % 2) == 0)
            {
                TheColor = ScadaColor.uxLightColor;
            }
            else
            {
                TheColor = ScadaColor.uxItemColor;
            }

            var myRowButton = new ScadaButton
            {
                ItemID = 1,//ScadaItem.ItemID,
                StyleId = r.ToString(),
                WidthRequest = w,
                HeightRequest = 26,
                Background = ScadaColor.uxItemColor.ToMauiColor(),
                BarBackgroundColor = ScadaColor.uxItemColor,
                IndicatorType = 2,

                IndicatorColor = TheColor,
                GradientStartColor = TheColor,
                GradientEndColor = TheColor,

                CornerRadius = 0,
                TextColor = ScadaColor.uxTextColor,
                FontSize = 18.5F,
                SvgBase64 = "",
            };

            var gridRow = new gridRow
            {
                Status = 0,
                col1text = "",
                col1width = 0F,
                col2text = "",
                col2width = 0F,
                col3text = "",
                col3width = 0F,
                col4text = "",
                col4width = 0F,
                col5text = "",
                col5width = 0F,
                col6text = "",
                col6width = 0F,
                Row = r,
                Id = -1
            };
            myRowButton.ButtonRow = gridRow;
            myRowButton.ButtonText = "";

            myRowButton.Touch += (sender, args) =>
            {
                var pt = args.Location;
                switch (args.ActionType)
                {
                    case SKTouchAction.Released:
                        /*ScadaClasses.Telegram oTelegram = new ScadaClasses.Telegram()
                        {
                            MessageType = 4,
                            TagID = ID,
                            Page = ScadaGlobals.Currentpage,
                            Action = " "
                        };*/
                        MyDataAccessLayer.confirmAlarm(myRowButton.ButtonRow.Id);
                        RefreshGui();
                        /*
                        SKCanvasPopupViews.Clear();

                        ScadaAlarmPopup(myRowButton.ButtonRow.Id, myRowButton.ButtonRow.TagID, myRowButton.ButtonRow.col2text);
                       
                        foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                        {
                            absoluteLayout.Add(viewItem);
                        }
                        Content = absoluteLayout;
                        */
                        break;

                    case SKTouchAction.Entered:
                        myRowButton.GradientStartColor = ScadaColor.uxTouchColor;
                        myRowButton.GradientEndColor = ScadaColor.uxTouchColor;
                        myRowButton.IndicatorColor = ScadaColor.uxTouchColor;
                        break;

                    case SKTouchAction.Exited:
                        if ((myRowButton.ButtonRow.Row % 2) == 0)
                        {
                            TheColor = ScadaColor.uxLightColor;
                        }
                        else
                        {
                            TheColor = ScadaColor.uxItemColor;
                        }
                        myRowButton.GradientStartColor = TheColor;
                        myRowButton.GradientEndColor = TheColor;
                        myRowButton.IndicatorColor = TheColor;
                        break;
                }
                args.Handled = true;
            };

            myRowButton.InputTransparent = false;
            myRowButton.EnableTouchEvents = true;
            myRowButton.EnableIndicatorBlink();
            //myRowButton.EnableFaceFade();
            AbsoluteLayout.SetLayoutBounds(myRowButton, new Rect(x, y, myRowButton.WidthRequest, 26));
            AbsoluteLayout.SetLayoutFlags(myRowButton, AbsoluteLayoutFlags.None);
            SKCanvasPopupViews.Add(myRowButton);
            y = y + 26;
        }
    }



    private void ScadaProtocols()
    {
        double panelWith = 0.4;
        double panelHeight = 0.55;

        double x = (Width / 2) - panelWith * Width / 2;
        double y = (Height / 2) - panelHeight * Height / 2;
        double w = (panelWith * Width);
        double h = (panelHeight * Height);
        var popupAlarm = new ScadaButton();
        popupAlarm.AnchorX = 0;
        popupAlarm.AnchorY = 0;
        popupAlarm.CornerRadius = 10;
        popupAlarm.BarBackgroundColor = ScadaColor.uxPanelColor;
        popupAlarm.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        popupAlarm.GradientStartColor = ScadaColor.uxItemColor;
        popupAlarm.GradientEndColor = ScadaColor.uxItemColor;
        popupAlarm.IndicatorColor = ScadaColor.uxPanelColor;
        popupAlarm.IndicatorType = 0;
        popupAlarm.WidthRequest = w;
        popupAlarm.HeightRequest = h;
        popupAlarm.AlternativeTextColor = ScadaColor.uxTextColor;
        popupAlarm.TextColor = ScadaColor.uxTextColor;
        popupAlarm.IsEnabled = true;
        popupAlarm.IsVisible = true;
        AbsoluteLayout.SetLayoutBounds(popupAlarm, new Rect(x, y, w, h));
        AbsoluteLayout.SetLayoutFlags(popupAlarm, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(popupAlarm);
        CreateCloseButton(x, y, w, h, ScadaClasses.uxParameters);

        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);
        var gridRows = MyDataAccessLayer.GetCommProtocols();

        int r = 0;
        y = y + popupAlarm.CornerRadius;
        var TheColor = ScadaColor.uxItemColor;
        y = y + 30;
        foreach (gridRow myRow in gridRows)
        {
            if ((r % 2) == 0)
            {
                TheColor = ScadaColor.uxLightColor;
            }
            else
            {
                TheColor = ScadaColor.uxItemColor;
            }

            var myRowButton = new ScadaButton
            {
                ItemID = 1,
                StyleId = r.ToString(),
                WidthRequest = w,
                HeightRequest = 26,
                Background = ScadaColor.uxItemColor.ToMauiColor(),
                BarBackgroundColor = ScadaColor.uxItemColor,
                IndicatorType = 2,

                IndicatorColor = TheColor,
                GradientStartColor = TheColor,
                GradientEndColor = TheColor,

                CornerRadius = 0,
                TextColor = ScadaColor.uxTextColor,
                FontSize = 18.5F,
                SvgBase64 = "",
            };

            myRowButton.ButtonRow = myRow;
            myRowButton.ButtonText = "";

            //Add Toggle
            /*
            var tgRow = new Toggle();
            tgRow.ItemID = -1;
            tgRow.StyleId = myRow.Row.ToString();
            tgRow.CornerRadius = 1;
            tgRow.BarBackgroundColor = ScadaColor.uxItemColor;
            tgRow.BackgroundColor = ScadaColor.uxItemColor.ToMauiColor();
            tgRow.GradientStartColor = ScadaColor.uxPopupItemColor;
            tgRow.GradientEndColor = ScadaColor.uxItemColor;
            tgRow.TextColor = ScadaColor.uxTextColor;
            tgRow.WidthRequest = 50;
            tgRow.HeightRequest = 22;
            tgRow.FontSize = 18;
            tgRow.PV = new ItemValue();
            tgRow.SV = new ItemValue();
            tgRow.IsEnabled = true;
            tgRow.IsVisible = true;
            tgRow.EnableTouchEvents = true;
            tgRow.InputTransparent = false;
            tgRow.StyleId = r.ToString();
            tgRow.PV.TagID = myRowButton.ButtonRow.Id;
            tgRow.SV.TagID = myRowButton.ButtonRow.Id;

            tgRow.Touch += (sender, args) =>
            {
                switch (args.ActionType)
                {
                    case SKTouchAction.Pressed:
                        if (tgRow.SV.Value > 0.5F)
                        {
                            tgRow.SV.Value = 0;
                            MyDataAccessLayer.UpdateParameterValue(myRowButton.ButtonRow.Id, tgRow.SV.Value.ToString());
                        }
                        else
                        {
                            tgRow.SV.Value = 1;
                            MyDataAccessLayer.UpdateParameterValue(myRowButton.ButtonRow.Id, tgRow.SV.Value.ToString());
                        }
                        tgRow.InvalidateSurface();
                        break;
                }
                args.Handled = true;
            };
            */
            myRowButton.Touch += (sender, args) =>
            {
                var pt = args.Location;
                switch (args.ActionType)
                {
                    case SKTouchAction.Released:
                        //SKCanvasPopupViews.Clear();
                        ScadaGlobals.CurrentCategory = myRowButton.ButtonRow.Id;
                        ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxProtocolSettings;
                        ScadaGlobals.PreviousScadaPopup = ScadaClasses.uxProtocols;


                        foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                        {
                            absoluteLayout.Remove(viewItem);
                        }
                        SKCanvasPopupViews.Clear();
                        uxtimer.Interval = 100;
                        /*
                        ScadaProtocolSettings();
                        foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                        {
                            absoluteLayout.Add(viewItem);
                        }
                        Content = absoluteLayout;
                        */
                        //RefreshGui();
                        break;

                    case SKTouchAction.Entered:
                        myRowButton.GradientStartColor = ScadaColor.uxTouchColor;
                        myRowButton.GradientEndColor = ScadaColor.uxTouchColor;
                        myRowButton.IndicatorColor = ScadaColor.uxTouchColor;
                        break;

                    case SKTouchAction.Exited:
                        if ((myRowButton.ButtonRow.Row % 2) == 0)
                        {
                            TheColor = ScadaColor.uxLightColor;
                        }
                        else
                        {
                            TheColor = ScadaColor.uxItemColor;
                        }
                        myRowButton.GradientStartColor = TheColor;
                        myRowButton.GradientEndColor = TheColor;
                        myRowButton.IndicatorColor = TheColor;
                        break;
                }
                args.Handled = true;
            };

            myRowButton.EnableIndicatorBlink();
            myRowButton.InputTransparent = false;
            myRowButton.EnableTouchEvents = true;

            AbsoluteLayout.SetLayoutBounds(myRowButton, new Rect(x, y, myRowButton.WidthRequest, 26));
            AbsoluteLayout.SetLayoutFlags(myRowButton, AbsoluteLayoutFlags.None);
            SKCanvasPopupViews.Add(myRowButton);
            //AbsoluteLayout.SetLayoutBounds(tgRow, new Rect(x + 520, y + 2, 50, 22));
            //AbsoluteLayout.SetLayoutFlags(tgRow, AbsoluteLayoutFlags.None);
            //SKCanvasViews.Add(tgRow);
            y = y + 26;
            r++;
        }
    }


    private void ScadaItemTypesMenu(int ItemID)
    {
        double panelWith = 0.2;
        double panelHeight = 0.4;

        double x = (Width / 2) - panelWith * Width / 2;
        double y = (Height / 2) - panelHeight * Height / 2;
        double w = (panelWith * Width);
        double h = (panelHeight * Height);
        //ScadaItem.Width = w;
        //ScadaItem.Height = h;

        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);

        var FloatPanel3 = new ScadaButton();
        FloatPanel3.ItemID = -1;// ScadaItem.ItemID;
        FloatPanel3.StyleId = "-1";// ScadaItem.ItemID.ToString();
        FloatPanel3.AnchorX = 0;
        FloatPanel3.AnchorY = 0;
        FloatPanel3.CornerRadius = 10;
        FloatPanel3.BarBackgroundColor = ScadaColor.uxBackGroundColor;
        FloatPanel3.BackgroundColor = ScadaColor.uxBackGroundColor.ToMauiColor();
        FloatPanel3.GradientStartColor = ScadaColor.uxPopupColor;
        FloatPanel3.GradientEndColor = ScadaColor.uxPopupColor;
        FloatPanel3.IndicatorColor = ScadaColor.uxBackGroundColor;

        FloatPanel3.WidthRequest = w;
        FloatPanel3.HeightRequest = h;
        FloatPanel3.AlternativeTextColor = ScadaColor.uxTextColor;
        FloatPanel3.TextColor = ScadaColor.uxTextColor;
        FloatPanel3.IsEnabled = true;
        FloatPanel3.IsVisible = true;
        FloatPanel3.IndicatorType = 10;
        AbsoluteLayout.SetLayoutBounds(FloatPanel3, new Rect(x, y, w, h));
        AbsoluteLayout.SetLayoutFlags(FloatPanel3, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(FloatPanel3);

        /*
        x = (Width / 100) * ScadaItem.Left;
        y = (Height / 100) * ScadaItem.Top;
        w = (Width / 100) * ScadaItem.Width;F
        h = (Height / 100) * ScadaItem.Height;
        */
        var gridRows = MyDataAccessLayer.ReadItemTypes(sFilter, iMenuOffsetRows, iMenuOffsetRows + 5);

        //Next and Previous buttons 
        CreateCloseButton(x, y, w, h, ScadaGlobals.CurrentScadaPopup);
        CreateFwdButton(x, y, w, h);
        CreateRwdButton(x, y, w, h);

        y = y + FloatPanel3.CornerRadius + 50;
        foreach (gridRow row in gridRows)
        {
            var myButton = new ScadaButton
            {
                WidthRequest = w,
                HeightRequest = 25,
                Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
                BarBackgroundColor = ScadaColor.uxBackGroundColor,
                IndicatorType = 3,
                IndicatorColor = GetStatusColor(row.Status),
                GradientStartColor = ScadaColor.uxPopupItemColor,
                GradientEndColor = ScadaColor.uxPopupItemColor,
                CornerRadius = 0,
                TextColor = ScadaColor.uxTextColor,
                FontSize = 16.5F,
                SvgBase64 = MyDataAccessLayer.LoadLibItem("Compass", 1)
            };

            myButton.ButtonRow = row;
            myButton.ButtonText = row.col1text;
            myButton.EnableTouchEvents = true;
            myButton.InputTransparent = false;

            myButton.Touch += (sender, args) =>
            {
                var pt = args.Location;
                switch (args.ActionType)
                {
                    case SKTouchAction.Released:
                        /* ScadaClasses.Telegram oTelegram = new ScadaClasses.Telegram()
                         {
                             MessageType = ScadaItem.MessageType,
                             Page = ScadaItem.Nextpage,
                             ItemType = row.ItemType,
                             ItemID = ScadaGlobals.CurrentItem,
                             TagID = row.TagID,
                             TagName = ScadaItem.TagName,
                             Action = ScadaItem.Action,
                         };
                         */


                        //double w = MyDataAccessLayer.GetDefaultWidth(row.ItemType);
                        MyDataAccessLayer.SetItemType(ItemID, row.ItemType);
                        MyDataAccessLayer.AddItemDefaultTags(ItemID, row.ItemType);
                        ScadaGlobals.Previouspage = -1;

                        RefreshGui();

                        /*
                        if (row.ItemType == ScadaClasses.uxButton)
                        {
                            ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxItemAction;
                        }
                        else
                        {
                            ScadaGlobals.CurrentScadaPopup = -1;
                        }
                        */
                        //ScadaClasses.Previouspage = -1;
                        //ScadaClasses.Refresh = true;
                        break;

                    case SKTouchAction.Moved:
                        myButton.GradientStartColor = ScadaColor.uxHoverColor;
                        myButton.GradientEndColor = ScadaColor.uxHoverColor;
                        myButton.IndicatorColor = GetStatusColor(row.Status);
                        break;

                    case SKTouchAction.Exited:
                        myButton.GradientStartColor = ScadaColor.uxPopupItemColor;
                        myButton.GradientEndColor = ScadaColor.uxPopupItemColor;
                        myButton.IndicatorColor = GetStatusColor(row.Status);
                        break;
                }
                args.Handled = true;
            };
            AbsoluteLayout.SetLayoutBounds(myButton, new Rect(x, y, w, 20));
            AbsoluteLayout.SetLayoutFlags(myButton, AbsoluteLayoutFlags.None);
            SKCanvasPopupViews.Add(myButton);
            y = y + 26;

        }
    }


    private void ScadaItemTagsMenu(int ItemID)
    {
        double panelWith = 0.2;
        double panelHeight = 0.4;

        double x = (Width / 2) - panelWith * Width / 2;
        double y = (Height / 2) - panelHeight * Height / 2;
        double w = (panelWith * Width);
        double h = (panelHeight * Height);
        //ScadaItem.Width = w;
        //ScadaItem.Height = h;


        var FloatPanel11 = new ScadaButton();
        FloatPanel11.ItemID = -1; //ScadaItem.ItemID;
        FloatPanel11.StyleId = "-1";// ScadaItem.ItemID.ToString();
        FloatPanel11.AnchorX = 0;
        FloatPanel11.AnchorY = 0;
        FloatPanel11.CornerRadius = 10;
        FloatPanel11.BarBackgroundColor = ScadaColor.uxBackGroundColor;
        FloatPanel11.BackgroundColor = ScadaColor.uxBackGroundColor.ToMauiColor();
        FloatPanel11.GradientStartColor = ScadaColor.uxPopupColor;
        FloatPanel11.GradientEndColor = ScadaColor.uxPopupColor;
        FloatPanel11.IndicatorColor = ScadaColor.uxBackGroundColor;

        FloatPanel11.WidthRequest = w;
        FloatPanel11.HeightRequest = h;
        FloatPanel11.AlternativeTextColor = ScadaColor.uxTextColor;
        FloatPanel11.TextColor = ScadaColor.uxTextColor;
        FloatPanel11.IsEnabled = true;
        FloatPanel11.IsVisible = true;
        FloatPanel11.IndicatorType = 10;
        //FloatPanel11.EnableFaceFade();

        AbsoluteLayout.SetLayoutBounds(FloatPanel11, new Rect(x, y, w, h));
        AbsoluteLayout.SetLayoutFlags(FloatPanel11, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(FloatPanel11);


        CreateCloseButton(x, y, w, h, ScadaGlobals.CurrentScadaPopup);
        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);

        var gridRows = MyDataAccessLayer.ReadItemTags(ItemID);

        y = y + FloatPanel11.CornerRadius + 50;

        foreach (gridRow row in gridRows)
        {
            var myItemTagsBtn = new ScadaButton
            {
                WidthRequest = w,
                HeightRequest = 25,
                Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
                BarBackgroundColor = ScadaColor.uxBackGroundColor,
                IndicatorType = 4,
                IndicatorColor = GetStatusColor(row.Status),
                GradientStartColor = ScadaColor.uxPopupItemColor,
                GradientEndColor = ScadaColor.uxPopupItemColor,
                CornerRadius = 0,
                TextColor = ScadaColor.uxTextColor,
                FontSize = 16.5F,
                SvgBase64 = MyDataAccessLayer.LoadLibItem("InputSource", 1)
            };

            myItemTagsBtn.ButtonRow = row;
            myItemTagsBtn.ButtonText = row.col1text;
            myItemTagsBtn.EnableTouchEvents = true;
            myItemTagsBtn.InputTransparent = false;
            myItemTagsBtn.Touch += (sender, args) =>
            {
                var pt = args.Location;
                switch (args.ActionType)
                {
                    case SKTouchAction.Released:
                        //FYNUScadaGlobals.CurrentItem = ScadaItem.ItemID;
                        ScadaGlobals.CurrentTag = -1; //Make new binding
                        ScadaGlobals.CurrentTagSequence = myItemTagsBtn.ButtonRow.TagSequence;
                        /*SKCanvasPopupViews.Clear();
                        ScadaTagsMenu();
                        foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                        {
                            absoluteLayout.Add(viewItem);
                        }
                        Content = absoluteLayout;      
                        */
                        //Clear
                        foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                        {
                            absoluteLayout.Remove(viewItem);
                        }
                        SKCanvasPopupViews.Clear();

                        ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxTagsGrid;
                        break;

                    case SKTouchAction.Moved:
                        myItemTagsBtn.GradientStartColor = ScadaColor.uxHoverColor;
                        myItemTagsBtn.GradientEndColor = ScadaColor.uxHoverColor;
                        myItemTagsBtn.IndicatorColor = GetStatusColor(row.Status);
                        break;

                    case SKTouchAction.Exited:
                        myItemTagsBtn.GradientStartColor = ScadaColor.uxPopupItemColor;
                        myItemTagsBtn.GradientEndColor = ScadaColor.uxPopupItemColor;
                        myItemTagsBtn.IndicatorColor = GetStatusColor(row.Status);
                        break;
                }
                args.Handled = true;
            };
            AbsoluteLayout.SetLayoutBounds(myItemTagsBtn, new Rect(x, y, w, 20));
            AbsoluteLayout.SetLayoutFlags(myItemTagsBtn, AbsoluteLayoutFlags.None);
            SKCanvasPopupViews.Add(myItemTagsBtn);
            y = y + 26;
        }
    }


    private void ScadaItemSizesMenu(int ItemID, int ItemType)
    {
        double panelWith = 0.2;
        double panelHeight = 0.4;

        double x = (Width / 2) - panelWith * Width / 2;
        double y = (Height / 2) - panelHeight * Height / 2;
        double w = (panelWith * Width);
        double h = (panelHeight * Height);
        //ScadaItem.Width = w;
        //ScadaItem.Height = h;

        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);

        var FloatPanelSizes = new ScadaButton();
        FloatPanelSizes.ItemID = -1;// ScadaItem.ItemID;
        FloatPanelSizes.StyleId = "-1"; //ScadaItem.ItemID.ToString();
        FloatPanelSizes.AnchorX = 0;
        FloatPanelSizes.AnchorY = 0;
        FloatPanelSizes.CornerRadius = 10;
        FloatPanelSizes.BarBackgroundColor = ScadaColor.uxBackGroundColor;
        FloatPanelSizes.BackgroundColor = ScadaColor.uxBackGroundColor.ToMauiColor();
        FloatPanelSizes.GradientStartColor = ScadaColor.uxPopupColor;
        FloatPanelSizes.GradientEndColor = ScadaColor.uxPopupColor;
        FloatPanelSizes.IndicatorColor = ScadaColor.uxBackGroundColor;

        FloatPanelSizes.WidthRequest = w;
        FloatPanelSizes.HeightRequest = h;
        FloatPanelSizes.AlternativeTextColor = ScadaColor.uxTextColor;
        FloatPanelSizes.TextColor = ScadaColor.uxTextColor;
        FloatPanelSizes.IsEnabled = true;
        FloatPanelSizes.IsVisible = true;
        FloatPanelSizes.IndicatorType = 10;
        AbsoluteLayout.SetLayoutBounds(FloatPanelSizes, new Rect(x, y, w, h));
        AbsoluteLayout.SetLayoutFlags(FloatPanelSizes, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(FloatPanelSizes);

        CreateCloseButton(x, y, w, h, ScadaGlobals.CurrentScadaPopup);
        var gridRows = MyDataAccessLayer.GetItemSizes(ScadaGlobals.CurrentType);
        y = y + FloatPanelSizes.CornerRadius + 50;

        foreach (gridRow row in gridRows)
        {
            var myItemTagsBtn = new ScadaButton
            {
                WidthRequest = w,
                HeightRequest = 25,
                Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
                BarBackgroundColor = ScadaColor.uxBackGroundColor,
                IndicatorType = 3,
                IndicatorColor = GetStatusColor(row.Status),
                GradientStartColor = ScadaColor.uxPopupItemColor,
                GradientEndColor = ScadaColor.uxPopupItemColor,
                CornerRadius = 0,
                TextColor = ScadaColor.uxTextColor,
                FontSize = 16.5F,
                SvgBase64 = MyDataAccessLayer.LoadLibItem("InputSource", 1)

            };

            myItemTagsBtn.ButtonRow = row;
            myItemTagsBtn.ButtonText = row.col1text;
            myItemTagsBtn.EnableTouchEvents = true;
            myItemTagsBtn.InputTransparent = false;
            myItemTagsBtn.Touch += (sender, args) =>
            {
                var pt = args.Location;
                switch (args.ActionType)
                {
                    case SKTouchAction.Released:
                        /*
                        ScadaClasses.Telegram oTelegram = new ScadaClasses.Telegram()
                        {
                            MessageType = ScadaItem.MessageType,
                            Page = ScadaItem.Nextpage,
                            ItemType = ScadaGlobals.CurrentType,
                            ItemID = ScadaGlobals.CurrentItem,
                            TagID = row.TagID,
                            TagName = ScadaItem.TagName,
                            size = row.Row,
                            Action = ScadaItem.Action,
                        };
                        */
                        //ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxTagsMenu;
                        ScadaGlobals.Previouspage = -1;
                        //ScadaGlobals.CurrentRow = row.Id;

                        MyDataAccessLayer.SetItemSize(ItemID, ItemType, row.Row);

                        foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                        {
                            absoluteLayout.Remove(viewItem);
                        }
                        SKCanvasPopupViews.Clear();
                        RefreshGui();

                        //ScadaClasses.Refresh = true;
                        break;

                    case SKTouchAction.Moved:
                        myItemTagsBtn.GradientStartColor = ScadaColor.uxHoverColor;
                        myItemTagsBtn.GradientEndColor = ScadaColor.uxHoverColor;
                        myItemTagsBtn.IndicatorColor = GetStatusColor(row.Status);
                        break;

                    case SKTouchAction.Exited:
                        myItemTagsBtn.GradientStartColor = ScadaColor.uxPopupItemColor;
                        myItemTagsBtn.GradientEndColor = ScadaColor.uxPopupItemColor;
                        myItemTagsBtn.IndicatorColor = GetStatusColor(row.Status);
                        break;
                }
                args.Handled = true;
            };

            AbsoluteLayout.SetLayoutBounds(myItemTagsBtn, new Rect(x, y, w, 20));
            AbsoluteLayout.SetLayoutFlags(myItemTagsBtn, AbsoluteLayoutFlags.None);
            SKCanvasPopupViews.Add(myItemTagsBtn);
            y = y + 26;


        }
    }

    private void ScadaConfigMenu(int ItemID, int ItemType)
    {
        double panelWith = 0.2;
        double panelHeight = 0.4;

        double x = (Width / 2) - panelWith * Width / 2;
        double y = (Height / 2) - panelHeight * Height / 2;
        double w = (panelWith * Width);
        double h = (panelHeight * Height);
        //ScadaItem.Width = w;
        //ScadaItem.Height = h;

        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);

        var FloatPanel = new ScadaButton();
        FloatPanel.ItemID = -1;// ScadaItem.ItemID;
        FloatPanel.StyleId = "-1";// ScadaItem.ItemID.ToString();
        FloatPanel.AnchorX = 0;
        FloatPanel.AnchorY = 0;
        FloatPanel.CornerRadius = 10;
        FloatPanel.BarBackgroundColor = ScadaColor.uxPanelColor;
        FloatPanel.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        FloatPanel.GradientStartColor = ScadaColor.uxPopupColor;
        FloatPanel.GradientEndColor = ScadaColor.uxPopupColor;
        FloatPanel.IndicatorColor = ScadaColor.uxPopupColor;
        FloatPanel.WidthRequest = w;// (Width / 100) * ScadaItem.Width;
        FloatPanel.HeightRequest = h;// (Height / 100) * ScadaItem.Height;
        FloatPanel.AlternativeTextColor = ScadaColor.uxTextColor;
        FloatPanel.TextColor = ScadaColor.uxTextColor;
        FloatPanel.IsEnabled = true;
        FloatPanel.IsVisible = true;
        FloatPanel.IndicatorType = 10;
        // AbsoluteLayout.SetLayoutBounds(FloatPanel, new Rect( (Width / 100) * ScadaItem.Left, (Height / 100) * ScadaItem.Top,  (Width / 100) * ScadaItem.Width, (Height / 100) * ScadaItem.Height));
        AbsoluteLayout.SetLayoutBounds(FloatPanel, new Rect(x, y, w, h));
        AbsoluteLayout.SetLayoutFlags(FloatPanel, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(FloatPanel);

        var myDeleteButton = new ScadaButton
        {
            WidthRequest = 25,
            HeightRequest = 25,
            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
            BarBackgroundColor = ScadaColor.uxBackGroundColor,
            IndicatorType = 9,

            GradientStartColor = ScadaColor.uxItemColor,
            GradientEndColor = ScadaColor.uxItemColor,
            CornerRadius = 10,
            TextColor = ScadaColor.uxTextColor,
            FontSize = 21.5F,
            Margin = 0.15F,
            SvgBase64 = MyDataAccessLayer.LoadLibItem("Trashcan", 1),
            ButtonText = ""
        };
        myDeleteButton.IsVisible = true;
        myDeleteButton.InputTransparent = false;
        myDeleteButton.EnableTouchEvents = true;
        myDeleteButton.Touch += (sender, args) =>
        {
            var pt = args.Location;
            switch (args.ActionType)
            {
                case SKTouchAction.Released:
                    MyDataAccessLayer.deleteItem(ItemID);
                    ScadaGlobals.CurrentScadaPopup = -1;
                    ScadaGlobals.Previouspage = -1;
                    RefreshGui();
                    //ScadaClasses.Refresh = true;
                    break;

                case SKTouchAction.Moved:
                    myDeleteButton.GradientStartColor = ScadaColor.uxHoverColor;
                    myDeleteButton.GradientEndColor = ScadaColor.uxHoverColor;
                    break;

                case SKTouchAction.Exited:
                    myDeleteButton.GradientStartColor = ScadaColor.uxItemColor;
                    myDeleteButton.GradientEndColor = ScadaColor.uxItemColor;
                    break;

            }
            args.Handled = true;
        };
        AbsoluteLayout.SetLayoutBounds(myDeleteButton, new Rect(x + w - 35, y + h - 35, 30, 30));
        AbsoluteLayout.SetLayoutFlags(myDeleteButton, AbsoluteLayoutFlags.None);

        CreateCloseButton(x, y, w, h, -1);
        SKCanvasPopupViews.Add(myDeleteButton);

        var myAddItemButton = new ScadaButton
        {
            WidthRequest = 25,
            HeightRequest = 25,
            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
            BarBackgroundColor = ScadaColor.uxBackGroundColor,
            IndicatorType = 9,
            GradientStartColor = ScadaColor.uxItemColor,
            GradientEndColor = ScadaColor.uxItemColor,
            CornerRadius = 10,
            TextColor = ScadaColor.uxTextColor,
            FontSize = 21.5F,
            Margin = 0.15F,
            SvgBase64 = MyDataAccessLayer.LoadLibItem("GreenPlus", 1),
            ButtonText = ""
        };
        myAddItemButton.IsVisible = true;
        myAddItemButton.InputTransparent = false;
        myAddItemButton.EnableTouchEvents = true;
        myAddItemButton.Touch += (sender, args) =>
        {
            var pt = args.Location;
            switch (args.ActionType)
            {
                case SKTouchAction.Released:
                    /*
                    ScadaClasses.Telegram oTelegram = new ScadaClasses.Telegram()
                    {
                        MessageType = ScadaItem.MessageType,
                        Page = ScadaGlobals.Currentpage,
                        ItemType = ScadaGlobals.CurrentType,
                        ItemID = ScadaGlobals.CurrentItem,
                        TagID = ScadaItem.TagID,
                        TagName = ScadaItem.TagName,
                        Action = ScadaItem.Action,
                    };*/
                    MyDataAccessLayer.copyItem(ItemID);
                    MyDataAccessLayer.AddItemDefaultTags(ItemID, ScadaGlobals.CurrentType);
                    ScadaGlobals.CurrentScadaPopup = -1;
                    ScadaGlobals.Previouspage = -1;
                    RefreshGui();
                    //ScadaClasses.Refresh = true;
                    break;

                case SKTouchAction.Moved:
                    myAddItemButton.GradientStartColor = ScadaColor.uxHoverColor;
                    myAddItemButton.GradientEndColor = ScadaColor.uxHoverColor;
                    break;

                case SKTouchAction.Exited:
                    myAddItemButton.GradientStartColor = ScadaColor.uxItemColor;
                    myAddItemButton.GradientEndColor = ScadaColor.uxItemColor;
                    break;
            }
            args.Handled = true;
        };
        AbsoluteLayout.SetLayoutBounds(myAddItemButton, new Rect(x + 5, y + 5, 30, 30));
        AbsoluteLayout.SetLayoutFlags(myAddItemButton, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(myAddItemButton);

        y = y + FloatPanel.CornerRadius + 50;

        var mySvgButton1 = new ScadaButton
        {
            WidthRequest = w,
            HeightRequest = 25,
            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
            BarBackgroundColor = ScadaColor.uxBackGroundColor,
            IndicatorType = 4,
            GradientStartColor = ScadaColor.uxPopupItemColor,
            GradientEndColor = ScadaColor.uxPopupItemColor,
            CornerRadius = 0,
            TextColor = ScadaColor.uxTextColor,
            FontSize = 16.5F,
            Margin = 0.15F,
            SvgBase64 = MyDataAccessLayer.LoadLibItem("InputSource", 1),
            ButtonText = "Input"
        };
        mySvgButton1.InputTransparent = false;
        mySvgButton1.EnableTouchEvents = true;
        mySvgButton1.Touch += (sender, args) =>
        {
            var pt = args.Location;
            switch (args.ActionType)
            {
                case SKTouchAction.Released:
                    ScadaGlobals.CurrentScadaPopup = -1;
                    ScadaGlobals.PreviousScadaPopup = -1;
                    //ScadaGlobals.CurrentID = ItemID;
                    SKCanvasPopupViews.Clear();
                    ScadaItemTagsMenu(ItemID);
                    foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                    {
                        absoluteLayout.Add(viewItem);
                    }
                    Content = absoluteLayout;

                    //ScadaClasses.CurrentScadaPopup = ScadaClasses.uxItemTagsMenu;
                    //ScadaClasses.Previouspage = -1;
                    //ScadaClasses.Refresh = true;                            
                    break;

                case SKTouchAction.Moved:
                    mySvgButton1.GradientStartColor = ScadaColor.uxHoverColor;
                    mySvgButton1.GradientEndColor = ScadaColor.uxHoverColor;
                    break;

                case SKTouchAction.Exited:
                    mySvgButton1.GradientStartColor = ScadaColor.uxPopupItemColor;
                    mySvgButton1.GradientEndColor = ScadaColor.uxPopupItemColor;
                    break;
            }
            args.Handled = true;
        };
        AbsoluteLayout.SetLayoutBounds(mySvgButton1, new Rect(x, y, w, 20));
        AbsoluteLayout.SetLayoutFlags(mySvgButton1, AbsoluteLayoutFlags.None);

        SKCanvasPopupViews.Add(mySvgButton1);
        y = y + 26;


        var mySvgButton2 = new ScadaButton
        {
            WidthRequest = w,
            HeightRequest = 25,
            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
            BarBackgroundColor = ScadaColor.uxBackGroundColor,
            IndicatorType = 4,

            GradientStartColor = ScadaColor.uxPopupItemColor,
            GradientEndColor = ScadaColor.uxPopupItemColor,
            CornerRadius = 0,
            TextColor = ScadaColor.uxTextColor,
            FontSize = 16.5F,
            Margin = 0.15F,
            SvgBase64 = MyDataAccessLayer.LoadLibItem("RobotArm", 1),
            ButtonText = "Visual"
        };
        mySvgButton2.InputTransparent = false;
        mySvgButton2.EnableTouchEvents = true;
        mySvgButton2.Touch += (sender, args) =>
        {
            var pt = args.Location;
            switch (args.ActionType)
            {
                case SKTouchAction.Released:
                    SKCanvasPopupViews.Clear();
                    ScadaItemTypesMenu(ItemID);
                    foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                    {
                        absoluteLayout.Add(viewItem);
                    }
                    Content = absoluteLayout;

                    //ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxItemTypeMenu;
                    //ScadaGlobals.Previouspage = -1;
                    //ScadaGlobals.Refresh = true;                       
                    break;

                case SKTouchAction.Moved:
                    mySvgButton2.GradientStartColor = ScadaColor.uxHoverColor;
                    mySvgButton2.GradientEndColor = ScadaColor.uxHoverColor;
                    break;

                case SKTouchAction.Exited:
                    mySvgButton2.GradientStartColor = ScadaColor.uxPopupItemColor;
                    mySvgButton2.GradientEndColor = ScadaColor.uxPopupItemColor;
                    break;
            }
            args.Handled = true;
        };

        AbsoluteLayout.SetLayoutBounds(mySvgButton2, new Rect(x, y, w, 20));
        AbsoluteLayout.SetLayoutFlags(mySvgButton2, AbsoluteLayoutFlags.None);

        SKCanvasPopupViews.Add(mySvgButton2);
        y = y + 26;


        var mySvgButton3 = new ScadaButton
        {
            WidthRequest = w,
            HeightRequest = 25,
            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
            BarBackgroundColor = ScadaColor.uxBackGroundColor,
            IndicatorType = 4,
            GradientStartColor = ScadaColor.uxPopupItemColor,
            GradientEndColor = ScadaColor.uxPopupItemColor,
            CornerRadius = 0,
            TextColor = ScadaColor.uxTextColor,
            FontSize = 16.5F,
            Margin = 0.15F,
            SvgBase64 = MyDataAccessLayer.LoadLibItem("GrayPlus", 1),
            ButtonText = "Size"
        };
        mySvgButton3.InputTransparent = false;
        mySvgButton3.EnableTouchEvents = true;
        mySvgButton3.Touch += (sender, args) =>
        {
            var pt = args.Location;
            switch (args.ActionType)
            {
                case SKTouchAction.Released:
                    SKCanvasPopupViews.Clear();
                    ScadaItemSizesMenu(ItemID, ItemType);
                    foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                    {
                        absoluteLayout.Add(viewItem);
                    }
                    Content = absoluteLayout;

                    //ScadaClasses.CurrentScadaPopup = ScadaClasses.uxItemSizeMenu;
                    //ScadaClasses.Previouspage = -1;
                    //ScadaClasses.Refresh = true;                
                    break;

                case SKTouchAction.Moved:
                    mySvgButton3.GradientStartColor = ScadaColor.uxHoverColor;
                    mySvgButton3.GradientEndColor = ScadaColor.uxHoverColor;
                    break;

                case SKTouchAction.Exited:
                    mySvgButton3.GradientStartColor = ScadaColor.uxPopupItemColor;
                    mySvgButton3.GradientEndColor = ScadaColor.uxPopupItemColor;
                    break;
            }
            args.Handled = true;
        };
        AbsoluteLayout.SetLayoutBounds(mySvgButton3, new Rect(x, y, w, 20));
        AbsoluteLayout.SetLayoutFlags(mySvgButton3, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(mySvgButton3);
        y = y + 26;


        var mySvgButton4 = new ScadaButton
        {
            WidthRequest = w,
            HeightRequest = 25,
            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
            BarBackgroundColor = ScadaColor.uxBackGroundColor,
            IndicatorType = 4,

            GradientStartColor = ScadaColor.uxPopupItemColor,
            GradientEndColor = ScadaColor.uxPopupItemColor,
            CornerRadius = 0,
            TextColor = ScadaColor.uxTextColor,
            FontSize = 16.5F,
            Margin = 0.15F,
            SvgBase64 = MyDataAccessLayer.LoadLibItem("AndroidLogo", 1),
            ButtonText = "Page"
        };
        mySvgButton4.InputTransparent = false;
        mySvgButton4.EnableTouchEvents = true;
        mySvgButton4.Touch += (sender, args) =>
        {
            var pt = args.Location;
            switch (args.ActionType)
            {
                case SKTouchAction.Released:
                    SKCanvasPopupViews.Clear();
                    ScadaPages();
                    foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                    {
                        absoluteLayout.Add(viewItem);
                    }
                    Content = absoluteLayout;
                    //ScadaClasses.CurrentScadaPopup = ScadaClasses.uxPageChangeMenu;
                    //ScadaClasses.Previouspage = -1;
                    //ScadaClasses.Refresh = true;                       
                    break;

                case SKTouchAction.Moved:
                    mySvgButton4.GradientStartColor = ScadaColor.uxHoverColor;
                    mySvgButton4.GradientEndColor = ScadaColor.uxHoverColor;
                    break;

                case SKTouchAction.Exited:
                    mySvgButton4.GradientStartColor = ScadaColor.uxPopupItemColor;
                    mySvgButton4.GradientEndColor = ScadaColor.uxPopupItemColor;
                    break;
            }
            args.Handled = true;
        };

        AbsoluteLayout.SetLayoutBounds(mySvgButton4, new Rect(x, y, w, 20));
        AbsoluteLayout.SetLayoutFlags(mySvgButton4, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(mySvgButton4);
        y = y + 26;






        /*

        var popupAlarm = new ScadaButton();
        popupAlarm.AnchorX = 0;
        popupAlarm.AnchorY = 0;
        popupAlarm.CornerRadius = 10;
        popupAlarm.BarBackgroundColor = ScadaColor.uxPanelColor;
        popupAlarm.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        popupAlarm.GradientStartColor = ScadaColor.uxItemColor;
        popupAlarm.GradientEndColor = ScadaColor.uxItemColor;
        popupAlarm.IndicatorColor = ScadaColor.uxPanelColor;
        popupAlarm.IndicatorType = 0;
        popupAlarm.WidthRequest = w;
        popupAlarm.HeightRequest = h;
        popupAlarm.AlternativeTextColor = ScadaColor.uxTextColor;
        popupAlarm.TextColor = ScadaColor.uxTextColor;
        popupAlarm.IsEnabled = true;
        popupAlarm.IsVisible = true;
        AbsoluteLayout.SetLayoutBounds(popupAlarm, new Rect(x, y, w, h));
        AbsoluteLayout.SetLayoutFlags(popupAlarm, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(popupAlarm);
        CreateCloseButton(x, y, w, h, ScadaClasses.uxParameters);

        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);
        var gridRows = MyDataAccessLayer.GetCommProtocols();

        int r = 0;
        y = y + popupAlarm.CornerRadius;
        var TheColor = ScadaColor.uxItemColor;
        y = y + 30;
        foreach (gridRow myRow in gridRows)
        {
            if ((r % 2) == 0)
            {
                TheColor = ScadaColor.uxLightColor;
            }
            else
            {
                TheColor = ScadaColor.uxItemColor;
            }

            var myRowButton = new ScadaButton
            {
                ItemID = 1,
                StyleId = r.ToString(),
                WidthRequest = w,
                HeightRequest = 26,
                Background = ScadaColor.uxItemColor.ToMauiColor(),
                BarBackgroundColor = ScadaColor.uxItemColor,
                IndicatorType = 2,

                IndicatorColor = TheColor,
                GradientStartColor = TheColor,
                GradientEndColor = TheColor,

                CornerRadius = 0,
                TextColor = ScadaColor.uxTextColor,
                FontSize = 18.5F,
                SvgBase64 = "",
            };

            myRowButton.ButtonRow = myRow;
            myRowButton.ButtonText = "";

       
            myRowButton.Touch += (sender, args) =>
            {
                var pt = args.Location;
                switch (args.ActionType)
                {
                    case SKTouchAction.Released:
                        SKCanvasPopupViews.Clear();
                        ScadaGlobals.CurrentCategory = myRowButton.ButtonRow.Id;
                        ScadaProtocolSettings();
                        foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                        {
                            absoluteLayout.Add(viewItem);
                        }
                        Content = absoluteLayout;
                        break;

                    case SKTouchAction.Entered:
                        myRowButton.GradientStartColor = ScadaColor.uxTouchColor;
                        myRowButton.GradientEndColor = ScadaColor.uxTouchColor;
                        myRowButton.IndicatorColor = ScadaColor.uxTouchColor;
                        break;

                    case SKTouchAction.Exited:
                        if ((myRowButton.ButtonRow.Row % 2) == 0)
                        {
                            TheColor = ScadaColor.uxLightColor;
                        }
                        else
                        {
                            TheColor = ScadaColor.uxItemColor;
                        }
                        myRowButton.GradientStartColor = TheColor;
                        myRowButton.GradientEndColor = TheColor;
                        myRowButton.IndicatorColor = TheColor;
                        break;
                }
                args.Handled = true;
            };

            myRowButton.EnableIndicatorBlink();
            myRowButton.InputTransparent = false;
            myRowButton.EnableTouchEvents = true;

            AbsoluteLayout.SetLayoutBounds(myRowButton, new Rect(x, y, myRowButton.WidthRequest, 26));
            AbsoluteLayout.SetLayoutFlags(myRowButton, AbsoluteLayoutFlags.None);
            SKCanvasPopupViews.Add(myRowButton);
            //AbsoluteLayout.SetLayoutBounds(tgRow, new Rect(x + 520, y + 2, 50, 22));
            //AbsoluteLayout.SetLayoutFlags(tgRow, AbsoluteLayoutFlags.None);
            //SKCanvasViews.Add(tgRow);
            y = y + 26;
            r++;
        }
        */
    }




    private void UpdateTagSetting(int tagId, int settingId, string value)
    {
        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);
        switch (settingId)
        {
           case ScadaClasses.pmTagSettingTagname:         
               MyDataAccessLayer.UpdateTagName(tagId, edtInputText.Text);                        
               break;
            case ScadaClasses.pmTagSettingDescription:
                MyDataAccessLayer.UpdateTagDescription(tagId, edtInputText.Text);
                break;
            case ScadaClasses.pmTagSettingHL:
               MyDataAccessLayer.UpdateHL(tagId, edtInputText.Text);
               break;
           case ScadaClasses.pmTagSettingLL:
               MyDataAccessLayer.UpdateLL(tagId, edtInputText.Text);
               break;
           case ScadaClasses.pmTagSettingAlarmEnable:
                MyDataAccessLayer.UpdateAlarmEnable(tagId, edtInputText.Text);
                break;
            case ScadaClasses.pmTagSettingColor:
                MyDataAccessLayer.UpdateTagColor(tagId, edtInputText.Text);
                break;
            case ScadaClasses.pmTagSettingStorageInterval:
                MyDataAccessLayer.UpdateStoreInterval(tagId, edtInputText.Text);
                break;
            case ScadaClasses.pmTagSettingDriver:
                //MyDataAccessLayer.UpdateTagDriver(tagId, edtInputText.Text);
                break;

        }
    }
                  





    private void ScadaEditParameterText()
    {
        double panelWith = 0.4;
        double panelHeight = 0.55;

        double x = (Width / 2) - panelWith * Width / 2;
        double y = (Height / 2) - panelHeight * Height / 2;
        double w = (panelWith * Width);
        double h = (panelHeight * Height);

        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);

        var EditParamPanel = new ScadaButton();
        EditParamPanel.ItemID = -1; // ScadaItem.ItemID;
        EditParamPanel.StyleId = "-1"; // ScadaItem.ItemID.ToString();
        EditParamPanel.AnchorX = 0;
        EditParamPanel.AnchorY = 0;
        EditParamPanel.CornerRadius = 10;
        EditParamPanel.BarBackgroundColor = ScadaColor.uxPanelColor;
        EditParamPanel.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        EditParamPanel.GradientStartColor = ScadaColor.uxItemBackGroundColor;
        EditParamPanel.GradientEndColor = ScadaColor.uxItemBackGroundColor;
        EditParamPanel.IndicatorColor = ScadaColor.uxPanelColor;
        EditParamPanel.WidthRequest = w;
        EditParamPanel.HeightRequest = h;
        EditParamPanel.AlternativeTextColor = ScadaColor.uxTextColor;
        EditParamPanel.TextColor = ScadaColor.uxTextColor;
        EditParamPanel.IsEnabled = true;
        EditParamPanel.IsVisible = true;
        EditParamPanel.IndicatorType = 0;

        var btnApplyParamText = new ScadaButton();
        btnApplyParamText.CornerRadius = 10;
        btnApplyParamText.IndicatorType = 1;
        btnApplyParamText.BarBackgroundColor = ScadaColor.uxPanelColor;
        btnApplyParamText.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        btnApplyParamText.GradientStartColor = ScadaColor.uxPopupItemColor;
        btnApplyParamText.GradientEndColor = ScadaColor.uxPopupItemColor;
        btnApplyParamText.IndicatorColor = ScadaColor.uxItemColor;
        btnApplyParamText.TextColor = ScadaColor.uxTextColor;
        btnApplyParamText.ButtonText = "Apply";
        btnApplyParamText.WidthRequest = 70;
        btnApplyParamText.HeightRequest = 40;
        btnApplyParamText.FontSize = 18;
        btnApplyParamText.IsEnabled = true;
        btnApplyParamText.IsVisible = true;
        btnApplyParamText.EnableTouchEvents = true;
        btnApplyParamText.InputTransparent = false;
        btnApplyParamText.Touch += (sender, args) =>
        {
            var pt = args.Location;
            switch (args.ActionType)
            {
                case SKTouchAction.Released:
                    switch (ScadaGlobals.CurrentType)
                    {
                        case ScadaClasses.pmComputer:
                            Preferences.Default.Set("ConnectionString", edtInputText.Text);
                            break; 
                   
                        case ScadaClasses.pmParameter:
                            MyDataAccessLayer.UpdateParameterValue(ScadaGlobals.CurrentID, edtInputText.Text);
                            break;
                        case ScadaClasses.pmPages:
                            MyDataAccessLayer.UpdatePageName(ScadaGlobals.CurrentID, edtInputText.Text);
                            ScadaGlobals.Currentpage = MyDataAccessLayer.GetPageNoByName(edtInputText.Text);
                            break;
                        case ScadaClasses.pmTagSetting:
                            UpdateTagSetting(ScadaGlobals.CurrentTag, ScadaGlobals.CurrentID, edtInputText.Text);                                                   
                            break;
                    }
                    SKCanvasPopupViews.Clear();              
                    ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxTagSettings;
                    ScadaGlobals.PreviousScadaPopup = ScadaClasses.uxEditParameterText;
                    ScadaGlobals.Previouspage = -1;
                    uxtimer.Interval = 50;
                    break;

                case SKTouchAction.Pressed:
                    break;

                case SKTouchAction.Moved:
                    btnApplyParamText.GradientStartColor = ScadaColor.uxHoverColor;
                    btnApplyParamText.GradientEndColor = ScadaColor.uxHoverColor;
                    break;

                case SKTouchAction.Exited:
                    btnApplyParamText.GradientStartColor = ScadaColor.uxPopupItemColor;
                    btnApplyParamText.GradientEndColor = ScadaColor.uxPopupItemColor;
                    break;
            }
            args.Handled = true;
        };

        AbsoluteLayout.SetLayoutBounds(btnApplyParamText, new Rect(x + EditParamPanel.WidthRequest - (EditParamPanel.WidthRequest / 2) - (btnApplyParamText.WidthRequest / 2), y + EditParamPanel.HeightRequest - btnApplyParamText.HeightRequest * 1.5F, btnApplyParamText.WidthRequest, btnApplyParamText.HeightRequest));
        AbsoluteLayout.SetLayoutFlags(btnApplyParamText, AbsoluteLayoutFlags.None);
        AbsoluteLayout.SetLayoutBounds(EditParamPanel, new Rect(x, y,  w, h));
        AbsoluteLayout.SetLayoutFlags(EditParamPanel, AbsoluteLayoutFlags.None);

        SKCanvasPopupViews.Add(EditParamPanel);
        SKCanvasPopupViews.Add(btnApplyParamText);
        CreateCloseButton(x, y, w, h, ScadaGlobals.PreviousScadaPopup);

        foreach (SKCanvasView viewItem in SKCanvasPopupViews)
        {
            absoluteLayout.Add(viewItem);
        }
    
        edtInputText.Placeholder = "EditText";
        edtInputText.WidthRequest = w - 20;
        edtInputText.HeightRequest = h / 2;
        edtInputText.TextColor = ScadaColor.uxTextColor.ToMauiColor();
        edtInputText.BackgroundColor = ScadaColor.uxItemColor.ToMauiColor();
        edtInputText.PlaceholderColor = ScadaColor.uxPopupItemColor.ToMauiColor();
        edtInputText.FontAttributes = FontAttributes.Bold;
        edtInputText.IsVisible = true;
        AbsoluteLayout.SetLayoutBounds(edtInputText, new Rect(x + 10, y + 30, w - 20, h / 2));
        AbsoluteLayout.SetLayoutFlags(edtInputText, AbsoluteLayoutFlags.None);
        edtInputText.TextChanged += OnEditorTextChanged;
        edtInputText.Completed += OnEditorCompleted;
        absoluteLayout.Add(edtInputText);




        /*
        foreach (SKCanvasView viewItem in SKCanvasPopupViews)
        {
            absoluteLayout.Remove(viewItem);
        }
        SKCanvasPopupViews.Clear();
        edtInputText.Text = myRow.col1text;
        ScadaGlobals.CurrentScadaPopup = -1;
        ScadaGlobals.CurrentType = ScadaClasses.pmPages;
        ScadaGlobals.CurrentID = myRow.Id;
        SKCanvasPopupViews.Clear();
        ScadaEditParameterText();

        foreach (SKCanvasView viewItem in SKCanvasPopupViews)
        {
            absoluteLayout.Add(viewItem);
        }

        edtInputText.Placeholder = "EditText";
        edtInputText.WidthRequest = w - 20;
        edtInputText.HeightRequest = h / 2;
        edtInputText.TextColor = ScadaColor.uxTextColor.ToMauiColor();
        edtInputText.BackgroundColor = ScadaColor.uxItemColor.ToMauiColor();
        edtInputText.PlaceholderColor = ScadaColor.uxPopupItemColor.ToMauiColor();
        edtInputText.FontAttributes = FontAttributes.Bold;
        edtInputText.IsVisible = true;
        AbsoluteLayout.SetLayoutBounds(edtInputText, new Rect(x + 10, yPlaceHolder, w - 20, h / 2));
        AbsoluteLayout.SetLayoutFlags(edtInputText, AbsoluteLayoutFlags.None);
        edtInputText.TextChanged += OnEditorTextChanged;
        edtInputText.Completed += OnEditorCompleted;
        absoluteLayout.Add(edtInputText);
        Content = absoluteLayout;
        */
    }


    /*
    private void ScadaTagsGrid()
    {
        double panelWith = 0.4;
        double panelHeight = 0.55;

        double x = (Width / 2) - panelWith * Width / 2;
        double y = (Height / 2) - panelHeight * Height / 2;
        double w = (panelWith * Width);
        double h = (panelHeight * Height);
        var popupTags = new ScadaButton();
        popupTags.AnchorX = 0;
        popupTags.AnchorY = 0;
        popupTags.CornerRadius = 10;
        popupTags.BarBackgroundColor = ScadaColor.uxPanelColor;
        popupTags.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        popupTags.GradientStartColor = ScadaColor.uxItemColor;
        popupTags.GradientEndColor = ScadaColor.uxItemColor;
        popupTags.IndicatorColor = ScadaColor.uxPanelColor;
        popupTags.IndicatorType = 0;
        popupTags.WidthRequest = w;
        popupTags.HeightRequest = h;
        popupTags.AlternativeTextColor = ScadaColor.uxTextColor;
        popupTags.TextColor = ScadaColor.uxTextColor;
        popupTags.IsEnabled = true;
        popupTags.IsVisible = true;
        AbsoluteLayout.SetLayoutBounds(popupTags, new Rect(x, y, w, h));
        AbsoluteLayout.SetLayoutFlags(popupTags, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(popupTags);
        CreateCloseButton(x, y, w, h, ScadaClasses.uxParameters);
        
        CreateFwdButton(x, y, w, h);
        CreateRwdButton(x, y, w, h);
        CreateRemoveTagButton(ScadaGlobals.CurrentItem, ScadaGlobals.CurrentTag, x - (w / 2), y, w, h);

        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);

        //int Dig = MyDataAccessLayer.GetItemIsDigital(ScadaClasses.CurrentItem);

        //int TypeOfTag = 1;
        
        //if ((ScadaClasses.CurrentType == ScadaClasses.uxToggle) ||
        //    (ScadaClasses.CurrentType == ScadaClasses.uxButton) ||
        //    (ScadaClasses.CurrentType == ScadaClasses.uxCircularProgress))
        //    TypeOfTag = 1;
        //else
            
        //TypeOfTag = 3;

        var gridRows = MyDataAccessLayer.ReadTags("", 0, iMenuOffsetRows, iMenuOffsetRows + 5);

        y = y + popupTags.CornerRadius + 50;
        foreach (gridRow row in gridRows)
        {
            var myButton = new ScadaButton
            {
                WidthRequest = w,
                HeightRequest = 25,
                Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
                BarBackgroundColor = ScadaColor.uxBackGroundColor,
                IndicatorType = 3,
                IndicatorColor = GetStatusColor(row.Status),
                GradientStartColor = ScadaColor.uxPopupItemColor,
                GradientEndColor = ScadaColor.uxPopupItemColor,
                CornerRadius = 0,
                TextColor = ScadaColor.uxTextColor,
                FontSize = 16.5F,
                SvgBase64 = MyDataAccessLayer.LoadLibItem("InputSource", 1),
            };

            myButton.ButtonRow = row;
            myButton.ButtonText = row.col1text;
            myButton.EnableTouchEvents = true;
            myButton.InputTransparent = false;

            myButton.Touch += (sender, args) =>
            {
                var pt = args.Location;
                switch (args.ActionType)
                {
                    case SKTouchAction.Released:
                    
                        if (ScadaGlobals.CurrentTag == 0)
                        {
                            MyDataAccessLayer.AddItemTag(ScadaGlobals.CurrentItem, ScadaGlobals.CurrentType, row.TagID, row.Row, "");
                        }
                        else
                        {
                            MyDataAccessLayer.SetItemTag(ScadaGlobals.CurrentItem, row.TagID, ScadaGlobals.CurrentTagSequence);
                        }


                        // SKCanvasPopupViews.Clear();
                        // RefreshGui();
                        // ScadaGlobals.CurrentTag = row.TagID;

                       // ScadaGlobals.PreviousScadaPopup = ScadaClasses.uxTagsGrid;
                         ScadaGlobals.CurrentScadaPopup = -1;
                        //ScadaGlobals.Previouspage = -1;
                        uxtimer.Interval = 100;
                        break;

                    case SKTouchAction.Moved:
                        myButton.GradientStartColor = ScadaColor.uxHoverColor;
                        myButton.GradientEndColor = ScadaColor.uxHoverColor;
                        myButton.IndicatorColor = GetStatusColor(row.Status);
                        break;

                    case SKTouchAction.Exited:
                        myButton.GradientStartColor = ScadaColor.uxPopupItemColor;
                        myButton.GradientEndColor = ScadaColor.uxPopupItemColor;
                        myButton.IndicatorColor = GetStatusColor(row.Status);
                        break;
                }
                args.Handled = true;
            };

            AbsoluteLayout.SetLayoutBounds(myButton, new Rect(x, y, myButton.WidthRequest, 25));
            AbsoluteLayout.SetLayoutFlags(myButton, AbsoluteLayoutFlags.None);
            myButton.EnableIndicatorBlink();
            myButton.InputTransparent = false;
            myButton.EnableTouchEvents = true;
            SKCanvasPopupViews.Add(myButton);
            y = y + 26;
     
        }
    }
    */
    private void ScadaTimeSpanMenu()
    {
        double panelWith = 0.4;
        double panelHeight = 0.55;

        double x = (Width / 2) - panelWith * Width / 2;
        double y = (Height / 2) - panelHeight * Height / 2;
        double w = (panelWith * Width);
        double h = (panelHeight * Height);
        var popupAlarm = new ScadaButton();
        popupAlarm.AnchorX = 0;
        popupAlarm.AnchorY = 0;
        popupAlarm.CornerRadius = 10;
        popupAlarm.BarBackgroundColor = ScadaColor.uxPanelColor;
        popupAlarm.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        popupAlarm.GradientStartColor = ScadaColor.uxItemColor;
        popupAlarm.GradientEndColor = ScadaColor.uxItemColor;
        popupAlarm.IndicatorColor = ScadaColor.uxPanelColor;
        popupAlarm.IndicatorType = 0;
        popupAlarm.WidthRequest = w;
        popupAlarm.HeightRequest = h;
        popupAlarm.AlternativeTextColor = ScadaColor.uxTextColor;
        popupAlarm.TextColor = ScadaColor.uxTextColor;
        popupAlarm.IsEnabled = true;
        popupAlarm.IsVisible = true;
        AbsoluteLayout.SetLayoutBounds(popupAlarm, new Rect(x, y, w, h));
        AbsoluteLayout.SetLayoutFlags(popupAlarm, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(popupAlarm);
        CreateCloseButton(x, y, w, h, -1);

        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);
        //var gridRows = MyDataAccessLayer.GetCommProtocols();
        var gridRows = MyDataAccessLayer.ReadTimeSpans();

        int r = 0;
        y = y + popupAlarm.CornerRadius;
        var TheColor = ScadaColor.uxItemColor;
        y = y + 30;
        foreach (gridRow myRow in gridRows)
        {
            if ((r % 2) == 0)
            {
                TheColor = ScadaColor.uxLightColor;
            }
            else
            {
                TheColor = ScadaColor.uxItemColor;
            }

            var myRowButton = new ScadaButton
            {
                ItemID = 1,
                StyleId = r.ToString(),
                WidthRequest = w,
                HeightRequest = 26,
                Background = ScadaColor.uxItemColor.ToMauiColor(),
                BarBackgroundColor = ScadaColor.uxItemColor,
                IndicatorType = 2,

                IndicatorColor = TheColor,
                GradientStartColor = TheColor,
                GradientEndColor = TheColor,

                CornerRadius = 0,
                TextColor = ScadaColor.uxTextColor,
                FontSize = 18.5F,
                SvgBase64 = "",
            };

            myRowButton.ButtonRow = myRow;
            myRowButton.ButtonText = "";

            myRowButton.Touch += (sender, args) =>
            {
                var pt = args.Location;
                switch (args.ActionType)
                {
                    case SKTouchAction.Released:
                        var myChartSettings = MyDataAccessLayer.GetChartSettings(ScadaGlobals.CurrentItem);
                        myChartSettings.iSpan = myRow.Id;
                        MyDataAccessLayer.SetChartSettings(myChartSettings, ScadaGlobals.CurrentItem);
                        RefreshGui();
                
                        break;

                    case SKTouchAction.Entered:
                        myRowButton.GradientStartColor = ScadaColor.uxTouchColor;
                        myRowButton.GradientEndColor = ScadaColor.uxTouchColor;
                        myRowButton.IndicatorColor = ScadaColor.uxTouchColor;
                        break;

                    case SKTouchAction.Exited:
                        if ((myRowButton.ButtonRow.Row % 2) == 0)
                        {
                            TheColor = ScadaColor.uxLightColor;
                        }
                        else
                        {
                            TheColor = ScadaColor.uxItemColor;
                        }
                        myRowButton.GradientStartColor = TheColor;
                        myRowButton.GradientEndColor = TheColor;
                        myRowButton.IndicatorColor = TheColor;
                        break;
                }
                args.Handled = true;
            };

            myRowButton.EnableIndicatorBlink();
            myRowButton.InputTransparent = false;
            myRowButton.EnableTouchEvents = true;

            AbsoluteLayout.SetLayoutBounds(myRowButton, new Rect(x, y, myRowButton.WidthRequest, 26));
            AbsoluteLayout.SetLayoutFlags(myRowButton, AbsoluteLayoutFlags.None);
            SKCanvasPopupViews.Add(myRowButton);
            y = y + 26;
            r++;
        }
    }






    private void ScadaTagsGrid()
    {
        double panelWith = 0.4;
        double panelHeight = 0.55;

        double x = (Width / 2) - panelWith * Width / 2;
        double y = (Height / 2) - panelHeight * Height / 2;
        double w = (panelWith * Width);
        double h = (panelHeight * Height);


        var popupAlarm = new ScadaButton();
        popupAlarm.AnchorX = 0;
        popupAlarm.AnchorY = 0;
        popupAlarm.CornerRadius = 10;
        popupAlarm.BarBackgroundColor = ScadaColor.uxPanelColor;
        popupAlarm.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        popupAlarm.GradientStartColor = ScadaColor.uxItemColor;
        popupAlarm.GradientEndColor = ScadaColor.uxItemColor; 
        popupAlarm.IndicatorColor = ScadaColor.uxItemColor;
        popupAlarm.IndicatorType = 0;
        popupAlarm.WidthRequest = w;
        popupAlarm.HeightRequest = h;
        popupAlarm.AlternativeTextColor = ScadaColor.uxTextColor;
        popupAlarm.TextColor = ScadaColor.uxTextColor;
        popupAlarm.IsEnabled = true;
        popupAlarm.IsVisible = true;
        AbsoluteLayout.SetLayoutBounds(popupAlarm, new Rect(x, y, w, h));
        AbsoluteLayout.SetLayoutFlags(popupAlarm, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(popupAlarm);
        CreateCloseButton(x, y, w, h, -1);
        CreateAddTagButton(x, y, w, h);

        CreateFwdButton(x, y, w, h);
        CreateRwdButton(x, y, w, h);
        //CreateRemoveTagButton(ScadaGlobals.CurrentItem, ScadaGlobals.CurrentTag, x - (w / 2), y, w, h);

        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);

        y = y + popupAlarm.CornerRadius;
        var TheColor = ScadaColor.uxItemColor;
        y = y + 30;
        for (int r = 0; r < 6; r++)
        {
            if ((r % 2) == 0)
            {
                TheColor = ScadaColor.uxLightColor;
            }
            else
            {
                TheColor = ScadaColor.uxItemColor;
            }

            var myRowButton = new ScadaButton
            {
                ItemID = 1,//ScadaItem.ItemID,
                StyleId = r.ToString(),
                WidthRequest = w,
                HeightRequest = 26,
                Background = ScadaColor.uxItemColor.ToMauiColor(),
                BarBackgroundColor = ScadaColor.uxItemColor,
                IndicatorType = 2,

                IndicatorColor = TheColor,
                GradientStartColor = TheColor,
                GradientEndColor = TheColor,

                CornerRadius = 0,
                TextColor = ScadaColor.uxTextColor,
                FontSize = 18.5F,
                SvgBase64 = "",
            };

            var gridRow = new gridRow
            {
                Status = 0,
                col1text = "",
                col1width = 0F,
                col2text = "",
                col2width = 0F,
                col3text = "",
                col3width = 0F,
                col4text = "",
                col4width = 0F,
                col5text = "",
                col5width = 0F,
                col6text = "",
                col6width = 0F,
                Row = r,
                Id = -1
            };
            myRowButton.ButtonRow = gridRow;
            myRowButton.ButtonText = "";
            myRowButton.Touch += (sender, args) =>
            {
                var pt = args.Location;
                switch (args.ActionType)
                {
                    case SKTouchAction.Released:
                     
                        ScadaGlobals.PreviousScadaPopup = ScadaClasses.uxTagsGrid;
                        ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxTagSettings;
                        ScadaGlobals.CurrentTag = myRowButton.ButtonRow.TagID;
                        foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                        {
                           absoluteLayout.Remove(viewItem);
                        }
                        SKCanvasPopupViews.Clear();

                        uxtimer.Interval = 50;
                        break;

                    case SKTouchAction.Entered:
                        myRowButton.GradientStartColor = ScadaColor.uxTouchColor;
                        myRowButton.GradientEndColor = ScadaColor.uxTouchColor;
                        myRowButton.IndicatorColor = ScadaColor.uxTouchColor;
                        break;

                    case SKTouchAction.Exited:
                        
                        if ((myRowButton.ButtonRow.Row % 2) == 0)
                        {
                            TheColor = ScadaColor.uxLightColor;
                        }
                        else
                        {
                            TheColor = ScadaColor.uxItemColor;
                        }
                        
                        myRowButton.GradientStartColor = TheColor;
                        myRowButton.GradientEndColor = TheColor;
                        myRowButton.IndicatorColor = TheColor;
                        break;
                }
                args.Handled = true;
            };
            myRowButton.EnableIndicatorBlink();
            myRowButton.InputTransparent = false;
            myRowButton.EnableTouchEvents = true;

            AbsoluteLayout.SetLayoutBounds(myRowButton, new Rect(x, y, w, 26));
            AbsoluteLayout.SetLayoutFlags(myRowButton, AbsoluteLayoutFlags.None);
            SKCanvasPopupViews.Add(myRowButton);
            y = y + 26;
        }
    }





    private void ScadaTagsMenu()
    {
        double panelWith = 0.4;
        double panelHeight = 0.55;

        double x = (Width / 2) - panelWith * Width / 2;
        double y = (Height / 2) - panelHeight * Height / 2;
        double w = (panelWith * Width);
        double h = (panelHeight * Height);


        var popupAlarm = new ScadaButton();
        popupAlarm.AnchorX = 0;
        popupAlarm.AnchorY = 0;
        popupAlarm.CornerRadius = 10;
        popupAlarm.BarBackgroundColor = ScadaColor.uxPanelColor;
        popupAlarm.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        popupAlarm.GradientStartColor = ScadaColor.uxItemColor;
        popupAlarm.GradientEndColor = ScadaColor.uxItemColor;
        popupAlarm.IndicatorColor = ScadaColor.uxItemColor;
        popupAlarm.IndicatorType = 0;
        popupAlarm.WidthRequest = w;
        popupAlarm.HeightRequest = h;
        popupAlarm.AlternativeTextColor = ScadaColor.uxTextColor;
        popupAlarm.TextColor = ScadaColor.uxTextColor;
        popupAlarm.IsEnabled = true;
        popupAlarm.IsVisible = true;
        AbsoluteLayout.SetLayoutBounds(popupAlarm, new Rect(x, y, w, h));
        AbsoluteLayout.SetLayoutFlags(popupAlarm, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(popupAlarm);
        CreateCloseButton(x, y, w, h, -1);
        CreateAddTagButton(x, y, w, h);

        CreateFwdButton(x, y, w, h);
        CreateRwdButton(x, y, w, h);
        CreateRemoveTagButton(ScadaGlobals.CurrentItem, ScadaGlobals.CurrentTag, x - (w / 2), y, w, h);

        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);

        y = y + popupAlarm.CornerRadius;
        var TheColor = ScadaColor.uxItemColor;
        y = y + 30;
        for (int r = 0; r < 6; r++)
        {
            if ((r % 2) == 0)
            {
                TheColor = ScadaColor.uxLightColor;
            }
            else
            {
                TheColor = ScadaColor.uxItemColor;
            }

            var myRowButton = new ScadaButton
            {
                ItemID = 1,//ScadaItem.ItemID,
                StyleId = r.ToString(),
                WidthRequest = w,
                HeightRequest = 26,
                Background = ScadaColor.uxItemColor.ToMauiColor(),
                BarBackgroundColor = ScadaColor.uxItemColor,
                IndicatorType = 2,

                IndicatorColor = TheColor,
                GradientStartColor = TheColor,
                GradientEndColor = TheColor,

                CornerRadius = 0,
                TextColor = ScadaColor.uxTextColor,
                FontSize = 18.5F,
                SvgBase64 = "",
            };

            var gridRow = new gridRow
            {
                Status = 0,
                col1text = "",
                col1width = 0F,
                col2text = "",
                col2width = 0F,
                col3text = "",
                col3width = 0F,
                col4text = "",
                col4width = 0F,
                col5text = "",
                col5width = 0F,
                col6text = "",
                col6width = 0F,
                Row = r,
                Id = -1
            };
            myRowButton.ButtonRow = gridRow;
            myRowButton.ButtonText = "";
            myRowButton.Touch += (sender, args) =>
            {
                var pt = args.Location;
                switch (args.ActionType)
                {
                    case SKTouchAction.Released:
                        ScadaGlobals.Previouspage = -1;
                        ScadaGlobals.PreviousScadaPopup = ScadaClasses.uxTagsGrid;
                        ScadaGlobals.CurrentScadaPopup = -1;

                        if (ScadaGlobals.CurrentTag == 0)
                        {
                            MyDataAccessLayer.AddItemTag(ScadaGlobals.CurrentItem, ScadaGlobals.CurrentType, myRowButton.ButtonRow.TagID, ScadaGlobals.CurrentTagSequence, myRowButton.ButtonRow.col1text);
                        }
                        else
                        {
                            MyDataAccessLayer.SetItemTag(ScadaGlobals.CurrentItem, myRowButton.ButtonRow.TagID, ScadaGlobals.CurrentTagSequence);
                        }
                        uxtimer.Interval = 50;
                        break;

                    case SKTouchAction.Entered:
                        myRowButton.GradientStartColor = ScadaColor.uxTouchColor;
                        myRowButton.GradientEndColor = ScadaColor.uxTouchColor;
                        myRowButton.IndicatorColor = ScadaColor.uxTouchColor;
                        break;

                    case SKTouchAction.Exited:

                        if ((myRowButton.ButtonRow.Row % 2) == 0)
                        {
                            TheColor = ScadaColor.uxLightColor;
                        }
                        else
                        {
                            TheColor = ScadaColor.uxItemColor;
                        }

                        myRowButton.GradientStartColor = TheColor;
                        myRowButton.GradientEndColor = TheColor;
                        myRowButton.IndicatorColor = TheColor;
                        break;
                }
                args.Handled = true;
            };
            myRowButton.EnableIndicatorBlink();
            myRowButton.InputTransparent = false;
            myRowButton.EnableTouchEvents = true;

            AbsoluteLayout.SetLayoutBounds(myRowButton, new Rect(x, y, w, 26));
            AbsoluteLayout.SetLayoutFlags(myRowButton, AbsoluteLayoutFlags.None);
            SKCanvasPopupViews.Add(myRowButton);
            y = y + 26;
        }
    }






    private void ScadaPages()
    {
        double panelWith = 0.4;
        double panelHeight = 0.55;

        double x = (Width / 2) - panelWith * Width / 2;
        double y = (Height / 2) - panelHeight * Height / 2;
        double w = (panelWith * Width);
        double h = (panelHeight * Height);
        double yPlaceHolder = y + 50;
        var popupPages = new ScadaButton();
        popupPages.AnchorX = 0;
        popupPages.AnchorY = 0;
        popupPages.CornerRadius = 10;
        popupPages.BarBackgroundColor = ScadaColor.uxPanelColor;
        popupPages.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        popupPages.GradientStartColor = ScadaColor.uxItemColor;
        popupPages.GradientEndColor = ScadaColor.uxItemColor;
        popupPages.IndicatorColor = ScadaColor.uxPanelColor;
        popupPages.IndicatorType = 0;
        popupPages.WidthRequest = w;
        popupPages.HeightRequest = h;
        popupPages.AlternativeTextColor = ScadaColor.uxTextColor;
        popupPages.TextColor = ScadaColor.uxTextColor;
        popupPages.IsEnabled = true;
        popupPages.IsVisible = true;
        AbsoluteLayout.SetLayoutBounds(popupPages, new Rect(x, y, w, h));
        AbsoluteLayout.SetLayoutFlags(popupPages, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(popupPages);
        CreateCloseButton(x, y, w, h, -1);

        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);
        var gridRows = MyDataAccessLayer.ReadPages(sFilter, 0, 100, 0, 10);

        int r = 0;
        y = y + popupPages.CornerRadius;
        var TheColor = ScadaColor.uxItemColor;
        y = y + 30;
        foreach (gridRow myRow in gridRows)
        {
            if ((r % 2) == 0)
            {
                TheColor = ScadaColor.uxLightColor;
            }
            else
            {
                TheColor = ScadaColor.uxItemColor;
            }

            var myRowButton = new ScadaButton
            {
                ItemID = 1,
                StyleId = r.ToString(),
                WidthRequest = w,
                HeightRequest = 26,
                Background = ScadaColor.uxItemColor.ToMauiColor(),
                BarBackgroundColor = ScadaColor.uxItemColor,
                IndicatorType = 2,
                IndicatorColor = TheColor,
                GradientStartColor = TheColor,
                GradientEndColor = TheColor,
                CornerRadius = 0,
                TextColor = ScadaColor.uxTextColor,
                FontSize = 18.5F,
                SvgBase64 = "",
            };

            myRowButton.ButtonRow = myRow;
            myRowButton.ButtonText = "";
            myRowButton.EnableIndicatorBlink();
            myRowButton.InputTransparent = true;
            myRowButton.EnableTouchEvents = false;
            AbsoluteLayout.SetLayoutBounds(myRowButton, new Rect(x, y, myRowButton.WidthRequest, 26));
            AbsoluteLayout.SetLayoutFlags(myRowButton, AbsoluteLayoutFlags.None);
            SKCanvasPopupViews.Add(myRowButton);
        
            var btnDots = new ScadaButton();
            btnDots.GradientStartColor = ScadaColor.uxPopupItemColor;
            btnDots.GradientEndColor = ScadaColor.uxPopupItemColor;
            btnDots.CornerRadius = 15;
            btnDots.ItemID = 1;
            btnDots.EnableTouchEvents = true;
            btnDots.InputTransparent = false;
            btnDots.HeightRequest = 22;
            btnDots.WidthRequest = 22;
            btnDots.SvgBase64 = MyDataAccessLayer.LoadLibItem("Threedots", 1);
            btnDots.IndicatorType = 3;
            btnDots.Margin = 0.15F;
            btnDots.ButtonText = "";
            btnDots.Touch += (sender, args) =>
            {
                switch (args.ActionType)
                {
                    case SKTouchAction.Released:
                        foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                        {
                            absoluteLayout.Remove(viewItem);
                        }
                        SKCanvasPopupViews.Clear();
                        edtInputText.Text = myRow.col1text;
                        ScadaGlobals.CurrentScadaPopup = -1;
                        ScadaGlobals.CurrentType = ScadaClasses.pmPages;
                        ScadaGlobals.CurrentID = myRow.Id;                 
                        SKCanvasPopupViews.Clear();
                        ScadaEditParameterText();   
                       /*
                        foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                        {
                            absoluteLayout.Add(viewItem);
                        }
                        edtInputText.Placeholder = "EditText";
                        edtInputText.WidthRequest = w-20;
                        edtInputText.HeightRequest = h/2;
                        edtInputText.TextColor = ScadaColor.uxTextColor.ToMauiColor();
                        edtInputText.BackgroundColor = ScadaColor.uxItemColor.ToMauiColor();
                        edtInputText.PlaceholderColor = ScadaColor.uxPopupItemColor.ToMauiColor();
                        edtInputText.FontAttributes = FontAttributes.Bold;
                        edtInputText.IsVisible = true;
                        AbsoluteLayout.SetLayoutBounds(edtInputText, new Rect(x+10, yPlaceHolder, w-20, h/2));
                        AbsoluteLayout.SetLayoutFlags(edtInputText, AbsoluteLayoutFlags.None);
                        edtInputText.TextChanged += OnEditorTextChanged;
                        edtInputText.Completed += OnEditorCompleted;
                        absoluteLayout.Add(edtInputText);
                        */
                        Content = absoluteLayout;
                        break;

                    case SKTouchAction.Pressed:
                        break;

                    case SKTouchAction.Moved:
                        btnDots.GradientStartColor = ScadaColor.uxHoverColor;
                        btnDots.GradientEndColor = ScadaColor.uxHoverColor;
                        btnDots.IndicatorColor = ScadaColor.uxHoverColor;
                        break;

                    case SKTouchAction.Exited:
                        btnDots.GradientStartColor = ScadaColor.uxPopupItemColor;
                        btnDots.GradientEndColor = ScadaColor.uxPopupItemColor;
                        btnDots.IndicatorColor = ScadaColor.uxPopupItemColor;
                        break;
                }
                args.Handled = true;
            };
            AbsoluteLayout.SetLayoutBounds(btnDots, new Rect(x + w - 80 + 15, y + 2, 50, 22));
            AbsoluteLayout.SetLayoutFlags(btnDots, AbsoluteLayoutFlags.None);
            SKCanvasPopupViews.Add(btnDots);
                   
            y = y + 26;
            r++;
        }
    }



    private void ScadaParameters()
    {
        double panelWith = 0.4;
        double panelHeight = 0.55;

        double x = (Width / 2) - panelWith * Width / 2;
        double y = (Height / 2) - panelHeight * Height / 2;
        double w = (panelWith * Width);
        double h = (panelHeight * Height);
        var popupParameters = new ScadaButton();
        popupParameters.AnchorX = 0;
        popupParameters.AnchorY = 0;
        popupParameters.CornerRadius = 10;
        popupParameters.BarBackgroundColor = ScadaColor.uxPanelColor;
        popupParameters.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        popupParameters.GradientStartColor = ScadaColor.uxItemColor;
        popupParameters.GradientEndColor = ScadaColor.uxItemColor; 
        popupParameters.IndicatorColor = ScadaColor.uxPanelColor;
        popupParameters.IndicatorType = 0;
        popupParameters.WidthRequest = w;
        popupParameters.HeightRequest = h;
        popupParameters.AlternativeTextColor = ScadaColor.uxTextColor;
        popupParameters.TextColor = ScadaColor.uxTextColor;
        popupParameters.IsEnabled = true;
        popupParameters.IsVisible = true;
        AbsoluteLayout.SetLayoutBounds(popupParameters, new Rect(x, y, w, h));
        AbsoluteLayout.SetLayoutFlags(popupParameters, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(popupParameters);
        CreateCloseButton(x, y, w, h, -1);

        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);
        var gridRows = MyDataAccessLayer.ReadParameters(sFilter,1,5,10,0,10);

        int r = 0;
        y = y + popupParameters.CornerRadius;
        var TheColor = ScadaColor.uxItemColor;
        y = y + 30;
        foreach (gridRow myRow in gridRows)
        {
            if ((r % 2) == 0)
            {
                TheColor = ScadaColor.uxLightColor;
            }
            else
            {
                TheColor = ScadaColor.uxItemColor;
            }

            var myRowButton = new ScadaButton
            {
                ItemID = 1,
                StyleId = r.ToString(),
                WidthRequest = w,
                HeightRequest = 26,
                Background = ScadaColor.uxItemColor.ToMauiColor(),
                BarBackgroundColor = ScadaColor.uxItemColor,
                IndicatorType = 2,

                IndicatorColor = TheColor,
                GradientStartColor = TheColor,
                GradientEndColor = TheColor,

                CornerRadius = 0,
                TextColor = ScadaColor.uxTextColor,
                FontSize = 18.5F,
                SvgBase64 = "",
            };
            
            //var btnDots = new ScadaButton() { };
            //var tgRow = new Toggle() { };
            //var Items = new List<ScadaClasses.Telegram> { };

            myRowButton.ButtonRow = myRow;
            myRowButton.ButtonText = "";
            myRowButton.EnableIndicatorBlink();
            myRowButton.InputTransparent = true;
            myRowButton.EnableTouchEvents = false;
            AbsoluteLayout.SetLayoutBounds(myRowButton, new Rect(x, y, myRowButton.WidthRequest, 26));
            AbsoluteLayout.SetLayoutFlags(myRowButton, AbsoluteLayoutFlags.None);
            SKCanvasPopupViews.Add(myRowButton);

            if (myRow.DataType == 0)
            {
                //Add Toggle
                var tgRow = new Toggle();
                tgRow.ItemID = -1;
                tgRow.StyleId = myRow.Row.ToString();
                tgRow.CornerRadius = 1;
                tgRow.BarBackgroundColor = ScadaColor.uxItemColor;
                tgRow.BackgroundColor = ScadaColor.uxItemColor.ToMauiColor();
                tgRow.GradientStartColor = ScadaColor.uxPopupItemColor;
                tgRow.GradientEndColor = ScadaColor.uxPopupItemColor;
                tgRow.TextColor = ScadaColor.uxTextColor;
                tgRow.WidthRequest = 50;
                tgRow.HeightRequest = 22;
                tgRow.FontSize = 18;
                tgRow.PV = new ItemValue();
                tgRow.SV = new ItemValue();
                tgRow.IsEnabled = true;
                tgRow.IsVisible = true;
                tgRow.EnableTouchEvents = true;
                tgRow.InputTransparent = false;
                tgRow.StyleId = r.ToString();
                tgRow.PV.TagID = myRowButton.ButtonRow.Id;
                tgRow.SV.TagID = myRowButton.ButtonRow.Id;
                tgRow.PV.Value = myRowButton.ButtonRow.Value;
                tgRow.SV.Value = myRowButton.ButtonRow.Value;
                //Value måste tillldelas
                tgRow.Touch += (sender, args) =>
                {
                    switch (args.ActionType)
                    {
                        case SKTouchAction.Released:
                            if (tgRow.SV.Value > 0.5F)
                            {
                                tgRow.SV.Value = 0;
                                tgRow.PV.Value = 0;
                                MyDataAccessLayer.UpdateParameterValue(myRowButton.ButtonRow.Id, tgRow.SV.Value.ToString());
                            }
                            else
                            {
                                tgRow.SV.Value = 1;
                                tgRow.PV.Value = 1;
                                MyDataAccessLayer.UpdateParameterValue(myRowButton.ButtonRow.Id, tgRow.SV.Value.ToString());
                            }
                            tgRow.InvalidateSurface();
                            switch (myRow.col1text)
                            {                          
                                case "Dark":
                                    if (MyDataAccessLayer.ReadParameterByName("Dark") == "1")
                                    {
                                        ScadaColor.uxBackGroundColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkBackgroundColor"));
                                        ScadaColor.uxItemBackGroundColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkItemBackgroundColor"));
                                        ScadaColor.uxPanelColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkPanelColor"));
                                        ScadaColor.uxItemColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkItemColor"));
                                        ScadaColor.uxGradientStartColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkItemColor"));
                                        ScadaColor.uxGradientEndColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkItemColor"));
                                        ScadaColor.uxPopupColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkPopupColor"));
                                        ScadaColor.uxPopupItemColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkPopupItemColor"));
                                        ScadaColor.uxTextColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkTextColor"));
                                        ScadaColor.uxLightColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkLightColor"));
                                        ScadaColor.uxHoverColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkHoverColor"));
                                        ScadaColor.uxTouchColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkTouchColor"));
                                        ScadaColor.uxOffColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkOffColor"));
                                        ScadaColor.uxLightColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkLightColor"));
                                        ScadaColor.uxGridThinColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkGridThinColor"));
                                        ScadaColor.uxGridFatColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkGridFatColor"));
                                        ScadaColor.uxTransparentButtonColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkTransparentButtonColor"));
                                    }
                                    else
                                    {
                                        ScadaColor.uxBackGroundColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtBackgroundColor"));
                                        ScadaColor.uxItemBackGroundColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtItemBackgroundColor"));
                                        ScadaColor.uxPanelColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtPanelColor"));
                                        ScadaColor.uxItemColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtItemColor"));
                                        ScadaColor.uxGradientStartColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtItemColor"));
                                        ScadaColor.uxGradientEndColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtItemColor"));
                                        ScadaColor.uxPopupColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtPopupColor"));
                                        ScadaColor.uxPopupItemColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtPopupItemColor"));
                                        ScadaColor.uxTextColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtTextColor"));
                                        ScadaColor.uxLightColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtLightColor"));
                                        ScadaColor.uxHoverColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtHoverColor"));
                                        ScadaColor.uxTouchColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtTouchColor"));
                                        ScadaColor.uxOffColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtOffColor"));
                                        ScadaColor.uxLightColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtLightColor"));
                                        ScadaColor.uxGridThinColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtGridThinColor"));
                                        ScadaColor.uxGridFatColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtGridFatColor"));
                                        ScadaColor.uxTransparentButtonColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtTransparentButtonColor"));
                                    }
                                    Background = ScadaColor.uxBackGroundColor.ToMauiColor();
                                    
                                    break;
                                case "Interpolate":
                        
                                    break;

                            }
                            ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxParameters;
                            ScadaGlobals.Previouspage = -1;
                            uxtimer.Interval = 100;
                            //RefreshGui();                   
                            break;

                        case SKTouchAction.Pressed:
                            break;

                        case SKTouchAction.Moved:                         
                            break;

                        case SKTouchAction.Exited:                          
                            break;

                    }
                    args.Handled = true;
                   
                    /*
                    var Items = new List<ScadaClasses.Telegram> { };
                    Items = MyDataAccessLayer.getItems(ScadaClasses.Currentpage);
                    UpdateGui(Items);
                    */

                };
                AbsoluteLayout.SetLayoutBounds(tgRow, new Rect(x + w -80, y + 2, 50, 22));
                AbsoluteLayout.SetLayoutFlags(tgRow, AbsoluteLayoutFlags.None);
                SKCanvasPopupViews.Add(tgRow);
            }
            if (myRow.DataType == 1)
            {
                var btnDots = new ScadaButton();
                btnDots.GradientStartColor = ScadaColor.uxPopupItemColor;
                btnDots.GradientEndColor = ScadaColor.uxPopupItemColor;
                btnDots.CornerRadius = 15;
                btnDots.ItemID = 1;
                btnDots.EnableTouchEvents = true;
                btnDots.InputTransparent = false;
                btnDots.HeightRequest = 22;
                btnDots.WidthRequest = 22;
                btnDots.SvgBase64 = MyDataAccessLayer.LoadLibItem("Threedots", 1);
                btnDots.IndicatorType = 9;
                btnDots.Margin = 0.15F;
                btnDots.ButtonText = "";
                btnDots.Touch += (sender, args) =>
                {
                    switch (args.ActionType)
                    {
                        case SKTouchAction.Released:
                            
                            foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                            {
                                absoluteLayout.Remove(viewItem);
                            }

                            SKCanvasPopupViews.Clear();
                            
                            switch (myRow.col1text)
                            {
                                case "Connection string":
                                   
                                    foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                                    {
                                        absoluteLayout.Remove(viewItem);
                                    }
                                    SKCanvasPopupViews.Clear();
                                    edtInputText.Text = ConnectionString;
                                    ScadaGlobals.CurrentScadaPopup = -1;
                                    ScadaGlobals.CurrentType = ScadaClasses.pmComputer;
                                    ScadaGlobals.CurrentID = myRow.Id;
                                    SKCanvasPopupViews.Clear();
                                    ScadaEditParameterText();
                                    /*
                                    foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                                    {
                                        absoluteLayout.Add(viewItem);
                                    }
                                    edtInputText.Placeholder = "EditText";
                                    edtInputText.WidthRequest = w - 20;
                                    edtInputText.HeightRequest = h / 2;
                                    edtInputText.TextColor = ScadaColor.uxTextColor.ToMauiColor();
                                    edtInputText.BackgroundColor = ScadaColor.uxItemColor.ToMauiColor();
                                    edtInputText.PlaceholderColor = ScadaColor.uxPopupItemColor.ToMauiColor();
                                    edtInputText.FontAttributes = FontAttributes.Bold;
                                    edtInputText.IsVisible = true;
                                    AbsoluteLayout.SetLayoutBounds(edtInputText, new Rect(x + 10, y+30, w - 20, h / 2));
                                    AbsoluteLayout.SetLayoutFlags(edtInputText, AbsoluteLayoutFlags.None);
                                    edtInputText.TextChanged += OnEditorTextChanged;
                                    edtInputText.Completed += OnEditorCompleted;
                                    absoluteLayout.Add(edtInputText);
                                    */
                                    Content = absoluteLayout;

                                    break;
                                case "Items":
                                    foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                                    {
                                        absoluteLayout.Remove(viewItem);
                                    }
                                    SKCanvasPopupViews.Clear();
                                    edtInputText.Text = myRow.col2text;
                                    ScadaGlobals.CurrentScadaPopup = -1;
                                    ScadaGlobals.CurrentType = ScadaClasses.pmTagSetting;
                                    ScadaGlobals.CurrentID = myRow.Id;
                                    SKCanvasPopupViews.Clear();
                                    ScadaUpload();
                                    /*
                                    foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                                    {
                                        absoluteLayout.Add(viewItem);
                                    }
                                    */
                                   /*
                                    edtSvgName.Placeholder = "SvgName";
                                    edtSvgName.WidthRequest = 200;
                                    edtSvgName.HeightRequest = 30;
                                    edtSvgName.TextColor = ScadaColor.uxTextColor.ToMauiColor();
                                    edtSvgName.BackgroundColor = ScadaColor.uxPopupItemColor.ToMauiColor();
                                    edtSvgName.PlaceholderColor = ScadaColor.uxPopupItemColor.ToMauiColor();
                                    edtSvgName.FontAttributes = FontAttributes.None;
                                    edtSvgName.IsVisible = true;
                                    AbsoluteLayout.SetLayoutBounds(edtSvgName, new Rect(400, 200, edtSvgName.WidthRequest, edtSvgName.HeightRequest));
                                    AbsoluteLayout.SetLayoutFlags(edtSvgName, AbsoluteLayoutFlags.None);
                                    edtSvgName.TextChanged += OnEditorTextChanged;
                                    edtSvgName.Completed += OnEditorCompleted;
                                    absoluteLayout.Add(edtSvgName);

                                    edtSvgEditor.Placeholder = "Paste your SVG text";
                                    edtSvgEditor.WidthRequest = 400;
                                    edtSvgEditor.HeightRequest = 200;
                                    edtSvgEditor.TextColor = ScadaColor.uxTextColor.ToMauiColor();
                                    edtSvgEditor.BackgroundColor = ScadaColor.uxPopupItemColor.ToMauiColor();
                                    edtSvgEditor.PlaceholderColor = ScadaColor.uxItemColor.ToMauiColor();
                                    edtSvgEditor.FontAttributes = FontAttributes.None;
                                    edtSvgEditor.IsVisible = true;
                                    AbsoluteLayout.SetLayoutBounds(edtSvgEditor, new Rect(400, 300, edtSvgEditor.WidthRequest, edtSvgEditor.HeightRequest));
                                    AbsoluteLayout.SetLayoutFlags(edtSvgEditor, AbsoluteLayoutFlags.None);
                                    edtSvgEditor.TextChanged += OnEditorTextChanged;
                                    edtSvgEditor.Completed += OnEditorCompleted;
                                    absoluteLayout.Add(edtSvgEditor);
                                    */
                                    Content = absoluteLayout;
                                    break;
                                case "Signals":
                                      ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxTagsGrid;                                 
                                    break;
                                case "Pages":
                                      ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxPagesMenu;                                
                                    break;
                                case "Protocols":
                                      ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxProtocols;
                                      ScadaGlobals.PreviousScadaPopup = ScadaClasses.uxParameters;                     
                                    break;
                            }
                            /*
                                foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                                {
                                    absoluteLayout.Add(viewItem);
                                }
                                Content = absoluteLayout;
                                */
                            //RefreshGui();
                            uxtimer.Interval = 50;
                            break;

                        case SKTouchAction.Pressed:
                            break;

                        case SKTouchAction.Moved:
                            btnDots.GradientStartColor = ScadaColor.uxHoverColor;
                            btnDots.GradientEndColor = ScadaColor.uxHoverColor;
                            btnDots.IndicatorColor = ScadaColor.uxHoverColor;
                            break;

                        case SKTouchAction.Exited:
                            btnDots.GradientStartColor = ScadaColor.uxPopupItemColor;
                            btnDots.GradientEndColor = ScadaColor.uxPopupItemColor;
                            btnDots.IndicatorColor = ScadaColor.uxPopupItemColor;
                            break;
                    }
                    args.Handled = true;
                };
                AbsoluteLayout.SetLayoutBounds(btnDots, new Rect(x + w - 80 + 15, y + 2, 50, 22));
                AbsoluteLayout.SetLayoutFlags(btnDots, AbsoluteLayoutFlags.None);
                SKCanvasPopupViews.Add(btnDots);
            }

              
            y = y + 26;
            r++;
        }
    }



    private void ScadaAlarmPopup(int ID, int TagID, string sAlarmtext)
    {
        double panelWith = 0.24;
        double panelHeight = 0.24;

        double btnWidth = 0.3;
        double btnHeight = 0.2;
        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(ConnectionString);
        SKCanvasPopupViews.Clear();
        var gp = new ScadaButton();
        gp.AnchorX = 0;
        gp.AnchorY = 0;
        gp.CornerRadius = 10;
        gp.BarBackgroundColor = ScadaColor.uxPanelColor;
        gp.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        gp.GradientStartColor = ScadaColor.uxPopupColor;
        gp.GradientEndColor = ScadaColor.uxPopupColor;
        gp.IndicatorColor = ScadaColor.uxPopupColor;

        double x = (Width / 2) - ((panelWith * Width)) / 2;
        double y = (Height / 2) - ((panelHeight * Height)) / 2;
        double w = (panelWith * Width);
        double h = (panelHeight * Height);
        double btnw = w * btnWidth;
        double btnh = h * btnHeight;

        gp.WidthRequest = w;
        gp.HeightRequest = h;
        gp.AlternativeTextColor = ScadaColor.uxTextColor;
        gp.TextColor = ScadaColor.uxTextColor;
        gp.IsEnabled = true;
        gp.IsVisible = true;
        AbsoluteLayout.SetLayoutBounds(gp, new Rect(x, y, w, h));
        AbsoluteLayout.SetLayoutFlags(gp, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(gp);

        var t1 = new ScadaText();
        t1.CornerRadius = 1;
        t1.BarBackgroundColor = ScadaColor.uxPanelColor;
        t1.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        t1.GradientStartColor = ScadaColor.uxPopupColor;
        t1.GradientEndColor = ScadaColor.uxPopupColor;
        t1.TextColor = ScadaColor.uxTextColor;
        t1.TheText = sAlarmtext;
        t1.WidthRequest = w;
        t1.HeightRequest = 30;
        t1.FontSize = 16.5F;

        AbsoluteLayout.SetLayoutBounds(t1, new Rect(x, y, w, h));
        AbsoluteLayout.SetLayoutFlags(t1, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(t1);

        var a1 = new ScadaButton();
        a1.CornerRadius = (float)(btnh / 2F); ;
        a1.IndicatorType = 1;
        a1.BarBackgroundColor = ScadaColor.uxPanelColor;
        a1.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        a1.GradientStartColor = ScadaColor.uxPopupItemColor;
        a1.GradientEndColor = ScadaColor.uxPopupItemColor;
        a1.IndicatorColor = ScadaColor.uxPanelColor;
        a1.TextColor = ScadaColor.uxTextColor;
        a1.ButtonText = "Confirm";
        a1.WidthRequest = btnw;
        a1.HeightRequest = btnh;
        a1.FontSize = 16.5F;
        a1.IsEnabled = true;
        a1.IsVisible = true;
        a1.EnableTouchEvents = true;
        a1.InputTransparent = false;
        a1.Touch += (sender, args) =>
        {
            var pt = args.Location;
            switch (args.ActionType)
            {
                case SKTouchAction.Pressed:
                    break;

                case SKTouchAction.Released:
                    ScadaClasses.Telegram oTelegram = new ScadaClasses.Telegram()
                    {
                        MessageType = 4,
                        TagID = ID,
                        Page = ScadaGlobals.Currentpage,
                        Action = " "
                    };
                    MyDataAccessLayer.confirmAlarm(ID);
                    //ScadaGlobals.Refresh = true;
                    ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxAlarmGrid;
                    //SendMessage(JsonSerializer.Serialize(oTelegram));
                    break;

                case SKTouchAction.Moved:
                    a1.GradientStartColor = ScadaColor.uxHoverColor;
                    a1.GradientEndColor = ScadaColor.uxHoverColor;
                    break;

                case SKTouchAction.Exited:
                    a1.GradientStartColor = ScadaColor.uxPopupItemColor;
                    a1.GradientEndColor = ScadaColor.uxPopupItemColor;
                    break;
            }
            args.Handled = true;
        };

        AbsoluteLayout.SetLayoutBounds(a1, new Rect(x + (w / 2) - btnw - (btnw / 4), y + h - (2 * btnh), btnw, btnh));
        AbsoluteLayout.SetLayoutFlags(a1, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(a1);

        var a2 = new ScadaButton();
        a2.CornerRadius = (float)(btnh / 2F);
        a2.IndicatorType = 1;
        a2.BarBackgroundColor = ScadaColor.uxPanelColor;
        a2.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
        a2.GradientStartColor = ScadaColor.uxPopupItemColor;
        a2.GradientEndColor = ScadaColor.uxPopupItemColor;
        a2.IndicatorColor = ScadaColor.uxPanelColor;
        a2.TextColor = ScadaColor.uxTextColor;
        a2.ButtonText = "Disable";
        a2.WidthRequest = btnw;
        a2.HeightRequest = btnh;
        a2.FontSize = 18.5F;
        a2.IsEnabled = true;
        a2.IsVisible = true;
        a2.EnableTouchEvents = true;
        a2.InputTransparent = false;

        a2.Touch += (sender, args) =>
        {
            var pt = args.Location;
            switch (args.ActionType)
            {
                case SKTouchAction.Pressed:
                    break;

                case SKTouchAction.Released:
                    ScadaClasses.Telegram oTelegram = new ScadaClasses.Telegram()
                    {
                        MessageType = 4,
                        TagID = ID,
                        Page = ScadaGlobals.Currentpage,
                        Action = " "
                    };
                    MyDataAccessLayer.disableAlarm(TagID);
                    MyDataAccessLayer.confirmAlarm(TagID);
                    MyDataAccessLayer.deleteAlarm(oTelegram);
                    ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxAlarmGrid;
                    //SendMessage(JsonSerializer.Serialize(oTelegram));
                    //ScadaClasses.Refresh = true;
                    break;

                case SKTouchAction.Moved:
                    a2.GradientStartColor = ScadaColor.uxHoverColor;
                    a2.GradientEndColor = ScadaColor.uxHoverColor;
                    break;

                case SKTouchAction.Exited:
                    a2.GradientStartColor = ScadaColor.uxPopupItemColor;
                    a2.GradientEndColor = ScadaColor.uxPopupItemColor;
                    break;
            }
            args.Handled = true;
        };
        AbsoluteLayout.SetLayoutBounds(a2, new Rect(x + (w / 2) + (btnw / 4), y + h - (2 * btnh), btnw, btnh));
        AbsoluteLayout.SetLayoutFlags(a2, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(a2);


        var btnPages = new ScadaButton();
        btnPages.GradientStartColor = ScadaColor.uxItemColor;
        btnPages.GradientEndColor = ScadaColor.uxItemColor;
        btnPages.CornerRadius = 15;
        btnPages.ItemID = 1;
        btnPages.EnableTouchEvents = true;
        btnPages.InputTransparent = false;
        btnPages.HeightRequest = 25;
        btnPages.WidthRequest = 25;
        btnPages.SvgBase64 = MyDataAccessLayer.LoadLibItem("CloseCross", 1);
        btnPages.IndicatorType = 3;
        btnPages.Margin = 0.15F;
        btnPages.ButtonText = "";
        btnPages.Touch += (sender, args) =>
        {
            switch (args.ActionType)
            {
                case SKTouchAction.Released:
                    ScadaGlobals.Previouspage = -1;
                    ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxAlarmGrid;
                    //ScadaClasses.Refresh = true;
                    break;

                case SKTouchAction.Pressed:
                    break;

                case SKTouchAction.Moved:
                    btnPages.GradientStartColor = ScadaColor.uxHoverColor;
                    btnPages.GradientEndColor = ScadaColor.uxHoverColor;
                    btnPages.IndicatorColor = ScadaColor.uxHoverColor;
                    break;

                case SKTouchAction.Exited:
                    btnPages.GradientStartColor = ScadaColor.uxItemColor;
                    btnPages.GradientEndColor = ScadaColor.uxItemColor;
                    btnPages.IndicatorColor = ScadaColor.uxItemColor;
                    break;
            }
            args.Handled = true;
        };
        AbsoluteLayout.SetLayoutBounds(btnPages, new Rect(x + gp.WidthRequest - btnPages.WidthRequest - (btnPages.WidthRequest / 4), y + btnPages.HeightRequest / 4, 25, 25));
        AbsoluteLayout.SetLayoutFlags(btnPages, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(btnPages);
    }

    private double Nearest(double value, double n)
    {
        // int v = ((int)(value));
        return Math.Round((value / n)) * n;
    }
    /*
    public static int GetIso8601WeekOfYear(DateTime time)
    {
        // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
        // be the same week# as whatever Thursday, Friday or Saturday are,
        // and we always get those right
        DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
        if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
        {
            time = time.AddDays(3);
        }

        // Return the week of our adjusted day
        return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }
    */
    public  DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
    {
        DateTime jan1 = new DateTime(year, 1, 1);
        int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

        // Use first Thursday in January to get first week of the year as
        // it will never be in Week 52/53
        DateTime firstThursday = jan1.AddDays(daysOffset);
        var cal = CultureInfo.CurrentCulture.Calendar;
        int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

        var weekNum = weekOfYear;
        // As we're adding days to a date in Week 1,
        // we need to subtract 1 in order to get the right date for week #1
        if (firstWeek == 1)
        {
            weekNum -= 1;
        }

        // Using the first Thursday as starting week ensures that we are starting in the right year
        // then we add number of weeks multiplied with days
        var result = firstThursday.AddDays(weekNum * 7);

        // Subtract 3 days from Thursday to get Monday, which is the first weekday in ISO8601
        return result.AddDays(-3);
    }


    public int ChartSetTimeScaleStep(int ItemID, int Direction)
    {
        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(ConnectionString);
        var myChartSettings = MyDataAccessLayer.GetChartSettings(ItemID);
        switch (myChartSettings.iSpan)
        {
            case 1:
                {
                    // No step
                }
                break;

            case 2:
                {   // 1 Hour step
                    DateTime myTime = new DateTime(myChartSettings.iYear, myChartSettings.iMonth, myChartSettings.iDay, myChartSettings.iHour, 0, 0);
                    DateTime OneHour = myTime.AddHours(Direction);
                    myChartSettings.iYear = OneHour.Year;
                    myChartSettings.iMonth = OneHour.Month;
                    myChartSettings.iDay = OneHour.Day;
                    myChartSettings.iHour = OneHour.Hour;
                    myChartSettings.iMinute = 0;
                }
                break;

            case 3:
                {   // 1 Day step (24Hour)
                    DateTime myTime = new DateTime(myChartSettings.iYear, myChartSettings.iMonth, myChartSettings.iDay, myChartSettings.iHour, 0, 0);
                    DateTime OneDay = myTime.AddDays(Direction);
                    myChartSettings.iYear = OneDay.Year;
                    myChartSettings.iMonth = OneDay.Month;
                    myChartSettings.iDay = OneDay.Day;
                    myChartSettings.iHour = OneDay.Hour;
                    myChartSettings.iMinute = 0;
                }
                break;

            case 4:
                {
                    DateTime myTime = new DateTime(myChartSettings.iYear, myChartSettings.iMonth, myChartSettings.iDay, myChartSettings.iHour, 0, 0);
                    myChartSettings.iHour = 0;
                    myChartSettings.iMinute = 0;

                    if (Direction > 0)
                    {
                        if (myChartSettings.iWeek < GetIso8601WeekOfYear(new DateTime(myChartSettings.iYear, 12, 30, 0, 0, 0)))
                        {
                            myChartSettings.iWeek = myChartSettings.iWeek + 1;
                        }
                        else
                        {
                            myChartSettings.iWeek = 1;
                            myChartSettings.iYear = myChartSettings.iYear + 1;
                        }
                    }

                    if (Direction < 0)
                    {
                        if (myChartSettings.iWeek > 1)
                        {
                            myChartSettings.iWeek = myChartSettings.iWeek - 1;
                        }
                        else
                        {
                            myChartSettings.iYear = myChartSettings.iYear - 1;
                            myChartSettings.iWeek = GetIso8601WeekOfYear(new DateTime(myChartSettings.iYear, 12, 26, 0, 0, 0));
                           
                        }
                    }
              
                    //FirstDateOfWeekISO8601(myChartSettings.iYear ,myChartSettings.iWeek);

                    }
                break;

            case 5:
                {
                    DateTime myTime = new DateTime(myChartSettings.iYear, myChartSettings.iMonth, myChartSettings.iDay, myChartSettings.iHour, 0, 0);
                    DateTime OneMonth = myTime.AddMonths(Direction);
                    myChartSettings.iYear = OneMonth.Year;
                    myChartSettings.iMonth = OneMonth.Month;
                    myChartSettings.iDay = 1;
                    myChartSettings.iHour = 0;
                    myChartSettings.iMinute = 0;
                }
                break;
            case 6:
                {
                    DateTime myTime = new DateTime(myChartSettings.iYear, myChartSettings.iMonth, myChartSettings.iDay, myChartSettings.iHour, 0, 0);
                    DateTime OneYear = myTime.AddYears(Direction);
                    myChartSettings.iYear = OneYear.Year;
                    myChartSettings.iMonth = 1;
                    myChartSettings.iDay = 1;
                    myChartSettings.iHour = 0;
                    myChartSettings.iMinute = 0;
                }
                break;
        }
        MyDataAccessLayer.SetChartSettings(myChartSettings, ItemID);
        return (0);
    }


    public int GetIso8601WeekOfYear(DateTime time)
    {
        DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
        if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
        {
            time = time.AddDays(3);
        }
        return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }


    public SKCanvasView AttachDesignEvents(SKCanvasView sn, ScadaClasses.Telegram ScadaItem)
    {
       
        sn.EnableTouchEvents = true;
        sn.Touch += (sender, args) =>
        {
            var pt = args.Location;
            switch (args.ActionType)
            {
                case SKTouchAction.Pressed:
                    if (Moving == false)
                    {
                        Xpos = sn.X;
                        Ypos = sn.Y;
                        Xwidth = sn.Width;
                        Yheight = sn.Height;
                        LastXpos = pt.X;
                        LastYpos = pt.Y;
                        Moving = true;
                    }
                    break;

                case SKTouchAction.Released:
                    Moving = false;
                    //Editing = true;
                    ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxConfigMenu;
                    Xpos = sn.X;
                    Ypos = sn.Y;
                    ScadaClasses.Telegram oTelegram = new ScadaClasses.Telegram()
                    {
                        MessageType = ScadaClasses.CmdupdateSV,
                        Page = ScadaGlobals.Currentpage,
                        Top = ((sn.Y / Height) * 100),
                        Left = ((sn.X / Width) * 100),
                        ItemType = ScadaItem.ItemType,
                        ItemID = ScadaItem.ItemID,
                        TagID = ScadaItem.TagID,
                        SV = "",
                        TagName = ScadaItem.TagName,
                        Action = ScadaItem.Action,
                    };
                    ScadaGlobals.CurrentScadaPopup = -1;
                    ScadaGlobals.PreviousScadaPopup = -1;
                    ScadaGlobals.CurrentItem = ScadaItem.ItemID;
                    ScadaGlobals.CurrentType = ScadaItem.ItemType;
                    DataAccessLayer MyDataAccessLayer = new DataAccessLayer(ConnectionString); 
                    MyDataAccessLayer.moveItem(oTelegram);


                    foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                    {
                        absoluteLayout.Remove(viewItem);
                    }
                  
                    SKCanvasPopupViews.Clear();
                    ScadaConfigMenu(ScadaItem.ItemID,ScadaItem.ItemType);
                    foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                    {
                        absoluteLayout.Add(viewItem);
                    }
                    Content = absoluteLayout;

                    //ScadaGlobals.Previouspage = -1;
                    //ScadaClasses.Refresh = true;
                    break;

                case SKTouchAction.Moved:
                    {
                        if (Moving == true)
                        {
                            double MovingX = Math.Round(pt.X - LastXpos);
                            double MovingY = Math.Round(pt.Y - LastYpos);

                            if (MovingX > 0)
                            {
                                Xpos = Xpos + 1;
                            }

                            if (MovingX < 0)
                            {
                                Xpos = Xpos - 1;
                            }

                            if (MovingY > 0)
                            {
                                Ypos = Ypos + 1;
                            }
                            if (MovingY < 0)
                            {
                                Ypos = Ypos - 1;
                            }

                            if (Math.Abs(OldXpos - Xpos) >= 10)
                            {
                                Xpos = Nearest(Xpos, 10);
                                OldXpos = Xpos;
                                AbsoluteLayout.SetLayoutBounds(sn, new Rect(Xpos, Ypos, sn.Width, sn.Height));
                            }

                            if (Math.Abs(OldYpos - Ypos) >= 10)
                            {
                                Ypos = Nearest(Ypos, 10);
                                OldYpos = Ypos;
                                AbsoluteLayout.SetLayoutBounds(sn, new Rect(Xpos, Ypos, sn.Width, sn.Height));
                            }
                            LastXpos = pt.X;
                            LastYpos = pt.Y;
                        }
                    }
                    break;
            }
            args.Handled = true;
        };
        return (sn);
    }


    public void CreateCloseButton(double x, double y, double w, double h, int callerPopup )
    {
        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(ConnectionString);
        var myCloseButton = new ScadaButton
        {
            WidthRequest = 25,
            HeightRequest = 25,
            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
            BarBackgroundColor = ScadaColor.uxBackGroundColor,
            IndicatorType = 9,
            GradientStartColor = ScadaColor.uxItemColor,
            GradientEndColor = ScadaColor.uxItemColor,
            CornerRadius = 10,
            TextColor = ScadaColor.uxTextColor,
            FontSize = 21.5F,
            Margin = 0.15F,
            SvgBase64 = MyDataAccessLayer.LoadLibItem("CloseCross", 1),
            ButtonText = ""
        };
        myCloseButton.IsVisible = true;
        myCloseButton.InputTransparent = false;
        myCloseButton.EnableTouchEvents = true;
        myCloseButton.Touch += (sender, args) =>
                        {
                            var pt = args.Location;
                            switch (args.ActionType)
                            {
                                case SKTouchAction.Released:
                                    /*
                                     ScadaClasses.CurrentScadaPopup = callerPopup;
                                     ScadaClasses.Previouspage = -1;
                                     ScadaClasses.Refresh = true;
                                     SKCanvasPopupViews.Clear();
                                     SKCanvasViews.Clear();

                                     if (ScadaClasses.CurrentScadaPopup == -1)
                                     {
                                         Editing = false;
                                     }
                                     */

                                    /*
                                    foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                                     {
                                        absoluteLayout.Remove(viewItem);
                                    }
                                    SKCanvasPopupViews.Clear();
                                    switch (ScadaGlobals.PreviousScadaPopup)
                                    {
                                        case ScadaClasses.uxParameters:
                                            ScadaParameters();
                                            break;
                                        case ScadaClasses.uxTagsGrid:

                                            break;
                                        case ScadaClasses.uxProtocols:
                                            ScadaProtocols();
                                            break;
                                    }
                                    foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                                    {
                                        absoluteLayout.Add(viewItem);
                                    }
                                    Content = absoluteLayout;
                                    */
                                    foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                                    {
                                        absoluteLayout.Remove(viewItem);
                                    }
                                    absoluteLayout.Remove(edtInputText);
                                    absoluteLayout.Remove(edtSvgName);
                                    absoluteLayout.Remove(edtSvgEditor);

                                    SKCanvasPopupViews.Clear();

                                    ScadaGlobals.CurrentScadaPopup = callerPopup;
                                    uxtimer.Interval = 50;
                                   // Thread.Sleep(500);
                                    //RefreshGui();
                                 
                                    break;

                                case SKTouchAction.Moved:
                                    myCloseButton.GradientStartColor = ScadaColor.uxHoverColor;
                                    myCloseButton.GradientEndColor = ScadaColor.uxHoverColor;
                                    break;

                                case SKTouchAction.Exited:
                                    myCloseButton.GradientStartColor = ScadaColor.uxItemColor;
                                    myCloseButton.GradientEndColor = ScadaColor.uxItemColor;
                                    break;

                            }
                            args.Handled = true;
                        };

        AbsoluteLayout.SetLayoutBounds(myCloseButton, new Rect(x + w - 35, y + 3, 30, 30));
        AbsoluteLayout.SetLayoutFlags(myCloseButton, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(myCloseButton);
    }


    public void CreateAddTagButton(double x, double y, double w, double h)
    {
        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(ConnectionString);

        var myCreateTagButton = new ScadaButton
        {
            WidthRequest = 25,
            HeightRequest = 25,
            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
            BarBackgroundColor = ScadaColor.uxBackGroundColor,
            IndicatorType = 9,
            GradientStartColor = ScadaColor.uxItemColor,
            GradientEndColor = ScadaColor.uxItemColor,
            CornerRadius = 10,
            TextColor = ScadaColor.uxTextColor,
            FontSize = 21.5F,
            Margin = 0.15F,
            SvgBase64 = MyDataAccessLayer.LoadLibItem("GreenPlus", 1),
            ButtonText = ""
        };
        myCreateTagButton.IsVisible = true;
        myCreateTagButton.InputTransparent = false;
        myCreateTagButton.EnableTouchEvents = true;
        myCreateTagButton.Touch += (sender, args) =>
        {
            var pt = args.Location;
            switch (args.ActionType)
            {
                case SKTouchAction.Released:
                    MyDataAccessLayer.DeleteTagByName("%your%");
                    var newTag = new ScadaClasses.Tag();
                    newTag.Name = "your Tag name";
                    newTag.Description = "your Description";
                    newTag.Unit = "your Unit";
                    newTag.HL = 100.0;
                    newTag.LL = -100.0;
                    newTag.Color = "rgba(100,100,200,100)";
                    newTag.Driver = 1;
                    newTag.AlarmEnable = 0;
                    ScadaGlobals.CurrentTag = MyDataAccessLayer.AddTag(newTag);
                    ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxTagSettings;
                    ScadaGlobals.Previouspage = -1;
                    //ScadaGlobals.Refresh = true;
                    break;

                case SKTouchAction.Moved:
                    myCreateTagButton.GradientStartColor = ScadaColor.uxHoverColor;
                    myCreateTagButton.GradientEndColor = ScadaColor.uxHoverColor;
                    break;

                case SKTouchAction.Exited:
                    myCreateTagButton.GradientStartColor = ScadaColor.uxItemColor;
                    myCreateTagButton.GradientEndColor = ScadaColor.uxItemColor;
                    break;
            }
            args.Handled = true;
        };

        AbsoluteLayout.SetLayoutBounds(myCreateTagButton, new Rect(x + 5, y + 5, 30, 30));
        AbsoluteLayout.SetLayoutFlags(myCreateTagButton, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(myCreateTagButton);
    }


    public void CreateRemoveTagButton(int ItemID, int TagID, double x, double y, double w, double h)
    {
        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(ConnectionString);
        var myRemoveButton = new ScadaButton
        {
            WidthRequest = 25,
            HeightRequest = 25,
            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
            BarBackgroundColor = ScadaColor.uxBackGroundColor,
            IndicatorType = 9,
            GradientStartColor = ScadaColor.uxItemColor,
            GradientEndColor = ScadaColor.uxItemColor,
            CornerRadius = 10,
            TextColor = ScadaColor.uxTextColor,
            FontSize = 21.5F,
            Margin = 0.15F,
            SvgBase64 = MyDataAccessLayer.LoadLibItem("Trashcan", 1),
            ButtonText = ""
        };
        myRemoveButton.IsVisible = true;
        myRemoveButton.InputTransparent = false;
        myRemoveButton.EnableTouchEvents = true;
        myRemoveButton.Touch += (sender, args) =>
        {
            var pt = args.Location;
            switch (args.ActionType)
            {
                case SKTouchAction.Released:
                    MyDataAccessLayer.DeleteTagFromUxItem(ItemID, TagID);
                    ScadaGlobals.CurrentScadaPopup = -1;
                    ScadaGlobals.Previouspage = -1;
                    //ScadaClasses.Refresh = true;
                    break;

                case SKTouchAction.Moved:
                    myRemoveButton.GradientStartColor = ScadaColor.uxHoverColor;
                    myRemoveButton.GradientEndColor = ScadaColor.uxHoverColor;
                    break;

                case SKTouchAction.Exited:
                    myRemoveButton.GradientStartColor = ScadaColor.uxItemColor;
                    myRemoveButton.GradientEndColor = ScadaColor.uxItemColor;
                    break;

            }
            args.Handled = true;
        };

        AbsoluteLayout.SetLayoutBounds(myRemoveButton, new Rect(x + w - 35, y + h - 35, 30, 30));
        AbsoluteLayout.SetLayoutFlags(myRemoveButton, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(myRemoveButton);
    }

    public void CreateFwdButton(double x, double y, double w, double h)
    {
        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(ConnectionString);
        var myFwdButton = new ScadaButton
        {
            WidthRequest = 25,
            HeightRequest = 25,
            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
            BarBackgroundColor = ScadaColor.uxBackGroundColor,
            IndicatorType = 9,
            GradientStartColor = ScadaColor.uxItemColor,
            GradientEndColor = ScadaColor.uxItemColor,
            CornerRadius = 10,
            TextColor = ScadaColor.uxTextColor,
            FontSize = 21.5F,
            Margin = 0.15F,
            SvgBase64 = MyDataAccessLayer.LoadLibItem("Arrowright", 1),
            ButtonText = ""
        };
        myFwdButton.IsVisible = true;
        myFwdButton.InputTransparent = false;
        myFwdButton.EnableTouchEvents = true;
        myFwdButton.Touch += (sender, args) =>
        {
            var pt = args.Location;
            switch (args.ActionType)
            {
                case SKTouchAction.Released:
                    ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxTagsGrid;
                    ScadaGlobals.PreviousScadaPopup = -1;
                    iMenuOffsetRows = iMenuOffsetRows + 5;
                    foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                    {
                        absoluteLayout.Remove(viewItem);
                    }
                    SKCanvasPopupViews.Clear();
                    uxtimer.Interval = 50;
                    break;

                case SKTouchAction.Moved:
                    myFwdButton.GradientStartColor = ScadaColor.uxHoverColor;
                    myFwdButton.GradientEndColor = ScadaColor.uxHoverColor;
                    break;

                case SKTouchAction.Exited:
                    myFwdButton.GradientStartColor = ScadaColor.uxItemColor;
                    myFwdButton.GradientEndColor = ScadaColor.uxItemColor;
                    break;

            }
            args.Handled = true;
        };
        AbsoluteLayout.SetLayoutBounds(myFwdButton, new Rect(x + w - 35, y + h - 35, 30, 30));
        AbsoluteLayout.SetLayoutFlags(myFwdButton, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(myFwdButton);
    }

    public void CreateRwdButton(double x, double y, double w, double h)
    {
        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(ConnectionString);
        var myRwdButton = new ScadaButton
        {
            WidthRequest = 25,
            HeightRequest = 25,
            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
            BarBackgroundColor = ScadaColor.uxBackGroundColor,
            IndicatorType = 9,
            GradientStartColor = ScadaColor.uxItemColor,
            GradientEndColor = ScadaColor.uxItemColor,
            CornerRadius = 10,
            TextColor = ScadaColor.uxTextColor,
            FontSize = 21.5F,
            Margin = 0.15F,
            SvgBase64 = MyDataAccessLayer.LoadLibItem("Arrowleft", 1),
            ButtonText = ""
        };
        myRwdButton.IsVisible = true;
        myRwdButton.InputTransparent = false;
        myRwdButton.EnableTouchEvents = true;
        myRwdButton.Touch += (sender, args) =>
        {
            var pt = args.Location;
            switch (args.ActionType)
            {
                case SKTouchAction.Released:
                    ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxTagsGrid;
                    ScadaGlobals.PreviousScadaPopup = -1;  
                    iMenuOffsetRows = iMenuOffsetRows - 5;
                    foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                    {
                        absoluteLayout.Remove(viewItem);
                    }
                    SKCanvasPopupViews.Clear();
                    uxtimer.Interval = 50;
                    break;

                case SKTouchAction.Moved:
                    myRwdButton.GradientStartColor = ScadaColor.uxHoverColor;
                    myRwdButton.GradientEndColor = ScadaColor.uxHoverColor;
                    break;

                case SKTouchAction.Exited:
                    myRwdButton.GradientStartColor = ScadaColor.uxItemColor;
                    myRwdButton.GradientEndColor = ScadaColor.uxItemColor;
                    break;
            }
            args.Handled = true;
        };
        AbsoluteLayout.SetLayoutBounds(myRwdButton, new Rect(x + 5, y + h - 35, 30, 30));
        AbsoluteLayout.SetLayoutFlags(myRwdButton, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(myRwdButton);
    }


    void UpdateGui(List<ScadaClasses.Telegram> ScadaItems)
    {
        Window.MaximumWidth = 4000;
        Window.MaximumHeight = 2000;
        Window.IsMaximizable = true;
        
        double x = 0, y = 0, w = 0, h = 0;   
        double wScale = Width / 100;
        double hScale = Height / 100;
  
        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(ConnectionString);

        if ((ScadaGlobals.Previouspage != ScadaGlobals.Currentpage) && (ScadaItems.Count > 0))
        {
            if (MyDataAccessLayer.ReadParameterByName("Design") == "1")
            {
                Designing = true;
            }
            else
            {
                Designing = false;
            }

            if (MyDataAccessLayer.ReadParameterByName("Dark") == "1")
            {
                ScadaColor.uxBackGroundColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkBackgroundColor"));
                ScadaColor.uxItemBackGroundColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkItemBackgroundColor"));
                ScadaColor.uxPanelColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkPanelColor"));
                ScadaColor.uxItemColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkItemColor"));
                ScadaColor.uxGradientStartColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkItemColor"));
                ScadaColor.uxGradientEndColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkItemColor"));
                ScadaColor.uxPopupColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkPopupColor"));
                ScadaColor.uxPopupItemColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkPopupItemColor"));
                ScadaColor.uxTextColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkTextColor"));
                ScadaColor.uxLightColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkLightColor"));
                ScadaColor.uxHoverColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkHoverColor"));
                ScadaColor.uxTouchColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkTouchColor"));
                ScadaColor.uxOffColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkOffColor"));
                ScadaColor.uxLightColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkLightColor"));
                ScadaColor.uxGridThinColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkGridThinColor"));
                ScadaColor.uxGridFatColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkGridFatColor"));
                ScadaColor.uxTransparentButtonColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemDkTransparentButtonColor"));
            }
            else
            {
                ScadaColor.uxBackGroundColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtBackgroundColor"));
                ScadaColor.uxItemBackGroundColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtItemBackgroundColor"));
                ScadaColor.uxPanelColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtPanelColor"));
                ScadaColor.uxItemColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtItemColor"));
                ScadaColor.uxGradientStartColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtItemColor"));
                ScadaColor.uxGradientEndColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtItemColor"));
                ScadaColor.uxPopupColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtPopupColor"));
                ScadaColor.uxPopupItemColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtPopupItemColor"));
                ScadaColor.uxTextColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtTextColor"));
                ScadaColor.uxLightColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtLightColor"));
                ScadaColor.uxHoverColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtHoverColor"));
                ScadaColor.uxTouchColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtTouchColor"));
                ScadaColor.uxOffColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtOffColor"));
                ScadaColor.uxLightColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtLightColor"));
                ScadaColor.uxGridThinColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtGridThinColor"));
                ScadaColor.uxGridFatColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtGridFatColor"));
                ScadaColor.uxTransparentButtonColor = RGBStringToColor(MyDataAccessLayer.ReadParameterByName("SystemLtTransparentButtonColor"));
            }
            Background = ScadaColor.uxBackGroundColor.ToMauiColor();
            //AppShell.SetBackgroundColor(this, Color.FromRgb(255, 0, 0));
            //AppShell.SetTitleColor(this, Color.FromRgb(255, 0, 0));
            SKCanvasPopupViews.Clear();
            SKCanvasViews.Clear();
            //ContentViews.Clear();

            if (Designing == true)
            {
                ScadaGrid grdSnap = new ScadaGrid();
                grdSnap.ItemID = 0;
                grdSnap.AnchorX = 0;
                grdSnap.AnchorY = 0;
                //grdSnap.CornerRadius = 0;
                grdSnap.BarBackgroundColor = ScadaColor.uxPanelColor;
                grdSnap.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
                grdSnap.GridThinColor = ScadaColor.uxGridThinColor;
                grdSnap.GridFatColor = ScadaColor.uxGridFatColor;
                grdSnap.WidthRequest = Window.Width;
                grdSnap.HeightRequest = Window.Height;
                //grdSnap.AlternativeTextColor = ScadaColor.uxPanelColor;
                //grdSnap.TextColor = ScadaColor.uxTextColor;
                grdSnap.IsEnabled = true;
                grdSnap.IsVisible = true; 
                AbsoluteLayout.SetLayoutBounds(grdSnap, new Rect(0, 0, Window.Width, Window.Height));            
                AbsoluteLayout.SetLayoutFlags(grdSnap, AbsoluteLayoutFlags.None);
                //AbsoluteLayout.SetLayoutBounds(grdSnap, new Rect(0, 0, Width, Height));
                //AbsoluteLayout.SetLayoutFlags(grdSnap, AbsoluteLayoutFlags.None);
                SKCanvasViews.Add(grdSnap);
            }

            foreach (var ScadaItem in ScadaItems)
                switch (ScadaItem.ItemType)
                {
                    case ScadaClasses.uxPanel:
                        ScadaPanel SKPanel = new ScadaPanel();
                        SKPanel.Init(wScale, hScale, SKPanel, ScadaItem, ScadaColor);                   
                        if (Designing == true)
                        {
                            SKPanel = (ScadaPanel)AttachDesignEvents(SKPanel, ScadaItem);
                        }
                        //SKPanel.GradientStartColor = ScadaColor.uxPanelColor;
                        //SKPanel.GradientEndColor = ScadaColor.uxPanelColor;
                   
                        SKPanel.EnableTouchEvents = false;
                        AbsoluteLayout.SetLayoutBounds(SKPanel, new Rect(wScale * ScadaItem.Left, hScale * ScadaItem.Top, wScale * ScadaItem.Width, hScale * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(SKPanel, AbsoluteLayoutFlags.None);
                        SKCanvasViews.Add(SKPanel);
                        break;

                    case ScadaClasses.uxNumeric:
                        var SKNumeric = new ScadaNumeric();
                        SKNumeric.Init(wScale, hScale, SKNumeric, ScadaItem, ScadaColor);
                        if (Designing == true)
                        {
                            SKNumeric = (ScadaNumeric)AttachDesignEvents(SKNumeric, ScadaItem);
                        }
                        AbsoluteLayout.SetLayoutBounds(SKNumeric, new Rect(wScale * ScadaItem.Left,hScale * ScadaItem.Top,wScale * ScadaItem.Width,hScale * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(SKNumeric, AbsoluteLayoutFlags.None);
                        SKCanvasViews.Add(SKNumeric);
                        break;
                    case ScadaClasses.uxCircularProgress:
                        var SKCircularProgress = new CircularProgress();
                        SKCircularProgress.Init(wScale, hScale, SKCircularProgress, ScadaItem, ScadaColor);
                        SKCircularProgress.Touch += (sender, args) =>
                        {
                            switch (args.ActionType)
                            {
                                case SKTouchAction.Released:
                                    if (ScadaItem.Action == "TOGGLE")
                                    {
                                        if (SKCircularProgress.SV.Value > 0.5)
                                        {
                                            SKCircularProgress.SV.Value = 0;
                                        }
                                        else
                                        {
                                            SKCircularProgress.SV.Value = 1;
                                        }
                                        MyDataAccessLayer.UpdateTagValue(SKCircularProgress.SV.TagID, SKCircularProgress.SV.Value, 0);
                                        MyDataAccessLayer.StoreTagValue(SKCircularProgress.SV.TagID, SKCircularProgress.SV.Value);

                                    }
                                    else if (ScadaItem.Action == "ON")
                                    {
                                        SKCircularProgress.SV.Value = 1;
                                        MyDataAccessLayer.UpdateTagValue(SKCircularProgress.SV.TagID, SKCircularProgress.SV.Value, 0);
                                        MyDataAccessLayer.StoreTagValue(SKCircularProgress.SV.TagID, SKCircularProgress.SV.Value);
                                    }
                                    else if (ScadaItem.Action == "OFF")
                                    {
                                        SKCircularProgress.SV.Value = 0;
                                        MyDataAccessLayer.UpdateTagValue(SKCircularProgress.SV.TagID, SKCircularProgress.SV.Value, 0);
                                        MyDataAccessLayer.StoreTagValue(SKCircularProgress.SV.TagID, SKCircularProgress.SV.Value);
                                    }
                                 
                                    else
                                    {
                                        //ScadaGlobals.Currentpage = ScadaItem.Nextpage;
                                    }
                                    SKCircularProgress.GradientStartColor = ScadaColor.uxItemColor;
                                    SKCircularProgress.GradientEndColor = ScadaColor.uxItemColor;
                                    //SKCircularProgress.InvalidateSurface();
                                    
                                    RefreshGui();
                                    break;

                                case SKTouchAction.Pressed:
                                    SKCircularProgress.GradientStartColor = ScadaColor.uxTouchColor;
                                    SKCircularProgress.GradientEndColor = ScadaColor.uxTouchColor;
                                    break;

                                case SKTouchAction.Moved:
                                    break;

                                case SKTouchAction.Entered:
                                    SKCircularProgress.bFaceFade = false;
                                    break;

                                case SKTouchAction.Exited:
                                    SKCircularProgress.bFaceFade = true;
                                    break;
                            }
                        };


                        if (Designing == true)
                        {
                            SKCircularProgress = (CircularProgress)AttachDesignEvents(SKCircularProgress, ScadaItem);
                        }
                        AbsoluteLayout.SetLayoutBounds(SKCircularProgress, new Rect(wScale * ScadaItem.Left, hScale * ScadaItem.Top, wScale * ScadaItem.Width, hScale * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(SKCircularProgress, AbsoluteLayoutFlags.None);
                        SKCanvasViews.Add(SKCircularProgress);
                        break;

                    case ScadaClasses.uxCircularGauge:
                        var SKCircularGauge = new CircularGauge();
                        SKCircularGauge.Init(wScale, hScale, SKCircularGauge, ScadaItem, ScadaColor);
                        SKCircularGauge.Touch += (sender, args) =>
                        {
                            var pt = args.Location;
                            switch (args.ActionType)
                            {
                                case SKTouchAction.Pressed:
                                    SKCircularGauge.LastY = pt.Y;
                                    SKCircularGauge.State = 1;
                                    break;

                                case SKTouchAction.Released:
                                    SKCircularGauge.State = 2;
                                    MyDataAccessLayer.UpdateTagValue(SKCircularGauge.SV.TagID, SKCircularGauge.SV.Value, 0);
                                    MyDataAccessLayer.StoreTagValue(SKCircularGauge.SV.TagID, SKCircularGauge.SV.Value);
                                    //ScadaClasses.Refresh = true;
                                    RefreshGui();
                                    break;

                                case SKTouchAction.Moved:
                                    if (SKCircularGauge.State == 1)
                                    {
                                        if (SKCircularGauge.LastY < pt.Y)
                                        {
                                            SKCircularGauge.SV.Value = SKCircularGauge.SV.Value - 1.0F;
                                        }
                                        else
                                        {
                                            SKCircularGauge.SV.Value = SKCircularGauge.SV.Value + 1.0F;
                                        }
                                        SKCircularGauge.LastY = pt.Y;
                                        SKCircularGauge.InvalidateSurface();

                                    }
                                    break;
                                case SKTouchAction.Exited:
                                    break;
                            }
                            args.Handled = true;
                        };
                        if (Designing == true)
                        {
                            SKCircularGauge = (CircularGauge)AttachDesignEvents(SKCircularGauge, ScadaItem);
                        }
                       


                        AbsoluteLayout.SetLayoutBounds(SKCircularGauge, new Rect(wScale * ScadaItem.Left, hScale * ScadaItem.Top, wScale * ScadaItem.Width, hScale * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(SKCircularGauge, AbsoluteLayoutFlags.None);
                        SKCanvasViews.Add(SKCircularGauge);
                        break;
                    case ScadaClasses.uxDonutChart:
                        var SKDonut = new DonutChart();
                        SKDonut.Init(wScale, hScale, SKDonut, ScadaItem, ScadaColor);
                        if (Designing == true)
                        {
                            SKDonut = (DonutChart)AttachDesignEvents(SKDonut, ScadaItem);
                        }
                        AbsoluteLayout.SetLayoutBounds(SKDonut, new Rect(wScale * ScadaItem.Left, hScale * ScadaItem.Top, wScale * ScadaItem.Width, hScale * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(SKDonut, AbsoluteLayoutFlags.None);
                        SKCanvasViews.Add(SKDonut);
                        break;

                    case ScadaClasses.uxBarGraph:
                        var SKBarGraph = new ScadaBarGraph();
                        SKBarGraph.Init(wScale, hScale, SKBarGraph, ScadaItem, ScadaColor);
                        if (Designing == true)
                        {
                            SKBarGraph = (ScadaBarGraph)AttachDesignEvents(SKBarGraph, ScadaItem);
                        }
                        SKBarGraph.Start();
                        AbsoluteLayout.SetLayoutBounds(SKBarGraph, new Rect(wScale * ScadaItem.Left, hScale * ScadaItem.Top, wScale * ScadaItem.Width, hScale * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(SKBarGraph, AbsoluteLayoutFlags.None);
                        SKCanvasViews.Add(SKBarGraph);
                        break;

                    case ScadaClasses.ux3D:
                        var SK3D = new Scada3D();
                        SK3D.Init(wScale, hScale, SK3D, ScadaItem, ScadaColor);             
                        if (Designing == true)
                        {
                            SK3D = (Scada3D)AttachDesignEvents(SK3D, ScadaItem);
                        }
                        AbsoluteLayout.SetLayoutBounds(SK3D, new Rect(wScale * ScadaItem.Left, hScale * ScadaItem.Top, wScale * ScadaItem.Width, hScale * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(SK3D, AbsoluteLayoutFlags.None);                   
                        SKCanvasViews.Add(SK3D);
                        break;

                    case ScadaClasses.uxAlarmGrid:
                        //ScadaAlarmGrid();
                        break;

                    case ScadaClasses.uxParameters:
                        //ScadaParameters();
                        break;

                    case ScadaClasses.uxTagsGrid:
                        //ScadaTagsGrid();                  
                        break;

                    case ScadaClasses.uxTagSettings:
                        break;

                    case ScadaClasses.uxProtocols:
                        ScadaProtocols();     
                        break;

                    case ScadaClasses.uxProtocolSettings:
                        ScadaProtocolSettings();
                        break;

                    case ScadaClasses.uxSvg:
                        var SKSvg = new ScadaSvg();
                        SKSvg.Init(wScale, hScale, SKSvg, ScadaItem, ScadaColor);
                        if (Designing == true)
                        {
                            SKSvg = (ScadaSvg)AttachDesignEvents(SKSvg, ScadaItem);
                        }
                        AbsoluteLayout.SetLayoutBounds(SKSvg, new Rect(wScale * ScadaItem.Left, hScale * ScadaItem.Top, wScale * ScadaItem.Width, hScale * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(SKSvg, AbsoluteLayoutFlags.None);
                        SKCanvasViews.Add(SKSvg);                    
                        break;

                    case ScadaClasses.uxText:
                        var SKText = new ScadaText();
                        SKText.Init(wScale, hScale, SKText, ScadaItem, ScadaColor);
                        if (Designing == true)
                        {
                            SKText = (ScadaText)AttachDesignEvents(SKText, ScadaItem);
                        }
                        AbsoluteLayout.SetLayoutBounds(SKText, new Rect(wScale * ScadaItem.Left, hScale * ScadaItem.Top, wScale * ScadaItem.Width, hScale * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(SKText, AbsoluteLayoutFlags.None);
                        SKCanvasViews.Add(SKText);
                        break;

                    case ScadaClasses.uxButton:                      
                        var SKButton = new ScadaButton();
                        SKButton.SvgBase64 = MyDataAccessLayer.LoadLibItem(ScadaItem.Action,1);
                        SKButton.Init(wScale, hScale, SKButton, ScadaItem, ScadaColor, Designing);
                        if (Designing == true)//&&(Editing==false))
                        {
                            SKButton = (ScadaButton)AttachDesignEvents(SKButton, ScadaItem);
                        }
                        SKButton.Touch += (sender, args) =>
                        {
                            switch (args.ActionType)
                            {
                                case SKTouchAction.Released:
                                    if (ScadaItem.Action == "TOGGLE")
                                    {
                                        if (SKButton.SV.Value > 0.5)
                                        {
                                            SKButton.SV.Value = 0;
                                        }
                                        else
                                        {
                                            SKButton.SV.Value = 1;
                                        }
                                        MyDataAccessLayer.UpdateTagValue(SKButton.SV.TagID, SKButton.SV.Value, 0);
                                        MyDataAccessLayer.StoreTagValue(SKButton.SV.TagID, SKButton.SV.Value);

                                    }
                                    else if (ScadaItem.Action == "ON")
                                    {
                                        SKButton.SV.Value = 1;
                                        MyDataAccessLayer.UpdateTagValue(SKButton.SV.TagID, SKButton.SV.Value, 0);
                                        MyDataAccessLayer.StoreTagValue(SKButton.SV.TagID, SKButton.SV.Value);
                                    }
                                    else if (ScadaItem.Action == "OFF")
                                    {
                                        SKButton.SV.Value = 0;
                                        MyDataAccessLayer.UpdateTagValue(SKButton.SV.TagID, SKButton.SV.Value, 0);
                                        MyDataAccessLayer.StoreTagValue(SKButton.SV.TagID, SKButton.SV.Value);
                                    }
                                    else if (ScadaItem.Action == "POPUPALARM")
                                    {
                                        ScadaGlobals.Previouspage = -1;
                                        ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxAlarmGrid;
                                    }
                                    else if (ScadaItem.Action == "POPUPLOGIN")
                                    {
                                        ScadaGlobals.Previouspage = -1;
                                        ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxLoginMenu;
                                    }
                                    else if (ScadaItem.Action == "POPUPDESIGN")
                                    {
                                        ScadaGlobals.Previouspage = -1;
                                        ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxDesignMenu;
                                    }
                                    else
                                    {
                                        ScadaGlobals.Currentpage = ScadaItem.Nextpage;
                                    }
                                    SKButton.GradientStartColor = ScadaColor.uxItemColor;
                                    SKButton.GradientEndColor = ScadaColor.uxItemColor;
                                    //ScadaClasses.Refresh = true;
                                    RefreshGui();
                                    break;

                                case SKTouchAction.Pressed:
                                    SKButton.GradientStartColor = ScadaColor.uxTouchColor;
                                    SKButton.GradientEndColor = ScadaColor.uxTouchColor;
                                    break;

                                case SKTouchAction.Moved:
                                    break;

                                case SKTouchAction.Entered:
                                    SKButton.bFaceFade = false;
                                    break;

                                case SKTouchAction.Exited:
                                    SKButton.bFaceFade = true;
                                    break;
                            }
                            args.Handled = true;
                        };


                        AbsoluteLayout.SetLayoutBounds(SKButton, new Rect(wScale * ScadaItem.Left, hScale * ScadaItem.Top, wScale * ScadaItem.Width, hScale * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(SKButton, AbsoluteLayoutFlags.None);
                        SKCanvasViews.Add(SKButton);
                     break;

                    case ScadaClasses.uxRobot:
                        var SKRobot = new ScadaRobot();
                        SKRobot.Init(wScale, hScale, SKRobot, ScadaItem, ScadaColor);
                        if (Designing == true)
                        {
                            SKRobot = (ScadaRobot)AttachDesignEvents(SKRobot, ScadaItem);
                            SKRobot.EnableTouchEvents = true;
                            SKRobot.InputTransparent = false;
                            SKRobot.Stop();
                        }
                        else
                        {
                            SKRobot.EnableTouchEvents = false;
                            SKRobot.InputTransparent = true;
                            SKRobot.Start();
                        }
                        AbsoluteLayout.SetLayoutBounds(SKRobot, new Rect(wScale * ScadaItem.Left, hScale * ScadaItem.Top, wScale * ScadaItem.Width, hScale * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(SKRobot, AbsoluteLayoutFlags.None);
                        SKCanvasViews.Add(SKRobot);
                       
                        break;

                    case ScadaClasses.uxHistoryChart:
                        var trh = new HistGraph();
                        trh.ItemID = ScadaItem.ItemID;
                        trh.StyleId = ScadaItem.ItemID.ToString();                    
                        trh.ListOfValues = ScadaItem.ListOfValues;
                        trh.AnchorX = 0;
                        trh.AnchorY = 0;
                        trh.CornerRadius = 10;

                        trh.BackgroundColor = ScadaColor.uxItemColor.ToMauiColor();
                        trh.GradientStartColor = ScadaColor.uxItemBackGroundColor;
                        trh.GradientEndColor = ScadaColor.uxItemBackGroundColor;
                        trh.WidthRequest = (Width / 100) * ScadaItem.Width;
                        trh.HeightRequest = (Height / 100) * ScadaItem.Height;
                        trh.AlternativeTextColor = ScadaColor.uxTextColor;
                        trh.TextColor = ScadaColor.uxTextColor;
      
                        trh.IsEnabled = true;
                        trh.IsVisible = true;
                        trh.EnableTouchEvents = true;
                        trh.InputTransparent = false;
                        if (Designing == true)
                        {
                            trh = (HistGraph)AttachDesignEvents(trh, ScadaItem);
                        }
        
                        AbsoluteLayout.SetLayoutBounds(trh, new Rect(
                                           (Width / 100) * ScadaItem.Left,
                                           (Height / 100) * ScadaItem.Top,
                                           (Width / 100) * ScadaItem.Width,
                                           (Height / 100) * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(trh, AbsoluteLayoutFlags.None);
                        SKCanvasViews.Add(trh);

                        double wTSButton = ((Width / 100) * ScadaItem.Width) / 4;
                        double hTSButton = 24;
                        double xTSButton = (Width / 100) * ScadaItem.Left + ((trh.WidthRequest / 2) - (wTSButton / 2));
                        double yTSButton = (Height / 100) * ScadaItem.Top + (10.0);

                        var myTimeScaleButton = new ScadaButton
                        {
                            ItemID = ScadaItem.ItemID,
                            StyleId = "myTimeScaleButton",
                            WidthRequest = wTSButton,
                            HeightRequest = hTSButton,
                            Background = ScadaColor.uxPanelColor.ToMauiColor(),
                            BarBackgroundColor = ScadaColor.uxPanelColor,
                            IndicatorType = 1,
                            IndicatorColor = ScadaColor.uxPopupItemColor,
                            GradientStartColor = ScadaColor.uxPopupItemColor,
                            GradientEndColor = ScadaColor.uxPopupItemColor,
                            ButtonText = "Today last 15 minutes",
                            CornerRadius = 13,
                            TextColor = ScadaColor.uxTextColor,
                            FontSize = 12F,
                            SvgBase64 = "",
                        };
                        myTimeScaleButton.PV = new ItemValue();
                        myTimeScaleButton.SV = new ItemValue();
                        myTimeScaleButton.IsEnabled = true;
                        myTimeScaleButton.IsVisible = true;
                        myTimeScaleButton.EnableTouchEvents = true;
                        myTimeScaleButton.InputTransparent = false;

                        AbsoluteLayout.SetLayoutBounds(myTimeScaleButton, new Rect(xTSButton, yTSButton, wTSButton, hTSButton));
                        AbsoluteLayout.SetLayoutFlags(myTimeScaleButton, AbsoluteLayoutFlags.None);
                        myTimeScaleButton.Touch += (sender, args) =>
                        {
                            var pt = args.Location;
                            switch (args.ActionType)
                            {
                                case SKTouchAction.Released:
                                    //TimeScale Span menu
                                    SKCanvasPopupViews.Clear();
                                    ScadaGlobals.CurrentItem = ScadaItem.ItemID;
                                    ScadaTimeSpanMenu();
                                                              
                                    foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                                    {
                                        absoluteLayout.Add(viewItem);
                                    }
                                    Content = absoluteLayout;

                                    break;

                                case SKTouchAction.Moved:
                                    myTimeScaleButton.GradientStartColor = ScadaColor.uxHoverColor;
                                    myTimeScaleButton.GradientEndColor = ScadaColor.uxHoverColor;
                                    break;

                                case SKTouchAction.Exited:
                                    myTimeScaleButton.GradientStartColor = ScadaColor.uxPopupItemColor;
                                    myTimeScaleButton.GradientEndColor = ScadaColor.uxPopupItemColor;
                                    break;

                            }
                            args.Handled = true;
                        };
                        SKCanvasViews.Add(myTimeScaleButton);

                        double wFwd = 22;
                        double hFwd = 24;
                        double xFwd = (Width / 100) * ScadaItem.Left + (trh.WidthRequest / 2) + (wTSButton / 2) + 10;
                        double yFwd = (Height / 100) * ScadaItem.Top + 10;

                        var myTimeFwdButton = new ScadaButton
                        {
                            ItemID = ScadaItem.ItemID,
                            StyleId = "2",
                            WidthRequest = wFwd,
                            HeightRequest = hFwd,
                            Background = ScadaColor.uxPanelColor.ToMauiColor(),
                            BarBackgroundColor = ScadaColor.uxPanelColor,
                            IndicatorType = 1,

                            IndicatorColor = ScadaColor.uxPopupItemColor,
                            GradientStartColor = ScadaColor.uxPopupItemColor,
                            GradientEndColor = ScadaColor.uxPopupItemColor,
                            ButtonText = ">",
                            CornerRadius = 13,
                            TextColor = ScadaColor.uxTextColor,
                            FontSize = 12F,
                            SvgBase64 = "",
                        };
                        myTimeFwdButton.PV = new ItemValue();
                        myTimeFwdButton.SV = new ItemValue();

                        myTimeFwdButton.IsEnabled = true;
                        myTimeFwdButton.IsVisible = true;
                        myTimeFwdButton.EnableTouchEvents = true;
                        myTimeFwdButton.InputTransparent = false;

                        AbsoluteLayout.SetLayoutBounds(myTimeFwdButton, new Rect(xFwd, yFwd, wFwd, hFwd));
                        AbsoluteLayout.SetLayoutFlags(myTimeFwdButton, AbsoluteLayoutFlags.None);
                        myTimeFwdButton.Touch += (sender, args) =>
                        {
                            var pt = args.Location;
                            switch (args.ActionType)
                            {
                                case SKTouchAction.Released:
                                    ChartSetTimeScaleStep(ScadaItem.ItemID, 1);
                                    RefreshGui();                       
                                    break;

                                case SKTouchAction.Moved:
                                    myTimeFwdButton.GradientStartColor = ScadaColor.uxHoverColor;
                                    myTimeFwdButton.GradientEndColor = ScadaColor.uxHoverColor;
                                    break;

                                case SKTouchAction.Exited:
                                    myTimeFwdButton.GradientStartColor = ScadaColor.uxPopupItemColor;
                                    myTimeFwdButton.GradientEndColor = ScadaColor.uxPopupItemColor;
                                    break;

                            }
                            args.Handled = true;
                        };
                        SKCanvasViews.Add(myTimeFwdButton);

                        double wRwd = 22;
                        double hRwd = 24;
                        double xRwd = (Width / 100) * ScadaItem.Left + (trh.WidthRequest / 2) - (wTSButton / 2) - wRwd - 10;
                        double yRwd = (Height / 100) * ScadaItem.Top + 10;
                        var myTimeRwdButton = new ScadaButton
                        {
                            ItemID = ScadaItem.ItemID,
                            StyleId = "2",
                            WidthRequest = wRwd,
                            HeightRequest = hRwd,
                            Background = ScadaColor.uxPanelColor.ToMauiColor(),
                            BarBackgroundColor = ScadaColor.uxPanelColor,
                            IndicatorType = 1,

                            IndicatorColor = ScadaColor.uxPopupItemColor,
                            GradientStartColor = ScadaColor.uxPopupItemColor,
                            GradientEndColor = ScadaColor.uxPopupItemColor,
                            ButtonText = "<",
                            CornerRadius = 13,
                            TextColor = ScadaColor.uxTextColor,
                            FontSize = 12F,
                            SvgBase64 = "",
                        };
                        myTimeRwdButton.PV = new ItemValue();
                        myTimeRwdButton.SV = new ItemValue();
                        myTimeRwdButton.IsEnabled = true;
                        myTimeRwdButton.IsVisible = true;
                        myTimeRwdButton.EnableTouchEvents = true;
                        myTimeRwdButton.InputTransparent = false;

                        AbsoluteLayout.SetLayoutBounds(myTimeRwdButton, new Rect(xRwd, yRwd, wRwd, hRwd));
                        AbsoluteLayout.SetLayoutFlags(myTimeRwdButton, AbsoluteLayoutFlags.None);
                        myTimeRwdButton.Touch += (sender, args) =>
                        {
                            var pt = args.Location;
                            switch (args.ActionType)
                            {
                                case SKTouchAction.Released:
                                    ChartSetTimeScaleStep(ScadaItem.ItemID, -1);
                                    RefreshGui();
                                    break;

                                case SKTouchAction.Moved:
                                    myTimeRwdButton.GradientStartColor = ScadaColor.uxHoverColor;
                                    myTimeRwdButton.GradientEndColor = ScadaColor.uxHoverColor;
                                    break;

                                case SKTouchAction.Exited:
                                    myTimeRwdButton.GradientStartColor = ScadaColor.uxPopupItemColor;
                                    myTimeRwdButton.GradientEndColor = ScadaColor.uxPopupItemColor;
                                    break;

                            }
                            args.Handled = true;
                        };
                        SKCanvasViews.Add(myTimeRwdButton);

                        int btnRow = 0;
                        float btnY = 40F;
                      
                        var GraphItems = MyDataAccessLayer.GetItemTags(ScadaItem.ItemID);
                        foreach (var myGraph in GraphItems)
                        {
                            double wTagButton = ((Width / 100) * ScadaItem.Width) / 4;
                            double hTagButton = 24;
                            double xTagButton = (Width / 100) * ScadaItem.Left + ((trh.WidthRequest / 2) - (wTSButton / 2));
                            double yTagButton = (Height / 100) * ScadaItem.Top + btnY;
                            btnRow++;
                            var myTagButton = new ScadaButton
                            {
                                ItemID = ScadaItem.ItemID,
                                StyleId = "myTagButton",
                               
                                WidthRequest = wTagButton,
                                HeightRequest = hTagButton,
                                Background = ScadaColor.uxPanelColor.ToMauiColor(),
                                BarBackgroundColor = ScadaColor.uxPanelColor,
                                IndicatorType = 1,
                                TagSequence = btnRow,
                                IndicatorColor = ScadaColor.uxPopupItemColor,
                                GradientStartColor = ScadaColor.uxPopupItemColor,
                                GradientEndColor = ScadaColor.uxPopupItemColor,
                                ButtonText = MyDataAccessLayer.GetTagDescription(myGraph.TagID),
                                CornerRadius = 13,
                                TextColor = ScadaColor.uxTextColor,
                                FontSize = 12F,
                                SvgBase64 = "",
                            }; 
                            myTagButton.PV = new ItemValue();
                            myTagButton.SV = new ItemValue();
                            myTagButton.IsEnabled = true;
                            myTagButton.IsVisible = true;
                            myTagButton.EnableTouchEvents = true;
                            myTagButton.InputTransparent = false;

                            AbsoluteLayout.SetLayoutBounds(myTagButton, new Rect(xTagButton, yTagButton, wTagButton, hTagButton));
                            AbsoluteLayout.SetLayoutFlags(myTagButton, AbsoluteLayoutFlags.None);
                            myTagButton.Touch += (sender, args) =>
                            {
                                var pt = args.Location;
                                switch (args.ActionType)
                                {
                                    case SKTouchAction.Released:

                                        ScadaGlobals.CurrentItem = ScadaItem.ItemID;
                                        ScadaGlobals.CurrentTag = myGraph.TagID;
                                        ScadaGlobals.CurrentTagSequence = myTagButton.TagSequence;
                                        ScadaGlobals.PreviousScadaPopup = -1;
                                        ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxTagsMenu;
                                        ScadaGlobals.CurrentType = ScadaClasses.uxHistoryChart;
                                        foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                                        {
                                            absoluteLayout.Remove(viewItem);
                                        }
                                        SKCanvasPopupViews.Clear();
                                        uxtimer.Interval = 50;
                                        
                                        
                                       /* ScadaTagsMenu();
                                        foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                                        {
                                            absoluteLayout.Add(viewItem);
                                        }
                                      
                                        Content = absoluteLayout;
                                        */
                                        break;

                                    case SKTouchAction.Moved:
                                        myTagButton.GradientStartColor = ScadaColor.uxHoverColor;
                                        myTagButton.GradientEndColor = ScadaColor.uxHoverColor;
                                        break;

                                    case SKTouchAction.Exited:
                                        myTagButton.GradientStartColor = ScadaColor.uxPopupItemColor;
                                        myTagButton.GradientEndColor = ScadaColor.uxPopupItemColor;
                                        break;

                                }
                                args.Handled = true;
                            };
                            SKCanvasViews.Add(myTagButton);
                           
                            btnY = btnY + 30;
                        }

                        double wAddButton = 22;
                        double hAddButton = 24;
                        double xAddButton = (Width / 100) * ScadaItem.Left + ((trh.WidthRequest / 2) - 11)/* (wTSButton / 2)*/;
                        double yAddButton = (Height / 100) * ScadaItem.Top + btnY;

                        var myAddButton = new ScadaButton
                        {
                            ItemID = ScadaItem.ItemID,
                            StyleId = "myTagButton",
                            WidthRequest = wAddButton,
                            HeightRequest = hAddButton,
                            Background = ScadaColor.uxPanelColor.ToMauiColor(),
                            BarBackgroundColor = ScadaColor.uxPanelColor,
                            IndicatorType = 1,
                            TagSequence = btnRow+1,//Addbutton
                            IndicatorColor = ScadaColor.uxPopupItemColor,
                            GradientStartColor = ScadaColor.uxPopupItemColor,
                            GradientEndColor = ScadaColor.uxPopupItemColor,
                            ButtonText = "+",
                            CornerRadius = 13,
                            TextColor = ScadaColor.uxTextColor,
                            FontSize = 12F,
                            SvgBase64 = MyDataAccessLayer.LoadLibItem("GrayPlus", 1),
                        };
                        myAddButton.PV = new ItemValue();
                        myAddButton.SV = new ItemValue();
                        myAddButton.IsEnabled = true;
                        myAddButton.IsVisible = true;
                        myAddButton.EnableTouchEvents = true;
                        myAddButton.InputTransparent = false;

                        AbsoluteLayout.SetLayoutBounds(myAddButton, new Rect(xAddButton, yAddButton, wAddButton, hAddButton));
                        AbsoluteLayout.SetLayoutFlags(myAddButton, AbsoluteLayoutFlags.None);
                        myAddButton.Touch += (sender, args) =>
                        {
                            var pt = args.Location;
                            switch (args.ActionType)
                            {
                                case SKTouchAction.Released:
                                    ScadaGlobals.CurrentItem = ScadaItem.ItemID;
                                    ScadaGlobals.CurrentTag = 0;
                                    ScadaGlobals.CurrentRow = btnRow; 
                                    ScadaGlobals.CurrentTagSequence = myAddButton.TagSequence;
                                    ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxTagsGrid;
                                    ScadaGlobals.CurrentType = ScadaClasses.uxHistoryChart;
                                    foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                                    {
                                        absoluteLayout.Remove(viewItem);
                                    }
                                    SKCanvasPopupViews.Clear();
                                    uxtimer.Interval = 50;
                                    /*
                                    ScadaTagsMenu();
                                    foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                                    {
                                        absoluteLayout.Add(viewItem);
                                    }
                                    Content = absoluteLayout;
                                    */
                                    break;

                                case SKTouchAction.Moved:
                                    myAddButton.GradientStartColor = ScadaColor.uxHoverColor;
                                    myAddButton.GradientEndColor = ScadaColor.uxHoverColor;
                                    break;

                                case SKTouchAction.Exited:
                                    myAddButton.GradientStartColor = ScadaColor.uxPopupItemColor;
                                    myAddButton.GradientEndColor = ScadaColor.uxPopupItemColor;
                                    break;

                            }
                            args.Handled = true;
                        };
                        SKCanvasViews.Add(myAddButton);
                        break;

                    case ScadaClasses.uxToggle:
                        var SKToggle = new Toggle();
                        SKToggle.ItemID = ScadaItem.ItemID;
                        SKToggle.StyleId = ScadaItem.ItemID.ToString();
                        SKToggle.AnchorX = 0;
                        SKToggle.AnchorY = 0;
                        SKToggle.CornerRadius = 1;
                        SKToggle.BarBackgroundColor = ScadaColor.uxItemColor;
                        SKToggle.BackgroundColor = ScadaColor.uxItemColor.ToMauiColor();
                        SKToggle.GradientStartColor = ScadaColor.uxItemColor;
                        SKToggle.GradientEndColor = ScadaColor.uxItemColor;
                        SKToggle.WidthRequest = (Width / 100) * ScadaItem.Width;
                        SKToggle.HeightRequest = (Height / 100) * ScadaItem.Height;

                        SKToggle.PV = new ItemValue();
                        SKToggle.SV = new ItemValue();

                        var ItemValuesToggle = MyDataAccessLayer.ReadItemValues(SKToggle.ItemID);
                        int idxToggle = 0;
                        foreach (var item in ItemValuesToggle)
                        {
                            if (idxToggle == 0)
                            {
                                SKToggle.PV.TagID = item.TagID;
                                SKToggle.PV.Value = item.Value;
                            }
                            if (idxToggle == 1)
                            {
                                SKToggle.SV.TagID = item.TagID;
                                SKToggle.SV.Value = item.Value;
                            }
                            idxToggle++;
                        }
                        SKToggle.IsEnabled = true;
                        SKToggle.IsVisible = true;
                        SKToggle.EnableTouchEvents = true;
                        SKToggle.InputTransparent = false;
                        SKToggle.TextColor = ScadaColor.uxTextColor;
                        SKToggle.Start();
                        if (Designing == true)
                        {
                            SKToggle = (Toggle)AttachDesignEvents(SKToggle, ScadaItem);
                        }
                        else
                        {
                            SKToggle.Touch += (sender, args) =>
                            {
                                switch (args.ActionType)
                                {
                                    case SKTouchAction.Released:                                     
                                        if (SKToggle.SV.Value > 0.5)
                                        {
                                            SKToggle.SV.Value = 0;
                                        }
                                        else
                                        {
                                            SKToggle.SV.Value = 1;
                                        }                                 
                                        MyDataAccessLayer.UpdateTagValue(SKToggle.SV.TagID, SKToggle.SV.Value,0);                                  
                                        MyDataAccessLayer.StoreTagValue(SKToggle.SV.TagID, SKToggle.SV.Value);                                    
                                        SKToggle.InvalidateSurface();
                                        RefreshGui();
                                        break;
                                }
                                args.Handled = true;
                            };
                        }
                        AbsoluteLayout.SetLayoutBounds(SKToggle, new Rect(wScale * ScadaItem.Left, hScale * ScadaItem.Top, wScale * ScadaItem.Width, hScale * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(SKToggle, AbsoluteLayoutFlags.None);
                        SKCanvasViews.Add(SKToggle);
                        break;

                    case ScadaClasses.uxProgressBar:
                        var pr = new ScadaProgress();
                        pr.ItemID = ScadaItem.ItemID;
                        pr.StyleId = ScadaItem.ItemID.ToString();
                        pr.AnchorX = 0;
                        pr.AnchorY = 0;
                        pr.CornerRadius = 1;
                        pr.BarBackgroundColor = ScadaColor.uxItemColor;
                        pr.BackgroundColor = ScadaColor.uxItemColor.ToMauiColor();
                        pr.GradientStartColor = ScadaColor.uxItemColor;
                        pr.GradientEndColor = ScadaColor.uxItemColor;
                        pr.WidthRequest = wScale * ScadaItem.Width;
                        pr.HeightRequest = hScale * ScadaItem.Height;
                        pr.PV = new ItemValue();
                        pr.SV = new ItemValue();
                        var ItemValuesPr = MyDataAccessLayer.ReadItemValues(pr.ItemID);
                        int idxpr = 0;
                        foreach (var item in ItemValuesPr)
                        {
                            if (idxpr == 0)
                            {
                                pr.PV.TagID = item.TagID;
                                pr.PV.Value = item.Value;
                            }
                            if (idxpr == 1)
                            {
                                pr.SV.TagID = item.TagID;
                                pr.SV.Value = item.Value;
                            }
                            idxpr++;
                        }

                        pr.TextColor = ScadaColor.uxTextColor;
                        pr.IsEnabled = true;
                        pr.IsVisible = true;
                        pr.EnableTouchEvents = true;
                        pr.InputTransparent = false;
                        if (Designing == true)
                        {
                            pr = (ScadaProgress)AttachDesignEvents(pr, ScadaItem);
                        }
                        pr.Start();
                        AbsoluteLayout.SetLayoutBounds(pr, new Rect(wScale * ScadaItem.Left, hScale * ScadaItem.Top, wScale * ScadaItem.Width, hScale * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(pr, AbsoluteLayoutFlags.None);
                        SKCanvasViews.Add(pr);
                        break;

                    case ScadaClasses.uxLine:
                        var SKLine = new ScadaLine();
                        SKLine.ItemID = ScadaItem.ItemID;
                        SKLine.StyleId = ScadaItem.ItemID.ToString();
                        SKLine.AnchorX = 0;
                        SKLine.AnchorY = 0;
                        SKLine.CornerRadius = 10;
                        SKLine.BarBackgroundColor = ScadaColor.uxBackGroundColor;
                        SKLine.BackgroundColor = ScadaColor.uxBackGroundColor.ToMauiColor();
                        SKLine.GradientStartColor = ScadaColor.uxItemColor;
                        SKLine.GradientEndColor = ScadaColor.uxItemColor;
                        SKLine.WidthRequest = wScale * ScadaItem.Width;
                        SKLine.HeightRequest = hScale * ScadaItem.Height;
                        SKLine.AlternativeTextColor = ScadaColor.uxTextColor;
                        SKLine.TextColor = ScadaColor.uxTextColor;
                        SKLine.FontSize = 10;
                        SKLine.IsEnabled = true;
                        SKLine.IsVisible = true;            
                        SKLine.EnableTouchEvents = true;
                        SKLine.InputTransparent = false;
                        if (Designing == true)
                        {
                            SKLine = (ScadaLine)AttachDesignEvents(SKLine, ScadaItem);
                        }
                        AbsoluteLayout.SetLayoutBounds(SKLine, new Rect(wScale * ScadaItem.Left, hScale * ScadaItem.Top, wScale * ScadaItem.Width, hScale * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(SKLine, AbsoluteLayoutFlags.None);
                        SKCanvasViews.Add(SKLine);
                        break;
                    case ScadaClasses.uxSimulator:
                        var sim = new Simulator();
                        sim.ItemID = ScadaItem.ItemID;
                        sim.StyleId = ScadaItem.ItemID.ToString();
                        sim.AnchorX = 0;
                        sim.AnchorY = 0;
                        sim.CornerRadius = 10;
                        sim.BarBackgroundColor = ScadaColor.uxBackGroundColor;
                        sim.BackgroundColor = ScadaColor.uxBackGroundColor.ToMauiColor();
                        sim.GradientStartColor = ScadaColor.uxItemColor;
                        sim.GradientEndColor = ScadaColor.uxItemColor;
                        sim.WidthRequest = (Width / 100) * ScadaItem.Width;
                        sim.HeightRequest = (Height / 100) * ScadaItem.Height;

                        sim.IsEnabled = true;
                        sim.IsVisible = true;
                        sim.TextColor = ScadaColor.uxTextColor;
                        sim.IsEnabled = true;
                        sim.IsVisible = true;
                        sim.EnableTouchEvents = true;
                        sim.InputTransparent = false;
                        sim.PV = new ItemValue();
                        sim.Acutator = new ItemValue();
                        var ItemValuesSim = MyDataAccessLayer.ReadItemValues(sim.ItemID);
                        int simVal = 0;
                        foreach (var item in ItemValuesSim)
                        {
                            if (simVal == 0)
                            {
                                sim.PV.TagID = item.TagID;
                                sim.PV.Value = item.Value;
                            }
                            if (simVal == 1)
                            {
                                sim.Acutator.TagID = item.TagID;
                                sim.Acutator.Value = item.Value;
                            }
                            simVal++;
                        }
                        sim.Start();
                        if (Designing == true)
                        {
                            sim = (Simulator)AttachDesignEvents(sim, ScadaItem);
                        }
                        AbsoluteLayout.SetLayoutBounds(sim, new Rect(wScale * ScadaItem.Left, hScale * ScadaItem.Top, wScale * ScadaItem.Width, hScale * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(sim, AbsoluteLayoutFlags.None);
                        SKCanvasViews.Add(sim);
                        break;


                    case ScadaClasses.uxController:
                        var controller = new Controller();
                        controller.ItemID = ScadaItem.ItemID;
                        controller.StyleId = ScadaItem.ItemID.ToString();
                        controller.AnchorX = 0;
                        controller.AnchorY = 0;
                        controller.CornerRadius = 10;
                        controller.BarBackgroundColor = ScadaColor.uxBackGroundColor;
                        controller.BackgroundColor = ScadaColor.uxBackGroundColor.ToMauiColor();
                        controller.GradientStartColor = ScadaColor.uxItemColor;
                        controller.GradientEndColor = ScadaColor.uxItemColor;
                        controller.WidthRequest = (Width / 100) * ScadaItem.Width;
                        controller.HeightRequest = (Height / 100) * ScadaItem.Height;

                        controller.IsEnabled = true;
                        controller.IsVisible = true;
                        controller.TextColor = ScadaColor.uxTextColor;
                        controller.EnableTouchEvents = true;
                        controller.InputTransparent = false;
                        controller.PV = new ItemValue();
                        controller.SV = new ItemValue();
                        controller.Output = new ItemValue();
                        var ItemValuesController = MyDataAccessLayer.ReadItemValues(controller.ItemID);
                        int contrVal = 0;
                        foreach (var item in ItemValuesController)
                        {
                            if (contrVal == 0)
                            {
                                controller.PV.TagID = item.TagID;
                                controller.PV.Value = item.Value;
                            }
                            if (contrVal == 1)
                            {
                                controller.SV.TagID = item.TagID;
                                controller.SV.Value = item.Value;
                            }
                            if (contrVal == 2)
                            {
                                controller.Output.TagID = item.TagID;
                                controller.Output.Value = item.Value;
                            }
                            contrVal++;
                        }
                        controller.Start();
                        if (Designing == true)
                        {
                            controller = (Controller)AttachDesignEvents(controller, ScadaItem);
                        }
                        AbsoluteLayout.SetLayoutBounds(controller, new Rect(wScale * ScadaItem.Left, hScale * ScadaItem.Top, wScale * ScadaItem.Width, hScale * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(controller, AbsoluteLayoutFlags.None);
                        SKCanvasViews.Add(controller);
                        break;

                    case ScadaClasses.uxLoginMenu:
                        ScadaLogin(infoType);
                        break;

                    case ScadaClasses.uxDesignMenu:
                        //ScadaDesign(0);
                        break;

                    case ScadaClasses.uxSvgPopupMenu:
                        break;

                    case ScadaClasses.uxUploadMenu:
                        ScadaUpload();
                        break;

                    case ScadaClasses.uxConfigMenu:
                      /*  var FloatPanel = new ScadaButton();
                        FloatPanel.ItemID = ScadaItem.ItemID;
                        FloatPanel.StyleId = ScadaItem.ItemID.ToString();
                        FloatPanel.AnchorX = 0;
                        FloatPanel.AnchorY = 0;
                        FloatPanel.CornerRadius = 10;
                        FloatPanel.BarBackgroundColor = ScadaColor.uxPanelColor;
                        FloatPanel.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
                        FloatPanel.GradientStartColor = ScadaColor.uxPopupColor;
                        FloatPanel.GradientEndColor = ScadaColor.uxPopupColor;
                        FloatPanel.IndicatorColor = ScadaColor.uxPopupColor; ;
                        FloatPanel.WidthRequest = (Width / 100) * ScadaItem.Width;
                        FloatPanel.HeightRequest = (Height / 100) * ScadaItem.Height;
                        FloatPanel.AlternativeTextColor = ScadaColor.uxTextColor;
                        FloatPanel.TextColor = ScadaColor.uxTextColor;
                        FloatPanel.IsEnabled = true;
                        FloatPanel.IsVisible = true;
                        FloatPanel.IndicatorType = 10;
                        AbsoluteLayout.SetLayoutBounds(FloatPanel, new Rect(
                            (Width / 100) * ScadaItem.Left,
                            (Height / 100) * ScadaItem.Top,
                            (Width / 100) * ScadaItem.Width,
                            (Height / 100) * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(FloatPanel, AbsoluteLayoutFlags.None);
                        SKCanvasPopupViews.Add(FloatPanel);

                        x = (Width / 100) * ScadaItem.Left;
                        y = (Height / 100) * ScadaItem.Top;
                        w = (Width / 100) * ScadaItem.Width;
                        h = (Height / 100) * ScadaItem.Height;

                        var myDeleteButton = new ScadaButton
                        {
                            WidthRequest = 25,
                            HeightRequest = 25,
                            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
                            BarBackgroundColor = ScadaColor.uxBackGroundColor,
                            IndicatorType = 9,

                            GradientStartColor = ScadaColor.uxItemColor,
                            GradientEndColor = ScadaColor.uxItemColor,
                            CornerRadius = 10,
                            TextColor = ScadaColor.uxTextColor,
                            FontSize = 21.5F,
                            Margin = 0.15F,
                            SvgBase64 = MyDataAccessLayer.LoadLibItem("Trashcan", 1),
                            ButtonText = ""
                        };
                        myDeleteButton.IsVisible = true;
                        myDeleteButton.InputTransparent = false;
                        myDeleteButton.EnableTouchEvents = true;
                        myDeleteButton.Touch += (sender, args) =>
                        {
                            var pt = args.Location;
                            switch (args.ActionType)
                            {
                                case SKTouchAction.Released:                  
                                    MyDataAccessLayer.deleteItem(ScadaGlobals.CurrentItem);
                                    ScadaGlobals.CurrentScadaPopup = -1;
                                    ScadaGlobals.Previouspage = -1;
                                    //ScadaClasses.Refresh = true;
                                    break;

                                case SKTouchAction.Moved:
                                    myDeleteButton.GradientStartColor = ScadaColor.uxHoverColor;
                                    myDeleteButton.GradientEndColor = ScadaColor.uxHoverColor;
                                    break;

                                case SKTouchAction.Exited:
                                    myDeleteButton.GradientStartColor = ScadaColor.uxItemColor;
                                    myDeleteButton.GradientEndColor = ScadaColor.uxItemColor;
                                    break;

                            }
                            args.Handled = true;
                        };
                        AbsoluteLayout.SetLayoutBounds(myDeleteButton, new Rect(x + w - 35, y + h - 35, 30, 30));
                        AbsoluteLayout.SetLayoutFlags(myDeleteButton, AbsoluteLayoutFlags.None);

                        CreateCloseButton(x, y, w, h, -1);
                        SKCanvasPopupViews.Add(myDeleteButton);

                        var myAddItemButton = new ScadaButton
                        {
                            WidthRequest = 25,
                            HeightRequest = 25,
                            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
                            BarBackgroundColor = ScadaColor.uxBackGroundColor,
                            IndicatorType = 9,
                            GradientStartColor = ScadaColor.uxItemColor,
                            GradientEndColor = ScadaColor.uxItemColor,
                            CornerRadius = 10,
                            TextColor = ScadaColor.uxTextColor,
                            FontSize = 21.5F,
                            Margin = 0.15F,
                            SvgBase64 = MyDataAccessLayer.LoadLibItem("GreenPlus", 1),
                            ButtonText = ""
                        };
                        myAddItemButton.IsVisible = true;
                        myAddItemButton.InputTransparent = false;
                        myAddItemButton.EnableTouchEvents = true;
                        myAddItemButton.Touch += (sender, args) =>
                        {
                            var pt = args.Location;
                            switch (args.ActionType)
                            {
                                case SKTouchAction.Released:
                                    ScadaClasses.Telegram oTelegram = new ScadaClasses.Telegram()
                                    {
                                        MessageType = ScadaItem.MessageType,
                                        Page = ScadaGlobals.Currentpage,
                                        ItemType = ScadaGlobals.CurrentType,
                                        ItemID = ScadaGlobals.CurrentItem,
                                        TagID = ScadaItem.TagID,
                                        TagName = ScadaItem.TagName,
                                        Action = ScadaItem.Action,
                                    };
                                    oTelegram.ItemID = MyDataAccessLayer.copyItem(oTelegram);
                                    MyDataAccessLayer.AddItemDefaultTags(oTelegram);
                                    ScadaGlobals.CurrentScadaPopup = -1;
                                    ScadaGlobals.Previouspage = -1;
                                    //ScadaClasses.Refresh = true;
                                    break;

                                case SKTouchAction.Moved:
                                    myAddItemButton.GradientStartColor = ScadaColor.uxHoverColor;
                                    myAddItemButton.GradientEndColor = ScadaColor.uxHoverColor;
                                    break;

                                case SKTouchAction.Exited:
                                    myAddItemButton.GradientStartColor = ScadaColor.uxItemColor;
                                    myAddItemButton.GradientEndColor = ScadaColor.uxItemColor;
                                    break;
                            }
                            args.Handled = true;
                        };
                        AbsoluteLayout.SetLayoutBounds(myAddItemButton, new Rect(x + 5, y + 5, 30, 30));
                        AbsoluteLayout.SetLayoutFlags(myAddItemButton, AbsoluteLayoutFlags.None);
                        SKCanvasPopupViews.Add(myAddItemButton);

                        y = y + FloatPanel.CornerRadius + 50;

                        var mySvgButton1 = new ScadaButton
                        {
                            WidthRequest = (Width / 100) * ScadaItem.Width,
                            HeightRequest = 25,
                            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
                            BarBackgroundColor = ScadaColor.uxBackGroundColor,
                            IndicatorType = 4,
                            GradientStartColor = ScadaColor.uxPopupItemColor,
                            GradientEndColor = ScadaColor.uxPopupItemColor,
                            CornerRadius = 0,
                            TextColor = ScadaColor.uxTextColor,
                            FontSize = 16.5F,
                            Margin = 0.15F,
                            SvgBase64 = MyDataAccessLayer.LoadLibItem("InputSource", 1),
                            ButtonText = "Input"
                        };
                        mySvgButton1.InputTransparent = false;
                        mySvgButton1.EnableTouchEvents = true;
                        mySvgButton1.Touch += (sender, args) =>
                        {
                            var pt = args.Location;
                            switch (args.ActionType)
                            {
                                case SKTouchAction.Released:                          
                                    //ScadaClasses.CurrentScadaPopup = ScadaClasses.uxItemTagsMenu;
                                    //ScadaClasses.Previouspage = -1;
                                    //ScadaClasses.Refresh = true;                            
                                    break;

                                case SKTouchAction.Moved:
                                    mySvgButton1.GradientStartColor = ScadaColor.uxHoverColor;
                                    mySvgButton1.GradientEndColor = ScadaColor.uxHoverColor;
                                    break;

                                case SKTouchAction.Exited:
                                    mySvgButton1.GradientStartColor = ScadaColor.uxPopupItemColor;
                                    mySvgButton1.GradientEndColor = ScadaColor.uxPopupItemColor;
                                    break;
                            }
                            args.Handled = true;
                        };
                        AbsoluteLayout.SetLayoutBounds(mySvgButton1, new Rect(x, y, FloatPanel.WidthRequest, 20));
                        AbsoluteLayout.SetLayoutFlags(mySvgButton1, AbsoluteLayoutFlags.None);
                        if (y < ((Height / 100) * ScadaItem.Top) + ((Height / 100) * ScadaItem.Height) - 26)
                        {
                            SKCanvasPopupViews.Add(mySvgButton1);
                            y = y + 26;
                        }

                        var mySvgButton2 = new ScadaButton
                        {
                            WidthRequest = (Width / 100) * ScadaItem.Width,
                            HeightRequest = 25,
                            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
                            BarBackgroundColor = ScadaColor.uxBackGroundColor,
                            IndicatorType = 4,

                            GradientStartColor = ScadaColor.uxPopupItemColor,
                            GradientEndColor = ScadaColor.uxPopupItemColor,
                            CornerRadius = 0,
                            TextColor = ScadaColor.uxTextColor,
                            FontSize = 16.5F,
                            Margin = 0.15F,
                            SvgBase64 = MyDataAccessLayer.LoadLibItem("RobotArm", 1),
                            ButtonText = "Visual"
                        };
                        mySvgButton2.InputTransparent = false;
                        mySvgButton2.EnableTouchEvents = true;
                        mySvgButton2.Touch += (sender, args) =>
                        {
                            var pt = args.Location;
                            switch (args.ActionType)
                            {
                                case SKTouchAction.Released:                              
                                    ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxItemTypeMenu;
                                    ScadaGlobals.Previouspage = -1;
                                    //ScadaGlobals.Refresh = true;                       
                                    break;

                                case SKTouchAction.Moved:
                                    mySvgButton2.GradientStartColor = ScadaColor.uxHoverColor;
                                    mySvgButton2.GradientEndColor = ScadaColor.uxHoverColor;
                                    break;

                                case SKTouchAction.Exited:
                                    mySvgButton2.GradientStartColor = ScadaColor.uxPopupItemColor;
                                    mySvgButton2.GradientEndColor = ScadaColor.uxPopupItemColor;
                                    break;
                            }
                            args.Handled = true;
                        };

                        AbsoluteLayout.SetLayoutBounds(mySvgButton2, new Rect(x, y, WidthRequest, 20));
                        AbsoluteLayout.SetLayoutFlags(mySvgButton2, AbsoluteLayoutFlags.None);
                        if (y < ((Height / 100) * ScadaItem.Top) + ((Height / 100) * ScadaItem.Height) - 26)
                        {
                            SKCanvasPopupViews.Add(mySvgButton2);
                            y = y + 26;
                        }

                        var mySvgButton3 = new ScadaButton
                        {
                            WidthRequest = (Width / 100) * ScadaItem.Width,
                            HeightRequest = 25,
                            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
                            BarBackgroundColor = ScadaColor.uxBackGroundColor,
                            IndicatorType = 4,
                            GradientStartColor = ScadaColor.uxPopupItemColor,
                            GradientEndColor = ScadaColor.uxPopupItemColor,
                            CornerRadius = 0,
                            TextColor = ScadaColor.uxTextColor,
                            FontSize = 16.5F,
                            Margin = 0.15F,     
                            SvgBase64 = MyDataAccessLayer.LoadLibItem("GrayPlus", 1),
                            ButtonText = "Size"
                        };
                        mySvgButton3.InputTransparent = false;
                        mySvgButton3.EnableTouchEvents = true;
                        mySvgButton3.Touch += (sender, args) =>
                        {
                            var pt = args.Location;
                            switch (args.ActionType)
                            {
                                case SKTouchAction.Released:                                 
                                    //ScadaClasses.CurrentScadaPopup = ScadaClasses.uxItemSizeMenu;
                                    //ScadaClasses.Previouspage = -1;
                                    //ScadaClasses.Refresh = true;                
                                    break;

                                case SKTouchAction.Moved:
                                    mySvgButton3.GradientStartColor = ScadaColor.uxHoverColor;
                                    mySvgButton3.GradientEndColor = ScadaColor.uxHoverColor;
                                    break;

                                case SKTouchAction.Exited:
                                    mySvgButton3.GradientStartColor = ScadaColor.uxPopupItemColor;
                                    mySvgButton3.GradientEndColor = ScadaColor.uxPopupItemColor;
                                    break;
                            }
                            args.Handled = true;
                        };
                        AbsoluteLayout.SetLayoutBounds(mySvgButton3, new Rect(x, y, WidthRequest, 20));
                        AbsoluteLayout.SetLayoutFlags(mySvgButton3, AbsoluteLayoutFlags.None);
                        if (y < ((Height / 100) *ScadaItem.Top) + ((Height / 100) * ScadaItem.Height) - 26)
                        {
                            SKCanvasPopupViews.Add(mySvgButton3);
                            y = y + 26;
                        }

                        var mySvgButton4 = new ScadaButton
                        {
                            WidthRequest = (Width / 100) * ScadaItem.Width,
                            HeightRequest = 25,
                            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
                            BarBackgroundColor = ScadaColor.uxBackGroundColor,
                            IndicatorType = 4,

                            GradientStartColor = ScadaColor.uxPopupItemColor,
                            GradientEndColor = ScadaColor.uxPopupItemColor,
                            CornerRadius = 0,
                            TextColor = ScadaColor.uxTextColor,
                            FontSize = 16.5F,
                            Margin = 0.15F,
                            SvgBase64 = MyDataAccessLayer.LoadLibItem("AndroidLogo", 1),
                            ButtonText = "Page"
                        };
                        mySvgButton4.InputTransparent = false;
                        mySvgButton4.EnableTouchEvents = true;
                        mySvgButton4.Touch += (sender, args) =>
                        {
                            var pt = args.Location;
                            switch (args.ActionType)
                            {
                                case SKTouchAction.Released:
                                    //ScadaClasses.CurrentScadaPopup = ScadaClasses.uxPageChangeMenu;
                                    //ScadaClasses.Previouspage = -1;
                                    //ScadaClasses.Refresh = true;                       
                                    break;

                                case SKTouchAction.Moved:
                                    mySvgButton4.GradientStartColor = ScadaColor.uxHoverColor;
                                    mySvgButton4.GradientEndColor = ScadaColor.uxHoverColor;
                                    break;

                                case SKTouchAction.Exited:
                                    mySvgButton4.GradientStartColor = ScadaColor.uxPopupItemColor;
                                    mySvgButton4.GradientEndColor = ScadaColor.uxPopupItemColor;
                                    break;
                            }
                            args.Handled = true;
                        };

                        AbsoluteLayout.SetLayoutBounds(mySvgButton4, new Rect(x, y, WidthRequest, 20));
                        AbsoluteLayout.SetLayoutFlags(mySvgButton4, AbsoluteLayoutFlags.None);
                        if (y < ((Height / 100) * ScadaItem.Top) + ((Height / 100) * ScadaItem.Height) - 26)
                        {
                            SKCanvasPopupViews.Add(mySvgButton4);
                            y = y + 26;
                        }
                       */
                        //mySvgButton1.EnableFaceFade();
                        //mySvgButton2.EnableFaceFade();
                        //mySvgButton3.EnableFaceFade();
                        // mySvgButton4.EnableFaceFade();
                        //Thread.Sleep(200);
                        break;

                    case ScadaClasses.uxItemTagsMenu:
                        //ScadaItemTagsMenu();
                        /*
                        var FloatPanel11 = new ScadaButton();
                        FloatPanel11.ItemID = ScadaItem.ItemID;
                        FloatPanel11.StyleId = ScadaItem.ItemID.ToString();
                        FloatPanel11.AnchorX = 0;
                        FloatPanel11.AnchorY = 0;
                        FloatPanel11.CornerRadius = 10;
                        FloatPanel11.BarBackgroundColor = ScadaColor.uxBackGroundColor;
                        FloatPanel11.BackgroundColor = ScadaColor.uxBackGroundColor.ToMauiColor();
                        FloatPanel11.GradientStartColor = ScadaColor.uxPopupColor;
                        FloatPanel11.GradientEndColor = ScadaColor.uxPopupColor;
                        FloatPanel11.IndicatorColor = ScadaColor.uxBackGroundColor;

                        FloatPanel11.WidthRequest = (Width / 100) * ScadaItem.Width;
                        FloatPanel11.HeightRequest = (Height / 100) * ScadaItem.Height;
                        FloatPanel11.AlternativeTextColor = ScadaColor.uxTextColor;
                        FloatPanel11.TextColor = ScadaColor.uxTextColor;
                        FloatPanel11.IsEnabled = true;
                        FloatPanel11.IsVisible = true;
                        FloatPanel11.IndicatorType = 10;
                        FloatPanel11.EnableFaceFade();
                        AbsoluteLayout.SetLayoutBounds(FloatPanel11, new Rect(
                            (Width / 100) * ScadaItem.Left,
                            (Height / 100) * ScadaItem.Top,
                            (Width / 100) * ScadaItem.Width,
                            (Height / 100) * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(FloatPanel11, AbsoluteLayoutFlags.None);
                        SKCanvasPopupViews.Add(FloatPanel11);


                        x = (Width / 100) * ScadaItem.Left;
                        y = (Height / 100) * ScadaItem.Top;
                        w = (Width / 100) * ScadaItem.Width;
                        h = (Height / 100) * ScadaItem.Height;

                        CreateCloseButton(x, y, w, h, ScadaGlobals.CurrentScadaPopup);

                        ScadaItem.gridRows = MyDataAccessLayer.ReadItemTags(ScadaItem.ItemID);

                        y = y + FloatPanel11.CornerRadius + 50;

                        foreach (gridRow row in ScadaItem.gridRows)
                        {
                            var myItemTagsBtn = new ScadaButton
                            {
                                WidthRequest = (Width / 100) * ScadaItem.Width,
                                HeightRequest = 25,
                                Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
                                BarBackgroundColor = ScadaColor.uxBackGroundColor,
                                IndicatorType = 4,
                                IndicatorColor = GetStatusColor(row.Status),
                                GradientStartColor = ScadaColor.uxPopupItemColor,
                                GradientEndColor = ScadaColor.uxPopupItemColor,
                                CornerRadius = 0,
                                TextColor = ScadaColor.uxTextColor,
                                FontSize = 16.5F,
                                SvgBase64 = MyDataAccessLayer.LoadLibItem("InputSource", 1)
                            };
                          
                            myItemTagsBtn.ButtonRow = row;
                            myItemTagsBtn.ButtonText = row.col1text;
                            myItemTagsBtn.EnableTouchEvents = true;
                            myItemTagsBtn.InputTransparent = false;
                            myItemTagsBtn.Touch += (sender, args) =>
                            {
                                var pt = args.Location;
                                switch (args.ActionType)
                                {
                                    case SKTouchAction.Released:
                                        ScadaClasses.Telegram oTelegram = new ScadaClasses.Telegram()
                                        {
                                            MessageType = ScadaItem.MessageType,
                                            Page = ScadaItem.Nextpage,
                                            ItemType = ScadaItem.ItemType,
                                            ItemID = ScadaGlobals.CurrentItem,
                                            TagID = row.TagID,
                                            TagName = ScadaItem.TagName,
                                            Action = ScadaItem.Action,
                                        };
                                        //ScadaClasses.CurrentScadaPopup = ScadaClasses.uxTagsMenu;
                                        //ScadaClasses.Previouspage = -1;
                                        //ScadaClasses.CurrentRow = row.Id;
                                        //ScadaClasses.Refresh = true;                                    
                                        break;

                                    case SKTouchAction.Moved:
                                        myItemTagsBtn.GradientStartColor = ScadaColor.uxHoverColor;
                                        myItemTagsBtn.GradientEndColor = ScadaColor.uxHoverColor;
                                        myItemTagsBtn.IndicatorColor = GetStatusColor(row.Status);
                                        break;

                                    case SKTouchAction.Exited:
                                        myItemTagsBtn.GradientStartColor = ScadaColor.uxPopupItemColor;
                                        myItemTagsBtn.GradientEndColor = ScadaColor.uxPopupItemColor;
                                        myItemTagsBtn.IndicatorColor = GetStatusColor(row.Status);
                                        break;
                                }
                                args.Handled = true;
                            };

                            AbsoluteLayout.SetLayoutBounds(myItemTagsBtn, new Rect(x, y, FloatPanel11.WidthRequest, 20));
                            AbsoluteLayout.SetLayoutFlags(myItemTagsBtn, AbsoluteLayoutFlags.None);
                            if (y < ((Height / 100) * ScadaItem.Top) + ((Height / 100) * ScadaItem.Height) - 26)
                            {
                                SKCanvasPopupViews.Add(myItemTagsBtn);
                                y = y + 26;
                            }
                            
                        }*/
                        break;

                    case ScadaClasses.uxItemSizeMenu:
                        /*
                        var FloatPanelSizes = new ScadaButton();
                        FloatPanelSizes.ItemID = ScadaItem.ItemID;
                        FloatPanelSizes.StyleId = ScadaItem.ItemID.ToString();
                        FloatPanelSizes.AnchorX = 0;
                        FloatPanelSizes.AnchorY = 0;
                        FloatPanelSizes.CornerRadius = 10;
                        FloatPanelSizes.BarBackgroundColor = ScadaColor.uxBackGroundColor;
                        FloatPanelSizes.BackgroundColor = ScadaColor.uxBackGroundColor.ToMauiColor();
                        FloatPanelSizes.GradientStartColor = ScadaColor.uxPopupColor;
                        FloatPanelSizes.GradientEndColor = ScadaColor.uxPopupColor;
                        FloatPanelSizes.IndicatorColor = ScadaColor.uxBackGroundColor;

                        FloatPanelSizes.WidthRequest = (Width / 100) * ScadaItem.Width;
                        FloatPanelSizes.HeightRequest = (Height / 100) * ScadaItem.Height;
                        FloatPanelSizes.AlternativeTextColor = ScadaColor.uxTextColor;
                        FloatPanelSizes.TextColor = ScadaColor.uxTextColor;
                        FloatPanelSizes.IsEnabled = true;
                        FloatPanelSizes.IsVisible = true;
                        FloatPanelSizes.IndicatorType = 10;
                        AbsoluteLayout.SetLayoutBounds(FloatPanelSizes, new Rect(
                            (Width / 100) * ScadaItem.Left,
                            (Height / 100) * ScadaItem.Top,
                            (Width / 100) * ScadaItem.Width,
                            (Height / 100) * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(FloatPanelSizes, AbsoluteLayoutFlags.None);
                        SKCanvasPopupViews.Add(FloatPanelSizes);

                        x = (Width / 100) * ScadaItem.Left;
                        y = (Height / 100) * ScadaItem.Top;
                        w = (Width / 100) * ScadaItem.Width;
                        h = (Height / 100) * ScadaItem.Height;

                        CreateCloseButton(x, y, w, h, ScadaGlobals.CurrentScadaPopup);
                        ScadaItem.gridRows = MyDataAccessLayer.GetItemSizes(ScadaGlobals.CurrentType);
                        y = y + FloatPanelSizes.CornerRadius + 50;

                        foreach (gridRow row in ScadaItem.gridRows)
                        {
                            var myItemTagsBtn = new ScadaButton
                            {
                                WidthRequest = (Width / 100) * ScadaItem.Width,
                                HeightRequest = 25,
                                Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
                                BarBackgroundColor = ScadaColor.uxBackGroundColor,
                                IndicatorType = 3,
                                IndicatorColor = GetStatusColor(row.Status),
                                GradientStartColor = ScadaColor.uxPopupItemColor,
                                GradientEndColor = ScadaColor.uxPopupItemColor,
                                CornerRadius = 0,
                                TextColor = ScadaColor.uxTextColor,
                                FontSize = 16.5F,
                                SvgBase64 = MyDataAccessLayer.LoadLibItem("InputSource", 1)

                            };

                            myItemTagsBtn.ButtonRow = row;
                            myItemTagsBtn.ButtonText = row.col1text;
                            myItemTagsBtn.EnableTouchEvents = true;
                            myItemTagsBtn.InputTransparent = false;
                            myItemTagsBtn.Touch += (sender, args) =>
                            {
                                var pt = args.Location;
                                switch (args.ActionType)
                                {
                                    case SKTouchAction.Released:
                                        ScadaClasses.Telegram oTelegram = new ScadaClasses.Telegram()
                                        {
                                            MessageType = ScadaItem.MessageType,
                                            Page = ScadaItem.Nextpage,
                                            ItemType = ScadaGlobals.CurrentType,
                                            ItemID = ScadaGlobals.CurrentItem,
                                            TagID = row.TagID,
                                            TagName = ScadaItem.TagName,
                                            size = row.Row,
                                            Action = ScadaItem.Action,
                                        };
                                        ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxTagsMenu;
                                        ScadaGlobals.Previouspage = -1;
                                        ScadaGlobals.CurrentRow = row.Id;
                                        MyDataAccessLayer.SetItemSize(oTelegram);
                                        //ScadaClasses.Refresh = true;
                                        break;

                                    case SKTouchAction.Moved:
                                        myItemTagsBtn.GradientStartColor = ScadaColor.uxHoverColor;
                                        myItemTagsBtn.GradientEndColor = ScadaColor.uxHoverColor;
                                        myItemTagsBtn.IndicatorColor = GetStatusColor(row.Status);
                                        break;

                                    case SKTouchAction.Exited:
                                        myItemTagsBtn.GradientStartColor = ScadaColor.uxPopupItemColor;
                                        myItemTagsBtn.GradientEndColor = ScadaColor.uxPopupItemColor;
                                        myItemTagsBtn.IndicatorColor = GetStatusColor(row.Status);
                                        break;
                                }
                                args.Handled = true;
                            };

                            AbsoluteLayout.SetLayoutBounds(myItemTagsBtn, new Rect(x, y, FloatPanelSizes.WidthRequest, 20));
                            AbsoluteLayout.SetLayoutFlags(myItemTagsBtn, AbsoluteLayoutFlags.None);
                            if (y < ((Height / 100) * ScadaItem.Top) + ((Height / 100) * ScadaItem.Height) - 26)
                            {
                                SKCanvasPopupViews.Add(myItemTagsBtn);
                                y = y + 26;
                            }
                        }*/
                        break;

                    case ScadaClasses.uxItemAction:
                        var FloatPanelAction = new ScadaButton();
                        FloatPanelAction.ItemID = ScadaItem.ItemID;
                        FloatPanelAction.StyleId = ScadaItem.ItemID.ToString();
                        FloatPanelAction.AnchorX = 0;
                        FloatPanelAction.AnchorY = 0;
                        FloatPanelAction.CornerRadius = 10;
                        FloatPanelAction.BarBackgroundColor = ScadaColor.uxPanelColor;
                        FloatPanelAction.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
                        FloatPanelAction.GradientStartColor = ScadaColor.uxPopupColor;
                        FloatPanelAction.GradientEndColor = ScadaColor.uxPopupColor;
                        FloatPanelAction.IndicatorColor = ScadaColor.uxPopupColor; ;

                        FloatPanelAction.WidthRequest = (Width / 100) * ScadaItem.Width;
                        FloatPanelAction.HeightRequest = (Height / 100) * ScadaItem.Height;
                        FloatPanelAction.AlternativeTextColor = ScadaColor.uxTextColor;
                        FloatPanelAction.TextColor = ScadaColor.uxTextColor;
                        FloatPanelAction.IsEnabled = true;
                        FloatPanelAction.IsVisible = true;
                        FloatPanelAction.IndicatorType = 10;
                        AbsoluteLayout.SetLayoutBounds(FloatPanelAction, new Rect(
                            (Width / 100) * ScadaItem.Left,
                            (Height / 100) * ScadaItem.Top,
                            (Width / 100) * ScadaItem.Width,
                            (Height / 100) * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(FloatPanelAction, AbsoluteLayoutFlags.None);
                        SKCanvasPopupViews.Add(FloatPanelAction);

                        x = (Width / 100) * ScadaItem.Left;
                        y = (Height / 100) * ScadaItem.Top;
                        w = (Width / 100) * ScadaItem.Width;
                        h = (Height / 100) * ScadaItem.Height;
                
                        y = y + FloatPanelAction.CornerRadius + 50;

                        var mySvgButton10 = new ScadaButton
                        {
                            WidthRequest = (Width / 100) * ScadaItem.Width,
                            HeightRequest = 25,
                            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
                            BarBackgroundColor = ScadaColor.uxBackGroundColor,
                            IndicatorType = 3,
                            GradientStartColor = ScadaColor.uxPopupItemColor,
                            GradientEndColor = ScadaColor.uxPopupItemColor,
                            CornerRadius = 0,
                            TextColor = ScadaColor.uxTextColor,
                            FontSize = 16.5F,
                            SvgBase64 = MyDataAccessLayer.LoadLibItem("InputSource", 1),
                            ButtonText = "On"
                        };
                        mySvgButton10.InputTransparent = false;
                        mySvgButton10.EnableTouchEvents = true;
                        mySvgButton10.Touch += (sender, args) =>
                        {
                            var pt = args.Location;
                            switch (args.ActionType)
                            {
                                case SKTouchAction.Released:
                                    /*ScadaClasses.CurrentScadaPopup = ScadaClasses.uxItemTagsMenu;
                                    ScadaClasses.Previouspage = -1;
                                    ScadaClasses.Refresh = true;*/
                            
                                    break;

                                case SKTouchAction.Moved:
                                    mySvgButton10.GradientStartColor = ScadaColor.uxHoverColor;
                                    mySvgButton10.GradientEndColor = ScadaColor.uxHoverColor;
                                    break;

                                case SKTouchAction.Exited:
                                    mySvgButton10.GradientStartColor = ScadaColor.uxPopupItemColor;
                                    mySvgButton10.GradientEndColor = ScadaColor.uxPopupItemColor;
                                    break;
                            }
                            args.Handled = true;
                        };
                        AbsoluteLayout.SetLayoutBounds(mySvgButton10, new Rect(x, y, FloatPanelAction.WidthRequest, 20));
                        AbsoluteLayout.SetLayoutFlags(mySvgButton10, AbsoluteLayoutFlags.None);
                        if (y < ((Height / 100) * ScadaItem.Top) + ((Height / 100) * ScadaItem.Height) - 26)
                        {
                            SKCanvasPopupViews.Add(mySvgButton10);
                            y = y + 26;
                        }

                        var mySvgButton20 = new ScadaButton
                        {
                            WidthRequest = (Width / 100) * ScadaItem.Width,
                            HeightRequest = 25,
                            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
                            BarBackgroundColor = ScadaColor.uxBackGroundColor,
                            IndicatorType = 3,

                            GradientStartColor = ScadaColor.uxPopupItemColor,
                            GradientEndColor = ScadaColor.uxPopupItemColor,
                            CornerRadius = 0,
                            TextColor = ScadaColor.uxTextColor,
                            FontSize = 16.5F,
                            SvgBase64 = MyDataAccessLayer.LoadLibItem("RobotArm", 1),
                            ButtonText = "Toggle"
                        };
                        mySvgButton20.InputTransparent = false;
                        mySvgButton20.EnableTouchEvents = true;
                        mySvgButton20.Touch += (sender, args) =>
                        {
                            var pt = args.Location;
                            switch (args.ActionType)
                            {
                                case SKTouchAction.Released:
                                    /*ScadaClasses.CurrentScadaPopup = ScadaClasses.uxItemTypeMenu;
                                    ScadaClasses.Previouspage = -1;
                                    ScadaClasses.Refresh = true;*/
                             
                                    break;

                                case SKTouchAction.Moved:
                                    mySvgButton20.GradientStartColor = ScadaColor.uxHoverColor;
                                    mySvgButton20.GradientEndColor = ScadaColor.uxHoverColor;
                                    break;

                                case SKTouchAction.Exited:
                                    mySvgButton20.GradientStartColor = ScadaColor.uxPopupItemColor;
                                    mySvgButton20.GradientEndColor = ScadaColor.uxPopupItemColor;
                                    break;
                            }
                            args.Handled = true;
                        };

                        AbsoluteLayout.SetLayoutBounds(mySvgButton20, new Rect(x, y, WidthRequest, 20));
                        AbsoluteLayout.SetLayoutFlags(mySvgButton20, AbsoluteLayoutFlags.None);
                        if (y < ((Height / 100) * ScadaItem.Top) + ((Height / 100) * ScadaItem.Height) - 26)
                        {
                            SKCanvasPopupViews.Add(mySvgButton20);
                            y = y + 26;
                        }

                        var mySvgButton30 = new ScadaButton
                        {
                            WidthRequest = (Width / 100) * ScadaItem.Width,
                            HeightRequest = 25,
                            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
                            BarBackgroundColor = ScadaColor.uxBackGroundColor,
                            IndicatorType = 3,

                            GradientStartColor = ScadaColor.uxPopupItemColor,
                            GradientEndColor = ScadaColor.uxPopupItemColor,
                            CornerRadius = 0,
                            TextColor = ScadaColor.uxTextColor,
                            FontSize = 16.5F,
                            SvgBase64 = MyDataAccessLayer.LoadLibItem("RobotArm", 1),
                            ButtonText = "Page switch"
                        };
                        mySvgButton30.InputTransparent = false;
                        mySvgButton30.EnableTouchEvents = true;
                        mySvgButton30.Touch += (sender, args) =>
                        {
                            var pt = args.Location;
                            switch (args.ActionType)
                            {
                                case SKTouchAction.Released:
                                    /*ScadaClasses.CurrentScadaPopup = ScadaClasses.uxPagesMenu;
                                    ScadaClasses.Previouspage = -1;
                                    ScadaClasses.Refresh = true;  */                          
                                    break;

                                case SKTouchAction.Moved:
                                    mySvgButton30.GradientStartColor = ScadaColor.uxHoverColor;
                                    mySvgButton30.GradientEndColor = ScadaColor.uxHoverColor;
                                    break;

                                case SKTouchAction.Exited:
                                    mySvgButton30.GradientStartColor = ScadaColor.uxPopupItemColor;
                                    mySvgButton30.GradientEndColor = ScadaColor.uxPopupItemColor;
                                    break;
                            }
                            args.Handled = true;
                        };

                        AbsoluteLayout.SetLayoutBounds(mySvgButton30, new Rect(x, y, WidthRequest, 20));
                        AbsoluteLayout.SetLayoutFlags(mySvgButton30, AbsoluteLayoutFlags.None);
                        if (y < ((Height / 100) * ScadaItem.Top) + ((Height / 100) * ScadaItem.Height) - 26)
                        {
                            SKCanvasPopupViews.Add(mySvgButton30);
                            y = y + 26;
                        }
                        break;

                    case ScadaClasses.uxEditTagText:
                        x = (Width / 100) * ScadaItem.Left;
                        y = (Height / 100) * ScadaItem.Top;
                        w = (Width / 100) * ScadaItem.Width;
                        h = (Height / 100) * ScadaItem.Height;
                        var EditPanel = new ScadaButton();
                        EditPanel.ItemID = ScadaItem.ItemID;
                        EditPanel.StyleId = ScadaItem.ItemID.ToString();
                        EditPanel.AnchorX = 0;
                        EditPanel.AnchorY = 0;
                        EditPanel.CornerRadius = 10;
                        EditPanel.BarBackgroundColor = ScadaColor.uxPanelColor;
                        EditPanel.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
                        EditPanel.GradientStartColor = ScadaColor.uxItemBackGroundColor;
                        EditPanel.GradientEndColor = ScadaColor.uxItemBackGroundColor;
                        EditPanel.IndicatorColor = ScadaColor.uxPanelColor;
                        EditPanel.WidthRequest = w;
                        EditPanel.HeightRequest = h;
                        EditPanel.AlternativeTextColor = ScadaColor.uxTextColor;
                        EditPanel.TextColor = ScadaColor.uxTextColor;
                        EditPanel.IsEnabled = true;
                        EditPanel.IsVisible = true;
                        EditPanel.IndicatorType = 0;

                        var btnApplyText = new ScadaButton();
                        btnApplyText.CornerRadius = 10;
                        btnApplyText.IndicatorType = 1;
                        btnApplyText.BarBackgroundColor = ScadaColor.uxPanelColor;
                        btnApplyText.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
                        btnApplyText.GradientStartColor = ScadaColor.uxPopupItemColor;
                        btnApplyText.GradientEndColor = ScadaColor.uxPopupItemColor;
                        btnApplyText.IndicatorColor = ScadaColor.uxItemColor;
                        btnApplyText.TextColor = ScadaColor.uxTextColor;
                        btnApplyText.ButtonText = "Apply";
                        btnApplyText.WidthRequest = 70;
                        btnApplyText.HeightRequest = 40;
                        btnApplyText.FontSize = 18;
                        btnApplyText.IsEnabled = true;
                        btnApplyText.IsVisible = true;
                        btnApplyText.EnableTouchEvents = true;
                        btnApplyText.InputTransparent = false;
                        btnApplyText.Touch += (sender, args) =>
                        {
                            var pt = args.Location;
                            switch (args.ActionType)
                            {
                                case SKTouchAction.Released:                                 
                                    switch (ScadaGlobals.CurrentRow)
                                    {
                                        case 0:
                                            MyDataAccessLayer.UpdateTagName(ScadaGlobals.CurrentTag, edtInputText.Text);
                                            break;
                                        case 1:
                                            MyDataAccessLayer.UpdateTagDescription(ScadaGlobals.CurrentTag, edtInputText.Text);
                                            break;
                                        case 2:
                                            MyDataAccessLayer.UpdateHL(ScadaGlobals.CurrentTag, edtInputText.Text);
                                            break;
                                        case 3:
                                            MyDataAccessLayer.UpdateLL(ScadaGlobals.CurrentTag, edtInputText.Text);
                                            break;
                                        case 4:
                                            MyDataAccessLayer.UpdateTagColor(ScadaGlobals.CurrentTag, edtInputText.Text);
                                            break;
                                        case 5:
                                            MyDataAccessLayer.UpdateTagUnit(ScadaGlobals.CurrentTag, edtInputText.Text);
                                            break;
                                        case 6:
                                            MyDataAccessLayer.UpdateStoreInterval(ScadaGlobals.CurrentTag, edtInputText.Text);
                                            break;
                                        case 7:
                                            MyDataAccessLayer.UpdateAlarmEnable(ScadaGlobals.CurrentTag, edtInputText.Text);
                                            break;
                                        case 8:
                                            MyDataAccessLayer.UpdateTagType(ScadaGlobals.CurrentTag, edtInputText.Text);
                                            break;
                                      
                                    }
                                    /*ScadaClasses.Previouspage = -1;
                                    ScadaClasses.CurrentScadaPopup = ScadaClasses.uxTagSettings;
                                    ScadaClasses.Refresh = true;*/
                                    break;

                                case SKTouchAction.Pressed:
                                    break;

                                case SKTouchAction.Moved:
                                    btnApplyText.GradientStartColor = ScadaColor.uxHoverColor;
                                    btnApplyText.GradientEndColor = ScadaColor.uxHoverColor;
                                    break;

                                case SKTouchAction.Exited:
                                    btnApplyText.GradientStartColor = ScadaColor.uxPopupItemColor;
                                    btnApplyText.GradientEndColor = ScadaColor.uxPopupItemColor;
                                    break;
                            }
                            args.Handled = true;
                        };

                        AbsoluteLayout.SetLayoutBounds(btnApplyText, new Rect(x + EditPanel.WidthRequest - (EditPanel.WidthRequest / 2) - (btnApplyText.WidthRequest / 2), y + EditPanel.HeightRequest - btnApplyText.HeightRequest * 1.5F, btnApplyText.WidthRequest, btnApplyText.HeightRequest));
                        AbsoluteLayout.SetLayoutFlags(btnApplyText, AbsoluteLayoutFlags.None);
                        AbsoluteLayout.SetLayoutBounds(EditPanel, new Rect(( Width / 100) * ScadaItem.Left,(Height / 100) * ScadaItem.Top,(Width / 100) * ScadaItem.Width,(Height / 100) * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(EditPanel, AbsoluteLayoutFlags.None);

                        SKCanvasPopupViews.Add(EditPanel);
                        SKCanvasPopupViews.Add(btnApplyText);
                        CreateCloseButton(x, y, w, h, ScadaClasses.uxTagSettings);
                        break;

                    case ScadaClasses.uxEditParameterText:
                      //  ScadaEditParameterText();
                        /*
                        x = (Width / 100) * ScadaItem.Left;
                        y = (Height / 100) * ScadaItem.Top;
                        w = (Width / 100) * ScadaItem.Width;
                        h = (Height / 100) * ScadaItem.Height;
                        var EditParamPanel = new ScadaButton();
                        EditParamPanel.ItemID = ScadaItem.ItemID;
                        EditParamPanel.StyleId = ScadaItem.ItemID.ToString();
                        EditParamPanel.AnchorX = 0;
                        EditParamPanel.AnchorY = 0;
                        EditParamPanel.CornerRadius = 10;
                        EditParamPanel.BarBackgroundColor = ScadaColor.uxPanelColor;
                        EditParamPanel.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
                        EditParamPanel.GradientStartColor = ScadaColor.uxItemBackGroundColor;
                        EditParamPanel.GradientEndColor = ScadaColor.uxItemBackGroundColor;
                        EditParamPanel.IndicatorColor = ScadaColor.uxPanelColor;
                        EditParamPanel.WidthRequest = w;
                        EditParamPanel.HeightRequest = h;
                        EditParamPanel.AlternativeTextColor = ScadaColor.uxTextColor;
                        EditParamPanel.TextColor = ScadaColor.uxTextColor;
                        EditParamPanel.IsEnabled = true;
                        EditParamPanel.IsVisible = true;
                        EditParamPanel.IndicatorType = 0;

                        var btnApplyParamText = new ScadaButton();
                        btnApplyParamText.CornerRadius = 10;
                        btnApplyParamText.IndicatorType = 1;
                        btnApplyParamText.BarBackgroundColor = ScadaColor.uxPanelColor;
                        btnApplyParamText.BackgroundColor = ScadaColor.uxPanelColor.ToMauiColor();
                        btnApplyParamText.GradientStartColor = ScadaColor.uxPopupItemColor;
                        btnApplyParamText.GradientEndColor = ScadaColor.uxPopupItemColor;
                        btnApplyParamText.IndicatorColor = ScadaColor.uxItemColor;
                        btnApplyParamText.TextColor = ScadaColor.uxTextColor;
                        btnApplyParamText.ButtonText = "Apply";
                        btnApplyParamText.WidthRequest = 70;
                        btnApplyParamText.HeightRequest = 40;
                        btnApplyParamText.FontSize = 18;
                        btnApplyParamText.IsEnabled = true;
                        btnApplyParamText.IsVisible = true;
                        btnApplyParamText.EnableTouchEvents = true;
                        btnApplyParamText.InputTransparent = false;
                        btnApplyParamText.Touch += (sender, args) =>
                        {
                            var pt = args.Location;
                            switch (args.ActionType)
                            {
                                case SKTouchAction.Released:
                                    switch (ScadaGlobals.CurrentType)
                                    {
                                        case ScadaClasses.pmComputer:
                                              Preferences.Default.Set("ConnectionString", edtInputText.Text);
                                            break;
                                        case ScadaClasses.pmParameter:
                                              MyDataAccessLayer.UpdateParameterValue(ScadaGlobals.CurrentID, edtInputText.Text);
                                            break;
                                        case ScadaClasses.pmPages:
                                              MyDataAccessLayer.UpdatePageName(ScadaGlobals.CurrentID, edtInputText.Text);
                                            ScadaGlobals.Currentpage = MyDataAccessLayer.GetPageNoByName(edtInputText.Text);
                                            break;
                                    }                            
                                    //ScadaClasses.Previouspage = -1;
                                    //ScadaClasses.CurrentScadaPopup = ScadaClasses.PreviousScadaPopup;
                                    //ScadaClasses.Refresh = true;
                                    break;

                                case SKTouchAction.Pressed:
                                    break;

                                case SKTouchAction.Moved:
                                    btnApplyParamText.GradientStartColor = ScadaColor.uxHoverColor;
                                    btnApplyParamText.GradientEndColor = ScadaColor.uxHoverColor;
                                    break;

                                case SKTouchAction.Exited:
                                    btnApplyParamText.GradientStartColor = ScadaColor.uxPopupItemColor;
                                    btnApplyParamText.GradientEndColor = ScadaColor.uxPopupItemColor;
                                    break;
                            }
                            args.Handled = true;
                        };

                        AbsoluteLayout.SetLayoutBounds(btnApplyParamText, new Rect(x + EditParamPanel.WidthRequest - (EditParamPanel.WidthRequest / 2) - (btnApplyParamText.WidthRequest / 2), y + EditParamPanel.HeightRequest - btnApplyParamText.HeightRequest * 1.5F, btnApplyParamText.WidthRequest, btnApplyParamText.HeightRequest));
                        AbsoluteLayout.SetLayoutFlags(btnApplyParamText, AbsoluteLayoutFlags.None);
                        AbsoluteLayout.SetLayoutBounds(EditParamPanel, new Rect((Width / 100) * ScadaItem.Left, (Height / 100) * ScadaItem.Top, (Width / 100) * ScadaItem.Width, (Height / 100) * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(EditParamPanel, AbsoluteLayoutFlags.None);

                        SKCanvasPopupViews.Add(EditParamPanel);
                        SKCanvasPopupViews.Add(btnApplyParamText);
                        CreateCloseButton(x, y, w, h, ScadaGlobals.PreviousScadaPopup);
                        break;
                        */

                        break;
                    case ScadaClasses.uxTagsMenu:
                      //  ScadaTagsMenu();

                      
                        break;

                    case ScadaClasses.uxTimeSpanMenu:
                        //ScadaTimeSpanMenu();

                        /*
                        var TimeSpanPanel = new ScadaButton();
                        TimeSpanPanel.ItemID = ScadaItem.ItemID;
                        TimeSpanPanel.StyleId = ScadaItem.ItemID.ToString();
                        TimeSpanPanel.AnchorX = 0;
                        TimeSpanPanel.AnchorY = 0;
                        TimeSpanPanel.CornerRadius = 10;
                        TimeSpanPanel.BarBackgroundColor = ScadaColor.uxBackGroundColor;
                        TimeSpanPanel.BackgroundColor = ScadaColor.uxBackGroundColor.ToMauiColor();
                        TimeSpanPanel.GradientStartColor = ScadaColor.uxItemColor;
                        TimeSpanPanel.GradientEndColor = ScadaColor.uxItemColor;
                        TimeSpanPanel.IndicatorColor = ScadaColor.uxBackGroundColor;
                        TimeSpanPanel.WidthRequest = (Width / 100) * ScadaItem.Width;
                        TimeSpanPanel.HeightRequest = (Height / 100) * ScadaItem.Height;
                        TimeSpanPanel.AlternativeTextColor = ScadaColor.uxTextColor;
                        TimeSpanPanel.TextColor = ScadaColor.uxTextColor;
                        TimeSpanPanel.IsEnabled = true;
                        TimeSpanPanel.IsVisible = true;
                        TimeSpanPanel.IndicatorType = 10;
                        AbsoluteLayout.SetLayoutBounds(TimeSpanPanel, new Rect(
                            (Width / 100) * ScadaItem.Left,
                            (Height / 100) * ScadaItem.Top,
                            (Width / 100) * ScadaItem.Width,
                            (Height / 100) * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(TimeSpanPanel, AbsoluteLayoutFlags.None);
                        SKCanvasViews.Add(TimeSpanPanel);

                        x = (Width / 100) * ScadaItem.Left;
                        y = (Height / 100) * ScadaItem.Top;

                        y = y + TimeSpanPanel.CornerRadius + 50;
                        ScadaItem.gridRows = MyDataAccessLayer.ReadTimeSpans();
                        foreach (gridRow row in ScadaItem.gridRows)
                        {
                            var myButton = new ScadaButton
                            {
                                WidthRequest = (Width / 100) * ScadaItem.Width,
                                HeightRequest = 25,
                                Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
                                BarBackgroundColor = ScadaColor.uxBackGroundColor,
                                IndicatorType = 3,
                                IndicatorColor = GetStatusColor(row.Status),
                                GradientStartColor = ScadaColor.uxLightColor,
                                GradientEndColor = ScadaColor.uxLightColor,
                                CornerRadius = 0,
                                TextColor = ScadaColor.uxTextColor,
                                FontSize = 16.5F,
                                SvgBase64 = MyDataAccessLayer.LoadLibItem("InputSource", 1)
                            };
                            myButton.ButtonRow = row;
                            myButton.ButtonText = row.col1text;
                            myButton.EnableTouchEvents = true;
                            myButton.InputTransparent = false;
                            myButton.Touch += (sender, args) =>
                            {
                                var pt = args.Location;
                                switch (args.ActionType)
                                {
                                    case SKTouchAction.Released:
                                        ScadaClasses.Telegram oTelegram = new ScadaClasses.Telegram()
                                        {
                                            MessageType = ScadaItem.MessageType,
                                            Page = ScadaItem.Nextpage,
                                            ItemType = ScadaItem.ItemType,
                                            ItemID = ScadaItem.ItemID,
                                            TagID = row.TagID,

                                            TagName = ScadaItem.TagName,
                                            Action = ScadaItem.Action,
                                        };
                                        var myChartSettings = MyDataAccessLayer.GetChartSettings(ScadaItem.ItemID);
                                        myChartSettings.iSpan = row.Id;
                                        MyDataAccessLayer.SetChartSettings(myChartSettings, ScadaItem.ItemID);

                                        ScadaClasses.CurrentScadaPopup = -1;
                                        ScadaClasses.Previouspage = -1;
                                        ScadaClasses.Refresh = true;
                                        break;

                                    case SKTouchAction.Moved:
                                        myButton.GradientStartColor = ScadaColor.uxHoverColor;
                                        myButton.GradientEndColor = ScadaColor.uxHoverColor;
                                        myButton.IndicatorColor = GetStatusColor(row.Status);
                                        break;

                                    case SKTouchAction.Exited:
                                        myButton.GradientStartColor = ScadaColor.uxLightColor;
                                        myButton.GradientEndColor = ScadaColor.uxLightColor;
                                        myButton.IndicatorColor = GetStatusColor(row.Status);
                                        break;
                                }
                                args.Handled = true;
                            };

                            AbsoluteLayout.SetLayoutBounds(myButton, new Rect(x, y, TimeSpanPanel.WidthRequest, 20));
                            AbsoluteLayout.SetLayoutFlags(myButton, AbsoluteLayoutFlags.None);
                            // if (yStart < ((Height / 100) * double.Parse(ScadaItem.Top, CultureInfo.InvariantCulture)) + (Height / 100) * double.Parse(ScadaItem.Height, CultureInfo.InvariantCulture) - 26)
                            {
                                SKCanvasViews.Add(myButton);
                                y = y + 26;
                            }
                        }
                        */
                        break;

                    case ScadaClasses.uxItemTypeMenu:
                        //ScadaItemTypesMenu();

                        /*
                        var FloatPanel3 = new ScadaButton();
                        FloatPanel3.ItemID = ScadaItem.ItemID;
                        FloatPanel3.StyleId = ScadaItem.ItemID.ToString();
                        FloatPanel3.AnchorX = 0;
                        FloatPanel3.AnchorY = 0;
                        FloatPanel3.CornerRadius = 10;
                        FloatPanel3.BarBackgroundColor = ScadaColor.uxBackGroundColor;
                        FloatPanel3.BackgroundColor = ScadaColor.uxBackGroundColor.ToMauiColor();
                        FloatPanel3.GradientStartColor = ScadaColor.uxPopupColor;
                        FloatPanel3.GradientEndColor = ScadaColor.uxPopupColor;
                        FloatPanel3.IndicatorColor = ScadaColor.uxBackGroundColor;

                        FloatPanel3.WidthRequest = (Width / 100) * ScadaItem.Width;
                        FloatPanel3.HeightRequest = (Height / 100) * ScadaItem.Height;
                        FloatPanel3.AlternativeTextColor = ScadaColor.uxTextColor;
                        FloatPanel3.TextColor = ScadaColor.uxTextColor;
                        FloatPanel3.IsEnabled = true;
                        FloatPanel3.IsVisible = true;
                        FloatPanel3.IndicatorType = 10;
                        AbsoluteLayout.SetLayoutBounds(FloatPanel3, new Rect(
                            (Width / 100) * ScadaItem.Left,
                            (Height / 100) *ScadaItem.Top,
                            (Width / 100) * ScadaItem.Width,
                            (Height / 100) * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(FloatPanel3, AbsoluteLayoutFlags.None);
                        SKCanvasPopupViews.Add(FloatPanel3);


                        x = (Width / 100) * ScadaItem.Left;
                        y = (Height / 100) * ScadaItem.Top;
                        w = (Width / 100) * ScadaItem.Width;
                        h = (Height / 100) * ScadaItem.Height;                       
                        ScadaItem.gridRows = MyDataAccessLayer.ReadItemTypes(sFilter, iMenuOffsetRows, iMenuOffsetRows + 5);

                        //Next and Previous buttons 
                        CreateCloseButton(x, y, w, h, ScadaGlobals.CurrentScadaPopup);
                        CreateFwdButton(x, y, w, h);
                        CreateRwdButton(x, y, w, h);

                        y = y + FloatPanel3.CornerRadius + 50;
                        foreach (gridRow row in ScadaItem.gridRows)
                        {
                            var myButton = new ScadaButton
                            {
                                WidthRequest = (Width / 100) * ScadaItem.Width,
                                HeightRequest = 25,
                                Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
                                BarBackgroundColor = ScadaColor.uxBackGroundColor,
                                IndicatorType = 3,
                                IndicatorColor = GetStatusColor(row.Status),
                                GradientStartColor = ScadaColor.uxPopupItemColor,
                                GradientEndColor = ScadaColor.uxPopupItemColor,
                                CornerRadius = 0,
                                TextColor = ScadaColor.uxTextColor,
                                FontSize = 16.5F,
                                SvgBase64 = MyDataAccessLayer.LoadLibItem("Compass", 1)
                            };

                            myButton.ButtonRow = row;
                            myButton.ButtonText = row.col1text;
                            myButton.EnableTouchEvents = true;
                            myButton.InputTransparent = false;

                            myButton.Touch += (sender, args) =>
                            {
                                var pt = args.Location;
                                switch (args.ActionType)
                                {
                                    case SKTouchAction.Released:
                                        ScadaClasses.Telegram oTelegram = new ScadaClasses.Telegram()
                                        {
                                            MessageType = ScadaItem.MessageType,
                                            Page = ScadaItem.Nextpage,
                                            ItemType = row.TagID,
                                            ItemID = ScadaGlobals.CurrentItem,
                                            TagID = row.TagID,
                                            TagName = ScadaItem.TagName,
                                            Action = ScadaItem.Action,
                                        };
                                        MyDataAccessLayer.SetItemType(oTelegram);
                                        MyDataAccessLayer.AddItemDefaultTags(oTelegram);

                                        if (oTelegram.ItemType == ScadaClasses.uxButton)
                                        {
                                            ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxItemAction;
                                        }
                                        else
                                        {
                                            ScadaGlobals.CurrentScadaPopup = -1;
                                        }

                                        //ScadaClasses.Previouspage = -1;
                                        //ScadaClasses.Refresh = true;
                                        break;

                                    case SKTouchAction.Moved:
                                        myButton.GradientStartColor = ScadaColor.uxHoverColor;
                                        myButton.GradientEndColor = ScadaColor.uxHoverColor;
                                        myButton.IndicatorColor = GetStatusColor(row.Status);
                                        break;

                                    case SKTouchAction.Exited:
                                        myButton.GradientStartColor = ScadaColor.uxPopupItemColor;
                                        myButton.GradientEndColor = ScadaColor.uxPopupItemColor;
                                        myButton.IndicatorColor = GetStatusColor(row.Status);
                                        break;
                                }
                                args.Handled = true;
                            };
                            AbsoluteLayout.SetLayoutBounds(myButton, new Rect(x, y, WidthRequest, 20));
                            AbsoluteLayout.SetLayoutFlags(myButton, AbsoluteLayoutFlags.None);
                            if (y < ((Height / 100) * ScadaItem.Top) + ((Height / 100) * ScadaItem.Height) - 26)
                            {
                                SKCanvasPopupViews.Add(myButton);
                                y = y + 26;
                            }
                         
                        }
                        */
                        break;

                        case ScadaClasses.uxPagesMenu:
                        ScadaPages();
                       
                        break;

                    case ScadaClasses.uxPageChangeMenu:
                        var FloatPanelChangePage = new ScadaButton();
                        FloatPanelChangePage.ItemID = ScadaItem.ItemID;
                        FloatPanelChangePage.StyleId = ScadaItem.ItemID.ToString();
                        FloatPanelChangePage.AnchorX = 0;
                        FloatPanelChangePage.AnchorY = 0;
                        FloatPanelChangePage.CornerRadius = 10;
                        FloatPanelChangePage.BarBackgroundColor = ScadaColor.uxBackGroundColor;
                        FloatPanelChangePage.BackgroundColor = ScadaColor.uxBackGroundColor.ToMauiColor();
                        FloatPanelChangePage.GradientStartColor = ScadaColor.uxPopupColor;
                        FloatPanelChangePage.GradientEndColor = ScadaColor.uxPopupColor;
                        FloatPanelChangePage.IndicatorColor = ScadaColor.uxBackGroundColor;

                        FloatPanelChangePage.WidthRequest = (Width / 100) * ScadaItem.Width;
                        FloatPanelChangePage.HeightRequest = (Height / 100) * ScadaItem.Height;
                        FloatPanelChangePage.AlternativeTextColor = ScadaColor.uxTextColor;
                        FloatPanelChangePage.TextColor = ScadaColor.uxTextColor;
                        FloatPanelChangePage.IsEnabled = true;
                        FloatPanelChangePage.IsVisible = true;
                        FloatPanelChangePage.IndicatorType = 10;
                        AbsoluteLayout.SetLayoutBounds(FloatPanelChangePage, new Rect(
                            (Width / 100) *ScadaItem.Left,
                            (Height / 100) * ScadaItem.Top,
                            (Width / 100) * ScadaItem.Width,
                            (Height / 100) * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(FloatPanelChangePage, AbsoluteLayoutFlags.None);
                        SKCanvasPopupViews.Add(FloatPanelChangePage);

                        x = (Width / 100) * ScadaItem.Left;
                        y = (Height / 100) * ScadaItem.Top;
                        w = (Width / 100) * ScadaItem.Width;
                        h = (Height / 100) * ScadaItem.Height;

                        var gridRows1 = new List<gridRow>();
                        int Page1 = 1;
                        int MaxPage1 = MyDataAccessLayer.GetMaxPageNo();
                        while (Page1 < MaxPage1)
                        {
                            gridRows1.Add(new gridRow
                            {
                                TagID = Page1,
                                col1text = Page1.ToString(),
                                col1width = 100F,
                                col2text = "",
                                col2width = 10F,
                                col3text = "",
                                col3width = 10F,
                                col4text = "",
                                col4width = 10F,
                                col5text = "",
                                col5width = 10F,
                                col6text = "",
                                col6width = 10F,
                                Status = 5,
                                Id = 0
                            });
                            Page1++;
                        }
                        ScadaItem.gridRows = gridRows1;

                        CreateCloseButton(x, y, w, h, ScadaGlobals.CurrentScadaPopup);
                        CreateFwdButton(x, y, w, h);
                        CreateRwdButton(x, y, w, h);

                        y = y + FloatPanelChangePage.CornerRadius + 50;

                        foreach (gridRow row in ScadaItem.gridRows)
                        {
                            var myButton = new ScadaButton
                            {
                                WidthRequest = (Width / 100) * ScadaItem.Width,
                                HeightRequest = 25,
                                Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
                                BarBackgroundColor = ScadaColor.uxBackGroundColor,
                                IndicatorType = 3,
                                IndicatorColor = GetStatusColor(row.Status),
                                GradientStartColor = ScadaColor.uxPopupItemColor,
                                GradientEndColor = ScadaColor.uxPopupItemColor,
                                CornerRadius = 0,
                                TextColor = ScadaColor.uxTextColor,
                                FontSize = 16.5F,
                                SvgBase64 = MyDataAccessLayer.LoadLibItem("Compass", 1),
                            };
                            myButton.ButtonRow = row;
                            myButton.ButtonText = row.col1text;
                            myButton.EnableTouchEvents = true;
                            myButton.InputTransparent = false;
                            myButton.Touch += (sender, args) =>
                            {
                                var pt = args.Location;
                                switch (args.ActionType)
                                {
                                    case SKTouchAction.Released:
                                        ScadaClasses.Telegram oTelegram = new ScadaClasses.Telegram()
                                        {
                                            MessageType = ScadaItem.MessageType,
                                            Page = ScadaItem.Nextpage,
                                            ItemType = row.TagID,
                                            ItemID = ScadaGlobals.CurrentItem,
                                            TagID = row.TagID,
                                            TagName = ScadaItem.TagName,
                                            Action = ScadaItem.Action,
                                        };
                                        ScadaGlobals.Currentpage = row.TagID;
                                        MyDataAccessLayer.SetItemPage(ScadaGlobals.CurrentItem, ScadaGlobals.Currentpage);
                                        ScadaGlobals.CurrentScadaPopup = -1;
                                        //ScadaClasses.Refresh = true;
                                        break;

                                    case SKTouchAction.Moved:
                                        myButton.GradientStartColor = ScadaColor.uxHoverColor;
                                        myButton.GradientEndColor = ScadaColor.uxHoverColor;
                                        myButton.IndicatorColor = GetStatusColor(row.Status);
                                        break;

                                    case SKTouchAction.Exited:
                                        myButton.GradientStartColor = ScadaColor.uxPopupItemColor;
                                        myButton.GradientEndColor = ScadaColor.uxPopupItemColor;
                                        myButton.IndicatorColor = GetStatusColor(row.Status);
                                        break;
                                }
                                args.Handled = true;
                            };
                            AbsoluteLayout.SetLayoutBounds(myButton, new Rect(x, y, WidthRequest, 20));
                            AbsoluteLayout.SetLayoutFlags(myButton, AbsoluteLayoutFlags.None);
                            if (y < ((Height / 100) * ScadaItem.Top + (Height / 100) * ScadaItem.Height) - 26)
                            {
                                SKCanvasPopupViews.Add(myButton);
                                y = y + 26;
                            }
                        }
                        break;
                  
             }


            //Home (house)
            var btnHome = new ScadaButton();
            btnHome.GradientStartColor = ScadaColor.uxItemColor;
            btnHome.GradientEndColor = ScadaColor.uxItemColor;
            btnHome.CornerRadius = 15;
            btnHome.ItemID = 1;
            btnHome.EnableTouchEvents = true;
            btnHome.InputTransparent = false;
            btnHome.HeightRequest = 25;
            btnHome.WidthRequest = 25;
            btnHome.SvgBase64 = MyDataAccessLayer.LoadLibItem("Home", 1);
            btnHome.IndicatorType = 9;
            btnHome.Margin = 0.15F;
            btnHome.ButtonText = "";
            btnHome.Touch += (sender, args) =>
            {
                switch (args.ActionType)
                {
                    case SKTouchAction.Released:
                        //ScadaGlobals.Previouspage = -1;
                        ScadaGlobals.Currentpage = 1;
                        ScadaGlobals.CurrentScadaPopup = -1;
                        //ScadaClasses.Refresh = true;
                        RefreshGui();
                        break;

                    case SKTouchAction.Pressed:
                        break;

                    case SKTouchAction.Moved:
                        btnHome.GradientStartColor = ScadaColor.uxHoverColor;
                        btnHome.GradientEndColor = ScadaColor.uxHoverColor;
                        btnHome.IndicatorColor = ScadaColor.uxHoverColor;
                        break;

                    case SKTouchAction.Exited:
                        btnHome.GradientStartColor = ScadaColor.uxItemColor;
                        btnHome.GradientEndColor = ScadaColor.uxItemColor;
                        btnHome.IndicatorColor = ScadaColor.uxItemColor;
                        break;
                }
                args.Handled = true;
            };
            AbsoluteLayout.SetLayoutBounds(btnHome, new Rect(20 + btnHome.WidthRequest / 5, 20 + btnHome.HeightRequest / 5, 25, 25));
            AbsoluteLayout.SetLayoutFlags(btnHome, AbsoluteLayoutFlags.None);
            SKCanvasViews.Add(btnHome);

            //Alarm bell
            var btnAlarmbell = new ScadaButton();
            btnAlarmbell.GradientStartColor = ScadaColor.uxItemColor;
            btnAlarmbell.GradientEndColor = ScadaColor.uxItemColor;
            btnAlarmbell.CornerRadius = 15;
            btnAlarmbell.ItemID = 1;
            btnAlarmbell.EnableTouchEvents = true;
            btnAlarmbell.InputTransparent = false;
            btnAlarmbell.HeightRequest = 25;
            btnAlarmbell.WidthRequest = 25;
            btnAlarmbell.SvgBase64 = MyDataAccessLayer.LoadLibItem("Alarmbell", 1);
            btnAlarmbell.IndicatorType = 9;
            btnAlarmbell.Margin = 0.15F;
            btnAlarmbell.ButtonText = "";
            btnAlarmbell.Touch += (sender, args) =>
            {
                switch (args.ActionType)
                {
                    case SKTouchAction.Released:
                        //ScadaClasses.Previouspage = -1;
                        ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxAlarmGrid;
                        ScadaGlobals.PreviousScadaPopup = -1;
                        //ScadaClasses.Refresh = true;
                        /*
                        SKCanvasPopupViews.Clear();
                        ScadaAlarmGrid();
                        foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                        {
                            absoluteLayout.Add(viewItem);
                        }
                        Content = absoluteLayout;
                        */
                        RefreshGui();
                        break;

                    case SKTouchAction.Pressed:
                        break;

                    case SKTouchAction.Moved:
                        btnAlarmbell.GradientStartColor = ScadaColor.uxHoverColor;
                        btnAlarmbell.GradientEndColor = ScadaColor.uxHoverColor;
                        btnAlarmbell.IndicatorColor = ScadaColor.uxHoverColor;
                        break;

                    case SKTouchAction.Exited:
                        btnAlarmbell.GradientStartColor = ScadaColor.uxItemColor;
                        btnAlarmbell.GradientEndColor = ScadaColor.uxItemColor;
                        btnAlarmbell.IndicatorColor = ScadaColor.uxItemColor;
                        break;
                }
                args.Handled = true;
            };
            AbsoluteLayout.SetLayoutBounds(btnAlarmbell, new Rect(20 + btnAlarmbell.WidthRequest / 5, 60 + btnAlarmbell.HeightRequest / 5, 25, 25));
            AbsoluteLayout.SetLayoutFlags(btnAlarmbell, AbsoluteLayoutFlags.None);
            SKCanvasViews.Add(btnAlarmbell);

            //Settings Cogwheel
            var btnSettings = new ScadaButton();
            btnSettings.GradientStartColor = ScadaColor.uxItemColor;
            btnSettings.GradientEndColor = ScadaColor.uxItemColor;
            btnSettings.CornerRadius = 15;
            btnSettings.ItemID = 1;
            btnSettings.EnableTouchEvents = true;
            btnSettings.InputTransparent = false;
            btnSettings.HeightRequest = 25;
            btnSettings.WidthRequest = 25;
            btnSettings.SvgBase64 = MyDataAccessLayer.LoadLibItem("CogWheel", 1);
            btnSettings.IndicatorType = 9;
            btnSettings.Margin = 0.15F;
            btnSettings.ButtonText = "";
            btnSettings.Touch += (sender, args) =>
            {
                switch (args.ActionType)
                {
                    case SKTouchAction.Released:                     
                        ScadaGlobals.CurrentScadaPopup = ScadaClasses.uxParameters;
                        ScadaGlobals.PreviousScadaPopup = -1;
                        uxtimer.Interval = 50;
                        /*
                         SKCanvasPopupViews.Clear();
                         ScadaParameters();
                         foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                         {
                             absoluteLayout.Add(viewItem);
                         }
                         Content = absoluteLayout;
                         */
                        //RefreshGui();
                        break;

                    case SKTouchAction.Pressed:
                        break;

                    case SKTouchAction.Moved:
                        btnSettings.GradientStartColor = ScadaColor.uxHoverColor;
                        btnSettings.GradientEndColor = ScadaColor.uxHoverColor;
                        btnSettings.IndicatorColor = ScadaColor.uxHoverColor;
                        break;

                    case SKTouchAction.Exited:
                        btnSettings.GradientStartColor = ScadaColor.uxItemColor;
                        btnSettings.GradientEndColor = ScadaColor.uxItemColor;
                        btnSettings.IndicatorColor = ScadaColor.uxItemColor;
                        break;
                }
                args.Handled = true;
            };
            AbsoluteLayout.SetLayoutBounds(btnSettings, new Rect(20 + btnSettings.WidthRequest / 5, Height - 50 + btnSettings.HeightRequest / 5, 25, 25));
            AbsoluteLayout.SetLayoutFlags(btnSettings, AbsoluteLayoutFlags.None);
            SKCanvasViews.Add(btnSettings);


            if (ScadaGlobals.CurrentScadaPopup == ScadaClasses.uxLoginMenu)
            {                  
                ScadaLogin(0);
                ScadaGlobals.CurrentScadaPopup = -1;
            }

            absoluteLayout.Clear();
            foreach (SKCanvasView SKItem in SKCanvasViews)
            {
                absoluteLayout.Add(SKItem);
            }
            /*
            foreach (ContentView CoItem in ContentViews)
            {
                absoluteLayout.Add(CoItem);
            }
            */
            foreach (SKCanvasView viewItem in SKCanvasPopupViews)
            {
                absoluteLayout.Add(viewItem);
            }

            //Content = absoluteLayout;
            /*
            Thread.Sleep(1000);
            foreach (SKCanvasView viewItem in SKCanvasPopupViews)
            {
                absoluteLayout.Remove(viewItem);
            }
            */

            /*
            var editor = new SkiaEditor
            {
                HorizontalOptions = LayoutOptions.Fill,
                MaxLines = 1,
                FontSize = 16,
                TextColor = Colors.Black,
                CursorColor = Colors.DodgerBlue,
                BackgroundColor = Color.Parse("#F5F5F5"),
                Padding = new Thickness(12, 8),
                ReturnType = ReturnType.Done,
                KeyboardType = SkiaEditor.SkiaEditorKeyboard.Default,
            };
            editor.TextSubmitted += (s, text) => Console.WriteLine(text);
            */


            /*
            if (ScadaGlobals.CurrentScadaPopup == ScadaClasses.uxUploadMenu)
            {
                double panelWith = 0.4;
                double panelHeight = 0.55;
                x = (Width / 2) - panelWith * Width / 2;
                y = (Height / 2) - panelHeight * Height / 2;
                w = (panelWith * Width);
                h = (panelHeight * Height);

                edtSvgName.Placeholder = "SvgName";
                edtSvgName.WidthRequest = 200;
                edtSvgName.HeightRequest = 30;
                edtSvgName.TextColor = ScadaColor.uxTextColor.ToMauiColor();
                edtSvgName.BackgroundColor = ScadaColor.uxPopupItemColor.ToMauiColor();
                edtSvgName.PlaceholderColor = ScadaColor.uxPopupItemColor.ToMauiColor();
                edtSvgName.FontAttributes = FontAttributes.None;
                edtSvgName.IsVisible = true;
                AbsoluteLayout.SetLayoutBounds(edtSvgName, new Rect(400, 200, edtSvgName.WidthRequest, edtSvgName.HeightRequest));
                AbsoluteLayout.SetLayoutFlags(edtSvgName, AbsoluteLayoutFlags.None);
                edtSvgName.TextChanged += OnEditorTextChanged;
                edtSvgName.Completed += OnEditorCompleted;
                absoluteLayout.Add(edtSvgName);
    
                edtSvgEditor.Placeholder = "Paste your SVG text";
                edtSvgEditor.WidthRequest = 400;
                edtSvgEditor.HeightRequest = 200;
                edtSvgEditor.TextColor = ScadaColor.uxTextColor.ToMauiColor();
                edtSvgEditor.BackgroundColor = ScadaColor.uxPopupItemColor.ToMauiColor();
                edtSvgEditor.PlaceholderColor = ScadaColor.uxItemColor.ToMauiColor();
                edtSvgEditor.FontAttributes = FontAttributes.None;
                edtSvgEditor.IsVisible = true;
                AbsoluteLayout.SetLayoutBounds(edtSvgEditor, new Rect(400, 300, edtSvgEditor.WidthRequest, edtSvgEditor.HeightRequest));
                AbsoluteLayout.SetLayoutFlags(edtSvgEditor, AbsoluteLayoutFlags.None);
                edtSvgEditor.TextChanged += OnEditorTextChanged;
                edtSvgEditor.Completed += OnEditorCompleted;
                absoluteLayout.Add(edtSvgEditor);
            }
            */
            /*
            if ((ScadaGlobals.CurrentScadaPopup == ScadaClasses.uxEditTagText) ||
                (ScadaGlobals.CurrentScadaPopup == ScadaClasses.uxEditParameterText))
            {  
                edtInputText.Placeholder = "EditText";
                edtInputText.WidthRequest = 400;
                edtInputText.HeightRequest = 100;
                edtInputText.TextColor = ScadaColor.uxTextColor.ToMauiColor();
                edtInputText.BackgroundColor = ScadaColor.uxPopupItemColor.ToMauiColor();
                edtInputText.PlaceholderColor = ScadaColor.uxItemColor.ToMauiColor();
                edtInputText.FontAttributes = FontAttributes.Bold;
                edtInputText.IsVisible = true;
                AbsoluteLayout.SetLayoutBounds(edtInputText, new Rect(400, 200, 400,100));
                AbsoluteLayout.SetLayoutFlags(edtInputText, AbsoluteLayoutFlags.None);
                edtInputText.TextChanged += OnEditorTextChanged;
                edtInputText.Completed += OnEditorCompleted;
                absoluteLayout.Add(edtInputText);        
            }*/



            Content = absoluteLayout;
            ScadaGlobals.Previouspage = ScadaGlobals.Currentpage;       
            
        }
        

        if ((ScadaGlobals.CurrentScadaPopup != -1)&&(ScadaGlobals.CurrentScadaPopup != ScadaGlobals.PreviousScadaPopup))
        {          
       
                switch (ScadaGlobals.CurrentScadaPopup)
                {
                    case ScadaClasses.uxAlarmGrid:
                        ScadaAlarmGrid();
                    break;
                    case ScadaClasses.uxParameters:
                        ScadaParameters();
                    break;
                    case ScadaClasses.uxPagesMenu:
                        ScadaPages();
                        break;
                    case ScadaClasses.uxProtocolSettings:
                        ScadaProtocolSettings();
                        break;
                    case ScadaClasses.uxProtocols:
                        ScadaProtocols();
                        break;
                    case ScadaClasses.uxTagsMenu:
                        ScadaTagsMenu();
                        break;
                    case ScadaClasses.uxTagsGrid:
                        ScadaTagsGrid();
                        break;
                    case ScadaClasses.uxTagSettings:
                        ScadaTagSettings();
                    break;
                    case ScadaClasses.uxUploadMenu:
                        ScadaUpload();
                    break;
                     case ScadaClasses.uxEditParameterText:
                      //  ScadaEditParameterText();
                    break;


            }
            foreach (SKCanvasView viewItem in SKCanvasPopupViews)
                {
                    absoluteLayout.Add(viewItem);
                }             
                ScadaGlobals.PreviousScadaPopup = ScadaGlobals.CurrentScadaPopup;              
                Content = absoluteLayout;
           
        }
       
        

        foreach (SKCanvasView uxItem in SKCanvasViews)
        {/*
            if (uxItem is ScadaSvg)
            {
                var dItem = uxItem as ScadaSvg;
                AbsoluteLayout.SetLayoutBounds(dItem, new Rect((Width / 100) * dItem.XPosition,(Height / 100) * dItem.YPosition,(Width / 100) * dItem.Width, (Height / 100) * dItem.Height));              
            }
            */
            if (uxItem is HistGraph)
            {
                var dItem = uxItem as HistGraph;
                foreach (var ScadaItem in ScadaItems)
                {
                    if (dItem != null)
                    {
                        if (dItem.ItemID == ScadaItem.ItemID)
                        {
                            dItem.ListOfValues = ScadaItem.ListOfValues;
                        }
                    }
                }
                if (dItem.IsLoaded == true)
                {
                    dItem.InvalidateSurface();
                }
            }
            
            
            if (uxItem is Controller)
            {
                var dItem = uxItem as Controller;
                var ItemValues = MyDataAccessLayer.ReadItemValues(dItem.ItemID);
                foreach (var item in ItemValues)
                {
                    if (item.TagID == dItem.PV.TagID)
                    {
                        dItem.PV.Value = item.Value;
                    }
                    if (item.TagID == dItem.SV.TagID)
                    {
                        dItem.SV.Value = item.Value;
                    }
                    if (item.TagID == dItem.Output.TagID)
                    {
                        dItem.Output.Value = item.Value;
                    }
                }
                if (dItem.IsLoaded)
                {
                    dItem.InvalidateSurface();
                }
            }
            

            if (uxItem is Simulator)
            {
                var dItem = uxItem as Simulator;
                var ItemValues = MyDataAccessLayer.ReadItemValues(dItem.ItemID);
                foreach (var item in ItemValues)
                {
                    if (item.TagID == dItem.PV.TagID)
                    {
                        dItem.PV.Value = item.Value;
                    }
                    if (item.TagID == dItem.Acutator.TagID)
                    {
                        dItem.Acutator.Value = item.Value;
                    }
                }
                if (dItem.IsLoaded)
                {
                    dItem.InvalidateSurface();
                }
            }
           
            
            if (uxItem is ScadaNumeric)
            {
                var dItem = uxItem as ScadaNumeric;
                foreach (var Item in ScadaItems)
                {
                    if (dItem.ItemID == Item.ItemID)
                    {
                       dItem.RefreshValues(Item.ItemValues);                    
                    }
                }
            }


            if (uxItem is Toggle)
            {
                var dItem = uxItem as Toggle;
                  dItem.RefreshValues();
            }

            if (uxItem is ScadaRobot)
            {
                var dItem = uxItem as ScadaRobot;
                if (Designing == false)
                {
                    dItem.RefreshValues();
                }
                /*
                foreach (var Item in ScadaItems)
                {
                    if (dItem.ItemID == Item.ItemID)
                    {
                        dItem.RefreshValues();
                       // dItem.RefreshValues(Item.ItemValues);
                    }
                }
                */
            }



            if (uxItem is CircularProgress)
            {
                var dItem = uxItem as CircularProgress;
                dItem.RefreshValues();
            }
            if (uxItem is CircularGauge)
            {
                var dItem = uxItem as CircularGauge;
                dItem.RefreshValues();
            }

        

            /*
            if (uxItem is CircularGauge)
            {
                var dItem = uxItem as CircularGauge;
               
                var ItemValues = MyDataAccessLayer.ReadItemValues(dItem.ItemID);
                foreach (var item in ItemValues)
                {
                    if (item.TagID == dItem.PV.TagID)
                    {
                        dItem.PV.Value = item.Value;
                    }
                    if ((item.TagID == dItem.SV.TagID) && (dItem.State == 2))
                    {
                        dItem.SV.Value = item.Value;
                    }
                }
                if (dItem.IsLoaded == true)
                {
                    dItem.InvalidateSurface();
                }
                dItem = null;
            }
            */

            /*
            if (uxItem is Compass)
            {
                var dItem = uxItem as Compass;
                var ItemValues = MyDataAccessLayer.ReadItemValues(dItem.ItemID);
                foreach (var item in ItemValues)
                {
                    if (item.TagID == dItem.PV.TagID)
                    {
                        dItem.PV.Value = item.Value;
                    }
                    if (item.TagID == dItem.SV.TagID)
                    {
                        dItem.SV.Value = item.Value;
                    }
                }
                dItem.InvalidateSurface();
            }

            if (uxItem is Gyroscope)
            {
                var dItem = uxItem as Gyroscope;
                var ItemValues = MyDataAccessLayer.ReadItemValues(dItem.ItemID);
                foreach (var item in ItemValues)
                {
                    if (item.TagID == dItem.BankAngle.TagID)
                    {
                        dItem.BankAngle.Value = item.Value;
                    }
                }
                dItem.InvalidateSurface();
            }

            if (uxItem is Altimeter)
            {
                var dItem = uxItem as Altimeter;
                var ItemValues = MyDataAccessLayer.ReadItemValues(dItem.ItemID);
                foreach (var item in ItemValues)
                {
                    if (item.TagID == dItem.PV.TagID)
                    {
                        dItem.PV.Value = item.Value;
                    }
                    if (item.TagID == dItem.SV.TagID)
                    {
                        dItem.SV.Value = item.Value;
                    }
                }
                dItem.InvalidateSurface();
                dItem = null;
            }

            if (uxItem is Airspeed)
            {
                var dItem = uxItem as Airspeed;
                var ItemValues = MyDataAccessLayer.ReadItemValues(dItem.ItemID);
                foreach (var item in ItemValues)
                {
                    if (item.TagID == dItem.PV.TagID)
                    {
                        dItem.PV.Value = item.Value;
                    }
                    if (item.TagID == dItem.SV.TagID)
                    {
                        dItem.SV.Value = item.Value;
                    }
                }
                dItem.InvalidateSurface();
                dItem = null;
            }
            */

            
            if (uxItem is ScadaRobot)
            {
                var dItem = uxItem as ScadaRobot;
                var ItemValues = MyDataAccessLayer.ReadItemValues(dItem.ItemID);

                if (Designing == false)
                {
                    foreach (var item in ItemValues)
                    {
                        if (item.TagID == dItem.Gripper.TagID)
                        {
                            dItem.Gripper.Value = item.Value;
                        }
                        if (item.TagID == dItem.LowerArm.TagID)
                        {
                            dItem.LowerArm.Value = item.Value;
                        }
                        if (item.TagID == dItem.UpperArm.TagID)
                        {
                            dItem.UpperArm.Value = item.Value;
                        }
                        if (item.TagID == dItem.XTraverse.TagID)
                        {
                            dItem.XTraverse.Value = item.Value;
                        }
                        if (item.TagID == dItem.YTraverse.TagID)
                        {
                            dItem.YTraverse.Value = item.Value;
                        }
                    }
                    dItem.InvalidateSurface();
                }
            }
            




            if (uxItem is ScadaButton)
            {
                var dItem = uxItem as ScadaButton;
               /*
                foreach (var Item in ScadaItems)
                {
                    if (dItem.ItemID == Item.ItemID)
                    {
                        dItem.RefreshValues(Item.ItemValues);
                        dItem.ButtonRow = Item.gridRows[0];
                    }
                }
                */
                //Sets Time span Button text                
                if (dItem.StyleId == "myTimeScaleButton")
                {
                    var ChartSetting = MyDataAccessLayer.GetChartSettings(dItem.ItemID);
                    DateTime myDT = new DateTime(ChartSetting.iYear, ChartSetting.iMonth, ChartSetting.iDay, new GregorianCalendar());
                    switch (ChartSetting.iSpan)
                    {
                        case 1:
                            dItem.ButtonText = "Latest 5 minutes";
                            break;
                        case 2:
                            dItem.ButtonText = myDT.ToString("hh:mm  dd", CultureInfo.InvariantCulture) + " " + myDT.ToString("MMMM", CultureInfo.InvariantCulture) + " " + myDT.ToString("yyyy", CultureInfo.InvariantCulture);
                            break;
                        case 3:
                            dItem.ButtonText = myDT.ToString("dd", CultureInfo.InvariantCulture) + " " + myDT.ToString("MMMM", CultureInfo.InvariantCulture) + " " + myDT.ToString("yyyy", CultureInfo.InvariantCulture);
                            break;
                        case 4:
                            dItem.ButtonText = "Week " + ChartSetting.iWeek.ToString("00") + " " + myDT.ToString("yyyy", CultureInfo.InvariantCulture); 
                            break;
                        case 5:
                            dItem.ButtonText = myDT.ToString("MMMM", CultureInfo.InvariantCulture) + " " + myDT.ToString("yyyy", CultureInfo.InvariantCulture);
                            break;
                        case 6:
                            dItem.ButtonText = "Year " + myDT.ToString("yyyy", CultureInfo.InvariantCulture);
                            break;
                    }
                }
                           
            }
        }


        List<gridRow> gridRows = new List<gridRow>();
        switch (ScadaGlobals.CurrentScadaPopup)
        {
            case ScadaClasses.uxAlarmGrid:
                gridRows = MyDataAccessLayer.ReadAlarmMessages("All");
                break;
            case ScadaClasses.uxTagsGrid:
                gridRows = MyDataAccessLayer.ReadTags("", 0, iMenuOffsetRows, iMenuOffsetRows+5 );
                break;
            case ScadaClasses.uxTagSettings:
                gridRows = MyDataAccessLayer.GetTagParams(ScadaGlobals.CurrentTag);
                break;

            case ScadaClasses.uxProtocolSettings:
                gridRows = MyDataAccessLayer.ReadParameters("", 2, ScadaGlobals.CurrentCategory, ScadaGlobals.CurrentCategory, 0, 10);
                break;

        }

        //Populate only livegrids
        if ((ScadaGlobals.CurrentScadaPopup == ScadaClasses.uxAlarmGrid) ||
            (ScadaGlobals.CurrentScadaPopup == ScadaClasses.uxTagsGrid)  ||
            (ScadaGlobals.CurrentScadaPopup == ScadaClasses.uxTagSettings) ||
            (ScadaGlobals.CurrentScadaPopup == ScadaClasses.uxProtocolSettings))
        {
            foreach (SKCanvasView uxItem in SKCanvasPopupViews)
            {
                if (uxItem is ScadaButton)
                {
                    var dItem = uxItem as ScadaButton;
                    bool isNumeric = int.TryParse(dItem.StyleId, out int iRow);
                    if (isNumeric)
                    {
                        var emptyrow = new gridRow
                        {
                            Status = 5,
                            col1text = "",
                            col1width = 0F,
                            col2text = "",
                            col2width = 0F,
                            col3text = "",
                            col3width = 0F,
                            col4text = "",
                            col4width = 0F,
                            col5text = "",
                            col5width = 0F,
                            col6text = "",
                            col6width = 0F,
                            TagID = 0,
                            Row = iRow,
                            Id = 0
                        };

                        dItem.ButtonRow = emptyrow;
                        foreach (gridRow myRow in gridRows)
                        {
                            if (iRow == myRow.Row)
                            {
                                if (dItem.IsVisible == true)
                                {
                                    dItem.ButtonRow = myRow;
                                    dItem.InvalidateSurface();
                                }
                            }
                        }
                    }
                }

            }

        }
        
    }

}

