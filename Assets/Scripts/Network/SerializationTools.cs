using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace SerializationTool
{

    public class SerializationTools
    {
        static public Byte[] Serialize<T>(T data)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();

            try
            {
                binaryFormatter.Serialize(memoryStream, data);
                memoryStream.Position = 0;

                return memoryStream.GetBuffer();
            }
            finally
            {
                memoryStream.Close();
            }
        }

        static public object Deserialize(Byte[] serialData)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream(serialData);

            try
            {
                object data = binaryFormatter.Deserialize(memoryStream);

                return data;
            }

            finally
            {
                memoryStream.Close();
            }
        }
    }
}
