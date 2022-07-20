namespace son;
class common
{
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
