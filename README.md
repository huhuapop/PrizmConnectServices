# PrizmConnectServices
PrizmConnectServices is developed by vb.net 2003 from 2010 to 2014 via about three big version. This is an service that is based on config file to generate several threads to receive xml file from MSMQ, verify data then store the data to database.
For normally, it could generate more than twenty threads for light workload. If some theads need to process more than one hundred xml file(from 100 KB to 5MB) per second per thread, the number of the thread should not be exceed (cpu number * core number)* 2 of threads.
This program has been tested by a lot of extremity serious situation, such as the usage of CPU is nearly 100% for about twenty-four hours, the hard disk is full and could not write any file to the server.
This program is about 800 lines.
