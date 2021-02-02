using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace NetSim_Scenario_Generation_IOT
{
    /*  structure to store the attributes of APPLICATION node  */
    public struct APPLICATION
    {
        public int DESTINATION_ID;
        public int ID;
        public string NAME;
        public int SOURCE_ID;
    }

    /*  structure to store the Postion of a DEVICE*/
    public struct POS_3D
    {
        public double X_OR_LON;
        public double Y_OR_LAT;
        public double Z;
    }

    /* structure for containing extra info about the the devices*/
    public struct OTHER_INFO
    {
        public string POWER_SOURCE;
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
        public string link_type;
        public LINK_DEVICE[] link_device;
    }

    /* structure which stores information regarding position from centre of the grid */
    public struct LEVEL
    {
        public double r;
        public double theta;
        public int level;
        public double low_angle;
        public double increment;

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
        public string CONNECTED_TO;
        public string INTERFACE_TYPE;
    }

    /*  structure to store attributes,position,level and interfaces of a DEVICE */
    public struct DEVICE_CONTAINER
    {
        public DEVICE device;
        public POS_3D pos_3d;
        public LEVEL level;
        public INTERFACE[] _interface;
    }


    class NetSimScenario
    {
        public static XmlWriter nsWriter;
        public static AddNetworkElement addNet;
        static void Main(string[] args)
        {
            int max_router;
            int max_node;
            int max_sensor;
            int max_gateway;
            int max_application;
            string config_helper_location;
            string exp_name;
            string version_name;
            string version_number;
            float simulation_time;

            string application_from_file = "";


            if (args.Length == 10)
            {
                max_router = Int32.Parse(args[0]);
                max_node = Int32.Parse(args[1]);
                max_sensor = Int32.Parse(args[2]);
                max_gateway = Int32.Parse(args[3]);
                max_application = Int32.Parse(args[4]);
                config_helper_location = args[5];
                exp_name = args[6];
                version_name = args[7];
                version_number = args[8];
                simulation_time = float.Parse(args[9]);
            }
            else
            {
                Console.WriteLine("Incorrect!! Required 10 arguments\n");
                return;
            }

            Console.WriteLine(max_router);
            Console.WriteLine(max_node);
            Console.WriteLine(max_sensor);
            Console.WriteLine(max_gateway);
            Console.WriteLine(max_application);
            Console.WriteLine(config_helper_location);
            Console.WriteLine(exp_name);                /*test_case name*/
            Console.WriteLine(version_name);            /* version_name and version_number taken from netsim_path_location\Docs\GUI\NSF-utility.txt */
            Console.WriteLine(version_number);
            Console.WriteLine(simulation_time);         /* Browser input 0.001 to 1,00,000 */


            /********************************************************************************************/
            // these parameters are to be decided by user
            //int max_router = 1;          //(between 1-25000)
            //int max_node = 1;            //max 4*(max_router - 1)
            //int max_sensor = 20;         //(between 1-10000)
            //int max_gateway = 1;          //(fixed to 1 only dont change)
            //int max_application = 2;      //(between 1- max_node*max_sesor)
            //string application_from_file = "ConfigHelper\\Application_Config.txt"; //(give the file name along with path from which application has to be created or leave it as it is if you randomly want to create the application)
            /********************************************************************************************/


            /********************************************************************************************/
            //mac address of devices from this set of character and other variables
            string mac = "123456789ABC";
            int i, j, ipv4_count=0,ipv6_count = 0, link_count = 0,application_count=0;
            /********************************************************************************************/


            /********************************************************************************************/
            //fixed router info 
            int max_router_interface = 6;   
            int max_hop = 4;
            int branch = max_router_interface - 1;
            int height = (int)Math.Ceiling(Math.Log(max_router *(branch-1)* 1.0+1.0, branch * 1.0))-1;
            int not_leaf_node = ((int)Math.Pow(branch, height) - 1) / (branch - 1);
            double router_x0 = 70.0, router_y0 = 70.0, offset_radius = 9.0;
            /********************************************************************************************/

            /********************************************************************************************/
            //fixed gateway info
            int max_gateway_interface = 2;
            double gateway_x0=20.0, gateway_y0=50.0;
            string gateway_interface_1_ip = "fdec:3017:e256:9bb8:1fe7:bac6:3931:6e66";
            /********************************************************************************************/

            /********************************************************************************************/
            //fixed adhoc info
            double adhoc_x0 = 20.0, adhoc_y0 = 20.0, sensor_radius = 18.0;
            double sensor_angle_increment = 360.0 / max_sensor;
            string mobility_model = "NO_MOBILITY";/*from set (NO_MOBILITY,RANDOM_WALK,RANDOM_WAY_POINT)*/
            string power_source = "Battery";/*from set (Battery,Main_Line)*/
            OTHER_INFO other_info;
            other_info.POWER_SOURCE = power_source;
            /********************************************************************************************/

            /********************************************************************************************/
            //intermediate parameters used in generating scenario
            int total_device = max_router + max_node+max_sensor+max_gateway;
            int max_link = max_router + max_node+100;
            Random rnd = new Random();
            nsWriter = new XmlWriter();
            addNet = new AddNetworkElement();
            XmlNode root = nsWriter.open_document();
            /********************************************************************************************/

            /********************************************************************************************/
            //application for containing detailes of applications
            APPLICATION[] application = new APPLICATION[max_application];

            //device_container to store varible for each device
            DEVICE_CONTAINER[] device_container = new DEVICE_CONTAINER[total_device];

            //link fot storing details of all the links
            LINK[] link = new LINK[max_link];
            /********************************************************************************************/

            /********************************************************************************************/
            // this block initialises set of independed attributtes of router and wirednode
            for (i = 0; i < max_router; i++)
            {
                device_container[i].device.DEVICE_ID = i + 1;
                device_container[i].device.DEVICE_NAME = "Router_" + Convert.ToString(i + 1);
                device_container[i].device.DEVICE_TYPE = "ROUTER";
                device_container[i].device.INTERFACE_COUNT = 0;
                device_container[i].device.WIRESHARK_OPTION = "Disable";

                device_container[i]._interface = new INTERFACE[max_router_interface];
            }

            for (; i < (max_router+max_node); i++)
            {
                device_container[i].device.DEVICE_ID = i + 1;
                device_container[i].device.DEVICE_NAME = "Wired_Node_" + Convert.ToString(i + 1);
                device_container[i].device.DEVICE_TYPE = "WIREDNODE";
                device_container[i].device.INTERFACE_COUNT = 0;
                device_container[i].device.WIRESHARK_OPTION = "Disable";
                device_container[i]._interface = new INTERFACE[1];
            }
            /********************************************************************************************/


            /********************************************************************************************/
            // this block sets the attributes,positions of all the wireless_seonsor connected wire adhoc link
            for (; i < (max_router + max_node+max_sensor); i++)
            { 
                device_container[i].device.DEVICE_ID = i + 1;
                device_container[i].device.DEVICE_NAME = "Wireless_Sensor_" + Convert.ToString(i + 1);
                device_container[i].device.DEVICE_TYPE = "IOT_Sensors";
                device_container[i].device.INTERFACE_COUNT = 0;
                device_container[i].device.WIRESHARK_OPTION = "Disable";
                device_container[i].device.MOBILITY_MODEL = mobility_model;

                double theta = i * sensor_angle_increment;
                device_container[i].pos_3d.X_OR_LON = adhoc_x0 + sensor_radius * Math.Cos(Math.PI * (theta / 180.0));
                device_container[i].pos_3d.Y_OR_LAT = adhoc_y0 + sensor_radius * Math.Sin(Math.PI * (theta / 180.0));

                int last_interface = device_container[i].device.INTERFACE_COUNT = 0;
                device_container[i]._interface = new INTERFACE[1];
                device_container[i]._interface[last_interface].ID = last_interface + 1;
                device_container[i]._interface[last_interface].DEFAULT_GATEWAY = gateway_interface_1_ip;
                device_container[i]._interface[last_interface].IP_ADDRESS = addNet.next_ipv6(++ipv6_count);
                mac = addNet.next_mac(mac);
                device_container[i]._interface[last_interface].MAC_ADDRESS = mac;

                device_container[i].device.INTERFACE_COUNT = last_interface + 1;
            }
            /**********************************************************************************************/

            /**********************************************************************************************/
            //this block sets the attributes,position of gateway and first router and link b/w them
            device_container[i]._interface = new INTERFACE[2];
            device_container[i].device.DEVICE_ID = i + 1;
            device_container[i].device.DEVICE_NAME = "6_LOWPAN_Gateway_" + Convert.ToString(i + 1);
            device_container[i].device.DEVICE_TYPE = "LOWPAN_Gateway";

            device_container[i].pos_3d.X_OR_LON = gateway_x0;
            device_container[i].pos_3d.Y_OR_LAT = gateway_y0;

            device_container[i]._interface[0].ID = 1;
            device_container[i]._interface[0].DEFAULT_GATEWAY = "";
            device_container[i]._interface[0].IP_ADDRESS = gateway_interface_1_ip;
            mac = addNet.next_mac(mac);
            device_container[i]._interface[0].MAC_ADDRESS = mac;
            device_container[i]._interface[0].INTERFACE_TYPE = "ZIGBEE";
            device_container[i]._interface[0].CONNECTED_TO = "";

            device_container[i]._interface[1].ID = 2;
            device_container[i]._interface[1].DEFAULT_GATEWAY = "";
            device_container[i]._interface[1].IP_ADDRESS = addNet.next_ip(++ipv4_count,1);
            device_container[i]._interface[1].SUBNET_MASK = addNet.subnet_mask(); 
            mac = addNet.next_mac(mac);
            device_container[i]._interface[1].MAC_ADDRESS = mac;
            device_container[i]._interface[1].INTERFACE_TYPE = "WAN";
            device_container[i]._interface[1].CONNECTED_TO = "Router_1";

            device_container[0]._interface[0].ID = 1;
            device_container[0]._interface[0].DEFAULT_GATEWAY = "";
            device_container[0]._interface[0].IP_ADDRESS = addNet.next_ip(ipv4_count, 2);
            device_container[0]._interface[0].SUBNET_MASK = addNet.subnet_mask();
            mac = addNet.next_mac(mac);
            device_container[0]._interface[0].MAC_ADDRESS = mac;
            device_container[0]._interface[0].INTERFACE_TYPE = "WAN";
            device_container[0]._interface[0].CONNECTED_TO = "";
            device_container[0].device.INTERFACE_COUNT = 1;

            device_container[i].device.INTERFACE_COUNT = 2;

            link[link_count].link_device = new LINK_DEVICE[2];
            link[link_count].DEVICE_COUNT = 2;
            link[link_count].link_type = "P_to_P";
            link[link_count].LINK_ID = link_count + 1;
            link[link_count].LINK_NAME = Convert.ToString(link_count + 1);
            link[link_count].link_device[0].DEVICE_ID = 1;
            link[link_count].link_device[0].INTERFACE_ID = 1;
            link[link_count].link_device[0].NAME ="Router_"+Convert.ToString(1);
            link[link_count].link_device[1].DEVICE_ID = i+1;
            link[link_count].link_device[1].INTERFACE_ID = 2;
            link[link_count].link_device[1].NAME = "6_LOWPAN_Gateway_" + Convert.ToString(i+1);
            link_count++;



            device_container[0].level.r = 0.0;
            double theta0 = device_container[0].level.theta = 0.0;
            double level0 = device_container[0].level.level = 0;
            double increment0 = device_container[0].level.increment = (360.0 / Math.Pow(branch, level0 + 1));
            device_container[0].level.low_angle = theta0 - (branch * 1.0) * increment0 / 2.0;
            device_container[0].pos_3d.X_OR_LON = router_x0;
            device_container[0].pos_3d.Y_OR_LAT = router_y0;

            /***********************************************************************************************/

            
            /***********************************************************************************************/
            //this block sets the attributes positions and interfaces ,link of all the routers
            for (i = 0; i < max_router; i++)
            {
                int id = device_container[i].device.DEVICE_ID, index = 2;
                int n_id_ = (id - 1) * branch + index;
                not_leaf_node = id-1;
                if (n_id_ > max_router)
                    break;
                int last_interface = device_container[i].device.INTERFACE_COUNT;
                POS_3D pos_3d = device_container[i].pos_3d;

                double r = device_container[i].level.r;
                double theta = device_container[i].level.theta;
                int level = device_container[i].level.level;
                double low_angle = device_container[i].level.low_angle;
                double increment = device_container[i].level.increment;
                j = 0;
                while (j < branch)
                {
                    int n_id = (id - 1) * branch + (index++);
                    if (n_id > max_router)
                        break;

                    device_container[i]._interface[last_interface].ID = last_interface + 1;
                    device_container[i]._interface[last_interface].DEFAULT_GATEWAY = "";
                    device_container[i]._interface[last_interface].IP_ADDRESS = addNet.next_ip(++ipv4_count, 1);
                    mac = addNet.next_mac(mac);
                    device_container[i]._interface[last_interface].MAC_ADDRESS = mac;
                    device_container[i]._interface[last_interface].SUBNET_MASK = addNet.subnet_mask();
                    device_container[i]._interface[last_interface].CONNECTED_TO = "";
                    device_container[i]._interface[last_interface].INTERFACE_TYPE = "WAN";

                    int n_last_interface = device_container[n_id - 1].device.INTERFACE_COUNT;
                    device_container[n_id - 1]._interface[n_last_interface].ID = n_last_interface + 1;
                    device_container[n_id - 1]._interface[n_last_interface].DEFAULT_GATEWAY = "";
                    device_container[n_id - 1]._interface[n_last_interface].IP_ADDRESS = addNet.next_ip(ipv4_count, 2);
                    mac = addNet.next_mac(mac);
                    device_container[n_id - 1]._interface[n_last_interface].MAC_ADDRESS = mac;
                    device_container[n_id - 1]._interface[n_last_interface].SUBNET_MASK = addNet.subnet_mask();
                    device_container[n_id - 1]._interface[n_last_interface].CONNECTED_TO = "";
                    device_container[n_id - 1]._interface[n_last_interface].INTERFACE_TYPE = "WAN";



                    double r1 = device_container[n_id - 1].level.r = r + offset_radius;
                    double level1 = device_container[n_id - 1].level.level = level + 1;
                    double theta1 = device_container[n_id - 1].level.theta = low_angle + j * increment;
                    double increment1 = device_container[n_id - 1].level.increment = (360.0 / Math.Pow(branch, level1 + 1));
                    device_container[n_id - 1].level.low_angle = theta1 - (branch * 1.0) * increment1 / 2.0;

                    device_container[n_id - 1].pos_3d.X_OR_LON = (router_x0 + r1 * Math.Cos((theta1 / 180.0) * Math.PI));
                    device_container[n_id - 1].pos_3d.Y_OR_LAT = (router_y0 + r1 * Math.Sin((theta1 / 180.0) * Math.PI));

                    link[link_count].link_device = new LINK_DEVICE[2];
                    link[link_count].DEVICE_COUNT = 2;
                    link[link_count].LINK_ID = link_count + 1;
                    link[link_count].LINK_NAME = Convert.ToString(link_count + 1);
                    link[link_count].link_type = "P_to_P";
                    link[link_count].link_device[0].DEVICE_ID= id;
                    link[link_count].link_device[0].INTERFACE_ID = last_interface + 1;
                    link[link_count].link_device[0].NAME = "Router_" + Convert.ToString(id);
                    link[link_count].link_device[1].DEVICE_ID = n_id;
                    link[link_count].link_device[1].INTERFACE_ID = n_last_interface + 1;
                    link[link_count].link_device[1].NAME = "Router_" + Convert.ToString(n_id);
                    device_container[n_id - 1].device.INTERFACE_COUNT = n_last_interface + 1;
                    link_count++;
                    last_interface++;
                    j++;
                }
                device_container[i].device.INTERFACE_COUNT = last_interface;
            }
            /*************************************************************************************************/

           //Console.WriteLine(not_leaf_node);
            //Console.ReadLine();

            /************************************************************************************************/
            // this block of code sets the sttributes,positions,interfaces,and link(to router) of all the wirednodes
            i = max_router;
            int max_node_temp = max_node;
            while (max_node_temp != 0)
            {
                for (j = not_leaf_node; j < max_router; j++)
                {
                    int router_id = j +1;
                    int id = device_container[i].device.DEVICE_ID;
                    int last_interface = device_container[id - 1].device.INTERFACE_COUNT;
                    int router_last_interface = device_container[router_id - 1].device.INTERFACE_COUNT;
                    POS_3D pos_3d = device_container[router_id - 1].pos_3d;

                    double r = device_container[router_id - 1].level.r;
                    double theta = device_container[router_id - 1].level.theta;
                    int level = device_container[router_id - 1].level.level;
                    double low_angle = device_container[router_id - 1].level.low_angle;
                    double increment = device_container[router_id - 1].level.increment;




                    device_container[router_id - 1]._interface[router_last_interface].ID = router_last_interface + 1;
                    device_container[router_id - 1]._interface[router_last_interface].DEFAULT_GATEWAY = "";
                    string ip = addNet.next_ip(++ipv4_count, 1);
                    device_container[router_id - 1]._interface[router_last_interface].IP_ADDRESS = ip;
                    mac = addNet.next_mac(mac);
                    device_container[router_id - 1]._interface[router_last_interface].MAC_ADDRESS = mac;
                    device_container[router_id - 1]._interface[router_last_interface].SUBNET_MASK = addNet.subnet_mask();
                    device_container[router_id - 1]._interface[router_last_interface].CONNECTED_TO = "";
                    device_container[router_id - 1]._interface[router_last_interface].INTERFACE_TYPE = "ETHERNET";


                    device_container[id - 1]._interface[last_interface].ID = last_interface + 1;
                    device_container[id - 1]._interface[last_interface].DEFAULT_GATEWAY = ip;
                    device_container[id - 1]._interface[last_interface].IP_ADDRESS = addNet.next_ip(ipv4_count, 2);
                    mac = addNet.next_mac(mac);
                    device_container[id - 1]._interface[last_interface].MAC_ADDRESS = mac;
                    device_container[id - 1]._interface[last_interface].SUBNET_MASK = addNet.subnet_mask();
                    device_container[id - 1]._interface[last_interface].CONNECTED_TO = "";
                    device_container[id - 1]._interface[last_interface].INTERFACE_TYPE = "ETHERNET";


                    double r1 = device_container[id - 1].level.r = r + offset_radius;
                    //Console.WriteLine(r);
                    //Console.WriteLine(r1);
                    double level1 = device_container[id - 1].level.level = level + 1;
                    double theta1 = device_container[id - 1].level.theta = low_angle + (router_last_interface - 1) * increment;
                    double increment1 = device_container[id - 1].level.increment = (360.0 / Math.Pow(branch, level1 + 1));
                    device_container[id - 1].level.low_angle = theta1 - (branch * 1.0) * increment1 / 2.0;

                    device_container[id - 1].pos_3d.X_OR_LON = (router_x0 + r1 * Math.Cos((theta1 / 180.0) * Math.PI));
                    device_container[id - 1].pos_3d.Y_OR_LAT = (router_y0 + r1 * Math.Sin((theta1 / 180.0) * Math.PI));


                    //device_container[id - 1].pos_3d.X_OR_LON = (pos_3d.X_OR_LON + 0.1);
                    //device_container[id - 1].pos_3d.Y_OR_LAT = (pos_3d.Y_OR_LAT + 0.1);

                    link[link_count].link_device = new LINK_DEVICE[2];
                    link[link_count].DEVICE_COUNT = 2;
                    link[link_count].LINK_ID = link_count + 1;
                    link[link_count].LINK_NAME = Convert.ToString(link_count + 1);
                    link[link_count].link_type = "P_to_P_Router_WiredNode";
                    link[link_count].link_device[0].DEVICE_ID = id;
                    link[link_count].link_device[0].INTERFACE_ID = last_interface + 1;
                    link[link_count].link_device[0].NAME = "Wired_Node_" + Convert.ToString(id);
                    link[link_count].link_device[1].DEVICE_ID= router_id;
                    link[link_count].link_device[1].INTERFACE_ID = router_last_interface + 1;
                    link[link_count].link_device[1].NAME = "Router_" + Convert.ToString(router_id);


                    device_container[id - 1].device.INTERFACE_COUNT = last_interface + 1;
                    device_container[router_id - 1].device.INTERFACE_COUNT = router_last_interface + 1;

                    link_count++;
                    i++;
                    max_node_temp--;
                    if (max_node_temp == 0)
                        break;
                }
            }
            /**************************************************************************************************/


            /**************************************************************************************************/
            //this block of code sets the attributes for the adhoc link
            link[link_count].link_device = new LINK_DEVICE[max_sensor+max_gateway];
            link[link_count].DEVICE_COUNT = max_sensor + max_gateway;
            link[link_count].link_type = "M_to_M";
            link[link_count].LINK_ID = link_count + 1;
            link[link_count].LINK_NAME = Convert.ToString(link_count + 1);
            link[link_count].X = adhoc_x0;
            link[link_count].Y = adhoc_y0;
            for (i = max_router+max_node,j=0; i< (max_router + max_node+max_sensor);i++,j++)
            {
                link[link_count].link_device[j].DEVICE_ID = i + 1;
                link[link_count].link_device[j].INTERFACE_ID = 1;
                link[link_count].link_device[j].NAME = "Wireless_Sensor_"+Convert.ToString(i + 1);

            }
            link[link_count].link_device[j].DEVICE_ID = i+1;
            link[link_count].link_device[j].INTERFACE_ID = 1;
            link[link_count].link_device[j].NAME = "6_LOWPAN_Gateway_" + Convert.ToString(i+1);
            link_count++;
            /*************************************************************************************************/

            /************************************************************************************************/
            //this block of code randomly or from file  sets the  attributes for all the applications
            if (!application_from_file.Equals(""))
            {
                /*
                  source id(max_router+1,max_router+max_node) and destination id(max_router+max_node+1,max_router+max_node+max_sensor) or vice versa
                */
                application_count = 0;
                using (StreamReader sr = new StreamReader(application_from_file)) 
                 {
                     string line;
                    int count = 0;
                     while ((line = sr.ReadLine()) != null)
                     {
                        if(count==0)
                        {
                            count++;
                            continue;
                        }
                         string[] tokens = line.Split(' ');
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

                    application[i].DESTINATION_ID = rnd.Next(max_router + 1, max_router + max_node);
                    application[i].ID = i + 1;
                    application[i].NAME = Convert.ToString(i + 1);
                    application[i].SOURCE_ID = rnd.Next(max_router + max_node + 1, max_router + max_node + max_sensor);
                }
            }
            /***************************************************************************************/


            /***************************************************************************************/
            //this block of code calls the diffrent funtions to create the Configuration.netsim
            nsWriter.add_experimentInfo(root, exp_name, version_name, version_number);
            //nsWriter.add_element_from_file(root, config_helper_location + "\\ConfigHelper\\Experiment_Info.txt");
            nsWriter.add_element_from_file(root, config_helper_location + "\\ConfigHelper\\Gui_Info.txt");
            addNet.add_network(root,total_device, link_count, max_application, device_container, link, application, other_info, config_helper_location);
            nsWriter.add_simulation_parameter(root, simulation_time);
            //nsWriter.add_element_from_file(root, config_helper_location + "\\ConfigHelper\\Simulation_Parameter.txt");

            nsWriter.add_element_from_file(root, config_helper_location + "\\ConfigHelper\\Protocol_Configuration.txt");
            nsWriter.add_element_from_file(root, config_helper_location + "\\ConfigHelper\\Statistics_Collection.txt");
            nsWriter.save_document(config_helper_location + "\\Configuration.netsim");
            /***************************************************************************************/
        }
    }
}
