using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    #region Variables
    public static GameManager instance; // singleton for references to a single game manager

    public enum GameState
    {
        Menu = default,
        Playing,
    }

    GameState state = GameState.Menu;

    [Header("Required Miscellaneous"), SerializeField] Score scoreboard;
    float score;


    [Header("Main Game Settings"), SerializeField] float minutesToGameEnd;
    float timer;

    [SerializeField] int maxStrikes = 3;
    int currentStrikes;

    [Header("Customer Difficulty"), SerializeField] float customerPatienceSeconds = 30;
    [SerializeField] float customerSpawnTimeSecondsHigh = 10;
    [SerializeField] float customerSpawnTimeSecondsLow = 5;
    float customerSpawnTimeSeconds;

    [SerializeField] float minutesToMaxDifficultyRamp = 3;
    float customerSpawnTimeBackupHelper;
    float customerSpawnTimeBackupHelperTimer;
    [SerializeField] AnimationCurve customerSpawnTimeCurve;

    [Header("Locations"), SerializeField] Transform[] customerSpawnLocations;
    [SerializeField] public List<WaitingLocation> customerWaitLocations = new List<WaitingLocation>();
    [SerializeField] Transform[] customerLeaveLocations;
    Dictionary<int, bool> usedWaitLocations = new Dictionary<int, bool>();

    [Header("Customers"), SerializeField] GameObject[] customerPresets;
    [SerializeField] GameObject[] requestableObjects;
    [SerializeField] int itemsPerPerson = 3;
    [SerializeField] GameObject customerItemRequestList;
    [SerializeField] GameObject customerBox;

    [Header("Customer Accessories"), SerializeField] GameObject[] customerHeadAccessories;
    [SerializeField] GameObject[] customerEarsAccessories;
    [SerializeField] GameObject[] customerNeckAccessories;

    List<Customer> currentCustomers = new List<Customer>();
    [HideInInspector] public List<Box> preparedBoxes = new List<Box>();

    [Header("UI"), SerializeField] GameObject[] mainMenuUI;
    [SerializeField] List<GameObject> playtimeUI = new List<GameObject>();
    [Space, SerializeField] GameObject strikesUI;
    [SerializeField] Color strikeFineColor = Color.white;
    [SerializeField] Color strikeFailColor = Color.red;

    Image[] strikesImages;

    List<GameObject> clearList = new List<GameObject>();
    #endregion

    #region Access Properties
    public GameState currentGameState { get { return state; } }
    public float remainingTime { get { return timer; } }
    public float currentScore { get { return score; } }
    public Score currentScoreboard { get { return scoreboard; } }
    public List<Customer> customers { get { return currentCustomers; } }
    #endregion

    #region Functions
    #region Unity
    void Start()
    {
        // instance setup
        if (instance == null) instance = this;
        else Destroy(this);

        // score setup
        if (FileSystem.LoadFile("scores.txt", out Score loadedScores)) {
            scoreboard = loadedScores;
        }

        for (int i = 0; i < customerWaitLocations.Count; ++i) {
            usedWaitLocations.Add(i, false);
        }

        // ui setup
        if (!playtimeUI.Contains(strikesUI)) {
            playtimeUI.Add(strikesUI);
        }

        strikesImages = new Image[maxStrikes];
        Image[] foundStrikesImages = strikesUI.GetComponentsInChildren<Image>();

        for (int i = 0; i < strikesImages.Length; ++i) {
            if (i >= foundStrikesImages.Length) {
                strikesImages[i] = Instantiate(strikesImages[0], strikesImages[0].transform.parent);
            } else {
                strikesImages[i] = foundStrikesImages[i];
            }
        }

        SetMenuUI(state);
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

                // difficulty ramp
                customerSpawnTimeSeconds = EvaluateDifficultyRamp();

                CustomerSpawningUpdate();

                for (int i = 0; i < preparedBoxes.Count; ++i) {
                    if (CheckBoxDone(preparedBoxes[i], out Customer happyCustomer, out float scoreChange)) {
                        Destroy(preparedBoxes[i].gameObject); // onDestroy on boxes removes them from preparedBoxes
                        --i; // our list is smaller, so we have to step back to ensure we check all elements

                        RemoveCustomer(happyCustomer);
                        score += scoreChange;
                    }
                }
                break;
        }
    }
    #endregion

    #region Gameplay & Flow
    #region Gameplay Updates
    float EvaluateDifficultyRamp()
    {
        float difficultyRampMaxTime = minutesToMaxDifficultyRamp * 60;
        float gameEndTime = minutesToGameEnd * 60;
        float difficulty =
            customerSpawnTimeCurve.Evaluate((Mathf.Clamp(timer, difficultyRampMaxTime, gameEndTime) - difficultyRampMaxTime) / (gameEndTime - difficultyRampMaxTime));

        return Mathf.Lerp(customerSpawnTimeSecondsHigh, customerSpawnTimeSecondsLow, difficulty);
    }

    void CustomerSpawningUpdate()
    {
        if (currentCustomers.Count == 0) { // ensures a single customer at all times
            CreateNewCustomer();
        }

        customerSpawnTimeBackupHelperTimer -= Time.deltaTime;
        if (customerSpawnTimeBackupHelperTimer <= 0 && (int)(timer % customerSpawnTimeSeconds) == 0) {
            customerSpawnTimeBackupHelperTimer = customerSpawnTimeBackupHelper;
            CreateNewCustomer();
        }
    }
    #endregion

    #region Customers
    void CreateNewCustomer()
    {
        if (!AnyAvailableLocations()) return; // a customer cannot be spawned if there is nowhere for them to wait

        Vector3 chosenSpawnLocation = customerSpawnLocations[Random.Range(0, customerSpawnLocations.Length)].position;

        // create a random customer
        Customer newCustomer = Instantiate(customerPresets[Random.Range(0, customerPresets.Length)], chosenSpawnLocation, Quaternion.identity).GetComponent<Customer>();
        currentCustomers.Add(newCustomer);

        List<PackItem> newCustomerItems = new List<PackItem>();

        for (int i = 0; i < itemsPerPerson; ++i) {
            // add random items to our list
            newCustomerItems.Add(requestableObjects[Random.Range(0, requestableObjects.Length)].GetComponent<PackItem>());
        }

        AssignCounterLocation(newCustomer);

        newCustomer.AssignItems(newCustomerItems);
        newCustomer.SetSpawnItems(customerItemRequestList, customerBox, customerWaitLocations[newCustomer.assignedWaitIndex]);
        newCustomer.SetPatience(customerPatienceSeconds);
        newCustomer.AttachAccessories(GenerateAccessories());
    }

    public void RemoveCustomer(Customer customer, bool failed = false)
    {
        currentCustomers.Remove(customer);
        AssignLeavingLocation(customer);

        if (failed) AddFail();
    }

    bool AssignCounterLocation(Customer customer)
    {
        bool successful = false;

        List<int> attemptedNumbers = new List<int>();

        while (!successful) {
            int attemptingNumber = Random.Range(0, customerWaitLocations.Count);

            if (attemptedNumbers.Count >= customerWaitLocations.Count) return false; // no available positions
            if (attemptedNumbers.Contains(attemptingNumber)) attemptingNumber = (attemptingNumber + 1) % customerWaitLocations.Count;

            if (AssignCounterLocation(customer, attemptingNumber)) {
                successful = true;
            } else {
                attemptedNumbers.Add(attemptingNumber);
            }
        }

        return true;
    }

    bool AssignCounterLocation(Customer customer, int index)
    {
        if (!usedWaitLocations[index]) {
            if (customer.assignedWaitIndex >= 0) { // customer had a different location assigned to them
                usedWaitLocations[customer.assignedWaitIndex] = false;
            }

            usedWaitLocations[index] = true;
            customer.assignedWaitIndex = index;
            customer.SetMoveTarget(customerWaitLocations[index].customerWaitLocation);
            return true;
        }

        return false;
    }

    void AssignLeavingLocation(Customer customer)
    {
        AssignLeavingLocation(customer, Random.Range(0, customerLeaveLocations.Length));
    }

    void AssignLeavingLocation(Customer customer, int index)
    {
        if (!customer.leaving && customer.assignedWaitIndex >= 0) { // customer had a location assigned at the counter
            usedWaitLocations[customer.assignedWaitIndex] = false;
        }

        customer.leaving = true;
        customer.SetMoveTarget(customerLeaveLocations[index]);
    }

    /// <summary>
    /// Returns true if there are any available locations for a customer to wait at
    /// </summary>
    /// <returns></returns>
    bool AnyAvailableLocations()
    {
        for (int i = 0; i < customerWaitLocations.Count; ++i) {
            if (usedWaitLocations.ContainsKey(i) && !usedWaitLocations[i]) return true;
        }

        return false;
    }

    GameObject[] GenerateAccessories()
    {
        return new GameObject[] {
            ArrayHelper<GameObject>.GetRandomElement(customerHeadAccessories),
            ArrayHelper<GameObject>.GetRandomElement(customerEarsAccessories),
            ArrayHelper<GameObject>.GetRandomElement(customerNeckAccessories),
        };
    }

    
    #endregion

    #region Boxes
    /// <summary>
    /// Checks all customers currently waiting and outputs the first customer with a matching request.
    /// </summary>
    /// <param name="box"></param>
    /// <param name="customer"></param>
    /// <returns></returns>
    bool CheckBoxDone(Box box, out Customer customer, out float score)
    {
        box.ValidateItems();

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

    public void AddPreparedBox(Box box)
    {
        if (!preparedBoxes.Contains(box)) preparedBoxes.Add(box);
    }

    public void RemovePreparedBox(Box box)
    {
        preparedBoxes.Remove(box);
    }
    #endregion

    #region Utility
    void AddFail()
    {
        if (state != GameState.Playing) return;

        strikesImages[currentStrikes].color = strikeFailColor;

        ++currentStrikes;
        CheckGameFail();
    }

    public void AddObjectToClearList(GameObject go)
    {
        clearList.Add(go);
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
    #endregion

    #region Gamestate
    public static bool canStartGame { get { return instance != null && instance.state == GameState.Menu; } }

    public void StartGame()
    {
        if (!canStartGame) return;

        state = GameState.Playing;
        timer = minutesToGameEnd * 60; // turn to seconds
        score = 0.0f;
        currentStrikes = 0;

        customerSpawnTimeBackupHelper = customerSpawnTimeSecondsHigh / 2;
        customerSpawnTimeBackupHelperTimer = customerSpawnTimeBackupHelper;

        foreach(Image i in strikesImages) {
            i.color = strikeFineColor;
        }

        SetMenuUI(state);
    }

    public void EndGame()
    {
        state = GameState.Menu;
        scoreboard.AddScore(score);
        score = 0;

        for (int i = currentCustomers.Count - 1; i >= 0; --i) {
            RemoveCustomer(currentCustomers[i]);
        }

        while (preparedBoxes.Count > 0) {
            Box temp = preparedBoxes[0];
            Destroy(preparedBoxes[0].gameObject);
            if (preparedBoxes[0] == temp) preparedBoxes.Remove(temp);
        }

        SetMenuUI(state);

        ClearItems();
    }

    [ContextMenu("Clear Items")]
    public void ClearItems()
    {
        for (int i = 0; i < clearList.Count; ++i) {
            if(clearList[i] != null) Destroy(clearList[i]);
        }

        clearList.Clear();
    }

    void SetMenuUI(GameState gs)
    {
        bool mainMenuActive = true;
        switch (gs) {
            case GameState.Menu:
                mainMenuActive = true;
                break;
            case GameState.Playing:
                mainMenuActive = false;
                break;
        }

        foreach (GameObject go in mainMenuUI) {
            go.SetActive(mainMenuActive);
        }

        foreach (GameObject go in playtimeUI) {
            go.SetActive(!mainMenuActive);
        }
    }

    /// <summary>
    /// Returns true if a fail condition has been met
    /// </summary>
    /// <returns></returns>
    bool CheckGameFail()
    {
        if (currentStrikes >= maxStrikes) {
            EndGame();
            return true;
        }

        return false;
    }

    void OnApplicationQuit()
    {
        if (scoreboard != null) FileSystem.SaveFile("scores.txt", scoreboard);
    }
    #endregion

    #region Helpers
    public void SetObjectLayerGrabbed(SelectEnterEventArgs args)
    {
        args.interactableObject.transform.gameObject.layer = LayerMask.NameToLayer("Grabbed");
    }

    public void SetObjectLayerDefault(SelectExitEventArgs args)
    {
        args.interactableObject.transform.gameObject.layer = LayerMask.NameToLayer("Default");
    }
    #endregion

    #region Cheats
    [ContextMenu("Start Game")]
    void CheatStartGame()
    {
        StartGame();
    }

    [ContextMenu("Satisfy First Customer")]
    void CheatCompleteCustomer()
    {
        if (instance != null && currentCustomers.Count > 0) {
            RemoveCustomer(currentCustomers[0]);
        }
    }

    [ContextMenu("Spawn New Customer")]
    void CheatSpawnCustomer()
    {
        if (instance != null && AnyAvailableLocations()) {
            CreateNewCustomer();
        }
    }
    #endregion
    #endregion
}
