using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AuthMovementExample
{
    /* 
     * Byte array to Object serializer/deserializer
     */
    public class ByteArrayUtilities
    {
        // Convert an Object to a byte array
        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        // Convert a byte array to an Object
        public static Object ByteArrayToObject(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                ms.Write(bytes, 0, bytes.Length);
                ms.Seek(0, SeekOrigin.Begin);
                Object obj = bf.Deserialize(ms);
                return obj;
            }
        }
    }
}