using BepInEx.Unity.IL2CPP.Utils;
using BetterAmongUs.Helpers;
using BetterAmongUs.Modules.Support;
using BetterAmongUs.Network.Loaders;
using Il2CppInterop.Runtime.Attributes;
using System.Collections;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Managers;

/// <summary>
/// Manages update functionality for BetterAmongUs, including download and installation.
/// </summary>
internal sealed class UpdateManager : MonoBehaviour
{
    private bool AmUpdateing;

    /// <summary>
    /// Gets the singleton instance of the UpdateManager.
    /// </summary>
    internal static UpdateManager? Instance { get; private set; }

    /// <summary>
    /// Gets whether the application is waiting for a restart after an update.
    /// </summary>
    internal static bool WaitForRestart { get; private set; }

    /// <summary>
    /// Initializes the UpdateManager singleton.
    /// </summary>
    internal static void Init()
    {
        var obj = new GameObject("UpdateManager(BAU)") { hideFlags = HideFlags.HideAndDontSave };
        DontDestroyOnLoad(obj);
        Instance = obj.AddComponent<UpdateManager>();
    }

    /// <summary>
    /// Called when the main menu is loaded to set up update UI elements.
    /// </summary>
    internal void OnMainMenu()
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_ModUpdate))
            return;

        var doNotPress = FindObjectOfType<DoNotPressButton>(true);
        if (doNotPress != null)
        {
            doNotPress.gameObject.SetActive(UpdateLoader.UpdateInfo?.IsNewUpdate() == true && !WaitForRestart);
            var buttonPressed = doNotPress.transform.Find("ButtonPressed");
            if (buttonPressed != null)
            {
                doNotPress.pressedSprite = buttonPressed.gameObject.GetComponent<SpriteRenderer>();
            }
            var buttonUnpressed = doNotPress.transform.Find("ButtonUnpressed");
            if (buttonUnpressed != null)
            {
                doNotPress.unpressedSprite = buttonUnpressed.gameObject.GetComponent<SpriteRenderer>();
            }
            doNotPress.pressedSprite.enabled = false;
            doNotPress.pressedSprite.color = new(0.15f, 0.8f, 0.4f);
            doNotPress.unpressedSprite.color = new(0.15f, 0.8f, 0.4f);
            var button = doNotPress.GetComponent<PassiveButton>();
            if (button != null)
            {
                button.OnClick = new();
                button.OnClick.AddListener(() =>
                {
                    if (AmUpdateing || WaitForRestart)
                        return;

                    this.StartCoroutine(CoPressDownload(doNotPress));
                });
            }

            var obj = new GameObject("Update(TMP)");
            obj.transform.SetParent(doNotPress.transform, false);
            obj.transform.localPosition = new Vector3(-0.1018f, -0.1883f, 0f);
            var text = obj.AddComponent<TextMeshPro>();
            text.color = Color.black;
            text.fontSize = 1.5f;
            text.alignment = TextAlignmentOptions.Center;
            text.horizontalAlignment = HorizontalAlignmentOptions.Center;
            text.SetText("Update");
        }
    }

    /// <summary>
    /// Unity Start method called when the component is initialized.
    /// </summary>
    private void Start()
    {
        var oldDll = Assembly.GetExecutingAssembly().Location + ".old";
        if (File.Exists(oldDll))
        {
            File.Delete(oldDll);
        }
    }

    private GameObject? mainMenu;
    private GameObject? ambience;

    /// <summary>
    /// Coroutine that handles the download process when the update button is pressed.
    /// </summary>
    /// <param name="button">The DoNotPressButton that was clicked.</param>
    /// <returns>An IEnumerator for the coroutine.</returns>
    [HideFromIl2Cpp]
    private IEnumerator CoPressDownload(DoNotPressButton button)
    {
        AmUpdateing = true;

        button.pressedSprite.enabled = true;
        button.unpressedSprite.enabled = false;
        yield return new WaitForSeconds(0.1f);
        button.unpressedSprite.enabled = true;
        button.pressedSprite.enabled = false;
        yield return new WaitForSeconds(0.1f);
        button.gameObject.SetActive(false);

        mainMenu = GameObject.Find("MainMenuManager");
        ambience = GameObject.Find("Ambience");
        mainMenu?.SetActive(false);
        ambience?.SetActive(false);

        if (UpdateLoader.UpdateInfo != null && UpdateLoader.UpdateInfo.DllLink != string.Empty)
        {
            if (UpdateLoader.UpdateInfo.IsNewUpdate())
            {
                yield return UpdateLoader.UpdateInfo.CoDownload();
                WaitForRestart = true;
                mainMenu?.SetActive(true);
                ambience?.SetActive(true);
                yield return new WaitForSeconds(0.2f);
                Utils.ShowPopUp("Update complete\nRestart required!");
            }
        }
        else
        {
            mainMenu?.SetActive(true);
            ambience?.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            Utils.ShowPopUp("Download link missing!");
        }

        AmUpdateing = false;
    }
}