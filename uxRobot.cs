
using Microsoft.Maui.Layouts;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Globalization;

namespace Scada
{
    public class ScadaRobot : SKCanvasView
    { 
        float LowerArmAngle = (-3.14F / 3);
        float UpperArmAngle = (3.14F / 12);
        float LowerArmLength = 38;
        float LowerArmWidth = 25;
        float UpperArmLength = 40;
        float UpperArmWidth = 18;

        SKPaint GripperJointPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.StrokeAndFill,
            Color = new SKColor(247, 221, 6, 255),
            StrokeWidth = 0
        };

        SKPaint jointStroke = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.StrokeAndFill,
            Color = new SKColor(172, 171, 177, 255),
            StrokeWidth = 0
        };

        SKPaint LowerLinePaint = new SkiaSharp.SKPaint
        {
            IsAntialias = true,
            Style = SkiaSharp.SKPaintStyle.Stroke,
            Color = new SKColor(247, 221, 6, 255),
            StrokeWidth = 0,
            StrokeCap = SkiaSharp.SKStrokeCap.Round
        };

        SKPaint UpperLinePaint = new SkiaSharp.SKPaint
        {
            IsAntialias = true,
            Style = SkiaSharp.SKPaintStyle.Stroke,
            Color = new SKColor(228, 177, 12, 255),
            StrokeWidth = 0,
            StrokeCap = SkiaSharp.SKStrokeCap.Round
        };

        SKPaint BasePaint = new SkiaSharp.SKPaint
        {
            IsAntialias = true,
            Style = SkiaSharp.SKPaintStyle.StrokeAndFill,
            Color = new SKColor(228, 177, 12, 255),
            StrokeWidth = 0,
            StrokeCap = SkiaSharp.SKStrokeCap.Round
        };

        SKPaint BasePlatePaint = new SkiaSharp.SKPaint
        {
            IsAntialias = true,
            Style = SkiaSharp.SKPaintStyle.StrokeAndFill,
            Color = new SKColor(172, 171, 177, 255),
            StrokeWidth = 11,
            StrokeCap = SkiaSharp.SKStrokeCap.Round
        };

        SKPoint LowerJoint = new SKPoint(30, 60);
        SKPoint MiddleJoint = new SKPoint(0, 0);
        SKPoint GripperJoint = new SKPoint(0, 0);

        public static BindableProperty ItemIDProperty = BindableProperty.Create(nameof(ItemID), typeof(int),
        typeof(ScadaRobot), 0, BindingMode.OneWay,
            validateValue: (_, value) => value != null,
        propertyChanged: OnPropertyChangedInvalidate);

        public int ItemID
        {
            get => (int)GetValue(ItemIDProperty);
            set => SetValue(ItemIDProperty, value);
        }

        public static BindableProperty WindowWidthProperty = BindableProperty.Create(nameof(WindowWidth), typeof(float),
                 typeof(ScadaRobot), 1500f, BindingMode.OneWay,
                 validateValue: (_, value) => value != null,
                 propertyChanged: OnPropertyChangedInvalidate);

        public float WindowWidth
        {
            get => (float)GetValue(WindowWidthProperty);
            set => SetValue(WindowWidthProperty, value);
        }

        public static BindableProperty WindowHeightProperty = BindableProperty.Create(nameof(WindowHeight), typeof(float),
                       typeof(ScadaRobot), 1000f, BindingMode.OneWay,
                       validateValue: (_, value) => value != null,
                       propertyChanged: OnPropertyChangedInvalidate);

        public float WindowHeight
        {
            get => (float)GetValue(WindowHeightProperty);
            set => SetValue(WindowHeightProperty, value);
        }

        public static BindableProperty XTraverseProperty = BindableProperty.Create(nameof(XTraverse), typeof(ItemValue),
                     typeof(ScadaRobot), null, BindingMode.OneWay,
                     validateValue: (_, value) => value != null,
                     propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue XTraverse
        {
            get => (ItemValue)GetValue(XTraverseProperty);
            set => SetValue(XTraverseProperty, value);
        }

        public static BindableProperty YTraverseProperty = BindableProperty.Create(nameof(YTraverse), typeof(ItemValue),
                    typeof(ScadaRobot), null, BindingMode.OneWay,
                    validateValue: (_, value) => value != null,
                    propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue YTraverse
        {
            get => (ItemValue)GetValue(YTraverseProperty);
            set => SetValue(YTraverseProperty, value);
        }

        public static BindableProperty LeftProperty = BindableProperty.Create(nameof(Left), typeof(float),
                     typeof(ScadaRobot), null, BindingMode.OneWay,
                     validateValue: (_, value) => value != null,
                     propertyChanged: OnPropertyChangedInvalidate);

        public float Left
        {
            get => (float)GetValue(LeftProperty);
            set => SetValue(LeftProperty, value);
        }

        public static BindableProperty TopProperty = BindableProperty.Create(nameof(Top), typeof(float),
                         typeof(ScadaRobot), null, BindingMode.OneWay,
                         validateValue: (_, value) => value != null,
                         propertyChanged: OnPropertyChangedInvalidate);

        public float Top
        {
            get => (float)GetValue(TopProperty);
            set => SetValue(TopProperty, value);
        }

        public static BindableProperty UpperArmProperty = BindableProperty.Create(nameof(UpperArm), typeof(ItemValue),
            typeof(ScadaRobot), null, BindingMode.OneWay,
            validateValue: (_, value) => value != null,
            propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue UpperArm
        {
            get => (ItemValue)GetValue(UpperArmProperty);
            set => SetValue(UpperArmProperty, value);
        }

        public static BindableProperty LowerArmProperty = BindableProperty.Create(nameof(LowerArm), typeof(ItemValue),
              typeof(ScadaRobot), null, BindingMode.OneWay,
              validateValue: (_, value) => value != null,
              propertyChanged: OnPropertyChangedInvalidate);
        public ItemValue LowerArm
        {
            get => (ItemValue)GetValue(UpperArmProperty);
            set => SetValue(UpperArmProperty, value);
        }

        public static BindableProperty GripperProperty = BindableProperty.Create(nameof(Gripper), typeof(ItemValue),
               typeof(ScadaRobot), null, BindingMode.OneWay,
               validateValue: (_, value) => value != null,
               propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue Gripper
        {
            get => (ItemValue)GetValue(UpperArmProperty);
            set => SetValue(UpperArmProperty, value);
        }

        public static BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(float),
            typeof(ScadaRobot), 5f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float CornerRadius
        {
            get => (float)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static BindableProperty BarBackgroundColorProperty = BindableProperty.Create(nameof(BarBackgroundColor), typeof(SKColor),
            typeof(ScadaRobot), SKColors.White, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor BarBackgroundColor
        {
            get => (SKColor)GetValue(BarBackgroundColorProperty);
            set => SetValue(BarBackgroundColorProperty, value);
        }

        public static BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float),
            typeof(ScadaRobot), 12f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float FontSize
        {
            get => (float)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static BindableProperty GradientStartColorProperty = BindableProperty.Create(nameof(GradientStartColor), typeof(SKColor),
            typeof(ScadaRobot), SKColors.Purple, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientStartColor
        {
            get => (SKColor)GetValue(GradientStartColorProperty);
            set => SetValue(GradientStartColorProperty, value);
        }

        public static BindableProperty GradientEndColorProperty = BindableProperty.Create(nameof(GradientEndColor), typeof(SKColor),
            typeof(ScadaRobot), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientEndColor
        {
            get => (SKColor)GetValue(GradientEndColorProperty);
            set => SetValue(GradientEndColorProperty, value);
        }

        public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(SKColor),
            typeof(ScadaRobot), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor TextColor
        {
            get => (SKColor)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public static BindableProperty AlternativeTextColorProperty = BindableProperty.Create(nameof(AlternativeTextColor), typeof(SKColor),
            typeof(ScadaRobot), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor AlternativeTextColor
        {
            get => (SKColor)GetValue(AlternativeTextColorProperty);
            set => SetValue(AlternativeTextColorProperty, value);
        }

        private static void OnPropertyChangedInvalidate(BindableObject bindable, object oldvalue, object newvalue)
        {
           /* var control = (ScadaRobot)bindable;
            if (oldvalue != newvalue)
                control.InvalidateSurface();
            */
        }

        bool AnimationRunning = false;
        float DV = 0;
        float OV = 0;

        public void Start()
        {
          if (AnimationRunning == false)
             UpdateArc();
        }

        public void Stop()
        {
            AnimationRunning = false;

        }

        private async void UpdateArc()
        {
            AnimationRunning = true;
           
            DV = XTraverse.Value;
            OV = DV;

            float Step = 0.0F;
            float DVStep;
            if ( UpperArm.Value > 0.5) 
            {
                UpperArmAngle = -(3.14F / 12);
                Step = -0.01F;
            }
            else
            {
                UpperArmAngle = (3.14F / 4);
                Step = 0.01F;
            }
          
            while (AnimationRunning)
            {
                if (Math.Abs(OV - XTraverse.Value) > 0.1F)
                {
                   OV = XTraverse.Value + 1F;
                }
                if (UpperArmAngle > (3.14F / 4))
                {
                   Step = -0.01F;
                   if (UpperArm.Value < 0.5)
                     { Step = 0; }
                }
                if (UpperArmAngle < -(3.14F / 12))
                {
                   Step = 0.01F;
                   if (UpperArm.Value > 0.5)
                        { Step = 0F; }
                }
                LowerArmAngle = LowerArmAngle - Step;
                UpperArmAngle = UpperArmAngle + (Step * 3.0F);

                if (IsLoaded == true)
                {
                   if (Math.Abs(DV - OV) > 0.1F)
                   {
                      DVStep = (DV - OV) / 100.0F * -1.0F;
                      DV = DV + DVStep;
                   }

                   AbsoluteLayout.SetLayoutBounds(this, new Rect(
                        (WindowWidth / 100) * (Left + DV),
                        (WindowHeight / 100) * (Top + YTraverse.Value),
                        (WindowWidth / 100) * 12,
                        (WindowHeight / 100) * 12));                        
                   AbsoluteLayout.SetLayoutFlags(this, AbsoluteLayoutFlags.None);          
                   InvalidateSurface();             
                }
                await Task.Delay(20);
            }         
        }


        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            //var info = e.Info;
            var canvas = e.Surface.Canvas;

            LowerLinePaint.StrokeWidth = LowerArmWidth;
            UpperLinePaint.StrokeWidth = UpperArmWidth;
           
            MiddleJoint.X = LowerJoint.X + LowerArmLength * (float)Math.Cos(LowerArmAngle);
            MiddleJoint.Y = LowerJoint.Y + LowerArmLength * (float)Math.Sin(LowerArmAngle);

            GripperJoint.X = MiddleJoint.X + UpperArmLength * (float)Math.Cos(UpperArmAngle);
            GripperJoint.Y = MiddleJoint.Y + UpperArmLength * (float)Math.Sin(UpperArmAngle);
    
            canvas.Clear();
            canvas.DrawRect(new SKRect(6F, 66F, 54F, 85F), BasePaint);
            canvas.DrawCircle(LowerJoint.X, LowerJoint.Y + 8F, 24.5F, BasePaint);
            canvas.DrawLine(MiddleJoint.X, MiddleJoint.Y, GripperJoint.X, GripperJoint.Y, UpperLinePaint);
            canvas.DrawLine(LowerJoint.X, LowerJoint.Y, MiddleJoint.X, MiddleJoint.Y, LowerLinePaint);
            canvas.DrawLine(5F, 87F, 56F, 87F, BasePlatePaint);
            
            canvas.DrawCircle(LowerJoint.X, LowerJoint.Y, 5F, jointStroke);
            canvas.DrawCircle(MiddleJoint.X, MiddleJoint.Y, 5F, jointStroke);
          
            canvas.DrawCircle(GripperJoint.X, GripperJoint.Y, 12.0F, GripperJointPaint);
            canvas.DrawCircle(GripperJoint.X, GripperJoint.Y, 5F, jointStroke);
            canvas.DrawCircle(GripperJoint.X+10F, GripperJoint.Y+10F, 5F, jointStroke);
            canvas.DrawArc(new SKRect(GripperJoint.X + 10F, GripperJoint.Y + 20F, GripperJoint.X + 10F, GripperJoint.Y + 20F), 3.1415F, 0F,true, jointStroke);
        }
    }
}