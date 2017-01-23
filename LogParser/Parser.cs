﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogParser
{
	public class Parser
	{
		protected string StartPath { get; set; }

		public Parser(string startPath)
		{
			StartPath = startPath;
		}

		protected bool HasLogHeader(string line)
		{
			if (!string.IsNullOrEmpty(line))
			{
				int commaIndex = line.IndexOf(",");
				if (commaIndex < 24 && commaIndex > -1)
				{
					string tmpDateTime = line.Substring(0, commaIndex);
					DateTime dummyDate;
					if (DateTime.TryParse(tmpDateTime.Trim(), out dummyDate))
					{
						tmpDateTime = line.Substring(0, commaIndex + 4);
						tmpDateTime = tmpDateTime.Replace(",", ".");
						if (DateTime.TryParse(tmpDateTime.Trim(), out dummyDate))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		protected DateTime GetLogHeaderDateTime(string line)
		{
			if (!string.IsNullOrEmpty(line))
			{
				int commaIndex = line.IndexOf(",");
				if (commaIndex < 24 && commaIndex > -1)
				{
					string tmpDateTime = line.Substring(0, commaIndex);
					DateTime dummyDate;
					if (DateTime.TryParse(tmpDateTime.Trim(), out dummyDate))
					{
						tmpDateTime = line.Substring(0, commaIndex + 4);
						tmpDateTime = tmpDateTime.Replace(",", ".");
						if (DateTime.TryParse(tmpDateTime.Trim(), out dummyDate))
						{
							return dummyDate;
						}
					}
				}
			}
			return DateTime.MinValue;
		}

		public bool ProcessZipFile(string file)
		{
			FileInfo info = new FileInfo(file);
			return true;
		}
		public bool ProcessZipEntry(ZipArchiveEntry archiveFile)
		{
			return true;
		}
		public bool ProcessZipEntry(string file)
		{
			return true;
		}
		public bool ProcessLogEntry(string line)
		{
			if(HasLogHeader(line))
			{
				DateTime logDate = GetLogHeaderDateTime(line);
			}
			return true;
		}

		public void Parse()
		{
			try
			{
				if(System.IO.Directory.Exists("C:\\TmpLogParser"))
				{
					DirectoryInfo info = new DirectoryInfo("C:\\TmpLogParser");
					info.Delete(true);
				}
				System.IO.Directory.CreateDirectory("C:\\TmpLogParser");
				string[] directories = System.IO.Directory.GetDirectories(StartPath);
				foreach(string directory in directories)
				{
					string[] files = System.IO.Directory.GetFiles(directory, "*.zip");
					foreach(string file in files)
					{
						if (ProcessZipFile(file))
						{
							try
							{
								ZipArchive zipArchive = ZipFile.OpenRead(file);
								foreach(ZipArchiveEntry entry in zipArchive.Entries)
								{
									if(ProcessZipEntry(entry))
									{
										entry.ExtractToFile("C:\\TmpLogParser");
									}
								}
								//ZipFile.ExtractToDirectory(file, "C:\\TmpLogParser");
								string[] zipFiles = System.IO.Directory.GetFiles("C:\\TmpLogParser");
								foreach (string zipFile in zipFiles)
								{
									string[] lines = System.IO.File.ReadAllLines(zipFile);
									string logEntry = string.Empty;
									bool validLog = false;
									foreach (string currentLine in lines)
									{
										string line = currentLine;
										if (!string.IsNullOrEmpty(line))
										{
											if (!line.EndsWith(System.Environment.NewLine))
											{
												line = line + System.Environment.NewLine;
											}

											if (HasLogHeader(line)&&ProcessLogEntry(line))
											{
												validLog = true;
												if (!string.IsNullOrEmpty(logEntry))
												{
													WriteLog(logEntry);
													logEntry = string.Empty;
												}

												logEntry = line;
											}
											else
											{
												if(HasLogHeader(line)&&!ProcessLogEntry(line))
												{
													validLog = false;
												}
												if (validLog)
												{
													logEntry = logEntry + line;
												}
											}
										}
									}
									if (!string.IsNullOrEmpty(logEntry))
									{
										WriteLog(logEntry);
										logEntry = string.Empty;
									}
									if (System.IO.File.Exists(zipFile))
									{
										System.IO.File.Delete(zipFile);
									}
								}
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex.Message);
							}
						}
					}
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		protected void WriteLog(string log)
		{
			System.Diagnostics.Debug.WriteLine(log);
		}
	}
}
