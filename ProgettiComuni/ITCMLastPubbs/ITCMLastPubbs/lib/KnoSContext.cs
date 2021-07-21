using KnosUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Xml;

namespace Christoc.Modules.ITCMLastPubbs.lib
{
	public class KnoSContext
	{
		private bool needLogin = true;
		private string sToken = "";									// security token
		private string KnosUrl;										// URL knoS
		private string adminUserName;								// user amministratore
		private string adminPassword;								// password amministratore
		private string targetUserName;								// target user
		private List<Item> itemsList = new List<Item>();			// lista di pubblicazioni
		private List<UsersItem> usersList = new List<UsersItem>();
		private int pageSize;										// dimensione della pagina dei risultati
		private int timeoutIssue;									// timeout richiesta

		/// <summary>
		/// Costruttore classe
		/// </summary>
		/// <param name="adminUser"></param>
		/// <param name="adminPsw"></param>
		/// <param name="targetUser"></param>
		/// <param name="url"></param>
		/// <param name="resultsPageSize"></param>
		public KnoSContext(string adminUser, string adminPsw, string targetUser, string url, int resultsPageSize)
		{
			try
			{
				adminUserName = adminUser;
				adminPassword = adminPsw;
				targetUserName = targetUser;
				KnosUrl = url;
				pageSize = resultsPageSize;
			}
			catch (Exception e)
			{
				throw new Exception("Errore durante la fase di caricamento file configurazione", e);
			}
		}

		/// <summary>
		/// Metodo per effettuare il login a knoS
		/// </summary>
		/// <returns></returns>
		public bool DoLogin(out string idSubject, out string ticket, out string sToken)
		{
			bool result = true;

			sToken = "";
			idSubject = "-1";
			ticket = "-1";
			UriBuilder uriBuilder = new UriBuilder(KnosUrl);
			uriBuilder.Path = "/knos/system/ClientInfo.asp";
			uriBuilder.Query = "level=1";
			XmlDocument xmlRequest;
			XmlDocument xmlResponse;
			Exception exception;
			if (Utility.HttpRequest(uriBuilder.Uri, null, out xmlResponse, out exception))
			{
				string clientIpAddress;
				if (Utility.GetXmlNodeValue(xmlResponse, "//Ticket", out ticket, out exception))
				{
					if (Utility.GetXmlNodeValue(xmlResponse, "//ClientIpAddress", out clientIpAddress, out exception))
					{
						string key = Utility.HashingSha1(adminUserName + adminPassword);
						string digest = Utility.HashingSha1(adminUserName + clientIpAddress + key + ticket);
						string SecurityToken;

						xmlRequest = Utility.SoapRequestEnvelope();
						Utility.SoapRequestSetParameter(xmlRequest, "Subject", adminUserName);
						Utility.SoapRequestSetParameter(xmlRequest, "TargetSubject", targetUserName);
						Utility.SoapRequestSetParameter(xmlRequest, "Ticket", ticket);
						Utility.SoapRequestSetParameter(xmlRequest, "Digest", digest);
						uriBuilder = new UriBuilder(KnosUrl);
						uriBuilder.Path = "/knos/system/Security_LoginEx.asp";
						if (Utility.HttpRequest(uriBuilder.Uri, xmlRequest, out xmlResponse, out exception))
						{
							if (!Utility.GetXmlNodeValue(xmlResponse, "//SecurityToken", out SecurityToken, out exception))
							{
								result = false;
							}
							else
							{
								Utility.GetXmlNodeValue(xmlResponse, "//IdSubject", out idSubject, out exception);
								sToken = SecurityToken;
							}
						}

						if (exception != null)
						{
							result = false;
						}
					}
				}
			}

			return result;
		}

		object sync = new Object();


		/// <summary>
		/// Metodo per la validazione del login
		/// </summary>
		/// <returns></returns>
		public bool EnsureLogin()
		{
			string idSubject = "";
			string ticket = "";
			var result = true;
			int delay = 10;
			if (needLogin)
			{
				lock (sync)
				{
					int nRetries = 3;
					do
					{
						result = DoLogin(out idSubject, out ticket, out sToken);
						--nRetries;

						if (!result && (nRetries > 0))
						{
							Thread.Sleep(delay * 1000);
						}

					} while ((!result) && (nRetries > 0));
				}

				needLogin = !result;
			}
			if (!result)
				throw new Exception(String.Format("Unable to login with {0}/{1} credentials. ", adminUserName, adminPassword));
			return result;
		}

		/// <summary>
		/// Chiama WS Search_Search
		/// </summary>
		/// <param name="IdClass"></param>
		/// <param name="IdStatus"></param>
		/// <returns>List<Item></returns>
		public List<Item> GetItems(string idObject, string idClass, string searchExpression, string searchAttributes, string sortExpression, int pageNumber, string idSubject, string toolTipField, string idView)
		{
			Thread.Sleep(timeoutIssue);
			EnsureLogin();
			UriBuilder uriBuilder = new UriBuilder(KnosUrl);
			uriBuilder.Path = "knos/system/webservices/Search_Search.asp";
			uriBuilder.Query = "stoken=" + sToken;
			XmlDocument xmlRequest = Utility.SoapRequestEnvelope();
			XmlDocument xmlResponse = new XmlDocument();

			Exception exception;

			if (!String.IsNullOrEmpty(idView))
				Utility.SoapRequestSetParameter(xmlRequest, "IdView", idView);

			Utility.SoapRequestSetParameter(xmlRequest, "Attributes", searchAttributes);
			Utility.SoapRequestSetParameter(xmlRequest, "SearchExpression", searchExpression);
			Utility.SoapRequestSetParameter(xmlRequest, "OrderBy", sortExpression);
			Utility.SoapRequestSetParameter(xmlRequest, "PageSize", pageSize);

			if (idSubject.Length > 0 && idSubject != "-1")
			{
				Utility.SoapRequestSetParameter(xmlRequest, "SearchSubject", idSubject);
			}

			if (pageNumber > 0)
				Utility.SoapRequestSetParameter(xmlRequest, "PageNumber", pageNumber);

			if (Utility.HttpRequest(uriBuilder.Uri, xmlRequest, out xmlResponse, out exception))
			{
				// Cerco campo relativo al ToolTip
				string toolTipAttrName = "";
				if (!String.IsNullOrEmpty(toolTipField))
				{
					var nodeViewConfig = xmlResponse.SelectNodes("//ViewConfig");
					if (nodeViewConfig != null && nodeViewConfig.Item(0) != null)
					{
						var content = nodeViewConfig.Item(0);
						if (content != null && !String.IsNullOrEmpty(content.InnerText))
						{
							string[] arr = content.InnerText.Replace("'", "").Split(',');
							var i = Array.IndexOf(arr, toolTipField);
							if (i > 1)
								toolTipAttrName = arr[i - 1];
						}
					}
				}
				
				var nodeData = xmlResponse.SelectSingleNode("//Data");


				var dataList = nodeData.SelectNodes("row");
				if (dataList.Count > 0)
				{
					foreach (XmlNode xn in dataList)
					{
						try
						{
							Item item = Item.CreateItemFromXmlResponse(xmlResponse, xn, toolTipAttrName);
							itemsList.Add(item);
						}
						catch
						{
						}
					}
				}
			}
			else
			{
				if (exception != null)
				{
					throw exception;
				}
			}

			return itemsList;
		}

		public List<UsersItem> GetUsers(string searchExpression)
		{
			Thread.Sleep(timeoutIssue);
			EnsureLogin();
			UriBuilder uriBuilder = new UriBuilder(KnosUrl);
			uriBuilder.Path = "knos/system/webservices/Subjects_Items.asp";
			uriBuilder.Query = "stoken=" + sToken;
			XmlDocument xmlRequest = Utility.SoapRequestEnvelope();
			XmlDocument xmlResponse = new XmlDocument();

			Exception exception;

			if (!String.IsNullOrEmpty(searchExpression))
				Utility.SoapRequestSetParameter(xmlRequest, "SearchExpression", searchExpression);

			if (Utility.HttpRequest(uriBuilder.Uri, xmlRequest, out xmlResponse, out exception))
			{
				var nodeData = xmlResponse.SelectSingleNode("//Data");
				var dataList = nodeData.SelectNodes("row");
				if (dataList.Count > 0)
				{
					foreach (XmlNode xn in dataList)
					{
						try
						{
							UsersItem userItem = UsersItem.CreateItemFromXmlResponse(xmlResponse, xn);
							usersList.Add(userItem);
						}
						catch
						{
						}
					}
				}
			}
			else
			{
				if (exception != null)
				{
					throw exception;
				}
			}

			return usersList;
		}
	}
}