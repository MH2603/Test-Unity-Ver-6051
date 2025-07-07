using Reflex.Core;
using UnityEngine;

/// <summary>
/// Project-level dependency injection installer using Reflex framework.
/// Configures and registers services/bindings available throughout the application lifecycle.
/// </summary>
public class ProjectInstaller : MonoBehaviour, IInstaller
{
    /// <summary>
    /// Configures dependency injection bindings for the project.
    /// </summary>
    /// <param name="builder">ContainerBuilder used to register services, singletons, and other dependencies that will be resolved throughout the application</param>
    public void InstallBindings(ContainerBuilder builder)
    {
        // Register a singleton string for demonstration
        builder.AddSingleton("Hello");
        
        builder.AddSingleton(new Logger(), typeof(ICustomLogger));
        builder.AddSingleton(new Logger());
        
        Debug.Log("Setup project-level dependency injection bindings.");
    }
}

public interface ICustomLogger
{
    void Log(string message);
}

public class Logger : ICustomLogger
{
    public void Log(string message)
    {
        Debug.Log(message);
    }
}