using System;
using System.IO;
using System.Threading.Tasks;

namespace DataCompressor.Program
{
    public static class Program
    {
        static void Help()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("\t-h|help");
            Console.WriteLine("\t\tDisplay this help message");
            Console.WriteLine();

            Console.WriteLine("\t-c dirName [outputFileName]");
            Console.WriteLine("\tcompress dirName [outputFileName]");
            Console.WriteLine("\t\tCompress directory (dirName) by default into the");
            Console.WriteLine("\t\tfile named after directory to be compressed into the same");
            Console.WriteLine("\t\tdirectory. Optionally into defined file (outputFileName).");
            Console.WriteLine();

            Console.WriteLine("\t-d compressedName [outputDirName]");
            Console.WriteLine("\tdecompress compressedName [outputDirName]");
            Console.WriteLine("\t\tDecompress file (compressedName) by default into the");
            Console.WriteLine("\t\tsame directory as compressed file.");
            Console.WriteLine("\t\tOptionally into defined directory (outputDirName)");
            Console.WriteLine();
        }


        static int Main(string[] args)
        {
            if (args.Length < 2 || args[0] == "-h" || args[0] == "help")
            {
                Help();
                return 0;
            }

            if (args[0] == "-c" || args[0] == "compress")
            {
                if (args.Length > 2)
                {
                    return Compress(args[1], args[2]).GetAwaiter().GetResult();
                }

                return Compress(args[1]).GetAwaiter().GetResult();
            }

            if (args[0] == "-d" || args[0] == "decompress")
            {
                if (args.Length > 2)
                {
                    return Decompress(args[1], args[2]).GetAwaiter().GetResult();
                }

                return Decompress(args[1]).GetAwaiter().GetResult();
            }

            Help();
            return 1;
        }


        static async Task<int> Compress(string dirName, string? outputFileName = null)
        {
            if (!Directory.Exists(dirName))
            {
                Console.WriteLine("1. argument - Invalid directory or it does not exist.");
                return 1;
            }

            if (outputFileName != null && !File.Exists(outputFileName))
            {
                Console.WriteLine("2. argument - Invalid file or it does not exist.");
                return 1;
            }

            try
            {
                await new Creator(dirName, outputFileName).Compress();
                return 0;
            }
            catch
            {
                return 1;
            }
        }

        static async Task<int> Decompress(string compressedName,
            string? outpudDirName = null)
        {
            if (!File.Exists(compressedName))
            {
                Console.WriteLine("1. argument - Invalid file or it does not exist.");
                return 1;
            }

            if (outpudDirName != null && !Directory.Exists(outpudDirName))
            {
                Console.WriteLine("2. argument - Invalid directory or it does not exist.");
                return 1;
            }

            try
            {
                Reconstructor reconstructor = new(compressedName, outpudDirName);
                await reconstructor.Decompress();
                return 0;
            }
            catch
            {
                return 1;
            }
        }
    }
}