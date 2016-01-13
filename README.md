# HeapStringAnalyser
Analyse Memory Dumps looking at .NET String types

## Usage Instructions

1. Create a **Full** Process Dump (e.g. in Task Manager, right-click on the process and click **Create Dump File**)
1. Run the tool by executing the command line `HeapStringAnalyser.exe <Full path of the Process Dump>`

## Sample output

```
.NET Memory Dump Heap Analyser - created by Matt Warren - github.com/mattwarren

Found CLR Version: v4.0.30319.18444

...

Overall 35,903 "System.String" objects take up 2,717,588 bytes (2.59 MB)
Of this underlying byte arrays (as Unicode) take up 1,784,110 bytes (1.70 MB)
Remaining data (object headers, other fields, etc) is 933,478 bytes (0.89 MB), at 26 bytes per object

Actual Encoding that the "System.String" could be stored as (with corresponding data size)
        1,388,604 bytes are ASCII
              330 bytes are ISO-8859-1 (Latin-1)
          395,176 bytes are Unicode (UTF-16)
Total: 1,784,110 bytes (expected: 1,784,110)

Compression Summary:
          694,467 bytes Compressed (from Unicode -> ISO-8859-1 (Latin-1))
          395,176 bytes Uncompressed (as Unicode/UTF-16)
           35,903 bytes EXTRA to enable compression (one byte field, per "System.String" object)
Total: 1,125,546 bytes, compared to 1,784,110 before compression

Press any key to continue . . .
```
