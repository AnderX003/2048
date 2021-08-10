using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;

namespace _Scripts
{
    public static class LocalDataManager
    {
        private static readonly string path = Application.persistentDataPath + "/localdata.sas";

        public static async void WriteLocalData(LocalData data)
        {
            await Task.Run(() =>
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(path, FileMode.Create);

                formatter.Serialize(stream, data);
                stream.Close();
                //Debug.Log("Data Written");
            });
        }

        public static async void ReadLocalData(Action<LocalData> action)
        {
            await Task.Run(() =>
            {
                action(ReadLocalData());
            });
        }

        public static LocalData ReadLocalData()
        {
            if (File.Exists(path))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(path, FileMode.Open);
                LocalData data = formatter.Deserialize(stream) as LocalData;
                stream.Close();

                //Debug.Log("Data Read\n" + data?.LastGameMode);
                return data;
            }

            //Debug.Log("No Data to Read");
            return null;
        }
    }
}
