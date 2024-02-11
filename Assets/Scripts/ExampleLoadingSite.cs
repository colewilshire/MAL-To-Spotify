using UnityEngine;
using VoltstroStudios.UnityWebBrowser.Core;

public class ExampleLoadingSite : MonoBehaviour
{
    //SerializeField exposes the control in Unity
    [SerializeField] private BaseUwbClientManager clientManager;
        
    private WebBrowserClient webBrowserClient;

    private void Start()
    {
        //Makes life easier having a local reference to WebBrowserClient
        //webBrowserClient = clientManager.browserClient;
    }

    //Call this from were ever, and it will load 'https://voltstro.dev'
    public void LoadMySite()
    {
        //webBrowserClient.LoadUrl("https://voltstro.dev");
    }
}