using System.Text;

namespace AshproSAASERP.ZatcaEngine.Qr;

public class TlvBuilder
{
    private readonly List<byte> _buffer = new();

    public void AddTag(int tagNumber, string value)
    {
        var valueBytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
        AddTag(tagNumber, valueBytes);
    }

    public void AddTag(int tagNumber, byte[] valueBytes)
    {
        _buffer.Add((byte)tagNumber);

        int length = valueBytes.Length;
        if (length <= 127)
        {
            _buffer.Add((byte)length);
        }
        else
        {
            List<byte> lenBytes = new();
            int temp = length;
            while (temp > 0)
            {
                lenBytes.Insert(0, (byte)(temp & 0xFF));
                temp >>= 8;
            }
            _buffer.Add((byte)(0x80 | lenBytes.Count));
            _buffer.AddRange(lenBytes);
        }

        _buffer.AddRange(valueBytes);
    }

    public byte[] Build()
    {
        return _buffer.ToArray();
    }

    public string BuildBase64()
    {
        return Convert.ToBase64String(Build());
    }
}
