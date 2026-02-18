

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace Scada
{
    public class ScadaBarGraph : SKCanvasView
    { 
        public int State;
        public float LastY;

        public ScadaBarGraph Init(double w, double h, ScadaBarGraph scb, ScadaClasses.Telegram Item, ScadaClasses.Colors Color)
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
            scb.PV = new ItemValue();
            scb.SV = new ItemValue();
            scb.Output = new ItemValue();
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
                if (i == 2)
                {
                    scb.Output.TagID = item.TagID;
                    scb.Output.Value = item.Value;
                }
                i++;
            }

            scb.Touch += (sender, args) =>
            {
                var pt = args.Location;
                switch (args.ActionType)
                {
                    case SKTouchAction.Pressed:
                        scb.LastY = pt.Y;
                        scb.State = 1;
                        break;

                    case SKTouchAction.Released:
                        scb.State = 2;
                        MyDataAccessLayer.UpdateTagValue(scb.SV.TagID, scb.SV.Value,0);
                        MyDataAccessLayer.StoreTagValue(scb.SV.TagID, scb.SV.Value);
                        ScadaClasses.Refresh = true;
                        break;

                    case SKTouchAction.Moved:
                        if (scb.State == 1)
                        {
                            if (scb.LastY < pt.Y)
                            {
                                scb.SV.Value = scb.SV.Value - 1.0F;
                            }
                            else
                            {
                                scb.SV.Value = scb.SV.Value + 1.0F;
                            }
                            scb.LastY = pt.Y;
                            scb.InvalidateSurface();

                        }
                        break;
                    case SKTouchAction.Exited:
                        break;
                }
                args.Handled = true;
            };

            scb.IsEnabled = true;
            scb.IsVisible = true;
            scb.EnableTouchEvents = true;
            scb.InputTransparent = false;

            return (scb);
        }


        public static BindableProperty ItemIDProperty = BindableProperty.Create(nameof(ItemID), typeof(int),
        typeof(ScadaBarGraph), 0, BindingMode.OneWay,
            validateValue: (_, value) => value != null,
        propertyChanged: OnPropertyChangedInvalidate);

        public int ItemID
        {
            get => (int)GetValue(ItemIDProperty);
            set => SetValue(ItemIDProperty, value);
        }


        public static BindableProperty PVProperty = BindableProperty.Create(nameof(PV), typeof(ItemValue),
               typeof(ScadaBarGraph), null, BindingMode.OneWay,
               validateValue: (_, value) => value != null,
               propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue PV
        {
            get => (ItemValue)GetValue(PVProperty);
            set => SetValue(PVProperty, value);
        }


        public static BindableProperty SVProperty = BindableProperty.Create(nameof(SV), typeof(ItemValue),
           typeof(ScadaBarGraph), null, BindingMode.OneWay,
           validateValue: (_, value) => value != null,
           propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue SV
        {
            get => (ItemValue)GetValue(SVProperty);
            set => SetValue(SVProperty, value);
        }

        public static BindableProperty OutputProperty = BindableProperty.Create(nameof(Output), typeof(ItemValue),
        typeof(ScadaBarGraph), null, BindingMode.OneWay,
        validateValue: (_, value) => value != null,
        propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue Output
        {
            get => (ItemValue)GetValue(OutputProperty);
            set => SetValue(OutputProperty, value);
        }

        public static BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(float),
            typeof(ScadaBarGraph), 5f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float CornerRadius
        {
            get => (float)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static BindableProperty BarBackgroundColorProperty = BindableProperty.Create(nameof(BarBackgroundColor), typeof(SKColor),
            typeof(ScadaBarGraph), SKColors.White, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor BarBackgroundColor
        {
            get => (SKColor)GetValue(BarBackgroundColorProperty);
            set => SetValue(BarBackgroundColorProperty, value);
        }

        public static BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float),
            typeof(ScadaBarGraph), 12f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float FontSize
        {
            get => (float)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static BindableProperty GradientStartColorProperty = BindableProperty.Create(nameof(GradientStartColor), typeof(SKColor),
            typeof(ScadaBarGraph), SKColors.Purple, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientStartColor
        {
            get => (SKColor)GetValue(GradientStartColorProperty);
            set => SetValue(GradientStartColorProperty, value);
        }

        public static BindableProperty GradientEndColorProperty = BindableProperty.Create(nameof(GradientEndColor), typeof(SKColor),
            typeof(ScadaBarGraph), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientEndColor
        {
            get => (SKColor)GetValue(GradientEndColorProperty);
            set => SetValue(GradientEndColorProperty, value);
        }

        public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(SKColor),
            typeof(ScadaBarGraph), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor TextColor
        {
            get => (SKColor)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public static BindableProperty AlternativeTextColorProperty = BindableProperty.Create(nameof(AlternativeTextColor), typeof(SKColor),
            typeof(ScadaBarGraph), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor AlternativeTextColor
        {
            get => (SKColor)GetValue(AlternativeTextColorProperty);
            set => SetValue(AlternativeTextColorProperty, value);
        }

        private static void OnPropertyChangedInvalidate(BindableObject bindable, object oldvalue, object newvalue)
        {
            var control = (ScadaBarGraph)bindable;
            if ( control != null ) {
                if (oldvalue != newvalue)
                     control.InvalidateSurface();
            }
        }
        public void Start()
        {
            UpdateValue();
        }
        private async void UpdateValue()
        {
            DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);
            while (true)
            {
                await Task.Delay(5000);
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
                    if (item.TagID == Output.TagID)
                    {
                        Output.Value = item.Value;
                        Output.StatusQuality = item.StatusQuality;
                    }
                }
                if (IsLoaded == true)
                {
                    InvalidateSurface();
                }
            }
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;
            var canvas = e.Surface.Canvas;
          
            float w = e.Info.Width;           
            float h = e.Info.Height;
            float MaxValue = 100;

            if (Output.Value > MaxValue)
            {
                Output.Value = MaxValue;
            }
            float pixOutput = ( Output.Value / 100.0F) * h;

            var backgroundBar = new SKRoundRect(new SKRect(0, 0, w, h), 5, 5);
            var progressBar = new SKRoundRect(new SKRect(5, h-1, w - 5, h- pixOutput), 1, 1);
            var background = new SKPaint { Color = GradientStartColor, IsAntialias = true };

            var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill,
                Color = new SKColor(175, 175, 175, 200),
                StrokeWidth = 1
            };

            if (PV.Value > MaxValue)
            {
                PV.Value = MaxValue;
            }    
          
             
             var pathStroke2 = new SKPaint
             {
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill,
                Color = new SKColor(240, 16, 16, 230),
                BlendMode = SKBlendMode.Overlay,
                FilterQuality = SKFilterQuality.High,
                StrokeWidth = 1
             };

             if (SV.Value > MaxValue) 
             {
                    SV.Value = MaxValue;
             }

             //calc render for SV Setpoint 
             float pixSP = ( SV.Value / 100.0F) * h;

             var path2 = new SKPath { FillType = SKPathFillType.EvenOdd };
             path2.MoveTo(10, h - pixSP);
             path2.LineTo(0, h - pixSP + 4);
             path2.LineTo(0, h - pixSP - 4);
             path2.LineTo(10, h - pixSP);
             path2.Close();


             // calc render for PV Processvalue
             var pathStroke3 = new SKPaint
             {
                 IsAntialias = true,
                 Style = SKPaintStyle.StrokeAndFill,
                 Color = new SKColor(3, 156, 35, 255),
                 StrokeWidth = 1
             };
            
             float pixPV = (( PV.Value / 100.0F) * h);
             var path3 = new SKPath { FillType = SKPathFillType.EvenOdd };
             path3.MoveTo(w-10, h - pixPV );
             path3.LineTo(w, h - pixPV + 4 );
             path3.LineTo(w, h - pixPV - 4 );
             path3.LineTo(w-10, h - pixPV );
             path3.Close();
            
             canvas.Clear();
             canvas.DrawRoundRect(backgroundBar, background);
             canvas.DrawRoundRect(progressBar, paint);
             canvas.DrawPath(path2, pathStroke2);
             canvas.DrawPath(path3, pathStroke3);
        }         
        
    }
}