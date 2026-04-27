using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Database.DataReader
{
    public class RawJsonReader : IDataReader
    {
        public List<DataFrame> Read(string path)
        {
            Debug.Log($"Read {path}");
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    return ReadJSON(sr.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new List<DataFrame>();
            }
        }

        public List<DataFrame> ReadJSON(string json)
        {
            List<DataFrame> dfs = new List<DataFrame>();
            JObject jObject = JObject.Parse(json);
            foreach (var jSheet in jObject)
            {
                dfs.Add(ReadSheet(jSheet.Key, jSheet.Value));
            }

            return dfs;
        }

        public DataFrame ReadSheet(string name, JToken jSheet)
        {
            DataFrame df = new DataFrame(name);
            if (jSheet["type"] == null)
                return df;

            JObject jType = (JObject)jSheet["type"];
            Dictionary<string, int> nameIdx = new Dictionary<string, int>();
            int idx = 0;
            List<string> varNames = new List<string>();
            List<string> types = new List<string>();
            foreach (var jVar in jType)
            {
                string varName = jVar.Key;
                if (string.IsNullOrWhiteSpace(varName))
                    continue;

                nameIdx[varName] = idx++;
                varNames.Add(varName);
                types.Add(jVar.Value.ToString());
            }

            df.varNames = varNames.ToArray();
            df.types = types.ToArray();

            var comments = new string[types.Count];
            if (jSheet["comment"] != null)
            {
                foreach (var jComment in jSheet["comment"] as JObject)
                {
                    var varName = jComment.Key;
                    if (string.IsNullOrWhiteSpace(varName))
                        continue;
                    if (nameIdx.ContainsKey(varName))
                        comments[nameIdx[varName]] = jComment.Value.ToString();
                }
            }

            df.comments = comments;
            List<string[]> data = new List<string[]>();
            if (jSheet["data"] == null)
            {
                df.data = Array.Empty<string[]>();
                return df;
            }

            foreach (JObject jDataSection in jSheet["data"])
            {
                string[] dataLine = new string[types.Count];
                foreach (var jData in jDataSection)
                {
                    var varName = jData.Key;
                    if (nameIdx.ContainsKey(varName))
                        dataLine[nameIdx[varName]] = ToDataCellString(jData.Value);
                }

                data.Add(dataLine);
            }

            df.data = data.ToArray();
            return df;
        }

        private static string ToDataCellString(JToken token)
        {
            if (token == null)
            {
                return null;
            }

            return token.Type == JTokenType.Array || token.Type == JTokenType.Object
                ? token.ToString(Formatting.None)
                : token.ToString();
        }
    }
}
