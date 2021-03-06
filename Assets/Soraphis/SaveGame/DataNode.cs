﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// This is an moddified version from DataNode class found in tasharen.com's game windward
/// i found it while modding, and thought its a nice way of serialize game-data
/// </summary>

namespace Assets.Soraphis.SaveGame {
    
    [Serializable]
    public class DataNode {

        public object Value;
        public List<DataNode> Children = new List<DataNode>();

        public string Name;

        public System.Type type {
            get {
                if (this.Value != null)
                    return this.Value.GetType();
                return typeof(void);
            }
        }

        public T Get<T>(T defaultVal = default(T)) {
            if (Value is T) return (T)Value;
            return defaultVal;
        }

        public DataNode AddChild(DataNode dataNode) {
            this.Children.Add(dataNode);
            return dataNode;
        }

        public DataNode AddChild(string name = "", object value = null, bool overwrite = true) {
            if(Children.Any(node => node.Name == name))
                if(!overwrite)
                    return Children.Find(node => node.Name == name);
                else {
                    var child = Children.Find(node => node.Name == name);
                    child.Value = value;
                    return child;
                }
            
            DataNode dataNode = new DataNode();
            this.Children.Add(dataNode);
            dataNode.Name = name;
            dataNode.Value = value;
            return dataNode;
        }

        public DataNode GetChild(string name, bool crateifMissing = false) {
            if (Children.Any(node => node.Name == name))
                return Children.Find(node => node.Name == name);
            if(crateifMissing) return AddChild(name);
            return null;
        }

        public void Merge(DataNode other, bool overwrite = false) {
            if(other == null) return;

            if(overwrite || this.Value == null) this.Value = other.Value;
            foreach(DataNode t in other.Children) {
                if(t == this) continue;
                this.GetChild(t.Name, true).Merge(t, overwrite);
            }
        }

        public void Write(StreamWriter writer, int tab = 0) {
            DataNode.Write(writer, tab, !string.IsNullOrEmpty(Name) ? Name : "Node", Value);
            foreach (var node in Children) {
                node.Write(writer, tab + 1);
            }
            if (tab != 0) return;
            writer.Flush();
        }

        private static void Write(StreamWriter writer, int tab, string name, object value) {
            if (string.IsNullOrEmpty(name) && value == null) return;
            WriteTabs(writer, tab);
            if (name != null) {
                writer.Write(Escape(name));
                if (value == null) {
                    writer.Write('\n');
                    return;
                }
            }

            if (name != null) writer.Write(" = \"");
            // write value:
            IFormatter formatter = new BinaryFormatter();
            byte[] bytevalue;

            using (MemoryStream stream = new MemoryStream()) {
                formatter.Serialize(stream, value);
                bytevalue = stream.ToArray();
            }
            var t1 = BitConverter.ToString(bytevalue);
            //var t2 = Escape(t1);
            writer.Write(t1);
            // ----
            if (name != null) writer.Write('"');
            writer.Write('\n');
        }

        private static void WriteTabs(StreamWriter writer, int tab) {
            for (int i = 0; i < tab; ++i) {
                writer.Write("\t");
            }
        }

        private static string Escape(string val) {
            if (!string.IsNullOrEmpty(val)) {
                val = val.Replace("\n", "\\n");
                val = val.Replace("\t", "\\t");
            }
            return val;
        }

        private static string Unescape(string val) {
            if (!string.IsNullOrEmpty(val)) {
                val = val.Replace("\\n", "\n");
                val = val.Replace("\\t", "\t");
            }
            return val;
        }

        public static DataNode Read(TextReader reader) {
            string nl = reader.ReadLine()?.Trim();
            int offset = CalculateOffset(nl);
            DataNode dn = new DataNode();
            dn.Read(reader, nl, ref offset);
            return dn;
        }

        private static int CalculateOffset(string line) {
            if(line == null) return 0;
            for (int i = 0; i < line.Length; ++i) {
                if ((char)line[i] != '\t') {
                    return i;
                }
            }
            return 0;
        }

        private string Read(TextReader reader, string line, ref int offset) {
            if (line != null) {
                int startIndex = offset;
                int num = line.IndexOf("=", startIndex);
                if(num == -1) {
                    Name = Unescape(line.Substring(offset)).Trim();
                    Value = null;
                } else {
                    var r1 = line.Substring(offset, num - offset);
                    Name = Unescape(r1).Trim();
                    var strValue = Unescape(line.Substring(num + 1)).Trim().Trim('"');


                    String[] arr = strValue.Split('-');
                    byte[] bytes = new byte[arr.Length];
                    for (int i = 0; i < arr.Length; i++)
                        bytes[i] = Convert.ToByte(arr[i], 16);


                    IFormatter formatter = new BinaryFormatter();
                    using (var ms = new MemoryStream(bytes)) {
                        Value = formatter.Deserialize(ms);
                    }
                }
                line = reader.ReadLine();
                offset = CalculateOffset(line);
                
                while(line != null && offset == startIndex + 1) {
                    line = this.AddChild().Read(reader, line, ref offset);
                }

            }
            return line;
        }

        public string PrintAll() {
            string s = "";

            s += $"[{Name} = {Value}, ";
            foreach(var child in Children) {
                s += child.PrintAll();
            }
            if(Children.Count == 0) s += "[]";
            s += "]";

            return s;
        }
    }
}
