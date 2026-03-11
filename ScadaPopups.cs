using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace Scada
{ 
    public class ScadaPopups
    {
        public List<ScadaClasses.Telegram> AddCurrentPopup(int CurrentScadaPopup,int Currentpage,int CurrentTag,int CurrentItem,double Width,double Height,
                                                           double Xpos,double Ypos,double Xwidth,double Yheight, List<ScadaClasses.Telegram> ScadaItems)
        {
            double panelWith = 0.4;
            double panelHeight = 0.55;
            double x = (Width / 2) - ((panelWith * Width)) / 2;
            double y = (Height / 2) - ((panelHeight * Height)) / 2;
            double w = (panelWith * Width);
            double h = (panelHeight * Height);
            DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);

            if (CurrentScadaPopup == ScadaClasses.uxPagesMenu)
            {
                ScadaClasses.Telegram oMenuTelegram = new ScadaClasses.Telegram()
                {
                    MessageType = 0,
                    Top = (((Ypos / Height) * 100) + 1.7 + (Yheight / Width) * 100),
                    Left = (((Xpos / Width) * 100) + ((Xwidth / Width) * 100) / 5),
                    Width = 20,
                    Height = 36,
                    Page = Currentpage,
                    ItemType = ScadaClasses.uxPagesMenu,
                    ItemID = -1,
                    TagID = 4,
                    TagName = " ",
                    Action = " ",
                    PV = "0.0",
                    SV = "0.0",
                    Text = " ",
                    Radius = 10
                };
                ScadaItems.Add(oMenuTelegram);
            }

            if (CurrentScadaPopup == ScadaClasses.uxPageChangeMenu)
            {
                ScadaClasses.Telegram oMenuTelegram = new ScadaClasses.Telegram()
                {
                    MessageType = 0,//Ändras todo
                    Top = (((Ypos / Height) * 100) + 1.7 + (Yheight / Width) * 100),
                    Left = (((Xpos / Width) * 100) + ((Xwidth / Width) * 100) / 5),
                    Width = 20,
                    Height = 36,
                    Page = Currentpage,
                    ItemType = ScadaClasses.uxPageChangeMenu,
                    ItemID = -1,
                    TagID = 4,
                    TagName = " ",
                    Action = " ",
                    PV = "0.0",
                    SV = "0.0",
                    Text = " ",
                    Radius = 10
                };
                ScadaItems.Add(oMenuTelegram);
            }

            if (CurrentScadaPopup == ScadaClasses.uxConfigMenu)
            {
                ScadaClasses.Telegram oMenuTelegram = new ScadaClasses.Telegram()
                {
                    MessageType = 0,
                    Top = (((Ypos / Height) * 100) + 1.7 + (Yheight / Width) * 100),
                    Left = (((Xpos / Width) * 100) + ((Xwidth / Width) * 100) / 5),
                    Width = 20,
                    Height = 30,
                    Page = 2000,
                    ItemType = ScadaClasses.uxConfigMenu,
                    ItemID = 599,
                    TagID = 4,
                    TagName = " ",
                    Action = " ",
                    PV = "0.0",
                    SV = "0.0",
                    Text = " ",
                    Radius = 10
                };
                ScadaItems.Add(oMenuTelegram);
            }

            if (CurrentScadaPopup == ScadaClasses.uxItemTagsMenu)
            {
                ScadaClasses.Telegram oMenuTelegram = new ScadaClasses.Telegram()
                {
                    MessageType = 0,
                    Top = (((Ypos / Height) * 100) + 1.7 + (Yheight / Width) * 100),
                    Left = (((Xpos / Width) * 100) + ((Xwidth / Width) * 100) / 5),
                    Width = 20,
                    Height = 36,
                    Page = Currentpage,
                    ItemType = ScadaClasses.uxItemTagsMenu,
                    ItemID = CurrentItem,
                    TagID = -1,
                    TagName = " ",
                    Action = " ",
                    PV = "0.0",
                    SV = "0.0",
                    Text = " ",
                    Radius = 10
                };
                ScadaItems.Add(oMenuTelegram);
            }

            if (CurrentScadaPopup == ScadaClasses.uxTagsMenu)
            {
                ScadaClasses.Telegram oMenuTelegram = new ScadaClasses.Telegram()
                {
                    MessageType = 0,
                    Top = (((Ypos / Height) * 100) + 1.7 + (Yheight / Width) * 100),
                    Left = (((Xpos / Width) * 100) + ((Xwidth / Width) * 100) / 5),
                    Width = 20,
                    Height = 40,
                    Page = Currentpage,
                    ItemType = ScadaClasses.uxTagsMenu,
                    ItemID = CurrentItem,
                    TagID = 4,
                    TagName = " ",
                    Action = " ",
                    PV = "0.0",
                    SV = "0.0",
                    Text = " ",
                    Radius = 10
                };
                ScadaItems.Add(oMenuTelegram);
            }


            if (CurrentScadaPopup == ScadaClasses.uxTagsGrid)
            {            
                ScadaClasses.Telegram oMenuTelegram = new ScadaClasses.Telegram()
                {
                    MessageType = 0,
                    Top = 20,
                    Left = 30,
                    Width = 20,
                    Height = 55,
                    Page = Currentpage,
                    ItemType = ScadaClasses.uxTagsGrid,
                    ItemID = 777,
                    TagID = 4,
                    TagName = " ",
                    Action = " ",
                    PV = "0.0",
                    SV = "0.0",
                    Text = " ",
                    Radius = 10,
                    gridRows = MyDataAccessLayer.LoadTags()
                };
                ScadaItems.Add(oMenuTelegram);
            }

            if (CurrentScadaPopup == ScadaClasses.uxParameters)
            {
                ScadaClasses.Telegram oMenuTelegram = new ScadaClasses.Telegram()
                {
                    MessageType = 0,
                    Top = 20,
                    Left = 30,
                    Width = 20,
                    Height = 55,
                    Page = Currentpage,
                    ItemType = ScadaClasses.uxParameters,
                    ItemID = 777,
                    TagID = 4,
                    TagName = " ",
                    Action = " ",
                    PV = "0.0",
                    SV = "0.0",
                    Text = " ",
                    Radius = 10,
                    gridRows = MyDataAccessLayer.ReadParameters("",0,5)
                };
                ScadaItems.Add(oMenuTelegram);
            }

            if (CurrentScadaPopup == ScadaClasses.uxTagSettings)
            {
                ScadaClasses.Telegram oMenuTelegram = new ScadaClasses.Telegram()
                {
                    MessageType = 0,
                    Top = 20,
                    Left = 30,
                    Width = 40,
                    Height = 55,
                    Page = Currentpage,
                    ItemType = ScadaClasses.uxTagSettings,
                    ItemID = 888,
                    TagID = CurrentTag,
                    TagName = " ",
                    Action = " ",
                    PV = "0.0",
                    SV = "0.0",
                    Text = " ",
                    Radius = 10,
                    gridRows = MyDataAccessLayer.GetTagParams(CurrentTag)
                };
                ScadaItems.Add(oMenuTelegram);
            }

            if (CurrentScadaPopup == ScadaClasses.uxEditText)
            {
                ScadaClasses.Telegram oMenuTelegram = new ScadaClasses.Telegram()
                {
                    MessageType = 0,
                    Top = 20,
                    Left = 30,
                    Width = 40,
                    Height = 55,
                    Page = Currentpage,
                    ItemType = ScadaClasses.uxEditText,
                    ItemID = 888,
                    TagID = CurrentTag,
                    TagName = " ",
                    Action = " ",
                    PV = "0.0",
                    SV = "0.0",
                    Text = " ",
                    Radius = 10
                };
                ScadaItems.Add(oMenuTelegram);
            }


            if (CurrentScadaPopup == ScadaClasses.uxItemSizeMenu)
            {
                ScadaClasses.Telegram oMenuTelegram = new ScadaClasses.Telegram()
                {
                    MessageType = 0,//Ändras todo
                    Top = (((Ypos / Height) * 100) + 1.7 + (Yheight / Width) * 100),
                    Left = (((Xpos / Width) * 100) + ((Xwidth / Width) * 100) / 5),
                    Width = 20,
                    Height = 36,
                    Page = Currentpage,
                    ItemType = ScadaClasses.uxItemSizeMenu,
                    ItemID = -1,
                    TagID = 4,
                    TagName = " ",
                    Action = " ",
                    PV = "0.0",
                    SV = "0.0",
                    Text = " ",
                    Radius = 10
                };
                ScadaItems.Add(oMenuTelegram);
            }

            if (CurrentScadaPopup == ScadaClasses.uxTimeSpanMenu)
            {
                ScadaClasses.Telegram oMenuTelegram = new ScadaClasses.Telegram()
                {
                    MessageType = 0,
                    Top = (((Ypos / Height) * 100) + 1.7 + (Yheight / Width) * 100),
                    Left = (((Xpos / Width) * 100) + ((Xwidth / Width) * 100) / 5),
                    Width = 20,
                    Height = 36,
                    Page = Currentpage,
                    ItemType = ScadaClasses.uxTimeSpanMenu,
                    ItemID = CurrentItem,
                    TagID = 4,
                    TagName = " ",
                    Action = " ",
                    PV = "0.0",
                    SV = "0.0",
                    Text = " ",
                    Radius = 10
                };
                ScadaItems.Add(oMenuTelegram);
            }

            if (CurrentScadaPopup == ScadaClasses.uxItemTypeMenu)
            {
                ScadaClasses.Telegram oMenuTelegram = new ScadaClasses.Telegram()
                {
                    MessageType = 0,
                    Top = (((Ypos / Height) * 100) + 1.7 + (Yheight / Width) * 100),
                    Left = (((Xpos / Width) * 100) + ((Xwidth / Width) * 100) / 5),
                    Width = 20,
                    Height = 36,
                    Page = Currentpage,
                    ItemType = ScadaClasses.uxItemTypeMenu,
                    ItemID = -1,
                    TagID = 4,
                    TagName = " ",
                    Action = " ",
                    PV = "0.0",
                    SV = "0.0",
                    Text = " ",
                    Radius = 10
                };
                ScadaItems.Add(oMenuTelegram);
            }


            if (CurrentScadaPopup == ScadaClasses.uxItemAction)
            {
                ScadaClasses.Telegram oMenuTelegram = new ScadaClasses.Telegram()
                {
                    MessageType = 0,
                    Top = (((Ypos / Height) * 100) + 1.7 + (Yheight / Width) * 100),
                    Left = (((Xpos / Width) * 100) + ((Xwidth / Width) * 100) / 5),
                    Width = 20,
                    Height = 36,
                    Page = Currentpage,
                    ItemType = ScadaClasses.uxItemAction,
                    ItemID = -1,
                    TagID = 4,
                    TagName = " ",
                    Action = " ",
                    PV = "0.0",
                    SV = "0.0",
                    Text = " ",
                    Radius = 10
                };
                ScadaItems.Add(oMenuTelegram);
            }

            if (CurrentScadaPopup == ScadaClasses.uxLoginMenu)
            {
                ScadaClasses.Telegram oMenuTelegram = new ScadaClasses.Telegram()
                {
                    MessageType = 0,
                    Top = (((Ypos / Height) * 100) + 1.7 + (Yheight / Width) * 100),
                    Left = (((Xpos / Width) * 100) + ((Xwidth / Width) * 100) / 5),
                    Width = 20,
                    Height = 50,
                    Page = Currentpage,
                    ItemType = ScadaClasses.uxLoginMenu,
                    ItemID = -1,
                    TagID = 1,
                    TagName = " ",
                    Action = " ",
                    PV = "0.0",
                    SV = "0.0",
                    Text = " ",
                    Radius = 10
                };
                ScadaItems.Add(oMenuTelegram);
            }

            if (CurrentScadaPopup == ScadaClasses.uxAlarmGrid)
            {
                ScadaClasses.Telegram oMenuTelegram = new ScadaClasses.Telegram()
                {
                    MessageType = 0,
                    Top = (((Ypos / Height) * 100) + 1.7 + (Yheight / Width) * 100),
                    Left = (((Xpos / Width) * 100) + ((Xwidth / Width) * 100) / 5),
                    Width = 80,
                    Height = 50,
                    Page = Currentpage,
                    ItemType = ScadaClasses.uxAlarmGrid,
                    ItemID = -1,
                    TagID = 1,
                    TagName = " ",
                    Action = " ",
                    PV = "0.0",
                    SV = "0.0",
                    Text = " ",                    
                    gridRows = MyDataAccessLayer.ReadAlarmMessages(""),
                    Radius = 10
                };
                ScadaItems.Add(oMenuTelegram);
            }

            if (CurrentScadaPopup == ScadaClasses.uxDesignMenu)
            {
                ScadaClasses.Telegram oMenuTelegram = new ScadaClasses.Telegram()
                {
                    MessageType = 0,
                    Top = (((Ypos / Height) * 100) + 1.7 + (Yheight / Width) * 100),
                    Left = (((Xpos / Width) * 100) + ((Xwidth / Width) * 100) / 5),
                    Width = 20,
                    Height = 50,
                    Page = Currentpage,
                    ItemType = ScadaClasses.uxDesignMenu,
                    ItemID = -1,
                    TagID = 1,
                    TagName = " ",
                    Action = " ",
                    PV = "0.0",
                    SV = "0.0",
                    Text = " ",
                    Radius = 10
                };
                ScadaItems.Add(oMenuTelegram);
            }

            if (CurrentScadaPopup == ScadaClasses.uxSvgPopupMenu)
            {
                ScadaClasses.Telegram oMenuTelegram = new ScadaClasses.Telegram()
                {
                    MessageType = 0,
                    Top = (((Ypos / Height) * 100) + 1.7 + (Yheight / Width) * 100),
                    Left = (((Xpos / Width) * 100) + ((Xwidth / Width) * 100) / 5),
                    Width = 20,
                    Height = 50,
                    Page = Currentpage,
                    ItemType = ScadaClasses.uxSvgPopupMenu,
                    ItemID = -1,
                    TagID = 1,
                    TagName = " ",
                    Action = " ",
                    PV = "0.0",
                    SV = "0.0",
                    Text = " ",
                    Radius = 10
                };
                ScadaItems.Add(oMenuTelegram);
            }

            if (CurrentScadaPopup == ScadaClasses.uxUploadMenu)
            {
                ScadaClasses.Telegram oMenuTelegram = new ScadaClasses.Telegram()
                {
                    MessageType = 0,
                    Top = (((Ypos / Height) * 100) + 1.7 + (Yheight / Width) * 100),
                    Left = (((Xpos / Width) * 100) + ((Xwidth / Width) * 100) / 5),
                    Width = 20,
                    Height = 50,
                    Page = Currentpage,
                    ItemType = ScadaClasses.uxUploadMenu,
                    ItemID = -1,
                    TagID = 1,
                    TagName = " ",
                    Action = " ",
                    PV = "0.0",
                    SV = "0.0",
                    Text = " ",
                    Radius = 10
                };
                ScadaItems.Add(oMenuTelegram);
            }
            return ScadaItems;
        }

        public class A
        {
            public string X { get; set; }
            public string Y { get; set; }
           
        }          
    }
}
