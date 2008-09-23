using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Golem.Core
{
    public class Configuration
    {
        public static bool ConfigFileExists
        {
            get
            {
                return File.Exists(Environment.CurrentDirectory + "\\" + DEFAULT_FILE_NAME);   
            }
        }
        public static readonly string DEFAULT_FILE_NAME = "golem.xml";

        private FileInfo _file;
        private ConfigurationMemento _memento;
        
        public Configuration()
        {
            _memento = new ConfigurationMemento();
            _file = new FileInfo(Environment.CurrentDirectory + "\\" + DEFAULT_FILE_NAME);

            if (_file.Exists)
                LoadFrom(_file);
            else
                CreateNew(_file);

        }

        public bool IsNew{ get; set;}

        /// <summary>
        /// Paths where DLLs or EXEs containing recipes are. 
        /// Can be relative or absolute,and contain * wildcard
        /// </summary>
        public List<string> RecipeSearchHints { get { return _memento.RecipeSearchHints; } }

        private void CreateNew(FileInfo file)
        {
            WriteFile(file);
            IsNew = true;
        }

        private void WriteFile(FileInfo file)
        {
            using(TextWriter writer = file.CreateText())
            {
                var s = new XmlSerializer(typeof (ConfigurationMemento));
                s.Serialize(writer,_memento);
            }
        }

        private void LoadFrom(FileInfo file)
        {
            IsNew = false;
            var s = new XmlSerializer(typeof(ConfigurationMemento));
            using(var reader = file.OpenText())
            {
                _memento =  (ConfigurationMemento) s.Deserialize(reader);
            }
        }

        public void Save()
        {
            WriteFile(_file);   
        }
    }

    public class ConfigurationMemento
    {
        public ConfigurationMemento()
        {
            RecipeSearchHints = new List<string>();
        }
        public List<string> RecipeSearchHints { get; set; }
    }
}