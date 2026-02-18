
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace Scada
{
    public class Simulator : SKCanvasView
    {
        DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);

        public static BindableProperty ItemIDProperty = BindableProperty.Create(nameof(ItemID), typeof(int),
        typeof(Simulator), 0, BindingMode.OneWay,
            validateValue: (_, value) => value != null,
        propertyChanged: OnPropertyChangedInvalidate);

        public int ItemID
        {
            get => (int)GetValue(ItemIDProperty);
            set => SetValue(ItemIDProperty, value);
        }

        public static BindableProperty PVProperty = BindableProperty.Create(nameof(PV), typeof(ItemValue),
             typeof(Simulator), null, BindingMode.OneWay,
             validateValue: (_, value) => value != null,
             propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue PV
        {
            get => (ItemValue)GetValue(PVProperty);
            set => SetValue(PVProperty, value);
        }

        public static BindableProperty AcutatorProperty = BindableProperty.Create(nameof(Acutator), typeof(ItemValue),
           typeof(Simulator), null, BindingMode.OneWay,
           validateValue: (_, value) => value != null,
           propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue Acutator
        {
            get => (ItemValue)GetValue(AcutatorProperty);
            set => SetValue(AcutatorProperty, value);
        }

        public static BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(float),
            typeof(Simulator), 5f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);


        public float CornerRadius
        {
            get => (float)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static BindableProperty BarBackgroundColorProperty = BindableProperty.Create(nameof(BarBackgroundColor), typeof(SKColor),
            typeof(Simulator), SKColors.White, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor BarBackgroundColor
        {
            get => (SKColor)GetValue(BarBackgroundColorProperty);
            set => SetValue(BarBackgroundColorProperty, value);
        }

        public static BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float),
            typeof(Simulator), 12f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float FontSize
        {
            get => (float)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static BindableProperty GradientStartColorProperty = BindableProperty.Create(nameof(GradientStartColor), typeof(SKColor),
            typeof(Simulator), SKColors.Purple, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientStartColor
        {
            get => (SKColor)GetValue(GradientStartColorProperty);
            set => SetValue(GradientStartColorProperty, value);
        }

        public static BindableProperty GradientEndColorProperty = BindableProperty.Create(nameof(GradientEndColor), typeof(SKColor),
            typeof(Simulator), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientEndColor
        {
            get => (SKColor)GetValue(GradientEndColorProperty);
            set => SetValue(GradientEndColorProperty, value);
        }

        public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(SKColor),
            typeof(Simulator), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor TextColor
        {
            get => (SKColor)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public static BindableProperty AlternativeTextColorProperty = BindableProperty.Create(nameof(AlternativeTextColor), typeof(SKColor),
            typeof(Simulator), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor AlternativeTextColor
        {
            get => (SKColor)GetValue(AlternativeTextColorProperty);
            set => SetValue(AlternativeTextColorProperty, value);
        }

        private static void OnPropertyChangedInvalidate(BindableObject bindable, object oldvalue, object newvalue)
        {
            var control = (Simulator)bindable;
            if (oldvalue != newvalue)
                control.InvalidateSurface();
        }


        bool AnimationRunning = false;
        float DV = 0;
        float OV = 0;

        public void Start()
        {
            if (AnimationRunning == false)
                UpdateAnimation();
        }


        double green = 100f;

        private async void UpdateAnimation()
        {
            AnimationRunning = true;
            var Step = 1;
            while (AnimationRunning)
            {
                if (green > 230)
                {
                    Step = -2;
                }

                if (green < 100)
                {
                    Step = 10;
                }
                green = green + Step;

                await Task.Delay(50);
                if (Math.Abs(DV - OV) > 0.05)
                {
                    float Step2 = (DV - OV) / 10 * -1;
                    DV = DV + Step2;
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
            float MaxValue = 100F;
            float MinValue = 0F;
            var backgroundBar = new SKRoundRect(new SKRect(0, 0, info.Width, info.Height), 5, 5);
            var Background = new SKPaint { Color = GradientStartColor, IsAntialias = true };

            var ProgressPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = new SKColor(100, (byte)(green), 100, 200),
                StrokeWidth = 0
            };

            float DecreaseIncrease =  (50F - Acutator.Value)/25F;
            PV.Value = PV.Value - DecreaseIncrease;           
           
            //Limiter
            if (PV.Value > MaxValue)
            {
                PV.Value = MaxValue;
            }

            if (PV.Value < MinValue)
            {
                PV.Value = MinValue;
            }

            MyDataAccessLayer.UpdateTagValue(PV.TagID, PV.Value,0);
            MyDataAccessLayer.StoreTagValue(PV.TagID, PV.Value);
            canvas.Clear();
            canvas.DrawRoundRect(backgroundBar, Background);
            canvas.DrawArc(new SKRect(5, 5, info.Height / 5, info.Height / 5), -90, 360, false, ProgressPaint);
        }
    }
}