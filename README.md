# TrainReservation

F# implementation of the [TrainReservation Kata](https://github.com/emilybache/KataTrainReservation). The main purpose
for doing this Kata is to become more experienced with F# development and concepts of Functional programming. The
application uses an ports & adapters application style.

### Building

```sh
> build.cmd <optional buildtarget> // on windows
$ ./build.sh  <optional buildtarget>// on unix
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

