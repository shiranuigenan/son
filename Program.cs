using System.Drawing;
namespace son;
struct Field
{
    public double Luminance;
    public double PosX;
    public double PosY;
    public double Radius;
    public double SpeedX;
    public double SpeedY;
}

class Program
{
    static int ResolutionFactor;
    static int Width;
    static int Height;
    static byte[]? Pixels;
    static int FieldCount;
    static Field[]? Fields;

    static void Main(string[] args)
    {
        var Fps = 60;
        var Duration = 10;
        var FrameCount = Duration * Fps;

        ResolutionFactor = 16;
        Width = 16 * ResolutionFactor;
        Height = 9 * ResolutionFactor;
        Pixels = new byte[Width * Height * 3];

        FieldCount = 400;
        Fields = new Field[FieldCount];

        var random = new Random();
        for (int i = 1; i < 201; i++)
        {
            Fields[2 * i - 1].Radius = i * ResolutionFactor / 48;
            Fields[2 * i - 1].Luminance = i * 5;
            Fields[2 * i - 1].PosX = Width / 2;
            Fields[2 * i - 1].PosY = Height / 2;
            Fields[2 * i - 1].SpeedX = ResolutionFactor * (random.NextDouble() - 0.5) / i;
            Fields[2 * i - 1].SpeedY = ResolutionFactor * (random.NextDouble() - 0.5) / i;

            Fields[2 * i - 2].Radius = i * ResolutionFactor / 48;
            Fields[2 * i - 2].Luminance = -i * 5;
            Fields[2 * i - 2].PosX = Width / 2;
            Fields[2 * i - 2].PosY = Height / 2;
            Fields[2 * i - 2].SpeedX = ResolutionFactor * (random.NextDouble() - 0.5) / i;
            Fields[2 * i - 2].SpeedY = ResolutionFactor * (random.NextDouble() - 0.5) / i;
        }

        using (var Writer = new BinaryWriter(File.Create("video.raw")))
        {
            for (int f = 0; f < FrameCount; f++)
            {
                if (f % 100 == 0) Console.WriteLine(f.ToString("D4") + " frame başladı");

                Do16X9Optimized();

                Writer.Write(Pixels);
                Writer.Flush();

                Update();

                if (f % 100 == 0) Console.WriteLine(f.ToString("D4") + " frame bitti");
            }
        }

        System.Diagnostics.Process.Start("ffmpeg",
        "-y -f rawvideo -pix_fmt rgb24 -s:v 256x144 -r 60 -i video.raw v144.mp4");
    }
    static void Update()
    {
        for (int i = 0; i < FieldCount; i++)
        {
            if (Fields[i].PosX + Fields[i].Radius + Fields[i].SpeedX > Width) Fields[i].SpeedX *= -1;
            if (Fields[i].PosX - Fields[i].Radius + Fields[i].SpeedX < 0) Fields[i].SpeedX *= -1;
            if (Fields[i].PosY + Fields[i].Radius + Fields[i].SpeedY > Height) Fields[i].SpeedY *= -1;
            if (Fields[i].PosY - Fields[i].Radius + Fields[i].SpeedY < 0) Fields[i].SpeedY *= -1;

            Fields[i].PosX += Fields[i].SpeedX;
            Fields[i].PosY += Fields[i].SpeedY;
        }
    }
    static void DoLine()
    {
        Parallel.For(0, Height, j =>
        {
            var startIndex = j * Width * 3;
            for (var i = 0; i < Width; i++)
            {
                var p = 2040;
                for (int z = 0; z < FieldCount; z++)
                {
                    var distance = Math.Sqrt((i - Fields[z].PosX) * (i - Fields[z].PosX) + (j - Fields[z].PosY) * (j - Fields[z].PosY));
                    if (distance < Fields[z].Radius)
                        p += (int)(Fields[z].Luminance * (1.0 - distance / Fields[z].Radius));
                }

                var rgb = common.PsuedoGreyPlus(p);

                Pixels[startIndex++] = rgb[0];
                Pixels[startIndex++] = rgb[1];
                Pixels[startIndex++] = rgb[2];
            }
        });
    }
    static void Do16X9()
    {
        Parallel.For(0, 9, j => Parallel.For(0, 16, i =>
        {
            for (int jj = j * ResolutionFactor; jj < (j + 1) * ResolutionFactor; jj++)
            {
                var startIndex = (jj * Width + i * ResolutionFactor) * 3;
                for (int ii = i * ResolutionFactor; ii < (i + 1) * ResolutionFactor; ii++)
                {
                    var p = 2040;
                    for (int z = 0; z < FieldCount; z++)
                    {
                        var distance = Math.Sqrt((ii - Fields[z].PosX) * (ii - Fields[z].PosX) + (jj - Fields[z].PosY) * (jj - Fields[z].PosY));
                        if (distance < Fields[z].Radius)
                            p += (int)(Fields[z].Luminance * (1.0 - distance / Fields[z].Radius));
                    }

                    var rgb = common.PsuedoGreyPlus(p);

                    Pixels[startIndex++] = rgb[0];
                    Pixels[startIndex++] = rgb[1];
                    Pixels[startIndex++] = rgb[2];
                }
            }
        }));
    }
    static void Do16X9Optimized()
    {
        Parallel.For(0, 9, j => Parallel.For(0, 16, i =>
        {
            var fij = Fields.Where(f => IsIntersect2(f, i, j)).ToArray();
            //var fij = Fields.Where(f => IsIntersect(f, new Rectangle(ResolutionFactor * i, ResolutionFactor * j, ResolutionFactor, ResolutionFactor))).ToArray();
            // Rectangle ctor'u düzeltilmeli?!
            for (int jj = j * ResolutionFactor; jj < (j + 1) * ResolutionFactor; jj++)
            {
                var startIndex = (jj * Width + i * ResolutionFactor) * 3;
                for (int ii = i * ResolutionFactor; ii < (i + 1) * ResolutionFactor; ii++)
                {
                    var p = 2040;
                    for (int z = 0; z < fij.Length; z++)
                    {
                        var distance = Math.Sqrt((ii - fij[z].PosX) * (ii - fij[z].PosX) + (jj - fij[z].PosY) * (jj - fij[z].PosY));
                        if (distance < fij[z].Radius)
                            p += (int)(fij[z].Luminance * (1.0 - distance / fij[z].Radius));
                    }

                    var rgb = common.PsuedoGreyPlus(p);

                    Pixels[startIndex++] = rgb[0];
                    Pixels[startIndex++] = rgb[1];
                    Pixels[startIndex++] = rgb[2];
                }
            }
        }));
        static bool IsIntersect(Field f, Rectangle rect)
        {

            var circleDistanceX = Math.Abs((int)f.PosX - ((rect.X + (rect.X + rect.Width)) / 2)); // field-kare merkez farkı
            var circleDistanceY = Math.Abs((int)f.PosY - ((rect.Y + (rect.Y + rect.Height)) / 2)); // field-kare merkez farkı

            if (circleDistanceX > (rect.Width / 2 + f.Radius)) { return false; } // fark, genişlik / 2 + yarıçaptan büyükse uzak
            if (circleDistanceY > (rect.Height / 2 + f.Radius)) { return false; } // fark, yükseklik / 2 + yarıçaptan büyükse uzak

            if (circleDistanceX <= (rect.Width / 2)) { return true; } // fark, genişlik / 2 den küçükse kesişim var
            if (circleDistanceY <= (rect.Height / 2)) { return true; } // fark, yükselik / 2 den küçükse kesişim var

            var cornerDistance_sq = Math.Sqrt((circleDistanceX - rect.Width / 2) ^ 2 +
                                 (circleDistanceY - rect.Height / 2) ^ 2);

            // merkez farklarının karenin köşesine uzaklık farklarının kareleri toplamının kökü

            return (cornerDistance_sq <= (f.Radius)); // bu fark r den küçükse kesişim var.
        }
        static bool IsIntersect2(Field f, int i, int j)
        {
            var x = i * ResolutionFactor + ResolutionFactor / 2;
            var y = j * ResolutionFactor + ResolutionFactor / 2;

            var dist2 = Math.Sqrt((x - f.PosX) * (x - f.PosX) + (y - f.PosY) * (y - f.PosY));

            if (dist2 < f.Radius + ResolutionFactor * Math.Sqrt(2))
                return true;

            return false;
        }
    }
}
