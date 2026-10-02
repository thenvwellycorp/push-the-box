namespace PushTheBox.Gameplay
{
    /// <summary>
    /// Contract for entities that can be restored to initial state upon level restart.
    /// </summary>
    public interface IResettable
    {
        void ResetState();
    }
}
