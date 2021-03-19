#  Build Errors

When adding the <EntryPoint> attribute the following error appeared:

    /home/rob/git/fsharp/kata/TrainReservation/src/TrainReservation/TicketOffice.Server.fs(33,5): error FS0433: A function labeled with the 'EntryPointAttribute' attribute must be the last declaration in the last file in the compilation sequence. [/home/rob/git/fsharp/kata/TrainReservation/src/TrainReservation/TrainReservation.fsproj]

This was resolved by adjusting TrainReservation.fsproj 

1. The source file containing the main function having the <EntryPoint> attribute i.e. *TicketOffice.Server.fs* must be last of the included Compile items.
```
        ....
        <Compile Include="TicketOffice.Controller.fs" />
        <Compile Include="TicketOffice.WebApp.fs" />  
    </ItemGroup> 

   
```

2. GenerateProgramFile directive

The directive GenerateProgramFile must be set to false

    <GenerateProgramFile>false</GenerateProgramFile>

# Added packages

#### Fantomas

    dotnet tool install fantomas-tool

#### Unit Testing

To enable testing via ``dotnet`` and resolve 'Unable to find testhost.dll not found'

    dotnet add tests/TrainReservation.Tests package xunit
    dotnet add tests/TrainReservation.Tests package xunit.runner.visualstudio    
    dotnet add tests/TrainReservation.Tests package FsUnit.xUnit
    dotnet add tests/TrainReservation.Tests package Microsoft.NET.Test.Sdk 

    dotnet add tests/TrainReservation.Client.Tests package xunit
    dotnet add tests/TrainReservation.Client.Tests package xunit.runner.visualstudio    
    dotnet add tests/TrainReservation.Client.Tests package FsUnit.xUnit
    dotnet add tests/TrainReservation.Client.Tests package Microsoft.NET.Test.Sdk 

#### Thoth

paket add Thoth.Json.Net -> give an error: 'invalid parameter 'net50' after >= or < in '>= net50'', there for NuGet from Rider is used
    
    dotnet add src/TrainReservation package Thoth.Json.Net
    dotnet add tests/TrainReservation.Tests package Thoth.Json.Net

## Giraffe

    dotnet add src/TrainReservatio package Microsoft.AspNetCore.App
    dotnet add src/TrainReservatio package Giraffe

### Giraffe HttpHandler testing

    dotnet add tests/TrainReservation.Tests package NSubstitute

## Thoth Development

https://package.elm-lang.org/packages/elm/json/latest/Json-Decode#dict
https://github.com/MangelMaxime/Thoth/blob/master/src/Thoth.Json/Decode.fs
https://github.com/MangelMaxime/Thoth/blob/master/tests/Tests.Json.Decode.fs
