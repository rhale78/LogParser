using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
		protected StringBuilder StringBuilder { get; set; }
		public Parser(string startPath)
		{
			StartPath = startPath;
			StringBuilder = new StringBuilder();
		}
		protected string Directory { get; set; }

		protected bool HasLogHeader(string line, out DateTime date)
		{
			date = DateTime.MinValue;
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
							date = dummyDate;
							return true;
						}
					}
				}
			}
			return false;
		}
		protected bool HasLogHeader(string line)
		{
			DateTime dummyDate;
			return HasLogHeader(line, out dummyDate);
		}

		DateTime searchMinDateTime = new DateTime(2017, 1, 12, 19, 00, 00);
		DateTime searchMaxDateTime = new DateTime(2017, 1, 14, 19, 59, 00);

		public bool ProcessZipFile(string file)
		{
			Console.WriteLine(file);
			string tmp = file;
			tmp = tmp.Replace("server.log.", "").Replace(".zip", "");
			tmp = System.IO.Path.GetFileNameWithoutExtension(tmp);
			DateTime fileLog;
			if (DateTime.TryParse(tmp, out fileLog))
			{
				if (fileLog > searchMinDateTime && fileLog < searchMaxDateTime)
				{
					FileInfo info = new FileInfo(file);
					if ((info.CreationTime > searchMinDateTime && info.CreationTime < searchMaxDateTime) || (info.LastWriteTime > searchMinDateTime && info.LastWriteTime < searchMaxDateTime))
					{
						return true;
					}
				}
			}
			else
			{
				FileInfo info = new FileInfo(file);
				if ((info.CreationTime > searchMinDateTime && info.CreationTime < searchMaxDateTime) || (info.LastWriteTime > searchMinDateTime && info.LastWriteTime < searchMaxDateTime))
				{
					return true;
				}
			}
			return false;
		}
		public bool ProcessRegularFile(string file)
		{
			Console.WriteLine(file);
			FileInfo info = new FileInfo(file);
			if ((info.CreationTime > searchMinDateTime && info.CreationTime < searchMaxDateTime) || (info.LastWriteTime > searchMinDateTime && info.LastWriteTime < searchMaxDateTime))
			{
				return true;
			}
			return false;
		}
		public bool ProcessZipEntry(ZipArchiveEntry info)
		{
			if (info.LastWriteTime > searchMinDateTime && info.LastWriteTime < searchMaxDateTime)
			{
				return true;
			}
			return false;
		}
		public bool ProcessLogFile(string file)
		{
			Console.WriteLine(file);
			FileInfo info = new FileInfo(file);
			if ((info.CreationTime > searchMinDateTime && info.CreationTime < searchMaxDateTime) || (info.LastWriteTime > searchMinDateTime && info.LastWriteTime < searchMaxDateTime))
			{
				return true;
			}
			return true;
		}
		public bool ProcessLogEntry(string line)
		{
			DateTime searchMinDateTime = new DateTime(2017, 1, 13, 11, 17, 00);
			DateTime searchMaxDateTime = new DateTime(2017, 1, 13, 17, 25, 00);
			DateTime logDate;
			if (HasLogHeader(line, out logDate))
			{
				if ((logDate > searchMinDateTime && logDate < searchMaxDateTime) && (line.IndexOf("OA-Y4FE5UQE0", StringComparison.OrdinalIgnoreCase) > -1 || line.IndexOf("OA-B2RALPJ1K", StringComparison.OrdinalIgnoreCase) > -1 || line.IndexOf("SOAP call 1532<", StringComparison.OrdinalIgnoreCase) > -1 || line.IndexOf("SOAP call 3310<", StringComparison.OrdinalIgnoreCase) > -1 || line.IndexOf("SOAP call 3675<", StringComparison.OrdinalIgnoreCase) > -1))
				{
					return true;
				}
			}
			return false;
		}

		public void Parse()
		{
			try
			{
				if (System.IO.Directory.Exists("C:\\TmpLogParser"))
				{
					DirectoryInfo info = new DirectoryInfo("C:\\TmpLogParser");
					info.Delete(true);
				}
				System.IO.Directory.CreateDirectory("C:\\TmpLogParser");
				string[] directories = System.IO.Directory.GetDirectories(StartPath);
				foreach (string directory in directories)
				{
					FileToZip.Clear();
					Directory = directory;
					ProcessZipFiles(directory);
					ProcessLogFiles();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		StringBuilder LogWriter = new StringBuilder();

		private void ProcessLogFiles()
		{
			string[] logFiles = System.IO.Directory.GetFiles("C:\\TmpLogParser");
			foreach (string logFile in logFiles)
			{
				if (ProcessLogFile(logFile))
				{
					string zipFilename = FileToZip[System.IO.Path.GetFileName(logFile)];
					int index = 0;
					if (zipFilename != "")
					{
						index = GetProcessFileEntry(Directory, zipFilename + "(" + System.IO.Path.GetFileName(logFile) + ")");
					}
					else
					{
						index = GetProcessFileEntry(Directory, System.IO.Path.GetFileName(logFile));
					}
					if (index == 0)
					{
						int a = 0;
					}
					try
					{
						string[] lines = System.IO.File.ReadAllLines(logFile);
						StringBuilder.Clear();
						LogWriter.Clear();
						string logEntry = string.Empty;
						bool validLog = false;
						DateTime logHeader = DateTime.MinValue;
						DateTime formerLogHeader = DateTime.MinValue;
						foreach (string currentLine in lines)
						{
							string line = currentLine;

							if (!string.IsNullOrEmpty(line))
							{
								if (!line.EndsWith(System.Environment.NewLine))
								{
									line = line + System.Environment.NewLine;
								}


								bool hasLogHeader = HasLogHeader(line, out logHeader);
								bool processLogEntry = ProcessLogEntry(line);
								if (hasLogHeader && processLogEntry)
								{
									validLog = true;
									if (StringBuilder.Length != 0)
									{
										WriteLog(logFile, StringBuilder.ToString());
										StringBuilder.Clear();
									}
									if (LogWriter.Length != 0)
									{
										WriteLogEntry(formerLogHeader, index, LogWriter.ToString());
										formerLogHeader = logHeader;
										LogWriter.Clear();
									}

									StringBuilder.Append(line);
									LogWriter.Append(line);
								}
								else
								{
									if (hasLogHeader && !processLogEntry)
									{
										validLog = false;

										if (LogWriter.Length != 0)
										{
											WriteLogEntry(formerLogHeader, index, LogWriter.ToString());
											formerLogHeader = logHeader;
											LogWriter.Clear();
										}

										LogWriter.Append(line);
										continue;
									}
									if (validLog)
									{
										StringBuilder.Append(line);
									}
									LogWriter.Append(line);
								}
							}
						}
						if (StringBuilder.Length != 0)
						{
							WriteLog(logFile, StringBuilder.ToString());
							StringBuilder.Clear();
						}
						if (LogWriter.Length != 0)
						{
							WriteLogEntry(logHeader, index, LogWriter.ToString());
							LogWriter.Clear();
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
					}
				}
				if (System.IO.File.Exists(logFile))
				{
					System.IO.File.Delete(logFile);
				}
			}
			FileToZip.Clear();
		}

		protected Dictionary<string, string> FileToZip = new Dictionary<string, string>();

		private void ProcessZipFiles(string directory)
		{
			string[] files = System.IO.Directory.GetFiles(directory, "*.zip");
			if (files != null && files.Count() > 0)
			{
				foreach (string file in files)
				{
					if (file.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
					{
						if (ProcessZipFile(file))
						{
							try
							{
								ZipArchive zipArchive = ZipFile.OpenRead(file);
								foreach (ZipArchiveEntry entry in zipArchive.Entries)
								{
									if (ProcessZipEntry(entry))
									{
										FileToZip.Add(System.IO.Path.GetFileName(entry.FullName), System.IO.Path.GetFileName(file));
										WriteProcessFileEntry(Directory, System.IO.Path.GetFileName(file) + "(" + System.IO.Path.GetFileName(entry.FullName) + ")");
										entry.ExtractToFile("C:\\TmpLogParser\\" + entry.FullName);
									}
								}
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex.Message);
							}
						}
					}
					else
					{
						if (ProcessRegularFile(file))
						{
							FileToZip.Add(System.IO.Path.GetFileName(file), "");
							try
							{
								WriteProcessFileEntry(Directory, System.IO.Path.GetFileName(file));
								System.IO.File.Copy(file, "C:\\TmpLogParser\\" + System.IO.Path.GetFileName(file));
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex.Message);
							}
						}
					}
				}
			}
		}

		protected void WriteLog(string filename, string log)
		{
			System.Diagnostics.Debug.WriteLine(Directory);
			System.Diagnostics.Debug.WriteLine(filename);
			System.Diagnostics.Debug.WriteLine(log);
		}

		bool needsVerify = false;

		protected int WriteProcessFileEntry(string server, string filename)
		{
			if (server.Contains("\\"))
			{
				server = server.Substring(server.LastIndexOf("\\") + 1);
			}
			try
			{
				int id = GetProcessFileEntry(server, filename);
				if (id == 0)
				{
					needsVerify = false;
					using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection("Server=localhost; Database=LogDatabase; Trusted_Connection=True;"))
					{
						try
						{
							connection.Open();
							using (SqlCommand cmd = new SqlCommand("INSERT INTO ProcessedFiles(Filename, Server) VALUES ('" + filename + "','" + server + "')", connection))
							{
								cmd.ExecuteNonQuery();
							}
						}
						catch (Exception ex)
						{

						}
					}
				}
				else
				{
					needsVerify = true;
					return id;
				}
			}
			catch (Exception ex)
			{ }
			return GetProcessFileEntry(server, filename);
		}

		protected int GetProcessFileEntry(string server, string filename)
		{
			if (server.Contains("\\"))
			{
				server = server.Substring(server.LastIndexOf("\\") + 1);
			}
			using (SqlConnection connection = new SqlConnection("Server=localhost; Database=LogDatabase; Trusted_Connection=True;"))
			{
				try
				{
					connection.Open();
					using (SqlCommand cmd = new SqlCommand("SELECT ID FROM PROCESSEDFILES WHERE Filename='" + filename + "' and Server='" + server + "'", connection))
					{
						object tmp = cmd.ExecuteScalar();
						if (tmp == null || tmp is DBNull)
						{
							return 0;
						}
						return (int)tmp;
					}
				}
				catch (Exception ex)
				{ }
			}
			return 0;
		}
		protected int GetLogEntry(DateTime logTime, int fileprocessId, string data)
		{
			using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection("Server=localhost; Database=LogDatabase; Trusted_Connection=True;"))
			{
				try
				{
					connection.Open();
					using (SqlCommand cmd = new SqlCommand("SELECT ID FROM LogEntry WHERE LogDateTime=convert(datetime2,'" + String.Format("{0:o}", logTime) + "') AND ProcessedFilesID='" + fileprocessId + "' AND Data='" + data + "'", connection))
					{
						object tmp = cmd.ExecuteScalar();
						if (tmp == null)
						{
							return 0;
						}
						else
						{
							return (int)tmp;
						}


					}
				}
				catch (Exception ex)
				{

				}
			}
			return 0;
		}

		protected void WriteLogEntry(DateTime logTime, int fileprocessId, string data)
		{
			try
			{
				int id = 0;
				if (needsVerify)
				{
					id = GetLogEntry(logTime, fileprocessId, data);
				}
				if (id == 0)
				{
					using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection("Server=localhost; Database=LogDatabase; Trusted_Connection=True;"))
					{
						try
						{
							connection.Open();
							using (SqlCommand cmd = new SqlCommand("INSERT INTO LogEntry(LogDateTime, ProcessedFilesID, Data) VALUES (convert(datetime2,'" + String.Format("{0:o}", logTime) + "'), '" + fileprocessId + "', '" + data + "')", connection))
							{
								cmd.ExecuteNonQuery();
							}
						}
						catch (Exception ex)
						{

						}
					}
				}
			}
			catch (Exception ex)
			{ }
		}
	}
}