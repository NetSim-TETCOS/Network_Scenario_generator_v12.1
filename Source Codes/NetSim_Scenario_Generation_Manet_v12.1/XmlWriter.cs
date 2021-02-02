using System;
using System.Xml;
using System.IO;

namespace NetSim_Scenario_Generation_Manet
{
    class XmlWriter
    {
        private static XmlDocument outputDoc;

        /*
          func: appends the attribute to a given node
          params: node-the node to which atribute has to be appended
                  name-name of the attribute
                  value-value of the attribute
          return: doesn't return
        */
        public void add_attribute(XmlNode node, String name, String value)
        {
            XmlAttribute attrib = outputDoc.CreateAttribute(name);
            attrib.Value = value;
            node.Attributes.Append(attrib);
        }

        /*
          func: creates an Xml document with root elemet as "TETCOS_NETSIM"
          params: no params
          return: return the root created root node(element)
        */
        public XmlNode open_document()
        {
            outputDoc = new XmlDocument();
            XmlNode rootNode = outputDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            outputDoc.AppendChild(rootNode);
            rootNode = outputDoc.CreateElement("TETCOS_NETSIM");
            outputDoc.AppendChild(rootNode);
            add_attribute(rootNode, "xmlns:ns0", "http://www.w3.org/2001/XMLSchema-instance");
            add_attribute(rootNode, "ns0:noNamespaceSchemaLocation", "Configuration.xsd");

            return rootNode;
        }


        //Appending EXPERIMENT_INFORMATION for Configuration.netsim
        public XmlNode add_experimentInfo(XmlNode parent, string exp_name, string version_name, string version_number)
        {
            DateTime aDate = DateTime.Now;
            string curDate = aDate.ToString("yyyy/MM/dd HH:mm:ss");

            XmlElement verNameElement = outputDoc.CreateElement("VERSION");
            verNameElement.InnerText = version_name;
            XmlElement verNumElement = outputDoc.CreateElement("NUMBER");
            verNumElement.InnerText = version_number;
            XmlElement userElement = outputDoc.CreateElement("USER");
            userElement.InnerText = "PC";
            XmlElement nameElement = outputDoc.CreateElement("NAME");
            nameElement.InnerText = exp_name;
            XmlElement dateElement = outputDoc.CreateElement("DATE");
            dateElement.InnerText = curDate;
            XmlElement commentElement = outputDoc.CreateElement("COMMENT");
            commentElement.InnerText = "Manet Test Case";

            XmlElement expElement = outputDoc.CreateElement("EXPERIMENT_INFORMATION");

            expElement.AppendChild(verNameElement);
            expElement.AppendChild(verNumElement);
            expElement.AppendChild(userElement);
            expElement.AppendChild(nameElement);
            expElement.AppendChild(dateElement);
            expElement.AppendChild(commentElement);

            parent.AppendChild(expElement);

            return parent;
        }

        //Appends SIMULATION_PARAMETER for Configuration.netsim
        public XmlNode add_simulation_parameter(XmlNode parent, float simulation_time)
        {
            //child nodes
            XmlElement seedElement = outputDoc.CreateElement("SEED");
            add_attribute(seedElement, "SEED1", "12345678");
            add_attribute(seedElement, "SEED2", "23456789");
            XmlElement animElement = outputDoc.CreateElement("ANIMATION");
            add_attribute(animElement, "STATUS", "DISABLE");
            XmlElement interactiveElement = outputDoc.CreateElement("INTERACTIVE_SIMULATION");
            add_attribute(interactiveElement, "INPUT_FILE", "");
            add_attribute(interactiveElement, "STATUS", "FALSE");

            //root node
            XmlElement simElement = outputDoc.CreateElement("SIMULATION_PARAMETER");
            add_attribute(simElement, "SIMULATION_EXIT_TYPE", "Time");
            add_attribute(simElement, "SIMULATION_TIME", simulation_time.ToString());

            simElement.AppendChild(seedElement);
            simElement.AppendChild(animElement);
            simElement.AppendChild(interactiveElement);

            parent.AppendChild(simElement);

            return parent;
        }


        /*
        func: appends the input txt file after converting to xml as child to the given node
        params: parent-parent node(node to which child has to be appended)
                filename-name of the txt file
        return: return the reference to child node apppeded
        */
        public XmlNode add_element_from_file(XmlNode parent, String filename)
        {
            String content = File.ReadAllText(filename);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(content);
            XmlNode newNode = doc.DocumentElement;
            XmlNode importNode = outputDoc.ImportNode(newNode, true);
            parent.AppendChild(importNode);
            return importNode;
        }

        /*
          func: appends a txt file after formatting with passed parameters to the given node.
          params: parent-node to which the child has be appended
                  filename-filename of the txt file to be formatted and appended
                  args- parameters for foramatting the txt file
          return: return the reference to created child node
        */
        public XmlNode add_element_from_file_with_format(XmlNode parent, String filename, params String[] args)
        {
            String format = File.ReadAllText(filename);
            String content = string.Format(format, args);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(content);
            XmlNode newNode = doc.DocumentElement;
            XmlNode importNode = outputDoc.ImportNode(newNode, true);
            parent.AppendChild(importNode);
            return importNode;
        }

        /*
          func: appends a child node to the given parent node
          params: parent- the node to which child node is to be appended
                  value-name of the child node(element)
          return: return the reference to created child
        */
        public XmlNode add_element(XmlNode parent, String name)
        {
            XmlNode node = outputDoc.CreateElement(name);
            parent.AppendChild(node);
            return node;
        }

        /*
         func: The function save the created document as configuration.xml
         params: no params
         return: doesn't return
       */
        public void save_document(string filename)
        {
            outputDoc.Save(filename);
        }

    }
}
