using System;
using TMPro;
using UnityEngine;

public enum CameraAngle
{
    Menu = 0,
    WhiteTeam = 1,
    BlackTeam = 2
}
public class GameUI : MonoBehaviour
{
    
    [SerializeField] private Animator MenuAnimator;
    [SerializeField] private TMP_InputField AddressInput;
    [SerializeField] private GameObject[] CameraAngles;
    [SerializeField] private AudioSource AudioSource;
    [SerializeField] private AudioClip MenuNoise;
    [SerializeField] private AudioClip BackGround;

    public Action<bool> SetLocalGame;

    public Server Server;
    public Client Client;
    public static GameUI instance { set; get; }

    private void Awake()
    {
        instance = this;

        RegisterEvents();
    }

    private void Start()
    {
        AudioSource = GetComponent<AudioSource>();
        AudioSource.Play();
        
    }

    //cameras
    public void ChangeCamera(CameraAngle index)
    {
        for(int i = 0; i < CameraAngles.Length; i++)
            CameraAngles[i].SetActive(false);

        CameraAngles[(int)index].SetActive(true);
    }

    public void OnLocalGameButton()
    {
        MenuAnimator.SetTrigger("InGameMenu");
        SetLocalGame?.Invoke(true);
        Server.Init(8007);
        Client.Init("127.0.0.1", 8007);
        AudioSource.Stop();
    }

    public void OnOnlineButton()
    {
        MenuAnimator.SetTrigger("OnlineMenu");
        AudioSource.PlayOneShot(MenuNoise);
    }

    public void OnOnlineHostButton()
    {
        SetLocalGame?.Invoke(false);
        Server.Init(8007);
        Client.Init("127.0.0.1", 8007);
        MenuAnimator.SetTrigger("HostMenu");
        AudioSource.PlayOneShot(MenuNoise);
        AudioSource.Stop();
    }

    public void OnOnlineConnectButton()
    {
        SetLocalGame?.Invoke(false);
        Client.Init(AddressInput.text, 8007);
        AudioSource.PlayOneShot(MenuNoise);
        AudioSource.Stop();
    }

    public void OnOnlineBackButton()
    {
        MenuAnimator.SetTrigger("StartMenu");
        AudioSource.PlayOneShot(MenuNoise);
        
    }

    public void OnHostBackButton()
    {
        Server.Shutdown();
        Client.Shutdown();
        MenuAnimator.SetTrigger("OnlineMenu");
        AudioSource.PlayOneShot(MenuNoise);
        AudioSource.Play();
    }

    public void OnLeaveFromGameMenu()
    {
        ChangeCamera(CameraAngle.Menu);
        MenuAnimator.SetTrigger("StartMenu");
        AudioSource.PlayOneShot(MenuNoise);
        Client.Shutdown();
        Server.Shutdown();
        AudioSource.Stop();

    }

    private void RegisterEvents()
    {
        NetUtility.C_Start_Game += OnStartGameClient;
    }

    private void UnRegisterEvents()
    {
        NetUtility.C_Start_Game -= OnStartGameClient;
    }

    private void OnStartGameClient(NetMessage obj)
    {
        MenuAnimator.SetTrigger("InGameMenu");
    }
}
