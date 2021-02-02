using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace NetSim_Scenario_Generation_Internetworks
{
    /*  structure to store the variables in Application  */
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

    /* structure which stores information regarding position from centre of the grid */
    public struct LEVEL
    {
        public double r;
        public double theta;
        public int level;
        public double low_angle;
        public double increment;

    }
    /*  structure to store the variables in LINK configuration between two node */
    public struct LINK
    {
        public int LINK_ID;
        public string LINK_NAME;
        public int DEVICE_ID_1;
        public int INTERFACE_ID_1;
        public string NAME_1;
        public int DEVICE_ID_2;
        public int INTERFACE_ID_2;
        public string NAME_2;
    }

    /*  structure to store the variables in a node(device)) */
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
    }

    /*  structure to store the variables of an Interface of a node  */
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

    /*  structure to store node ,pos_3d and all interfaces of that node  */
    public struct DEVICE_CONTAINER
    {
        public DEVICE device;
        public POS_3D pos_3d;
        public LEVEL level;
        public INTERFACE[] _interface;
    }
    public class NetSimScenario
    {
        public static XmlWriter nsWriter;
        public static AddNetworkElement addNet;

        static void Main(string[] args)
        {
            //mac address of devices from this set of character
            string mac = "123456789ABC";
            int i, j, ip_count = 0, link_count = 0, application_count = 0;

            //user parameters for generating scenario(Configuration.netsim)
            int max_interface = 11;   /* max upto 25 */
            int max_hop = 6;          /* should be 2,4,6(even)*/
            int max_node = 100
                ;        /*max upto Math.Pow(max_interface-1,max_hop/2)*(max_interface-1) */

            int max_application = 2;  /*max up to (max_node*(max_node-1)/2)*/
            double x0 = 250.0, y0 = 250.0, offset_radius = 50.0;


            //intermediate parameters used in generating scenario
            int branch = max_interface - 1;
            int height = max_hop / 2;
            int max_router = ((int)Math.Pow(branch, height + 1) - 1) / (branch - 1);
            int not_leaf_node = ((int)Math.Pow(branch, height) - 1) / (branch - 1);
            int total_node = max_router + max_node;
            int max_link = (int)Math.Pow(max_interface - 1, (max_hop / 2)) + max_node + 1000;
            Random rnd = new Random();
            nsWriter = new XmlWriter();
            addNet = new AddNetworkElement();
            XmlNode root = nsWriter.open_document();
 
            //application for containing detailes of applications
            APPLICATION[] application = new APPLICATION[max_application];

            //device_container to store varible for each device
            DEVICE_CONTAINER[] device_container = new DEVICE_CONTAINER[total_node];

            //link fot storing details of all the links
            LINK[] link = new LINK[max_link];



            for (i = 0; i < max_router; i++)
            {
                device_container[i].device.DEFAULT_DEVICE_NAME = "Router";
                device_container[i].device.DEVICE_ID = i + 1;
                device_container[i].device.DEVICE_IMAGE = "InternalRouter";
                device_container[i].device.DEVICE_NAME = "Router_" + Convert.ToString(i + 1);
                device_container[i].device.DEVICE_TYPE = "ROUTER";
                device_container[i].device.INTERFACE_COUNT = 0;
                device_container[i].device.TYPE = "ROUTER";
                device_container[i].device.WIRESHARK_OPTION = "Disable";

                device_container[i].pos_3d.X_OR_LON = 0.0;
                device_container[i].pos_3d.Y_OR_LAT = 0.0;
                device_container[i].pos_3d.Z = 0.0;

                device_container[i]._interface = new INTERFACE[max_interface];
            }

            for (; i < total_node; i++)
            {
                device_container[i].device.DEFAULT_DEVICE_NAME = "Wired_Node";
                device_container[i].device.DEVICE_ID = i + 1;
                device_container[i].device.DEVICE_IMAGE = "WiredNode";
                device_container[i].device.DEVICE_NAME = "Wired_Node_" + Convert.ToString(i + 1);
                device_container[i].device.DEVICE_TYPE = "WIREDNODE";
                device_container[i].device.INTERFACE_COUNT = 0;
                device_container[i].device.TYPE = "NODE";
                device_container[i].device.WIRESHARK_OPTION = "Disable";

                device_container[i].pos_3d.X_OR_LON = 0.0;
                device_container[i].pos_3d.Y_OR_LAT = 0.0;
                device_container[i].pos_3d.Z = 0.0;

                device_container[i]._interface = new INTERFACE[1];
            }

            device_container[0].level.r = 0.0;
            double theta0 = device_container[0].level.theta = 0.0;
            double level0 = device_container[0].level.level = 0;
            double increment0 = device_container[0].level.increment = (360.0 / Math.Pow(branch, level0 + 1));
            device_container[0].level.low_angle = theta0 - (branch * 1.0) * increment0 / 2.0;

            device_container[0].pos_3d.X_OR_LON = x0;
            device_container[0].pos_3d.Y_OR_LAT = y0;



            for (i = 0; i < max_router; i++)
            {
                if (i + 1 > not_leaf_node)
                    break;
                int id = device_container[i].device.DEVICE_ID, index = 2;
                int last_interface = device_container[i].device.INTERFACE_COUNT;
                POS_3D pos_3d = device_container[i].pos_3d;

                double r = device_container[i].level.r;
                double theta = device_container[i].level.theta;
                int level = device_container[i].level.level;
                double low_angle = device_container[i].level.low_angle;
                double increment = device_container[i].level.increment;

                //double half = (360.0 / Math.Pow(branch, level)) / 2; 




                j = 0;
                while (j < branch)
                {
                    device_container[i]._interface[last_interface].ID = last_interface + 1;
                    device_container[i]._interface[last_interface].DEFAULT_GATEWAY = "";
                    device_container[i]._interface[last_interface].IP_ADDRESS = addNet.next_ip(++ip_count, 1);
                    mac =addNet.next_mac(mac);
                    device_container[i]._interface[last_interface].MAC_ADDRESS = mac;
                    device_container[i]._interface[last_interface].SUBNET_MASK = addNet.subnet_mask();
                    device_container[i]._interface[last_interface].CONNECTED_TO = "";
                    device_container[i]._interface[last_interface].INTERFACE_TYPE = "WAN";


                    int n_id = (id - 1) * branch + (index++);
                    int n_last_interface = device_container[n_id - 1].device.INTERFACE_COUNT;
                    device_container[n_id - 1]._interface[n_last_interface].ID = n_last_interface + 1;
                    device_container[n_id - 1]._interface[n_last_interface].DEFAULT_GATEWAY = "";
                    device_container[n_id - 1]._interface[n_last_interface].IP_ADDRESS = addNet.next_ip(ip_count, 2);
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

                    device_container[n_id - 1].pos_3d.X_OR_LON = (x0 + r1 * Math.Cos((theta1 / 180.0) * Math.PI));
                    device_container[n_id - 1].pos_3d.Y_OR_LAT = (y0 + r1 * Math.Sin((theta1 / 180.0) * Math.PI));

                    link[link_count].LINK_ID = link_count + 1;
                    link[link_count].LINK_NAME = Convert.ToString(link_count + 1);
                    link[link_count].DEVICE_ID_1 = id;
                    link[link_count].INTERFACE_ID_1 = last_interface + 1;
                    link[link_count].NAME_1 = "Router_" + Convert.ToString(id);
                    link[link_count].DEVICE_ID_2 = n_id;
                    link[link_count].INTERFACE_ID_2 = n_last_interface + 1;
                    link[link_count].NAME_2 = "Router_" + Convert.ToString(n_id);
                    device_container[n_id - 1].device.INTERFACE_COUNT = n_last_interface + 1;
                    link_count++;
                    last_interface++;
                    j++;
                }
                device_container[i].device.INTERFACE_COUNT = last_interface;
            }

            i = max_router;
            while (max_node != 0)
            {
                for (j = not_leaf_node; j < max_router; j++)
                {
                    int router_id = j + 1;
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
                    string ip = addNet.next_ip(++ip_count, 1);
                    device_container[router_id - 1]._interface[router_last_interface].IP_ADDRESS = ip;
                    mac = addNet.next_mac(mac);
                    device_container[router_id - 1]._interface[router_last_interface].MAC_ADDRESS = mac;
                    device_container[router_id - 1]._interface[router_last_interface].SUBNET_MASK = addNet.subnet_mask();
                    device_container[router_id - 1]._interface[router_last_interface].CONNECTED_TO = "";
                    device_container[router_id - 1]._interface[router_last_interface].INTERFACE_TYPE = "ETHERNET";


                    device_container[id - 1]._interface[last_interface].ID = last_interface + 1;
                    device_container[id - 1]._interface[last_interface].DEFAULT_GATEWAY = ip;
                    device_container[id - 1]._interface[last_interface].IP_ADDRESS = addNet.next_ip(ip_count, 2);
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

                    device_container[id - 1].pos_3d.X_OR_LON = (x0 + r1 * Math.Cos((theta1 / 180.0) * Math.PI));
                    device_container[id - 1].pos_3d.Y_OR_LAT = (y0 + r1 * Math.Sin((theta1 / 180.0) * Math.PI));


                    //device_container[id - 1].pos_3d.X_OR_LON = (pos_3d.X_OR_LON + 0.1);
                    //device_container[id - 1].pos_3d.Y_OR_LAT = (pos_3d.Y_OR_LAT + 0.1);

                    link[link_count].LINK_ID = link_count + 1;
                    link[link_count].LINK_NAME = Convert.ToString(link_count + 1);
                    link[link_count].DEVICE_ID_1 = id;
                    link[link_count].INTERFACE_ID_1 = last_interface + 1;
                    link[link_count].NAME_1 = "Wired_Node_" + Convert.ToString(id);
                    link[link_count].DEVICE_ID_2 = router_id;
                    link[link_count].INTERFACE_ID_2 = router_last_interface + 1;
                    link[link_count].NAME_2 = "Router_" + Convert.ToString(router_id);


                    device_container[id - 1].device.INTERFACE_COUNT = last_interface + 1;
                    device_container[router_id - 1].device.INTERFACE_COUNT = router_last_interface + 1;

                    link_count++;
                    i++;
                    max_node--;
                    if (max_node == 0)
                        break;
                }

            }



           
            /* application_count = 0;
             using (StreamReader sr = new StreamReader("ConfigHelper\\Application_Config.txt"))
             {
                 string line;
                 while ((line = sr.ReadLine()) != null)
                 {
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
             }*/


            for (i = 0; i < max_application; i++)
            {

                application[i].DESTINATION_ID = rnd.Next(max_router + 2 + i, total_node);
                application[i].ID = i + 1;
                application[i].NAME = "App" + Convert.ToString(i + 1) + "_CBR";
                application[i].SOURCE_ID = max_router + 1 + i;
            }

            
        
            nsWriter.add_element_from_file(root, "ConfigHelper\\experimentInfo.txt");
            nsWriter.add_element_from_file(root, "ConfigHelper\\guiInfo.txt");
            addNet.add_network(root, total_node, link_count, max_application, device_container, link, application);
            nsWriter.add_element_from_file(root, "ConfigHelper\\Simulation_Parameter.txt");

            nsWriter.add_element_from_file(root, "ConfigHelper\\Protocol_Configuration.txt");
            nsWriter.add_element_from_file(root, "ConfigHelper\\Statistics_Collection.txt");
            nsWriter.save_document();

        }  
    }
}
