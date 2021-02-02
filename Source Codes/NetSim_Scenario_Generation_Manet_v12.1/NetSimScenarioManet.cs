using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace NetSim_Scenario_Generation_Manet
{
    /*  structure to store the attributes of APPLICATION node  */
    public struct APPLICATION
    {
        public int DESTINATION_ID;
        public int ID;
        public string NAME;
        public int SOURCE_ID;
    }

    /*  structure to store the Postion of a node */
    public struct POS_3D
    {
        public double X_OR_LON;
        public double Y_OR_LAT;
        public double Z;
    }

    /* structure for containing extra info about wireless_node*/
    public struct OTHER_INFO
    {
        public string POWER_SOURCE;
        public string ROUTING_PROTOCOL_TYPE;
    }

    /*structure for containing DEVICE INFO of a LINK*/
    public struct LINK_DEVICE
    {
        public int DEVICE_ID;
        public int INTERFACE_ID;
        public string NAME;
    }

    /*  structure for containing attributes of a link  along with DEVICE INFO of the link */
    public struct LINK
    {
        public int DEVICE_COUNT;
        public int LINK_ID;
        public string LINK_NAME;
        public double X;
        public double Y;
        public LINK_DEVICE[] link_device;
    }

    /*  structure to store the attributes of a DEVICE */
    public struct DEVICE
    {
        public string DEFAULT_DEVICE_NAME;
        public int DEVICE_ID;
        public string DEVICE_IMAGE;
        public string DEVICE_NAME;
        public string DEVICE_TYPE;
        public int INTERFACE_COUNT;
        public string TYPE;
        public string WIRESHARK_OPTION;
        public string MOBILITY_MODEL;
    }

    /*  structure to store the attributes of an Interface of a DEVICE  */
    public struct INTERFACE
    {
        public int ID;
        public string DEFAULT_GATEWAY;
        public string IP_ADDRESS;
        public string SUBNET_MASK;
        public string MAC_ADDRESS;
        public string INTERFACE_TYPE;
    }

   /*  structure to store attributes,positionand interfaces of a DEVICE */
    public struct DEVICE_CONTAINER
    {
        public DEVICE device;
        public POS_3D pos_3d;
        public INTERFACE[] _interface;
    }

    class NetSimScenarioManet
    {
        public static XmlWriter nsWriter;
        public static AddNetworkElement addNet;
        static void Main(string[] args)
        {
            int total_node;
            int max_application;
            string config_helper_location;
            string exp_name;
            string version_name;
            string version_number;
            float simulation_time;

            if (args.Length == 7)
            {
                total_node = Int32.Parse(args[0]);
                max_application = Int32.Parse(args[1]);
                config_helper_location = args[2];
                exp_name = args[3];
                version_name = args[4];
                version_number = args[5];
                simulation_time = float.Parse(args[6]);
            }
            else
            {
                Console.WriteLine("Incorrect!! Required 7 arguments\n");
                return;
            }

            Console.WriteLine(total_node);
            Console.WriteLine(max_application);
            Console.WriteLine(config_helper_location);
            Console.WriteLine(exp_name);                /*test_case name*/
            Console.WriteLine(version_name);            /* version_name and version_number taken from netsim_path_location\Docs\GUI\NSF-utility.txt */
            Console.WriteLine(version_number);
            Console.WriteLine(simulation_time);         /* Browser input 0.001 to 1,00,000 */



            /****************************************************************************************************/
            //user parameters for generating scenario(Configuration.netsim)
            int max_interface = 1;   // fixed to 1 only 
            //int total_node = 100;    // 1 to 10000
            int max_link = 1;        // fixed to 1 only
            //int max_application = 2;  /*max up to (total_node*(total_node-1)/2)*/
            string mobility_model = "RANDOM_WAY_POINT";// from set (NO_MOBILITY,RANDOM_WALK,RANDOM_WAY_POINT)
            string routing_protocol = "DSR"; //from set(DSR,AODV,OLSR,ZRP)
            string power_source = "Battery";//from set (Battery,Main_Line)
            string application_from_file = ""; //(give the file name along with path from which application has to be created or leave it as it is if you randomly want to create the application)
            OTHER_INFO other_info;
            other_info.ROUTING_PROTOCOL_TYPE = routing_protocol;
            other_info.POWER_SOURCE = power_source;
            /*****************************************************************************************************/


            /*****************************************************************************************************/
            //mac address of devices from this set of character and other variables
            string mac = "123456789ABC";
            int i, j, ip_count = 0, link_count = 0, application_count = 0;
            double x0 = 250.0, y0 = 250.0, radius = 200.0;
            double increment = 360.0 / total_node;
            Random rnd = new Random();
            nsWriter = new XmlWriter();
            addNet = new AddNetworkElement();
            XmlNode root = nsWriter.open_document();
            /*****************************************************************************************************/


            /*****************************************************************************************************/
            //application for containing detailes of applications
            APPLICATION[] application = new APPLICATION[max_application];

            //device_container to store varible for each device
            DEVICE_CONTAINER[] device_container = new DEVICE_CONTAINER[total_node];

            //link fot storing details of all the links
            LINK[] link = new LINK[max_link];
            /*****************************************************************************************************/


            /******************************************************************************************************/
            // this block of code sets the attributes,position and interface of wireless_nodes
            for (i = 0; i < total_node; i++)
            {
                device_container[i].device.DEFAULT_DEVICE_NAME = "Wireless_Node";
                device_container[i].device.DEVICE_ID = i + 1;
                device_container[i].device.DEVICE_IMAGE = "WirelessNode.png";
                device_container[i].device.DEVICE_NAME = "Wireless_Node_" + Convert.ToString(i + 1);
                device_container[i].device.DEVICE_TYPE = "WIRELESSNODE";
                device_container[i].device.INTERFACE_COUNT = 0;
                device_container[i].device.TYPE = "NODE";
                device_container[i].device.WIRESHARK_OPTION = "Disable";
                device_container[i].device.MOBILITY_MODEL = mobility_model;

                double theta = i * increment;
                device_container[i].pos_3d.X_OR_LON = x0 + radius * Math.Cos(Math.PI * (theta / 180.0));
                device_container[i].pos_3d.Y_OR_LAT = y0 + radius * Math.Sin(Math.PI * (theta / 180.0));
          
                int last_interface= device_container[i].device.INTERFACE_COUNT = 0;
                device_container[i]._interface = new INTERFACE[max_interface];
                device_container[i]._interface[last_interface].ID = last_interface + 1;
                device_container[i]._interface[last_interface].DEFAULT_GATEWAY = "";
                device_container[i]._interface[last_interface].IP_ADDRESS = addNet.next_ip(++ip_count);
                device_container[i]._interface[last_interface].SUBNET_MASK = addNet.subnet_mask();
                mac = addNet.next_mac(mac);
                device_container[i]._interface[last_interface].MAC_ADDRESS = mac;

                device_container[i].device.INTERFACE_COUNT = last_interface + 1;
            }
            /***************************************************************************************************************/


            /***************************************************************************************************************/
            //this block of code sets the link attributes for adhoc link
            for(i=0;i<max_link;i++)
            {
                link[i].DEVICE_COUNT = total_node;
                link[i].LINK_ID = i + 1;
                link[i].LINK_NAME = Convert.ToString(i + 1);
                link[i].X = x0;
                link[i].Y = y0;

                link[i].link_device = new LINK_DEVICE[link[i].DEVICE_COUNT];
                for (j = 0; j < link[i].DEVICE_COUNT; j++)
                {
                    int id = device_container[j].device.DEVICE_ID;
                    link[i].link_device[j].DEVICE_ID = id;
                    link[i].link_device[j].INTERFACE_ID = device_container[id - 1].device.INTERFACE_COUNT;
                    link[i].link_device[j].NAME = device_container[id - 1].device.DEVICE_NAME;
                }
            }
            /***************************************************************************************************************/


            /**************************************************************************************************************/
            //this block of code randomly or from file  sets the  attributes for all the applications
            if (!application_from_file.Equals(""))
            {
                 application_count = 0;
                 using (StreamReader sr = new StreamReader(application_from_file))
                 {
                     string line;
                    int count = 0;
                     while ((line = sr.ReadLine()) != null)
                     {
                         string[] tokens = line.Split(' ');
                         if(count==0)
                        {
                            count++;
                            continue;
                        }
                        // Console.WriteLine(tokens[0]+","+tokens[1]);
                         if(application_count<max_application)
                         {
                             application[application_count].DESTINATION_ID =Convert.ToInt32(tokens[1]);
                             application[application_count].ID = application_count+1;
                             application[application_count].NAME = "App" + Convert.ToString(application_count+1) + "_CBR";
                             application[application_count].SOURCE_ID = Convert.ToInt32(tokens[0]);
                             application_count++;
                         }
                     }
                    // Console.ReadLine();
                 }
            }
            else
            {
                for (i = 0; i < max_application; i++)
                {

                    application[i].DESTINATION_ID = rnd.Next(1, total_node+1);
                    application[i].ID = i + 1;
                    application[i].NAME = Convert.ToString(i + 1);
                    int source;
                    while ((source = rnd.Next(1, total_node+1)) == application[i].DESTINATION_ID) ;
                    application[i].SOURCE_ID = source;
                }
            }
            /********************************************************************************************************/

            /********************************************************************************************************/
            //this block of code calls the diffrent funtions to create the Configuration.netsim
            nsWriter.add_experimentInfo(root, exp_name, version_name, version_number);
            //nsWriter.add_element_from_file(root, config_helper_location + "\\ConfigHelper\\Experiment_Info.txt");
            nsWriter.add_element_from_file(root, config_helper_location + "\\ConfigHelper\\Gui_Info.txt");
            addNet.add_network(root, total_node, max_link, max_application, device_container, link, application,other_info, config_helper_location);
            nsWriter.add_simulation_parameter(root, simulation_time);
            //nsWriter.add_element_from_file(root, config_helper_location + "\\ConfigHelper\\Simulation_Parameter.txt");

            nsWriter.add_element_from_file(root, config_helper_location + "\\ConfigHelper\\Protocol_Configuration.txt");
            nsWriter.add_element_from_file(root, config_helper_location + "\\ConfigHelper\\Statistics_Collection.txt");
            nsWriter.save_document(config_helper_location + "\\Configuration.netsim");
            /*********************************************************************************************************/
        }
    }
}
