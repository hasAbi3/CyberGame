using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Globalization;
using TMPro;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    public static UIHandler instance { get; private set; }

    public float displayTime = 4.0f;
    
    private VisualElement m_Healthbar;
    private VisualElement m_DialogWindowJambi;
    private VisualElement m_DialogWindowRuby;
    private Label m_DialogLabel;
    private TextField m_DialogText;
    private float m_TimerDisplay;
    private float m_TimerDisplayRuby;

    public RubyController controller;
    private GameObject m_Thinking;

    private int m_DialogStep = 0;

    private PasswordCracker passCracker;

    [SerializeField] private GameObject signInPanel;
    public TMP_Text signInButtonText;
    private bool isSignInPanelOpen = false;
    bool isRegistering = false; 
    public TMP_InputField usernameInput;  
    public TMP_InputField passwordInput;  
    public UnityEngine.UI.Button switchButton;  
    public UnityEngine.UI.Button loginButton;  

    public AuthManager authManager;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        signInPanel.SetActive(false);

        UIDocument uiDocument = GetComponent<UIDocument>();
        m_Healthbar = uiDocument.rootVisualElement.Q<VisualElement>("Healthbar");

        m_DialogWindowJambi = uiDocument.rootVisualElement.Q<VisualElement>("DialogWindowJambi");
        m_DialogWindowRuby = uiDocument.rootVisualElement.Q<VisualElement>("DialogWindowRuby");

        m_DialogLabel = uiDocument.rootVisualElement.Q<Label>("JambiLabel");
        m_DialogText = uiDocument.rootVisualElement.Q<TextField>("RubyText");
        m_DialogWindowJambi.style.display = DisplayStyle.None;
        m_DialogWindowRuby.style.display = DisplayStyle.None;
        m_TimerDisplay = -1.0f;

        SetHealthValue(1.0f);

        m_Thinking = GameObject.Find("Thinking");
        m_Thinking.SetActive(false);
    }

    System.Threading.Thread thread;

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            m_TimerDisplay = displayTime;
        }

        if (m_TimerDisplay > 0)
        {
            m_TimerDisplay -= Time.deltaTime;
            if (m_TimerDisplay < 0)
            {
                m_DialogWindowJambi.style.display = DisplayStyle.None;
                m_DialogWindowRuby.style.display = DisplayStyle.None;
            }
        }

        //Entered Password
        if (Input.GetKeyDown(KeyCode.Return)) {
            string password = m_DialogText.text;

            thread = new System.Threading.Thread(()=> PasswordCracking(password));
            thread.Start();
            if (m_Thinking.activeInHierarchy == false) {
                m_Thinking.SetActive(true);
            }
        }

        if (thread != null && !thread.IsAlive)
        {
            //PasswordCracking(password);

            m_DialogLabel.text = "Jambi: Your password has been cracked! Here is your password that I \"guessed\": " + passCracker.password + ", length: " + passCracker.passwordLength + ". I tried " + passCracker.tries.ToString("#,#", CultureInfo.InvariantCulture) + " times, and used " + passCracker.elapsedTime + " seconds.";

            if (controller != null)
            {
                if (passCracker.elapsedTime < 2)
                {
                    controller.ChangeHealth(-1);
                }
                else
                {
                    controller.ChangeHealth(1);
                }
            }
            thread = null;
            m_Thinking.SetActive(false);
        }
    }

    private void PasswordCracking(string password) {
        passCracker = new PasswordCracker();
        passCracker.password = password;
        passCracker.password = passCracker.password.ToLower();
        passCracker.passwordLength = passCracker.password.Length;

        Debug.Log("\nCracking your password...");
        System.Diagnostics.Stopwatch timer = System.Diagnostics.Stopwatch.StartNew();
        // Brute force the password
        
        passCracker.CreatePasswords(string.Empty);
        timer.Stop();
        long elapsedMs = timer.ElapsedMilliseconds;
        double elapsedTime = elapsedMs / 1000;

        passCracker.elapsedTime = elapsedTime;
    }


    public void DisplayDialog()
    {
        m_DialogWindowJambi.style.display = DisplayStyle.Flex;
        if (m_DialogStep >= 2)
        {
            m_DialogWindowRuby.style.display = DisplayStyle.Flex;
        }
        m_TimerDisplay = displayTime;
        m_DialogLabel.text = GetNextDialog(m_DialogStep++);
    }

    private int GetDialogStep() 
    {
        return m_DialogStep;
    }

    public void SetHealthValue(float percentage)
    {
        m_Healthbar.style.width = Length.Percent(100 * percentage);
    }

    private string GetNextDialog(int step) 
    {
        if (step == 0)
        {
            return "Jambi: Hi, I am Jambi. I am an account manager. Do you want to setup your account?";
        }
        else if (step == 1)
        {
            return "Ruby: Yes. I don't have an account yet. Can you please help me setting one up?";
        }
        else {
            return "Jambi: Of course! Please create a password for your new account!";
        }
    }

       public void signInButtonClick()
     {
        isSignInPanelOpen = !isSignInPanelOpen;

        signInPanel.SetActive(isSignInPanelOpen);

        if (isSignInPanelOpen)
        {
            signInButtonText.text = "Back";
        }
        else
        {
            signInButtonText.text = "Sign In";
        }
     }

    

    public void ToggleLoginRegisterPanel()
    {
        isRegistering = !isRegistering;
        Debug.Log(isRegistering);

        if (isRegistering)
        {
            ((TMP_Text)usernameInput.placeholder).text = "Choose a username";
            ((TMP_Text)passwordInput.placeholder).text = "Choose a password";
    
            loginButton.GetComponentInChildren<TMP_Text>().text = "Register";
            switchButton.GetComponentInChildren<TMP_Text>().text = "Already have an account? Login";
        }
        else
        {    
            ((TMP_Text)usernameInput.placeholder).text = "Enter your username";
            ((TMP_Text)passwordInput.placeholder).text = "Enter your password";

            loginButton.GetComponentInChildren<TMP_Text>().text = "Login";
            switchButton.GetComponentInChildren<TMP_Text>().text = "Don't have an account? Register";
        }
    }

    public void OnLoginRegisterButtonClick()
    {
        if (isRegistering)
        {
            authManager.OnRegisterClicked();
        }
        else
        {
            authManager.OnLoginClicked();
        }
    }
}
