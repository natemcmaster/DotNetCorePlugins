Plugin Config File
==================

Plugins can be configured with an xml file, conventionally named 'plugin.config'. This file is optional,
and allows plugins to control some settings about how they load.

```xml
<PluginConfig MainAssembly="Banana, Version=1.0.0.0, Culture=neutral, PublicKeyToken=abc12341873">
    <PrivateDependency Identity="MyPrivateDependency, Version=2.0.0.0" />
</PluginConfig>
```

## Elements

The following elements are supported.

### `<PluginConfig>`

The root element of the config file is `PluginConfig`.

The following attributes are supported:

* `MainAssembly` - **required**. This value defines the default assembly for the plugin. Its value should be a valid assembly name.

### `<PrivateDependency>`

This element defines assemblies which the plugin will prefer to load as private versions instead of attempting
to unify with the version with the host application.

 The following attributes are supported:
 * `Identity` - **required**. The assembly identity of the private dependency.
