
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using Svg.Skia;
using System.Text;


namespace Scada
{
    public class ScadaPanel : SKCanvasView
    {
        bool bFaceFade = true;
        float localBtnFaceIntensity = 0f;
        bool FaceFadeEnabled = false;

        public ScadaPanel Init(double wScale, double hScale, ScadaPanel scb, ScadaClasses.Telegram Item, ScadaClasses.SystemColors Color)
        {
            scb.AnchorX = 0;
            scb.AnchorY = 0;
            scb.CornerRadius = 10;
            scb.BarBackgroundColor = Color.uxBackGroundColor;
            scb.BackgroundColor = Color.uxPanelColor.ToMauiColor();
            scb.GradientStartColor = Color.uxPanelColor;
            scb.GradientEndColor = Color.uxPanelColor;          
            scb.WidthRequest = wScale * Item.Width;
            scb.HeightRequest = hScale * Item.Height;
          
            scb.TextColor = Color.uxTextColor;
            scb.ItemID = Item.ItemID;
            scb.StyleId = Item.ItemID.ToString();
            scb.FontSize = 18.5F;
            scb.IsEnabled = true;
            scb.IsVisible = true;
            scb.EnableTouchEvents = false;
            scb.InputTransparent = false;                                   
            return (scb);
        }


        public static BindableProperty ItemIDProperty = BindableProperty.Create(nameof(ItemID), typeof(int),
        typeof(ScadaPanel), 0, BindingMode.OneWay,
            validateValue: (_, value) => value != null,
        propertyChanged: OnPropertyChangedInvalidate);

        public int ItemID
        {
            get => (int)GetValue(ItemIDProperty);
            set => SetValue(ItemIDProperty, value);
        }

        public static BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(float),
            typeof(ScadaPanel), 5f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float CornerRadius
        {
            get => (float)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static BindableProperty BarBackgroundColorProperty = BindableProperty.Create(nameof(BarBackgroundColor), typeof(SKColor),
            typeof(ScadaPanel), SKColors.White, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor BarBackgroundColor
        {
            get => (SKColor)GetValue(BarBackgroundColorProperty);
            set => SetValue(BarBackgroundColorProperty, value);
        }


        public static BindableProperty ButtonFaceIntensityProperty = BindableProperty.Create(nameof(ButtonFaceIntensity), typeof(float),
            typeof(ScadaPanel), 5f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float ButtonFaceIntensity
        {
            get => (float)GetValue(ButtonFaceIntensityProperty);
            set => SetValue(ButtonFaceIntensityProperty, value);
        }

        public static BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float),
            typeof(ScadaPanel), 12f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float FontSize
        {
            get => (float)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static BindableProperty MarginSizeProperty = BindableProperty.Create(nameof(Padding), typeof(float),
           typeof(ScadaPanel), 0f, BindingMode.OneWay,
           validateValue: (_, value) => value != null && (float)value >= 0,
           propertyChanged: OnPropertyChangedInvalidate);

        public float Padding
        {
            get => (float)GetValue(MarginSizeProperty);
            set => SetValue(MarginSizeProperty, value);
        }

        public static BindableProperty GradientStartColorProperty = BindableProperty.Create(nameof(GradientStartColor), typeof(SKColor),
            typeof(ScadaPanel), SKColors.Purple, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientStartColor
        {
            get => (SKColor)GetValue(GradientStartColorProperty);
            set => SetValue(GradientStartColorProperty, value);
        }

        public static BindableProperty GradientEndColorProperty = BindableProperty.Create(nameof(GradientEndColor), typeof(SKColor),
            typeof(ScadaPanel), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientEndColor
        {
            get => (SKColor)GetValue(GradientEndColorProperty);
            set => SetValue(GradientEndColorProperty, value);
        }

  
        public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(SKColor),
            typeof(ScadaPanel), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor TextColor
        {
            get => (SKColor)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

      
        public static BindableProperty SvgBase64Property = BindableProperty.Create(nameof(SvgBase64), typeof(string),
        typeof(ScadaPanel), "", BindingMode.OneWay,
        validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public string SvgBase64
        {
            get => (string)GetValue(SvgBase64Property);
            set => SetValue(SvgBase64Property, value);
        }

        private static void OnPropertyChangedInvalidate(BindableObject bindable, object oldvalue, object newvalue)
        {
            var control = (ScadaPanel)bindable;
            if (oldvalue != newvalue)
            {              
                if (control.IsLoaded == true)
                {
                    control.InvalidateSurface();
                }
            }
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
            FaceFadeEnabled = true;
            float Step = 7.0F;
            while (FaceFadeEnabled)
            {
                if (bFaceFade == true)
                {
                    if (localBtnFaceIntensity > GradientStartColor.Blue)  
                    {
                        localBtnFaceIntensity = localBtnFaceIntensity - Step;
                        if (IsLoaded == true)
                        {
                            InvalidateSurface();
                        }
                    }
                    if (localBtnFaceIntensity < GradientStartColor.Blue)  
                    {
                        localBtnFaceIntensity = localBtnFaceIntensity + Step;
                        if (IsLoaded == true)
                        {
                            InvalidateSurface();
                        }
                    }
                }
                if (bFaceFade == false)
                {
                    if (localBtnFaceIntensity < GradientStartColor.Blue+30) 
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


        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;
            var canvas = e.Surface.Canvas;
            float w = info.Width;
            float h = info.Height;

            var backgroundPanel = new SKRoundRect(new SKRect(0, 0, w, h), CornerRadius, CornerRadius);

            var panelPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.StrokeAndFill,
                Color = GradientStartColor,
                BlendMode = SKBlendMode.Overlay,
                FilterQuality = SKFilterQuality.High,
                StrokeWidth = 1
            };
            canvas.Clear();
            canvas.DrawRoundRect(backgroundPanel, panelPaint);


            /*
            using (var panelPaint = new SKPaint() { IsAntialias = true, FilterQuality = SKFilterQuality.High, BlendMode = SKBlendMode.Overlay })
            {
                var Rectangle = new SKRect(0, 0, w, h);
                    panelPaint.Shader = SKShader.CreateLinearGradient(
                    new SKPoint(Rectangle.Left, Rectangle.Top),
                    new SKPoint(Rectangle.Right, Rectangle.Bottom),
                    new[]
                    {
                        GrStart,
                        GrEnd
                    },
                    new float[] { 0, 1 },
                    SKShaderTileMode.Decal);
                    canvas.DrawRoundRect(backgroundPanel, panelPaint);                       
            }
            */

        }
    }
}