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
            });
        }

        public static async void ReadLocalData(Action<LocalData> action)
        {
            action(await Task.Run(ReadLocalData));
        }

        private static LocalData ReadLocalData()
        {
            if (File.Exists(path))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(path, FileMode.Open);
                LocalData data = formatter.Deserialize(stream) as LocalData;
                stream.Close();

                return data;
            }

            return null;
        }
    }
}
