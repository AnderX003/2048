using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace _Scripts
{
    public static class LocalDataManager
    {
        private static readonly string path = Application.persistentDataPath+ "/localdata.sas";
        public static void WriteLocalData(LocalData data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Create);
        
            formatter.Serialize(stream, data);
            stream.Close();
            Debug.Log("Data Written");
        }

        public static LocalData ReadLocalData()
        {
            if (File.Exists(path))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(path, FileMode.Open);
                LocalData data = formatter.Deserialize(stream) as LocalData;
                stream.Close();

                Debug.Log("Data Read\n"+ data.LastGameMode);
                return data;
            }
            else
            {
                Debug.Log("No Data to Read");
                return null;
            }
        }

    }
}
