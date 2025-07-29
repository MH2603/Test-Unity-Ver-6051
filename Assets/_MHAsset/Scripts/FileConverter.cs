using System;
using System.IO;
using DUCK.Crypto;

namespace MH
{
    public class FileConverter
    {
        private readonly string _inputPath;
        private readonly string _outputPath;

        public FileConverter(string inputPath, string outputPath)
        {
            _inputPath = inputPath;
            _outputPath = outputPath;
        }

        public byte[] ConvertGlbToByteArray()
        {
            try
            {
                return File.ReadAllBytes(_inputPath);
            }
            catch (Exception ex)
            {
                throw new IOException($"Error reading .glb file: {ex.Message}");
            }
        }

        public void ConvertByteArrayToDoj(byte[] data)
        {
            try
            {
                File.WriteAllBytes(_outputPath, data);
            }
            catch (Exception ex)
            {
                throw new IOException($"Error writing .doj file: {ex.Message}");
            }
        }

        public void ConvertGlbToDoj()
        {
            try
            {
                byte[] data = ConvertGlbToByteArray();
                ConvertByteArrayToDoj(data);
            }
            catch (Exception ex)
            {
                throw new IOException($"Error during conversion: {ex.Message}");
            }
        }
        
        public void ConvertGlbToDoj_EnCode(string password)
        {
            try
            {
                byte[] data = ConvertGlbToByteArray();
                if (string.IsNullOrEmpty(password))
                {
                    ConvertByteArrayToDoj(data);
                    return;
                }
                
                byte[] enCodedData = SimpleAESEncryption.Encode(data, password);
                ConvertByteArrayToDoj(enCodedData);
            }
            catch (Exception ex)
            {
                throw new IOException($"Error during conversion: {ex.Message}");
            }
        }
    }
}