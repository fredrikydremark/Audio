
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using Svg.Skia;
using System.Text;


namespace Scada
{
    public class ItemValue
    {
        public int TagID { get; set; } = -1;
        public float Value { get; set; } = -1F;
        public string Unit { get; set; } = "";
        public int StatusQuality { get; set; } = -1;
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
        public string ?Color { get; set; }
        public int DataType { get; set; }
        public int Row { get; set; }    
        public int Id { get; set; }
    }

    public class ScadaButton : SKCanvasView
    {
        bool bFaceFade = true;
        bool bIndicatorFade = true;
        float localBtnIndicatorIntensity = 70f;
        float localBtnFaceIntensity = 0f;
        bool FaceFadeEnabled = false;

        public ScadaButton Init(double wScale, double hScale, ScadaButton scb, ScadaClasses.Telegram Item, ScadaClasses.SystemColors Color, bool Designing)
        {
            scb.AnchorX = 0;
            scb.AnchorY = 0;
            scb.CornerRadius = 10;
            scb.BarBackgroundColor = Color.uxBackGroundColor;
            scb.BackgroundColor = Color.uxPanelColor.ToMauiColor();
            scb.GradientStartColor = Color.uxItemColor;
            scb.GradientEndColor = Color.uxItemColor;
            scb.IndicatorColor = Color.uxOffColor;
            scb.IndicatorType = 3;
            scb.ButtonFaceIntensity = 0;
            scb.IndicatorIntensity = 0;
            

            if ((Item.Action == "TOGGLE") || (Item.Action == "ON") || (Item.Action == "OFF"))
            {
                scb.IndicatorType = 5;
                scb.IndicatorColor = Color.uxHoverColor;
            }

            scb.WidthRequest = wScale * Item.Width;
            scb.HeightRequest = hScale * Item.Height;
            scb.AlternativeTextColor = Color.uxTextColor;
            scb.TextColor = Color.uxTextColor;
            scb.ItemID = Item.ItemID;

            scb.StyleId = Item.ItemID.ToString();
            scb.ButtonText = Item.Text;
            scb.FontSize = 14.5F;
            scb.EnableFaceFade();
            scb.EnableIndicatorBlink();
            scb.PV = new ItemValue();
            scb.PV.TagID = -1;
            scb.PV.Value = -1;

            scb.SV = new ItemValue();
            scb.SV.TagID = -1;
            scb.SV.Value = -1;

            DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);
            var ItemValuesBtn = MyDataAccessLayer.ReadItemValues(scb.ItemID);
            int idx = 0;
            foreach (var item in ItemValuesBtn)
            {
                if (idx == 0)
                {
                    scb.SV.TagID = item.TagID;
                    scb.SV.Value = item.Value;
                }
                if (idx == 1)
                {
                    scb.PV.TagID = item.TagID;
                    scb.PV.Value = item.Value;
                }
                idx++;
            }

         
            if (Designing == true)
            {
                scb.IsEnabled = true;
                scb.IsVisible = true;
                scb.EnableTouchEvents = true;
                scb.InputTransparent = false;
                //scb.Start();

            }
            else
            {
                scb.IsEnabled = true;
                scb.IsVisible = true;
                scb.EnableTouchEvents = true;
                scb.InputTransparent = false;
                scb.Start();
                scb.Touch += (sender, args) =>
                {
                    switch (args.ActionType)
                    {
                        case SKTouchAction.Released:
                            if (Item.Action == "TOGGLE")
                            {
                                if (scb.SV.Value > 0.5)
                                {
                                    scb.SV.Value = 0;
                                }
                                else
                                {
                                    scb.SV.Value = 1;
                                }
                                MyDataAccessLayer.UpdateTagValue(scb.SV.TagID, scb.SV.Value, 0);
                                MyDataAccessLayer.StoreTagValue(scb.SV.TagID, scb.SV.Value);

                            }
                            else if (Item.Action == "ON")
                            {
                                scb.SV.Value = 1;
                                MyDataAccessLayer.UpdateTagValue(scb.SV.TagID, scb.SV.Value, 0);
                                MyDataAccessLayer.StoreTagValue(scb.SV.TagID, scb.SV.Value);
                            }
                            else if (Item.Action == "OFF")
                            {
                                scb.SV.Value = 0;
                                MyDataAccessLayer.UpdateTagValue(scb.SV.TagID, scb.SV.Value, 0);
                                MyDataAccessLayer.StoreTagValue(scb.SV.TagID, scb.SV.Value);
                            }
                            else if (Item.Action == "POPUPALARM")
                            {
                                ScadaClasses.Previouspage = -1;
                                ScadaClasses.CurrentScadaPopup = ScadaClasses.uxAlarmGrid;
                            }
                            else if (Item.Action == "POPUPLOGIN")
                            {
                                ScadaClasses.Previouspage = -1;
                                ScadaClasses.CurrentScadaPopup = ScadaClasses.uxLoginMenu;
                            }
                            else if (Item.Action == "POPUPDESIGN")
                            {
                                ScadaClasses.Previouspage = -1;
                                ScadaClasses.CurrentScadaPopup = ScadaClasses.uxDesignMenu;
                            }
                            else
                            {
                                ScadaClasses.Currentpage = Item.Nextpage;
                            }
                            scb.GradientStartColor = Color.uxItemColor;
                            scb.GradientEndColor = Color.uxItemColor;
                            ScadaClasses.Refresh = true;

                            break;

                        case SKTouchAction.Pressed:
                            scb.GradientStartColor = Color.uxTouchColor;
                            scb.GradientEndColor = Color.uxTouchColor;
                            break;

                        case SKTouchAction.Moved:
                            break;

                        case SKTouchAction.Entered:
                            bFaceFade = false;
                            break;

                        case SKTouchAction.Exited:
                            bFaceFade = true;
                            break;
                    }
                    args.Handled = true;
                };


            }
            return(scb);
        }


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


        public static BindableProperty ButtonFaceIntensityProperty = BindableProperty.Create(nameof(ButtonFaceIntensity), typeof(float),
            typeof(ScadaButton), 5f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float ButtonFaceIntensity
        {
            get => (float)GetValue(ButtonFaceIntensityProperty);
            set => SetValue(ButtonFaceIntensityProperty, value);
        }


        public static BindableProperty IndicatorIntensityProperty = BindableProperty.Create(nameof(IndicatorIntensity), typeof(float),
          typeof(ScadaButton), 5f, BindingMode.OneWay,
          validateValue: (_, value) => value != null && (float)value >= 0,
          propertyChanged: OnPropertyChangedInvalidate);

        public float IndicatorIntensity
        {
            get => (float)GetValue(IndicatorIntensityProperty);
            set => SetValue(IndicatorIntensityProperty, value);
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
            {              
                if (control.IsLoaded == true)
                {
                    control.InvalidateSurface();
                }
            }
        }

        private SKCanvas DrawRoundRectWithArrow(SKCanvas c, SKPaint Paint, float x, float y, float w, float h, float radius)
        {
            var Rect = new SKRect(0, 0, w, h);
            var RoundRect = new SKRoundRect(Rect, 10, 10);
            c.DrawRoundRect(RoundRect, Paint);
            return c;
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
                await Task.Delay(30);
            }
        }



        public async void EnableIndicatorBlink()
        {
            const int cStep = 2;
            int iStep = cStep;
            while (bIndicatorFade)
            {
                if ((localBtnIndicatorIntensity + iStep) > 255)
                {
                    iStep = -cStep;
                }
                if (localBtnIndicatorIntensity < 100)
                {
                    iStep = cStep;
                }
                if (IsLoaded == true)
                {
                    InvalidateSurface();
                }
                localBtnIndicatorIntensity = localBtnIndicatorIntensity + iStep;
                await Task.Delay(10);
            }
            localBtnIndicatorIntensity = 255;
        }

        public void Start()
        {
            //RefreshTask();
        }

        public void RefreshValues(List<ItemValue> ItemValues)
        {
            if ((ItemValues != null) && (PV != null))
            {
                foreach (var item in ItemValues)
                {
                    if (item.TagID == PV.TagID)
                    {
                        PV.Value = item.Value;
                        PV.StatusQuality = item.StatusQuality;
                    }
                    if (item.TagID == SV.TagID)
                    {
                        SV.Value = item.Value;
                        SV.StatusQuality = item.StatusQuality;
                    }
                }
                if (IsLoaded == true)
                {
                    InvalidateSurface();
                }
            }
        }

        private async void RefreshTask()
        {
            DataAccessLayer MyDataAccessLayer = new DataAccessLayer(MainPage.ConnectionString);
            while (true)
            {
                await Task.Delay(5000);
                var ItemValues = MyDataAccessLayer.ReadItemValues(ItemID);
                RefreshValues(ItemValues);       
            }
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            const int cIndicator_0 = 0;
            const int cIndicator_1 = 1;
            const int cIndicator_2 = 2;
            const int cIndicator_3 = 3;
            const int cIndicator_4 = 4;
            //const int cIndicator_5 = 5;
            //const int cIndicator_6 = 6;
            //const int cIndicator_7 = 7;
            //const int cIndicator_8 = 8;
            const int cIndicator_9 = 9;
            const int cIndicator_10 = 10;
            const int cIndicator_11 = 11;

            var info = e.Info;
            var canvas = e.Surface.Canvas;
            float w = info.Width;
            float h = info.Height;

            var progressBar = new SKRoundRect(new SKRect(0, 0, w, h), CornerRadius, CornerRadius);

            SKColor GrStart = GradientStartColor;
            SKColor GrEnd = GradientEndColor;

            if (FaceFadeEnabled == true)
            {
                GrStart = GrStart.WithRed((byte)localBtnFaceIntensity);
                GrStart = GrStart.WithGreen((byte)localBtnFaceIntensity);
                GrStart = GrStart.WithBlue((byte)localBtnFaceIntensity);

                GrEnd = GrEnd.WithRed((byte)localBtnFaceIntensity);
                GrEnd = GrEnd.WithGreen((byte)localBtnFaceIntensity);
                GrEnd = GrEnd.WithBlue((byte)localBtnFaceIntensity);
            }

            using (var facePaint = new SKPaint() { IsAntialias = true, FilterQuality = SKFilterQuality.High, BlendMode = SKBlendMode.Overlay })
            {
                var Rectangle = new SKRect(0, 0, w, h);
                    facePaint.Shader = SKShader.CreateLinearGradient(
                    new SKPoint(Rectangle.Left, Rectangle.Top),
                    new SKPoint(Rectangle.Right, Rectangle.Bottom),
                    new[]
                    {
                        GrStart,
                        GrEnd
                    },
                    new float[] { 0, 1 },
                    SKShaderTileMode.Decal);

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
                canvas.Clear();
                if (IndicatorType == cIndicator_0)
                {
                    canvas.DrawRoundRect(progressBar, facePaint);
                    NewTextPaint.MeasureText(ButtonText, ref textBounds);
                    float x = info.Width / 2 - textBounds.MidX;
                    float y = info.Height / 2 - textBounds.MidY;
                    canvas.DrawText(ButtonText, x, y, NewTextPaint);
                }

                if (IndicatorType == cIndicator_1)
                {
                    //var indicatorPaint = new SKPaint { Color = Colors.Blue.ToSKColor(), TextSize = FontSize, FilterQuality = SKFilterQuality.High, IsAntialias = true };
                    //indicatorPaint.Color = IndicatorColor;
                    canvas.DrawRoundRect(progressBar, facePaint);
                    //canvas.DrawRect((w / 10), (float)(h - (h / 3)), (float)(w - (2 * (w / 10))), (float)(h / 5), palPaint);
                    NewTextPaint.MeasureText(ButtonText, ref textBounds);
                    float x = info.Width / 2 - textBounds.MidX;
                    float y = info.Height / 2 - textBounds.MidY;
                    canvas.DrawText(ButtonText, x, y, NewTextPaint);
                }

                if (IndicatorType == cIndicator_2)
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
                        bIndicatorFade = true;
                        circlePaint.Color = new SKColor((byte)(localBtnIndicatorIntensity), 32, 32, 255);
                    }

                    if (ButtonRow.Status == 2)
                    {
                        bIndicatorFade = false;
                        circlePaint.Color = new SKColor(210, 32, 32, 255);
                    }

                    float h1 = info.Height;
                    float radius = (h1 / 3f);
                    var center = new SKPoint(radius + 2.5f, (info.Height / 2) + 0.5f);
                    NewTextPaint.MeasureText(ButtonRow.col6text, ref textBounds);
                    float y = info.Height / 2 - textBounds.MidY;

             
                    canvas.DrawRoundRect(progressBar, facePaint);
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


                if (IndicatorType == cIndicator_3)
                {
                    if (SvgBase64 == "") return;

                    var mySKSvg = new SKSvg();
                    try
                    {
                        string sBase64Svg = SvgBase64;
                        byte[] data = Convert.FromBase64String(sBase64Svg);
                        string decodedString = Encoding.UTF8.GetString(data);
                        var picture = mySKSvg.FromSvg(decodedString);
                       
                        if (picture == null) return;
                        var dimension = new SkiaSharp.SKSizeI
                        (
                             (int)Math.Ceiling(info.Height * 1.0),
                             (int)Math.Ceiling(info.Height * 1.0)
                        );

                        float Offset = info.Height - h + info.Height * Padding;
                        float ScaleX = (info.Height / picture.CullRect.Width) * 0.7F;
                        float ScaleY = (info.Height / picture.CullRect.Height) * 0.7F;
                        var matrix = SKMatrix.CreateScale(ScaleX * (1 - Padding * 2), ScaleY * (1 - Padding * 2));
                        var img = SKImage.FromPicture(picture, dimension, matrix);

                        canvas.DrawRoundRect(progressBar, facePaint);
                        canvas.DrawImage(img, new SKPoint(info.Width / 6, info.Height / 20));

                        NewTextPaint.MeasureText(ButtonText, ref textBounds);
                        float x = info.Width / 2 - textBounds.MidX;
                        //canvas.DrawText(ButtonText, x, yText, NewTextPaint);

                        canvas.DrawText(ButtonText, x, info.Height-(info.Height/10), NewTextPaint);
                    }
                    catch
                    {
                    }
                }

                if (IndicatorType == cIndicator_4)
                {
                }

                if (IndicatorType == 5)
                {   
                    var onPaint = new SKPaint { Color = new SKColor(100, 100, 100, 255), TextSize = FontSize, FilterQuality = SKFilterQuality.High, IsAntialias = true };
                    if (SV.Value > 0.5)
                    {
                        onPaint.Color = new SKColor(3, 156, 35, (byte)localBtnIndicatorIntensity);
                    }
                    else
                    {
                        onPaint.Color = new SKColor(100, 100, 100, 255);
                    }
                    canvas.DrawRoundRect(progressBar, facePaint);
                    canvas.DrawRect((0), (float)(h / 6), (float)(w), (float)(h / 5), onPaint);
                    NewTextPaint.MeasureText(ButtonText, ref textBounds);
                    float x = info.Width / 2 - textBounds.MidX;
                    canvas.DrawText(ButtonText, x, yText, NewTextPaint);
                }

                // Circular button Draw the svg indicator
                if (IndicatorType == cIndicator_9)
                {
                    float h1 = info.Height;
                    float radius = (h1 / 2.5f);
                    var center = new SKPoint(radius + 2.5f, (info.Height / 2) + 0.5f);

                    //Draw Circle
                    var circlePaint = new SKPaint { Color = GradientStartColor, TextSize = FontSize, FilterQuality = SKFilterQuality.High, IsAntialias = true };
                    canvas.DrawCircle(center, radius, circlePaint);

                    if (SvgBase64 == "") return;
              
                    string sBase64Svg = SvgBase64;
                    byte[] data = Convert.FromBase64String(sBase64Svg);
                    string decodedString = Encoding.UTF8.GetString(data);

                    var svg2 = new SKSvg();
                    var picture = svg2.FromSvg(decodedString);
                    if (picture != null)
                    {
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
                        canvas.DrawImage(img, new SKPoint(Offset + 8, Offset + 10));
                        canvas.DrawText(ButtonText, xText + info.Height, yText, NewTextPaint);
                    }
                }

                if (IndicatorType == cIndicator_10)
                {
                    DrawRoundRectWithArrow(canvas, facePaint, 0, 0, w, h, 10);
                }

                if (IndicatorType == cIndicator_11)
                {
                    if (SvgBase64 == "") return;

                    var svg2 = new SKSvg();
                    string sBase64Svg = SvgBase64;
                    byte[] data = Convert.FromBase64String(sBase64Svg);
                    string decodedString = Encoding.UTF8.GetString(data);

            
                    var picture = svg2.FromSvg(decodedString);
                    if (picture != null)
                    {
                        var dimension = new SkiaSharp.SKSizeI
                        (
                             (int)Math.Ceiling(info.Height * 1.0),
                             (int)Math.Ceiling(info.Height * 1.0)
                        );

                        //float Offset = info.Height - h + info.Height * Padding;
                        float ScaleX = (info.Height / picture.CullRect.Width) * 0.6F;
                        float ScaleY = (info.Height / picture.CullRect.Height) * 0.6F;
                        var matrix = SKMatrix.CreateScale(ScaleX * (1 - Padding * 2), ScaleY * (1 - Padding * 2));
                        var img = SKImage.FromPicture(picture, dimension, matrix);

                        canvas.DrawRoundRect(progressBar, facePaint);
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
}