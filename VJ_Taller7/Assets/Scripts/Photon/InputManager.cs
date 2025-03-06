using Fusion;
using Fusion.Addons.KCC;
using Fusion.Menu;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputManager : SimulationBehaviour,IBeforeUpdate, INetworkRunnerCallbacks
{
    public PlayerControllerPhoton LocalPlayer;
    public Vector2 AccumulatedMouseDelta => mouseDeltaAccumulator.AccumulatedValue;
    private NetworkInputData accumulatedInput;
    private Vector2Accumulator mouseDeltaAccumulator = new() { SmoothingWindow = 0.025f };
    private bool resetInput;

    //[SerializeField] private NetworkPrefabRef _playerPrefab;
    //private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    public void BeforeUpdate()
    {
        if (resetInput)
        {
            resetInput = false;
            accumulatedInput = default;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        NetworkButtons buttons = default;

        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();
            Vector2 lookRotationDelta = new(-mouseDelta.y, mouseDelta.x);
            //accumulatedInput.LookDelta += lookRotationDelta;
            mouseDeltaAccumulator.Accumulate(lookRotationDelta);
            buttons.Set(InputButton.OnAim, mouse.rightButton.isPressed);
            buttons.Set(InputButton.Fire, mouse.leftButton.isPressed);
        }

        if (keyboard != null)
        {
            Vector2 moveDirection = Vector2.zero;
            if (keyboard.wKey.isPressed)
                moveDirection += Vector2.up;
            if (keyboard.sKey.isPressed)
                moveDirection += Vector2.down;
            if (keyboard.aKey.isPressed)
                moveDirection += Vector2.left;
            if (keyboard.dKey.isPressed)
                moveDirection += Vector2.right;
            accumulatedInput.Direction += moveDirection;
            buttons.Set(InputButton.Jump, keyboard.spaceKey.isPressed);
            buttons.Set(InputButton.Run, keyboard.leftShiftKey.isPressed);
            buttons.Set(InputButton.Dash, keyboard.fKey.isPressed);
            buttons.Set(InputButton.Crouch, keyboard.ctrlKey.isPressed);
            buttons.Set(InputButton.Emote, keyboard.tKey.isPressed);
            buttons.Set(InputButton.ChangeWeapon, keyboard.qKey.isPressed);
            buttons.Set(InputButton.Interact, keyboard.eKey.isPressed);


            // buttons.Set(InputButton.Jump, keyboard.spaceKey.isPressed);
            accumulatedInput.Buttons = new NetworkButtons(accumulatedInput.Buttons.Bits | buttons.Bits); //Combine the 2 networkButtons bits
        }

    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        /*if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }*/
    }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        accumulatedInput.Direction.Normalize();
        accumulatedInput.LookDelta = mouseDeltaAccumulator.ConsumeTickAligned(runner);
        input.Set(accumulatedInput);
        resetInput = true;

        //accumulatedInput.LookDelta = default;
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) 
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        /*if (shutdownReason == ShutdownReason.DisconnectedByPluginLogic)
        {
            await FindFirstObjectByType<IntroSampleMenuConnectionBehaviour>(FindObjectsInactive.Include).DisconnectAsync(ConnectFailReason.Disconnect);
            FindFirstObjectByType<FusionMenuUIGameplay>(FindObjectsInactive.Include).Controller.Show<FusionMenuUIMain>();
        }*/
    }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

   // private NetworkRunner _runner;

    async void StartGame(GameMode mode)
    {
        /*// Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // Create the NetworkSceneInfo from the current scene
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        // Start or join (depends on gamemode) a session with a specific name
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });*/
    }

   /* private void OnGUI()
    {
        if (_runner == null)
        {
            if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
            {
                StartGame(GameMode.Host);
            }
            if (GUI.Button(new Rect(0, 40, 200, 40), "Join"))
            {
                StartGame(GameMode.Client);
            }
        }
    }*/
    public void OnMove(InputAction.CallbackContext context)
    {

        //Debug.Log(moveDirection.x + ", " + moveDirection.y);
        //moveDirection = context.ReadValue<Vector2>();

    }

    /*public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        aimInput = context.ReadValue<float>();
    }*/
}