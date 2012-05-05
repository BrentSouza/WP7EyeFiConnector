using System.Xml.Linq;
using System;

namespace EyeFiConnector
{
    public static class EyeFiSOAPMessages
    {
        #region Response XML

        public static XDocument GetStartSessionResponseXML(string credential, string snonce, string transferMode, string transferModeTimestamp)
        {
            XNamespace soapEnv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace ns1 = "http://localhost/api/soap/eyefilm";
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(soapEnv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/"),
                    new XElement(soapEnv + "Body",
                        new XElement(ns1 + "StartSessionResponse",
                            new XAttribute("xmlns", "http://localhost/api/soap/eyefilm"),
                            new XElement(ns1 + "credential", credential),
                            new XElement(ns1 + "snonce", snonce),
                            new XElement(ns1 + "transfermode", transferMode),
                            new XElement(ns1 + "transfermodetimestamp", transferModeTimestamp),
                            new XElement(ns1 + "upsyncallowed", "true")
                        )
                    )
                )
            );

            return doc;
        }

        public static XDocument GetPhotoStatusResponseXML(int fileId)
        {
            XNamespace soapEnv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace ns1 = "http://localhost/api/soap/eyefilm";
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(soapEnv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/"),
                    new XElement(soapEnv + "Body",
                        new XElement(ns1 + "GetPhotoStatusResponse",
                            new XAttribute("xmlns", "http://localhost/api/soap/eyefilm"),
                            new XElement(ns1 + "fileid", fileId),
                            new XElement(ns1 + "offset", "0")
                        )
                    )
                )
            );

            return doc;
        }

        public static XDocument GetUploadPhotoResponseXML(string status)
        {
            XNamespace soapEnv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace ns1 = "http://localhost/api/soap/eyefilm";
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(soapEnv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/"),
                    new XElement(soapEnv + "Body",
                        new XElement(ns1 + "UploadPhotoResponse",
                            new XAttribute("xmlns", "http://localhost/api/soap/eyefilm"),
                            new XElement(ns1 + "success", status)
                        )
                    )
                )
            );

            return doc;
        }

        public static XDocument GetMarkLastPhotoInRollResponseXML()
        {
            XNamespace soapEnv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace ns1 = "http://localhost/api/soap/eyefilm";
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(soapEnv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/"),
                    new XElement(soapEnv + "Body",
                        new XElement(ns1 + "MarkLastPhotoInRollResponse",
                            new XAttribute("xmlns", "http://localhost/api/soap/eyefilm")
                        )
                    )
                )
            );

            return doc;
        }

        #endregion

        #region Request XML

        public static XDocument GetStartSessionXML(string macAddress, string cnonce, string transferMode, string transferModeTimestamp)
        {
            XNamespace soapEnv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace ns1 = "EyeFi/SOAP/EyeFilm";
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(soapEnv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/"),
                    new XAttribute(XNamespace.Xmlns + "ns1", "EyeFi/SOAP/EyeFilm"),
                    new XElement(soapEnv + "Body",
                        new XElement(ns1 + "StartSession",
                            new XElement(ns1 + "macaddress", macAddress),
                            new XElement(ns1 + "cnonce", cnonce),
                            new XElement(ns1 + "transfermode", transferMode),
                            new XElement(ns1 + "transfermodetimestamp", transferModeTimestamp)
                        )
                    )
                )
            );

            return doc;
        }

        public static XDocument GetPhotoStatusXML(string credential, string macAddress, string fileName, double fileSize, string fileSignature)
        {
            XNamespace soapEnv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace ns1 = "EyeFi/SOAP/EyeFilm";
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(soapEnv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/"),
                    new XAttribute(XNamespace.Xmlns + "ns1", "EyeFi/SOAP/EyeFilm"),
                    new XElement(soapEnv + "Body",
                        new XElement(ns1 + "GetPhotoStatus",
                            new XElement("credential", credential),
                            new XElement("macaddress", macAddress),
                            new XElement("filename", fileName),
                            new XElement("filesize", fileSize.ToString()),
                            new XElement("filesignature", fileSignature)
                        )
                    )
                )
            );

            return doc;
        }

        public static XDocument GetUploadPhotoXML(int fileId, string macAddress, string fileName, double fileSize, string fileSignature, string encryption)
        {
            XNamespace soapEnv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace ns1 = "EyeFi/SOAP/EyeFilm";
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(soapEnv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/"),
                    new XAttribute(XNamespace.Xmlns + "ns1", "EyeFi/SOAP/EyeFilm"),
                    new XElement(soapEnv + "Body",
                        new XElement(ns1 + "UploadPhoto",
                            new XElement("fileid", fileId.ToString()),
                            new XElement("macaddress", macAddress),
                            new XElement("filename", fileName),
                            new XElement("filesize", fileSize.ToString()),
                            new XElement("filesignature", fileSignature),
                            new XElement("encryption", encryption)
                        )
                    )
                )
            );

            return doc;
        }

        public static XDocument GetSOAPFaultXML(string faultValue, string faultText)
        {
            XNamespace soapEnv = "http://schemas.xmlsoap.org/soap/envelope/";
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(soapEnv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/"),
                    new XElement(soapEnv + "Body",
                        new XElement(soapEnv + "Fault",
                            new XElement(soapEnv + "Code",
                                new XElement(soapEnv + "Value", faultValue)
                            ),
                            new XElement(soapEnv + "Reason",
                                new XElement(soapEnv + "Text",
                                    new XAttribute(XNamespace.Xml + "lang", "en-US"),
                                    faultText
                                )
                            )
                        )
                    )
                )
            );

            return doc;
        }

        #endregion

    }
}
