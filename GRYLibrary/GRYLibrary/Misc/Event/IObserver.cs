namespace GRYLibrary.Core.Misc.Event
{
    public interface IObserver<SenderType, EventArgumentType>
    {
        void Update(object sender, Argument<SenderType, EventArgumentType> argument);
    }
}