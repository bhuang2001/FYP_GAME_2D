using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForwardingTableManager : MonoBehaviour
{
    // get access to updating the forwarding table for the selected router if it is active
    [SerializeField] private SelectionManager selectionManager;
    //  to know whether or not to update forwarding tables
    [SerializeField] private bool updateForwardingTable, prevUpdate;
    // This is to know if there is update in OSPF/RIP mode.
    [SerializeField]private RoutingInformation routingInformation;
    [SerializeField]private GraphCreator graphCreator;
    // To store the gameobject for the forwarding table UI
    [SerializeField] private GameObject forwardingTable;
    // Start is called before the first frame update
    void Start()
    {
        selectionManager = GameObject.Find("SelectObjectsFunc").GetComponent<SelectionManager>();
        routingInformation = GameObject.Find("Routing Information").GetComponent<RoutingInformation>();
        graphCreator = GameObject.Find("Graph Creator").GetComponent<GraphCreator>();
    }

    // Update is called once per frame
    void Update()
    {
        // if OSPF is on, then update forwarding table whenever there is routing information exchange
        // For OSPF, routing information exchange occurs when there is a change in network topology,
        // which is the addition of network components and every hello interval to check if links are still up.
        if(graphCreator.OSPF == true)
        {
            updateForwardingTable = routingInformation.updateOSPF;
        }
        // else if RIP is on, then update forwarding table whenever there is routing information exchange
        // For RIP, it occurs every periodic interval (5 seconds in our case)
        else if (graphCreator.RIP == true)
        {
            updateForwardingTable = routingInformation.updateRIP;
        }
        

        // Check if there is a change in updateForwardingTable (this ensures it runs once only when it 
        // needs to update the forwarding table)
        if(prevUpdate != updateForwardingTable)
        {
            if(forwardingTable.gameObject.activeSelf && updateForwardingTable == true)
            {
                Debug.Log ("Forwarding Table is updated because it is active and update is triggered");
                selectionManager.CreateForwardingTable();
            }
        }
        prevUpdate = updateForwardingTable;
    }
}
