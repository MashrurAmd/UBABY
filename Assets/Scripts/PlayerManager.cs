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
    public GameObject miniGameButton;
    public Text sleepButtonText;
    public float _waitTime, _elapsed;
    public int maxRecordTime;
    public Image sleepProgressBar;
    public float fillAmount;

    [Header("Body Hit")]
    public Collider playerCollider;
    public Collider legCollider;
    public Collider headCollider;

    private string wardrobe = "wardrobe";
    public Transform wardrobeCamera;
    public GameObject wardrobeButton;
    public GameObject wardrobeUI;

    public AudioSource recordingAudioSource;
    public Text recordText;
    public Image recordIcon;
    public Color recordIconColor;
    public GameObject sleepParticleEffect;

    [Header("Sleep Sounds")]
    public AudioSource sleepSound1;
    public AudioSource sleepSound2;
    
    [Header("Eating Settings")]
    public float mouthRadius = 200f; // ✅ adjust this in Inspector
    public float grabRadiusMultiplier = 2.5f;
    

    // ✅ track actual recorded samples
    private int recordedSamples = 0;

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

        // ✅ Apply multiplier
        productMaxX *= grabRadiusMultiplier;
        productMinX *= grabRadiusMultiplier;
        productMaxY *= grabRadiusMultiplier;
        productMinY *= grabRadiusMultiplier;

        Debug.Log($"Grab zone X: {productMinX} to {productMaxX}");
        Debug.Log($"Grab zone Y: {productMinY} to {productMaxY}");

        UpdateRoomCamera();
    }

    void Update()
    {
        // ✅ Body hit detection
        if (Input.GetMouseButtonDown(0))
        {
            if (currentRoom != kitchen)
                CheckBodyHit(Input.mousePosition);
        }

#if !UNITY_EDITOR
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            CheckBodyHit(Input.GetTouch(0).position);
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
                // ✅ Use distance instead of box check
                Vector2 foodPos = floatingFoodImage.transform.position;
                Vector2 mouthPos = Camera.main.WorldToScreenPoint(playerMouth.position);
                float distToMouth = Vector2.Distance(foodPos, mouthPos);

                Debug.Log($"Distance to mouth: {distToMouth}, radius: {mouthRadius}");

                if (distToMouth < mouthRadius) // ✅ circle check instead of box
                {
                    playerAnimator.SetTrigger("Eat");
                    floatingFoodImage.GetComponent<Image>().sprite = null;
                    floatingFoodImage.transform.position = floatingFoodStartPosition;
                    eatingAudio.Play();
                    storeManager.Eat();
                    Debug.Log("✅ Food eaten!");
                }
                else
                {
                    floatingFoodImage.transform.position = floatingFoodStartPosition;
                    Debug.Log("❌ Missed mouth, distance: " + distToMouth);
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

                Debug.Log($"Finger distance from food: {distance}"); // ✅ debug

                if (!isHoldingFood &&
                    distance.x < productMaxX && distance.x > productMinX &&
                    distance.y < productMaxY && distance.y > productMinY)
                {
                    isHoldingFood = true;
                    playerAnimator.SetBool("OpenMouth", true);
                    Debug.Log("✅ Food grabbed!"); // ✅ debug
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

        officeButton.SetActive(true);
        kitchenButton.SetActive(true);
        showerButton.SetActive(true);
        bedRommButtom.SetActive(true);
        wardrobeButton.SetActive(true);
        wardrobeUI.SetActive(false);
        playerAnimator.SetBool("LyingOnBed", false); 

        switchAudio.Play();
        player.transform.parent = myCamera.transform;
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
        playerAnimator.SetBool("LyingOnBed", false); 

        switchAudio.Play();
        player.transform.parent = myCamera.transform;
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
        playerAnimator.SetBool("LyingOnBed", false); 

        switchAudio.Play();
        player.transform.parent = myCamera.transform;
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
        playerAnimator.SetBool("isinShower", false);

        myCamera.position = bedroomCamera.position;
        myCamera.rotation = bedroomCamera.rotation;

        openFridgeButton.SetActive(false);
        closeFridgeButton.SetActive(false);
        storeManager.availableProductsUI.SetActive(false);
        showerBottomUI.SetActive(false);
        sleepButton.SetActive(true);

        // ✅ Enter lying animation when entering bedroom
        playerAnimator.SetBool("LyingOnBed", true);
        playerAnimator.SetBool("Sleep", false);

        officeButton.GetComponent<Image>().enabled = false;
        kitchenButton.GetComponent<Image>().enabled = false;
        showerButton.GetComponent<Image>().enabled = false;
        bedRommButtom.GetComponent<Image>().enabled = true;
    }
    public void GoWardrobe()
    {
        currentRoom = wardrobe;
        UpdateRoomCamera();

        switchAudio.Play();
        player.transform.parent = myCamera.transform;

        myCamera.position = wardrobeCamera.position;
        myCamera.rotation = wardrobeCamera.rotation;

        openFridgeButton.SetActive(false);
        closeFridgeButton.SetActive(false);
        storeManager.availableProductsUI.SetActive(false);
        showerBottomUI.SetActive(false);
        sleepButton.SetActive(false);
        wardrobeUI.SetActive(true);
        playerAnimator.SetBool("LyingOnBed", false); 
        WakeUp();

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

        // MiniGame button only shows up while the Office Camera is the active camera
        miniGameButton.SetActive(officeCamera.gameObject.activeSelf);
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
        myCameraController.moveBack = false;
        myCameraController.moveToFridge = true;
        StartCoroutine(OpenStoreWithDelay(0.6f));

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
            playerAnimator.SetBool("LyingOnBed", false); // ✅ exit lying
            playerAnimator.SetBool("Sleep", true);        // ✅ enter sleep
            sleepButtonText.text = "Wake Up";
            sleepBG.SetActive(true);
            sleepParticleEffect.SetActive(true);
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
        playerAnimator.SetBool("Sleep", false);           // ✅ exit sleep

        // ✅ Go back to lying if still in bedroom
        if (currentRoom == bedroom)
            playerAnimator.SetBool("LyingOnBed", true);  // ✅ back to lying
        else
            playerAnimator.SetBool("LyingOnBed", false);

        sleepButtonText.text = "Sleep";
        sleepBG.SetActive(false);
        sleepParticleEffect.SetActive(false);
        sleepSound1.Stop();
        sleepSound2.Stop();
    }

    // ===========================
    // 👊 BODY HIT
    // ===========================

    void CheckBodyHit(Vector2 screenPos)
    {
        if (isSleeping || isRecording) return;

        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider == playerCollider)
                playerAnimator.SetTrigger("BodyHit");
            else if (hit.collider == legCollider)
                playerAnimator.SetTrigger("LegHit");
            else if (hit.collider == headCollider)
                playerAnimator.SetTrigger("HeadHit");
        }
    }

    // ===========================
    // 🎤 RECORD / TALK
    // ===========================

    public void Record()
    {
        if (!isRecording)
        {
            isRecording = true;
            _elapsed = 0;

            Debug.Log("🎤 Available microphones:");
            foreach (string device in Microphone.devices)
                Debug.Log("   - " + device);

            recordingAudioSource.clip = Microphone.Start("", false, maxRecordTime, 44100);
            recordText.text = "No";
            recordIcon.color = Color.red;
            playerAnimator.SetBool("Listening", true);
            Debug.Log("🎤 Recording STARTED");
        }
        else
        {
            PlayRecordingAudio();
        }
    }

    void PlayRecordingAudio()
    {
        isRecording = false;

        // ✅ capture actual recorded position BEFORE stopping
        recordedSamples = Microphone.GetPosition("");
        Debug.Log("🎤 Recorded samples: " + recordedSamples);

        Microphone.End("");
        _elapsed = 0;

        recordIcon.color = recordIconColor;
        recordText.text = "Yes";
        playerAnimator.SetBool("Listening", false);
        playerAnimator.SetBool("Talking", true);

        StartCoroutine(PlayAfterMicReady());
    }

    IEnumerator PlayAfterMicReady()
    {
        // wait until mic fully stops
        while (Microphone.IsRecording(""))
            yield return null;

        yield return null;

        if (recordingAudioSource == null || recordingAudioSource.clip == null)
        {
            Debug.LogError("❌ AudioSource or clip is NULL!");
            yield break;
        }

        if (recordedSamples <= 0)
        {
            Debug.LogError("❌ No samples recorded!");
            yield break;
        }

        // ✅ Trim to actual recorded length
        AudioClip trimmedClip = TrimClip(recordingAudioSource.clip, recordedSamples);
        recordingAudioSource.clip = trimmedClip;

        Debug.Log("🔊 Trimmed clip length: " + trimmedClip.length + "s");

        recordingAudioSource.outputAudioMixerGroup = null;
        recordingAudioSource.spatialBlend = 0f;
        recordingAudioSource.volume = 1f;
        recordingAudioSource.Play();

        Debug.Log("▶️ Playing audio!");

        // ✅ stop talking animation after clip finishes
        StartCoroutine(StopTalkingAnimation(trimmedClip.length * 0.75f));
    }

    AudioClip TrimClip(AudioClip clip, int samples)
    {
        if (samples <= 0) return clip;

        float[] data = new float[samples * clip.channels];
        clip.GetData(data, 0);

        AudioClip trimmed = AudioClip.Create(
            "RecordedAudio",
            samples,
            clip.channels,
            clip.frequency,
            false
        );
        trimmed.SetData(data, 0);
        return trimmed;
    }

    IEnumerator StopTalkingAnimation(float wait)
    {
        yield return new WaitForSeconds(wait);
        playerAnimator.SetBool("Talking", false);
    }

    // ===========================
    // 🔄 SWITCH ROOMS
    // ===========================

    public void SwitchRight()
    {
        switch (currentRoom)
        {
            case "office":   GoKitchen();  break;
            case "kitchen":  GoShower();   break;
            case "shower":   GoBedroom();  break;
            case "bedroom":  GoWardrobe(); break;
            case "wardrobe": GoOffice();   break;
        }
    }

    public void SwitchLeft()
    {
        switch (currentRoom)
        {
            case "office":   GoWardrobe(); break;
            case "kitchen":  GoOffice();   break;
            case "shower":   GoKitchen();  break;
            case "bedroom":  GoShower();   break;
            case "wardrobe": GoBedroom();  break;
        }
    }
}