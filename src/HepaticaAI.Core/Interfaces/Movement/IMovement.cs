namespace HepaticaAI.Core.Interfaces.Movement
{
    public interface IMovement
    {
        void Initialize();

        void StartIdleAnimation();

        void StartWinkAnimation();

        void OpenMouth();

        void CloseMouth();
    }
}
