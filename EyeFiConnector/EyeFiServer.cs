using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using System.Xml.Linq;
using HttpServer;
using HttpServer.BodyDecoders;
using HttpServer.Headers;
using HttpServer.Messages;
using SharpCompress.Archive.Tar;
using SharpCompress.Reader;
using SharpCompress.Common;
using System.Linq;
using System.Windows.Media.Imaging;
using Microsoft.Phone;
using System.Windows.Threading;

namespace EyeFiConnector
{
    public class EyeFiServer
    {
        public string ServerNonce;

        public event EventHandler<PictureReceivedEventArgs> OnPictureReceived;

        private void NotifyPictureReceived(string imagePath)
        {
            EventHandler<PictureReceivedEventArgs> handler = OnPictureReceived;
            if (null != handler)
            {
                SmartDispatcher.BeginInvoke(() =>
                {
                    handler(this, new PictureReceivedEventArgs(imagePath));
                });
            }
        }

        public EyeFiServer()
        {
            // Generate a nonce to be used by the server. The nonce should be very hard if not
            // impossible to predict. The method used here is to MD5 hash a random number.
            ServerNonce = MD5Core.GetHashString(new Random().Next().ToString());

            // Setup the http server
            new System.Threading.Thread(() =>
            {
                HttpListener listener = HttpListener.Create(Homebrew.Net.IPAddress.Any, 59278);
                listener.RequestReceived += new EventHandler<RequestEventArgs>(server_RequestReceived);
                listener.Start(1);
            }).Start();
        }

        void server_RequestReceived(object sender, RequestEventArgs e)
        {
            IHttpContext context = e.Context;
            IRequest request = e.Request;
            IResponse response = e.Response;
            string path = request.Uri.AbsolutePath;
            path = Uri.UnescapeDataString(path);
            
            Utilities.Log("Connection from " + context.RemoteEndPoint, false);
            Utilities.Log("  Method: " + request.Method.ToString(), false);
            Utilities.Log("  Path: " + path, false);

            try
            {
                if (request.Method == Method.Get)
                {
                    string output = "";

                    response.Add(new ContentTypeHeader("text/html"));
                    response.Add(new StringHeader("Server", "Eye-Fi Agent/3.4.26 (Windows 7 Ultimate Edition Service Pack 1, 32-bit)"));

                    output += context.RemoteEndPoint;
                    foreach (IHeader h in response.Headers)
                    {
                        output += h.Name + ": " + h.ToString();
                    }
                    SendFile(context, response, output);
                }
                else if (request.Method == Method.Post)
                {
                    response.Add(new DateHeader("Date", DateTime.Now));
                    response.Add(new StringHeader("Pragma", "no-cache"));
                    response.Add(new StringHeader("Server", "Eye-Fi Agent/3.4.26 (Windows 7 Ultimate Edition Service Pack 1, 32-bit)"));
                    response.Add(new ContentTypeHeader("text/xml; charset=\"utf-8\""));

                    string soapAction = (from x in request.Headers
                                where x.Name == "SOAPAction"
                                select x.ToString()).FirstOrDefault();

                    Utilities.Log("  SOAP Action: " + soapAction, false);

                    // A soapAction of StartSession indicates the beginning of an EyeFi auth request
                    if (path == "/api/soap/eyefilm/v1" && soapAction == "\"urn:StartSession\"")
                        SendFile(context, response, StartSession(request.Body));

                    // GetPhotoStatus allows the card to query if a photo has been uploaded to the server yet
                    else if (path == "/api/soap/eyefilm/v1" && soapAction == "\"urn:GetPhotoStatus\"")
                        SendFile(context, response, GetPhotoStatus(request.Body));
                    
                    // If the URL is upload and there's no soapAction the card is ready to send a picture
                    else if (path == "/api/soap/eyefilm/v1/upload" && soapAction == null)
                        SendFile(context, response, UploadPhoto(request));

                    // If the URL is upload and SOAPAction is MarkLastPhotoInRoll
                    else if (path == "/api/soap/eyefilm/v1/upload" && soapAction == "\"urn:MarkLastPhotoInRoll\"")
                        SendFile(context, response, MarkLastPhotoInRoll());
                }
            }
            catch (Exception ex)
            {
                SendFile(context, response, ex.Message);
            }
            
        }

        private void SendFile(IHttpContext context, IResponse response, string data)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(
                Encoding.UTF8.GetBytes(data));

            SendFile(context, response, ms);
        }

        private void SendFile(IHttpContext context, IResponse response, System.IO.Stream stream)
        {
            response.ContentLength.Value = stream.Length;

            var generator = HttpFactory.Current.Get<ResponseWriter>();
            generator.SendHeaders(context, response);
            generator.SendBody(context, stream);
        }

        private string StartSession(Stream data)
        {
            Utilities.Log("  StartSession Begin", false);
            
            XDocument doc = XDocument.Load(data);
            EyeFiContentHandler contentHandler = new EyeFiContentHandler(doc);
            
            string credentialString = contentHandler.MacAddress + contentHandler.Cnonce + App.ViewModel.EyeFiUploadKey;
            byte[] binaryCredentials = Utilities.HexToBytes(credentialString);
            string credential = MD5Core.GetHashString(binaryCredentials).ToLower();

            doc = EyeFiSOAPMessages.GetStartSessionResponseXML(credential, App.ViewModel.EyeFiServerInstance.ServerNonce, contentHandler.TransferMode, contentHandler.TransferModeTimeStamp);
            return Utilities.EncodeXml(doc, UTF8Encoding.UTF8);
        }

        // GetPhotoStatus allows the Eye-Fi card to query the server as to the current uploaded
        // status of a file. Even more important is that it authenticates the card to the server
        // by the use of the <credential> field. Essentially if the credential is correct the
        // server should allow files with the given filesignature to be uploaded.
        private string GetPhotoStatus(Stream data)
        {
            Utilities.Log("  GetPhotoStatus Begin", false);

            XDocument doc = XDocument.Load(data);
            EyeFiContentHandler contentHandler = new EyeFiContentHandler(doc);

            string credentialString = contentHandler.MacAddress + App.ViewModel.EyeFiUploadKey + App.ViewModel.EyeFiServerInstance.ServerNonce;
            byte[] binaryCredentials = Utilities.HexToBytes(credentialString);
            string credential = MD5Core.GetHashString(binaryCredentials).ToLower();

            if (contentHandler.Credential != credential)
            {
                Utilities.Log("  Authentication Error:" + Environment.NewLine + "    EyeFi Credential: " + contentHandler.Credential + Environment.NewLine + "    App Credential: " + credential, false);
                return "Authentication error";
            }

            doc = EyeFiSOAPMessages.GetPhotoStatusResponseXML(App.ViewModel.EyeFiFileId);
            
            App.ViewModel.EyeFiFileId++;

            return Utilities.EncodeXml(doc, UTF8Encoding.UTF8);
        }

        // Handles receiving the actual photograph from the card.
        // data will most likely contain multipart binary post data that needs to be parsed
        private string UploadPhoto(IRequest request)
        {
            Utilities.Log("  UploadPhoto Begin", false);
            
            // Parse the multipart/form-data
            MultiPartDecoder decoder = new MultiPartDecoder();
            DecodedData data = decoder.Decode(request.Body, request.ContentType, request.Encoding);
            
            // Parse the SOAPENVELOPE using the EyeFiContentHandler()
            XDocument doc = XDocument.Parse(data.Parameters.Get("SOAPENVELOPE").Value);
            EyeFiContentHandler contentHandler = new EyeFiContentHandler(doc);         

            // Get the newly uploaded file into memory
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream untrustedFile = new IsolatedStorageFileStream(data.Files["FILENAME"].TempFileName,FileMode.Open,store))
                {
                    // Integrity check the file before writing it out
                    string verifiedDigest = EyeFiCrypto.CalculateIntegrityDigest(Utilities.StreamToBytes(untrustedFile), App.ViewModel.EyeFiUploadKey);
                    string unverifiedDigest = data.Parameters.Get("INTEGRITYDIGEST").Value;

                    Utilities.Log("  Comparing my digest [" + verifiedDigest + "] to card's digest [" + unverifiedDigest + "].", false);

                    if (verifiedDigest == unverifiedDigest)
                    {
                        TarArchive tarball = TarArchive.Open(untrustedFile);
                        using (IReader reader = tarball.ExtractAllEntries())
                        {
                            while (reader.MoveToNextEntry())
                            {
                                if (!reader.Entry.IsDirectory &! reader.Entry.FilePath.Contains(".log"))
                                {
                                    string imageName = (reader.Entry.FilePath).Substring(0, reader.Entry.FilePath.IndexOf('.') + 4);
                                    string imagePath = Path.Combine(App.ViewModel.DownloadLocation, imageName);
                                                                       
                                    Utilities.Log("  Extracting image to " + imagePath, false);

                                    using (IsolatedStorageFileStream image = new IsolatedStorageFileStream(imagePath, FileMode.Create, store))
                                    {
                                        reader.WriteEntryTo(image);
                                    }
                                    
                                    NotifyPictureReceived(imagePath);
                                }
                            }
                        }
                        doc = EyeFiSOAPMessages.GetUploadPhotoResponseXML("true");
                        return Utilities.EncodeXml(doc, UTF8Encoding.UTF8);
                    }
                    else
                    {
                        doc = EyeFiSOAPMessages.GetUploadPhotoResponseXML("false");
                        return Utilities.EncodeXml(doc, UTF8Encoding.UTF8);
                    }
                }
            }
        }

        // Handles MarkLastPhotoInRoll action
        private string MarkLastPhotoInRoll()
        {
            XDocument doc = EyeFiSOAPMessages.GetMarkLastPhotoInRollResponseXML();
            return Utilities.EncodeXml(doc, UTF8Encoding.UTF8);
        }
    }
}
