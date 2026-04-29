
using Microsoft.Maui.Layouts;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Globalization;
using System.Text.RegularExpressions;
using static Scada.DataAccessLayer;

namespace Scada;

public partial class MainPage : ContentPage
{
    ScadaClasses.SystemColors ScadaColor = new ScadaClasses.SystemColors();

    public System.Timers.Timer bcktimer = new System.Timers.Timer();
    public System.Timers.Timer uxtimer = new System.Timers.Timer();

    //private static WebSocket client;
    public static string ConnectionString = "";
    ScadaPopups MyPopups = new ScadaPopups();
    Editor edtSvgEditor = new Editor { Placeholder = "Paste your SVG text", Text = "Paste your SVG text" };
    Editor edtSvgName = new Editor { Placeholder = "SvgName", Text = "SvgName" };
    Editor edtInputText = new Editor { Placeholder = "InputText", Text = "" };
    
    bool bStartup = true;
    bool Designing = false;
    bool Moving = false;
    bool bLoginSuccess = false;
    int  infoType = ScadaClasses.infoStartup;

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
        ScadaClasses.Previouspage = -1;
        ScadaClasses.Refresh = true;      
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

        //ConnectionString = "Data Source = PC-5CG5125C24; Initial Catalog = SCADA; Integrated Security = true; TrustServerCertificate=true";
         //ConnectionString = "Server=tcp:scada.database.windows.net,1433;Initial Catalog = scada; Encrypt=True;TrustServerCertificate=False;Connection Timeout = 30; Authentication=Active Directory Default";

        //SQL
        ConnectionString = "Data Source =PC-5CG5125C24; Initial Catalog = SCADA; Integrated Security = true; TrustServerCertificate = true";

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
    
        bcktimer.Interval = 2000;
        bcktimer.Elapsed += bckUpdate;
        bcktimer.Start();
        bcktimer.Enabled = true;

        uxtimer.Interval = 100;
        uxtimer.Elapsed += uxUpdate;
        uxtimer.Start();
        uxtimer.Enabled = true;
        ScadaClasses.CurrentScadaPopup = ScadaClasses.uxLoginMenu;
    
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
                
                /*
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
                */

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
                
                ScadaClasses.Refresh = true;
                bcktimer.Interval = 5000;
            });
        }
        catch
        {
        }
    }

    void uxUpdate(object sender, EventArgs e)
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (!Moving)
                {
                    if (bStartup)
                    {
                        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(ConnectionString);
                        var ListOfDigitalTagsToStore = MyDataAccessLayer.GetDigitalTagsToStore(-1);
                        foreach (var Tag in ListOfDigitalTagsToStore)
                        {
                            MyDataAccessLayer.StoreTagValue(Tag.TagID, Tag.Value);
                        }

                        if (MyDataAccessLayer.GetTagValueByName("Enable ADAM") > 0.5)
                        {
                            MyDataAccessLayer.SetTagStatus(1, 1);
                        }
                        bStartup = false;
                    }
                    if (ScadaClasses.Refresh == true)
                    {
                        ScadaClasses.Refresh = false;
                        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(ConnectionString);

                        var Items = new List<ScadaClasses.Telegram> {};
                        if (bLoginSuccess == true)
                        {
                            Items = MyDataAccessLayer.getItems(ScadaClasses.Currentpage);
                        }
                        Items = MyPopups.AddCurrentPopup(ScadaClasses.CurrentScadaPopup, ScadaClasses.Currentpage, ScadaClasses.CurrentTag, ScadaClasses.CurrentItem, Width, Height, Xpos, Ypos, Xwidth, Yheight, Items);
                        UpdateGui(Items);
                    }
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
                    ScadaClasses.Previouspage = -1;
                    ScadaClasses.CurrentScadaPopup = ScadaClasses.uxEditParameterText;
                    ScadaClasses.PreviousScadaPopup = ScadaClasses.uxLoginMenu;
                    edtInputText.Text = ConnectionString;
                    ScadaClasses.CurrentType = ScadaClasses.pmComputer;
                    ScadaClasses.Refresh = true;
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
        AbsoluteLayout.SetLayoutBounds(btnSettings, new Rect(x + btnSettings.WidthRequest/5, y + btnSettings.HeightRequest/5, 25, 25));
        AbsoluteLayout.SetLayoutFlags(btnSettings, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(btnSettings);

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
                        ScadaClasses.Previouspage = -1;
                        ScadaClasses.CurrentScadaPopup = -1;
                        ScadaClasses.Refresh = true;
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
                case SKTouchAction.Pressed:
                    ScadaClasses.Previouspage = -1;
                    ScadaClasses.CurrentScadaPopup = -1;
                    ScadaClasses.Refresh = true;
                    bLoginSuccess = true;
                    infoType = ScadaClasses.infoStartup;
                    //Go
                    uxtimer.Enabled = true;
                    bcktimer.Enabled = true;
                    break;

                case SKTouchAction.Released:
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
        s2.WidthRequest = 70;
        s2.HeightRequest = 70;
        s2.IsEnabled = true;
        s2.IsVisible = true;
        AbsoluteLayout.SetLayoutBounds(s2, new Rect(x, y - 150, w, h));
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
        var btnPages = new ScadaButton();
        btnPages.GradientStartColor = ScadaColor.uxItemColor;
        btnPages.GradientEndColor = ScadaColor.uxItemColor;
        btnPages.CornerRadius = 15;
        btnPages.ItemID = 1;
        btnPages.EnableTouchEvents = true;
        btnPages.InputTransparent = false;
        btnPages.HeightRequest = 25;
        btnPages.WidthRequest = 25;
        btnPages.SvgBase64 = MyDataAccessLayer.LoadLibItem("Closecross", 1);
        btnPages.IndicatorType = 3;
        btnPages.Margin = 0.15F;
        btnPages.ButtonText = "";
        btnPages.Touch += (sender, args) =>
        {
            switch (args.ActionType)
            {
                case SKTouchAction.Released:
                    /* ScadaControlTelegram oTelegram = new ScadaControlTelegram()
                     {
                         MessageType = 22,
                         Page = ScadaClasses.Currentpage,
                         ItemType = 3,
                         ItemID = 1,
                         TagID = 1,
                         TagName = "",
                         Action = " ",
                     };
                     */
                    ScadaClasses.Previouspage = -1;
                    ScadaClasses.CurrentScadaPopup = ScadaClasses.uxParameters;
                    ScadaClasses.Refresh = true;
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

        //x = x + 30;
        AbsoluteLayout.SetLayoutBounds(btnPages, new Rect(x + gp.WidthRequest - btnPages.WidthRequest - (btnPages.WidthRequest / 4), y + btnPages.HeightRequest / 4, 25, 25));
        AbsoluteLayout.SetLayoutFlags(btnPages, AbsoluteLayoutFlags.None);
        SKCanvasPopupViews.Add(btnPages);

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
                    ScadaClasses.Previouspage = -1;
                    //CurrentScadaPopup = -1;
                    string sBase64 = Base64Encode(edtSvgEditor.Text);
                    MyDataAccessLayer.DeleteLibItem(edtSvgName.Text);
                    MyDataAccessLayer.SaveLibItem(edtSvgName.Text, 1, sBase64);
                    ScadaClasses.Refresh = true;
                    break;

                case SKTouchAction.Released:
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
        SKCanvasViews.Add(popupParameters);
        CreateCloseButton(x, y, w, h, ScadaClasses.uxProtocols);

        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);
        //var gridRows = MyDataAccessLayer.GetParams(ScadaClasses.CurrentRow);

    
        var gridRows = MyDataAccessLayer.ReadParameters("",2, ScadaClasses.CurrentRow, ScadaClasses.CurrentRow, 0, 10);

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
            SKCanvasViews.Add(myRowButton);

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
                            ScadaClasses.Previouspage = -1;
                            ScadaClasses.CurrentScadaPopup = ScadaClasses.uxProtocolSettings;
                            ScadaClasses.Refresh = true;
                            break;
                    }
                    args.Handled = true;
                };
                AbsoluteLayout.SetLayoutBounds(tgRow, new Rect(x + w - 80, y + 2, 50, 22));
                AbsoluteLayout.SetLayoutFlags(tgRow, AbsoluteLayoutFlags.None);
                SKCanvasViews.Add(tgRow);
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
                btnDots.IndicatorType = 3;
                btnDots.Margin = 0.15F;
                btnDots.ButtonText = "";
                btnDots.Touch += (sender, args) =>
                {
                    switch (args.ActionType)
                    {
                        case SKTouchAction.Released:
                            ScadaClasses.Previouspage = -1;
                            ScadaClasses.PreviousScadaPopup = ScadaClasses.CurrentScadaPopup;
                            ScadaClasses.CurrentScadaPopup = ScadaClasses.uxEditParameterText;
                            edtInputText.Text = myRowButton.ButtonRow.col2text;
                            ScadaClasses.CurrentID = myRowButton.ButtonRow.Id;
                            ScadaClasses.CurrentType = ScadaClasses.pmParameter;
                        
                            ScadaClasses.Refresh = true;
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
                SKCanvasViews.Add(btnDots);
            }
            y = y + 26;
            r++;
        }
    }

    /*
    private void ScadaProtocolSettingsOld()
    {
        double panelWith = 0.4;
        double panelHeight = 0.55;

        double x = (Width / 2) - panelWith * Width / 2;
        double y = (Height / 2) - panelHeight * Height / 2;
        double w = (panelWith * Width);
        double h = (panelHeight * Height);

        var TagsPropPanel = new ScadaButton();
        TagsPropPanel.AnchorX = 0;
        TagsPropPanel.AnchorY = 0;
        TagsPropPanel.ItemID = 1;
        TagsPropPanel.StyleId = "1";
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
        SKCanvasViews.Add(TagsPropPanel);

        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);
        var gridRows = MyDataAccessLayer.GetParams(ScadaClasses.CurrentRow);

        CreateCloseButton(x, y, w, h, ScadaClasses.uxProtocols);
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

           
            myRowButton.Touch += (sender, args) =>
            {
                var pt = args.Location;
                switch (args.ActionType)
                {
                    case SKTouchAction.Released:
                        ScadaClasses.Previouspage = -1;
                        ScadaClasses.PreviousScadaPopup = ScadaClasses.CurrentScadaPopup;
                        ScadaClasses.CurrentScadaPopup = ScadaClasses.uxEditParameterText;
                        edtInputText.Text = myRowButton.ButtonRow.col2text;
                        ScadaClasses.CurrentID = myRowButton.ButtonRow.Id;
                        ScadaClasses.CurrentType = ScadaClasses.pmParameter;
                        ScadaClasses.Refresh = true;
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
            SKCanvasViews.Add(myRowButton);
            y = y + 30;

        }
    }
    */


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
        double panelHeight = 0.7;

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
        //popupAlarm.EnableFaceFade();
        AbsoluteLayout.SetLayoutBounds(popupAlarm, new Rect(x, y, w, h));
        AbsoluteLayout.SetLayoutFlags(popupAlarm, AbsoluteLayoutFlags.None);
        SKCanvasViews.Add(popupAlarm);
        CreateCloseButton(x, y, w, h, -1);

        y = y + popupAlarm.CornerRadius;
        var TheColor = ScadaColor.uxItemColor;
        y = y + 20;
        for (int r = 0; r < 17; r++)
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
                        ScadaAlarmPopup(myRowButton.ButtonRow.Id, myRowButton.ButtonRow.TagID, myRowButton.ButtonRow.col2text);
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
        
            myRowButton.InputTransparent = false;
            myRowButton.EnableTouchEvents = true;
            myRowButton.EnableIndicatorBlink();
            //myRowButton.EnableFaceFade();
            AbsoluteLayout.SetLayoutBounds(myRowButton, new Rect(x, y, myRowButton.WidthRequest, 26));
            AbsoluteLayout.SetLayoutFlags(myRowButton, AbsoluteLayoutFlags.None);          
            SKCanvasViews.Add(myRowButton);
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
        SKCanvasViews.Add(popupAlarm);
        CreateCloseButton(x, y, w, h, ScadaClasses.uxParameters );

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
                        ScadaClasses.Previouspage = -1;
                        ScadaClasses.CurrentScadaPopup = ScadaClasses.uxProtocolSettings;
                        ScadaClasses.CurrentRow = myRowButton.ButtonRow.Id;
                        ScadaClasses.Refresh = true;                      
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
            SKCanvasViews.Add(myRowButton);
            //AbsoluteLayout.SetLayoutBounds(tgRow, new Rect(x + 520, y + 2, 50, 22));
            //AbsoluteLayout.SetLayoutFlags(tgRow, AbsoluteLayoutFlags.None);
            //SKCanvasViews.Add(tgRow);
            y = y + 26;
            r++;
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
        SKCanvasViews.Add(popupTags);
        CreateCloseButton(x, y, w, h, ScadaClasses.uxParameters);
        CreateCloseButton(x, y, w, h, -1);
        CreateFwdButton(x, y, w, h);
        CreateRwdButton(x, y, w, h);
        CreateRemoveTagButton(ScadaClasses.CurrentItem, ScadaClasses.CurrentTag, x - (w / 2), y, w, h);

        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);

        //int Dig = MyDataAccessLayer.GetItemIsDigital(ScadaClasses.CurrentItem);

        int TypeOfTag = 1;
        /*
        if ((ScadaClasses.CurrentType == ScadaClasses.uxToggle) ||
            (ScadaClasses.CurrentType == ScadaClasses.uxButton) ||
            (ScadaClasses.CurrentType == ScadaClasses.uxCircularProgress))
            TypeOfTag = 1;
        else
            */
            TypeOfTag = 3;
        /*
        if (ScadaClasses.CurrentType == ScadaClasses.uxHistoryChart)
        {
            ScadaItem.gridRows = MyDataAccessLayer.ReadTags("", 0, iMenuOffsetRows, iMenuOffsetRows + 5);
        }
        else
        {
            ScadaItem.gridRows = MyDataAccessLayer.ReadTags("", TypeOfTag, iMenuOffsetRows, iMenuOffsetRows + 5);
        }
        */
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
                        /*
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
                        */
                        /*
                        if (ScadaClasses.CurrentType == ScadaClasses.uxHistoryChart)
                        {
                            if (ScadaClasses.CurrentTag == 0)
                            {
                                MyDataAccessLayer.AddItemTag(ScadaItem.ItemID, ScadaItem.ItemType, row.TagID, ScadaClasses.CurrentRow, "");
                            }
                            else
                            {
                                MyDataAccessLayer.SetItemTag(ScadaItem.ItemID, row.TagID, ScadaClasses.CurrentRow);
                            }
                        }
                        else
                        {
                            MyDataAccessLayer.SetItemTag(ScadaItem.ItemID, row.TagID, ScadaClasses.CurrentRow);
                        }
                        */

                        if (ScadaClasses.CurrentTag == 0)
                        {
                            MyDataAccessLayer.AddItemTag(ScadaClasses.CurrentItem, ScadaClasses.CurrentType, row.TagID, ScadaClasses.CurrentRow, "");
                        }
                        else
                        {
                            MyDataAccessLayer.SetItemTag(ScadaClasses.CurrentItem, row.TagID, ScadaClasses.CurrentRow);
                        }

                        ScadaClasses.CurrentTag = row.TagID;
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
            SKCanvasViews.Add(myButton);
            y = y + 26;
      
            //Thread.Sleep(100);
        }
    }

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
        SKCanvasViews.Add(popupAlarm);
        CreateCloseButton(x, y, w, h, ScadaClasses.uxParameters);

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
                        var myChartSettings = MyDataAccessLayer.GetChartSettings(ScadaClasses.CurrentItem);
                        myChartSettings.iSpan = myRow.Id;
                        MyDataAccessLayer.SetChartSettings(myChartSettings, ScadaClasses.CurrentItem);
                        ScadaClasses.CurrentScadaPopup = -1;
                        ScadaClasses.Previouspage = -1;
                        ScadaClasses.Refresh = true;
   
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
            SKCanvasViews.Add(myRowButton);
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
        popupAlarm.GradientStartColor = ScadaColor.uxItemColor;//ScadaColor.uxItemBackGroundColor
        popupAlarm.GradientEndColor = ScadaColor.uxItemColor; //ScadaColor.uxItemBackGroundColor
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
        SKCanvasViews.Add(popupAlarm);
        CreateCloseButton(x, y, w, h, -1);
        CreateAddTagButton(x, y, w, h);

        y = y + popupAlarm.CornerRadius;
        var TheColor = ScadaColor.uxItemColor;
        y = y + 30;
        for (int r = 0; r < 14; r++)
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
                        ScadaClasses.Previouspage = -1;
                        ScadaClasses.CurrentScadaPopup = ScadaClasses.uxTagSettings;
                        ScadaClasses.CurrentTag = myRowButton.ButtonRow.TagID;
                        ScadaClasses.Refresh = true;
                        /*ScadaAlarmPopup(myRowButton.ButtonRow.Id, myRowButton.ButtonRow.TagID, myRowButton.ButtonRow.col2text);
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
            myRowButton.EnableIndicatorBlink();
            myRowButton.InputTransparent = false;
            myRowButton.EnableTouchEvents = true;

            AbsoluteLayout.SetLayoutBounds(myRowButton, new Rect(x, y, myRowButton.WidthRequest, 26));
            AbsoluteLayout.SetLayoutFlags(myRowButton, AbsoluteLayoutFlags.None);
            SKCanvasViews.Add(myRowButton);
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
        SKCanvasViews.Add(popupPages);
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
            SKCanvasViews.Add(myRowButton);
        
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
                        edtInputText.Text = myRow.col1text;
                        ScadaClasses.CurrentScadaPopup = ScadaClasses.uxEditParameterText;
                        ScadaClasses.CurrentType = ScadaClasses.pmPages;
                        ScadaClasses.CurrentID = myRow.Id;
                        ScadaClasses.Previouspage = -1;
                        ScadaClasses.Refresh = true;
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
            SKCanvasViews.Add(btnDots);
                   
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
        SKCanvasViews.Add(popupParameters);
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
            SKCanvasViews.Add(myRowButton);

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
                            ScadaClasses.Previouspage = -1;
                            ScadaClasses.Refresh = true;
                            break;
                    }
                    args.Handled = true;
                };
                AbsoluteLayout.SetLayoutBounds(tgRow, new Rect(x + w -80, y + 2, 50, 22));
                AbsoluteLayout.SetLayoutFlags(tgRow, AbsoluteLayoutFlags.None);
                SKCanvasViews.Add(tgRow);
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
                btnDots.IndicatorType = 3;
                btnDots.Margin = 0.15F;
                btnDots.ButtonText = "";
                btnDots.Touch += (sender, args) =>
                {
                    switch (args.ActionType)
                    {
                        case SKTouchAction.Released:
                            switch (myRow.col1text)
                            {
                                case "Connection string":
                                    edtInputText.Text = ConnectionString;
                                    ScadaClasses.CurrentType = ScadaClasses.pmComputer;
                                    ScadaClasses.CurrentScadaPopup = ScadaClasses.uxEditParameterText;
                                    break;
                                case "Items":
                                    ScadaClasses.CurrentScadaPopup = ScadaClasses.uxUploadMenu;
                                    break;
                                case "Signals":
                                      ScadaClasses.CurrentScadaPopup = ScadaClasses.uxTagsGrid;
                                    break;
                                case "Pages":
                                    ScadaClasses.CurrentScadaPopup = ScadaClasses.uxPagesMenu;
                                    break;
                                case "Protocols":
                                    ScadaClasses.CurrentScadaPopup = ScadaClasses.uxProtocols;
                                    break;

                            }
                            ScadaClasses.Previouspage = -1;                          
                            ScadaClasses.Refresh = true;
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
                SKCanvasViews.Add(btnDots);
            }

            /*
            myRowButton.Touch += (sender, args) =>
            {
                var pt = args.Location;
                switch (args.ActionType)
                {
                    case SKTouchAction.Released:
                        ScadaClasses.Previouspage = -1;
                        ScadaClasses.CurrentScadaPopup = ScadaClasses.uxTagSettings;
                        ScadaClasses.CurrentTag = myRowButton.ButtonRow.TagID;
                        ScadaClasses.Refresh = true;
                       
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
            */

      
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
                        Page = ScadaClasses.Currentpage,
                        Action = " "
                    };
                    MyDataAccessLayer.confirmAlarm(oTelegram);
                    ScadaClasses.Refresh = true;
                    ScadaClasses.CurrentScadaPopup = ScadaClasses.uxAlarmGrid;
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
                        Page = ScadaClasses.Currentpage,
                        Action = " "
                    };
                    MyDataAccessLayer.disableAlarm(TagID);
                    MyDataAccessLayer.confirmAlarm(oTelegram);
                    MyDataAccessLayer.deleteAlarm(oTelegram);
                    ScadaClasses.CurrentScadaPopup = ScadaClasses.uxAlarmGrid;
                    //SendMessage(JsonSerializer.Serialize(oTelegram));
                    ScadaClasses.Refresh = true;
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
                    ScadaClasses.Previouspage = -1;
                    ScadaClasses.CurrentScadaPopup = ScadaClasses.uxAlarmGrid;
                    ScadaClasses.Refresh = true;
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
                    ScadaClasses.CurrentScadaPopup = ScadaClasses.uxConfigMenu;
                    Xpos = sn.X;
                    Ypos = sn.Y;
                    ScadaClasses.Telegram oTelegram = new ScadaClasses.Telegram()
                    {
                        MessageType = ScadaClasses.CmdupdateSV,
                        Page = ScadaClasses.Currentpage,
                        Top = ((sn.Y / Height) * 100),
                        Left = ((sn.X / Width) * 100),
                        ItemType = ScadaItem.ItemType,
                        ItemID = ScadaItem.ItemID,
                        TagID = ScadaItem.TagID,
                        SV = "",
                        TagName = ScadaItem.TagName,
                        Action = ScadaItem.Action,
                    };
                    ScadaClasses.CurrentItem = ScadaItem.ItemID;
                    ScadaClasses.CurrentType = ScadaItem.ItemType;
                    DataAccessLayer MyDataAccessLayer = new DataAccessLayer(ConnectionString); 
                    MyDataAccessLayer.moveItem(oTelegram);

                    ScadaClasses.Previouspage = -1;
                    ScadaClasses.Refresh = true;
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
            IndicatorType = 3,
            GradientStartColor = ScadaColor.uxItemColor,
            GradientEndColor = ScadaColor.uxItemColor,
            CornerRadius = 10,
            TextColor = ScadaColor.uxTextColor,
            FontSize = 21.5F,
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
                                case SKTouchAction.Pressed:
                                    ScadaClasses.CurrentScadaPopup = callerPopup;
                                    ScadaClasses.Previouspage = -1;
                                    ScadaClasses.Refresh = true;
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
        SKCanvasViews.Add(myCloseButton);
    }


    public void CreateAddTagButton(double x, double y, double w, double h)
    {
        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(ConnectionString);

        var myRemoveButton = new ScadaButton
        {
            WidthRequest = 25,
            HeightRequest = 25,
            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
            BarBackgroundColor = ScadaColor.uxBackGroundColor,
            IndicatorType = 3,
            GradientStartColor = ScadaColor.uxItemColor,
            GradientEndColor = ScadaColor.uxItemColor,
            CornerRadius = 10,
            TextColor = ScadaColor.uxTextColor,
            FontSize = 21.5F,
            SvgBase64 = MyDataAccessLayer.LoadLibItem("GreenPlus", 1),
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
                case SKTouchAction.Pressed:
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
                    ScadaClasses.CurrentTag = MyDataAccessLayer.AddTag(newTag);
                    ScadaClasses.CurrentScadaPopup = ScadaClasses.uxTagSettings;
                    ScadaClasses.Previouspage = -1;
                    ScadaClasses.Refresh = true;
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

        AbsoluteLayout.SetLayoutBounds(myRemoveButton, new Rect(x + 5, y + 5, 30, 30));
        AbsoluteLayout.SetLayoutFlags(myRemoveButton, AbsoluteLayoutFlags.None);
        SKCanvasViews.Add(myRemoveButton);
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
            IndicatorType = 3,
            GradientStartColor = ScadaColor.uxItemColor,
            GradientEndColor = ScadaColor.uxItemColor,
            CornerRadius = 10,
            TextColor = ScadaColor.uxTextColor,
            FontSize = 21.5F,
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
                case SKTouchAction.Pressed:
                    MyDataAccessLayer.DeleteTagFromUxItem(ItemID, TagID);
                    ScadaClasses.CurrentScadaPopup = -1;
                    ScadaClasses.Previouspage = -1;
                    ScadaClasses.Refresh = true;
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
        SKCanvasViews.Add(myRemoveButton);
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
            IndicatorType = 3,
            GradientStartColor = ScadaColor.uxItemColor,
            GradientEndColor = ScadaColor.uxItemColor,
            CornerRadius = 10,
            TextColor = ScadaColor.uxTextColor,
            FontSize = 21.5F,
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
                case SKTouchAction.Pressed:
                    //CurrentScadaPopup = -1;
                    ScadaClasses.Previouspage = -1;
                    iMenuOffsetRows = iMenuOffsetRows + 5;
                    ScadaClasses.Refresh = true;
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
        SKCanvasViews.Add(myFwdButton);
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
            IndicatorType = 3,
            GradientStartColor = ScadaColor.uxItemColor,
            GradientEndColor = ScadaColor.uxItemColor,
            CornerRadius = 10,
            TextColor = ScadaColor.uxTextColor,
            FontSize = 21.5F,
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
                case SKTouchAction.Pressed:
                    //CurrentScadaPopup = -1;
                    ScadaClasses.Previouspage = -1;
                    iMenuOffsetRows = iMenuOffsetRows - 5;
                    ScadaClasses.Refresh = true;
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
        SKCanvasViews.Add(myRwdButton);
    }


    public void UpdateGui(List<ScadaClasses.Telegram> ScadaItems)
    {
        Window.MaximumWidth = 4000;
        Window.MaximumHeight = 2000;
        Window.MinimumWidth = 1280;
        Window.MinimumHeight = 600;
        Window.IsMaximizable = true;
        double x = 0, y = 0, w = 0, h = 0;   
        double wScale = Width / 100;
        double hScale = Height / 100;
  
        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(ConnectionString);

        if ((ScadaClasses.Previouspage != ScadaClasses.Currentpage) && (ScadaItems.Count > 0))
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
                        ScadaAlarmGrid();
                        break;

                    case ScadaClasses.uxParameters:
                        ScadaParameters();
                        break;

                    case ScadaClasses.uxTagsGrid:
                        ScadaTagsGrid();                  
                        break;

                    case ScadaClasses.uxTagSettings:                 
                        x = (Width / 100) * ScadaItem.Left;
                        y = (Height / 100) * ScadaItem.Top;
                        w = (Width / 100) * ScadaItem.Width;
                        h = (Height / 100) * ScadaItem.Height;

                        var TagsPropPanel = new ScadaButton();
                        TagsPropPanel.AnchorX = 0;
                        TagsPropPanel.AnchorY = 0;
                        TagsPropPanel.ItemID = ScadaItem.ItemID;
                        TagsPropPanel.StyleId = ScadaItem.ItemID.ToString();
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

                        if (Designing == true)
                        {
                            TagsPropPanel.EnableTouchEvents = true;
                            TagsPropPanel.InputTransparent = false;
                            TagsPropPanel = (ScadaButton)AttachDesignEvents(TagsPropPanel, ScadaItem);
                        }

                        AbsoluteLayout.SetLayoutBounds(TagsPropPanel, new Rect(
                                (Width / 100) * ScadaItem.Left,
                                (Height / 100) * ScadaItem.Top,
                                (Width / 100) * ScadaItem.Width,
                                (Height / 100) * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(TagsPropPanel, AbsoluteLayoutFlags.None);
                        SKCanvasViews.Add(TagsPropPanel);

                        ScadaItem.gridRows = MyDataAccessLayer.GetTagParams(ScadaClasses.CurrentTag);

                        CreateCloseButton(x, y, w, h, ScadaClasses.uxTagsGrid);
                        var TagRowColor = ScadaColor.uxItemColor;
                        y = y + 40;
                        for (int r = 0; r < 10; r++)
                        {
                            if ((r % 2) == 0)
                            {
                                TagRowColor = ScadaColor.uxLightColor;
                            }
                            else
                            {
                                TagRowColor = ScadaColor.uxItemColor;
                            }

                            var myRowButton = new ScadaButton
                            {
                                ItemID = ScadaItem.ItemID,
                                StyleId = r.ToString(),
                                WidthRequest = w,
                                HeightRequest = 26,
                                Background = ScadaColor.uxItemColor.ToMauiColor(),
                                BarBackgroundColor = ScadaColor.uxItemColor,
                                IndicatorType = 2,

                                IndicatorColor = TagRowColor,
                                GradientStartColor = TagRowColor,
                                GradientEndColor = TagRowColor,

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
                            myRowButton.EnableTouchEvents = true;
                       
                            myRowButton.Touch += (sender, args) =>
                            {
                                var pt = args.Location;
                                switch (args.ActionType)
                                {
                                    case SKTouchAction.Released:
                                        ScadaClasses.Previouspage = -1;
                                        ScadaClasses.CurrentScadaPopup = ScadaClasses.uxEditTagText;

                                        var TagParams = MyDataAccessLayer.GetTagParams(ScadaClasses.CurrentTag);
                                        edtInputText.Text = TagParams[myRowButton.ButtonRow.Row].col2text;
                                        ScadaClasses.CurrentRow = myRowButton.ButtonRow.Row;

                                        ScadaClasses.Refresh = true;
                                        break;

                                    case SKTouchAction.Entered:
                                        myRowButton.GradientStartColor = ScadaColor.uxTouchColor;
                                        myRowButton.GradientEndColor = ScadaColor.uxTouchColor;
                                        myRowButton.IndicatorColor = ScadaColor.uxTouchColor;
                                        break;

                                    case SKTouchAction.Exited:
                                        if ((myRowButton.ButtonRow.Row % 2) == 0)
                                        {
                                            myRowButton.GradientStartColor = ScadaColor.uxLightColor;
                                            myRowButton.GradientEndColor = ScadaColor.uxLightColor;
                                            myRowButton.IndicatorColor = ScadaColor.uxLightColor;
                                        }
                                        else
                                        {
                                            myRowButton.GradientStartColor = ScadaColor.uxItemColor;
                                            myRowButton.GradientEndColor = ScadaColor.uxItemColor;
                                            myRowButton.IndicatorColor = ScadaColor.uxItemColor;
                                        }
                                        break;
                                }
                                args.Handled = true;
                            };
                            myRowButton.EnableIndicatorBlink();
                            myRowButton.InputTransparent = false;
                            AbsoluteLayout.SetLayoutBounds(myRowButton, new Rect(x, y, w, 26));
                            AbsoluteLayout.SetLayoutFlags(myRowButton, AbsoluteLayoutFlags.None);

                            if (y < (Height / 100 * ScadaItem.Top) + (Height / 100 * ScadaItem.Height) - 26)
                            {
                                SKCanvasViews.Add(myRowButton);
                                y = y + 26;
                            }
                        }
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
                        SKButton.Init(wScale, hScale, SKButton, ScadaItem, ScadaColor);
                        if (Designing == true)
                        {
                            SKButton = (ScadaButton)AttachDesignEvents(SKButton, ScadaItem);
                        }
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

                        double wTSButton = ((Width / 100) * ScadaItem.Width) / 3;
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
                                case SKTouchAction.Pressed:
                                    //TimeScale Span menu
                                    ScadaClasses.CurrentItem = ScadaItem.ItemID;

                                    ScadaClasses.CurrentScadaPopup = ScadaClasses.uxTimeSpanMenu;
                                    ScadaClasses.Previouspage = -1;
                                    ScadaClasses.CurrentType = ScadaClasses.uxHistoryChart;
                                    ScadaClasses.Refresh = true;
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
                                case SKTouchAction.Pressed:
                                    ChartSetTimeScaleStep(ScadaItem.ItemID, 1);
                                    ScadaClasses.Refresh = true;
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
                                case SKTouchAction.Pressed:
                                    ChartSetTimeScaleStep(ScadaItem.ItemID, -1);
                                    ScadaClasses.Refresh = true;
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

                        int btnRow = 1;
                        float btnY = 40F;
                      
                        var GraphItems = MyDataAccessLayer.GetItemTags(ScadaItem.ItemID);
                        foreach (var myGraph in GraphItems)
                        {
                            double wTagButton = ((Width / 100) * ScadaItem.Width) / 3;
                            double hTagButton = 24;
                            double xTagButton = (Width / 100) * ScadaItem.Left + ((trh.WidthRequest / 2) - (wTSButton / 2));
                            double yTagButton = (Height / 100) * ScadaItem.Top + btnY;

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
                                    case SKTouchAction.Pressed:
                                        ScadaClasses.CurrentItem = ScadaItem.ItemID;
                                        ScadaClasses.CurrentTag = myGraph.TagID;
                                        ScadaClasses.CurrentRow = myTagButton.TagSequence;
                                        ScadaClasses.CurrentScadaPopup = ScadaClasses.uxTagsMenu;
                                        ScadaClasses.Previouspage = -1;
                                        ScadaClasses.CurrentType = ScadaClasses.uxHistoryChart;
                                        ScadaClasses.Refresh = true;

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
                            btnRow = btnRow + 1;
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
                            TagSequence = btnRow,
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
                                case SKTouchAction.Pressed:
                                    ScadaClasses.CurrentItem = ScadaItem.ItemID;
                                    ScadaClasses.CurrentTag = 0;
                                    ScadaClasses.CurrentRow = btnRow;
                                    ScadaClasses.CurrentScadaPopup = ScadaClasses.uxTagsMenu;
                                    ScadaClasses.Previouspage = -1;
                                    ScadaClasses.CurrentType = ScadaClasses.uxHistoryChart;
                                    ScadaClasses.Refresh = true;

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
                                    case SKTouchAction.Pressed:                                     
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

                                        ScadaClasses.Refresh = true;
                                        //SKToggle.InvalidateSurface();
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
                        var FloatPanel = new ScadaButton();
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
                        SKCanvasViews.Add(FloatPanel);

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
                            IndicatorType = 3,

                            GradientStartColor = ScadaColor.uxItemColor,
                            GradientEndColor = ScadaColor.uxItemColor,
                            CornerRadius = 10,
                            TextColor = ScadaColor.uxTextColor,
                            FontSize = 21.5F,
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
                                case SKTouchAction.Pressed:                  
                                    MyDataAccessLayer.deleteItem(ScadaClasses.CurrentItem);
                                    ScadaClasses.CurrentScadaPopup = -1;
                                    ScadaClasses.Previouspage = -1;
                                    ScadaClasses.Refresh = true;
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
                        SKCanvasViews.Add(myDeleteButton);

                        var myAddItemButton = new ScadaButton
                        {
                            WidthRequest = 25,
                            HeightRequest = 25,
                            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
                            BarBackgroundColor = ScadaColor.uxBackGroundColor,
                            IndicatorType = 3,
                            GradientStartColor = ScadaColor.uxItemColor,
                            GradientEndColor = ScadaColor.uxItemColor,
                            CornerRadius = 10,
                            TextColor = ScadaColor.uxTextColor,
                            FontSize = 21.5F,
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
                                case SKTouchAction.Pressed:
                                    ScadaClasses.Telegram oTelegram = new ScadaClasses.Telegram()
                                    {
                                        MessageType = ScadaItem.MessageType,
                                        Page = ScadaClasses.Currentpage,
                                        ItemType = ScadaClasses.CurrentType,
                                        ItemID = ScadaClasses.CurrentItem,
                                        TagID = ScadaItem.TagID,
                                        TagName = ScadaItem.TagName,
                                        Action = ScadaItem.Action,
                                    };
                                    oTelegram.ItemID = MyDataAccessLayer.copyItem(oTelegram);
                                    MyDataAccessLayer.AddItemDefaultTags(oTelegram);
                                    ScadaClasses.CurrentScadaPopup = -1;
                                    ScadaClasses.Previouspage = -1;
                                    ScadaClasses.Refresh = true;
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
                        SKCanvasViews.Add(myAddItemButton);

                        y = y + FloatPanel.CornerRadius + 50;

                        var mySvgButton1 = new ScadaButton
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
                            ButtonText = "Input"
                        };
                        mySvgButton1.InputTransparent = false;
                        mySvgButton1.EnableTouchEvents = true;
                        mySvgButton1.Touch += (sender, args) =>
                        {
                            var pt = args.Location;
                            switch (args.ActionType)
                            {
                                case SKTouchAction.Pressed:                          
                                    ScadaClasses.CurrentScadaPopup = ScadaClasses.uxItemTagsMenu;
                                    ScadaClasses.Previouspage = -1;
                                    ScadaClasses.Refresh = true;                             
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
                            SKCanvasViews.Add(mySvgButton1);
                            y = y + 26;
                        }

                        var mySvgButton2 = new ScadaButton
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
                            ButtonText = "Visual"
                        };
                        mySvgButton2.InputTransparent = false;
                        mySvgButton2.EnableTouchEvents = true;
                        mySvgButton2.Touch += (sender, args) =>
                        {
                            var pt = args.Location;
                            switch (args.ActionType)
                            {
                                case SKTouchAction.Pressed:                              
                                    ScadaClasses.CurrentScadaPopup = ScadaClasses.uxItemTypeMenu;
                                    ScadaClasses.Previouspage = -1;
                                    ScadaClasses.Refresh = true;                       
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
                            SKCanvasViews.Add(mySvgButton2);
                            y = y + 26;
                        }

                        var mySvgButton3 = new ScadaButton
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
                                case SKTouchAction.Pressed:                                 
                                    ScadaClasses.CurrentScadaPopup = ScadaClasses.uxItemSizeMenu;
                                    ScadaClasses.Previouspage = -1;
                                    ScadaClasses.Refresh = true;                        
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
                            SKCanvasViews.Add(mySvgButton3);
                            y = y + 26;
                        }

                        var mySvgButton4 = new ScadaButton
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
                                case SKTouchAction.Pressed:
                                    ScadaClasses.CurrentScadaPopup = ScadaClasses.uxPageChangeMenu;
                                    ScadaClasses.Previouspage = -1;
                                    ScadaClasses.Refresh = true;                               
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
                            SKCanvasViews.Add(mySvgButton4);
                            y = y + 26;
                        }

                        mySvgButton1.EnableFaceFade();
                        mySvgButton2.EnableFaceFade();
                        mySvgButton3.EnableFaceFade();
                        mySvgButton4.EnableFaceFade();
                        Thread.Sleep(200);
                        break;

                    case ScadaClasses.uxItemTagsMenu:
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
                        SKCanvasViews.Add(FloatPanel11);


                        x = (Width / 100) * ScadaItem.Left;
                        y = (Height / 100) * ScadaItem.Top;
                        w = (Width / 100) * ScadaItem.Width;
                        h = (Height / 100) * ScadaItem.Height;

                        CreateCloseButton(x, y, w, h, ScadaClasses.CurrentScadaPopup);

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
                                            ItemType = ScadaItem.ItemType,
                                            ItemID = ScadaClasses.CurrentItem,
                                            TagID = row.TagID,
                                            TagName = ScadaItem.TagName,
                                            Action = ScadaItem.Action,
                                        };
                                        ScadaClasses.CurrentScadaPopup = ScadaClasses.uxTagsMenu;
                                        ScadaClasses.Previouspage = -1;
                                        ScadaClasses.CurrentRow = row.Id;
                                        ScadaClasses.Refresh = true;                                      
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
                                SKCanvasViews.Add(myItemTagsBtn);
                                y = y + 26;
                            }
                            Thread.Sleep(100);
                        }
                        break;

                    case ScadaClasses.uxItemSizeMenu:
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
                        SKCanvasViews.Add(FloatPanelSizes);

                        x = (Width / 100) * ScadaItem.Left;
                        y = (Height / 100) * ScadaItem.Top;
                        w = (Width / 100) * ScadaItem.Width;
                        h = (Height / 100) * ScadaItem.Height;

                        CreateCloseButton(x, y, w, h, ScadaClasses.CurrentScadaPopup);
                        ScadaItem.gridRows = MyDataAccessLayer.GetItemSizes(ScadaClasses.CurrentType);
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
                                            ItemType = ScadaClasses.CurrentType,
                                            ItemID = ScadaClasses.CurrentItem,
                                            TagID = row.TagID,
                                            TagName = ScadaItem.TagName,
                                            size = row.Row,
                                            Action = ScadaItem.Action,
                                        };
                                        ScadaClasses.CurrentScadaPopup = ScadaClasses.uxTagsMenu;
                                        ScadaClasses.Previouspage = -1;
                                        ScadaClasses.CurrentRow = row.Id;
                                        MyDataAccessLayer.SetItemSize(oTelegram);
                                        ScadaClasses.Refresh = true;
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
                                SKCanvasViews.Add(myItemTagsBtn);
                                y = y + 26;
                            }
                        }
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
                        SKCanvasViews.Add(FloatPanelAction);

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
                                case SKTouchAction.Pressed:
                                    ScadaClasses.CurrentScadaPopup = ScadaClasses.uxItemTagsMenu;
                                    ScadaClasses.Previouspage = -1;
                                    ScadaClasses.Refresh = true;
                            
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
                            SKCanvasViews.Add(mySvgButton10);
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
                                case SKTouchAction.Pressed:
                                    ScadaClasses.CurrentScadaPopup = ScadaClasses.uxItemTypeMenu;
                                    ScadaClasses.Previouspage = -1;
                                    ScadaClasses.Refresh = true;
                             
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
                            SKCanvasViews.Add(mySvgButton20);
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
                                case SKTouchAction.Pressed:
                                    ScadaClasses.CurrentScadaPopup = ScadaClasses.uxPagesMenu;
                                    ScadaClasses.Previouspage = -1;
                                    ScadaClasses.Refresh = true;                            
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
                            SKCanvasViews.Add(mySvgButton30);
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
                                case SKTouchAction.Pressed:                                 
                                    switch (ScadaClasses.CurrentRow)
                                    {
                                        case 0:
                                            MyDataAccessLayer.UpdateTagName(ScadaClasses.CurrentTag, edtInputText.Text);
                                            break;
                                        case 1:
                                            MyDataAccessLayer.UpdateTagDescription(ScadaClasses.CurrentTag, edtInputText.Text);
                                            break;
                                        case 2:
                                            MyDataAccessLayer.UpdateHL(ScadaClasses.CurrentTag, edtInputText.Text);
                                            break;
                                        case 3:
                                            MyDataAccessLayer.UpdateLL(ScadaClasses.CurrentTag, edtInputText.Text);
                                            break;
                                        case 4:
                                            MyDataAccessLayer.UpdateTagColor(ScadaClasses.CurrentTag, edtInputText.Text);
                                            break;
                                        case 5:
                                            MyDataAccessLayer.UpdateTagUnit(ScadaClasses.CurrentTag, edtInputText.Text);
                                            break;
                                        case 6:
                                            MyDataAccessLayer.UpdateStoreInterval(ScadaClasses.CurrentTag, edtInputText.Text);
                                            break;
                                        case 7:
                                            MyDataAccessLayer.UpdateAlarmEnable(ScadaClasses.CurrentTag, edtInputText.Text);
                                            break;
                                        case 8:
                                            MyDataAccessLayer.UpdateTagType(ScadaClasses.CurrentTag, edtInputText.Text);
                                            break;
                                      
                                    }
                                    ScadaClasses.Previouspage = -1;
                                    ScadaClasses.CurrentScadaPopup = ScadaClasses.uxTagSettings;
                                    ScadaClasses.Refresh = true;
                                    break;

                                case SKTouchAction.Released:
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

                        SKCanvasViews.Add(EditPanel);
                        SKCanvasViews.Add(btnApplyText);
                        CreateCloseButton(x, y, w, h, ScadaClasses.uxTagSettings);
                        break;

                    case ScadaClasses.uxEditParameterText:
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
                                case SKTouchAction.Pressed:
                                    switch (ScadaClasses.CurrentType)
                                    {
                                        case ScadaClasses.pmComputer:
                                              Preferences.Default.Set("ConnectionString", edtInputText.Text);
                                            break;
                                        case ScadaClasses.pmParameter:
                                              MyDataAccessLayer.UpdateParameterValue(ScadaClasses.CurrentID, edtInputText.Text);
                                            break;
                                        case ScadaClasses.pmPages:
                                              MyDataAccessLayer.UpdatePageName(ScadaClasses.CurrentID, edtInputText.Text);
                                            break;
                                    }                            
                                    ScadaClasses.Previouspage = -1;
                                    ScadaClasses.CurrentScadaPopup = ScadaClasses.PreviousScadaPopup;
                                    ScadaClasses.Refresh = true;
                                    break;

                                case SKTouchAction.Released:
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

                        SKCanvasViews.Add(EditParamPanel);
                        SKCanvasViews.Add(btnApplyParamText);
                        CreateCloseButton(x, y, w, h, ScadaClasses.PreviousScadaPopup);
                        break;



                    case ScadaClasses.uxTagsMenu:
                        ScadaTagsMenu();

                        /*
                        x = (Width / 100) * ScadaItem.Left;
                        y = (Height / 100) * ScadaItem.Top;
                        w = (Width / 100) * ScadaItem.Width;
                        h = (Height / 100) * ScadaItem.Height;
                        var FloatPanel5 = new ScadaButton();
                        FloatPanel5.ItemID = ScadaItem.ItemID;
                        FloatPanel5.StyleId = ScadaItem.ItemID.ToString();
                        FloatPanel5.AnchorX = 0;
                        FloatPanel5.AnchorY = 0;
                        FloatPanel5.CornerRadius = 10;
                        FloatPanel5.BarBackgroundColor = ScadaColor.uxBackGroundColor;
                        FloatPanel5.BackgroundColor = ScadaColor.uxBackGroundColor.ToMauiColor();
                        FloatPanel5.GradientStartColor = ScadaColor.uxPopupColor;
                        FloatPanel5.GradientEndColor = ScadaColor.uxPopupColor;
                        FloatPanel5.IndicatorColor = ScadaColor.uxPopupColor;

                        FloatPanel5.WidthRequest = (Width / 100) * ScadaItem.Width;
                        FloatPanel5.HeightRequest = (Height / 100) *ScadaItem.Height;
                        FloatPanel5.AlternativeTextColor = ScadaColor.uxTextColor;
                        FloatPanel5.TextColor = ScadaColor.uxTextColor;
                        FloatPanel5.IsEnabled = true;
                        FloatPanel5.IsVisible = true;
                        FloatPanel5.IndicatorType = 10;
                        AbsoluteLayout.SetLayoutBounds(FloatPanel5, new Rect(
                            (Width / 100) * ScadaItem.Left,
                            (Height / 100) * ScadaItem.Top,
                            (Width / 100) * ScadaItem.Width,
                            (Height / 100) * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(FloatPanel5, AbsoluteLayoutFlags.None);
                        SKCanvasViews.Add(FloatPanel5);


                        x = (Width / 100) * ScadaItem.Left;
                        y = (Height / 100) * ScadaItem.Top;
                        w = (Width / 100) * ScadaItem.Width;
                        h = (Height / 100) * ScadaItem.Height;
                        //AddTagToUxItem(ScadaItem.ItemID, ScadaItem.ItemType, ScadaItem.TagID, 1, "SV");

                        CreateCloseButton(x, y, w, h, -1);
                        CreateFwdButton(x, y, w, h);
                        CreateRwdButton(x, y, w, h);
                        CreateRemoveTagButton(ScadaClasses.CurrentItem, ScadaClasses.CurrentTag, x - (w / 2), y, w, h );

                        //int Dig = MyDataAccessLayer.GetItemIsDigital(ScadaClasses.CurrentItem);
                        
                        int TypeOfTag = 1;
                        if ((ScadaClasses.CurrentType == ScadaClasses.uxToggle) ||
                            (ScadaClasses.CurrentType == ScadaClasses.uxButton) || 
                            (ScadaClasses.CurrentType == ScadaClasses.uxCircularProgress))
                            TypeOfTag = 1;
                        else
                            TypeOfTag = 3;

                        if (ScadaClasses.CurrentType == ScadaClasses.uxHistoryChart)
                        {
                            ScadaItem.gridRows = MyDataAccessLayer.ReadTags("", 0, iMenuOffsetRows, iMenuOffsetRows + 5);
                        }
                        else
                        {
                            ScadaItem.gridRows = MyDataAccessLayer.ReadTags("", TypeOfTag, iMenuOffsetRows, iMenuOffsetRows + 5);
                        }

                        y = y + FloatPanel5.CornerRadius + 50;
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

                                        if (ScadaClasses.CurrentType == ScadaClasses.uxHistoryChart)
                                        {
                                            if (ScadaClasses.CurrentTag == 0)
                                            {
                                                MyDataAccessLayer.AddItemTag(ScadaItem.ItemID, ScadaItem.ItemType, row.TagID, ScadaClasses.CurrentRow, "");
                                            }
                                            else
                                            {
                                                MyDataAccessLayer.SetItemTag(ScadaItem.ItemID, row.TagID, ScadaClasses.CurrentRow);
                                            }
                                        }
                                        else
                                        {
                                            MyDataAccessLayer.SetItemTag(ScadaItem.ItemID, row.TagID, ScadaClasses.CurrentRow);
                                        }
                                        ScadaClasses.CurrentTag = row.TagID;
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
                                        myButton.GradientStartColor = ScadaColor.uxPopupItemColor;
                                        myButton.GradientEndColor = ScadaColor.uxPopupItemColor;
                                        myButton.IndicatorColor = GetStatusColor(row.Status);
                                        break;
                                }
                                args.Handled = true;
                            };

                            AbsoluteLayout.SetLayoutBounds(myButton, new Rect(x, y, FloatPanel5.WidthRequest, 20));
                            AbsoluteLayout.SetLayoutFlags(myButton, AbsoluteLayoutFlags.None);
                            if (y < ((Height / 100) * ScadaItem.Top + (Height / 100) * ScadaItem.Height) - 26)
                            {
                                SKCanvasViews.Add(myButton);
                                y = y + 26;
                            }
                            Thread.Sleep(100);
                        }
                        */
                        break;

                    case ScadaClasses.uxTimeSpanMenu:
                        ScadaTimeSpanMenu();

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
                        SKCanvasViews.Add(FloatPanel3);


                        x = (Width / 100) * ScadaItem.Left;
                        y = (Height / 100) * ScadaItem.Top;
                        w = (Width / 100) * ScadaItem.Width;
                        h = (Height / 100) * ScadaItem.Height;                       
                        ScadaItem.gridRows = MyDataAccessLayer.ReadItemTypes(sFilter, iMenuOffsetRows, iMenuOffsetRows + 5);

                        //Next and Previous buttons 
                        CreateCloseButton(x, y, w, h, ScadaClasses.CurrentScadaPopup);
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
                                            ItemID = ScadaClasses.CurrentItem,
                                            TagID = row.TagID,
                                            TagName = ScadaItem.TagName,
                                            Action = ScadaItem.Action,
                                        };
                                        MyDataAccessLayer.SetItemType(oTelegram);
                                        MyDataAccessLayer.AddItemDefaultTags(oTelegram);

                                        if (oTelegram.ItemType == ScadaClasses.uxButton)
                                        {
                                            ScadaClasses.CurrentScadaPopup = ScadaClasses.uxItemAction;
                                        }
                                        else
                                        {
                                            ScadaClasses.CurrentScadaPopup = -1;
                                        }

                                        ScadaClasses.Previouspage = -1;
                                        ScadaClasses.Refresh = true;
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
                                SKCanvasViews.Add(myButton);
                                y = y + 26;
                            }
                            Thread.Sleep(200);
                        }
                        break;

                        case ScadaClasses.uxPagesMenu:
                        ScadaPages();
                        /*
                        var FloatPanelPages = new ScadaButton();
                        FloatPanelPages.ItemID = ScadaItem.ItemID;
                        FloatPanelPages.StyleId = ScadaItem.ItemID.ToString();
                        FloatPanelPages.AnchorX = 0;
                        FloatPanelPages.AnchorY = 0;
                        FloatPanelPages.CornerRadius = 10;
                        FloatPanelPages.BarBackgroundColor = ScadaColor.uxBackGroundColor;
                        FloatPanelPages.BackgroundColor = ScadaColor.uxBackGroundColor.ToMauiColor();
                        FloatPanelPages.GradientStartColor = ScadaColor.uxPopupColor;
                        FloatPanelPages.GradientEndColor = ScadaColor.uxPopupColor;
                        FloatPanelPages.IndicatorColor = ScadaColor.uxBackGroundColor;
                        FloatPanelPages.WidthRequest = wScale * ScadaItem.Width;
                        FloatPanelPages.HeightRequest = hScale * ScadaItem.Height;
                        FloatPanelPages.AlternativeTextColor = ScadaColor.uxTextColor;
                        FloatPanelPages.TextColor = ScadaColor.uxTextColor;
                        FloatPanelPages.IsEnabled = true;
                        FloatPanelPages.IsVisible = true;
                        FloatPanelPages.IndicatorType = 10;
                        AbsoluteLayout.SetLayoutBounds(FloatPanelPages, new Rect(wScale * ScadaItem.Left, hScale * ScadaItem.Top, wScale * ScadaItem.Width, hScale * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(FloatPanelPages, AbsoluteLayoutFlags.None);
                        SKCanvasViews.Add(FloatPanelPages);

                        x = (Width / 100) * ScadaItem.Left;
                        y = (Height / 100) * ScadaItem.Top;
                        w = (Width / 100) * ScadaItem.Width;
                        h = (Height / 100) *ScadaItem.Height;

                        var gridRows = new List<gridRow>();
                        int Page = 1;
                        int MaxPage = MyDataAccessLayer.GetMaxPageNo();
                        while (Page < MaxPage)
                        {
                            gridRows.Add(new gridRow
                            {
                                TagID = Page,
                                col1text = Page.ToString(),
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
                            Page++;
                        }
                        ScadaItem.gridRows = gridRows;
                        CreateCloseButton(x, y, w, h, ScadaClasses.CurrentScadaPopup);
                        CreateFwdButton(x, y, w, h);
                        CreateRwdButton(x, y, w, h);

                        var myAddPageButton = new ScadaButton
                        {
                            WidthRequest = 20,
                            HeightRequest = 20,
                            Background = ScadaColor.uxBackGroundColor.ToMauiColor(),
                            BarBackgroundColor = ScadaColor.uxBackGroundColor,
                            IndicatorType = 3,

                            GradientStartColor = ScadaColor.uxItemColor,
                            GradientEndColor = ScadaColor.uxItemColor,
                            CornerRadius = 10,
                            TextColor = ScadaColor.uxTextColor,
                            FontSize = 21.5F,
                            SvgBase64 = MyDataAccessLayer.LoadLibItem("GreenPlus", 1),
                            ButtonText = ""
                        };
                        myAddPageButton.IsVisible = true;
                        myAddPageButton.InputTransparent = false;
                        myAddPageButton.EnableTouchEvents = true;
                        myAddPageButton.Touch += (sender, args) =>
                        {
                            var pt = args.Location;
                            switch (args.ActionType)
                            {
                                case SKTouchAction.Pressed:
                                    ScadaClasses.Telegram oTelegram = new ScadaClasses.Telegram()
                                    {
                                        MessageType = ScadaItem.MessageType,
                                        Page = ScadaClasses.Currentpage,
                                        ItemType = ScadaClasses.CurrentType,
                                        ItemID = ScadaClasses.CurrentItem,
                                        TagID = ScadaItem.TagID,
                                        TagName = ScadaItem.TagName,
                                        Action = ScadaItem.Action,
                                    };

                                    MyDataAccessLayer.addPage();
                                    ScadaClasses.CurrentScadaPopup = -1;
                                    ScadaClasses.Previouspage = -1;
                                    ScadaClasses.Refresh = true;
                                    break;

                                case SKTouchAction.Moved:
                                    myAddPageButton.GradientStartColor = ScadaColor.uxHoverColor;
                                    myAddPageButton.GradientEndColor = ScadaColor.uxHoverColor;
                                    break;

                                case SKTouchAction.Exited:
                                    myAddPageButton.GradientStartColor = ScadaColor.uxItemColor;
                                    myAddPageButton.GradientEndColor = ScadaColor.uxItemColor;
                                    break;
                            }
                            args.Handled = true;
                        };
                        AbsoluteLayout.SetLayoutBounds(myAddPageButton, new Rect(x + 4, y + 24, 20, 20));
                        AbsoluteLayout.SetLayoutFlags(myAddPageButton, AbsoluteLayoutFlags.None);
                        SKCanvasViews.Add(myAddPageButton);

                        y = y + FloatPanelPages.CornerRadius + 50;
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
                                            ItemID = ScadaClasses.CurrentItem,
                                            TagID = row.TagID,
                                            TagName = ScadaItem.TagName,
                                            Action = ScadaItem.Action,
                                        };
                                        ScadaClasses.Currentpage = row.TagID;
                                        MyDataAccessLayer.SetNextPage(ScadaClasses.CurrentItem, ScadaClasses.Currentpage);

                                        ScadaClasses.CurrentScadaPopup = -1;
                                        ScadaClasses.Refresh = true;
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
                                SKCanvasViews.Add(myButton);
                                y = y + 26;
                            }
                        }
                        */
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
                        SKCanvasViews.Add(FloatPanelChangePage);

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

                        CreateCloseButton(x, y, w, h, ScadaClasses.CurrentScadaPopup);
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
                                            ItemID = ScadaClasses.CurrentItem,
                                            TagID = row.TagID,
                                            TagName = ScadaItem.TagName,
                                            Action = ScadaItem.Action,
                                        };
                                        ScadaClasses.Currentpage = row.TagID;
                                        MyDataAccessLayer.SetItemPage(ScadaClasses.CurrentItem, ScadaClasses.Currentpage);
                                        ScadaClasses.CurrentScadaPopup = -1;
                                        ScadaClasses.Refresh = true;
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
                                SKCanvasViews.Add(myButton);
                                y = y + 26;
                            }
                        }
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
                        AbsoluteLayout.SetLayoutBounds(SKLine, new Rect(wScale * ScadaItem.Left, hScale * ScadaItem.Top, wScale * ScadaItem.Width, hScale * ScadaItem.Height));
                        AbsoluteLayout.SetLayoutFlags(SKLine, AbsoluteLayoutFlags.None);
                        SKCanvasViews.Add(SKLine);
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
            btnHome.IndicatorType = 3;
            btnHome.Margin = 0.15F;
            btnHome.ButtonText = "";
            btnHome.Touch += (sender, args) =>
            {
                switch (args.ActionType)
                {
                    case SKTouchAction.Released:
                        ScadaClasses.Previouspage = -1;
                        ScadaClasses.Currentpage = 1;
                        ScadaClasses.CurrentScadaPopup = -1;
                        ScadaClasses.Refresh = true;
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
            SKCanvasPopupViews.Add(btnHome);

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
            btnAlarmbell.IndicatorType = 3;
            btnAlarmbell.Margin = 0.15F;
            btnAlarmbell.ButtonText = "";
            btnAlarmbell.Touch += (sender, args) =>
            {
                switch (args.ActionType)
                {
                    case SKTouchAction.Released:
                        ScadaClasses.Previouspage = -1;
                        ScadaClasses.CurrentScadaPopup = ScadaClasses.uxAlarmGrid;                        
                        ScadaClasses.Refresh = true;
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
            SKCanvasPopupViews.Add(btnAlarmbell);

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
            btnSettings.IndicatorType = 3;
            btnSettings.Margin = 0.15F;
            btnSettings.ButtonText = "";
            btnSettings.Touch += (sender, args) =>
            {
                switch (args.ActionType)
                {
                    case SKTouchAction.Released:
                        ScadaClasses.Previouspage = -1;
                        ScadaClasses.CurrentScadaPopup = ScadaClasses.uxParameters;                       
                        ScadaClasses.Refresh = true;
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
            SKCanvasPopupViews.Add(btnSettings);


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

            if (ScadaClasses.CurrentScadaPopup == ScadaClasses.uxUploadMenu)
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
                AbsoluteLayout.SetLayoutBounds(edtSvgName, new Rect(x+20, y+40, edtSvgName.WidthRequest, edtSvgName.HeightRequest));
                AbsoluteLayout.SetLayoutFlags(edtSvgName, AbsoluteLayoutFlags.None);
                edtSvgName.TextChanged += OnEditorTextChanged;
                edtSvgName.Completed += OnEditorCompleted;
                absoluteLayout.Add(edtSvgName);

       
                edtSvgEditor.Placeholder = "Paste your SVG text";
                edtSvgEditor.WidthRequest = w-40;
                edtSvgEditor.HeightRequest = h/2F;
                edtSvgEditor.TextColor = ScadaColor.uxTextColor.ToMauiColor();
                edtSvgEditor.BackgroundColor = ScadaColor.uxPopupItemColor.ToMauiColor();
                edtSvgEditor.PlaceholderColor = ScadaColor.uxItemColor.ToMauiColor();
                edtSvgEditor.FontAttributes = FontAttributes.None;
                edtSvgEditor.IsVisible = true;
                AbsoluteLayout.SetLayoutBounds(edtSvgEditor, new Rect(x+20, y+75, edtSvgEditor.WidthRequest, edtSvgEditor.HeightRequest));
                AbsoluteLayout.SetLayoutFlags(edtSvgEditor, AbsoluteLayoutFlags.None);
                edtSvgEditor.TextChanged += OnEditorTextChanged;
                edtSvgEditor.Completed += OnEditorCompleted;
                absoluteLayout.Add(edtSvgEditor);
            }

            if ((ScadaClasses.CurrentScadaPopup == ScadaClasses.uxEditTagText) ||
                (ScadaClasses.CurrentScadaPopup == ScadaClasses.uxEditParameterText))
            {
                edtInputText.Placeholder = "EditText";
                edtInputText.WidthRequest = w - 40;
                edtInputText.HeightRequest = h/2;

                edtInputText.TextColor = ScadaColor.uxTextColor.ToMauiColor();
                edtInputText.BackgroundColor = ScadaColor.uxPopupItemColor.ToMauiColor();
                edtInputText.PlaceholderColor = ScadaColor.uxItemColor.ToMauiColor();
                edtInputText.FontAttributes = FontAttributes.Bold;
                edtInputText.IsVisible = true;
                AbsoluteLayout.SetLayoutBounds(edtInputText, new Rect(x+20, y+70, edtInputText.WidthRequest, edtInputText.HeightRequest));
                AbsoluteLayout.SetLayoutFlags(edtInputText, AbsoluteLayoutFlags.None);
                edtInputText.TextChanged += OnEditorTextChanged;
                edtInputText.Completed += OnEditorCompleted;
                absoluteLayout.Add(edtInputText);
            }

            Content = absoluteLayout;
            ScadaClasses.Previouspage = ScadaClasses.Currentpage;
        }

        foreach (SKCanvasView uxItem in SKCanvasViews)
        {
            if (uxItem is ScadaSvg)
            {
                var dItem = uxItem as ScadaSvg;
               /* AbsoluteLayout.SetLayoutBounds(dItem, new Rect(
                                          (Width / 100) * dItem.XPosition,
                                          (Height / 100) * dItem.YPosition,
                                          (Width / 100) * dItem.Width,
                                          (Height / 100) * dItem.Height));
                */
            }

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

            /*
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
            */



      
            if (uxItem is ScadaButton)
            {
                var dItem = uxItem as ScadaButton;
               
                foreach (var Item in ScadaItems)
                {
                    if (dItem.ItemID == Item.ItemID)
                    {
                        dItem.RefreshValues(Item.ItemValues);
                    }
                }

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
                
                
                     

                    foreach (var ScadaItem in ScadaItems)
                    {
                        if (dItem != null)
                        {
                            if ((ScadaItem.ItemType == ScadaClasses.uxAlarmGrid) ||
                                (ScadaItem.ItemType == ScadaClasses.uxTagsGrid) ||
                                (ScadaItem.ItemType == ScadaClasses.uxParameters) ||
                              //  (ScadaItem.ItemType == ScadaClasses.uxDataSourcesSettings) ||
                                (ScadaItem.ItemType == ScadaClasses.uxTagSettings))
                            {
                                          
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
                                foreach (gridRow myRow in ScadaItem.gridRows)
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

        foreach (SKCanvasView uxItem in SKCanvasViews)
        {
        }

    }

}

