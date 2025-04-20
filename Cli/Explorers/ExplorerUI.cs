using CodeBaseContextGenerator.Cli.Models;

namespace CodeBaseContextGenerator.Cli;

public abstract class ExplorerUI<TNode> where TNode : BaseNode
{
    // ─── Fields & Properties ─────────────────────────────────────────────
    private int _selectedIndex;
    private int _windowOffset;
    private string _lastSelectedLabel = "";

    protected List<TNode> Nodes = new();

    protected string LastSelectedLabel
    {
        get => _lastSelectedLabel;
        set => _lastSelectedLabel = value;
    }

    protected virtual int PageSize => 15;
    protected virtual string CurrentPath => null;
    protected virtual ConsoleColor HighlightColor => ConsoleColor.DarkGray;
    protected virtual string GetNodeKey(TNode node) => node?.Label ?? string.Empty;

    // ─── Public Interface ────────────────────────────────────────────────
    public void Browse()
    {
        Nodes = LoadCurrentLevel();
        _selectedIndex = 0;
        _windowOffset = 0;

        while (true)
        {
            Console.Clear();
            RenderHeader();

            if (Nodes.Count == 0)
            {
                Console.WriteLine("📭 This level is empty.");
            }
            else
            {
                RenderVisibleWindow();
            }

            RenderFooter();
            var key = Console.ReadKey(true).Key;

            OnBeforeKey(key);

            switch (key)
            {
                case ConsoleKey.W:
                    if (Nodes.Count > 0) MoveUp();
                    break;

                case ConsoleKey.S:
                    if (Nodes.Count > 0) MoveDown();
                    break;

                case ConsoleKey.A:
                    if (Raise()) Reload();
                    break;

                case ConsoleKey.D:
                    if (Nodes.Count > 0 && Lower(GetCurrent())) Reload();
                    break;

                case ConsoleKey.Spacebar:
                    if (Nodes.Count > 0 && Confirm(GetCurrent())) return;
                    break;

                case ConsoleKey.T:
                    return;
            }

            OnAfterKey(key);
        }
    }

    // ─── Navigation Helpers ──────────────────────────────────────────────
    protected void MoveUp()
    {
        int newIndex = _selectedIndex > 0 ? _selectedIndex - 1 : Nodes.Count - 1;
        SetSelectedIndex(newIndex);
    }

    protected void MoveDown()
    {
        int newIndex = (_selectedIndex + 1) % Nodes.Count;
        SetSelectedIndex(newIndex);
    }

    protected void SetSelectedIndex(int index)
    {
        _selectedIndex = Math.Clamp(index, 0, Nodes.Count - 1);
        int idealOffset = _selectedIndex - PageSize / 2;
        _windowOffset = Math.Clamp(idealOffset, 0, Math.Max(0, Nodes.Count - PageSize));
    }

    protected TNode GetCurrent()
    {
        if (Nodes.Count == 0)
            throw new InvalidOperationException("No nodes available to select.");

        return Nodes[_selectedIndex];
    }

    // ─── Lifecycle & Reloading ───────────────────────────────────────────
    protected void Reload()
    {
        if (Nodes.Count > 0)
            _lastSelectedLabel = GetNodeKey(GetCurrent());

        Nodes = LoadCurrentLevel();

        OnReload();
        RestoreSelection();
    }

    protected void RestoreSelection()
    {
        if (string.IsNullOrWhiteSpace(_lastSelectedLabel)) return;

        for (int i = 0; i < Nodes.Count; i++)
        {
            if (GetNodeKey(Nodes[i]) == _lastSelectedLabel)
            {
                SetSelectedIndex(i);
                break;
            }
        }

        _lastSelectedLabel = null;
    }

    // ─── Rendering ───────────────────────────────────────────────────────
    protected virtual void RenderHeader()
    {
        if (!string.IsNullOrWhiteSpace(CurrentPath))
            Console.WriteLine($"📁 {CurrentPath}\n");
    }

    protected virtual void RenderFooter() => Console.WriteLine("\nW/S: Navigate  A: Up  D: Down  ␣: Select  T: Exit");
    
    private void RenderVisibleWindow()
    {
        int end = Math.Min(Nodes.Count, _windowOffset + PageSize);

        for (int i = _windowOffset; i < end; i++)
            RenderNode(Nodes[i], i == _selectedIndex);
    }

    // ─── Lifecycle Hooks ─────────────────────────────────────────────────
    protected virtual void OnBeforeKey(ConsoleKey key)
    {
    }

    protected virtual void OnAfterKey(ConsoleKey key)
    {
    }

    protected virtual void OnReload()
    {
    }

    // ─── Abstract Methods for Subclasses ─────────────────────────────────
    protected abstract List<TNode> LoadCurrentLevel();
    protected abstract void RenderNode(TNode node, bool isSelected);
    protected abstract bool Raise();
    protected abstract bool Lower(TNode node);
    protected abstract bool Confirm(TNode node);
}