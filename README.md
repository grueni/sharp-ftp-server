sharp-ftp-server
================

Asynchronous ftp server written in C#. Extensible to support any telnet type server (SMTP, POP3, etc.).

http://www.codeproject.com/Articles/380769/Creating-an-FTP-Server-in-Csharp-with-IPv-Support

This project is based on Rick Bassham's project.

I added

* support for AUTH TLS 1.2
* an optional unique folder per upload
* an optional start of post processing after an upload
* virtual directories


You find the project in Branch "feature-uniqueUploads" 

https://github.com/grueni/sharp-ftp-server/tree/feature-uniqueUploads
