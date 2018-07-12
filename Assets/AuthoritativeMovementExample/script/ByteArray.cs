using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AuthMovementExample
{
    /// <summary>
    ///	Binary (De)serializer using BinaryFormatter
    /// </summary>
    public class ByteArray
    {
        private static BinaryFormatter _bf = new BinaryFormatter();
        
        /// <summary>
        ///		Serialize the object to a binary byte array
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] Serialize(Object obj)
        {
            _bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                _bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        /// <summary>
        ///		Deserialize a binary byte array to an object
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Object Deserialize(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Seek(0, SeekOrigin.Begin);
                Object obj = _bf.Deserialize(ms);
                return obj;
            }
        }
    }
}