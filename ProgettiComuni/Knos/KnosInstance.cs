using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Knos.API;
using Knos.API.COM;
using Knos.API.NET;

namespace Knos
{
	public static class KnosInstance
	{	

		#region //----------------------------------------- KnoS ------------------------------------------------------

		/// <summary>
		/// Handler per la libreria KnosInstance
		/// </summary>
		public static LibraryHandler API = new LibraryHandler();
		
		/// <summary>
		/// Contesto KnoS
		/// </summary>
		public static IKnosContext Context = new KnosContext();
		
		/// <summary>
		/// Client KnoS
		/// </summary>
		public static IKnosClient Client = null;

		/// <summary>
		/// Indirizzo del sito KnoS di default
		/// </summary>
		public static string DefaultSite = "";

		/// <summary>
		/// Utente applicativo da utilizzare per il login
		/// </summary>
		public static string ApplicationUser = "administrator";

		/// <summary>
		/// Password dell'utente applicativo da utilizzare per il login
		/// </summary>
		public static string ApplicationPassword = "nottorrius";

		/// <summary>
		/// Inizializzazione KnoS
		/// </summary>
		public static IKnosResult Initialization()
		{
			IKnosResult result = new KnosResult();

			// Inizializzazione libreria
			KnosInstance.API = new LibraryHandler();

			// Eventuale controllo di versione
			string version = KnosInstance.API.Version;

			// Aggancio del contesto di Internet Explorer
			// da commentare se si vuole un contesto autonomo
			result = Context.AttachToInternetExplorer();

			return result;
		}

		/// <summary>
		/// Chiusura di KnoS con eventuale logout
		/// </summary>
		public static void Finalization()
		{
			// Se il KnosClient è instanziato si valuta se eseguire il logout
			if (KnosInstance.Client != null)
			{
				// Si valuta se il contesto è agganciato ad Internet Explorer
				if (KnosInstance.Context.IsAttachedToInternetExplorer)
				{
					// Normalmente non si esegue il logout per non buttare fuori l'utente
					// su Internet Explorer con la produzione di errori e richieste di login
					// per eventuali finestre del browser già aperte	
				}
				else
				{
					// Se il contesto è sganciato da Internet Explorer va sempre eseguito il logout 
					// per non saturare le slot di licenza
					KnosInstance.Client.Logout();
				}
			}
		}

		/// <summary>
		/// Apertura del sito KnoS di default
		/// </summary>
		/// <returns></returns>
		public static IKnosResult Open()
		{
			return KnosInstance.Open(KnosInstance.DefaultSite);
		}

		/// <summary>
		/// Apertura del sito KnoS indicato
		/// </summary>
		/// <param name="knosUrl">Url di KnosInstance da aprire</param>
		/// <returns></returns>
		public static IKnosResult Open(string knosUrl)
		{
			IKnosResult result = new KnosResult();

			// Prima di rilasciare un client esistente bisogna decidere se eseguire 
			// o meno il logout dell'eventuale utente corrente
			if (KnosInstance.Client != null)
			{
				// Le politiche di rilascio sono lasciate all'utilizzatore delle API
				// In generale conviene chiedersi se si ha a che fare o meno con lo stesso server
				if (KnosInstance.Client.KnosBaseUrlMatch(knosUrl))
				{
					// Se si resta sullo stesso sito non serve eseguire il logout perchè la slot resta la stessa
				}
				else
				{
					// Se si rilascia un sito diverso da quello nuovo si può decidere se eseguire o meno
					// il logout in base al fatto che il contesto sia agganciato o meno a Internet Explorer
					if (KnosInstance.Client.KnosContext.IsAttachedToInternetExplorer)
					{
						// Se si esegue il logout dell'utente corrente anche in Internet Explorer bisognerà riloggarsi
					}
					else
					{
						// Nel caso di contesto autonomo è buona norma liberare la slot
						KnosInstance.Client.Logout();
					}
				}
			}
			result = KnosInstance.Context.GetKnosClient(knosUrl, out KnosInstance.Client);  
			return result;
		}

		/// <summary>
		/// Login applicativo sul sito corrente con ApplicationUser
		/// </summary>
		/// <returns></returns>
		public static IKnosResult Login()
		{
			return KnosInstance.Login(KnosInstance.ApplicationUser);
		}
		/// <summary>
		/// Login applicativo sul sito corrente con UserName specificato
		/// </summary>
		/// <param name="userName">Nome dell'utente con il quale si vuole eseguire il login</param>
		/// <returns></returns>
		public static IKnosResult Login(string userName)
		{
			IKnosResult result = new KnosResult();
			int idSubject = 0;
			if (KnosInstance.Client == null)
				result.AddReturnCode(EnumKnosReturnCode.Error_InvalidKnosClient, "KnosInstance.Login()");
			else
				result = KnosInstance.Client.LoginWithAdministratorCredential(KnosInstance.ApplicationUser, KnosInstance.ApplicationPassword, ref idSubject, ref userName);
			return result;
		}

		/// <summary>
		/// Login applicativo sul sito corrente con IdSubject specificato
		/// </summary>
		/// <param name="idSubject">IdSubject dell'utente con il quale si vuole eseguire il login</param>
		/// <returns></returns>
		public static IKnosResult Login(int idSubject)
		{
			string userName = "";
			return KnosInstance.Client.LoginWithAdministratorCredential(KnosInstance.ApplicationUser, KnosInstance.ApplicationPassword, ref idSubject, ref userName);
		}


        public class Attributo
        {
            int idAttr;

            public int IdAttr
            {
                get { return idAttr; }
                set { idAttr = value; }
            }

            string description;

            public string Description
            {
                get { return description; }
                set { description = value; }
            }

            public Attributo(int idAttr, string description) { this.idAttr = idAttr; this.description = description; }
        }

        public class Tipologia
        {
            private Dictionary<String, Attributo> attributi = new Dictionary<string, Attributo>();

            public Dictionary<String, Attributo> Attributi
            {
                get { return attributi; }
                set { attributi = value; }
            }

            int idClass;

            public int IdClass
            {
                get { return idClass; }
                set { idClass= value; }
            }
            string description;

            public string Description
            {
                get { return description; }
                set { description = value; }
            }

            public Tipologia(int idClass, string description)
            {
                this.idClass = idClass;
                this.description = description;
            }
            
        }
		
		#endregion
	}
}
