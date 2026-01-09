using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace Scada
{
    public class ScadaCircularProgress : SKCanvasView
    {
        public static BindableProperty ItemIDProperty = BindableProperty.Create(nameof(ItemID), typeof(int),
        typeof(ScadaCircularProgress), 0, BindingMode.OneWay,
            validateValue: (_, value) => value != null,
        propertyChanged: OnPropertyChangedInvalidate);

        public int ItemID
        {
            get => (int)GetValue(ItemIDProperty);
            set => SetValue(ItemIDProperty, value);
        }

       
        public static BindableProperty PVProperty = BindableProperty.Create(nameof(PV), typeof(ItemValue),
           typeof(ScadaCircularProgress), null, BindingMode.OneWay,
           validateValue: (_, value) => value != null,
           propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue PV
        {
            get => (ItemValue)GetValue(PVProperty);
            set => SetValue(PVProperty, value);
        }


        public static BindableProperty SVProperty = BindableProperty.Create(nameof(SV), typeof(ItemValue),
           typeof(ScadaCircularProgress), null, BindingMode.OneWay,
           validateValue: (_, value) => value != null,
           propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue SV
        {
            get => (ItemValue)GetValue(SVProperty);
            set => SetValue(SVProperty, value);
        }


        public static BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(float),
            typeof(ScadaCircularProgress), 0f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);


        public float CornerRadius
        {
            get => (float)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static BindableProperty BarBackgroundColorProperty = BindableProperty.Create(nameof(BarBackgroundColor), typeof(SKColor),
            typeof(ScadaCircularProgress), SKColors.White, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor BarBackgroundColor
        {
            get => (SKColor)GetValue(BarBackgroundColorProperty);
            set => SetValue(BarBackgroundColorProperty, value);
        }

        public static BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float),
            typeof(ScadaCircularProgress), 0f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float FontSize
        {
            get => (float)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static BindableProperty GradientStartColorProperty = BindableProperty.Create(nameof(GradientStartColor), typeof(SKColor),
            typeof(ScadaCircularProgress), SKColors.Purple, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientStartColor
        {
            get => (SKColor)GetValue(GradientStartColorProperty);
            set => SetValue(GradientStartColorProperty, value);
        }

        public static BindableProperty GradientEndColorProperty = BindableProperty.Create(nameof(GradientEndColor), typeof(SKColor),
            typeof(ScadaCircularProgress), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientEndColor
        {
            get => (SKColor)GetValue(GradientEndColorProperty);
            set => SetValue(GradientEndColorProperty, value);
        }

        public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(SKColor),
            typeof(ScadaCircularProgress), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor TextColor
        {
            get => (SKColor)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public static BindableProperty AlternativeTextColorProperty = BindableProperty.Create(nameof(AlternativeTextColor), typeof(SKColor),
            typeof(ScadaCircularProgress), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor AlternativeTextColor
        {
            get => (SKColor)GetValue(AlternativeTextColorProperty);
            set => SetValue(AlternativeTextColorProperty, value);
        }

        private static void OnPropertyChangedInvalidate(BindableObject bindable, object oldvalue, object newvalue)
        {
            var control = (ScadaCircularProgress)bindable;
            if (oldvalue != newvalue)
                control.InvalidateSurface();
        }

        private SKPoint PointFromDegrees(float degrees, int radius, SKRect rect, int padding = 0)
        {
            const int offset = 90;
            var x = (float)(rect.MidX + (radius + padding) * Math.Cos((degrees - offset) * (Math.PI / 180)));
            var y = (float)(rect.MidY + (radius + padding) * Math.Sin((degrees - offset) * (Math.PI / 180)));
            return new SKPoint(x, y);
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
                Color = new SKColor(3, 156, 35, 240),
                StrokeWidth = 7.5F
            };

            var OffPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = new SKColor(100, 100, 100, 200),
                StrokeWidth = 7.5F
            };

            if (PV.Value > MaxValue ) {
               PV.Value = (float)(MaxValue);
            }
            if (PV.Value > 0.5) //FY Temporary fix for digital
            {
                PV.Value = (float)(MaxValue);
            }

            var radius = (info.Height / 2);
            var center = new SKPoint(info.Rect.MidX, info.Rect.MidY);
            var degrees = ( PV.Value / 100 ) * 360;

            //Draw Circle        
            canvas.Clear();
            canvas.DrawArc(new SKRect(0, 0, info.Height, info.Height), -90, 360, false, Backgroundpaint);

         
            float frame = radius / 4;
            float diam = radius * 2.0F;       
            
            if (PV.Value > 0.5)
            {
                canvas.DrawArc(new SKRect(frame, frame, diam - frame, diam - frame), -90, degrees, false, ProgressPaint);               
            }
            else
            {
                canvas.DrawArc(new SKRect(frame, frame, diam - frame, diam - frame), -90, 360, false, OffPaint);
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
        }

    }
}