using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace TIA_Datei_Viewer
{
    class TiaFile
    {
        // complete *.tia
        private XmlDocument tiaFile;

        // multi purpose List of nodes
        private XmlNodeList nodes;

        // keeps a List of unique Type values of all nodes
        private List<string> types;

        public TiaFile(string tiaFileName)
        {
            tiaFile = new XmlDocument();
            try
            {
                tiaFile.Load(tiaFileName);
            }
            catch (System.IO.FileNotFoundException) {
                //TODO
                return;
            }

            //TODO check schema matches expectations

            nodes = tiaFile.SelectSingleNode("//nodes").ChildNodes;

        }

        ////////////////////////////////////////////////////////////////
        
        //              functions to create strings for UI            //

        ////////////////////////////////////////////////////////////////

        // returns a List of unique attributes "Type" and their occurrences
        // format of one string: "typename (#occurences)"
        public List<string> TypeButtonNames()
        {
            nodes = tiaFile.SelectSingleNode("//nodes").ChildNodes;
            types = new List<string>();
            List<string> typesAndNr = new List<string>();
            List<int> occurrences = new List<int>();
            
            foreach (XmlNode node in nodes)
            {
                string type = node.Attributes["Type"].Value;
                int i = types.IndexOf(type);

                if (i < 0)
                {
                    //type is new
                    types.Add(type);
                    occurrences.Add(1);
                }
                else
                {
                    //type is already known
                    occurrences[i]++;
                }
            }
            for (int i = 0; i < occurrences.Count; i++) {
                typesAndNr.Add(types[i] + i+" (" + occurrences[i] +")");
            }

            return typesAndNr;
        }

        // returns a List of nodes with the attribute type
        // the type is referenced by its number in List<string> types
        public List<string> NodesOfType(int type)
        {
            XmlNode nodesRoot = tiaFile.SelectSingleNode("//nodes");
            List<string> nodesOfType = new List<string>();
            //select nodes with the attribute Type = type
            nodes = nodesRoot.SelectNodes("node[@Type = \"" + types[type] + "\"]");
            foreach (XmlNode node in nodes)
            {
                string nodeAndPropNr;
                // look for the InnerText of a sibling of the node with key=Name
                XmlNode nameOrIdNode = node.SelectSingleNode("properties/property[key =\"Name\"]/value");
                if (nameOrIdNode == null)
                {
                    // we did not find Id, look for Id
                    nameOrIdNode = node.SelectSingleNode("properties/property[key=\"Id\"]/value");
                }
                
                if (nameOrIdNode == null)
                {
                    //this should not be possible, there must be either name or id, right? TODO
                }
                else {
                    nodeAndPropNr = nameOrIdNode.InnerText;

                    //how many properties? (Name and Id included so guaranteed at least one at this point)
                    XmlNode properties = node.SelectSingleNode("properties");
                    nodeAndPropNr += "\t\tEigenschaften: " + properties.ChildNodes.Count;

                    nodesOfType.Add(nodeAndPropNr);
                }

                

            }
            return nodesOfType;
        }
    }
}
