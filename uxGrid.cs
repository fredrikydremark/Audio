

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace Scada
{
    public class ScadaGrid : SKCanvasView
    {
        public static BindableProperty ItemIDProperty = BindableProperty.Create(nameof(ItemID), typeof(int),
        typeof(ScadaGrid), 0, BindingMode.OneWay,
            validateValue: (_, value) => value != null,
        propertyChanged: OnPropertyChangedInvalidate);

        public int ItemID
        {
            get => (int)GetValue(ItemIDProperty);
            set => SetValue(ItemIDProperty, value);
        }
        /*
        public static BindableProperty PercentageProperty = BindableProperty.Create(nameof(Percentage), typeof(float),
            typeof(ScadaGrid), 0f, BindingMode.OneWay,
            validateValue: (_, value) => value != null,
            propertyChanged: OnPropertyChangedInvalidate);

        public float Percentage
        {
            get => (float)GetValue(PercentageProperty);
            set => SetValue(PercentageProperty, value);
        }*/
        /*
        public static BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(float),
            typeof(ScadaGrid), 5f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float CornerRadius
        {
            get => (float)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        */

        public static BindableProperty BarBackgroundColorProperty = BindableProperty.Create(nameof(BarBackgroundColor), typeof(SKColor),
            typeof(ScadaGrid), SKColors.White, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor BarBackgroundColor
        {
            get => (SKColor)GetValue(BarBackgroundColorProperty);
            set => SetValue(BarBackgroundColorProperty, value);
        }
        /*
        public static BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float),
            typeof(ScadaGrid), 12f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float FontSize
        {
            get => (float)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }
        */

        public static BindableProperty GridThinColorProperty = BindableProperty.Create(nameof(GridThinColor), typeof(SKColor),
            typeof(ScadaGrid), SKColors.Purple, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GridThinColor
        {
            get => (SKColor)GetValue(GridThinColorProperty);
            set => SetValue(GridThinColorProperty, value);
        }

        public static BindableProperty GridFatColorProperty = BindableProperty.Create(nameof(GridFatColor), typeof(SKColor),
            typeof(ScadaGrid), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GridFatColor
        {
            get => (SKColor)GetValue(GridFatColorProperty);
            set => SetValue(GridFatColorProperty, value);
        }
        /*
        public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(SKColor),
            typeof(ScadaGrid), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor TextColor
        {
            get => (SKColor)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public static BindableProperty AlternativeTextColorProperty = BindableProperty.Create(nameof(AlternativeTextColor), typeof(SKColor),
            typeof(ScadaGrid), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor AlternativeTextColor
        {
            get => (SKColor)GetValue(AlternativeTextColorProperty);
            set => SetValue(AlternativeTextColorProperty, value);
        }
        */
        
        private static void OnPropertyChangedInvalidate(BindableObject bindable, object oldvalue, object newvalue)
        {
           /* var control = (ScadaGrid)bindable;
            if (oldvalue != newvalue)
                control.InvalidateSurface();
           */
        }
        
        public void DrawGrid(SKCanvas c, float Top, float Left, float bw, float bh)
        {
            var HorzPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = GridFatColor,

                SubpixelText = true,
                FilterQuality = SKFilterQuality.High,
                StrokeWidth = 1.2F
            };
            var VertPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = GridFatColor,
                SubpixelText = true,
                FilterQuality = SKFilterQuality.High,
                StrokeWidth = 2.4F
            };
            var HorzSmallPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = GridThinColor,
                SubpixelText = true,
                FilterQuality = SKFilterQuality.High,
                StrokeWidth = 0.5F
            };
            var VertSmallPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = GridThinColor,
                SubpixelText = true,
                FilterQuality = SKFilterQuality.High,
                StrokeWidth = 1F
            };

            //var PathEffect = SKPathEffect.CreateDash(new[] { 5f, 20f }, 25),
            //var effect = SKPathEffect.CreateDash(new[] { 10f, 20f }, 25);
            //var paint = new SKPaint { Color = SKColors.Black, IsStroke = true, StrokeWidth = 1, PathEffect = effect };

            float h = (float)Window.MaximumHeight;
            float w = (float)Window.MaximumWidth;
            //float h = Top + bh;
            //float w = Left + bw;

            for (float x = Left; x < w; x += 10)
            {
                if (x % 100 == 0)
                {
                    c.DrawLine(x + 0.5F, Top, x + 0.5F, h, VertPaint);
                }
                else
                {
                    c.DrawLine(x + 0.5F, Top, x + 0.5F, h, VertSmallPaint);
                }
                for (float y = Top; y < h; y += 10)
                {
                    if (y % 100 == 0)
                    {
                        c.DrawLine(Left, y + 0.5F, w, y + 0.5F, HorzPaint);
                    }
                    else
                    {
                        c.DrawLine(Left, y + 0.5F, w, y + 0.5F, HorzSmallPaint);
                    }
                }
            }
        }

        /*
                public void DrawGrid(SKCanvas c, float Top, float Left, float bw, float bh)
                {
                    var HorzPaint = new SKPaint
                    {
                        IsAntialias = true,
                        Style = SKPaintStyle.Stroke,
                        Color = new SKColor(42, 42, 42, 50),
                        SubpixelText = true,
                        FilterQuality = SKFilterQuality.High,
                        StrokeWidth = 0.5F
                    };
                    var VertPaint = new SKPaint
                    {
                        IsAntialias = true,
                        Style = SKPaintStyle.Stroke,
                        Color = new SKColor(42, 42, 42, 150),
                        SubpixelText = true,
                        FilterQuality = SKFilterQuality.High,
                        StrokeWidth = 1F
                    };

                    //var PathEffect = SKPathEffect.CreateDash(new[] { 5f, 20f }, 25),
                    //var effect = SKPathEffect.CreateDash(new[] { 10f, 20f }, 25);
                    //var paint = new SKPaint { Color = SKColors.Black, IsStroke = true, StrokeWidth = 1, PathEffect = effect };

                    float h = Top + bh;
                    float w = Left + bw;

                    for (float x = Left; x < w; x += 40)
                    {
                        c.DrawLine(x + 0.5F, Top, x + 0.5F, h, VertPaint);
                        for (float y = Top; y < h; y += 40)
                        {
                            c.DrawLine(Left, y + 0.5F, w, y + 0.5F, HorzPaint);
                        }
                    }
                }
        */


        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;

            using var paint = new SKPaint() { IsAntialias = true };
            canvas.Clear();
            DrawGrid(canvas, 0, 0, (float)Width, (float)Height);
        }
    }
}