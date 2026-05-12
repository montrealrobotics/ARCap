using System;
using UnityEngine;
using SceneManagement = UnityEngine.SceneManagement.SceneManager;
using TMPro;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;

public class StartScene : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI init_text;
    [SerializeField] private Button connectButton;
    [SerializeField] private Button gripperButton;
    [SerializeField] private Button robotiqButton;
    [SerializeField] private Button xgripperButton;
    [SerializeField] private Button leapButton;
    [SerializeField] private Button bimanualButton;
    [SerializeField] private Toggle armSelectButton;

    private bool isLeapHand = false;
    private bool isRobotiq = false;
    private bool isXarmGripper = false;
    private bool isGripper = false;
    private bool isBimanual = false;
    private bool isIpValid = false;
    private bool isFranka = false;
    private bool isXarm = true;
    public Color selectedColor = Color.yellow;
    public Color defaultColor = Color.white;

    public static string pc_ip;
    public static string local_ip;
    private TouchScreenKeyboard overlayKeyboard;
    private const string IP_SAVE_KEY = "SavedPCIP";

    void Start()
    {
        armSelectButton.onValueChanged.AddListener(OnToggleValueChanged);
        OnToggleValueChanged(false);
        init_text = GameObject.Find("StartText").GetComponent<TextMeshProUGUI>();
        GetLocalIPAddress();
        CoordinateFrame.isBimanual = false;

        // Initialize UI State
        if (connectButton != null) connectButton.interactable = false;
        if (isXarm)
        {
            gripperButton.interactable = false;
        }
        else
        {
            xgripperButton.interactable = false;
        }
        

        // Load last IP and open keyboard
        string lastIp = PlayerPrefs.GetString(IP_SAVE_KEY, "");
        overlayKeyboard = TouchScreenKeyboard.Open(lastIp, TouchScreenKeyboardType.ASCIICapable);
        init_text.text = string.IsNullOrEmpty(lastIp) ? "Enter PC IP..." : $"Saved IP: {lastIp}\nEdit or confirm on keyboard to continue.";
    }

    public void SelectGripper()
    {
        SetSelection(gripper: true);
        init_text.text = "Mode: <color=green>Gripper</color> selected.";
        CheckReadyToConnect();
    }

    public void SelectLeap()
    {
        SetSelection(leap: true);
        init_text.text = "Mode: <color=green>Leap Hand</color> selected.";
        CheckReadyToConnect();
    }

    public void SelectRobotiq()
    {
        SetSelection(robotiq: true);
        init_text.text = "Mode: <color=green>Robotiq gripper</color> selected.";
        CheckReadyToConnect();
    }

    public void SelectXGripper()
    {
        SetSelection(xarmgripper: true);
        init_text.text = "Mode: <color=green>Xarm gripper</color> selected.";
        CheckReadyToConnect();
    }

    public void SelectBimanual()
    {
        SetSelection(bimanual: true);
        init_text.text = "Mode: <color=green>Bimanual</color> selected.";
        CheckReadyToConnect();
    }

    public void OnToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            SetArmSelection(franka: true);
        }
        else
        {
            SetArmSelection(xarm: true);
        }
        CheckReadyToConnect();
    }

    public void OnConnectButtonPressed()
    {
        if (!isIpValid) return;
        if (isFranka)
        {
            if (isLeapHand || isBimanual) SceneManagement.LoadScene("HandSelect");
            else if (isGripper) SceneManagement.LoadScene("GripperSelect");
            else if (isRobotiq) SceneManagement.LoadScene("RobotiqSelect");
        }
        else
        {
            if (isLeapHand || isBimanual) SceneManagement.LoadScene("HandSelect_xarm");
            else if (isXarmGripper) SceneManagement.LoadScene("X_GripperSelect_xarm");
            else if (isRobotiq) SceneManagement.LoadScene("RobotiqSelect_xarm");
        }
    }

    private void CheckReadyToConnect()
    {
        bool modeSelected = isLeapHand || isGripper || isBimanual;
        bool armSelected = isFranka || isXarm;
        if (connectButton != null)
        {
            connectButton.interactable = isIpValid && modeSelected && armSelected;
        }
    }

    private bool IsValidIP(string ipString)
    {
        if (string.IsNullOrWhiteSpace(ipString)) return false;
        return IPAddress.TryParse(ipString, out IPAddress address) 
               && address.AddressFamily == AddressFamily.InterNetwork;
    }

    void Update()
    {
        if (overlayKeyboard != null && overlayKeyboard.status == TouchScreenKeyboard.Status.Done)
        {
            if (IsValidIP(overlayKeyboard.text))
            {
                pc_ip = overlayKeyboard.text;
                isIpValid = true;
                PlayerPrefs.SetString(IP_SAVE_KEY, pc_ip);
                PlayerPrefs.Save();
                init_text.text = "<color=yellow>Select a Mode below.</color>";
                CheckReadyToConnect();
            }
            else
            {
                isIpValid = false;
                init_text.text = "<color=red>Invalid IP!</color> Click screen to re-open keyboard.";
                if (connectButton != null) connectButton.interactable = false;
            }
        }
    }
    private void SetArmSelection(bool xarm = false, bool franka = false)
    {
        isXarm = xarm;
        isFranka = franka;
    }

    private void SetSelection(bool leap = false, bool gripper = false, bool bimanual = false, bool xarmgripper = false, bool robotiq = false)
    {
        isLeapHand = leap;
        isGripper = gripper;
        isRobotiq = robotiq;
        isXarmGripper = xarmgripper;
        isBimanual = bimanual;
        CoordinateFrame.isBimanual = bimanual;
    }

    public void GetLocalIPAddress()
    {
        try {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) local_ip = ip.ToString();
            }
        } catch { local_ip = "127.0.0.1"; }
    }
}
