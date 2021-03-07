# TrainReservation

An F# implementation of the [TrainReservation Kata](https://github.com/emilybache/KataTrainReservation) following Ports
& Adapters style. The main purpose for doing this Kata is to become more experienced with F# development and concepts of
functional programming. Business rules and constraints defined in the Kata were followed as close as possible and the
API contracts and data-models are taken from the
original [TrainReservation Kata](https://github.com/emilybache/KataTrainReservation).

### Building

```sh
$ ./build.sh
```

---

### Publishing

Guide in https://docs.microsoft.com/en-us/dotnet/core/deploying/

**Release Configuration**

When building the Release configuration, and optimizing via the directive `<Optimize>true</Optimize>` in .fsproj, will
prevent the Giraffe/HTTP server to start properly.

Create a *framework-dependent executable*

    dotnet publish -c Release

Create a *framework-dependent single executable*

    dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true -p:UseAppHost=True

Create a single and trimmed executable:

    dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true -p:UseAppHost=True
