using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    class Config
    {
        public SortedList<string, List<string>> config;
        public int size;
        public Config()
        {
            config = new SortedList<string, List<string>>();
            size = 0;
        }
        public Config(string str)
        {
            config = new SortedList<string, List<string>>();
            SetConfig(str);
            size = config.Count;
        }

        public string ToString()
        {
            string st = "";
            for (int i = 0; i < size; i++)
            {

                string key = config.Keys[i];
                st += key + "=";
                List<string> values;
                config.TryGetValue(key, out values);
                for (int j = 0; j < values.Count; j++)
                {
                    st += values[j];
                    if (i + 1 != values.Count) st += ",";
                }
                if (i + 1 != size) st += ";";
            }
            return st;

        }

        //add and set

        public void SetConfig(string str)
        {
            string[] prms = str.Split(';');
            for (int i = 0; i < prms.Length; i++)
            {
                string[] prm = prms[i].Split('=');
                SetParam(prm[0], prm[1].Split(','));
            }
        }
        public void SetParam(string key, string value)
        {
            config.Add(key, new string[] { value }.ToList());
            size = config.Count;
        }
        public void SetParam(string key, string[] values)
        {
            config.Add(key, values.ToList());
            size = config.Count;
        }
        public void SetParam(string key, List<string> values)
        {
            config.Add(key, values);
            size = config.Count;
        }
        public void AddValue(string key, string value)
        {
            try
            {
                List<string> values;
                config.TryGetValue(key, out values);
                values.Add(value);
                SetParam(key, values);
            }
            catch
            {

            }
        }

        //get

        public string[] GetValues(string key)
        {

            List<string> values;
            bool b = config.TryGetValue(key, out values);
            if (!b) { return null; }
            return values.ToArray();

        }
        public string GetValue(string key)
        {

            List<string> values;
            bool b = config.TryGetValue(key, out values);
            if (!b) { return ""; }
            return values[0];

        }
        public byte[] GetValuesAsBytes(string key)
        {
            List<string> values;
            bool b = config.TryGetValue(key, out values);
            if (!b) { return null; }
            byte[] bytes = new byte[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                bytes[i] = byte.Parse(values[i]);
            }
            return bytes;
        }
        public int[] GetValuesAsInts(string key)
        {

            List<string> values;
            bool b = config.TryGetValue(key, out values);
            if (!b) { return null; }
            int[] bytes = new int[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                bytes[i] = int.Parse(values[i]);
            }
            return bytes;
        }
    }
}
