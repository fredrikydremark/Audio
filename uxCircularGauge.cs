
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace Scada
{
    public class CircularGauge : SKCanvasView
    {
        public int State;
        public float LastY;

        public CircularGauge Init(double w, double h, CircularGauge scb, ScadaClasses.Telegram Item, ScadaClasses.SystemColors Color)
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
        typeof(CircularGauge), 0, BindingMode.OneWay,
            validateValue: (_, value) => value != null,
        propertyChanged: OnPropertyChangedInvalidate);

        public int ItemID
        {
            get => (int)GetValue(ItemIDProperty);
            set => SetValue(ItemIDProperty, value);
        }

    
        public static BindableProperty PVProperty = BindableProperty.Create(nameof(PV), typeof(ItemValue),
           typeof(CircularGauge), null, BindingMode.OneWay,
           validateValue: (_, value) => value != null,
           propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue PV
        {
            get => (ItemValue)GetValue(PVProperty);
            set => SetValue(PVProperty, value);
        }


        public static BindableProperty SVProperty = BindableProperty.Create(nameof(SV), typeof(ItemValue),
           typeof(CircularGauge), null, BindingMode.OneWay,
           validateValue: (_, value) => value != null,
           propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue SV
        {
            get => (ItemValue)GetValue(SVProperty);
            set => SetValue(SVProperty, value);
        }
    

        public static BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(float),
            typeof(CircularGauge), 5f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float) value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);


        public float CornerRadius
        {
            get => (float)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }


        public static BindableProperty BarBackgroundColorProperty = BindableProperty.Create(nameof(BarBackgroundColor), typeof(SKColor),
            typeof(CircularGauge), SKColors.White, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);


        public SKColor BarBackgroundColor
        {
            get => (SKColor)GetValue(BarBackgroundColorProperty);
            set => SetValue(BarBackgroundColorProperty, value);
        }

        public static BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float),
            typeof(CircularGauge), 12f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float FontSize
        {
            get => (float)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static BindableProperty GradientStartColorProperty = BindableProperty.Create(nameof(GradientStartColor), typeof(SKColor),
            typeof(CircularGauge), SKColors.Purple, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientStartColor
        {
            get => (SKColor)GetValue(GradientStartColorProperty);
            set => SetValue(GradientStartColorProperty, value);
        }

        public static BindableProperty GradientEndColorProperty = BindableProperty.Create(nameof(GradientEndColor), typeof(SKColor),
            typeof(CircularGauge), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientEndColor
        {
            get => (SKColor)GetValue(GradientEndColorProperty);
            set => SetValue(GradientEndColorProperty, value);
        }

        public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(SKColor),
            typeof(CircularGauge), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor TextColor
        {
            get => (SKColor)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public static BindableProperty AlternativeTextColorProperty = BindableProperty.Create(nameof(AlternativeTextColor), typeof(SKColor),
            typeof(CircularGauge), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor AlternativeTextColor
        {
            get => (SKColor)GetValue(AlternativeTextColorProperty);
            set => SetValue(AlternativeTextColorProperty, value);
        }

        private static void OnPropertyChangedInvalidate(BindableObject bindable, object oldvalue, object newvalue)
        {
            var control = (CircularGauge)bindable;

            //if (oldvalue != newvalue)
                control.InvalidateSurface();
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
         
                if ((item.TagID == SV.TagID) && (State == 2))
                {
                    SV.Value = item.Value;
                }

            }
            if (IsLoaded == true)
            {
                InvalidateSurface();
            }
        }

        private double ValueToAngle(Double v)
        {
            double PI = Math.PI;
            double start = PI + (PI / 2) + (PI / 4);
            double stop = PI / 4;
            double range = 100;

            double r = start - (v * ((2 * 3.14) - (PI / 2)) / range);
            return r;
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;
            var canvas = e.Surface.Canvas;
            float MaxValue = 100;

            var Backgroundpaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill,
                Color = GradientStartColor,
                StrokeWidth = 0
            };

            var ProgressPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = new SKColor(3, 156, 35, 255),
                StrokeWidth = 7
            };

            var pathStroke2 = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill,
                Color = new SKColor(240, 32, 32, 200),
                StrokeWidth = 1
            };


            var TickPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = new SKColor(192, 192, 192, 230),
                StrokeWidth = 1
            };

            FontSize = 12F;
            var TextPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill,
                Color = TextColor,
                TextSize = FontSize,
                SubpixelText = true,
                FilterQuality = SKFilterQuality.High,
                StrokeWidth = 0.5F
            };

            if (PV.Value > MaxValue)
            {
                PV.Value = (float)(MaxValue);
            }
        
            float radius = (info.Height / 2);
            float frame = radius / 3;
            float diam = radius * 2;
            //float aspect = (float)info.Width / (float)info.Height;

            canvas.Clear();
            canvas.DrawArc(new SKRect(0, 0, info.Height, info.Height), -90, 360, false, Backgroundpaint);     
            canvas.DrawArc(new SKRect(frame,  frame , diam - frame, diam - frame ), -90, 360, false, ProgressPaint);

            var Tickpath = new SKPath { FillType = SKPathFillType.EvenOdd };
            int scale = 0;
            int range = 100;
            float X = 0;
            float Y = 0;
            float Xstart = 0;
            float Ystart = 0;
            float Xstop = 0;
            float Ystop = 0;
            float v = 0;
            float r = 45;

            for (scale = 0; scale <= range; scale++)
            {
                if ((scale % (range / 5)) == 0)
                {
                    v = (float)(ValueToAngle((double)(scale)));
                    
                    Xstart = (float)(Math.Sin(v) * (r));
                    Ystart = (float)(Math.Cos(v) * (r));
                    Xstop = (float)(Math.Sin((double)(v)) * (radius - (radius /4)));
                    Ystop = (float)(Math.Cos(v) * (radius - (radius / 4)));
                    // Scale tick marks
                    //canvas.DrawLine(radius + Xstart, radius + Ystart, radius + Xstop, radius + Ystop, TickPaint);

                    var bounds = new SKRect();
                    TickPaint.MeasureText(scale.ToString(), ref bounds);
                    var w = bounds.Width;
                    w = w / 2;
                    canvas.RotateRadians( -v + 3.14F, radius + Xstop, radius + Ystop );
                    canvas.DrawText( scale.ToString(), radius + Xstop-w, radius + Ystop, TextPaint );
                    canvas.RotateRadians( v + 3.1415F, radius + Xstop, radius + Ystop );             
                }
            }

          
            float angle = SV.Value;
            var Path = new SKPath { FillType = SKPathFillType.EvenOdd }; 

            v = (float)(ValueToAngle((double)(angle-5)));
            X = radius+(float)(Math.Sin((double)(v)) * (radius - (radius / 1.5)));
            Y = radius+(float)(Math.Cos((double)(v)) * (radius - (radius / 1.5)));
            Path.MoveTo(X, Y);

            v = (float)(ValueToAngle((double)(angle)));
            X = radius+(float)(Math.Sin((double)(v)) * (radius - (radius / 2.0)));
            Y = radius+(float)(Math.Cos((double)(v)) * (radius - (radius / 2.0)));
            Path.LineTo(X, Y);

            v = (float)(ValueToAngle((double)(angle+5)));
            X = radius+(float)(Math.Sin((double)(v)) * (radius - (radius / 1.5)));
            Y = radius+(float)(Math.Cos((double)(v)) * (radius - (radius / 1.5)));
            Path.LineTo(X, Y);
            
            Path.Close();
            canvas.DrawPath(Path, pathStroke2);            
        }
    }
}