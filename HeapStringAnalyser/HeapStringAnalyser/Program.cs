using Microsoft.Diagnostics.Runtime;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace HeapStringAnalyser
{
    // See https://github.com/Microsoft/clrmd/blob/master/Documentation/GettingStarted.md
    // and https://github.com/Microsoft/clrmd/blob/master/Documentation/ClrRuntime.md
    // and https://github.com/Microsoft/clrmd/blob/master/Documentation/WalkingTheHeap.md
    // A useful list of instructions for working with CLRMD, 
    // see http://blogs.msdn.com/b/kirillosenkov/archive/2014/07/05/get-most-duplicated-strings-from-a-heap-dump-using-clrmd.aspx
    class Program
    {
        static void Main(string[] args)
        {
            var crashDump = @"C:\Share\Ninject - MVC App.DMP";
            using (DataTarget target = DataTarget.LoadCrashDump(crashDump))
            {
                string dacLocation = null;
                foreach (ClrInfo version in target.ClrVersions)
                {
                    Console.WriteLine("Found CLR Version: " + version.Version.ToString());
                    dacLocation = LoadCorrectDacForMemoryDump(version);
                }

                var runtimeInfo = target.ClrVersions[0]; // just using the first runtime
                ClrRuntime runtime = null;
                try
                {
                    if (string.IsNullOrEmpty(dacLocation))
                        runtime = runtimeInfo.CreateRuntime();
                    else
                        runtime = runtimeInfo.CreateRuntime(dacLocation);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Console.WriteLine("\nEnsure that this program is compliled for the same architecture as the memory dump (i.e. 32-bit or 64-bit)");
                }

                var heap = runtime.GetHeap();

                PrintMemoryRegionInfo(runtime);
                PrintGCHeapInfo(heap);

                if (!heap.CanWalkHeap)
                {
                    Console.WriteLine("Cannot walk the heap!");
                    return;
                }

                ulong totalStringObjectSize = 0, stringObjectCounter = 0, byteArraySize = 0;
                ulong asciiStringSize = 0, unicodeStringSize = 0, isoStringSize = 0;
                ulong compressedStringSize = 0, uncompressedStringSize = 0;
                
                foreach (var obj in heap.EnumerateObjectAddresses())
                {
                    ClrType type = heap.GetObjectType(obj);
                    // If heap corruption, continue past this object. Or if it's NOT a String we also ignore it
                    if (type == null || type.IsString == false)
                        continue;

                    stringObjectCounter++;
                    var text = (string)type.GetValue(obj);
                    var rawBytes = Encoding.Unicode.GetBytes(text);
                    totalStringObjectSize += type.GetSize(obj);
                    byteArraySize += (ulong)rawBytes.Length;

                    byte[] textAsBytes = null;
                    if (IsASCII(text, out textAsBytes))
                    {
                        asciiStringSize += (ulong)rawBytes.Length;
                        // ASCII is compressed as ISO-8859-1 (Latin-1) NOT ASCII
                        if (IsIsoLatin1(text, out textAsBytes))
                            compressedStringSize += (ulong)textAsBytes.Length;
                        else
                            Console.WriteLine("ERROR: \"{0}\" is ASCII but can't be encoded as ISO-8859-1 (Latin-1)", text);
                    }
                    else if (IsIsoLatin1(text, out textAsBytes))
                    {
                        isoStringSize += (ulong)rawBytes.Length;
                        compressedStringSize += (ulong)textAsBytes.Length;
                    }
                    else
                    {
                        unicodeStringSize += (ulong)rawBytes.Length;
                        uncompressedStringSize += (ulong)rawBytes.Length;
                    }
                }

                Console.WriteLine("Overall {0:N0} \"System.String\" objects take up {1:N0} bytes", stringObjectCounter, totalStringObjectSize);
                Console.WriteLine("Of this underlying byte arrays (as Unicode) take up {0:N0} bytes", byteArraySize);
                Console.WriteLine("Remaining data (object headers, other fields, etc) is {0:N0} bytes ({1:N} bytes per object)\n",
                                    totalStringObjectSize - byteArraySize, (totalStringObjectSize - byteArraySize) / (double)stringObjectCounter);

                Console.WriteLine("Actual Encoding that the \"System.String\" could be stored as (with corresponding data size)");
                Console.WriteLine("  {0,15:N0} bytes are ASCII", asciiStringSize);
                Console.WriteLine("  {0,15:N0} bytes are ISO-8859-1 (Latin-1)", isoStringSize);
                Console.WriteLine("  {0,15:N0} bytes are Unicode", unicodeStringSize);
                Console.WriteLine("Total: {0:N0} bytes (expected: {1:N0})\n", asciiStringSize + isoStringSize + unicodeStringSize, byteArraySize);

                Console.WriteLine("Compression Summary:");
                Console.WriteLine("  {0,15:N0} bytes Compressed (from Unicode -> ISO-8859-1 (Latin-1))", compressedStringSize);
                Console.WriteLine("  {0,15:N0} bytes Uncompressed (as Unicode)", uncompressedStringSize);
                Console.WriteLine("Total: {0:N0} bytes, compared to {1:N0} before compression\n", compressedStringSize + uncompressedStringSize, byteArraySize);
            }
        }

        // By default the encoder just replaces the invalid characters, so force it to throw an exception
        private static Encoding asciiEncoder = Encoding.GetEncoding(Encoding.ASCII.EncodingName, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
        private static Encoding isoLatin1Encoder = Encoding.GetEncoding("ISO-8859-1", EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);

        private static bool IsASCII(string text, out byte[] textAsBytes)
        {
            var unicodeBytes = Encoding.Unicode.GetBytes(text);
            try
            {
                textAsBytes = Encoding.Convert(Encoding.Unicode, asciiEncoder, unicodeBytes);
                return true;
            }
            catch (EncoderFallbackException /*efEx*/)
            {
                textAsBytes = null;
                return false;
            }
        }

        private static bool IsIsoLatin1(string text, out byte[] textAsBytes)
        {
            var unicodeBytes = Encoding.Unicode.GetBytes(text);
            try
            {
                textAsBytes = Encoding.Convert(Encoding.Unicode, isoLatin1Encoder, unicodeBytes);
                return true;
            }
            catch (EncoderFallbackException /*efEx*/)
            {
                textAsBytes = null;
                return false;
            }
        }

        private static string LoadCorrectDacForMemoryDump(ClrInfo version)
        {
            // This is the data needed to request the dac from the symbol server:
            ModuleInfo dacInfo = version.DacInfo;
            // Location: <TEMP>\symbols\mscordacwks_amd64_amd64_4.0.30319.18444.dll\52717f9a96b000\mscordacwks_amd64_amd64_4.0.30319.18444.dll
            var dacLocation = string.Format(@"C:\Users\warma11\AppData\Local\Temp\symbols\{0}\{1:x}{2:x}\{3}",
                                            dacInfo.FileName, dacInfo.TimeStamp, dacInfo.FileSize, dacInfo.FileName);

            if (File.Exists(dacLocation))
            {
                Console.WriteLine("\nDac {0} already exists locally at:\n{1}", dacInfo.FileName, dacLocation);
                return dacLocation;
            }
            else
            {
                Console.WriteLine("\nUnable to find local copy of the dac, it will now be downloaded from the Microsoft Symbol Server");
                string downloadLocation = version.TryDownloadDac();
                Console.WriteLine("Downloaded a copy of the dac to:\n" + downloadLocation);
                return downloadLocation;
            }
        }

        private static void PrintMemoryRegionInfo(ClrRuntime runtime)
        {
            Console.WriteLine("\nMemory Region Information");
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine("{0,6} {1,15} {2}", "Count", "Total Size", "Type");
            Console.WriteLine("--------------------------------------------");
            foreach (var region in (from r in runtime.EnumerateMemoryRegions()
                                    where r.Type != ClrMemoryRegionType.ReservedGCSegment
                                    group r by r.Type into g
                                    let total = g.Sum(p => (uint)p.Size)
                                    orderby total descending
                                    select new
                                    {
                                        TotalSize = total,
                                        Count = g.Count(),
                                        Type = g.Key
                                    }))
            {
                Console.WriteLine("{0,6:n0} {1,15:n0} {2}", region.Count, region.TotalSize, region.Type.ToString());
            }
            Console.WriteLine("--------------------------------------------");
        }

        private static void PrintGCHeapInfo(ClrHeap heap)
        {
            Console.WriteLine("\nGC Heap Information");
            Console.WriteLine("--------------------------------------------------------------------");
            Console.WriteLine("{0,12} {1,12} {2,12} {3,12} {4,4} {5}", "Start", "End", "Committed", "Reserved", "Heap", "Type");
            Console.WriteLine("--------------------------------------------------------------------");
            foreach (ClrSegment segment in heap.Segments)
            {
                string type;
                if (segment.IsEphemeral)
                    type = "Ephemeral";
                else if (segment.IsLarge)
                    type = "Large";
                else
                    type = "Gen2";

                Console.WriteLine("{0,12:X} {1,12:X} {2,12:X} {3,12:X} {4,4} {5}",
                                    segment.Start, segment.End, segment.CommittedEnd, segment.ReservedEnd, segment.ProcessorAffinity, type);
            }
            Console.WriteLine("--------------------------------------------------------------------");

            Console.WriteLine();
            foreach (var item in (from seg in heap.Segments
                                  group seg by seg.ProcessorAffinity into g
                                  orderby g.Key
                                  select new
                                  {
                                      Heap = g.Key,
                                      Size = g.Sum(p => (uint)p.Length)
                                  }))
            {
                Console.WriteLine("Heap {0,2}: {1,12:n0} bytes", item.Heap, item.Size);
            }
            Console.WriteLine();
        }
    }
}
