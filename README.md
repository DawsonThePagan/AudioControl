
# AudioControl

Allows easy control of windows audio devices from a command line.  
Part of BatchExtensions [Trello](https://trello.com/b/4J5sT1MN/batchextensions)

## Return Codes:

+ 0 = Successful
+ 1 = Error, external. Windows has passed back an error
+ 2 = Error, user. Invalid arguments

## Arguments:

All arguments are non case sensitive.  

+ /GetDefault = Get the default audio output device
+ /GetAll = Get all connected audio devices
+ /GetVolume ""{Device}"" = Get current volume level of default audio device, or set device to get it by ID or full name
+ /SetDefault ""\<Device>"" = Set the default audio device, can be set by ID or by full name
+ /SetVolume \<Level> ""{Device}"" = Set the volume level of the default audio device, or set device to get it by ID or full name

Be careful when setting default. Its suggested to run /GetAll first to double check your device name is correct

## License

Developed by Bailey-Tyreese Dawson as part of BatchExtensions
Licensed under MIT License
