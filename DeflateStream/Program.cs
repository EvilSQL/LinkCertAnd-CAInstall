using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace AppDeflateStream
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileInputName = @"CryptoPro.Sharpei.ServiceModel.dll";
            var assembly = File.ReadAllBytes(fileInputName);

            var fileOutputName = @"CryptoPro.Sharpei.ServiceModel.dll.deflated";
            using (var file = File.Open(fileOutputName, FileMode.Create))
            using (var stream = new DeflateStream(file, CompressionMode.Compress))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(assembly);
            }
        }
    }
}
