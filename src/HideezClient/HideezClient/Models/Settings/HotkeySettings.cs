using HideezMiddleware.Settings;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace HideezClient.Models.Settings
{
    [Serializable]
    public class HotkeySettings : BaseSettings
    {
        /// <summary>
        /// The class acts in the same way as <see cref="Tuple{T1, T2}"/> but allows Xml serialization
        /// by providing a default constructor
        /// </summary>
        public class SerializableTuple<T1, T2>
        {
            public SerializableTuple() { }

            public SerializableTuple(T1 item1, T2 item2)
            {
                Item1 = item1;
                Item2 = item2;
            }

            public T1 Item1 { get; set; }
            public T2 Item2 { get; set; }
        }

        /// <summary>
        /// Initializes new instance of <see cref="HotkeySettings"/> with default values
        /// </summary>
        public HotkeySettings()
        {
            SettingsVersion = new Version(1, 1, 0);
            Hotkeys = new Dictionary<UserAction, string>()
            {
                {UserAction.InputLogin, "Control + Alt + L" },
                {UserAction.InputPassword, "Control + Alt + P" },
                {UserAction.InputOtp, "Control + Alt + O" },
                {UserAction.AddAccount, "Control + Alt + A" },
            };
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="copy">Intance to copy from</param>
        public HotkeySettings(HotkeySettings copy)
            :base()
        {
            if (copy == null)
                return;

            SettingsVersion = (Version)copy.SettingsVersion.Clone();
            Hotkeys = new Dictionary<UserAction, string>(copy.Hotkeys.Count, copy.Hotkeys.Comparer);
            foreach (var h in copy.Hotkeys)
            {
                Hotkeys.Add(h.Key, h.Value);
            }
        }


        [Setting]
        public Version SettingsVersion { get; }

        [Setting]
        [XmlIgnore]
        public Dictionary<UserAction, string> Hotkeys { get; set; }

        /// <summary>
        /// Do not use this property. Instead use <see cref="Hotkeys"/>
        /// This property is used for serialization instead of <see cref="Hotkeys"/>, because XmlSerializer does not support
        /// serialization of types that implement IDictionary
        /// </summary>
        public List<SerializableTuple<UserAction, string>> SerializableHotkeys
        {
            get
            {
                var serializableList = new List<SerializableTuple<UserAction, string>>();
                foreach (var key in Hotkeys.Keys)
                {
                    var tuple = new SerializableTuple<UserAction, string>(key, Hotkeys[key]);
                    serializableList.Add(tuple);
                }
                return serializableList;
            }
            set
            {
                Hotkeys = new Dictionary<UserAction, string>();
                foreach (var tuple in value)
                {
                    Hotkeys.Add(tuple.Item1, tuple.Item2);
                }
            }
        }

        public override object Clone()
        {
            return new HotkeySettings(this);
        }
    }
}
