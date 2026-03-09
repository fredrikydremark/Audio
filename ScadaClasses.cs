using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace Scada
{
    public class ScadaClasses
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
        
        public const int uxProgressBar = 21;
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
        public const int uxEditText = 53;
        public const int uxParameters = 54;

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

        public static int Currentpage = 1;
        public static int CurrentScadaPopup = -1;
        public static int CurrentItem = 0;
        public static int CurrentRow = 0;
        public static int CurrentType = 0;
        public static int CurrentTag = 0;
        public static int Previouspage = -1;
        public static bool Refresh = false;

        /*

        public class Param
        {
            public string Data { get; set; }
            public int TypeOfData { get; set; }

        }
        */
        /*
   public class PopupRow
   {
       public int ItemID { get; set; }
       public int TagID { get; set; }
       public int Left { get; set; }
       public int Top { get; set; }
       public int Width { get; set; }
       public int Height { get; set; }
       public int ItemType { get; set; }
       public string InputType { get; set; }
       public string GridAction { get; set; }
       public string Action { get; set; }
       public string Text { get; set; }
       public string Destination { get; set; }
       public string DestinationType { get; set; }
       public string Value { get; set; }
   }
   */

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
        }

        public class LibItem
        {
            public string sTypeInLib { get; set; }
            public string sNameInLib { get; set; }
            public string sBase64 { get; set; }
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

        /*
        public List<ScadaClasses.LibItem> LibItems = new List<ScadaClasses.LibItem>();

        
        public class ScadaControlTelegram
        {
            public int MessageType { get; set; }
            public int ItemID { get; set; }
            public int TagID { get; set; }
            public string TagName { get; set; }
            public int Page { get; set; }
            public int ItemType { get; set; }
            public string Action { get; set; }
            public double SV { get; set; }
        }
        */

        public class Value
        {
            public string X { get; set; }
            public string Y { get; set; }
            public string Color { get; set; }
            public int TypeOfTag { get; set; }
        }

        public class Colors
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
}
