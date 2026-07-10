namespace Vendex.App.Navigation;

public interface INavigationService
{
    void NavegarPara<TViewModel>(string titulo) where TViewModel : notnull;
}
