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
    static int Width;
    static int Height;
    static byte[]? Pixels;
    static int FieldCount;
    static Field[]? Fields;

    static void Main(string[] args)
    {
        var Fps = 60;
        var Duration = 15;
        var FrameCount = Duration * Fps;

        var Res = 80;

        Width = 16 * Res;
        Height = 9 * Res;
        Pixels = new byte[Width * Height * 3];

        FieldCount = 400;
        Fields = new Field[FieldCount];

        var random = new Random();
        for (int i = 1; i < 201; i++)
        {
            Fields[2 * i - 1].Radius = i * Res / 48;
            Fields[2 * i - 1].Luminance = i * 5;
            Fields[2 * i - 1].PosX = Width / 2;
            Fields[2 * i - 1].PosY = Height / 2;
            Fields[2 * i - 1].SpeedX = Res * (random.NextDouble() - 0.5) / i;
            Fields[2 * i - 1].SpeedY = Res * (random.NextDouble() - 0.5) / i;

            Fields[2 * i - 2].Radius = i * Res / 48;
            Fields[2 * i - 2].Luminance = -i * 5;
            Fields[2 * i - 2].PosX = Width / 2;
            Fields[2 * i - 2].PosY = Height / 2;
            Fields[2 * i - 2].SpeedX = Res * (random.NextDouble() - 0.5) / i;
            Fields[2 * i - 2].SpeedY = Res * (random.NextDouble() - 0.5) / i;
        }

        using (var Writer = new BinaryWriter(File.Create("video.raw")))
        {
            for (int f = 0; f < FrameCount; f++)
            {
                if (f % 100 == 0) Console.WriteLine(f.ToString("D4") + " frame başladı");

                DoLine();

                Writer.Write(Pixels);
                Writer.Flush();

                Update();

                if (f % 100 == 0) Console.WriteLine(f.ToString("D4") + " frame bitti");
            }
        }

        System.Diagnostics.Process.Start("ffmpeg",
        "-y -f rawvideo -pix_fmt rgb24 -s:v 1280x720 -r 60 -i video.raw video720p.mp4");
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
}
