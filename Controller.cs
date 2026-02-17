

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;


namespace Scada
{
    public class Controller : SKCanvasView
    {
        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);
        PID MyPID = new PID();

        public static BindableProperty ItemIDProperty = BindableProperty.Create(nameof(ItemID), typeof(int),
        typeof(Controller), 0, BindingMode.OneWay,
            validateValue: (_, value) => value != null,
        propertyChanged: OnPropertyChangedInvalidate);

        public int ItemID
        {
            get => (int)GetValue(ItemIDProperty);
            set => SetValue(ItemIDProperty, value);
        }


        public static BindableProperty PVProperty = BindableProperty.Create(nameof(PV), typeof(ItemValue),
             typeof(Controller), null, BindingMode.OneWay,
             validateValue: (_, value) => value != null,
             propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue PV
        {
            get => (ItemValue)GetValue(PVProperty);
            set => SetValue(PVProperty, value);
        }


        public static BindableProperty SVProperty = BindableProperty.Create(nameof(SV), typeof(ItemValue),
           typeof(Controller), null, BindingMode.OneWay,
           validateValue: (_, value) => value != null,
           propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue SV
        {
            get => (ItemValue)GetValue(SVProperty);
            set => SetValue(SVProperty, value);
        }


        public static BindableProperty OutputProperty = BindableProperty.Create(nameof(Output), typeof(ItemValue),
           typeof(Controller), null, BindingMode.OneWay,
           validateValue: (_, value) => value != null,
           propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue Output
        {
            get => (ItemValue)GetValue(OutputProperty);
            set => SetValue(OutputProperty, value);
        }


        public static BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(float),
            typeof(Controller), 5f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);


        public float CornerRadius
        {
            get => (float)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static BindableProperty BarBackgroundColorProperty = BindableProperty.Create(nameof(BarBackgroundColor), typeof(SKColor),
            typeof(Controller), SKColors.White, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor BarBackgroundColor
        {
            get => (SKColor)GetValue(BarBackgroundColorProperty);
            set => SetValue(BarBackgroundColorProperty, value);
        }

        public static BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float),
            typeof(Controller), 12f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float FontSize
        {
            get => (float)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static BindableProperty GradientStartColorProperty = BindableProperty.Create(nameof(GradientStartColor), typeof(SKColor),
            typeof(Controller), SKColors.Purple, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientStartColor
        {
            get => (SKColor)GetValue(GradientStartColorProperty);
            set => SetValue(GradientStartColorProperty, value);
        }

        public static BindableProperty GradientEndColorProperty = BindableProperty.Create(nameof(GradientEndColor), typeof(SKColor),
            typeof(Controller), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientEndColor
        {
            get => (SKColor)GetValue(GradientEndColorProperty);
            set => SetValue(GradientEndColorProperty, value);
        }

        public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(SKColor),
            typeof(Controller), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor TextColor
        {
            get => (SKColor)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public static BindableProperty AlternativeTextColorProperty = BindableProperty.Create(nameof(AlternativeTextColor), typeof(SKColor),
            typeof(Controller), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor AlternativeTextColor
        {
            get => (SKColor)GetValue(AlternativeTextColorProperty);
            set => SetValue(AlternativeTextColorProperty, value);
        }

        private static void OnPropertyChangedInvalidate(BindableObject bindable, object oldvalue, object newvalue)
        {
            var control = (Controller)bindable;

            if (oldvalue != newvalue)
                control.InvalidateSurface();
        }


        float PVold = 0F;
        float IntegralSum = 0F;
        bool AnimationRunning = false;
        float DV = 0;
        float OV = 0;

        public void Start()
        {
            MyPID.initPID(1.4, 0.05, 0, 100, 0, 100, 0);
            if (AnimationRunning == false)
                UpdateAnimation();
        }

        private async void UpdateAnimation()
        {
            AnimationRunning = true;
            DV = PV.Value;
            OV = DV;
            PVold=PV.Value;
            while (AnimationRunning)
            {
                await Task.Delay(50);
                if (Math.Abs(DV - OV) > 0.05)
                {
                    float Step = ((DV - OV) / 10) * -1;
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
            float MaxValue = 100;
        
            var backgroundBar = new SKRoundRect(new SKRect(0, 0, info.Width, info.Height), 5, 5);
            var Background = new SKPaint { Color = GradientStartColor, IsAntialias = true };

            if (PV.Value > MaxValue)
            {
                PV.Value = MaxValue;
            }
            
            /*
            if (Math.Abs(OV - PV.Value) > 0.1)
            {
                OV = PV.Value;
            }
            */


            //   On/Off Control 
            
            float Hysteresis = MaxValue/50.0F;

            /*
            if ( PV.Value > ( SV.Value + Hysteresis ) )
            {
               Output.Value = 0;
            }
            if ( PV.Value < ( SV.Value - Hysteresis ) )
            {
               Output.Value = 1;
            }
            */

            MyPID.pv = PV.Value;
            MyPID.sp = SV.Value;

            float kP = 10F;
            float kI = 0;
            float kD = 3F;

            float P = kP * (SV.Value-PV.Value);
            IntegralSum = IntegralSum + (SV.Value - PV.Value);
            if (IntegralSum > 50)
            {
                IntegralSum = 50;
            }
            if (IntegralSum < -50)
            {
                IntegralSum = -50;
            }
            float I = kI + IntegralSum;
            float D = kD * (PV.Value-PVold);

            Output.Value = P + I + D;
            if (Output.Value > 100)
            {
                Output.Value = 100;
            }
            if (Output.Value < 0)
            {
                Output.Value = 0;
            }
            PVold = PV.Value;

       
            //Output.Value = (float)MyPID.ComputePID();

            MyDataAccessLayer.UpdateTagValue(Output.TagID, Output.Value);
            MyDataAccessLayer.StoreTagValue(Output.TagID, Output.Value);
            
            canvas.Clear();
            canvas.DrawRoundRect(backgroundBar, Background);
          
        }
    }
}