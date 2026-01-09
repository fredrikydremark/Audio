
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace Scada
{
    public class ScadaLine : SKCanvasView
    {
        public static BindableProperty ItemIDProperty = BindableProperty.Create(nameof(ItemID), typeof(int),
        typeof(ScadaLine), 0, BindingMode.OneWay,
            validateValue: (_, value) => value != null,
        propertyChanged: OnPropertyChangedInvalidate);

        public int ItemID
        {
            get => (int)GetValue(ItemIDProperty);
            set => SetValue(ItemIDProperty, value);
        }

        public static BindableProperty DigitalValueProperty = BindableProperty.Create(nameof(DigitalValue), typeof(float),
            typeof(ScadaLine), 0f, BindingMode.OneWay,
            validateValue: (_, value) => value != null,
            propertyChanged: OnPropertyChangedInvalidate);

        public float DigitalValue
        {
            get => (float)GetValue(DigitalValueProperty);
            set => SetValue(DigitalValueProperty, value);
        }

        public static BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(float),
            typeof(ScadaLine), 5f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float CornerRadius
        {
            get => (float)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static BindableProperty BarBackgroundColorProperty = BindableProperty.Create(nameof(BarBackgroundColor), typeof(SKColor),
            typeof(ScadaLine), SKColors.White, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor BarBackgroundColor
        {
            get => (SKColor)GetValue(BarBackgroundColorProperty);
            set => SetValue(BarBackgroundColorProperty, value);
        }

        public static BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float),
            typeof(ScadaLine), 12f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float FontSize
        {
            get => (float)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static BindableProperty GradientStartColorProperty = BindableProperty.Create(nameof(GradientStartColor), typeof(SKColor),
            typeof(ScadaLine), SKColors.Purple, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientStartColor
        {
            get => (SKColor)GetValue(GradientStartColorProperty);
            set => SetValue(GradientStartColorProperty, value);
        }

        public static BindableProperty GradientEndColorProperty = BindableProperty.Create(nameof(GradientEndColor), typeof(SKColor),
            typeof(ScadaLine), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientEndColor
        {
            get => (SKColor)GetValue(GradientEndColorProperty);
            set => SetValue(GradientEndColorProperty, value);
        }

        public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(SKColor),
            typeof(ScadaLine), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor TextColor
        {
            get => (SKColor)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public static BindableProperty AlternativeTextColorProperty = BindableProperty.Create(nameof(AlternativeTextColor), typeof(SKColor),
            typeof(ScadaLine), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor AlternativeTextColor
        {
            get => (SKColor)GetValue(AlternativeTextColorProperty);
            set => SetValue(AlternativeTextColorProperty, value);
        }

        private static void OnPropertyChangedInvalidate(BindableObject bindable, object oldvalue, object newvalue)
        {
            var control = (ScadaLine)bindable;

            if (oldvalue != newvalue)
                control.InvalidateSurface();
        }


        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;
            var canvas = e.Surface.Canvas;

            float d = 0;
            float r = info.Height;
            float w = info.Width;

            var pathStroke = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = new SKColor(148, 34, 204, 200),
                StrokeWidth = 0
            };

            var path3 = new SKPath { FillType = SKPathFillType.EvenOdd };
            path3.AddArc(new SKRect(d, d, r, r), -90, -180);
            path3.AddRect(new SKRect(d + r / 2, d, w - r / 2, r));
            path3.AddArc(new SKRect(w - r, d, w, r), -90, 180);
            path3.Close();

            canvas.Clear();
            canvas.DrawPath(path3, pathStroke);
        }
    }
}