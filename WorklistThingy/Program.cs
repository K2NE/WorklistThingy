using SourceCode.Workflow.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorklistThingy
{
    class Program
    {

        static void Main(string[] args)
        {
            string server = "localhost";
            string filter = string.Empty;
            if (args.Length >= 1)
            {
                server = args[0];
            }
            if (args.Length >= 2)
            {
                filter = string.Concat("*", args[1], "*");
            }

            Console.WriteLine("Starting worklist application. Connecting to {0}. Using filter '{1}'.", server, filter);
            using (Connection k2con = new Connection())
            {
                k2con.Open(server);
                ConsoleKeyInfo kInfo = new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false);
                int startIndex = 0;
                while (kInfo.KeyChar != 'q')
                {
                    WorklistCriteria wc = new WorklistCriteria();
                    wc.AddFilterField(WCLogical.Or, WCField.WorklistItemOwner, "Me", WCCompare.Equal, WCWorklistItemOwner.Me); //This will return all the user’s items
                    wc.AddFilterField(WCLogical.Or, WCField.WorklistItemOwner, "Other", WCCompare.Equal, WCWorklistItemOwner.Other); //This will return all the user’s shared items (out of office items)
  
                    if (!string.IsNullOrEmpty(filter))
                    {
                        wc.AddFilterField(WCField.ProcessFolio, WCCompare.Like, filter);
                    }
                    wc.StartIndex = startIndex;
                    wc.Count = 10;



                    Worklist wl = k2con.OpenWorklist(wc);
                    Console.WriteLine("Nr {0,-9} {1,-9} {2}", "Status", "SN", "Folio");
                    int i = 0;
                    Dictionary<int, string> helperList = new Dictionary<int, string>();
                    foreach (WorklistItem wli in wl)
                    {
                        Console.WriteLine("{0}  {1,-9} {2,-7} {3}", i, wli.Status, wli.SerialNumber, wli.ProcessInstance.Folio);
                        helperList.Add(i, wli.SerialNumber);
                        i++;
                    }

                    Console.WriteLine("\nEnter item number, q(uit), n(ext), p(rev), r(efresh) or f(ilter).");
                    kInfo = Console.ReadKey();
                    if (kInfo.KeyChar == 'n')
                    {
                        startIndex += wc.Count;
                    }
                    else if (kInfo.KeyChar == 'p')
                    {
                        startIndex -= wc.Count;
                        if (startIndex < 0)
                        {
                            startIndex = 0;
                        }
                    }
                    else if (kInfo.KeyChar == 'r')
                    {
                        startIndex = 0;
                    }
                    else if (kInfo.KeyChar == 'f')
                    {
                        Console.WriteLine("Enter filter:");
                        filter = string.Concat("*", Console.ReadLine(), "*");
                    }
                    else if (kInfo.KeyChar >= '0' && kInfo.KeyChar <= '9')
                    {
                        int nr = int.Parse(kInfo.KeyChar.ToString());
                        if (nr >= i)
                        {
                            Console.WriteLine("Task does not exist.");
                        }
                        else
                        {
                            ShowWorklistItem(k2con, helperList[nr]);
                        }
                    }
                }
            }
        }

        private static void ShowWorklistItem(Connection k2con, string p)
        {
            WorklistItem item = k2con.OpenWorklistItem(p);
            Console.WriteLine("");
            Console.WriteLine("Process Instance Id: {0}", item.ProcessInstance.ID);
            Console.WriteLine("Process Fullname: {0}", item.ProcessInstance.FullName);
            Console.WriteLine("Process Folio: {0}", item.ProcessInstance.Folio);

            Console.WriteLine("ACTIONS:\nNr\tBatchable\tName");
            int i = 0;
            foreach (SourceCode.Workflow.Client.Action a in item.Actions)
            {
                Console.WriteLine("{0}\t{1}\t{2}", i, a.Batchable, a.Name);
                i++;
            }
            Console.WriteLine("Enter nr to action, or r(elease).");
            ConsoleKeyInfo kInfo = Console.ReadKey();
            if (kInfo.KeyChar >= '0' && kInfo.KeyChar <= '9')
            {
                int nr = int.Parse(kInfo.KeyChar.ToString());
                if (nr >= i)
                {
                    Console.WriteLine("Nr does not exist.");
                }
                else
                {
                    item.Actions[nr].Execute();
                }
            }
            else if (kInfo.KeyChar == 'r')
            {
                item.Release();
            }
            else
            {
                Console.WriteLine("NR or R dude.");
            }

        }
    }
}
