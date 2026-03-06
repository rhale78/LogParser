# LogParser

A C# .NET console application that parses server log files and stores filtered log entries into a SQL Server database for analysis and troubleshooting.

## Overview

LogParser reads web service log files from a network path (including compressed ZIP archives), filters entries by date/time range and content patterns, and persists the matching entries into a structured SQL Server database. It is designed to help trace specific requests (e.g., SOAP calls or request IDs) across large sets of server log files.

## Features

- Reads log files from local or UNC network paths
- Handles both compressed `.zip` archives and plain log files
- Filters log entries by configurable date/time ranges
- Matches entries by content keywords (e.g., request IDs, SOAP call numbers)
- Persists filtered entries into a normalized SQL Server database
- Tracks which files have already been processed to avoid duplicate imports
- In-memory caching for lookup tables to minimize database round-trips
- Auto-reconnects to the database if the connection is lost

## Prerequisites

- [Visual Studio 2015 or later](https://visualstudio.microsoft.com/)
- [.NET Framework 4.5+](https://dotnet.microsoft.com/en-us/download/dotnet-framework)
- [SQL Server](https://www.microsoft.com/en-us/sql-server) (local or remote instance)
- [SQL Server Data Tools (SSDT)](https://learn.microsoft.com/en-us/sql/ssdt/download-sql-server-data-tools-ssdt) (to deploy the database project)
- Network access to the directory containing the log files
- Write permissions to `C:\TmpLogParser` (used as a temporary extraction directory)

## Project Structure

```
LogParser/
├── LogParser/                  # Console application
│   ├── Program.cs              # Entry point
│   ├── Parser.cs               # Core parsing logic
│   ├── App.config              # .NET runtime configuration
│   └── LogParser.csproj        # C# project file
├── LogDB/                      # SQL Server database project
│   ├── Tables/
│   │   ├── LogEntry.sql        # Main log records table
│   │   ├── ProcessedFiles.sql  # Tracks processed log files
│   │   ├── LogEventType.sql    # Event type lookup table
│   │   ├── LogSource.sql       # Log source lookup table
│   │   ├── LogURLOrigin.sql    # URL origin lookup table
│   │   └── SoapParameters.sql  # SOAP parameters lookup table
│   └── LogDB.sqlproj
├── LogDatabase/                # Secondary SQL database project
│   └── LogDatabase.sqlproj
└── LogParser.sln               # Visual Studio solution file
```

## Database Schema

The application writes to the following SQL Server tables:

| Table | Description |
|-------|-------------|
| `LogEntry` | Main table storing each filtered log entry |
| `ProcessedFiles` | Tracks log files that have been imported |
| `LogEventType` | Lookup table for event type classification |
| `LogSource` | Lookup table for log source names |
| `LogURLOrigin` | Lookup table for URL endpoints |
| `SoapParameters` | Lookup table for SOAP method parameter strings |

`LogEntry` references all lookup tables via foreign keys and is indexed by `LogDateTime` for efficient querying.

## Setup & Configuration

### 1. Deploy the Database

Open `LogParser.sln` in Visual Studio, right-click the `LogDB` project, and choose **Publish** to deploy the schema to your SQL Server instance. Alternatively, execute the `.sql` scripts in the `LogDB/Tables/` folder against your target database.

### 2. Configure the Database Connection

In `LogParser/Parser.cs`, update the connection string constant to point to your SQL Server instance:

```csharp
private const string DatabaseConnection =
    "Server=localhost; Database=LogDatabase; Trusted_Connection=True;MultipleActiveResultSets=True";
```

### 3. Set the Log Source Path

In `LogParser/Program.cs`, update the path passed to the `Parser` constructor to point to the directory containing your log files:

```csharp
Parser p = new Parser(@"\\server\share\logs\directory");
```

### 4. Configure Date/Time Filters

In `LogParser/Parser.cs`, adjust the date range fields to cover the period you want to import:

```csharp
// Controls which log files are selected (by file creation/write time)
DateTime searchMinFileDateTime = new DateTime(2016, 10, 18, 19, 00, 00);
DateTime searchMaxFileDateTime = new DateTime(2016, 10, 20, 19, 59, 00);

// Controls which individual log entries are imported (by entry timestamp)
DateTime searchMinDateTime = new DateTime(2016, 10, 19, 11, 00, 00);
DateTime searchMaxDateTime = new DateTime(2016, 10, 19, 13, 25, 00);
```

### 5. Configure Content Filters

In `LogParser/Parser.cs`, update the `ProcessLogEntry` method to match the request IDs or SOAP call numbers you are looking for:

```csharp
if (line.IndexOf("MC-L93VC9HMJ", StringComparison.OrdinalIgnoreCase) > -1 ||
    line.IndexOf("SOAP call 3023<", StringComparison.OrdinalIgnoreCase) > -1 ||
    line.IndexOf("SOAP call 2676<", StringComparison.OrdinalIgnoreCase) > -1)
```

## Build & Run

1. Open `LogParser.sln` in Visual Studio.
2. Build the solution (**Build → Build Solution** or `Ctrl+Shift+B`).
3. Run the application (**Debug → Start Without Debugging** or `Ctrl+F5`), or execute the compiled binary directly:

```
bin\Release\LogParser.exe
```

The application will:
1. Clear and recreate the temporary directory `C:\TmpLogParser`.
2. Iterate over subdirectories in the configured source path.
3. Extract matching ZIP archives and copy matching plain log files to the temp directory.
4. Read each extracted file line-by-line, accumulating multi-line log entries.
5. Write entries that pass the date/time and content filters into the database.
6. Print progress information to the console.

## How It Works

```
Source path (network share)
       │
       ▼
Scan subdirectories
       │
       ├─► ZIP files matching file date range ──► Extract to C:\TmpLogParser
       └─► Plain log files matching file date range ─► Copy to C:\TmpLogParser
                          │
                          ▼
             Read each file line by line
                          │
                          ▼
         Does line contain a log entry header?
          (timestamp at start of line)
                          │
              Yes ────────┴──────── No (append to current entry)
               │
               ▼
    Does entry fall in date/time window
    AND match a content keyword?
               │
              Yes
               │
               ▼
    Parse event type, source, URL, SOAP call
    number and parameters from entry text
               │
               ▼
    Write to SQL Server (with lookup
    table caching to reduce DB calls)
```

## License

Copyright © 2017–2026. All rights reserved.
