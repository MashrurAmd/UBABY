using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public ShowerManager showerManager;
    public string currentRoom;
    private string office = "office", kitchen = "kitchen", shower = "shower", bedroom = "bedroom";

    public Transform myCamera, officeCamera, kitchenCamera, showerCamera, bedroomCamera, player, playerMouth, buttonMaxRight;
    public Animator fridgeRightDoorAnimator, fridgeLeftDoorAnimator;
    public GameObject openFridgeButton, closeFridgeButton, showerBottomUI;
    public CameraController myCameraController;
    public StoreManager storeManager;
    public Vector3 fingerPos, distance, mouthDistance, floatingFoodStartPosition;
    public bool isHoldingFood, isRecording;
    public GameObject floatingFoodImage, productMiddle;
    public float productMaxY, productMinY, productMaxX, productMinX;
    public AudioSource eatingAudio, switchAudio;
    public Animator playerAnimator;
    public GameObject officeButton, kitchenButton, showerButton, bedRommButtom, sleepBG, sleepButton;
    public Text sleepButtonText;
    public float _waitTime, _elapsed;
    public int maxRecordTime;
    public Image sleepProgressBar;
    public float fillAmount;

    
    [Header("Body Hit")]
    public Collider playerCollider; // drag character's collider here
    
    private string wardrobe = "wardrobe";

    public Transform wardrobeCamera;
    public GameObject wardrobeButton;
    public GameObject wardrobeUI; // your clothes changing UI

    AudioSource audioSource;
    public Text recordText;
    public Image recordIcon;
    public Color recordIconColor;
    public GameObject sleepParticleEffect;
    
    
    [Header("Sleep Sounds")]
    public AudioSource sleepSound1;  // drag first sleep sound here
    public AudioSource sleepSound2;  // drag second sleep sound here

    void Start()
    {
        currentRoom = office;
        floatingFoodStartPosition = floatingFoodImage.transform.position;
        recordIconColor = recordIcon.color;
        productMaxX = buttonMaxRight.position.x - productMiddle.transform.position.x;
        productMinX = -productMaxX;
        productMaxY = productMaxX;
        productMinY = -productMaxX;
        _waitTime = maxRecordTime;
    }

    void Update()
    {
        
        // ✅ Body hit detection - add at the very top
        if (Input.GetMouseButtonDown(0))
        {
            CheckBodyHit(Input.mousePosition);
        }

#if !UNITY_EDITOR
    if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
    {
        CheckBodyHit(Input.GetTouch(0).position);
    }
#endif
        
        if (isRecording)
        {
            _elapsed += Time.deltaTime;
            if (_elapsed >= _waitTime)
                PlayRecordingAudio();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isHoldingFood)
            {
                mouthDistance = floatingFoodImage.transform.position - Camera.main.WorldToScreenPoint(playerMouth.position);
                if (mouthDistance.x < productMaxX && mouthDistance.x > productMinX &&
                    mouthDistance.y < productMaxY && mouthDistance.y > productMinY)
                {
                    playerAnimator.SetTrigger("Eat");
                    floatingFoodImage.GetComponent<Image>().sprite = null;
                    floatingFoodImage.transform.position = floatingFoodStartPosition;
                    eatingAudio.Play();
                    storeManager.Eat();
                }
                else
                {
                    floatingFoodImage.transform.position = floatingFoodStartPosition;
                }
                playerAnimator.SetBool("OpenMouth", false);
            }
            isHoldingFood = false;
        }

        if (Input.GetMouseButton(0))
        {
            fingerPos = Input.mousePosition;

            if (currentRoom == kitchen && storeManager.myProducts.Count > 0)
            {
                distance = fingerPos - productMiddle.transform.position;
                if (!isHoldingFood &&
                    distance.x < productMaxX && distance.x > productMinX &&
                    distance.y < productMaxY && distance.y > productMinY)
                {
                    isHoldingFood = true;
                    playerAnimator.SetBool("OpenMouth", true);
                }
            }

            if (isHoldingFood)
                floatingFoodImage.transform.position = fingerPos;
        }

        if (isSleeping)
        {
            if (sleepProgressBar.fillAmount <= 1)
                sleepProgressBar.fillAmount += fillAmount;
        }
    }

    // ===========================
    // 🎯 ROOM BUTTON FUNCTIONS
    // ===========================

    public void GoOffice()
    {
        currentRoom = office;
        UpdateRoomCamera();

        // ✅ Restore bottom nav buttons
        officeButton.SetActive(true);
        kitchenButton.SetActive(true);
        showerButton.SetActive(true);
        bedRommButtom.SetActive(true);
        wardrobeButton.SetActive(true);

        wardrobeUI.SetActive(false); // ✅ hide wardrobe UI

        switchAudio.Play();
        player.transform.parent = myCamera.transform;
        currentRoom = office;
        playerAnimator.SetBool("isinShower", false);

        

        myCamera.position = officeCamera.position;
        myCamera.rotation = officeCamera.rotation;

        openFridgeButton.SetActive(false);
        closeFridgeButton.SetActive(false);
        storeManager.availableProductsUI.SetActive(false);
        showerBottomUI.SetActive(false);
        sleepButton.SetActive(false);
        WakeUp();

        officeButton.GetComponent<Image>().enabled = true;
        kitchenButton.GetComponent<Image>().enabled = false;
        showerButton.GetComponent<Image>().enabled = false;
        bedRommButtom.GetComponent<Image>().enabled = false;

        wardrobeUI.SetActive(false); // add this line in GoOffice, GoKitchen, GoShower, GoBedroom
    }
    
    // ===========================
// 👊 BODY HIT
// ===========================

    void CheckBodyHit(Vector2 screenPos)
    {
        // ✅ Don't trigger if sleeping or recording
        if (isSleeping || isRecording) return;

        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider == playerCollider)
            {
                playerAnimator.SetTrigger("BodyHit");
            }
        }
    }

    public void GoKitchen()
    {
        currentRoom = kitchen;
        UpdateRoomCamera();

        officeButton.SetActive(true);
        kitchenButton.SetActive(true);
        showerButton.SetActive(true);
        bedRommButtom.SetActive(true);
        wardrobeButton.SetActive(true);

        wardrobeUI.SetActive(false);


        switchAudio.Play();
        player.transform.parent = myCamera.transform;
        currentRoom = kitchen;

        playerAnimator.SetBool("isinShower", false);

        myCamera.position = kitchenCamera.position;
        myCamera.rotation = kitchenCamera.rotation;

        openFridgeButton.SetActive(true);
        closeFridgeButton.SetActive(false);
        storeManager.availableProductsUI.SetActive(true);
        storeManager.CheckPurchasedProducts();
        showerBottomUI.SetActive(false);
        sleepButton.SetActive(false);
        WakeUp();

        officeButton.GetComponent<Image>().enabled = false;
        kitchenButton.GetComponent<Image>().enabled = true;
        showerButton.GetComponent<Image>().enabled = false;
        bedRommButtom.GetComponent<Image>().enabled = false;
    }

    public void GoShower()
    {

        currentRoom = shower;
        UpdateRoomCamera();


        officeButton.SetActive(true);
        kitchenButton.SetActive(true);
        showerButton.SetActive(true);
        bedRommButtom.SetActive(true);
        wardrobeButton.SetActive(true);

        wardrobeUI.SetActive(false);

        switchAudio.Play();
        player.transform.parent = myCamera.transform;
        currentRoom = shower;

        // Add animation for shower transition
        playerAnimator.SetBool("isinShower", true);

        myCamera.position = showerCamera.position;
        myCamera.rotation = showerCamera.rotation;

        openFridgeButton.SetActive(false);
        closeFridgeButton.SetActive(false);
        storeManager.availableProductsUI.SetActive(false);
        showerBottomUI.SetActive(true);
        sleepButton.SetActive(false);
        showerManager.ClearAllFoam();
        WakeUp();

        officeButton.GetComponent<Image>().enabled = false;
        kitchenButton.GetComponent<Image>().enabled = false;
        showerButton.GetComponent<Image>().enabled = true;
        bedRommButtom.GetComponent<Image>().enabled = false;

        wardrobeUI.SetActive(false); // add this line in GoOffice, GoKitchen, GoShower, GoBedroom

        if (currentRoom != shower)
            playerAnimator.SetBool("isinShower", false);
        



    }

    public void GoBedroom()
    {

        currentRoom = bedroom;
        UpdateRoomCamera();

        officeButton.SetActive(true);
        kitchenButton.SetActive(true);
        showerButton.SetActive(true);
        bedRommButtom.SetActive(true);
        wardrobeButton.SetActive(true);

        wardrobeUI.SetActive(false);

        switchAudio.Play();
        player.transform.parent = myCamera.transform;
        currentRoom = bedroom;
        playerAnimator.SetBool("isinShower", false);

        myCamera.position = bedroomCamera.position;
        myCamera.rotation = bedroomCamera.rotation;

        openFridgeButton.SetActive(false);
        closeFridgeButton.SetActive(false);
        storeManager.availableProductsUI.SetActive(false);
        showerBottomUI.SetActive(false);
        sleepButton.SetActive(true);

        officeButton.GetComponent<Image>().enabled = false;
        kitchenButton.GetComponent<Image>().enabled = false;
        showerButton.GetComponent<Image>().enabled = false;
        bedRommButtom.GetComponent<Image>().enabled = true;
        wardrobeUI.SetActive(false); // add this line in GoOffice, GoKitchen, GoShower, GoBedroom
    }

    // ===========================
    // 🍔 FRIDGE
    // ===========================

    public void OpenFridge()
    {
        player.transform.parent = transform;
        fridgeRightDoorAnimator.SetBool("Open", true);
        fridgeLeftDoorAnimator.SetBool("Open", true);
        openFridgeButton.SetActive(false);
        //closeFridgeButton.SetActive(true);
        myCameraController.moveBack = false;
        myCameraController.moveToFridge = true;
        StartCoroutine(OpenStoreWithDelay(0.6f));
        //storeManager.StoreIsOpen();

        IEnumerator OpenStoreWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            storeManager.StoreIsOpen();
            closeFridgeButton.SetActive(true);
        }
    }



    public void CloseFridge()
    {
        fridgeRightDoorAnimator.SetBool("Open", false);
        fridgeLeftDoorAnimator.SetBool("Open", false);
        openFridgeButton.SetActive(true);
        closeFridgeButton.SetActive(false);
        myCameraController.moveToFridge = false;
        myCameraController.moveBack = true;
        storeManager.StoreIsClosed();
    }

    // ===========================
    // 😴 SLEEP
    // ===========================

    public bool isSleeping;
    public void Sleep()
    {
        if (!isSleeping)
        {
            isSleeping = true;
            playerAnimator.ResetTrigger("Hungry");
            playerAnimator.SetBool("Sleep", true);
            sleepButtonText.text = "Wake Up";
            sleepBG.SetActive(true);
            sleepParticleEffect.SetActive(true);

            // ✅ play sleep sounds
            sleepSound1.Play();
            sleepSound2.Play();
        }
        else
        {
            WakeUp();
        }
    }

    void WakeUp()
    {
        isSleeping = false;
        playerAnimator.SetBool("Sleep", false);
        sleepButtonText.text = "Sleep";
        sleepBG.SetActive(false);
        sleepParticleEffect.SetActive(false);

        // ✅ stop sleep sounds when waking up
        sleepSound1.Stop();
        sleepSound2.Stop();
    }

    // ===========================
    // 🎤 RECORD / TALK
    // ===========================

    public void Record()
    {
        audioSource = GetComponent<AudioSource>();
        if (!isRecording)
        {
            isRecording = true;
            audioSource.clip = Microphone.Start("", false, maxRecordTime, 44100);
            recordText.text = "No";
            recordIcon.color = Color.red;
            playerAnimator.SetBool("Listening", true);
        }
        else
        {
            PlayRecordingAudio();
        }
    }

    void PlayRecordingAudio()
    {
        isRecording = false;
        StartCoroutine(StopTalkingAnimation(_elapsed * 0.75f));
        _elapsed = 0;
        Microphone.End("");
        audioSource.Play();
        recordIcon.color = recordIconColor;
        recordText.text = "Yes";
        playerAnimator.SetBool("Listening", false);
        playerAnimator.SetBool("Talking", true);
    }

    IEnumerator StopTalkingAnimation(float wait)
    {
        yield return new WaitForSeconds(wait);
        playerAnimator.SetBool("Talking", false);
    }

  
    
    //switch left and right

    public void SwitchRight()
    {
        switch (currentRoom)
        {
            case "office":   GoKitchen();   break;
            case "kitchen":  GoShower();    break;
            case "shower":   GoBedroom();   break;
            case "bedroom":  GoWardrobe();  break; // ✅
            case "wardrobe": GoOffice();    break; // ✅
        }
    }

    public void SwitchLeft()
    {
        switch (currentRoom)
        {
            case "office":   GoWardrobe();  break; // ✅
            case "kitchen":  GoOffice();    break;
            case "shower":   GoKitchen();   break;
            case "bedroom":  GoShower();    break;
            case "wardrobe": GoBedroom();   break; // ✅
        }
    }

    public void GoWardrobe()
        {

            currentRoom = wardrobe;
            UpdateRoomCamera();


            switchAudio.Play();
            player.transform.parent = myCamera.transform;
            currentRoom = wardrobe;

            myCamera.position = wardrobeCamera.position;
            myCamera.rotation = wardrobeCamera.rotation;

            openFridgeButton.SetActive(false);
            closeFridgeButton.SetActive(false);
            storeManager.availableProductsUI.SetActive(false);
            showerBottomUI.SetActive(false);
            sleepButton.SetActive(false);
            wardrobeUI.SetActive(true);
            WakeUp();

            // ✅ Hide bottom nav buttons
            officeButton.SetActive(false);
            kitchenButton.SetActive(false);
            showerButton.SetActive(false);
            bedRommButtom.SetActive(false);
            wardrobeButton.SetActive(false);
        }

    void UpdateRoomCamera()
    {
        officeCamera.gameObject.SetActive(currentRoom == office);
        kitchenCamera.gameObject.SetActive(currentRoom == kitchen);
        showerCamera.gameObject.SetActive(currentRoom == shower);
        bedroomCamera.gameObject.SetActive(currentRoom == bedroom);
        wardrobeCamera.gameObject.SetActive(currentRoom == wardrobe);

        // optional: disable main camera
        //myCamera.gameObject.SetActive(false);
    }

}
