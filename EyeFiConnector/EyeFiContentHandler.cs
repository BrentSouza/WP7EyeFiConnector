using System.Linq;
using System.Xml.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EyeFiConnector
{
    // Eye Fi XML SAX ContentHandler
    public class EyeFiContentHandler
    {
        // These are the element names to parse out of the XML        
        public string MacAddress { get; set; }
        public string Cnonce { get; set; }
        public string TransferMode { get; set; }
        public string TransferModeTimeStamp { get; set; }
        public int FileId { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string FileSignature { get; set; }
        public string Credential { get; set; }

        public EyeFiContentHandler(XDocument doc)
        {            
            MacAddress = (string)(from xml in doc.Descendants("macaddress") select xml).FirstOrDefault();
            Cnonce = (string)(from xml in doc.Descendants("cnonce") select xml).FirstOrDefault();
            TransferMode = (string)(from xml in doc.Descendants("transfermode") select xml).FirstOrDefault();
            TransferModeTimeStamp = (string)(from xml in doc.Descendants("transfermodetimestamp") select xml).FirstOrDefault();
            int? fileId = (int?)(from xml in doc.Descendants("fileid") select xml).FirstOrDefault();
            FileName = (string)(from xml in doc.Descendants("filename") select xml).FirstOrDefault();
            long? fileSize = (long?)(from xml in doc.Descendants("filesize") select xml).FirstOrDefault();
            FileSignature = (string)(from xml in doc.Descendants("filesignature") select xml).FirstOrDefault();
            Credential = (string)(from xml in doc.Descendants("credential") select xml).FirstOrDefault();

            if (fileId != null)
                FileId = (int)fileId;

            if (fileSize != null)
                FileSize = (long)fileSize;
        }

    }
}
