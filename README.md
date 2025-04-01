# ServiceDiscovery.Nomad
Simple service discovery for .NET Aspire and Hashicorp Nomad

## Usage
Add package ServiceDiscovery.Nomad to your Aspire ServiceDefaults project.

```shell
dotnet add package ServiceDiscovery.Nomad
```

In your Aspire ServiceDefaults project, replace the following:
```csharp
~~builder.Services.AddServiceDiscovery();~~
```
with
```csharp
builder.Services.AddNomadServiceDiscovery();
```
and you're good to go.

## How it works
ServiceDiscovery.Nomad uses the Nomad environment variables ```NOMAD_ADDR_*``` to connect to the correct service. Simple, right?
