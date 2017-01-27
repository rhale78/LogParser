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

		protected bool HasLogHeader(string line, out DateTime date, out string logTime)
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
							logTime = tmpDateTime.Trim();
							date = dummyDate;
							return true;
						}
					}
				}
			}
			logTime = "";
			return false;
		}
		protected bool HasLogHeader(string line)
		{
			DateTime dummyDate;
			string logtime = "";
			return HasLogHeader(line, out dummyDate, out logtime);
		}

		private const string DatabaseConnection = "Server=localhost; Database=LogDatabase; Trusted_Connection=True;MultipleActiveResultSets=True";
		private const string TempLogPath = "C:\\TmpLogParser";
		DateTime searchMinFileDateTime = new DateTime(2016, 10, 18, 19, 00, 00);
		DateTime searchMaxFileDateTime = new DateTime(2016, 10, 20, 19, 59, 00);

		public bool ProcessZipFile(string file)
		{
			Console.WriteLine(file);
			string tmp = file;
			tmp = tmp.Replace("server.log.", "").Replace(".zip", "");
			tmp = System.IO.Path.GetFileNameWithoutExtension(tmp);
			DateTime fileLog;
			//if (DateTime.TryParse(tmp, out fileLog))
			//{
			//	if (fileLog > searchMinDateTime && fileLog < searchMaxDateTime)
			//	{
			//		FileInfo info = new FileInfo(file);
			//		if ((info.CreationTime > searchMinDateTime && info.CreationTime < searchMaxDateTime) || (info.LastWriteTime > searchMinDateTime && info.LastWriteTime < searchMaxDateTime))
			//		{
			//			return true;
			//		}
			//	}
			//}
			//else
			{
				FileInfo info = new FileInfo(file);
				if ((info.CreationTime > searchMinFileDateTime && info.CreationTime < searchMaxFileDateTime) || (info.LastWriteTime > searchMinFileDateTime && info.LastWriteTime < searchMaxFileDateTime))
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
			if ((info.CreationTime > searchMinFileDateTime && info.CreationTime < searchMaxFileDateTime) || (info.LastWriteTime > searchMinFileDateTime && info.LastWriteTime < searchMaxFileDateTime))
			{
				return true;
			}
			return false;
		}
		public bool ProcessZipEntry(ZipArchiveEntry info)
		{
			if (info.LastWriteTime > searchMinFileDateTime && info.LastWriteTime < searchMaxFileDateTime)
			{
				return true;
			}
			return false;
		}
		public bool ProcessLogFile(string file)
		{
			Console.WriteLine(file);
			FileInfo info = new FileInfo(file);
			if ((info.CreationTime > searchMinFileDateTime && info.CreationTime < searchMaxFileDateTime) || (info.LastWriteTime > searchMinFileDateTime && info.LastWriteTime < searchMaxFileDateTime))
			{
				return true;
			}
			return true;
		}

		DateTime searchMinDateTime = new DateTime(2016, 10, 19, 11, 00, 00);
		DateTime searchMaxDateTime = new DateTime(2016, 10, 19, 13, 25, 00);
		public bool ProcessLogEntry(string line)
		{
			DateTime logDate;
			string logtime = "";
			if (HasLogHeader(line, out logDate, out logtime))
			{
				if ((logDate > searchMinDateTime && logDate < searchMaxDateTime))
				{
					//if ((line.IndexOf("OA-Y4FE5UQE0", StringComparison.OrdinalIgnoreCase) > -1 || line.IndexOf("OA-B2RALPJ1K", StringComparison.OrdinalIgnoreCase) > -1 || line.IndexOf("SOAP call 1532<", StringComparison.OrdinalIgnoreCase) > -1 || line.IndexOf("SOAP call 3310<", StringComparison.OrdinalIgnoreCase) > -1 || line.IndexOf("SOAP call 3675<", StringComparison.OrdinalIgnoreCase) > -1))
						if ((line.IndexOf("MC-L93VC9HMJ", StringComparison.OrdinalIgnoreCase) > -1 ||  line.IndexOf("SOAP call 3023<", StringComparison.OrdinalIgnoreCase) > -1 || line.IndexOf("SOAP call 2676<", StringComparison.OrdinalIgnoreCase) > -1 ))
						{
						return true;
					}
				}
			}
			return false;
		}

		public void Parse()
		{
			try
			{
				if (System.IO.Directory.Exists(TempLogPath))
				{
					DirectoryInfo info = new DirectoryInfo(TempLogPath);
					info.Delete(true);
				}
				System.IO.Directory.CreateDirectory(TempLogPath);
				if (!System.IO.Directory.Exists(TempLogPath))
				{
					System.IO.Directory.CreateDirectory(TempLogPath);
				}

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
			string[] logFiles = System.IO.Directory.GetFiles(TempLogPath);
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
						string logheaderTime = "";
						DateTime formerLogHeader = DateTime.MinValue;
						string formerLogHeaderTime = "";
						foreach (string currentLine in lines)
						{
							string line = currentLine;

							if (!string.IsNullOrEmpty(line))
							{
								//if (!line.EndsWith(System.Environment.NewLine))
								//{
									line = line + System.Environment.NewLine;
								//}

								bool hasLogHeader = HasLogHeader(line, out logHeader, out logheaderTime);
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
										WriteLogEntry(formerLogHeaderTime, index, LogWriter.ToString());
										formerLogHeader = logHeader;
										formerLogHeaderTime = logheaderTime;
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
											WriteLogEntry(formerLogHeaderTime, index, LogWriter.ToString());
											formerLogHeader = logHeader;
											formerLogHeaderTime = logheaderTime;
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
							WriteLogEntry(logheaderTime, index, LogWriter.ToString());
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
										entry.ExtractToFile(TempLogPath + "\\" + entry.FullName);
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
								System.IO.File.Copy(file, TempLogPath + "\\" + System.IO.Path.GetFileName(file));
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

		protected SqlConnection Connection { get; set; }
		protected void VerifyValidConnection()
		{
			do
			{
				try
				{
					if (Connection == null)
					{
						Connection = new SqlConnection(DatabaseConnection);
						Connection.Open();
					}
					if (Connection.State != System.Data.ConnectionState.Open)
					{
						Connection.Open();
					}
				}
				catch (Exception ex)
				{
					Connection = null;
					System.Threading.Thread.Sleep(5000);
				}
			} while (Connection == null || Connection.State != System.Data.ConnectionState.Open);
		}

		protected int GetProcessFileEntry(string server, string filename)
		{
			if (server.Contains("\\"))
			{
				server = server.Substring(server.LastIndexOf("\\") + 1);
			}
			VerifyValidConnection();
			try
			{
				//using (SqlCommand cmd = new SqlCommand("SELECT ID FROM PROCESSEDFILES WHERE Filename='" + filename + "' and Server='" + server + "'", connection))
				using (SqlCommand cmd = new SqlCommand("GetProcessFileEntryID", Connection)) // "SELECT ID FROM PROCESSEDFILES WHERE Filename='" + filename + "' and Server='" + server + "'", connection))
				{
					cmd.CommandType = System.Data.CommandType.StoredProcedure;
					cmd.Parameters.AddWithValue("@Filename", filename);
					cmd.Parameters.AddWithValue("@Server", server);
					object tmp = null; //cmd.ExecuteScalar();
					if (tmp == null || tmp is DBNull)
					{
						return 0;
					}
					return (int)tmp;
				}
			}
			catch (Exception ex)
			{ }

			return 0;
		}
		protected int GetLogEntry(string logTime, int fileprocessId, string data)
		{
			int? logEventTypeID = null;
			int? logSourceID = null;
			int? logURLID = null;
			int? soapParameterID = null;
			string soapCallNumber = "";
			string eventTypeName, logSourceName, urlName, soapParameter = "";

			ProcessDataForLogEntry(ref logTime, ref data, ref soapCallNumber, ref soapParameter, out eventTypeName, out logSourceName, out urlName);
			GetSubTableIDs(out logEventTypeID, out logSourceID, out logURLID, out soapParameterID, eventTypeName, logSourceName, urlName, soapParameter);
			VerifyValidConnection();
			try
			{
				using (SqlCommand cmd = new SqlCommand("GetLogEntryID", Connection))
				{
					cmd.CommandType = System.Data.CommandType.StoredProcedure;
					cmd.Parameters.AddWithValue("@LogTime", String.Format("{0:o}", logTime));
					cmd.Parameters.AddWithValue("@FileProcessID", fileprocessId);
					cmd.Parameters.AddWithValue("@LogEventTypeID", logEventTypeID);
					cmd.Parameters.AddWithValue("@LogSourceID", logSourceID);
					cmd.Parameters.AddWithValue("@LogURLOriginID", logURLID);
					cmd.Parameters.AddWithValue("@SoapCallNumber", soapCallNumber);
					if (!string.IsNullOrEmpty(soapCallNumber))
					{
						cmd.Parameters.AddWithValue("@SoapParameterID", soapParameterID);
					}
					cmd.Parameters.AddWithValue("@Data", data);

					object tmp =null; //cmd.ExecuteScalar();
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

			return 0;
		}

		private void GetSubTableIDs(out int? logEventTypeID, out int? logSourceID, out int? logURLID, out int? soapParameterID, string eventTypeName, string logSourceName, string urlName, string soapParameter)
		{
			if (EventTypeCache.ContainsKey(eventTypeName))
			{
				logEventTypeID = EventTypeCache[eventTypeName];
			}
			else
			{
				int id = GetLogEventTypeID(eventTypeName);
				if (id == 0)
				{
					WriteLogEventType(eventTypeName);
				}
				EventTypeCache.Add(eventTypeName, GetLogEventTypeID(eventTypeName));
				logEventTypeID = EventTypeCache[eventTypeName];
			}
			if (SoapParameterCache.ContainsKey(soapParameter))
			{
				soapParameterID = SoapParameterCache[soapParameter];
			}
			else
			{
				int id = GetSoapParameterID(soapParameter);
				if (id == 0)
				{
					WriteSoapParameter(soapParameter);
				}
				SoapParameterCache.Add(soapParameter, GetSoapParameterID(soapParameter));
				soapParameterID = SoapParameterCache[soapParameter];
			}

			if (LogSourceCache.ContainsKey(logSourceName))
			{
				logSourceID = LogSourceCache[logSourceName];
			}
			else
			{
				int id = GetLogSourceID(logSourceName);
				if (id == 0)
				{
					WriteLogSource(logSourceName);
				}
				LogSourceCache.Add(logSourceName, GetLogSourceID(logSourceName));
				logSourceID = LogSourceCache[logSourceName];
			}

			if (URLCache.ContainsKey(urlName))
			{
				logURLID = URLCache[urlName];
			}
			else
			{
				int id = GetLogURLOriginID(urlName);
				if (id == 0)
				{
					WriteLogURLOrigin(urlName);
				}
				URLCache.Add(urlName, GetLogURLOriginID(urlName));
				logURLID = URLCache[urlName];
			}
		}
		protected int GetSubtableID(string storedProcName, string name)
		{
			VerifyValidConnection();
			try
			{
				using (SqlCommand cmd = new SqlCommand(storedProcName, Connection))
				{
					cmd.CommandType = System.Data.CommandType.StoredProcedure;
					cmd.Parameters.AddWithValue("@Name", name);

					object tmp = null; //cmd.ExecuteScalar();
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
			return 0;
		}

		protected int GetSoapParameterID(string name)
		{
			return GetSubtableID("GetSoapParameterID", name);
		}
		protected int GetLogEventTypeID(string name)
		{
			return GetSubtableID("GetLogEventTypeID", name);
		}
		protected int GetLogSourceID(string name)
		{
			return GetSubtableID("GetLogSourceID", name);
		}
		protected int GetLogURLOriginID(string name)
		{
			return GetSubtableID("GetLogURLOriginID", name);
		}
		protected void WriteSubtableData(string storedProcName, string name)
		{
			VerifyValidConnection();
			try
			{
				using (SqlCommand cmd = new SqlCommand(storedProcName, Connection))
				{
					cmd.CommandType = System.Data.CommandType.StoredProcedure;
					cmd.Parameters.AddWithValue("@Name", name);

					//cmd.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{

			}
		}
		protected void WriteLogURLOrigin(string name)
		{
			WriteSubtableData("WriteLogURLOrigin", name);
		}
		protected void WriteLogSource(string name)
		{
			WriteSubtableData("WriteLogSource", name);
		}
		protected void WriteLogEventType(string name)
		{
			WriteSubtableData("WriteLogEventType", name);
		}
		protected void WriteSoapParameter(string name)
		{
			WriteSubtableData("WriteSoapParameter", name);
		}

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
					VerifyValidConnection();
					try
					{
						using (SqlCommand cmd = new SqlCommand("WriteProcessFileEntry", Connection))
						{
							cmd.CommandType = System.Data.CommandType.StoredProcedure;
							cmd.Parameters.AddWithValue("@Filename", filename);
							cmd.Parameters.AddWithValue("@Server", server);

							//cmd.ExecuteNonQuery();
						}
					}
					catch (Exception ex)
					{

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

		Dictionary<string, int> EventTypeCache = new Dictionary<string, int>();
		Dictionary<string, int> SoapParameterCache = new Dictionary<string, int>();
		Dictionary<string, int> LogSourceCache = new Dictionary<string, int>();
		Dictionary<string, int> URLCache = new Dictionary<string, int>();

		protected void WriteLogEntry(string logTime, int fileprocessId, string data)
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
					int? logEventTypeID = null;
					int? logSourceID = null;
					int? logURLID = null;
					int? soapParameterID = null;
					string soapCallNumber = "";
					string eventTypeName, logSourceName, urlName, soapParameter = "";

					ProcessDataForLogEntry(ref logTime, ref data, ref soapCallNumber, ref soapParameter, out eventTypeName, out logSourceName, out urlName);
					GetSubTableIDs(out logEventTypeID, out logSourceID, out logURLID, out soapParameterID, eventTypeName, logSourceName, urlName, soapParameter);

					VerifyValidConnection();
					try
					{
						using (SqlCommand cmd = new SqlCommand("WriteLogEntry", Connection))
						{
							cmd.CommandType = System.Data.CommandType.StoredProcedure;
							cmd.Parameters.AddWithValue("@LogTime", String.Format("{0:o}", logTime));
							cmd.Parameters.AddWithValue("@FileProcessID", fileprocessId);
							cmd.Parameters.AddWithValue("@LogEventTypeID", logEventTypeID);
							cmd.Parameters.AddWithValue("@LogSourceID", logSourceID);
							cmd.Parameters.AddWithValue("@LogURLOriginID", logURLID);
							cmd.Parameters.AddWithValue("@SoapCallNumber", soapCallNumber);
							if (!string.IsNullOrEmpty(soapCallNumber))
							{
								cmd.Parameters.AddWithValue("@SoapParameterID", soapParameterID);
							}
							cmd.Parameters.AddWithValue("@Data", data);

							//cmd.ExecuteNonQuery();
						}
					}
					catch (Exception ex)
					{

					}
				}
			}
			catch (Exception ex)
			{
			}
		}

		private void ProcessDataForLogEntry(ref string logTime, ref string data, ref string soapCallNumber, ref string soapParameter, out string eventTypeName, out string logSourceName, out string urlName)
		{
			if (logTime == "")
			{
				DateTime dummy;
				HasLogHeader(data, out dummy, out logTime);
			}
			eventTypeName = data.TrimStart().Substring(23, data.IndexOf(" ", 25) - 23).Trim();
			int braceStart = data.IndexOf("[");
			int braceEnd = data.IndexOf("]");
			logSourceName = data.Substring(braceStart + 1, braceEnd - braceStart - 1);
			int parenStart = data.IndexOf("(");
			int parenEnd = data.IndexOf(")");
			urlName = data.Substring(parenStart + 1, parenEnd - parenStart - 1);
			if (urlName.Contains(":SOAP call "))
			{
				int soapStart = urlName.IndexOf(":SOAP call ");
				int leftArrowStart = urlName.IndexOf("<");
				soapCallNumber = urlName.Substring(soapStart + 11, leftArrowStart - soapStart - 11);
				int tmpCallNumber = 0;
				if (!int.TryParse(soapCallNumber, out tmpCallNumber))
				{
					soapCallNumber = "";
				}
				else
				{
					urlName = urlName.Substring(0, urlName.IndexOf(":"));
					leftArrowStart = data.IndexOf("<");
					int rightParenStart = data.IndexOf(")");
					soapParameter = data.Substring(leftArrowStart, rightParenStart - leftArrowStart).Trim();
					data = data.Substring(rightParenStart + 1).TrimStart();
				}
			}
			else
			{
				data = data.Substring(data.IndexOf(":)") + 2).TrimStart();
			}
		}
	}
}