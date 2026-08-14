using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace Scada
{
    public class CircularProgress : SKCanvasView
    {
        public bool bFaceFade = true;
        bool bIndicatorFade = true;
        float localBtnIndicatorIntensity = 255f;
        float localBtnFaceIntensity = 255f;
        
        public CircularProgress Init(double w, double h, CircularProgress scb, ScadaClasses.Telegram Item, ScadaClasses.SystemColors Color)
        {
            scb.AnchorX = 0;
            scb.AnchorY = 0;
            scb.CornerRadius = 10;
            scb.BarBackgroundColor = Color.uxBackGroundColor;
            scb.BackgroundColor = Color.uxPanelColor.ToMauiColor();
            scb.GradientStartColor = Color.uxGradientStartColor;
            scb.GradientEndColor = Color.uxGradientEndColor;
            scb.WidthRequest = w * Item.Width;
            scb.HeightRequest = h * Item.Height;
            scb.AlternativeTextColor = Color.uxTextColor;
            scb.TextColor = Color.uxTextColor;
            scb.ItemID = Item.ItemID;
            scb.StyleId = Item.ItemID.ToString();
            scb.FontSize = 18.5F;
            scb.EnableFaceFade();
            scb.EnableIndicatorBlink();
            scb.PV = new ItemValue();
            scb.SV = new ItemValue();
            DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);

            var ItemValuesBtn = MyDataAccessLayer.ReadItemValues(scb.ItemID);
            int i = 0;
            foreach (var item in ItemValuesBtn)
            {
                if (i == 0)
                {
                    scb.PV.TagID = item.TagID;
                    scb.PV.Value = item.Value;
                }
                if (i == 1)
                {
                    scb.SV.TagID = item.TagID;
                    scb.SV.Value = item.Value;
                }
                i++;
            }
            /*
            scb.Touch += (sender, args) =>
            {
                switch (args.ActionType)
                {
                    case SKTouchAction.Released:
                        if (Item.Action == "TOGGLE")
                        {
                            if (scb.SV.Value > 0.5)
                            {
                                scb.SV.Value = 0;
                            }
                            else
                            {
                                scb.SV.Value = 1;
                            }
                            MyDataAccessLayer.UpdateTagValue(scb.SV.TagID, scb.SV.Value,0);
                            MyDataAccessLayer.StoreTagValue(scb.SV.TagID, scb.SV.Value);

                        }
                        else if (Item.Action == "ON")
                        {
                            scb.SV.Value = 1;
                            MyDataAccessLayer.UpdateTagValue(scb.SV.TagID, scb.SV.Value,0);
                            MyDataAccessLayer.StoreTagValue(scb.SV.TagID, scb.SV.Value);
                        }
                        else if (Item.Action == "OFF")
                        {
                            scb.SV.Value = 0;
                            MyDataAccessLayer.UpdateTagValue(scb.SV.TagID, scb.SV.Value,0);
                            MyDataAccessLayer.StoreTagValue(scb.SV.TagID, scb.SV.Value);
                        }
                        else if (Item.Action == "POPUPALARM")
                        {
                            ScadaClasses.Previouspage = -1;
                            ScadaClasses.CurrentScadaPopup = ScadaClasses.uxAlarmGrid;
                        }
                        else if (Item.Action == "POPUPLOGIN")
                        {
                            ScadaClasses.Previouspage = -1;
                            ScadaClasses.CurrentScadaPopup = ScadaClasses.uxLoginMenu;
                        }
                        else if (Item.Action == "POPUPDESIGN")
                        {
                            ScadaClasses.Previouspage = -1;
                            ScadaClasses.CurrentScadaPopup = ScadaClasses.uxDesignMenu;
                        }
                        else
                        {
                            ScadaClasses.Currentpage = Item.Nextpage;
                        }
                        scb.GradientStartColor = Color.uxItemColor;
                        scb.GradientEndColor = Color.uxItemColor;
                        ScadaClasses.Refresh = true;

                        break;

                    case SKTouchAction.Pressed:
                        scb.GradientStartColor = Color.uxTouchColor;
                        scb.GradientEndColor = Color.uxTouchColor;
                        break;

                    case SKTouchAction.Moved:
                        break;

                    case SKTouchAction.Entered:
                        bFaceFade = false;
                        break;

                    case SKTouchAction.Exited:
                        bFaceFade = true;
                        break;
                }
            };
             */
            scb.IsEnabled = true;
            scb.IsVisible = true;
            scb.EnableTouchEvents = true;
            scb.InputTransparent = false;
            return (scb);
        }


        public static BindableProperty ItemIDProperty = BindableProperty.Create(nameof(ItemID), typeof(int),
        typeof(CircularProgress), 0, BindingMode.OneWay,
            validateValue: (_, value) => value != null,
        propertyChanged: OnPropertyChangedInvalidate);

        public int ItemID
        {
            get => (int)GetValue(ItemIDProperty);
            set => SetValue(ItemIDProperty, value);
        }
      
        public static BindableProperty PVProperty = BindableProperty.Create(nameof(PV), typeof(ItemValue),
           typeof(CircularProgress), null, BindingMode.OneWay,
           validateValue: (_, value) => value != null,
           propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue PV
        {
            get => (ItemValue)GetValue(PVProperty);
            set => SetValue(PVProperty, value);
        }


        public static BindableProperty SVProperty = BindableProperty.Create(nameof(SV), typeof(ItemValue),
           typeof(CircularProgress), null, BindingMode.OneWay,
           validateValue: (_, value) => value != null,
           propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue SV
        {
            get => (ItemValue)GetValue(SVProperty);
            set => SetValue(SVProperty, value);
        }


        public static BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(float),
            typeof(CircularProgress), 0f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);


        public float CornerRadius
        {
            get => (float)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static BindableProperty BarBackgroundColorProperty = BindableProperty.Create(nameof(BarBackgroundColor), typeof(SKColor),
            typeof(CircularProgress), SKColors.White, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor BarBackgroundColor
        {
            get => (SKColor)GetValue(BarBackgroundColorProperty);
            set => SetValue(BarBackgroundColorProperty, value);
        }

        public static BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float),
            typeof(CircularProgress), 0f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float FontSize
        {
            get => (float)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static BindableProperty GradientStartColorProperty = BindableProperty.Create(nameof(GradientStartColor), typeof(SKColor),
            typeof(CircularProgress), SKColors.Purple, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientStartColor
        {
            get => (SKColor)GetValue(GradientStartColorProperty);
            set => SetValue(GradientStartColorProperty, value);
        }

        public static BindableProperty GradientEndColorProperty = BindableProperty.Create(nameof(GradientEndColor), typeof(SKColor),
            typeof(CircularProgress), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientEndColor
        {
            get => (SKColor)GetValue(GradientEndColorProperty);
            set => SetValue(GradientEndColorProperty, value);
        }

        public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(SKColor),
            typeof(CircularProgress), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor TextColor
        {
            get => (SKColor)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public static BindableProperty AlternativeTextColorProperty = BindableProperty.Create(nameof(AlternativeTextColor), typeof(SKColor),
            typeof(CircularProgress), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor AlternativeTextColor
        {
            get => (SKColor)GetValue(AlternativeTextColorProperty);
            set => SetValue(AlternativeTextColorProperty, value);
        }

        private static void OnPropertyChangedInvalidate(BindableObject bindable, object oldvalue, object newvalue)
        {
            var control = (CircularProgress)bindable;
            if (oldvalue != newvalue)
                control.InvalidateSurface();
        }


        public void FadeDown()
        {
            bFaceFade = false;
        }

        public void FadeUp()
        {
            bFaceFade = true;
        }

        public async void EnableFaceFade()
        {
            float Step = 5.0F;
            while (true)
            {
                if (bFaceFade == true)
                {
                    if (localBtnFaceIntensity > 200)
                    {
                        localBtnFaceIntensity = localBtnFaceIntensity - Step;
                        if (IsLoaded == true)
                        {
                            InvalidateSurface();
                        }
                    }
                }
                if (bFaceFade == false)
                {
                    if (localBtnFaceIntensity < 250)
                    {
                        localBtnFaceIntensity = localBtnFaceIntensity + Step;
                        if (IsLoaded == true)
                        {
                            InvalidateSurface();
                        }
                    }
                }
                await Task.Delay(10);
            }
        }



        public async void EnableIndicatorBlink()
        {
            const int cStep = 2;
            int iStep = cStep;
            while (bIndicatorFade)
            {
                if ((localBtnIndicatorIntensity + iStep) > 255)
                {
                    iStep = -cStep;
                }
                if (localBtnIndicatorIntensity < 100)
                {
                    iStep = cStep;
                }
                if (IsLoaded == true)
                {
                    InvalidateSurface();
                }
                localBtnIndicatorIntensity = localBtnIndicatorIntensity + iStep;
                await Task.Delay(10);
            }
            localBtnIndicatorIntensity = 255;
        }

        public void Start()
        {
            RefreshTask();
        }

        public void RefreshValues()
        {
            DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);
            var ItemValues = MyDataAccessLayer.ReadItemValues(ItemID);
            foreach (var item in ItemValues)
            {
                if (item.TagID == PV.TagID)
                {
                    PV.Value = item.Value;
                    PV.StatusQuality = item.StatusQuality;
                }
                if (item.TagID == SV.TagID)
                {
                    SV.Value = item.Value;
                    SV.StatusQuality = item.StatusQuality;
                }
            }
            if (IsLoaded == true)
            {
                InvalidateSurface();
            }
        }

        private async void RefreshTask()
        {
            while (true)
            {
                await Task.Delay(5000);
                RefreshValues();
            }
        }




        private SKPoint PointFromDegrees(float degrees, int radius, SKRect rect, int padding = 0)
        {
            const int offset = 90;
            var x = (float)(rect.MidX + (radius + padding) * Math.Cos((degrees - offset) * (Math.PI / 180)));
            var y = (float)(rect.MidY + (radius + padding) * Math.Sin((degrees - offset) * (Math.PI / 180)));
            return new SKPoint(x, y);
        }

    
        SKPaint OnPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(100, 100, 100, 255),
            TextSize = 16.5F,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 7.5F,
            FilterQuality = SKFilterQuality.High
        };

        SKPaint OffPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            Color = new SKColor(100, 100, 100, 220),
            StrokeWidth = 7.5F
        };


        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;
            var canvas = e.Surface.Canvas;          
            float MaxValue = 100;


            SKPaint Backgroundpaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill,
                Color = GradientStartColor,
                StrokeWidth = 0,
                FilterQuality = SKFilterQuality.High
            };


            if (PV.Value > MaxValue ) 
            {
               PV.Value = (float)(MaxValue);
            }
            if (PV.Value > 0.5) 
            {
               PV.Value = (float)(MaxValue);
            }

            float radius = (info.Height / 2);
            float frame = radius / 4;
            float diam = radius * 2.0F;
            var center = new SKPoint(info.Rect.MidX, info.Rect.MidY);
            var degrees = ( PV.Value / 100 ) * 360;

            //Draw Circle        
            canvas.Clear();
            canvas.DrawArc(new SKRect(0, 0, info.Height, info.Height), -90, 360, false, Backgroundpaint);
         
            if (PV.Value > 0.5)
            {
                OnPaint.Color = new SKColor(3, 156, 35, (byte)localBtnIndicatorIntensity);
                canvas.DrawArc(new SKRect(frame, frame, diam - frame, diam - frame), -90, degrees, false, OnPaint);               
            }
            else
            {
                canvas.DrawArc(new SKRect(frame, frame, diam - frame, diam - frame), -90, 360, false, OffPaint);
            }                
        }
    }
}


/* 
        Dotted experiment
        if (Percentage > 0.5)
        {
            float Angle = -90;
            while (Angle < degrees)
            {
                canvas.DrawArc(new SKRect(9, 10, info.Height - 10, info.Height - 10), Angle, 10, false, ProgressPaint);
                Angle = Angle + 20;
            }
        }
   */