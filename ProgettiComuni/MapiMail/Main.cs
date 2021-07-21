using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

using Log4NetLogger;
using System.Reflection;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SendFileTo
{
    public class OTLK
    {

        public int SendMail(string strMsgTo, string strMsgCC, string strMsgObject, string strMsgText, bool popup = false)
        {
            Logger log = new Logger();

            log.Setup();
            log.LogSomething("Start Outlook");

            try
            {
                Microsoft.Office.Interop.Outlook.Application oApp = new Microsoft.Office.Interop.Outlook.Application();
                Microsoft.Office.Interop.Outlook.MailItem oMsg = (Microsoft.Office.Interop.Outlook.MailItem)oApp.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);

                oMsg.To = strMsgTo;
                oMsg.CC = strMsgCC;
                oMsg.Subject = strMsgObject;
                oMsg.BodyFormat = Microsoft.Office.Interop.Outlook.OlBodyFormat.olFormatHTML;
                oMsg.HTMLBody = strMsgText; //Here comes your body;
                //oMsg.Attachments.Add("c:/temp/test.txt", Microsoft.Office.Interop.Outlook.OlAttachmentType.olByValue, Type.Missing, Type.Missing);

                if (popup)
                {
                    log.LogSomething("Invio con anteprima");
                    oMsg.Display(false); //In order to display it in modal inspector change the argument to true
                }
                else
                {
                     log.LogSomething("Invio automatico");
                    oMsg.Send();                  
                }

                //oApp.ActiveExplorer().Activate();
                //oMsg.Close();

                return 0;


            }
            catch (Exception ex)
            {
                //MessageBox.Show("Problemi di invio mail!\r\n" + ex.Message, "Invio mail ");
                log.LogSomething("Problemi di invio mail!\r\n" + ex.Message);
                return -1;
            }
        }
    }


    public class MAPI
    {

        public bool AddRecipientTo(string email)
        {
            return AddRecipient(email, HowTo.MAPI_TO);
        }

        public bool AddRecipientCC(string email)
        {
            return AddRecipient(email, HowTo.MAPI_CC);
        }

        public bool AddRecipientBCC(string email)
        {
            return AddRecipient(email, HowTo.MAPI_BCC);
        }

        public void AddAttachment(string strAttachmentFileName)
        {
            m_attachments.Add(strAttachmentFileName);
        }

        public int SendMailPopup(string strSubject, string strBody)
        {
            return SendMail(strSubject, strBody, MAPI_LOGON_UI | MAPI_DIALOG);
        }

        public int SendMailDirect(string strSubject, string strBody)
        {
            return SendMail(strSubject, strBody, MAPI_LOGON_UI);
        }





        [DllImport("MAPI32.DLL")]
        static extern int MAPISendMail(IntPtr sess, IntPtr hwnd, 
            MapiMessage message, int flg, int rsv);

        int SendMail(string strSubject, string strBody, int how)
        {

            
            MapiMessage msg = new MapiMessage();
            msg.subject = strSubject;
            msg.noteText = strBody;

            
            msg.recips = GetRecipients(out msg.recipCount);
            msg.files = GetAttachments(out msg.fileCount);

            

            m_lastError = MAPISendMail(new IntPtr(0), new IntPtr(0), msg, how, 
                0);
            if (m_lastError > 1)
                MessageBox.Show("MAPISendMail failed! " + GetLastError(), 
                    "MAPISendMail");

            Cleanup(ref msg);
            return m_lastError;
        }

        bool AddRecipient(string email, HowTo howTo)
        {
            MapiRecipDesc recipient = new MapiRecipDesc();

            recipient.recipClass = (int)howTo;
            recipient.name = email;
            m_recipients.Add(recipient);

            return true;
        }

        IntPtr GetRecipients(out int recipCount)
        {
            recipCount = 0;
            if (m_recipients.Count == 0)
                return IntPtr.Zero;

            int size = Marshal.SizeOf(typeof(MapiRecipDesc));
            IntPtr intPtr = Marshal.AllocHGlobal(m_recipients.Count * size);

            int ptr = (int)intPtr;
            foreach (MapiRecipDesc mapiDesc in m_recipients)
            {
                Marshal.StructureToPtr(mapiDesc, (IntPtr)ptr, false);
                ptr += size;
            }

            recipCount = m_recipients.Count;
            return intPtr;
        }

        IntPtr GetAttachments(out int fileCount)
        {
            fileCount = 0;
            if (m_attachments == null)
                return IntPtr.Zero;

            if ((m_attachments.Count <= 0) || (m_attachments.Count > 
                maxAttachments))
                return IntPtr.Zero;

            int size = Marshal.SizeOf(typeof(MapiFileDesc));
            IntPtr intPtr = Marshal.AllocHGlobal(m_attachments.Count * size);

            MapiFileDesc mapiFileDesc = new MapiFileDesc();
            mapiFileDesc.position = -1;
            int ptr = (int)intPtr;
            
            foreach (string strAttachment in m_attachments)
            {
                mapiFileDesc.name = Path.GetFileName(strAttachment);
                mapiFileDesc.path = strAttachment;
                Marshal.StructureToPtr(mapiFileDesc, (IntPtr)ptr, false);
                ptr += size;
            }

            fileCount = m_attachments.Count;
            return intPtr;
        }

        void Cleanup(ref MapiMessage msg)
        {
            int size = Marshal.SizeOf(typeof(MapiRecipDesc));
            int ptr = 0;

            if (msg.recips != IntPtr.Zero)
            {
                ptr = (int)msg.recips;
                for (int i = 0; i < msg.recipCount; i++)
                {
                    Marshal.DestroyStructure((IntPtr)ptr, 
                        typeof(MapiRecipDesc));
                    ptr += size;
                }
                Marshal.FreeHGlobal(msg.recips);
            }

            if (msg.files != IntPtr.Zero)
            {
                size = Marshal.SizeOf(typeof(MapiFileDesc));

                ptr = (int)msg.files;
                for (int i = 0; i < msg.fileCount; i++)
                {
                    Marshal.DestroyStructure((IntPtr)ptr, 
                        typeof(MapiFileDesc));
                    ptr += size;
                }
                Marshal.FreeHGlobal(msg.files);
            }
            
            m_recipients.Clear();
            m_attachments.Clear();
            m_lastError = 0;
        }
        
        public string GetLastError()
        {
            if (m_lastError <= 26)
                return errors[ m_lastError ];
            return "MAPI error [" + m_lastError.ToString() + "]";
        }

        readonly string[] errors = new string[] {
        "OK [0]", "User abort [1]", "General MAPI failure [2]", 
                "MAPI login failure [3]", "Disk full [4]", 
                "Insufficient memory [5]", "Access denied [6]", 
                "-unknown- [7]", "Too many sessions [8]", 
                "Too many files were specified [9]", 
                "Too many recipients were specified [10]", 
                "A specified attachment was not found [11]",
        "Attachment open failure [12]", 
                "Attachment write failure [13]", "Unknown recipient [14]", 
                "Bad recipient type [15]", "No messages [16]", 
                "Invalid message [17]", "Text too large [18]", 
                "Invalid session [19]", "Type not supported [20]", 
                "A recipient was specified ambiguously [21]", 
                "Message in use [22]", "Network failure [23]",
        "Invalid edit fields [24]", "Invalid recipients [25]", 
                "Not supported [26]" 
        };


        List<MapiRecipDesc> m_recipients    = new 
            List<MapiRecipDesc>();
        List<string> m_attachments    = new List<string>();
        int m_lastError = 0;

        const int MAPI_LOGON_UI = 0x00000001;
        const int MAPI_DIALOG = 0x00000008;
        const int maxAttachments = 20;

        enum HowTo{MAPI_ORIG=0, MAPI_TO, MAPI_CC, MAPI_BCC};
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class MapiMessage
    {
        public int reserved;
        public string subject;
        public string noteText;
        public string messageType;
        public string dateReceived;
        public string conversationID;
        public int flags;
        public IntPtr originator;
        public int recipCount;
        public IntPtr recips;
        public int fileCount;
        public IntPtr files;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class MapiFileDesc
    {
        public int reserved;
        public int flags;
        public int position;
        public string path;
        public string name;
        public IntPtr type;
    }

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
    public class MapiRecipDesc
    {
        public int        reserved;
        public int        recipClass;
        public string    name;
        public string    address;
        public int        eIDSize;
        public IntPtr    entryID;
    }

    public class Cdo
    {
        public bool IsBodyHtml;
        public string from;
        public string to;
        public string toCC;
        public string toBCC;

        public string SMTPServerName;
        public int SMTPPort;
        public bool SMTPUseSSL;
        public string SMTPAUTHUser;
        public string SMTPAUTHPassword;
        public string SMTPAUTHUserEmail;
        public string SMTPAlwaysSendAs;
        public List<string> attachments;
        public List<string> bodyimages;

        public Logger log = new Logger();

        public string localmailsaved = "";

        public int SendMail(string strSubject, string strBody, int how)
        {
            log.Setup();
            log.LogSomething("Start servizio");

            int intOK = 0;

            try
            {
                System.Net.Mail.MailMessage eMail = new System.Net.Mail.MailMessage();
                eMail.IsBodyHtml = true;

                if (string.IsNullOrEmpty(from))
                    from = SMTPAUTHUserEmail;

                if (SMTPAlwaysSendAs != "")
                {
                    eMail.From = new System.Net.Mail.MailAddress(from, SMTPAlwaysSendAs);
                }
                else
                {
                    eMail.From = new System.Net.Mail.MailAddress(from);
                }

                to = to.Replace(";", ",");
                if (to.LastIndexOf(",") == to.Length - 1)
                {
                    to = to.Substring(0, to.Length - 1);
                }

                if(!string.IsNullOrEmpty(toCC) && (toCC != ""))
                {
                        toCC = toCC.Replace(";", ",");
                    if (toCC.LastIndexOf(",") == toCC.Length - 1)
                    {
                        
                        toCC = toCC.Substring(0, toCC.Length - 1);
                        eMail.CC.Add(toCC);
                    }
                }


                if (!string.IsNullOrEmpty(toBCC) && (toBCC != ""))
                {   
                    toBCC = toBCC.Replace(";", ",");
                    if (toBCC.LastIndexOf(",") == toBCC.Length - 1)
                    {
                        
                        toBCC = toBCC.Substring(0, toBCC.Length - 1);
                    }
                    eMail.Bcc.Add(toBCC);
                }

                if (!string.IsNullOrEmpty(to) && (to != ""))
                    eMail.To.Add(to);

                //if (!string.IsNullOrEmpty(toCC) && (toCC != ""))
                //    eMail.CC.Add(toCC);

                //if (!string.IsNullOrEmpty(toBCC) && (toBCC != ""))
                //    eMail.Bcc.Add(toBCC);

                eMail.Subject = strSubject;
                eMail.BodyEncoding = System.Text.Encoding.UTF8;
                eMail.Body = strBody;

                if (attachments != null)
                {
                    foreach (var a in attachments)
                    {
                        if (File.Exists(a.Replace("file://", "")))
                        {
                            eMail.Attachments.Add(new System.Net.Mail.Attachment(a.Replace("file://", "")));
                        }
                    }
                }

                // Connecting to the server and configuring it
                System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient();
                //client SmtpDeliveryFormat.International;

                client.Host = SMTPServerName;
                client.Port = SMTPPort;
                // The server requires user's credentials
                // not the default credentials
                client.UseDefaultCredentials = false;
                // Provide your credentials
                client.Credentials = new System.Net.NetworkCredential(SMTPAUTHUser, SMTPAUTHPassword);
                client.EnableSsl = SMTPUseSSL;
                //client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                client.Timeout = 20000;

                //eMail.Save(localmailsaved);

                // Use SendAsync to send the message asynchronously
                client.Send(eMail);


                log.LogSomething(string.Format("MAIL INVIATA CORRETTAMENTE SendMail: {0} - {1} - {2} - {3} - {4} - {5} - {6}", "", SMTPServerName, SMTPPort, SMTPAUTHUser, SMTPAUTHPassword, to, toBCC));




            }
            catch (Exception ex)
            {
                log.LogSomething(string.Format("ERRORE SendMail: {0} - {1} - {2} - {3} - {4} - {5} - {6}", ex.Message, SMTPServerName, SMTPPort, SMTPAUTHUser, SMTPAUTHPassword, to, toBCC));

                throw ex;
            }
            return intOK;
        }

        public int SendMailWeb(string strSubject, string strBody, int how)
        {
            log.Setup();
            log.LogSomething("Start in vio mail da SendFileTo");

            // using System.Web.Mail;
            System.Web.Mail.MailMessage eMail = new System.Web.Mail.MailMessage();
            eMail.BodyFormat = System.Web.Mail.MailFormat.Html;
            eMail.From = SMTPAUTHUser;

            eMail.Fields["http://schemas.microsoft.com/cdo/configuration/smtsperver"] = SMTPServerName;
            eMail.Fields["http://schemas.microsoft.com/cdo/configuration/smtpserverport"] = SMTPPort;
            eMail.Fields["http://schemas.microsoft.com/cdo/configuration/sendusing"] = 2;
            if (SMTPAUTHUser != null && SMTPAUTHPassword != null)
            {
                eMail.Fields["http://schemas.microsoft.com/cdo/configuration/smtpauthenticate"] = 1;
                eMail.Fields["http://schemas.microsoft.com/cdo/configuration/sendusername"] = SMTPAUTHUser;
                eMail.Fields["http://schemas.microsoft.com/cdo/configuration/sendpassword"] = SMTPAUTHPassword;
                if (SMTPUseSSL == true)
                    eMail.Fields["http://schemas.microsoft.com/cdo/configuration/smtpusessl"] = 1;
            }

            eMail.To = to;
            eMail.Cc = toCC;
            eMail.Bcc= toBCC;
            eMail.Subject = strSubject;
            eMail.Body = strBody;

            if (attachments != null)
            {
                foreach (var a in attachments)
                {
                    if (File.Exists(a.Replace("file://", "")))
                    {
                        eMail.Attachments.Add(new System.Web.Mail.MailAttachment(a.Replace("file://", "")));
                    }
                }
            }

            try
            {
                

                System.Web.Mail.SmtpMail.SmtpServer = SMTPServerName;
                System.Web.Mail.SmtpMail.Send(eMail);

                

                log.LogSomething(string.Format("OK SendMailWeb: {0} - {1} - {2} ", SMTPServerName, SMTPPort, SMTPAUTHUser));
                //MessageBox.Show("Mail Sent!", "Success", MessageBoxButtons.OK);
                return 1;
            }
            catch (Exception ex)
            {
                log.LogSomething(string.Format("ERRORE SendMailWeb: {0} - {1} - {2} - {3}", ex.Message, SMTPServerName, SMTPPort, SMTPAUTHUser));
                return -1;
                throw ex;
            }

        }

        public void SendSaveEML(string strSubject, string strBody, int how)
        {
            //string dummyEmail = from;

            if (string.IsNullOrEmpty(from))
                from = SMTPAUTHUserEmail;

            var mailMessage = new MailMessage();

            mailMessage.From = new MailAddress(from);

            to = to.Replace(";", ",");
            if ((to.LastIndexOf(",") == to.Length - 1) && (to.Length > 0))
            {
                to = to.Substring(0, to.Length - 1);
                
            }

            if (to.Length > 0)
                mailMessage.To.Add(to);

            toCC = toCC.Replace(";", ",");
            if ((toCC.LastIndexOf(",") == toCC.Length - 1) && (toCC.Length > 0))
            {
                toCC = toCC.Substring(0, toCC.Length - 1);
             
           }
            if (toCC.Length > 0)
                mailMessage.CC.Add(toCC);

                toBCC = toBCC.Replace(";", ",");
            if ((toBCC.LastIndexOf(",") == toBCC.Length - 1) && (toBCC.Length > 0))
            {
                toBCC = toBCC.Substring(0, toBCC.Length - 1);
                
            }
            if (toBCC.Length > 0)
                mailMessage.Bcc.Add(toBCC);

            mailMessage.Subject = strSubject;
            mailMessage.Body = strBody;
            mailMessage.IsBodyHtml = true;

            // mark as draft
            mailMessage.Headers.Add("X-Unsent", "1");

            //// download image and save it as attachment
            //using (var httpClient = new HttpClient())
            //{
            //    var imageStream = await httpClient.GetStreamAsync(new Uri("http://dcaric.com/favicon.ico"));
            //    mailMessage.Attachments.Add(new Attachment(imageStream, "favicon.ico"));
            //}

            var stream = new MemoryStream();
            ToEmlStream(mailMessage, stream, from);

            stream.Position = 0;

            return;// File(stream, "message/rfc822", "test_email.eml");
        }

        private void ToEmlStream(MailMessage msg, Stream str, string dummyEmail)
        {
            using (var client = new SmtpClient())
            {
                var id = Guid.NewGuid();

                var tempFolder = Path.Combine(Path.GetTempPath(), Assembly.GetExecutingAssembly().GetName().Name);

                tempFolder = Path.Combine(tempFolder, "MailMessageToEMLTemp");

                // create a temp folder to hold just this .eml file so that we can find it easily.
                tempFolder = Path.Combine(tempFolder, id.ToString());

                if (!Directory.Exists(tempFolder))
                {
                    Directory.CreateDirectory(tempFolder);
                }

                //client.UseDefaultCredentials = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(SMTPAUTHUser, SMTPAUTHPassword);
                client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                client.PickupDirectoryLocation = tempFolder;
                client.Send(msg);

                // tempFolder should contain 1 eml file
                var filePath = Directory.GetFiles(tempFolder)[0];

                // create new file and remove all lines that start with 'X-Sender:' or 'From:'
                string newFile = localmailsaved; //Path.Combine(tempFolder, "modified.eml");
                using (var sr = new StreamReader(filePath))
                {
                    using (var sw = new StreamWriter(newFile))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (!line.StartsWith("X-Sender:") &&
                                !line.StartsWith("From:") &&
                                // dummy email which is used if receiver address is empty
                                !line.StartsWith("X-Receiver: " + dummyEmail) &&
                                // dummy email which is used if receiver address is empty
                                !line.StartsWith("To: " + dummyEmail))
                            {
                                sw.WriteLine(line);
                            }
                        }
                    }
                }

                // stream out the contents
                //File.Delete(localmailsaved);

                //using (var fs = new FileStream(newFile, FileMode.Open))
                //{
                //    fs.CopyTo(str);
                //}
            }
        }

    }



    public class LotusNotes {

        public bool IsBodyHtml;
        public string from;
        public string to;
        public string toCC;
        public string toBCC;

        public string SMTPServerName;
        public int SMTPPort;
        public bool SMTPUseSSL;
        public string SMTPAUTHUser;
        public string SMTPAUTHPassword;
        public string SMTPAUTHUserEmail;
        public string SMTPAlwaysSendAs;
        public List<string> attachments;
        public List<string> bodyimages;

        public Logger log = new Logger();

        //public bool SendMailPopup(string strSubject, string strBody)
        //{
        //    bool bOK = false;

        //    //Create new notes session 
        //    Domino.NotesSession _notesSession = new Domino.NotesSession();
        //    //Initialize Notes Database to null; nothing in VB. 
        //    Domino.NotesDatabase _notesDataBase = null;
        //    //Initialize Notes Document to null; nothing in VB. 
        //    Domino.NotesDocument _notesDocument = null;
        //    //Notes Server Name in form of: ServerName/Domain. 
        //    string sServerName = SMTPServerName;

        //    try
        //    {
        //        MessageBox.Show(string.Format("step {0} : {1}", "inizio", "1"));

        //        //Mail File is in form of: mail\\userName.nsf 
        //        string sMailFile = SMTPAUTHUserEmail;
        //        string password = SMTPAUTHPassword;
        //        string sSendTo = to;
        //        string sSubject = strSubject;
        //        string UserName = "";
        //        //required for send, since it's byRef and not byVal, gets set later. 
        //        object oItemValue = null;
        //        //use string array to CC Send 
        //        string[] sCopyTo = toCC.Split(';');

        //        //Initialize Notes Session 
        //        MessageBox.Show(string.Format("step {0} : {1}", "Initialize Notes Session", password));
        //        _notesSession.Initialize(password);

        //        UserName = _notesSession.UserName;
        //        MessageBox.Show(string.Format("step {0} : {1}", "UserName database", UserName));

        //        Domino.NotesDbDirectory _notesDbDir = _notesSession.GetDbDirectory("Local");
        //        MessageBox.Show(string.Format("step {0} : {1}", "get _notesDbDir", _notesDbDir));

        //        sMailFile = UserName.Substring(0, 1) + UserName.Substring(UserName.IndexOf(' ') + 1) + ".nsf";
        //        MessageBox.Show(string.Format("step {0} : {1}", "get database", sMailFile));

        //        //Get Database via server name & c:\notes\data\mailfilename.nsf 
        //        //if not found set to false to not create one 
        //        //_notesDataBase = _notesSession.GetDatabase("", sMailFile, false);
        //        _notesDataBase = _notesDbDir.OpenMailDatabase();
        //        MessageBox.Show(string.Format("step {0} : {1}", "get database", _notesDataBase));

        //        //If the database is not already open then open it. 
        //        if (!_notesDataBase.IsOpen)
        //        {
        //            MessageBox.Show(string.Format("step {0} : {1}", "open database", ""));
        //            _notesDataBase.Open();
        //        }

        //        //Create the notes document 
        //        MessageBox.Show(string.Format("step {0} : {1}", "Create the notes document", ""));
        //        _notesDocument = _notesDataBase.CreateDocument();

        //        //Set document type 
        //        MessageBox.Show(string.Format("step {0} : {1}", "Form", "Memo"));
        //        _notesDocument.ReplaceItemValue(
        //            "Form", "Memo");

        //        //sent notes memo fields (To: CC: Bcc: Subject etc) 
        //        MessageBox.Show(string.Format("step {0} : {1}", "sSendTo", sSendTo));
        //        _notesDocument.ReplaceItemValue(
        //            "SendTo", sSendTo);
        //        //_notesDocument.ReplaceItemValue(
        //        //    "CopyTo", sCopyTo);
        //        MessageBox.Show(string.Format("step {0} : {1}", "Subject", sSubject));
        //        _notesDocument.ReplaceItemValue(
        //            "Subject", sSubject);

        //        //Set the body of the email. This allows you to use the appendtext 
        //        Domino.NotesRichTextItem _richTextItem = _notesDocument.CreateRichTextItem("Body");

        //        //add lines to memo email body. the \r\n is needed for each new line. 
        //        //MessageBox.Show(string.Format("step {0} : {1}", "Subject", sSubject));
        //        _richTextItem.AppendText("strBody");
        //        MessageBox.Show(string.Format("step {0} : {1}", "Body", _richTextItem.Text));
        //        //send email & pass in byRef field, this case SendTo (always have this, 

        //        //foreach (string a in attachments)
        //        //{
        //        //    Domino.NotesRichTextItem _attachement = _notesDocument.CreateRichTextItem("Attachment");
        //        //    _attachement.EmbeddedObjects(1454, "", a, "Attachment");
        //        //}
        //        // save

        //        _notesDocument.SaveMessageOnSend = true;
        //        MessageBox.Show(string.Format("step {0} : {1}", "save", ""));

        //        //cc or bcc may not always be there. 
        //        oItemValue = _notesDocument.GetItemValue(
        //            "SendTo");
        //        MessageBox.Show(string.Format("step {0} : {1}", "oItemValue", oItemValue.ToString()));
        //        _notesDocument.Send(
        //            false, ref oItemValue);

        //        _richTextItem =
        //            null;

        //        bOK = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Errore nell'invio mail" + ex.Message);


        //    }
        //    //release resources. 
        //    _notesDocument =
        //        null;
        //    _notesDataBase =
        //        null;
        //    _notesSession =
        //        null;

        //    return bOK;

        //}

    }
}