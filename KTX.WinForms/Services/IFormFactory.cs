using Microsoft.Extensions.DependencyInjection;

namespace KTX.WinForms.Services;

public interface IFormFactory
{
    T Create<T>() where T : Form;
}

public class FormFactory : IFormFactory
{
    private readonly IServiceProvider _serviceProvider;

    public FormFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public T Create<T>() where T : Form
    {
        return _serviceProvider.GetRequiredService<T>();
    }
}
