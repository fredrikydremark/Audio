
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace Scada
{
    public class ScadaProgress : SKCanvasView
    {
        public static BindableProperty ItemIDProperty = BindableProperty.Create(nameof(ItemID), typeof(int),
        typeof(ScadaProgress), 0, BindingMode.OneWay,
            validateValue: (_, value) => value != null,
        propertyChanged: OnPropertyChangedInvalidate);

        public int ItemID
        {
            get => (int)GetValue(ItemIDProperty);
            set => SetValue(ItemIDProperty, value);
        }

        public static BindableProperty PVProperty = BindableProperty.Create(nameof(PV), typeof(ItemValue),
              typeof(ScadaProgress), null, BindingMode.OneWay,
              validateValue: (_, value) => value != null,
              propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue PV
        {
            get => (ItemValue)GetValue(PVProperty);
            set => SetValue(PVProperty, value);
        }

        public static BindableProperty SVProperty = BindableProperty.Create(nameof(SV), typeof(ItemValue),
           typeof(ScadaProgress), null, BindingMode.OneWay,
           validateValue: (_, value) => value != null,
           propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue SV
        {
            get => (ItemValue)GetValue(SVProperty);
            set => SetValue(SVProperty, value);
        }

        public static BindableProperty ButtonRowProperty = BindableProperty.Create(nameof(ButtonRow), typeof(gridRow),
            typeof(ScadaProgress), null, BindingMode.OneWay,
            validateValue: (_, value) => value != null,
            propertyChanged: OnPropertyChangedInvalidate);

        public gridRow ButtonRow
        {
            get => (gridRow)GetValue(ButtonRowProperty);
            set => SetValue(ButtonRowProperty, value);
        }

        public static BindableProperty ButtonTextProperty = BindableProperty.Create(nameof(ButtonText), typeof(string),
        typeof(ScadaProgress), "", BindingMode.OneWay,
        validateValue: (_, value) => value != null,
        propertyChanged: OnPropertyChangedInvalidate);

        public string ButtonText
        {
            get => (string)GetValue(ButtonTextProperty);
            set => SetValue(ButtonTextProperty, value);
        }

        public static BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(float),
            typeof(ScadaProgress), 5f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float CornerRadius
        {
            get => (float)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static BindableProperty BarBackgroundColorProperty = BindableProperty.Create(nameof(BarBackgroundColor), typeof(SKColor),
            typeof(ScadaProgress), SKColors.White, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor BarBackgroundColor
        {
            get => (SKColor)GetValue(BarBackgroundColorProperty);
            set => SetValue(BarBackgroundColorProperty, value);
        }

        public static BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float),
            typeof(ScadaProgress), 12f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float FontSize
        {
            get => (float)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static BindableProperty MarginSizeProperty = BindableProperty.Create(nameof(Padding), typeof(float),
           typeof(ScadaProgress), 0f, BindingMode.OneWay,
           validateValue: (_, value) => value != null && (float)value >= 0,
           propertyChanged: OnPropertyChangedInvalidate);

        public float Padding
        {
            get => (float)GetValue(MarginSizeProperty);
            set => SetValue(MarginSizeProperty, value);
        }

        public static BindableProperty GradientStartColorProperty = BindableProperty.Create(nameof(GradientStartColor), typeof(SKColor),
            typeof(ScadaProgress), SKColors.Purple, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientStartColor
        {
            get => (SKColor)GetValue(GradientStartColorProperty);
            set => SetValue(GradientStartColorProperty, value);
        }

        public static BindableProperty GradientEndColorProperty = BindableProperty.Create(nameof(GradientEndColor), typeof(SKColor),
            typeof(ScadaProgress), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientEndColor
        {
            get => (SKColor)GetValue(GradientEndColorProperty);
            set => SetValue(GradientEndColorProperty, value);
        }

        public static BindableProperty IndicatorTypeProperty = BindableProperty.Create(nameof(IndicatorType), typeof(int),
        typeof(ScadaProgress), 1, BindingMode.OneWay,
         validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public int IndicatorType
        {
            get => (int)GetValue(IndicatorTypeProperty);
            set => SetValue(IndicatorTypeProperty, value);
        }

        public static BindableProperty IndicatorColorProperty = BindableProperty.Create(nameof(IndicatorColor), typeof(SKColor),
        typeof(ScadaProgress), SKColors.Black, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor IndicatorColor
        {
            get => (SKColor)GetValue(IndicatorColorProperty);
            set => SetValue(IndicatorColorProperty, value);
        }

        public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(SKColor),
            typeof(ScadaProgress), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor TextColor
        {
            get => (SKColor)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public static BindableProperty AlternativeTextColorProperty = BindableProperty.Create(nameof(AlternativeTextColor), typeof(SKColor),
            typeof(ScadaProgress), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor AlternativeTextColor
        {
            get => (SKColor)GetValue(AlternativeTextColorProperty);
            set => SetValue(AlternativeTextColorProperty, value);
        }

        public static BindableProperty SvgBase64Property = BindableProperty.Create(nameof(SvgBase64), typeof(string),
        typeof(ScadaProgress), "", BindingMode.OneWay,
        validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public string SvgBase64
        {
            get => (string)GetValue(SvgBase64Property);
            set => SetValue(SvgBase64Property, value);
        }

        private static void OnPropertyChangedInvalidate(BindableObject bindable, object oldvalue, object newvalue)
        {
            var control = (ScadaProgress)bindable;
            if (oldvalue != newvalue)
                if (control.IsLoaded == true)
                {
                    control.InvalidateSurface();
                }
        }

        private SKCanvas DrawRoundRectWithArrow(SKCanvas c, SKPaint Paint, float x, float y, float w, float h, float radius)
        {
            var Rect = new SKRect(0, 0 + 20, w, h - 20);
            var RoundRect = new SKRoundRect(Rect, 10, 10);
            c.DrawRoundRect(RoundRect, Paint);
            return c;
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
                }
                if (IsLoaded == true)
                {
                    InvalidateSurface();
                }
            }
        }

      
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);
            var info = e.Info;
            var canvas = e.Surface.Canvas;
            float w = info.Width;
            float h = info.Height;
            // var progressBar = new SKRoundRect(new SKRect(0, 0, w, h), CornerRadius, CornerRadius);  

            using (var paint = new SKPaint() { IsAntialias = true, FilterQuality = SKFilterQuality.High, BlendMode = SKBlendMode.Overlay })
            {
                var Rectangle = new SKRect(0, 0, w, h);
                paint.Shader = SKShader.CreateLinearGradient(
                    new SKPoint(Rectangle.Left, Rectangle.Top),
                    new SKPoint(Rectangle.Right, Rectangle.Top),
                    new[]
                    {
                        GradientStartColor,
                        GradientEndColor
                    },
                    new float[] { 0, 1 },
                    SKShaderTileMode.Clamp);

                var NewTextPaint = new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.StrokeAndFill,
                    Color = TextColor,
                    TextSize = FontSize,
                    SubpixelText = true,
                    FilterQuality = SKFilterQuality.High,
                    StrokeWidth = 0.4F
                };

                h = 12;
                float MaxValue = 100F;
                float l = (PV.Value / MaxValue) * w;
                var backgroundBar = new SKRoundRect(new SKRect(0, 0, w, h), h / 2, h / 2);
                var progressBar = new SKRoundRect(new SKRect(1, 2, l, h - 2), h / 10, h / 10);
                var background = new SKPaint { Color = BackgroundColor.ToSKColor(), IsAntialias = true };

                var Barpaint = new SKPaint
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.StrokeAndFill,
                    Color = new SKColor(3, 156, 35, 255),
                    BlendMode = SKBlendMode.Overlay,
                    FilterQuality = SKFilterQuality.High,
                    StrokeWidth = 1
                };
        
                canvas.Clear();
                canvas.DrawRoundRect(backgroundBar, background);
                canvas.DrawRoundRect(progressBar, Barpaint);

            }




        }
    }
}