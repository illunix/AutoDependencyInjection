# AutoDependencyInjection
[![NuGet](https://img.shields.io/nuget/dt/mediatr.svg)](https://www.nuget.org/packages/AutoDependencyInjection) 
[![NuGet](https://img.shields.io/nuget/vpre/mediatr.svg)](https://www.nuget.org/packages/AutoDependencyInjection)

Auto generate dependency injection in .NET applications

### Installation

You should install [AutoDependencyInjection with NuGet](https://www.nuget.org/packages/AutoDependencyInjection):

    Install-Package AutoDependencyInjection
    
Or via the .NET Core command line interface:

    dotnet add package AutoDependencyInjection
    
## Usage

```csharp
public partial class Program
{
    public readonly Foo _foo;

    static void Main(string[] args)
    {
    }
}
```

When compile, following source will be injected.

```csharp
public partial class Program
{
    public readonly Foo _foo;

    public Program(Foo foo)
    { 
        _foo = foo
    }

    static void Main(string[] args)
    {
    }
}
```
