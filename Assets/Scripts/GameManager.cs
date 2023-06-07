using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Variables
    public static GameManager instance; // singleton for references to a single game manager

    public enum GameState
    {
        Menu,
        Playing,
    }

    GameState state = GameState.Menu;

    float score;

    [SerializeField] float minutesToGameEnd;
    float timer;

    [SerializeField] Transform customerSpawnLocation;
    [SerializeField] Transform customerWaitLocation;
    [SerializeField] int itemsPerPerson = 3;

    [SerializeField] GameObject[] customers;
    [SerializeField] GameObject[] requestableObjects;

    List<Customer> currentCustomers = new List<Customer>();
    [HideInInspector] public List<Box> preparedBoxes = new List<Box>();
    #endregion

    #region Access Properties
    public GameState currentGameState { get { return state; } }
    public float remainingTime { get { return timer; } }
    #endregion

    #region Functions
    #region Unity
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        state = GameState.Playing; // just a debug measure to actually get gameplay happening
    }

    void Update()
    {
        switch (state) {
            case GameState.Menu:

                break;
            case GameState.Playing:
                timer -= Time.deltaTime;
                if (timer <= 0) EndGame(); // the game ends when time hits 0

                // game logic
                if (currentCustomers.Count == 0) {
                    CreateNewCustomer();
                }

                foreach (Box b in preparedBoxes) {
                    if (CheckBoxDone(b, out Customer happyCustomer)) {
                        Destroy(b.gameObject);
                        Destroy(happyCustomer.gameObject);
                    }
                }
                break;
        }
    }
    #endregion

    #region Gameplay & Flow
    void CreateNewCustomer()
    {
        // create a random customer
        Customer newCustomer = Instantiate(customers[Random.Range(0, customers.Length)], customerSpawnLocation.position, Quaternion.identity).GetComponent<Customer>();
        currentCustomers.Add(newCustomer);

        List<PackItem> newCustomerItems = new List<PackItem>();

        for (int i = 0; i < itemsPerPerson; ++i) {
            // add random items to our list
            newCustomerItems.Add(requestableObjects[Random.Range(0, requestableObjects.Length)].GetComponent<PackItem>());

            Debug.Log(newCustomerItems[i].itemID); // for debug purposes
        }

        newCustomer.AssignItems(newCustomerItems);
        newCustomer.SetMoveTarget(customerWaitLocation);
    }

    bool CheckBoxDone(Box box, out Customer targetCustomer)
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
                targetCustomer = c;
                int unnecessaryItems = box.itemsInBox.Count - usedItems.Count;

                // calculate score

                return true;
            }
        }

        targetCustomer = null;
        return false;
    }
    #endregion

    #region Gamestate
    void StartGame()
    {
        state = GameState.Playing;
        timer = minutesToGameEnd * 60; // turn to seconds
    }

    void EndGame()
    {
        state = GameState.Menu;
    }
    #endregion
    #endregion
}
