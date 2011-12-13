using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Ionic.Zip;

namespace OSBIDE.Library.Events
{
    public class EventFactory
    {
        /// <summary>
        /// Converts the supplied IOsbideEvent into a zipped, binary format
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        public static byte[] ToZippedBinary(IOsbideEvent evt)
        {
            MemoryStream memStream = new MemoryStream();
            MemoryStream zipStream = new MemoryStream();
            BinaryFormatter serializer = new BinaryFormatter();
            serializer.Serialize(memStream, evt);

            //go back to position zero so that the zip file can read the memory stream
            memStream.Position = 0;

            //zip up to save space
            using (ZipFile zip = new ZipFile())
            {
                ZipEntry entry = zip.AddEntry(evt.EventName, memStream);
                zip.Save(zipStream);
                zipStream.Position = 0;
            }
            return zipStream.ToArray();
        }

        /// <summary>
        /// Converts a zipped, binary format of IOsbideEvent back into object form
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static IOsbideEvent FromZippedBinary(byte[] data)
        {
            MemoryStream zippedStream = new MemoryStream(data);
            MemoryStream rawStream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            string FileName = "";

            //unzip the memory stream
            using (ZipFile zip = ZipFile.Read(zippedStream))
            {
                if (zip.Entries.Count == 1)
                {
                    ZipEntry entry = zip.Entries.ElementAt(0);
                    FileName = entry.FileName;
                    entry.Extract(rawStream);
                    rawStream.Position = 0;
                }
                else
                {
                    throw new Exception("Expecting a zip file with exactly one item.");
                }
            }

            //figure out what needs to be deserialized
            IOsbideEvent evt = (IOsbideEvent)formatter.Deserialize(rawStream);            
            return evt;
        }
    }
}
