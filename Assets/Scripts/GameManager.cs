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
    #region Gamestate
    public enum GameState
    {
        Menu = default,
        Playing,
    }

    GameState state = GameState.Menu;
    #endregion
    #region Miscellaneous
    [Header("Miscellaneous"), SerializeField] Score scoreboard;
    float score;

    [SerializeField] bool clearItemsOnGameEnd = true;
    #endregion
    #region Main Game Settings
    [Header("Main Game Settings"), SerializeField] float minutesToGameEnd;
    float timer;

    [SerializeField] int maxStrikes = 3;
    int currentStrikes;
    #endregion
    #region Customer Difficulty
    // patience
    [Header("Customer Difficulty"), SerializeField] float customerPatienceSecondsHigh = 30;
    [SerializeField] float customerPatienceSecondsLow = 10;
    float customerPatienceSeconds = 30;

    // spawn time
    [Space, SerializeField] float customerSpawnTimeSecondsHigh = 10;
    [SerializeField] float customerSpawnTimeSecondsLow = 5;
    float customerSpawnTimeSeconds;
    float customerSpawnTimeBackupHelper; 
    float customerSpawnTimeBackupHelperTimer;

    // walk speed
    [Space, SerializeField] float customerWalkSpeedHigh = 7;
    [SerializeField] float customerWalkSpeedLow = 3.5f;
    float customerWalkSpeed;

    // item number requests
    [Space, SerializeField] int customerItemsRequestedMaxHigh = 5;
    [SerializeField] int customerItemsRequestedMaxLow = 3;
    [SerializeField] int customerItemsRequestedMinHigh = 3;
    [SerializeField] int customerItemsRequestedMinLow = 1;
    int customerItemsRequestedMax;
    int customerItemsRequestedMin;

    // duration & curve
    [Space, SerializeField] float minutesToMaxDifficultyRamp = 3;
    [SerializeField] AnimationCurve customerDifficultyCurve;
    #endregion
    #region Locations
    [Header("Locations"), SerializeField] Transform[] customerSpawnLocations;
    [SerializeField] public List<WaitingLocation> customerWaitLocations = new List<WaitingLocation>();
    [SerializeField] Transform[] customerLeaveLocations;
    [SerializeField] List<Transform> windowShopperLocations = new List<Transform>();

    Dictionary<int, bool> usedWaitLocations = new Dictionary<int, bool>();
    Dictionary<int, bool> usedWindowshopperLocations = new Dictionary<int, bool>();
    #endregion
    #region Customers
    [Header("Customers"), SerializeField] GameObject[] customerPresets;
    [SerializeField] GameObject[] requestableObjects;
    [SerializeField] GameObject customerItemRequestList;
    [SerializeField] GameObject customerBox;
    [SerializeField] int maxWindowShoppers = 3;
    [SerializeField] float windowShopperStayDuration = 15;
    [SerializeField] float windowShopperWaitDuration = 4;
    [SerializeField] float attemptSpawnWindowShopperTime = 10;

    [HideInInspector] public int windowShopperCount;
    float windowShopperSpawnTimer;
    #endregion
    #region Customer Accessories
    [Header("Customer Accessories"), SerializeField] GameObject[] customerHeadAccessories;
    [SerializeField] GameObject[] customerEarsAccessories;
    [SerializeField] GameObject[] customerNeckAccessories;
    #endregion
    #region UI
    [Header("UI"), SerializeField] GameObject[] mainMenuUI;
    [SerializeField] List<GameObject> playtimeUI = new List<GameObject>();
    [Space, SerializeField] GameObject strikesUI;
    [SerializeField] Color strikeFineColor = Color.white;
    [SerializeField] Color strikeFailColor = Color.red;

    Image[] strikesImages;
    #endregion
    #region Tracking Lists
    List<Customer> currentCustomers = new List<Customer>();
    [HideInInspector] public List<Box> preparedBoxes = new List<Box>();
    List<GameObject> clearList = new List<GameObject>();
    #endregion
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
        if (instance == null) instance = this;
        else Destroy(this);

        #region Score
        if (FileSystem.LoadFile("scores.txt", out Score loadedScores)) {
            scoreboard = loadedScores;
        }

        for (int i = 0; i < customerWaitLocations.Count; ++i) {
            usedWaitLocations.Add(i, false);
        }

        for (int i = 0; i < windowShopperLocations.Count; ++i) {
            usedWindowshopperLocations.Add(i, false);
        }
        #endregion

        #region UI
        if (strikesUI) {
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
        }

        SetMenuUI(state);
        #endregion

        #region Window Shoppers
        windowShopperSpawnTimer = attemptSpawnWindowShopperTime;
        if (maxWindowShoppers > windowShopperLocations.Count) maxWindowShoppers = windowShopperLocations.Count;
        #endregion
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
                float rampEval = EvaluateDifficultyRamp();
                customerPatienceSeconds = Mathf.Lerp(customerPatienceSecondsHigh, customerPatienceSecondsLow, rampEval);
                customerSpawnTimeSeconds = Mathf.Lerp(customerSpawnTimeSecondsHigh, customerSpawnTimeSecondsLow, rampEval);
                customerWalkSpeed = Mathf.Lerp(customerWalkSpeedLow, customerWalkSpeedHigh, rampEval);
                customerItemsRequestedMax = Mathf.RoundToInt(Mathf.Lerp(customerItemsRequestedMaxLow, customerItemsRequestedMaxHigh, rampEval));
                customerItemsRequestedMin = Mathf.RoundToInt(Mathf.Lerp(customerItemsRequestedMinLow, customerItemsRequestedMinHigh, rampEval));

                CustomerSpawningUpdate();

                for (int i = 0; i < preparedBoxes.Count; ++i) {
                    if (CheckBoxDone(preparedBoxes[i], out Customer happyCustomer, out float scoreChange)) {
                        Box b = preparedBoxes[i];
                        preparedBoxes.Remove(b);
                        Destroy(b.gameObject);

                        --i; // our list is smaller, so we have to step back to ensure we check all elements

                        RemoveCustomer(happyCustomer);
                        score += scoreChange;
                    }
                }
                break;
        }

        windowShopperSpawnTimer -= Time.deltaTime;
        if (windowShopperSpawnTimer <= 0) {
            windowShopperSpawnTimer = attemptSpawnWindowShopperTime;
            CreateNewWindowShopper();
        }
    }
    #endregion

    #region Gameplay & Flow
    #region Gameplay Updates
    /// <summary>
    /// returns the value of the difficulty ramp based on the current time
    /// </summary>
    /// <returns></returns>
    float EvaluateDifficultyRamp()
    {
        float difficultyRampMaxTime = minutesToMaxDifficultyRamp * 60;
        float gameEndTime = minutesToGameEnd * 60;
        return customerDifficultyCurve.Evaluate((Mathf.Clamp(timer, difficultyRampMaxTime, gameEndTime) - difficultyRampMaxTime) / (gameEndTime - difficultyRampMaxTime));
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
    #region Customer Creation
    /// <summary>
    /// Spawns a new Customer if it wouldn't raise the number past the cap
    /// </summary>
    void CreateNewCustomer()
    {
        if (!AnyAvailableLocations()) return; // a customer cannot be spawned if there is nowhere for them to wait

        Vector3 chosenSpawnLocation = customerSpawnLocations[Random.Range(0, customerSpawnLocations.Length)].position;

        // create a random customer
        Customer newCustomer = Instantiate(customerPresets[Random.Range(0, customerPresets.Length)], chosenSpawnLocation, Quaternion.identity).GetComponent<Customer>();
        currentCustomers.Add(newCustomer);

        List<PackItem> newCustomerItems = new List<PackItem>();
        int itemCount = Random.Range(customerItemsRequestedMin, customerItemsRequestedMax);

        for (int i = 0; i < itemCount; ++i) {
            // add random items to our list
            newCustomerItems.Add(requestableObjects[Random.Range(0, requestableObjects.Length)].GetComponent<PackItem>());
        }

        AssignCounterLocation(newCustomer);

        newCustomer.AssignItems(newCustomerItems);
        newCustomer.SetSpawnItems(customerItemRequestList, customerBox, customerWaitLocations[newCustomer.assignedWaitIndex]);
        newCustomer.SetPatience(customerPatienceSeconds);
        newCustomer.SetMoveSpeed(customerWalkSpeed);
        newCustomer.AttachAccessories(GenerateAccessories());
    }

    /// <summary>
    /// Spawns a new window shopper if it wouldn't raise the number over the cap
    /// </summary>
    void CreateNewWindowShopper()
    {
        if (windowShopperCount >= maxWindowShoppers) return; // a shopper cannot be spawned if there are too many
        ++windowShopperCount;

        // choose a random spawn location
        Vector3 chosenSpawnLocation = customerSpawnLocations[Random.Range(0, customerSpawnLocations.Length)].position;

        // create a random customer, add accessories, then disable everything unnecessary
        Customer newCustomer = Instantiate(customerPresets[Random.Range(0, customerPresets.Length)], chosenSpawnLocation, Quaternion.identity).GetComponent<Customer>();
        newCustomer.AttachAccessories(GenerateAccessories());

        newCustomer.enabled = false;

        // assign all required variables
        WindowShopper shopper = newCustomer.AddComponent<WindowShopper>();
        shopper.AssignTimes(windowShopperStayDuration, windowShopperWaitDuration);
        shopper.AssignLocations(windowShopperLocations, customerLeaveLocations[Random.Range(0, customerLeaveLocations.Length)]);
    }

    /// <summary>
    /// Begins the process of removing a customer. The rest of the process is continued in the Customer class
    /// </summary>
    /// <param name="customer"></param>
    /// <param name="failed"></param>
    public void RemoveCustomer(Customer customer, bool failed = false)
    {
        AssignLeavingLocation(customer);

        customer.EndRequest(failed);

        if (failed) {
            AddFail();
        }
    }

    /// <summary>
    /// Gets an array of random accessories for a character.
    /// </summary>
    /// <returns></returns>
    GameObject[] GenerateAccessories()
    {
        return new GameObject[] {
            ArrayHelper<GameObject>.GetRandomElement(customerHeadAccessories),
            ArrayHelper<GameObject>.GetRandomElement(customerEarsAccessories),
            ArrayHelper<GameObject>.GetRandomElement(customerNeckAccessories),
        };
    }
    #endregion

    #region Assign Locations
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

    public bool AssignWindowShopperLocation(WindowShopper shopper)
    {
        bool successful = false;

        List<int> attemptedNumbers = new List<int>();

        while (!successful) {
            int attemptingNumber = Random.Range(0, windowShopperLocations.Count);

            if (attemptedNumbers.Count >= windowShopperLocations.Count) return false; // no available positions
            if (attemptedNumbers.Contains(attemptingNumber)) attemptingNumber = (attemptingNumber + 1) % windowShopperLocations.Count;

            if (AssignWindowShopperLocation(shopper, attemptingNumber)) {
                successful = true;
            } else {
                attemptedNumbers.Add(attemptingNumber);
            }
        }

        return true;
    }

    public bool AssignWindowShopperLocation(WindowShopper shopper, int index)
    {
        if (!usedWindowshopperLocations[index]) {
            if (shopper.assignedIndex >= 0) { // customer had a different location assigned to them
                usedWindowshopperLocations[shopper.assignedIndex] = false;
            }

            usedWindowshopperLocations[index] = true;
            shopper.assignedIndex = index;
            shopper.GoToLocation(windowShopperLocations[index].position);
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

        customer.leavingLocation = customerLeaveLocations[index];
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
    #endregion
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
        if (state != GameState.Playing || strikesUI == null) return;

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

        if (strikesUI != null) {
            foreach (Image i in strikesImages) {
                i.color = strikeFineColor;
            }
        }

        SetMenuUI(state);

        if (AudioManager.instance != null) {
            AudioManager.instance.audioSource.PlayOneShot(ArrayHelper<AudioClip>.GetRandomElement(AudioManager.instance.gameStartSounds));
        }
    }

    public void EndGame()
    {
        state = GameState.Menu;
        scoreboard.AddScore(score);
        score = 0;

        for (int i = currentCustomers.Count - 1; i >= 0; --i) {
            RemoveCustomer(currentCustomers[i], true);
        }

        while (preparedBoxes.Count > 0) {
            Box temp = preparedBoxes[0];
            Destroy(preparedBoxes[0].gameObject);
            if (preparedBoxes[0] == temp) preparedBoxes.Remove(temp);
        }

        SetMenuUI(state);

        if (AudioManager.instance != null) {
            AudioManager.instance.audioSource.PlayOneShot(ArrayHelper<AudioClip>.GetRandomElement(AudioManager.instance.gameEndSounds));
        }

        if (clearItemsOnGameEnd) ClearItems();
    }

    [ContextMenu("Clear Items")]
    public void ClearItems()
    {
        for (int i = 0; i < clearList.Count; ++i) {
            if(clearList[i] != null) Destroy(clearList[i]);
        }

        clearList.Clear();

        if (AudioManager.instance != null) {
            AudioManager.instance.audioSource.PlayOneShot(ArrayHelper<AudioClip>.GetRandomElement(AudioManager.instance.clearItemsSounds));
        }
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
