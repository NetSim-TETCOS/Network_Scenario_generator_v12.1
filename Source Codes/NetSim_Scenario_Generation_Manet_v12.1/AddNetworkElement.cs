﻿using System;
using System.Text;
using System.Xml;

namespace NetSim_Scenario_Generation_Manet
{
    class AddNetworkElement
    {
        private static XmlWriter nsWriter = new XmlWriter();

        /*
          func: function generated an Ipv4 
          params: id- rank(numbered) of device to which ipv4 has to be assigned
          return: return the generated ipv4
        */
        public string next_ip(int id)
        {
            int _4th_octate =0, _3rd_octate = 0, _2nd_octate = 0, _1st_octate = 11;
            _4th_octate += id % 200;
            _3rd_octate += (id / 200) % 200;
            _2nd_octate += ((id / 200) / 200) % 200;
            string ip = Convert.ToString(_1st_octate) + "." + Convert.ToString(_2nd_octate) + "." +
                        Convert.ToString(_3rd_octate) + "." + Convert.ToString(_4th_octate);
            return ip;
        }

        /*
          func: generates a mac address  
          params: mac- mac address assigned to the previous device
          return: return the generated mac address
        */
        public string next_mac(string mac)
        {
            int i = 0, j, n, index = 0, id = -1;
            char temp;
            n = mac.Length;
            //Console.WriteLine("value of n: {0}", n);
            StringBuilder str = new StringBuilder(mac);
            for (i = 0; i < n - 1; i++)
            {
                if (str[i] <= str[i + 1])
                {
                    id = i;
                }
            }

            if (id == -1)
                return str.ToString();
            //Console.WriteLine("value of i: {0}", i);
            for (j = id + 1; j < n; j++)
            {
                if (str[j] > str[id])
                    index = j;
            }
            temp = str[id];
            str[id] = str[index];
            str[index] = temp;
            i = id + 1; j = n - 1;
            while (i <= j)
            {
                temp = str[i];
                str[i] = str[j];
                str[j] = temp;
                i++; j--;
            }
            return str.ToString();
        }

        /*
          func: generates subnet mask(fixed here to 255.255.0.0)
          params: no params
          return: return the subnet mask
        */
        public string subnet_mask()
        {
            return "255.255.0.0";
        }

        /*
          func: adds a wireless_node(device) as child node to DEVICE_CONFIGUARTION(parent node)
          params: deviceConfig:-reference to parent node(DEVICE_CONFIGUARTION)
                  device_attribute:- attributs of wireless_node
                  pos_3d_attribute:-postion info of wireless_node
                  interface_variables:- interface info of wireless_node
                  other_info- extra info about the wireless_node
          return: doesnt return
        */
        public void add_wireless_node(XmlNode deviceConfig, DEVICE device_attribute, POS_3D pos_3d_attribute, INTERFACE[] interface_variables,OTHER_INFO other_info, string config_helper_location)
        {
            XmlNode device = nsWriter.add_element_from_file_with_format(deviceConfig, config_helper_location + "\\ConfigHelper\\Wireless_Node\\Device.txt",                 
                  Convert.ToString(device_attribute.DEVICE_ID),           
                  Convert.ToString(device_attribute.DEVICE_NAME),
                  Convert.ToString(device_attribute.WIRESHARK_OPTION)
                );
            string mobility_file_name= "Random_Way_Point.txt";
            if (device_attribute.MOBILITY_MODEL.Equals("NO_MOBILITY"))
                mobility_file_name = "No_Mobility.txt";
            else if (device_attribute.MOBILITY_MODEL.Equals("RANDOM_WALK"))
                mobility_file_name = "Random_Walk.txt"; 

            nsWriter.add_element_from_file_with_format(device, config_helper_location + "\\ConfigHelper\\Wireless_Node\\Mobility_Model\\" +mobility_file_name,
                  Convert.ToString(pos_3d_attribute.X_OR_LON),
                  Convert.ToString(pos_3d_attribute.Y_OR_LAT)
                );


            for (int i = 0; i < device_attribute.INTERFACE_COUNT; i++)
            {
                XmlNode _interface=nsWriter.add_element_from_file_with_format(device, config_helper_location + "\\ConfigHelper\\Wireless_Node\\Interface.txt",
                    Convert.ToString(interface_variables[i].ID),
                    Convert.ToString(interface_variables[i].DEFAULT_GATEWAY),
                    Convert.ToString(interface_variables[i].IP_ADDRESS),
                    Convert.ToString(interface_variables[i].SUBNET_MASK),
                    Convert.ToString(interface_variables[i].MAC_ADDRESS));
                string power_filename = "Physical_Layer_Power_Battery.txt";
                if (other_info.POWER_SOURCE.Equals("Main_Line"))
                    power_filename = "Physical_Layer_Power_MainLine.txt";

                nsWriter.add_element_from_file(_interface, config_helper_location + "\\ConfigHelper\\Wireless_Node\\" +power_filename);
            }
            

            nsWriter.add_element_from_file(device, config_helper_location + "\\ConfigHelper\\Wireless_Node\\Application_Layer.txt");
            nsWriter.add_element_from_file(device, config_helper_location + "\\ConfigHelper\\Wireless_Node\\Transport_Layer.txt");
            XmlNode routing_protocol=nsWriter.add_element_from_file(device, config_helper_location + "\\ConfigHelper\\Wireless_Node\\Network_Layer.txt");

            string routing_filename = other_info.ROUTING_PROTOCOL_TYPE + ".txt";
            nsWriter.add_element_from_file(routing_protocol, config_helper_location + "\\ConfigHelper\\Wireless_Node\\Routing_Protocol\\" +routing_filename);
        }

        /*
          func: adds all the devices as child node to DEVICE_CONFIGUARTION(parent node)
          params: parent:-reference to parent node(NETWORK_CONFIGURATION)
                  device_count :- number of devices to be added
                  device_container :- contains whole info for all the devices 
                  other_info :- extra info about wireless_node device
          return: doesnt return
        */
        public void add_deviceConfig(XmlNode parent, int device_count, DEVICE_CONTAINER[] device_container,OTHER_INFO other_info, string config_helper_location)
        {
            XmlNode deviceConfig = nsWriter.add_element(parent, "DEVICE_CONFIGURATION");
            int deviceCount = device_count;
            nsWriter.add_attribute(deviceConfig, "DEVICE_COUNT", Convert.ToString(deviceCount));

            for (int i = 0; i < deviceCount; i++)
            {     
                 if (device_container[i].device.DEVICE_TYPE.Equals("WIRELESSNODE"))
                    add_wireless_node(deviceConfig, device_container[i].device, device_container[i].pos_3d, device_container[i]._interface,other_info, config_helper_location);
            }
        }

        /*
          func: appends all the link info inside CONNECTION
          params: parent :- reference to parent node(NETWORK_CONFIGUARTION)
                  link_count :- number of links
                  link :- contains whole info about all the links 
          return: doesnt return
        */
        public void add_connection(XmlNode parent, int link_count, LINK[] link, string config_helper_location)
        {
            XmlNode con = nsWriter.add_element(parent, "CONNECTION");
            for (int i = 0; i < link_count; i++)
            {
                XmlNode link_element=nsWriter.add_element_from_file_with_format(con, config_helper_location + "\\ConfigHelper\\Link.txt",
                Convert.ToString(link[i].DEVICE_COUNT),
                Convert.ToString(link[i].LINK_ID),
                Convert.ToString(link[i].LINK_NAME),
                Convert.ToString(link[i].X),
                Convert.ToString(link[i].Y));
                for(int j=0;j<link[i].DEVICE_COUNT;j++)
                {
                    nsWriter.add_element_from_file_with_format(link_element, config_helper_location + "\\ConfigHelper\\Link_Device.txt",
                    Convert.ToString(link[i].link_device[j].DEVICE_ID),
                    Convert.ToString(link[i].link_device[j].INTERFACE_ID),
                    Convert.ToString(link[i].link_device[j].NAME));
                }
                nsWriter.add_element_from_file_with_format(link_element, config_helper_location + "\\ConfigHelper\\Link_Medium_Property.txt");
            }
        }

        /*
          func: adds all the Applications in NETWORK_CONFIGUARTION(parent node)
          params: parent :- reference to parent node(NETWORK_CONFIGUARTION)
                  application_count :- number of applications
                  application :- container of all whole info about all the application
          return: doesnt return
        */
        public void add_application(XmlNode parent, int application_count, APPLICATION[] application, string config_helper_location)
        {
            XmlNode app = nsWriter.add_element(parent, "APPLICATION_CONFIGURATION");
            nsWriter.add_attribute(app, "COUNT", Convert.ToString(application_count));
            for (int i = 0; i < application_count; i++)
            {
                nsWriter.add_element_from_file_with_format(app, config_helper_location + "\\ConfigHelper\\Application.txt",
                    Convert.ToString(application[i].DESTINATION_ID),
                    Convert.ToString(application[i].ID),
                    Convert.ToString(application[i].NAME),
                    Convert.ToString(application[i].SOURCE_ID));
            }
        }

        /*
          func: append all the child nodes of NETWORK_CONFIGURATION
          params: parent :- reference to parent node(TETCOS_NETSIM)
                  device count:-  number of devices
                  link_count:- number of links
                  application_count:- number of applications
                  device_containe:- container containing whole info of all the devices
                  link:- container contaning whole info of all the links
                  application :- container of all whole info about all the application
                  other_info:- extra info about the wireless_node device
          return: doesn't return
        */
        public void add_network(XmlNode parent, int device_count, int link_count, int application_count, DEVICE_CONTAINER[] device_container, LINK[] link, APPLICATION[] application,OTHER_INFO other_info, string config_helper_location)
        {
            XmlNode nwConfig = nsWriter.add_element(parent, "NETWORK_CONFIGURATION");

            add_deviceConfig(nwConfig, device_count, device_container,other_info, config_helper_location);
            add_connection(nwConfig, link_count, link, config_helper_location);
            add_application(nwConfig, application_count, application, config_helper_location);
        }
    }
}
