using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace Scada
{
    public static class Cubic
    {
        /// Generate a smooth (interpolated) curve that follows the path of the given X/Y points
        public static (double[] xs, double[] ys) InterpolateXY(double[] xs, double[] ys, int count)
        {
            if (xs is null || ys is null || xs.Length != ys.Length)
                throw new ArgumentException($"{nameof(xs)} and {nameof(ys)} must have same length");

            int inputPointCount = xs.Length;
            double[] inputDistances = new double[inputPointCount];
            for (int i = 1; i < inputPointCount; i++)
            {
                double dx = xs[i] - xs[i - 1];
                double dy = ys[i] - ys[i - 1];
                double distance = Math.Sqrt(dx * dx + dy * dy);
                inputDistances[i] = inputDistances[i - 1] + distance;
            }

            double meanDistance = inputDistances.Last() / (count - 1);
            double[] evenDistances = Enumerable.Range(0, count).Select(x => x * meanDistance).ToArray();
            double[] xsOut = Interpolate(inputDistances, xs, evenDistances);
            double[] ysOut = Interpolate(inputDistances, ys, evenDistances);
            return (xsOut, ysOut);
        }

        private static double[] Interpolate(double[] xOrig, double[] yOrig, double[] xInterp)
        {
            (double[] a, double[] b) = FitMatrix(xOrig, yOrig);

            double[] yInterp = new double[xInterp.Length];
            for (int i = 0; i < yInterp.Length; i++)
            {
                int j;
                for (j = 0; j < xOrig.Length - 2; j++)
                    if (xInterp[i] <= xOrig[j + 1])
                        break;

                double dx = xOrig[j + 1] - xOrig[j];
                double t = (xInterp[i] - xOrig[j]) / dx;
                double y = (1 - t) * yOrig[j] + t * yOrig[j + 1] +
                    t * (1 - t) * (a[j] * (1 - t) + b[j] * t);
                yInterp[i] = y;
            }

            return yInterp;
        }

        private static (double[] a, double[] b) FitMatrix(double[] x, double[] y)
        {
            int n = x.Length;
            double[] a = new double[n - 1];
            double[] b = new double[n - 1];
            double[] r = new double[n];
            double[] A = new double[n];
            double[] B = new double[n];
            double[] C = new double[n];

            double dx1, dx2, dy1, dy2;

            dx1 = x[1] - x[0];
            C[0] = 1.0f / dx1;
            B[0] = 2.0f * C[0];
            r[0] = 3 * (y[1] - y[0]) / (dx1 * dx1);

            for (int i = 1; i < n - 1; i++)
            {
                dx1 = x[i] - x[i - 1];
                dx2 = x[i + 1] - x[i];
                A[i] = 1.0f / dx1;
                C[i] = 1.0f / dx2;
                B[i] = 2.0f * (A[i] + C[i]);
                dy1 = y[i] - y[i - 1];
                dy2 = y[i + 1] - y[i];
                r[i] = 3 * (dy1 / (dx1 * dx1) + dy2 / (dx2 * dx2));
            }

            dx1 = x[n - 1] - x[n - 2];
            dy1 = y[n - 1] - y[n - 2];
            A[n - 1] = 1.0f / dx1;
            B[n - 1] = 2.0f * A[n - 1];
            r[n - 1] = 3 * (dy1 / (dx1 * dx1));

            double[] cPrime = new double[n];
            cPrime[0] = C[0] / B[0];
            for (int i = 1; i < n; i++)
                cPrime[i] = C[i] / (B[i] - cPrime[i - 1] * A[i]);

            double[] dPrime = new double[n];
            dPrime[0] = r[0] / B[0];
            for (int i = 1; i < n; i++)
                dPrime[i] = (r[i] - dPrime[i - 1] * A[i]) / (B[i] - cPrime[i - 1] * A[i]);

            double[] k = new double[n];
            k[n - 1] = dPrime[n - 1];
            for (int i = n - 2; i >= 0; i--)
                k[i] = dPrime[i] - cPrime[i] * k[i + 1];

            for (int i = 1; i < n; i++)
            {
                dx1 = x[i] - x[i - 1];
                dy1 = y[i] - y[i - 1];
                a[i - 1] = k[i - 1] * dx1 - dy1;
                b[i - 1] = -k[i] * dx1 + dy1;
            }

            return (a, b);
        }
    }


    /*Usage
    
    // generate sample data using a random walk
    Random rand = new(1268);
    int pointCount = 20;
    double[] xs1 = new double[pointCount];
    double[] ys1 = new double[pointCount];
    for (int i = 1; i < pointCount; i++)
    {
        xs1[i] = xs1[i - 1] + rand.NextDouble() - .5;
        ys1[i] = ys1[i - 1] + rand.NextDouble() - .5;
    }

    // Use cubic interpolation to smooth the original data
    (double[] xs2, double[] ys2) = Cubic.InterpolateXY(xs1, ys1, 200); 
     
    Using ScottPlot
    Plot the original vs. interpolated data
    var plt = new ScottPlot.Plot(600, 400);
    plt.AddScatter(xs1, ys1, label: "original", markerSize: 7);
    plt.AddScatter(xs2, ys2, label: "interpolated", markerSize: 3);
    plt.Legend();
    plt.SaveFig("interpolation.png");
     */



    /* 
     using ScottPlot;
     using ScottPlot.Maui;
     private void CreatePlot()
    {
        // Declare and configure the MauiPlot
        _mauiPlot = new MauiPlot
        {
            AutomationId = "MauiPlot1",   // equivalent to x:Name
            WidthRequest = 400,
            HeightRequest = 300,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        // Add some data
        _mauiPlot.Plot.Add.Signal(new double[] { 1, 4, 9, 16, 25 });
        _mauiPlot.Plot.Axes.AutoScale();

        // Add to a layout and set as page content
        var layout = new AbsoluteLayout();

        AbsoluteLayout.SetLayoutBounds(_mauiPlot, new Rect(0, 0, 400, 300));
        AbsoluteLayout.SetLayoutFlags(_mauiPlot, AbsoluteLayoutFlags.None);

        layout.Children.Add(_mauiPlot);

        Content = layout;

        _mauiPlot.Refresh();
    }
    */

    public  class ScadaClasses
    {
        public const int uxPanel = 0;
        public const int uxSvg = 1;
        public const int uxToggle = 2;
        public const int uxNumeric = 3;
        public const int uxButton = 4;
        public const int uxCircularProgress = 5;
        public const int uxCircularGauge = 6;
        public const int uxBarGraph = 7;
        public const int uxRobot = 8;
        public const int uxAlarmGrid = 9;
        public const int uxHistoryChart = 10;

        //Aircrafts
        public const int uxGyro = 11;
        public const int uxCompass = 12;
        public const int uxAltimeter = 13;
        public const int uxAirspeed = 14;
         
        //Mapsui
        public const int uxMovingMap = 15; 
       
        //Simulation,demo testing
        public const int uxController = 16;
        public const int uxSimulator = 17;
        public const int uxInterference = 18;
        
        //Automotive
        public const int uxAutomotivePower = 19;
        public const int uxAutomotiveSpeed = 20;
        
        // Extra
        public const int uxProgressBar = 21;
        public const int uxArrow = 22;
        public const int uxLine = 23;
        public const int uxText = 24;
        public const int uxDonutChart = 25;
        
        //Grids
        public const int uxGridMessage = 26;
        public const int uxGridStringCmd = 27;
        public const int uxGridItemCmd = 28;
        public const int uxGridRowCmd = 29;
        public const int uxChatMessages = 30;
        public const int uxFloatingMenu = 34;

        //Animation
        public const int uxAnimation = 36;
        public const int uxMovingItem = 37;
        public const int ux3D = 90;

        //Menus
        public const int uxLoginMenu = 38;
        public const int uxDesignMenu = 39;
        public const int uxConfigMenu = 40;
        public const int uxItemTypeMenu = 41;
        public const int uxTagsMenu = 42;
        public const int uxTimeSpanMenu = 43;
        public const int uxItemTagsMenu = 44;
        public const int uxSvgPopupMenu = 45;
        public const int uxPagesMenu = 46;
        public const int uxPageChangeMenu = 47;
        public const int uxItemAction = 48;
        public const int uxUploadMenu = 49;

        public const int uxTagsGrid = 50;
        public const int uxTagSettings = 51;
        public const int uxItemSizeMenu = 52;
        public const int uxEditTagText = 53;
        public const int uxParameters = 54;
        public const int uxEditParameterText = 55;
        public const int uxProtocols = 56;
        public const int uxProtocolSettings = 57;
        

        public const int CmdInvalid = -1;
        public const int Cmdgetpictures = 1;
        public const int Cmdgettags = 2;
        public const int Cmdgetstyles = 3;
        public const int Cmdgetitems = 4;
        public const int CmdupdateSV = 21;
        public const int CmdsendCommand = 29;
        public const int Cmdloadpictures = 34;
        public const int Cmdscadalogin = 36;
        public const int Cmdloadtags = 38;
        public const int Cmdgetgriddata = 30;
        public const int Cmdgriddata = 41;
        public const int Cmdendofpackets = 99;

        public int Currentpage = 1;
        public int CurrentScadaPopup = -1;
        
        public  int CurrentItem = 0;
        public  int CurrentRow = 0;
        public  int CurrentType = 0;
        public  int CurrentTag = 0;
        public  int CurrentID = -1;
        public int CurrentTagSequence = 0;
        public  int CurrentCategory = -1;

        public  int Previouspage = -1;
        public  int PreviousScadaPopup = -1;
        

        //Parameter categories
        public const int CategoryAdam = 101;
        public const int CategoryMQTT = 102;
        public const int CategoryOpcUA = 103;
        public const int CategorySimulator = 103;


        public const int tsNow = 0;
        public const int tsHour = 1;
        public const int tsDay = 2;
        public const int tsWeek = 3;
        public const int tsMonth = 4;
        public const int tsYear = 5;

        public const int infoNormal = 0;
        public const int infoStartup = 1;

        public const int pmComputer = 1;
        public const int pmParameter = 2;
        public const int pmPages = 3;

        public const int chAnalogInput = 1;
        public const int chAnalogOutput = 2;
        public const int chDigitalInput = 3;
        public const int chDigitalOutput = 4;

        public class Adam
        {
            public int TagID { get; set; }
            public int Adress { get; set; }
            public int Channel { get; set; }
            public int ChType { get; set; }
        }


        public class Tag
        {
            public int TagID { get; set; }
            public double Value { get; set; }
            public double HL { get; set; }
            public double LL { get; set; }
            public string Color { get; set; } = "";
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public string Unit { get; set; } = "";
            public int TypeOfTag { get; set; }
            public int AlarmEnable { get; set; }
            public int StoreIntervalSec { get; set; }
            public int Driver { get; set; }
            public int TagSequence { get; set; }
        }

        public class LibItem
        {
            public string ?sTypeInLib { get; set; }
            public string ?sNameInLib { get; set; }
            public string ?sBase64 { get; set; }
        }

        public class ChartSetting
        {
            public int iSpan { get; set; }
            public int iYear { get; set; }
            public int iWeek { get; set; }
            public int iMonth { get; set; }
            public int iDay { get; set; }
            public int iHour { get; set; }
            public int iMinute { get; set; }

        }


        public class Value
        {
            public string X { get; set; }
            public string Y { get; set; }
            public string Color { get; set; }
            public int TypeOfTag { get; set; }
            public double Max { get; set; }
            public double Min { get; set; }
            public int TimeSpan { get; set; }
        }

        public class SystemColors
        {
            public SKColor uxBackGroundColor = new SKColor();
            public SKColor uxItemBackGroundColor = new SKColor();
            public SKColor uxPanelColor = new SKColor();
            public SKColor uxPopupColor = new SKColor();
            public SKColor uxPopupItemColor = new SKColor();
            public SKColor uxItemColor = new SKColor();
            public SKColor uxGradientStartColor = new SKColor();
            public SKColor uxGradientEndColor = new SKColor();
            public SKColor uxTouchColor = new SKColor();
            public SKColor uxHoverColor = new SKColor();
            public SKColor uxTextColor = new SKColor();
            public SKColor uxOffColor = new SKColor();
            public SKColor uxLightColor = new SKColor();
            public SKColor uxGridThinColor = new SKColor();
            public SKColor uxGridFatColor = new SKColor();
            public SKColor uxTransparentButtonColor = new SKColor();
        }

        public class Telegram
        {
            public int PacketSeq { get; set; }
            public int MessageType { get; set; }
            public int ItemID { get; set; }
            public int TagID { get; set; }
            public string ?TagName { get; set; }
            public int Page { get; set; }
            public int ItemType { get; set; }
            public double Left { get; set; }
            public double Top { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
            public string ?PV { get; set; }                 
            public string ?HL { get; set; }                
            public string ?LL { get; set; }                 
            public string ?SV { get; set; }                  
            public string ?Max { get; set; }
            public string ?Min { get; set; }
            public string ?Unit { get; set; }                
            public string ?Color { get; set; }               
            public string ?Analyze { get; set; }                   
            public string ?Status { get; set; }
            public string ?Text { get; set; }
            public string ?Time { get; set; }
            public int Hoover { get; set; }
            public int Fade { get; set; }
            public double Radius { get; set; }       
            public int Nextpage { get; set; }
            public string Action { get; set; } = string.Empty;
            public int size { get; set; }        
            public List<List<Value>> ListOfValues = new List<List<Value>>() { };
            public List<gridRow> ?gridRows { get; set; }
            public List<ItemValue> ?ItemValues { get; set; }
  
        }
    }

 /*
public static SKColor HexStringToColor(string hexColor)
{
    string hc = ExtractHexDigits(hexColor);
    string r = hc.Substring(0, 2);
    string g = hc.Substring(2, 2);
    string b = hc.Substring(4, 2);
    string a = hc.Substring(6, 2);
    SKColor color;
    try
    {
        byte ri = Byte.Parse(r, NumberStyles.HexNumber);
        byte gi = Byte.Parse(g, NumberStyles.HexNumber);
        byte bi = Byte.Parse(b, NumberStyles.HexNumber);
        byte ai = Byte.Parse(a, NumberStyles.HexNumber);
        color = new SKColor(ri, gi, bi, ai);
    }
    catch
    {
        color = SKColor.Empty;
    }
    return color;
}

public static string ExtractHexDigits(string input)
{
    // remove any characters that are not digits (like #)
    var isHexDigit = new Regex("[abcdefABCDEF\\d]+", RegexOptions.Compiled);
    string newnum = "";
    foreach (char c in input)
    {
        if (isHexDigit.IsMatch(c.ToString()))
            newnum += c.ToString();
    }
    return newnum;
}
*/

}
