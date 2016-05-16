using System.IO;

namespace AFT.RegoV2.Infrastructure.Extensions
{
    public static class CommonExtensions
    {
        public static byte[] ToByteArray(this Stream stream)
        {
            using (stream)
            {
                using (var memStream = new MemoryStream())
                {
                    stream.CopyTo(memStream);
                    return memStream.ToArray();
                }
            }
        }
    }
}
