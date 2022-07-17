namespace son;
class Program
{
    struct Daire
    {
        public double a;
        public double x;
        public double y;
        public double r;
        public double vx;
        public double vy;
    }
    static void Main(string[] args)
    {
        var fps = 60;
        var saniye = 1;
        var frameSayisi = saniye * fps;

        var w = 1280;
        var h = 720;

        var pixels = new byte[w * h * 3];

        var ds = 1;//daire sayısı
        var d = new Daire[ds];

        var r = new Random();
        for (int i = 0; i < ds; i++)
        {
            d[i].r = r.Next(h / 9, h / 3);
            d[i].a = r.Next(1, 999);
            d[i].x = w / 2;
            d[i].y = h / 2;
            d[i].vx = 99.9 * (r.NextDouble() - 0.5) / d[i].r;
            d[i].vy = 99.9 * (r.NextDouble() - 0.5) / d[i].r;
        }

        var isikMiktari = 0.0;
        for (int i = 0; i < ds; i++)
            isikMiktari += d[i].a * d[i].r * d[i].r * Math.PI;

        isikMiktari /= w * h;

        using (var Writer = new BinaryWriter(File.Create("video.raw")))
        {
            for (int f = 0; f < frameSayisi; f++)
            {
                if (f % 100 == 0)
                    Console.WriteLine(f.ToString("D4") + " frame başladı");

                Parallel.For(0, h, j =>
                {
                    var t = j * w * 3;
                    for (var i = 0; i < w; i++)
                    {
                        var p = 0;
                        for (int z = 0; z < ds; z++)
                        {
                            var u = Math.Sqrt((i - d[z].x) * (i - d[z].x) + (j - d[z].y) * (j - d[z].y));
                            if (u < d[z].r)
                                p += (int)(d[z].a * (1.0 - u / d[z].r));
                        }

                        var rgb = PsuedoGreyPlus(p);

                        pixels[t++] = rgb[0];
                        pixels[t++] = rgb[1];
                        pixels[t++] = rgb[2];

                    }
                });

                Writer.Write(pixels);
                Writer.Flush();

                for (int i = 0; i < ds; i++)
                {
                    if (d[i].x + d[i].r + d[i].vx > w) d[i].vx *= -1;
                    if (d[i].x - d[i].r + d[i].vx < 0) d[i].vx *= -1;
                    if (d[i].y + d[i].r + d[i].vy > h) d[i].vy *= -1;
                    if (d[i].y - d[i].r + d[i].vy < 0) d[i].vy *= -1;

                    d[i].x += d[i].vx;
                    d[i].y += d[i].vy;
                }

                if (f % 100 == 0)
                    Console.WriteLine(f.ToString("D4") + " frame bitti");
            }
        }
    }
    public static byte[] PsuedoGreyPlus(int x)
    {
        if (x < 0) return new byte[3] { 0, 0, 0 };
        if (x > 4079) return new byte[3] { 255, 255, 255 };

        var i = x / 16;
        var j = x % 16;

        var k = new byte[16, 3] { { 0, 0, 0 }, { 0, 0, 1 }, { 0, 0, 2 }, { 1, 0, 0 }, { 1, 0, 1 }, { 1, 0, 1 }, { 1, 0, 2 }, { 2, 0, 0 }, { 2, 0, 1 }, { 2, 0, 2 }, { 2, 0, 2 }, { 0, 1, 0 }, { 0, 1, 1 }, { 0, 1, 2 }, { 0, 1, 2 }, { 1, 1, 0 } };
        var r = Math.Min(i + k[j, 0], 255);
        var g = Math.Min(i + k[j, 1], 255);
        var b = Math.Min(i + k[j, 2], 255);

        return new byte[3] { (byte)r, (byte)g, (byte)b };
    }
}
