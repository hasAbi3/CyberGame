using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class AuthManager : MonoBehaviour
{
    public GameObject signInPanel;
    public GameObject signInButton;
    public TMP_Text signInButtonText;

    public TMP_InputField usernameInput;  
    public TMP_InputField passwordInput;  
    public Button loginButton;       

    public UIHandler uiHandler;

    private string loginURL = "http://localhost:3000/login"; 
    private string registerURL = "http://localhost:3000/register";  
    private string saveProgressURL = "http://localhost:3000/save-progress";
    private string loadProgressURL = "http://localhost:3000/load-progress";
 
    // Start is called before the first frame update
    void Start()
    {
        string token = PlayerPrefs.GetString("jwtToken", "");

       if (!string.IsNullOrEmpty(token))
        {
       
            signInButtonText.text = "user: " + PlayerPrefs.GetString("username", "");
            LoadProgress();  
            signInPanel.SetActive(false); 
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    

    // This function is called when the login button is clicked
    public void OnLoginClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            // Make the HTTP POST request
            StartCoroutine(Login(username, password));
        }
        else
        {
            Debug.Log("Please enter both username and password.");
        }
    }

    public void OnRegisterClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            // Make the HTTP POST request
            StartCoroutine(Register(username, password));
        }
        else
        {
            Debug.Log("Please enter both username and password.");
        }
    }

    // Coroutine for making the login request
    private IEnumerator Login(string username, string password)
    {
        // Create a new form for sending login data
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);

        Debug.Log("Form: " + form);

        // Send POST request to the server
        using (UnityWebRequest www = UnityWebRequest.Post(loginURL, form))
        {
            // Wait for the server response
            yield return www.SendWebRequest();

            // Check if request is successful
            if (www.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = www.downloadHandler.text;
                Debug.Log("Login Successful: " + jsonResponse);

                string token = JsonUtility.FromJson<TokenResponse>(jsonResponse).token;

                //Store token for future use
                PlayerPrefs.SetString("jwtToken", token);

                PlayerPrefs.SetString("username", username);

                signInButtonText.text = "user: " + username;
                LoadProgress();

            }
            else
            {
                Debug.Log("Login Failed: " + www.error);
            }
        }
       
    }

    // Coroutine for making the login request
    private IEnumerator Register(string username, string password)
    {
        // Create a new form for sending login data
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);

        // Send POST request to the server
        using (UnityWebRequest www = UnityWebRequest.Post(registerURL, form))
        {
            // Wait for the server response
            yield return www.SendWebRequest();

            // Check if request is successful
            if (www.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = www.downloadHandler.text;
                Debug.Log("Registration Successful: " + jsonResponse);

                string token = JsonUtility.FromJson<TokenResponse>(jsonResponse).token;

                //Store token for future use
                PlayerPrefs.SetString("jwtToken", token);
                PlayerPrefs.Save();
                PlayerPrefs.SetString("username", username);

                signInButtonText.text = "user: " + username;

                uiHandler.ToggleLoginRegisterPanel();
            }
            else
            {
                Debug.Log("registration Failed: " + www.error);
            }
        }
    }

    public void SaveProgress(int health, Vector2 position){
        StartCoroutine(SaveProgressCoroutine(health, position));
    }

    private IEnumerator SaveProgressCoroutine(int health, Vector2 position)
    {
        string token = PlayerPrefs.GetString("jwtToken");   // from login function
        WWWForm form = new WWWForm();
        form.AddField("health", health);
        form.AddField("position", JsonUtility.ToJson(new Position { x = position.x, y = position.y }));

        using (UnityWebRequest www = UnityWebRequest.Post(saveProgressURL, form)){
            www.SetRequestHeader("Authorization", "Bearer " + token);
            yield return www.SendWebRequest();

             if (www.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = www.downloadHandler.text;
                Debug.Log("Progress saved successfully: " + jsonResponse);
            }
            else
            {
                Debug.Log("Error saving progress: " + www.error);
            }
         }
     }

     public void LoadProgress()
    {
        StartCoroutine(LoadProgressCoroutine());
    }

    private IEnumerator LoadProgressCoroutine()
    {
        string token = PlayerPrefs.GetString("jwtToken");

         if (string.IsNullOrEmpty(token))
    {
        Debug.LogError("JWT token is missing or empty.");
        yield break;
    }
    
        // Create a new UnityWebRequest to get the progress
        UnityWebRequest request = UnityWebRequest.Get(loadProgressURL);
        request.SetRequestHeader("Authorization", "Bearer " + token);

        Debug.Log(token);

        yield return request.SendWebRequest();
        if(request.result == UnityWebRequest.Result.Success )
        {
            string jsonResponse = request.downloadHandler.text;
            ProgressData progress = JsonUtility.FromJson<ProgressData>(jsonResponse);
            Debug.Log("Loaded Progress: Health = " + progress.health + ", Position = (" + progress.position.x + ", " + progress.position.y + ")");

            RubyController ruby = FindFirstObjectByType<RubyController>(); 
            if (ruby != null)
            {
                ruby.transform.position = new Vector3(progress.position.x, progress.position.y, 0f);

                int deltaHealth = progress.health - ruby.health;
                ruby.ChangeHealth(deltaHealth);
            }else{
                Debug.LogError("RubyController not found in the scene!");
            }

            signInPanel.SetActive(false);
        }
        else{
             Debug.Log("Error loading progress: " + request.error);
             Debug.LogError("HTTP Status Code: " + request.responseCode);
        }
    }
}

[System.Serializable]
public class TokenResponse
{
    public string token;
}

[System.Serializable]
public class ProgressData
{
    public int health;
    public Position position;
}

[System.Serializable]
public class Position
{
    public float x;
    public float y;
}


