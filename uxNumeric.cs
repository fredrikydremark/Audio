
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;


namespace Scada
{
    public class ScadaNumeric : SKCanvasView
    {
        public ScadaNumeric Init(double w, double h, ScadaNumeric scb, ScadaClasses.Telegram Item, ScadaClasses.Colors Color)
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

            DataAccessLayer MyDataAccessLayer = new DataAccessLayer();
            
            var ItemValuesBtn = MyDataAccessLayer.ReadItemValues(scb.ItemID);
            int i = 0;
            foreach (var item in ItemValuesBtn)
            {
                if (i == 0)
                {
                    scb.PV.TagID = item.TagID;
                    scb.PV.Value = item.Value;
                }
                i++;
            }

            //scb = (ScadaNumeric)AttachDesignEvents(scb, Item);

            scb.IsEnabled = true;
            scb.IsVisible = true;
            scb.EnableTouchEvents = true;
            scb.InputTransparent = false;
            scb.Start();
            return (scb);
        }


        public static BindableProperty ItemIDProperty = BindableProperty.Create(nameof(ItemID), typeof(int),
        typeof(ScadaNumeric), 0, BindingMode.OneWay,
            validateValue: (_, value) => value != null,
        propertyChanged: OnPropertyChangedInvalidate);

        public int ItemID
        {
            get => (int)GetValue(ItemIDProperty);
            set => SetValue(ItemIDProperty, value);
        }

        public static BindableProperty PVProperty = BindableProperty.Create(nameof(PV), typeof(ItemValue),
                  typeof(ScadaNumeric), null, BindingMode.OneWay,
                  validateValue: (_, value) => value != null,
                  propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue PV
        {
            get => (ItemValue)GetValue(PVProperty);
            set => SetValue(PVProperty, value);
        }


        /*
        public static BindableProperty ValuesProperty = BindableProperty.Create(nameof(ItemValues), typeof(List<ItemValue>),
            typeof(ScadaNumeric), 0f, BindingMode.OneWay,
            validateValue: (_, value) => value != null,
            propertyChanged: OnPropertyChangedInvalidate);


        public  List<ItemValue> ItemValues
        {
            get => (List<ItemValue>) GetValue(ValuesProperty);
            set => SetValue(ValuesProperty, value);
        }
        */

        /*
        public static BindableProperty sUnitProperty = BindableProperty.Create(nameof(sUnit), typeof(string),
               typeof(ScadaNumeric), "", BindingMode.OneWay,
               validateValue: (_, value) => value != null,
               propertyChanged: OnPropertyChangedInvalidate);

        public string sUnit
        {
            get => (string)GetValue(sUnitProperty);
            set => SetValue(sUnitProperty, value);
        }
        */


        public static BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(float),
            typeof(ScadaNumeric), 5f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float CornerRadius
        {
            get => (float)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static BindableProperty BarBackgroundColorProperty = BindableProperty.Create(nameof(BarBackgroundColor), typeof(SKColor),
            typeof(ScadaNumeric), SKColors.White, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor BarBackgroundColor
        {
            get => (SKColor)GetValue(BarBackgroundColorProperty);
            set => SetValue(BarBackgroundColorProperty, value);
        }

        public static BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float),
            typeof(ScadaNumeric), 12f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float FontSize
        {
            get => (float)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static BindableProperty GradientStartColorProperty = BindableProperty.Create(nameof(GradientStartColor), typeof(SKColor),
            typeof(ScadaNumeric), SKColors.Purple, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientStartColor
        {
            get => (SKColor)GetValue(GradientStartColorProperty);
            set => SetValue(GradientStartColorProperty, value);
        }

        public static BindableProperty GradientEndColorProperty = BindableProperty.Create(nameof(GradientEndColor), typeof(SKColor),
            typeof(ScadaNumeric), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientEndColor
        {
            get => (SKColor)GetValue(GradientEndColorProperty);
            set => SetValue(GradientEndColorProperty, value);
        }

        public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(SKColor),
            typeof(ScadaNumeric), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor TextColor
        {
            get => (SKColor)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public static BindableProperty AlternativeTextColorProperty = BindableProperty.Create(nameof(AlternativeTextColor), typeof(SKColor),
            typeof(ScadaNumeric), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor AlternativeTextColor
        {
            get => (SKColor)GetValue(AlternativeTextColorProperty);
            set => SetValue(AlternativeTextColorProperty, value);
        }

        private static void OnPropertyChangedInvalidate(BindableObject bindable, object oldvalue, object newvalue)
        {
            var control = (ScadaNumeric)bindable;
            if (control != null)
            {
                if (oldvalue != newvalue)
                    control.InvalidateSurface();
            }
        }

        bool AnimationRunning = false;
        float DV = 0;
        float OV = 0;
        public void Start()
        {
            if (AnimationRunning == false)
                UpdateAnimation();
        }

        private async void UpdateAnimation()
        {
            AnimationRunning = true;
            DV = PV.Value;
            OV = DV;

            while (AnimationRunning)
            {
                await Task.Delay(50);
                if (Math.Abs(DV - OV) > 0.05)
                {
                    float Step = (DV - OV) / 10 * -1;
                    DV = DV + Step;
                    if (IsLoaded == true)
                    {
                        InvalidateSurface();
                    }
                }
            }
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;
            var canvas = e.Surface.Canvas;

            var Backgroundpaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill,
                Color = GradientStartColor,
                StrokeWidth = 0
            };

            FontSize = 18.5F;
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

            float d = 0;
            float r = info.Height;
            float w = info.Width;

            var path3 = new SKPath { FillType = SKPathFillType.EvenOdd };
            path3.AddArc(new SKRect(d, d, r, r), -90, -180);
            path3.AddRect(new SKRect(d + r / 2, d, w - r / 2, r));
            path3.AddArc(new SKRect(w - r, d, w, r), -90, 180);
            path3.Close();

            string DegreeC = "\u00B0C";
            string Unit = DegreeC;
            if (PV.Unit == "C")
            {
                Unit = DegreeC;
            }
            else
            {
                Unit = PV.Unit;
            }

            string Format = "0.0";
            var bounds = new SKRect();

            if (Math.Abs(OV - PV.Value) > 0.1)
            {
                OV = PV.Value;
            }

            string s = "";
            if (PV.StatusQuality == 0)
            {
                s = DV.ToString(Format) + Unit;
            }
            else
            {
                s = "--.-" + Unit;
            }


            TextPaint.MeasureText(s, ref bounds);
            float TextWidth = bounds.Width;
            TextWidth = TextWidth / 2;

            canvas.Clear();
            canvas.DrawPath(path3, Backgroundpaint);
            canvas.DrawText(s, w / 2 - TextWidth, r - r / 4, TextPaint);
        }
    }
}