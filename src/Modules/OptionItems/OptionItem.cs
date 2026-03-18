using BetterAmongUs.Data;
using BetterAmongUs.Helpers;
using BetterAmongUs.Modules.Support;
using System.Text;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Modules.OptionItems;

/// <summary>
/// Base abstract class for configuration option items in BetterAmongUs.
/// </summary>
public abstract class OptionItem
{
    internal static int MaskLayer => 20;
    internal static List<OptionItem> AllOptions = [];
    internal static List<OptionItem> AllOptionsTemp = [];
    internal const string InfiniteIcon = "<b>∞</b>";
    internal virtual bool CanLoad => true;
    internal virtual bool IsOption => true;
    public string Name => Translation != null ? Translator.GetString(Translation, showInvalid: false) : "None";
    public int Id => _id ?? -1;
    protected int? _id { get; set; } = null;
    protected string? Translation { get; set; } = null;
    internal OptionTab? Tab { get; set; }
    internal OptionBehaviour? Option { get; set; }
    internal GameObject? Obj { get; set; }
    internal OptionItem? Parent { get; set; }
    internal bool IsParent => Children.Count > 0;
    internal List<OptionItem?> Children { get; set; } = [];
    internal virtual bool Show => ShowCondition.Invoke();
    internal virtual bool ShowChildren => Show;
    internal Func<bool>? ShowCondition = () => { return true; };
    internal bool Hide => !Show || GetParents().Any(opt => !opt.ShowChildren) || BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_GameOption + Translation);
    internal static OptionItem? GetOptionById(int id) => AllOptions.FirstOrDefault(opt => opt._id == id);
    internal virtual void UpdateVisuals(bool updateTabVisuals = true) { }
    public abstract string ValueAsString();
    internal virtual void TryLoad(bool forceLoad = false) { }
    internal virtual void SetToDefault() { }

    /// <summary>
    /// Sets up the Among Us option behavior with proper masking for UI rendering.
    /// </summary>
    /// <param name="optionBehaviour">The option behavior to set up.</param>
    protected void SetupAUOption(OptionBehaviour optionBehaviour)
    {
        Option?.SetClickMask(Tab.AUTab.ButtonClickMask);

        SpriteRenderer[] componentsInChildren = optionBehaviour.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            componentsInChildren[i].material.SetInt(PlayerMaterial.MaskLayer, MaskLayer);
        }
        foreach (TextMeshPro textMeshPro in optionBehaviour.GetComponentsInChildren<TextMeshPro>(true))
        {
            textMeshPro.fontMaterial.SetFloat("_StencilComp", 3f);
            textMeshPro.fontMaterial.SetFloat("_Stencil", MaskLayer);
        }
    }

    /// <summary>
    /// Displays a notification with the option's current value or custom text.
    /// </summary>
    /// <param name="custom">Custom text to display instead of the value.</param>
    internal void PopNotification(string custom = "")
    {
        if (_id == null)
            return;

        string value = custom == string.Empty ? ValueAsString() : custom;
        string msg = $"<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">{GetParentPath()} " +
        $"<color=#868686><size=85%>{Translator.GetString("BetterSetting.SetTo")}</size></color> {value}";
        Utils.SettingsChangeNotifier(Id, msg, false);
    }

    /// <summary>
    /// Gets the hierarchical path of parent options leading to this option.
    /// </summary>
    /// <returns>A formatted string showing the option hierarchy.</returns>
    internal string GetParentPath()
    {
        List<string> names = [Name ?? "???"];
        OptionItem tempOption = this;

        while (tempOption.Parent != null)
        {
            names.Add(tempOption.Parent.Name);
            tempOption = tempOption.Parent;
        }
        return Utils.RemoveSizeHtmlText(string.Join("<b><color=#868686>/</color></b>", names.AsEnumerable().Reverse()));
    }

    /// <summary>
    /// Gets the highest-level parent option in the hierarchy.
    /// </summary>
    /// <returns>The root parent option, or null if no parent exists.</returns>
    internal OptionItem? GetLastParent() => GetParents().LastOrDefault();

    /// <summary>
    /// Enumerates all parent options in the hierarchy from immediate parent to root.
    /// </summary>
    /// <returns>An enumerable of parent options.</returns>
    internal IEnumerable<OptionItem> GetParents()
    {
        if (Parent == null) yield break;

        var target = Parent;
        while (target != null)
        {
            yield return target;
            target = target.Parent;
        }
    }

    /// <summary>
    /// Gets the child index depth of this option in the hierarchy.
    /// </summary>
    /// <returns>The depth level (0 for root options).</returns>
    internal int GetChildIndex()
    {
        int index = 0;
        var target = this;
        while (target.Parent != null)
        {
            index++;
            target = target.Parent;
        }
        return index;
    }

    /// <summary>
    /// Generates a text tree representation of the option hierarchy.
    /// </summary>
    /// <param name="size">Text size percentage.</param>
    /// <param name="showForPercentOption">Whether to show child options for percent items.</param>
    /// <returns>A formatted string showing the option tree structure.</returns>
    internal string FormatOptionsToTextTree(float size = 50f, bool showForPercentOption = true)
    {
        StringBuilder sb = new();
        sb.Append($"<size={size}%>");

        string arrow = "▶";
        string branch = "━";
        string midBranch = "┣";
        string closeBranch = "┗";
        string vertical = "┃";

        List<TreeNode> treeNodes = [];

        void CollectTreeData(OptionItem option, int depth, bool isLastChild, TreeNode? parent)
        {
            var node = new TreeNode
            {
                ParentNode = parent,
                Text = $"{Utils.RemoveSizeHtmlText(option.Name)}: {option.ValueAsString()}",
                Depth = depth,
                IsLastChild = isLastChild
            };
            treeNodes.Add(node);

            if (option.IsParent && option.ShowChildren || option.TryCast<OptionPercentItem>() && showForPercentOption)
            {
                for (int i = 0; i < option.Children.Count; i++)
                {
                    CollectTreeData(option.Children[i], depth + option.GetChildIndex(), i == option.Children.Count - 1, node);
                }
            }
        }

        CollectTreeData(this, 0, true, null);

        bool isSingleOption = treeNodes.Count == 1;

        for (int i = 0; i < treeNodes.Count; i++)
        {
            TreeNode node = treeNodes[i];

            StringBuilder indent = new();

            if (node.Depth > 0)
            {
                bool parentHasSibling = node.ParentNode != null && node.ParentNode.IsLastChild == false;
                indent.Append(parentHasSibling ? $"{vertical} " : "     ");
            }

            string prefix;
            if (isSingleOption)
            {
                prefix = branch;
            }
            else
            {
                prefix = i == 0 ? "┏" : node.IsLastChild ? closeBranch : midBranch;
            }

            sb.AppendLine($"{indent}{prefix}{branch}{arrow} {node.Text}");
        }

        sb.Append("</size>");
        return sb.ToString();
    }

    /// <summary>
    /// Generates text tree representations for multiple option hierarchies.
    /// </summary>
    /// <param name="optionItems">Array of root option items.</param>
    /// <param name="size">Text size percentage.</param>
    /// <param name="showForPercentOption">Whether to show child options for percent items.</param>
    /// <returns>A formatted string showing all option tree structures.</returns>
    internal static string FormatOptionsToTextTrees(OptionItem?[] optionItems, float size = 50f, bool showForPercentOption = true)
    {
        StringBuilder sb = new();
        sb.Append($"<size={size}%>");

        string arrow = "▶";
        string branch = "━";
        string midBranch = "┣";
        string closeBranch = "┗";
        string vertical = "┃";
        string rootPrefix = "┏";

        List<TreeNode> treeNodes = [];

        void CollectTreeData(OptionItem option, int depth, bool isLastChild, TreeNode? parent)
        {
            var node = new TreeNode
            {
                ParentNode = parent,
                Text = $"{Utils.RemoveSizeHtmlText(option.Name)}: {option.ValueAsString()}",
                Depth = depth,
                IsLastChild = isLastChild
            };
            treeNodes.Add(node);

            if (option.IsParent && option.ShowChildren || option.TryCast<OptionPercentItem>() && showForPercentOption)
            {
                for (int i = 0; i < option.Children.Count; i++)
                {
                    CollectTreeData(option.Children[i], depth + option.GetChildIndex(), i == option.Children.Count - 1, node);
                }
            }
        }

        for (int i = 0; i < optionItems.Length; i++)
        {
            if (optionItems[i] == null) continue;
            CollectTreeData(optionItems[i], 0, i == optionItems.Length - 1, null);
        }

        bool isSingleOption = optionItems.Length == 1 && treeNodes.Count == 1;

        for (int i = 0; i < treeNodes.Count; i++)
        {
            TreeNode node = treeNodes[i];

            StringBuilder indent = new();

            if (node.Depth > 0)
            {
                bool parentHasSibling = node.ParentNode != null && node.ParentNode.IsLastChild == false;
                indent.Append(parentHasSibling ? $"{vertical} " : "  ");
            }

            string prefix;
            if (isSingleOption)
            {
                prefix = branch;
            }
            else if (node.Depth == 0)
            {
                prefix = i == 0 ? rootPrefix : node.IsLastChild ? closeBranch : midBranch;
            }
            else
            {
                prefix = node.IsLastChild ? closeBranch : midBranch;
            }

            sb.AppendLine($"{indent}{prefix}{branch}{arrow} {node.Text}");
        }

        sb.Append("</size>");
        return sb.ToString();
    }

    /// <summary>
    /// Creates a description button that shows additional information when clicked.
    /// </summary>
    /// <param name="text">The description text to display.</param>
    internal void CreateDescriptionButton(string text)
    {
        if (Option == null)
            return;

        NumberOption optionBehaviourNum = UnityEngine.Object.Instantiate(Tab.AUTab.numberOptionOrigin, Vector3.zero, Quaternion.identity, Tab.AUTab.settingsContainer);
        SetupAUOption(optionBehaviourNum);
        var button = UnityEngine.Object.Instantiate(optionBehaviourNum.PlusBtn, Option.transform);
        optionBehaviourNum.DestroyObj();
        button.transform.position = button.transform.position - new Vector3(4.75f, 0f, 0f);
        button.transform.GetComponentInChildren<TextMeshPro>(true).gameObject.DestroyObj();
        button.ReceiveMouseOut();
        button.interactableHoveredColor = Color.gray;
        button.interactableClickColor = Color.white;
        button.buttonSprite.sprite = Utils.LoadSprite("BetterAmongUs.Resources.Images.QuestionMark.png", 50);
        button.OnClick = new();
        button.OnClick.AddListener(() =>
        {
            var menu = GameSettingMenu.Instance;
            if (menu != null)
            {
                menu.MenuDescriptionText.text = text;
            }
        });
    }

    /// <summary>
    /// Gets the boolean value of the option (for CheckboxOption).
    /// </summary>
    /// <returns>The boolean value.</returns>
    /// <exception cref="NotImplementedException">Thrown when the option type doesn't support boolean values.</exception>
    public virtual bool GetBool()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the float value of the option (for FloatOption).
    /// </summary>
    /// <returns>The float value.</returns>
    /// <exception cref="NotImplementedException">Thrown when the option type doesn't support float values.</exception>
    public virtual float GetFloat()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the integer value of the option (for IntOption).
    /// </summary>
    /// <returns>The integer value.</returns>
    /// <exception cref="NotImplementedException">Thrown when the option type doesn't support integer values.</exception>
    public virtual int GetInt()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the string value index of the option (for StringOption and PlayerOption).
    /// </summary>
    /// <returns>The string value index.</returns>
    /// <exception cref="NotImplementedException">Thrown when the option type doesn't support string values.</exception>
    public virtual int GetStringValue()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Checks if the option's value matches a specific boolean.
    /// </summary>
    /// <param name="@bool">The boolean value to check against.</param>
    /// <returns>True if the option value matches, false otherwise.</returns>
    /// <exception cref="NotImplementedException">Thrown when the option type doesn't support boolean comparison.</exception>
    public virtual bool Is(bool @bool)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Checks if the option's value matches a specific float.
    /// </summary>
    /// <param name="@float">The float value to check against.</param>
    /// <returns>True if the option value matches, false otherwise.</returns>
    /// <exception cref="NotImplementedException">Thrown when the option type doesn't support float comparison.</exception>
    public virtual bool Is(float @float)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Checks if the option's value matches a specific integer.
    /// </summary>
    /// <param name="@int">The integer value to check against.</param>
    /// <returns>True if the option value matches, false otherwise.</returns>
    /// <exception cref="NotImplementedException">Thrown when the option type doesn't support integer comparison.</exception>
    public virtual bool Is(int @int)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Checks if the option's value matches a specific string.
    /// </summary>
    /// <param name="@string">The string value to check against.</param>
    /// <returns>True if the option value matches, false otherwise.</returns>
    /// <exception cref="NotImplementedException">Thrown when the option type doesn't support string comparison.</exception>
    public virtual bool Is(string @string)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Represents a node in the option hierarchy tree for text formatting.
    /// </summary>
    internal class TreeNode
    {
        /// <summary>
        /// Gets or sets the parent tree node.
        /// </summary>
        internal TreeNode? ParentNode { get; set; }

        /// <summary>
        /// Gets or sets the display text for this node.
        /// </summary>
        internal string? Text { get; set; }

        /// <summary>
        /// Gets or sets the depth level in the tree.
        /// </summary>
        internal int Depth { get; set; }

        /// <summary>
        /// Gets or sets whether this is the last child of its parent.
        /// </summary>
        internal bool IsLastChild { get; set; }
    }
}

/// <summary>
/// Generic base class for typed option items with value storage and serialization.
/// </summary>
/// <typeparam name="T">The type of value stored by this option.</typeparam>
public abstract class OptionItem<T> : OptionItem
{
    protected TextMeshPro? TitleTMP { get; set; }
    protected TextMeshPro? ValueTMP { get; set; }
    private bool HasLoadValue { get; set; }
    protected T? Value { get; set; } = default;
    protected T? DefaultValue { get; set; } = default;
    public virtual T? GetValue()
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_GameOption + Translation))
        {
            return DefaultValue;
        }

        return Value;
    }
    internal virtual T? GetDefaultValue() =>
        DefaultValue;
    public override string ValueAsString() =>
        Value?.ToString() ?? string.Empty;
    internal override void SetToDefault()
    {
        Value = DefaultValue;
    }
    protected abstract void CreateBehavior();
    internal virtual void OnValueChange(T oldValue, T newValue) { }
    internal Action<OptionItem>? OnValueChangeAction = (opt) => { };

    /// <summary>
    /// Sets up common text properties for option display.
    /// </summary>
    /// <param name="textPro">The TextMeshPro component to configure.</param>
    protected void SetupText(TextMeshPro textPro)
    {
        textPro.transform.SetLocalX(-2.5f);
        textPro.transform.SetLocalY(-0.05f);
        textPro.alignment = TextAlignmentOptions.Right;
        textPro.enableWordWrapping = false;
        textPro.enableAutoSizing = true;
        textPro.fontSize = 3.3f;
        textPro.fontSizeMax = 3.3f;
        textPro.fontSizeMin = 1f;
        textPro.rectTransform.sizeDelta = new(4.5f, 1f);
        textPro.outlineColor = Color.black;
        textPro.outlineWidth = 0.25f;
    }

    /// <summary>
    /// Sets up the option behavior after creation.
    /// </summary>
    protected virtual void SetupOptionBehavior() { }

    /// <summary>
    /// Configures the visual appearance of the option based on its hierarchy depth.
    /// </summary>
    protected void SetOptionVisuals()
    {
        if (Option != null)
        {
            float colorNum = 1f - 0.25f * GetChildIndex();
            Option.LabelBackground.color = new Color(colorNum, colorNum, colorNum, 1f);
            Option.LabelBackground.transform.SetLocalZ(1f);
            Option.LabelBackground.transform.localScale = new Vector3(1.6f, 0.8f, 1f);
            Option.LabelBackground.transform.SetLocalX(-2.4f);
            float resize = 0f + 0.1f * GetChildIndex();
            if (resize > 0f)
            {
                if (TitleTMP != null)
                {
                    TitleTMP.rectTransform.sizeDelta -= new Vector2(resize * 2.5f, 0f);
                    TitleTMP.transform.SetLocalX(TitleTMP.transform.localPosition.x + resize * 2.5f);
                }
                var pos = Option.LabelBackground.transform.localPosition;
                var size = Option.LabelBackground.transform.localScale;
                Option.LabelBackground.transform.localPosition = new Vector3(pos.x + resize * 1.8f, pos.y, pos.z);
                Option.LabelBackground.transform.localScale = new Vector3(size.x - resize, size.y, size.z);
            }
        }

        UpdateVisuals();
    }

    /// <summary>
    /// Sets the option's value and triggers updates and notifications.
    /// </summary>
    /// <param name="newValue">The new value to set.</param>
    public virtual void SetValue(T newValue)
    {
        T? oldValue = Value;
        Value = newValue;
        UpdateVisuals();
        PopNotification();
        Save();
        OnValueChange(oldValue, newValue);
        OnValueChangeAction.Invoke(this);
    }

    /// <summary>
    /// Attempts to load the option's value from persistent storage.
    /// </summary>
    /// <param name="forceLoad">Whether to force reload even if already loaded.</param>
    internal override void TryLoad(bool forceLoad = false)
    {
        if (!CanLoad)
            return;

        if (!HasLoadValue || forceLoad)
        {
            HasLoadValue = true;
            Load();
        }
    }

    /// <summary>
    /// Loads the option's value from persistent storage.
    /// </summary>
    protected virtual void Load()
    {
        if (!CanLoad)
            return;

        if (_id == null)
            return;

        Value = BetterDataManager.LoadSetting(Id, DefaultValue);
    }

    /// <summary>
    /// Saves the option's value to persistent storage.
    /// </summary>
    internal virtual void Save()
    {
        if (!CanLoad)
            return;

        if (_id == null)
            return;

        BetterDataManager.SaveSetting(Id, Value);
    }
}