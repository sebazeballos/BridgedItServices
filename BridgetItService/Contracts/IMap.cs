namespace BridgetItService.Contracts
{
    public interface IMap<X, Y>
        where X : class
        where Y : class
    {
        Y Map(X obj);
    }
}
