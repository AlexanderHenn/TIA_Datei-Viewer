﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace TIA_Datei_Viewer
{
    class TiaFile
    {
        // complete *.tia (if this is not null, it is guaranteed to be of the desired schema)
        private XmlDocument tiaFile;

        // keeps a List of unique Type values found in nodes in tiaFile
        private List<string> types;

        // List of nodes. After a TypeLoad(type) call, it will contain all nodes from tiaFile that have the Type type
        private XmlNodeList nodes;

        //validation
        private bool validated;
        private string validationMessage;

        // it is necessary to call Validate after this, because not validated objects are considered not valid
        // methods of objects that are not valid will return null (except for functions for validation)
        public TiaFile(string tiaFileName)
        {
            validated = false;
            try
            {
                tiaFile = new XmlDocument();
                tiaFile.Load(tiaFileName);
            }
            catch (System.IO.FileNotFoundException) {}
        }

        ///////////////////////////////////////////////////////////////////
        ///
        ///         tia validation
        ///
        ///////////////////////////////////////////////////////////////////

        // this is a bit of a mess, because of the way tiaFile.Validate works.
        // moving all validation to MainWindow could make it "better", but also seems wrong.

        public bool Validate()
        {
            // validation may not be incorrect, because schema is just autogenerated from "Test3.tia" and modified to allow empty "contexts"
            // (also i do not know the requirements for a tia file)
            // this is fine though, because it works for the samples and xsd can easily be adjusted
            validated = false;
            if (tiaFile != null)
            {
                tiaFile.Schemas.Add("", "xml/schema.xsd");
                XmlSchema schema = new XmlSchema();
                //set validated=true, because ValidationHandler only acts if something is wrong
                validated = true;
                tiaFile.Validate(ValidationHandler);
            }
            return validated;
        }

        // set validated according to the result of validation
        // currently even warnings currently make the tiaFile invalid
        void ValidationHandler(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Warning)
            {
                validated = false;
                validationMessage = "Warning: " + e.Message; //visual studio says something about accessibility modifiers being required..?
            }
            else if (e.Severity == XmlSeverityType.Error)
            {
                validated = false;
                validationMessage = "Critical Error: " + e.Message;
            }
        }

        public string ValidationMessage
        {
            get{
                return validationMessage;
            }
        }

        ///////////////////////////////////////////////////////////////////
        ///
        ///           find nodes and data to create strings
        ///
        ///////////////////////////////////////////////////////////////////

        // returns a List of unique attributes "Type" and their occurrences
        // format of one string: "typename (#occurences)"
        // returns null, if tiaFile is not validated
        public List<string> LoadTypes()
        {
            if (!validated) return null;
            // prepare variables
            nodes = tiaFile.SelectSingleNode("//nodes").ChildNodes;
            types = new List<string>();
            List<string> typesAndNr = new List<string>();
            List<int> occurrences = new List<int>();

            // create list of strings with typenames and their occurrences
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
                typesAndNr.Add(types[i] + " (" + occurrences[i] +")");
            }

            return typesAndNr;
        }

        // returns a List of nodes with the attribute type and their properties count
        // the type is referenced by its number in List<string> types
        // also fills this.nodes with the nodes of that type
        // returns null if tiaFile is not validated
        // format of output is "nameOrId\t\tEigenschaften: #properties"
        public List<string> LoadType(int type)
        {
            if (!validated) return null;
            //select nodes with the attribute Type = type
            nodes = tiaFile.SelectNodes("//nodes/node[@Type = \"" + types[type] + "\"]");

            //create list of strings
            List<string> nodesOfType = new List<string>();
            foreach (XmlNode node in nodes)
            {
                string nodeAndPropNr;
                // look for the InnerText of a sibling of the node with key=Name
                XmlNode nameOrIdNode = node.SelectSingleNode("properties/property[key =\"Name\"]/value");
                if (nameOrIdNode == null)
                {
                    // we did not find Name, look for Id
                    nameOrIdNode = node.SelectSingleNode("properties/property[key=\"Id\"]/value");
                }
                
                if (nameOrIdNode == null)
                {
                    // this should be impossible
                    nodesOfType.Add("No Name or Id found for this node");
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
