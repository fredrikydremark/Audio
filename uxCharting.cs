using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Net;
using System.Text.RegularExpressions;

namespace Scada
{
    public class dValue
    {
        public double v { get; set; }
        public DateTime t { get; set; }
    }

    public class MyValue
    {
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
        public string sValue { get; set; }
        public string sTime { get; set; }
    }

    public class HistGraph : SKCanvasView
    {
        public List<List<ScadaClasses.Value>> ListOfValues = new List<List<ScadaClasses.Value>>() { };

        public float LastY;
        public float LL = -9999, HL = 9999;

        public static BindableProperty ItemIDProperty = BindableProperty.Create(nameof(ItemID), typeof(int),
        typeof(HistGraph), 0, BindingMode.OneWay,
            validateValue: (_, value) => value != null,
        propertyChanged: OnPropertyChangedInvalidate);

        public int ItemID
        {
            get => (int)GetValue(ItemIDProperty);
            set => SetValue(ItemIDProperty, value);
        }

        public static BindableProperty PVProperty = BindableProperty.Create(nameof(PV), typeof(float),
            typeof(HistGraph), 0f, BindingMode.OneWay,
            validateValue: (_, value) => value != null,
            propertyChanged: OnPropertyChangedInvalidate);


        public float PV
        {
            get => (float)GetValue(PVProperty);
            set => SetValue(PVProperty, value);
        }


        public static BindableProperty SVProperty = BindableProperty.Create(nameof(SV), typeof(float),
          typeof(HistGraph), 0f, BindingMode.OneWay,
          validateValue: (_, value) => value != null,
          propertyChanged: OnPropertyChangedInvalidate);

        public float SV
        {
            get => (float)GetValue(SVProperty);
            set => SetValue(SVProperty, value);
        }


        public static BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(float),
            typeof(HistGraph), 5f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float CornerRadius
        {
            get => (float)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static BindableProperty ChartBackgroundColorProperty = BindableProperty.Create(nameof(ChartBackgroundColor), typeof(SKColor),
            typeof(HistGraph), SKColors.White, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor ChartBackgroundColor
        {
            get => (SKColor)GetValue(ChartBackgroundColorProperty);
            set => SetValue(ChartBackgroundColorProperty, value);
        }

        public static BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float),
            typeof(HistGraph), 12f, BindingMode.OneWay,
            validateValue: (_, value) => value != null && (float)value >= 0,
            propertyChanged: OnPropertyChangedInvalidate);

        public float FontSize
        {
            get => (float)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static BindableProperty GradientStartColorProperty = BindableProperty.Create(nameof(GradientStartColor), typeof(SKColor),
            typeof(HistGraph), SKColors.Purple, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientStartColor
        {
            get => (SKColor)GetValue(GradientStartColorProperty);
            set => SetValue(GradientStartColorProperty, value);
        }

        public static BindableProperty GradientEndColorProperty = BindableProperty.Create(nameof(GradientEndColor), typeof(SKColor),
            typeof(HistGraph), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor GradientEndColor
        {
            get => (SKColor)GetValue(GradientEndColorProperty);
            set => SetValue(GradientEndColorProperty, value);
        }

        public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(SKColor),
            typeof(HistGraph), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor TextColor
        {
            get => (SKColor)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public static BindableProperty AlternativeTextColorProperty = BindableProperty.Create(nameof(AlternativeTextColor), typeof(SKColor),
            typeof(HistGraph), SKColors.Blue, BindingMode.OneWay,
            validateValue: (_, value) => value != null, propertyChanged: OnPropertyChangedInvalidate);

        public SKColor AlternativeTextColor
        {
            get => (SKColor)GetValue(AlternativeTextColorProperty);
            set => SetValue(AlternativeTextColorProperty, value);
        }


        private static void OnPropertyChangedInvalidate(BindableObject bindable, object oldvalue, object newvalue)
        {
            var control = (HistGraph)bindable;
            if (control != null)
            {
                if (oldvalue != newvalue)
                    control.InvalidateSurface();
            }
        }




        /*
        procedure THistGraph.ScaleMarks(MemBitMap : TBitmap; TheAxis, XpStart, XpStopp, YpTopLine, YpBaseLine:integer;
                                MaxValue,MinValue : double;OnScreen:Boolean; UnitText: string  );
const
  Left = 1;
        Right=2;
  InnerLeft = 3;
  InnerRight = 4;

Var
  scale       : integer;
  S           : String[15];
  Line        : array[1..2] of TPoint;
  YValue      : double;
  Yp          : integer;
  ScaleMarkLength : integer;

  TextHeightYp,
  TextWidthXp,
  ScaleMarkStartXp,
  ScaleMarkStopXp,
  TabDirection   : integer;

Begin
{ Span  :=   MaxValue-MinValue;
  Scale := (Span div 5 )-((Span div 5 ) mod 5 );}

    ScaleMarkLength := (XpStopp - XpStart ) div 75;
  Scale := 100;
  Case Round(MaxValue-MinValue) Of
     0..14     : Scale := 1;
     15..26    : Scale := 5;
     27..50    : Scale := 10;
     51..100   : Scale := 20;
     101..199  : Scale := 25;
     200..500  : Scale := 50;
     501..2000 : Scale := 200;
     2001..10000 : Scale := 500;
     10001..25000 : Scale := 2000;
  End;



  ScaleMarkStartXp := XpStart;
  ScaleMarkStopXp  := XpStart - ScaleMarkLength;

  TabDirection := -1;
  case TheAxis of

    InnerLeft  : begin
              ScaleMarkStartXp := XpStart;
              ScaleMarkStopXp  := XpStart + ScaleMarkLength;
              TabDirection := 0;
            end;
    InnerRight : begin
              ScaleMarkStartXp := XpStopp;
              ScaleMarkStopXp  := XpStopp - ScaleMarkLength;
              TabDirection := -1;
            end;

  end;


  YValue:=MinValue;
  YValue:=YValue+Scale;
  Yp := Round(YpBaseLine - (YpBaseLine* YValue ) / MaxValue  );

    Repeat
  { Scale marks }
Line[1].X := ScaleMarkStartXp;
Line[1].Y := Yp;
Line[2].X := ScaleMarkStopXp;
Line[2].Y := Yp;

if TheAxis = Left then
      HorizontalHelpLine(MemBitmap, XpStart, XpStopp, Yp);

str(YValue: 5:0, S);

MemBitMap.Canvas.polyLine(Line);
TextWidthXp:= MemBitMap.Canvas.TextWidth(S);
MemBitMap.Canvas.TextOut(ScaleMarkStopXp + (TextWidthXp * TabDirection), Yp - TextHeightYp + 2, S);

YValue:= YValue + Scale;
Yp:= Round(YpBaseLine - (YpBaseLine * YValue) / MaxValue);

Until Yp<YpTopLine;

TextWidthXp:= MemBitMap.Canvas.TextWidth(UnitText);
MemBitMap.Canvas.TextOut(ScaleMarkStopXp + (TextWidthXp * TabDirection), YpTopLine - (TextHeightYp * 2), UnitText);
end;

      */

        /*  Smooth curves
         using SKPaint p1 = new() { IsAntialias = true, Color = SKColors.White, Style = SKPaintStyle.Stroke };

        p1.StrokeWidth = 8;
        p1.StrokeCap = SKStrokeCap.Round;

        using SKPath path = new();
        path.MoveTo(10, 10);
        path.QuadTo(256, 64, 128, 128);
        path.QuadTo(10, 192, 250, 250);

        canvas.DrawPath(path, p1);

         */


        /*
                    if (Values[j - 1].Y != "0")
                               {
                                   var p1 = new SKPoint(x2, h - (float)(Convert.ToDouble(Values[j - 1].Y) / 100.0F) * h);
               var p2 = new SKPoint(x, h - (float)(Convert.ToDouble(Values[j - 2].Y) / 100.0F) * h);
               canvas.DrawLine(p1, p2, Barpaint);
                               }
       */


        public SKColor RGBStringToColor(string RGBColor)
        {
            SKColor color = SKColors.White;
            if (RGBColor != null)
            {
                Regex regex = new Regex(@"rgba\((?<r>\d{1,3}),(?<g>\d{1,3}),(?<b>\d{1,3}),(?<a>\d{1,3})\)");
                Match match = regex.Match(RGBColor);
                if (match.Success)
                {
                    byte r = byte.Parse(match.Groups["r"].Value);
                    byte g = byte.Parse(match.Groups["g"].Value);
                    byte b = byte.Parse(match.Groups["b"].Value);
                    byte a = byte.Parse(match.Groups["a"].Value);
                    color = new SKColor(r, g, b, a);
                }
            }
            return color;
        }



        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;
            var canvas = e.Surface.Canvas;

            float w = e.Info.Width;
            float h = e.Info.Height;
            float MaxValue = 100;
            var backgroundBar = new SKRoundRect(new SKRect(0, 0, w, h), 5, 5);
            var background = new SKPaint { Color = GradientStartColor, IsAntialias = true };

            var pathStroke5 = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = new SKColor(128, 128, 128, 200),
                StrokeWidth = 1
            };
            /*
            SKPaint Line1Paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = new SKColor(158, 45, 128, 230),
                StrokeWidth = 1.8F,
                StrokeCap = SKStrokeCap.Butt
            };

            SKPaint Line2Paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = new SKColor(35, 35, 35, 230),
                StrokeWidth = 0.5F,
                StrokeCap = SKStrokeCap.Butt
            };

            SKPaint Line3Paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = new SKColor(35, 35, 180, 230),
                StrokeWidth = 0F,
                StrokeCap = SKStrokeCap.Square
            };
            */

            FontSize = 12F;
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
            FontSize = 12F;
     
            int incColor = 1;
            canvas.Clear();
            canvas.DrawRoundRect(backgroundBar, background);

            foreach (var LocalValues in ListOfValues)
            {
                if (LocalValues.Count > 2)
                {
                    SKColor theColor = RGBStringToColor(LocalValues[1].Color);
                    SKPaint myPaint = new SKPaint
                    {
                        IsAntialias = true,
                        Style = SKPaintStyle.Fill,
                        Color = theColor,
                        SubpixelText = true,
                        StrokeWidth = 2.5F,
                        FilterQuality= SKFilterQuality.High,
                        StrokeCap = SKStrokeCap.Square
                    };

                    SKPaint my2Paint = new SKPaint
                    {
                        IsAntialias = true,
                        Style = SKPaintStyle.Fill,
                        Color = theColor,
                        SubpixelText = true,
                        StrokeWidth = w/50,
                        FilterQuality = SKFilterQuality.High,
                        StrokeCap = SKStrokeCap.Round
                    };
                    int Count = LocalValues.Count;
                    string sv = "0";
                    for (int m = 0; m < Count; m++)
                    {
                        if (LocalValues[m].Y != "")
                        {
                            if (Convert.ToDouble(LocalValues[m].Y) > 0.5F)
                            {
                                if (LocalValues[m].TypeOfTag == 1)
                                {
                                    sv = "1,0";
                                }
                                else
                                {
                                    sv = LocalValues[m].Y;
                                }
                            }
                            else
                            {
                                sv = "0,0";
                            }
                        }
                        LocalValues[m].Y = sv;
                    }

                    int j = Count - 1;
                    float x = w;
                    var thePoint = new SKPoint(-1, -1);
                    var oldPoint = new SKPoint(-1, -1);

                    var pts = new List<SKPoint>();
                    int AddPoint = 0;
                    while (x >= 0)
                    {
                        x = x - (w / Count);
                        if (j >= 0)
                        {
                            if (LocalValues[j].Y != "0")
                            {
                                thePoint = new SKPoint(x, h - (float)(Convert.ToDouble(LocalValues[j].Y) / 100.0F) * h);
                                if (oldPoint.X > -1)
                                {
                                    if (LocalValues[0].TypeOfTag == 0)
                                    {
                                        //canvas.DrawRect(x - (w / Count) / 2, h - h / 6, (w / Count) * 4F, 5 * -(float)(Convert.ToDouble(LocalValues[j].Y) / 100.0F) * h, Line3Paint);
                                    }
                                    if (LocalValues[0].TypeOfTag == 1)
                                    {
                                        canvas.DrawRect(x - (w / Count) / 2, h - h / 6, (w / Count) * 4F, 5 * -(float)(Convert.ToDouble(LocalValues[j].Y) / 100.0F) * h, myPaint);
                                        //canvas.DrawLine(oldPoint, thePoint, LinePaint);                                         
                                        /*
                                        if (Math.Abs(thePoint.Y - oldPoint.Y) > 0.05F)
                                        {
                                            pts.Add(new SKPoint(thePoint.X, thePoint.Y));
                                        }*/
                                    }
                                    if (LocalValues[0].TypeOfTag == 2)
                                    {
                                        canvas.DrawRect(thePoint.X, thePoint.Y, (w / Count) / 1.5F, thePoint.Y + h, myPaint);
                                    }
                                    if (LocalValues[0].TypeOfTag == 3)
                                    {
                                        /*
                                        if (Math.Abs(thePoint.Y - oldPoint.Y) > 0.05F)
                                        {
                                            canvas.DrawLine(oldPoint, thePoint, myPaint);
                                        }
                                        */
                                        canvas.DrawLine(oldPoint, thePoint, myPaint);
                                     
                                        
                                        /*
                                        if (pts.Count == 0)
                                        {
                                            pts.Add(new SKPoint(thePoint.X, thePoint.Y));
                                        }
                                        if (AddPoint < 3)
                                        {
                                            pts.Add(new SKPoint(thePoint.X, thePoint.Y));
                                        }
                                        if (Math.Abs(thePoint.Y - oldPoint.Y) > 0.05F)
                                        {
                                            AddPoint = 0;
                                        }
                                        else
                                        {
                                            AddPoint++;
                                        }
                                        */


                                        /*
                                        var R = new SKRect(thePoint.X, thePoint.Y, thePoint.X + ((w / Count) / 1.5F), thePoint.Y + h);
                                        var RR = new SKRoundRect(R, 2F, 2F);
                                        canvas.DrawRoundRect(RR, LinePaint);
                                        */
                                    }
                                    if (LocalValues[0].TypeOfTag == 4)
                                    {
                                        //canvas.DrawRect(thePoint.X, thePoint.Y, 1, thePoint.Y + h, my2Paint);
                                        if (Math.Abs(thePoint.Y - oldPoint.Y) > 0.05F)
                                        {
                                            canvas.DrawLine(thePoint.X, thePoint.Y, thePoint.X, thePoint.Y + h, my2Paint);
                                        }
                                    }

                                }
                                oldPoint = thePoint;
                            }

                          

                            //X Scale
                            if (incColor == 1)
                            {
                                var basetick = new SKPoint(x, h);
                                var shorttick = new SKPoint(x, h - h / 50);
                                var longtick = new SKPoint(x, h - h / 30);
                                var textBounds = new SKRect();
                                if (LocalValues[j].X != "")
                                {
                                    canvas.DrawLine(basetick, longtick, TextPaint);
                                    float tw = TextPaint.MeasureText(LocalValues[j].X, ref textBounds);
                                    var pText = new SKPoint(x - tw / 2 - 1, h - h / 25);
                                    canvas.DrawText(LocalValues[j].X, pText, TextPaint);
                                }
                                else
                                {
                                    //canvas.DrawLine(basetick, shorttick, TextPaint);
                                }
                            }
                        }
                        j--;
                    }

                    if (LocalValues[0].TypeOfTag == 3)
                    {
                        //Some tests with Draw as polygon
                        if (pts.Count > 0)
                        {
                            //pts.Add(new SKPoint(oldPoint.X, oldPoint.Y));
                            //float latestY = pts[0].Y;
                            //pts.Insert(0,new SKPoint(x+w-1, latestY));
                            //canvas.DrawPoints(SKPointMode.Polygon, pts.ToArray(), myPaint);
                        }
                    }
                    incColor++;
                }
            }


            //Y Scale        
            var pathY = new SKPath { FillType = SKPathFillType.EvenOdd };

            float Scale = 0F;
            while (Scale < MaxValue)
            {
                if (Scale % 20 == 0)
                {
                    pathY.MoveTo(0, h - (Scale / 100.0F) * h);
                    pathY.LineTo(w / 100, h - (Scale / 100.0F) * h);
                    if (Scale > 1)
                    {
                        canvas.DrawText(Scale.ToString(), w / 80, h - (Scale / 100.0F) * h, TextPaint);
                    }
                }
                else
                {
                    pathY.MoveTo(0, h - (Scale / 100.0F) * h);
                    pathY.LineTo(w / 150, h - (Scale / 100.0F) * h);
                }
                Scale = Scale + 5.0F;
            }
            canvas.DrawPath(pathY, TextPaint);

        }
    }
}