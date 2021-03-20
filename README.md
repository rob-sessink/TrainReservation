# TrainReservation

An F# implementation of the [TrainReservation Kata](https://github.com/emilybache/KataTrainReservation) following Ports
& Adapters style. The main purpose for doing this Kata is to become more experienced with F# development and concepts of
functional programming. Business rules and constraints defined in the Kata were followed as close as possible and the
API contracts and data-models are taken from the
original [TrainReservation Kata](https://github.com/emilybache/KataTrainReservation).

### Building

    ./build.sh

---

### Publishing

See detailed guide in https://docs.microsoft.com/en-us/dotnet/core/deploying/

**Release Configuration**

Create a *framework-dependent executable*

    dotnet publish -c Release

Create a *framework-dependent single executable*

    dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true -p:UseAppHost=True

Create a single and trimmed executable:

    dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true -p:UseAppHost=True

---

### Running

Running the TicketOffice API is done via command:

    cd src/TrainReservation/bin/Release/net5.0/linux-x64/publish
    ./TrainReservation --run 

---
