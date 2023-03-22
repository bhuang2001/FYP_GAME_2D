using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RouteAggregation : MonoBehaviour
{
    // Class for routes with similar networks and same outgoing links
    public class RouteEntry
    {
        public List<string> destinationAddress = new List<string>();
        public List<uint> destinationAddressUINT =  new List<uint>();
        public string hopAddress;
        public string outgoingLink;
        public uint networkIP;
        public string destinationAddressPrefix; 

    }

    // Routing information known to the router 
    public List<string> destinationAddress =  new List<string>();
    public List<uint> destinationAddressUINT = new List<uint>();
    public List<string> hopAddress = new List<string>();
    public List<string> outgoingLink = new List<string>();


    // Refer to Example Network created in the Route Aggregation word file
    // Destination Addresses        Hop Address     Outgoing Link
    // 128.128.1.0 - 128.128.3.0    128.1.1.1       Link 1
    // 128.192.1.0 - 128.192.3.0    128.1.2.1       Link 2
    // 128.224.1.0 - 128.192.3.0    128.1.3.1       Link 3
    // 128.240.1.0 - 128.240.3.0    128.1.4.1       Link 4

    // Create this set of data in Start
    // Start is called before the first frame update
    void Start()
    {
        // First range of entries
       destinationAddress.Add("128.128.1.0");
       destinationAddress.Add("128.128.2.0");
       destinationAddress.Add("128.128.3.0");
       destinationAddressUINT.Add(2155872512);
       destinationAddressUINT.Add(2155872768);
       destinationAddressUINT.Add(2155873024);
       hopAddress.Add("128.1.1.1");
       hopAddress.Add("128.1.1.1");
       hopAddress.Add("128.1.1.1");
       outgoingLink.Add("Link 1");
       outgoingLink.Add("Link 1");
       outgoingLink.Add("Link 1");

       // Second range of entries
       destinationAddress.Add("128.192.1.0");
       destinationAddress.Add("128.192.2.0");
       destinationAddress.Add("128.192.3.0");
       destinationAddressUINT.Add(2160066816);
       destinationAddressUINT.Add(2160067072);
       destinationAddressUINT.Add(2160067328);
       hopAddress.Add("128.1.2.1");
       hopAddress.Add("128.1.2.1");
       hopAddress.Add("128.1.2.1");
       outgoingLink.Add("Link 2");
       outgoingLink.Add("Link 2");
       outgoingLink.Add("Link 2");

       // Third range of entries
       destinationAddress.Add("128.224.1.0");
       destinationAddress.Add("128.224.2.0");
       destinationAddress.Add("128.224.3.0");
       destinationAddressUINT.Add(2162163968);
       destinationAddressUINT.Add(2162164224);
       destinationAddressUINT.Add(2162164480);
       hopAddress.Add("128.1.3.1");
       hopAddress.Add("128.1.3.1");
       hopAddress.Add("128.1.3.1");
       outgoingLink.Add("Link 3");
       outgoingLink.Add("Link 3");
       outgoingLink.Add("Link 3");

        // Fourth range of entries
       destinationAddress.Add("128.240.1.0");
       destinationAddress.Add("128.240.2.0");
       destinationAddress.Add("128.240.3.0");
       destinationAddressUINT.Add(2163212544);
       destinationAddressUINT.Add(2163212800);
       destinationAddressUINT.Add(2163213056);
       hopAddress.Add("128.1.4.1");
       hopAddress.Add("128.1.4.1");
       hopAddress.Add("128.1.4.1");
       outgoingLink.Add("Link 4");
       outgoingLink.Add("Link 4");
       outgoingLink.Add("Link 4");

       //Organize into RouteEntry Objects
        List<RouteEntry> routeEntriesList = new List<RouteEntry>(); 
        int entriesCount = destinationAddress.Count;
        uint networkPortion;
        // To know if the entry was added into a route entry object
        bool entryAdded = false;
       
        for (int i = 0 ; i < entriesCount; i++)
        {
            // Check if route entries list is empty
            if( routeEntriesList.Count == 0)
            {
                //Create the first route entry and add to list
                RouteEntry routeEntry = new RouteEntry();
                routeEntry.destinationAddress.Add(destinationAddress[i]);
                routeEntry.destinationAddressUINT.Add(destinationAddressUINT[i]);
                routeEntry.hopAddress = hopAddress[i];
                routeEntry.outgoingLink = outgoingLink[i];
                // logical AND the destination address with the network mask (excludes subnet)
                routeEntry.networkIP = destinationAddressUINT[i] & 0xFFFF0000;
                routeEntriesList.Add(routeEntry);
            }
            // Route entries list is not empty, check to see if the current destination address can be
            // grouped with another 
            else
            {
                // Get the network portion of the current destination address
                networkPortion = destinationAddressUINT[i] & 0xFFFF0000;
                // First check the outgoing link and the network portion of the current
                // destination address to see if there is a match
                for(int j = 0; j < routeEntriesList.Count; j++)
                {
                    // If it matches then add the current entry into the route entry object 
                    // and it hasn't been added into a route entry object
                    if(outgoingLink[i] == routeEntriesList[j].outgoingLink 
                      && networkPortion == routeEntriesList[j].networkIP && entryAdded == false)
                    {
                        // Add destination address to the existing route entry object
                        routeEntriesList[j].destinationAddress.Add(destinationAddress[i]);
                        routeEntriesList[j].destinationAddressUINT.Add(destinationAddressUINT[i]);
                        entryAdded = true;
                    }
                }
                // If after iterating through existing route entries and entryAdded is still false
                // meaning that there were no existing matches, then create its own route entry
                if(entryAdded == false)
                {
                    RouteEntry newEntry = new RouteEntry();
                    newEntry.destinationAddress.Add(destinationAddress[i]);
                    newEntry.destinationAddressUINT.Add(destinationAddressUINT[i]);
                    newEntry.hopAddress = hopAddress[i];
                    newEntry.outgoingLink = outgoingLink[i];
                    // logical AND the destination address with the network mask (excludes subnet)
                    newEntry.networkIP = destinationAddressUINT[i] & 0xFFFF0000;
                    routeEntriesList.Add(newEntry);
                }
                // make sure to set entryAdded back to false at the end of each iteration 
                entryAdded = false;
            }
            
        }

        DisplayAllRouteEntries(routeEntriesList);
        SummarizeAllRouteEntries(routeEntriesList);
        DisplayAllSummarizedRoutes(routeEntriesList);
    }


    // Display Methods for Singular Entries
    private void DisplayRouteEntry(RouteEntry routeEntry)
    {
        for(int i = 0 ; i < routeEntry.destinationAddress.Count; i++)
        {
            Debug.Log(routeEntry.destinationAddress[i] + "\t\t" + routeEntry.hopAddress + "\t\t" 
            + routeEntry.outgoingLink + "\n");
        }
    }
    // Display Methods for Singular Entries
    private void DisplayAllRouteEntries(List<RouteEntry> routeEntryList)
    {
        for(int i = 0; i < routeEntryList.Count; i++)
        {
            Debug.Log("Route Entry " + i + "\n");
            Debug.Log("Destination Address \t Next Hop Address \t Outgoing Link \n" );
            DisplayRouteEntry(routeEntryList[i]);
        }
    }

    //Display Methods for Forwarding Table with Route Aggregation
    private void DisplaySummarizedRoute(RouteEntry routeEntry)
    {
        
        Debug.Log(routeEntry.destinationAddressPrefix + "\t\t" + routeEntry.hopAddress + "\t\t" 
        + routeEntry.outgoingLink + "\n");
        
    }
    //Display Methods for Forwarding Table with Route Aggregation
    private void DisplayAllSummarizedRoutes(List<RouteEntry> routeEntryList)
    {
        for(int i = 0; i < routeEntryList.Count; i++)
        {
            Debug.Log("Route Entry " + i + "\n");
            Debug.Log("Destination Address \t Next Hop Address \t Outgoing Link \n" );
            DisplaySummarizedRoute(routeEntryList[i]);
        }
    }

    // Route Summarization Methods
    // Calculate the destination address prefix that encompasses all routes in this route entry object
    private void SummarizeRouteEntry(RouteEntry routeEntry)
    {
        uint summarizedIP = 0;
        uint prefixMask = 0;
        int prefixSize = 0;
        bool done = false;
        uint firstIP = 0;
        // If there are 2 or more destination addresses in the route entry object then perform route aggregation
        // Else the destination "prefix" address wil remain as the sole destination address in the list
        if(routeEntry.destinationAddressUINT.Count > 1)
        {
        // Algorithm to get the most specific match amongst all destination addresses in the route entry object
        for(int i = 0; i < 32; i++)
        {
            prefixSize = i;
            uint one = 1;
            firstIP = routeEntry.destinationAddressUINT[0];
            // Check the first occurence where the bit isn't matching
            firstIP = firstIP & (one << (31 -i));
            foreach(uint ip in routeEntry.destinationAddressUINT)
            {
                uint otherIP = ip;
                otherIP = otherIP & (one << (31-i));
                if(otherIP == firstIP)
                {
                    
                }
                else
                {   
                    // The summarized IP would take the smaller of the two IPs that differed in bits
                    summarizedIP = Math.Min(routeEntry.destinationAddressUINT[0],ip);
                    prefixMask = GetMask(prefixSize);
                    // mask the smaller of the two IPs to get the summarized route
                    summarizedIP = summarizedIP & prefixMask;
                    Debug.Log("Summarized IP :" + summarizedIP);
                    done = true;
                }
                // if the longest/ most specific match is found then break out of loop
                if(done == true)
                {
                    break;
                }
            }
            // if the longest/ most specific match is found then break out of loop
            if(done == true)
            {
                break;
            }
        }
        routeEntry.destinationAddressPrefix = ConvertToIPString(summarizedIP);
        routeEntry.destinationAddressPrefix += "/" + prefixSize;
        }
        else
        {
        routeEntry.destinationAddressPrefix = routeEntry.destinationAddress[0];
        }
        
    }

    private void SummarizeAllRouteEntries(List<RouteEntry> routeEntryList)
    {
        for( int i = 0; i < routeEntryList.Count; i++)
        {
            SummarizeRouteEntry(routeEntryList[i]);
        }
    }

    // Returns a mask depending on the prefix size in uint
    public static uint GetMask(int prefixSize)
         {
             switch (prefixSize)
             {
                 case 0: return   0;          //("0.0.0.0");
                 case 1: return   2147483648;          //("128.0.0.0");
                 case 2: return   3221225472;          //("192.0.0.0");
                 case 3: return   3758096384;          //("224.0.0.0");
                 case 4: return   4026531840;          //("240.0.0.0");
                 case 5: return   4160749568;          //("248.0.0.0");
                 case 6: return   4227858432;          //("252.0.0.0");
                 case 7: return   4261412864;          //("254.0.0.0");
                 case 8: return   4278190080;          //("255.0.0.0");
                 case 9: return   4286578688;          //("255.128.0.0");
                 case 10: return  4290772992;          //("255.192.0.0");
                 case 11: return  4292870144;          //("255.224.0.0");
                 case 12: return  4293918720;          //("255.240.0.0");
                 case 13: return  4294443008;          //("255.248.0.0");
                 case 14: return  4294705152;          //("255.252.0.0");
                 case 15: return  4294836224;          //("255.254.0.0");
                 case 16: return  4294901760;          //("255.255.0.0");
                 case 17: return  4294934528;          //("255.255.128.0");
                 case 18: return  4294950912;          //("255.255.192.0");
                 case 19: return  4294959104;          //("255.255.224.0");
                 case 20: return  4294963200;          //("255.255.240.0");
                 case 21: return  4294965248;          //("255.255.248.0");
                 case 22: return  4294966272;          //("255.255.252.0");
                 case 23: return  4294966784;          //("255.255.254.0");
                 case 24: return  4294967040;          //("255.255.255.0");
                 case 25: return  4294967168;          //("255.255.255.128");
                 case 26: return  4294967232;          //("255.255.255.192");
                 case 27: return  4294967264;          //("255.255.255.224");
                 case 28: return  4294967280;          //("255.255.255.240");
                 case 29: return  4294967288;          //(255.255.255.248");
                 case 30: return  4294967292;          //("255.255.255.252");
                 case 31: return  4294967294;          //("255.255.255.254");
                 case 32: return  4294967295;          //("255.255.255.255");



                 default: return 0;                    //("0.0.0.0");
             }
         }
    // Convert uint to IP in dotted decimal format
    private string ConvertToIPString(uint ip)
    {   
        string IPString = "";
        byte[] bytes= BitConverter.GetBytes(ip);
        if(BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
     // subnetIPString = BitConverter.ToString(bytes);

        for ( int i = 0; i < 4; i++)
        {
            int octet = 0xFF & bytes[i];
            IPString += "." + octet;
        }
        IPString = IPString.Substring(1);
        return IPString;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
