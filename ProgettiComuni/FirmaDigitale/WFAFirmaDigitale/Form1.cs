using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;


using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using System.IO;
using System.Diagnostics;

namespace WFAFirmaDigitale
{
    public partial class Form1 : Form
    {

        X509Certificate2Collection certList;
        X509Certificate2 cerSelected;

        string ProviderNameCert;
        string KeyContainerNameCert;

        List<certItemProps> certItemPropsList = new List<certItemProps>();

        class certProps
        {
            public string PROPRIETA { get; set; }
            public string VALORE { get; set; }

        }

        class certItemProps
        {
            public string SN { get; set; }
            public string ProviderName { get; set; }
            public string KeyContainerName { get; set; }
        }

        public Form1()
        {
            InitializeComponent();
        }


        private void debug(string txt)
        {
            txt = string.Format("{0} {1} - {2}", System.DateTime.Now.ToLongDateString(), System.DateTime.Now.ToLongTimeString(), txt);
            DebugBox.AppendText(txt + System.Environment.NewLine);
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            button5_Click(null, null);



            //getLocalCerts();
        }

        void getLocalCerts()
        {
            //X509Store store = new X509Store("MY", StoreLocation.CurrentUser);
            //store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            //X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
            //X509Certificate2Collection fcollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
            //X509Certificate2Collection scollection = X509Certificate2UI.SelectFromCollection(fcollection, "Test Certificate Select", "Select a certificate from the following list to get information on that certificate", X509SelectionFlag.MultiSelection);
            //Console.WriteLine("Number of certificates: {0}{1}", scollection.Count, Environment.NewLine);

            //label7.Text = string.Format("Number of certificates: {0}", scollection.Count);

            //foreach (X509Certificate2 x509 in scollection)
            //{
            //    try
            //    {
            //        byte[] rawdata = x509.RawData;

            //        //listBox1.Items.Add(x509.GetNameInfo(X509NameType.SimpleName, true));
            //        Console.WriteLine("Content Type: {0}{1}", X509Certificate2.GetCertContentType(rawdata), Environment.NewLine);
            //        Console.WriteLine("Friendly Name: {0}{1}", x509.FriendlyName, Environment.NewLine);
            //        Console.WriteLine("Certificate Verified?: {0}{1}", x509.Verify(), Environment.NewLine);
            //        Console.WriteLine("Simple Name: {0}{1}", x509.GetNameInfo(X509NameType.SimpleName, true), Environment.NewLine);
            //        Console.WriteLine("Signature Algorithm: {0}{1}", x509.SignatureAlgorithm.FriendlyName, Environment.NewLine);
            //        Console.WriteLine("Private Key: {0}{1}", x509.PrivateKey.ToXmlString(false), Environment.NewLine);
            //        Console.WriteLine("Public Key: {0}{1}", x509.PublicKey.Key.ToXmlString(false), Environment.NewLine);
            //        Console.WriteLine("Certificate Archived?: {0}{1}", x509.Archived, Environment.NewLine);
            //        Console.WriteLine("Length of Raw Data: {0}{1}", x509.RawData.Length, Environment.NewLine);
            //        X509Certificate2UI.DisplayCertificate(x509);
            //        x509.Reset();
            //    }
            //    catch (CryptographicException)
            //    {
            //        Console.WriteLine("Information could not be written out for this certificate.");
            //    }
            //}
            //store.Close();
        }

        private SecureString GetSecurePin(string PinCode)
        {
            SecureString pwd = new SecureString();
            foreach (var c in PinCode.ToCharArray()) pwd.AppendChar(c);
            return pwd;
        }
        private void button1_Click(object sender, EventArgs e)
        {

            comboBox1.Items.Clear();

            //Sign from SmartCard
            //note : ProviderName and KeyContainerName can be found with the dos command : CertUtil -ScInfo
            string ProviderName = textBox2.Text;
            string KeyContainerName = textBox3.Text;
            string PinCode = textBox4.Text;
            if (PinCode != "")
            {
                //if pin code is set then no windows form will popup to ask it
                SecureString pwd = GetSecurePin(PinCode);
                CspParameters csp = new CspParameters(1,
                                                        ProviderName,
                                                        KeyContainerName,
                                                        new System.Security.AccessControl.CryptoKeySecurity(),
                                                        pwd);
                try
                {
                    RSACryptoServiceProvider rsaCsp = new RSACryptoServiceProvider(csp);
                    // the pin code will be cached for next access to the smart card
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Crypto error: " + ex.Message);
                    return;
                }
            }
            X509Store store = new X509Store(StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2 cert = null;
            if ((ProviderName == "") || (KeyContainerName == ""))
            {
                MessageBox.Show("You must set Provider Name and Key Container Name");
                return;
            }

            label7.Text = string.Format("Number of certificates: {0}", store.Certificates.Count);

            certList = store.Certificates;


            foreach (X509Certificate2 cert2 in store.Certificates)
            {
                if (cert2.HasPrivateKey)
                {
                    RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)cert2.PrivateKey;
                    if (rsa == null) continue; // not smart card cert again
                    if (rsa.CspKeyContainerInfo.HardwareDevice) // sure - smartcard
                    {
                        comboBox1.Items.Add(rsa.CspKeyContainerInfo.KeyContainerName);
                       

                        if ((rsa.CspKeyContainerInfo.KeyContainerName == KeyContainerName) && (rsa.CspKeyContainerInfo.ProviderName == ProviderName))
                        {
                            //we find it
                            cert = cert2;
                            break;
                        }
                    }
                }
            }
            if (cert == null)
            {
                MessageBox.Show("Certificate not found");
                return;
            }
            SignWithThisCert(cert);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ////Sign with certificate selection in the windows certificate store
            //X509Store store = new X509Store(StoreLocation.CurrentUser);
            //store.Open(OpenFlags.ReadOnly);
            //X509Certificate2 cert = null;
            ////manually chose the certificate in the store
            //X509Certificate2Collection sel = X509Certificate2UI.SelectFromCollection(store.Certificates, null, null, X509SelectionFlag.SingleSelection);
            //if (sel.Count > 0)
            //    cert = sel[0];
            //else
            //{
            //    MessageBox.Show("Certificate not found");
            //    return;
            //}
            //SignWithThisCert(cert);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ////Sign from certificate in a pfx or a p12 file
            //string PfxFileName = textBox5.Text;
            //string PfxPassword = textBox6.Text;
            //X509Certificate2 cert = new X509Certificate2(PfxFileName, PfxPassword);
            //SignWithThisCert(cert);
        }

        private void SignWithThisCert(X509Certificate2 cert)
        {
            string SourcePdfFileName = textBox1.Text;
            string DestPdfFileName = textBox1.Text + "-Signed.pdf";
            Org.BouncyCastle.X509.X509CertificateParser cp = new Org.BouncyCastle.X509.X509CertificateParser();
            Org.BouncyCastle.X509.X509Certificate[] chain = new Org.BouncyCastle.X509.X509Certificate[] { cp.ReadCertificate(cert.RawData) };
            IExternalSignature externalSignature = new X509Certificate2Signature(cert, "SHA-1");
            PdfReader pdfReader = new PdfReader(SourcePdfFileName);
            FileStream signedPdf = new FileStream(DestPdfFileName, FileMode.Create);  //the output pdf file
            PdfStamper pdfStamper = PdfStamper.CreateSignature(pdfReader, signedPdf, '\0');
            PdfSignatureAppearance signatureAppearance = pdfStamper.SignatureAppearance;
            //here set signatureAppearance at your will
            signatureAppearance.Reason = textBox2.Text;
            signatureAppearance.Location = textBox3.Text;
            signatureAppearance.SignatureCreator = textBox5.Text;
            signatureAppearance.SignatureRenderingMode = PdfSignatureAppearance.RenderingMode.DESCRIPTION;
            if (checkBox1.Checked)
            {
                signatureAppearance.SetVisibleSignature(new iTextSharp.text.Rectangle(100, 100, 250, 150), 1, null);
            }

            MakeSignature.SignDetached(signatureAppearance, externalSignature, chain, null, null, null, 0, CryptoStandard.CMS);
            //MakeSignature.SignDetached(signatureAppearance, externalSignature, chain, null, null, null, 0, CryptoStandard.CADES);
            if (MessageBox.Show("Apro il file firmato?", "File firmato con successo!", MessageBoxButtons.YesNo , MessageBoxIcon.Information,     MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                Process.Start(DestPdfFileName);

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            textBox1.Text = openFileDialog1.FileName;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                comboBox1.DataSource = null;
                certItemPropsList.Clear();

                DebugBox.Text = "";
                debug("Lettura certificati dal dispositivo..");

                //Sign from SmartCard
                X509Store store = new X509Store(StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);

                debug("certificati letti!");

                certList = store.Certificates;

                BindingList<X509Certificate2> objects = new BindingList<X509Certificate2>();

                foreach (X509Certificate2 cert2 in store.Certificates)
                {
                    debug("certificato letto!");

                    if (cert2.HasPrivateKey)
                    {
                        debug("certificato con chiave privata!");

                        if (cert2.Issuer.ToLower().Contains("qualificata 2"))
                        {
                            objects.Add(cert2);


                            debug(string.Format("certificato {0} {1}", cert2.Subject, cert2.SerialNumber));

                            RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)cert2.PrivateKey;
                            if (rsa == null) continue; // not smart card cert again
                            if (rsa.CspKeyContainerInfo.HardwareDevice) // sure - smartcard
                            {
                                try
                                {
                                    debug(rsa.CspKeyContainerInfo.KeyContainerName);
                                    debug(rsa.CspKeyContainerInfo.ProviderName);

                                    comboBox1.Items.Add(rsa.CspKeyContainerInfo.KeyContainerName);
                                    comboBox1.Items.Add(rsa.CspKeyContainerInfo.ProviderName);

                                    certItemProps c = new certItemProps();
                                    c.SN = cert2.SerialNumber;
                                    c.KeyContainerName = rsa.CspKeyContainerInfo.KeyContainerName;
                                    c.ProviderName = rsa.CspKeyContainerInfo.ProviderName;


                                    certItemPropsList.Add(c);
                                }
                                catch (Exception ex)
                                {
                                    debug(ex.Message);
                                }
                            }
                        }
                    }
                }

                comboBox1.ValueMember = null;
                comboBox1.DisplayMember = "Subject";
                comboBox1.DataSource = objects;

                if (comboBox1.Items.Count > 0)
                    comboBox1.SelectedIndex = 0;

                debug(string.Format("{0}", "lettura certificati completata"));
            }
            catch (Exception ex)
            {
                debug(ex.Message);
            }

        }

        private string ObjectToXml(object output)
        {
            string objectAsXmlString;

            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(output.GetType());
            using (System.IO.StringWriter sw = new System.IO.StringWriter())
            {
                try
                {
                    xs.Serialize(sw, output);
                    objectAsXmlString = sw.ToString();
                }
                catch (Exception ex)
                {
                    objectAsXmlString = ex.ToString();
                }
            }

            return objectAsXmlString;
        }



        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue != null)
            {
                cerSelected = (X509Certificate2)comboBox1.SelectedValue;
                //MessageBox.Show(current.Value.ToString());

                string[] subjectProps = cerSelected.Subject.Split(',');
                string[] issuerProps = cerSelected.Issuer.Split(',');
                List<certProps> certPropsList = new List<WFAFirmaDigitale.Form1.certProps>();

                certPropsList.Add(new certProps() { PROPRIETA = "Subject", VALORE = cerSelected.Subject });
                foreach (string s in subjectProps)
                {
                    string[] p = s.Split('=');

                    certPropsList.Add(new certProps() { PROPRIETA = string.Format("Subject ({0})", p[0].TrimStart().TrimEnd()), VALORE = p[1].TrimStart().TrimEnd() });

                    if (p[0].TrimStart().TrimEnd() == "CN")
                        textBox5.Text = p[1].TrimStart().TrimEnd();
                }

                certPropsList.Add(new certProps() { PROPRIETA = "SerialNumber", VALORE = cerSelected.SerialNumber });

                certPropsList.Add(new certProps() { PROPRIETA = "Issuer", VALORE = cerSelected.Issuer });
                foreach (string s in issuerProps)
                {
                    string[] p = s.Split('=');

                    certPropsList.Add(new certProps() { PROPRIETA = string.Format("Issuer ({0})", p[0].TrimStart().TrimEnd()), VALORE = p[1].TrimStart().TrimEnd() });

                }
                dataGridView1.DataSource = certPropsList;

                



            }
        }

        private void button6_Click(object sender, EventArgs e)
        {

            DebugBox.Text = "";

            if (!File.Exists(textBox1.Text))
            {
                MessageBox.Show("Selezionare il file da firmare!");
                return;
            }

            if (textBox4.Text != "")
            {
                debug(string.Format("{0}", "Autenticazione con PIN"));
                foreach (certItemProps ci in certItemPropsList)
                {
                    if (ci.SN == cerSelected.SerialNumber)
                    {
                        ProviderNameCert = ci.ProviderName;
                        KeyContainerNameCert = ci.KeyContainerName;
                        break;
                    }


                }
                //Sign from SmartCard
                //note : ProviderName and KeyContainerName can be found with the dos command : CertUtil -ScInfo
                string ProviderName = ProviderNameCert;
                string KeyContainerName = KeyContainerNameCert;
                string PinCode = textBox4.Text;
                if (PinCode != "")
                {
                    //if pin code is set then no windows form will popup to ask it
                    SecureString pwd = GetSecurePin(PinCode);
                    CspParameters csp = new CspParameters(1,
                                                            ProviderName,
                                                            KeyContainerName,
                                                            new System.Security.AccessControl.CryptoKeySecurity(),
                                                            pwd);
                    try
                    {
                        RSACryptoServiceProvider rsaCsp = new RSACryptoServiceProvider(csp);
                        // the pin code will be cached for next access to the smart card
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Crypto error: " + ex.Message);
                        debug(string.Format("{0}", ex.Message));
                        return;
                    }
                }
                X509Store store = new X509Store(StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2 cert = null;
                if ((ProviderName == "") || (KeyContainerName == ""))
                {
                    MessageBox.Show("You must set Provider Name and Key Container Name");
                    debug(string.Format("{0}", "Verificare il certificato selezionato"));
                    return;
                }

                debug(string.Format("{0}", "Verifica corrispondenza certificato selezionato"));

                foreach (X509Certificate2 cert2 in store.Certificates)
                {
                    if (cert2.HasPrivateKey)
                    {
                        RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)cert2.PrivateKey;
                        if (rsa == null) continue; // not smart card cert again
                        if (rsa.CspKeyContainerInfo.HardwareDevice) // sure - smartcard
                        {
                            if ((rsa.CspKeyContainerInfo.KeyContainerName == KeyContainerName) && (rsa.CspKeyContainerInfo.ProviderName == ProviderName))
                            {
                                //we find it
                                cert = cert2;
                                break;
                            }
                        }
                    }
                }
                if (cert == null)
                {
                    MessageBox.Show("Certificate not found");
                    debug(string.Format("{0}", "Verificare il certificato selezionato, non è presente sul dispositivo"));
                    return;
                }

                cerSelected = cert;

            }

            debug(string.Format("Firma file {0} in corso..........", textBox1.Text));
            SignWithThisCert(cerSelected);
            debug(string.Format("Firma file {0} completata", textBox1.Text));
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
