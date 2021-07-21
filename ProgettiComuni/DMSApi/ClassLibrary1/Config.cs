using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace DMSConnector
{
	/// <summary>
	/// Classe contenente le impostazioni dello stress test
	/// </summary>
	public class Config
	{
		/// <summary>
		/// URL di KnoS da agganciare
		/// </summary>
		public string BaseUrl;

		/// <summary>
		/// Utente KnoS
		/// </summary>
		public string UserName;

		/// <summary>
		/// Password dell'utente di KnoS
		/// </summary>
		public string Password;

		/// <summary>
		/// Identificativo della tipologia primaria da utilizzare per i test
		/// </summary>
		public int IdClass;

		/// <summary>
		/// Eventuale identificativo della vista da utilizzare in ricerca pubblicazioni
		/// </summary>
		public int IdView;

		/// <summary>
		/// Eventuale identificativo dell'azione da eseguire sulla pubblicazione della tipologia primaria
		/// </summary>
		public int IdAction;

		/// <summary>
		/// Eventuale identificativo della tipologia secondaria da utilizzare per i test
		/// </summary>
		public int LinkageIdClass;

		/// <summary>
		/// Eventuale identificativo dell'attributo elenco pubblicazioni da utilizzare per i test
		/// </summary>
		public int LinkageIdAttr;

		/// <summary>
		/// Numero massimo di pubblicazioni da creare
		/// </summary>
		public int NumberOfObjects;

		/// <summary>
		/// Intervallo di attesa in millisecondi tra i cicli di stress
		/// </summary>
		public int DelayLoopIntervalMilliseconds;

		/// <summary>
		/// Dimensione dei file da allegare in bytes.
		/// I file sono creati con caratteri casuali fino alla dimensione indicata.
		/// </summary>
		public int FileSizeBytes;

		/// <summary>
		/// Numero di file da allegare
		/// </summary>
		public int NumberOfDocuments;

		/// <summary>
		/// Intervallo di attesa tra un upload e l'altro
		/// </summary>
		public int DelayUploadIntervalMilliseconds;

		/// <summary>
		/// Lettura di App.config
		/// </summary>
		/// <returns></returns>
		public bool ReadConfig(out Exception ex)
		{
			ex = new Exception("Errore interno");
			try
			{
				BaseUrl = ConfigurationManager.AppSettings["BaseUrl"];
				UserName = ConfigurationManager.AppSettings["UserName"];
				Password = ConfigurationManager.AppSettings["Password"];
				IdClass = int.Parse(ConfigurationManager.AppSettings["IdClass"]);
				int.TryParse(ConfigurationManager.AppSettings["IdAction"], out IdAction);
				int.TryParse(ConfigurationManager.AppSettings["IdView"], out IdView);
				int.TryParse(ConfigurationManager.AppSettings["LinkageIdClass"], out LinkageIdClass);
				int.TryParse(ConfigurationManager.AppSettings["LinkageIdAttr"], out LinkageIdAttr);
				NumberOfObjects = int.Parse(ConfigurationManager.AppSettings["NumberOfObjects"]);
				DelayLoopIntervalMilliseconds = int.Parse(ConfigurationManager.AppSettings["DelayLoopIntervalMilliseconds"]);
				FileSizeBytes = int.Parse(ConfigurationManager.AppSettings["FileSizeBytes"]);
				NumberOfDocuments = int.Parse(ConfigurationManager.AppSettings["NumberOfDocuments"]);
				DelayUploadIntervalMilliseconds = int.Parse(ConfigurationManager.AppSettings["DelayUploadIntervalMilliseconds"]);
				return true;
			}
			catch (Exception e)
			{
				ex = e;
				return false;
			}
		}
	}
}
