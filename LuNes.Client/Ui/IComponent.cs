namespace LuNes.Client.Ui;

public interface IComponent
{
    bool IsVisible { get; set; }

    void Update(float deltaTime);
    void Draw();
    void Resize(int width, int height);
}