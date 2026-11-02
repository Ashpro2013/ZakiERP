namespace ZakiSAASERP.Web.Services;

public static class VectorHelper
{
    // Convert float[] → byte[] (for DB storage)
    public static byte[] ToBytes(float[] vector)
    {
        if (vector == null || vector.Length == 0)
            return Array.Empty<byte>();

        var bytes = new byte[vector.Length * sizeof(float)];
        Buffer.BlockCopy(vector, 0, bytes, 0, bytes.Length);

        return bytes;
    }

    // Convert byte[] → float[]
    public static float[] FromBytes(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return Array.Empty<float>();

        var vector = new float[bytes.Length / sizeof(float)];
        Buffer.BlockCopy(bytes, 0, vector, 0, bytes.Length);

        return vector;
    }
    public static double Cosine(float[] v1, float[] v2)
    {
        double dot = 0, mag1 = 0, mag2 = 0;

        for (int i = 0; i < v1.Length; i++)
        {
            dot += v1[i] * v2[i];
            mag1 += v1[i] * v1[i];
            mag2 += v2[i] * v2[i];
        }

        return dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2));
    }

}

