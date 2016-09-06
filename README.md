# HeapStringAnalyser
Analyse Memory Dumps looking at .NET String types

## Usage Instructions

1. Create a **Full** Process Dump (e.g. in Task Manager, right-click on the process and click **Create Dump File**)
1. Run the tool by executing the command line `HeapStringAnalyser.exe <Full path of the Process Dump>`
1. You can also add the `--gcInfo` argument to the cmd-line and you will get additional information about the GC Heaps

## Sample output

```
.NET Memory Dump Heap Analyser - created by Matt Warren - github.com/mattwarren                         
                                                                                                        
Found CLR Version: v4.6.1076.00                                                                         
                                                                                                        
Dac already exists on the local machine at:                                                             
C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscordacwks.dll                                           
                                                                                                        
Memory Region Information                                                                               
------------------------------------------------                                                        
                    Type Count Total Size (MB)                                                          
------------------------------------------------                                                        
               GCSegment     6           75.67                                                          
       ReservedGCSegment     6           20.30                                                          
  LowFrequencyLoaderHeap   246           16.10                                                          
 HighFrequencyLoaderHeap   208           12.92                                                          
             ResolveHeap    22            1.25                                                          
            DispatchHeap    12            0.61                                                          
        HandleTableChunk     8            0.50                                                          
                StubHeap    10            0.47                                                          
          CacheEntryHeap     7            0.29                                                          
             IndcellHeap     6            0.21                                                          
              LookupHeap     6            0.21                                                          
------------------------------------------------                                                        
                                                                                                        
GC Heap Information - Workstation                                                                       
-----------------------------------------------------------                                             
Heap  0:   78,030,788 bytes (74.42 MB) in use                                                           
-----------------------------------------------------------                                             
        Type    Size (MB) Committed (MB)  Reserved (MB)                                                 
-----------------------------------------------------------                                             
   Ephemeral         2.66           3.79          16.00                                                 
        Gen2        16.00          16.00          16.00                                                 
        Gen2        16.00          16.00          16.00                                                 
        Gen2        16.00          16.00          16.00                                                 
        Gen2        15.89          15.89          16.00                                                 
       Large         7.88           8.00          16.00                                                 
-----------------------------------------------------------                                             
Total (across all heaps): 78,030,788 bytes (74.42 MB)                                                   
-----------------------------------------------------------                                             
                                                                                                        
"System.String" memory usage info                                                                       
Overall 145,872 "System.String" objects take up 12,391,286 bytes (11.82 MB)                             
Of this underlying byte arrays (as Unicode) take up 10,349,078 bytes (9.87 MB)                          
Remaining data (object headers, other fields, etc) are 2,042,208 bytes (1.95 MB), at 14 bytes per object
                                                                                                        
Actual Encoding that the "System.String" could be stored as (with corresponding data size)              
       10,339,638 bytes ( 145,505 strings) as ASCII                                                     
            3,370 bytes (      65 strings) as ISO-8859-1 (Latin-1)                                      
            6,070 bytes (     302 strings) as Unicode                                                   
Total: 10,349,078 bytes (expected: 10,349,078)                                                          
                                                                                                        
Compression Summary:                                                                                    
        5,171,504 bytes Compressed (to ISO-8859-1 (Latin-1))                                            
            6,070 bytes Uncompressed (as Unicode)                                                       
          145,872 bytes EXTRA to enable compression (1-byte field, per "System.String" object)          
                                                                                                        
Total Usage:  5,323,446 bytes (5.08 MB), compared to 10,349,078 (9.87 MB) before compression            
Total Saving: 5,025,632 bytes (4.79 MB)

Press any key to continue . . .
```