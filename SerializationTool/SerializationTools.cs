using System;
using System.Collections.Generic;
using System.Text;
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

    [Serializable]
    public class Player : ISerializable
    {
        public float speed = 5;
        public float pos = 2;

        public Player() { }

        public Player(SerializationInfo info, StreamingContext ctxt)
        {
            speed = (float)info.GetValue("speed", typeof(float));
            pos = (float)info.GetValue("pos", typeof(float));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("speed", speed, typeof(float));
            info.AddValue("pos", pos, typeof(float));
        }
    }
}
