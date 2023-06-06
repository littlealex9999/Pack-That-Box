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
        Customer newCustomer = Instantiate(customers[Random.Range(0, customers.Length)]).GetComponent<Customer>();
        currentCustomers.Add(newCustomer);

        List<PackItem> newCustomerItems = new List<PackItem>();

        for (int i = 0; i < itemsPerPerson; ++i) {
            newCustomerItems.Add(requestableObjects[Random.Range(0, requestableObjects.Length)].GetComponent<PackItem>());
        }

        newCustomer.AssignItems(newCustomerItems);
    }
}
