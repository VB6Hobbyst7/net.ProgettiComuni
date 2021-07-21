using Knos.API.COM;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace DMSConnector
{
	/// <summary>
	/// Semplice classe per la scrittura del log
	/// </summary>
	public class Logger
	{
		/// <summary>
		/// File di log
		/// </summary>
		public string LogFolderPath;

		/// <summary>
		/// Dimensione massima dei file di log
		/// </summary>
		public int MaxFileSize;

		/// <summary>
		/// Numero di errori loggati
		/// </summary>
		public int ErrorCounter;

		/// <summary>
		/// Costruttore del logger
		/// </summary>
		public Logger()
		{
			string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
			LogFolderPath = Path.Combine(Path.GetDirectoryName(exePath), "log");
			MaxFileSize = 250 * 1024; // 250 KB
			ErrorCounter = 0;
		}

		public enum LogEvent
		{
			Info,
			Warning,
			Error
		}

		/// <summary>
		/// Log su file
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="message"></param>
		/// <param name="logException"></param>
		/// <param name="soapResponse"></param>
		/// <param name="exception"></param>
		/// <returns></returns>
		public bool Log(LogEvent logEvent, string logMessage, Exception logException, XmlDocument soapResponse, out Exception exception)
		{
			// Il log non sollevare eccezioni e se non riesce a scrivere restituisce l'eventuale eccezione rilevata come parametro
			exception = new Exception("Errore interno");
			try
			{
				// I nomi della cartella e del file di log derivano data corrente
				DateTime now = DateTime.Now;

				// La parte iniziale del log comprende comunque il timestamp
				StringBuilder str = new StringBuilder(now.ToString("yyyy-MM-dd HH:mm:ss.fff", DateTimeFormatInfo.InvariantInfo));

				// La directory di log viene creata se non è già presente
				if (!Directory.Exists(LogFolderPath))
					Directory.CreateDirectory(LogFolderPath);

				// La sotto-cartella per la registrazione corrente del log è quella con la data corrente
				int i;
				FileInfo fi;
				string logFolderDatePath = Path.Combine(LogFolderPath, now.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo));
				// Se la cartella con la data corrente non c'è ancora la si crea
				if (!Directory.Exists(logFolderDatePath))
					Directory.CreateDirectory(logFolderDatePath);

				// Individuata la directory si può passare al file di log
				// Si inizia con il log per l'ora corrente
				string logFileName = now.ToString("HH-00-00") + ".txt";
				string logFilePath = Path.Combine(logFolderDatePath, logFileName);

				// Se il file hh-00-00.txt esiste già bisogna verificarne la dimensione
				if (File.Exists(logFilePath))
				{
					fi = new FileInfo(logFilePath);
					if (fi.Length > MaxFileSize)
					{
						// Superata la dimensione massima si passa ai minuti: hh-mm-00.txt
						logFileName = now.ToString("HH-mm-00") + ".txt";
						logFilePath = Path.Combine(logFolderDatePath, logFileName);
						if (File.Exists(logFilePath))
						{
							// Se il file esiste già bisogna verificarne la dimensione
							fi = new FileInfo(logFilePath);
							if (fi.Length > MaxFileSize)
							{
								// Superata la dimensione massima si passa ai secondi: hh-mm-ss.txt
								logFileName = now.ToString("HH-mm-ss") + ".txt";
								logFilePath = Path.Combine(logFolderDatePath, logFileName);
								if (File.Exists(logFilePath))
								{
									// Se il file esiste già bisogna verificarne la dimensione
									fi = new FileInfo(logFilePath);
									if (fi.Length > MaxFileSize)
									{
										// E' una condizione anomala
										exception = new Exception("Log Overflow");
										exception.Source = "Logger()";
										return false;
									}
								}
							}
						}
					}
				}

				// Arrivati qui si può creare il file e appendere il log
				using (StreamWriter stream = new StreamWriter(File.Open(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read), Encoding.UTF8))
				{
					// Evento
					string eventName = "Error";
					switch (logEvent)
					{
						case LogEvent.Info: eventName = "Info: ";
							break;
						case LogEvent.Warning: eventName = "Warning: ";
							break;
						case LogEvent.Error: eventName = "Error: ";
							ErrorCounter++;
							break;
					}
					str.Append(" " + eventName);

					// Messaggio di log
					str.Append(" " + logMessage);

					// Eventuale log automatizzato per le eccezioni
					if (logException != null)
					{
						AppendLogKeyValue(str, "ExceptionSource", logException.Source);
						AppendLogKeyValue(str, "ExceptionSource", logException.Message);
						if (logException.InnerException != null)
						{
							AppendLogKeyValue(str, "InnerExceptionSource", logException.InnerException.Source);
							AppendLogKeyValue(str, "InnerExceptionMessage", logException.InnerException.Message);
						}
					}

					// In caso di risposta del server si logga l'eventuale errore
					if (soapResponse != null)
					{
						XmlNode node = soapResponse.SelectSingleNode("//faultcode");
						XmlAttribute attr;
						if (node != null)
						{
							node = soapResponse.SelectSingleNode("//detail/error");
							if (node != null)
							{
								attr = node.Attributes["Source"];
								if (attr != null)
									AppendLogKeyValue(str, "ErrorSource", attr.Value);
								attr = node.Attributes["Number"];
								if (attr != null)
									AppendLogKeyValue(str, "ErrorNumber", attr.Value);
								attr = node.Attributes["Description"];
								if (attr != null)
									AppendLogKeyValue(str, "ErrorDescription", attr.Value);
							}
						}
					}

					stream.WriteLine(str.ToString());
					return true;
				}
			}
			catch (Exception ex)
			{
				exception = ex;
				return false;
			}
		}

		/// <summary>
		/// Overload in assenza di logException e soapResponse
		/// </summary>
		/// <param name="logEvent"></param>
		/// <param name="record"></param>
		/// <param name="exception"></param>
		/// <returns></returns>
		public bool Log(LogEvent logEvent, string logMessage, out Exception exception)
		{
			return Log(logEvent, logMessage, null, null, out exception);
		}

		/// <summary>
		/// Overload in assenza di soapResponse
		/// </summary>
		/// <param name="logEvent"></param>
		/// <param name="record"></param>
		/// <param name="exception"></param>
		/// <returns></returns>
		public bool Log(LogEvent logEvent, string logMessage, Exception logException)
		{
			Exception exception;
			return Log(logEvent, logMessage, logException, null, out exception);
		}

		/// <summary>
		/// Overload in assenza di logException
		/// </summary>
		/// <param name="logEvent"></param>
		/// <param name="record"></param>
		/// <param name="exception"></param>
		/// <returns></returns>
		public bool Log(LogEvent logEvent, string logMessage, XmlDocument soapResponse, out Exception exception)
		{
			return Log(logEvent, logMessage, null, soapResponse, out exception);
		}

		/// <summary>
		/// Overload senza gestione dell'eccezione in scrittura del log
		/// </summary>
		/// <param name="logEvent"></param>
		/// <param name="record"></param>
		/// <param name="exception"></param>
		/// <returns></returns>
		public bool Log(LogEvent logEvent, string logMessage)
		{
			Exception exception;
			return Log(logEvent, logMessage, null, null, out exception);
		}

		/// <summary>
		/// Molte delle funzioni della libreria KnosAPI restituiscono un KnosResult
		/// che può contenere più errori e/o warning
		/// Questa funzione prova a farne il dump sul file di log
		/// </summary>
		/// <param name="result"></param>
		public void LogResult(string operation, IKnosResult result)
		{
			int i;
			IKnosResultEntry entry;
			string text = "";
			LogEvent logEvent = LogEvent.Info;
			Exception ex = null;
			if (result.ErrorCount > 0)
			{
				logEvent = LogEvent.Error;
				for (i = 0; i < result.ErrorCount; i++)
				{
					entry = result.GetError(i);
					text += "\r\n\t  Errore: " + entry.Number + " " + entry.Description + " [" + entry.Source + "]";
				}
			}
			if (result.WarningCount > 0)
			{
				logEvent = LogEvent.Warning;
				for (i = 0; i < result.WarningCount; i++)
				{
					entry = result.GetWarning(i);
					text += "\r\n\t  Avviso: " + entry.Number + " " + entry.Description + " [" + entry.Source + "]";
				}
			}
			if (logEvent != LogEvent.Info)
				Log(logEvent, operation +"\r\n\tKnosResult:"+ text);
		}

		/// <summary>
		/// Accoda una coppia key, value alla riga di log
		/// </summary>
		/// <param name="str">Riga a cui accodare la coppia key, value</param>
		/// <param name="key">Il nome della property deve essere un nome valido come identificatore JavaScript. Non si eseguono controlli per ragioni di efficienza.</param>
		/// <param name="value">Il valore è sottoposto ad escape perchè potrebbe essere un testo non noto (ad esempio il messaggio di un'eccezione)</param>
		private void AppendLogKeyValue(StringBuilder str, string key, string value)
		{
			str.Append(", " + key + ": " + value);
		}
	}
}
