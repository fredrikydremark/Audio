using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using Svg.Skia;
using System.Text;

namespace Scada
{
    public class ScadaSvg : SKCanvasView
    {   public ScadaSvg Init(double w, double h, ScadaSvg scb, ScadaClasses.Telegram Item, ScadaClasses.Colors Color)
        {
            scb.AnchorX = 0;
            scb.AnchorY = 0;
            scb.CornerRadius = 10;
            scb.BarBackgroundColor = Color.uxBackGroundColor;
            scb.BackgroundColor = Color.uxPanelColor.ToMauiColor();
            scb.GradientStartColor = Color.uxGradientStartColor;
            scb.GradientEndColor = Color.uxGradientEndColor;
            scb.WidthRequest = w * Item.Width;
            scb.HeightRequest = h  * Item.Height;
            scb.ItemID = Item.ItemID;
            scb.StyleId = Item.ItemID.ToString();       
            DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);
            scb.SvgBase64 = MyDataAccessLayer.LoadLibItem(Item.Action, 1);
            scb.IsEnabled = true;
            scb.IsVisible = true;
            scb.EnableTouchEvents = true;
            scb.InputTransparent = false;    
            return (scb);
        }

       public static BindableProperty ItemIDProperty = BindableProperty.Create(nameof(ItemID), typeof(int),
       typeof(ScadaSvg), 0, BindingMode.OneWay,
           validateValue: (_, value) => value != null,
       propertyChanged: OnPropertyChangedInvalidate);

        public int ItemID
        {
            get => (int)GetValue(ItemIDProperty);
            set => SetValue(ItemIDProperty, value);
        }

        public static BindableProperty XPositionProperty = BindableProperty.Create(nameof(XPosition), typeof(float),
                typeof(ScadaSvg), 50f, BindingMode.OneWay,
                validateValue: (_, value) => value != null && (float)value >= 0,
                propertyChanged: OnPropertyChangedInvalidate);

        public float XPosition
        {
            get => (float)GetValue(XPositionProperty);
            set => SetValue(XPositionProperty, value);
        }

        public static BindableProperty YPositionProperty = BindableProperty.Create(nameof(YPosition), typeof(float),
                    typeof(ScadaSvg), 40f, BindingMode.OneWay,
                    validateValue: (_, value) => value != null && (float)value >= 0,
                    propertyChanged: OnPropertyChangedInvalidate);

        public float YPosition
        {
            get => (float)GetValue(YPositionProperty);
            set => SetValue(YPositionProperty, value);
        }

        public static BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(float),
            typeof(ScadaSvg), 40f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float CornerRadius
        {
            get => (float)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static BindableProperty BarBackgroundColorProperty = BindableProperty.Create(nameof(BarBackgroundColor), typeof(SKColor),
            typeof(ScadaSvg), SKColors.White, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor BarBackgroundColor
        {
            get => (SKColor)GetValue(BarBackgroundColorProperty);
            set => SetValue(BarBackgroundColorProperty, value);
        }

        public static BindableProperty GradientStartColorProperty = BindableProperty.Create(nameof(GradientStartColor), typeof(SKColor),
            typeof(ScadaSvg), SKColors.Purple, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientStartColor
        {
            get => (SKColor)GetValue(GradientStartColorProperty);
            set => SetValue(GradientStartColorProperty, value);
        }

        public static BindableProperty GradientEndColorProperty = BindableProperty.Create(nameof(GradientEndColor), typeof(SKColor),
            typeof(ScadaSvg), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientEndColor
        {
            get => (SKColor)GetValue(GradientEndColorProperty);
            set => SetValue(GradientEndColorProperty, value);
        }

        public static BindableProperty sProperty = BindableProperty.Create(nameof(SvgBase64), typeof(string),
        typeof(ScadaSvg), "", BindingMode.OneWay,
        validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public string SvgBase64
        {
            get => (string)GetValue(sProperty);
            set => SetValue(sProperty, value);
        }

        private static void OnPropertyChangedInvalidate(BindableObject bindable, object oldvalue, object newvalue)
        {
            var control = (ScadaSvg)bindable;
            if (oldvalue != newvalue)
                control.InvalidateSurface();
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;
            var canvas = e.Surface.Canvas;
            if (SvgBase64 == "") return;
            try
            {
                string sBase64Svg = SvgBase64;
                byte[] data = Convert.FromBase64String(sBase64Svg);
                string decodedString = Encoding.UTF8.GetString(data);
                SKSvg svg = new SKSvg();
                var picture = svg.FromSvg(decodedString);
                if (picture != null)
                {
                    var dimensions = new SKSizeI
                    (
                         (int)Math.Ceiling(info.Width * 1.0),
                         (int)Math.Ceiling(info.Height * 1.0)
                    );

                    float ScaleX = info.Width / picture.CullRect.Width;
                    float ScaleY = info.Height / picture.CullRect.Height;
                    SKMatrix matrix = SKMatrix.CreateScale(ScaleX, ScaleY);
                    SKImage image = SKImage.FromPicture(picture, dimensions, matrix);
                    canvas.Clear();
                    canvas.DrawImage(image, new SKPoint(0, 0));
                }
            }
            catch
            {
            }
        }
    }
}