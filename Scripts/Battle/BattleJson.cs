using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;

namespace BattleJson
{

    [System.Runtime.Serialization.DataContract]
    public class Summon
    {
        [System.Runtime.Serialization.DataMember()]
        public bool player { get; set; }

        [System.Runtime.Serialization.DataMember()]
        public string name { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string sex { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public bool item { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public float hpPer { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string hpText { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public int[] rank { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public int error { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public int errorTurn { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public int[] error2 { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public int[] buffer { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public int lockSkill { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string type { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string exception { get; set; }

        public new string ToString()
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Summon));
            MemoryStream ms = new MemoryStream();
            serializer.WriteObject(ms, this);
            return Encoding.UTF8.GetString(ms.ToArray());
        }
        public static Summon ToSummon(string str)
        {
            Debug.Log(str);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Summon));
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            MemoryStream ms = new MemoryStream(bytes);
            return (Summon)serializer.ReadObject(ms);
        }
    }
    [System.Runtime.Serialization.DataContract]
    public class Select
    {
        [System.Runtime.Serialization.DataMember()]
        public bool player { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public bool skill { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public bool member { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public int num { get; set; }


        public new string ToString()
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Select));
            MemoryStream ms = new MemoryStream();
            serializer.WriteObject(ms, this);
            return Encoding.UTF8.GetString(ms.ToArray());
        }
        public static Select ToSelect(string str)
        {
            Debug.Log(str);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Select));
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            MemoryStream ms = new MemoryStream(bytes);
            return (Select)serializer.ReadObject(ms);
        }
    }
    [System.Runtime.Serialization.DataContract]
    public class Damage
    {
        [System.Runtime.Serialization.DataMember()]
        public bool player { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public float hpPer { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string hpText { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string message { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string skillNo { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public int typePer { get; set; }
        public new string ToString()
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Damage));
            MemoryStream ms = new MemoryStream();
            serializer.WriteObject(ms, this);
            return Encoding.UTF8.GetString(ms.ToArray());
        }
        public static Damage ToDamage(string str)
        {
            Debug.Log(str);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Damage));
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            MemoryStream ms = new MemoryStream(bytes);
            return (Damage)serializer.ReadObject(ms);
        }
    }
    public class Effect
    {
        [System.Runtime.Serialization.DataMember()]
        public bool player { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string name { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string message { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string skillNo { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public int[] rank { get; set; }
        public int error { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public int errorTurn { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public int[] error2 { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public int[] buffer { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public int lockSkill { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public bool item { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string effect { get; set; }
        public new string ToString()
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Effect));
            MemoryStream ms = new MemoryStream();
            serializer.WriteObject(ms, this);
            return Encoding.UTF8.GetString(ms.ToArray());
        }
        public static Effect ToEffect(string str)
        {
            Debug.Log(str);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Effect));
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            MemoryStream ms = new MemoryStream(bytes);
            return (Effect)serializer.ReadObject(ms);
        }
    }
    public class Area
    {
        [System.Runtime.Serialization.DataMember()]
        public string message { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string skillNo { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public int[] both { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public int[] player { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public int[] enemy { get; set; }
        public new string ToString()
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Area));
            MemoryStream ms = new MemoryStream();
            serializer.WriteObject(ms, this);
            Debug.Log(Encoding.UTF8.GetString(ms.ToArray()));
            return Encoding.UTF8.GetString(ms.ToArray());
        }
        public static Area ToArea(string str)
        {
            Debug.Log(str);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Area));
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            MemoryStream ms = new MemoryStream(bytes);
            return (Area)serializer.ReadObject(ms);
        }
    }
}
