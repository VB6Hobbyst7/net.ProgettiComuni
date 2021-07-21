using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Knos.API.COM;
using Knos.API.NET;

namespace DMSConnector
{
	

	public class DMSEngine
    {
		#region //----------------------------------------- GLOBALS ---------------------------------------------------

		/// <summary>
		/// Handler della libreria per le impostazioni globali come la lingua, ecc.
		/// Le impostazioni valgono per la libreria KnosAPI e quindi per qualunque contesto e client istanziato
		/// </summary>
		public static Knos.API.LibraryHandler KnosAPILibrary;

		/// <summary>
		/// KnosContext dichiarato globalmente
		/// E' possibile averne più di uno ma normalmente non è necessario a meno di non dover lavorare siti di KnoS che usano proxy diversi.
		/// Il contesto memorizza i token di sicurezza come cookie e consente di eseguire la login una volta sola.
		/// </summary>
		public static IKnosContext KnosContext;

		/// <summary>
		/// KnosClient dichiarato globalmente
		/// Il KnosClient memorizza l'url del sito a cui interfacciarsi 
		/// e quindi occorre utilizzarne uno diverso per ogni sito di KnoS a cui ci si vuole collegare.
		/// In questo esempio viene utilizzato un solo sito e poichè l'oggetto è utilizzato spesso lo si
		/// è chiamato Knos invece di KnosClient semplicemente per scrivere meno caratteri.
		/// </summary>
		public static IKnosClient Knos;

		/// <summary>
		/// Parametri di configurazione per il test
		/// </summary>
		public static Config Config;

		/// <summary>
		/// Istanza della classe logger
		/// <remarks>
		/// Nei log per convenzione:
		/// "t" sta per timastamp ed è valorizzato internamente con la data e ora al millesimo di secondo
		/// "e" sta per evento e può valere "Info", "Warning", "Error"
		/// "o" sta per operation ed il valore è lasciato libero
		/// Ogni altro voce del record di log è libera sia nel nome che nel valore
		/// </remarks>
		/// </summary>
		public static Logger Logger;
		#endregion

		#region ----------------------------- Inizializzazione libreria, contesto e client ------------------------

		// I metodi delle KnosAPI normalmente non sollevano eccezioni e se non vanno a buon fine restituiscono 
		// un risultato booleano a false o di tipo IKnosResult.
		// Quindi per sapere se una chiamata è andata a buon fine bisogna controllare il risultato restituito
		// Eventuali eccezioni possono comunque verificarsi per errori interni
		IKnosResult result;

		//// La prima cosa da fare è inizializzare la libreria KnosAPI e questo non produce mai errori
		//// se la libreria è referenziata correttamente
		//InizializzazioneLibreria();

		//// Poi tocca al contesto KnosContext
		//if (!InizializzazioneContesto())
		//	return;

		//// Infine si può creare il KnosClient client per il sito KnoS con cui si vuole interagire
		//if (!InizializzazioneClient())
		//	return;

		#endregion -------------------------- Inizializzazione libreria, contesto e client ------------------------




		/// <summary>
		/// Inizializzazione libreria
		/// </summary>
		public static void InizializzazioneLibreria()
		{

			// Alcune funzioni restutuisco le eventuali eccezioni come parametro di out
			Exception ex;

			#region ----------------------------- Preparazione ambiente -----------------------------------------------

			// Lettura di App.Config con i parametri di test
			// Se la lettura dei dati di configurazione non va a buon fine non si può proseguire
			Config = new Config();
			//if (!Config.ReadConfig(out ex))
			//{
			//	if (ex == null)
			//		Console.WriteLine("Errore in lettura dei dati di configurazione");
			//	else
			//		Console.WriteLine("Errore in lettura dei dati di configurazione: " + ex.Message);
			//	return;
			//}

			// Creazione del file di log
			// Si registra su log l'avvio del programma se la registrazione non va a buon fine si esce
			Logger = new Logger();
			if (!Logger.Log(Logger.LogEvent.Info, "Avvio programma", out ex))
			{
				if (ex == null)
					Console.WriteLine("Errore in scrittura sul file di log");
				else
					Console.WriteLine("Errore in scrittura sul file di log: " + ex.Message);
				return;
			}

            #endregion

            Console.WriteLine("\nInizializzazioneLibreria()");

			// L'inizializzazione è una semplice istanza del LibraryHandler
			KnosAPILibrary = new Knos.API.LibraryHandler();

			// Si può visualizzare la versione della libreria
			Console.WriteLine("\tKnosAPI version: " + KnosAPILibrary.Version);

			// Occorre indicare la lingua in cui si vogliono i messaggi
			// Al momento sono disponibili solo "it" e "en"
			KnosAPILibrary.SetLanguage("it");

			// Si puo' visualizzare la lingua dell'interfaccia corrente
			Console.WriteLine("\tInterface language: " + KnosAPILibrary.GetCurrentLanguageId() + " - " + KnosAPILibrary.GetCurrentLanguageName());

			/*
			// Volendo si può anche impostare un'altra lingua di interfaccia
			// Nativamente sono presenti solo italiano "it" e l'inglese "en" ma l'utente può aggiungerne altre
			// semplicemente inserendo i file con le traduzioni nella sottocartella \Resource, ad esempio
			// KnosAPI-de.xaml, KnosAPI-fr.xaml, ecc.
			string interfaceLanguage = "en";
			KnosAPILibrary.SetLanguage(interfaceLanguage);
			Console.WriteLine("\tNuova lingua interfaccia: " + KnosAPILibrary.GetCurrentLanguageId() + " - " + KnosAPILibrary.GetCurrentLanguageName());

			// Se si imposta una lingua non presente viene mantenuta la lingua corrente
			interfaceLanguage = "fr";
			string currentLanguage = KnosAPILibrary.SetLanguage(interfaceLanguage);
			if (currentLanguage != interfaceLanguage)
				Console.WriteLine("\tLingua non supportata: " + interfaceLanguage);
			else
				Console.WriteLine("\tNuova lingua interfaccia: " + KnosAPILibrary.GetCurrentLanguageId() + " - " + KnosAPILibrary.GetCurrentLanguageName());
			*/
		}

		/// <summary>
		/// Inizializzazione contesto
		/// </summary>
		/// <returns></returns>
		public static bool InizializzazioneContesto()
		{
			Console.WriteLine("\nInizializzazioneContesto()");

			// Un contesto si crea in modo banale
			KnosContext = new KnosContext();

			// Si tratta di decidere poi se agganciare o meno il contesto ad Internet Explorer
			bool bAttachToInternetExplorer = false;
			if (bAttachToInternetExplorer)
			{
				IKnosResult result;
				result = KnosContext.AttachToInternetExplorer();
				Logger.LogResult("InizializzazioneContesto()", result);
				if (result.HasErrors)
					return false;
			}

			if (KnosContext.IsAttachedToInternetExplorer)
				Console.WriteLine("\tContesto agganciato a Internet Explorer");
			else
			{
				Console.WriteLine("\tContesto indipendente da Internet Explorer");

				// Se possibile è preferibile non utilizare il proxy
				//KnosContext.Proxy.ProxyType = EnumKnosProxyType.NoProxy;

				// Se il proxy è obbligatorio si può usare quello di Internet Explorer
				// KnosContext.Proxy.ProxyType = EnumKnosProxyType.SystemProxy;

				// E' comunque possibile indicare tutti i parametri del proxy
				//KnosContext.Proxy.ProxyType = EnumKnosProxyType.CustomProxy;
				//KnosContext.Proxy.SetCustomProxy("proxy1", 8080);
				//KnosContext.Proxy.SetCustomProxyCredential("glevo", "pswd", "teamsystem");
			}

			return true;
		}

		/// <summary>
		/// Inizializzazione client
		/// </summary>
		public static bool InizializzazioneClient()
		{
			Console.WriteLine("\nInizializzazioneClient()");

			IKnosResult result;

			// Prima di rilasciare un client esistente bisogna decidere se eseguire 
			// o meno il logout dell'eventuale utente corrente
			if (Knos != null)
			{
				// Le politiche di rilascio sono lasciate all'utilizzatore delle API
				// In generale conviene chiedersi se si ha a che fare o meno con lo stesso server
				if (Knos.KnosBaseUrlMatch(Config.BaseUrl))
				{
					// Se si resta sullo stesso sito non serve eseguire il logout perchè la slot resta la stessa
				}
				else
				{
					// Se si rilascia un sito diverso da quello nuovo si può decidere se eseguire o meno
					// il logout in base al fatto che il contesto sia agganciato o meno a Internet Explorer
					if (Knos.KnosContext.IsAttachedToInternetExplorer)
					{
						// Se si esegue il logout dell'utente corrente anche in Internet Explorer bisognerà riloggarsi
					}
					else
					{
						Knos.Logout();
					}
				}
			}

			result = KnosContext.GetKnosClient(Config.BaseUrl, out Knos);
			Logger.LogResult("KnosContext.GetKnosClient()", result);
			if (result.HasErrors)
				return false;

			// Visualizzazione dei dati di connessione
			Console.WriteLine("\tServer KnoS: " + Knos.KnosBaseUrl);

			// Visualizzazione dell'eventuale proxy utilizzato
			string proxyHost;
			EnumKnosProxyType proxyType;
			int proxyPort;
			result = KnosContext.Proxy.GetCurrentProxy(Knos.KnosBaseUrl, out proxyType, out proxyHost, out proxyPort);
			Logger.LogResult("KnosContext.Proxy.GetCurrentProxy()", result);
			if (!string.IsNullOrWhiteSpace(proxyHost))
				Console.WriteLine("\tProxy utilizzato: " + proxyHost + ":" + proxyPort);

			// E' possibile eseguire esercitare diverse opzioni su un KnosClient
			// Normalmente le opzioni di default vanno bene ma volendo si possono modificare
			//
			// Ad esempio si può decidere se evitare la login quando un utente risulta già autenticato
			Knos.AvoidLoginForAlreadyLoggedUser = false;    // L'impostazione di default è true ma meglio eseguire un login completo
															//
															// Modificare la dimensione di default delle pagine di ricerca pubblicazioni
			Knos.Default_KnosObjectSelector_PageSize = 10;  // impostazione di default
															//
															// Limitare la dimensione massima dei file in upload
															// NOTA: non si può superare comunque il limite eventualmente impostato in ICKnoS.ini o in IIS
			Knos.Limit_Upload_MaxFileSize_Bytes = 0;        // impostazione di default
															//
															//...

			return true;
		}

		/// <summary>
		/// Login utente
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public static bool Login(string userName, string password)
		{
			Console.WriteLine("\nLogin()");
			IKnosResult result;

			// Se si vuole sapere qual'è l'utente corrente lo si può ottenere come segue 
			int currentIdSubject;
			string currentUserName;
			Knos.CheckCurrentUser(out currentIdSubject, out currentUserName);
			Console.WriteLine("\tUtente corrente: " + currentIdSubject + " - " + currentUserName);

			// Esistono tre diverse modalità di login in KnoS
			// - login applicativo
			// - login con autenticazione integrata
			// - login con utente e password

			// Con il login applicativo è sufficiente avere una login di un amministratore KnoS per poter eseguire
			// un login con un qualunque utente. Si evita in questo modo di dover memorizzare le password di tutti
			// gli utenti di KnoS. Per l'abbinamento ad eventuali utenti esterni basta associare all'utente esterno
			// l'IdSubject o il SubjectName dell'utente KnoS
			// Qui come esempio si indica come targetUserName lo stesso utente del test (che deve essere un amministratore) 
			string applicationUserName = userName;
			string applicationPassword = password;
			int targetIdSubject = 0;
			string targetUserName = userName;

			result = Knos.LoginWithAdministratorCredential(applicationUserName, applicationPassword, ref targetIdSubject, ref targetUserName);
			if (result.NoErrors)
				Console.WriteLine("\tLogin eseguito: " + targetIdSubject + " - " + targetUserName);
			// Utente corrente
			Knos.CheckCurrentUser(out currentIdSubject, out currentUserName);
			Console.WriteLine("\tUtente corrente: " + currentIdSubject + " - " + currentUserName);

			// L'autenticazione integrata prevede che l'utente di windows che utilizza la libreria KnosAPI
			// sia mappato in un utente KnoS, ad esempio Administrator con teamsystem\glevo
			result = Knos.LoginWithIntegratedAuthentication(out currentIdSubject, out currentUserName);
			if (result.NoErrors)
				Console.WriteLine("\tLogin eseguito: " + targetIdSubject + " - " + targetUserName);
			// Utente corrente
			Knos.CheckCurrentUser(out currentIdSubject, out currentUserName);
			Console.WriteLine("\tUtente corrente: " + currentIdSubject + " - " + currentUserName);

			// Da qui in poi proseguiamo con l'utente applicativo con la login tradizionale a titolo di esempio
			result = Knos.Login(userName, password, out targetIdSubject);
			Logger.LogResult("Knos.Login()", result);
			if (result.HasErrors)
				return false;

			// Utente corrente
			Knos.CheckCurrentUser(out currentIdSubject, out currentUserName);
			string str = "Utente corrente: " + currentIdSubject + " - " + currentUserName;
			Logger.Log(Logger.LogEvent.Info, "Knos.Login() - " + str);
			Console.WriteLine(str);
			return true;
		}

		/// <summary>
		/// Elenco utenti KnoS
		/// </summary>
		static bool ElencoUtenti()
		{
			Console.WriteLine("\nElencoUtenti()");
			IKnosResult result;

			// Per poter configurare il mapping di utenti esterni su utenti interni 
			// può essere utile avere l'elenco degli utenti KnoS
			// In generale quando si devono selezionare dati da KnoS si utilizzano le classi Selector
			// NOTA: tutti gli oggetti utilizzati per interagire con Knos non sono istanziati direttamente
			// ma vengono creati dal KnosClient che provvede a propagare le informazioni utili in modo trasparente
			IKnosSubjectSelector subjectSelector = Knos.CreateKnosSubjectSelector();

			// Ogni selettore ha delle proprietà o dei metodi Select che semplificano i filtraggi sulle informazioni
			// Ad esempio IKnosSubjectSelector senza filtri restituisce utenti, gruppi e ruoli
			// Se ad esempio si vogliono solo gli utenti:
			subjectSelector.SelectUser = true;
			subjectSelector.SelectRole = false;
			subjectSelector.SelectGroup = true;

			// E' possibile avere i dati paginati o meno, in questo caso recuperiamo tutti gli utenti
			// La paginazione verrà illustrata con la ricerca sulle pubblicazioni
			result = subjectSelector.GetAll();
			if (result.HasErrors)
			{
				Logger.LogResult("ElencoUtenti() - subjectSelector.GetAll()", result);
				return false;
			}

			// Elenco utenti restituiti
			IKnosSubject subject;
			string str = string.Format("{0, 10} | {1, 12} | {2, 40} | {3,40}", "IdSubject", "FlagSubject", "Subject", "DomainUser");
			Console.WriteLine(str);
			for (int i = 0; i < subjectSelector.ItemCount; i++)
			{
				subject = subjectSelector.GetItem(i);
				str = string.Format("{0, 10} | {1, 12} | {2, 40} | {3,40}",
					subject.IdSubject.ToString(),
					subject.FlagSubject.ToString(),
					subject.Subject.ToString(),
					subject.DomainUser.ToString());
				Console.WriteLine(str);
			}

			// E' anche possibile ottenere la trasformazione dei dati restituiti in Ado Recordset
			// ma SOLO dopo che sono stati ottenuti con GetPage o GetAll
			//ADODB.Recordset rs;
			//result = subjectSelector.GetAdoRecordset(out rs);
			if (result.HasErrors)
			{
				Logger.LogResult("ElencoUtenti() - subjectSelector.GetAdoRecordset", result);
				return false;
			}
			return true;
		}




		//#region ----------------------------- Esempio di recupero degli attributi ---------------------------------

		//// Esempio di recupero degli attributi della tipologia primaria
		//IKnosAttributeSelector attrSelector = AttributiTipologia(Config.IdClass);

		//// Se indicata si recuperano anche quelli della tipologia collegata
		//IKnosAttributeSelector linkageAttrSelector = AttributiTipologia(Config.LinkageIdClass);

		//#endregion -------------------------- Esempio di recupero degli attributi ---------------------------------



		/// <summary>
		/// Elenco attributi della tipologia indicata
		/// </summary>
		public static IKnosAttributeSelector AttributiTipologia(int idClass)
		{
			Console.WriteLine("\nElencoAttributiTipologia(" + idClass.ToString() + ")");
			IKnosResult result;

			IKnosAttributeSelector attrSelector = Knos.CreateKnosAttributeSelector();

			// Per recuperare solo gli attributi di una specifica tipologia si utilizza
			attrSelector.SelectIdClass = idClass;

			// L'attributo Title (Oggetto) è speciale ed è presente in tutte le tipologie
			// La configurazione della tipologia indica se è usato o meno
			// Si può indicare se lo si vuole sempre, mai o solo se utilizzato nella tipologia (Auto)
			attrSelector.SelectTitle = EnumKnosFlagTitle.Auto;

			// Si recuperano tutti gli attributi della tipologia indicata
			result = attrSelector.GetAll();
			Logger.LogResult("attrSelector.GetAll()", result);

			// Si visualizza solo il numero totale degli attributi
			// ATTENZIONE: i record recuperati sono solo relativi agli attributi quindi Title non è mai presente
			Console.WriteLine("\tTotale attributi tipologia IdClass=" + idClass.ToString() + ": " + attrSelector.RecordCount.ToString());
			// Se si vuole anche contare Title quando presente bisogna rilevare il conteggio degli elementi ottenuti:
			Console.WriteLine("\tTotale attributi tipologia IdClass=" + idClass.ToString() + " (compreso Title): " + attrSelector.ItemCount.ToString());

			return attrSelector;
		}



		/// <summary>
		/// Creazione di una nuova pubblicazione
		/// </summary>
		public static int CreaPubblicazioneTest(int idClass, IKnosAttributeSelector attributeSelector)
		{
			IKnosResult result;

			// Tutte le operazioni di editing sulle pubblicazioni vengono svolte da KnosObjectMaker
			IKnosObjectMaker objectMaker;
			objectMaker = (KnosObjectMaker)Knos.CreateKnosObjectMaker();

			// Se un object maker è utilizzato più volte bisogna ricordarsi di ripulire i valori
			// assegnati agli attributi prima di procedere ad una nuova modifica o creazione.
			// Se poi si decide di resettare comunque va bene
			objectMaker.Reset();

			// Per evitare di dover testare gli errori ad ogni operazione di valorizzazione degli attributi
			// KnosObjectMaker ha un GlobalResult che fa da accumulatore di errori e warning.
			// Si ripulisce GlobalResult prima di cominciare e si controlla alla fine se ci sono errori
			// Se prima si è eseguita una Reset GlobalResult è già azzerato
			objectMaker.GlobalResult.ClearAll();

			//-----------
			// Tipologia
			//-----------
			objectMaker.IdClass = idClass;

			//-----------
			// Attributi 
			// ----------
			int i;
			IKnosAttribute attr;
			object attrValue;
			for (i = 0; i < attributeSelector.ItemCount; i++)
			{
				attr = attributeSelector.GetItem(i);
				switch (attr.DataType)
				{
					case EnumKnosDataType.BooleanType:
						// Per i booleani si può passare un oggetto booleano come esempio si imposta a true
						attrValue = true;
						objectMaker.SetAttrValue(attr.ColumnName, attrValue, attr.DataType);
						// oppure una stringa case insensitive dove "1" o "true" significano true
						// objectMaker.SetAttrValue(attr.ColumnName, "1", attr.DataType);
						// objectMaker.SetAttrValue(attr.ColumnName, "True", attr.DataType);
						// objectMaker.SetAttrValue(attr.ColumnName, "true", attr.DataType);
						break;

					case EnumKnosDataType.DateTimeType:
						// Si puo passare un oggetto Datetime
						attrValue = DateTime.Now;
						objectMaker.SetAttrValue(attr.ColumnName, attrValue, attr.DataType);
						// oppure una stringa che deve essere rigorosamente in formato yyyy-mm-dd hh:mm:ss" (ora locale)
						// string stringValue = DateTime.Now.ToString("yyyy-mm-dd hh:mm:ss", DateTimeFormatInfo.InvariantInfo);
						// objectMaker.SetAttrValue(attr.ColumnName, stringValue, attr.DataType);
						break;

					case EnumKnosDataType.DecimalType:
						// KnoS di default gestisce decimali in formato decimal(19, 5)
						// quindi con precisione a 19 cifre di cui 5 decimali
						// Si può passare un numero
						attrValue = 1234567890.12345;
						objectMaker.SetAttrValue(attr.ColumnName, attrValue, attr.DataType);
						// oppure una stringa dove come punto decimale si accettano sia "." che ","
						// non è lecito passare numeri con il separatore delle migliaia 1,000.5 o 1.000,5 sono entrambi non validi
						// objectMaker.SetAttrValue(attr.ColumnName, "1000.5", attr.DataType);
						// objectMaker.SetAttrValue(attr.ColumnName, "1000,5", attr.DataType);
						break;

					case EnumKnosDataType.IntegerType:
						// KnoS gestisce interi a 32 bit con segno
						// Si può passare un numero
						attrValue = 1;
						objectMaker.SetAttrValue(attr.ColumnName, attrValue, attr.DataType);
						// oppure una stringa che rappresenti un intero con eventuale segno
						// objectMaker.SetAttrValue(attr.ColumnName, "1", attr.DataType);
						// objectMaker.SetAttrValue(attr.ColumnName, "-1", attr.DataType);
						break;

					case EnumKnosDataType.ShortTextType:
						// Gli attributi di tipo testo breve possono essere al massimo lunghi 200 cartteri
						// Se la stringa passata è più lunga viene troncata
						objectMaker.SetAttrValue(attr.ColumnName, "Stress Test", attr.DataType);
						break;

					case EnumKnosDataType.LongTextType:
						// Gli attributi di tipo lungo sono nvarchar(max) e non hanno limitazione in lunghezza
						// se non quelle del sistema
						objectMaker.SetAttrValue(attr.ColumnName, "Stress Test\nStress Test", attr.DataType);
						break;

					case EnumKnosDataType.EnumListType:
						// Per gli elenchi la valorizzazione è un po' più complicata e va fatta con un multivalue editor
						IKnosMultivalueEditor enumEditor = Knos.CreateKnosMultivalueEditor();
						// I valori sono assegnati utilizzando il valore IdAttrValue contenuto nella tabella Attr_Values
						// E' possibile recuperare i valori con lo specifico selettore
						IKnosEnumSelector enumSelector = Knos.CreateKnosEnumSelector();
						enumSelector.SelectIdAttr = attr.IdAttr;
						enumSelector.GetAll();
						if (!(enumSelector.ItemCount > 0))
							continue;
						// Il primo valore è quello che verrà assegnato come test
						int idAttrValueOK = enumSelector.GetItem(0).IdAttrValue;

						// Si inserisce il valore nell'editor
						// Esempi delle operazioni che si possono fare sull'editor sono in EnumKnosDataType.ObjectListType
						enumEditor.AddValue(idAttrValueOK);

						// La valorizzazione dell'attributo è fatta alla fine delle operazioni di editing
						// passando l'editor come valore
						objectMaker.SetAttrValue(attr.ColumnName, enumEditor, attr.DataType);
						break;

					case EnumKnosDataType.ObjectListType:
						// Per gli elenchi la valorizzazione è un po' più complicata e va fatta con un multivalue editor
						IKnosMultivalueEditor linkageEditor = Knos.CreateKnosMultivalueEditor();

						// Ecco un po' di esempi di quello che si può fare con l'editor
						// per il valore in questione, qui i valori 1,2,3,4 sono solo di esempio
						// Eventuali operazioni di editing vengono cumulate e al server arriva solo il risultato
						// Questo può essere utile per semplificare la gestione di editing da interfaccia
						// Internamente l'editor gestisce due code ed un flag che indica se i valori presistenti vanno eliminati
						// le due code contengono gli elementi da aggiungere e da togliere ai valori esistenti
						int[] idAttrValueList1 = { 1, 2 };
						int[] idAttrValueList2 = { 3, 4 };
						linkageEditor.AddValue(1); // Add={1} Remove={} FlagClear=false
						linkageEditor.AddValues(idAttrValueList1); // Add={1,2} Remove={} FlagClear=false
						linkageEditor.ReplaceValues(idAttrValueList1, idAttrValueList2); // Add={3,4} Remove={1,2} FlagClear=false
						linkageEditor.RemoveValue(3); // Add={4} Remove={1,2,3} FlagClear=false
						linkageEditor.RemoveValues(idAttrValueList2); // Add={} Remove={1,2,3,4} FlagClear=false

						// ATTENZIONE: quando si usano i metodi con All si modifica il FlagClear
						linkageEditor.ReplaceAllWithValue(1); // Add={1} Remove={} FlagClear=true

						// A questo punto se si esegue il salvataggio l'effetto sarebbe di eliminare tutti i valori preesistenti
						// Per riportare il FlagClear a false si deve richiamare il metodo ResetUpdateInfo()
						linkageEditor.ResetUpdateInfo(); // Add={1} Remove={} FlagClear=false

						// Per azzerare tutte le modifiche e ricominciare da capo invece vanno richiamati in sequenza
						linkageEditor.RemoveAllValues(); // Add={} Remove={} FlagClear=true
						linkageEditor.ResetUpdateInfo(); // Add={} Remove={} FlagClear=false

						// La valorizzazione va fatta con un IdObject valido, altrimenti il valore è ignorato
						IKnosObjectSelector knosObjectSelector = Knos.CreateKnosObjectSelector();
						// Si collega alla pubblicazione più vecchia
						knosObjectSelector.GetPage(1);
						knosObjectSelector.OrderBy = "IdObject ASC";
						if (!(knosObjectSelector.ItemCount > 0))
							continue;
						linkageEditor.AddValue(knosObjectSelector.GetItem(0).IdObject);

						// La valorizzazione dell'attributo è fatta alla fine delle operazioni di editing
						// passando l'editor come valore
						objectMaker.SetAttrValue(attr.ColumnName, linkageEditor, attr.DataType);
						break;
				}
			}

			//--------------------
			// Riferimenti (Link)
			//--------------------
			// Ci sono tre tipi di riferimenti
			// - url
			// - pubblicazioni 
			// - documenti di pubblicazioni

			// Occorre utilizzare un oggetto KnosLink, ecco il caso di un url
			IKnosLink link = Knos.CreateKnosLink();
			link.Url = "http://www.google.it";
			link.LinkDescr = "Link esterno";
			// Per aggiungere il link si utilizza il LinkEditor perfettamente analogo a quello descritto in EnumKnosDataType.ObjectListType
			objectMaker.LinkEditor.AddValue(link);

			// Per i link a pubblicazioni si deve indicare l'IdObject della pubblicazione ed opzionalmente la descrizione
			// NOTA: in fase di salvataggio eventuali link autoreferenzianti o a pubblicazioni inesistenti 
			// o già presenti vengono scartati senza sollevare errori o warning.
			// ATTENZIONE che non si può riutilizzare l'oggetto precedente ma ne va creato un'altro
			link = Knos.CreateKnosLink();
			link.IdObjectTo = 554;
			link.LinkDescr = "Link interno";
			objectMaker.LinkEditor.AddValue(link);

			// Per i link a documenti va indicato l'IdDoc del documento e l'IdObject della pubblicazione che lo contiene
			link = Knos.CreateKnosLink();
			link.IdObjectTo = 554;
			link.IdDoc = 1;
			link.LinkDescr = "Link a documento";
			objectMaker.LinkEditor.AddValue(link);

			//-----------
			// Documenti
			//-----------
			// In fase di creazione di una pubblicazione si possono anche inserire dei documenti
			// il codice che descrive come farlo è in UploadSincrono

			//-------------
			// Destinatari
			//-------------
			// L'impostazione dei destinatari è fatta con il RecipientEditor 
			// che funziona allo stesso modo del LinkEditor solo che opera su KnosRecipient
			// Il parametro da passare è 
			IKnosRecipient knosRecipient = Knos.CreateKnosRecipient();

			// Si può indicare il destinatario solo per IdSubject
			knosRecipient.IdSubject = 1; // Ruolo Administrators
			objectMaker.RecipientEditor.AddValue(knosRecipient);

			// Per indicarlo per nome bisogna recuperare l'IdSubject con una ricerca
			IKnosSubjectSelector subjectSelector = Knos.CreateKnosSubjectSelector();
			subjectSelector.SearchExpression = "Subject = '" + Config.UserName + "'";
			subjectSelector.GetPage(1);
			if (subjectSelector.ItemCount == 1)
			{
				// Non si può riutilizzare l'oggetto precedente
				knosRecipient = Knos.CreateKnosRecipient();
				knosRecipient.IdSubject = subjectSelector.GetItem(0).IdSubject;
				objectMaker.RecipientEditor.AddValue(knosRecipient);
			}


			// Si controlla l'eventuale presenza di errori 
			result = objectMaker.GlobalResult;
			Logger.LogResult("CreaPubblicazione(" + idClass.ToString() + ")", result);

			// Se non ci sono stati errori si può procedere alla creazione
			if (result.NoErrors)
			{
				int idObject;
				result = objectMaker.CreateObject(out idObject);
				Logger.LogResult("CreaPubblicazione(" + idClass.ToString() + ")", result);
				if (result.NoErrors)
				{
					Logger.Log(Logger.LogEvent.Info, "E' stata creata la pubblicazione " + idObject.ToString());
					return idObject;
				}
			}
			return 0;
		}




		/// <summary>
		/// Creazione di una nuova pubblicazione
		/// </summary>
		public static int CreaPubblicazione(int idClass, System.Data.DataRow attrvalues)
		{
			IKnosResult result;

			IKnosAttributeSelector attributeSelector = AttributiTipologia(idClass);

			// Tutte le operazioni di editing sulle pubblicazioni vengono svolte da KnosObjectMaker
			IKnosObjectMaker objectMaker;
			objectMaker = (KnosObjectMaker)Knos.CreateKnosObjectMaker();

			// Se un object maker è utilizzato più volte bisogna ricordarsi di ripulire i valori
			// assegnati agli attributi prima di procedere ad una nuova modifica o creazione.
			// Se poi si decide di resettare comunque va bene
			objectMaker.Reset();

			// Per evitare di dover testare gli errori ad ogni operazione di valorizzazione degli attributi
			// KnosObjectMaker ha un GlobalResult che fa da accumulatore di errori e warning.
			// Si ripulisce GlobalResult prima di cominciare e si controlla alla fine se ci sono errori
			// Se prima si è eseguita una Reset GlobalResult è già azzerato
			objectMaker.GlobalResult.ClearAll();

			//-----------
			// Tipologia
			//-----------
			objectMaker.IdClass = idClass;

			//-----------
			// Attributi 
			// ----------
			int i;
			IKnosAttribute attr;
			object attrValue;
			for (i = 0; i < attributeSelector.ItemCount; i++)
			{
				attr = attributeSelector.GetItem(i);
				switch (attr.DataType)
				{
					case EnumKnosDataType.BooleanType:
						// Per i booleani si può passare un oggetto booleano come esempio si imposta a true
						attrValue = true;
						objectMaker.SetAttrValue(attr.ColumnName, attrValue, attr.DataType);
						// oppure una stringa case insensitive dove "1" o "true" significano true
						// objectMaker.SetAttrValue(attr.ColumnName, "1", attr.DataType);
						// objectMaker.SetAttrValue(attr.ColumnName, "True", attr.DataType);
						// objectMaker.SetAttrValue(attr.ColumnName, "true", attr.DataType);
						break;

					case EnumKnosDataType.DateTimeType:
						// Si puo passare un oggetto Datetime
						attrValue = DateTime.Now;
						objectMaker.SetAttrValue(attr.ColumnName, attrValue, attr.DataType);
						// oppure una stringa che deve essere rigorosamente in formato yyyy-mm-dd hh:mm:ss" (ora locale)
						// string stringValue = DateTime.Now.ToString("yyyy-mm-dd hh:mm:ss", DateTimeFormatInfo.InvariantInfo);
						// objectMaker.SetAttrValue(attr.ColumnName, stringValue, attr.DataType);
						break;

					case EnumKnosDataType.DecimalType:
						// KnoS di default gestisce decimali in formato decimal(19, 5)
						// quindi con precisione a 19 cifre di cui 5 decimali
						// Si può passare un numero
						attrValue = 1234567890.12345;
						objectMaker.SetAttrValue(attr.ColumnName, attrValue, attr.DataType);
						// oppure una stringa dove come punto decimale si accettano sia "." che ","
						// non è lecito passare numeri con il separatore delle migliaia 1,000.5 o 1.000,5 sono entrambi non validi
						// objectMaker.SetAttrValue(attr.ColumnName, "1000.5", attr.DataType);
						// objectMaker.SetAttrValue(attr.ColumnName, "1000,5", attr.DataType);
						break;

					case EnumKnosDataType.IntegerType:
						// KnoS gestisce interi a 32 bit con segno
						// Si può passare un numero
						//attrValue = 1;
						//objectMaker.SetAttrValue(attr.ColumnName, attrValue, attr.DataType);
						try
						{
							objectMaker.SetAttrValue(attr.ColumnName, attrvalues[attr.AttrName.ToUpper()].ToString(), attr.DataType);
						}
						catch (Exception ex)
						{ }
						// oppure una stringa che rappresenti un intero con eventuale segno
						// objectMaker.SetAttrValue(attr.ColumnName, "1", attr.DataType);
						// objectMaker.SetAttrValue(attr.ColumnName, "-1", attr.DataType);
						break;

					case EnumKnosDataType.ShortTextType:
						// Gli attributi di tipo testo breve possono essere al massimo lunghi 200 cartteri
						// Se la stringa passata è più lunga viene troncata
						try
						{
							objectMaker.SetAttrValue(attr.ColumnName, attrvalues[attr.AttrName.ToUpper()].ToString(), attr.DataType);
						}
						catch (Exception ex)
						{ }

						break;

					case EnumKnosDataType.LongTextType:
						// Gli attributi di tipo lungo sono nvarchar(max) e non hanno limitazione in lunghezza
						// se non quelle del sistema
						try
						{
							objectMaker.SetAttrValue(attr.ColumnName, attrvalues[attr.AttrName.ToUpper()].ToString(), attr.DataType);
						}
						catch (Exception ex)
						{ }

						break;

					case EnumKnosDataType.EnumListType:
						// Per gli elenchi la valorizzazione è un po' più complicata e va fatta con un multivalue editor
						IKnosMultivalueEditor enumEditor = Knos.CreateKnosMultivalueEditor();
						// I valori sono assegnati utilizzando il valore IdAttrValue contenuto nella tabella Attr_Values
						// E' possibile recuperare i valori con lo specifico selettore
						IKnosEnumSelector enumSelector = Knos.CreateKnosEnumSelector();
						enumSelector.SelectIdAttr = attr.IdAttr;
						enumSelector.GetAll();
						if (!(enumSelector.ItemCount > 0))
							continue;
						// Il primo valore è quello che verrà assegnato come test
						int idAttrValueOK = enumSelector.GetItem(0).IdAttrValue;

						// Si inserisce il valore nell'editor
						// Esempi delle operazioni che si possono fare sull'editor sono in EnumKnosDataType.ObjectListType
						enumEditor.AddValue(idAttrValueOK);

						// La valorizzazione dell'attributo è fatta alla fine delle operazioni di editing
						// passando l'editor come valore
						objectMaker.SetAttrValue(attr.ColumnName, enumEditor, attr.DataType);
						break;

					case EnumKnosDataType.ObjectListType:
						// Per gli elenchi la valorizzazione è un po' più complicata e va fatta con un multivalue editor
						IKnosMultivalueEditor linkageEditor = Knos.CreateKnosMultivalueEditor();

						// Ecco un po' di esempi di quello che si può fare con l'editor
						// per il valore in questione, qui i valori 1,2,3,4 sono solo di esempio
						// Eventuali operazioni di editing vengono cumulate e al server arriva solo il risultato
						// Questo può essere utile per semplificare la gestione di editing da interfaccia
						// Internamente l'editor gestisce due code ed un flag che indica se i valori presistenti vanno eliminati
						// le due code contengono gli elementi da aggiungere e da togliere ai valori esistenti
						int[] idAttrValueList1 = { 1, 2 };
						int[] idAttrValueList2 = { 3, 4 };
						linkageEditor.AddValue(1); // Add={1} Remove={} FlagClear=false
						linkageEditor.AddValues(idAttrValueList1); // Add={1,2} Remove={} FlagClear=false
						linkageEditor.ReplaceValues(idAttrValueList1, idAttrValueList2); // Add={3,4} Remove={1,2} FlagClear=false
						linkageEditor.RemoveValue(3); // Add={4} Remove={1,2,3} FlagClear=false
						linkageEditor.RemoveValues(idAttrValueList2); // Add={} Remove={1,2,3,4} FlagClear=false

						// ATTENZIONE: quando si usano i metodi con All si modifica il FlagClear
						linkageEditor.ReplaceAllWithValue(1); // Add={1} Remove={} FlagClear=true

						// A questo punto se si esegue il salvataggio l'effetto sarebbe di eliminare tutti i valori preesistenti
						// Per riportare il FlagClear a false si deve richiamare il metodo ResetUpdateInfo()
						linkageEditor.ResetUpdateInfo(); // Add={1} Remove={} FlagClear=false

						// Per azzerare tutte le modifiche e ricominciare da capo invece vanno richiamati in sequenza
						linkageEditor.RemoveAllValues(); // Add={} Remove={} FlagClear=true
						linkageEditor.ResetUpdateInfo(); // Add={} Remove={} FlagClear=false

						// La valorizzazione va fatta con un IdObject valido, altrimenti il valore è ignorato
						IKnosObjectSelector knosObjectSelector = Knos.CreateKnosObjectSelector();
						// Si collega alla pubblicazione più vecchia
						knosObjectSelector.GetPage(1);
						knosObjectSelector.OrderBy = "IdObject ASC";
						if (!(knosObjectSelector.ItemCount > 0))
							continue;
						linkageEditor.AddValue(knosObjectSelector.GetItem(0).IdObject);

						// La valorizzazione dell'attributo è fatta alla fine delle operazioni di editing
						// passando l'editor come valore
						objectMaker.SetAttrValue(attr.ColumnName, linkageEditor, attr.DataType);
						break;
				}
			}







            //--------------------
            // Riferimenti (Link)
            //--------------------
            // Ci sono tre tipi di riferimenti
            // - url
            // - pubblicazioni CAMPO CHE SI CHIAMA IDOBJECTTO
            // - documenti di pubblicazioni

            // Occorre utilizzare un oggetto KnosLink, ecco il caso di un url
            IKnosLink link = Knos.CreateKnosLink();
            //link.Url = "http://www.google.it";
            //link.LinkDescr = "Link esterno";
            //// Per aggiungere il link si utilizza il LinkEditor perfettamente analogo a quello descritto in EnumKnosDataType.ObjectListType
            //objectMaker.LinkEditor.AddValue(link);

            // Per i link a pubblicazioni si deve indicare l'IdObject della pubblicazione ed opzionalmente la descrizione
            // NOTA: in fase di salvataggio eventuali link autoreferenzianti o a pubblicazioni inesistenti 
            // o già presenti vengono scartati senza sollevare errori o warning.
            // ATTENZIONE che non si può riutilizzare l'oggetto precedente ma ne va creato un'altro
            link = Knos.CreateKnosLink();
            //link.IdObjectTo = 554;
            //link.LinkDescr = "Link interno";
			try
			{
				int idobjectto = 0;
				int.TryParse(attrvalues["IDOBJECTTO"].ToString(), out idobjectto);
				link.IdObjectTo = idobjectto;
				link.LinkDescr = attrvalues["IDOBJECTTODESCR"].ToString();
			}
			catch (Exception ex)
			{ }


			objectMaker.LinkEditor.AddValue(link);

            //// Per i link a documenti va indicato l'IdDoc del documento e l'IdObject della pubblicazione che lo contiene
            //link = Knos.CreateKnosLink();
            //link.IdObjectTo = 554;
            //link.IdDoc = 1;
            //link.LinkDescr = "Link a documento";
            //objectMaker.LinkEditor.AddValue(link);

            //-----------
            // Documenti
            //-----------
            // In fase di creazione di una pubblicazione si possono anche inserire dei documenti
            // il codice che descrive come farlo è in UploadSincrono

            ////-------------
            //// Destinatari
            ////-------------
            //// L'impostazione dei destinatari è fatta con il RecipientEditor 
            //// che funziona allo stesso modo del LinkEditor solo che opera su KnosRecipient
            //// Il parametro da passare è 
            //IKnosRecipient knosRecipient = Knos.CreateKnosRecipient();

            //// Si può indicare il destinatario solo per IdSubject
            //knosRecipient.IdSubject = 1; // Ruolo Administrators
            //objectMaker.RecipientEditor.AddValue(knosRecipient);

            //// Per indicarlo per nome bisogna recuperare l'IdSubject con una ricerca
            //IKnosSubjectSelector subjectSelector = Knos.CreateKnosSubjectSelector();
            //subjectSelector.SearchExpression = "Subject = '" + Config.UserName + "'";
            //subjectSelector.GetPage(1);
            //if (subjectSelector.ItemCount == 1)
            //{
            //	// Non si può riutilizzare l'oggetto precedente
            //	knosRecipient = Knos.CreateKnosRecipient();
            //	knosRecipient.IdSubject = subjectSelector.GetItem(0).IdSubject;
            //	objectMaker.RecipientEditor.AddValue(knosRecipient);
            //}


            // Si controlla l'eventuale presenza di errori 
            result = objectMaker.GlobalResult;
			Logger.LogResult("CreaPubblicazione(" + idClass.ToString() + ")", result);

			// Se non ci sono stati errori si può procedere alla creazione
			if (result.NoErrors)
			{
				int idObject;
				result = objectMaker.CreateObject(out idObject);
				Logger.LogResult("CreaPubblicazione(" + idClass.ToString() + ")", result);
				if (result.NoErrors)
				{
					Logger.Log(Logger.LogEvent.Info, "E' stata creata la pubblicazione " + idObject.ToString());
					return idObject;
				}
			}
			return 0;
		}


		/// <summary>
		/// Upload sincrono di un file
		/// </summary>
		/// <param name="idObject">IdObject della pubblicazione su cui eseguire l'upload</param>
		/// <returns>Informazioni sull'upload eseguito</returns>
		public static bool UploadSincrono(int idObject, string filePath, string fileName, string fileDescription)
		{
			IKnosResult result;
			
			bool bOK = false;

			// Anche per l'upload si usa un object maker
			IKnosObjectMaker objectMaker;
			objectMaker = Knos.CreateKnosObjectMaker();

			// Modalità analoga a quella della modifica di una pubblicazione
			objectMaker.GlobalResult.ClearAll();

			// L'oggetto utilizzato per specificare i dati di upload è:
			IKnosUploadItem uploadItem;

			// Mentre il risultato dell'upload lo ritroveremo in:
			IKnosUploadInfo uploadInfoResult = null;

			// Upload di un file in append senza indicare la versione
			uploadItem = Knos.CreateKnosUploadItem();
			uploadItem.FilePath = filePath;
			uploadItem.FileName = fileName;
			uploadItem.FileDescr = fileDescription;
			//uploadItem.FileType = "file";
			//uploadItem.IdDoc
			//uploadItem.IdVersion
			//uploadItem.Version
			//uploadItem.Release
			uploadItem.UploadType = EnumKnosUploadType.Append;
			objectMaker.AddUploadItem(uploadItem);

			// Upload di un file indicandone versione e release
			/*
			uploadItem = Knos.CreateKnosUploadItem();
			uploadItem.FilePath = @"c:\tmp\file1.txt";
			uploadItem.FileName = "pluto.txt";
			uploadItem.FileDescr = "descrizione del file";
			//uploadItem.FileType = "file";
			uploadItem.IdDoc = 5;
			uploadItem.IdVersion = 5;
			uploadItem.Version = 2;
			uploadItem.Release = 4;
			//uploadItem.UploadType = EnumKnosUploadType.NewVersion;
			uploadItem.Option_SetAsCurrentVersion = true;

			//uploadItem.FileName = "L14-0000000328.txt";
			//uploadItem.UploadType = EnumKnosUploadType.BarcodeUpload;
			*/

			objectMaker.AddUploadItem(uploadItem);
			result = objectMaker.GlobalResult;
			Logger.LogResult("AddUploadItem(" + uploadItem.FileName + ")", result);

			// Se non ci sono stati errori si può procedere con l'upload
			if (result.NoErrors)
			{
				bOK = true;
				result = objectMaker.UploadFiles(idObject, out uploadInfoResult);
				Logger.LogResult("UploadFiles(" + idObject.ToString() + ")", result);
				if (result.NoErrors)
				{
					int idDoc = uploadInfoResult.UploadList.GetItem(0).IdDoc;
					Logger.Log(Logger.LogEvent.Info, "Upload eseguito: IdObject=" + uploadInfoResult.IdObject + ", IdDoc=" + idDoc.ToString());
				}
			}
			//return uploadInfoResult;
			return bOK;
		}


		public static List<int> RicercaPubblicazioni(string searchExpression)
		{
			Console.WriteLine("\nRicercaPubblicazioni()");
			IKnosResult result;
			string str;

			// Si utilizza il selettore specifico
			IKnosObjectSelector objectSelector = Knos.CreateKnosObjectSelector();

			// E' possibile indicare una vista specifica per gli attributi da recuperare immediatamente
			// Non è necessario indicare una vista se i valori degli attributi si vogliono recuperare
			// per ogni pubblicazione trovata in un secondo tempo
			if (Config.IdView > 0)
				objectSelector.SelectIdView = Config.IdView;

			// In caso di installazioni con pubblicazioni multilingue occorre indicare la lingua di selezione
			// ATTENZIONE: non è la lingua dell'interfaccia (di default è già l'italiano)
			objectSelector.SelectIdLanguage = "it";

			// L'espressione di ricerca va formulata conoscendo i nomi delle colonne a DB
			// ed usando la normale sintassi SQL
			// L'esempio qui riportato cerca solo le pubblicazioni delle tipologie di test
			// che hanno nel titolo la parola Stress
			objectSelector.SearchExpression = searchExpression; // "IdClass IN (" + Config.IdClass.ToString() + "," + Config.LinkageIdClass.ToString() + ") AND Title LIKE '%Stress%'";

			// Si può esprimere l'ordinamento che di default è per IdObject DESC o quello indicato dalla vista
			// Qui si ordina per IdObject decrescente per trovare le ultime pubblicazioni create dalla più recente alla meno recente
			objectSelector.OrderBy = "IdObject DESC";

			// E' buona norma eseguire sempre una ricerca paginata
			// La dimensione di default della pagina è 10 ma si può cambiare
			// Qui si usa 1 perchè durante il loop di test ci sono al più solo due pubblicazioni
			// La principale e quella collegata
			objectSelector.PageSize = 1;

			// Si recupera la prima pagina
			result = objectSelector.GetPage(1);
			Logger.LogResult("objectSelector.GetPage(1)", result);
			if (result.NoErrors)
			{
				str = string.Format("La ricerca ha restituito RecordCount={0} PageCount={1} PageSize={2} PageNumber={3}",
					objectSelector.RecordCount.ToString(),
					objectSelector.PageCount.ToString(),
					objectSelector.PageSize.ToString(),
					objectSelector.PageNumber.ToString()
				);
				Logger.Log(Logger.LogEvent.Info, str);
				Console.WriteLine("\t" + str);

				//// Visualizzazione con GetItem
				//VisualizzazioneObjectSelector(objectSelector);

				//// Visualizzazione via ADORecordset
				//VisualizzazioneAdoRecordsetObjectSelector(objectSelector);

			}

			//// Per passare alle pagine successive bisognerebbe controllare se ce ne sono altre
			//// Se si richiede una pagina che non c'è viene restituita l'ultima pagina disponibile

			//// Se si volessero tutti di dati di una pubblicazione di cui si conosce l'IdObject
			//// (proprietà, attributi, documenti, riferimenti, destinatari)
			//// Si può fare così:
			//if (objectSelector.ItemCount > 0)
			//{
			//	int idObject = objectSelector.GetItem(0).IdObject;
			//	IKnosObject obj = Knos.CreateKnosObject();
			//	result = obj.GetAllObjectData(idObject);
			//	Logger.LogResult("obj.GetAllObjectData", result);

			//	// In alternativa ci sono i singoli metodi
			//	obj.GetObjectAttributes(idObject); // Recupera proprietà e attributi
			//	obj.GetObjectDocuments(idObject); // Recupera i documenti
			//	obj.GetObjectLinks(idObject); // Recupera i riferimenti
			//	obj.GetObjectRecipients(idObject); // Recupera i destinatari
			//}

			List<int> idObjectList = new List<int>();

			if (objectSelector.ItemCount > 0)
			{
				for (int i = 0; i < objectSelector.ItemCount; i++)
				{
					idObjectList.Add(objectSelector.GetItem(i).IdObject);
				}


			}
			else
			{
				idObjectList.Add(0);
			}

			return idObjectList;
		}
	}
}
