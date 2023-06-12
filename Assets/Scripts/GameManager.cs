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

    [SerializeField] Score scoreboard;
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
    public float currentScore { get { return score; } }
    #endregion

    #region Functions
    #region Unity
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        StartGame(); // just a debug measure to actually get gameplay happening
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
                    if (CheckBoxDone(b, out Customer happyCustomer, out float scoreChange)) {
                        Destroy(b.gameObject);
                        RemoveCustomer(happyCustomer);
                        score += scoreChange;
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

    public void RemoveCustomer(Customer customer)
    {
        currentCustomers.Remove(customer);
        Destroy(customer.gameObject); // for prototype; this should have the customer leave eventually
    }

    /// <summary>
    /// Checks all customers currently waiting and outputs the first customer with a matching request.
    /// </summary>
    /// <param name="box"></param>
    /// <param name="customer"></param>
    /// <returns></returns>
    bool CheckBoxDone(Box box, out Customer customer, out float score)
    {
        foreach (Customer c in currentCustomers) {
            if (CheckBoxDone(box, c, out score)) {
                customer = c;
                return true;
            }
        }

        customer = null;
        score = 0.0f;
        return false;
    }

    /// <summary>
    /// Checks the given customer for if they have all their requested items or not. Outputs a score based on how many items are correct.
    /// </summary>
    /// <param name="box"></param>
    /// <param name="customer"></param>
    /// <param name="score"></param>
    /// <returns></returns>
    bool CheckBoxDone(Box box, Customer customer, out float score)
    {
        List<int> usedItems = new List<int>();

        for (int i = 0; i < customer.items.Count; ++i) {
            bool itemFound = false;

            for (int j = 0; j < box.itemsInBox.Count; ++j) {
                if (usedItems.Contains(j)) continue;

                if (customer.items[i].itemID == box.itemsInBox[j].itemID) {
                    itemFound = true;
                    usedItems.Add(j); // don't use the same object twice for checking if we have everything

                    break;
                }
            }

            if (!itemFound) break; // we are missing one of the items meant to be in the box
        }

        score = CalculateScore(usedItems.Count, box.itemsInBox.Count - usedItems.Count);

        if (usedItems.Count >= customer.items.Count) { // we have all the items the customer wants
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Returns a score based on how many items were correct and how many were unnecessary.
    /// </summary>
    /// <param name="customer"></param>
    /// <param name="unnecessaryItems"></param>
    /// <returns></returns>
    float CalculateScore(int correctItems, int unnecessaryItems)
    {
        float output = correctItems * 50 - unnecessaryItems * 60;
        return output;
    }
    #endregion

    #region Gamestate
    void StartGame()
    {
        state = GameState.Playing;
        timer = minutesToGameEnd * 60; // turn to seconds
        score = 0.0f;
    }

    void EndGame()
    {
        state = GameState.Menu;
        scoreboard.AddScore(score);
        score = 0;
    }
    #endregion
    #endregion
}
