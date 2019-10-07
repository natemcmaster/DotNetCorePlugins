Dynamic implementation
===========================

This sample contains 4 projects which demonstrate a plugin scenario which coordinates types between
plugins using a dependency injection container. In this scenario, one  implements a default service definition (Mixer), but adding a plugin the normal behaviour is changed.

This sample can be useful to create pluggable application that can be extended or altered just by adding new modules.

* 'Host' : console app that contains the sample
* 'Contracts' which contains an interface shared by plugins and the host.
* 'Mixer' : this plugin contains the default mixer implementation. It contains the FruitService, which gives 3 fruits and the Mixer service that returns the fruit shake.
* 'ServiceImplementation': this plugin contains an alternative version of the FruitService. By adding this to the plugin set, the default behavior is altered and the shake is compose by only Banana.

## Running the sample

Open a command line to this folder and run:

```
dotnet restore
dotnet run -p Host/
```
