using System;
using System.Text;
using System.Xml;

namespace NetSim_Scenario_Generation_Internetworks
{
    public class AddNetworkElement
    {
        private static XmlWriter nsWriter=new XmlWriter();

        /*
          func: function generated an Ipv4 
          params: id- rank(numbered) of device to which ipv4 has to be assigned
                  _4th- fourth octate value of the ip to be generated 
          return: return the generated ipv4
        */
        public string next_ip(int id, int _4th)
        {
            int _4th_octate = _4th, _3rd_octate = 0, _2nd_octate = 0, _1st_octate = 11;
            _3rd_octate += id % 200;
            _2nd_octate += (id / 200) % 200;
            _1st_octate += ((id / 200) / 200) % 200;
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
          func: generates subnet mask(fixed here to 255.255.255.0)
          params: no params
          return: return the subnet mask
        */
        public string subnet_mask()
        {
            return "255.255.255.0";
        }

        /*
          func: adds a router(device) as child node to DEVICE_CONFIGUARTION(parent node)
          params: deviceConfig:-reference to parent node(DEVICE_CONFIGUARTION)
                  device_attribute:- attributs of router
                  pos_3d_attribute:-postion info of router
                  interface_variables:- interface info of router
          return: doesnt return
        */
        public void add_router(XmlNode deviceConfig, DEVICE device_attribute, POS_3D pos_3d_attribute, INTERFACE[] interface_variables, string config_helper_location)
        {
            XmlNode device = nsWriter.add_element_from_file_with_format(deviceConfig, config_helper_location + "\\ConfigHelper\\Router\\Device.txt",
                  Convert.ToString(device_attribute.DEFAULT_DEVICE_NAME),
                  Convert.ToString(device_attribute.DEVICE_ID),
                  Convert.ToString(device_attribute.DEVICE_IMAGE),
                  Convert.ToString(device_attribute.DEVICE_NAME),
                  Convert.ToString(device_attribute.DEVICE_TYPE),
                  Convert.ToString(device_attribute.INTERFACE_COUNT),
                  Convert.ToString(device_attribute.TYPE),
                  Convert.ToString(device_attribute.WIRESHARK_OPTION),
                  Convert.ToString(pos_3d_attribute.X_OR_LON),
                  Convert.ToString(pos_3d_attribute.Y_OR_LAT),
                  Convert.ToString(pos_3d_attribute.Z));


            for (int i = 0; i < device_attribute.INTERFACE_COUNT; i++)
            {
                string file_name;
                if (interface_variables[i].INTERFACE_TYPE.Equals("WAN"))
                    file_name = "Interface_Wan.txt";
                else
                    file_name = "Interface_Ethernet.txt";

                nsWriter.add_element_from_file_with_format(device, config_helper_location + "\\ConfigHelper\\Router\\" + file_name,
                    Convert.ToString(interface_variables[i].ID),
                    Convert.ToString(interface_variables[i].DEFAULT_GATEWAY),
                    Convert.ToString(interface_variables[i].IP_ADDRESS),
                    Convert.ToString(interface_variables[i].SUBNET_MASK),
                    Convert.ToString(interface_variables[i].MAC_ADDRESS),
                    Convert.ToString(interface_variables[i].CONNECTED_TO));
            }
            XmlNode routing_protocol = nsWriter.add_element_from_file(device, config_helper_location + "\\ConfigHelper\\Router\\Application_Layer.txt");
            XmlNode protocol_property = nsWriter.add_element_from_file(routing_protocol, config_helper_location + "\\ConfigHelper\\Router\\Application_Layer_Routing_Protocol.txt");
            XmlNode property_interface = nsWriter.add_element_from_file(protocol_property, config_helper_location + "\\ConfigHelper\\Router\\Application_Layer_Routing_Protocol_Property.txt");
            for (int i = 0; i < device_attribute.INTERFACE_COUNT; i++)
            {
                if (interface_variables[i].INTERFACE_TYPE.Equals("WAN"))
                    nsWriter.add_element_from_file_with_format(property_interface, config_helper_location + "\\ConfigHelper\\Router\\Application_Layer_Routing_Protocol_Property_Interface_Wan.txt",
                    Convert.ToString(interface_variables[i].ID));
                else
                    nsWriter.add_element_from_file_with_format(property_interface, config_helper_location + "\\ConfigHelper\\Router\\Application_Layer_Routing_Protocol_Property_Interface_Ethernet.txt",
                         Convert.ToString(interface_variables[i].ID));
            }
            nsWriter.add_element_from_file(device, config_helper_location + "\\ConfigHelper\\Router\\Transport_Layer.txt");
            nsWriter.add_element_from_file(device, config_helper_location + "\\ConfigHelper\\Router\\Network_Layer.txt");
        }

        /*
          func: adds a wirednode(device) as child node to DEVICE_CONFIGUARTION(parent node)
          params: deviceConfig:-reference to parent node(DEVICE_CONFIGUARTION)
                  device_attribute:- attributs of wirednode
                  pos_3d_attribute:-postion info of wirednode
                  interface_variables:- interface info of wirednode
          return: doesnt return
        */
        public void add_node(XmlNode deviceConfig, DEVICE device_attribute, POS_3D pos_3d_attribute, INTERFACE[] interface_variables, string config_helper_location)
        {
            XmlNode device = nsWriter.add_element_from_file_with_format(deviceConfig, config_helper_location + "\\ConfigHelper\\Node\\Device.txt",
                  Convert.ToString(device_attribute.DEFAULT_DEVICE_NAME),
                  Convert.ToString(device_attribute.DEVICE_ID),
                  Convert.ToString(device_attribute.DEVICE_IMAGE),
                  Convert.ToString(device_attribute.DEVICE_NAME),
                  Convert.ToString(device_attribute.DEVICE_TYPE),
                  Convert.ToString(device_attribute.INTERFACE_COUNT),
                  Convert.ToString(device_attribute.TYPE),
                  Convert.ToString(device_attribute.WIRESHARK_OPTION),
                  Convert.ToString(pos_3d_attribute.X_OR_LON),
                  Convert.ToString(pos_3d_attribute.Y_OR_LAT),
                  Convert.ToString(pos_3d_attribute.Z));


            for (int i = 0; i < device_attribute.INTERFACE_COUNT; i++)
            {
                nsWriter.add_element_from_file_with_format(device, config_helper_location + "\\ConfigHelper\\Node\\Interface_Ethernet.txt",
                    Convert.ToString(interface_variables[i].ID),
                    Convert.ToString(interface_variables[i].DEFAULT_GATEWAY),
                    Convert.ToString(interface_variables[i].IP_ADDRESS),
                    Convert.ToString(interface_variables[i].SUBNET_MASK),
                    Convert.ToString(interface_variables[i].MAC_ADDRESS),
                    Convert.ToString(interface_variables[i].CONNECTED_TO));
            }
            nsWriter.add_element_from_file(device, config_helper_location + "\\ConfigHelper\\Node\\Application_Layer.txt");
            nsWriter.add_element_from_file(device, config_helper_location + "\\ConfigHelper\\Node\\Transport_Layer.txt");
            nsWriter.add_element_from_file(device, config_helper_location + "\\ConfigHelper\\Node\\Network_Layer.txt");
        }

        /*
          func: adds all the devices as child node to DEVICE_CONFIGUARTION(parent node)
          params: parent:-reference to parent node(NETWORK_CONFIGURATION)
                  device_count :- number of devices to be added
                  device_container :- contains whole info for all the devices 
          return: doesnt return
        */
        public void add_deviceConfig(XmlNode parent, int device_count, DEVICE_CONTAINER[] device_container, string config_helper_location)
        {
            XmlNode deviceConfig = nsWriter.add_element(parent, "DEVICE_CONFIGURATION");
            int deviceCount = device_count;
            nsWriter.add_attribute(deviceConfig, "DEVICE_COUNT", Convert.ToString(deviceCount));

            for (int i = 0; i < deviceCount; i++)
            {
                if (device_container[i].device.DEVICE_TYPE.Equals("ROUTER"))
                    add_router(deviceConfig, device_container[i].device, device_container[i].pos_3d, device_container[i]._interface, config_helper_location);
                else if (device_container[i].device.DEVICE_TYPE.Equals("WIREDNODE"))
                    add_node(deviceConfig, device_container[i].device, device_container[i].pos_3d, device_container[i]._interface, config_helper_location);
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
                nsWriter.add_element_from_file_with_format(con, config_helper_location + "\\ConfigHelper\\Link.txt",
                Convert.ToString(link[i].LINK_ID),
                Convert.ToString(link[i].LINK_NAME),
                Convert.ToString(link[i].DEVICE_ID_1),
                Convert.ToString(link[i].INTERFACE_ID_1),
                Convert.ToString(link[i].NAME_1),
                Convert.ToString(link[i].DEVICE_ID_2),
                Convert.ToString(link[i].INTERFACE_ID_2),
                Convert.ToString(link[i].NAME_2));
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
          return: doesn't return
        */
        public void add_network(XmlNode parent, int device_count, int link_count, int application_count, DEVICE_CONTAINER[] device_container, LINK[] link, APPLICATION[] application, string config_helper_location)
        {
            XmlNode nwConfig = nsWriter.add_element(parent, "NETWORK_CONFIGURATION");

            add_deviceConfig(nwConfig, device_count, device_container, config_helper_location);
            add_connection(nwConfig, link_count, link, config_helper_location);
            add_application(nwConfig, application_count, application, config_helper_location);
        }

    }
}
