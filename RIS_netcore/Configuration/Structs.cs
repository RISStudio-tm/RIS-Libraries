using System;

namespace RIS.Configuration
{
    public struct AppConfigElement
    {
        public string Name { get; }
        public string XmlPath { get; }

        internal AppConfigElement(string name, string xmlPath)
        {
            Name = name;
            XmlPath = xmlPath;
        }
    }
}
