
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using Svg.Skia;
using System.Text;


namespace Scada
{
    public class ItemValue
    {
        public int TagID { get; set; }
        public float Value { get; set; }
        public string? Unit { get; set; }
        public int StatusQuality { get; set; }

    }

    public class gridRow
    {
        public string col1text { get; set; } = "";
        public float col1width { get; set; }
        public string col2text { get; set; } = "";
        public float col2width { get; set; }
        public string col3text { get; set; } = "";
        public float col3width { get; set; }
        public string col4text { get; set; } = "";
        public float col4width { get; set; }
        public string col5text { get; set; } = "";
        public float col5width { get; set; }
        public string col6text { get; set; } = "";
        public float col6width { get; set; }

        public int MsgID { get; set; }
        public int ItemID { get; set; }
        public int TagID { get; set; }
        public int TagSequence { get; set; }
        public int Status { get; set; }
        public int Row { get; set; }
        public int DataType { get; set; }
        public int Id { get; set; }
    }


    public class ScadaButton : SKCanvasView
    {
        bool blink = false;
        double intensity = 100f;

        public static BindableProperty ItemIDProperty = BindableProperty.Create(nameof(ItemID), typeof(int),
        typeof(ScadaButton), 0, BindingMode.OneWay,
            validateValue: (_, value) => value != null,
        propertyChanged: OnPropertyChangedInvalidate);

        public int ItemID
        {
            get => (int)GetValue(ItemIDProperty);
            set => SetValue(ItemIDProperty, value);
        }

        public static BindableProperty PVProperty = BindableProperty.Create(nameof(PV), typeof(ItemValue),
              typeof(ScadaButton), null, BindingMode.OneWay,
              validateValue: (_, value) => value != null,
              propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue PV
        {
            get => (ItemValue)GetValue(PVProperty);
            set => SetValue(PVProperty, value);
        }

        public static BindableProperty SVProperty = BindableProperty.Create(nameof(SV), typeof(ItemValue),
           typeof(ScadaButton), null, BindingMode.OneWay,
           validateValue: (_, value) => value != null,
           propertyChanged: OnPropertyChangedInvalidate);

        public ItemValue SV
        {
            get => (ItemValue)GetValue(SVProperty);
            set => SetValue(SVProperty, value);
        }



        public static BindableProperty ButtonRowProperty = BindableProperty.Create(nameof(ButtonRow), typeof(gridRow),
            typeof(ScadaButton), null, BindingMode.OneWay,
            validateValue: (_, value) => value != null,
            propertyChanged: OnPropertyChangedInvalidate);

        public gridRow ButtonRow
        {
            get => (gridRow)GetValue(ButtonRowProperty);
            set => SetValue(ButtonRowProperty, value);
        }

        public static BindableProperty ButtonTextProperty = BindableProperty.Create(nameof(ButtonText), typeof(string),
        typeof(ScadaButton), "", BindingMode.OneWay,
        validateValue: (_, value) => value != null,
        propertyChanged: OnPropertyChangedInvalidate);

        public string ButtonText
        {
            get => (string)GetValue(ButtonTextProperty);
            set => SetValue(ButtonTextProperty, value);
        }

        public static BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(float),
            typeof(ScadaButton), 5f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float CornerRadius
        {
            get => (float)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static BindableProperty BarBackgroundColorProperty = BindableProperty.Create(nameof(BarBackgroundColor), typeof(SKColor),
            typeof(ScadaButton), SKColors.White, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor BarBackgroundColor
        {
            get => (SKColor)GetValue(BarBackgroundColorProperty);
            set => SetValue(BarBackgroundColorProperty, value);
        }

        public static BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float),
            typeof(ScadaButton), 12f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float FontSize
        {
            get => (float)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static BindableProperty MarginSizeProperty = BindableProperty.Create(nameof(Padding), typeof(float),
           typeof(ScadaButton), 0f, BindingMode.OneWay,
           validateValue: (_, value) => value != null && (float)value >= 0,
           propertyChanged: OnPropertyChangedInvalidate);

        public float Padding
        {
            get => (float)GetValue(MarginSizeProperty);
            set => SetValue(MarginSizeProperty, value);
        }

        public static BindableProperty GradientStartColorProperty = BindableProperty.Create(nameof(GradientStartColor), typeof(SKColor),
            typeof(ScadaButton), SKColors.Purple, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientStartColor
        {
            get => (SKColor)GetValue(GradientStartColorProperty);
            set => SetValue(GradientStartColorProperty, value);
        }

        public static BindableProperty GradientEndColorProperty = BindableProperty.Create(nameof(GradientEndColor), typeof(SKColor),
            typeof(ScadaButton), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientEndColor
        {
            get => (SKColor)GetValue(GradientEndColorProperty);
            set => SetValue(GradientEndColorProperty, value);
        }

        public static BindableProperty IndicatorTypeProperty = BindableProperty.Create(nameof(IndicatorType), typeof(int),
        typeof(ScadaButton), 1, BindingMode.OneWay,
         validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public int TagSequence
        {
            get => (int)GetValue(TagSequenceProperty);
            set => SetValue(TagSequenceProperty, value);
        }

        public static BindableProperty TagSequenceProperty = BindableProperty.Create(nameof(TagSequence), typeof(int),
                typeof(ScadaButton), 1, BindingMode.OneWay,
                 validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public int IndicatorType
        {
            get => (int)GetValue(IndicatorTypeProperty);
            set => SetValue(IndicatorTypeProperty, value);
        }



        public static BindableProperty IndicatorColorProperty = BindableProperty.Create(nameof(IndicatorColor), typeof(SKColor),
        typeof(ScadaButton), SKColors.Black, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor IndicatorColor
        {
            get => (SKColor)GetValue(IndicatorColorProperty);
            set => SetValue(IndicatorColorProperty, value);
        }

        public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(SKColor),
            typeof(ScadaButton), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor TextColor
        {
            get => (SKColor)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public static BindableProperty AlternativeTextColorProperty = BindableProperty.Create(nameof(AlternativeTextColor), typeof(SKColor),
            typeof(ScadaButton), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor AlternativeTextColor
        {
            get => (SKColor)GetValue(AlternativeTextColorProperty);
            set => SetValue(AlternativeTextColorProperty, value);
        }

        public static BindableProperty SvgBase64Property = BindableProperty.Create(nameof(SvgBase64), typeof(string),
        typeof(ScadaButton), "", BindingMode.OneWay,
        validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public string SvgBase64
        {
            get => (string)GetValue(SvgBase64Property);
            set => SetValue(SvgBase64Property, value);
        }

        private static void OnPropertyChangedInvalidate(BindableObject bindable, object oldvalue, object newvalue)
        {
            var control = (ScadaButton)bindable;
            if (oldvalue != newvalue)
                if (control.IsLoaded == true)
                {
                    control.InvalidateSurface();
                }
        }

        private SKCanvas DrawRoundRectWithArrow(SKCanvas c, SKPaint Paint, float x, float y, float w, float h, float radius)
        {
            var Rect = new SKRect(0, 0, w, h);
            var RoundRect = new SKRoundRect(Rect, 10, 10);
            c.DrawRoundRect(RoundRect, Paint);
            return c;
        }

        public void Start()
        {
            UpdateArc();
        }

        public void Stop()
        {

        }

        private async void UpdateArc()
        {

            var Step = 10;
            while (true)
            {
                await Task.Delay(30);
                if (blink == true)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (IsLoaded == true)
                        {
                            InvalidateSurface();
                        }
                        intensity = intensity - Step;
                        await Task.Delay(30);
                    }
                }
            }
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;
            var canvas = e.Surface.Canvas;
            float w = info.Width;
            float h = info.Height;
            var progressBar = new SKRoundRect(new SKRect(0, 0, w, h), CornerRadius, CornerRadius);

            canvas.Clear();
            using (var paint = new SKPaint() { IsAntialias = true, FilterQuality = SKFilterQuality.High, BlendMode = SKBlendMode.Overlay })
            {
                var Rectangle = new SKRect(0, 0, w, h);
                paint.Shader = SKShader.CreateLinearGradient(
                    new SKPoint(Rectangle.Left, Rectangle.Top),
                    new SKPoint(Rectangle.Right, Rectangle.Bottom),
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

                var textBounds = new SKRect();
                NewTextPaint.MeasureText("GgA!j@", ref textBounds);
                float xText = 22F;
                float yText = h - (FontSize / 2);

                if (IndicatorType == 0)
                {
                    canvas.DrawRoundRect(progressBar, paint);
                    NewTextPaint.MeasureText(ButtonText, ref textBounds);
                    float x = info.Width / 2 - textBounds.MidX;
                    float y = info.Height / 2 - textBounds.MidY;
                    canvas.DrawText(ButtonText, x, y, NewTextPaint);
                }

                if (IndicatorType == 1)
                {
                    var palPaint = new SKPaint { Color = Colors.Blue.ToSKColor(), TextSize = FontSize, FilterQuality = SKFilterQuality.High, IsAntialias = true };
                    palPaint.Color = IndicatorColor;
                    canvas.DrawRoundRect(progressBar, paint);
                    //canvas.DrawRect((w / 10), (float)(h - (h / 3)), (float)(w - (2 * (w / 10))), (float)(h / 5), palPaint);
                    NewTextPaint.MeasureText(ButtonText, ref textBounds);
                    float x = info.Width / 2 - textBounds.MidX;
                    float y = info.Height / 2 - textBounds.MidY;
                    canvas.DrawText(ButtonText, x, y, NewTextPaint);
                }

                if (IndicatorType == 2)
                {
                    var circlePaint = new SKPaint
                    {
                        IsAntialias = true,
                        Style = SKPaintStyle.Fill,
                        Color = IndicatorColor,
                        FilterQuality = SKFilterQuality.High,
                        StrokeWidth = 0
                    };

                    if (ButtonRow.Status == 1)
                    {
                        blink = true;
                        circlePaint.Color = new SKColor((byte)(intensity), 32, 32, 255);
                    }

                    if (ButtonRow.Status == 2)
                    {
                        blink = false;
                        circlePaint.Color = new SKColor(210, 32, 32, 255);
                    }

                    float h1 = info.Height;
                    float radius = (h1 / 2.5f);
                    var center = new SKPoint(radius + 2.5f, (info.Height / 2) + 0.5f);
                    NewTextPaint.MeasureText(ButtonRow.col6text, ref textBounds);
                    float y = info.Height / 2 - textBounds.MidY;


                    canvas.DrawRoundRect(progressBar, paint);
                    canvas.DrawCircle(center, radius, circlePaint);
                    canvas.DrawText(ButtonRow.col1text, xText + 5, y, NewTextPaint);
                    xText = xText + ButtonRow.col1width;

                    canvas.DrawText(ButtonRow.col2text, xText, y, NewTextPaint);
                    xText = xText + ButtonRow.col2width;

                    canvas.DrawText(ButtonRow.col3text, xText, y, NewTextPaint);
                    xText = xText + ButtonRow.col3width;

                    canvas.DrawText(ButtonRow.col4text, xText, y, NewTextPaint);
                    xText = xText + ButtonRow.col4width;

                    canvas.DrawText(ButtonRow.col5text, xText, y, NewTextPaint);
                    xText = xText + ButtonRow.col5width;

                    float tw = NewTextPaint.MeasureText(ButtonRow.col6text, ref textBounds);
                    canvas.DrawText(ButtonRow.col6text, w - tw - 5, y, NewTextPaint);
                }


                if (IndicatorType == 3)
                {
                    if (SvgBase64 == "") return;

                    var svg2 = new SKSvg();
                    try
                    {
                        string sBase64Svg = SvgBase64;
                        byte[] data = Convert.FromBase64String(sBase64Svg);
                        string decodedString = Encoding.UTF8.GetString(data);
                        var picture = svg2.FromSvg(decodedString);

                        var dimension = new SkiaSharp.SKSizeI
                        (
                             (int)Math.Ceiling(info.Height * 1.0),
                             (int)Math.Ceiling(info.Height * 1.0)
                        );

                        float Offset = info.Height - h + info.Height * Padding;
                        float ScaleX = (info.Height / picture.CullRect.Width) * 0.9F;
                        float ScaleY = (info.Height / picture.CullRect.Height) * 0.9F;
                        var matrix = SKMatrix.CreateScale(ScaleX * (1 - Padding * 2), ScaleY * (1 - Padding * 2));
                        var img = SKImage.FromPicture(picture, dimension, matrix);

                        canvas.DrawRoundRect(progressBar, paint);
                        canvas.DrawImage(img, new SKPoint(info.Height / 20, info.Height / 20));

                        //NewTextPaint.MeasureText(ButtonText, ref textBounds);
                        //float x = info.Width / 2 - textBounds.MidX;
                        //canvas.DrawText(ButtonText, x, yText, NewTextPaint);

                        canvas.DrawText(ButtonText, xText + info.Height, yText, NewTextPaint);
                    }
                    catch
                    {
                    }
                    
                }

                if (IndicatorType == 4)
                {

                }

                if (IndicatorType == 5)
                {
                    var onPaint = new SKPaint { Color = new SKColor(100, 100, 100, 255), TextSize = FontSize, FilterQuality = SKFilterQuality.High, IsAntialias = true };

                    if (PV.Value > 0.5)
                    {
                        onPaint.Color = new SKColor(3, 156, 35, 255);
                    }
                    else
                    {
                        onPaint.Color = new SKColor(100, 100, 100, 255);
                    }
                    canvas.DrawRoundRect(progressBar, paint);
                    canvas.DrawRect((0), (float)(h / 6), (float)(w), (float)(h / 5), onPaint);
                    NewTextPaint.MeasureText(ButtonText, ref textBounds);
                    float x = info.Width / 2 - textBounds.MidX;
                    canvas.DrawText(ButtonText, x, yText, NewTextPaint);
                }

                // Circular button Draw the svg indicator
                if (IndicatorType == 9)
                {
                    float h1 = info.Height;
                    float radius = (h1 / 2.5f);
                    var center = new SKPoint(radius + 2.5f, (info.Height / 2) + 0.5f);

                    //Draw Circle
                    var circlePaint = new SKPaint { Color = GradientStartColor, TextSize = FontSize, FilterQuality = SKFilterQuality.High, IsAntialias = true };
                    canvas.DrawCircle(center, radius, circlePaint);

                    
                    if (SvgBase64 == "") return;
                    var svg2 = new SKSvg();
                    string sBase64Svg = SvgBase64;
                    byte[] data = Convert.FromBase64String(sBase64Svg);
                    string decodedString = Encoding.UTF8.GetString(data);

                    var picture = svg2.FromSvg(decodedString);
                    //var picture = svg2.FromSvg(decodedString);
                    var dimension = new SkiaSharp.SKSizeI
                    (
                         (int)Math.Ceiling(info.Height * 1.0),
                         (int)Math.Ceiling(info.Height * 1.0)
                    );
                    float Offset = info.Height - h + info.Height * Padding;
                    float ScaleX = (info.Height / picture.CullRect.Width) * 0.6F;
                    float ScaleY = (info.Height / picture.CullRect.Height) * 0.6F;
                    var matrix = SKMatrix.CreateScale(ScaleX * (1 - Padding * 2), ScaleY * (1 - Padding * 2));
                    var img = SKImage.FromPicture(picture, dimension, matrix);
                    canvas.DrawImage(img, new SKPoint(Offset + 9, Offset + 11));
                    
                    canvas.DrawText(ButtonText, xText + info.Height, yText, NewTextPaint);
                }

                if (IndicatorType == 10)
                {
                    DrawRoundRectWithArrow(canvas, paint, 0, 0, w, h, 10);
                }

                if (IndicatorType == 11)
                {
                    if (SvgBase64 == "") return;
                    
                    var svg2 = new SKSvg();
                    string sBase64Svg = SvgBase64;
                    byte[] data = Convert.FromBase64String(sBase64Svg);
                    string decodedString = Encoding.UTF8.GetString(data);
                    var picture = svg2.FromSvg(decodedString);

                    var dimension = new SkiaSharp.SKSizeI
                    (
                         (int)Math.Ceiling(info.Height * 1.0),
                         (int)Math.Ceiling(info.Height * 1.0)
                    );

                    float Offset = info.Height - h + info.Height * Padding;
                    float ScaleX = (info.Height / picture.CullRect.Width) * 0.6F;
                    float ScaleY = (info.Height / picture.CullRect.Height) * 0.6F;
                    var matrix = SKMatrix.CreateScale(ScaleX * (1 - Padding * 2), ScaleY * (1 - Padding * 2));
                    var img = SKImage.FromPicture(picture, dimension, matrix);

                    canvas.DrawRoundRect(progressBar, paint);
                    canvas.DrawImage(img, new SKPoint(16, 6));
                    
                    //NewTextPaint.MeasureText(ButtonText, ref textBounds);
                    //float x = info.Width / 2 - textBounds.MidX;
                    //canvas.DrawText(ButtonText, x, yText, NewTextPaint);

                    canvas.DrawText(ButtonText, xText + info.Height, yText, NewTextPaint);
                }

            }

        }
    }
}