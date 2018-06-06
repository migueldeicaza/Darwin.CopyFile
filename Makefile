doc-update:
	dotnet build --configuration release
	mdoc update -i ./bin/Release/netstandard1.5/CopyFile.xml -o ecmadocs/en ./bin/Release/netstandard1.5/CopyFile.dll
