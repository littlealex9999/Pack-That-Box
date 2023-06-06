using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // singleton for references to a single game manager

    enum GameState
    {
        Menu,
        Playing,
    }

    GameState state = GameState.Menu;

    float score;

    [SerializeField] Transform customerSpawnLocation;
    [SerializeField] Transform customerWaitLocation;
    [SerializeField] int itemsPerPerson = 3;

    [SerializeField] GameObject[] customers;
    [SerializeField] GameObject[] requestableObjects;

    List<Customer> currentCustomers = new List<Customer>();

    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    void Update()
    {
        
    }

    void CreateNewCustomer()
    {
        // create a random customer
        Customer newCustomer = Instantiate(customers[Random.Range(0, customers.Length)]).GetComponent<Customer>();
        currentCustomers.Add(newCustomer);

        List<PackItem> newCustomerItems = new List<PackItem>();

        for (int i = 0; i < itemsPerPerson; ++i) {
            // add random items to our list
            newCustomerItems.Add(requestableObjects[Random.Range(0, requestableObjects.Length)].GetComponent<PackItem>());
        }

        newCustomer.AssignItems(newCustomerItems);
    }

    bool CheckBoxDone(Box box)
    {
        foreach (Customer c in currentCustomers) {
            List<int> usedItems = new List<int>();

            for (int i = 0; i < c.items.Count; ++i) {
                bool itemFound = false;

                for (int j = 0; j < box.itemsInBox.Count; ++j) {
                    if (usedItems.Contains(j)) continue;

                    if (c.items[i].itemID == box.itemsInBox[j].itemID) {
                        itemFound = true;
                        usedItems.Add(j); // don't use the same object twice for checking if we have everything

                        break;
                    }
                }

                if (!itemFound) break; // we are missing one of the items meant to be in the box
            }

            if (usedItems.Count >= c.items.Count) { // we have all the items the customer wants
                int unnecessaryItems = box.itemsInBox.Count - usedItems.Count;

                // calculate score

                return true;
            }
        }

        return false;
    }
}
