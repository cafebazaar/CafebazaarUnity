public interface ICachable<T>
{
    T cachedObj { get; }
    bool isCacheValid { get; }
    void InvalidateCache();
}
