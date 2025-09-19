namespace videolib
{
    public interface IPlayable
    {
        bool Play();

        void Stop();

        void Dispose(bool disposing);
    }
}
