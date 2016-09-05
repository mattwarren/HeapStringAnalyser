# HeapStringAnalyser
Analyse Memory Dumps looking at .NET String types

## Usage Instructions

1. Create a **Full** Process Dump (e.g. in Task Manager, right-click on the process and click **Create Dump File**)
1. Run the tool by executing the command line `HeapStringAnalyser.exe <Full path of the Process Dump>`

## Sample output

```
.NET Memory Dump Heap Analyser - created by Matt Warren - github.com/mattwarren

Found CLR Version: v4.6.100.00

...

GC Heap Information
-----------------------------------------------------------
Heap  0:   65,926,764 bytes (62.87 MB)
-----------------------------------------------------------
Total (across all heaps): 65,926,764 bytes (62.87 MB)
-----------------------------------------------------------

Overall 139,361 "System.String" objects take up 11,546,476 bytes (11.01 MB)
Of this underlying byte arrays (as Unicode) take up 9,595,422 bytes (9.15 MB)
Remaining data (object headers, other fields, etc) are 1,951,054 bytes (1.86 MB), at 14 bytes per object

Actual Encoding that the "System.String" could be stored as (with corresponding data size)
        9,575,954 bytes ( 138,983 strings) as ASCII
            5,172 bytes (      72 strings) as ISO-8859-1 (Latin-1)
           14,296 bytes (     306 strings) as Unicode (UTF-16)
Total: 9,595,422 bytes (expected: 9,595,422)

Compression Summary:
        4,790,563 bytes Compressed (to ISO-8859-1 (Latin-1))
           14,296 bytes Uncompressed (as Unicode/UTF-16)
          139,361 bytes EXTRA to enable compression (one byte field, per "System.String" object)

Total Usage:  4,944,220 bytes (4.72 MB), compared to 9,595,422 (9.15 MB) before compression
Total Saving: 4,651,202 bytes (4.44 MB)

Press any key to continue . . .
```