Darwin.CopyFile
===============

A binding to the Darwin OS `copyfile` API.

The Darwin `copyfile` API can copy files, their metadata, extended
attributes, recursive directory and supports fast copies using
cloning (for files hosted on APFS on macOS and iOS).

You can use this as either a NuGet
([Darwin.CopyFile](https://www.nuget.org/packages/Darwin.CopyFile)),
or you can just copy the single C# file into your project.

API Documentation
=================

You can browse the [API Documentation](https://migueldeicaza.github.io/Darwin.CopyFile/api/Darwin.html)

Using the API
=============

The main API is:

```csharp
public class CopyFile {
    static Status Copy (string from, string to, Flags flags, State state = null);
}
```

Generally, you will configure the operatiob by passing the desired
flags to Copy which controls whether to perform nested copied, whether
to use cloning, copy attributes and so on.

Examples
--------

Copy a file:

```csharp
CopyFile.Copy ("/etc/hosts", "/tmp/hostscopy", CopyFile.Flags.All);
```

Copy a file, using cloning:
```csharp
CopyFile.Copy ("/etc/hosts", "/tmp/hosts", CopyFile.Flags.All | CopyFile.Flags.Clone);
```

Copy a file, using clone, two times:
```csharp
CopyFile.Copy ("/etc/hosts", "/tmp/hosts2", CopyFile.Flags.All | CopyFile.Flags.Clone);

// This fails, becaue Clone refuses to override the file if the target exists:
CopyFile.Copy ("/etc/hosts", "/tmp/hosts2", CopyFile.Flags.All | CopyFile.Flags.Clone);

// Tell Copy to unlink (remove) the target if it is present:
CopyFile.Copy ("/etc/hosts", "/tmp/hosts2", CopyFile.Flags.All | CopyFile.Flags.Clone | CopyFile.Flags.Unlink);
```

Copy a file, only if the target does not exist (returns EEXIST) error if this is the case:

```
CopyFile.Copy ("/etc/hosts", "/tmp/hosts", CopyFile.Flags.All | CopyFile.Flags.Excl)  
```

Recursively copy a directory, using clone:

```csharp
CopyFile.Copy ("/Users/miguel/Desktop", "/tmp/CopyDesktop", CopyFile.Flags.All |  CopyFile.Flags.Clone | CopyFile.Flags.Recursive);
```

Controling Recursive Copies
===========================

The `State` object is used to control how recursive copies are
performed, you create a state object and you can further customize how
the copy works by providing a callback to it and accessing the State
object from the callback.
