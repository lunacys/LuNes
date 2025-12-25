namespace LuNes.Client.Ui;

public class ComponentManager
{
    private readonly List<IComponent> _components = new();
    private readonly Dictionary<string, IComponent> _namedComponents = new();

    public T Register<T>(T component, string? name = null) where T : class, IComponent
    {
        _components.Add(component);
        if (!string.IsNullOrEmpty(name))
        {
            _namedComponents[name] = component;
        }

        return component;
    }

    public void Update(float deltaTime)
    {
        foreach (var component in _components.Where(c => c.IsVisible))
        {
            component.Update(deltaTime);
        }
    }

    public void Draw()
    {
        foreach (var component in _components.Where(c => c.IsVisible))
        {
            component.Draw();
        }
    }

    public void ResizeAll(int width, int height)
    {
        foreach (var component in _components)
        {
            component.Resize(width, height);
        }
    }

    public T? GetComponent<T>(string name) where T : class, IComponent
    {
        return _namedComponents.TryGetValue(name, out var component) ? component as T : null;
    }
}