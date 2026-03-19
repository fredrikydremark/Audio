

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;


namespace Scada
{

    public class Scada3D : SKCanvasView
    {
        public Scada3D Init(double w, double h, Scada3D scb, ScadaClasses.Telegram Item, ScadaClasses.SystemColors Color)
        {
            scb.AnchorX = 0;
            scb.AnchorY = 0;
            scb.CornerRadius = 10;
            scb.BarBackgroundColor = Color.uxBackGroundColor;
            scb.BackgroundColor = Color.uxPanelColor.ToMauiColor();
            scb.GradientStartColor = Color.uxGradientStartColor;
            scb.GradientEndColor = Color.uxGradientEndColor;
            scb.WidthRequest = (w / 100) * Item.Width;
            scb.HeightRequest = (h / 100) * Item.Height;
            scb.AlternativeTextColor = Color.uxTextColor;
            scb.TextColor = Color.uxTextColor;
            scb.ItemID = Item.ItemID;
            scb.StyleId = Item.ItemID.ToString();
            scb.FontSize = 18.5F;
            scb.PV = new ItemValue();
            DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);
            var ItemValues = MyDataAccessLayer.ReadItemValues(scb.ItemID);
            int i = 0;
            foreach (var item in ItemValues)
            {
                if (i == 0)
                {
                    scb.PV.TagID = item.TagID;
                    scb.PV.Value = item.Value;
                    scb.PV.Unit = item.Unit;
                    scb.PV.StatusQuality = item.StatusQuality;
                }
                i++;
            }
            scb.IsEnabled = true;
            scb.IsVisible = true;
            scb.EnableTouchEvents = true;
            scb.InputTransparent = false;
            scb.Start();
            return (scb);
        }


        public static BindableProperty ItemIDProperty = BindableProperty.Create(nameof(ItemID), typeof(int),
        typeof(Scada3D), 0, BindingMode.OneWay,
            validateValue: (_, value) => value != null,
        propertyChanged: OnPropertyChangedInvalidate);

        public int ItemID
        {
            get => (int)GetValue(ItemIDProperty);
            set => SetValue(ItemIDProperty, value);
        }

        public static BindableProperty PVProperty = BindableProperty.Create(nameof(PV), typeof(ItemValue),
                  typeof(Scada3D), null, BindingMode.OneWay,
                  validateValue: (_, value) => value != null,
                  propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue PV
        {
            get => (ItemValue)GetValue(PVProperty);
            set => SetValue(PVProperty, value);
        }




        public static BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(float),
            typeof(Scada3D), 5f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float CornerRadius
        {
            get => (float)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static BindableProperty BarBackgroundColorProperty = BindableProperty.Create(nameof(BarBackgroundColor), typeof(SKColor),
            typeof(Scada3D), SKColors.White, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor BarBackgroundColor
        {
            get => (SKColor)GetValue(BarBackgroundColorProperty);
            set => SetValue(BarBackgroundColorProperty, value);
        }

        public static BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float),
            typeof(Scada3D), 12f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float FontSize
        {
            get => (float)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static BindableProperty GradientStartColorProperty = BindableProperty.Create(nameof(GradientStartColor), typeof(SKColor),
            typeof(Scada3D), SKColors.Purple, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientStartColor
        {
            get => (SKColor)GetValue(GradientStartColorProperty);
            set => SetValue(GradientStartColorProperty, value);
        }

        public static BindableProperty GradientEndColorProperty = BindableProperty.Create(nameof(GradientEndColor), typeof(SKColor),
            typeof(Scada3D), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientEndColor
        {
            get => (SKColor)GetValue(GradientEndColorProperty);
            set => SetValue(GradientEndColorProperty, value);
        }

        public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(SKColor),
            typeof(Scada3D), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor TextColor
        {
            get => (SKColor)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public static BindableProperty AlternativeTextColorProperty = BindableProperty.Create(nameof(AlternativeTextColor), typeof(SKColor),
            typeof(Scada3D), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor AlternativeTextColor
        {
            get => (SKColor)GetValue(AlternativeTextColorProperty);
            set => SetValue(AlternativeTextColorProperty, value);
        }

        private static void OnPropertyChangedInvalidate(BindableObject bindable, object oldvalue, object newvalue)
        {
            var control = (Scada3D)bindable;
            if (control != null)
            {
                if (oldvalue != newvalue)
                    control.InvalidateSurface();
            }
        }





        bool AnimationRunning = false;
        float XCam = -25f;
        float YCam = 50F;
        float YDirection = 0.1F;



        public void Start()
        {
            if (AnimationRunning == false)
                UpdateAnimation();
        }

        private async void UpdateAnimation()
        {

            AnimationRunning = true;


            while (AnimationRunning)
            {
                YCam = YCam + YDirection;

                if (YCam > 75)
                {
                    YDirection = -0.1F;
                }
                if (YCam < 25)
                {
                    YDirection = 0.1F;
                }
                await Task.Delay(10);
                if (IsLoaded == true)
                {
                    InvalidateSurface();
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


            float h = info.Height;
            float w = info.Width;

            //var info = new SKImageInfo(256, 256);
            //using (var surface = SKSurface.Create(info))
            //{
            //SKCanvas canvas = surface.Canvas;

            canvas.Clear(SKColors.Black);

            // center the entire drawing
            canvas.Translate(128, 128);

            // the "3D camera"
            var view = new SK3dView();

            // rotate to a nice 3D view
            view.RotateXDegrees(XCam);
            view.RotateYDegrees(YCam);

            // move the origin of the 3D view
            view.Translate(-50, 50, 50);

            // define the cube face
            var face = SKRect.Create(0, 0, 100, 100);

            // draw the left face
            using (new SKAutoCanvasRestore(canvas, true))
            {
                // get the face in the correct location
                view.Save();
                view.RotateYDegrees(-90);
                view.ApplyToCanvas(canvas);
                view.Restore();

                // draw the face
                var leftFace = new SKPaint
                {
                    Color = SKColors.LightGray,
                    IsAntialias = true
                };
                canvas.DrawRect(face, leftFace);
            }

            // draw the right face
            using (new SKAutoCanvasRestore(canvas, true))
            {
                // get the face in the correct location
                view.Save();
                view.TranslateZ(-100);
                view.ApplyToCanvas(canvas);
                view.Restore();

                // draw the face
                var rightFace = new SKPaint
                {
                    Color = SKColors.Gray,
                    IsAntialias = true
                };
                canvas.DrawRect(face, rightFace);
            }

            // draw the top face
            using (new SKAutoCanvasRestore(canvas, true))
            {
                // get the face in the correct location
                view.Save();
                view.RotateXDegrees(90);
                view.ApplyToCanvas(canvas);
                view.Restore();

                // draw the face
                var topFace = new SKPaint
                {
                    Color = SKColors.DarkGray,
                    IsAntialias = true
                };
                canvas.DrawRect(face, topFace);
            }
            // }
        }
    }
}