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

    AudioSource audioSource;
    public Text recordText;
    public Image recordIcon;
    public Color recordIconColor;

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
        switchAudio.Play();
        player.transform.parent = myCamera.transform;
        currentRoom = office;

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
    }

    public void GoKitchen()
    {
        switchAudio.Play();
        player.transform.parent = myCamera.transform;
        currentRoom = kitchen;

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
        switchAudio.Play();
        player.transform.parent = myCamera.transform;
        currentRoom = shower;

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
    }

    public void GoBedroom()
    {
        switchAudio.Play();
        player.transform.parent = myCamera.transform;
        currentRoom = bedroom;

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
        closeFridgeButton.SetActive(true);
        myCameraController.moveBack = false;
        myCameraController.moveToFridge = true;
        storeManager.StoreIsOpen();
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
            playerAnimator.SetBool("Sleep", true);
            sleepButtonText.text = "Wake Up";
            sleepBG.SetActive(true);
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
}
