using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    public GameManager gameManager;
    public Transform storeUI;
    public List<int> productsPrices = new List<int>();
    public List<String> productsName = new List<String>();
    public List<Sprite> productsImage = new List<Sprite>();
    public AudioSource purchaseAudio;
    public float fillAmount;

    public Image selectedProductImage;
    public Text selectedProductText;

    public GameObject storeText,availableProductsUI,myProductUI,getCoinsUI;
    

    List<Product> listOfProducts = new List<Product>();
    public List<Product> myProducts = new List<Product>();
    public List<GameObject> hideOnOpen = new List<GameObject>();
    public List<GameObject> hideOnClose = new List<GameObject>();
    int selectedProductIndex;
    public Image kitchenProgressBar;
    float fill;
    
    void Start()
    {
        LoadData();
    }


    void Update()
    {
        //kitchenProgressBar.fillAmount = .5f;
        if (kitchenProgressBar.fillAmount < fill)
        {
            kitchenProgressBar.fillAmount+=fillAmount;
        }
    }

    void LoadData()
    {
        int index = 0;
        foreach (var price in productsPrices)
        {
            Product product = new Product();
            product.index = index;
            product.price = price;
            product.name = productsName[index];
            product.image = productsImage[index];
            
            //print(" price "+price+" index "+index+" name "+ product.name+" image "+product.image );
            listOfProducts.Add(product);
            index++;
        }

    }

    int currentIndex;
    public void Buy(int productIndex)
    {
        int price = productsPrices[productIndex];
        if (gameManager.currentCoins >= price)
        {
            gameManager.AddCoins(-price);
            purchaseAudio.Play();

             currentIndex=0;
             bool found=false;
            foreach (var product in myProducts)
            {
                if (product.index == productIndex)
                {
                    found = true;
                    myProducts[currentIndex].quantity++;
                    print(" quantity "+ myProducts[currentIndex].quantity +" index "+productIndex);
                }
                currentIndex++;
              
            }

            if (!found)
            {
                Product newProduct = new Product();
                newProduct.index = productIndex;
                newProduct.quantity = 1;
                newProduct.name = productsName[productIndex];
                newProduct.image = productsImage[productIndex];
                
                myProducts.Add(newProduct);

                print(" quantity "+ newProduct.quantity +" index "+productIndex);
            }
            
            print(myProducts.Count);
        }
        else
        {
            getCoinsUI.SetActive(true);
        }

        CheckPurchasedProducts();
    }

   public void CheckPurchasedProducts()
    {
        if (myProducts.Count > 0)
        {

            storeText.SetActive(false);
            myProductUI.SetActive(true);
            selectedProductImage.sprite = myProducts[selectedProductIndex].image;
            selectedProductText.text = myProducts[selectedProductIndex].name+" x"+myProducts[selectedProductIndex].quantity;
        }
        else
        {
            selectedProductIndex = 0;
            myProductUI.SetActive(false);
            storeText.SetActive(true);
        }
    }

   public void Eat()
   {
       fill += .2f;
       /*
       kitchenProgressBar.fillAmount=kitchenProgressBar.fillAmount+fill;
       */
       int qnt = myProducts[selectedProductIndex].quantity;
       if (qnt > 1)
       {
           myProducts[selectedProductIndex].quantity=qnt-1;
       }
       else
       {
           myProducts.RemoveAt(selectedProductIndex);
           if (selectedProductIndex >= 1)
           {
               selectedProductIndex--;
           }
       }

       CheckPurchasedProducts();
   }



    public void NextProduct()
    {
        selectedProductIndex ++;
        if (selectedProductIndex > myProducts.Count-1)
        {
            selectedProductIndex = 0;
        }
        selectedProductImage.sprite = myProducts[selectedProductIndex].image;
        selectedProductText.text= myProducts[selectedProductIndex].name+" x"+myProducts[selectedProductIndex].quantity;
        print(myProducts[selectedProductIndex].name);
    }
    
    public void PreviousProduct()
    {
        selectedProductIndex --;
        if (selectedProductIndex < 0)
        {
            selectedProductIndex = myProducts.Count-1 ;
        }
        selectedProductImage.sprite = myProducts[selectedProductIndex].image;
        selectedProductText.text= myProducts[selectedProductIndex].name+" x"+myProducts[selectedProductIndex].quantity;
        print(myProducts[selectedProductIndex].name); 
    }

    public void StoreIsOpen()
    {
        foreach (GameObject obj in hideOnOpen)
        {
              obj.SetActive(false);
        }
        
        foreach (GameObject obj in hideOnClose)
        {
            obj.SetActive(true);
        }
        availableProductsUI.SetActive(false);
        CheckPurchasedProducts();
    }
    
    public void StoreIsClosed()
    {
        foreach (GameObject obj in hideOnClose)
        {
            obj.SetActive(false);
        }
        
        foreach (GameObject obj in hideOnOpen)
        {
            obj.SetActive(true);
        }
        storeUI.gameObject.SetActive(false);
        availableProductsUI.SetActive(true);
        CloseRewardUI();
    }

    public void CloseRewardUI()
    {
        getCoinsUI.SetActive(false);
    }

}

public class  Product{
    public int index;
    public int quantity;
    public int price;
    public String name;
    public Sprite image;
}

