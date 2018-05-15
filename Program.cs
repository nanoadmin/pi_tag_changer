using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PITagChanger_displaydigits
{
    class Program
    {

        public static string border_string = "\n\n################################################################################################\n\n";

        static void Main(string[] args)
        {


            bool ISDEBUGGING = Properties.Settings.Default.IS_DEBUG;

            Console.WriteLine("is debug set to: {0}", ISDEBUGGING);

            string piservername = Properties.Settings.Default.pi_server_to_alter;
            string monitorPIPointName = Properties.Settings.Default.monitor_tag_pending_events;
            string monitorPISErverNAme = Properties.Settings.Default.monitor_tag_pi_server;

            PISDK.PISDK pisdk = new PISDK.PISDK();
            PISDK.Server piserv = pisdk.Servers[piservername];
            PISDK.PIPoints pips = piserv.PIPoints;

            int i = 0;
            int thisI = 0;


            //get the pending events pi point (have to connect to another PI server for this)
            PISDK.PISDK moni_PISDK = new PISDK.PISDK();
            PISDK.Server moni_PIServer = moni_PISDK.Servers[monitorPISErverNAme];
            PISDK.PIPoint pendingEventsPIPoint = moni_PIServer.PIPoints[monitorPIPointName];


            int pendingEvents = (int)pendingEventsPIPoint.Data.Snapshot.Value;
            Console.WriteLine("There are currently {0} pending events in the PI update manager", pendingEvents);
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));


            foreach (PISDK.PIPoint pipnt in pips)
            {
                try
                {
                    //Console.WriteLine(pipnt.Name);

                    PISDK.PointTypeConstants pt = pipnt.PointType;

                    if (pt == PISDK.PointTypeConstants.pttypFloat32 || pt == PISDK.PointTypeConstants.pttypFloat64
                                    || pt == PISDK.PointTypeConstants.pttypInt16 || pt == PISDK.PointTypeConstants.pttypInt32)
                    {

                        //if integer then 0 else 3
                        int replacementVal = (pt == PISDK.PointTypeConstants.pttypFloat32 ||
                                                    pt == PISDK.PointTypeConstants.pttypFloat64) ? 3 : 0;

                        Console.WriteLine("iteration {0}", i);

                        //only change tags which habe the defaulty setting (-5). OR if we are debugging, do all tags (for ease of development).
                        if (pipnt.PointAttributes["displaydigits"].Value == -5 || ISDEBUGGING)
                        {
                            Console.WriteLine("point type is of type, {0}, converting value to {1}, tagname:{2}", pt, replacementVal, pipnt.Name);
                            pipnt.PointAttributes.ReadOnly = false;
                            pipnt.PointAttributes["displaydigits"].Value = replacementVal;
                            thisI++;
                        }


                        if (thisI >= 200)
                        {
                            int pending_events_max = Properties.Settings.Default.pending_events_max;

                            Console.WriteLine(border_string);

                            Console.WriteLine("there have been 200 changes, check PIupdate manager pending events is less than {0}", pending_events_max);

                            bool leaveWhile = false;

                            while (!leaveWhile)
                            {
                                pendingEvents = (int)pendingEventsPIPoint.Data.Snapshot.Value;
                                Console.WriteLine("There are currently {0} pending events in the PI update manager, (max set to {1})", pendingEvents, pending_events_max);

                                //leave the while loop if there are less than X events pending 
                                leaveWhile = (pendingEvents < pending_events_max);

                                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));

                            }

                            Console.WriteLine(border_string);

                            thisI = 0;
                        }

                        i++;
                    }

                }

                catch (Exception e)
                {

                    Console.WriteLine("exception caused: {0}", e.Message);
                }

                i++;                
            }

        }
    }
}
